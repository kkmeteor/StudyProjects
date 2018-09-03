using System;
using System.Collections;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// MultiHeaderCollection 的摘要说明。
	/// </summary>
	public class MultiHeaders : CollectionBase,ICloneable
	{
		public IMultiHeader this[int index]
		{
			get {  return InnerList[index] as IMultiHeader;  }
		}

		internal int Add(IMultiHeader c)
		{
			if (Contains(c))
				return IndexOf(c);
			for (int i=0; i<Count; i++)
			{
				if((c as Cell).Y <(this[i] as Cell).Y )
				{
					AddAt(c,i);
					return i;
				}
			}
			return InnerList.Add(c);
		}

		internal void AddAt(IMultiHeader c, int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(c))
				return;

			InnerList.Insert(index, c);
		}

		internal void AddAt(IMultiHeader c, IMultiHeader beforeMultiHeader)
		{
			if (!Contains(beforeMultiHeader))
				return;

			if (Contains(c))
				return;

			AddAt(c, IndexOf(beforeMultiHeader));
		}

		public bool Contains(IMultiHeader c)
		{
			return InnerList.Contains(c);
		}

		internal void Dispose()
		{
			//			for (int i=Count - 1; i>=0; i--)
			//				this[i].Dispose();
		}

		public int IndexOf(IMultiHeader c)
		{
			if (!Contains(c))
				return -1;
			else
				return InnerList.IndexOf(c);
		}

		internal void Remove(IMultiHeader c)
		{
			if (!Contains(c))
				return;

			InnerList.Remove(c);
		}
		#region ICloneable 成员

		public object Clone()
		{
			MultiHeaders mhs=new MultiHeaders();
			foreach(IMultiHeader value in List)
			{
				mhs.Add((value as ICloneable).Clone() as IMultiHeader);
			}
			return mhs;
		}

		#endregion
	}
}
