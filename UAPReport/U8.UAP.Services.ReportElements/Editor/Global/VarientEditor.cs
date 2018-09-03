using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// VarientEditor 的摘要说明。
	/// </summary>
	public class VarientEditor:UITypeEditor
	{
		private VarientEditorForm dsec;
		public VarientEditor()
		{
		}

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
			if (dsec == null) 
			{
				dsec=new  VarientEditorForm();
			}
			dsec.Varients=(value as GlobalVarients).Clone() as GlobalVarients ;
			if(edSvc.ShowDialog(dsec)==DialogResult.OK)
				return dsec.Varients;
			return value;
		}
	}
}
