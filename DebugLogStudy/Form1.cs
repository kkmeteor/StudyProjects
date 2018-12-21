using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DebugLogStudy
{
    public partial class Form1 : Form
    {
        protected override void OnResize(EventArgs e)
        {
            //System.Diagnostics.Debug.Assert(this.Width > 200, "Width should be larger than 200.");
            //System.Diagnostics.Debug.WriteLine("SIZE = " + Size.ToString());
        }
        public Form1()
        {
            InitializeComponent();
            System.Diagnostics.Trace.Listeners.Clear();
            System.Diagnostics.Trace.AutoFlush = true;
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("app.log"));

            Calculate();
        }
        private void Calculate()
        {
            int a = 1, b = 1;
            try
            {
                System.Random r = new Random();
                while(true)
                {
                    a = (int)(r.NextDouble() * 10);
                    b = (int)(r.NextDouble() * 10);
                    System.Diagnostics.Trace.WriteLine(System.DateTime.Now.ToString()+":    "+ a.ToString()+"/"+b.ToString() +"=" +(a/b).ToString());
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(System.DateTime.Now.ToString() + ":    " + a.ToString() + "/" + b.ToString() + "="+"Error:   "+ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
    }
}
