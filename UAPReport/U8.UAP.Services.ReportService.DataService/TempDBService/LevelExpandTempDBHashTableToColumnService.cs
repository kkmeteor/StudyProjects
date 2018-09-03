using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 提供从哈希表信息提出具体列信息的服务
	/// 作者:卢达其
	/// 时间:2007.5.22
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
		/// 构造函数
		/// </summary>
		/// <param name="columnInfo">
		/// 物理表的列定义集合，具体请参阅LevelExpandTempDBManager
		/// 构造函数中参数columnInfo的说明
		/// </param>
		/// <param name="sourceTableName">源表名称</param>
		/// <param name="destTableName">目标表名称</param>
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
					"级次展开服务:从哈希表构建物理表字段时错误" );
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
			// 哈希表的值可能存在重名的情况
			if( this._dataTableInMemory.Columns.Contains( name ))
				return false;
			this._createNewTableSql.Append( name );
			this._createNewTableSql.AppendLine( " NVARCHAR(256)," );
			this._dataTableInMemory.Columns.Add( name, Type.GetType( "System.String" ));
            return true;
		}

		/// <summary>
		/// 注意:需要先删除各个StringBuilder最后的逗号
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
		/// 源数据列和目标数据列的原始映射信息
		/// </summary>
		public Hashtable BasicColumnInfo
		{
			get { return this._baseColumnInfor; }
		}

		/// <summary>
		/// 取得创建新表的sql语句
		/// </summary>
		public string CreateNewTableSql
		{
			get { return this._createNewTableSql.ToString(); }
		}

		/// <summary>
		/// 取得从源表查询数据以填充内存中的DataTable的sql语句
		/// 此SELECT语句的SELECT部分包含源表和目标表的列,其还包含
		/// 一个GROUP BY,将根据源表内的字段进行GROUP BY.类似的语句如:
		/// SELECT newCol1,newCol2,sourceCol 
		/// FROM sourceTable 
		/// GROUP BY sourceCol
		/// </summary>
		public string SelectDataToLoadSql
		{
			get { return this._selectDataToLoadSql.ToString(); }
		}

		/// <summary>
		/// 获取从基础列信息创建的内存中的DataTable以用来填充数据
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
