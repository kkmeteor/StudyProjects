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
	/// BorderSideEditor ��ժҪ˵����
	/// </summary>
	public class DataSourceEditor:UITypeEditor
	{
		public DataSourceEditor()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			// get the editor service.
			IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

			if (edSvc == null) 
			{
				// uh oh.
				return value;
			}

            DataSourceEditorControl dsec = new DataSourceEditorControl(edSvc);
			dsec.Source=(DataSource)value;
            object editobject = context.Instance;
            dsec.EditObject = editobject;
            dsec.Init();
			edSvc.DropDownControl(dsec);
		
			// return the updated value;
			return dsec.Source;
		}


	}

}
