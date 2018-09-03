/*
 * 作者:卢达其
 * 时间:2009.1.13
 * 
 * 890重构“报表中心”:
 * 1.提高加载速度
 * 2.剔除不必要的功能
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;
using Microsoft.Win32;

namespace UFIDA.U8.UAP.Services.ReportData
{
    /// <summary>
    /// “报表中心”的数据层:
    /// 考虑到使用的所有数据大小存在一个常数上限，
    /// 并且上限占用内存在可接受的范围内，所以一次
    /// 取回所有的数据，在内存中做逻辑处理
    /// </summary>
    public class ReportCenterDataService
    {
        public const string KEY_UAP_ReportMyReport = "UAP_ReportMyReport";
        public const string KEY_ReportAuth = "KEY_ReportAuth";
        public const string KEY_SystemInfo = "UA_SubSys_Base";
        public const string KEY_ReportMata = "KEY_ReportMata";
        public const string KEY_BusinessRule1 = "KEY_BusinessRule1";
        public const string KEY_BusinessRule2 = "KEY_BusinessRule2";
        public const string KEY_U861FormatRelated = "KEY_U861FormatRelated";
        public const string KEY_IsUsingU861Format = "KEY_IsUsingU861Format";
        public const string KEY_MyPublish = "我的发布任务";
        public const string KEY_MyAcception = "我的接收任务";
        public const string KEY_StaticReport = "静态报表";
        public const string KEY_ReportCenterCatalog = "报表分类"; //报表中心分类信息

        public const string KeyExpandedNodes = "ExtendedNodes";
        public const string KeyDeletedStaticReports = "DeletedStaticReports";
        public const string KeyCustomNodes = "CustomNodes";
        public const string KeyCustomStaticReportNames = "CustomStaticReportNames";
        public const string KeyFindHistory = "FindHistory";
        public const string KeyAllFinalNodes = "AllFinalNodes";
        public const string KeySelectedNode = "SelectedNode";
        public const string KeyNodesPostion = "NodesPostion";
        public const string KeyAll = "AllReports";
        public const string KeySystem = "系统报表";
        public const string KeyCustom = "自定义报表";
        public const string KeyReportCenterState = "ReportCenterState";

        private U8LoginInfor _loginInfo = null;
        private Hashtable _meta = new Hashtable();
        internal const string REGKEYPATH = @"SOFTWARE\Ufsoft\WF\V8.700\Install\Installed";
        public U8LoginInfor LogInfo
        {
            get { return this._loginInfo; }
            set { this._loginInfo = value; }
        }

        public Hashtable GetData()
        {
            return this.GetData(false, false);
        }

        public Hashtable GetData(bool is4Web, bool is4Ref)
		{
			// 参考存储过程UAP_Report_ReportCenterGetData,返回数据的顺序为:
			//-- 1.报表中心树状态
			//-- 2.3.报表权限信息
			//-- 4.系统信息:是否启用，名称等
			//-- 5.报表元数据
			//-- 6.业务启用1
			//-- 7.业务启用2
			//-- 8.我的发布任务
			//-- 9.我的接收任务
			//-- 10.静态报表
			//-- 11.U861老报表相关
			int index = 0;
			DataSet ds = this.GetDataFromDB(is4Web,is4Ref);
			
			//1
			this._meta[KEY_UAP_ReportMyReport] = ds.Tables[index++];
			
			//2,3
			Hashtable ht = this.GetDataMap(index++, new string[]{"ReportGuid", ""}, ds);
			this._meta[KEY_ReportAuth] = this.GetDataMap(index++, new string[]{"ReportGuid", ""}, ds, ht);
			
			//4
			this._meta[KEY_SystemInfo] = this.GetDataMap(index++, new string[]{"cSub_Id", "cSub_Name"}, ds);
            //11.0增加crm模块
            Hashtable sysht = this._meta[KEY_SystemInfo] as Hashtable;
            if (sysht.ContainsKey("MK") || sysht.ContainsKey("CS"))
            {
                if (!sysht.ContainsKey("CB"))
                {
                    sysht.Add("CB", "CRM");
                }
                else
                {
                    sysht["CB"]= "CRM";
                }
                //this._meta[KEY_SystemInfo] = sysht;
                try
                {
                    this._meta[KEY_SystemInfo] = HandleLocalInstall(sysht);
                }
                catch
                {
                    this._meta[KEY_SystemInfo] = sysht;
                }
            }

			//5,6,7
			this._meta[KEY_ReportMata] = ds.Tables[index++];
			this._meta[KEY_BusinessRule1] = ds.Tables[index++];
			this._meta[KEY_BusinessRule2] = ds.Tables[index++];

			//8,9,10
			this._meta[KEY_MyPublish] = ds.Tables[index++];
			this._meta[KEY_MyAcception] = ds.Tables[index++];
			this._meta[KEY_StaticReport] = ds.Tables[index++];

			//11,12
			this._meta[KEY_U861FormatRelated] = this.GetDataMap(index++, new string[]{"ReportId", "bNewRpt"}, ds);
			this._meta[KEY_IsUsingU861Format] = this.GetDataMap(index++, new string[]{"ReportId", "bNewRpt"}, ds);
           
            this._meta[KEY_ReportCenterCatalog] = ds.Tables[index++];//报表中心分类信息
            

			return this._meta;
		}

        bool IsDefaultId(string sid)
        {
            string[] defaultstarted = new string[] { "AS", "DP", "MR", "EA", "FP", "FT", "OA", /*"SC",*/ "RR", "CO", "SO", "PO", "UA" };
            foreach (string s in defaultstarted)
                if (sid.Equals(s))
                    return true;
            return false;
        }


       private  Hashtable HandleLocalInstall(Hashtable ht)
        {
            Hashtable newHt = new Hashtable();
           
            foreach (string subId in ht.Keys)
            {
                string sid = subId;
                if (IsDefaultId(sid))
                {
                    newHt.Add(sid, ht[sid]);
                    continue;
                }
                RegistryKey key = Registry.LocalMachine.OpenSubKey(REGKEYPATH + "\\" + sid);
                if(key!=null)
                {
                    newHt.Add(sid, ht[sid]);
                }
            }
            return newHt;
        }
        public Dictionary<string, Reports> GetData4ReportDataForVB6(object vbLogin)
        {
            this._loginInfo = new U8LoginInfor(vbLogin);
            ReportCenterUiStateHandler uihandler = new ReportCenterUiStateHandler();
            ReportCenterObjAssembler assembler = new ReportCenterObjAssembler(null, this._loginInfo);
            Hashtable meta = this.GetData(true, false);
            assembler.Assemble(meta, uihandler, true);
            return assembler.ReportInfo;
        }

        public void DeleteStaticReport(StaticReport sr)
        {
            sr.LogInfo = this.LogInfo;
            sr.Delete(null);
        }

        /// <summary>
        /// 批量删除静态报表
        /// </summary>
        public void DeleteStaticReport(List<StaticReport> srs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (StaticReport sr in srs)
            {
                sb.Append("exec UAP_Report_MyReportStaticReportDelete ");
                sb.AppendFormat(
                    "'{0}','{1}','{2}'\r\n",
                    sr.ID,
                    this.LogInfo.cAccId,
                    this.LogInfo.Year);
            }
            if (sb.Length > 0)
            {
                SqlCommand cmd = new SqlCommand(sb.ToString());
                cmd.CommandType = CommandType.Text;
                SqlHelper.ExecuteNonQuery(this.LogInfo.UfMetaCnnString, cmd);
            }
        }

        private Hashtable GetDataMap(int index, string[] cols, DataSet ds)
        {
            return this.GetDataMap(index, cols, ds, null);
        }

        private Hashtable GetDataMap(
            int index,
            string[] cols,
            DataSet ds,
            Hashtable ht)
        {
            if (ht == null)
                ht = new Hashtable();
            DataTable dt = ds.Tables[index];
            string c1 = cols[0];
            string c2 = cols[1];
            if (string.IsNullOrEmpty(c2))
                c2 = c1;
            foreach (DataRow dr in dt.Rows)
                ht[dr[c1]] = dr[c2];
            return ht;
        }

        private DataSet GetDataFromDB(bool is4Web, bool is4Ref)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_ReportCenterGetData");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._loginInfo.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 10, this._loginInfo.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, this._loginInfo.cYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleId", SqlDbType.NVarChar, 10, this._loginInfo.LocaleID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Is4Web", SqlDbType.Bit, is4Web));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Is4Ref", SqlDbType.Bit, is4Ref));

            return SqlHelper.ExecuteDataSet(this._loginInfo.UfMetaCnnString, cmd);
        }

        public void UpdateReportCenterState(string userId, string xml)
        {
            string sql = string.Format(@"
				if exists(select UserId from UAP_ReportMyReport where UserId=N'{1}')
				begin
					update UAP_ReportMyReport set DataStructure=N'{0}' where UserId=N'{1}'
				end
				else
				begin
					insert into UAP_ReportMyReport(DataStructure,UserId) values(N'{0}', N'{1}')
				end",
                xml,
                userId);
            SqlHelper.ExecuteNonQuery(this._loginInfo.UfMetaCnnString, sql);
        }

        /// <summary>
        /// 目标对象是否存在
        /// </summary>
        public bool IsItemExists(IReportCenterNode ircn)
        {
            ReportDataFacade rdf = new ReportDataFacade(this._loginInfo);
            if (ircn is DynamicReport)
                return rdf.IsDynamicReportExist(ircn.ID);
            if (ircn is StaticReport)
                return rdf.IsStaticReportExist(ircn.ID);
            if (ircn is PublishData)
                return rdf.IsPublishDataExist(ircn.ID);
            return true;
        }
    }
}