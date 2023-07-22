using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dart
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        bool password_right=false;
        public string username;
        private void button3_Click(object sender, EventArgs e)
        {
            string acc = "test";
            string pas = "1234";
            username = "Dennis";
            if (textBox1.Text.Equals(acc) && textBox2.Text.Equals(pas))
            {
                password_right = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("帳號或密碼錯誤");
            }
        }
        private void Form2_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (password_right == false)
            {
                System.Environment.Exit(0);
            }
            else
            {
            }            
        }

    }
}
