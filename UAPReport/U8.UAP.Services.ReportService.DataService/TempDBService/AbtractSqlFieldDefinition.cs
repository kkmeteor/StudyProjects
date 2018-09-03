using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 抽象的字段定义父类
	/// 作者:卢达其
	/// 时间:2007.5.17
	/// </summary>
	abstract class AbtractSqlFieldDefinition : ISqlFieldDefinition
	{
		/// <summary>
		/// 当前处理的字段类型的集合
		/// </summary>
		protected string[] _sqlDataTypeKeys = null;
		
		/// <summary>
		/// 子类需要重写此类来初始化_sqlDataTypeKeys
		/// </summary>
		public abstract void InitSqlDataTypeKeys();

		/// <summary>
		/// 获取字段最终的类型
		/// </summary>
		/// <param name="sqlDataType">原始的字段类型</param>
		/// <returns>返回最终的类型</returns>
		public virtual string GetDataType( string sqlDataType )
		{
			return sqlDataType;
		}

		/// <summary>
		/// 获取字段最终的长度定义
		/// </summary>
		/// <param name="size">原始的字段长度</param>
		/// <returns>返回字段最终的长度定义</returns>
		public virtual string GetDataTypeLength( string size )
		{
			return string.Empty;
		}

		/// <summary>
		/// 模版方法。子类通过设置<see cref="_sqlDataTypeKeys"/>来影响此方法
		/// 判断其能该处理哪一类型的字段
		/// </summary>
		/// <param name="sqlDataType">原始的字段类型</param>
		/// <returns>返回true表明此字段类型由当前类型处理</returns>
		public virtual bool IsMatchToHandle( string sqlDataType )
		{
			InitSqlDataTypeKeys();
			if( _sqlDataTypeKeys != null )
			{
				foreach( string key in _sqlDataTypeKeys )
					if( key.ToLower() == sqlDataType.ToLower() )
						return true;
			}
			return false;
		}

		/// <summary>
		/// 模版方法。子类通过重写GetDataType和GetDataTypeLength来影响返回结果
		/// </summary>
		/// <param name="dr">字段的基础数据信息.<see cref="ISqlFieldDefinition.ToSqlFieldDefinition"/></param>
		/// <returns>返回的字段定义的形式如:"column1 nvarchar(100)"</returns>
		public virtual string ToSqlFieldDefinition( DataRow dr )
		{
			string sqlDataType = dr["DataTypeName"].ToString();
			if( IsMatchToHandle( sqlDataType ))
			{
                return string.Format("[{0}] {1}{2}",
					dr["ColumnName"].ToString(),
					GetDataType( sqlDataType ),
					GetDataTypeLength( dr["ColumnSize"].ToString()));
			}
			return null;
		}
	}
}
