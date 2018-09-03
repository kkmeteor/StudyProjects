using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �ṩת�����ݵ���ʱ������й���SqlCommand���ܵķ�����
	/// ����:¬����
	/// ʱ��:2007.5.18
	/// </summary>
	class SqlCommandHelper
	{
		private SqlCommandHelper() { }

		public static void ExecuteStoreProcedure(
			string storeprocedureName, 
			Hashtable paras,
			string cnnString )
		{
			SqlCommand cmd = GetSqlCommand( storeprocedureName, paras );
			ExecuteStoreProcedure( cmd, cnnString );
		}

		public static void ExecuteStoreProcedure(
			SqlCommand cmd,
			string cnnString )
		{
			try
			{
				SqlHelper.ExecuteNonQuery( cnnString, cmd );
			}
			catch( Exception e )
			{ 
				string exMsg = GetMsgWhenStoreProcedureError( cmd, e );
				throw new TempDBServiceException( e, exMsg );
			}
		}

		public static SqlCommand GetSqlCommand(
			string storeprocedureName, 
			Hashtable paras )
		{ 
			SqlCommand cmd = new SqlCommand();
			cmd.CommandText = storeprocedureName;
			cmd.CommandType = CommandType.StoredProcedure;
            if (paras != null && paras.Count > 0 )
            {
                foreach (string key in paras.Keys)
					cmd.Parameters.AddWithValue(key, paras[key] );
            }
            return cmd;
		}

		public static string GetMsgWhenStoreProcedureError( 
			SqlCommand cmd,
			Exception e )
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "ִ��SqlCommandʱ��������.(�ܿ����Ǵ洢���̻�sql��䱾����,���֤)" );
			sb.Append( "�ؼ���Ϣ:CommandType = " );
			sb.Append( cmd.CommandType.ToString() );
			sb.Append( "; CommandText = " );
			sb.AppendLine( GetCommandText( cmd, e ));
			sb.AppendLine( GetMsgParasList( cmd.Parameters ));
			return sb.ToString();
		}

		private static string GetCommandText( 
			SqlCommand cmd,
			Exception e )
		{
			string text = cmd.CommandText;
			if( text.Length > 100 )
			{
				Logger logger = Logger.GetLogger( ReportDataFacade._loggerName );
				StringBuilder sb = new StringBuilder();
				sb.Append( "����:" );
				if( e != null )
					sb.AppendLine( e.Message );
				sb.AppendLine( "������������:" );
				sb.AppendLine( text );
				logger.Info( sb.ToString());
				logger.Close();
				return text.Substring( 0, 100 ) + "...";
			}
			return text;
		}

		private static string GetMsgParasList( SqlParameterCollection paras )
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "�����б�:" );
			if( paras != null && paras.Count > 0 )
			{
				foreach( SqlParameter para in paras )
				{
					sb.Append( "��������:" );
					sb.Append( para.ParameterName );
					sb.Append( ",����ֵ:" );
					sb.AppendLine( GetParaVal( para.Value ));
				}
			}
			else
				sb.AppendLine( "None" );
			return sb.ToString();
		}

		private static string GetParaVal( object val )
		{ 
			if( val == null )
				return "null";
			return val.ToString();
		}
	}
}
