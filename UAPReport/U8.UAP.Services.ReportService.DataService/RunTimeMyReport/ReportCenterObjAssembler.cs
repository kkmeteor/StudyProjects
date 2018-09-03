/*
 * ����:¬����
 * ʱ��:2009.1.13
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
	/// ���ݿ����ݷ�װ���ڴ����
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
			// ����Ԫ����
			this.ClearData();
			DataTable dt = null;

			// �ҵķ������ҵĽ���
			this._rawMeta[ReportCenterDataService.KEY_MyPublish] = meta[ReportCenterDataService.KEY_MyPublish] as DataTable;
			this._rawMeta[ReportCenterDataService.KEY_MyAcception] = meta[ReportCenterDataService.KEY_MyAcception] as DataTable;
            this._rawMeta[ReportCenterDataService.KEY_ReportCenterCatalog] = meta[ReportCenterDataService.KEY_ReportCenterCatalog] as DataTable;

			// BS�Ż�����Ҫ�������ĵ�״̬����
			// ��������״̬
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

			// �Ǿ�̬����Ȩ��
			this._hashData[ReportCenterDataService.KEY_ReportAuth] = meta[ReportCenterDataService.KEY_ReportAuth] as Hashtable;

			// ����ĳЩ����ҵ�������������Ӧ��׼����Ŀɼ���
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
		/// �����Ƿ�ɼ�
		/// </summary>
		private bool IsAllOk(IReportNode rn, Hashtable sysInfos)
		{ 
			// ϵͳ�����ò���ʾ����
			if(!sysInfos.ContainsKey(rn.SubID))
				return false;

			// �Ǿ�̬����
			Hashtable reportsWithAuth = this._hashData[ReportCenterDataService.KEY_ReportAuth];
			DynamicReport dr = rn as DynamicReport;
			if (dr != null && this.CheckAuth4DynamicReport(dr))
			{
				//��Щϵͳ������������ҵ���������ʱ�ſɼ�
				if (dr.bSystem
					&& !string.IsNullOrEmpty(dr.cMenuId)
					&& !this._standardReportMenuRuleHandler.IsReportVisible(dr.cMenuId))
					return false;
				return true;
			}

			// ��̬�������ܻ�������xml������ֵ䲻�����˹ؼ��ֵĹ�ϣ��
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
			//����������¼��ű������ʾ���:
			//�������ܲ����ڱ������Ŀ���
			//AS[__]7e3531d6-cfd7-4f64-9f39-24061e80e88b		�û��˺�ʹ����Ʋ�ѯ
			//AU[__]c08fe2c2-86c7-4d6a-9884-2e6351bb8af0		����վ����־��ѯ
			//AU[__]b2424754-4128-4c61-9a85-2249c8f03a8f		��ȫ���Ա����Ʋ�ѯ
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

				//�������ϱ��������������:(�Զ��屨�������л�)
				//1.ֻ�����ϱ�������:
				//	-a. bNewRpt=NULL�������л�������������ʹ���ϱ����
				//	-b. bNewRpt<>NULL�������л�
				//		-b.1 bNewRpt=1 ->����������ʹ���±����
				//		-b.2 bNewRpt=0 ->����������ʹ���ϱ����
				//2.ֻ�����±�������:�������л�����������ʹ���±����
				//3.���ϱ�������ͬʱ����:
				//  -a. bNewRpt=NULL����1-a����
				//  -b. bNewRpt<>NULL��bHadUpgradedFrom861=NULL ->�������л�
				//		 -a.1 bNewRpt=0 ->����������ʹ���ϱ����
				//		 -a.2 bNewRpt=1 ->��������ʹ���±����
				//  -c. bNewRpt<>NULL��bHadUpgradedFrom861<>NULL
				//		 -a.1 bNewRpt=0 ->�����л�������������ʹ���ϱ����
				//		 -a.2 bNewRpt=1 ->�����л���ʹ���±����
				//				-a.1 bHadUpgradedFrom861=1��������
				//				-a.1 bHadUpgradedFrom861=0��������

				//CanUseAs861Report��ʾ�˱����ܹ������л�
				rep.CanUseAs861Report = false;
				if (this.ContainsKey(u861FormatRelated, rep, ref val))
					rep.CanUseAs861Report = true;

				//IsUsing861Report��ʾ��ǰ����ʹ���ϱ���ʽ��ѯ
				//��ӦbNewRpt��ֵ:
				//bNewRpt=0��null -> IsUsing861Report=true
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
		/// �ؼ�Ҫ����һЩ�����ַ��Ĳ�ͬ״̬���硰������
		/// ��ر���Ϊ���������������Ʊ����
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
					|| id.IndexOf('��') != -1
					|| id.IndexOf('��') != -1))
			{ 
				string tp = string.Empty;
				tp = id.Replace('(', '��');
				tp = tp.Replace(')','��');
				isContained = u861FormatRelated.ContainsKey(tp);
				val = u861FormatRelated[tp];
				if (!isContained)
				{
					tp = id.Replace('��', '(');
					tp = tp.Replace('��',')');
					isContained = u861FormatRelated.ContainsKey(tp);
					val = u861FormatRelated[tp];
				}
			}
			return isContained;
		}
	}
}