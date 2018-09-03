using System;
using System.Text;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Globalization;
using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class StaticReport : ReportNode, IFillData
	{
		/// <summary>
		/// 包装静态报表id时使用的分隔符
		/// </summary>
		public const char WrappedIdCompartChar = '#';

		private int		_reportViewType	= 0;
		private string	_publishID		= string.Empty;
		private string	_reportName		= string.Empty;
		private string	_publisherID	= string.Empty;
		private string	_reportViewID	= string.Empty;
		private string	_publisherName	= string.Empty;
		private string	_reportViewName	= string.Empty;
		
		private DateTime _createTime;
		private U8LoginInfor _loginInfo = null;

		public StaticReport() : this(null)
		{ 
		}
		
		public StaticReport(IOpenReportCenterNode iors)
		{ 
			this.OpenEngine = iors;
		}

		public U8LoginInfor LogInfo
		{
			get { return this._loginInfo; }
			set { this._loginInfo = value; }
		}

		public override string Text
		{
			get 
			{
				if (string.IsNullOrEmpty(base.Text))
				{
					base.Text = string.Format(
						"{0}({1})",
						this.ReportViewName,
						Util.GetTimeExtension(this.CreateTime));
				}
				return base.Text;
			}
			set { base.Text = value; }
		}

		public string PublishID
		{ 
			get{ return _publishID; }
			set{ _publishID = value; }
		}

		public string PublisherID
		{ 
			get{ return _publisherID; }
			set{ _publisherID = value; }
		}

		public string PublisherName
		{ 
			get{ return _publisherName; }
			set{ _publisherName = value; }
		}

		public string ReportViewID
		{ 
			get{ return _reportViewID; }
			set{ _reportViewID = value; }
		}

		public string ReportViewName
		{ 
			get{ return _reportViewName; }
			set{ _reportViewName = value; }
		}

		public int ReportViewType
		{ 
			get{ return _reportViewType; }
			set{ _reportViewType = value; }
		}

		public DateTime CreateTime
		{ 
			get{ return _createTime;}
			set{ _createTime = value; }
		}

		public string SubSysIDForPortal
		{
			get { return ReportDataFacade._VirtualSysIdForMyReportToPortalMenuSystem; }
		}

		public string AuthIdForPortal
		{
			get { return ReportDataFacade._VirtualAuthIdForStaticReportToPortalMenuSystem; }
		}

		public string CmdLineForPortal
		{
			get { return this.ReportViewType.ToString(); }
		}

		public override string ToPortalMenuCmdString()
        {
            return string.Format( 
				"ID:{0}&&&Name:{1}&&&SubSysID:{2}&&&AuthID:{3}&&&CMDLINE:{4}", 
				this.GetWrappedStaticReportId(),
				this.Text,
				this.SubSysIDForPortal,
				this.AuthIdForPortal,
				this.CmdLineForPortal );
        }

		/// <summary>
		/// 特殊包装静态报表ID:ID##当前名称
		/// 以解决门户报表的标签上正确显示静态报表的当前名称
		/// UFIDA.U8.Portal.ReportFacade.ReportPortalRunner
		/// 对象在获取静态报表ID将进行解析来获取信息
		/// </summary>
		public string GetWrappedStaticReportId()
		{ 
			return string.Format("{0}{1}{2}",
				this.ID,
				StaticReport.WrappedIdCompartChar.ToString(),
				this.Text);
		}

		internal void Insert(SqlTransaction tr)
		{ 
			SqlCommand cmd	= new SqlCommand("UAP_Report_MyReportStaticReportInsert");
			cmd.CommandType	= CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@StaticRptID", SqlDbType.NVarChar, 100, this.ID));
			cmd.Parameters.Add(SqlHelper.GetParameter("@PublishID", SqlDbType.NVarChar, 100, this.PublishID));
			cmd.Parameters.Add(SqlHelper.GetParameter("@CreateTime", SqlDbType.DateTime, this.CreateTime));
			cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 10, this.LogInfo.cAccId));
			cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, this.LogInfo.Year.ToString()));
			if(tr != null)
				SqlHelper.ExecuteNonQuery(tr, cmd);
			else
				SqlHelper.ExecuteNonQuery(this.LogInfo.UfMetaCnnString, cmd);
		}

		internal void Delete(SqlTransaction tr)
		{ 
			SqlCommand cmd	= new SqlCommand("UAP_Report_MyReportStaticReportDelete");
			cmd.CommandType	= CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@StaticRptID", SqlDbType.NVarChar, 100, this.ID));
			cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 10, this.LogInfo.cAccId));
			cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, this.LogInfo.Year.ToString()));
			if(tr != null)
				SqlHelper.ExecuteNonQuery(tr, cmd);
			else
				SqlHelper.ExecuteNonQuery(this.LogInfo.UfMetaCnnString, cmd);
		}

		public void FillData(DataRow dr)
		{
			this.ID				= SqlHelper.GetStringFrom(dr["ID"]);
			this.PublishID		= SqlHelper.GetStringFrom(dr["PublishID"]);
			this.PublisherID	= SqlHelper.GetStringFrom(dr["PublisherID"]);
			this.ReportViewID	= SqlHelper.GetStringFrom(dr["ReportViewID"]);
			this.SubID			= SqlHelper.GetStringFrom(dr["SubID"]);
			this.ReportViewType	= SqlHelper.GetIntFrom(dr["ReportViewType"], 0);
			this.CreateTime		= SqlHelper.GetDataTimeFrom(dr["CreateTime"], "2006-09-19");
			this.ReportViewName	= SqlHelper.GetStringFrom(dr["ReportViewName"]);
		}

		internal void Retrive(SqlTransaction tr)
		{
			string sql = string.Format(
				@"SELECT * FROM UAP_ReportStaticRpt WHERE ID=N'{0}'",
				this.ID);
			DataSet ds = null;
			if(tr != null)	
				ds = SqlHelper.ExecuteDataSet(tr, sql);
			else
				ds = SqlHelper.ExecuteDataSet(this.LogInfo.UfMetaCnnString, sql);
			this.FillData(ds.Tables[0].Rows[0]);
		}
	}
}