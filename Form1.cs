using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace vet
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string imagePath = @"vet.jpg";
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = System.Drawing.Image.FromFile(imagePath);
        }

        private void btngiris_Click(object sender, EventArgs e)
        {
            XDocument xDoc = XDocument.Load(@"Admin.xml");
            XElement rootElement = xDoc.Root;

            foreach (XElement User in rootElement.Elements())
            {
                if (txtusername.Text == User.Attribute("Username").Value)
                {
                    if (txtpassword.Text == User.Element("Password").Value)
                    {
                        Form2 f2 = new Form2();
                        f2.Show();
                    }
                    else { MessageBox.Show("Parola yanlış"); return; }
                }
                else { MessageBox.Show("Kullanıcı adı bulunamadı"); return; }
            }
        }
    }
    
}
