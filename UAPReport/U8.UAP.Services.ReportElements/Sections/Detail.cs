using System;
using System.Collections;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Detail 的摘要说明。
	/// </summary>
	[Serializable]
    public class Detail : Section, ISerializable, ICloneable, IAutoSequence, IAlternativeStyle
	{
        protected bool _bautosequence=false;
        protected Color _backcolor2;
        protected Color _bordercolor2;
        protected Color _forecolor2;
        protected ServerFont _serverfont2 = new ServerFont();
        protected bool _bapplyalternative = false;
        protected bool _bapplysecondstyle = false;
        protected bool _balreadysetsecondstyle = false;
		public Detail():base()
		{
		}

		public Detail(int height):base(height)
		{
		}

		public Detail(Detail detail):base(detail)
		{
            _bautosequence = detail.bAutoSequence;
            _backcolor2 = detail.BackColor2;
            _bordercolor2 = detail.BorderColor2;
            _forecolor2 = detail.ForeColor2;
            //_serverfont2 = detail.ServerFont2;
            _bapplyalternative = detail.bApplyAlternative;

		}
       
		protected Detail( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _bautosequence = info.GetBoolean("bAutoSequence");
            if (_version > 1)
            {
                _backcolor2 = (Color)info.GetValue("BackColor2", typeof(Color));
                _bordercolor2 = (Color)info.GetValue("BorderColor2", typeof(Color));
                _forecolor2 = (Color)info.GetValue("ForeColor2", typeof(Color));
                _serverfont2 = (ServerFont)info.GetValue("ServerFont2", typeof(ServerFont));
                _bapplyalternative = info.GetBoolean("bApplyAlternative");
            }
		}

		protected override void SetOrderID()
		{
			_orderid=5;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.Detail;
		}

		public override void SetType()
		{
			_type="Detail";
			_x=0;
		}

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx17"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            switch (ds.Type )
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return new DBDecimal(ds);
                case DataType.DateTime:
                    return new DBDateTime(ds);
                case DataType.Image:
                    return new DBImage(ds);
                default:
                    return new DBText(ds);
            }
        }

        public override States GetDefaultState(DataType type)
        {
            switch (type)
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return States.DBDecimal;
                case DataType.DateTime:
                    return States.DBDateTime;
                case DataType.Image:
                    return States.DBImage;
                default:
                    return States.DBText;
            }
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(type=="commonlabel" ||
				type=="dbboolean" ||
				type=="dbimage" ||
				type=="decimalalgorithmcolumn" ||
				type=="algorithmcolumn" ||
				type=="dbdecimal" ||
				type=="calculatecolumn" ||
				type=="columnexpression" ||
				type=="dbdatetime" ||
				type=="dbexchangerate" ||
				type=="dbtext" ||
                type == "subreport" ||
                type == "gridproportiondecimal" ||
                type == "barcode")
				return true;
			else
				return false;
		}
        [Browsable(false)]
        public bool bAlreadySetSecondStyle
        {
            get
            {
                return _balreadysetsecondstyle ;
            }
            set
            {
                _balreadysetsecondstyle  = value;
            }
        }
        [Browsable(false)]
        public bool bApplySecondStyle
        {
            get
            {
                return _bapplysecondstyle;
            }
            set
            {
                _bapplysecondstyle = value;
            }
        }
        [Browsable(false)]
        public override int VisibleWidth
        {
            get
            {
                if (_bautosequence)
                {
                    int max = 0;
                    if (this._sectionlines.Count == 0)
                        this.AsignToSectionLines();
                    for (int i = 0; i < _sectionlines.Count; i++)
                    {
                        int tmp=this.CalcVisibleWidth(_sectionlines[i].Cells );
                        if (tmp > max)
                            max = tmp;
                    }
                    return max;                    
                }
                else
                    return base.VisibleWidth;
            }
        }

        [Browsable(false)]
        public bool bApplyAlternative
        {
            get
            {
                return _bapplyalternative;
            }
            set
            {
                _bapplyalternative = value;
            }
        }

        [Browsable(false)]
        public Color BackColor2
        {
            get
            {
                return _backcolor2;
            }
            set
            {
                _backcolor2 = value;
            }
        }

        [Browsable(false)]
        public Color BorderColor2
        {
            get
            {
                return _bordercolor2;
            }
            set
            {
                _bordercolor2 = value;
            }
        }

        [Browsable(false)]
        public Color ForeColor2
        {
            get
            {
                return _forecolor2;
            }
            set
            {
                _forecolor2 = value;
            }
        }

        [Browsable(false)]
        public ServerFont ServerFont2
        {
            get
            {
                return _serverfont2;
            }
        }
		
		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("bAutoSequence", _bautosequence);
            info.AddValue("BackColor2", _backcolor2);
            info.AddValue("BorderColor2", _bordercolor2 );
            info.AddValue("ForeColor2", _forecolor2);
            info.AddValue("ServerFont2", _serverfont2 );
            info.AddValue("bApplyAlternative", _bapplyalternative);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new Detail(this);
		}

		#endregion

        #region IAutoSequence 成员
        [DisplayText("U8.UAP.Services.Report.bAutoSequence")]
        [LocalizeDescription("U8.UAP.Services.Report.bAutoSequence")]
        public bool bAutoSequence
        {
            get
            {
                return _bautosequence;
            }
            set
            {
                _bautosequence = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class IndicatorDetail : Detail
    {
        public IndicatorDetail():base()
		{
		}

		public IndicatorDetail(int height):base(height)
		{
		}

		public IndicatorDetail(IndicatorDetail detail):base(detail)
		{
		}

        protected IndicatorDetail(SerializationInfo info, StreamingContext context)
            : base(info, context)
		{
		}

        protected override void SetSectionType()
        {
            _sectiontype = SectionType.IndicatorDetail;
        }

        public override void SetType()
        {
            _type = "IndicatorDetail";
            
        }
        public override void SetDefault()
        {
            base.SetDefault();
            _x = 0;
            _h = 400;
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            switch (ds.Type)
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return new Indicator(ds);
                default:
                    return new GroupDimension (ds);
            }
        }

        public override States GetDefaultState(DataType type)
        {
            switch (type)
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return States.Indicator;
                default:
                    return States.GroupDimension ;
            }
        }

        public override bool CanBeParent(string type)
        {
            type = type.ToLower();
            if (//type == "commonlabel" ||
                //type=="expression" ||
                //type == "commonexpression" ||
                //type == "filterexpression" ||
				//type=="image" ||
                type == "groupdimension" ||
                type == "calculategroupdimension" ||
                type == "crossdimension" ||
                type == "calculatecrossdimension" ||
                type == "indicator" ||
                type == "calculateindicator" ||
                type == "indicatormetrix" ||
                type == "gridproportiondecimalindicator" ||
                type == "chart" //||
                //type == "gauge" ||
                //type=="dbtext" ||
                //type == "calculatorindicator"
                )
                return true;
            else
                return false;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override object Clone()
        {
            return new IndicatorDetail (this);
        }
    }

}
