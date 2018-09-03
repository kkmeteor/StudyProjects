using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GridAlgorithmCalculator ��ժҪ˵����
	/// </summary>
	[Serializable]
    public class GridAlgorithmColumn : AlgorithmColumn, IGridEvent, ISerializable, ICloneable, IDisposable, IWithSizable, IMergeStyle
	{
        protected EventType _eventtype = EventType.OnTitle ;
        protected bool _bshowatreal;
        protected bool _bMergeCell = false;

		#region constructor
		public GridAlgorithmColumn():base()
		{
		}

		public GridAlgorithmColumn(int x,int y):base(x,y)
		{
		}

		public GridAlgorithmColumn(int x,int y,int w,int h):base(x,y,w,h)
		{
		}

		public GridAlgorithmColumn(GridAlgorithmColumn gridalgorithmcalculator):base(gridalgorithmcalculator)
		{
            _eventtype = gridalgorithmcalculator.EventType;
            _bshowatreal = gridalgorithmcalculator.bShowAtReal;
            _bMergeCell = gridalgorithmcalculator.bMergeCell;
		}

		public GridAlgorithmColumn(SerializationInfo info,StreamingContext context):base(info,context)
		{
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bshowatreal = info.GetBoolean("bShowAtReal");
            try
            {
                _bMergeCell = info.GetBoolean("bMergeCell");
            }
            catch
            {
                _bMergeCell = false;
            }
		}

		public GridAlgorithmColumn(Rect ds)
            : base(ds)
        {
        }

		#endregion

		#region override
		public override void SetType()
		{
			_type="GridAlgorithmColumn";
		}

        public override void SetDefault()
        {
            base.SetDefault();
            _backcolor = DefaultConfigs.DefaultTitleBackColor;
            _visibleposition = 0;
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

        [Browsable(false)]
        public override System.Drawing.Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Font ClientFont
        {
            get
            {
                return base.ClientFont;
            }
            set
            {
                base.ClientFont = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.ContentAlignment CaptionAlign
        {
            get
            {
                return base.CaptionAlign;
            }
            set
            {
                base.CaptionAlign = value;
            }
        }

		#endregion

		#region ISerializable ��Ա

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("EventType", _eventtype);
            info.AddValue("bShowAtReal", _bshowatreal);
            info.AddValue("bMergeCell", _bMergeCell);
		}

		#endregion

		#region ICloneable ��Ա

		public override object Clone()
		{
			return new GridAlgorithmColumn(this);
		}

		#endregion

		#region IDisposable ��Ա

		public override void Dispose()
		{
            base.Dispose();
		}

		#endregion


        #region IGridEvent ��Ա

        [DisplayText("U8.UAP.Services.ReportElements.GridCalculateColumn.����¼�������")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.GridCalculateColumn.����¼�������")]
        public EventType EventType
        {
            get
            {
                return _eventtype;
            }
            set
            {
                _eventtype = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.GridCalculateColumn.��ʾ�ڽ����к���")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.GridCalculateColumn.��ʾ�ڽ����к���")]
        public bool bShowAtReal
        {
            get
            {
                return _bshowatreal;
            }
            set
            {
                _bshowatreal = value;
            }
        }

        #endregion
        #region IMergeSyle ��Ա

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.Report.MergeCell")]
        [LocalizeDescription("U8.UAP.Services.Report.MergeCell")]
        public virtual bool bMergeCell
        {
            get
            {
                return _bMergeCell;
            }
            set
            {
                _bMergeCell = value;
            }
        }

        #endregion
    }
}
