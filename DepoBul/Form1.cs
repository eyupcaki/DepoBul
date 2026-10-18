using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;


namespace DepoBul
{
    public partial class Form1 : Form
    {
        private AutoDepotForm automaticPanel;
        private DepoBul.Research.RoadData activeData;
        private DepoBul.Research.RouteSolution activeSolution;
        AracBilgi f2 = new AracBilgi();

        public List<Magaza> MagazaListesi = new List<Magaza>();//magaza koordinatları ve kapasite ve isim 
        public List<Arac> AracListesi = new List<Arac>(); //tüm araç isimleri ve kapasiteleri
        public List<Birey> baslagicToplum = new List<Birey>();//ilki depo diğerleri müşteriler
        public List<Birey> yeniToplum = new List<Birey>();//ilki depo diğerleri müşteriler

        public int NesilSayisi = 200;  //ardarda toplum dönüşümünü içerir nesil sayısı
        int BireySayisi = 300;
        string harflerKromozom = "ABCDEFGHIJKLMNOPRSTUVYZ";

        GMarkerGoogle marker;
        GMapOverlay markerOverlay;
        DataTable dt;

        DataTable dt2;
        int secilenkayit = 0;
        int secilenkayit2 = 0;

        int filaSelecccionada = 0;
        double enlem = 39.4819262433;
        double boylam = 29.8975646495;

        // Uzaklik 

        bool rotahesap = false;
       // int Rota = 0;
        PointLatLng baslagic;
        PointLatLng bitis;
        double mesafe = 0;
        string[,] googlemesafe = new string[529, 3];

        // Değişken Tanımları





        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Legacy editor controls remain outside the active UI; no dummy rows or depot marker.
            dt=new DataTable();dt2=new DataTable();
            BuildResearchLayout();
            gMapControl1.DragButton=MouseButtons.Left;gMapControl1.CanDragMap=true;
            gMapControl1.MapProvider=GMapProviders.GoogleMap;
            gMapControl1.Position=new PointLatLng(37.06,37.38);
            gMapControl1.MinZoom=0;gMapControl1.MaxZoom=20;gMapControl1.Zoom=8;
            gMapControl1.Overlays.Clear();
            gMapControl1.MouseDoubleClick-=gMapControl1_MouseDoubleClick;
            listBox1.Items.Add("Otomatik Depo Ara ile mağaza verilerinden depo konumu üretin.");
            listBox2.Items.Add("Seçilen çözümün araç rotaları burada gösterilir.");
            mesaj.Text="Hazır. Mağazalar ve kapasite CSV girdilerinden alınır.";
        }

