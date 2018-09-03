/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:业务员相关的权限处理
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	class RowAuthPickerPerson : RowAuthPickerAbstract
	{
		public RowAuthPickerPerson()
		{
			this.Key = "Person";
			this.AddHitKey(new string[]{"cPersonCode", "cPerson", "cPersonName",});
		}
		
		protected override string GetWhereBy(string field, string whereFromAuthSrv)
		{
            if (_bhandled)
                return null;
            _bhandled = true;
            if (field.ToLower() == "cpersonname")
            {
                string wh = string.Format(
                    "select cPersonName from person where cpersoncode in ({0})",
                    whereFromAuthSrv);
                return string.Format(
                        "({0} in ({1}) or isnull({0},'') = '')", field,wh);
            }
                //return string.Format(
                //    "(Person.cPersonCode in ({0}) or isnull ( Person.cPersonCode,'')='')", whereFromAuthSrv);
                //return string.Format(
                //    "(cPerson in ({0}) or isnull (cPerson,'')='')", whereFromAuthSrv);
			return string.Format("({0} in ({1}) or isnull({0},'')='' )", field, whereFromAuthSrv);
		}
	}
}
