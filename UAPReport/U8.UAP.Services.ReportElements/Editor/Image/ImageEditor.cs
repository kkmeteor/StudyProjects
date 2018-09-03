using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.IO;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// BorderSideEditor 的摘要说明。
	/// </summary>
	public class ImageEditor:UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return System.Drawing.Design.UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

			if (edSvc == null) 
			{
				return value;
			}
            string imagestring = SelectImage();
            if (imagestring == null)
                return value;
            else
                return imagestring;
		}

        public string SelectImage()
        {
            Stream imageStream;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx3", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((imageStream = openFileDialog1.OpenFile()) != null)
                {
                    if (imageStream.Length > 50000)
                    {
                        MessageBox.Show(UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx4", System.Threading.Thread.CurrentThread.CurrentUICulture.Name));
                        return null;
                    }
                    BinaryReader br = new BinaryReader(imageStream);
                    byte[] imageByte = br.ReadBytes((int)imageStream.Length);
                    return Convert.ToBase64String(imageByte);
                }

            }
            return null;
        }
	}

}
