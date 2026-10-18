using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DepoBul.Research
{
    public sealed class GeoPoint
    {
        public double Latitude, Longitude;
        public GeoPoint(double latitude,double longitude){Latitude=latitude;Longitude=longitude;}
    }
    public sealed class AutoSearchOptions
    {
        public double[] StepsKm=new[]{40.0,20.0,10.0};
        public GaOptions Screening=new GaOptions{Population=100,Generations=100,Capacity=20,MaxVehicles=28,ExtraVehiclePenalty=100};
        public GaOptions Final=new GaOptions{Population=300,Generations=200,Capacity=20,MaxVehicles=28,ExtraVehiclePenalty=100};
        public int[] ScreeningSeeds=new[]{1,2,3},FinalSeeds=Enumerable.Range(1001,30).ToArray();
        public int Finalists=3;
        public double MaxAnchorSnapMetres=2000;
        public void Validate()
        {
            Screening.Validate();Final.Validate();
            if(Screening.Capacity!=Final.Capacity||Screening.MaxVehicles!=Final.MaxVehicles||Screening.ExtraVehiclePenalty!=Final.ExtraVehiclePenalty)
                throw new ArgumentException("Tarama ve son değerlendirmede kapasite, filo ve ceza aynı olmalı.");
            if(StepsKm==null||StepsKm.Length<1||StepsKm.Length>8||StepsKm.Any(x=>!RoadData.Finite(x)||x<=0||x>500)||
                StepsKm.Where((x,i)=>i>0 && x>=StepsKm[i-1]).Any())throw new ArgumentException("Arama adımları azalan, pozitif km değerleri olmalı (en fazla 8 düzey).");
            foreach(var seeds in new[]{ScreeningSeeds,FinalSeeds})
                if(seeds==null||seeds.Length==0||seeds.Distinct().Count()!=seeds.Length)throw new ArgumentException("Başlangıç kodları boş veya tekrarlı olamaz.");
            if(Finalists<1||Finalists>20||!RoadData.Finite(MaxAnchorSnapMetres)||MaxAnchorSnapMetres<=0||MaxAnchorSnapMetres>10000)
                throw new ArgumentException("Geçersiz arama sınırı.");
        }
    }
    public interface IAutoRoadProvider
    {
        Location Snap(GeoPoint anchor,string id,double maxDistance,CancellationToken token);
        RoadData Build(Location[] stores,Location depot,CancellationToken token);
    }
    public sealed class SearchVisit
    {
        public int Level;
        public GeoPoint Anchor;
        public string Status,Detail;
        public AutoCandidate Candidate;
    }
    public sealed class AutoCandidate
    {
        public Location Depot;
        public RoadData Data;
        public List<RouteSolution> ScreeningRuns,FinalRuns;
        public double ScreeningMean {get{return ScreeningRuns.Average(r=>r.Score);}}
        public double FinalMean {get{return FinalRuns.Average(r=>r.Score);}}
    }
    public sealed class AutoSearchResult
    {
        public GeoPoint InitialCentre;
        public List<SearchVisit> Visits=new List<SearchVisit>();
        public List<AutoCandidate> Candidates=new List<AutoCandidate>();
        public AutoCandidate Winner;
        public RouteSolution BestSolution,NearestNeighbour;
        public AutoSearchOptions Options;
    }
    public static class AutoDepotSearch
    {
        public static void ValidateStores(Location[] stores,int capacity)
        {
            if(stores==null||stores.Length<2||stores.Length>94)throw new ArgumentException("Bu sürüm 2–94 mağaza içindir. Pilot veri 28 mağazadır.");
            if(stores.Any(n=>n==null||n.Kind!="store"||String.IsNullOrWhiteSpace(n.Id)||n.Demand<=0||n.Demand>capacity||
                !RoadData.Finite(n.Latitude)||!RoadData.Finite(n.Longitude)||Math.Abs(n.Latitude)>75||Math.Abs(n.Longitude)>180)||
                stores.Select(n=>n.Id).Distinct(StringComparer.Ordinal).Count()!=stores.Length)
                throw new ArgumentException("Mağaza kodu, koordinatı veya palet talebi geçersiz.");
            if(stores.Max(n=>n.Longitude)-stores.Min(n=>n.Longitude)>30||stores.Max(n=>n.Latitude)-stores.Min(n=>n.Latitude)>30)
                throw new ArgumentException("Bu bölgesel arama 30 dereceden geniş koordinat aralığı için tasarlanmadı.");
        }
        public static GeoPoint WeightedCentre(Location[] stores)
        {
            double total=stores.Sum(n=>(double)n.Demand);
            return new GeoPoint(stores.Sum(n=>n.Latitude*n.Demand)/total,stores.Sum(n=>n.Longitude*n.Demand)/total);
        }
        public static IEnumerable<GeoPoint> Neighbourhood(GeoPoint centre,double stepKm)
        {
            double lat=stepKm/111.32,lon=stepKm/(111.32*Math.Cos(centre.Latitude*Math.PI/180));
            yield return new GeoPoint(centre.Latitude,centre.Longitude);
            yield return new GeoPoint(centre.Latitude+lat,centre.Longitude);
            yield return new GeoPoint(centre.Latitude-lat,centre.Longitude);
            yield return new GeoPoint(centre.Latitude,centre.Longitude+lon);
            yield return new GeoPoint(centre.Latitude,centre.Longitude-lon);
        }
        static List<RouteSolution> Evaluate(RoadData data,GaOptions opt,int[] seeds,CancellationToken token)
        {
            var runs=new List<RouteSolution>();int depot=Array.FindIndex(data.Nodes,n=>n.Kind=="depot");
            foreach(int seed in seeds){token.ThrowIfCancellationRequested();runs.Add(RoutingEngine.Run(data,depot,opt,seed,token));}
            return runs;
        }
        public static AutoSearchResult Run(Location[] stores,AutoSearchOptions options,IAutoRoadProvider provider,CancellationToken token,Action<string> progress)
        {
            options.Validate();ValidateStores(stores,options.Final.Capacity);
            var result=new AutoSearchResult{InitialCentre=WeightedCentre(stores),Options=options};
            GeoPoint centre=result.InitialCentre;var evaluated=new Dictionary<string,AutoCandidate>();int serial=0;
            Action<string> report=message=>{if(progress!=null)progress(message);};
            for(int level=0;level<options.StepsKm.Length;level++)
            {
                token.ThrowIfCancellationRequested();var current=new List<AutoCandidate>();
                foreach(var anchor in Neighbourhood(centre,options.StepsKm[level]))
                {
                    token.ThrowIfCancellationRequested();var visit=new SearchVisit{Level=level+1,Anchor=anchor};result.Visits.Add(visit);
                    try
                    {
                        report("Konum araması: düzey "+(level+1)+", adım "+options.StepsKm[level]+" km. Yol bağlantısı alınıyor.");
                        var depot=provider.Snap(anchor,"AUTO"+(++serial).ToString("D3"),options.MaxAnchorSnapMetres,token);
                        if(stores.Any(n=>n.Id==depot.Id))throw new InvalidOperationException("Üretilen depo kodu mağaza koduyla çakıştı.");
                        string key=depot.Latitude.ToString("F6",CultureInfo.InvariantCulture)+","+depot.Longitude.ToString("F6",CultureInfo.InvariantCulture);
                        AutoCandidate candidate;
                        if(!evaluated.TryGetValue(key,out candidate))
                        {
                            var data=provider.Build(stores,depot,token);data.Validate(options.Final.Capacity);
                            report(depot.Id+": karayolu matrisi hazır, GA taraması çalışıyor.");
                            candidate=new AutoCandidate{Depot=depot,Data=data,ScreeningRuns=Evaluate(data,options.Screening,options.ScreeningSeeds,token)};
                            evaluated.Add(key,candidate);result.Candidates.Add(candidate);
                        }
                        visit.Candidate=candidate;visit.Status="evaluated";visit.Detail="Yol bağlantısı ve kapasite denetlendi.";current.Add(candidate);
                    }
                    catch(OperationCanceledException){throw;}
                    catch(RoadPointRejectedException ex){visit.Status="rejected";visit.Detail=ex.Message;report("Konum elendi: "+ex.Message);}
                }
                if(current.Count>0)
                {
                    var best=current.OrderBy(c=>c.ScreeningMean).ThenBy(c=>c.Depot.Id,StringComparer.Ordinal).First();
                    centre=new GeoPoint(best.Depot.Latitude,best.Depot.Longitude);
                }
                else if(result.Candidates.Count==0)throw new InvalidOperationException("Bu düzeyde erişilebilir ve kapasiteye uygun depo konumu bulunamadı. Yol verilerini ve arama adımlarını kontrol edin.");
            }
            var finalists=result.Candidates.OrderBy(c=>c.ScreeningMean).ThenBy(c=>c.Depot.Id,StringComparer.Ordinal).Take(options.Finalists).ToArray();
            foreach(var candidate in finalists)
            {
                report(candidate.Depot.Id+": son değerlendirme, "+options.FinalSeeds.Length+" bağımsız GA koşumu.");
                candidate.FinalRuns=Evaluate(candidate.Data,options.Final,options.FinalSeeds,token);
            }
            result.Winner=finalists.OrderBy(c=>c.FinalMean).ThenBy(c=>c.Depot.Id,StringComparer.Ordinal).First();
            result.BestSolution=result.Winner.FinalRuns.OrderBy(r=>r.Score).ThenBy(r=>r.Seed).First();
            result.NearestNeighbour=RoutingEngine.NearestNeighbour(result.Winner.Data,result.BestSolution.DepotIndex,options.Final);
            report("Arama tamamlandı. Son seçim: "+result.Winner.Depot.Id+"; ölçüt son koşumların ortalama F değeri.");return result;
        }
    }
    public sealed class RoadPointRejectedException : Exception
    {public RoadPointRejectedException(string message):base(message){}}
}
