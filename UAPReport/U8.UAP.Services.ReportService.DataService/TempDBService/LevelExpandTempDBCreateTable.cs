using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �������漶��չ�����ݵ������
	/// ����:¬����
	/// ʱ��:2007.5.21
	/// </summary>
	class LevelExpandTempDBCreateTable
	{
		private string _cnnString = string.Empty;
		private LevelExpandTempDBHashTableToColumnService _columnInfoService = null;

		/// <summary>
		/// ���캯����
		/// </summary>
		/// <param name="columnInfoService">
		/// ����Ϣ�������,���Դ��л�ȡ����������"CREATE TABLE"���
		/// </param>
		/// <param name="cnnString">���ݿ����Ӵ�</param>
		public LevelExpandTempDBCreateTable( 
			LevelExpandTempDBHashTableToColumnService columnInfoService,
			string cnnString )
		{
			this._cnnString = cnnString;
			this._columnInfoService = columnInfoService;
		}

		public void Create()
		{
			string sql = string.Empty;
			try
			{
				sql = this._columnInfoService.CreateNewTableSql;
				SqlHelper.ExecuteNonQuery( this._cnnString, sql );
			}
			catch( Exception e )
			{
				throw new TempDBServiceException( e, GetExtraMessage( sql ));
			}
		}

		public void DropTableWhenErr()
		{
			string sql = string.Empty;
			try
			{
				sql = this._columnInfoService.CreateNewTableSql;
				SqlHelper.ExecuteNonQuery( this._cnnString, sql );
			}
			catch( Exception e )
			{
				throw new TempDBServiceException( e, GetExtraMessage( sql ));
			}
		}

		private string GetExtraMessage( string sql )
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "��ͼ�������漶��չ�����ݻ�����ʱ����ִ��������Ϣ:" );
			sb.AppendLine( "������sql���:" );
			sb.AppendLine( sql );
			return sb.ToString();
		}
	}
}
