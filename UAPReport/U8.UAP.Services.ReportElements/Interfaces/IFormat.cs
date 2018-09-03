using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IFormatString 的摘要说明。
	/// </summary>
	public interface IFormat
	{
		string FormatString{get;set;}
	}
    public interface IMergeStyle
    {
        bool bMergeCell { get; set; }
    }
}
