using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IExchangeRate 的摘要说明。
	/// </summary>
	public interface IExchangeRate:IFormat
	{
		Double Value{get;set;}
		string ExchangeCode{get;set;}
	}
}
