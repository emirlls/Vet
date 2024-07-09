using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.OleDb;

namespace vet
{
    public partial class Form2 : Form
    {
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\H.EMİRLEYLEK\Desktop\Hayvanlar.accdb;Persist Security Info=True"; // Buraya kendi veritabanınızın bağlantı dizesi girilecek
        //Bu kodla uyumlu olması için veritabanınızda 'Vet' isimli bir tablo oluşturmalısınız.Tabloya 
        //HayvanID(Otomatik Sayı),Ad(Kısa Metin),Cins(Kısa Metin),Hastalik(Kısa Metin),TedaviYontemi(Kısa Metin),
        //SahipNumarasi(Kısa Metin) ve Resim(OLE Nesnesi) değerlerini eklemelisiniz.

        public Form2()
        {
            InitializeComponent();
        }

        private void btnara_Click(object sender, EventArgs e)
        {
            string ad = textBox1.Text.Trim();
            string hastalik = textBox2.Text.Trim();

            // Arama sorgusu hazırla
            string query = "SELECT Ad,Cins,Hastalik,TedaviYontemi,SahipNumarasi,Resim FROM Vet WHERE (@Ad = '' OR Ad LIKE @Ad) AND (@Hastalik = '' OR Hastalik LIKE @Hastalik)";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Parametre ekle
                command.Parameters.AddWithValue("@Ad", "%" + ad + "%");
                command.Parameters.AddWithValue("@Hastalik", "%" + hastalik + "%");

                // Veri okuyucu oluştur
                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                DataTable dataTable = new DataTable();

                // Veri tablosunu doldur
                adapter.Fill(dataTable);

                // DataGridView'a veri kaynağını ata
                dataGridView1.DataSource = dataTable;
                if (dataTable.Rows.Count > 0)
                {

                    DataRow row = dataTable.Rows[0];
                    if (row["Resim"] != DBNull.Value)
                    {
                        byte[] imageBytes = (byte[])dataTable.Rows[0]["Resim"];
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    else
                    {
                        // Resmi olmayan hayvan
                        pictureBox1.Image = null;
                    }

                }
                else
                {
                    // Eşleşen bir kayıt bulunamadı
                    MessageBox.Show("Hayvan bulunamadı.");
                }
            }
        }

        private void btnresimekle_Click(object sender, EventArgs e)
        {
            string hayvanAdi = textBox3.Text.Trim();
            string resimYolu;

            // Kullanıcıdan resmi seçmesini iste
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                resimYolu = openFileDialog.FileName;

                // Resmi ekleme işlemi
                try
                {
                    byte[] imageBytes;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Resmi byte dizisine dönüştür
                        Image image = Image.FromFile(resimYolu);
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        imageBytes = ms.ToArray();
                    }

                    // Veritabanına resmi ekle
                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    using (OleDbCommand command = new OleDbCommand("UPDATE Vet SET Resim = @Resim WHERE Ad = @Ad", connection))
                    {
                        connection.Open();
                        command.Parameters.AddWithValue("@Resim", imageBytes);
                        command.Parameters.AddWithValue("@Ad", hayvanAdi);
                        int affectedRows = command.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Resim başarıyla eklendi.");
                        }
                        else
                        {
                            MessageBox.Show("Hayvan bulunamadı.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resim eklenirken bir hata oluştu: " + ex.Message);
                }
            }
        }

