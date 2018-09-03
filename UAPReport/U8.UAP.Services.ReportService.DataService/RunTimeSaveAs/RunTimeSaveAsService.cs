/*
 * 作者:卢达其
 * 时间:2007.6.15
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 提供另存服务
	/// </summary>
	class RunTimeSaveAsService
	{
		/// <summary>
		/// 另存元数据(报表或视图)
		/// </summary>
		/// <param name="login">包装的login信息</param>
		/// <param name="sourceId">另存的源Id(报表id或视图Id)</param>
		/// <param name="savaAsName">另存的新名称</param>
		/// <param name="saveAsType">另存类型</param>
		/// <returns>另存成功,返回true;已存在要另存的名称,返回false</returns>
		public bool SaveAs(
			U8LoginInfor login, 
			string sourceId,
			string savaAsName,
			string reportSubId, 
			SaveAsType saveAsType,
            string runtimeForamtXml,
            string colorstyleid,
            string currentViewId)
		{ 
			SqlCommand cmd = new SqlCommand( "UAP_Report_RuntimeSaveAs" );
			cmd.CommandType	= CommandType.StoredProcedure;
			cmd.Parameters.Add( SqlHelper.GetParameter( "@SourceId", SqlDbType.NVarChar, 100, sourceId ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@SaveAsName", SqlDbType.NVarChar, 256, savaAsName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@SaveAsType", SqlDbType.NVarChar, 100, saveAsType.ToString() ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId", SqlDbType.NVarChar, 100, login.cAccId ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear", SqlDbType.NVarChar, 100, login.cYear ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cUserId", SqlDbType.NVarChar, 100, login.UserID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@IsExistedName", SqlDbType.Bit ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportSubId", SqlDbType.NVarChar, 100, reportSubId ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@NewAuthIds", SqlDbType.NVarChar, 2000 ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@RuntimeFormat",SqlDbType.NText,runtimeForamtXml));
            cmd.Parameters.Add( SqlHelper.GetParameter( "@colorstyleid",SqlDbType.NVarChar,100,colorstyleid));
            cmd.Parameters.Add(SqlHelper.GetParameter ( "@CurrentViewId", SqlDbType.NVarChar, 100, currentViewId));
			
			bool isNameExisted = false;
			this.ExcuteStoreProc( login.UfMetaCnnString, cmd, ref isNameExisted );
			if( isNameExisted )
				return false;
			if( saveAsType == SaveAsType.SaveAsReport )
				this.AddAuth2Cache( cmd, login );
			return true;
		}

		/// <summary>
		/// 运行时生成的权限号要添加到登录信息的缓存中,
		/// 否则查询新另存的报表时会出现没有权限的错误
		/// </summary>
		private void AddAuth2Cache( SqlCommand cmd, U8LoginInfor login )
		{ 
			string authIds = cmd.Parameters["@NewAuthIds"].Value.ToString();
			if( !string.IsNullOrEmpty( authIds ) )
			{
				string[] ids = authIds.Split( ';' );
				foreach( string id in ids )
					if( ! string.IsNullOrEmpty( id ))
						login.AddAuth2Cache( id );
			}
		}

		/// <summary>
		/// 并发时需要控制不能另存相同的名称
		/// </summary>
		private void ExcuteStoreProc(
			string ufMetaCnnString,
			SqlCommand cmd,
			ref bool isNameExisted )
		{ 
			SqlConnection cnn = new SqlConnection( ufMetaCnnString );
			cnn.Open();
			SqlTransaction sqlTransaction = cnn.BeginTransaction();
			try
			{
				SqlHelper.ExecuteNonQuery( sqlTransaction, cmd );
				if( Convert.ToBoolean( cmd.Parameters["@IsExistedName"].Value ) )
				{
					sqlTransaction.Rollback();
					isNameExisted = true;
				}
				else
					sqlTransaction.Commit();
			}
			catch( Exception e )
			{
				sqlTransaction.Rollback();
				throw new Exception( 
					String4Report.GetString( "执行另存操作时出错，原因可能是其他操作员也在执行的另存操作导致互斥,请稍后重试." )
						+ "\r\n错误描述:\r\n" + e.Message );
			}
			finally
			{ 
				if( cnn.State == ConnectionState.Open )
					cnn.Close();
			}
		}
	}
}
