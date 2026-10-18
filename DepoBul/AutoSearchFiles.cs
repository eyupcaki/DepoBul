using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DepoBul.Research
{
    public static class AutoSearchFiles
    {
        static string N(double x){return x.ToString("R",CultureInfo.InvariantCulture);}
        public static Location[] LoadStores(string path,int capacity)
        {
            var rows=ResearchCsv.Read(path);
            if(String.Join(";",rows[0])!="id;kind;latitude;longitude;demand_pallet;access_confirmed;access_note")throw new FormatException("Mağaza CSV başlığı pilot dosyasındaki biçimle aynı olmalı.");
            if(rows.Skip(1).Any(r=>r.Length!=7 || (r[1]!="store"&&r[1]!="depot")))throw new FormatException("Konum CSV sütunları veya türü geçersiz.");
            var stores=rows.Skip(1).Where(r=>r[1]=="store").Select(r=>new Location{Id=r[0],Kind="store",
                Latitude=Double.Parse(r[2],CultureInfo.InvariantCulture),Longitude=Double.Parse(r[3],CultureInfo.InvariantCulture),Demand=Int32.Parse(r[4],CultureInfo.InvariantCulture)}).ToArray();
            AutoDepotSearch.ValidateStores(stores,capacity);return stores;
        }
        public static void WriteData(string directory,RoadData data)
        {
            Directory.CreateDirectory(directory);
            var nodes=new List<string[]>{new[]{"id","kind","latitude","longitude","demand_pallet","access_confirmed","access_note"}};
            foreach(var n in data.Nodes)nodes.Add(new[]{n.Id,n.Kind,N(n.Latitude),N(n.Longitude),n.Demand.ToString(),n.AccessConfirmed.ToString(),n.AccessNote});
            ResearchCsv.Write(Path.Combine(directory,"konumlar.csv"),nodes);
            var distances=new List<string[]>{new[]{"from_id","to_id","distance_km"}};
            for(int i=0;i<data.Nodes.Length;i++)for(int j=0;j<data.Nodes.Length;j++)distances.Add(new[]{data.Nodes[i].Id,data.Nodes[j].Id,N(data.Km[i,j])});
            ResearchCsv.Write(Path.Combine(directory,"mesafeler.csv"),distances);
            ResearchCsv.Write(Path.Combine(directory,"kaynak.csv"),new[]{new[]{"key","value"},new[]{"source",data.Source},new[]{"query_date",data.QueryDate},new[]{"travel_mode",data.TravelMode},new[]{"route_options",data.RouteOptions}});
        }
        public static void Export(string directory,AutoSearchResult result)
        {
            Directory.CreateDirectory(directory);WriteData(directory,result.Winner.Data);
            var visits=new List<string[]>{new[]{"level","anchor_latitude","anchor_longitude","status","depot_id","detail"}};
            foreach(var v in result.Visits)visits.Add(new[]{v.Level.ToString(),N(v.Anchor.Latitude),N(v.Anchor.Longitude),v.Status,v.Candidate==null?"":v.Candidate.Depot.Id,v.Detail});
            ResearchCsv.Write(Path.Combine(directory,"arama_izleri.csv"),visits);
            var summary=new List<string[]>{new[]{"depot_id","latitude","longitude","screen_runs","screen_mean_F","final_runs","final_mean_F","final_sd_F","selected"}};
            var runs=new List<string[]>{new[]{"phase","depot_id","seed","distance_km","vehicles","score","seconds","routes"}};
            var traces=new List<string[]>{new[]{"depot_id","seed","generation","best_score"}};
            foreach(var c in result.Candidates)
            {
                WriteData(Path.Combine(directory,"konum_degerlendirmeleri",c.Depot.Id),c.Data);
                summary.Add(new[]{c.Depot.Id,N(c.Depot.Latitude),N(c.Depot.Longitude),c.ScreeningRuns.Count.ToString(),N(c.ScreeningMean),c.FinalRuns==null?"0":c.FinalRuns.Count.ToString(),c.FinalRuns==null?"":N(c.FinalMean),c.FinalRuns==null?"":N(Sd(c.FinalRuns.Select(r=>r.Score))),Object.ReferenceEquals(c,result.Winner).ToString()});
                foreach(var phase in new[]{new{Key="screening",Values=c.ScreeningRuns},new{Key="final",Values=c.FinalRuns}})
                {
                    if(phase.Values==null)continue;
                    foreach(var x in phase.Values)
                    {
                        runs.Add(new[]{phase.Key,c.Depot.Id,x.Seed.ToString(),N(x.DistanceKm),x.Routes.Count.ToString(),N(x.Score),N(x.Seconds),String.Join(" | ",x.Routes.Select(r=>c.Depot.Id+">"+String.Join(">",r.Select(i=>c.Data.Nodes[i].Id))+">"+c.Depot.Id))});
                        if(phase.Key=="final")for(int generation=0;generation<x.BestByGeneration.Length;generation++)traces.Add(new[]{c.Depot.Id,x.Seed.ToString(),generation.ToString(),N(x.BestByGeneration[generation])});
                    }
                }
            }
            ResearchCsv.Write(Path.Combine(directory,"konum_ozeti.csv"),summary);
            ResearchCsv.Write(Path.Combine(directory,"ga_kosumlari.csv"),runs);
            ResearchCsv.Write(Path.Combine(directory,"yakinsama.csv"),traces);
            var o=result.Options;
            ResearchCsv.Write(Path.Combine(directory,"parametreler.csv"),new[]{new[]{"key","value"},new[]{"method","weighted_multires_local_ga_v1"},new[]{"capacity",o.Final.Capacity.ToString()},new[]{"max_vehicles",o.Final.MaxVehicles.ToString()},new[]{"lambda",N(o.Final.ExtraVehiclePenalty)}});
            File.WriteAllText(Path.Combine(directory,"ayarlar.txt"),
                "Yöntem: talep ağırlıklı merkez + azalan adımlı yerel konum araması + her konum için GA.\r\n"+
                "Her düzeyde merkez ve kuzey/güney/doğu/batı; düzey başına bir konum güncellemesi.\r\n"+
                "Adımlar(km)="+String.Join(",",o.StepsKm.Select(N))+"; en çok başlangıç-yol eşleştirme(m)="+N(o.MaxAnchorSnapMetres)+"; matris uç eşleştirmesi en çok 100m.\r\n"+
                "Ortak: kapasite="+o.Final.Capacity+"; filo="+o.Final.MaxVehicles+"; lambda="+N(o.Final.ExtraVehiclePenalty)+"; F=D+lambda*(K-1).\r\n"+
                "Tarama: popülasyon="+o.Screening.Population+"; nesil="+o.Screening.Generations+"; mutasyon="+N(o.Screening.MutationProbability)+"; tohumlar="+String.Join(",",o.ScreeningSeeds)+"\r\n"+
                "Son değerlendirme: en iyi "+o.Finalists+" tarama konumu; popülasyon="+o.Final.Population+"; nesil="+o.Final.Generations+"; mutasyon="+N(o.Final.MutationProbability)+"; tohumlar="+String.Join(",",o.FinalSeeds)+"\r\n"+
                "Konum seçimi ortalama F ile; haritada seçilen konumun en düşük F'li koşumu gösterilir. Tarama ve son koşumlar ayrı tutulur.\r\n"+
                "Başlangıç merkezi="+N(result.InitialCentre.Latitude)+","+N(result.InitialCentre.Longitude)+"\r\n"+
                "İlk başarılı table sorgusunun mağaza-mağaza matrisi arama boyunca sabittir. Yeni konumların depo bacakları yeni sorgulardan alınır. Ham sorgular osrm_kayitlari içindedir.\r\n"+
                "Arama küresel optimum veya tüm bölgenin tarandığı iddiası değildir. Yol ağına bağlı nokta parsel/imar/yükleme alanı uygunluğu değildir. Talepler ve kapasite senaryodur.\r\n",Encoding.UTF8);
            var chosenRows=new List<string[]>{new[]{"depot_id","seed","distance_km","vehicles","score","seconds","routes"}};
            foreach(var run in result.Winner.FinalRuns)
                chosenRows.Add(new[]{result.Winner.Depot.Id,run.Seed.ToString(),N(run.DistanceKm),run.Routes.Count.ToString(),N(run.Score),N(run.Seconds),String.Join(" | ",run.Routes.Select(route=>result.Winner.Depot.Id+">"+String.Join(">",route.Select(i=>result.Winner.Data.Nodes[i].Id))+">"+result.Winner.Depot.Id))});
            ResearchCsv.Write(Path.Combine(directory,"secilen_depo_kosumlari.csv"),chosenRows);
            var nn=result.NearestNeighbour;var best=result.BestSolution;
            ResearchCsv.Write(Path.Combine(directory,"en_yakin_komsu.csv"),new[]{new[]{"depot_id","nn_distance_km","nn_vehicles","nn_F","ga_best_F","ga_mean_F","ga_mean_improvement_percent"},new[]{result.Winner.Depot.Id,N(nn.DistanceKm),nn.Routes.Count.ToString(),N(nn.Score),N(best.Score),N(result.Winner.FinalMean),RoadData.Finite(nn.Score)&&nn.Score>0?N(100*(nn.Score-result.Winner.FinalMean)/nn.Score):""}});
            File.WriteAllText(Path.Combine(directory,"OZET.txt"),"Önerilen depo: "+result.Winner.Depot.Id+"\r\nKoordinatlar: "+N(result.Winner.Depot.Latitude)+", "+N(result.Winner.Depot.Longitude)+"\r\nÜretilip değerlendirilen farklı konum: "+result.Candidates.Count+"\r\nSon ortalama F: "+N(result.Winner.FinalMean)+"\r\nEn iyi koşum mesafesi(km): "+N(best.DistanceKm)+"\r\nBu koşumdaki araç: "+best.Routes.Count+"\r\nMakale öncesi saha/faaliyet ve veri tutarlılığı incelemesi gereklidir. Bu bir pilot dağıtım senaryosudur.\r\n",Encoding.UTF8);
        }
        public static double Sd(IEnumerable<double> values)
        {var a=values.ToArray();double mean=a.Average();return a.Length<2?0:Math.Sqrt(a.Sum(x=>(x-mean)*(x-mean))/(a.Length-1));}
    }
}
