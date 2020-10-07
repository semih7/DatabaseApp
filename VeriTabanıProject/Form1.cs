using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VeriTabanıProject
{
    public partial class Form1 : Form
    {
        SqlConnection connect;  //connection oluşturuldu.
        SqlCommand command;     //sql komutları için command değişkeni tanımlandı.
        SqlDataAdapter da;      //Data Adapter tanımlandı. 
        public Form1()
        {
            InitializeComponent();
        }

        void GetCustomer()      //Aktifliği 1 olan yani silinmemiş olan kullanıcıların listelendiği fonksiyon
        {
            connect = new SqlConnection("server=.; Initial Catalog = Customer; Integrated Security = SSPI");
            connect.Open();
            da = new SqlDataAdapter("SELECT * FROM customer WHERE aktif = 1", connect);
            DataTable tablo = new DataTable();
            da.Fill(tablo);
            dataGridView1.DataSource = tablo;
            connect.Close();
        }
        
        void GetCustomerAll()       //Kullanıcıların yaptığı işlemlerin detaylarını veren fonksiyon
        {
            connect = new SqlConnection("server = .; Initial Catalog = Customer; Integrated Security = SSPI");
            connect.Open();
            da = new SqlDataAdapter("SELECT * FROM customer_log", connect);
            DataTable tablo = new DataTable();
            da.Fill(tablo);
            dataGridView1.DataSource = tablo;
            connect.Close();
        }
   
        private void button3_Click(object sender, EventArgs e)
        {
            //Güncelle butonuna tıklandığında yazılan adın soyadın ve yaşın kayıtlarının veritabanında güncellenmesi işlemi
            string sorgu = "UPDATE customer SET ad = @ad, soyad = @soyad, yas = @yas WHERE id = @id";
            command = new SqlCommand(sorgu, connect);
            command.Parameters.AddWithValue("@id", Convert.ToInt32(txtID.Text));
            command.Parameters.AddWithValue("@ad", txtAd.Text);
            command.Parameters.AddWithValue("@soyad", txtSoyad.Text);
            command.Parameters.AddWithValue("@yas", txtYaş.Text);
            connect.Open();
            command.ExecuteNonQuery();
            connect.Close();

            //Güncelleme işlemi yapılırken geçmişi tutan veritabanında eski verilerin kaydedilmesi ve yeni verilerin yazılması işlemi
            string sorgu2 = "INSERT INTO customer_log (c_id, eski_ad, eski_soyad, eski_yas, yeni_ad, yeni_soyad, yeni_yas, islem_turu)" +
                "VALUES (@c_id, @eski_ad, @eski_soyad, @eski_yas, @yeni_ad, @yeni_soyad, @yeni_yas, 'guncelleme')";
            command = new SqlCommand(sorgu2, connect);
            command.Parameters.AddWithValue("@c_id", Convert.ToInt32(txtID.Text));
            command.Parameters.AddWithValue("@eski_ad", txtAd_secret.Text);
            command.Parameters.AddWithValue("@eski_soyad", txtSoyad_secret.Text);
            command.Parameters.AddWithValue("@eski_yas", txtYas_secret.Text);
            command.Parameters.AddWithValue("@yeni_ad", txtAd.Text);
            command.Parameters.AddWithValue("@yeni_soyad", txtSoyad.Text);
            command.Parameters.AddWithValue("@yeni_yas", txtYaş.Text);
            connect.Open();
            GetCustomer();
            command.ExecuteNonQuery();
            connect.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Form yüklendiğinde aktif kullanıcıların listelenmesi ve kutuların temizlenmesi
            GetCustomer();
            txtID.Clear();
            txtAd.Clear();
            txtSoyad.Clear();
            txtYaş.Clear();

        }

        
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            //Kullanıcıların gösterildiği ekranda bir kullanıcıya tıklandığında verilerin text box lara dolması
            txtID.Text = dataGridView1.CurrentRow.Cells[0].Value.ToString();
            txtAd_secret.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
            txtSoyad_secret.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
            txtYas_secret.Text = dataGridView1.CurrentRow.Cells[3].Value.ToString();
            txtAd.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
            txtSoyad.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
            txtYaş.Text = dataGridView1.CurrentRow.Cells[3].Value.ToString();


        }


        private void btnEkle_Click(object sender, EventArgs e)
        {
            /*
            //Ekle butonuna basıldığında tüm alanların dolu olup olmadığını kontrol eden kısım
            if (String.IsNullOrEmpty(txtAd.Text) || String.IsNullOrEmpty(txtSoyad.Text) || String.IsNullOrEmpty(txtYaş.Text) ) {
                System.Windows.Forms.MessageBox.Show("Lütfen Tüm Alanları Doldurun");
                System.Windows.Forms.Application.Restart();
                Application.Exit();
                
            }
            */
            if (String.IsNullOrEmpty(txtAd.Text) || String.IsNullOrEmpty(txtSoyad.Text) || String.IsNullOrEmpty(txtYaş.Text))
            {
                string message = "Lütfen Tüm Alanları Doldurun !";
                string title = "HATA";
                System.Windows.Forms.MessageBox.Show(message,title);
            }
            else
            {
                //Kullanıcı ekle butonuna bastığında veritabanına ekleme işlemi
                connect.Open();
                string q1 = "INSERT INTO customer(ad,soyad,yas) VALUES (@ad,@soyad,@yas)";
                command = new SqlCommand(q1, connect);
                command.Parameters.AddWithValue("@ad", txtAd.Text);
                command.Parameters.AddWithValue("@soyad", txtSoyad.Text);
                command.Parameters.AddWithValue("@yas", txtYaş.Text);
                command.ExecuteNonQuery();
                GetCustomer();
                connect.Close();



                //Tablodaki son id değerini çeken kod parçası
                string sonid = "-1";
                connect.Open();
                SqlCommand s1 = new SqlCommand("Select max(id) as sonid from customer ", connect);
                using (SqlDataReader reader = s1.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sonid = String.Format("{0}", reader["sonid"]);
                        sonid.ToString();
                    }
                }
                connect.Close();

                //İlk kayıt işlemi yapıldığında geçmiş tablosuna verileri ekleme 
                connect.Open();
                string q2 = "INSERT INTO customer_log (c_id, yeni_ad, yeni_soyad, yeni_yas, islem_turu) " +
                    "VALUES (@c_id, @yeni_ad, @yeni_soyad, @yeni_yas,'İlk Kayıt')";
                command = new SqlCommand(q2, connect);
                command.Parameters.AddWithValue("@yeni_ad", txtAd.Text);
                command.Parameters.AddWithValue("@yeni_soyad", txtSoyad.Text);
                command.Parameters.AddWithValue("@yeni_yas", txtYaş.Text);
                command.Parameters.AddWithValue("@c_id", sonid);
                command.ExecuteNonQuery();
                connect.Close();
            }


            txtID.Clear();
            txtAd.Clear();
            txtSoyad.Clear();
            txtYaş.Clear();

        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            //kullanıcı seçilip sil butonuna basıldığında aktiflik durumunu 0 yaparak tablo üzerinde gösterilmesi engellenmiştir.
            //Sil butonuna tıklandığında veritabanında eşleşen id değerinin aktifliğini 0 yapan kod parçası
            string sorgu = "UPDATE customer SET ad = @ad, soyad = @soyad, yas = @yas, aktif = @aktif WHERE id = @id";
            command = new SqlCommand(sorgu, connect);
            command.Parameters.AddWithValue("@id", Convert.ToInt32(txtID.Text));
            command.Parameters.AddWithValue("@ad", txtAd_secret.Text);
            command.Parameters.AddWithValue("@soyad", txtSoyad_secret.Text);
            command.Parameters.AddWithValue("@yas", txtYas_secret.Text);
            command.Parameters.AddWithValue("@aktif", 0);
            connect.Open();
            command.ExecuteNonQuery();
            connect.Close();

            //Kullanıcı silindiğinde geçmiş veritabanına kullanıcının silindiğini bildiren kod parçası
            string sorgu2 = "INSERT INTO customer_log (c_id, yeni_ad, yeni_soyad, yeni_yas, islem_turu) " +
                "VALUES (@c_id, @yeni_ad, @yeni_soyad, @yeni_yas,'Kayıt Silme')";
            
            command = new SqlCommand(sorgu2, connect);
            command.Parameters.AddWithValue("@c_id", Convert.ToInt32(txtID.Text));
            command.Parameters.AddWithValue("@yeni_ad", txtAd_secret.Text);
            command.Parameters.AddWithValue("@yeni_soyad", txtSoyad_secret.Text);
            command.Parameters.AddWithValue("@yeni_yas", txtYas_secret.Text);
            connect.Open();
            command.ExecuteNonQuery();
            GetCustomer();
            connect.Close();
        }

        private void txtID_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
           
        }

        private void Temizle_Click(object sender, EventArgs e)
        {
            //Temizle butonuna tıklandığında text boxları temizleyen kod parçası
            txtID.Clear();
            txtAd.Clear();
            txtSoyad.Clear();
            txtYaş.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Geçmiş tuşuna tıklandığında geçmişi getirecek olan event.
            GetCustomerAll();
        }

        private void btntum_kayitlar_Click(object sender, EventArgs e)
        {
            //Tüm kayıtlara tıklandığında o zamana kadar olan tüm kayıtları listeleyen kısım. Aktiflik durumunun 1 olması hala kayıtlı olduğunu gösterir.
            connect = new SqlConnection("server=.; Initial Catalog = Customer; Integrated Security = SSPI");
            connect.Open();
            da = new SqlDataAdapter("SELECT * FROM customer", connect);
            DataTable tablo = new DataTable();
            da.Fill(tablo);
            dataGridView1.DataSource = tablo;
            connect.Close();
        }
    }
}
