using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IMultiHeader ��ժҪ˵����
	/// </summary>
	public interface IMultiHeader:IMapName
	{
		Columns Columns{get;}
        DataSource SortSource { get;set;}
	}
}
