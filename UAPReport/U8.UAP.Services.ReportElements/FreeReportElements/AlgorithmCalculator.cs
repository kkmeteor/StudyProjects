using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// AlgorithmCalculator 的摘要说明。
	/// </summary>
	[Serializable]
	public class AlgorithmCalculator:Rect,IAlgorithm,IBDateTime,ICalculateSequence,ISerializable,ICloneable,IDisposable
	{
		#region fields
		protected string _algorithm="";
		protected bool _bdatetime=false;
		protected int _calculateindex;
        protected string _formatstring = "";
		#endregion

		#region constructor
		public AlgorithmCalculator():base()
		{
		}

		public AlgorithmCalculator(int x,int y):base(x,y)
		{
		}

		public AlgorithmCalculator(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public AlgorithmCalculator(string algorithm):base()
		{
			_algorithm=algorithm;
		}

		public AlgorithmCalculator(string algorithm,int x,int y):base(x,y)
		{
			_algorithm=algorithm;
		}

		public AlgorithmCalculator(string algorithm,int x,int y,int width,int height):base(x,y,width,height)
		{
			_algorithm=algorithm;
		}

		public AlgorithmCalculator(AlgorithmCalculator algorithmcalculator):base(algorithmcalculator)
		{
			_algorithm=algorithmcalculator.Algorithm;
			_calculateindex=algorithmcalculator.CalculateIndex;
            _formatstring = algorithmcalculator.FormatString;
		}

		protected AlgorithmCalculator( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_algorithm=info.GetString("Algorithm");
			_bdatetime=info.GetBoolean("bDateTime");
			_calculateindex=info.GetInt32("CalculateIndex");
            _formatstring = info.GetString("FormatString");
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="AlgorithmCalculator";
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
			info.AddValue("CalculateIndex",_calculateindex);
            info.AddValue("FormatString", _formatstring);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new AlgorithmCalculator(this);
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

		#region IBDateTime 成员

        [Browsable(false)]
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

		[Browsable(true)]
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
				_calculateindex=value;
			}
		}

		#endregion
	}
}
