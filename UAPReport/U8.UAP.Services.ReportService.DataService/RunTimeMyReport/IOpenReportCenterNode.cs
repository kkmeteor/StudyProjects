/*
 * 作者:卢达其
 * 时间:2009.1.20
 */

using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 打开报表的服务接口
	/// </summary>
	public interface IOpenReportCenterNode
	{
		void Open(IReportCenterNode ircn);
	}
}
