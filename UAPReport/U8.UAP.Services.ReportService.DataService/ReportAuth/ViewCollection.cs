using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ViewCollection ��ժҪ˵����
	/// </summary>
	public class ViewCollection:CollectionBase
	{
		public ViewCollection()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
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
			// TODO: �ڴ˴���ӹ��캯���߼�
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
