using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class SqlHelper
	{
		private SqlHelper(){}

		#region ExecuteNonQuery
        public static void ExecuteNoneQueryNoneTransaction(string CnnString, string sql)
        {
            SqlConnection cnn = new SqlConnection(CnnString);
            cnn.Open();
            SqlCommand cmd = new SqlCommand(sql);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 7200;
            try
            {
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                WriteLog(e.Message,sql);
                throw e;
            }
            finally
            {
                if (cnn.State == ConnectionState.Open)
                    cnn.Close();
            }
        }

		public static void ExecuteNonQuery( string CnnString, string sql )
		{ 
			SqlConnection cnn = new SqlConnection( CnnString );
			cnn.Open();
			SqlTransaction sqlTransaction = cnn.BeginTransaction();
			SqlCommand cmd	= new SqlCommand( sql );
			cmd.CommandType	= CommandType.Text;
            cmd.CommandTimeout = 7200;
			try
			{
				cmd.Connection	= cnn;
				cmd.Transaction	= sqlTransaction;
				cmd.ExecuteNonQuery();
				sqlTransaction.Commit();
			}
			catch( Exception e )
			{
				sqlTransaction.Rollback();
                WriteLog(e.Message, sql);
				throw e;
			}
			finally
			{
				if( cnn.State == ConnectionState.Open )
					cnn.Close();
			}
		}
        public static void ExecuteNonQuery(string CnnString, SqlCommand cmd,IsolationLevel tranLevel)
        {
            SqlConnection cnn = new SqlConnection(CnnString);
            cnn.Open();
            SqlTransaction sqlTransaction = cnn.BeginTransaction(tranLevel);            
            try
            {
                cmd.Connection = cnn;
                cmd.CommandTimeout = 7200;
                cmd.Transaction = sqlTransaction;
                cmd.ExecuteNonQuery();
                sqlTransaction.Commit();
            }
            catch (Exception e)
            {
                sqlTransaction.Rollback();
                WriteLog(e.Message, cmd.CommandText);
                throw e;
            }
            finally
            {
                if (cnn.State == ConnectionState.Open)
                    cnn.Close();
            }
        }
		public static void ExecuteNonQuery( string CnnString, SqlCommand cmd )
		{
			SqlConnection cnn = new SqlConnection( CnnString );
			cnn.Open();
			SqlTransaction sqlTransaction = cnn.BeginTransaction();

			try
			{
				cmd.Connection	= cnn;
                cmd.CommandTimeout = 7200;
				cmd.Transaction	= sqlTransaction;
				cmd.ExecuteNonQuery();
				sqlTransaction.Commit();
			}
			catch( Exception e )
			{
				sqlTransaction.Rollback();
                WriteLog(e.Message, cmd.CommandText);
				throw e;
			}
			finally
			{
				if( cnn.State == ConnectionState.Open )
					cnn.Close();
			}
		}

		public static void ExecuteNonQuery(
			SqlTransaction sqlTransaction,
			SqlCommand cmd )
		{
            try
            {
                cmd.Connection = sqlTransaction.Connection;
                cmd.Transaction = sqlTransaction;
                cmd.CommandTimeout = 7200;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                WriteLog(e.Message, cmd.CommandText);
                throw e;
            }
		}

		public static void ExecuteNonQuery( SqlTransaction sqlTransaction, string sql )
		{
            try
            {
                SqlCommand cmd = new SqlCommand(sql, sqlTransaction.Connection);
                cmd.Transaction = sqlTransaction;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 7200;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                WriteLog(e.Message, sql);
                throw e;
            }
		}

		#endregion

		#region ExecuteDataSet

        public static DataSet ExecuteDataSet(SqlTransaction sqlTransaction, string sql)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sql, sqlTransaction.Connection);
                cmd.Transaction = sqlTransaction;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 7200;
                return SqlHelper.ExecuteDataSet(sqlTransaction, cmd);
            }
            catch (Exception e)
            {
                WriteLog(e.Message, sql);
                throw e;
            }
        }

		public static DataSet ExecuteDataSet(
			SqlTransaction sqlTransaction, 
			SqlCommand cmd )
        {
            try
            {
                cmd.Transaction = sqlTransaction;
                cmd.Connection = sqlTransaction.Connection;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message, cmd.CommandText);
                throw e;
            }
        }

		public static DataSet ExecuteDataSet(SqlConnection cnn, string sql)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sql, cnn);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 7200;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message, sql);
                throw e;
            }
        }

		public static DataSet ExecuteDataSet( string CnnString, string sql )
		{
			SqlConnection cnn = new SqlConnection( CnnString );
			cnn.Open();
			try
			{
				return SqlHelper.ExecuteDataSet(cnn, sql);
			}
			catch( Exception e )
			{
                WriteLog(e.Message, sql);
				throw e;
			}
			finally
			{
				if( cnn.State == ConnectionState.Open )
					cnn.Close();
			}
		}

		public static DataSet ExecuteDataSet( string CnnString, SqlCommand cmd )
		{
			SqlConnection cnn = new SqlConnection( CnnString );
			cnn.Open();
			cmd.Connection	= cnn;
            cmd.CommandTimeout = 7200;
			try
			{
				using ( SqlDataAdapter da = new SqlDataAdapter( cmd ) )
				{
					DataSet ds = new DataSet();
					da.Fill( ds );
					return ds;
				}
			}
			catch( Exception e )
			{
                WriteLog(e.Message, cmd.CommandText);
				throw e;
			}
			finally
			{
				if( cnn.State == ConnectionState.Open )
					cnn.Close();
			}
		}

		#endregion

		#region ExecuteDataReader

		public static SqlDataReader ExecuteReader( string CnnString, SqlCommand cmd )
		{
			SqlConnection cnn = new SqlConnection( CnnString );
			cnn.Open();
			cmd.Connection	= cnn;
            cmd.CommandTimeout = 7200;
			try
			{
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
			}
			catch( Exception e )
			{
                WriteLog(e.Message, cmd.CommandText);
				throw e;
			}
		}

		public static SqlDataReader ExecuteReader( string CnnString, string sql )
		{
			SqlConnection cnn = new SqlConnection( CnnString );
			cnn.Open();
			SqlCommand cmd = new SqlCommand( sql,cnn );
            cmd.CommandTimeout = 7200;
			try
			{
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
			}
			catch( Exception e )
			{
                WriteLog(e.Message, sql);
				throw e;
			}
		}

		#endregion

		public static object ExecuteScalar( string CnnString, string sql )
		{
			SqlConnection cnn = new SqlConnection( CnnString );
			cnn.Open();
			SqlCommand cmd = new SqlCommand( sql,cnn );
            cmd.CommandTimeout = 7200;
			try
			{
                return cmd.ExecuteScalar();
			}
			catch( Exception e )
			{
                WriteLog(e.Message, sql);
				throw e;
			}
			finally
			{
				if( cnn.State == ConnectionState.Open )
					cnn.Close();
			}
		}

		#region GetParameter

		// �ر�ע��
		// GetParameter(string,SqlDbType,int)��GetParameter(string,SqlDbType,object)
		// ������:ǰ��Ϊ���ֵ����,����Ϊ����ֵ����

		// �����ط������صĲ���Ϊ���ֵ����
		public static SqlParameter GetParameter( 
			string Name, 
			SqlDbType sqlDbType )
		{
			SqlParameter param	= new SqlParameter( Name , sqlDbType );
			param.Direction		= ParameterDirection.Output;

			return param;
		}

		// �����ط������صĲ���Ϊ���ֵ����
		public static SqlParameter GetParameter( 
			string Name, 
			SqlDbType sqlDbType,
			int size )
		{
			SqlParameter param	= new SqlParameter( Name , sqlDbType , size );
			param.Direction		= ParameterDirection.Output;
			return param;
		}

		// �������ط���Value����Ϊ����ֵ����
		public static SqlParameter GetParameter( 
			string Name, 
			SqlDbType sqlDbType, 
			object Value )
		{
			SqlParameter param	= new SqlParameter( Name , sqlDbType );
			param.Value			= Value;

			return param;
		}

		public static SqlParameter GetParameter( 
			string Name, 
			SqlDbType sqlDbType,
			int size,
			object Value )
		{
			return GetParameter( Name, sqlDbType, size, ParameterDirection.Input, Value );
		}

		public static SqlParameter GetParameter( 
			string Name, 
			SqlDbType sqlDbType,
			int size,
			ParameterDirection direction,
			object Value )
		{
			SqlParameter param	= new SqlParameter( Name , sqlDbType , size );
			param.Direction		= direction;
			param.Value			= Value;

			return param;
		}
		
		#endregion

		#region GetRow

		public static DataRow GetDataRowFrom( int i, DataSet ds )
		{
			if( ds != null
				&& ds.Tables != null
				&& ds.Tables.Count > 0
				&& ds.Tables[0].Rows != null
				&& ds.Tables[0].Rows.Count > i
				&& ds.Tables[0].Rows[i] != null )
			{
				return ds.Tables[0].Rows[i];
			}

			return null;
		}

		#endregion

		#region Orther

		/// <summary>
		/// �ж�������Ƿ����
		/// </summary>
		/// <param name="tableName">���������,�����в��ܺ������ݿ�ǰ׺</param>
		/// <param name="cnnString">���ݿ����Ӵ�</param>
		/// <returns>�Ƿ���ڱ�ʶ</returns>
		public static bool IsTableExsited(
			string tableName,
			string cnnString )
		{ 
			StringBuilder sb = new StringBuilder();
			sb.Append( "select * from dbo.sysobjects where id = object_id(N'[" );
			sb.Append( tableName );
			sb.AppendLine( "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1" );
			DataSet ds = SqlHelper.ExecuteDataSet( cnnString, sb.ToString());
			if( ds != null 
				&& ds.Tables.Count > 0
				&& ds.Tables[0].Rows.Count > 0)
				return true;
			return false;
		}

		/// <summary>
		/// ���ڴ��е�DataTable���������
		/// </summary>
		/// <param name="tableName">���������</param>
		/// <param name="dataTable">�ڴ��е�DataTable����</param>
		/// <param name="cnnString">���ݿ����Ӵ�</param>
		public static void CreateTableUsingSqlBulkCopy(
			string tableName,
			DataTable dataTable,
			string cnnString )
		{ 
			throw new Exception( "��û��ʵ��!!" );
		}

		/// <summary>
		/// ���ڴ��е�SqlDataReader���������
		/// </summary>
		/// <param name="tableName">���������</param>
		/// <param name="dataTable">SqlDataReader����,�����������������Ľṹ</param>
		/// <param name="cnnString">���ݿ����Ӵ�</param>
		public static void CreateTableUsingSqlBulkCopy(
			string tableName,
			SqlDataReader reader,
			string cnnString )
		{ 
			throw new Exception( "��û��ʵ��!!" );
		}

		public static string GetStringFrom( object dc )
		{
			if( dc != null && dc != DBNull.Value )
				return dc.ToString();
			else
				return string.Empty;
		}

		public static string GetStringFrom( object dc, string valueWhenStringEmpty )
		{
			string sString = GetStringFrom( dc );
			if( sString == string.Empty )
				return valueWhenStringEmpty;
			return sString;
		}

		public static int GetIntFrom( object dc, int valueWhenDBnull )
		{
			if( dc != null && dc != DBNull.Value )
				return Convert.ToInt32( dc.ToString() );
			else
				return valueWhenDBnull;
		}

		public static int GetIntFrom(object dc)
		{
			return SqlHelper.GetIntFrom(dc, 0);
		}

		public static DateTime GetDataTimeFrom( object dc, string valueWhenDBnull )
		{
			string timeString = string.Empty;
			if( dc != null && dc != DBNull.Value )
				timeString = dc.ToString();
			else
				timeString = valueWhenDBnull;

			return Convert.ToDateTime( timeString );
		}

		public static bool GetBooleanFrom( object dc, bool valueWhenDBnull )
		{
			if( dc != null && dc != DBNull.Value )
				return Convert.ToBoolean( dc );
			else
				return valueWhenDBnull;
		}

		#endregion        

        #region log
        public static void WriteLog(string s)
        {
            try
            {
                if (System.Diagnostics.EventLog.SourceExists("UFIDA ReportService"))
                    System.Diagnostics.EventLog.WriteEntry("UFIDA ReportService", s);
            }
            catch
            {
            }
        }

        public static void WriteLog(Exception e)
        {
            try
            {
                if (System.Diagnostics.EventLog.SourceExists("UFIDA ReportService"))
                    System.Diagnostics.EventLog.WriteEntry("UFIDA ReportService", e.Message + " At: " + e.StackTrace);
            }
            catch
            {
            }
        }

        public static void WriteLog(string s,string sql)
        {
            try
            {
                if (System.Diagnostics.EventLog.SourceExists("UFIDA ReportService"))
                    System.Diagnostics.EventLog.WriteEntry("UFIDA ReportService", s + " At: " + sql);
            }
            catch
            {
            }
        }
        #endregion
    }
}