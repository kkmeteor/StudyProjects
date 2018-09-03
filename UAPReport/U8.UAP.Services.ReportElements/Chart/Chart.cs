using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class Chart : Rect, ISerializable, ICloneable, IDisposable,IGap
    {
        protected Infragistics.Win.UltraWinChart.UltraChart _mychart;
        protected int _level=-1;
        private string _datasource="";
        private DataTable _data;
        private Hashtable _captiontoname;
        private int _gapheight=4;
        private bool _exceptionchart = false;

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Chart()
            : base()
        {
        }

        public Chart(int x, int y)
            : base(x, y)
        {
        }

        public Chart(Chart chart)
            : base(chart)
        {
            _level = chart.Level;
            _datasource = chart.DataSource;
            _data = chart.Data;
            _gapheight = chart.GapHeight;
            _captiontoname = chart.CaptionToName;
        }

        public Chart(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _level = info.GetInt32("Level");
            if (_version >= 90)
            {
                _datasource = info.GetString("DataSource");
                _gapheight = info.GetInt32("GapHeight");
            }
        }
        #endregion

        #region override
        public override void SetType()
        {
            _type = "Chart";
        }

        public override void SetDefault()
        {
            _w = 400;
            _h = 300;
            _borderside.NoneBorder();
            _bordercolor = Color.Black;
        }

        protected override void draw(Graphics g)
        {
            if (!_exceptionchart )
            {
                MyChart.Left = _x + 1;
                MyChart.Top = _y + 1;
                MyChart.Width = _w - 1;
                MyChart.Height = _h - 1;
                MyChart.BorderStyle = BorderStyle.None;
                MyChart.Border.Thickness = 1;
                MyChart.Border.Color = Color.White;
                MyChart.BackColor = Color.White;

                g.DrawImage(MyChart.Image, new Rectangle(_x + 1, _y + 1, _w - 1, _h - 1));
                base.DrawBorder(g);
            }
            else
                base.draw(g);
        }

        public override void SetRuntimeHeight(Graphics g,string s)
        {
            _runtimeheight = _h;
        }

        [Browsable(false)]
        public override bool bActive
        {
            get
            {
                return false;
            }
            set
            {
                //base.bActive = value;
            }
        }

        [Browsable(false)]
        public override bool bInActiveRow
        {
            get
            {
                return false;
            }
            set
            {
                //base.bInActiveRow = value;
            }
        }

        [Browsable(false)]
        public override int ExpandHeight
        {
            get
            {
                return base.MetaHeight;
            }
        }

        [DisplayText("U8.UAP.Report.下边距")]
        [LocalizeDescription("U8.UAP.Report.下边距")]
        public int GapHeight
        {
            get
            {
                return _gapheight;
            }
            set
            {
                _gapheight = value;
            }
        }

        [Browsable(false)]
        public int Level
        {
            get
            {
                return _parent != null && !(_parent is IndicatorDetail)  ? (_parent.Level + 1) : _level;
            }
            set
            {
                _level = value;
            }
        }

        #endregion

        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Level", Level);
            info.AddValue("DataSource", _datasource);
            info.AddValue("GapHeight", _gapheight);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new Chart(this);
        }

        #endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            // TODO:  添加 Chart.Dispose 实现
        }

        #endregion

        protected void OnPropertyChanged()
        {
            OnOtherChanged(null);
        }

        [Browsable(false)]
        public bool bExceptionChart
        {
            get
            {
                return _exceptionchart;
            }
            set
            {
                _exceptionchart = value;
            }
        }

        //运行时从外付值
        [Browsable(false)]
        public Infragistics.Win.UltraWinChart.UltraChart MyChart
        {
            get
            {
                if (bNullChart )
                {
                    NewChart();
                    InitMyChart();
                }
                return _mychart;
            }
            set
            {
                _mychart = value;
            }
        }

        [Browsable(false)]
        public bool bNullChart
        {
            get
            {
                return _mychart == null;
            }
        }

        private void NewChart()
        {
            _mychart = new Infragistics.Win.UltraWinChart.UltraChart();
        }

        private void InitMyChart()
        {
            this.GetReport();
            ChartService cs = null;
            if (this.Report.Type == ReportType.IndicatorReport)
                cs = new IndicatorChartService(this.Report , _captiontoname );
            else
                cs = new ChartService(this.Report );
            if (bNullChart)
                NewChart();
            cs.InitializeChart(Level,null,_mychart);
            _mychart.Data.DataSource = DemoDataSourceForChartType(_mychart.ChartType);
            _mychart.Data.DataBind();
        }

        private object DemoDataSourceForChartType(Infragistics.UltraChart.Shared.Styles.ChartType type)
        {
            switch (type)
            {
                case Infragistics.UltraChart.Shared.Styles.ChartType.ProbabilityChart:
                    return Infragistics.UltraChart.Data.DemoTable.Table(8);
                case Infragistics.UltraChart.Shared.Styles.ChartType.StepAreaChart:
                case Infragistics.UltraChart.Shared.Styles.ChartType.StepLineChart:
                case Infragistics.UltraChart.Shared.Styles.ChartType.CandleChart:
                    return Infragistics.UltraChart.Data.DemoTable.Table(1);
                case Infragistics.UltraChart.Shared.Styles.ChartType.GanttChart:
                    return Infragistics.UltraChart.Data.DemoTable.Table(4);
                case Infragistics.UltraChart.Shared.Styles.ChartType.PolarChart:
                    return Infragistics.UltraChart.Data.DemoTable.Table(5);
                case Infragistics.UltraChart.Shared.Styles.ChartType.BoxChart:
                    return Infragistics.UltraChart.Data.DemoTable.BoxChartData;
                case Infragistics.UltraChart.Shared.Styles.ChartType.BubbleChart3D:
                    return new double[,] {
											{45.0, 50.0, 38.0, 18.0},
											{60.0, 80.0, 35.0,  4.0},
											{45.0, 50.0, 67.0,  7.0},
											{60.0, 90.0, 46.0,  2.0},
											{65.0, 50.0, 74.0, 25.0},
											{70.0, 30.0, 26.0,  7.0},
											{20.0, 34.0, 67.0,  3.0}
										 };
                default:
                    return Infragistics.UltraChart.Data.DemoTable.AllPositive();
            }
        }

        [TypeConverter(typeof(ChartWizardTypeConverter))]
        [Editor(typeof(ChartWizardEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DisplayText("U8.Report.ChartSchema")]
        [LocalizeDescription("U8.Report.ChartSchema")]
        public string ChartWizard
        {
            get
            {
                return "";
            }
            set
            {
                InitMyChart();
                OnPropertyChanged();
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis16")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
        public string DataSource
        {
            get
            {
                return _datasource;
            }
            set
            {
                _datasource = value;
            }
        }

        [Browsable(false)]
        public DataTable Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        [Browsable(false)]
        public Hashtable CaptionToName
        {
            get
            {
                return _captiontoname;
            }
            set
            {
                _captiontoname = value;
            }
        }
    }
}
