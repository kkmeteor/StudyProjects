using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GridBoolean 的摘要说明。
	/// </summary>
	[Serializable]
    public class GridBoolean : DBBoolean, ISerializable, ICloneable, IDisposable, IWithSizable, IMergeStyle
    {
        protected bool _bMergeCell = false;
		#region constructor
		public GridBoolean():base()
		{
		}

		public GridBoolean(int x,int y):base(x,y)
		{
		}

		public GridBoolean(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public GridBoolean(bool bcheck):base()
		{
			_checked=bcheck;
		}

		public GridBoolean(bool bcheck,int x,int y):base(x,y)
		{
			_checked=bcheck;
		}

		public GridBoolean(bool bcheck,int x,int y,int width,int height):base(x,y,width,height)
		{
			_checked=bcheck;
		}

		public GridBoolean(GridBoolean gridboolean):base(gridboolean)
        {
            _bMergeCell = gridboolean.bMergeCell;
		}

		public GridBoolean(SerializationInfo info,StreamingContext context):base(info,context)
        {
            try
            {
                _bMergeCell = info.GetBoolean("bMergeCell");
            }
            catch
            {
                _bMergeCell = false;
            }
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="GridBoolean";
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

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
            base.GetObjectData(info, context);
            info.AddValue("bMergeCell", _bMergeCell);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new GridBoolean(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            base.Dispose();
		}

		#endregion
        #region IMergeSyle 成员

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
