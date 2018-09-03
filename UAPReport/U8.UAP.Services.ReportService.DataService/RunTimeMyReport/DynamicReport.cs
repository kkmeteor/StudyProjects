/*
 * 作者:卢达其
 * 时间:2009.1.13
 * 
 * 890重构“报表中心”:
 * 1.提高加载速度
 * 2.剔除不必要的功能
 */

using System;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
    public class DynamicReport : ReportNode, IFillData
    {
		private bool _bSystem = false;
		private bool _canUseAs861Report = false;
		private bool _isUsing861Report = false;
		private bool _bUsingReport4MenuId = false;
		private string _cMenuId = null;
		private string _cAuthId = null;

		public DynamicReport() : this(null)
		{ 
		}

		public DynamicReport(IOpenReportCenterNode iors)
		{ 
			this.OpenEngine = iors;
		}

		public bool CanUseAs861Report
        {
            get { return this._canUseAs861Report; }
			set { this._canUseAs861Report = value; }
        }

		public bool bUsingReport4MenuId
        {
            get { return this._bUsingReport4MenuId; }
			set { this._bUsingReport4MenuId = value; }
        }

		public bool IsUsing861Report
        {
            get { return this._isUsing861Report; }
			set { this._isUsing861Report = value; }
        }

		public bool bSystem
        {
            get { return this._bSystem; }
			set { this._bSystem = value; }
        }

		public string cMenuId
        {
            get { return this._cMenuId; }
			set { this._cMenuId = value; }
        }

		public override string ToPortalMenuCmdString()
        {
			string id4portal = this._cMenuId;
			string name4portal = this.Text;
			string subId4portal = this.SubID;
			string authId4porttal = this._cAuthId;
			//if (!this.bSystem)
			//{
			//    id4portal = this.ID;
			//    subId4portal = "RE";
			//    authId4porttal = "U870CUSTOMREPORT";
			//}

            return string.Format( 
				"ID:{0}&&&Name:{1}&&&SubSysID:{2}&&&AuthID:{3}&&&CMDLINE:",
				id4portal,
				name4portal,
				subId4portal,
				authId4porttal);
        }

		public void FillData(DataRow dr)
        {
            this.ID			= SqlHelper.GetStringFrom(dr["ID"]);
            this.SubID		= SqlHelper.GetStringFrom(dr["SubID"]);
            this.Text		= SqlHelper.GetStringFrom(dr["Name"]);
			this.bSystem	= SqlHelper.GetBooleanFrom(dr["bSystem"], false);
			this._cAuthId	= SqlHelper.GetStringFrom(dr["cAuth_Id"]);
			this._cMenuId	= SqlHelper.GetStringFrom(dr["MappingMenuId"]);
			this._bUsingReport4MenuId = SqlHelper.GetBooleanFrom(dr["bUsingReport4MenuId"], false);
        }
    }
}
