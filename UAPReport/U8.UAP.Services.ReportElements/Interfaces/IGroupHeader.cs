using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IGroupHeader ��ժҪ˵����
	/// </summary>
	public interface IGroupHeader
	{
		bool bShowNullGroup{get;set;}
		bool bHiddenGroup{get;set;}
        bool bAloneLine { get;set;}
	}
}
