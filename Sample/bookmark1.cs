using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sample
{
    public partial class bookmark1 : Form
    {
        private string name;
        private Boolean iscreat = false;
        public bookmark1()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e){

            name = textBox1.Text;
            textBox1.Text = "";
            iscreat = true;
            this.Close();
 
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            iscreat = false;
            textBox1.Text = "";
            this.Close();
        }

        private void txtBookmark_TextChanged(object sender, EventArgs e) {

            if (textBox1.Text == "")
            {
                button1.Enabled = false;
            }
            else {
                button1.Enabled = true;
            }
        }
        //是否创建书签变量为只读
        public Boolean Check
        {
            get { return iscreat; }
        }

        //设置书签名为只读
        public string Bookmark
        {
            get { return name; }
        }

    }
}
