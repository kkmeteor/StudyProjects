/*
 * ����:¬����
 * ʱ��:2007.6.15
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
	/// �ṩ������
	/// </summary>
	class RunTimeSaveAsService
	{
		/// <summary>
		/// ���Ԫ����(�������ͼ)
		/// </summary>
		/// <param name="login">��װ��login��Ϣ</param>
		/// <param name="sourceId">����ԴId(����id����ͼId)</param>
		/// <param name="savaAsName">����������</param>
		/// <param name="saveAsType">�������</param>
		/// <returns>���ɹ�,����true;�Ѵ���Ҫ��������,����false</returns>
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
		/// ����ʱ���ɵ�Ȩ�޺�Ҫ��ӵ���¼��Ϣ�Ļ�����,
		/// �����ѯ�����ı���ʱ�����û��Ȩ�޵Ĵ���
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
		/// ����ʱ��Ҫ���Ʋ��������ͬ������
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
					String4Report.GetString( "ִ��������ʱ����ԭ���������������ԱҲ��ִ�е����������»���,���Ժ�����." )
						+ "\r\n��������:\r\n" + e.Message );
			}
			finally
			{ 
				if( cnn.State == ConnectionState.Open )
					cnn.Close();
			}
		}
	}
}