        private void BuildResearchLayout()
        {
            SuspendLayout();
            // Retain designer ownership for disposal, but detach every obsolete input from view.
            var legacy=new Panel{Visible=false,Size=new Size(0,0)};
            foreach(Control c in Controls.Cast<Control>().ToArray())legacy.Controls.Add(c);
            Controls.Add(legacy);
            Text="DepoBul | Otomatik Depo Konumu ve Dağıtım Rotaları";
            MinimumSize=new Size(1050,650);Size=new Size(1280,820);
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(6)};
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var toolbar=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,WrapContents=true};
            btnDupoRota.Text="Otomatik Depo Ara";btnDepoAlani.Text="Seçilen Depo";btnDepoYolu.Text="Depo Bilgisi";
            btnTemizle.Text="Çizimi Temizle";btnRota.Text="Karayolu Rotaları";
            btnAracKapasite.Text="Çalışma Alanı";
            btnAracKapasite.Click-=btnAracKapasite_Click;
            btnAracKapasite.Click+=(sender,e)=>{var overlay=gMapControl1.Overlays.FirstOrDefault(o=>o.Markers.Count>0);if(overlay!=null)gMapControl1.ZoomAndCenterMarkers(overlay.Id);};
            foreach(Button button in new[]{btnDupoRota,btnDepoAlani,btnDepoYolu,btnAracKapasite,btnRota,btnTemizle,btnHarita,btnUydu})
            {button.Size=new Size(145,40);button.Margin=new Padding(3);toolbar.Controls.Add(button);}
            layout.Controls.Add(toolbar,0,0);
            var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Vertical,Size=new Size(1220,650),SplitterDistance=720};
            gMapControl1.Dock=DockStyle.Fill;split.Panel1.Controls.Add(gMapControl1);
            var right=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};
            right.RowStyles.Add(new RowStyle(SizeType.Percent,40));right.RowStyles.Add(new RowStyle(SizeType.Percent,60));
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            var summary=new GroupBox{Text="Seçilen Çözüm",Dock=DockStyle.Fill,Padding=new Padding(7)};
            var routes=new GroupBox{Text="Araç Rotaları",Dock=DockStyle.Fill,Padding=new Padding(7)};
            foreach(var list in new[]{listBox1,listBox2}){list.Dock=DockStyle.Fill;list.HorizontalScrollbar=true;list.IntegralHeight=false;}
            summary.Controls.Add(listBox1);routes.Controls.Add(listBox2);right.Controls.Add(summary,0,0);right.Controls.Add(routes,0,1);
            split.Panel2.Controls.Add(right);layout.Controls.Add(split,0,1);
            mesaj.AutoSize=true;mesaj.MaximumSize=new Size(1400,0);mesaj.Margin=new Padding(5,8,5,5);layout.Controls.Add(mesaj,0,2);
            Controls.Add(layout);layout.BringToFront();ResumeLayout(true);
        }

        private void SecilenKayit(object sender, DataGridViewCellMouseEventArgs e)
        {
            filaSelecccionada = e.RowIndex;
            try
            {

                txtSubeAdi.Text = dataGridView1.Rows[filaSelecccionada].Cells[0].Value.ToString();
                txtEnlem.Text = dataGridView1.Rows[filaSelecccionada].Cells[1].Value.ToString();
                txtBoylam.Text = dataGridView1.Rows[filaSelecccionada].Cells[2].Value.ToString();
                txtSatisMiktari.Text = dataGridView1.Rows[filaSelecccionada].Cells[3].Value.ToString();

                marker.Position = new PointLatLng(Convert.ToDouble(txtEnlem.Text), Convert.ToDouble(txtBoylam.Text));
                gMapControl1.Position = marker.Position;

            }
            catch (Exception)
            {

                MessageBox.Show("Seçim işlemi hatalı oldu.");
            }

            
        }

        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            double en = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
            double boy = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

            txtEnlem.Text = en.ToString();
            txtBoylam.Text = boy.ToString();

            marker.Position = new PointLatLng(en, boy);
            marker.ToolTipText = string.Format("Mağaza Koordinat: \n Enlem:{0} \n Boylam:{1}", en, boy);

        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            dt.Rows.Add(txtSubeAdi.Text, txtEnlem.Text, txtBoylam.Text, txtSatisMiktari.Text);

        }

        private void btnCikar_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.RemoveAt(filaSelecccionada);

        }

        private void btnHarita_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
        }

        private void btnUydu_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            trackBar1.Value = Math.Max(trackBar1.Minimum,Math.Min(trackBar1.Maximum,Convert.ToInt32(gMapControl1.Zoom)));
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            gMapControl1.Zoom = trackBar1.Value;
        }

        private void btnAracKapasite_Click(object sender, EventArgs e)
        {
            GMapOverlay Depoalani = new GMapOverlay("Depo Alanı");
            List<PointLatLng> puntolar = new List<PointLatLng>();
            double cen, cboy;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                cen = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value);
                cboy = Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value);
                puntolar.Add(new PointLatLng(cen, cboy));
            }


            GMapPolygon poligonalan = new GMapPolygon(puntolar, "Depo Alanı");
            Depoalani.Polygons.Add(poligonalan);
            gMapControl1.Overlays.Add(Depoalani);
            gMapControl1.Zoom = gMapControl1.Zoom + 1;
            gMapControl1.Zoom = gMapControl1.Zoom - 1;


          

           
        }

        public void magazayukle()
        {

            MagazaListesi.Clear();
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {

                if (dataGridView1.Rows[i].IsNewRow) continue;
                string magazaadi = dataGridView1.Rows[i].Cells[0].Value.ToString();
                double enyukle = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value);
                double boyyukle = Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value);
                int kapasiteyukle = Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value);
                double uzaklikyukle = 0.0;
                //Convert.ToDouble(dataGridView1.Rows[i].Cells[4].Value);

                Magaza kayit = new Magaza(magazaadi, enyukle, boyyukle, kapasiteyukle, uzaklikyukle);
                MagazaListesi.Add(kayit);

            }





        }

        public void aracyukle()
        {
            
               for (int j = 0; j < dataGridViewarac.RowCount; j++)
                {

                     string aracplakay = dataGridViewarac.Rows[j].Cells[0].Value.ToString();
                    int kapasitey = Convert.ToInt32(dataGridViewarac.Rows[j].Cells[1].Value);
                     bool durumy = Convert.ToBoolean(dataGridViewarac.Rows[j].Cells[2].Value);


                     Arac kayit2 = new Arac(aracplakay, kapasitey, durumy);
                    AracListesi.Add(kayit2);

                }
           
           

          
        }


        private void btnDupoRota_Click(object sender, EventArgs e)
        {
            DepoBulRotaGen();
        }

        /// uzaklik Hesaplama google Maps Göre
        public double ikiNoktaUzaklik(double en1, double boy1, double en2, double boy2)
        {
            try
            {
            rotahesap = true;
            if (rotahesap)
            {
                baslagic = new PointLatLng(en1, boy1);
                bitis = new PointLatLng(en2, boy2);
                GDirections direccion;

                var RutasDireccion = GMapProviders.GoogleMap.GetDirections(out direccion, baslagic, bitis, false, false, false, false, false);
                GMapRoute RutaObtenida = new GMapRoute(direccion.Route, "Rota Konumu");
                GMapOverlay Caparutas = new GMapOverlay("Rota Mesafe");
                Caparutas.Routes.Add(RutaObtenida);
                gMapControl1.Overlays.Add(Caparutas);
                gMapControl1.Zoom = gMapControl1.Zoom + 1;
                gMapControl1.Zoom = gMapControl1.Zoom - 1;
                rotahesap = false;

                mesafe = RutaObtenida.Distance;
            }

           
            }
            catch (Exception)
            {

                MessageBox.Show("Belirlemiş olduğunuz güzergaha yol bulunmakatadır\n Konumu tekrar seçiniz.");
            }
            return Math.Abs(mesafe);
        }



        private void SecilenArac(object sender, DataGridViewCellEventArgs e)
        {



        }

        public List<Birey> ToplumOlustur()
        {
            List<Birey> toplum = new List<Birey>();

            Random rastgele = new Random();
            while (toplum.Count < BireySayisi)   ///bireysayisi 300 
            {
                string harfler = harflerKromozom;
                Birey islemyapilacakBirey = new Birey("", "", 0, 0, new List<Arac>());

                while (islemyapilacakBirey.Kromozom.Length < harflerKromozom.Length)
                {
                    int rastint = rastgele.Next(harfler.Length);
                    Char harf = harfler[rastint];

                    if (!islemyapilacakBirey.Kromozom.Contains(harf))
                    {
                        islemyapilacakBirey.Kromozom += harf;
                        harfler = harfler.Remove(rastint, 1);
                    }
                }

                toplum.Add(islemyapilacakBirey);

            }
            return toplum;
        }

        public void DepoBulRotaGen()
        {
            if(automaticPanel==null || automaticPanel.IsDisposed)automaticPanel=new AutoDepotForm();
            automaticPanel.ShowDialog(this);
            if(automaticPanel.SelectedSolution!=null)
            {activeSolution=automaticPanel.SelectedSolution;activeData=automaticPanel.SelectedData;ShowResearchMap();}
        }


        private void durumdegisimi1(object sender, EventArgs e)
        {

            if (checkBox1.Checked == true)
            {
                checkBox2.Checked = false;
            }
            if (checkBox1.Checked == false)
            {
                checkBox2.Checked = true;
            }


        }

        private void durumdegisimi2(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                checkBox1.Checked = false;
            }
            if (checkBox2.Checked == false)
            {
                checkBox1.Checked = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dt2.Rows.Add(txtAracPlaka.Text, txtKapasite.Text, checkBox1.Checked);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridViewarac.Rows.RemoveAt(secilenkayit);
        }

        private void SecilenAracG(object sender, DataGridViewCellMouseEventArgs e)
        {
            secilenkayit2 = e.RowIndex;
            txtAracPlaka.Text = dataGridViewarac.Rows[secilenkayit2].Cells[0].Value.ToString();
            txtKapasite.Text = dataGridViewarac.Rows[secilenkayit2].Cells[1].Value.ToString();
            checkBox1.Checked = Convert.ToBoolean(dataGridViewarac.Rows[secilenkayit2].Cells[2].Value);
        }

        // Offline schematic: edges indicate visit order, never claimed as road geometry.
        private void ShowResearchMap()
        {
            var solution=activeSolution;var data=activeData;
            gMapControl1.Overlays.Clear();
            var overlay=new GMapOverlay("Deney - şematik ziyaret sırası");
            foreach(var n in data.Nodes.Where(n=>n.Kind=="store" || n.Id==data.Nodes[solution.DepotIndex].Id))
            {
                var marker=new GMarkerGoogle(new PointLatLng(n.Latitude,n.Longitude),n.Kind=="depot"?GMarkerGoogleType.green:GMarkerGoogleType.blue);
                marker.ToolTipText=n.Id+(n.Kind=="store"?" | "+n.Demand+" palet":" | Önerilen depo");
                overlay.Markers.Add(marker);
            }
            listBox1.Items.Clear();listBox2.Items.Clear();
            listBox1.Items.Add("Önerilen depo: "+data.Nodes[solution.DepotIndex].Id);
            listBox1.Items.Add("GA başlangıç kodu: "+solution.Seed);
            listBox1.Items.Add("Toplam karayolu mesafesi: "+solution.DistanceKm.ToString("F3")+" km");
            listBox1.Items.Add("Araç sayısı: "+solution.Routes.Count+" | F: "+solution.Score.ToString("F3"));
            listBox1.Items.Add("Talep ve kapasite deney panelindeki palet değerleridir.");

            listBox2.Items.Add("Renkli düz çizgiler şematik ziyaret sırasıdır. Yol çizimi: Karayolu Rotaları.");
            Color[] colors={Color.Red,Color.Blue,Color.DarkGreen,Color.DarkOrange,Color.Purple,Color.Brown};int vehicle=0;
            foreach(var route in solution.Routes)
            {
                var points=new List<PointLatLng>();var depot=data.Nodes[solution.DepotIndex];
                points.Add(new PointLatLng(depot.Latitude,depot.Longitude));
                points.AddRange(route.Select(i=>new PointLatLng(data.Nodes[i].Latitude,data.Nodes[i].Longitude)));
                points.Add(points[0]);
                var line=new GMapRoute(points,"Araç "+(++vehicle)+" - şematik");
                listBox2.Items.Add("Araç "+vehicle+": "+data.Nodes[solution.DepotIndex].Id+" > "+String.Join(" > ",route.Select(i=>data.Nodes[i].Id))+" > "+data.Nodes[solution.DepotIndex].Id);
                line.Stroke=new Pen(colors[(vehicle-1)%colors.Length],2);overlay.Routes.Add(line);
            }
            gMapControl1.Overlays.Add(overlay);gMapControl1.ZoomAndCenterMarkers(overlay.Id);
            mesaj.Text="Şematik ziyaret sırası (düz çizgiler yol geometrisi değildir). Deney mesafesi: "+solution.DistanceKm.ToString("F3")+" km. Yol çizimi için Karayolu Rotaları.";
        }

        private async void btnRota_Click(object sender, EventArgs e)
        {
            if (activeData == null || activeSolution == null)
            { MessageBox.Show("Önce otomatik depo araması yapın veya arşivden sonuç açın."); return; }
            var solution=activeSolution;var data=activeData;
            btnRota.Enabled=false;mesaj.Text="OSRM yol çizimleri yükleniyor. Mevcut harita korunuyor.";
            try
            {
                string cache=System.IO.Path.Combine(Application.StartupPath,"road_cache");
                var progress=new Progress<Tuple<int,int>>(p=>{if(!IsDisposed)mesaj.Text="OSRM yol bağlantıları: "+p.Item1+"/"+p.Item2;});
                var reporter=(IProgress<Tuple<int,int>>)progress;
                var lines=await Task.Run(()=>DepoBul.Research.RoadGeometry.Load(data,solution,cache,(a,b)=>reporter.Report(Tuple.Create(a,b))));
                if(IsDisposed || activeSolution!=solution)return;
                var overlay=new GMapOverlay("OSRM karayolu rotaları");
                foreach(var n in data.Nodes.Where(n=>n.Kind=="store" || n.Id==data.Nodes[solution.DepotIndex].Id))
                {
                    var marker=new GMarkerGoogle(new PointLatLng(n.Latitude,n.Longitude),n.Kind=="depot"?GMarkerGoogleType.green:GMarkerGoogleType.blue);
                    marker.ToolTipText=n.Id+(n.Kind=="store"?" | "+n.Demand+" palet":" | Seçilen depo");overlay.Markers.Add(marker);
                }
                Color[] colors={Color.Red,Color.Blue,Color.DarkGreen,Color.DarkOrange,Color.Purple,Color.Brown};
                for(int i=0;i<lines.Count;i++)
                {
                    foreach(var leg in lines[i])
                    {
                        var line=new GMapRoute(leg.Select(xy=>new PointLatLng(xy[1],xy[0])).ToList(),"Araç "+(i+1));
                        line.Stroke=new Pen(colors[i%colors.Length],3);overlay.Routes.Add(line);
                    }
                }
                // Commit only when every vehicle's geometry has loaded successfully.
                gMapControl1.Overlays.Clear();gMapControl1.Overlays.Add(overlay);
                gMapControl1.ZoomAndCenterMarkers(overlay.Id);
                mesaj.Text="OSRM yol çizimleri | © OpenStreetMap contributors. Deney mesafesi sabit matristendir.";
                if(listBox2.Items.Count>0)listBox2.Items[0]="OSRM karayolu çizimi. Her yol eşleştirmesi 100 m sınırıyla kontrol edildi.";
                listBox2.Items.Add("Yol çizimi: OSRM / OpenStreetMap. Görsel güzergâh deney puanını değiştirmez.");
            }
            catch(Exception ex)
            {
                if(!IsDisposed){mesaj.Text="Yol çizimi alınamadı; mevcut harita ve deney sonuçları korundu.";
                    MessageBox.Show("OSRM yol çizimi alınamadı: "+ex.Message+"\nMevcut harita ve deney sonuçları korundu.");}
            }
            finally{if(!IsDisposed)btnRota.Enabled=true;}
        }

        private void btnDepoAlani_Click(object sender, EventArgs e)
        {
            if (activeData == null || activeSolution == null)
            { MessageBox.Show("Önce deney panelinde bir depo sonucu seçin."); return; }
            var n = activeData.Nodes[activeSolution.DepotIndex];
            var overlay = new GMapOverlay("Önerilen depo");
            var mark = new GMarkerGoogle(new PointLatLng(n.Latitude, n.Longitude), GMarkerGoogleType.green);
            mark.ToolTipText = n.Id + "\n" + n.AccessNote;
            mark.ToolTipMode = MarkerTooltipMode.Always; overlay.Markers.Add(mark);
            gMapControl1.Overlays.Add(overlay);gMapControl1.Position = mark.Position;
        }

        private void btnDepoYolu_Click(object sender, EventArgs e)
        {
            if (activeData == null || activeSolution == null)
            { MessageBox.Show("Önce deney panelinde bir depo sonucu seçin."); return; }
            var n = activeData.Nodes[activeSolution.DepotIndex];
            MessageBox.Show("Önerilen depo: " + n.Id + "\nYol bağlantısı kaydı: " + n.AccessNote +
                "\nMağazalara erişim, dağıtım aracı senaryosu varsayımıdır. Parsel/imar incelemesi yapılmaz.");
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            gMapControl1.Overlays.Clear();
            gMapControl1.Zoom = gMapControl1.Zoom + 1;
            gMapControl1.Zoom = gMapControl1.Zoom - 1;
        }

        private void btnYolCiz_Click(object sender, EventArgs e)
        {
            dt.Rows.Add("A", 39.35042753, 29.31916237, 5200, 0);

            dt.Rows.Add("B", 39.16606259, 29.63330269, 2300, 0);

            dt.Rows.Add("C", 38.83151755, 29.96353626, 1800, 0);

            dt.Rows.Add("D", 38.6739851, 29.23753738, 2750, 0);

            dt.Rows.Add("E", 37.83829279, 27.8471446, 1220, 0);

            dt.Rows.Add("F", 37.68927119, 28.00136089, 4200, 0);

            dt.Rows.Add("G", 37.34380556, 28.15669298, 2050, 0);

            dt.Rows.Add("H", 37.28836021, 27.79191256, 950, 0);

            dt.Rows.Add("I", 37.13484082, 27.58154154, 520, 0);

            dt.Rows.Add("J", 37.03979782, 27.44101524, 1190, 0);

            dt.Rows.Add("K", 37.01538667, 28.46202493, 3450, 0);

            dt.Rows.Add("L", 36.76358583, 28.90533507, 2900, 0);

            dt.Rows.Add("M", 37.16396745, 29.69628096, 1150, 0);

            dt.Rows.Add("N", 37.71831026, 30.28219879, 1750, 0);

            dt.Rows.Add("O", 37.76002815, 30.54851532, 2350, 0);

            dt.Rows.Add("P", 38.75582354, 30.55559635, 5400, 0);

            dt.Rows.Add("R", 39.01598274, 30.52480459, 3980, 0);

            dt.Rows.Add("S", 39.77523125, 30.52371025, 2650, 0);

            dt.Rows.Add("T", 40.52189001, 31.08160973, 1900, 0);

            dt.Rows.Add("U", 40.73486855, 30.37642479, 3350, 0);

            dt.Rows.Add("V", 41.00662141, 30.61100006, 2800, 0);

            dt.Rows.Add("Y", 41.06369208, 30.15541077, 4200, 0);

            dt.Rows.Add("Z", 41.06138658, 29.94318902, 1950, 0);
        }

        private void btnSeferAlani_Click(object sender, EventArgs e)
        {
            dt2.Rows.Add("Ford-15000 ",15000, false);

            dt2.Rows.Add("Ford-7000  ",7000, false);

           dt2.Rows.Add("Ford-9000 ", 9000, false);
          
           dt2.Rows.Add("Ford-8000  ", 8000, false);
       
           dt2.Rows.Add("Ford-8000  ", 8000, false);
       
           dt2.Rows.Add("Ford-8000  ", 8000, false);
       
           dt2.Rows.Add("Ford-7000  ", 7000, false);
         
           dt2.Rows.Add("Ford-7000  ", 7000, false);
          
           dt2.Rows.Add("Ford-7000  ", 7000, false);
          
           dt2.Rows.Add("Ford-2000  ", 2000, false);
        

        }
    }

    internal static class Extensions
    {
        public static IList<T> CloneList<T>(this IList<T> list) where T : ICloneable
        {
            return list.Select(item => (T)item.Clone()).ToList();
        }
    }

}
