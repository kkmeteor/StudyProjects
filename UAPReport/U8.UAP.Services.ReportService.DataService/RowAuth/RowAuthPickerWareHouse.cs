/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 作用:仓库相关的权限处理
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	class RowAuthPickerWareHouse : RowAuthPickerAbstract
	{
		public RowAuthPickerWareHouse()
		{
			this.Key = "WareHouse";
			this.AddHitKey(new string[]{"cWhCode", "cWareHouseCode", "cWhName",});
		}
		
		protected override string GetWhereBy(string field, string whereFromAuthSrv)
		{
            if (_bhandled)
                return null;
            _bhandled = true;
			if (field.ToLower() == "cwhname")
				return string.Format(
					"(wareHouse.cWhCode in ({0}) or isnull(wareHouse.cWhCode,'')='')", whereFromAuthSrv);
			return string.Format("({0} in ({1}) or isnull({0},'')='')", field, whereFromAuthSrv);
		}
	}
}
