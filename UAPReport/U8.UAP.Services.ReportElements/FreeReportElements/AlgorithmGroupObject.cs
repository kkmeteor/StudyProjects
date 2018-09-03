using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// AlgorithmGroupObject ��ժҪ˵����
	/// </summary>
	[Serializable]
	public class AlgorithmGroupObject:Rect,IAlgorithm,IMapName,IGroup,IBDateTime,ISerializable,ICloneable,IDisposable,ICalculateSequence
	{
		#region fields
		protected string _algorithm="";
		protected bool _bdatetime=false;
        protected int _calculateindex;
        protected string _formatstring="";
		#endregion

		#region constructor
		public AlgorithmGroupObject():base()
		{
		}

		public AlgorithmGroupObject(int x,int y):base(x,y)
		{
		}

		public AlgorithmGroupObject(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public AlgorithmGroupObject(string algorithm):base()
		{
			_algorithm=algorithm;
		}

		public AlgorithmGroupObject(string algorithm,int x,int y):base(x,y)
		{
			_algorithm=algorithm;
		}

		public AlgorithmGroupObject(string algorithm,int x,int y,int width,int height):base(x,y,width,height)
		{
			_algorithm=algorithm;
		}

		public AlgorithmGroupObject(AlgorithmGroupObject groupobject):base(groupobject)
		{
			_algorithm=groupobject.Algorithm;
            _calculateindex = groupobject.CalculateIndex;
            _formatstring = groupobject.FormatString;
		}

		protected AlgorithmGroupObject( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_algorithm=info.GetString("Algorithm");
			_bdatetime=info.GetBoolean("bDateTime");
            _calculateindex = info.GetInt32("CalculateIndex");
            _formatstring = info.GetString("FormatString");
		}

		public AlgorithmGroupObject(GridAlgorithmColumn gridalgorithmcolumn):base(gridalgorithmcolumn)
		{
            _algorithm=gridalgorithmcolumn.Algorithm;
            _calculateindex = gridalgorithmcolumn.CalculateIndex;
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="AlgorithmGroupObject";
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

		#region ISerializable ��Ա

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("Algorithm",_algorithm);
			info.AddValue("bDateTime",_bdatetime);
            info.AddValue("CalculateIndex", _calculateindex);
            info.AddValue("FormatString", _formatstring);
		}

		#endregion

		#region ICloneable ��Ա

		public override object Clone()
		{
			return new AlgorithmGroupObject(this);
		}

		#endregion

		#region IDisposable ��Ա

		public override void Dispose()
		{
            _algorithm = null;
            _formatstring = null;
            base.Dispose();
		}

		#endregion

		#region IMapName ��Ա

		[Browsable(false)]
		public string MapName
		{
			get
			{
				return _name ;
			}
		}
		#endregion

		#region IBDateTime ��Ա

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

        #region IFormat ��Ա

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

        #region ICalculateSequence ��Ա

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