        private void btnguncelle_Click(object sender, EventArgs e)
        {
            string hayvanAdi = textBox3.Text.Trim();
            string cins = textBox4.Text.Trim();
            string hastalik = textBox5.Text.Trim();
            string tedaviYontemi = textBox6.Text.Trim();
            string sahipNumarasi = textBox7.Text.Trim();

            // Veritabanında hayvanı bul
            string query = "SELECT * FROM Vet WHERE Ad = @Ad";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Ad", hayvanAdi);

                connection.Open();
                OleDbDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // Hayvan bulundu, güncelleme yap
                    reader.Close();

                    string updateQuery = "UPDATE Vet SET Cins = @Cins, Hastalik = @Hastalik, TedaviYontemi = @TedaviYontemi, SahipNumarasi = @SahipNumarasi WHERE Ad = @Ad";
                    using (OleDbCommand updateCommand = new OleDbCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Cins", cins);
                        updateCommand.Parameters.AddWithValue("@Hastalik", hastalik);
                        updateCommand.Parameters.AddWithValue("@TedaviYontemi", tedaviYontemi);
                        updateCommand.Parameters.AddWithValue("@SahipNumarasi", sahipNumarasi);
                        updateCommand.Parameters.AddWithValue("@Ad", hayvanAdi);

                        int rowsAffected = updateCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Hayvan bilgileri başarıyla güncellendi.");
                        }
                        else
                        {
                            MessageBox.Show("Hayvan bulunamadı.");
                        }
                    }
                }
                else
                {
                    // Hayvan bulunamadı
                    MessageBox.Show("Hayvan bulunamadı.");
                }
            }
        }

        private void btnkayitekle_Click(object sender, EventArgs e)
        {

            string hayvanAdi = textBox3.Text.Trim();
            string cins = textBox4.Text.Trim();
            string hastalik = textBox5.Text.Trim();
            string tedaviYontemi = textBox6.Text.Trim();
            string sahipNumarasi = textBox7.Text.Trim();

            // TextBox'ların boş olup olmadığını kontrol et
            if (string.IsNullOrEmpty(hayvanAdi) || string.IsNullOrEmpty(cins) || string.IsNullOrEmpty(hastalik) || string.IsNullOrEmpty(tedaviYontemi) || string.IsNullOrEmpty(sahipNumarasi))
            {
                MessageBox.Show("Lütfen tüm bilgileri doldurun.");
                return;
            }

            // Veritabanında aynı isimde başka bir hayvan var mı diye kontrol et
            string query = "SELECT COUNT(*) FROM Vet WHERE Ad = @Ad";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Ad", hayvanAdi);

                connection.Open();
                int count = (int)command.ExecuteScalar();

                if (count > 0)
                {
                    // Aynı isimde hayvan bulundu
                    MessageBox.Show("Aynı isimde başka bir hayvan zaten var.");
                    return;
                }
            }

            // Hayvanı veritabanına ekle
            string insertQuery = "INSERT INTO Vet (Ad, Cins, Hastalik, TedaviYontemi, SahipNumarasi) VALUES (@Ad, @Cins, @Hastalik, @TedaviYontemi, @SahipNumarasi)";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            using (OleDbCommand command = new OleDbCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Ad", hayvanAdi);
                command.Parameters.AddWithValue("@Cins", cins);
                command.Parameters.AddWithValue("@Hastalik", hastalik);
                command.Parameters.AddWithValue("@TedaviYontemi", tedaviYontemi);
                command.Parameters.AddWithValue("@SahipNumarasi", sahipNumarasi);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Hayvan başarıyla kaydedildi.");
                    textBox3.Text = "";
                    textBox4.Text = "";
                    textBox5.Text = "";
                    textBox6.Text = "";
                    textBox7.Text = "";
                }
                else
                {
                    MessageBox.Show("Hayvan kaydedilemedi.");
                }
            }

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string imagePath = @"vet.jpg";
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Image = System.Drawing.Image.FromFile(imagePath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.ShowDialog();
        }

        private void btnsil_Click(object sender, EventArgs e)
        {
            string hayvanAdi = textBox1.Text.Trim();
            if (hayvanAdi != "")
            {
                // Veritabanında hayvanı bul
                string query = "SELECT * FROM Vet WHERE Ad = @Ad";

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ad", hayvanAdi);

                    connection.Open();
                    OleDbDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        // Hayvan bulundu, silme işlemini gerçekleştir
                        reader.Close();

                        string deleteQuery = "DELETE FROM Vet WHERE Ad = @Ad";
                        using (OleDbCommand deleteCommand = new OleDbCommand(deleteQuery, connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@Ad", hayvanAdi);

                            int rowsAffected = deleteCommand.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Hayvan başarıyla silindi.");

                            }
                            else
                            {
                                MessageBox.Show("Hayvan silinemedi.");
                            }
                        }
                    }
                    else
                    {
                        // Hayvan bulunamadı
                        MessageBox.Show("Hayvan bulunamadı.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Hayvan adını giriniz.");
            }
        }
    }
}




















//namespace vet
//{
//    public partial class Form2 : Form
//    {
//        private string connectionString = "Data Source=EMIR\\MSSQLSERVER01;Initial Catalog=VeterinerKlinigi;Integrated Security=True"; // Veritabanı bağlantı dizesi

//        public Form2()
//        {
//            InitializeComponent();
//        }

//        private void btnara_Click(object sender, EventArgs e)
//        {
//            string ad = textBox1.Text.Trim();
//            string hastalik = textBox2.Text.Trim();

//            // Arama sorgusu hazırla
//            string query = "SELECT Ad,Cins,Hastalik,TedaviYontemi,SahipNumarasi,Resim FROM Hayvanlar WHERE (@Ad = '' OR Ad LIKE @Ad) AND (@Hastalik = '' OR Hastalik LIKE @Hastalik)";

//            using (SqlConnection connection = new SqlConnection(connectionString))
//            using (SqlCommand command = new SqlCommand(query, connection))
//            {
//                // Parametre ekle
//                command.Parameters.AddWithValue("@Ad", "%" + ad + "%");
//                command.Parameters.AddWithValue("@Hastalik", "%" + hastalik + "%");

