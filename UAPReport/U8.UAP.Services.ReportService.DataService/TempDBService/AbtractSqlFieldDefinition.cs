using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ������ֶζ��常��
	/// ����:¬����
	/// ʱ��:2007.5.17
	/// </summary>
	abstract class AbtractSqlFieldDefinition : ISqlFieldDefinition
	{
		/// <summary>
		/// ��ǰ������ֶ����͵ļ���
		/// </summary>
		protected string[] _sqlDataTypeKeys = null;
		
		/// <summary>
		/// ������Ҫ��д��������ʼ��_sqlDataTypeKeys
		/// </summary>
		public abstract void InitSqlDataTypeKeys();

		/// <summary>
		/// ��ȡ�ֶ����յ�����
		/// </summary>
		/// <param name="sqlDataType">ԭʼ���ֶ�����</param>
		/// <returns>�������յ�����</returns>
		public virtual string GetDataType( string sqlDataType )
		{
			return sqlDataType;
		}

		/// <summary>
		/// ��ȡ�ֶ����յĳ��ȶ���
		/// </summary>
		/// <param name="size">ԭʼ���ֶγ���</param>
		/// <returns>�����ֶ����յĳ��ȶ���</returns>
		public virtual string GetDataTypeLength( string size )
		{
			return string.Empty;
		}

		/// <summary>
		/// ģ�淽��������ͨ������<see cref="_sqlDataTypeKeys"/>��Ӱ��˷���
		/// �ж����ܸô�����һ���͵��ֶ�
		/// </summary>
		/// <param name="sqlDataType">ԭʼ���ֶ�����</param>
		/// <returns>����true�������ֶ������ɵ�ǰ���ʹ���</returns>
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
		/// ģ�淽��������ͨ����дGetDataType��GetDataTypeLength��Ӱ�췵�ؽ��
		/// </summary>
		/// <param name="dr">�ֶεĻ���������Ϣ.<see cref="ISqlFieldDefinition.ToSqlFieldDefinition"/></param>
		/// <returns>���ص��ֶζ������ʽ��:"column1 nvarchar(100)"</returns>
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
