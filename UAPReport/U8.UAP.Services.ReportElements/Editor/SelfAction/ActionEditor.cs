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
	public class ActionEditor:UITypeEditor
	{
		private ActionEditorForm dsec;
		public ActionEditor()
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
				dsec=new  ActionEditorForm();
			}
			dsec.EditCollection  =(value as ICloneable).Clone() as ICollectonEditTypes  ;
			if(edSvc.ShowDialog(dsec)==DialogResult.OK)
				return dsec.EditCollection  ;
			return value;
		}
	}
}
