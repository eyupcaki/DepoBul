using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace DepoBul.Research
{
    [DataContract] public sealed class GeometryReply
    {
        [DataMember(Name="code")] public string Code;
        [DataMember(Name="routes")] public GeometryRoute[] Routes;
        [DataMember(Name="waypoints")] public GeometryWaypoint[] Waypoints;
    }
    [DataContract] public sealed class GeometryWaypoint
    {
        [DataMember(Name="distance")] public double Distance;
        [DataMember(Name="location")] public double[] Location;
    }
    [DataContract] public sealed class GeometryRoute
    {
        [DataMember(Name="geometry")] public GeometryLine Geometry;
    }
    [DataContract] public sealed class GeometryLine
    {
        [DataMember(Name="type")] public string Type;
        [DataMember(Name="coordinates")] public double[][] Coordinates;
    }
    public static class RoadGeometry
    {
        static readonly object Gate=new object();
        static DateTime lastRequest=DateTime.MinValue;
        public static string RequestUrl(RoadData data,int from,int to)
        {
            return "https://router.project-osrm.org/route/v1/driving/"+
                String.Join(";",new[]{from,to}.Select(i=>data.Nodes[i].Longitude.ToString("R",CultureInfo.InvariantCulture)+","+data.Nodes[i].Latitude.ToString("R",CultureInfo.InvariantCulture)))+
                "?overview=full&geometries=geojson&steps=false&continue_straight=false&radiuses=100;100";
        }
        public static string CacheName(string url)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(url))).Replace("-","").ToLowerInvariant()+".json";}
        public static double[][] Parse(string json,int expectedWaypoints=-1)
        {
            GeometryReply result;
            using(var stream=new MemoryStream(Encoding.UTF8.GetBytes(json)))
                result=(GeometryReply)new DataContractJsonSerializer(typeof(GeometryReply)).ReadObject(stream);
            if(result==null || result.Code!="Ok" || result.Routes==null || result.Routes.Length==0 ||
                result.Routes[0].Geometry==null || result.Routes[0].Geometry.Type!="LineString")
                throw new InvalidOperationException("OSRM yol geometrisi alınamadı.");
            if(result.Waypoints==null || result.Waypoints.Length<2 || (expectedWaypoints>=0 && result.Waypoints.Length!=expectedWaypoints) ||
                result.Waypoints.Any(w=>w==null || !RoadData.Finite(w.Distance) || w.Distance<0 || w.Distance>100 ||
                    w.Location==null || w.Location.Length<2 || !RoadData.Finite(w.Location[0]) || !RoadData.Finite(w.Location[1]) || Math.Abs(w.Location[0])>180 || Math.Abs(w.Location[1])>90))
                throw new InvalidOperationException("Yol eşleştirmesi eksik veya konumdan 100 metreden fazla uzak. Hatalı çizim reddedildi.");
            var xy=result.Routes[0].Geometry.Coordinates;
            if(xy==null || xy.Length<2 || xy.Any(p=>p==null || p.Length<2 || !RoadData.Finite(p[0]) || !RoadData.Finite(p[1]) || Math.Abs(p[0])>180 || Math.Abs(p[1])>90))
                throw new InvalidOperationException("OSRM geçersiz yol koordinatı döndürdü.");
            return xy;
        }
        public static List<List<double[][]>> Load(RoadData data,RouteSolution solution,string bundledCache,Action<int,int> progress=null)
        {
            var vehicles=new List<List<double[][]>>();int completed=0,total=solution.Routes.Sum(r=>r.Length+1);
            foreach(var route in solution.Routes)
            {
                var segments=new List<double[][]>();int previous=solution.DepotIndex;
                foreach(int next in route.Concat(new[]{solution.DepotIndex}))
                {
                    string url=RequestUrl(data,previous,next);
                    string path=Path.Combine(bundledCache,CacheName(url));string json;
                    if(File.Exists(path))json=File.ReadAllText(path,Encoding.UTF8);
                    else
                    {
                        lock(Gate)
                        {
                            int delay=(int)Math.Ceiling(1100-(DateTime.UtcNow-lastRequest).TotalMilliseconds);
                            if(delay>0)Thread.Sleep(delay);lastRequest=DateTime.UtcNow;
                            // SystemDefault: let Windows negotiate TLS.
                ServicePointManager.SecurityProtocol=(SecurityProtocolType)0;
                            var request=(HttpWebRequest)WebRequest.Create(url);
                            request.UserAgent="DepoBul-INFTEC2026-Research/1.0";
                            request.Timeout=30000;request.ReadWriteTimeout=30000;
                            using(var response=request.GetResponse())using(var stream=response.GetResponseStream())
                            using(var reader=new StreamReader(stream,Encoding.UTF8))json=reader.ReadToEnd();
                        }
                    }
                    segments.Add(Parse(json,2));previous=next;if(progress!=null)progress(++completed,total);
                }
                vehicles.Add(segments);
            }
            return vehicles;
        }
    }
}
