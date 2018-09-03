using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// CalculateColumn 的摘要说明。
    /// </summary>
    [Serializable]
    public class CalculateColumn : Rect, ICalculateColumn, IDecimal, ISort, IMapName, ISerializable, ICloneable, IDisposable, ICalculateSequence
    {
        #region fields
        protected PrecisionType _precision = PrecisionType.None;
        protected string _formatstring = "";
        protected string _expression = "";
        protected SortOption _sortoption;
        protected StimulateBoolean _bshowwhenzero = StimulateBoolean.None ;
        protected int _calculateindex;
        protected int _pointlength = -1;
        protected string _mapname;
        #endregion

        #region calculator
        public CalculateColumn()
            : base()
        {
        }

        public CalculateColumn(int x, int y)
            : base(x, y)
        {
        }

        public CalculateColumn(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
        }

        public CalculateColumn(string name, string expression)
            : base()
        {
            _name = name;
            Expression = expression;
        }

        public CalculateColumn(string name, string expression, int x, int y)
            : base(x, y)
        {
            _name = name;
            Expression = expression;
        }

        public CalculateColumn(string name, string expression, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _name = name;
            Expression = expression;
        }

        public CalculateColumn(GridLabel gridlalel)
            : base(gridlalel)
        {
            _name = gridlalel.Name;
        }

        public CalculateColumn(GridDecimal griddecimal)
            : base(griddecimal)
        {
            _name = griddecimal.Name;
            _precision = griddecimal.Precision;
            _formatstring = griddecimal.FormatString;
            _sortoption = griddecimal.SortOption;
            _bshowwhenzero = griddecimal.bShowWhenZero;
            _calculateindex = griddecimal.CalculateIndex;
            _pointlength = griddecimal.PointLength;
        }

        public CalculateColumn(GridCalculateColumn gridcalculatecolumn)
            : base(gridcalculatecolumn)
        {
            _name = gridcalculatecolumn.Name;
            _expression = gridcalculatecolumn.Expression;
            _precision = gridcalculatecolumn.Precision;
            _formatstring = gridcalculatecolumn.FormatString;
            _sortoption = gridcalculatecolumn.SortOption;
            _bshowwhenzero = gridcalculatecolumn.bShowWhenZero;
            _calculateindex = gridcalculatecolumn.CalculateIndex;
            _pointlength = gridcalculatecolumn.PointLength;
            _mapname = gridcalculatecolumn.GetMapName;
        }

        public CalculateColumn(CalculateColumn calculatecolumn)
            : base(calculatecolumn)
        {
            _name = calculatecolumn.Name;
            _expression = calculatecolumn.Expression;
            _precision = calculatecolumn.Precision;
            _formatstring = calculatecolumn.FormatString;
            _sortoption = calculatecolumn.SortOption;
            _bshowwhenzero = calculatecolumn.bShowWhenZero;
            _calculateindex = calculatecolumn.CalculateIndex;
            _pointlength = calculatecolumn.PointLength;
            _mapname = calculatecolumn.GetMapName;
        }

        protected CalculateColumn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _expression = info.GetString("Expression");
            _precision = (PrecisionType)info.GetValue("Precision", typeof(PrecisionType));
            _formatstring = info.GetString("FormatString");
            _calculateindex = info.GetInt32("CalculateIndex");
            _pointlength = info.GetInt32("PointLength");
            _sortoption = new SortOption();
            _sortoption.SortDirection = (SortDirection)info.GetValue("Direction", typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
            if (_version >= 20)
                _mapname = info.GetString("MapName");
            if (_version >= 90)
                _bshowwhenzero = (StimulateBoolean)info.GetValue("bShowWhenZero", typeof(StimulateBoolean));
        }
        #endregion

        #region override
        public override void SetType()
        {
            _type = "CalculateColumn";
        }

        //public override void SetSolid()
        public override void SetDefault()
        {
            base.SetDefault();
            _captionalign = ContentAlignment.MiddleRight;
        }

        #endregion

        #region ICalculateColumn 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis19")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis19")]
        [Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Expression
        {
            get
            {
                return _expression;
            }
            set
            {
                _expression = value;
            }
        }

        #endregion

        #region IDecimal 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis20")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis20")]
        [TypeConverter(typeof(PrecisionTypeConvertor))]
        public UFIDA.U8.UAP.Services.ReportElements.PrecisionType Precision
        {
            get
            {
                return _precision;
            }
            set
            {
                _precision = value;
            }
        }

        [Browsable(true )]
        [DisplayText("U8.UAP.Services.ReportElements.Dis21")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis21")]
        public StimulateBoolean bShowWhenZero
        {
            get
            {
                return _bshowwhenzero;
            }
            set
            {
                _bshowwhenzero = value;
            }
        }

        [Browsable(false)]
        public int PointLength
        {
            get
            {
                return _pointlength;
            }
            set
            {
                _pointlength = value;
            }
        }

        #endregion

        #region IFormat 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis22")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis22")]
        public string FormatString
        {
            get
            {
                return _formatstring;
            }
            set
            {
                _formatstring = value;
            }
        }

        #endregion

        #region ISort 成员

        [Browsable(true)]
        [DisplayText("U8.Report.SortOption")]
        [LocalizeDescription("U8.Report.SortOption")]
        public SortOption SortOption
        {
            get
            {
                if (_sortoption == null)
                    _sortoption = new SortOption();
                return _sortoption;
            }
            set
            {
                _sortoption = value;
            }
        }

        #endregion

        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Expression", _expression);
            info.AddValue("bShowWhenZero", _bshowwhenzero);
            info.AddValue("Precision", _precision);
            info.AddValue("FormatString", _formatstring);
            info.AddValue("Direction", SortOption.SortDirection);
            info.AddValue("Priority", SortOption.Priority);
            info.AddValue("CalculateIndex", _calculateindex);
            info.AddValue("PointLength", _pointlength);
            info.AddValue("MapName", _mapname);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new CalculateColumn(this);
        }

        #endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            _expression = null;
            _formatstring = null;
            _sortoption = null;
            base.Dispose();
        }

        #endregion

        #region IMapName 成员

        [Browsable(false)]
        public string MapName
        {
            get
            {
                if (!string.IsNullOrEmpty(_mapname))
                    return _mapname;
                if (!string.IsNullOrEmpty(_expression) && !ExpressionService.CheckIfATrueExpression(_expression))
                {
                    return _expression;
                }
                return _name;
            }
        }

        public void SetMapName(string mapname)
        {
            _mapname = mapname;
        }

        [Browsable(false)]
        public string GetMapName
        {
            get
            {
                return _mapname;
            }
        }

        #endregion

        #region ICalculateSequence 成员

        [DisplayText("U8.UAP.Services.ReportElements.Dis18")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis18")]
        public int CalculateIndex
        {
            get
            {
                return _calculateindex;
            }
            set
            {
                _calculateindex = value;
            }
        }

        #endregion
    }
}
