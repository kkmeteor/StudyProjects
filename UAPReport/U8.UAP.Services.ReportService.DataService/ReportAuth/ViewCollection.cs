using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ViewCollection 的摘要说明。
	/// </summary>
	public class ViewCollection:CollectionBase
	{
		public ViewCollection()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
		}
		public ViewAuth this[int index]
		{
			get
			{
				return (ViewAuth)base.List[index];
			}
		}

		public void Add(ViewAuth viewguid)
		{
			this.List.Add(viewguid);
		}
	
		public void Remove(ViewAuth viewguid)
		{
			this.List.Remove(viewguid);
		}
	}
	

	public class ReportCollection:CollectionBase
	{
		public ReportCollection()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
		}
		public string this[int index]
		{
			get
			{
				return (string)base.List[index];
			}
		}

		public void Add(string reportguid)
		{
			this.List.Add(reportguid);
		}
	
		public void Remove(string reportguid)
		{
			this.List.Remove(reportguid);
		}
	}
}
