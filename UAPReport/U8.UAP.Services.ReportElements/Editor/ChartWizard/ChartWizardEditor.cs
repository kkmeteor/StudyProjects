using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Runtime.Serialization;
using Infragistics.UltraChart.Design;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class ChartWizardEditor : UITypeEditor
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


            Chart chart = context.Instance as Chart;
            OpenChartWizard(chart);
            //ChartService cs = null;
            //OpenChartWizard(chart, ref _chartwizard, ref cs);                   
            //if (edSvc.ShowDialog(_chartwizard) == DialogResult.OK)
            //{
            //    cs.SaveSettings(_chartwizard);
            //}
            return value;
        }

        public void OpenChartWizard(Chart chart)
        {
            ChartService _chartservice = null;
            string id = null;
            if (chart.Report.Type == ReportType.IndicatorReport)
            {
                id = "IndicatorChart";
                Hashtable ctn = new Hashtable();
                SimpleHashtable sht = new SimpleHashtable();
                if (!string.IsNullOrEmpty(chart.DataSource))
                {
                    Section indicatordetail = chart.Report.Sections[SectionType.IndicatorDetail];
                    IIndicatorMetrix metrix = indicatordetail.Cells[chart.DataSource] as IIndicatorMetrix;
                    if (metrix == null)
                        throw new Exception("非法图表数据源矩阵");

                    sht.Add("__bcross__", metrix.CrossPart != null);
                    if (metrix.IndicatorParts != null)
                    {
                        foreach (IPart indi in metrix.IndicatorParts)
                            sht.Add((indi as Cell).Caption, (indi as Cell).Name);
                    }
                }
                else
                    sht.Add("__bcross__", false);
                ctn.Add(chart.Level, sht);
                _chartservice = new IndicatorChartService(chart.Report, ctn);
            }
            else
                _chartservice = new ChartService(chart.Report);

            ChartWizardAdapter _wizardadapter = new ChartWizardAdapter(_chartservice);

            if (_wizardadapter.ChartWizard.ShowDialog(_chartservice.GetSchemasAllLevel(chart.Level), chart.Level , id) == DialogResult.OK)
            {
                RemoteDataHelper rdh = DefaultConfigs.GetRemoteHelper();
                rdh.SaveChartStrings(ClientReportContext.Login.UfMetaCnnString, chart.Report.ViewID, chart.Report.ChartStrings);
            }
            else
                chart.Report.ChartStrings = null;
            chart.ChartWizard = null;
        }
    }
}
