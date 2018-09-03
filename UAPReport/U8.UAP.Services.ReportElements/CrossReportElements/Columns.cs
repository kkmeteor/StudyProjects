using System;
using System.Collections;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// MultiHeaderCollection 的摘要说明。
	/// </summary>
	public class Columns : CollectionBase
	{
		public IMapName this[int index]
		{
			get {  return InnerList[index] as IMapName;  }
		}

		internal int AddHeader(IMapName c)
		{
			if (Contains(c))
				return IndexOf(c);
			for (int i=0; i<Count; i++)
			{
				if((c as Cell).X <(this[i] as Cell).X )
				{
					AddAt(c,i);
					return i;
				}
			}
			return InnerList.Add(c);
		}

        internal int Add(IMapName c)
        {
            if (Contains(c))
                return IndexOf(c);
            
            return InnerList.Add(c);
        }

		internal void AddAt(IMapName c, int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(c))
				return;

			InnerList.Insert(index, c);
		}

		public bool Contains(IMapName c)
		{
			return InnerList.Contains(c);
		}

		internal void Dispose()
		{
			//			for (int i=Count - 1; i>=0; i--)
			//				this[i].Dispose();
		}

		public int IndexOf(IMapName c)
		{
			if (!Contains(c))
				return -1;
			else
				return InnerList.IndexOf(c);
		}

		internal void Remove(IMapName c)
		{
			if (!Contains(c))
				return;

			InnerList.Remove(c);
		}
	}
}
