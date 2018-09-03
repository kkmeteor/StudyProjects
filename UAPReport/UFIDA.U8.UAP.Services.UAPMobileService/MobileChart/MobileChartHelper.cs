using Infragistics.UltraChart.Shared.Styles;
using Infragistics.Win.UltraWinChart;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using UFIDA.U8.UAP.Services.ReportElements;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// U易联微信报表图表专用
    /// </summary>
    public static class MobileChartHelper
    {
        public static UltraChart CreateAchartByDesignTime(ChartSchemaItemAmong among)
        {
            UltraChart ultraChart = new UltraChart();
            //Color color = ultraChart.BackColor;
            if (!string.IsNullOrEmpty(among.ChartXml))
            {
                using (StringReader sR = new StringReader(among.ChartXml))
                {
                    ultraChart.LoadPreset(sR, true);
                }
            }
            //有些控件只能显示百分比，不能显示数字，所以要屏下面设置
            if (ultraChart.ChartType != ChartType.PieChart3D
                && ultraChart.ChartType != ChartType.DoughnutChart3D)
            {
                if (ultraChart.ChartType == ChartType.PieChart
                 || ultraChart.ChartType == ChartType.DoughnutChart)
                {
                    if (!among.ShowPercent)
                        ChartStyleHelper.InitChartWithSolidProperty(ultraChart);
                }
                else
                    ChartStyleHelper.InitChartWithSolidProperty(ultraChart);
            }

            ultraChart.Axis.Y.TickmarkStyle = AxisTickStyle.Smart;//Y轴数据自动算值的，去除了小数点坐标
            ultraChart.Data.ZeroAligned = true;//是否从0开始算值
            //ultraChart.Margin = new Padding(10, 10, 10, 10);
            //ultraChart.Legend.DataAssociation = ChartTypeData.DefaultData;
            //ultraChart.Legend.SpanPercentage = 10;
            //ultraChart.MouseHover += new EventHandler(ultraChart_MouseHover);
            ////ultraChart.Axis.Y.TickmarkStyle = AxisTickStyle.Percentage;
            ////ultraChart.Axis.Y.Labels.VerticalAlign = StringAlignment.Near;
            ////ultraChart.Axis.X.Labels.VerticalAlign = StringAlignment.Near;
            //ultraChart.Axis.Y.Labels.Flip = true;
            ultraChart.ColorModel.ModelStyle = ColorModels.LinearRange;
            //ultraChart.ColorModel.ColorBegin = Color.LimeGreen;
            //ultraChart.ColorModel.ColorEnd = Color.Gold;
            ultraChart.ColorModel.ColorBegin = Color.LimeGreen;
            ultraChart.ColorModel.ColorEnd = Color.Gray;
            //ultraChart.ColorModel.Scaling = ColorScaling.Oscillating;
            //ultraChart.Data.EmptyStyle.LineStyle.MidPointAnchors = false;
            //ultraChart.Data.EmptyStyle.LineStyle.StartStyle = LineCapStyle.NoAnchor;
            ultraChart.TitleTop.Text = among.Caption;//图表名称
            ultraChart.TitleTop.HorizontalAlign = StringAlignment.Center;
            ultraChart.BackColor = Color.White;
            ultraChart.Axis.X.Labels.SeriesLabels.Font = new Font("宋体", 9, FontStyle.Regular);
            ultraChart.Axis.Y.Labels.Font = new Font("宋体", 9, FontStyle.Regular);
            ultraChart.Axis.Y2.Labels.Font = new Font("宋体", 9, FontStyle.Regular);

            if (among.Width != 0)
            {
                ultraChart.Width = among.Width;
                ultraChart.Height = among.Height;
            }
            ((Control)ultraChart).Location = among.Position;
            ultraChart.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            //ChartTextAppearance text = new ChartTextAppearance();
            //text.Column = -2;
            //text.Row = -2;
            //text.PositionFromRadius = 50;
            //text.VerticalAlign = System.Drawing.StringAlignment.Far;
            //text.ItemFormatString = "<DATA_VALUE:00.00>";
            //text.Visible = true;
            //ultraChart.ColumnChart.ChartText.Add(text);


            return ultraChart;
        }

        public static string TransferDataTableToString(DataTable dataTable)
        {
            // 序列化DataTable
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb);
            XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
            serializer.Serialize(writer, dataTable);
            writer.Close();
            return sb.ToString();
        }

        internal static string TransferDataTableToString(MobileChart mobileChart)
        {
            string result = Serialize<MobileChart>(mobileChart);
            return result;
        }
        /// <summary>
        /// 序列化 对象到字符串
        /// </summary>
        /// <param name="obj">泛型对象</param>
        /// <returns>序列化后的字符串</returns>
        public static string Serialize<T>(T obj)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                stream.Close();
                return Convert.ToBase64String(buffer);
            }
            catch (Exception ex)
            {
                throw new Exception("序列化失败,原因:" + ex.Message);
            }
        }
        /// <summary>
        /// 反序列化 字符串到对象
        /// </summary>
        /// <param name="obj">泛型对象</param>
        /// <param name="str">要转换为对象的字符串</param>
        /// <returns>反序列化出来的对象</returns>
        public static T Desrialize<T>(T obj, string str)
        {
            try
            {
                obj = default(T);
                IFormatter formatter = new BinaryFormatter();
                byte[] buffer = Convert.FromBase64String(str);
                MemoryStream stream = new MemoryStream(buffer);
                obj = (T)formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("反序列化失败,原因:" + ex.Message);
            }
            return obj;
        }
    }
}
