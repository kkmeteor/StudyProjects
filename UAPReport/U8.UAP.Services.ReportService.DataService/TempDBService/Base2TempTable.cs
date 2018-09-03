using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportFilterService;

namespace UFIDA.U8.UAP.Services.ReportData
{
    /// <summary>
    /// ��������ʽ������Դ������ת�Ƶ���ʱ��Ļ���������
    /// ����:¬����
    /// ʱ��:2007.5.17
    /// </summary>
    public class Base2TempTable : I2TempTable
    {
        protected const string _cmdParaFilterString = "@filterstring";

        protected DataSourceType _dataSourceType = DataSourceType.None;
        protected CustomDataSource _customDataSource = null;

        private const string _cmdParaTableName = "@tablename";

        private string _tempTableName = string.Empty;
        private U8LoginInfor _loginInfor = null;

        /// <summary>
        /// ����ת�Ƶ���ʱ��Ļ��������๹�캯��
        /// </summary>
        /// <param name="rdf">UFIDA.U8.UAP.Services.ReportData.ReportDataFacade����,��Ҫ��ʹ�����ṩ��UFDataCnnString</param>
        /// <param name="customDataSource">ͨ���˲�������ʱ�����Ʒ��ص�����</param>
        public Base2TempTable(
            ReportDataFacade rdf,
            CustomDataSource customDataSource)
        {
            this._loginInfor = rdf._U8LoginInfor;
            this._customDataSource = customDataSource;
        }

        /// <summary>
        /// ����ԴͳһΪ��ʱ��.��ʱ��������customDataSource����,������䵽
        /// customDataSource.SQL�����ص����ߡ�
        /// ����һ��ģ�淽������������ࣺsql�ʹ洢����;������ֻ��Ҫ��д
        /// SetSqlOrStoreProcFlag(),GetSql(),GetHandlingCommand()��ʵ�ֲ�ͬ���
        /// <see cref=""/>
        /// </summary>
        public virtual void ToTempTable()
        {
            this.SetSqlOrStoreProcFlag();
            switch (this._dataSourceType)
            {
                default:
                    this.ThrowExceptionWhenNotAssignDataSourceType();
                    return;
                case DataSourceType.FromSql:
                    this.Sql2TempTable(GetSql(), GetTempTableName());
                    break;
                case DataSourceType.FromSqlExcuteBySqlCommand:
                    this.Sql2TempTable(GetHandlingCommand(), GetTempTableName());
                    break;
                case DataSourceType.FromStoreProcedure:
                    SqlCommand cmd = GetHandlingCommand();
                    if (!ExecuteStoreProOnlyIfNecessary(cmd))
                    {
                        //�÷�֧Ϊ�ƶ�Ӧ�������ģ��ƶ�Ӧ�õ���ʱ�����ǰ�����������me_��ͷ�����
                        //add by yanghx
                        //if (!string.IsNullOrEmpty(this._customDataSource.SQL) &&
                        //    this._customDataSource.SQL.StartsWith("tempdb..me_"))
                        //{
                        // this._tempTableName = this._customDataSource.SQL;
                        // this.Storeprocedure2TempTable(cmd, this._tempTableName);
                        // }
                        // else
                        //{
                        this.Storeprocedure2TempTable(cmd, GetTempTableName());
                        //}
                    }
                    else //cwh
                        this._customDataSource.Type = CustomDataSourceTypeEnum.StoreProc;
                    break;
            }
            this._customDataSource.SQL = this._tempTableName;
        }

        protected virtual void SetSqlOrStoreProcFlag()
        {
        }

        protected virtual string GetSql()
        {
            ThrowInvokeException();
            return null;
        }

        protected virtual SqlCommand GetHandlingCommand()
        {
            ThrowInvokeException();
            return null;
        }

        private void ThrowInvokeException()
        {
            throw new TempDBServiceException(
                "����ת�Ƶ���ʱ��ʱ���ô���",
                "���ܴ�Base2TempTableֱ�ӵ���ToTempTable");
        }

        protected virtual void ThrowExceptionWhenNotAssignDataSourceType()
        {
        }

        private string GetTempTableName()
        {
            if (!string.IsNullOrEmpty(this._customDataSource.SQL) &&
                         this._customDataSource.SQL.StartsWith("tempdb..[me_"))
            {
                this._tempTableName = this._customDataSource.SQL;
            }
            else
                //this._tempTableName = CustomDataSource.GetATableName();
                this._tempTableName = CustomDataSource.GetATableNameWithTaskId(_loginInfor.TaskID);

            return this._tempTableName;
        }

        /// <summary>
        /// ��sql��佫������д����ʱ��
        /// </summary>
        /// <param name="sql">SELECT * FROM TableName ...��ʽ��sql���,���ṩԴ����</param>
        /// <param name="tempTableName">Ŀ����ʱ������</param>
        public void Sql2TempTable(
            string sql,
            string tempTableName)
        {
            string wrappedSql = string.Empty;
            try
            {
                wrappedSql = GetWrappedSql(sql, tempTableName);
                SqlHelper.ExecuteNonQuery(
                    this._loginInfor.UfDataCnnString,
                    wrappedSql);
            }
            catch (Exception e)
            {
                throw new TempDBServiceException(e, wrappedSql);
            }
        }

        /// <summary>
        /// ��sql��佫������д����ʱ��
        /// </summary>
        /// <param name="cmd">׼���ò�ѯ��Ϣ��SqlCommand����</param>
        /// <param name="tempTableName">Ŀ����ʱ������</param>
        public void Sql2TempTable(
            SqlCommand cmd,
            string tempTableName)
        {
            string wrappedSql = string.Empty;
            try
            {
                wrappedSql = GetWrappedSql(cmd.CommandText, tempTableName);
                cmd.CommandText = wrappedSql;
                SqlHelper.ExecuteNonQuery(
                    this._loginInfor.UfDataCnnString,
                    cmd);
            }
            catch (Exception e)
            {
                throw new TempDBServiceException(e, wrappedSql);
            }
        }

