using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IDataSource ��ժҪ˵����
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
