/*
 * 作者:卢达其
 * 时间:2008.3.17
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.BizDAE.Elements;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 872报表元数据包装器:
	/// 为升级好的数据实现方便地向持久化对象进行填充
	/// </summary>
	internal class UpgradeReportMetaWrapper
	{
		// 常量字符串统一信息名称
		// 报表基础数据
		public const string ReportId = "ReportId";
		public const string ReportSubId = "ReportSubId";
		public const string ReportFilterId = "ReportFilterId";
		public const string ReportDataSourceBO = "ReportDataSourceBO";
		public const string ReportClassName = "ReportClassName";
		public const string ReportFilterClass = "ReportFilterClass";
		public const string ReportIsSystem = "ReportIsSystem";
		public const string ReportNameCN = "ReportNameCN";
		public const string ReportNameTW = "ReportNameTW";
		public const string ReportNameEN = "ReportNameEN";
		public const string ReportHelpFileName = "ReportHelpFileName";
		public const string ReportHelpIndex = "ReportHelpIndex";
		public const string ReportHelpKeyWord = "ReportHelpKeyWord";
		public const string ReportName861 = "ReportName861";
		public const string MappingMenuId = "MappingMenuId";
		public const string Description = "Description";
        public const string RootReportId = "RootReportId";

        public const string IsNeedExpandTW = "IsNeedExpandTW";
        public const string IsNeedExpandEN = "IsNeedExpandEN";

		// 数据库环境
		public const string DBAppServer = "DBAppServer";
		public const string DBUfMetaConnString = "DBUfMetaConnString";
		public const string DBcAccId = "DBcAccId";
		public const string DBcYear = "DBcYear";

		// 视图数据
		public const string ViewId = "ViewId";
		public const string ViewIsSystem = "ViewIsSystem";
		public const string ViewCommonFormat = "ViewCommonFormat";
		public const string ViewViewType = "ViewViewType";
		public const string ViewGroupSchemas = "ViewGroupSchemas";
		public const string ViewLevelExpand = "ViewLevelExpand";
		public const string ViewLocaleFormatCN = "ViewLocaleFormatCN";
		public const string ViewLocaleFormatTW = "ViewLocaleFormatTW";
		public const string ViewLocaleFormatEN = "ViewLocaleFormatEN";
		public const string ViewColumns	= "ViewColumns";
		public const string ViewBlandScape = "ViewBlandScape";
		public const string ViewPageMargins	= "ViewPageMargins";
		public const string ViewPaperType = "ViewPaperType";

		/// <summary>
		/// 必输项目
		/// </summary>
		private string[] _args = new string[]{
			ReportId,
			ReportSubId,
			ReportDataSourceBO,
			ReportIsSystem,
			ReportNameCN,
			ReportNameTW,
			ReportNameEN,
			ReportName861,
			DBAppServer,
			DBUfMetaConnString,
			DBcAccId,
			DBcYear,
			ViewId,
			ViewIsSystem,
			ViewCommonFormat,
			ViewViewType,
			ViewLocaleFormatCN,
			ViewLocaleFormatTW,
			ViewLocaleFormatEN,
            RootReportId,
		};
		private Hashtable _meta = new Hashtable();

		/// <summary>
		/// 升级程序使用此方法将升级的数据转换成对象，
		/// 由此对象实现数据到持久化对象转移
		/// </summary>
		/// <param name="key">数据的标识</param>
		/// <param name="val">数据值</param>
		public void SetArgument( string key, object val )
		{ 
			this._meta[key] = val;
		}

		/// <summary>
		/// 是否已包含该关键字
		/// </summary>
		public bool Contains(object key)
		{
			return this._meta.ContainsKey(key);
		}

		/// <summary>
		/// 检查是否包含必须的元数据信息
		/// </summary>
		private void Check()
		{ 
			ArrayList needTheseKeys = new ArrayList();
			ArrayList emptyValues = new ArrayList();
			foreach (string key in this._args)
			{
				if (!this._meta.Contains(key))
					needTheseKeys.Add(key);
				object val = this._meta[key];
				if ( val == null || string.IsNullOrEmpty( val.ToString()))
					emptyValues.Add(key);
			}
			if (needTheseKeys.Count > 0 || emptyValues.Count > 0 )
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine( "错误:" );
				sb.AppendLine( "报表缺少的元数据:" );
				foreach (string key in needTheseKeys)
					sb.AppendFormat("[{0}],", key);
				sb.AppendLine( "已经设置了以下元数据，但是其值为空:" );
				foreach (string key in emptyValues)
					sb.AppendFormat("[{0}],", key);
				throw new Exception( sb.ToString());
			}
		}

		/// <summary>
		/// 根据已填充的数据构造一个UpgradeReport对象
		/// </summary>
		public UpgradeReport WrapData2Object()
		{ 
			//this.Check();
			UpgradeReport ur = new UpgradeReport();
			this.SetReportBase(ur);
			this.SetViewMeta(ur);
			return ur;
		}

		/// <summary>
		/// 设置报表级基础信息
		/// </summary>
		private void SetReportBase(UpgradeReport ur)
		{
			ur.ID = this.GetArgString(UpgradeReportMetaWrapper.ReportId);
			ur.SubProjectID = this.GetArgString(UpgradeReportMetaWrapper.ReportSubId);
			ur.FilterID = this.GetArgString(UpgradeReportMetaWrapper.ReportFilterId);
			ur.ClassName = this.GetArgString(UpgradeReportMetaWrapper.ReportClassName);
			ur.FilterClass = this.GetArgString(UpgradeReportMetaWrapper.ReportFilterClass);
			ur.bSystem = this.GetArgBoolean(UpgradeReportMetaWrapper.ReportIsSystem);
			ur.HelpFileName = this.GetArgString(UpgradeReportMetaWrapper.ReportHelpFileName);
			ur.HelpIndex = this.GetArgString(UpgradeReportMetaWrapper.ReportHelpIndex);
			ur.HelpKeyWord = this.GetArgString(UpgradeReportMetaWrapper.ReportHelpKeyWord);
			ur.ReportName861 = this.GetArgString(UpgradeReportMetaWrapper.ReportName861);
			ur.MappingMenuId = this.GetArgString(UpgradeReportMetaWrapper.MappingMenuId);
			ur.Description = this.GetArgString(UpgradeReportMetaWrapper.Description);
			this.AddReportLoacleInfo("zh-CN", this.GetArgString(UpgradeReportMetaWrapper.ReportNameCN), ur);
			this.AddReportLoacleInfo("zh-TW", this.GetArgString(UpgradeReportMetaWrapper.ReportNameTW), ur);
			this.AddReportLoacleInfo("en-US", this.GetArgString(UpgradeReportMetaWrapper.ReportNameEN), ur);
			ur.AppServer = this.GetArgString(UpgradeReportMetaWrapper.DBAppServer);
			ur.DbConnString = this.GetArgString(UpgradeReportMetaWrapper.DBUfMetaConnString);
			ur.cAccId = this.GetArgString(UpgradeReportMetaWrapper.DBcAccId);
			ur.cYear = this.GetArgString(UpgradeReportMetaWrapper.DBcYear);
			ur.DataSourceInfo.DataSourceBO = (BusinessObject)this._meta[UpgradeReportMetaWrapper.ReportDataSourceBO];
            ur.RootReportId = this.GetArgString(UpgradeReportMetaWrapper.RootReportId);
            ur.IsNeedExpandEN = this.GetArgBoolean(UpgradeReportMetaWrapper.IsNeedExpandEN);
            ur.IsNeedExpandTW = this.GetArgBoolean(UpgradeReportMetaWrapper.IsNeedExpandTW);
		}

		/// <summary>
		/// 设置视图级信息
		/// </summary>
		private void SetViewMeta(UpgradeReport ur)
		{
			UpgradeReportView urv = new UpgradeReportView( ur );
			ur.ReportViews.Add( urv );
			urv.ID = this.GetArgString(UpgradeReportMetaWrapper.ViewId);
			urv.bSystem = this.GetArgBoolean(UpgradeReportMetaWrapper.ViewIsSystem);
			urv.ViewType = this.GetArgInt(UpgradeReportMetaWrapper.ViewViewType);
			urv.GroupSchemas = this.GetArgString(UpgradeReportMetaWrapper.ViewGroupSchemas);
			urv.CommonFormat = this.GetArgString(UpgradeReportMetaWrapper.ViewCommonFormat);
			urv.LevelExpand = this.GetArgString(UpgradeReportMetaWrapper.ViewLevelExpand);
			urv.Columns = this.GetArgString(UpgradeReportMetaWrapper.ViewColumns);
			urv.BlandScape = this.GetArgBoolean(UpgradeReportMetaWrapper.ViewBlandScape);
			urv.PageMargins = this.GetArgString(UpgradeReportMetaWrapper.ViewPageMargins, "80,80,80,80");
			urv.PaperType = this.GetArgInt(UpgradeReportMetaWrapper.ViewPaperType);
			this.AddViewLoacleInfo("zh-CN",
				this.GetArgString(UpgradeReportMetaWrapper.ViewLocaleFormatCN),
				urv );
			this.AddViewLoacleInfo("zh-TW",
				this.GetArgString(UpgradeReportMetaWrapper.ViewLocaleFormatTW),
				urv );
			this.AddViewLoacleInfo("en-US",
				this.GetArgString(UpgradeReportMetaWrapper.ViewLocaleFormatEN),
				urv );
		}

		private void AddReportLoacleInfo(
			string locale, 
			string name,
			UpgradeReport ur )
		{
			ReportLocaleInfo rli = new ReportLocaleInfo();
			rli.LocaleID = locale;
			rli.Name = name;
			ur.ReportLocaleInfos.Add( rli );
		}

		private void AddViewLoacleInfo(
			string locale, 
			string localeFormat,
			UpgradeReportView urv )
		{
			ReportViewLocaleInfo rvli = new ReportViewLocaleInfo();
			rvli.LocaleID = locale;
			rvli.LocaleFormat = localeFormat;
			urv.ViewLocaleInfos.Add( rvli );
		}

		private string GetArgString(string key)
		{
			if( !this._meta.Contains( key ) || this._meta[key]==null)
				return string.Empty;
			return this._meta[key].ToString();
		}

		private string GetArgString(string key, string valWhenEmpty )
		{
            if (!this._meta.Contains(key) || this._meta[key] == null)
				return valWhenEmpty;
			return this._meta[key].ToString();
		}

		private bool GetArgBoolean(string key)
		{
            if (!this._meta.Contains(key) || this._meta[key] == null)
				return false;
			return Convert.ToBoolean( this._meta[key] );
		}

		private int GetArgInt(string key)
		{
            if (!this._meta.Contains(key) || this._meta[key] == null)
				return 1;
			return Convert.ToInt32( this._meta[key] );
		}
	}
}