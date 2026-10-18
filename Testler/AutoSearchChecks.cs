using System;using System.Collections.Generic;using System.Globalization;using System.IO;using System.Linq;using System.Threading;using DepoBul.Research;
class AutoSearchChecks
{
 static int checks;
 static void Check(bool c,string message){if(!c)throw new Exception(message);checks++;}
 static void Throws(Action action,string message){try{action();}catch(Exception){checks++;return;}throw new Exception(message);}
 class FakeRoad : IAutoRoadProvider
 {
  public bool Duplicate,Reject;public int Builds;
  public Location Snap(GeoPoint a,string id,double max,CancellationToken t){t.ThrowIfCancellationRequested();if(Reject)throw new RoadPointRejectedException("fixture unreachable");return new Location{Id=id,Kind="depot",Demand=0,AccessConfirmed=true,AccessNote="synthetic fixture",Latitude=Duplicate?37:a.Latitude,Longitude=Duplicate?38:a.Longitude};}
  public RoadData Build(Location[] stores,Location depot,CancellationToken token){Builds++;var ns=stores.Concat(new[]{depot}).ToArray();var km=new double[ns.Length,ns.Length];for(int i=0;i<ns.Length;i++)for(int j=0;j<ns.Length;j++)km[i,j]=i==j?0:10+Math.Abs(ns[i].Latitude-ns[j].Latitude)*100+Math.Abs(ns[i].Longitude-ns[j].Longitude)*80+i*.2+j*.1;return new RoadData{Nodes=ns,Km=km,Source="synthetic",QueryDate="2026-10-15",TravelMode="fixture",RouteOptions="fixture"};}
 }
 static Location[] Stores(){return new[]{new Location{Id="S1",Kind="store",Latitude=37,Longitude=38,Demand=2},new Location{Id="S2",Kind="store",Latitude=38,Longitude=39,Demand=6},new Location{Id="S3",Kind="store",Latitude=37.5,Longitude=38,Demand=2}};}
 static AutoSearchOptions Options(){return new AutoSearchOptions{StepsKm=new[]{20.0,10.0},Screening=new GaOptions{Capacity=8,MaxVehicles=3,ExtraVehiclePenalty=100,Population=8,Generations=4},Final=new GaOptions{Capacity=8,MaxVehicles=3,ExtraVehiclePenalty=100,Population=8,Generations=4},ScreeningSeeds=new[]{1,2},FinalSeeds=new[]{1001,1002,1003},Finalists=2};}
 static string TableJson(int n,double storeValue,bool missing,bool far)
 {
  string[] w=Enumerable.Range(0,n).Select(i=>"{\"distance\":"+(far&&i==0?"500":"0")+",\"location\":[38,37]}").ToArray();
  string[] rows=Enumerable.Range(0,n).Select(i=>"["+String.Join(",",Enumerable.Range(0,n).Select(j=>i==j?"0":missing&&i==0&&j==1?"null":(i<n-1&&j<n-1?storeValue*1000:5000).ToString(CultureInfo.InvariantCulture)))+"]").ToArray();
  return "{\"code\":\"Ok\",\"sources\":["+String.Join(",",w)+"],\"destinations\":["+String.Join(",",w)+"],\"distances\":["+String.Join(",",rows)+"]}";
 }
 static void Main()
 {
  var stores=Stores();var c=AutoDepotSearch.WeightedCentre(stores);Check(Math.Abs(c.Latitude-37.7)<1e-9,"Demand weighted latitude");Check(Math.Abs(c.Longitude-38.6)<1e-9,"Demand weighted longitude");
  var ring=AutoDepotSearch.Neighbourhood(c,20).ToArray();Check(ring.Length==5,"Generated cross");Check(ring[1].Latitude>c.Latitude&&ring[2].Latitude<c.Latitude&&ring[3].Longitude>c.Longitude&&ring[4].Longitude<c.Longitude,"Directions");
  var opt=Options();var road=new FakeRoad();var result=AutoDepotSearch.Run(stores,opt,road,CancellationToken.None,null);
  Check(result.Visits.Count==10,"Two levels with five evaluations each");Check(result.Candidates.Count>=5,"Positions generated without depot input");Check(result.Candidates.Count<=10,"Finite search budget");Check(result.Candidates.All(x=>x.Depot.Id.StartsWith("AUTO")),"Auto depot IDs");
  Check(result.Candidates.All(x=>x.ScreeningRuns.Count==2),"Equal screening budget");Check(result.Candidates.Count(x=>x.FinalRuns!=null)==2,"Top shortlist only");Check(result.Candidates.Where(x=>x.FinalRuns!=null).All(x=>x.FinalRuns.Select(y=>y.Seed).SequenceEqual(opt.FinalSeeds)),"Equal final seeds");Check(result.Winner.FinalMean==result.Candidates.Where(x=>x.FinalRuns!=null).Min(x=>x.FinalMean),"Mean final criterion");Check(result.BestSolution.Score==result.Winner.FinalRuns.Min(x=>x.Score),"Best display at selected depot");
  foreach(var x in result.Candidates)foreach(var run in x.ScreeningRuns){Check(run.Routes.SelectMany(r=>r).Distinct().Count()==3,"Each store once");Check(run.Routes.All(r=>r.Sum(i=>x.Data.Nodes[i].Demand)<=8),"Capacity");}
  var again=AutoDepotSearch.Run(stores,Options(),new FakeRoad(),CancellationToken.None,null);Check(again.Winner.Depot.Latitude==result.Winner.Depot.Latitude && again.BestSolution.Score==result.BestSolution.Score,"Deterministic search on fixed provider");
  var dup=new FakeRoad{Duplicate=true};var d=AutoDepotSearch.Run(stores,Options(),dup,CancellationToken.None,null);Check(d.Candidates.Count==1 && dup.Builds==1,"Deduplicate snapped points");
  Throws(()=>AutoDepotSearch.Run(stores,Options(),new FakeRoad{Reject=true},CancellationToken.None,null),"No feasible point aborts");var cancel=new CancellationTokenSource();cancel.Cancel();Throws(()=>AutoDepotSearch.Run(stores,Options(),new FakeRoad(),cancel.Token,null),"Cancellation");
  var bad=Options();bad.StepsKm=new[]{10.0,20.0};Throws(()=>bad.Validate(),"Nondecreasing steps");bad=Options();bad.Final.ExtraVehiclePenalty=101;Throws(()=>bad.Validate(),"Changing objective between phases");
  Throws(()=>AutoDepotSearch.ValidateStores(new[]{stores[0],stores[0]},8),"Duplicate store IDs");Throws(()=>AutoDepotSearch.ValidateStores(stores,5),"Demand over capacity");
  string root=Path.Combine(Path.GetTempPath(),"depobul_auto_tests_"+Guid.NewGuid().ToString("N"));AutoSearchFiles.Export(root,result);Check(File.Exists(Path.Combine(root,"ga_kosumlari.csv"))&&File.Exists(Path.Combine(root,"yakinsama.csv")),"Exports");var restored=ResearchCsv.Load(Path.Combine(root,"konumlar.csv"),Path.Combine(root,"mesafeler.csv"),Path.Combine(root,"kaynak.csv"));restored.Validate(8);Check(restored.Nodes.Count(n=>n.Kind=="depot")==1,"Only generated selected depot exported");
  var equal=new[]{new Location{Id="A",Kind="store",Latitude=37,Longitude=38,Demand=1},new Location{Id="B",Kind="store",Latitude=37,Longitude=38,Demand=1}};var depot=new Location{Id="D",Kind="depot",Latitude=37,Longitude=38,Demand=0,AccessConfirmed=true,AccessNote="fixture"};int request=0;
  var provider=new OsrmDataProvider(root+"_provider_"+Guid.NewGuid().ToString("N"),(url,t)=>TableJson(3,++request==1?10:20,false,false));var one=provider.Build(equal,depot,CancellationToken.None);depot=new Location{Id="E",Kind="depot",Latitude=37.0001,Longitude=38,Demand=0,AccessConfirmed=true,AccessNote="fixture"};
  // A different depot produces a new table; store-to-store costs must stay frozen.
  var two=provider.Build(equal,depot,CancellationToken.None);Check(two.Km[0,1]==10 && one.Km[0,1]==10,"Stable store matrix across distinct requests");
  provider.Build(equal,depot,CancellationToken.None);Check(request==2,"Exact request replay uses cache");
  Throws(()=>OsrmDataProvider.CheckWaypoint(new GeometryWaypoint{Location=new[]{39.0,38.0},Distance=0},37,38,100),"Actual separation checked, not just reported distance");
  var nulls=new OsrmDataProvider(root+"_null_"+Guid.NewGuid().ToString("N"),(url,t)=>TableJson(3,10,true,false));Throws(()=>nulls.Build(equal,depot,CancellationToken.None),"Disconnected directed matrix rejected");
  var far=new OsrmDataProvider(root+"_far_"+Guid.NewGuid().ToString("N"),(url,t)=>TableJson(3,10,false,true));Throws(()=>far.Build(equal,depot,CancellationToken.None),"Far snaps rejected");
  Console.WriteLine("PASS: "+checks+" checks; generated neighbourhoods, mean selection, repeated seeds, deduplication, capacity, cancellation, road rejections, caching and exports. Synthetic fixtures only.");
 }
}
