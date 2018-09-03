/*
 * 作者:卢达其
 * 时间:2008.3.18
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 升级的某个项目将数据传递给升级控制器的方法
	/// </summary>
	internal interface IUpgradeItem
	{
		/// <summary>
		/// 传递环境信息
		/// </summary>
		/// <param name="infos">环境信息集合</param>
		void DeliverEnvironmentInfos( Hashtable infos );

		/// <summary>
		/// 设置升级完成的数据
		/// </summary>
		/// <param name="urmw">
		/// 升级数据包装器，其功能是升级的数据转换成持久化对象
		/// </param>
		void SetMeta( UpgradeReportMetaWrapper urmw );
	}
}
