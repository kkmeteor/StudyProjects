using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IExchangeRate ��ժҪ˵����
	/// </summary>
	public interface IExchangeRate:IFormat
	{
		Double Value{get;set;}
		string ExchangeCode{get;set;}
	}
}
