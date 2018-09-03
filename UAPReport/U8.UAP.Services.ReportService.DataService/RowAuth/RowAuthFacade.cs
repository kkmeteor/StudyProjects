/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:行权限处理总控制器 
 */

using System;
using System.Collections.Generic;
using System.Text;
using UFSoft.U8.Framework.SecurityCommon;
using System.Data.SqlClient;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class RowAuthFacade
	{
		private IRowAuthColPicker[] _pickers = null;
		private RowAuthContext _context = new RowAuthContext();

		public RowAuthFacade()
		{
			this._pickers = new IRowAuthColPicker[]{
				new RowAuthPickerCustomer(),
				new RowAuthPickerDepartment(),
				new RowAuthPickerInventory(),
				new RowAuthPickerPerson(),
				new RowAuthPickerVendor(),
				new RowAuthPickerWareHouse(),
			};
			foreach(IRowAuthColPicker picker in this._pickers)
				picker.Context = this._context;
		}

		private void InitContext(string subId,string csubId,U8LoginInfor login, bool bWeb)
		{
			try
			{
				ContentAuth ca = new ContentAuth(
					login.U8LoginClass.UfDbName,
					login.UserID,
					bWeb);
				
				this._context.ContentAuth = ca;
				this._context.U8LoginClass = login.U8LoginClass;
                this._context.SubId = subId;
                this._context.CSubId = csubId;

			}
			catch (Exception e)
			{
				throw new Exception("初始化RowAuthContext出错:" + e.Message);
			}
		}

        public string GetRowAuthFromAllColumnsWithStaticID(string staticid, string allcolumns, U8LoginInfor login, bool bweb)
        {
            string subId = "";
            string csubId = "";
            GetDbSubIdWithStaticId(staticid, login,out subId,out csubId);
            string columns = System.Text.RegularExpressions.Regex.Split(allcolumns, "@;@")[0].Replace("[", "").Replace("]", "");
            return GetRowAuth(subId, csubId,columns, login, bweb);
        }
        public string GetRowAuthFromAllColumns(string viewid,string allcolumns,U8LoginInfor login,bool bweb)
        {
            string subId = "";
            string csubId = "";
            GetDbSubIdwithViewId(viewid, login, out subId ,out csubId);
            string columns = System.Text.RegularExpressions.Regex.Split(allcolumns, "@;@")[0].Replace("[", "").Replace("]", "");
            return GetRowAuth(subId, csubId,columns, login, bweb);
        }
        private void GetDbSubIdWithStaticId(string staticid, U8LoginInfor login, out string subid, out string csubid)
        {
            //string strSQL = string.Format("select subId from uap_reportstaticrpt where id='{0}'", staticid);
            string strSQL = string.Format(@"select a.ReportViewID,b.ID,c.Subid,c.CSubID 
                                            from uap_reportstaticrpt a 
                                            left join UAP_ReportView  b on a.ReportViewID=b.ID
                                            left join UAP_Report c on b.ReportID=c.ID
                                            where a.ID='{0}'",staticid);
            SqlCommand cmd = new SqlCommand(strSQL);
            cmd.CommandType = CommandType.Text;
            subid = ""; csubid = "";
            DataSet ds = SqlHelper.ExecuteDataSet(login.UfMetaCnnString, cmd);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if(ds.Tables[0].Rows[0]["subId"] != null)
                {
                    subid = ds.Tables[0].Rows[0]["subId"].ToString();
                }
                if (ds.Tables[0].Rows[0]["csubId"] != null)
                {
                    csubid = ds.Tables[0].Rows[0]["csubId"].ToString();
                }
            }
        }
        private void GetDbSubIdwithViewId(string viewid, U8LoginInfor login,out string subid,out string csubid)
        {
            string strSQL = string.Format("select subId,csubid from uap_report where id in(select reportid from uap_reportview where id='{0}')", viewid);
            //string strSQL = string.Format("select subId from uap_report where id in(select reportid from uap_reportview where id='{0}')", viewid);
            SqlCommand cmd = new SqlCommand(strSQL);
            cmd.CommandType = CommandType.Text;

            DataSet ds = SqlHelper.ExecuteDataSet(login.UfMetaCnnString, cmd);
            subid = ""; csubid = "";
            if(ds.Tables.Count>0 && ds.Tables[0].Rows.Count>0)
            {
                if (ds.Tables[0].Rows[0]["subId"] != null)
                {
                    subid = ds.Tables[0].Rows[0]["subId"].ToString();
                }
                if (ds.Tables[0].Rows[0]["csubId"] != null)
                {
                    csubid = ds.Tables[0].Rows[0]["csubId"].ToString();
                }                
            }            
        }

        public string GetRowAuth(string subId, string fieldString, U8LoginInfor login, bool bWeb)
        {
            return GetRowAuth(subId, "",fieldString, login, bWeb);
        }
		public  string GetRowAuth(string subId,string csubId, string fieldString, U8LoginInfor login, bool bWeb)
		{ 
			if(string.IsNullOrEmpty(fieldString))
				return string.Empty;
			if(login == null)
				throw new Exception("登录对象U8LoginInfor为空");

            this.InitContext(subId, csubId, login, bWeb);
			
			//管理员不控制权限
			if(this._context.U8LoginClass != null
				&& this._context.U8LoginClass.IsAdmin)
				return string.Empty;

			string[] fileds = fieldString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			string condition = null;
			StringBuilder where = new StringBuilder();
			foreach (string field in fileds)
			{
				condition = this.GetWhere(field);
				if (!string.IsNullOrEmpty(condition))
				{
					//只要某个条件为固定无权，则返回无权表达式
					if(condition == RowAuthContext.NoAuthExpression)
						return RowAuthContext.NoAuthExpression;
					where.Append(condition);
					where.Append(" and ");
				}
			}

			if(where.Length > 5)
				where = where.Remove(where.Length -5, 5);
			return where.ToString();
		}

		private string GetWhere(string field)
		{
			foreach(IRowAuthColPicker picker in this._pickers)
                if (picker.bHit(field))
                {
                    string result = picker.GetWhere(field);
                    return result;
                }
			return null;
		}
	}
}
