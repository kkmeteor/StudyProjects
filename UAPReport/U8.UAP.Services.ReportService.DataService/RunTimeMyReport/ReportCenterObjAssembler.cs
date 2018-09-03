/*
 * 作者:卢达其
 * 时间:2009.1.13
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportResource;
using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 数据库数据封装到内存对象
	/// </summary>
	public class ReportCenterObjAssembler
	{
		private Dictionary<string, Hashtable> _hashData = new Dictionary<string, Hashtable>();
		private Dictionary<string, Reports> _reportInfo = new Dictionary<string, Reports>();
		private Dictionary<string, DataTable> _rawMeta = new Dictionary<string, DataTable>();
		private StandardReportMenuRuleHandler _standardReportMenuRuleHandler = new StandardReportMenuRuleHandler();
		private IOpenReportCenterNode _iOpenReportCenterNode = null;
		private U8LoginInfor _login = null;

		public ReportCenterObjAssembler(IOpenReportCenterNode iors, U8LoginInfor login)
		{
			this._iOpenReportCenterNode = iors;
			string[] rpKeys = new string[]{
				ReportCenterDataService.KeyAll,
				ReportCenterDataService.KeySystem,
				ReportCenterDataService.KeyCustom,
				ReportCenterDataService.KEY_StaticReport,
			};
			foreach(string k in rpKeys)
				this._reportInfo[k] = new Reports();

			string[] htKeys = new string[]{
				ReportCenterDataService.KeySystem,
				ReportCenterDataService.KeyCustom,
				ReportCenterDataService.KEY_StaticReport,
			};
			foreach(string k in htKeys)
				this._hashData[k] = new Hashtable();

			this._rawMeta = new Dictionary<string,DataTable>();
			this._login = login;
		}

		public Dictionary<string, Hashtable> HashData
		{
			get { return this._hashData; }
		}

		public Dictionary<string, Reports> ReportInfo
		{
			get { return this._reportInfo; }
		}

		public Dictionary<string, DataTable> RawMeta
		{
			get { return this._rawMeta; }
		}

		public U8LoginInfor Login
		{
			set { this._login = value; }
		}

		public void Assemble(
			Hashtable meta,
			ReportCenterUiStateHandler reportCenterUiStateHandler)
		{
			this.Assemble(meta, reportCenterUiStateHandler, false);
		}

		public void Assemble(
			Hashtable meta, 
			ReportCenterUiStateHandler reportCenterUiStateHandler,
			bool is4ReportDataForVB6)
		{ 
			// 报表元数据
			this.ClearData();
			DataTable dt = null;

			// 我的发布，我的接收
			this._rawMeta[ReportCenterDataService.KEY_MyPublish] = meta[ReportCenterDataService.KEY_MyPublish] as DataTable;
			this._rawMeta[ReportCenterDataService.KEY_MyAcception] = meta[ReportCenterDataService.KEY_MyAcception] as DataTable;
            this._rawMeta[ReportCenterDataService.KEY_ReportCenterCatalog] = meta[ReportCenterDataService.KEY_ReportCenterCatalog] as DataTable;

			// BS门户不需要报表中心的状态数据
			// 报表中心状态
			if (!is4ReportDataForVB6)
			{
				dt = meta[ReportCenterDataService.KEY_UAP_ReportMyReport] as DataTable;
				if (dt.Rows.Count > 0)
				{
					DataRow dr = dt.Rows[0];
					string xml = SqlHelper.GetStringFrom(dr["DataStructure"]);
					reportCenterUiStateHandler.Xml2Obj(xml);
					foreach (string k in reportCenterUiStateHandler.DataKeys)
						this._hashData[k] = reportCenterUiStateHandler.HashData[k];
				}
			}

			// 非静态报表权限
			this._hashData[ReportCenterDataService.KEY_ReportAuth] = meta[ReportCenterDataService.KEY_ReportAuth] as Hashtable;

			// 根据某些特殊业务规则来控制相应标准报表的可见性
			this._standardReportMenuRuleHandler.Init(
				meta[ReportCenterDataService.KEY_BusinessRule1] as DataTable,
				meta[ReportCenterDataService.KEY_BusinessRule2] as DataTable);

			Hashtable sysInfos = meta[ReportCenterDataService.KEY_SystemInfo] as Hashtable;
			Hashtable u861FormatRelated = meta[ReportCenterDataService.KEY_U861FormatRelated] as Hashtable;
			Hashtable isUsingU861Format = meta[ReportCenterDataService.KEY_IsUsingU861Format] as Hashtable;
			string[] keys = new string[]{
				ReportCenterDataService.KEY_ReportMata,
				ReportCenterDataService.KEY_StaticReport};
			foreach (string k in keys)
			{
				dt = meta[k] as DataTable;
				foreach (DataRow dr in dt.Rows)
					this.FormOneReport(k, dr, sysInfos, u861FormatRelated, isUsingU861Format);
			}
		}

		private void ClearData()
		{
			foreach(Reports rs in this._reportInfo.Values)
				rs.Clear();
			foreach(Hashtable ht in this._hashData.Values)
				ht.Clear();
			this._rawMeta.Clear();
		}

		private void FormOneReport(
			string key,
			DataRow dr,
			Hashtable sysInfos,
			Hashtable u861FormatRelated,
			Hashtable isUsingU861Format)
		{
			IReportNode rn = this.GetMyReportTreeNode(
				dr, 
				u861FormatRelated, 
				isUsingU861Format, 
				ref key);
			if (this.IsAllOk(rn, sysInfos))
			{
				object subsystemName = sysInfos[rn.SubID];
				if (subsystemName != null)
					rn.SubName = subsystemName.ToString();
				else
					rn.SubName = rn.SubID;

				this.SetStaticReportCustomName(rn);
				this._reportInfo[ReportCenterDataService.KeyAll].Add(rn);
				this._reportInfo[key].Add(rn);
				this._hashData[key][rn.SubID] = rn.SubName;
			}
		}

		private void SetStaticReportCustomName(IReportNode rn)
		{
			if (this._hashData.ContainsKey(ReportCenterDataService.KeyCustomStaticReportNames))
			{
				Hashtable staticReportNames = this._hashData[ReportCenterDataService.KeyCustomStaticReportNames];
				if (staticReportNames.ContainsKey(rn.ID))
				{
					IReportCenterNode ircn = staticReportNames[rn.ID] as IReportCenterNode;
					if(!string.IsNullOrEmpty(ircn.Text))
						rn.Text = ircn.Text;
				}
			}
		}

		/// <summary>
		/// 报表是否可见
		/// </summary>
		private bool IsAllOk(IReportNode rn, Hashtable sysInfos)
		{ 
			// 系统不启用不显示报表
			if(!sysInfos.ContainsKey(rn.SubID))
				return false;

			// 非静态报表
			Hashtable reportsWithAuth = this._hashData[ReportCenterDataService.KEY_ReportAuth];
			DynamicReport dr = rn as DynamicReport;
			if (dr != null && this.CheckAuth4DynamicReport(dr))
			{
				//有些系统报表必须在相关业务规则启用时才可见
				if (dr.bSystem
					&& !string.IsNullOrEmpty(dr.cMenuId)
					&& !this._standardReportMenuRuleHandler.IsReportVisible(dr.cMenuId))
					return false;
				return true;
			}

			// 静态报表，可能还不存在xml，因此字典不包含此关键字的哈希表
			if (rn is StaticReport)
			{
				if (this._hashData.ContainsKey(ReportCenterDataService.KeyDeletedStaticReports))
				{
					Hashtable deletedStaticReports = this._hashData[ReportCenterDataService.KeyDeletedStaticReports];
					if (deletedStaticReports.ContainsKey(rn.ID))
						return false;
				}
				return true;
			}
			return false;
		}

		private bool CheckAuth4DynamicReport(DynamicReport dr)
		{ 
			//特殊控制以下几张报表的显示情况:
			//帐套主管不能在报表中心看到
			//AS[__]7e3531d6-cfd7-4f64-9f39-24061e80e88b		用户账号使用审计查询
			//AU[__]c08fe2c2-86c7-4d6a-9884-2e6351bb8af0		清退站点日志查询
			//AU[__]b2424754-4128-4c61-9a85-2249c8f03a8f		安全策略变更审计查询
			//if (this._login.U8LoginClass.IsAdmin)
			//{
			//    if(dr.ID.ToUpper() == "AS[__]7E3531D6-CFD7-4F64-9F39-24061E80E88B"
			//        || dr.ID.ToUpper() == "AU[__]C08FE2C2-86C7-4D6A-9884-2E6351BB8AF0"
			//        || dr.ID.ToUpper() == "AU[__]B2424754-4128-4C61-9A85-2249C8F03A8F")
			//        return false;
			//}

			Hashtable reportsWithAuth = this._hashData[ReportCenterDataService.KEY_ReportAuth];
			return reportsWithAuth.ContainsKey(dr.ID);
		}

		private IReportNode GetMyReportTreeNode(
			DataRow dr, 
			Hashtable u861FormatRelated,
			Hashtable isUsingU861Format,
			ref string key)
		{
			IReportNode im = null;
			if (key == ReportCenterDataService.KEY_StaticReport)
				im = new StaticReport(this._iOpenReportCenterNode);
			else
				im = new DynamicReport(this._iOpenReportCenterNode);
			(im as IFillData).FillData(dr);

			DynamicReport rep = im as DynamicReport;
			if (rep != null)
			{
				object val = null;

				//关于新老报表存在如下情形:(自定义报表都不能切换)
				//1.只存在老报表数据:
				//	-a. bNewRpt=NULL则不允许切换，不允许发布，使用老报表打开
				//	-b. bNewRpt<>NULL则允许切换
				//		-b.1 bNewRpt=1 ->不允许发布，使用新报表打开
				//		-b.2 bNewRpt=0 ->不允许发布，使用老报表打开
				//2.只存在新报表数据:不允许切换，允许发布，使用新报表打开
				//3.新老报表数据同时存在:
				//  -a. bNewRpt=NULL则按照1-a情形
				//  -b. bNewRpt<>NULL且bHadUpgradedFrom861=NULL ->不允许切换
				//		 -a.1 bNewRpt=0 ->不允许发布，使用老报表打开
				//		 -a.2 bNewRpt=1 ->允许发布，使用新报表打开
				//  -c. bNewRpt<>NULL且bHadUpgradedFrom861<>NULL
				//		 -a.1 bNewRpt=0 ->允许切换，不允许发布，使用老报表打开
				//		 -a.2 bNewRpt=1 ->允许切换，使用新报表打开
				//				-a.1 bHadUpgradedFrom861=1则允许发布
				//				-a.1 bHadUpgradedFrom861=0则不允许发布

				//CanUseAs861Report表示此报表能够进行切换
				rep.CanUseAs861Report = false;
				if (this.ContainsKey(u861FormatRelated, rep, ref val))
					rep.CanUseAs861Report = true;

				//IsUsing861Report表示当前报表使用老报表方式查询
				//对应bNewRpt的值:
				//bNewRpt=0或null -> IsUsing861Report=true
				//bNewRpt=1 -> IsUsing861Report=false
				val = null;
				rep.IsUsing861Report = false;
				if (this.ContainsKey(isUsingU861Format, rep, ref val))
					rep.IsUsing861Report = !SqlHelper.GetBooleanFrom(val, false);

				if (rep.bSystem)
					key = ReportCenterDataService.KeySystem;
				else
					key = ReportCenterDataService.KeyCustom;
			}

			return im;
		}

		/// <summary>
		/// 关键要处理一些特殊字符的不同状态，如“（”，
		/// 相关报表为“货龄分析（按发票）”
		/// </summary>
		private bool ContainsKey(
			Hashtable u861FormatRelated, 
			DynamicReport dr,
			ref object val)
		{ 
			string id = dr.ID;
			bool isContained = u861FormatRelated.ContainsKey(id);
			val = u861FormatRelated[id];
			if (!isContained
				&& (id.IndexOf('(') != -1
					|| id.IndexOf(')') != -1
					|| id.IndexOf('（') != -1
					|| id.IndexOf('）') != -1))
			{ 
				string tp = string.Empty;
				tp = id.Replace('(', '（');
				tp = tp.Replace(')','）');
				isContained = u861FormatRelated.ContainsKey(tp);
				val = u861FormatRelated[tp];
				if (!isContained)
				{
					tp = id.Replace('（', '(');
					tp = tp.Replace('）',')');
					isContained = u861FormatRelated.ContainsKey(tp);
					val = u861FormatRelated[tp];
				}
			}
			return isContained;
		}
	}
}