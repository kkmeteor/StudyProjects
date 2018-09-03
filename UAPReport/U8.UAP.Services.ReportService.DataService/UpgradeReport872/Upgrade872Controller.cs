/*
 * ����:¬����
 * ʱ��:2008.3.13
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
    /// 872����������Ϊ:
    /// 1.���д�861����Ĳ�������ת�Ƶ�870����ϵͳ��
    /// 2.870����ϵͳ������򿪱�����������е���ȡ����Ԫ���ݽ׶�ʱ��
    /// ת�ƽ���������������
    ///	  A.��������Դ����
    ///   B.�����ʽ����
    ///   C.����Ȩ������
    /// 3.������Ϻ󣬽����������½ӻ�ԭ���Ŀ�����������������
    /// </summary>
    public class Upgrade872Controller
    {
        public const string InfoKeyLoginInfo = "InfoKeyLoginInfo";
        public const string InfoKeyReportName861 = "InfoKeyReportName861";
        public const string InfoKeyReportSubId = "InfoKeyReportSubId";

        private U8LoginInfor _loginInfor = null;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="loginInfor">��¼��Ϣ����ҪΪ��ȡ���Ӵ�</param>
        public Upgrade872Controller(U8LoginInfor loginInfor)
        {
            if (loginInfor == null
                || string.IsNullOrEmpty(loginInfor.UfDataCnnString)
                || string.IsNullOrEmpty(loginInfor.UfMetaCnnString))
                throw new Exception("loginInfor�����е����Ӵ�Ϊ��,���ܽ�������");
            this._loginInfor = loginInfor;
        }

        /// <summary>
        /// �ж��Ƿ���861����
        /// </summary>
        /// <param name="id">
        /// ����id:�����861������id��Ȼ������������ʽ��
        /// SA[__]����ͳ�Ʊ�
        /// </param>
        /// <remarks>
        /// �жϹ�����id��ֵ�SystemID="SA"��Name="����ͳ�Ʊ�",Ȼ��鿴
        /// SELECT bNewRpt FROM UFDATA_001_2008..Rpt_GlbDef_Base
        /// WHERE SystemID='SA' AND Name='����ͳ�Ʊ�'
        /// bNewRpt=1 ->870����
        /// bNewRpt=0 ->861����
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
        /// ����861����
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

                // ��������Դ,��ʽ....
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

                // ��������Դ,��ʽ....
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

                // ��������Դ,��ʽ....
                Hashtable infos = new Hashtable();
                infos[Upgrade872Controller.InfoKeyLoginInfo] = this._loginInfor;
                infos[Upgrade872Controller.InfoKeyReportSubId] = elements[0];
                infos[Upgrade872Controller.InfoKeyReportName861] = elements[1];

                UpgradeFormatService ufs = new UpgradeFormatService();
                ufs.DeliverEnvironmentInfos(infos);              

                //�����������õ���ͼ����
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

                // ��������Դ,��ʽ....
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
        /// ���캯��
        /// </summary>
        /// <param name="loginInfor">��¼��Ϣ����ҪΪ��ȡ���Ӵ�</param>
        public Upgrade872ControllerOutU8(U8LoginInfor loginInfor)
        {
            if (loginInfor == null
                || string.IsNullOrEmpty(loginInfor.UfDataCnnString)
                || string.IsNullOrEmpty(loginInfor.UfMetaCnnString))
                throw new Exception("loginInfor�����е����Ӵ�Ϊ��,���ܽ�������");
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

                // ��������Դ,��ʽ....
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
