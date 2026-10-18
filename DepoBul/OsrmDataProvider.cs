using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace DepoBul.Research
{
    [DataContract] public sealed class OsrmNearestReply
    {
        [DataMember(Name="code")] public string Code;
        [DataMember(Name="waypoints")] public GeometryWaypoint[] Waypoints;
    }
    [DataContract] public sealed class OsrmTableReply
    {
        [DataMember(Name="code")] public string Code;
        [DataMember(Name="distances")] public double?[][] Distances;
        [DataMember(Name="sources")] public GeometryWaypoint[] Sources;
        [DataMember(Name="destinations")] public GeometryWaypoint[] Destinations;
        [DataMember(Name="data_version")] public string DataVersion;
    }
    public sealed class OsrmDataProvider : IAutoRoadProvider
    {
        readonly string directory;
        readonly Func<string,CancellationToken,string> transport;
        DateTime last=DateTime.MinValue;
        double[,] fixedStoreKm;
        double[][] fixedSourcePoints,fixedDestinationPoints;
        string storeSignature,version;
        public OsrmDataProvider(string evidenceDirectory,Func<string,CancellationToken,string> customTransport=null)
        {directory=evidenceDirectory;Directory.CreateDirectory(directory);transport=customTransport;}
        public static T Parse<T>(string json)
        {using(var stream=new MemoryStream(Encoding.UTF8.GetBytes(json)))return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);}
        string Get(string url,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();string path=Path.Combine(directory,RoadGeometry.CacheName(url)+".json");
            if(File.Exists(path))return File.ReadAllText(path,Encoding.UTF8);
            int delay=(int)Math.Ceiling(1100-(DateTime.UtcNow-last).TotalMilliseconds);
            if(delay>0 && token.WaitHandle.WaitOne(delay))token.ThrowIfCancellationRequested();last=DateTime.UtcNow;
            string json;
            if(transport!=null)json=transport(url,token);
            else
            {
                // SystemDefault: let Windows negotiate TLS.
                ServicePointManager.SecurityProtocol=(SecurityProtocolType)0;
                var request=(HttpWebRequest)WebRequest.Create(url);request.UserAgent="DepoBul-INFTEC2026-Research/2.0";
                request.Timeout=30000;request.ReadWriteTimeout=30000;
                using(token.Register(()=>request.Abort()))
                {
                    try{using(var response=request.GetResponse())using(var reader=new StreamReader(response.GetResponseStream(),Encoding.UTF8))json=reader.ReadToEnd();}
                    catch(WebException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        if(ex.Response==null)throw new InvalidOperationException(
                            (ex.Status==WebExceptionStatus.SecureChannelFailure || ex.Status==WebExceptionStatus.TrustFailure
                            ? "OSRM güvenli bağlantısı kurulamadı (SSL/TLS). Windows/.NET güncelliğini ve ağın sertifika denetimini kontrol edin. "
                            : "OSRM bağlantısı kurulamadı. ")+"Ayrıntı: "+ex.Status+" — "+ex.Message,ex);
                        var httpResponse=ex.Response as HttpWebResponse;
                        if(httpResponse==null)throw new InvalidOperationException("OSRM HTTP yanıtı geçersiz.",ex);
                        var responseStatus=httpResponse.StatusCode;
                        using(var response=ex.Response)using(var reader=new StreamReader(response.GetResponseStream(),Encoding.UTF8))json=reader.ReadToEnd();
                        if(responseStatus!=HttpStatusCode.BadRequest)
                            throw new InvalidOperationException("OSRM sunucusu yanıt veremedi. Daha sonra yeniden deneyin.",ex);
                    }
                }
            }
            token.ThrowIfCancellationRequested();File.WriteAllText(path,json,Encoding.UTF8);
            File.WriteAllText(Path.ChangeExtension(path,"istek.txt"),DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture)+"\r\n"+url,Encoding.UTF8);
            return json;
        }
        static string Num(double x){return x.ToString("R",CultureInfo.InvariantCulture);}
        static double Separation(double lat,double lon,double[] xy)
        {
            double a=lat*Math.PI/180,b=xy[1]*Math.PI/180,dlat=b-a,dlon=(xy[0]-lon)*Math.PI/180;
            return 6371000*2*Math.Asin(Math.Min(1,Math.Sqrt(Math.Pow(Math.Sin(dlat/2),2)+Math.Cos(a)*Math.Cos(b)*Math.Pow(Math.Sin(dlon/2),2))));
        }
        public static void CheckWaypoint(GeometryWaypoint waypoint,double lat,double lon,double maxMetres)
        {
            if(waypoint==null||waypoint.Location==null||waypoint.Location.Length!=2||
                !RoadData.Finite(waypoint.Distance)||waypoint.Distance<0||waypoint.Distance>maxMetres||
                waypoint.Location.Any(x=>!RoadData.Finite(x))||Math.Abs(waypoint.Location[0])>180||Math.Abs(waypoint.Location[1])>90||
                Separation(lat,lon,waypoint.Location)>maxMetres+1)
                throw new RoadPointRejectedException("Yol ağına eşleştirme izin verilen uzaklığı aşıyor veya geçersiz.");
        }
        public Location Snap(GeoPoint anchor,string id,double maxDistance,CancellationToken token)
        {
            if(!RoadData.Finite(anchor.Latitude)||!RoadData.Finite(anchor.Longitude)||Math.Abs(anchor.Latitude)>85||Math.Abs(anchor.Longitude)>180)
                throw new RoadPointRejectedException("Üretilen arama koordinatı kapsam dışında.");
            string url="https://router.project-osrm.org/nearest/v1/driving/"+Num(anchor.Longitude)+","+Num(anchor.Latitude)+"?number=1&radiuses="+Num(maxDistance);
            var response=Parse<OsrmNearestReply>(Get(url,token));
            if(response!=null && response.Code=="NoSegment")throw new RoadPointRejectedException("Bu arama konumunun yakınında yol bağlantısı bulunamadı.");
            if(response==null||response.Code!="Ok"||response.Waypoints==null||response.Waypoints.Length!=1)
                throw new InvalidOperationException("OSRM nearest isteği beklenen biçimde yanıtlanmadı; sorgu kaydı korunmuştur.");
            var w=response.Waypoints[0];CheckWaypoint(w,anchor.Latitude,anchor.Longitude,maxDistance);
            return new Location{Id=id,Kind="depot",Latitude=w.Location[1],Longitude=w.Location[0],Demand=0,AccessConfirmed=true,
                AccessNote="Otomatik yerel arama; OSRM nearest, başlangıç noktasından "+Num(w.Distance)+" m. Yalnız yol ağı bağlantısı; parsel/imar uygunluğu denetlenmedi."};
        }
        public RoadData Build(Location[] stores,Location depot,CancellationToken token)
        {
            string signature=String.Join("|",stores.Select(store=>store.Id+":"+Num(store.Latitude)+","+Num(store.Longitude)));
            if(storeSignature!=null && storeSignature!=signature)throw new InvalidOperationException("Arama sırasında mağaza listesi değiştirilemez.");
            var nodes=stores.Concat(new[]{depot}).ToArray();int n=nodes.Length;
            string url="https://router.project-osrm.org/table/v1/driving/"+String.Join(";",nodes.Select(x=>Num(x.Longitude)+","+Num(x.Latitude)))+
                "?annotations=distance&radiuses="+String.Join(";",Enumerable.Repeat("100",n));
            var reply=Parse<OsrmTableReply>(Get(url,token));
            if(reply!=null && new[]{"NoSegment","NoRoute","NoTable"}.Contains(reply.Code))throw new RoadPointRejectedException("Bu konum için yönlü karayolu matrisi alınamadı ("+reply.Code+").");
            if(reply==null||reply.Code!="Ok")throw new InvalidOperationException("OSRM table isteği beklenen biçimde yanıtlanmadı; sorgu kaydı korunmuştur.");
            if(reply.Sources==null||reply.Sources.Length!=n||reply.Destinations==null||reply.Destinations.Length!=n||reply.Distances==null||reply.Distances.Length!=n)
                throw new InvalidOperationException("OSRM eksik matris/konum bilgisi döndürdü.");
            for(int i=0;i<n;i++){CheckWaypoint(reply.Sources[i],nodes[i].Latitude,nodes[i].Longitude,100);CheckWaypoint(reply.Destinations[i],nodes[i].Latitude,nodes[i].Longitude,100);}
            if(version!=null && reply.DataVersion!=null && version!=reply.DataVersion)
                throw new InvalidOperationException("OSRM yol verisi sürümü arama sırasında değişti. Yeni deney klasöründe yeniden başlatın.");
            var km=new double[n,n];
            for(int i=0;i<n;i++)
            {
                if(reply.Distances[i]==null||reply.Distances[i].Length!=n)throw new InvalidOperationException("OSRM matris boyutu hatalı.");
                for(int j=0;j<n;j++)
                {
                    double? value=reply.Distances[i][j];
                    if(!value.HasValue||!RoadData.Finite(value.Value)||value.Value<0||(i!=j && value.Value==0)||(i==j && value.Value!=0))
                        throw new RoadPointRejectedException("Mağazalar ve depo arasında bütün yönlerde sonlu karayolu bağlantısı yok.");
                    km[i,j]=value.Value/1000;
                }
            }
            if(fixedStoreKm!=null)
                for(int i=0;i<stores.Length;i++)
                    if(Separation(fixedSourcePoints[i][1],fixedSourcePoints[i][0],reply.Sources[i].Location)>1 ||
                        Separation(fixedDestinationPoints[i][1],fixedDestinationPoints[i][0],reply.Destinations[i].Location)>1)
                        throw new RoadPointRejectedException("Bu konum sorgusunda mağaza yol eşleştirmesi değişti; ortak matrisle tutarlı değil.");
            if(fixedStoreKm==null)
            {
                fixedSourcePoints=reply.Sources.Take(stores.Length).Select(w=>(double[])w.Location.Clone()).ToArray();
                fixedDestinationPoints=reply.Destinations.Take(stores.Length).Select(w=>(double[])w.Location.Clone()).ToArray();
                fixedStoreKm=new double[stores.Length,stores.Length];storeSignature=signature;version=reply.DataVersion;
                for(int i=0;i<stores.Length;i++)for(int j=0;j<stores.Length;j++)fixedStoreKm[i,j]=km[i,j];
            }
            // Hold store-to-store costs fixed throughout the location search; only depot legs vary.
            for(int i=0;i<stores.Length;i++)for(int j=0;j<stores.Length;j++)km[i,j]=fixedStoreKm[i,j];
            return new RoadData{Nodes=nodes,Km=km,Source="OSRM / OpenStreetMap contributors (ODbL)",QueryDate=DateTime.UtcNow.ToString("yyyy-MM-dd"),
                TravelMode="driving - otomobil profili",RouteOptions="radiuses=100 m; yönlü table hızlı rota mesafeleri; trafik yok; fallback yok; mağaza matrisi ilk başarılı sorguda sabitlendi; depo bacakları yeni sorgudan. OSM data_version="+(version??"sunucu belirtmedi")};
        }
    }
}
