/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:存货相关的权限处理
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	class RowAuthPickerInventory : RowAuthPickerAbstract
	{
		public RowAuthPickerInventory()
		{
			this.Key = "Inventory";
			this.AddHitKey(new string[]{"cInvCode", "cInvName","存货名称",});
		}

		protected override string GetWhereBy(string field, string whereFromAuthSrv)
		{
            if (_bhandled)
                return null;
            _bhandled = true;
            string wh ;
            if (field.ToLower() == "cinvname" || field.ToLower() == "存货名称")
            {
                wh = string.Format(
                    "select cinvname from inventory where iid in ({0})",
                    whereFromAuthSrv);
                //return string.Format(
                //        "(cinvname in ({0}) or isnull(cinvname,'') = '')", wh);
            }
            else
            {
                wh = string.Format(
                        "select cInvCode from inventory where iid in ({0})",
                        whereFromAuthSrv);
            }
			return string.Format("({0} in ({1}) or isnull({0},'')='') ", field, wh);
		}
	}
}
