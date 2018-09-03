/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:部门相关的权限处理
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
	class RowAuthPickerDepartment : RowAuthPickerAbstract
	{
		public RowAuthPickerDepartment()
		{
			this.Key = "Department";
			this.AddHitKey(new string[]{
				"cDepCode", "cAccDep", "cdept_id",
                "cDept_Num", 
				"cDeptCode", "cDeptID", "DeptID", "sDeptNum", "sNum",
				String4Report.GetString("U8.AA.Report.0579"), "cDepName", 
				String4Report.GetString("U8.AA.Report.0578"),
			});
		}

		protected override string GetWhereBy(string field, string whereFromAuthSrv)
		{
            if (_bhandled)
                return null;
            _bhandled = true;
            //if (field.ToLower() == "cdepname"
            //    || field.ToLower() == String4Report.GetString("U8.AA.Report.0579").ToLower())
                //return string.Format(
                //    " (Department.cDepCode in ({0}) or isnull(Department.cDepCode,'')='') ", whereFromAuthSrv);
            if (field.ToLower() == "cdepname")
            {
                string wh = string.Format(
                    "select cdepname from department where cdepcode in ({0})",
                    whereFromAuthSrv);
                return string.Format(
                        "(cdepname in ({0}) or isnull(cdepname,'') = '')", wh);
            }
            //return string.Format(
            //  " (cDepCode in ({0}) or isnull(cDepCode,'')='') ", whereFromAuthSrv);
            else if (field.ToLower() == String4Report.GetString("U8.AA.Report.0579").ToLower())
            {
                string wh = string.Format(
                    "select cdepname from department where cdepcode in ({0})",
                    whereFromAuthSrv);
                return string.Format(
                        "({0} in ({1}) or isnull({0},'') = '')", field,wh);
            }
                //return string.Format(
                // " ({0} in ({1}) or isnull({0},'')='') ", String4Report.GetString("U8.AA.Report.0578").ToLower(), whereFromAuthSrv);
			return string.Format("({0} in ({1}) or isnull({0} ,'') ='' )", field, whereFromAuthSrv);
		}
	}
}
