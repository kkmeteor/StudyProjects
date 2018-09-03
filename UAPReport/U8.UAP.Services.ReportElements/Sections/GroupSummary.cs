using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupSummary 的摘要说明。
	/// </summary>
	[Serializable]
	public class GroupSummary:GroupSection,ISerializable,ICloneable,IAutoSequence
	{
		private GroupHeader _header;
        private bool _bautosequence = false;

		public GroupSummary():base()
		{
		}

		public GroupSummary(int level):base(level)
		{
		}

		public GroupSummary(int height,int level):base(height,level)
		{
		}

		public GroupSummary(GroupSummary groupsummary):base(groupsummary)
		{
			_header=groupsummary.Header;
            _bautosequence = groupsummary.bAutoSequence;
		}

		protected GroupSummary( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _bautosequence = info.GetBoolean("bAutoSequence");
		}

		[Browsable(false)]
		public GroupHeader Header
		{
			get
			{
				return _header;
			}
			set
			{
				_header=value;
				
			}
		}

		protected override void SetOrderID()
		{
			_orderid=6;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.GroupSummary;
		}

		public override void SetType()
		{
			_type="GroupSummary";
			_x=0;
		}

        public override Cell GetDefaultRect(DataSource ds)
        {
            switch (ds.Type )
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
                    return States.Arrow ;
            }
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(	type=="decimalalgorithmcalculator" ||
				type=="algorithmcalculator" ||
				type=="calculator" ||
				type=="chart" ||
				type=="label" ||
				type=="superlabel" ||
				type=="commonlabel" ||
				type=="expression" ||
                type == "commonexpression" ||
                type == "filterexpression" ||
				type=="image" )
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
			return new GroupSummary(this);
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


        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Sections.级分组汇总区"; }
        }

        
    }
}
