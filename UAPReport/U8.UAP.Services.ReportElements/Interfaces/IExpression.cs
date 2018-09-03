using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IExpression 的摘要说明。
	/// </summary>
	public interface IExpression
	{
		Formula Formula{get;set;}
        PrecisionType Precision { get;set;}
        string FormatString { get;set;}
        bool bDate { get;set;}
	}

	public enum FormulaType
	{
		Common=0,
		Filter=1,
		//Business,
		UserDefine=3,
        Print=4
	}
}
