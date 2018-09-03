using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Detail 的摘要说明。
	/// </summary>
	[Serializable]
	public class ReportSummary:Section,ISerializable,ICloneable,IAutoSequence,IAddWhenDesign
	{
        private int _visiblewidth;
        private bool _bautosequence = false;
        private bool _baddwhendesign = false;
		public ReportSummary():base()
		{
		}

		public ReportSummary(int height):base(height)
		{
		}

		public ReportSummary(ReportSummary reportsummary):base(reportsummary)
		{
            _visiblewidth = reportsummary.VisibleWidth;
            _bautosequence = reportsummary.bAutoSequence;
            _baddwhendesign = reportsummary.bAddWhenDesign;
		}

		protected ReportSummary( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _visiblewidth = info.GetInt32("VisibleWidth");
            _bautosequence = info.GetBoolean("bAutoSequence");
            _baddwhendesign = info.GetBoolean("bAddWhenDesign");
		}

		protected override void SetOrderID()
		{
			_orderid=8;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.ReportSummary;
		}

		public override void SetType()
		{
			_type="ReportSummary";
			_x=0;
		}

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx23"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            switch (ds.Type)
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return new Calculator(ds);
                default:
                    return null;
            }
        }

        public override States GetDefaultState(DataType type)
        {
            switch (type)
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return States.Calculator;
                default:
                    return States.Arrow;
            }
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(type=="algorithmcalculator" ||
				type=="decimalalgorithmcalculator" ||
				type=="calculator" ||
				type=="chart" ||
                //type=="label" ||
                //type=="superlabel" ||
				type=="commonlabel" ||
				type=="expression" ||
                type == "commonexpression" ||
                type == "filterexpression" ||
                type == "image"  )
				return true;
			else
				return false;
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
                        int tmp = this.CalcVisibleWidth(_sectionlines[i].Cells);
                        if (tmp > max)
                            max = tmp;
                    }
                    return max;           
                }
                return _visiblewidth ;
            }
        }

        //public void InitVisibleWidth(int width)
        //{
        //    _visiblewidth = width;
        //}

        public void InitVisibleWidth()
        {
            _visiblewidth = _cells.Right ;
        }

        public void SetVisibleWidth(int width)
        {
            if (_visiblewidth < width)
                _visiblewidth = width;
        }
		
		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("VisibleWidth", _visiblewidth);
            info.AddValue("bAutoSequence", _bautosequence);
            info.AddValue("bAddWhenDesign", _baddwhendesign);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new ReportSummary(this);
		}

		#endregion
	
        #region IAutoSequence 成员
        [Browsable(false)]
        public bool  bAutoSequence
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

        #region IAddWhenDesign 成员

        [Browsable(false)]
        public bool bAddWhenDesign
        {
            get
            {
                return _baddwhendesign;
            }
            set
            {
                _baddwhendesign = value;
            }
        }

        #endregion
    }
}
