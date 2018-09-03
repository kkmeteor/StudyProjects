using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class ReportLocaleInfoCollection : CollectionBase
	{
		private UpgradeReport	Report = null;

		public ReportLocaleInfoCollection()
		{
		}

		internal ReportLocaleInfoCollection( UpgradeReport report )
		{
			this.Report = report;
		}

		public ReportLocaleInfo this[ int index ]
		{
			get{ return (ReportLocaleInfo) base.List[index]; }
		}

		public ReportLocaleInfo this[ string LocaleID ]
		{
			get
			{ 
				for( int i = 0; i < this.Count; i++ )
				{
					ReportLocaleInfo reportLocaleInfo = this[i];
					if( reportLocaleInfo.LocaleID.ToUpper() == LocaleID.ToUpper() )
						return reportLocaleInfo; 
				}
				return this[ "zh-CN" ];
			}
		}

		public void Add( ReportLocaleInfo item )
		{
			base.List.Add( item );
		}
	
		public void Remove( ReportLocaleInfo item )
		{
			base.List.Remove( item );
		}
	}
}
