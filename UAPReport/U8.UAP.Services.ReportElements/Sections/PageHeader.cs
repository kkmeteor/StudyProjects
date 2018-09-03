using System;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Detail 的摘要说明。
	/// </summary>
	[Serializable]
	public class PageHeader:Section,ISerializable,ICloneable
	{
		public PageHeader():base()
		{
		}

		public PageHeader(int height):base(height)
		{
		}

		public PageHeader(PageHeader pageheader):base(pageheader)
		{
		}

		protected PageHeader( SerializationInfo info, StreamingContext context ):base(info,context)
		{
		}

		protected override void SetOrderID()
		{
			_orderid=0;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.PageHeader;
		}

		public override void SetType()
		{
			_type="PageHeader";
			_x=0;
        }

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx19"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            return null;
        }

        public override States GetDefaultState(DataType type)
        {
            return States.Arrow;
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(type=="commonlabel" ||
				type=="expression" ||
                type == "commonexpression" ||
                type == "filterexpression" ||
				type=="image" )
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
			return new PageHeader(this);
		}

		#endregion
	}
}
