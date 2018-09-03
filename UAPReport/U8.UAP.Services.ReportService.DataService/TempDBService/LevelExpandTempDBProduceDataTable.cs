using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ���ڴ��д���һ������չ����Ϣ��DataTable,��װ����ʵ�ʵ�����
	/// ����:¬����
	/// ʱ��:2007.5.21
	/// </summary>
	class LevelExpandTempDBProduceDataTable
	{
		private string _cnnString = string.Empty;
		private LevelExpandTempDBHashTableToColumnService _columnInfoService = null;

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="columnInfoService">
		/// ����Ϣ�������,���Դ��л�ȡ��ѯװ�����ݵ�"SELECT"���
		/// ���Ѿ����úüܹ����ڴ��е�DataTable����
		/// </param>
		/// <param name="cnnStrng">���ݿ����Ӵ�</param>
		public LevelExpandTempDBProduceDataTable(
			LevelExpandTempDBHashTableToColumnService columnInfoService,
			string cnnStrng )
		{ 
			this._columnInfoService = columnInfoService;
			this._cnnString = cnnStrng;
		}

		/// <summary>
		/// װ�����ݵ�DataTable,�˹���Ϊ��
		/// 1.�����������ж��崴��DataTable����
		/// 2.�Ӵ�����ж����ϣ���ֵƴ��sql��֮���Դ���в�ѯ�����SqlDataReader
		/// 3.�����ⲿ������Ŀ�������ݲ���ӵ�DataTable��������ⲿ����
		/// ��ҪSqlDataReaderΪ����������ֵ����һ���ڱ��ഴ����DataRow������.
		/// ����1,2������_columnInfoService�����
		/// </summary>
		public void LoadDataToDataTable( 
			ILevelExpandTempDBGetDataService exteralService )
		{
			DataTable dt = this._columnInfoService.DataTableInMemory;
			SqlDataReader reader = this.GetReaderForExteralService();
			while( reader.Read() )
			{ 
				DataRow dr = dt.NewRow();
				dt.Rows.Add( dr );

				// �����ⲿ�����ȡʵ������
				try
				{
					exteralService.GetData(
						this._columnInfoService.BasicColumnInfo,
						reader,
						dr );
				}
				catch( Exception e )
				{
					throw new TempDBServiceException( e,
						"�����ⲿ�����Ի�ü���չ����Ϣʱ��������" );
				}
			}
		}

		private SqlDataReader GetReaderForExteralService()
		{
			string sql = string.Empty;
			try
			{
				sql = this._columnInfoService.SelectDataToLoadSql;
				return SqlHelper.ExecuteReader( this._cnnString, sql );
			}
			catch( Exception e )
			{ 
				throw new TempDBServiceException( e, 
					"Ϊ��ü���չ����Ϣ��������ѯԴ����Ϣ��SqlDataReader����ѯ���:\r\n" + sql );
			}
		}
	}
}
