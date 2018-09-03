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
	public class ReportHeader:Section,ISerializable,ICloneable,IAutoSequence
	{
        private bool _bautosequence = false;
		public ReportHeader():base()
		{
		}

		public ReportHeader(int height):base(height)
		{
		}

		public ReportHeader(ReportHeader reportheader):base(reportheader)
		{
            _bautosequence = reportheader.bAutoSequence;
		}

		protected ReportHeader( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _bautosequence = info.GetBoolean("bAutoSequence");
		}

		protected override void SetOrderID()
		{
			_orderid=1;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.ReportHeader;
		}

		public override void SetType()
		{
			_type="ReportHeader";
			_x=0;
		}

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx22"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            return null;
        }

        public override States GetDefaultState(DataType type)
        {
            return States.Arrow;
        }

        public Cell GetDefaultRect(DataSource ds,bool bgrid)
        {
            if (bgrid)
            {
                switch (ds.Type )
                {
                    case DataType.Currency:
                    case DataType.Decimal:
                    case DataType.Int:
                        return new GridDecimal(ds);
                    case DataType.DateTime:
                        return new GridDateTime(ds);
                    //case DataType.Image:
                    //    return new GridImage(ds);
                    default:
                        return new GridLabel(ds);
                }
            }
            else
            {
                switch (ds.Type )
                {
                    case DataType.Currency:
                    case DataType.Decimal:
                    case DataType.Int:
                        return new DBDecimal();
                    case DataType.DateTime:
                        return new DBDateTime();
                    case DataType.Image:
                        return new DBImage();
                    default:
                        return new DBText();
                }
            }
        }

        public States GetDefaultState(DataType type,bool bgrid)
        {
            if (bgrid)
            {
                switch (type)
                {
                    case DataType.Currency:
                    case DataType.Decimal:
                    case DataType.Int:
                        return States.GridDecimal;
                    case DataType.DateTime:
                        return States.GridDateTime;
                    //case DataType.Image:
                    //    return States.GridImage;
                    default:
                        return States.GridLabel;
                }
            }
            else
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
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(type=="label" ||
                //type=="superlabel" ||
				type=="commonlabel" ||
				type=="expression" ||
                type == "commonexpression" ||
                type == "filterexpression" ||
				type=="image" ||
				type=="dbimage" ||
				type=="dbdecimal" ||
				type=="calculatecolumn" ||
				type=="columnexpression" ||
				type=="dbdatetime" ||
				type=="dbtext" ||
                type == "gridimage" ||
                type == "griddecimal" ||
                type == "gridcalculatecolumn" ||
                type == "gridcolumnexpression" ||
                type == "griddatetime" ||
                type == "gridlabel")
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
                    if (max != 0)
                        return max;
                    else
                        return base.VisibleWidth;
                }
                else
                    return base.VisibleWidth;
            }
        }
		
		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("bAutoSequence", _bautosequence);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new ReportHeader(this);
		}

		#endregion

        #region IAutoSequence 成员
        [Browsable(false)]
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
}
