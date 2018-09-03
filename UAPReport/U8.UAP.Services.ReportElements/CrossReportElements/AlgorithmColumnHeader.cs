using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// AlgorithmColumnHeader 的摘要说明。
	/// </summary>
	[Serializable]
	public class AlgorithmColumnHeader:Rect,IAlgorithm,IMultiHeader,IBDateTime,ISerializable,ICloneable,IDisposable,ICalculateSequence ,IWithSizable
	{
		#region fields
		protected string _algorithm="";
		protected bool _bdatetime=false;
		protected Columns _columns=new Columns();
        protected int _calculateindex;
        protected DataSource _sortsource = new DataSource("EmptyColumn");
        protected string _formatstring = "";
		#endregion

		#region constructor
		public AlgorithmColumnHeader():base()
		{
		}

		public AlgorithmColumnHeader(int x,int y):base(x,y)
		{
		}

		public AlgorithmColumnHeader(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public AlgorithmColumnHeader(string algorithm):base()
		{
			_algorithm=algorithm;
		}

		public AlgorithmColumnHeader(string algorithm,int x,int y):base(x,y)
		{
			_algorithm=algorithm;
		}

		public AlgorithmColumnHeader(string algorithm,int x,int y,int width,int height):base(x,y,width,height)
		{
			_algorithm=algorithm;
		}

		public AlgorithmColumnHeader(AlgorithmColumnHeader groupobject):base(groupobject)
		{
			_algorithm=groupobject.Algorithm;
            _sortsource = groupobject.SortSource;
            _formatstring = groupobject.FormatString;
		}

		protected AlgorithmColumnHeader( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_algorithm=info.GetString("Algorithm");
			_bdatetime=info.GetBoolean("bDateTime");
            _calculateindex = info.GetInt32("CalculateIndex");
            _sortsource = (DataSource)info.GetValue("SortSource", typeof(DataSource));
            _formatstring = info.GetString("FormatString");
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="AlgorithmColumnHeader";
		}

        public override void SetDefault()
        {
            base.SetDefault();
            _captionalign = System.Drawing.ContentAlignment.MiddleCenter;
            _visibleposition = 0;
        }

        [Browsable(false)]
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        [Browsable(false)]
        public override bool bControlAuth
        {
            get
            {
                return base.bControlAuth;
            }
            set
            {
                base.bControlAuth = value;
            }
        }

		#endregion

		#region property
		[DisplayText("U8.UAP.Services.ReportElements.Dis14")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis14")]
		[Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public string Algorithm
		{
			get
			{
				return _algorithm;
			}
			set
			{
				_algorithm=value;
			}
		}
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("Algorithm",_algorithm);
			info.AddValue("bDateTime",_bdatetime);
            info.AddValue("CalculateIndex", _calculateindex);
            info.AddValue("SortSource", _sortsource);
            info.AddValue("FormatString", _formatstring);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new AlgorithmColumnHeader(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _algorithm = null;
            _columns.Clear();
            _columns = null;
            _sortsource = null;
            base.Dispose();
		}

		#endregion

		#region IMultiHeader 成员

		[Browsable(false)]
		public Columns Columns
		{
			get
			{
				return _columns;
			}
		}

        [DisplayText("排序关键字")]
        [LocalizeDescription("排序关键字")]
        public DataSource SortSource
        {
            get
            {
                return _sortsource;
            }
            set
            {
                _sortsource = value;
            }
        }
		#endregion

		#region IMapName 成员

		[Browsable(false)]
		public string MapName
		{
			get
			{
				return _name;
			}
		}
		#endregion

		#region IBDateTime 成员

		public bool bDateTime
		{
			get
			{
				return _bdatetime;
			}
			set
			{
				_bdatetime=value;
			}
		}

		#endregion

        #region IFormat 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis22")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis22")]
        public string FormatString
        {
            get
            {
                return _formatstring;
            }
            set
            {
                _formatstring = value;
            }
        }

        #endregion

        #region ICalculateSequence 成员

        [DisplayText("U8.UAP.Services.ReportElements.Dis18")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis18")]
        public int CalculateIndex
        {
            get
            {
                return _calculateindex;
            }
            set
            {
                _calculateindex = value;
            }
        }

        #endregion
    }
}
