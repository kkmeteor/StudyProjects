using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ISort 的摘要说明。
	/// </summary>
	public interface ISort
	{
		SortOption SortOption{get;set;}
	}

	public enum SortDirection
	{
		None,
		Ascend,
		Descend
	}
}
