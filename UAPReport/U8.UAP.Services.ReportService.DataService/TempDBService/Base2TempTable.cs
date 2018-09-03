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
    /// 从其它形式的数据源将数据转移到临时表的基础服务类
    /// 作者:卢达其
    /// 时间:2007.5.17
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
        /// 数据转移到临时表的基础服务类构造函数
        /// </summary>
        /// <param name="rdf">UFIDA.U8.UAP.Services.ReportData.ReportDataFacade类型,主要是使用其提供的UFDataCnnString</param>
        /// <param name="customDataSource">通过此参数将临时表名称返回调用者</param>
        public Base2TempTable(
            ReportDataFacade rdf,
            CustomDataSource customDataSource)
        {
            this._loginInfor = rdf._U8LoginInfor;
            this._customDataSource = customDataSource;
        }

        /// <summary>
        /// 数据源统一为临时表.临时表名称由customDataSource产生,最终填充到
        /// customDataSource.SQL来返回调用者。
        /// 这是一个模版方法，处理分两类：sql和存储过程;其子类只需要重写
        /// SetSqlOrStoreProcFlag(),GetSql(),GetHandlingCommand()来实现不同结果
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
                        //该分支为移动应用新增的，移动应用的临时表名是按照命名规则me_开头传入的
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
                "数据转移到临时表时调用错误",
                "不能从Base2TempTable直接调用ToTempTable");
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
        /// 从sql语句将数据填写到临时表
        /// </summary>
        /// <param name="sql">SELECT * FROM TableName ...形式的sql语句,其提供源数据</param>
        /// <param name="tempTableName">目标临时表名称</param>
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
        /// 从sql语句将数据填写到临时表
        /// </summary>
        /// <param name="cmd">准备好查询信息的SqlCommand对象</param>
        /// <param name="tempTableName">目标临时表名称</param>
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

            //    // 可能selectedFields已经存在BaseId标识字段
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
        /// 处于对效率的考虑，把order by字句去掉
        /// </summary>
        /// <param name="sql">原始的sql语句</param>
        /// <returns>返回不含有order by的sql语句</returns>
        protected string GetSqlWithoutOrderBy(string sql)
        {
            string wrappedSql = sql;
            int orderByIndex = wrappedSql.ToLower().IndexOf("order by");
            if (orderByIndex != -1)
                wrappedSql = wrappedSql.Substring(0, orderByIndex);
            return wrappedSql;
        }

        /// <summary>
        /// 从存储过程将数据填写到临时表
        /// </summary>
        /// <param name="storeprocedureName">存储过程名称</param>
        /// <param name="tempTableName">目标临时表名称</param>
        /// <param name="paras">存储过程的参数键值集合</param>
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
                string exMsg = "使用SqlBulkCopy从Reader复制数据到临时表时发生错误";
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
        /// 如果存储过程的参数中包含@TableName参数，
        /// 则使用UfDataCnnString执行给定的存储过程
        /// </summary>
        /// <param name="storeprocedureName">存储过程对应的SqlCommand对象</param>
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
        /// 判断含有@TableName的存储过程在被执行之后
        /// 是否成功创建了临时表，如果没有创建则抛出异常
        /// </summary>
        /// <param name="cmd">执行存储过程的SqlCommand对象</param>
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
                    "执行了含有@TableName参数的存储过程，但是存储过程内部没有成功创建临时表",
                    exMsg);
            }
        }
    }
}