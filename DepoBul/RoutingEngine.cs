using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DepoBul.Research
{
    public sealed class Location
    {
        public string Id, Kind, AccessNote;
        public double Latitude, Longitude;
        public int Demand;
        public bool AccessConfirmed;
    }

    public sealed class RoadData
    {
        public Location[] Nodes;
        public double[,] Km;
        public string Source, QueryDate, TravelMode, RouteOptions;

        public void Validate(int capacity)
        {
            if (Nodes == null || Nodes.Length < 2 || capacity <= 0)
                throw new ArgumentException("Konumlar ve pozitif araç kapasitesi gerekli.");
            if (Nodes.Select(n => n.Id).Distinct(StringComparer.Ordinal).Count() != Nodes.Length)
                throw new ArgumentException("Konum kodları benzersiz olmalı.");
            if (!Nodes.Any(n => n.Kind == "store") || !Nodes.Any(n => n.Kind == "depot"))
                throw new ArgumentException("En az bir mağaza ve aday depo gerekli.");
            if (Km == null || Km.GetLength(0) != Nodes.Length || Km.GetLength(1) != Nodes.Length)
                throw new ArgumentException("Mesafe matrisi boyutu konum listesiyle uyuşmuyor.");
            if (String.IsNullOrWhiteSpace(Source) || String.IsNullOrWhiteSpace(QueryDate) ||
                String.IsNullOrWhiteSpace(TravelMode) || String.IsNullOrWhiteSpace(RouteOptions))
                throw new ArgumentException("Mesafe kaynağı, sorgu tarihi, ulaşım türü ve güzergâh tercihleri gerekli.");
            for (int i = 0; i < Nodes.Length; i++)
            {
                var n = Nodes[i];
                if (String.IsNullOrWhiteSpace(n.Id) || (n.Kind != "store" && n.Kind != "depot"))
                    throw new ArgumentException("Geçersiz konum kodu/türü.");
                if (!Finite(n.Latitude) || !Finite(n.Longitude) || Math.Abs(n.Latitude) > 90 || Math.Abs(n.Longitude) > 180)
                    throw new ArgumentException("Geçersiz koordinat: " + n.Id);
                if (n.Kind == "store" && (n.Demand <= 0 || n.Demand > capacity))
                    throw new ArgumentException("Talep pozitif ve kapasiteden küçük/eşit olmalı: " + n.Id);
                if (n.Kind == "depot" && (n.Demand != 0 || !n.AccessConfirmed || String.IsNullOrWhiteSpace(n.AccessNote)))
                    throw new ArgumentException("Aday depo için sıfır talep, yol bağlantısı onayı ve dayanağı gerekli: " + n.Id);
                for (int j = 0; j < Nodes.Length; j++)
                {
                    // Depot-to-depot legs are never used. All store legs remain directional.
                    if (i != j && n.Kind == "depot" && Nodes[j].Kind == "depot") continue;
                    double v = Km[i,j];
                    if (!Finite(v) || v < 0 || (i == j && v != 0) || (i != j && v == 0))
                        throw new ArgumentException("Eksik/geçersiz yol mesafesi: " + n.Id + " -> " + Nodes[j].Id);
                }
            }
        }
        public static bool Finite(double v) { return !Double.IsNaN(v) && !Double.IsInfinity(v); }
    }

    public sealed class GaOptions
    {
        public int Population = 300, Generations = 200, Capacity = 20, MaxVehicles = 1000;
        public double ExtraVehiclePenalty = 2000, MutationProbability = 0.2;
        public void Validate()
        {
            if (Population < 2 || Generations < 1 || Capacity < 1 || MaxVehicles < 1 ||
                !RoadData.Finite(ExtraVehiclePenalty) || ExtraVehiclePenalty < 0 ||
                !RoadData.Finite(MutationProbability) || MutationProbability < 0 || MutationProbability > 1)
                throw new ArgumentException("Geçersiz GA parametreleri.");
        }
    }

    public sealed class RouteSolution
    {
        public int DepotIndex, Seed;
        public int[] Order;
        public List<int[]> Routes = new List<int[]>();
        public double DistanceKm, Score, Seconds;
        public double[] BestByGeneration;
    }

    public static class RoutingEngine
    {
        public static RouteSolution Evaluate(RoadData data, int depot, int[] order, GaOptions opt)
        {
            if (depot < 0 || depot >= data.Nodes.Length || data.Nodes[depot].Kind != "depot")
                throw new ArgumentException("Geçersiz depo.");
            var expected = Enumerable.Range(0,data.Nodes.Length).Where(i => data.Nodes[i].Kind == "store").ToArray();
            if (order == null || !order.OrderBy(i => i).SequenceEqual(expected))
                throw new ArgumentException("Her mağaza kromozomda tam bir kez yer almalı.");
            var result = new RouteSolution { DepotIndex = depot, Order = (int[])order.Clone() };
            var route = new List<int>();
            int load = 0;
            foreach (int customer in order)
            {
                int demand = data.Nodes[customer].Demand;
                if (demand <= 0 || demand > opt.Capacity) throw new ArgumentException("Geçersiz mağaza talebi.");
                if (route.Count > 0 && demand > opt.Capacity - load)
                { result.Routes.Add(route.ToArray()); route.Clear(); load = 0; }
                route.Add(customer); load += demand;
            }
            if (route.Count > 0) result.Routes.Add(route.ToArray());
            foreach (var r in result.Routes)
            {
                int previous = depot;
                foreach (int customer in r)
                { result.DistanceKm += data.Km[previous,customer]; previous = customer; }
                result.DistanceKm += data.Km[previous,depot];
            }
            result.Score = result.Routes.Count > opt.MaxVehicles ? Double.PositiveInfinity :
                result.DistanceKm + opt.ExtraVehiclePenalty * (result.Routes.Count - 1);
            return result;
        }

        // Deterministic capacity-feasible nearest neighbour, one run per depot.
        // Choose among unvisited customers that fit; otherwise return and start a new vehicle.
        // Equal directed distances are resolved by ordinal store ID.
        public static RouteSolution NearestNeighbour(RoadData data, int depot, GaOptions opt)
        {
            opt.Validate(); data.Validate(opt.Capacity);
            if(depot<0 || depot>=data.Nodes.Length || data.Nodes[depot].Kind!="depot")
                throw new ArgumentException("Geçersiz depo.");
            var watch=Stopwatch.StartNew();
            var remaining=Enumerable.Range(0,data.Nodes.Length).Where(i=>data.Nodes[i].Kind=="store").ToList();
            var order=new List<int>(); int current=depot, load=0;
            while(remaining.Count>0)
            {
                var fitting=remaining.Where(i=>data.Nodes[i].Demand<=opt.Capacity-load)
                    .OrderBy(i=>data.Km[current,i]).ThenBy(i=>data.Nodes[i].Id,StringComparer.Ordinal).ToArray();
                if(fitting.Length==0) {current=depot;load=0;continue;}
                int next=fitting[0];order.Add(next);remaining.Remove(next);load+=data.Nodes[next].Demand;current=next;
            }
            var result=Evaluate(data,depot,order.ToArray(),opt);
            // An infeasible fleet limit remains an infinite score, not a valid comparator.
            result.Seed=0; result.Seconds=watch.Elapsed.TotalSeconds;
            result.BestByGeneration=new double[0]; return result;
        }

        public static int[] Crossover(int[] first, int[] second, int prefixLength)
        {
            var child = first.Take(prefixLength).ToList();
            var used = new HashSet<int>(child);
            foreach (int gene in second) if (used.Add(gene)) child.Add(gene);
            return child.ToArray();
        }
        public static void SwapMutation(int[] order, Random rng)
        {
            if (order.Length < 2) return;
            int a = rng.Next(order.Length), b = rng.Next(order.Length - 1);
            if (b >= a) b++;
            int temp = order[a]; order[a] = order[b]; order[b] = temp;
        }
        private static int[] Shuffled(int[] input, Random rng)
        {
            int[] x = (int[])input.Clone();
            for (int i=x.Length-1;i>0;i--) { int j=rng.Next(i+1); int t=x[i];x[i]=x[j];x[j]=t; }
            return x;
        }
        private static RouteSolution Tournament(List<RouteSolution> pop, Random rng)
        {
            var a=pop[rng.Next(pop.Count)];var b=pop[rng.Next(pop.Count)];
            return a.Score <= b.Score ? a : b;
        }
        public static RouteSolution Run(RoadData data, int depot, GaOptions opt, int seed, CancellationToken token)
        {
            opt.Validate(); data.Validate(opt.Capacity);
            var watch=Stopwatch.StartNew(); var rng=new Random(seed);
            int[] stores=Enumerable.Range(0,data.Nodes.Length).Where(i=>data.Nodes[i].Kind=="store").ToArray();
            var population=new List<RouteSolution>();
            for(int i=0;i<opt.Population;i++) population.Add(Evaluate(data,depot,Shuffled(stores,rng),opt));
            var trace=new double[opt.Generations+1];
            population=population.OrderBy(s=>s.Score).ThenBy(s=>s.DistanceKm).ToList();trace[0]=population[0].Score;
            for(int generation=0;generation<opt.Generations;generation++)
            {
                token.ThrowIfCancellationRequested();
                var next=new List<RouteSolution>();
                // One copied elite. Children never mutate parents or the elite.
                next.Add(Evaluate(data,depot,population[0].Order,opt));
                while(next.Count<opt.Population)
                {
                    var p=Tournament(population,rng);var q=Tournament(population,rng);
                    int cut=stores.Length==1?1:rng.Next(1,stores.Length);
                    int[] child=Crossover(p.Order,q.Order,cut);
                    if(rng.NextDouble()<opt.MutationProbability) SwapMutation(child,rng);
                    next.Add(Evaluate(data,depot,child,opt));
                }
                population=next.OrderBy(s=>s.Score).ThenBy(s=>s.DistanceKm).ToList();
                trace[generation+1]=population[0].Score;
            }
            var best=population[0];
            if(!RoadData.Finite(best.Score)) throw new InvalidOperationException("Araç sınırını sağlayan çözüm bulunamadı. Filo sınırını ve talepleri inceleyin.");
            best.Seed=seed;best.BestByGeneration=trace;best.Seconds=watch.Elapsed.TotalSeconds;return best;
        }
        public static List<RouteSolution> Compare(RoadData data, GaOptions opt, int[] seeds,
            CancellationToken token, Action<int,int> progress)
        {
            if(seeds==null || seeds.Length==0 || seeds.Distinct().Count()!=seeds.Length)
                throw new ArgumentException("Tohumlar boş olamaz ve tekrar etmemeli.");
            opt.Validate();data.Validate(opt.Capacity);
            var depots=Enumerable.Range(0,data.Nodes.Length).Where(i=>data.Nodes[i].Kind=="depot").ToArray();
            var result=new List<RouteSolution>();int total=depots.Length*seeds.Length;
            foreach(int c in depots) foreach(int seed in seeds)
            {
                token.ThrowIfCancellationRequested();result.Add(Run(data,c,opt,seed,token));
                if(progress!=null) progress(result.Count,total);
            }
            return result;
        }
    }

    public static class ResearchCsv
    {
        // UTF-8 semicolon-separated files; invariant decimal point. Quotes supported.
        public static List<string[]> Read(string path)
        {
            var rows=new List<string[]>();
            foreach(string line in File.ReadAllLines(path,Encoding.UTF8))
            {
                if(String.IsNullOrWhiteSpace(line))continue;
                var fields=new List<string>();var text=new StringBuilder();bool quoted=false;
                for(int i=0;i<line.Length;i++)
                {
                    char c=line[i];
                    if(c=='"') { if(quoted&&i+1<line.Length&&line[i+1]=='"'){text.Append('"');i++;}else quoted=!quoted; }
                    else if(c==';'&&!quoted){fields.Add(text.ToString());text.Clear();}else text.Append(c);
                }
                if(quoted)throw new FormatException("Kapanmamış CSV tırnağı. Hücre içinde satır sonu desteklenmiyor.");
                fields.Add(text.ToString());rows.Add(fields.ToArray());
            }
            if(rows.Count==0)throw new FormatException("CSV dosyası boş.");return rows;
        }
        static double Number(string v){return Double.Parse(v,NumberStyles.Float,CultureInfo.InvariantCulture);}
        public static RoadData Load(string nodesPath,string distancesPath,string metadataPath)
        {
            var n=Read(nodesPath);var nodes=new List<Location>();
            if(String.Join(";",n[0])!="id;kind;latitude;longitude;demand_pallet;access_confirmed;access_note")
                throw new FormatException("Konum CSV başlığı örnek şablonla aynı olmalı.");
            foreach(var r in n.Skip(1))
            {
                if(r.Length!=7)throw new FormatException("Konum CSV: 7 sütun gerekli.");
                bool access;if(!Boolean.TryParse(r[5],out access))throw new FormatException("access_confirmed true/false olmalı.");
                nodes.Add(new Location{Id=r[0].Trim(),Kind=r[1].Trim(),Latitude=Number(r[2]),Longitude=Number(r[3]),Demand=Int32.Parse(r[4],CultureInfo.InvariantCulture),AccessConfirmed=access,AccessNote=r[6]});
            }
            if(nodes.Select(x=>x.Id).Distinct().Count()!=nodes.Count)throw new FormatException("Konum kodları tekrar ediyor.");
            var index=nodes.Select((v,i)=>new{v.Id,i}).ToDictionary(v=>v.Id,v=>v.i);
            var km=new double[nodes.Count,nodes.Count];
            for(int i=0;i<nodes.Count;i++)for(int j=0;j<nodes.Count;j++)km[i,j]=i==j?0:Double.NaN;
            var seen=new HashSet<string>();var distances=Read(distancesPath);
            if(String.Join(";",distances[0])!="from_id;to_id;distance_km")throw new FormatException("Mesafe CSV başlığı hatalı.");
            foreach(var r in distances.Skip(1))
            {
                if(r.Length!=3||!index.ContainsKey(r[0])||!index.ContainsKey(r[1]))throw new FormatException("Mesafe CSV: bilinmeyen kod/sütun.");
                int a=index[r[0]],b=index[r[1]];
                if(!seen.Add(a+":"+b))throw new FormatException("Tekrarlı yönlü mesafe: "+r[0]+" -> "+r[1]);
                km[a,b]=Number(r[2]);
            }
            var metaRows=Read(metadataPath);
            if(String.Join(";",metaRows[0])!="key;value"||metaRows.Skip(1).Any(r=>r.Length!=2))throw new FormatException("Üstveri CSV biçimi key;value olmalı.");
            var meta=metaRows.Skip(1).ToDictionary(r=>r[0],r=>r[1]);
            foreach(string key in new[]{"source","query_date","travel_mode","route_options"})
                if(!meta.ContainsKey(key))throw new FormatException("Eksik üstveri: "+key);
            DateTime date;if(!DateTime.TryParseExact(meta["query_date"],"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date))
                throw new FormatException("Sorgu tarihi YYYY-MM-DD olmalı.");
            return new RoadData{Nodes=nodes.ToArray(),Km=km,Source=meta["source"],QueryDate=meta["query_date"],TravelMode=meta["travel_mode"],RouteOptions=meta["route_options"]};
        }
        public static List<RouteSolution> LoadSavedForMap(string path,RoadData data,GaOptions opt)
        {
            data.Validate(opt.Capacity);opt.Validate();var rows=Read(path);
            if(String.Join(";",rows[0])!="depot_id;seed;distance_km;vehicles;score;seconds;routes")
                throw new FormatException("Kaydedilmiş koşum CSV başlığı geçersiz.");
            var index=data.Nodes.Select((n,i)=>new{n.Id,i}).ToDictionary(n=>n.Id,n=>n.i);
            var list=new List<RouteSolution>();var keys=new HashSet<string>();
            foreach(var row in rows.Skip(1))
            {
                if(row.Length!=7 || !index.ContainsKey(row[0]) || !keys.Add(row[0]+"|"+row[1]))
                    throw new FormatException("Koşum kaydı eksik, bilinmeyen veya tekrarlı.");
                int depot=index[row[0]];var routes=new List<int[]>();
                foreach(string text in row[6].Split('|'))
                {
                    var ids=text.Trim().Split('>');
                    if(ids.Length<3 || ids[0]!=row[0] || ids[ids.Length-1]!=row[0] || ids.Any(id=>!index.ContainsKey(id)))
                        throw new FormatException("Kayıtlı rota depo başlangıç/dönüşü veya kodları geçersiz.");
                    routes.Add(ids.Skip(1).Take(ids.Length-2).Select(id=>index[id]).ToArray());
                }
                var result=RoutingEngine.Evaluate(data,depot,routes.SelectMany(r=>r).ToArray(),opt);
                if(!RoadData.Finite(result.Score) || result.Routes.Count!=routes.Count ||
                    result.Routes.Where((r,i)=>!r.SequenceEqual(routes[i])).Any() ||
                    routes.Count!=Int32.Parse(row[3],CultureInfo.InvariantCulture) ||
                    Math.Abs(result.DistanceKm-Number(row[2]))>0.00001 || Math.Abs(result.Score-Number(row[4]))>0.00001 ||
                    !RoadData.Finite(Number(row[2])) || !RoadData.Finite(Number(row[4])))
                    throw new FormatException("Kayıtlı sonuç seçili matris, kapasite veya ceza ile uyuşmuyor.");
                result.Seed=Int32.Parse(row[1],CultureInfo.InvariantCulture);result.BestByGeneration=new double[0];list.Add(result);
            }
            if(list.Count==0)throw new FormatException("Koşum kaydı boş.");return list;
        }
        public static string Cell(string v){return "\""+(v??"").Replace("\"","\"\"").Replace("\r"," ").Replace("\n"," ")+"\"";}
        public static void Write(string path,IEnumerable<string[]> rows)
        {File.WriteAllLines(path,rows.Select(r=>String.Join(";",r.Select(Cell))),new UTF8Encoding(true));}
    }
}
