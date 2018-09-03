using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// DecimalAlgorithmColumn 的摘要说明。
	/// </summary>
	[Serializable]
	public class DecimalAlgorithmColumn:AlgorithmColumn,IDecimal,ICalculateSequence,ISerializable,ICloneable,IDisposable
	{
		#region fields
		protected PrecisionType _precision=PrecisionType.None;
        //protected string _formatstring="";
		protected StimulateBoolean  _bshowwhenzero= StimulateBoolean.None ;
        protected int _pointlength = -1;
		#endregion

		#region constructor
		public DecimalAlgorithmColumn():base()
		{
		}

		public DecimalAlgorithmColumn(int x,int y):base(x,y)
		{
		}

		public DecimalAlgorithmColumn(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public DecimalAlgorithmColumn(string algorithm):base(algorithm)
		{
		}

		public DecimalAlgorithmColumn(string algorithm,int x,int y):base(algorithm,x,y)
		{
		}

		public DecimalAlgorithmColumn(string algorithm,int x,int y,int width,int height):base(algorithm,x,y,width,height)
		{
		}

		public DecimalAlgorithmColumn(DecimalAlgorithmColumn DecimalAlgorithmColumn):base(DecimalAlgorithmColumn)
		{
			_precision=DecimalAlgorithmColumn.Precision;
			_formatstring=DecimalAlgorithmColumn.FormatString;
			_bshowwhenzero=DecimalAlgorithmColumn.bShowWhenZero;
            _pointlength = DecimalAlgorithmColumn.PointLength;
		}

		public DecimalAlgorithmColumn(GridDecimalAlgorithmColumn GridDecimalAlgorithmColumn):base(GridDecimalAlgorithmColumn)
		{
			_precision=GridDecimalAlgorithmColumn.Precision;
			_formatstring=GridDecimalAlgorithmColumn.FormatString;
            _bshowwhenzero=GridDecimalAlgorithmColumn.bShowWhenZero;
            _pointlength = GridDecimalAlgorithmColumn.PointLength;
		}

		protected DecimalAlgorithmColumn( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_precision=(PrecisionType)info.GetValue("Precision",typeof(PrecisionType));
            _pointlength = info.GetInt32("PointLength");
            if (_version >= 90)
                _bshowwhenzero = (StimulateBoolean)info.GetValue("bShowWhenZero", typeof(StimulateBoolean));
		}
		#endregion

		#region override 
		public override void SetType()
		{
			_type="DecimalAlgorithmColumn";
		}

        //public override void SetSolid()
        public override void SetDefault()
        {
            base.SetDefault();
            _captionalign = ContentAlignment.MiddleRight;
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

		[Browsable(true )]
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

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("Precision",_precision);
			
			info.AddValue("bShowWhenZero",_bshowwhenzero);
            info.AddValue("PointLength", _pointlength);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new DecimalAlgorithmColumn(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            base.Dispose();
		}

		#endregion
	}
}