//                // Veri okuyucu oluştur
//                SqlDataAdapter adapter = new SqlDataAdapter(command);
//                DataTable dataTable = new DataTable();

//                // Veri tablosunu doldur
//                adapter.Fill(dataTable);

//                // DataGridView'a veri kaynağını ata
//                dataGridView1.DataSource = dataTable;
//                if (dataTable.Rows.Count > 0)
//                {

//                    DataRow row = dataTable.Rows[0];
//                    if (row["Resim"] != DBNull.Value)
//                    {
//                        byte[] imageBytes = (byte[])dataTable.Rows[0]["Resim"];
//                        using (MemoryStream ms = new MemoryStream(imageBytes))
//                        {
//                            pictureBox1.Image = Image.FromStream(ms);
//                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
//                        }
//                    }
//                    else
//                    {
//                        // Resmi olmayan hayvan
//                        pictureBox1.Image = null;
//                    }

//                }
//                else
//                {
//                    // Eşleşen bir kayıt bulunamadı
//                    MessageBox.Show("Hayvan bulunamadı.");
//                }
//            }
//        }

//        private void btnresimekle_Click(object sender, EventArgs e)
//        {
//            string hayvanAdi = textBox3.Text.Trim();
//            string resimYolu;

//            // Kullanıcıdan resmi seçmesini iste
//            OpenFileDialog openFileDialog = new OpenFileDialog();
//            openFileDialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
//            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

//            if (openFileDialog.ShowDialog() == DialogResult.OK)
//            {
//                resimYolu = openFileDialog.FileName;

//                // Resmi ekleme işlemi
//                try
//                {
//                    byte[] imageBytes;
//                    using (MemoryStream ms = new MemoryStream())
//                    {
//                        // Resmi byte dizisine dönüştür
//                        Image image = Image.FromFile(resimYolu);
//                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
//                        imageBytes = ms.ToArray();
//                    }

//                    // Veritabanına resmi ekle
//                    using (SqlConnection connection = new SqlConnection(connectionString))
//                    using (SqlCommand command = new SqlCommand("UPDATE Hayvanlar SET Resim = @Resim WHERE Ad = @Ad", connection))
//                    {
//                        connection.Open();
//                        command.Parameters.AddWithValue("@Resim", imageBytes);
//                        command.Parameters.AddWithValue("@Ad", hayvanAdi);
//                        int affectedRows = command.ExecuteNonQuery();

//                        if (affectedRows > 0)
//                        {
//                            MessageBox.Show("Resim başarıyla eklendi.");
//                        }
//                        else
//                        {
//                            MessageBox.Show("Hayvan bulunamadı.");
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("Resim eklenirken bir hata oluştu: " + ex.Message);
//                }
//            }
//        }

//        private void btnguncelle_Click(object sender, EventArgs e)
//        {
//            string hayvanAdi = textBox3.Text.Trim();
//            string cins = textBox4.Text.Trim();
//            string hastalik = textBox5.Text.Trim();
//            string tedaviYontemi = textBox6.Text.Trim();
//            string sahipNumarasi = textBox7.Text.Trim();

//            // Veritabanında hayvanı bul
//            string query = "SELECT * FROM Hayvanlar WHERE Ad = @Ad";

//            using (SqlConnection connection = new SqlConnection(connectionString))
//            using (SqlCommand command = new SqlCommand(query, connection))
//            {
//                command.Parameters.AddWithValue("@Ad", hayvanAdi);

//                connection.Open();
//                SqlDataReader reader = command.ExecuteReader();

//                if (reader.Read())
//                {
//                    // Hayvan bulundu, güncelleme yap
//                    reader.Close();

//                    string updateQuery = "UPDATE Hayvanlar SET Cins = @Cins, Hastalik = @Hastalik, TedaviYontemi = @TedaviYontemi, SahipNumarasi = @SahipNumarasi WHERE Ad = @Ad";
//                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
//                    {
//                        updateCommand.Parameters.AddWithValue("@Cins", cins);
//                        updateCommand.Parameters.AddWithValue("@Hastalik", hastalik);
//                        updateCommand.Parameters.AddWithValue("@TedaviYontemi", tedaviYontemi);
//                        updateCommand.Parameters.AddWithValue("@SahipNumarasi", sahipNumarasi);
//                        updateCommand.Parameters.AddWithValue("@Ad", hayvanAdi);

//                        int rowsAffected = updateCommand.ExecuteNonQuery();
//                        if (rowsAffected > 0)
//                        {
//                            MessageBox.Show("Hayvan bilgileri başarıyla güncellendi.");
//                        }
//                        else
//                        {
//                            MessageBox.Show("Hayvan bulunamadı.");
//                        }
//                    }
//                }
//                else
//                {
//                    // Hayvan bulunamadı
//                    MessageBox.Show("Hayvan bulunamadı.");
//                }
//            }
//        }

