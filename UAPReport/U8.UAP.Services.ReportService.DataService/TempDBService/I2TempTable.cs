using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 提供从各类数据定义形式转移数据到临时表的服务
	/// 作者:卢达其
	/// 时间:2007.5.17
	/// </summary>
	internal interface I2TempTable
	{
		/// <summary>
		/// 转移数据到临时表的操作
		/// </summary>
		void ToTempTable();
	}
}
