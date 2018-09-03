using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IMultiHeader 的摘要说明。
	/// </summary>
	public interface IMultiHeader:IMapName
	{
		Columns Columns{get;}
        DataSource SortSource { get;set;}
	}
}
