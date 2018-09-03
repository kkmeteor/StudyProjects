/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:处理符合条件的情况,获得带有权限处理后的Sql语句
 * 接口对象
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal interface IRowAuthColPicker
	{
		RowAuthContext Context { get; set; }
		bool bHit(string filed);
		string GetWhere(string filed);
	}
}
