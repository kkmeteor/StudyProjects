using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IDataSource 的摘要说明。
	/// </summary>
	public interface IDataSource
	{
		DataSource DataSource{get;set;}
	}

	public enum DataType
	{
		String,
		Int,
		Decimal,
		Currency,
		Image,
		Boolean,
		DateTime,
        Text
	}
}
