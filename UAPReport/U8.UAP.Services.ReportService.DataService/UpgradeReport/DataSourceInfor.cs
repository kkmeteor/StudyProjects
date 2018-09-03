using System;
using System.Collections.Generic;
using System.Text;
using UFIDA.U8.UAP.Services.BizDAE.Elements;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class DataSourceInfor
	{
		public bool				IsShouldSave		= true;
		public string			MetaID				= string.Empty;
		public string			Name				= string.Empty;
		public string			ProjectNo			= string.Empty;
		public string			SubNo				= string.Empty;
		public string			MetaInfo			= string.Empty;
		public string			Description			= string.Empty;
		public BusinessObject	DataSourceBO		= null;
		
		public DataSourceInfor() { }
	}
}
