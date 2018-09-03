using System;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Detail 的摘要说明。
	/// </summary>
	[Serializable]
    public class CrossRowHeader : Section, ISerializable, ICloneable, IAutoDesign
	{
		public CrossRowHeader():base()
		{
		}

		public CrossRowHeader(int height):base(height)
		{
		}

		public CrossRowHeader(CrossRowHeader GridDetail):base(GridDetail)
		{
		}

		protected CrossRowHeader( SerializationInfo info, StreamingContext context ):base(info,context)
		{
		}

		protected override void SetOrderID()
		{
			_orderid=3;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.CrossRowHeader ;
		}

		public override void SetType()
		{
			_type="CrossRowHeader";
			_x=0;
		}

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx16"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
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

        public override States GetDefaultState(DataType type)
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

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(type=="gridlabel" ||
				type=="gridboolean" ||
				type=="gridimage" ||
                type == "griddecimalalgorithmcolumn" ||
                type == "gridalgorithmcolumn" ||
				type=="griddecimal" ||
				type=="gridcalculatecolumn" ||
				type=="gridcolumnexpression" ||
				type=="griddatetime" ||
                type=="gridproportiondecimal" ||
				type=="gridexchangerate" )
				return true;
			else
				return false;
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
			return new CrossRowHeader(this);
		}

		#endregion

        #region IAutoDesign 成员

        public void AutoDesign()
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesign();
        }

        public void AutoDesign(int x)
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesign(x);
        }

        public void AutoDesign(Cells cs)
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesign(cs);
        }

        public void AutoDesign(int y, int height)
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesign(y, height);
        }

        public void AutoDesignSuperLabel()
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesignSuperLabel();
        }

        #endregion
    }
}
