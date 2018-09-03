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
	/// 提供转移数据到临时表过程中关于SqlCommand功能的服务类
	/// 作者:卢达其
	/// 时间:2007.5.18
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
			sb.AppendLine( "执行SqlCommand时发生错误.(很可能是存储过程或sql语句本身报错,请查证)" );
			sb.Append( "关键信息:CommandType = " );
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
				sb.Append( "错误:" );
				if( e != null )
					sb.AppendLine( e.Message );
				sb.AppendLine( "发生错误的语句:" );
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
			sb.AppendLine( "参数列表:" );
			if( paras != null && paras.Count > 0 )
			{
				foreach( SqlParameter para in paras )
				{
					sb.Append( "参数名称:" );
					sb.Append( para.ParameterName );
					sb.Append( ",参数值:" );
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
