using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Service1Client service = new Service1Client();
            WcfService.CompositeType type = new WcfService.CompositeType();
            type.BoolValue = true;
            type.StringValue = "MaTengfei";
            Label label1 = new Label();
            label1.Text =service.GetDataUsingDataContract(type).StringValue;
            this.Controls.Add(label1);
            label1.Location = new Point(20, 20);
        }
    }
}
