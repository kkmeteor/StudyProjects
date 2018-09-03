using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupHeader 的摘要说明。
	/// </summary>
	[Serializable]
	public class GroupHeader:GroupSection,IGroupHeader,ISerializable,ICloneable,IAutoSequence
	{
		private bool _bshownullgroup=true;
		private bool _bhiddengroup;
		private GroupSummary _summary;
		private int _groupstartrow;
        private bool _bautosequence = false;
        private bool _baloneline = false;
        private SequenceCells _tmpcells;

        public GroupHeader():base()
		{
		}

		public GroupHeader(int level):base(level)
		{
		}

		public GroupHeader(int height,int level):base(height,level)
		{
		}

		public GroupHeader(GroupHeader groupheader):base(groupheader)
		{
			_bshownullgroup=groupheader.bShowNullGroup;
			_bhiddengroup=groupheader.bHiddenGroup;
			_summary=groupheader.Summary;
            _bautosequence = groupheader.bAutoSequence;
            _baloneline = groupheader.bAloneLine;
		}

		protected GroupHeader( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_bshownullgroup=info.GetBoolean("bShowNullGroup");
			_bhiddengroup=info.GetBoolean("bHiddenGroup");
            //_summary=(GroupSummary)info.GetValue("Summary",typeof(GroupSummary));
            _bautosequence = info.GetBoolean("bAutoSequence");
            if(_version>=90)
                _baloneline = info.GetBoolean("bAloneLine");
		}

        [Browsable(false)]
        public SequenceCells TmpCells
        {
            get
            {
                return _tmpcells;
            }
            set
            {
                _tmpcells = value;
            }
        }

        [DisplayText("U8.UAP.Report.bAloneLine")]
        [LocalizeDescription("U8.UAP.Report.bAloneLine")]
        public bool bAloneLine
        {
            get
            {
                return _baloneline;
            }
            set
            {
                _baloneline = value;
            }
        }

		[DisplayText("U8.UAP.Services.ReportElements.Dis42")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis42")]
		public bool bShowNullGroup
		{
			get
			{
				return _bshownullgroup;
			}
			set
			{
				_bshownullgroup=value;
			}
		}

        [Browsable(false)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis43")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis43")]
		public bool bHiddenGroup
		{
			get
			{
				return _bhiddengroup;
			}
			set
			{
				_bhiddengroup=value;
			}
		}

		[Browsable(false)]
		public int GroupStartRow
		{
			get
			{
				return _groupstartrow;
			}
			set
			{
				_groupstartrow=value;
			}
		}

		[Browsable(false)]
		public GroupSummary Summary
		{
			get
			{
				return _summary;
			}
			set
			{
				_summary=value;
				
			}
		}

		#region override
		protected override void SetOrderID()
		{
			_orderid=4;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.GroupHeader;
		}

		public override void SetType()
		{
			_type="GroupHeader";
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
                case DataType.Image:
                    return null;
                default:
                    return new GroupObject(ds);
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
                case DataType.Image:
                    return States.Arrow ;
                default:
                    return States.GroupObject;
            }
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(type=="commonlabel" ||
				type=="dbboolean" ||
				type=="dbimage" ||
				type=="decimalalgorithmcalculator" ||
				type=="dbdecimal" ||
				type=="calculatecolumn" ||
				type=="columnexpression" ||
				type=="algorithmcolumn" ||
				type=="decimalalgorithmcolumn" ||
				type=="dbdatetime" ||
				type=="dbexchangerate" ||
				type=="dbtext" ||
				type=="algorithmcalculator" ||
				type=="calculator" ||
                //type=="chart" ||
				type=="label" ||
				type=="superlabel" ||
				type=="commonlabel" ||
				type=="expression" ||
                type == "commonexpression" ||
                type == "filterexpression" ||
				type=="image" ||
				type=="groupobject" ||
				type=="calculategroupobject" ||
                type == "algorithmgroupobject" ||
                type == "barcode")
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
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("bShowNullGroup",_bshownullgroup);
			info.AddValue("bHiddenGroup",_bhiddengroup);
			info.AddValue("Summary",_summary);
            info.AddValue("bAutoSequence", _bautosequence);
            info.AddValue("bAloneLine", _baloneline);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new GroupHeader(this);
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
            get { return "U8.UAP.Services.ReportElements.Sections.级分组区"; }
        }

        
    }
}
