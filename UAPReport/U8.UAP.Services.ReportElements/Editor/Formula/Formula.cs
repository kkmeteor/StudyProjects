using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Formula 的摘要说明。
	/// </summary>
    //[TypeConverterAttribute(typeof(FormulaTypeConverter))]
    //[Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
    [Serializable]
	public class Formula:ISerializable
	{
		private FormulaType _type=FormulaType.Filter;
		private string _formulastring="";
        public event TypeChangingHandler TypeChanging;
		public Formula()
		{			
		}
        protected Formula(SerializationInfo info, StreamingContext context)
        {
            _type = (FormulaType )info.GetValue("Type", typeof(FormulaType));
            _formulastring = info.GetString("String");
        }

		public FormulaType Type
		{
			get
			{
				return _type;
			}
			set
			{
                if (TypeChanging != null)
                {
                    if (TypeChanging(this))
                        _type = value;
                }
                else
                {
                    _type = value;
                }
			}
		}

		public string FormulaExpression
		{
			get
			{
				return _formulastring;
			}
			set
			{
                
                _formulastring = value;
			}
		}

        #region ISerializable 成员

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", 1);
            info.AddValue("Type", _type);
            info.AddValue("String", _formulastring);
        }

        #endregion
    }
    public delegate bool TypeChangingHandler(Formula formula);
}
