using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 构造CREATE TABLE语句中字段定义的服务
	/// 作者:卢达其
	/// 时间:2007.5.17
	/// </summary>
	internal interface ISqlFieldDefinition
	{
		/// <summary>
		/// 构造CREATE TABLE语句中字段定义的服务唯一方法
		/// </summary>
		/// <param name="dr">
		/// 从SqlDataReader的GetSchemaTable()获取的DataTable中
		/// 一个DataRow，即数据表的字段定义数据
		/// </param>
		/// <returns>最终的字段定义串</returns>
		string ToSqlFieldDefinition( DataRow dr );
	}
}
