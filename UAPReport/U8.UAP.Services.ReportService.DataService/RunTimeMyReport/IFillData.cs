/*
 * 作者:卢达其
 * 时间:2009.3.9
 *
 */

using System;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 数据填充
	/// </summary>
	public interface IFillData
	{
		void FillData(DataRow dr);
	}
}