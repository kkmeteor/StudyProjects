/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:处理符合条件的情况,获得带有权限处理后的Sql语句
 * 基础抽象对象
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal abstract class RowAuthPickerAbstract : IRowAuthColPicker
	{
		private string _key = null;
		private Dictionary<string, string> _hitKeys = new Dictionary<string, string>();
		private RowAuthContext _context = null;
        protected bool _bhandled = false;

		public string Key
		{
			get { return this._key;}
			set { this._key = value;} 
		}

		public RowAuthContext Context 
		{
			get { return this._context;}
			set { this._context = value;} 
		}

		public bool bHit(string filed)
		{ 
			return this._hitKeys.ContainsKey(filed.ToLower());
		}

		/// <summary>
		/// 键值不区分大小写
		/// </summary>
		protected void AddHitKey(string key)
		{ 
			this._hitKeys.Add(key.ToLower(), key);
		}

		protected void AddHitKey(string[] keys)
		{ 
			foreach(string key in keys)
				this.AddHitKey(key);
		}

		protected abstract string GetWhereBy(string field, string whereFromAuthSrv);

		public virtual string GetWhere(string field)
		{
			if (this.IsApplyRowAuth())
			{ 
				string whereFromAuthSrv = this.Context.ContentAuth.GetAuthString(this.Key, string.Empty, "R", false);
				if (!string.IsNullOrEmpty(whereFromAuthSrv))
				{
					if(whereFromAuthSrv.Trim() == RowAuthContext.NoAuthExpression)
						return RowAuthContext.NoAuthExpression;
					else
						return this.GetWhereBy(field, whereFromAuthSrv);
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// 判断该业务系统是否控制权限
		/// </summary>
		private bool IsApplyRowAuth()
		{ 
            //string authstring = this.Context.U8LoginClass.AuthString ;
            //if(string.IsNullOrEmpty(authstring))
            //    return false;

            //string[] authexps = authstring.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //foreach(string exp in authexps)
            //    if(exp.IndexOf(this.Key, StringComparison.OrdinalIgnoreCase) != -1
            //        && exp.IndexOf("true", StringComparison.OrdinalIgnoreCase) != -1)
            //        return true;
            //return false;
           
            //return true;

            if (IsSysSubAuth(this.Context.CSubId))
            {
                if (IsEnable(this.Context.CSubId))
                    return true;
                else
                    return false;
            }
            else
                return true;
		}

        private bool IsSysSubAuth(string cSubId)
        {            
            DataSet ds = SqlHelper.ExecuteDataSet(
                    this._context.U8LoginClass.UFDataConnstringForNet,
                    string.Format(@"select cParaName,m.cSubId, sys.cSub_Name from BusObj_SysSub_Mapping m
                                    left outer join 
                                     (
                                    Select csub_id, cSub_Name From ufsystem..ua_subsys 
                                    Union All
                                    Select distinct csubid as csub_id, cSub_Name From vw_HR_AccInformation
                                    ) sys on m.cSubId = sys.csub_id
                                    where m.cSubId='{0}'",
                                                         cSubId==""? this.Context.SubId:cSubId
                    ));
            if (ds.Tables[0].Rows.Count == 0)
                return false;
            else
                return true;
        }
        private bool IsEnable(string cSubId)
        {
            DataSet ds = SqlHelper.ExecuteDataSet(
                       this._context.U8LoginClass.UFDataConnstringForNet,
                       string.Format(@"select  dbo.f_ReportSubSysControlAuth('{0}','{1}')",
                       cSubId == "" ? this.Context.SubId : cSubId,
                        this.Key
                       ));
            if (Convert.ToInt32(ds.Tables[0].Rows[0][0]) == 1)
                return true;
            else
                return false;
        }
	}
}