//        private void btnkayitekle_Click(object sender, EventArgs e)
//        {

//            string hayvanAdi = textBox3.Text.Trim();
//            string cins = textBox4.Text.Trim();
//            string hastalik = textBox5.Text.Trim();
//            string tedaviYontemi = textBox6.Text.Trim();
//            string sahipNumarasi = textBox7.Text.Trim();

//            // TextBox'ların boş olup olmadığını kontrol et
//            if (string.IsNullOrEmpty(hayvanAdi) || string.IsNullOrEmpty(cins) || string.IsNullOrEmpty(hastalik) || string.IsNullOrEmpty(tedaviYontemi) || string.IsNullOrEmpty(sahipNumarasi))
//            {
//                MessageBox.Show("Lütfen tüm bilgileri doldurun.");
//                return;
//            }

//            // Veritabanında aynı isimde başka bir hayvan var mı diye kontrol et
//            string query = "SELECT COUNT(*) FROM Hayvanlar WHERE Ad = @Ad";

//            using (SqlConnection connection = new SqlConnection(connectionString))
//            using (SqlCommand command = new SqlCommand(query, connection))
//            {
//                command.Parameters.AddWithValue("@Ad", hayvanAdi);

//                connection.Open();
//                int count = (int)command.ExecuteScalar();

//                if (count > 0)
//                {
//                    // Aynı isimde hayvan bulundu
//                    MessageBox.Show("Aynı isimde başka bir hayvan zaten var.");
//                    return;
//                }
//            }

//            // Hayvanı veritabanına ekle
//            string insertQuery = "INSERT INTO Hayvanlar (Ad, Cins, Hastalik, TedaviYontemi, SahipNumarasi) VALUES (@Ad, @Cins, @Hastalik, @TedaviYontemi, @SahipNumarasi)";
//            using (SqlConnection connection = new SqlConnection(connectionString))
//            using (SqlCommand command = new SqlCommand(insertQuery, connection))
//            {
//                command.Parameters.AddWithValue("@Ad", hayvanAdi);
//                command.Parameters.AddWithValue("@Cins", cins);
//                command.Parameters.AddWithValue("@Hastalik", hastalik);
//                command.Parameters.AddWithValue("@TedaviYontemi", tedaviYontemi);
//                command.Parameters.AddWithValue("@SahipNumarasi", sahipNumarasi);

//                connection.Open();
//                int rowsAffected = command.ExecuteNonQuery();
//                if (rowsAffected > 0)
//                {
//                    MessageBox.Show("Hayvan başarıyla kaydedildi.");
//                    textBox3.Text = "";
//                    textBox4.Text = "";
//                    textBox5.Text = "";
//                    textBox6.Text = "";
//                    textBox7.Text = "";
//                }
//                else
//                {
//                    MessageBox.Show("Hayvan kaydedilemedi.");
//                }
//            }

//        }

//        private void Form2_Load(object sender, EventArgs e)
//        {
//            string imagePath = @"vet.jpg";
//            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
//            pictureBox2.Image = System.Drawing.Image.FromFile(imagePath);
//        }

//        private void button1_Click(object sender, EventArgs e)
//        {
//            Form3 form3 = new Form3();
//            form3.ShowDialog();
//        }

//        private void btnsil_Click(object sender, EventArgs e)
//        {
//            string hayvanAdi = textBox1.Text.Trim();
//            if (hayvanAdi != "")
//            {
//                // Veritabanında hayvanı bul
//                string query = "SELECT * FROM Hayvanlar WHERE Ad = @Ad";

//                using (SqlConnection connection = new SqlConnection(connectionString))
//                using (SqlCommand command = new SqlCommand(query, connection))
//                {
//                    command.Parameters.AddWithValue("@Ad", hayvanAdi);

//                    connection.Open();
//                    SqlDataReader reader = command.ExecuteReader();

//                    if (reader.Read())
//                    {
//                        // Hayvan bulundu, silme işlemini gerçekleştir
//                        reader.Close();

//                        string deleteQuery = "DELETE FROM Hayvanlar WHERE Ad = @Ad";
//                        using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
//                        {
//                            deleteCommand.Parameters.AddWithValue("@Ad", hayvanAdi);

//                            int rowsAffected = deleteCommand.ExecuteNonQuery();
//                            if (rowsAffected > 0)
//                            {
//                                MessageBox.Show("Hayvan başarıyla silindi.");

//                            }
//                            else
//                            {
//                                MessageBox.Show("Hayvan silinemedi.");
//                            }
//                        }
//                    }
//                    else
//                    {
//                        // Hayvan bulunamadı
//                        MessageBox.Show("Hayvan bulunamadı.");
//                    }
//                }
//            }
//            else
//            {
//                MessageBox.Show("Hayvan adını giriniz.");
//            }
//        }
//    }
//}