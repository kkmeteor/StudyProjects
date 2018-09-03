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
	public class CrossColumnHeader:Section,ISerializable,ICloneable
	{
		public CrossColumnHeader():base()
		{
		}

		public CrossColumnHeader(int height):base(height)
		{
		}

		public CrossColumnHeader(CrossColumnHeader GridDetail):base(GridDetail)
		{
		}

		protected CrossColumnHeader( SerializationInfo info, StreamingContext context ):base(info,context)
		{
		}

		protected override void SetOrderID()
		{
			_orderid=4;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.CrossColumnHeader ;
		}

		public override void SetType()
		{
			_type="CrossColumnHeader";
			_x=0;
		}

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx14"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            return new ColumnHeader(ds);
        }

        public override States GetDefaultState(DataType type)
        {
            return States.ColumnHeader;
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(type=="columnheader" ||
				type=="calculatecolumnheader" ||
				type=="algorithmcolumnheader" )
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
			return new CrossColumnHeader(this);
		}

		#endregion
	}
}
