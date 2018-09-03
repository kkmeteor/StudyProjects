using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 获取级次展开信息服务的接口
	/// </summary>
	public interface ILevelExpandTempDBGetDataService
	{
		/// <summary>
		/// 此方法往DataRow参数中填入级次展开信息
		/// </summary>
		/// <param name="columnInfo">
		/// 与传递给ReportDataFacade.LevelExpandInfo2TempDB()
		/// 的columnInfo属于同一对象
		/// </param>
		/// <param name="reader">
		/// 源表数据信息,接口的实现者将使用reader当前行信息
		/// 计算得出级次展开的信息
		/// </param>
		/// <param name="dr">使用DataRow返回出级次展开的信息</param>
		void GetData( 
			Hashtable columnInfo, 
			SqlDataReader reader,
			DataRow dr );
	}
}
