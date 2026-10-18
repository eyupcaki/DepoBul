namespace DepoBul
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gMapControl1 = new GMap.NET.WindowsForms.GMapControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnCikar = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnEkle = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSatisMiktari = new System.Windows.Forms.TextBox();
            this.txtBoylam = new System.Windows.Forms.TextBox();
            this.txtEnlem = new System.Windows.Forms.TextBox();
            this.txtSubeAdi = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.btnHarita = new System.Windows.Forms.Button();
            this.btnUydu = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnDepoYolu = new System.Windows.Forms.Button();
            this.btnDepoAlani = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btnAracKapasite = new System.Windows.Forms.Button();
            this.btnDupoRota = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnTemizle = new System.Windows.Forms.Button();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.btnRota = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.dataGridViewarac = new System.Windows.Forms.DataGridView();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.txtKapasite = new System.Windows.Forms.TextBox();
            this.txtAracPlaka = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSeferAlani = new System.Windows.Forms.Button();
            this.btnYolCiz = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.mesaj = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewarac)).BeginInit();
            this.SuspendLayout();
            // 
            // gMapControl1
            // 
            this.gMapControl1.Bearing = 0F;
            this.gMapControl1.CanDragMap = true;
            this.gMapControl1.EmptyTileColor = System.Drawing.Color.Navy;
            this.gMapControl1.GrayScaleMode = false;
            this.gMapControl1.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.gMapControl1.LevelsKeepInMemmory = 5;
            this.gMapControl1.Location = new System.Drawing.Point(7, 7);
            this.gMapControl1.MarkersEnabled = true;
            this.gMapControl1.MaxZoom = 2;
            this.gMapControl1.MinZoom = 2;
            this.gMapControl1.MouseWheelZoomEnabled = true;
            this.gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            this.gMapControl1.Name = "gMapControl1";
            this.gMapControl1.NegativeMode = false;
            this.gMapControl1.PolygonsEnabled = true;
            this.gMapControl1.RetryLoadTile = 0;
            this.gMapControl1.RoutesEnabled = true;
            this.gMapControl1.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.gMapControl1.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.gMapControl1.ShowTileGridLines = false;
            this.gMapControl1.Size = new System.Drawing.Size(637, 591);
            this.gMapControl1.TabIndex = 0;
            this.gMapControl1.Zoom = 0D;
            this.gMapControl1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gMapControl1_MouseDoubleClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnCikar);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnEkle);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtSatisMiktari);
            this.groupBox1.Controls.Add(this.txtBoylam);
            this.groupBox1.Controls.Add(this.txtEnlem);
            this.groupBox1.Controls.Add(this.txtSubeAdi);
            this.groupBox1.Location = new System.Drawing.Point(7, 594);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(173, 220);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // btnCikar
            // 
            this.btnCikar.Image = global::DepoBul.Properties.Resources.minus;
            this.btnCikar.Location = new System.Drawing.Point(92, 182);
            this.btnCikar.Name = "btnCikar";
            this.btnCikar.Size = new System.Drawing.Size(75, 32);
            this.btnCikar.TabIndex = 9;
            this.btnCikar.Text = "Çıkar";
            this.btnCikar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCikar.UseVisualStyleBackColor = true;
            this.btnCikar.Click += new System.EventHandler(this.btnCikar_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 139);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Satış Miktarı";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Boylam";
            // 
            // btnEkle
            // 
            this.btnEkle.Image = global::DepoBul.Properties.Resources.plus;
            this.btnEkle.Location = new System.Drawing.Point(10, 182);
            this.btnEkle.Name = "btnEkle";
            this.btnEkle.Size = new System.Drawing.Size(75, 32);
            this.btnEkle.TabIndex = 8;
            this.btnEkle.Text = "Ekle";
            this.btnEkle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnEkle.UseVisualStyleBackColor = true;
            this.btnEkle.Click += new System.EventHandler(this.btnEkle_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Enlem";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Mağaza Adı";
            // 
            // txtSatisMiktari
            // 
            this.txtSatisMiktari.Location = new System.Drawing.Point(11, 156);
            this.txtSatisMiktari.Name = "txtSatisMiktari";
            this.txtSatisMiktari.Size = new System.Drawing.Size(156, 20);
            this.txtSatisMiktari.TabIndex = 3;
            this.txtSatisMiktari.Text = "0";
            // 
            // txtBoylam
            // 
            this.txtBoylam.Location = new System.Drawing.Point(11, 116);
            this.txtBoylam.Name = "txtBoylam";
            this.txtBoylam.Size = new System.Drawing.Size(156, 20);
            this.txtBoylam.TabIndex = 2;
            // 
            // txtEnlem
            // 
            this.txtEnlem.Location = new System.Drawing.Point(11, 74);
            this.txtEnlem.Name = "txtEnlem";
            this.txtEnlem.Size = new System.Drawing.Size(156, 20);
            this.txtEnlem.TabIndex = 1;
            // 
            // txtSubeAdi
            // 
            this.txtSubeAdi.Location = new System.Drawing.Point(11, 32);
            this.txtSubeAdi.Name = "txtSubeAdi";
            this.txtSubeAdi.Size = new System.Drawing.Size(156, 20);
            this.txtSubeAdi.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(195, 602);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(365, 164);
            this.dataGridView1.TabIndex = 10;
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.SecilenKayit);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.trackBar1);
            this.groupBox2.Controls.Add(this.btnHarita);
            this.groupBox2.Controls.Add(this.btnUydu);
            this.groupBox2.Location = new System.Drawing.Point(191, 592);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(453, 222);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(4, 174);
            this.trackBar1.Maximum = 20;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(446, 45);
            this.trackBar1.TabIndex = 13;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // btnHarita
            // 
            this.btnHarita.Image = global::DepoBul.Properties.Resources.worldwide;
            this.btnHarita.Location = new System.Drawing.Point(374, 11);
            this.btnHarita.Name = "btnHarita";
            this.btnHarita.Size = new System.Drawing.Size(72, 72);
            this.btnHarita.TabIndex = 11;
            this.btnHarita.Text = "Harita";
            this.btnHarita.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnHarita.UseVisualStyleBackColor = true;
            this.btnHarita.Click += new System.EventHandler(this.btnHarita_Click);
            // 
            // btnUydu
            // 
            this.btnUydu.Image = global::DepoBul.Properties.Resources.earth_globe;
            this.btnUydu.Location = new System.Drawing.Point(375, 100);
            this.btnUydu.Name = "btnUydu";
            this.btnUydu.Size = new System.Drawing.Size(72, 72);
            this.btnUydu.TabIndex = 12;
            this.btnUydu.Text = "Uydu";
            this.btnUydu.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUydu.UseVisualStyleBackColor = true;
            this.btnUydu.Click += new System.EventHandler(this.btnUydu_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnDepoYolu);
            this.groupBox3.Controls.Add(this.btnDepoAlani);
            this.groupBox3.Controls.Add(this.listBox1);
            this.groupBox3.Controls.Add(this.btnAracKapasite);
            this.groupBox3.Controls.Add(this.btnDupoRota);
            this.groupBox3.Location = new System.Drawing.Point(650, 1);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(533, 350);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            // 
            // btnDepoYolu
            // 
            this.btnDepoYolu.Image = global::DepoBul.Properties.Resources.work_tools;
            this.btnDepoYolu.Location = new System.Drawing.Point(270, 11);
            this.btnDepoYolu.Name = "btnDepoYolu";
            this.btnDepoYolu.Size = new System.Drawing.Size(120, 35);
            this.btnDepoYolu.TabIndex = 14;
            this.btnDepoYolu.Text = "Depo Yolu";
            this.btnDepoYolu.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDepoYolu.UseVisualStyleBackColor = true;
            this.btnDepoYolu.Click += new System.EventHandler(this.btnDepoYolu_Click);
            // 
            // btnDepoAlani
            // 
            this.btnDepoAlani.Image = global::DepoBul.Properties.Resources.package;
            this.btnDepoAlani.Location = new System.Drawing.Point(137, 11);
            this.btnDepoAlani.Name = "btnDepoAlani";
            this.btnDepoAlani.Size = new System.Drawing.Size(120, 35);
            this.btnDepoAlani.TabIndex = 13;
            this.btnDepoAlani.Text = "Seçilen Depo";
            this.btnDepoAlani.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDepoAlani.UseVisualStyleBackColor = true;
            this.btnDepoAlani.Click += new System.EventHandler(this.btnDepoAlani_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.Location = new System.Drawing.Point(7, 51);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(517, 290);
            this.listBox1.TabIndex = 12;
            // 
            // btnAracKapasite
            // 
            this.btnAracKapasite.Image = global::DepoBul.Properties.Resources.delivery_truck;
            this.btnAracKapasite.Location = new System.Drawing.Point(401, 11);
            this.btnAracKapasite.Name = "btnAracKapasite";
            this.btnAracKapasite.Size = new System.Drawing.Size(120, 35);
            this.btnAracKapasite.TabIndex = 5;
            this.btnAracKapasite.Text = "Çalışma Alanı";
            this.btnAracKapasite.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAracKapasite.UseVisualStyleBackColor = true;
            this.btnAracKapasite.Click += new System.EventHandler(this.btnAracKapasite_Click);
            // 
            // btnDupoRota
            // 
            this.btnDupoRota.Image = global::DepoBul.Properties.Resources.route__1_;
            this.btnDupoRota.Location = new System.Drawing.Point(6, 11);
            this.btnDupoRota.Name = "btnDupoRota";
            this.btnDupoRota.Size = new System.Drawing.Size(120, 35);
            this.btnDupoRota.TabIndex = 3;
            this.btnDupoRota.Text = "  Depo Bul Araç Rotala";
            this.btnDupoRota.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDupoRota.UseVisualStyleBackColor = true;
            this.btnDupoRota.Click += new System.EventHandler(this.btnDupoRota_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnTemizle);
            this.groupBox4.Controls.Add(this.listBox2);
            this.groupBox4.Controls.Add(this.btnRota);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Location = new System.Drawing.Point(650, 352);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(533, 240);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            // 
            // btnTemizle
            // 
            this.btnTemizle.Image = global::DepoBul.Properties.Resources.basket;
            this.btnTemizle.Location = new System.Drawing.Point(416, 9);
            this.btnTemizle.Name = "btnTemizle";
            this.btnTemizle.Size = new System.Drawing.Size(111, 35);
            this.btnTemizle.TabIndex = 3;
            this.btnTemizle.Text = "Rota Sil";
            this.btnTemizle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnTemizle.UseVisualStyleBackColor = true;
            this.btnTemizle.Click += new System.EventHandler(this.btnTemizle_Click);
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Location = new System.Drawing.Point(6, 48);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(521, 186);
            this.listBox2.TabIndex = 2;
            // 
            // btnRota
            // 
            this.btnRota.Image = global::DepoBul.Properties.Resources.route;
            this.btnRota.Location = new System.Drawing.Point(299, 9);
            this.btnRota.Name = "btnRota";
            this.btnRota.Size = new System.Drawing.Size(111, 35);
            this.btnRota.TabIndex = 2;
            this.btnRota.Text = "Rotayı Göster";
            this.btnRota.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnRota.UseVisualStyleBackColor = true;
            this.btnRota.Click += new System.EventHandler(this.btnRota_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.ForeColor = System.Drawing.Color.Maroon;
            this.label5.Location = new System.Drawing.Point(9, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(292, 20);
            this.label5.TabIndex = 1;
            this.label5.Text = "En iyi Depo Konumu ve En İyi Rota ";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.dataGridViewarac);
            this.groupBox5.Controls.Add(this.checkBox2);
            this.groupBox5.Controls.Add(this.checkBox1);
            this.groupBox5.Controls.Add(this.txtKapasite);
            this.groupBox5.Controls.Add(this.txtAracPlaka);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Controls.Add(this.button2);
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Location = new System.Drawing.Point(646, 592);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(533, 163);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            // 
            // dataGridViewarac
            // 
            this.dataGridViewarac.AllowUserToAddRows = false;
            this.dataGridViewarac.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewarac.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewarac.Location = new System.Drawing.Point(263, 11);
            this.dataGridViewarac.Name = "dataGridViewarac";
            this.dataGridViewarac.ReadOnly = true;
            this.dataGridViewarac.Size = new System.Drawing.Size(264, 145);
            this.dataGridViewarac.TabIndex = 21;
            this.dataGridViewarac.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.SecilenArac);
            this.dataGridViewarac.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.SecilenAracG);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(99, 86);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(57, 17);
            this.checkBox2.TabIndex = 20;
            this.checkBox2.Text = "Uygun";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.durumdegisimi2);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(180, 86);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(77, 17);
            this.checkBox1.TabIndex = 19;
            this.checkBox1.Text = "Kullanımda";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.durumdegisimi1);
            // 
            // txtKapasite
            // 
            this.txtKapasite.Location = new System.Drawing.Point(101, 53);
            this.txtKapasite.Name = "txtKapasite";
            this.txtKapasite.Size = new System.Drawing.Size(156, 20);
            this.txtKapasite.TabIndex = 18;
            // 
            // txtAracPlaka
            // 
            this.txtAracPlaka.Location = new System.Drawing.Point(101, 23);
            this.txtAracPlaka.Name = "txtAracPlaka";
            this.txtAracPlaka.Size = new System.Drawing.Size(156, 20);
            this.txtAracPlaka.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Kullanım Durumu";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 60);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Kapasite";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 32);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Araç Plaka";
            // 
            // button2
            // 
            this.button2.Image = global::DepoBul.Properties.Resources.minus;
            this.button2.Location = new System.Drawing.Point(181, 120);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 32);
            this.button2.TabIndex = 13;
            this.button2.Text = "Çıkar";
            this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Image = global::DepoBul.Properties.Resources.plus;
            this.button1.Location = new System.Drawing.Point(99, 120);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 32);
            this.button1.TabIndex = 12;
            this.button1.Text = "Ekle";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSeferAlani
            // 
            this.btnSeferAlani.Location = new System.Drawing.Point(1089, 758);
            this.btnSeferAlani.Name = "btnSeferAlani";
            this.btnSeferAlani.Size = new System.Drawing.Size(91, 27);
            this.btnSeferAlani.TabIndex = 3;
            this.btnSeferAlani.Text = "Araç Getir";
            this.btnSeferAlani.UseVisualStyleBackColor = true;
            this.btnSeferAlani.Click += new System.EventHandler(this.btnSeferAlani_Click);
            // 
            // btnYolCiz
            // 
            this.btnYolCiz.Location = new System.Drawing.Point(992, 757);
            this.btnYolCiz.Name = "btnYolCiz";
            this.btnYolCiz.Size = new System.Drawing.Size(91, 28);
            this.btnYolCiz.TabIndex = 2;
            this.btnYolCiz.Text = "Magaza Getir";
            this.btnYolCiz.UseVisualStyleBackColor = true;
            this.btnYolCiz.Click += new System.EventHandler(this.btnYolCiz_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // mesaj
            // 
            this.mesaj.AutoSize = true;
            this.mesaj.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.mesaj.ForeColor = System.Drawing.Color.Maroon;
            this.mesaj.Location = new System.Drawing.Point(652, 785);
            this.mesaj.Name = "mesaj";
            this.mesaj.Size = new System.Drawing.Size(52, 16);
            this.mesaj.TabIndex = 11;
            this.mesaj.Text = "Durum";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1186, 820);
            this.Controls.Add(this.mesaj);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnSeferAlani);
            this.Controls.Add(this.btnYolCiz);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gMapControl1);
            this.Name = "Form1";
            this.Text = "DepoBul | Dr. Öğr. Üyesi Durmuş ÖZDEMİR - Eyüp ÇAKI Abdurrahman GÜNAY";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewarac)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GMap.NET.WindowsForms.GMapControl gMapControl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCikar;
        private System.Windows.Forms.Button btnEkle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSatisMiktari;
        private System.Windows.Forms.TextBox txtBoylam;
        private System.Windows.Forms.TextBox txtEnlem;
        private System.Windows.Forms.TextBox txtSubeAdi;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnUydu;
        private System.Windows.Forms.Button btnHarita;
        private System.Windows.Forms.Button btnDupoRota;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btnSeferAlani;
        private System.Windows.Forms.Button btnYolCiz;
        private System.Windows.Forms.Button btnRota;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnAracKapasite;
        private System.Windows.Forms.Label mesaj;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox txtKapasite;
        private System.Windows.Forms.TextBox txtAracPlaka;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridViewarac;
        private System.Windows.Forms.Button btnDepoYolu;
        private System.Windows.Forms.Button btnDepoAlani;
        private System.Windows.Forms.Button btnTemizle;
    }
}

