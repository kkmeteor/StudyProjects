using System;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Detail 的摘要说明。
	/// </summary>
	[Serializable]
	public class PrintPageSummary:Section,ISerializable,ICloneable
	{
		public PrintPageSummary():base()
		{
		}

		public PrintPageSummary(int height):base(height)
		{
		}

		public PrintPageSummary(PrintPageSummary pagesummary):base(pagesummary)
		{
		}

        protected PrintPageSummary(SerializationInfo info, StreamingContext context)
            : base(info, context)
		{
		}

		protected override void SetOrderID()
		{
			_orderid=7;
		}

		protected override void SetSectionType()
		{
            _sectiontype = SectionType.PrintPageSummary;
		}

		public override void SetType()
		{
            _type = "PrintPageSummary";
			_x=0;
		}

        protected override string CaptionID
        {
            get { return "U8.Report.PrintPageSummary"; }
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
			if(	type=="commonlabel" ||
				type=="expression" ||
                type == "commonexpression" ||
                type == "filterexpression" ||
                type == "printexpression" ||
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
            return new PrintPageSummary(this);
		}

		#endregion
	}

    [Serializable]
    public class PrintPageTitle : Section, ISerializable, ICloneable
    {
        public PrintPageTitle()
            : base()
        {
        }

        public PrintPageTitle(int height)
            : base(height)
        {
        }

        public PrintPageTitle(PrintPageTitle pagesummary)
            : base(pagesummary)
        {
        }

        protected PrintPageTitle(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected override void SetOrderID()
        {
            _orderid = 2;
        }

        protected override void SetSectionType()
        {
            _sectiontype = SectionType.PrintPageTitle ;
        }

        public override void SetType()
        {
            _type = "PrintPageTitle";
            _x = 0;
        }

        protected override string CaptionID
        {
            get { return "U8.Report.PrintPageTitle"; }
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
            type = type.ToLower();
            if (type == "commonlabel" ||
                type == "expression" ||
                type == "commonexpression" ||
                type == "filterexpression" ||
                type == "printexpression" ||
                type == "image")
                return true;
            else
                return false;
        }

        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new PrintPageTitle(this);
        }

        #endregion
    }
}
