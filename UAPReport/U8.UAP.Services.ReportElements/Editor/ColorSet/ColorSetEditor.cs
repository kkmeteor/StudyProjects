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
using UFIDA.U8.UAP.Services.ReportColorSet;
using System.Xml;
using UFIDA.U8.UAP.Services.ReportResource;
namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class ColorSetEditor : UITypeEditor
    {
        public ColorSetEditor()
		{
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (edSvc == null) 
				return value;
            string datasourcexml;
            try
            {
                datasourcexml = getDataSourceXml(context.Instance);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, String4Report.GetString("Report"));
                return value;
            }
            string colsxml = getColsXml(context.Instance);
            string configXml = value.ToString();
            //string errs = "";
            //clsColorSet cs = new clsColorSet();
            //if (!cs.ColorSet(ClientReportContext.Login.U8LoginClass, datasourcexml, colsxml, ref configXml, false, ref errs))
            //{
            //    if (errs != "")
            //    {
            //        MessageBox.Show(errs);
            //        return "";
            //    }
            //    else
            //        return value;
            //}
            //return configXml;

            //【pengzhzh】修改编辑类型为下拉列表
            if (context != null && provider != null)
            {
                clsColorSet cs = new clsColorSet();
                try
                {
                    cs.ShowFormatCondition(edSvc, ClientReportContext.Login.U8LoginClass, datasourcexml, colsxml, ref configXml, false);
                    return configXml;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,String4Report.GetString("Report"));
                }
            }
            return value;
		}


        private string getDataSourceXml(object _object)
        {
            Report report = null;
            if (_object is Cell)
            {
                (_object as Cell).GetReport();
                report = (_object as Cell).Report;
            }
            else if (_object is Report)
            {
                report = _object as Report;
            }
            else
            {
                return null;
            }

            if (report.GridDetailCells == null)
            {
                throw new Exception(String4Report.GetString("U8.UAP.Services.ReportColorSet.不支持操作"));
            }
            DataSources dss = report.DataSources;
            //report.GridDetailCells.GetBySource(
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("cols");
            XmlElement ele;
            doc.AppendChild(root);
            foreach (string name in dss.Keys)
            {
                DataSource ds = dss[name];
                ele = doc.CreateElement("col");
                ele.SetAttribute("id", "id");
                //
                // pengzhzh
                //
                //Cell c = report.GridDetailCells.GetBySource(ds.Name);
                //if (c != null)
                //{
                //    ele.SetAttribute("fld", c.Name);
                //}
                //else
                //{
                //    ele.SetAttribute("fld", ds.Name);
                //}
                ele.SetAttribute("fld", ds.Name);
                ele.SetAttribute("name", ds.Caption);
                ele.SetAttribute("datatype", GetNodeType(ds.Type));
                root.AppendChild(ele);
            }
            return doc.InnerXml;
        }
        private string getColsXml(object _object)
        {            
            Report report = null;
            if (_object is Cell)
            {
                (_object as Cell).GetReport();
                report = (_object as Cell).Report;
            }
            else if (_object is Report)
            {
                report = _object as Report;
            }
            else
            {
                return null;
            }
            Section r = null;
            if(report.Type==ReportType.CrossReport)
                r=report.Sections[SectionType.CrossRowHeader];
            if (report.Type == ReportType.FreeReport)
                r = report.Sections[SectionType.Detail];
            if (report.Type == ReportType.GridReport)
                r = report.Sections[SectionType.GridDetail];
            if (report.Type == ReportType.IndicatorReport)
                r = report.Sections[SectionType.IndicatorDetail];
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("cols");
            XmlElement ele;
            doc.AppendChild(root);
            for (int i = 0; i < r.Cells.Count;i++ )
            {
                if (!(r.Cells[i] is IMapName))
                {
                    continue;
                }
                ele = doc.CreateElement("col");
                ele.SetAttribute("id", i.ToString());
                ele.SetAttribute("fld", r.Cells[i].Name);
                ele.SetAttribute("name", r.Cells[i].Caption);
                string mapname = ((r.Cells[i] as IMapName).MapName != null) ?
                    (r.Cells[i] as IMapName).MapName : string.Empty;
                ele.SetAttribute("mapname", mapname);

                root.AppendChild(ele);
                root.AppendChild(ele);
            }
            return doc.InnerXml;
        }
        private string GetNodeType(DataType type)
        {
            switch (type)
            {
                case DataType.Boolean:
                    return "Boolean";
                case DataType.Currency:
                    return "Currency";
                case DataType.DateTime:
                    return "DateTime";
                case DataType.Decimal:
                    return "Decimal";
                case DataType.Int:
                    return "Int";
                default:
                    return "String";
            }
        }

    }
}
