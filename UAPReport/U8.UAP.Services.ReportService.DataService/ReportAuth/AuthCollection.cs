using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// AuthCollection ��ժҪ˵����
	/// </summary>
	public class AuthCollection:CollectionBase
	{
		public AuthCollection()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}

		public OperationEnum this[int index]
		{
			get
			{
				return (OperationEnum) base.List[index];
			}
		}

		public void Add(OperationEnum item)
		{
			this.List.Add(item);
		}
	
		public void Remove(OperationEnum item)
		{
			this.List.Remove(item);
		}
	}
}
