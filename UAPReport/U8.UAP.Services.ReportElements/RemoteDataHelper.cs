using System;
using System.Data;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportResource;
using System.Data.SqlClient;
using Microsoft.Win32;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// singlecall
    /// </summary>
    public class RemoteDataHelper : MarshalByRefObject
    {
        public RemoteDataHelper()
        {
        }

        #region web auth check
        private object GetLogin(string usertoken, string taskid, string subid)
        {
            try
            {
                return ReportLoginManager.GetLogin(usertoken, taskid, subid);
            }
            catch (Exception ex)
            {
                //throw new ReportException(ex.Message);
                return null;
            }
        }

        public void AuthCheck(string usertoken, string taskid, string subid, string viewid, OperationEnum operation)
        {
            object login = GetLogin(usertoken, taskid, subid);
            try
            {
                ReportAuth ra = new ReportAuth();
                if (!ra.AuthCheck(login, viewid, operation))
                    throw new ResourceReportException("U8.UAP.Services.ReportExhibition.ReportControllerBase.Auth.Ex");//"没有权限" 
            }
            catch (Exception ex)
            {
                throw new ReportException(ex.Message);
            }
            finally
            {
                if (login != null)
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(login);
            }
        }

        public void ReleaseAuth(string usertoken, string taskid, string subid, string viewid, OperationEnum operation)
        {
            object login = GetLogin(usertoken, taskid, subid);
            try
            {
                ReportAuth ra = new ReportAuth();
                if (login != null)
                    ra.ReleaseAuth(login, viewid, operation);
            }
            catch (Exception ex)
            {
                throw new ReportException(ex.Message);
            }
            finally
            {
                if (login != null)
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(login);
            }

        }
        #endregion

        #region cs auth check

        #region 记录日志
        public void AuthCheck(
            object login,
            string reportIdOrViewId,
            OperationEnum operation)
        {
            this.AuthCheck(login, reportIdOrViewId, operation, null);
        }

        public void AuthCheck(
            object login,
            string reportIdOrViewId,
            OperationEnum operation,
            string otherAuthString)
        {
            try
            {
                ReportAuth ra = new ReportAuth();
                if (!ra.AuthCheck(login, reportIdOrViewId, operation, otherAuthString))
                    throw new ResourceReportException("U8.UAP.Services.ReportExhibition.ReportControllerBase.Auth.Ex");//"没有权限" 
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }
        #endregion

        #region 不记录日志
        public void AuthCheckNoLog(
            object login,
            string reportIdOrViewId,
            OperationEnum operation)
        {
            this.AuthCheckNoLog(login, reportIdOrViewId, operation, null);
        }

        public void AuthCheckNoLog(
            object login,
            string reportIdOrViewId,
            OperationEnum operation,
            string otherAuthString)
        {
            try
            {
                ReportAuth ra = new ReportAuth();
                if (!ra.AuthCheckNoLog(login, reportIdOrViewId, operation, otherAuthString))
                    throw new ResourceReportException("U8.UAP.Services.ReportExhibition.ReportControllerBase.Auth.Ex");//"没有权限" 
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }
        #endregion

        public void ReleaseAuth(
            object login,
            string reportIdOrViewId,
            OperationEnum operation)
        {
            this.ReleaseAuth(login, reportIdOrViewId, operation, null);
        }

        public void ReleaseAuth(
            object login,
            string reportIdOrViewId,
            OperationEnum operation,
            string otherAuthString)
        {
            try
            {
                ReportAuth ra = new ReportAuth();
                ra.ReleaseAuth(login, reportIdOrViewId, operation, otherAuthString);
            }
            catch (Exception ex)
            {
                throw new ReportException(ex.Message);
            }
        }
        #endregion

        #region web auth string
        public string GetColAuthString(string usertoken, string taskid, string subid, string viewid)
        {
            object login = GetLogin(usertoken, taskid, subid);
            try
            {
                ReportAuth ra = new ReportAuth();
                return ra.GetViewColAuthString(login, viewid);
            }
            catch (Exception ex)
            {
                throw new ReportException(ex.Message);
            }
            finally
            {
                if (login != null)
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(login);
            }
        }
        #endregion

        #region cs auth string
        public string GetColAuthString(object login, string viewid)
        {
            try
            {
                ReportAuth ra = new ReportAuth();
                return ra.GetViewColAuthString(login, viewid);
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }
        #endregion

        #region web get sql
        public FilterArgs GetSql(string usertoken, string taskid, string subid, FilterArgs e)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("Begin web get sql :" + e.ClassName);
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                System.Diagnostics.Trace.WriteLine("watch start");
                if (e.ClassName != "")
                {
                    object oComObj = null;
                    object[] oParams = null;
                    string errheader = "执行自定义数据源 " + e.ClassName + " 出错：";
                    try
                    {
                        e.Login = ReportLoginManager.GetLogin(usertoken, taskid, subid);

                        UFGeneralFilter.FilterSrv objfilter = new UFGeneralFilter.FilterSrvClass();
                        object err = null;
                        bool hidden = true;

                        foreach (string key in e.Args.Keys)
                        {
                            string o1 = key;
                            object o2 = e.Args[key];
                            objfilter.FilterArgs.Add(ref o2, ref o1);
                        }

                        objfilter.OpenFilter(e.Login, e.FilterID, "", "", ref err, ref hidden);
                        e.RawFilter = objfilter;
                        for (int i = 1; i <= objfilter.FilterList.Count; i++)
                        {
                            object o = i as object;
                            UFGeneralFilter.FilterItem fi = objfilter.FilterList.get_Item(ref o);
                            FilterItem netfi = e.FltSrv[fi.Key];
                            if (netfi != null)
                            {
                                fi.varValue = netfi.Value1;
                                if (fi.RangeInput)
                                    fi.varValue2 = netfi.Value2;
                            }
                            else
                            {
                                fi.varValue = "";
                                if (fi.RangeInput)
                                    fi.varValue2 = "";
                            }
                        }
                        if (e.ClassName.Contains(","))
                        {
                            string[] name = e.ClassName.Split(',');
                            ObjectHandle oh = Activator.CreateInstance(name[1].Trim(), name[0].Trim());
                            //(oh.Unwrap() as IGetSql).GetSql(e);
                            object o = CreateDotNetDataSourceObject(name[1].Trim(), name[0].Trim());
                            (o as IGetSql).GetSql(e);
                        }
                        else
                        {
                            oParams = new object[] { e };
                            oComObj = Activator.CreateInstance(
                                Type.GetTypeFromProgID(e.ClassName.Trim()));
                            oComObj.GetType().InvokeMember("GetSql",
                                BindingFlags.InvokeMethod,
                                null,
                                oComObj,
                                oParams);
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw new ReportException(errheader + ex.InnerException.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new ReportException(errheader + ex.Message);
                    }
                    finally
                    {
                        if (oComObj != null)
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oComObj);
                        oParams = null;
                        if (e.Login != null)
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(e.Login);
                    }
                    if (!string.IsNullOrEmpty(e.ErrString))
                        throw new ReportException(e.ErrString);
                }


                watch.Stop();
                System.Diagnostics.Trace.WriteLine("Bs Get sql TimeUsed :" + watch.ElapsedMilliseconds);
                
            }
            catch (ReportException ee)
            {
                throw ee;
            }
            catch (Exception ee)
            {
                throw new ReportException(ee.Message);
            }
            return e;
        }
        #endregion

        #region cs get sql
        public void GetSql(FilterArgs e)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("Begin get sql :" + e.ClassName);
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                System.Diagnostics.Trace.WriteLine("watch start");
                if (e.ClassName != "")
                {
                    string filterstring = e.DataSource.FilterString;
                    object oComObj = null;
                    object[] oParams = null;
                    string errheader = "执行自定义数据源 " + e.ClassName + " 出错：";
                    try
                    {
                        if (e.ClassName.Contains(","))//.net
                        {
                            string[] name = e.ClassName.Split(',');
                            //ObjectHandle oh = Activator.CreateInstance(name[1].Trim(), name[0].Trim());                            
                            //(oh.Unwrap() as IGetSql).GetSql(e);
                            object o = CreateDotNetDataSourceObject(name[1].Trim(), name[0].Trim());
                            (o as IGetSql).GetSql(e);
                        }
                        else//vb
                        {
                            oParams = new object[] { e };
                            oComObj = Activator.CreateInstance(
                                Type.GetTypeFromProgID(e.ClassName.Trim()));
                            oComObj.GetType().InvokeMember("GetSql",
                                BindingFlags.InvokeMethod,
                                null,
                                oComObj,
                                oParams);

                            //try
                            //{
                            //    object groupfilter = oComObj.GetType().InvokeMember("GetViewFilter",
                            //    BindingFlags.InvokeMethod,
                            //    null,
                            //    oComObj,
                            //    null);
                            //    if (groupfilter != null && groupfilter.ToString() != "")
                            //        e.Args.Add("GroupFilter", groupfilter.ToString());
                            //}
                            //catch
                            //{
                            //}
                        }
                        this.ExendGetSql(e);

                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException != null)
                            throw new ReportException(errheader + ex.InnerException.Message);
                        else throw new ReportException(errheader + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new ReportException(errheader + ex.Message);
                    }
                    finally
                    {
                        if (oComObj != null)
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oComObj);
                        oParams = null;
                    }
                    if (!string.IsNullOrEmpty(e.ErrString))
                        throw new ReportException(e.ErrString);
                    e.DataSource.FilterString = filterstring;

                    if (e.DataSource.Type.ToString() == "TemplateTable" && e.DataSource.SQL.StartsWith("tempdb.."))
                    {
                        object result = SqlHelper.ExecuteScalar(ClientReportContext.Login.UfMetaCnnString,
                            string.Format("select top 1 name from tempdb..sysobjects where name='{0}' and xtype='U'", e.DataSource.SQL.Replace("tempdb..", "")));
                        if (result == null)
                            throw new ResourceReportException("U8.UAP.Services.RemoteDataHelper.NonTempTableCreated");
                    }
                }
                System.Diagnostics.Trace.WriteLine("end cs get sql , datasource type:" + e.DataSource.Type.ToString());

                watch.Stop();
                System.Diagnostics.Trace.WriteLine("Cs get sqlTimeUsed :" + watch.ElapsedMilliseconds);
            }
            catch (ReportException ee)
            {
                throw ee;
            }
            catch (Exception ee)
            {
                throw new ReportException(ee.Message);
            }
        }
        private void ExendGetSql(FilterArgs e)
        {
            try
            {
                //判断扩展表是否存在
                object tablename = SqlHelper.ExecuteScalar(ClientReportContext.Login.UfMetaCnnString,
                                "select top 1 name from sysobjects where name='UAP_Report_ExtendGetSql' and xtype='U'");
                if (tablename == null)
                    return;

                string filterstring = e.DataSource.FilterString;
                object oComObj = null;
                object[] oParams = null;
                string exendtClassname = string.Empty;
                if (string.IsNullOrEmpty(e.ReportID))
                    return;
                string GetClssName = string.Format("select extendclassname from UAP_Report_ExtendGetSql where reportid='{0}'", e.ReportID);
                object result = SqlHelper.ExecuteScalar(ClientReportContext.Login.UfMetaCnnString, GetClssName);
                if (result == null)
                    return;
                else
                    exendtClassname = result.ToString();
                if (exendtClassname.Contains(","))//.net
                {
                    string[] name = exendtClassname.Split(',');
                    object o = CreateDotNetDataSourceObject(name[1].Trim(), name[0].Trim());
                    (o as IGetSql).GetSql(e);
                }
                else//vb
                {
                    oParams = new object[] { e };
                    oComObj = Activator.CreateInstance(
                        Type.GetTypeFromProgID(exendtClassname.Trim()));
                    oComObj.GetType().InvokeMember("GetSql",
                        BindingFlags.InvokeMethod,
                        null,
                        oComObj,
                        oParams);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("执行ExendGetSql出错 :" + ex.Message);
            }

        }
        private object CreateDotNetDataSourceObject(string assemblyname, string typename)
        {
            try
            {
                string path = null;
                RegistryKey pRegKey = Registry.LocalMachine;
                pRegKey = pRegKey.OpenSubKey(@"SOFTWARE\Ufsoft\WF\V8.700\Install\CurrentInstPath");
                path = pRegKey.GetValue("").ToString();
                if (!path.EndsWith(@"\"))
                    path += @"\";
                Assembly dll = Assembly.LoadFile(path + @"UAP\" + assemblyname + @".dll");
                return ScriptHelper.FindInterface(dll, typename);
            }
            catch (Exception e)
            {
                throw new Exception("Create data source component err : " + e.Message);
            }
        }
        #endregion

        #region save levelexpand
        public void SaveLevelExpand(U8LoginInfor login, string viewid, LevelExpandSchema levelexpand)
        {
            try
            {
                ReportDataFacade rdf = new ReportDataFacade(login);
                rdf.SaveLevelExpend(viewid, levelexpand.ToString());
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }
        #endregion

        #region save groupschema 11.0 新增并发
        //public void SaveGroupSchema(U8LoginInfor login, string viewid, GroupSchemas groupschemas)
        //{
        //    try
        //    {
        //        ReportDataFacade rdf = new ReportDataFacade(login);
        //        rdf.SaveGroupSchemas(viewid, groupschemas.ToXml().InnerXml);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new ReportException(e.Message);
        //    }
        //}

        /// <summary>
        /// 11.0新增的并发情况下的保存操作
        /// </summary>
        /// <param name="login"></param>
        /// <param name="viewid"></param>
        /// <param name="groupschemas"></param>
        public void AddGroupSchema(U8LoginInfor login, string viewid, GroupSchema groupschema)
        {
            try
            {
                SetLocaleID(login.LocaleID);
                GroupSchemas groupschemas = new GroupSchemas();
                groupschemas.Add(groupschema);
                //ReportDataFacade rdf = new ReportDataFacade(login);
                groupschemas.SaveGroupSchemasWithLock(login, viewid, groupschemas.ToXml().InnerXml, "add");
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        public void ModifyGroupSchema(U8LoginInfor login, string viewid, GroupSchema groupschema)
        {
            try
            {
                SetLocaleID(login.LocaleID);
                GroupSchemas groupschemas = new GroupSchemas();
                groupschemas.Add(groupschema);
                //ReportDataFacade rdf = new ReportDataFacade(login);
                groupschemas.SaveGroupSchemasWithLock(login, viewid, groupschemas.ToXml().InnerXml, "modify");
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        /// <summary>
        /// 11.0新增的并发情况下的删除操作
        /// </summary>
        /// <param name="login"></param>
        /// <param name="viewid"></param>
        /// <param name="groupschemas"></param>
        public void DeleteGroupSchema(U8LoginInfor login, string viewid, GroupSchema groupschema)
        {
            try
            {
                SetLocaleID(login.LocaleID);
                GroupSchemas groupschemas = new GroupSchemas();
                groupschemas.Add(groupschema);
                //ReportDataFacade rdf = new ReportDataFacade(login);
                groupschemas.SaveGroupSchemasWithLock(login, viewid, groupschemas.ToXml().InnerXml, "delete");
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        #endregion

        #region save crossschema
        //public void SaveCrossSchemas(U8LoginInfor login, string viewid, GroupSchemas crossschemas)
        //{
        //    try
        //    {
        //        ReportDataFacade rdf = new ReportDataFacade(login);
        //        rdf.SaveCrossSchemas(viewid, crossschemas.ToXml().InnerXml);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new ReportException(e.Message);
        //    }
        //}
        /// <summary>
        /// 11.0新增的并发情况下的保存操作
        /// </summary>
        /// <param name="login"></param>
        /// <param name="viewid"></param>
        /// <param name="groupschemas"></param>
        public void AddCrossSchema(U8LoginInfor login, string viewid, GroupSchema crossSchema)
        {
            try
            {
                SetLocaleID(login.LocaleID);
                GroupSchemas crossschemas = new GroupSchemas();
                crossschemas.Add(crossSchema);
                //ReportDataFacade rdf = new ReportDataFacade(login);
                crossschemas.SaveCrossSchemasWithLock(login, viewid, crossschemas.ToXml().InnerXml, "add");
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        public void ModifyCrossSchema(U8LoginInfor login, string viewid, GroupSchema crossSchema)
        {
            try
            {
                SetLocaleID(login.LocaleID);
                GroupSchemas crossschemas = new GroupSchemas();
                crossschemas.Add(crossSchema);
                //ReportDataFacade rdf = new ReportDataFacade(login);
                crossschemas.SaveCrossSchemasWithLock(login, viewid, crossschemas.ToXml().InnerXml, "modify");
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        protected void SetLocaleID(string localeID)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(localeID);
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(localeID);
        }

        ///<summary>
        ///11.0新增的并发情况下的删除操作
        ///</summary>
        ///<param name="login"></param>
        ///<param name="viewid"></param>
        ///<param name="groupschemas"></param>
        public void DeleteCrossSchema(U8LoginInfor login, string viewid, GroupSchema crossSchema)
        {
            try
            {
                SetLocaleID(login.LocaleID);
                GroupSchemas crossschemas = new GroupSchemas();
                crossschemas.Add(crossSchema);
                //ReportDataFacade rdf = new ReportDataFacade(login);
                crossschemas.SaveCrossSchemasWithLock(login, viewid, crossschemas.ToXml().InnerXml, "delete");
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }
        #endregion

        #region save runtimeformat
        public void SaveRuntimeFormat(
            U8LoginInfor login,
            string viewId,
            string runtimeForamtXml,
            string colorstyleid)
        {
            try
            {
                ReportDataFacade rdf = new ReportDataFacade(login);
                rdf.SavaRuntimeFormat(viewId, runtimeForamtXml, colorstyleid);
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }
        public void UpdateReportMergeCell(string viewId, bool bMerge, string ufMetaCnnString)
        {
            try
            {
                string update = string.Format("update uap_reportview set reportmergecell={0} where id='{1}'", Convert.ToInt16(bMerge), viewId);
                SqlHelper.ExecuteNonQuery(
                 ufMetaCnnString,
                 update);
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        /// <summary>
        /// V12.5 matfb新增，为了保存自由视图下自动折行状态，使用reportmergecell字段保存自由视图下的这行状态。
        /// </summary>
        /// <param name="viewId"></param>
        /// <param name="bMerge"></param>
        /// <param name="ufMetaCnnString"></param>
        public void UpdateReportisFoldRow(string viewId, bool bMerge, string ufMetaCnnString)
        {
            try
            {
                string update = string.Format("update uap_reportview set reportmergecell={0} where id='{1}'", Convert.ToInt16(bMerge), viewId);
                SqlHelper.ExecuteNonQuery(
                 ufMetaCnnString,
                 update);
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        public void RemoveGroupStyle(
            string viewId,
            string groupId,
            string ufMetaCnnString)
        {
            DataSet ds = SqlHelper.ExecuteDataSet(
                ufMetaCnnString,
                string.Format(
                    "select RuntimeFormat from UAP_ReportView where Id=N'{0}'",
                    viewId));
            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
            string runtimeFormatXml = SqlHelper.GetStringFrom(dr[0]);
            if (!string.IsNullOrEmpty(runtimeFormatXml))
            {
                RuntimeFormat rf = new RuntimeFormat();
                rf.FromXml(runtimeFormatXml);
                rf.RemoveSubItem(groupId);
                SqlHelper.ExecuteNonQuery(
                    ufMetaCnnString,
                    string.Format(
                        "update UAP_ReportView set RuntimeFormat=N'{0}' where Id=N'{1}'",
                        rf.ToXml(),
                        viewId));
            }
        }
        #endregion

        #region Save As...

        /// <summary>
        /// 另存元数据(报表或视图)
        /// </summary>
        /// <param name="login">包装的login信息</param>
        /// <param name="sourceId">另存的源Id(报表id或视图Id)</param>
        /// <param name="savaAsName">另存的新名称</param>
        /// <param name="saveAsType">另存类型</param>
        /// <returns>另存成功,返回true;已存在要另存的名称,返回false</returns>
        public bool SaveAs(
            U8LoginInfor login,
            string sourceId,
            string savaAsName,
            string reportSubId,
            SaveAsType saveAsType,
            string runtimeForamtXml,
            string colorstyleid,
            string currentViewId)
        {
            try
            {
                ReportDataFacade rdf = new ReportDataFacade(login);
                return rdf.SaveAs(sourceId, savaAsName, reportSubId, saveAsType, runtimeForamtXml, colorstyleid, currentViewId);
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        #endregion

        #region Runtime item exists...

        public bool IsRunTimeItemExists(
            U8LoginInfor login,
            string id,
            RunItemType type)
        {
            try
            {
                ReportDataFacade rdf = new ReportDataFacade(login);
                return rdf.IsRunTimeItemExists(id, type);
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        #endregion

        #region dispose cache

        public void DisposeDbAndCache(string tempdbstring, string cacheid, SimpleArrayList tablenames, SimpleHashtable levels)
        {
            try
            {
                #region cache
                try
                {
                    string filename = DefaultConfigs.CachePath;
                    if (!filename.EndsWith(@"\"))
                        filename += @"\";
                    filename += cacheid + @"\";
                    if (System.IO.Directory.Exists(filename))
                        System.IO.Directory.Delete(filename, true);
                }
                catch
                {
                }
                #endregion

                #region db

                foreach (string tablename in tablenames)
                {
                    var logger = Logger.GetLogger("kkmeteor");
                    DropFromDB(tempdbstring,
                               AllString(tablename, levels.Contains(tablename) ? (int)levels[tablename] : 0, cacheid));
                    logger.Info("Drop TempTable : " + tablename + " At Time : " + DateTime.Now.ToString());
                    logger.Close();
                }

                #endregion
            }
            catch (Exception e)
            {
            }
        }

        public void DropFromDB(string tempdbstring, string sql)
        {
            try
            {
                if (!string.IsNullOrEmpty(sql))
                    SqlHelper.ExecuteNonQuery(tempdbstring, sql);
            }
            catch (Exception e)
            {
            }
        }

        public string IndexString(string basetable, string cacheid)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(basetable))
            {
                string indexname = TableNoneHeader(basetable, cacheid) + "_index";
                sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
                sb.Append(indexname);
                sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)");
                sb.Append(" drop table ");
                sb.Append(indexname);
            }
            return sb.ToString();
        }

        public string IndexAndLevelsString(string basetable, int levels, string cacheid)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(basetable))
            {
                sb.Append(IndexString(basetable, cacheid));
                for (int level = 1; level <= levels; level++)
                {
                    string levelname = TableNoneHeader(basetable, cacheid) + "_" + level.ToString();
                    sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
                    sb.Append(levelname);
                    sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)");
                    sb.Append(" drop table ");
                    sb.Append(levelname);
                }
            }
            return sb.ToString();
        }

        public string BaseString(string basetable, string cacheid)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(basetable))
            {
                string basename = TableNoneHeader(basetable, cacheid);
                sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
                sb.Append(basename);
                sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 and 11 = 11 )");
                sb.Append(" drop table ");
                sb.Append(basename);
                var loger = Logger.GetLogger("kkmeteor");
                loger.Info("\n Drop TempTable : " + basename + " At Time : " + DateTime.Now.ToString());
                loger.Close();
            }
            return sb.ToString();
        }

        public string BaseAndLevelsString(string basetable, int levels, string cacheid)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(basetable))
            {
                string basename = TableNoneHeader(basetable, cacheid);
                sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
                sb.Append(basename);
                sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)");
                sb.Append(" drop table ");
                sb.Append(basename);

                for (int level = 1; level <= levels; level++)
                {
                    string levelname = basename + "_" + level.ToString();
                    sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
                    sb.Append(levelname);
                    sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)");
                    sb.Append(" drop table ");
                    sb.Append(levelname);
                }
            }

            return sb.ToString();
        }

        public string ExpandTableAndBaseString(string basetable, string cacheid)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(basetable))
            {
                string basename = TableNoneHeader(basetable, cacheid);
                string expandname = basename + "_expand";
                sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
                sb.Append(expandname);
                sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)");
                sb.Append(" drop table ");
                sb.Append(expandname);

                sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
                sb.Append(basename);
                sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)");
                sb.Append(" drop table ");
                sb.Append(basename);
            }

            return sb.ToString();
        }

        public string AllString(string tablename, int levels, string cacheid)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ExpandTableAndBaseString(tablename, cacheid));
            sb.Append(IndexAndLevelsString(tablename, levels, cacheid));
            return sb.ToString();
        }

        public string TableNoneHeader(string tablename, string cacheid)
        {
            //string tmptable=tablename.Replace("UFReport..UFTmpTable", "tempdb..UFTmpTable" + cacheid.Replace("-", "_"));
            if (!tablename.Contains(".."))
                //tablename = tablename.Replace("UFTmpTable", "tempdb..UFTmpTable" + cacheid.Replace("-", "_"));
                tablename = tablename.Replace("UFTmpTable", "tempdb..UFTmpTable") + cacheid.Replace("-", "_");
            int index = tablename.IndexOf("..");
            if (index == -1)
                return tablename;
            else
                return tablename.Substring(index + 2);
        }
        #endregion

        #region GetBaseId
        public int GetBaseId(string basetable, string baseTableInTmp, string detailstring, int beginIndex, string cnnStr, QuickLocationItem item)
        {
            string strSql = "";
            string fldIdName = "index__id";
            string baseId = item.BaseId;
            string tableName = basetable;
            bool bDown = item.Direction == FindDirection.Down ? true : false;

            string sMatch = item.DbMatchSql;
            string sFilter = "";
            string cOrder = "";
            if (!item.PageByGroup)
            {
                if (item.ShowDetail)//显示明细
                {
                    tableName = "(select A." + baseId + "," + detailstring + ",b.index__id from " + basetable + " A inner join " + baseTableInTmp + "_index B on A." + baseId + "=B." + baseId + " ) C ";
                }
                else//不显示明细
                {
                    tableName = "(select A.*,b.index__id from " + baseTableInTmp + "_" + item.GroupLevel.ToString() + " A inner join " + baseTableInTmp + "_index B on A." + baseId + "=B." + baseId + " ) C";
                }
            }
            else
            {
                ArrayList al = item.TopGroupItem;
                StringBuilder sb = new StringBuilder();
                foreach (string key in al)
                {
                    if (sb.Length > 0)
                        sb.Append(" and ");
                    sb.Append("isnull(convert(nvarchar(500),A.[");//防止NULL和空值关联比较
                    sb.Append(key);
                    sb.Append("]),'')=isnull(convert(nvarchar(500),B.[");
                    sb.Append(key);
                    sb.Append("]),'')");
                }
                if (item.ShowDetail)//显示明细
                {
                    tableName = "(select A." + baseId + "," + detailstring + ",b.index__id from " + basetable + " A inner join " + baseTableInTmp + "_index B on " + sb.ToString() + " ) C ";
                }
                else//不显示明细
                {
                    tableName = "(select A.*,b.index__id from " + baseTableInTmp + "_" + item.GroupLevel.ToString() + " A inner join " + baseTableInTmp + "_index B on " + sb.ToString() + " ) C";
                }
            }
            sFilter = bDown == true ? fldIdName + ">= " + beginIndex.ToString() : fldIdName + "<= " + beginIndex.ToString();
            if (sMatch.Length > 0)
                sFilter += " and " + sMatch;
            cOrder = bDown == true ? " ASC " : " DESC ";
            cOrder = " Order by  " + fldIdName + cOrder;

            strSql = "select top 1 " + fldIdName + " from " + tableName + " where " + sFilter + cOrder;
            SqlDataReader dr = SqlHelper.ExecuteReader(cnnStr, strSql);
            if (dr.Read())//定位到数据
            {
                return Convert.ToInt32(dr.GetValue(0));
            }
            else
            {
                return -1; ;//没有定位到数据
            }
        }
        #endregion

        #region pagesettings
        public void SavePrintSetting(U8LoginInfor login, string viewID, bool blandScape, string pageMargins, string other, string pagesetting)
        {
            try
            {
                ReportDataFacade rdf = new ReportDataFacade(login);
                rdf.SavePrintSetting(viewID, blandScape, pageMargins, other, pagesetting);
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        public ComplexView LoadPrintSetting(U8LoginInfor login, string viewID)
        {
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(login.UfMetaCnnString, "select isnull(blandscape,0) as blandscape,isnull(pagemargins,'') as pagemargins ,isnull(columns,'') as columns,isnull(PageSetting,'') as PageSetting from uap_reportview where id='" + viewID + "'"))
                {
                    if (reader.Read())
                    {
                        ComplexView cv = new ComplexView();
                        cv.BlandScape = Convert.ToBoolean(reader["blandscape"]);
                        cv.PageMargins = reader["pagemargins"].ToString();
                        cv.Columns = reader["columns"].ToString();
                        cv.PageSetting = reader["PageSetting"].ToString();
                        return cv;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        public PrintOption LoadProviderSetting(U8LoginInfor login, string viewID)
        {
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(login.UfMetaCnnString, "select isnull(bshowdetail,0) as bshowdetail,isnull(bmustshowdetail,1) as bmustshowdetail from uap_reportview where id='" + viewID + "'"))
                {
                    if (reader.Read())
                    {
                        PrintOption po = new PrintOption();
                        po.CanSelectProvider = Convert.ToBoolean(reader["bmustshowdetail"]);
                        po.PrintProvider = Convert.ToBoolean(reader["bshowdetail"]) ? PrintProvider.U8PrintComponent : PrintProvider.UAPReportPrintComponent;
                        return po;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }

        public void SaveProviderSetting(U8LoginInfor login, string viewID, PrintOption po)
        {
            try
            {
                int bmustshowdetail = po.CanSelectProvider ? 1 : 0;
                int bshowdetail = po.PrintProvider == PrintProvider.U8PrintComponent ? 1 : 0;
                SqlHelper.ExecuteNonQuery(login.UfMetaCnnString, "update uap_reportview set bmustshowdetail=" + bmustshowdetail.ToString() + ",bshowdetail=" + bshowdetail.ToString() + " where id='" + viewID + "'");
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }
        #endregion

        #region GetDiagramData
        public System.Data.DataTable GetDiagramData(string groupTableName, string groupFlds, string groupSortString, string cnnStr)
        {
            //string strSql = "select "+groupFlds+" from " + baseTable + "_index group by " +groupFlds ;
            string strSql = "select " + groupFlds + " from " + groupTableName + " Order by " + groupSortString;
            return SqlHelper.ExecuteDataSet(cnnStr, strSql).Tables[0];

        }
        #endregion

        #region get data from baseid
        public RowData GetRowData(string cnnStr, string sql, string localeid)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(localeid);
            RowData rd = null;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(cnnStr, sql))
            {
                if (reader.Read())
                {
                    rd = new RowData(reader);
                    rd.BeforeReaderClosed();
                }
                reader.Close();
            }
            return rd;
        }
        #endregion

        #region chart data
        public System.Data.DataTable GetChartData(string cnnStr, string sql, string localeid)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(localeid);
            return SqlHelper.ExecuteDataSet(cnnStr, sql).Tables[0];
        }

        public string LoadChartStrings(string cnnStr, string viewid)
        {
            string sql = "select chartstring from uap_reportview where id='" + viewid + "'";
            return SqlHelper.ExecuteScalar(cnnStr, sql).ToString();
        }

        public void SaveChartStrings(string cnnStr, string viewid, string chartstrings)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "update uap_reportview set chartstring=@chartstring where id=@viewid";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@chartstring", chartstrings));
                cmd.Parameters.Add(new SqlParameter("@viewid", viewid));
                SqlHelper.ExecuteNonQuery(cnnStr, cmd);
            }
        }

        public void SaveAssemblyString(string cnnStr, string viewid, string assemblystring)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "update uap_reportview set assemblystring=@assemblystring where id=@viewid";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@assemblystring", assemblystring));
                cmd.Parameters.Add(new SqlParameter("@viewid", viewid));
                SqlHelper.ExecuteNonQuery(cnnStr, cmd);
            }
        }
        #endregion

        #region simpleviews
        /// <summary>
        /// 设置格式和另存视图之后刷新视图列表
        /// </summary>
        /// <param name="login">登录信息对象</param>
        /// <param name="reportId">报表标识</param>
        /// <returns>返回报表的视图信息</returns>
        public SimpleViewCollection GetSimpleViews(
            U8LoginInfor login,
            string reportId)
        {
            try
            {
                ReportDataFacade rdf = new ReportDataFacade(login);
                ReportDefinition rd = null;
                rdf.RetrieveReport(reportId, out rd);
                if (rd == null)
                {
                    Logger logger = Logger.GetLogger("ReportElement");
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("在刷新视图列表时,获得的报表元数据对象ReportDefinition为null");
                    sb.AppendLine("ReportId:" + reportId);
                    sb.AppendLine("UserId:" + login.UserID);
                    logger.Info(sb.ToString());
                    logger.Close();
                    return null;
                }
                return rd.SimpleViews;
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
        }
        #endregion

        #region monitor
        public string ServicingList()
        {
            return ParallelCenter.ServicingList();
        }

        public string WaitingList()
        {
            return ParallelCenter.WaitingList();
        }
        #endregion

        #region indicator report filter
        public DataTable GetIndicatorFilter(string filtersource, string basetable, string eventfilter, string cnnStr)
        {
            string strSql = "select distinct " + filtersource + " from " + basetable + (string.IsNullOrEmpty(eventfilter) ? "" : " where " + eventfilter);
            return SqlHelper.ExecuteDataSet(cnnStr, strSql).Tables[0];
        }
        #endregion

        #region outu8login
        public void AnalyzeUsertoken(string usertoken, string classname, ref string datastring, ref string metastring, ref string tempstring)
        {
            string[] classtocreate = classname.Split(',');
            object oh = Activator.CreateInstance(classtocreate[1], classtocreate[0]).Unwrap();
            Hashtable connstrings = oh.GetType().InvokeMember("GetConnectionString",
                        BindingFlags.InvokeMethod,
                        null,
                        oh,
                        new object[] { usertoken }) as Hashtable;

            datastring = connstrings["DataDBString"].ToString();
            tempstring = connstrings["TempDBString"].ToString();
            metastring = connstrings["MetaDBString"].ToString();
        }
        #endregion

        #region color settings
        public DataTable ColorStyleRetrieve(string id, string cnnstring)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_FontColorRetrieve");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ID", SqlDbType.NVarChar, 100, id));
            return SqlHelper.ExecuteDataSet(cnnstring, cmd).Tables[0];
        }

        public void ColorStyleModify(string id, string cs, string namelist, string localeidlist, string cnnstring)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_FontColorModify");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ID", SqlDbType.NVarChar, 100, id));
            cmd.Parameters.Add(SqlHelper.GetParameter("@StyleFormat", SqlDbType.NText, cs));
            cmd.Parameters.Add(SqlHelper.GetParameter("@NameList", SqlDbType.NVarChar, 4000, namelist));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleIdList", SqlDbType.NVarChar, 1000, localeidlist));
            SqlHelper.ExecuteNonQuery(cnnstring, cmd);
        }

        public void ColorStyleDelete(string id, string connstring)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_FontColorDelete");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ID", SqlDbType.NVarChar, 100, id));
            SqlHelper.ExecuteNonQuery(connstring, cmd);
        }
        #endregion

        #region ReportColorSet
        public void SaveReportColorSet(string id, string cnnstring, string reportColorSet)
        {
            //
            // get all format from db
            //
            string sql = string.Format("select Format from uap_reportview where id='{0}'", id);
            string format = (string)SqlHelper.ExecuteScalar(cnnstring, sql);
            //
            // replace reportcolorset to format
            //
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(format);
            doc.DocumentElement.SetAttribute("ReportColorSet", reportColorSet);
            //
            // save to db
            //
            SqlCommand commond = new SqlCommand();
            sql = "update uap_reportview set format = @Format where id = @Id";
            commond.CommandText = sql;
            SqlParameter[] param = new SqlParameter[]
            {
                new SqlParameter("@Format", doc.InnerXml),
                new SqlParameter("@Id", id)
            };
            commond.Parameters.AddRange(param);
            SqlHelper.ExecuteNonQuery(cnnstring, commond);

            //sql = string.Format("update uap_reportview set format = '{0}' where id='{1}'", doc.InnerXml, id);
            //SqlHelper.ExecuteNonQuery(cnnstring, sql);
        }
        #endregion

        #region ReportRelateInfo
        public ReportRelateInfor GetReportInfor(U8LoginInfor login, string reportid, ref string metastring, ref string datastring, ref string tempstring)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            try
            {
                if (string.IsNullOrEmpty(login.UfDataCnnString))
                {
                    System.Diagnostics.Trace.WriteLine("login.UfDataCnnString =" + login.UfDataCnnString);
                    login = new U8LoginInfor(GetLogin(login.UserToken, login.TaskID, login.SubID));
                }
                ReportDataFacade rdf = new ReportDataFacade(login);
                ReportRelateInfor rri = new ReportRelateInfor();
                watch.Stop();
                System.Diagnostics.Trace.WriteLine("GetReportInfor 1 use time :" + watch.ElapsedMilliseconds);
                watch.Start();
                rdf.GetReportDatasByReportID(reportid, rri);
                watch.Stop();
                System.Diagnostics.Trace.WriteLine("GetReportInfor 2 use time :" + watch.ElapsedMilliseconds);
                watch.Start();
                metastring = login.UfMetaCnnString;
                datastring = login.UfDataCnnString;
                tempstring = login.TempDBCnnString;

                FilterXmlService fxs = new FilterXmlService(login, reportid, login.LocaleID);
                rri.FilterXML = fxs.GetFilterConditionXml();
                watch.Stop();
                System.Diagnostics.Trace.WriteLine("GetReportInfor 3 use time :" + watch.ElapsedMilliseconds);
                watch.Start();

                return rri;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("catch (Exception ex)" + ex.Message);
                throw new ReportException(ex.Message);
                //return null;
            }
        }

        public ReportRelateInfor GetReportInforOutU8(U8LoginInfor login, string reportid, ref string metastring, ref string datastring, ref string tempstring, string filterstring, string classname)
        {
            try
            {
                //object oh = Activator.CreateInstance("U8DRP_LoginInfo", "UFIDA.U8.DRP.Lib.Business.U8DRP_LoginInfo.LoginInfoC").Unwrap();
                //_login.UfDataCnnString = datastring;
                //_login.TempDBCnnString = tempstring = datastring.Replace("Initial Catalog=U8DRP_Template", "Initial Catalog=tempdb");
                //_login.UfMetaCnnString = metastring = datastring.Replace("Initial Catalog=U8DRP_Template", "Initial Catalog=ReportMeta");

                AnalyzeUsertoken(login.UserToken, classname, ref datastring, ref metastring, ref tempstring);

                login.UfDataCnnString = datastring;
                login.TempDBCnnString = tempstring;
                login.UfMetaCnnString = metastring;

                ReportDataFacade rdf = new ReportDataFacade(login);
                ReportRelateInfor rri = new ReportRelateInfor();
                rri.FilterXML = filterstring;
                rri.ClassName = classname;
                rdf.GetReportDatasByReportIDOutU8(reportid, rri);
                return rri;
            }
            catch (Exception ex)
            {
                new ReportException(ex.Message);
                return null;
            }
        }
        #endregion

        #region 报表用户状态相关的一些信息

        public void SaveReportUserState(U8LoginInfor login, ReportUIUserStateInfo uiState)
        {
            ReportDataFacade rdf = new ReportDataFacade(login);
            Hashtable uiSatateHt = new Hashtable();
            uiSatateHt.Add("userId", uiState.UserId);
            uiSatateHt.Add("ReportId", uiState.ReportId);
            uiSatateHt.Add("PrintSet", (int)uiState.PrintSet);
            uiSatateHt.Add("printDefault", (int)uiState.PrintDefault);
            uiSatateHt.Add("FilterInfo", (int)uiState.FilterInfo);
            //int extendStatus = ToExtendState(uiState);
            //uiSatateHt.Add("ExtendState", extendStatus);

            uiSatateHt.Add("FilterFlag", (int)uiState.FilterFlag);
            uiSatateHt.Add("SolutionVisible", uiState.SolutionVisible ? 1 : 0);
            uiSatateHt.Add("ConditionVisible", uiState.ConditionVisible ? 1 : 0);
            uiSatateHt.Add("QuickFilterLayoutFlag", (int)uiState.QuickFilterLayoutFlag);
            uiSatateHt.Add("LayoutIsSetted", uiState.LayoutIsSetted ? 1 : 0);
            uiSatateHt.Add("IncludeBackColor", uiState.IncludeBackColor ? 1 : 0);
            uiSatateHt.Add("Layout", (int)uiState.Layout);
            //uiSatateHt.Add("ShowChart", uiState.BShowChart?1:0);
            rdf.SaveReportUserStateNew(uiSatateHt);
        }

        public ReportUIUserStateInfo GetReportUserState(U8LoginInfor login, string reportId)
        {
            ReportDataFacade rdf = new ReportDataFacade(login);
            Hashtable uiSatateHt = rdf.GetReportUserStateNew(login.UserID, reportId);
            ReportUIUserStateInfo uiState = new ReportUIUserStateInfo();
            uiState.UserId = uiSatateHt["userId"].ToString();
            uiState.ReportId = uiSatateHt["ReportId"].ToString();
            uiState.PrintSet = (PrintSetings)(Convert.ToInt32(uiSatateHt["PrintSet"]));
            uiState.PrintDefault = (PrintDefaults)(Convert.ToInt32(uiSatateHt["printDefault"]));
            uiState.FilterInfo = (FilterInfos)(Convert.ToInt32(uiSatateHt["FilterInfo"]));
            if (uiSatateHt["FilterFlag"] == null)
                uiState.FilterFlag = FilterType.QuickFilter;
            else
                uiState.FilterFlag = (FilterType)Convert.ToInt32(uiSatateHt["FilterFlag"]);
            if (uiSatateHt["SolutionVisible"] == null)
            {
                uiState.SolutionVisible = true;
            }
            else
            {
                uiState.SolutionVisible = Convert.ToInt32(uiSatateHt["SolutionVisible"]) == 1 ? true : false;
            }
            uiState.ConditionVisible = Convert.ToInt32(uiSatateHt["ConditionVisible"]) == 1 ? true : false;
            uiState.QuickFilterLayoutFlag = (QuickFilterLayout)Convert.ToInt32(uiSatateHt["QuickFilterLayoutFlag"]);
            uiState.LayoutIsSetted = Convert.ToInt32(uiSatateHt["LayoutIsSetted"]) == 1 ? true : false;

            uiState.IncludeBackColor = (Convert.ToInt32(uiSatateHt["IncludeBackColor"]) == 1) ? true : false;
            uiState.Layout = (Layouts)(Convert.ToInt32(uiSatateHt["Layout"]));
            //if (uiSatateHt["ShowChart"] == null)
            //{
            //    uiState.BShowChart = false;
            //}
            //else
            //    uiState.BShowChart = Convert.ToInt32(uiSatateHt["ShowChart"]) == 1 ? true : false; ;

            //扩展信息
            //if (uiSatateHt["ExtendState"] != null)
            //{
            //    uiState.ExtendStatus = (int)uiSatateHt["ExtendState"];

            //    Hashtable htStatus = FromExtendState(uiState.ExtendStatus);

            //    uiState.FilterFlag = (FilterType)Convert.ToInt32(htStatus["FilterFlag"]);
            //    uiState.SolutionVisible = Convert.ToInt32(htStatus["SolutionVisible"]) == 1 ? true : false;
            //    uiState.ConditionVisible = Convert.ToInt32(htStatus["ConditionVisible"]) == 1 ? true : false;
            //    uiState.QuickFilterLayoutFlag = (QuickFilterLayout)Convert.ToInt32(htStatus["QuickFilterLayoutFlag"]);
            //    uiState.LayoutIsSetted = Convert.ToInt32(htStatus["LayoutIsSetted"]) == 1 ? true : false;
            //}


            return uiState;
        }

        private int ToExtendState(ReportUIUserStateInfo objStatus)
        {
            string strExtendStatus = Convert.ToString(objStatus.ExtendStatus, 2);
            while (strExtendStatus.Length < 15)
            {
                strExtendStatus = "0" + strExtendStatus;
            }
            string[] status = new string[strExtendStatus.Length];

            for (int i = 0; i < strExtendStatus.Length; i++)
            {
                status[i] = strExtendStatus.Substring(i, 1);
            }
            status[0] = Convert.ToInt32(objStatus.FilterFlag).ToString();
            status[1] = objStatus.SolutionVisible ? "1" : "0";
            status[2] = objStatus.ConditionVisible ? "1" : "0";
            status[3] = Convert.ToInt32(objStatus.QuickFilterLayoutFlag).ToString();
            status[4] = objStatus.LayoutIsSetted ? "1" : "0";

            string st = "";
            for (int i = 0; i < status.Length; i++)
            {
                st += status[i];
            }
            return Convert.ToInt32(st, 2);
        }
        private Hashtable FromExtendState(int extendStatus)
        {
            Hashtable htStatus = new Hashtable();

            //extendStatus转换为二进制一共15位
            //前4位，分别是 显示固定还是快捷条件/显示方案/显示条件/左右还是上下
            string strExtendStatus = Convert.ToString(extendStatus, 2);
            while (strExtendStatus.Length < 15)
            {
                strExtendStatus = "0" + strExtendStatus;
            }

            htStatus.Add("FilterFlag", strExtendStatus.Substring(0, 1));
            htStatus.Add("SolutionVisible", strExtendStatus.Substring(1, 1));
            htStatus.Add("ConditionVisible", strExtendStatus.Substring(2, 1));
            htStatus.Add("QuickFilterLayoutFlag", strExtendStatus.Substring(3, 1));
            htStatus.Add("LayoutIsSetted", strExtendStatus.Substring(4, 1));
            return htStatus;
        }

        #endregion

        /// <summary>
        /// 根据过滤Id获取方案ID和名称
        /// </summary>
        /// <param name="login"></param>
        /// <param name="filterId"></param>
        /// <returns></returns>
        public Hashtable GetSolutionListByFilterId(U8LoginInfor login, string filterId)
        {
            string sql = string.Empty;
            //if (ClientReportContext.Login.U8LoginClass.IsAdmin)
            //{
            //    sql = string.Format("select ID,SolutionName from flt_Solution where FilterID='{0}' and LocaleID='{1}'", filterId, login.LocaleID);
            //}
            //else 
            sql = string.Format("select ID,SolutionName from flt_Solution where FilterID='{0}' and LocaleID='{1}' and ispublic='1' union " +
                                "select ID,SolutionName from flt_Solution where FilterID='{0}' and LocaleID='{1}' and ispublic='0' and userid='{2}' ", filterId, login.LocaleID, ClientReportContext.Login.UserID);
            DataTable dt = SqlHelper.ExecuteDataSet(login.UfDataCnnString, sql).Tables[0];
            Hashtable ht = new Hashtable();
            for (int row = 0; row < dt.Rows.Count; row++)
            {
                DataRow dr = dt.Rows[row];
                ht.Add(SqlHelper.GetStringFrom(dr["ID"], string.Empty), SqlHelper.GetStringFrom(dr["SolutionName"], string.Empty));
            }
            return ht;
        }


    }
}
