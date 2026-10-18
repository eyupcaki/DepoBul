using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DepoBul
{
    public partial class AracBilgi : Form
    {
        public List<Arac> AracListesi_y = new List<Arac>();

        DataTable dt;
       int secilenkayit=0;
        public AracBilgi()
        {
            InitializeComponent();
        }

        private void AracBilgi_Load(object sender, EventArgs e)
        {
            dt = new DataTable();
           dt.Columns.Add(new DataColumn("Araç Plaka", typeof(string)));
           dt.Columns.Add(new DataColumn("Kapasite", typeof(double)));
           dt.Columns.Add(new DataColumn("Kullanım Durumu", typeof(bool)));



           
            dt.Rows.Add("Ford", 250, false);
            dataGridViewarac.DataSource = dt;

            // dataGridView1.Columns[1].Visible = false;
            // dataGridView1.Columns[2].Visible = false;
            // dataGridView1.Columns[3].Visible = false;
        }

        private void Secilen(object sender, DataGridViewCellMouseEventArgs e)
        {
            secilenkayit = e.RowIndex;
            txtAracPlaka.Text = dataGridViewarac.Rows[secilenkayit].Cells[0].Value.ToString();
            txtKapasite.Text = dataGridViewarac.Rows[secilenkayit].Cells[1].Value.ToString();
            checkBox1.Checked = Convert.ToBoolean(dataGridViewarac.Rows[secilenkayit].Cells[2].Value);
          
        }

        private void durumdegisimi(object sender, EventArgs e)
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
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

        private void button1_Click(object sender, EventArgs e)
        {
            dt.Rows.Add(txtAracPlaka.Text, txtKapasite.Text,checkBox1.Checked);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridViewarac.Rows.RemoveAt(secilenkayit);
        }

        private void AracBilgi_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

      
    }
}
