using System;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Text;
namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// GridProportionDecimal 的摘要说明。占比列继承DBDecimal 处理输出事件
    /// </summary>
    [Serializable]
    public class GridProportionDecimal : GridDecimal, IGridCollect, IGridEvent, ISerializable, ICloneable, IDisposable, IUserDefine, IWithSizable, IMergeStyle
    {
        #region Constructor
        public GridProportionDecimal()
            : base()
        {
        }

        public GridProportionDecimal(int x, int y)
            : base(x, y)
        {
        }

        public GridProportionDecimal(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
        }

        public GridProportionDecimal(GridProportionDecimal GridProportionDecimal)
            : base(GridProportionDecimal)
        {
            _bsummary = GridProportionDecimal.bSummary;
            _bcolumnsummary = GridProportionDecimal.bColumnSummary;
            _bclue = GridProportionDecimal.bClue;
            _operator = GridProportionDecimal.Operator;
            _unit = GridProportionDecimal.Unit;
            _calculateindex = GridProportionDecimal.CalculateIndex;
            _eventtype = GridProportionDecimal.EventType;
            _bshowatreal = GridProportionDecimal.bShowAtReal;
            _bcalcaftercross = GridProportionDecimal.bCalcAfterCross;
            _bMergeCell = GridProportionDecimal.bMergeCell;
        }

        public GridProportionDecimal(DataSource ds)
            : base(ds)
        {
        }

        public GridProportionDecimal(IDataSource ds)
            : base(ds)
        {
            _datasource = ds.DataSource;
        }

        public GridProportionDecimal(IMapName map,DataSource ds)
            : base(map, ds)
        {
            _datasource = ds;
            _bcalcaftercross = false;
            _bsummary = (map as IGridCollect).bSummary;
            _bclue = (map as IGridCollect).bClue;
            _operator = (map as IGridCollect).Operator;
            _unit = (map as IGridCollect).Unit;
            _calculateindex = (map as IGridCollect).CalculateIndex;
            _eventtype = (map as IGridEvent).EventType;
            _bshowatreal = (map as IGridEvent).bShowAtReal;

            _precision = (map as IDecimal).Precision;
            _formatstring = (map as IDecimal).FormatString;
            _sortoption = (map as ISort).SortOption;
            _bshowwhenzero = (map as IDecimal).bShowWhenZero;
            _pointlength = (map as IDecimal).PointLength;
        }

        protected GridProportionDecimal(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bsummary = info.GetBoolean("bSummary");
            _bclue = info.GetBoolean("bClue");
            _operator = (OperatorType)info.GetValue("Operator", typeof(OperatorType));
            _unit = (DataSource)info.GetValue("Unit", typeof(DataSource));
            _calculateindex = info.GetInt32("CalculateIndex");
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bcolumnsummary = info.GetBoolean("bColumnSummary");
            _bshowatreal = info.GetBoolean("bShowAtReal");
            _bcalcaftercross = info.GetBoolean("bCalcAfterCross");
            if (_version > 1)
                _userdefineitem = info.GetString("UserDefineItem");
            try
            {
                _bMergeCell = info.GetBoolean("bMergeCell");
            }
            catch
            {
                _bMergeCell = false;
            }
        }
        #endregion

        #region override
        public override void SetType()
        {
            _type = "GridProportionDecimal";
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _backcolor = DefaultConfigs.DefaultTitleBackColor;
            _captionalign = System.Drawing.ContentAlignment.MiddleRight;
            _visibleposition = 0;
        }

        [Browsable(false)]
        public override System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Font ClientFont
        {
            get
            {
                return base.ClientFont;
            }
            set
            {
                base.ClientFont = value;
            }
        }

        public override void SetSolid()
        {

        }

        [Browsable(false)]
        public override System.Drawing.ContentAlignment CaptionAlign
        {
            get
            {
                return base.CaptionAlign;
            }
            set
            {
                base.CaptionAlign = value;
            }
        }

        #endregion
        #region IDataSource 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis16")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
        public override DataSource DataSource
        {
            get
            {
                return _datasource;
            }
            set
            {
                _datasource = value;
                if (_caption == "" && value != null)
                {                               
                    _caption = _datasource.Caption+"%";
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("string crossinfo=\"\";");
                    sb.AppendLine("if(report.CrossTable != null && report.CrossTable.Trim()!=\"\")");
                    sb.AppendLine("{");
                    sb.AppendLine("if(cell.CrossIndex.ToString()==\"-1\")");
                    sb.AppendLine("crossinfo=\"\";");
                    sb.AppendLine("else");
                    sb.AppendLine("crossinfo=\"__\"+cell.CrossIndex.ToString();");
                    sb.AppendLine("}");
                    sb.AppendLine("if(Convert.ToDouble(reportsummary[\"" + _datasource.Name + "\"+crossinfo])==0)");
                    sb.AppendLine("cell.Caption=0.00;");
                    sb.AppendLine("else if(current!=null)");
                    sb.AppendLine("cell.Caption= 100*Convert.ToDouble(current[\"" + _datasource.Name + "\"+crossinfo])/Convert.ToDouble(reportsummary[\"" + _datasource.Name + "\"+crossinfo]);");
                    sb.AppendLine("else if (currentgroup != null)");
                    sb.AppendLine("if (currentgroup[\"" + _datasource.Name + "\"+crossinfo].ToString()==\"\")");
                    sb.AppendLine("cell.Caption=0.00;");
                    sb.AppendLine("else");
                    sb.AppendLine("cell.Caption= 100*Convert.ToDouble(currentgroup[\"" + _datasource.Name + "\"+crossinfo])/Convert.ToDouble(reportsummary[\"" + _datasource.Name + "\"+crossinfo]);");
                    sb.AppendLine("else");
                    sb.AppendLine("cell.Caption=100.00;");  
                  
                    _prepaintevent = sb.ToString();
                    EventType=ReportElements.EventType.BothContentAndSummary;
                    OnOtherChanged(null);
                }
            }
        }
         
        #endregion

       
        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);            
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new GridProportionDecimal(this);
        }

        #endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion
    }
    /*
     交叉报表交叉列处理方式，由于交叉报表不支持同数据源所以11.0暂不支持，可变通解决
     double summary=0;
if(reportsummary["数量__"+cell.CrossIndex.ToString()]!=null)
summary=Convert.ToDouble(reportsummary["数量__"+cell.CrossIndex.ToString()]);
else
summary=Convert.ToDouble(reportsummary["数量"]);
if(summary==0)cell.Caption=0.00;
else if(current!=null)
cell.Caption= 100*Convert.ToDouble(current["数量__"+cell.CrossIndex.ToString()])/summary;
else if (currentgroup != null)
if (currentgroup["数量__"+cell.CrossIndex.ToString()].ToString()=="")
cell.Caption=0.00;
else
cell.Caption= 100*Convert.ToDouble(currentgroup["数量__"+cell.CrossIndex.ToString()])/summary;
else
cell.Caption=100.00;
     */
    [Serializable]
    public class GridProportionDecimalIndicator : GridDecimal, IGridCollect, IGridEvent, ISerializable, ICloneable, IDisposable, IUserDefine, IWithSizable, IMergeStyle, IPart
    {
        #region Constructor
        public GridProportionDecimalIndicator()
            : base()
        {
        }

        public GridProportionDecimalIndicator(int x, int y)
            : base(x, y)
        {
        }

        public GridProportionDecimalIndicator(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
        }

        public GridProportionDecimalIndicator(GridProportionDecimalIndicator GridProportionDecimal)
            : base(GridProportionDecimal)
        {
            _bsummary = GridProportionDecimal.bSummary;
            _bcolumnsummary = GridProportionDecimal.bColumnSummary;
            _bclue = GridProportionDecimal.bClue;
            _operator = GridProportionDecimal.Operator;
            _unit = GridProportionDecimal.Unit;
            _calculateindex = GridProportionDecimal.CalculateIndex;
            _eventtype = GridProportionDecimal.EventType;
            _bshowatreal = GridProportionDecimal.bShowAtReal;
            _bcalcaftercross = GridProportionDecimal.bCalcAfterCross;
            _bMergeCell = GridProportionDecimal.bMergeCell;
        }

        public GridProportionDecimalIndicator(DataSource ds)
            : base(ds)
        {
        }

        public GridProportionDecimalIndicator(IDataSource ds)
            : base(ds)
        {
            _datasource = ds.DataSource;
        }

        public GridProportionDecimalIndicator(IMapName map, DataSource ds)
            : base(map, ds)
        {
            _datasource = ds;
            _bcalcaftercross = false;
            _bsummary = (map as IGridCollect).bSummary;
            _bclue = (map as IGridCollect).bClue;
            _operator = (map as IGridCollect).Operator;
            _unit = (map as IGridCollect).Unit;
            _calculateindex = (map as IGridCollect).CalculateIndex;
            _eventtype = (map as IGridEvent).EventType;
            _bshowatreal = (map as IGridEvent).bShowAtReal;

            _precision = (map as IDecimal).Precision;
            _formatstring = (map as IDecimal).FormatString;
            _sortoption = (map as ISort).SortOption;
            _bshowwhenzero = (map as IDecimal).bShowWhenZero;
            _pointlength = (map as IDecimal).PointLength;
        }

        protected GridProportionDecimalIndicator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bsummary = info.GetBoolean("bSummary");
            _bclue = info.GetBoolean("bClue");
            _operator = (OperatorType)info.GetValue("Operator", typeof(OperatorType));
            _unit = (DataSource)info.GetValue("Unit", typeof(DataSource));
            _calculateindex = info.GetInt32("CalculateIndex");
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bcolumnsummary = info.GetBoolean("bColumnSummary");
            _bshowatreal = info.GetBoolean("bShowAtReal");
            _bcalcaftercross = info.GetBoolean("bCalcAfterCross");
            if (_version > 1)
                _userdefineitem = info.GetString("UserDefineItem");
            try
            {
                _bMergeCell = info.GetBoolean("bMergeCell");
            }
            catch
            {
                _bMergeCell = false;
            }
        }
        #endregion

        #region override
        public override void SetType()
        {
            _type = "GridProportionDecimalIndicator";
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _backcolor = DefaultConfigs.DefaultTitleBackColor;
            _captionalign = System.Drawing.ContentAlignment.MiddleRight;
            _visibleposition = 0;
        }

        [Browsable(false)]
        public override System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Font ClientFont
        {
            get
            {
                return base.ClientFont;
            }
            set
            {
                base.ClientFont = value;
            }
        }

        public override void SetSolid()
        {

        }

        [Browsable(false)]
        public override System.Drawing.ContentAlignment CaptionAlign
        {
            get
            {
                return base.CaptionAlign;
            }
            set
            {
                base.CaptionAlign = value;
            }
        }

        #endregion
        #region IDataSource 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis16")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
        public override DataSource DataSource
        {
            get
            {
                return _datasource;
            }
            set
            {
                _datasource = value;
                if (_caption == "" && value != null)
                {
                    _caption = _datasource.Caption + "%"; 
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("string crossinfo=\"\";");
                    sb.AppendLine("if(report.CrossTable != null && report.CrossTable.Trim()!=\"\")");
                    sb.AppendLine("{");
                    sb.AppendLine("crossinfo=\"__\"+cell.CrossIndex.ToString();");
                    sb.AppendLine("}");
                    sb.AppendLine("if(Convert.ToDouble(reportsummary[\"" + _datasource.Name + "\"+crossinfo])==0)");
                    sb.AppendLine("cell.Caption=0.00;");
                    sb.AppendLine("else if(current!=null)");
                    sb.AppendLine("cell.Caption= 100*Convert.ToDouble(current[\"" + _datasource.Name + "\"+crossinfo])/Convert.ToDouble(reportsummary[\"" + _datasource.Name + "\"+crossinfo]);");
                    sb.AppendLine("else if (currentgroup != null)");
                    sb.AppendLine("if (currentgroup[\"" + _datasource.Name + "\"+crossinfo].ToString()==\"\")");
                    sb.AppendLine("cell.Caption=0.00;");
                    sb.AppendLine("else");
                    sb.AppendLine("cell.Caption= 100*Convert.ToDouble(currentgroup[\"" + _datasource.Name + "\"+crossinfo])/Convert.ToDouble(reportsummary[\"" + _datasource.Name + "\"+crossinfo]);");
                    sb.AppendLine("else");
                    sb.AppendLine("cell.Caption=100.00;");  
                    _prepaintevent = sb.ToString();
                    EventType = ReportElements.EventType.BothContentAndSummary;
                    OnOtherChanged(null);
                }
            }
        }

        #endregion


        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new GridProportionDecimal(this);
        }

        #endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion


        #region IPart Members

        private IMetrix _metrix;
        [Browsable(false)]
        public IMetrix Metrix
        {
            get
            {
                return _metrix;
            }
            set
            {
                _metrix = value;
            }
        }
        [Browsable(false)]
        public PartType PartType
        {
            get { return PartType.Indicator; }
        }

        #endregion
    }
}
