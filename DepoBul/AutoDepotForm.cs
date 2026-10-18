using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DepoBul.Research;

namespace DepoBul
{
    public sealed class AutoDepotForm : Form
    {
        readonly TextBox file=new TextBox{Width=820},steps=new TextBox{Text="40,20,10",Width=130};
        readonly NumericUpDown capacity=Number(1,100000,20),fleet=Number(1,1000,28),penalty=Number(0,100000,100),
            finalRuns=Number(1,100,30),finalists=Number(1,10,3);
        readonly Button run=new Button{Text="Depo konumunu araştır",AutoSize=true},cancel=new Button{Text="Durdur",Enabled=false,AutoSize=true},
            map=new Button{Text="Önerilen depoyu haritada göster",Enabled=false,AutoSize=true},folder=new Button{Text="Sonuç klasörünü aç",Enabled=false,AutoSize=true};
        readonly Label status=new Label{AutoSize=true,MaximumSize=new Size(1100,0),Text="Yalnız mağaza konumları ve palet talepleri gerekir. Hazır depo listesi kullanılmaz."};
        readonly DataGridView grid=new DataGridView{Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells};
        readonly TextBox detail=new TextBox{Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Both,WordWrap=false};
        readonly TableLayoutPanel top=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=1};
        Button browse;
        CancellationTokenSource cancellation;
        AutoSearchResult result;
        RoadData replayData;
        RouteSolution replaySolution;
        string exampleDirectory;
        readonly Button replay=new Button{Text="Kayıtlı otomatik sonucu aç",AutoSize=true};
        public RoadData SelectedData {get{return result==null?replayData:result.Winner.Data;}}
        public RouteSolution SelectedSolution {get{return result==null?replaySolution:result.BestSolution;}}
        public string RunDirectory {get;private set;}
        static NumericUpDown Number(decimal min,decimal max,decimal value){return new NumericUpDown{Minimum=min,Maximum=max,Value=value,Width=80};}
        public AutoDepotForm()
        {
            Text="DepoBul — otomatik depo konumu araştırması";Size=new Size(1240,800);MinimumSize=new Size(950,650);StartPosition=FormStartPosition.CenterParent;
            var files=new FlowLayoutPanel{AutoSize=true,Dock=DockStyle.Top};files.Controls.Add(new Label{Text="Mağaza CSV",AutoSize=true,Margin=new Padding(4,8,4,0)});files.Controls.Add(file);
            browse=new Button{Text="Seç",AutoSize=true};files.Controls.Add(browse);top.Controls.Add(files);
            var parameters=new FlowLayoutPanel{AutoSize=true,Dock=DockStyle.Top};
            AddParameter(parameters,"Kapasite (palet)",capacity);AddParameter(parameters,"En çok araç",fleet);AddParameter(parameters,"Ek araç cezası",penalty);
            AddParameter(parameters,"Son koşum sayısı",finalRuns);AddParameter(parameters,"Son değerlendirmeye alınan konum",finalists);
            parameters.Controls.Add(new Label{Text="Arama adımları (km)",AutoSize=true,Margin=new Padding(8,7,3,0)});parameters.Controls.Add(steps);top.Controls.Add(parameters);
            top.Controls.Add(new Label{AutoSize=true,Text="Tarama: 100 birey × 100 nesil, 3 başlangıç. Son değerlendirme: 300 birey × 200 nesil. Mutasyon: 0,20."});
            top.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(1150,0),Text="Talep ağırlıklı başlangıç → çevrede yol bağlantısı → kapasiteye uygun rotalar → daha düşük ortalama maliyet. Sonuç bir öneridir; arsa/imar uygunluğu değildir."});
            var buttons=new FlowLayoutPanel{AutoSize=true,Dock=DockStyle.Top};foreach(var b in new[]{run,cancel,map,folder,replay})buttons.Controls.Add(b);top.Controls.Add(buttons);top.Controls.Add(status);
            var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,Size=new Size(1160,560),SplitterDistance=300};split.Panel1.Controls.Add(grid);split.Panel2.Controls.Add(detail);Controls.Add(split);Controls.Add(top);
            browse.Click+=(s,e)=>{using(var d=new OpenFileDialog{Filter="Mağaza CSV|*.csv"})if(d.ShowDialog()==DialogResult.OK)file.Text=d.FileName;};
            run.Click+=async(s,e)=>await RunSearch();cancel.Click+=(s,e)=>{if(cancellation!=null)cancellation.Cancel();};
            map.Click+=(s,e)=>{if(SelectedSolution!=null)Hide();};
            replay.Click+=(s,e)=>OpenRecorded();
            folder.Click+=(s,e)=>{if(Directory.Exists(RunDirectory))System.Diagnostics.Process.Start("explorer.exe",RunDirectory);};
            Shown+=(s,e)=>FindInput();
            file.TextChanged+=(s,e)=>InvalidateResult();steps.TextChanged+=(s,e)=>InvalidateResult();
            foreach(var input in new[]{capacity,fleet,penalty,finalRuns,finalists})input.ValueChanged+=(s,e)=>InvalidateResult();
            FormClosing+=(s,e)=>{if(cancellation!=null){cancellation.Cancel();e.Cancel=true;}};
        }
        static void AddParameter(FlowLayoutPanel p,string title,Control control){p.Controls.Add(new Label{Text=title,AutoSize=true,Margin=new Padding(8,7,3,0)});p.Controls.Add(control);}
        void FindInput()
        {
            if(!String.IsNullOrWhiteSpace(file.Text))return;
            var d=new DirectoryInfo(Application.StartupPath);
            for(int i=0;i<6&&d!=null;i++,d=d.Parent)
            {
                string path=Path.Combine(d.FullName,"Pilot_Veriler","magazalar.csv");
                if(File.Exists(path)){exampleDirectory=Path.Combine(d.FullName,"Ornek_Otomatik_Deney");file.Text=path;status.Text="28 mağazalık pilot dosya seçildi. Talepler senaryodur; depo koordinatını program üretecek.";return;}
            }
        }
        void InvalidateResult(){result=null;replayData=null;replaySolution=null;map.Enabled=folder.Enabled=false;grid.DataSource=null;detail.Clear();}
        void Busy(bool busy)
        {
            run.Enabled=browse.Enabled=file.Enabled=steps.Enabled=replay.Enabled=!busy;cancel.Enabled=busy;
            foreach(var x in new[]{capacity,fleet,penalty,finalRuns,finalists})x.Enabled=!busy;
        }
        void OpenRecorded()
        {
            try
            {
                using(var dialog=new OpenFileDialog{Filter="Seçilen depo koşumları|secilen_depo_kosumlari.csv",InitialDirectory=exampleDirectory??""})
                {
                    if(dialog.ShowDialog()!=DialogResult.OK)return;
                    string directory=Path.GetDirectoryName(dialog.FileName);
                    var settings=ResearchCsv.Read(Path.Combine(directory,"parametreler.csv")).Skip(1).ToDictionary(r=>r[0],r=>r[1]);
                    if(settings["method"]!="weighted_multires_local_ga_v1")throw new FormatException("Bu kayıt otomatik depo araması biçiminde değil.");
                    var options=new GaOptions{Capacity=Int32.Parse(settings["capacity"],CultureInfo.InvariantCulture),MaxVehicles=Int32.Parse(settings["max_vehicles"],CultureInfo.InvariantCulture),ExtraVehiclePenalty=Double.Parse(settings["lambda"],CultureInfo.InvariantCulture)};
                    options.Validate();
                    var data=ResearchCsv.Load(Path.Combine(directory,"konumlar.csv"),Path.Combine(directory,"mesafeler.csv"),Path.Combine(directory,"kaynak.csv"));
                    var runs=ResearchCsv.LoadSavedForMap(dialog.FileName,data,options);
                    var best=runs.OrderBy(r=>r.Score).ThenBy(r=>r.Seed).First();
                    if(runs.Select(r=>r.DepotIndex).Distinct().Count()!=1)throw new FormatException("Kayıt seçilen tek depoyu içermeli.");
                    InvalidateResult();capacity.Value=options.Capacity;fleet.Value=options.MaxVehicles;penalty.Value=(decimal)options.ExtraVehiclePenalty;
                    replayData=data;replaySolution=best;RunDirectory=directory;
                    grid.DataSource=new[]{new{Konum=data.Nodes[best.DepotIndex].Id,KayitliKosum=runs.Count,OrtalamaF=runs.Average(r=>r.Score),EnIyiF=best.Score,EnIyiKm=best.DistanceKm}};
                    foreach(DataGridViewColumn col in grid.Columns)if(col.ValueType==typeof(double))col.DefaultCellStyle.Format="N3";
                    detail.Text="Kayıtlı otomatik aramanın seçilen deposu açıldı.\r\nGA yeniden çalıştırılmadı. Rota, kapasite ve maliyet kayıtlı matrisle doğrulandı.\r\nArama geçmişi ve diğer konumlar sonuç klasöründedir.\r\nKlasör: "+directory;
                    map.Enabled=folder.Enabled=true;status.Text="Kayıtlı otomatik sonuç hazır. Önerilen depoyu haritada gösterebilirsiniz.";
                }
            }
            catch(Exception ex){InvalidateResult();MessageBox.Show(this,"Kayıt açılamadı: "+ex.Message);}
        }
        async Task RunSearch()
        {
            try
            {
                InvalidateResult();if(!File.Exists(file.Text))throw new ArgumentException("Mağaza CSV dosyasını seçin.");
                var stores=AutoSearchFiles.LoadStores(file.Text,(int)capacity.Value);
                var options=new AutoSearchOptions{StepsKm=steps.Text.Split(',').Select(s=>Double.Parse(s.Trim(),CultureInfo.InvariantCulture)).ToArray(),Finalists=(int)finalists.Value,
                    FinalSeeds=Enumerable.Range(1001,(int)finalRuns.Value).ToArray()};
                foreach(var o in new[]{options.Screening,options.Final}){o.Capacity=(int)capacity.Value;o.MaxVehicles=(int)fleet.Value;o.ExtraVehiclePenalty=(double)penalty.Value;}
                options.Validate();
                string basePath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"DepoBul_Deneyleri");
                RunDirectory=Path.Combine(basePath,"Otomatik_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_"+Guid.NewGuid().ToString("N").Substring(0,6));
                Directory.CreateDirectory(RunDirectory);File.Copy(file.Text,Path.Combine(RunDirectory,"girdi_magazalar.csv"));
                File.WriteAllText(Path.Combine(RunDirectory,"BASLANGIC.txt"),"Başlatıldı: "+DateTime.UtcNow.ToString("o")+" UTC\r\nMağaza="+stores.Length+"; kapasite="+options.Final.Capacity+"; filo="+options.Final.MaxVehicles+"; lambda="+options.Final.ExtraVehiclePenalty.ToString(CultureInfo.InvariantCulture)+"; adımlar(km)="+steps.Text+"; finalist="+options.Finalists+"; son koşum="+options.FinalSeeds.Length+"\r\nBu dosya tek başına deneyin tamamlandığını göstermez.\r\n");
                var provider=new OsrmDataProvider(Path.Combine(RunDirectory,"osrm_kayitlari"));
                cancellation=new CancellationTokenSource();Busy(true);
                var progress=new Progress<string>(s=>status.Text=s);var reporter=(IProgress<string>)progress;
                var computed=await Task.Run(()=>AutoDepotSearch.Run(stores,options,provider,cancellation.Token,s=>reporter.Report(s)));
                status.Text="Arama tamamlandı, sonuçlar kaydediliyor.";AutoSearchFiles.Export(RunDirectory,computed);result=computed;
                grid.DataSource=result.Candidates.Select(c=>new{Konum=c.Depot.Id,Enlem=c.Depot.Latitude,Boylam=c.Depot.Longitude,
                    TaramaOrtalamaF=c.ScreeningMean,SonOrtalamaF=c.FinalRuns==null?(double?)null:c.FinalMean,Onerilen=Object.ReferenceEquals(c,result.Winner)})
                    .OrderByDescending(x=>x.Onerilen).ThenBy(x=>x.TaramaOrtalamaF).ToList();
                foreach(DataGridViewColumn col in grid.Columns)if(col.ValueType==typeof(double)||col.ValueType==typeof(double?))col.DefaultCellStyle.Format=(col.Name=="Enlem"||col.Name=="Boylam")?"N6":"N3";
                var w=result.Winner;var b=result.BestSolution;
                detail.Text=String.Format("Mağaza: {0}\r\nTalep ağırlıklı başlangıç: {1:F6}, {2:F6}\r\nÖnerilen depo: {3} ({4:F6}, {5:F6})\r\nAraştırılan farklı konum: {6}\r\nSon ortalama F: {7:F3}\r\nEn iyi koşum: {8:F3} km | Araç: {9} | F: {10:F3}\r\nEn yakın komşu F: {11:F3}\r\nSonuç klasörü: {12}\r\nSeçim ortalama F'ye göre; haritada bu depodaki en iyi koşum gösterilir.",stores.Length,result.InitialCentre.Latitude,result.InitialCentre.Longitude,w.Depot.Id,w.Depot.Latitude,w.Depot.Longitude,result.Candidates.Count,w.FinalMean,b.DistanceKm,b.Routes.Count,b.Score,result.NearestNeighbour.Score,RunDirectory);
                map.Enabled=folder.Enabled=true;status.Text="Tamamlandı. Önerilen depoyu haritada gösterebilirsiniz. Sonuçlar otomatik kaydedildi.";
            }
            catch(OperationCanceledException){InvalidateResult();status.Text="Arama iptal edildi. Tamamlanmış sonuç üretilmedi; ham sorgu kayıtları korundu.";}
            catch(Exception ex){InvalidateResult();status.Text="Arama tamamlanamadı. Ham sorgu kayıtları korundu.";MessageBox.Show(this,ex.Message,"Otomatik depo araması");}
            finally{if(cancellation!=null){cancellation.Dispose();cancellation=null;}Busy(false);}
        }
    }
}
