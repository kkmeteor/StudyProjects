/*
 * 作者:卢达其
 * 时间:2007.8.7
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 提供过滤861报表的服务
	/// </summary>
	partial class ReportListService
	{
		/// <summary>
		/// 存储UFData库rpt_glbdef_base中所有bNewRpt=0的报表id
		/// </summary>
		private static Hashtable _allUapReportIds = null;

		/// <summary>
		/// 判断指定id是否是UAP报表的id
		/// </summary>
		public bool IsUapReport( string reportId )
		{ 
			this.InitAllUapReportIds();
			return !_allUapReportIds.ContainsKey( reportId.ToUpper());
		}

		private void InitAllUapReportIds()
		{ 
			_allUapReportIds = new Hashtable();
			string sql = @"SELECT 
				DISTINCT SystemID + N'[__]'+ Name AS ReportId 
				FROM rpt_glbdef_base 
				WHERE isnull(bNewRpt,0)=0";
			DataSet ds = SqlHelper.ExecuteDataSet( this._login.UfDataCnnString, sql );
			for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = SqlHelper.GetDataRowFrom(i, ds);
				if( dr != null )
				{
					string id = SqlHelper.GetStringFrom( dr["ReportId"] );
					_allUapReportIds.Add( id.ToUpper(), id );
				}
            }
		}
	}
}
