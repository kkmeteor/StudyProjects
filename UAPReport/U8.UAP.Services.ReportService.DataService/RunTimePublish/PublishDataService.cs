/*
 * 作者:卢达其
 * 时间:2009.2.11
 * 
 * 890重构报表发布功能
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 报表发布功能的数据层服务
	/// </summary>
	public class PublishDataService
	{
		private U8LoginInfor _loginInfo = null;
		
		public U8LoginInfor LogInfo
		{
			get { return this._loginInfo; }
			set { this._loginInfo = value; }
		}

		/// <summary>
		/// 获取pd条件:
		/// 1.ID
		/// 2.LoginInfo
		/// </summary>
		public bool RetrievePublishData(PublishData pd, SqlTransaction tr)
		{ 
			pd.LogInfo = this._loginInfo;
			bool re = pd.Retrieve(tr);
			pd.IsDataSpeciafied = true;
			return re;
		}

		public void UpdatePublishData(PublishData pd, SqlTransaction tr)
		{ 
			pd.LogInfo = this._loginInfo;
			pd.Update(tr);
		}

		public void DeletePublishData(string id, SqlTransaction tr)
		{
			PublishData pd = new PublishData(id, null);
			pd.LogInfo = this._loginInfo;
			pd.Delete(tr);
		}

		/// <summary>
		/// 批量删除发布任务
		/// </summary>
		public void DeletePublishData(List<PublishData> pds, SqlTransaction tr)
		{ 
			StringBuilder sb = new StringBuilder();
			foreach (PublishData pd in pds)
			{
				sb.Append("exec UAP_Report_PublishmentDelete ");
				sb.AppendFormat(
					"'{0}','{1}','{2}'\r\n", 
					pd.ID,
					this.LogInfo.cAccId, 
					this.LogInfo.cYear);
			}
			if (sb.Length > 0)
			{
				SqlCommand cmd	= new SqlCommand(sb.ToString());
				cmd.CommandType	= CommandType.Text;
				if(tr != null)
					SqlHelper.ExecuteNonQuery(tr, cmd);
				else
					SqlHelper.ExecuteNonQuery(this.LogInfo.UfMetaCnnString, cmd);
			}
		}

		public ReportMeta4Publish ReportMeta4PublishRetrieve(
			object metaId, 
			bool is4portal,
			List<U8User> users,
            string currentViewId)
		{
			ReportMeta4Publish rp = new ReportMeta4Publish();
			if(is4portal)
				rp.ViewId = metaId.ToString();
            else
                rp.ReportId = metaId.ToString();
			rp.LogInfo = this.LogInfo;
			rp.Retrieve(is4portal, users,currentViewId);
			return rp;
		}

		public void RetriveStaticReport(
			StaticReport sr, 
			SqlTransaction tr)
		{ 
			sr.LogInfo = this.LogInfo;
			sr.Retrive(tr);
		}

		public void InsertStaticReport(
			StaticReport sr, 
			SqlTransaction tr)
		{ 
			sr.LogInfo = this.LogInfo;
			sr.Insert(tr);
		}

		public void DeleteStaticReport(StaticReport sr)
		{ 
			this.DeleteStaticReport(sr, null);
		}

		public void DeleteStaticReport(StaticReport sr, SqlTransaction tr)
		{ 
			sr.LogInfo = this.LogInfo;
			sr.Delete(tr);
		}
		
		public List<U8User> GetPublishUserInfo(string reportID)
		{
		    string sql = string.Format(
				@"EXECUTE uap_reportview_getusersbyreport '{0}','{1}','{2}'",
				reportID,
				this.LogInfo.cAccId,
				this.LogInfo.cYear);
		    DataSet ds = SqlHelper.ExecuteDataSet(this._loginInfo.UfMetaCnnString, sql);
		    List<U8User> users = new List<U8User>();
			PublishDataService.FillPublishUserInfo(users, ds.Tables[0]);
			return users;
		}

		public static void FillPublishUserInfo(List<U8User> users, DataTable dt)
		{
            users.Clear();
		    foreach(DataRow dr in dt.Rows)
				users.Add(PublishDataService.GetU8user(dr));
		}

		public static U8User GetU8user(DataRow dr)
		{ 
			U8User user	= new U8User();
			user.ID			= SqlHelper.GetStringFrom(dr["cUser_Id"]);
			user.Name		= SqlHelper.GetStringFrom(dr["cUser_Name"]);
			user.EMail		= SqlHelper.GetStringFrom(dr["cUserEmail"]);
			user.Department = SqlHelper.GetStringFrom(dr["cDept"]);
			user.PhoneNo	= SqlHelper.GetStringFrom(dr["cUserHand"]);
			return user;
		}
	}
}