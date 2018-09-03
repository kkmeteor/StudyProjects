using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// AuthCollection 的摘要说明。
	/// </summary>
	public class AuthCollection:CollectionBase
	{
		public AuthCollection()
		{
			//
			// TODO: 在此处添加构造函数逻辑
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
