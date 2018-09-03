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
	public class PageTitle:Section,ISerializable,ICloneable,IAutoSequence 
	{
        private bool _bautosequence = false;
		public PageTitle():base()
		{
		}

		public PageTitle(int height):base(height)
		{
		}

		public PageTitle(PageTitle pagetitle):base(pagetitle)
		{
            _bautosequence = pagetitle.bAutoSequence;
		}

		protected PageTitle( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _bautosequence = info.GetBoolean("bAutoSequence");
		}

		protected override void SetOrderID()
		{
			_orderid=3;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.PageTitle;
		}

		public override void SetType()
		{
			_type="PageTitle";
			_x=0;
		}

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx21"; }
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
			if(	type=="label" ||
				type=="superlabel") //||
                //type=="commonlabel" ||
                //type=="expression" ||
                //type=="image" )
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
			return new PageTitle(this);
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
}
