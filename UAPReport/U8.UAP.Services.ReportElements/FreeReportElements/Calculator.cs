using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Calculator 的摘要说明。
	/// </summary>
	[Serializable]
	public class Calculator:Rect,ISerializable,ICloneable,ICalculator,IMapName,ICalculateSequence,IDecimal,ICalculateColumn,IDisposable
	{
		#region fields
		protected OperatorType _operator=OperatorType.SUM;
		protected DataSource _unit = new DataSource("EmptyColumn");
		protected int _calculateindex;
		protected PrecisionType _precision=PrecisionType.None;
		protected string _formatstring="";
		protected string _expression="";
		protected StimulateBoolean  _bshowwhenzero= StimulateBoolean.None ;
        protected bool _bfromgriddecimal = false;
        protected int _pointlength = -1;
        //protected bool _baseonscript = false;
        protected string _mapname;
		#endregion

		#region constructor
		public Calculator():base()
		{

		}

		public Calculator(int x,int y):base(x,y)
		{
		}

		public Calculator(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public Calculator(string name,string expression):base()
		{
			_name=name;
			Expression=expression;
		}

		public Calculator(int x,int y,string name,string expression):base(x,y)
		{
			_name=name;
			Expression=expression;
		}

		public Calculator(int x,int y,int width,int height,string name,string expression):base(x,y,width,height)
		{
			_name=name;
			Expression=expression;
		}

        public Calculator(DataSource ds)
            : base()
        {
            _name = ds.Name;
            _expression = ds.Name;
            _cncaption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
            _caption = ds.Caption;
        }

		public Calculator(Calculator calculator):base(calculator)
		{
			_name=calculator.Name ;
			_expression=calculator.Expression;
			_bshowwhenzero=calculator.bShowWhenZero;
			_precision=calculator.Precision;
			_formatstring=calculator.FormatString;
			_operator=calculator.Operator;
			_unit=calculator.Unit;
			_calculateindex=calculator.CalculateIndex;
            _pointlength = calculator.PointLength;
            //_baseonscript = calculator.BaseOnScript;
            _mapname = calculator.GetMapName;
		}

		protected Calculator( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_expression=info.GetString("Expression");
			_calculateindex=info.GetInt32("CalculateIndex");
			_operator=(OperatorType)info.GetValue("Operator",typeof(OperatorType));
			_unit=(DataSource)info.GetValue("Unit",typeof(DataSource));
			_precision=(PrecisionType)info.GetValue("Precision",typeof(PrecisionType));
			_formatstring=info.GetString("FormatString");
            _pointlength = info.GetInt32("PointLength");

            if (_version >= 20)
                _mapname = info.GetString("MapName");
            if (_version >= 90)
                _bshowwhenzero = (StimulateBoolean)info.GetValue("bShowWhenZero", typeof(StimulateBoolean));
		}

		public Calculator(GridDecimal griddecimal):base(griddecimal)
		{
            _bfromgriddecimal = true;
			_expression=griddecimal.DataSource.Name ;
			_calculateindex=griddecimal.CalculateIndex;
			_operator=griddecimal.Operator;
			_unit=griddecimal.Unit;
			_precision=griddecimal.Precision;
			_formatstring=griddecimal.FormatString;
			_bshowwhenzero=griddecimal.bShowWhenZero;
            _caption=griddecimal.Caption;
            _pointlength = griddecimal.PointLength;
		} 

		public Calculator(GridCalculateColumn gridcalculatecolumn):base(gridcalculatecolumn)
		{
			_expression=gridcalculatecolumn.Expression   ;
			_calculateindex=gridcalculatecolumn.CalculateIndex;
			_operator=gridcalculatecolumn.Operator;
			_unit=gridcalculatecolumn.Unit;
			_precision=gridcalculatecolumn.Precision;
			_formatstring=gridcalculatecolumn.FormatString;
			_bshowwhenzero=gridcalculatecolumn.bShowWhenZero;
			_caption=gridcalculatecolumn.Caption;
            _pointlength = gridcalculatecolumn.PointLength;
            _mapname = gridcalculatecolumn.GetMapName;
		}

        public Calculator(GridDecimalAlgorithmColumn  gridcalculatecolumn)
            : base(gridcalculatecolumn)
        {
            _expression = gridcalculatecolumn.Name ;
            _calculateindex = gridcalculatecolumn.CalculateIndex;
            _operator = gridcalculatecolumn.Operator;
            _unit = gridcalculatecolumn.Unit;
            _precision = gridcalculatecolumn.Precision;
            _formatstring = gridcalculatecolumn.FormatString;
            _bshowwhenzero = gridcalculatecolumn.bShowWhenZero;
            _caption = gridcalculatecolumn.Caption ;
            _pointlength = gridcalculatecolumn.PointLength;
        } 
		#endregion

		#region override
		public override void SetType()
		{
			_type="Calculator";
		}
        //public override void SetSolid()
        public override void SetDefault()
        {
            base.SetDefault();
            _captionalign = ContentAlignment.MiddleRight;
        }

		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("CalculateIndex",_calculateindex);
			info.AddValue("Operator",_operator);
			info.AddValue("Unit",_unit);
			info.AddValue("Precision",_precision);
			info.AddValue("FormatString",_formatstring);
			info.AddValue("bShowWhenZero",_bshowwhenzero);
			info.AddValue("Expression",_expression);
            info.AddValue("PointLength", _pointlength);
            info.AddValue("MapName", _mapname );
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new Calculator(this);
		}

		#endregion

		#region ICalculator 成员

		[Browsable(true)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis23")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis23")]
        [TypeConverter(typeof(OperatorTypeConvertor))]
		public UFIDA.U8.UAP.Services.ReportElements.OperatorType Operator
		{
			get
			{
				return _operator;
			}
			set
			{
				_operator=value;
			}
		}

		[Browsable(false)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis24")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis24")]
		public DataSource Unit
		{
			get
			{
				return _unit;
			}
			set
			{
				_unit=value;
			}
		}

		#endregion

		#region ICalculateSequence 成员

		[Browsable(true)]
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
				_calculateindex=value;
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
				_precision=value;
			}
		}

		[Browsable(true)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis21")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis21")]
		public StimulateBoolean  bShowWhenZero
		{
			get
			{
				return _bshowwhenzero;
			}
			set
			{
				_bshowwhenzero=value;
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
				_formatstring=value;
			}
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
				_expression=value;
			}
		}
        

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _unit = null;
            _expression = null;
            _formatstring = null;
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
                //if (_expression != "" && (_bfromgriddecimal  || _expression.IndexOfAny(new char[] { '+', '-', '*', '/' }) == -1))
                //    return _expression;
                //return _name;
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
	}
}
