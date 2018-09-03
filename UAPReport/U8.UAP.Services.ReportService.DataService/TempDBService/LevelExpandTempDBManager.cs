using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �ṩ��TempDB���漶��չ�����ݵķ���
	/// ����:¬����
	/// ʱ��:2007.5.21
	/// </summary>
	class LevelExpandTempDBManager
	{
		private string _tempdbCnnString = string.Empty;
		private string _destTableName = string.Empty;
		private ReportDataFacade _reportDataFacade = null;
		private LevelExpandTempDBHashTableToColumnService _columnInfoService = null;
		private ILevelExpandTempDBGetDataService _exteralService = null;

		/// <summary>
		/// ���캯�������ཫ��ReportDataFacade�д�����ʹ��
		/// </summary>
		/// <param name="rdf">
		/// ReportDataFacade�࣬��ҪΪ��ʹ�����е�TempDBCnnString
		/// </param>
		/// <param name="columnInfo">
		/// Ŀ��������Ϣ����Ϊһ����ϣ��
		/// 1.��ΪĿ����������;
		/// 2.ֵΪӳ�䵽Դ�����ֶ�����.
		/// �������ֶ�����ΪĿ����е��ֶ�
		/// </param>
		/// <param name="sourceTableName">Դ�������</param>
		/// <param name="destTableName">Ŀ��������</param>
		/// /// <param name="exteralService">���㼶��չ����Ϣ�ķ������</param>
		public LevelExpandTempDBManager(
			ReportDataFacade rdf,
			Hashtable columnInfo,
			string sourceTableName,
			string destTableName,
			ILevelExpandTempDBGetDataService exteralService )
		{ 
			this._reportDataFacade = rdf;
			this._destTableName = destTableName;
			this._exteralService = exteralService;
			this.Check( columnInfo, sourceTableName );
			this._tempdbCnnString = this._reportDataFacade._U8LoginInfor.TempDBCnnString;
			this._columnInfoService = new LevelExpandTempDBHashTableToColumnService(
				columnInfo, sourceTableName, destTableName );
		}

		private void Check(
			Hashtable columnInfo,
			string sourceTableName )
		{
			string msg = string.Empty;
			if( this._exteralService == null )
				msg = "����չ����Ϣ�ķ������exteralServiceΪ��\r\n";
			if( this._reportDataFacade._U8LoginInfor == null
				|| string.IsNullOrEmpty( this._reportDataFacade._U8LoginInfor.TempDBCnnString ))
				msg += "U8LoginInfor��U8LoginInfor.TempDBCnnStringΪ��\r\n";
			if( columnInfo == null
				|| columnInfo.Count == 0 )
				msg += "�ж���columnInfo��ϣ��Ϊ��\r\n";
			if( string.IsNullOrEmpty( this._destTableName )
				|| string.IsNullOrEmpty( sourceTableName ))
				msg = "Դ���Ŀ�������Ϊ��\r\n";
			if( !string.IsNullOrEmpty( msg ))
				throw new TempDBServiceException( msg, "None" );
		}

		/// <summary>
		/// �����������ݵĹ���Ϊ��
		/// 1.�����ݿ��д���Ŀ���
		/// 2.���ڴ��д���һ����Ŀ����ͬ��DataTable����װ�غ�ʵ������
		/// 3.ʹ��SqlBulkCopy���ڴ��е�DataTable���Ƶ�Ŀ���
		/// </summary>
		public void CacheExpandData()
		{
			try
			{
				this.CreateDestinationTable();
				this.LoadMomeryDataTable();
				this.MoveDataToDataBase();
			}
			catch( Exception e )
			{ 
				// ɾ���Ѿ���������ʱ��
				try
				{
					string sql = this._columnInfoService.DropTableSql;
					SqlHelper.ExecuteNonQuery( this._tempdbCnnString, sql );
				}
				catch { }
				throw e;
			}
		}

		private void CreateDestinationTable()
		{ 
			LevelExpandTempDBCreateTable tableCreator = new LevelExpandTempDBCreateTable(
				this._columnInfoService,
				this._tempdbCnnString );
			tableCreator.Create();
		}

		private void LoadMomeryDataTable()
		{ 
			LevelExpandTempDBProduceDataTable dataTableLoader = new LevelExpandTempDBProduceDataTable(
				this._columnInfoService,
				this._tempdbCnnString );
			dataTableLoader.LoadDataToDataTable( this._exteralService );
		}

		private void MoveDataToDataBase()
		{ 
			SqlBulkCopy sbc = new SqlBulkCopy( this._tempdbCnnString );
			sbc.DestinationTableName = this._destTableName;
			sbc.WriteToServer( this._columnInfoService.DataTableInMemory );
		}
	}
}
