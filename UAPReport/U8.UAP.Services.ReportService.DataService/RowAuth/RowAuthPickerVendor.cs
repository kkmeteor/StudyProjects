/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:供应商相关的权限处理
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
    class RowAuthPickerVendor : RowAuthPickerAbstract
    {
        public RowAuthPickerVendor()
        {
            this.Key = "Vendor";
            this.AddHitKey(new string[]{
				"cVenCode", "cBVencode", "cRVenCode", 
				"csup_id", "cSupCode", "cSupId", "cVenName",});
        }

        protected override string GetWhereBy(string field, string whereFromAuthSrv)
        {
            if (_bhandled)
                return null;
            _bhandled = true;
            string wh = string.Format(
                "select cVenCode from vendor where  iid in ({0})",
                whereFromAuthSrv);
            if (field.ToLower() == "cvenname")
            //return string.Format(
            //    "Vendor.cVenCode in ({0})", wh);
            {
                wh = string.Format(
                       "select cvenname from Vendor where iid in ({0})",
                       whereFromAuthSrv);
                return string.Format(
                        "({0} in ({1}) or isnull({0},'') = '')", field, wh);
            }
            //return string.Format("{0} in ({1} or isnull({0},'') = '')", field, wh);
            return string.Format("({0} in ({1}) or isnull({0},'') = '')", field, wh);
        }
    }
}
