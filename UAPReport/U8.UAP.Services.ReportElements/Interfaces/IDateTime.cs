using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IDateTime 的摘要说明。
	/// </summary>
	public interface IDateTime:IFormat,ISort
	{
	}

	public interface IBDateTime:IFormat
	{
		bool bDateTime{get;set;}
	}
}
