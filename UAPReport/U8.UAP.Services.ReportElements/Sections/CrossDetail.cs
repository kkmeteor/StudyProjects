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
	public class CrossDetail:Section,ISerializable,ICloneable
	{
		public CrossDetail():base()
		{
		}

		public CrossDetail(int height):base(height)
		{
		}

		public CrossDetail(CrossDetail GridDetail):base(GridDetail)
		{
		}

		protected CrossDetail( SerializationInfo info, StreamingContext context ):base(info,context)
		{
		}

		protected override void SetOrderID()
		{
			_orderid=5;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.CrossDetail ;
		}

		public override void SetType()
		{
			_type="CrossDetail";
			_x=0;
		}

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx15"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            switch (ds.Type )
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return new GridDecimal(ds);
                case DataType.Image:
                    return null;
                default:
                    return new GridLabel(ds);
            }
        }

        public override States GetDefaultState(DataType type)
        {
            switch (type)
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return States.GridDecimal;
                //case DataType.Image:
                //    return States.Arrow;
                default:
                    return States.GridLabel;
            }
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
            if(	
                //type=="griddecimalalgorithmcolumn" ||
				type=="griddecimal" ||
                type == "gridcalculatecolumn" ||
                type == "gridproportiondecimal" || 
                type=="gridlabel")
				return true;
			else
				return false;
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

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new CrossDetail(this);
		}

		#endregion
	}
}
