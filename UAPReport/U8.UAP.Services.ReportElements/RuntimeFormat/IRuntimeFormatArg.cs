/*
 * 作者:卢达其
 * 时间:2008.4.7
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 管理运行时格式数据参数相关操作接口
	/// </summary>
	interface IRuntimeFormatArg
	{
		/// <summary>
		/// 数据参数对象标识
		/// </summary>
		string Id { get; }
		
		/// <summary>
		/// 数据参数对象值
		/// </summary>
		object Tag { get; }
	}
}
