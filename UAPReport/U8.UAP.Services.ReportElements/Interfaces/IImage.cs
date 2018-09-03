using System;
using System.Windows.Forms;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IImage 的摘要说明。
	/// </summary>
	public interface IImage
	{
		string ImageString{get;set;}
        System.Drawing.Image MyImage { get;}
		PictureBoxSizeMode SizeMode{get;set;}
		event EventHandler BeforeSizeModeChanged;
		event EventHandler AfterSizeModeChanged;
		void SetBack(string imagestring,int w,int h,PictureBoxSizeMode sizemode);
	}
}
