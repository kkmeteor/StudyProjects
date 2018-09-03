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

using Infragistics.UltraChart.Core.ColorModel;
using Infragistics.UltraChart.Core;
using Infragistics.UltraChart.Core.Layers;
using Infragistics.UltraChart.Core.Primitives;
using Infragistics.UltraChart.Data;
using Infragistics.UltraChart.Resources;
using Infragistics.UltraChart.Resources.Appearance;
using Infragistics.UltraChart.Resources.Editor;
using Infragistics.UltraChart.Shared.Styles;
using Infragistics.UltraChart.Core.Util;

using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class Gauge: Rect, ISerializable, ICloneable, IDisposable,IGap
    {
        protected GaugeType _gaugetype= GaugeType.Plat ;
        protected int _templateindex=0;
        protected string _indicatorname;
        protected Color _needlecolor=Color.White ;
        protected int _needlelength = 55;

        protected Color _tickcolor = Color.White;
        protected Color _linecolor = Color.White;
        protected Color _fontcolor = Color.White;
        protected Color _gaugecolor = Color.CornflowerBlue;

        protected int _sectionstart=50;
        protected int _sectionend=65;

        protected int _tickstart=65;
        protected int _tickend =80;
        protected int _textloc=85;

        protected int _gapheight=4;

        protected double _maxtick = 100;
        protected double _mintick = 0;

        protected bool _bsemicircle = true;
        
        protected CalculatorIndicator _indicator;
        protected bool _needupdate = true;
        protected int _invalidcode = 0;//1--cv is negative;2--value is negative
        protected Infragistics.Win.UltraWinChart.UltraChart _mychart;

        public Gauge()
            : base()
        {
        }
        public Gauge(int x, int y)
            : base(x, y)
        {
        }

        public Gauge(Gauge gauge)
            : base(gauge)
        {
            _indicatorname = gauge.IndicatorName;
            _templateindex = gauge.TemplateIndex;
            _indicator = gauge.Indicator;
            _gaugetype = gauge.GaugeType;
            _needlecolor = gauge.NeedleColor;
            _needlelength = gauge.NeedleLength;
            _fontcolor = gauge.FontColor;
            _linecolor = gauge.LineColor;
            _tickcolor = gauge.TickColor;
            _sectionstart = gauge.SectionStart;
            _sectionend = gauge.SectionEnd;
            _tickstart = gauge.TickStart;
            _tickend = gauge.TickEnd;
            _textloc = gauge.TextLoc;
            _gapheight = gauge.GapHeight;
            _maxtick = gauge.MaxTick;
            _mintick = gauge.MinTick;
            _bsemicircle = gauge.bSemiCircle;
            _gaugecolor = gauge.GaugeColor;
        }

        public Gauge(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _gaugetype = (GaugeType )info.GetValue("GaugeType", typeof(GaugeType ));
            _templateindex = info.GetInt32("TemplateIndex");
            _indicatorname = info.GetString("IndicatorName");
            _needlecolor =(Color) info.GetValue("NeedleColor", typeof(Color));
            _needlelength = info.GetInt32("NeedleLength");
            _fontcolor = (Color)info.GetValue("TickFontColor", typeof(Color));
            _linecolor = (Color)info.GetValue("LineColor", typeof(Color));
            _tickcolor = (Color)info.GetValue("TickColor", typeof(Color));
            _sectionstart = info.GetInt32("SectionStart");
            _sectionend = info.GetInt32("SectionEnd");
            _tickstart = info.GetInt32("TickStart");
            _tickend = info.GetInt32("TickEnd");
            _textloc = info.GetInt32("TextLoc");
            _gapheight = info.GetInt32("GapHeight");
            _maxtick = info.GetDouble ("MaxTick");
            _mintick = info.GetDouble("MinTick");
            _bsemicircle = info.GetBoolean("SemiCircle");
            _gaugecolor = (Color)info.GetValue("GaugeColor", typeof(Color));
        }

        public override void SetType()
        {
            _type = "Gauge";
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _w = 200;
            _h = 130;
            this.Border.NoneBorder();
            
        }
        
        protected override void draw(Graphics g)
        {
            _bactive = false;
            if (_needupdate)
                Update();

            if (_invalidcode == 0)
            {
                if (!_bsemicircle)
                {
                    if (_w < 130)
                        _w = 130;
                    if (_h < 130)
                        _h = 130;

                    base.DrawBackGround(g);
                    int w = Math.Min(_w, _h);
                    MyChart.Width = w + 80;
                    MyChart.Height = w + 80;//_h;            
                    g.DrawImage(MyChart.Image, new Rectangle(_x, _y, w, w), new Rectangle(40, 40, w, w), GraphicsUnit.Pixel);
                }
                else
                {
                    base.DrawBackGround(g);
                    MyChart.Width = _w + 80;
                    MyChart.Height = _w + 80;
                    g.DrawImage(MyChart.Image, new Rectangle(_x, _y, _w, _w / 2 + 20), new Rectangle(40, 40, _w, _w / 2 + 20), GraphicsUnit.Pixel);
                }
            }
            else
            {
                if (_invalidcode == 1)
                    _caption = "比较值为负数";
                else if (_invalidcode == 2)
                    _caption = "指标值为负数";
                base.DrawBackGround(g);
                g.DrawImage(String4Report.GetImage("error.png"), new Rectangle(_x, _y, 40, 40));
                using (SolidBrush sbfore = new SolidBrush(Color.Red ))
                {
                    g.DrawString(_caption, this.ClientFont, sbfore, new Rectangle(_x + 41, _y + 10, 100, 20));
                }

            }
            base.DrawBorder(g);
        }

        protected void DrawGaugeBorder(Graphics g)
        {
            //_x, _y, _w - 1, _h - 1
            using (Pen pen = new Pen(_bordercolor ))
            {
                if(_borderside.Left)
                    g.DrawLine(pen, new Point(_x,_y),new Point(_x,_y+_h-1));
                if (_borderside.Right )
                    g.DrawLine(pen, new Point(_x+_w-1, _y), new Point(_x+_w-1, _y + _h - 1));
                if (_borderside.Top )
                    g.DrawLine(pen, new Point(_x, _y), new Point(_x+_w-1, _y));
                if (_borderside.Bottom )
                    g.DrawLine(pen, new Point(_x, _y+_h-1), new Point(_x+_w-1, _y + _h - 1));
            }
        }

        public override void SetRuntimeHeight(Graphics g, string s)
        {
            _runtimeheight = _h;
        }

        [Browsable(false)]
        public override int ExpandHeight
        {
            get
            {
                return base.MetaHeight;
            }
        }

        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GaugeType", _gaugetype );
            info.AddValue("TemplateIndex", _templateindex);
            info.AddValue("IndicatorName", _indicatorname);
            info.AddValue("NeedleColor", _needlecolor);
            info.AddValue("NeedleLength", _needlelength);
            info.AddValue("TickFontColor", _fontcolor);
            info.AddValue("LineColor", _linecolor);
            info.AddValue("TickColor", _tickcolor);
            info.AddValue("SectionStart", _sectionstart);
            info.AddValue("SectionEnd", _sectionend);
            info.AddValue("TickStart", _tickstart);
            info.AddValue("TickEnd", _tickend);
            info.AddValue("TextLoc", _textloc);
            info.AddValue("GapHeight", _gapheight);
            info.AddValue("MaxTick", _maxtick);
            info.AddValue("MinTick", _mintick);
            info.AddValue("SemiCircle", _bsemicircle);
            info.AddValue("GaugeColor", _gaugecolor);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new Gauge (this);
        }

        #endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            // TODO:  添加 Chart.Dispose 实现
        }

        #endregion
        [Browsable(false)]
        public Color GaugeColor
        {
            get
            {
                return _gaugecolor;
            }
            set
            {
                _gaugecolor = value;
            }
        }

        [Browsable(false)]
        public bool bSemiCircle
        {
            get
            {
                return _bsemicircle;
            }
            set
            {
                _bsemicircle = value;
            }
        }

        [Browsable(false)]
        public double  MaxTick
        {
            get
            {
                return _maxtick;
            }
            set
            {
                _maxtick = value;
            }
        }

        [Browsable(false)]
        public double MinTick
        {
            get
            {
                return _mintick ;
            }
            set
            {
                _mintick  = value;
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
        public int TextLoc
        {
            get
            {
                return _textloc ;
            }
            set
            {
                _textloc  = value;
            }
        }


        [Browsable(false)]
        public int TickStart
        {
            get
            {
                return _tickstart ;
            }
            set
            {
                _tickstart  = value;
            }
        }

        [Browsable(false)]
        public int TickEnd
        {
            get
            {
                return _tickend ;
            }
            set
            {
                _tickend  = value;
            }
        }

        [Browsable(false)]
        public int SectionStart
        {
            get
            {
                return _sectionstart ;
            }
            set
            {
                _sectionstart  = value;
            }
        }

        [Browsable(false)]
        public int SectionEnd
        {
            get
            {
                return _sectionend ;
            }
            set
            {
                _sectionend = value;
            }
        }

        [Browsable(false)]
        public Color LineColor
        {
            get
            {
                return _linecolor ;
            }
            set
            {
                _linecolor  = value;
            }
        }

        [Browsable(false)]
        public Color FontColor
        {
            get
            {
                return _fontcolor;
            }
            set
            {
                _fontcolor = value;
            }
        }

        [Browsable(false)]
        public Color TickColor
        {
            get
            {
                return _tickcolor;
            }
            set
            {
                _tickcolor = value;
            }
        }

        [DisplayText("U8.UAP.Report.指针长度")]
        [LocalizeDescription("U8.UAP.Report.指针长度")]
        public int NeedleLength
        {
            get
            {
                return _needlelength;
            }
            set
            {
                _needlelength = value;
                _needupdate = true;
                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public Infragistics.Win.UltraWinChart.UltraChart MyChart
        {
            get
            {
                if (_mychart ==null)
                    _mychart = new Infragistics.Win.UltraWinChart.UltraChart();
                return _mychart;
            }
        }

        [Browsable(false)]
        public CalculatorIndicator Indicator
        {
            get
            {
                return _indicator;
            }
            set
            {
                _indicator = value;
            }
        }

        [TypeConverter(typeof(GaugeTemplateTypeConverter))]
        [Editor(typeof(GaugeTemplateEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DisplayText("U8.UAP.Report.详细设计")]
        [LocalizeDescription("U8.UAP.Report.详细设计")]
        public int TemplateIndex
        {
            get
            {
                return _templateindex;
            }
            set
            {
                //_templateindex = (value==-1?0:value);
                _needupdate = true;
                //if (_templateindex > 0)
                //    _gaugetype = GaugeType.Template;
                OnPropertyChanged();
            }
        }

        [DisplayText("U8.UAP.Report.样式")]
        [LocalizeDescription("U8.UAP.Report.样式")]
        public GaugeType GaugeType
        {
            get
            {
                return _gaugetype;
            }
            set
            {
                _gaugetype = value;
                _needupdate = true;
                //if (value != GaugeType.Template)
                //    _templateindex = 0;
                //else if (_templateindex == 0)
                //    _templateindex = 1;
                if (value == GaugeType.Blue3D)
                {
                    _templateindex = 0;
                    _bsemicircle = false;
                }
                else
                    _templateindex = -1;
                OnPropertyChanged();
            }
        }

        [DisplayText("U8.UAP.Report.指针颜色")]
        [LocalizeDescription("U8.UAP.Report.指针颜色")]
        public Color NeedleColor
        {
            get
            {
                return _needlecolor;
            }
            set
            {
                _needlecolor = value;
                _needupdate = true;
                OnPropertyChanged();
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis16")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
        public string IndicatorName
        {
            get
            {
                return _indicatorname; ;
            }
            set
            {
                _indicatorname = value;
            }
        }

        protected void OnPropertyChanged()
        {
            OnOtherChanged(null);
        }

        private void Update()
        {
            //if (_gaugetype == GaugeType.Blue3D )
            //    UpdateTemplate();
            //else
                UpdateNormal();
            _needupdate = false;
        }

        private void UpdateNormal(double[] ds, Color[] cs)
        {
            GaugeLayer theLayer = null;
            if (MyChart.Layer.Contains("GaugeLayer"))
                theLayer = _mychart.Layer["GaugeLayer"] as GaugeLayer;
            else
            {
                theLayer = new GaugeLayer();
                theLayer.Appearance = new GaugeAppearance();

                theLayer.Appearance.Radius = 300;
                //theLayer.Appearance.Center = new Point(190, 190);

                //theLayer.Appearance.DialPE.Stroke = Color.Black;
                //theLayer.Appearance.DialPE.StrokeWidth = 5;

                theLayer.ChartComponent = MyChart;

                _mychart.Layer.Add("GaugeLayer", theLayer);
                _mychart.UserLayerIndex = new string[] { "GaugeLayer" };

                // Set axes
                _mychart.ChartType = ChartType.PieChart;
                _mychart.Legend.Visible = false;

                _mychart.Axis.Y.Labels.ItemFormatString = "<DATA_VALUE:0>";
                _mychart.Axis.Y.MajorGridLines.Thickness = 5;
                _mychart.Axis.Y.MinorGridLines.Thickness = 2;
                _mychart.Axis.Y.MajorGridLines.DrawStyle = LineDrawStyle.Solid;
                _mychart.Axis.Y.MinorGridLines.DrawStyle = LineDrawStyle.Solid;

                // Set data source (this is meaningless to the gauge layer)
                _mychart.Data.DataSource = Infragistics.UltraChart.Data.DemoTable.Table();
                _mychart.Data.DataBind();
                //this.ultraChart1.BackgroundImage = System.Drawing.Image.FromFile(Config.ImagePath + @"\chart_gray_bg.jpg");
            }

            #region sections & needle
            theLayer.Appearance.Sections.Clear();
            theLayer.Appearance.Needles.Clear();
            GaugeSection section = null;

            _invalidcode = 0;
            double maxtick = _maxtick;
            if (!double.IsNaN(ds[0]))
            {
                if (ds[0] < 0)
                    _invalidcode = 1;
                maxtick = Math.Max(maxtick, ds[0]);
            }
            if (!double.IsNaN(ds[1]))
            {
                if (ds[1] < 0)
                    _invalidcode = 1;
                maxtick = Math.Max(maxtick, ds[1]);
            }
            if (!double.IsNaN(ds[2]))
            {
                if (ds[2] < 0)
                    _invalidcode = 2;
                maxtick = Math.Max(maxtick, ds[2]);
            }
            if (!double.Equals(maxtick, 100.0))
            {
                long mt = Convert.ToInt64(Math.Ceiling(maxtick));
                if (mt % 100 == 0)
                {
                    if (mt < 100)
                        maxtick = 100;
                    else
                        maxtick = Convert.ToDouble((mt / 100 ) * 100);
                }
                else
                    maxtick = Convert.ToDouble((mt / 100 + 1) * 100);
            }

            if (!double.IsNaN(ds[0]))
            {
                if (!double.IsNaN(ds[1]))
                {
                    section = new GaugeSection(ds[0] - _mintick);
                    section.StartWidth = _sectionstart;
                    section.EndWidth = _sectionend;
                    section.PE.ElementType = PaintElementType.SolidFill;//.Gradient;
                    section.PE.FillGradientStyle = GradientStyle.Horizontal;
                    section.PE.Fill = cs[0];//Color.Green;
                    //section.PE.FillStopColor = Color.Yellow;
                    theLayer.Appearance.Sections.Add(section);

                    section = new GaugeSection(ds[1] - ds[0]);
                    section.StartWidth = _sectionstart;
                    section.EndWidth = _sectionend;
                    section.PE.ElementType = PaintElementType.SolidFill;//.Gradient;
                    section.PE.FillGradientStyle = GradientStyle.Horizontal;
                    section.PE.Fill = cs[1];//Color.Green;
                    //section.PE.FillStopColor = Color.Yellow;
                    theLayer.Appearance.Sections.Add(section);

                    section = new GaugeSection(maxtick - ds[1]);
                    section.StartWidth = _sectionstart;
                    section.EndWidth = _sectionend;
                    section.PE.ElementType = PaintElementType.SolidFill;//.Gradient;
                    section.PE.FillGradientStyle = GradientStyle.Horizontal;
                    section.PE.Fill = cs[2];//Color.Green;
                    //section.PE.FillStopColor = Color.Yellow;
                    theLayer.Appearance.Sections.Add(section);
                }
                else
                {
                    section = new GaugeSection(ds[0] - _mintick);
                    section.StartWidth = _sectionstart;
                    section.EndWidth = _sectionend;
                    section.PE.ElementType = PaintElementType.SolidFill;//.Gradient;
                    section.PE.FillGradientStyle = GradientStyle.Horizontal;
                    section.PE.Fill = cs[0];//Color.Green;
                    //section.PE.FillStopColor = Color.Yellow;
                    theLayer.Appearance.Sections.Add(section);

                    section = new GaugeSection(maxtick - ds[0]);
                    section.StartWidth = _sectionstart;
                    section.EndWidth = _sectionend;
                    section.PE.ElementType = PaintElementType.SolidFill;//.Gradient;
                    section.PE.FillGradientStyle = GradientStyle.Horizontal;
                    section.PE.Fill = cs[2];//Color.Green;
                    //section.PE.FillStopColor = Color.Yellow;
                    theLayer.Appearance.Sections.Add(section);
                }
            }
            else
            {
                section = new GaugeSection(maxtick - _mintick);
                section.StartWidth = 1;
                section.EndWidth = 2;
                section.PE.ElementType = PaintElementType.SolidFill;//.Gradient;
                section.PE.FillGradientStyle = GradientStyle.Horizontal;
                section.PE.Fill = _gaugecolor;
                theLayer.Appearance.Sections.Add(section);
            }
            
            if (!double.IsNaN(ds[2]))
            {
                Needle nv = new Needle(ds[2], new PaintElement(_needlecolor));
                nv.PE.StrokeWidth = 12;
                nv.Length = _needlelength;
                theLayer.Appearance.Needles.Add(nv);
            }
            #endregion

            theLayer.Appearance.StartAngle = _bsemicircle ? 0 : -45;
            theLayer.Appearance.EndAngle = _bsemicircle ? 180 : 225;
            theLayer.Appearance.TextLoc = _textloc ;
            theLayer.Appearance.TickStart = _tickstart ;
            theLayer.Appearance.TickEnd = _tickend ;

            _mychart.Axis.Y.Visible = true;
            _mychart.Axis.Y.Labels.Visible = true;
            _mychart.Axis.Y.MajorGridLines.Visible = true;
            _mychart.Axis.Y.MinorGridLines.Visible = true;

            _mychart.Axis.Y.MajorGridLines.Color = _tickcolor;
            _mychart.Axis.Y.MinorGridLines.Color = _tickcolor;
            _mychart.Axis.Y.Labels.FontColor = _fontcolor;
            _mychart.Axis.Y.LineColor = _linecolor;

            if (_gaugetype == GaugeType.Blue3D )
                theLayer.Appearance.DialPE = new PaintElement(String4Report.GetImage("Template0.gif"));
            else
                theLayer.Appearance.DialPE = new PaintElement(_gaugecolor  );
                //theLayer.Appearance.DialPE = new PaintElement(Color.WhiteSmoke, Color.CornflowerBlue, GradientStyle.Elliptical);

            //if (_caption != null)
            //{
            //    _mychart.TitleBottom.Text = _caption;
            //    _mychart.TitleBottom.FontSizeBestFit = true;
            //    _mychart.TitleBottom.HorizontalAlign = StringAlignment.Center;
            //}
            _mychart.InvalidateLayers();
        }

        private void UpdateNormal()
        {
            double[] ds = new double[] { 40, 60, 30 };
            Color[] cs = new Color[] { Color.Red , Color.Yellow,Color.Green  };
            if (_understate != ReportStates.Designtime)
                SetData(ds, cs);
            UpdateNormal(ds, cs);
        }

        private void SetData(double[] ds, Color[] cs)
        {
            if (_indicator != null && _indicator.CompareValue != null)
            {
                if (!string.IsNullOrEmpty(_indicator.CompareValue.Expression1))
                {
                    double d1 = Convert.ToDouble(_indicator.CompareValue.Expression1);
                    if (!string.IsNullOrEmpty(_indicator.CompareValue.Expression2))
                    {
                        double d2 = Convert.ToDouble(_indicator.CompareValue.Expression2);
                        ds[0] = Math.Min(d1, d2);
                        ds[1] = Math.Max(d1, d2);
                        switch (_indicator.CompareValue.Performance)
                        {
                            case IndicatorPerformance.GreaterBetter:
                                cs[0] = Color.Red;
                                cs[1] = Color.Yellow;
                                cs[2] = Color.Green;
                                break;
                            case IndicatorPerformance.LessBetter:
                                cs[0] = Color.Green;
                                cs[1] = Color.Yellow;
                                cs[2] = Color.Red;
                                break;
                            case IndicatorPerformance.AmidBetter:
                                cs[0] = Color.Red;
                                cs[1] = Color.Green;
                                cs[2] = Color.Red;
                                break;
                            case IndicatorPerformance.BothSidesBetter:
                                cs[0] = Color.Green;
                                cs[1] = Color.Red;
                                cs[2] = Color.Green;
                                break;
                        }
                    }
                    else
                    {
                        ds[0] = d1;
                        ds[1] = double.NaN;
                        switch (_indicator.CompareValue.Performance)
                        {
                            case IndicatorPerformance.GreaterBetter:
                                cs[0] = Color.Red;
                                cs[1] = Color.Empty;
                                cs[2] = Color.Green;
                                break;
                            case IndicatorPerformance.LessBetter:
                                cs[0] = Color.Green;
                                cs[1] = Color.Empty;
                                cs[2] = Color.Red;
                                break;
                        }
                    }
                }
                else
                {
                    ds[0] = double.NaN;
                    ds[1] = double.NaN;
                }
                
            }
            else
            {
                ds[0] = double.NaN;
                ds[1] = double.NaN;
                
            }
            if (_indicator != null)
                ds[2] = string.IsNullOrEmpty(_indicator.Caption) ? 0 : Convert.ToDouble(_indicator.Caption);
            else
                ds[2] = double.NaN;
        }

    }

    public enum GaugeType
    {
        //SelfTick,
        //SelfTick3D,
        //Template
        Plat,
        Blue3D
    }
}
