using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class UpgradeReportViewCollection : CollectionBase
	{
		public UpgradeReportViewCollection()
		{
		}

		public UpgradeReportView this[ int index ]
		{
			get{ return ( UpgradeReportView ) base.List[index]; }
		}

		public void Add( UpgradeReportView item )
		{
			base.List.Add( item );
		}
	
		public void Remove( UpgradeReportView item )
		{
			base.List.Remove( item );
		}
	}
}
