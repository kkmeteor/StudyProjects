using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// AlgorithmColumn 的摘要说明。
	/// </summary>
	[Serializable]
	public class AlgorithmColumn:Rect,IAlgorithm,IMapName,IBDateTime,ISerializable,ICloneable,IDisposable,ICalculateSequence
	{
		#region fields
		protected string _algorithm="";
		protected bool _bdatetime=false;
        protected int _calculateindex;
        protected string _formatstring = "";
		#endregion

		#region constructor
		public AlgorithmColumn():base()
		{
		}

		public AlgorithmColumn( Rect ds )
            : base( ds )
        {
        }

		public AlgorithmColumn(int x,int y):base(x,y)
		{
		}

		public AlgorithmColumn(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public AlgorithmColumn(string algorithm):base()
		{
			_algorithm=algorithm;
		}

		public AlgorithmColumn(string algorithm,int x,int y):base(x,y)
		{
			_algorithm=algorithm;
		}

		public AlgorithmColumn(string algorithm,int x,int y,int width,int height):base(x,y,width,height)
		{
			_algorithm=algorithm;
		}

		public AlgorithmColumn(AlgorithmColumn groupobject):base(groupobject)
		{
			_algorithm=groupobject.Algorithm;
            _calculateindex = groupobject.CalculateIndex;
            _formatstring = groupobject.FormatString;
        }

        public AlgorithmColumn(GridAlgorithmColumn groupobject): base(groupobject)
        {
            _algorithm = groupobject.Algorithm;
            _calculateindex = groupobject.CalculateIndex;
        }

		protected AlgorithmColumn( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_algorithm=info.GetString("Algorithm");
			_bdatetime=info.GetBoolean("bDateTime");
            _calculateindex = info.GetInt32("CalculateIndex");
            _formatstring = info.GetString("FormatString");
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="AlgorithmColumn";
		}

		#endregion

		#region property
		[Browsable(true)]
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
            info.AddValue("FormatString", _formatstring);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new AlgorithmColumn(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _algorithm = null;
            _formatstring = null;
            base.Dispose();
		}

		#endregion

		#region IMapName 成员

		[Browsable(false)]
		public string MapName
		{
			get
			{
				return _name ;
			}
		}
		#endregion

		#region IBDateTime 成员
        [DisplayText("U8.Report.bDateTime")]
        [LocalizeDescription("U8.Report.bDateTime")]
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
