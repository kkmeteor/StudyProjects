using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �ṩ�ӹ�ϣ����Ϣ�����������Ϣ�ķ���
	/// ����:¬����
	/// ʱ��:2007.5.22
	/// </summary>
	class LevelExpandTempDBHashTableToColumnService
	{
		private string _sourceTableName = string.Empty;
		private string _destTableName = string.Empty;
		private StringBuilder _createNewTableSql = null;
		private StringBuilder _selectDataToLoadSql = null;
		private StringBuilder _groupByForSelect = null;
		private Hashtable _baseColumnInfor = null;
		private DataTable _dataTableInMemory = null;

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="columnInfo">
		/// �������ж��弯�ϣ����������LevelExpandTempDBManager
		/// ���캯���в���columnInfo��˵��
		/// </param>
		/// <param name="sourceTableName">Դ������</param>
		/// <param name="destTableName">Ŀ�������</param>
		public LevelExpandTempDBHashTableToColumnService( 
			Hashtable columnInfo,
			string sourceTableName,
			string destTableName )
		{ 
			this._baseColumnInfor = columnInfo;
			this._sourceTableName = sourceTableName;
			this._destTableName = destTableName;
			this.Init();
		}

		private void Init()
		{
			try
			{
				this.InitParameters();
				foreach( string key in this._baseColumnInfor.Keys )
					this.HandleOneKey( key );
				this.PostHandle();
			}
			catch( Exception e )
			{ 
				throw new TempDBServiceException( e, 
					"����չ������:�ӹ�ϣ����������ֶ�ʱ����" );
			}
		}

		private void InitParameters()
		{ 
			this._createNewTableSql = new StringBuilder();
			this._selectDataToLoadSql = new StringBuilder();
			this._groupByForSelect = new StringBuilder();
			this._dataTableInMemory = new DataTable();
			this._createNewTableSql.Append( "CREATE TABLE " );
			this._createNewTableSql.Append( this._destTableName );
			this._createNewTableSql.AppendLine( " (" );
			this._selectDataToLoadSql.Append( "SELECT " );
			this._groupByForSelect.Append( "GROUP BY " );
		}

		private void HandleOneKey( string key )
		{
			string val = this._baseColumnInfor[key].ToString();
			this.AddOneColumnInfor( key );
            if (this.AddOneColumnInfor(val))
            {
                this._selectDataToLoadSql.Append(val);
                this._selectDataToLoadSql.Append(",");
                this._groupByForSelect.Append(val);
                this._groupByForSelect.Append(",");
            }
		}

		private bool AddOneColumnInfor( string name )
		{ 
			// ��ϣ���ֵ���ܴ������������
			if( this._dataTableInMemory.Columns.Contains( name ))
				return false;
			this._createNewTableSql.Append( name );
			this._createNewTableSql.AppendLine( " NVARCHAR(256)," );
			this._dataTableInMemory.Columns.Add( name, Type.GetType( "System.String" ));
            return true;
		}

		/// <summary>
		/// ע��:��Ҫ��ɾ������StringBuilder���Ķ���
		/// </summary>
		private void PostHandle()
		{ 
			this.DeleteTheLastChar( this._createNewTableSql );
			this.DeleteTheLastChar( this._groupByForSelect );
			this.DeleteTheLastChar( this._selectDataToLoadSql );
			this._createNewTableSql.Append( ")" );
			this._selectDataToLoadSql.Append( "\r\nFROM " );
			this._selectDataToLoadSql.AppendLine( this._sourceTableName );
			this._selectDataToLoadSql.AppendLine( this._groupByForSelect.ToString());
		}

		private void DeleteTheLastChar( StringBuilder sb )
		{ 
			sb.Remove( sb.Length -1, 1 );
		}

		/// <summary>
		/// Դ�����к�Ŀ�������е�ԭʼӳ����Ϣ
		/// </summary>
		public Hashtable BasicColumnInfo
		{
			get { return this._baseColumnInfor; }
		}

		/// <summary>
		/// ȡ�ô����±��sql���
		/// </summary>
		public string CreateNewTableSql
		{
			get { return this._createNewTableSql.ToString(); }
		}

		/// <summary>
		/// ȡ�ô�Դ���ѯ����������ڴ��е�DataTable��sql���
		/// ��SELECT����SELECT���ְ���Դ���Ŀ������,�仹����
		/// һ��GROUP BY,������Դ���ڵ��ֶν���GROUP BY.���Ƶ������:
		/// SELECT newCol1,newCol2,sourceCol 
		/// FROM sourceTable 
		/// GROUP BY sourceCol
		/// </summary>
		public string SelectDataToLoadSql
		{
			get { return this._selectDataToLoadSql.ToString(); }
		}

		/// <summary>
		/// ��ȡ�ӻ�������Ϣ�������ڴ��е�DataTable�������������
		/// </summary>
		public DataTable DataTableInMemory
		{
			get { return this._dataTableInMemory; }
		}

		public string DropTableSql
		{
			get
			{
				string tableName = this._destTableName.ToLower().Replace( "tempdb..", string.Empty );
				StringBuilder sb = new StringBuilder();
				sb.Append( "if exists( select * from dbo.sysobjects where id = object_id(N'[" );
				sb.Append( tableName );
				sb.AppendLine( "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)" );
				sb.Append( "drop table " );
				sb.AppendLine( tableName );
				return sb.ToString();
			}
		}
	}
}
