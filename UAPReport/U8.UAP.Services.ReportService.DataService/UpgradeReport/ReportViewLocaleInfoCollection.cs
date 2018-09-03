using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class ReportViewLocaleInfoCollection : CollectionBase
	{
		private UpgradeReportView ReportView = null;

		public ReportViewLocaleInfoCollection(UpgradeReportView reportView)
		{
			this.ReportView = reportView;
		}

		public ReportViewLocaleInfo this[int index]
		{
			get{ return (ReportViewLocaleInfo) base.List[index]; }
		}

		public ReportViewLocaleInfo this[ string LocaleID ]
		{
			get
			{ 
				for( int i = 0; i < this.Count; i++ )
				{
					ReportViewLocaleInfo reportViewLocaleInfo = this[i];
					if( reportViewLocaleInfo.LocaleID.ToUpper() == LocaleID.ToUpper() )
						return reportViewLocaleInfo; 
				}
				return this[ "zh-CN" ];
			}
		}

		public void Add( ReportViewLocaleInfo item )
		{
			base.List.Add(item);
		}
	
		public void Remove( ReportViewLocaleInfo item )
		{
			base.List.Remove(item);
		}
	}
}
