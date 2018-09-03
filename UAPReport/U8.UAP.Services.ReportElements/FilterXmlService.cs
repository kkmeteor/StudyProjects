/*
 * 作者:卢达其
 * 时间:2007.7.19
 */

using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Data;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportResource;
using UFIDA.U8.UAP.Services.BizDAE.Elements;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 提供给过滤设计器的数据服务
	/// </summary>
	public class FilterXmlService
	{
		private const string _extendedColumnFlag = "#DE";

		private const string _xmlKeyDataSources = "DataSources";
		private const string _xmlKeyClass = "Class";
		private const string _xmlKeyClassID = "ClassID";
		private const string _xmlKeyClassName = "ClassName";
		private const string _xmlKeyBase = "base";
		private const string _xmlKeyItem = "Item";
		private const string _xmlKeyName = "Name";
		private const string _xmlKeyDescription = "Description";

		private string _reportId = null;
		private string _localeId = null;
		private XmlDataDocument _xmlDoc = null;
		private U8LoginInfor _login = null;

		public FilterXmlService(
			U8LoginInfor login, 
			string reportId,
            string localeId)
		{ 
			this._login = login;
			this._reportId = reportId;
			this._localeId = localeId;
		}

		/// <summary>
		/// 获取过滤项信息的xml串
		/// </summary>
		/// <param name="filterMeta">过滤项元数据</param>
		/// <returns>返回过滤项信息的xml串</returns>
		public string GetFilterConditionXml()
		{
			string sql = string.Format(
				@"select DataSourceID,FunctionName,DataSourceIdExtended 
				from uap_report where ID=N'{0}'",
				this._reportId);
			DataSet ds = SqlHelper.ExecuteDataSet(this._login.UfMetaCnnString, sql);
			string dataSourceID = SqlHelper.GetStringFrom(ds.Tables[0].Rows[0]["DataSourceID"]);
			string functionName = SqlHelper.GetStringFrom(ds.Tables[0].Rows[0]["FunctionName"]);
			string dataSourceIdExtended = SqlHelper.GetStringFrom(ds.Tables[0].Rows[0]["DataSourceIdExtended"]);
			
			this._xmlDoc = new XmlDataDocument();
			XmlElement root = this._xmlDoc.CreateElement(FilterXmlService._xmlKeyDataSources);
			this._xmlDoc.AppendChild(root);
			this.AddClass(root, dataSourceID, String4Report.GetString("原数据源条件"), functionName, true);
			this.AddClass(root, dataSourceIdExtended, String4Report.GetString("扩展后的数据源条件"), functionName, false);
			return this._xmlDoc.InnerXml;
		}

		private void AddClass(
			XmlElement parentXe, 
			string id, 
			string name,
			string functionName,
			bool isBase)
		{
			if(string.IsNullOrEmpty(id))
				return;
			XmlElement xe = this._xmlDoc.CreateElement(FilterXmlService._xmlKeyClass);
			parentXe.AppendChild(xe);
			string classId = id;
			if(isBase)//原数据源Id固定为base
				classId = FilterXmlService._xmlKeyBase;
			xe.SetAttribute(FilterXmlService._xmlKeyClassID, classId);
			xe.SetAttribute(FilterXmlService._xmlKeyClassName, name);
			
			ReportDataFacade rdf = new ReportDataFacade(this._login);
			FilterConditionCollection fcc = rdf.RetrieveFilterParas(id, functionName, this._localeId);
			this.AddFilterItems(isBase, xe, fcc);
		}

		private void AddFilterItems(
			bool isBase,
			XmlElement parentXe,
			FilterConditionCollection fcc)
		{
			foreach(FilterCondition fc in fcc)
			{ 
				//扩展的实体只取扩展列，其带有"#DE"标识
				if(!isBase
					&& !fc.Content.StartsWith(FilterXmlService._extendedColumnFlag))
					continue;

				XmlElement xe = this._xmlDoc.CreateElement( FilterXmlService._xmlKeyItem );
				parentXe.AppendChild(xe);
				xe.SetAttribute( FilterXmlService._xmlKeyName, fc.Content );
				xe.SetAttribute( FilterXmlService._xmlKeyDescription, fc.Description );
			}
		}
	}
}