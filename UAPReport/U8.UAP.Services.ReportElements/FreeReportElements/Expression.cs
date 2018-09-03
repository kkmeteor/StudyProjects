using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Expression 的摘要说明。
	/// </summary>
	[Serializable]
	public class Expression:Rect,IExpression,ISerializable,ICloneable,IDisposable
	{
		#region fields
		protected Formula _formula=new Formula();
        private PrecisionType _precision = PrecisionType.None;
        private bool _bdate = false;
        private string _formatstring = "";
		#endregion

		#region constructor
		public Expression():base()
		{
		}

		public Expression(int x,int y):base(x,y)
		{
		}

		public Expression(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public Expression(Expression expression):base(expression)
		{
			_formula=expression.Formula;
            _precision = expression.Precision;
            _bdate = expression.bDate;
            _formatstring = expression.FormatString;
		}

		public Expression(SerializationInfo info,StreamingContext context):base(info,context)
		{
			_formula=(Formula)info.GetValue("Formula",typeof(Formula));
            if (_version > 1)
            {
                _precision = (PrecisionType)info.GetValue("Precision", typeof(PrecisionType));
                _bdate = info.GetBoolean("bDateTime");
                _formatstring = info.GetString("FormatString");
            }
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="Expression";
		}
        public override void SetDefault()
        {
            base.SetDefault();
            _borderside.NoneBorder();
        }
        public override string GetStateType()
        {
            switch (_formula.Type)
            {
                case FormulaType.Common:
                    return "CommonExpression";
                case FormulaType.Filter:
                    return "FilterExpression";
                default: //FormulaType.Print :
                    return "PrintExpression";
            }
        }

        [Browsable(false)]
        public override bool bSupportLocate
        {
            get
            {
                return base.bSupportLocate;
            }
            set
            {
                base.bSupportLocate = value;
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
                base.bControlAuth = false;
            }
        }

        [Browsable(false)]
        public override string PrepaintEvent
        {
            get
            {
                return base.PrepaintEvent;
            }
            set
            {
                base.PrepaintEvent = value;
            }
        }
        
        [Browsable(false)]
        public override bool bHidden
        {
            get
            {
                return base.bHidden;
            }
            set
            {
                base.bHidden = value;
            }
        }
		#endregion

		#region IExpression 成员
        [Browsable(false)]
        public bool bGroupRelate
        {
            get
            {
                string expression = _formula.FormulaExpression.ToLower();
                return expression.StartsWith("getdata(\"")
                    || expression.StartsWith("groupsum(\"")
                    || expression.StartsWith("groupaccsum(\"")
                    || expression.StartsWith("grouppageaccsum(\"");
            }
        }

        [Browsable(false)]
        public bool bGetValueFromTask
        {
            get
            {
                string expression = _formula.FormulaExpression.ToLower();
                return expression.StartsWith("getdata(\"")
                    || expression.StartsWith("groupsum(\"")
                    || expression.StartsWith("groupaccsum(\"");
            }
        }

        [Browsable(false)]
        public bool bGetValueFromSemiRow
        {
            get
            {
                string expression = _formula.FormulaExpression.ToLower();
                return expression.StartsWith("pagesum(\"")
                    || expression.StartsWith("pageaccsum(\"")
                    || expression.StartsWith("grouppageaccsum(\"");
            }
        }

        public string GetExpressionKey()
        {
            int index1 = _formula.FormulaExpression.IndexOf("\"");
            int index2 = _formula.FormulaExpression.LastIndexOf("\"");
            return _formula.FormulaExpression.Substring(index1 + 1, index2 - index1 - 1).Trim();
        }

		[Browsable(false)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis25")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis25")]
		public Formula Formula
		{
			get
			{
				return _formula;
			}
			set
			{
				_formula=value;
			}
		}

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

        [Browsable(true)]
        [DisplayText("U8.Report.bDateTime")]
        [LocalizeDescription("U8.Report.bDateTime")]
        public bool bDate
        {
            get
            {
                return _bdate;
            }
            set
            {
                _bdate = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis20")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis20")]
        public PrecisionType Precision
        {
            get
            {
                return _precision;
            }
            set
            {
                _precision = value;
            }
        }

		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("Formula",_formula);
            info.AddValue("bDateTime", _bdate);
            info.AddValue("FormatString", _formatstring);
            info.AddValue("Precision", _precision);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new Expression(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _formula = null;
            _formatstring = null;
            base.Dispose();
		}

		#endregion
    }
}
