/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:客户相关的权限处理
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	class RowAuthPickerCustomer : RowAuthPickerAbstract
	{
		public RowAuthPickerCustomer()
		{
			this.Key = "Customer";
			this.AddHitKey(new string[]{"cCusCode", "cCus_ID", "cCusID", "cCusName"});
		}

		protected override string GetWhereBy(string field, string whereFromAuthSrv)
		{
            if (_bhandled)
                return null;
            _bhandled = true;
            string wh = string.Format(
                "select cCusCode from Customer where iid in ({0})",
                whereFromAuthSrv);
            if (field.ToLower() == "ccusname")
            {
             //   return string.Format("Customer.cCusCode in ({0})", wh);

                 wh = string.Format(
                   "select ccusname from Customer where iid in ({0})",
                   whereFromAuthSrv);
                return string.Format(
                        "({0} in ({1}) or isnull({0},'') = '')", field, wh);
            }
            return string.Format("({0} in ({1}) or isnull({0},'') = '')", field, wh);
		}
	}
}
