using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public partial class TemplateGaugeDesign : Form
    {
        private int _index;
        public TemplateGaugeDesign()
        {
            InitializeComponent();
        }

        public int TemplateIndex
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                Control pic = this.Controls[0].Controls["pictureBox" + value.ToString()];
                if (pic != null)
                    ((PictureBox)pic).BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {
            _index = Convert.ToInt32(((Control)sender).Name.Substring(10));
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            pic.BorderStyle = BorderStyle.Fixed3D;
            for (int i = 1; i <= 12; i++)
            {
                PictureBox pb = this.Controls[0].Controls["pictureBox" + i.ToString()] as PictureBox;
                if (pb != null && pb != pic)
                {
                    if (i == _index)
                        pb.BorderStyle = BorderStyle.FixedSingle;
                    else
                        pb.BorderStyle = BorderStyle.None;
                }
            }
        }

        private void TemplateGaugeDesign_Load(object sender, EventArgs e)
        {

        }


    }
}