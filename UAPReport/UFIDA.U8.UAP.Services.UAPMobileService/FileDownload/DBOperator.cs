using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using UFIDA.U8.UAP.Services.ReportElements;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public static class DBOperator
    {
        internal static void SaveMobileReport(MobileReport mobileReport, ref string filePath)
        {
            #region 1.将报表格式信息存入数据库中
            string tableSchema = DslTransfer.TransferToTableSchema(mobileReport);
            string root = GetU8Path();
            var directoryPath = Path.Combine(root, @"U8AuditWebSite/MobileReport");
            string guid = Guid.NewGuid().ToString();
            string fileName = string.Format("ReportData{0}.db", guid);
            var cachePath = Path.Combine(root, string.Format(@"U8AuditWebSite/MobileReport/ReportData{0}.db", guid));
            Directory.CreateDirectory(directoryPath);
            SQLiteConnection.CreateFile(cachePath);

            //连接数据库
            var conn = new SQLiteConnection();
            var connstr = new SQLiteConnectionStringBuilder
                {
                    DataSource = cachePath
                };
            conn.ConnectionString = connstr.ToString();
            conn.Open();
            //创建表
            var cmd = new SQLiteCommand();
            string sql = "drop table if exists TableSchema";
            cmd.CommandText = sql;
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            sql = "CREATE TABLE TableSchema(tableSchema varchar(10000))";
            cmd.CommandText = sql;
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            //插入数据
            sql = string.Format("INSERT INTO TableSchema VALUES('{0}')", tableSchema);
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            #endregion

            #region 2.报表数据存入数据库中
            // 创建报表格式表
            var cellList = new List<Cell>();
            bool isMutiReport = false;
            Hashtable ht = new Hashtable();
            Report report = mobileReport.Report;
            var supperCellList = new List<Cell>();
            var childCellList = new List<Cell>();
            if (mobileReport != null)
            {

                Section section = report.Sections[SectionType.PageTitle];
                foreach (object gridDetailCell in section.Cells)
                {
                    var cell = gridDetailCell as Cell;
                    if (cell == null || !cell.Visible)
                        continue;
                    if (cell is SuperLabel)
                    {
                        isMutiReport = true; //如果列头存在superLable类型，则为多列头格式报表
                        supperCellList.Add(cell);
                    }
                    cellList.Add(gridDetailCell as Cell);
                }
                if (isMutiReport)
                {
                    cellList = new List<Cell>();
                    section = report.Sections[SectionType.Detail];
                    foreach (object gridDetailCell in section.Cells)
                    {
                        var cell = gridDetailCell as Cell;
                        if (cell != null && cell.Visible)
                        {
                            cellList.Add(gridDetailCell as Cell);
                            childCellList.AddRange(from superCell in supperCellList
                                                   where (cell.Super != null && cell.Super.Name == superCell.Name)
                                                   select cell);
                        }
                    }
                }
                // 这里取pageTitle的一个对象来组成报表标题区域的值
                //var section = report.Sections[SectionType.PageTitle];
                //foreach (var gridDetailCell in section.Cells)
                //{
                //    var cell = gridDetailCell as Cell;
                //    if (cell != null && cell.Visible)
                //    {
                //        cellList.Add(gridDetailCell as Cell);
                //    }
                //}
            }
            // 应手机端开发人员要求，这里要通过后台设置各个列的次序等信息，采用f0,f1,f2,f3...等定位列头。
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < cellList.Count; i++)
            {
                sb.Append("f" + i);
                sb.Append(" varchar(50)");
                sb.Append(",");
            }
            //foreach (var cell in cellList)
            //{
            //    sb.Append(cell.Name);
            //    sb.Append(" varchar(50)");
            //    sb.Append(",");
            //}
            sb.Remove(sb.Length - 1, 1);
            sql = "drop table if exists ReportData";
            cmd.CommandText = sql;
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            sql = string.Format("CREATE TABLE ReportData({0})", sb);
            cmd.CommandText = sql;
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();

            if (!isMutiReport)
            {
                // 将reportData第一行添加报表中文名称等信息。
                sb = new StringBuilder();
                sb.Append("insert into ReportData VALUES(");
                foreach (var cell in cellList)
                {
                    sb.Append(string.Format("'{0}',", cell.Caption));
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(")");
                sql = sb.ToString();
                cmd.CommandText = sql;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
            else //如果该报表属于多列头报表，首先从Detail中取得子列，并对列头进行特殊处理
            {
                // 将reportData前两行添加报表中文名称等信息。
                sb = new StringBuilder();
                sb.Append("insert into ReportData VALUES(");
                foreach (var cell in cellList)
                {
                    sb.Append(string.Format("'{0}',", childCellList.Contains(cell) ? cell.Super.Caption : cell.Caption));
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(")");
                sql = sb.ToString();
                cmd.CommandText = sql;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                sb = new StringBuilder();
                sb.Append("insert into ReportData VALUES(");
                foreach (var cell in cellList)
                {

                    sb.Append(string.Format("'{0}',", cell.Caption));
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(")");
                sql = sb.ToString();
                cmd.CommandText = sql;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }


            // 插入报表数据
            DbTransaction trans = conn.BeginTransaction(); // <--加入事务控制
            try
            {
                foreach (SemiRow semiRow in mobileReport.SemiRows)
                {
                    sb = new StringBuilder();
                    sb.Append("insert into ReportData VALUES(");
                    foreach (var cell in cellList)
                    {
                        string value;
                        if (semiRow[cell.Name] == null)
                            value = "";
                        else
                        {
                            value = semiRow[cell.Name].ToString();
                        }
                        if (value == "'")
                            value = "''";
                        sb.Append(string.Format("'{0}',", value));
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(")");
                    sql = sb.ToString();
                    cmd.CommandText = sql;
                    cmd.Connection = conn;
                    cmd.ExecuteNonQuery();
                }
                trans.Commit(); // <--提交事务 
            }
            catch (Exception exception)
            {
                trans.Rollback(); // <--失败则回滚
                throw exception;
            }
            filePath = Path.ChangeExtension(fileName, ".txt");
            var filePath1 = Path.ChangeExtension(cachePath, ".txt");
            File.Copy(cachePath, filePath1);
            //File.Move(cachePath, filePath1);
            #endregion
        }

        #region 获取U8安装路径
        /// <summary>
        /// 通过注册表获取U8安装路径
        /// </summary>
        /// <returns>U8安装路径</returns>
        private static string GetU8Path()
        {
            string subKey = @"Software\UFSoft\WF\V8.700\Install\CurrentInstPath", sValue = "";
            try
            {
                using (var key = Registry.LocalMachine)
                {
                    using (RegistryKey key2 = key.OpenSubKey(subKey))
                    {
                        if ((key2 != null) && (key2.GetValue(sValue) != null))
                        {
                            return (string)key2.GetValue(sValue);
                        }
                    }
                }
                return GetInstallPath();
            }
            catch
            {
                return GetInstallPath();
            }

        }
        private static string GetInstallPath()
        {
            string referencepath = Path.GetDirectoryName(Application.ExecutablePath);
            if (referencepath != null && !referencepath.EndsWith(@"\"))
                referencepath += @"\";

            if (referencepath.ToLower().EndsWith(@"\uap\"))
            {
                //
                // 如果是从uap目录下执行 去掉路径中的"uap\"
                //
                referencepath.Remove(referencepath.Length - 4);
            }
            return referencepath;
        }
        #endregion
    }
}
