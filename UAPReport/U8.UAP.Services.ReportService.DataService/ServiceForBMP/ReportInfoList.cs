using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class ReportInfoList : CollectionBase
	{
		public ReportInfoList()
		{
		}

		public ReportInfo this[int index]
		{
			get{ return this.InnerList[index] as ReportInfo; }
		}

		public  ReportInfo this[string ReportID]
		{
			get
			{
				for( int i=0; i<this.Count; i++ )
				{
					ReportInfo reportInfo = (ReportInfo)List[i];
					if( reportInfo.ReportID.ToUpper() == ReportID.ToUpper() )
						return reportInfo;
				}
				return null;
			}
		}

		public void Add( ReportInfo report )
		{
			if( ! this.InnerList.Contains( report ) )
				this.InnerList.Add(report);
		}

		public void Remove( ReportInfo report )
		{
			if( this.InnerList.Contains(report) )
				this.InnerList.Remove(report);
		}

		public int IndexOf( ReportInfo report )
		{
			return this.InnerList.IndexOf(report);
		}

		public void Insert( int index, ReportInfo reportInfo )  
		{
			List.Insert( index, reportInfo );
		}

		public bool Contains( ReportInfo reportInfo )  
		{
			return List.Contains( reportInfo );
		}

		public bool Contains(string reportID)
		{
			if( this[reportID] != null )
				return true;
			return false;
		}
	}
}
