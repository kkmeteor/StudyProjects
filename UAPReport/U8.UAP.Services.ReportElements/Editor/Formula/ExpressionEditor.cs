using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using UFIDA.U8.UAP.Services.ExpressionDesigner;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// BorderSideEditor 的摘要说明。
	/// </summary>
	public class ExpressionEditor:UITypeEditor
	{
		public ExpressionEditor()
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
				return value;
            ExpressionEditorForm dsec = GetExpressionEditorForm(value.ToString(), context.Instance
                , context.PropertyDescriptor.Name);
			if(edSvc.ShowDialog(dsec)==DialogResult.OK)
				return dsec.Source;
			return value;
		}

        public ExpressionEditorForm GetExpressionEditorForm(string source,object editobject,string pname)
        {
            ExpressionEditorForm dsec = new ExpressionEditorForm();
            dsec.Source = source;
            dsec.EditObject = editobject;

            if (editobject is Expression)
            {
                //(editobject as Formula).TypeChanging -= new TypeChangingHandler(ExpressionEditor_TypeChanging);
                //(editobject as Formula).TypeChanging += new TypeChangingHandler(ExpressionEditor_TypeChanging);
                //if ((editobject as Formula).Type == FormulaType.Business)
                //    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.BusinessFunction;
                if ((editobject as Expression).Formula.Type == FormulaType.Common)
                    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.CommonFunction;
                else if ((editobject as Expression).Formula.Type == FormulaType.UserDefine)
                    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.UserDefine;
                if ((editobject as Expression).Formula.Type == FormulaType.Filter)
                    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.Filter;
                dsec.Text = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx5", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            }
            else// if(editobject is ICalculateColumn)
            {
                if (pname == "UserDefineItem")
                {
                    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.UserDefine;
                    dsec.Text = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx5", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                }
                else if (pname == "FilterString")
                {
                    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.Free;
                    dsec.Text = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx7", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                }
                else if (pname == "MapKeys")
                {
                    dsec.ScriptType = ScriptType.MapKeys;
                    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.Script;
                    dsec.Text = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx7", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                }
                else if (pname == "Expression")
                {
                    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.ColumnExpression;
                    dsec.Text = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx6", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                }
                else
                {
                    if (pname == "PrepaintEvent")
                    {
                        dsec.ScriptType = ScriptType.PreEvent;
                    }
                    else if (pname == "InitEvent")
                    {
                        dsec.ScriptType = ScriptType.InitEvent;
                    }
                    else if (pname == "GroupFilter")
                    {
                        dsec.ScriptType = ScriptType.GroupFilter;
                    }
                    else
                    {
                        dsec.ScriptType = ScriptType.Algorithm;
                    }
                    dsec.Type = ExpressionDesigner.ExpressionDesigner.EditType.Script;
                    dsec.Text = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx7", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                }
            }
            return dsec;
        }

        //private bool ExpressionEditor_TypeChanging(Formula formula)
        //{
        //    if (formula.FormulaExpression != null && formula.FormulaExpression != "")
        //    {
        //        System.Windows.Forms.MessageBox.Show(UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.ExpressionEditor.请先清空表达式", System.Threading.Thread.CurrentThread.CurrentUICulture.Name));
        //        return false;
        //    }
        //    return true;
        //}
	}
}
