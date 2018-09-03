/*
 * ����:¬����
 * ʱ��:2007.8.7
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
	/// �ṩ����861����ķ���
	/// </summary>
	partial class ReportListService
	{
		/// <summary>
		/// �洢UFData��rpt_glbdef_base������bNewRpt=0�ı���id
		/// </summary>
		private static Hashtable _allUapReportIds = null;

		/// <summary>
		/// �ж�ָ��id�Ƿ���UAP�����id
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
