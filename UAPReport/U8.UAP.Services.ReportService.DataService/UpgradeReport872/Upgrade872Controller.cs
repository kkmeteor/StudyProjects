/*
 * 作者:卢达其
 * 时间:2008.3.13
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
    /// <summary>
    /// 872升级的流程为:
    /// 1.所有打开861报表的操作都被转移到870报表系统中
    /// 2.870报表系统将在其打开报表控制流进行到获取报表元数据阶段时，
    /// 转移进行以下升级处理：
    ///	  A.报表数据源升级
    ///   B.报表格式升级
    ///   C.报表权限升级
    /// 3.升级完毕后，将控制流重新接回原来的控制流继续后续处理
    /// </summary>
    public class Upgrade872Controller
    {
        public const string InfoKeyLoginInfo = "InfoKeyLoginInfo";
        public const string InfoKeyReportName861 = "InfoKeyReportName861";
        public const string InfoKeyReportSubId = "InfoKeyReportSubId";

        private U8LoginInfor _loginInfor = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loginInfor">登录信息，主要为获取连接串</param>
        public Upgrade872Controller(U8LoginInfor loginInfor)
        {
            if (loginInfor == null
                || string.IsNullOrEmpty(loginInfor.UfDataCnnString)
                || string.IsNullOrEmpty(loginInfor.UfMetaCnnString))
                throw new Exception("loginInfor或其中的连接串为空,不能进行升级");
            this._loginInfor = loginInfor;
        }

        /// <summary>
        /// 判断是否是861报表
        /// </summary>
        /// <param name="id">
        /// 报表id:如果是861报表，此id必然是类似以下形式，
        /// SA[__]销售统计表
        /// </param>
        /// <remarks>
        /// 判断规则：由id拆分得SystemID="SA"和Name="销售统计表",然后查看
        /// SELECT bNewRpt FROM UFDATA_001_2008..Rpt_GlbDef_Base
        /// WHERE SystemID='SA' AND Name='销售统计表'
        /// bNewRpt=1 ->870报表
        /// bNewRpt=0 ->861报表
        /// </remarks>
        public int Is861Report(string id)
        {
            string mode = "SA[__]a";
            if (string.IsNullOrEmpty(id)
                || id.IndexOf("[__]") == -1
                || id.Length < mode.Length)
                return 0;
            string[] elements = this.GetSubIdAndReportName(id);
            if (elements.Length != 2)
                return 0;
            string sql = string.Format(
                @"select isnull(bHadUpgradedFrom861,1)as bHadUpgradedFrom861,NeedUpdateDataSource from uap_report where id ='{0}'",
                id);
            DataTable dt = SqlHelper.ExecuteDataSet(this._loginInfor.UfMetaCnnString, sql).Tables[0];
            if (dt.Rows.Count == 0)
                return 1;

            if (!SqlHelper.GetBooleanFrom(dt.Rows[0]["bHadUpgradedFrom861"], false))
                return 1;
            if (Convert.ToInt32(dt.Rows[0]["NeedUpdateDataSource"]) == 1)
                return 2;

            return 0;
            //return !SqlHelper.GetBooleanFrom(bhadupgraded, false );
        }
        public int IsNeedFactoryBak(string id)
        {
            string mode = "SA[__]a";
            if (string.IsNullOrEmpty(id)
                || id.IndexOf("[__]") == -1
                || id.Length < mode.Length)
                return 0;
            string[] elements = this.GetSubIdAndReportName(id);
            if (elements.Length != 2)
                return 0;

            string sql = string.Format(
                @"select isnull(IsNeedBak,1)as IsNeedBak from uap_report where id='{0}' ",
                id);
            DataTable dt = SqlHelper.ExecuteDataSet(this._loginInfor.UfMetaCnnString, sql).Tables[0];
            bool isneedbak = Convert.ToInt32(dt.Rows[0]["IsNeedBak"]) == 1;

            sql = string.Format(
                @"select top 1 * from rpt_glbdef_base where SystemID + '[__]' + Name='{0}' and  localeid='{1}'",
                id,
                this._loginInfor.LocaleID);               
            dt = SqlHelper.ExecuteDataSet(this._loginInfor.UfDataCnnString, sql).Tables[0];
            bool isoldreport=dt.Rows.Count == 1;
            if (isneedbak && isoldreport)
                return 1;
            else
            {
                if (isoldreport)
                {
                    sql = string.Format(
                @"select top 1 * from uap_reportview_factorybak where id='{0}' ",
                id);
                    dt = SqlHelper.ExecuteDataSet(this._loginInfor.UfMetaCnnString, sql).Tables[0];
                    if (dt.Rows.Count == 0)
                        return 1;
                }
            }
            return 0;

            /*
            string sql = string.Format(
                @"select isnull(IsNeedBak,1)as IsNeedBak from uap_report where id='{0}' ",
                id);
            DataTable dt = SqlHelper.ExecuteDataSet(this._loginInfor.UfMetaCnnString, sql).Tables[0];
            if (Convert.ToInt32(dt.Rows[0]["IsNeedBak"]) == 0)
                return 0;
            else 
            {
                sql = string.Format(
                @"select top 1 * from rpt_glbdef_base where SystemID + '[__]' + Name='{0}' and  localeid='{1}'",
                id,
                this._loginInfor.LocaleID);               
                dt = SqlHelper.ExecuteDataSet(this._loginInfor.UfDataCnnString, sql).Tables[0];
                if (dt.Rows.Count == 1)
                    return 1;
                return 0;
            }
             * */
           
        }
        public int IsNeedExpandLanguage(string id)
        {
            string mode = "SA[__]a";
            if (string.IsNullOrEmpty(id)
                || id.IndexOf("[__]") == -1
                || id.Length < mode.Length)
                return 0;
            string[] elements = this.GetSubIdAndReportName(id);
            if (elements.Length != 2)
                return 0;

            string sql = string.Format(
                @"select isnull(NeedExpand,1)as NeedExpand from uap_report_lang where reportid='{0}' and localeid='{1}'  ",
                id,
                this._loginInfor.LocaleID);
            DataTable dt = SqlHelper.ExecuteDataSet(this._loginInfor.UfMetaCnnString, sql).Tables[0];
            if (Convert.ToInt32(dt.Rows[0]["NeedExpand"]) == 1)
                return 1;
            return 0;
        }
        private string[] GetSubIdAndReportName(string reportId870)
        {
            string[] separators = new string[] { "[__]" };
            return reportId870.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 升级861报表
        /// </summary>
        public void Upgrade(string id)
        {
            try
            {
                string[] elements = this.GetSubIdAndReportName(id);
                UpgradeReportMetaWrapper urmw = new UpgradeReportMetaWrapper();
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportId, id);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportName861, elements[1]);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportSubId, elements[0]);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBAppServer, this._loginInfor.AppServer);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBUfMetaConnString, this._loginInfor.UfMetaCnnString);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcAccId, this._loginInfor.cAccId);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcYear, this._loginInfor.cYear);

                // 升级数据源,格式....
                Hashtable infos = new Hashtable();
                infos[Upgrade872Controller.InfoKeyLoginInfo] = this._loginInfor;
                infos[Upgrade872Controller.InfoKeyReportSubId] = elements[0];
                infos[Upgrade872Controller.InfoKeyReportName861] = elements[1];

                UpgradeFormatService ufs = new UpgradeFormatService();
                ufs.DeliverEnvironmentInfos(infos);
                ufs.SetMeta(urmw);

                UpgradeReport ur = urmw.WrapData2Object();
                ur.Save();             
            }
            catch (Exception e)
            {
                Logger logger = Logger.GetLogger("Report872UpradeError");
                logger.Error(e);
                throw e;
            }
            finally
            {
                GC.Collect();
            }
        }

        public void UpgradeBO(string id)
        {
            try
            {
                string[] elements = this.GetSubIdAndReportName(id);
                UpgradeReportMetaWrapper urmw = new UpgradeReportMetaWrapper();
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportId, id);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportName861, elements[1]);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportSubId, elements[0]);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBAppServer, this._loginInfor.AppServer);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBUfMetaConnString, this._loginInfor.UfMetaCnnString);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcAccId, this._loginInfor.cAccId);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcYear, this._loginInfor.cYear);

                // 升级数据源,格式....
                Hashtable infos = new Hashtable();
                infos[Upgrade872Controller.InfoKeyLoginInfo] = this._loginInfor;
                infos[Upgrade872Controller.InfoKeyReportSubId] = elements[0];
                infos[Upgrade872Controller.InfoKeyReportName861] = elements[1];

                UpgradeFormatService ufs = new UpgradeFormatService();
                ufs.DeliverEnvironmentInfos(infos);
                ufs.SetMeta(urmw);

                UpgradeReport ur = urmw.WrapData2Object();
                ur.SaveBO();
            }
            catch (Exception e)
            {
                Logger logger = Logger.GetLogger("Report872UpradeError");
                logger.Error(e);
                throw e;
            }
            finally
            {
                GC.Collect();
            }
        }
        public void UpgradeFactoryBak(string id)
        {
            try
            {
                string[] elements = this.GetSubIdAndReportName(id);
                UpgradeReportMetaWrapper urmw = new UpgradeReportMetaWrapper();
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportId, id);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportName861, elements[1]);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportSubId, elements[0]);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBAppServer, this._loginInfor.AppServer);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBUfMetaConnString, this._loginInfor.UfMetaCnnString);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcAccId, this._loginInfor.cAccId);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcYear, this._loginInfor.cYear);

                // 升级数据源,格式....
                Hashtable infos = new Hashtable();
                infos[Upgrade872Controller.InfoKeyLoginInfo] = this._loginInfor;
                infos[Upgrade872Controller.InfoKeyReportSubId] = elements[0];
                infos[Upgrade872Controller.InfoKeyReportName861] = elements[1];

                UpgradeFormatService ufs = new UpgradeFormatService();
                ufs.DeliverEnvironmentInfos(infos);              

                //升级出厂设置的视图数据
                ufs.SetMeta(urmw, true);
                UpgradeReport ur = urmw.WrapData2Object();
                ur.SaveFactoryView();
            }
            catch (Exception e)
            {
                Logger logger = Logger.GetLogger("Report872UpradeError");
                logger.Error(e);
                throw e;
            }
            finally
            {
                GC.Collect();
            }
        }
        public void UpgradeLang(string id)
        {
            try
            {
                string[] elements = this.GetSubIdAndReportName(id);
                UpgradeReportMetaWrapper urmw = new UpgradeReportMetaWrapper();
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportId, id);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportName861, elements[1]);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportSubId, elements[0]);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBAppServer, this._loginInfor.AppServer);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBUfMetaConnString, this._loginInfor.UfMetaCnnString);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcAccId, this._loginInfor.cAccId);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcYear, this._loginInfor.cYear);

                // 升级数据源,格式....
                Hashtable infos = new Hashtable();
                infos[Upgrade872Controller.InfoKeyLoginInfo] = this._loginInfor;
                infos[Upgrade872Controller.InfoKeyReportSubId] = elements[0];
                infos[Upgrade872Controller.InfoKeyReportName861] = elements[1];

                UpgradeFormatService ufs = new UpgradeFormatService();
                ufs.DeliverEnvironmentInfos(infos);
                ufs.SetMeta(urmw);

                UpgradeReport ur = urmw.WrapData2Object();
                ur.SaveLang(this._loginInfor.LocaleID);
            }
            catch (Exception e)
            {
                Logger logger = Logger.GetLogger("Report872UpradeError");
                logger.Error(e);
                throw e;
            }
            finally
            {
                GC.Collect();
            }
        }
    }

    public class Upgrade872ControllerOutU8
    {
        public const string InfoKeyLoginInfo = "InfoKeyLoginInfo";
        public const string InfoKeyReportName861 = "InfoKeyReportName861";
        public const string InfoKeyReportSubId = "InfoKeyReportSubId";

        private U8LoginInfor _loginInfor = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loginInfor">登录信息，主要为获取连接串</param>
        public Upgrade872ControllerOutU8(U8LoginInfor loginInfor)
        {
            if (loginInfor == null
                || string.IsNullOrEmpty(loginInfor.UfDataCnnString)
                || string.IsNullOrEmpty(loginInfor.UfMetaCnnString))
                throw new Exception("loginInfor或其中的连接串为空,不能进行升级");
            this._loginInfor = loginInfor;
        }

        public void Upgrade(string id, string filterstring, string classname)
        {
            try
            {
                UpgradeReportMetaWrapper urmw = new UpgradeReportMetaWrapper();
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportId, id);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportName861, id);
                urmw.SetArgument(UpgradeReportMetaWrapper.ReportSubId, _loginInfor.SubID);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBAppServer, this._loginInfor.AppServer);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBUfMetaConnString, this._loginInfor.UfMetaCnnString);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcAccId, this._loginInfor.cAccId);
                urmw.SetArgument(UpgradeReportMetaWrapper.DBcYear, this._loginInfor.cYear);

                // 升级数据源,格式....
                Hashtable infos = new Hashtable();
                infos[Upgrade872Controller.InfoKeyLoginInfo] = this._loginInfor;
                infos[Upgrade872Controller.InfoKeyReportSubId] = _loginInfor.SubID;
                infos[Upgrade872Controller.InfoKeyReportName861] = id;

                UpgradeFormatServiceOutU8 ufs = new UpgradeFormatServiceOutU8();
                ufs.DeliverEnvironmentInfos(infos);
                ufs.SetMeta(urmw, filterstring, classname);

                UpgradeReport ur = urmw.WrapData2Object();
                ur.Save();
            }
            catch (Exception e)
            {
                Logger logger = Logger.GetLogger("Report872UpradeError");
                logger.Error(e);
                throw e;
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
