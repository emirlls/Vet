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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("Boş bırakılamaz.");
            }
            else
            {
                XDocument xDoc1 = new XDocument();
                xDoc1 = XDocument.Load(@"Admin.xml");

                XElement rootElement1 = xDoc1.Root;

                foreach (XElement User in rootElement1.Elements())
                {
                    User.Attribute("Username").Value = textBox1.Text;
                    User.Element("Password").Value = textBox2.Text;
                    xDoc1.Save(@"Admin.xml");
                    textBox1.Text = "";
                    textBox2.Text = "";
                    MessageBox.Show("Kullanıcı Bilgileri Değiştirildi.");

                }
            }
        }
    }
}
