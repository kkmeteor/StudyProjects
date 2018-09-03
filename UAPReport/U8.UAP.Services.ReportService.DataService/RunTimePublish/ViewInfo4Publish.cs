/*
 * 作者:卢达其
 * 时间:2009.2.13
 * 
 * 890重构报表发布功能
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 发布功能需要的一些视图信息
	/// </summary>
	public class ViewInfo4Publish
	{
		private int _viewType = 1;
		private bool _isDefault = false;
		private string _id = string.Empty;
		private string _name = string.Empty;
		private string _reportId = string.Empty;
		private string _rootReportId = string.Empty;
		private string _reportName = string.Empty;
		private string _className = string.Empty;
		private string _filterClass = string.Empty;
		private string _subId = string.Empty;
		private string _filterId = string.Empty;
		private string _groupSchemas = string.Empty;
		private object _currentGroupSchema = null; 
		private object _groupSchemasObj = null; 
		private PublishData _currentPublishData = null;
		private List<PublishData> _publishes = new List<PublishData>();

		public string ClassName
		{
			get { return this._className; }
			set { this._className = value; }
		}

		public string FilterClass
		{
			get { return this._filterClass; }
			set { this._filterClass = value; }
		}

		public string ReportName
		{
			get { return this._reportName; }
			set { this._reportName = value; }
		}

		public string ReportId
		{
			get { return this._reportId; }
			set { this._reportId = value; }
		}

		public string RootReportId
		{
			get { return this._rootReportId; }
			set { this._rootReportId = value; }
		}

		public PublishData CurrentPublishData
		{
			get { return this._currentPublishData; }
			set { this._currentPublishData = value; }
		}

		public object CurrentGroupSchema
		{
			get { return this._currentGroupSchema; }
			set { this._currentGroupSchema = value; }
		}

		public object GroupSchemasObj
		{
			get { return this._groupSchemasObj; }
			set { this._groupSchemasObj = value; }
		}

		public int ViewType
		{
			get { return this._viewType; }
			set { this._viewType = value; }
		}

		public bool IsDefault
		{
			get { return this._isDefault; }
			set { this._isDefault = value; }
		}

		public string ID
		{
			get { return this._id; }
			set { this._id = value; }
		}

		public string SubId
		{
			get { return this._subId; }
			set { this._subId = value; }
		}

		public string FilterId
		{
			get { return this._filterId; }
			set { this._filterId = value; }
		}

		public string Name
		{
			get { return this._name; }
			set { this._name = value; }
		}

		public string GroupSchemas
		{
			get { return this._groupSchemas; }
			set { this._groupSchemas = value; }
		}

		public List<PublishData> Publishes
		{
			get { return this._publishes; }
			set { this._publishes = value; }
		}

		public PublishData GetPublishData(string id)
		{ 
			foreach(PublishData p in this.Publishes)
				if(p.ID == id)
					return p;
			return null;
		}

		public void RemovePublish(string id)
		{ 
			foreach(PublishData p in this.Publishes)
				if (p.ID == id)
				{
					this.Publishes.Remove(p);
					return;
				}
		}

		public void FillData(DataRow dr)
		{
			this.ID = SqlHelper.GetStringFrom(dr["ViewID"]);
			this.Name = SqlHelper.GetStringFrom(dr["ViewName"]);
			this.GroupSchemas = SqlHelper.GetStringFrom(dr["GroupSchemas"]);
			this.ViewType = SqlHelper.GetIntFrom(dr["ViewType"], 1);
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
