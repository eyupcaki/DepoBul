using System;
using System.IO;
using System.Linq;
using System.Threading;
using DepoBul.Research;
class EngineChecks
{
    static int checks;
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);checks++;}
    static void Throws(Action action,string message){try{action();}catch(ArgumentException){checks++;return;}throw new Exception(message);}
    static RoadData Data(int count)
    {
        var nodes=new Location[count+2];
        for(int i=0;i<nodes.Length;i++)nodes[i]=new Location{Id="N"+i,Kind=i<2?"depot":"store",Demand=i<2?0:1,
            Latitude=37,Longitude=38,AccessConfirmed=true,AccessNote="Synthetic test assumption"};
        var km=new double[nodes.Length,nodes.Length];
        for(int i=0;i<nodes.Length;i++)for(int j=0;j<nodes.Length;j++)km[i,j]=i==j?0:i*10+j+.25;
        return new RoadData{Nodes=nodes,Km=km,Source="SYNTHETIC TEST ONLY",QueryDate="2026-10-15",TravelMode="synthetic",RouteOptions="test"};
    }
    static void Main()
    {
        var d=Data(3);d.Nodes[2].Demand=3;d.Nodes[3].Demand=2;d.Nodes[4].Demand=4;
        var o=new GaOptions{Capacity=5,Population=16,Generations=15,ExtraVehiclePenalty=100,MutationProbability=.3};
        d.Validate(5);var a=RoutingEngine.Evaluate(d,0,new[]{2,3,4},o);
        double expected=d.Km[0,2]+d.Km[2,3]+d.Km[3,0]+d.Km[0,4]+d.Km[4,0];
        Check(a.Routes.Count==2,"Route split");Check(a.DistanceKm==expected,"Departure, directed legs, return");
        Check(a.Score==expected+100,"Penalty");Check(a.DistanceKm!=Math.Truncate(a.DistanceKm),"No integer truncation");
        Check(a.Routes.SelectMany(x=>x).Distinct().Count()==3,"Each store once");
        foreach(var r in a.Routes)Check(r.Sum(i=>d.Nodes[i].Demand)<=o.Capacity,"Capacity");
        int[] p={2,3,4},q={4,3,2};var child=RoutingEngine.Crossover(p,q,1);
        Check(p.SequenceEqual(new[]{2,3,4})&&q.SequenceEqual(new[]{4,3,2}),"Parent mutation");
        Check(child.SequenceEqual(new[]{2,4,3}),"Prefix order crossover");
        for(int i=0;i<100;i++){RoutingEngine.SwapMutation(child,new Random(i));Check(child.OrderBy(x=>x).SequenceEqual(p),"Swap permutation");}
        Throws(()=>RoutingEngine.Evaluate(d,0,new[]{2,2,4},o),"Duplicate gene accepted");
        d.Nodes[2].Demand=6;Throws(()=>d.Validate(5),"Excess demand accepted");d.Nodes[2].Demand=3;
        double saved=d.Km[0,2];d.Km[0,2]=Double.NaN;Throws(()=>d.Validate(5),"Missing road accepted");d.Km[0,2]=saved;
        d.Km[2,0]=0;Throws(()=>d.Validate(5),"Zero road accepted");d.Km[2,0]=20.25;
        d.Nodes[0].AccessConfirmed=false;Throws(()=>d.Validate(5),"Unchecked depot accepted");d.Nodes[0].AccessConfirmed=true;
        var r1=RoutingEngine.Run(d,0,o,123,CancellationToken.None);var r2=RoutingEngine.Run(d,0,o,123,CancellationToken.None);
        Check(r1.Order.SequenceEqual(r2.Order)&&r1.Score==r2.Score,"Seed reproducibility");
        Check(r1.BestByGeneration.Length==16,"Generation count");
        for(int i=1;i<r1.BestByGeneration.Length;i++)Check(r1.BestByGeneration[i]<=r1.BestByGeneration[i-1],"Elite lost");
        var runs=RoutingEngine.Compare(d,o,new[]{1,2,3},CancellationToken.None,null);
        Check(runs.Count==6,"Candidate/seed pairing");
        Check(runs.GroupBy(r=>r.DepotIndex).All(g=>g.Select(r=>r.Seed).SequenceEqual(new[]{1,2,3})),"Unequal seeds");
        Throws(()=>RoutingEngine.Compare(d,o,new[]{1,1},CancellationToken.None,null),"Duplicate seed accepted");
        var many=Data(28);var result=RoutingEngine.Run(many,0,o,8,CancellationToken.None);
        Check(result.Order.Length==28&&result.Routes.SelectMany(x=>x).Distinct().Count()==28,"23-store cap");
        var one=RoutingEngine.Run(Data(1),0,o,4,CancellationToken.None);Check(one.Routes.Count==1,"Single store");
        var limit=new GaOptions{Capacity=5,MaxVehicles=1};Check(Double.IsPositiveInfinity(RoutingEngine.Evaluate(d,0,new[]{2,3,4},limit).Score),"Fleet constraint");
        var cts=new CancellationTokenSource();cts.Cancel();try{RoutingEngine.Run(d,0,o,0,cts.Token);throw new Exception("Cancellation failed");}catch(OperationCanceledException){checks++;}
        string dir=Path.Combine(Path.GetTempPath(),"depobul-check-"+Guid.NewGuid());Directory.CreateDirectory(dir);
        string n=Path.Combine(dir,"nodes.csv"),m=Path.Combine(dir,"matrix.csv"),meta=Path.Combine(dir,"meta.csv");
        ResearchCsv.Write(n,new[]{new[]{"id","kind","latitude","longitude","demand_pallet","access_confirmed","access_note"},new[]{"D","depot","37","38","0","true","Yol; bağlantısı"},new[]{"S","store","37.1","38.1","2","false",""}});
        ResearchCsv.Write(m,new[]{new[]{"from_id","to_id","distance_km"},new[]{"D","S","1.25"},new[]{"S","D","2.5"}});
        ResearchCsv.Write(meta,new[]{new[]{"key","value"},new[]{"source","Synthetic"},new[]{"query_date","2026-10-15"},new[]{"travel_mode","test"},new[]{"route_options","test"}});
        var parsed=ResearchCsv.Load(n,m,meta);parsed.Validate(5);Check(parsed.Nodes[0].AccessNote=="Yol; bağlantısı"&&parsed.Km[0,1]==1.25&&parsed.Km[1,0]==2.5,"CSV roundtrip");
        Directory.Delete(dir,true);Console.WriteLine("PASS: "+checks+" deterministic checks. Synthetic data only; no real-world performance claim.");
    }
}