        private string GetWrappedSql(
            string sql,
            string tempTableName)
        {
            string wrappedSql = GetSqlWithoutOrderBy(sql);
            string selectedFields = "*";
            string selectPart = "SELECT ";

            //if (this._customDataSource != null
            //    && !string.IsNullOrEmpty(this._customDataSource.SelectString))
            //{
            //    selectedFields = this._customDataSource.SelectString;

            //    // ����selectedFields�Ѿ�����BaseId��ʶ�ֶ�
            //    if (selectedFields.ToLower().IndexOf("baseid") == -1)
            //        selectPart += "IDENTITY(int,0,1) AS BaseId, ";
            //}
            //else
            selectedFields = " * ";//" top 0 * ";

            return string.Format(
                @"{0}{1} INTO {2} FROM ({3}) Alias4ReportData",
                selectPart,
                selectedFields,
                tempTableName,
                wrappedSql);
        }

        /// <summary>
        /// ���ڶ�Ч�ʵĿ��ǣ���order by�־�ȥ��
        /// </summary>
        /// <param name="sql">ԭʼ��sql���</param>
        /// <returns>���ز�����order by��sql���</returns>
        protected string GetSqlWithoutOrderBy(string sql)
        {
            string wrappedSql = sql;
            int orderByIndex = wrappedSql.ToLower().IndexOf("order by");
            if (orderByIndex != -1)
                wrappedSql = wrappedSql.Substring(0, orderByIndex);
            return wrappedSql;
        }

        /// <summary>
        /// �Ӵ洢���̽�������д����ʱ��
        /// </summary>
        /// <param name="storeprocedureName">�洢��������</param>
        /// <param name="tempTableName">Ŀ����ʱ������</param>
        /// <param name="paras">�洢���̵Ĳ�����ֵ����</param>
        public void Storeprocedure2TempTable(
            string storeprocedureName,
            string tempTableName,
            Hashtable paras)
        {
            Storeprocedure2TempTable(
                SqlCommandHelper.GetSqlCommand(storeprocedureName, paras),
                tempTableName);
        }

        private void Storeprocedure2TempTable(
            SqlCommand cmd,
            string tempTableName)
        {
            SqlDataReader reader = null;
            try
            {
                reader = SqlHelper.ExecuteReader(
                    this._loginInfor.UfDataCnnString,
                    cmd);
            }
            catch (Exception e)
            {
                string exMsg = SqlCommandHelper.GetMsgWhenStoreProcedureError(cmd, e);
                throw new TempDBServiceException(e, exMsg);
            }
            this.Reader2TempTable(reader, tempTableName);
        }

        private void Reader2TempTable(
            SqlDataReader reader,
            string tempTableName)
        {
            try
            {
                this.CreateTable(tempTableName, reader);
                SqlBulkCopy sbc = new SqlBulkCopy(this._loginInfor.UfDataCnnString, SqlBulkCopyOptions.KeepNulls);
                sbc.BulkCopyTimeout = 7200;
                sbc.DestinationTableName = tempTableName;
                sbc.WriteToServer(reader);
            }
            catch (Exception e)
            {
                string exMsg = "ʹ��SqlBulkCopy��Reader�������ݵ���ʱ��ʱ��������";
                throw new TempDBServiceException(e, exMsg);
            }
        }

        private void CreateTable(string tempTableName, SqlDataReader reader)
        {
            CreateTableFromReader ctfr = new CreateTableFromReader(
                reader, this._loginInfor.UfDataCnnString, tempTableName);
            ctfr.CreateTable();
        }

        /// <summary>
        /// ����洢���̵Ĳ����а���@TableName������
        /// ��ʹ��UfDataCnnStringִ�и����Ĵ洢����
        /// </summary>
        /// <param name="storeprocedureName">�洢���̶�Ӧ��SqlCommand����</param>
        private bool ExecuteStoreProOnlyIfNecessary(SqlCommand cmd)
        {
            if (cmd != null
                && cmd.Parameters.Contains(_cmdParaTableName))
            {
                cmd.Parameters[_cmdParaTableName].Value = GetTempTableName();
                SqlCommandHelper.ExecuteStoreProcedure(cmd, this._loginInfor.UfDataCnnString);
                this.CheckWhetherHadCreatedTempTable(cmd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// �жϺ���@TableName�Ĵ洢�����ڱ�ִ��֮��
        /// �Ƿ�ɹ���������ʱ�����û�д������׳��쳣
        /// </summary>
        /// <param name="cmd">ִ�д洢���̵�SqlCommand����</param>
        private void CheckWhetherHadCreatedTempTable(SqlCommand cmd)
        {
            string tableName = this._tempTableName.ToLower().Replace("tempdb..", string.Empty);
            if ((!string.IsNullOrEmpty(tableName)) && tableName.StartsWith("[") && tableName.EndsWith("]"))
            {
                tableName = tableName.Substring(1, tableName.Length - 2);
            }
            if (!SqlHelper.IsTableExsited(tableName, this._loginInfor.TempDBCnnString))
            {
                string exMsg = SqlCommandHelper.GetMsgWhenStoreProcedureError(cmd, null);
                throw new TempDBServiceException(
                    "ִ���˺���@TableName�����Ĵ洢���̣����Ǵ洢�����ڲ�û�гɹ�������ʱ��",
                    exMsg);
            }
        }
    }
}