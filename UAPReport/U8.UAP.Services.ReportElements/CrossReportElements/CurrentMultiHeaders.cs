using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CurrentColumnHeaders 的摘要说明。
	/// </summary>
	public class CurrentMultiHeaders
	{
		private Hashtable _innerhash;
		public event EventHandler BeforeRemove;
		public CurrentMultiHeaders()
		{
			_innerhash=new Hashtable();
		}

		public IMultiHeader this[int level]
		{
			get
			{
				return _innerhash[level] as IMultiHeader;
			}
		}

        public void Clear()
        {
            _innerhash.Clear();
        }
		public int Count
		{
			get
			{
				return _innerhash.Count;
			}
		}

		public void Add(int level,IMultiHeader mh)
		{
			if(_innerhash.Contains(level))
				_innerhash[level]=mh;
			else
				_innerhash.Add(level,mh);
		}

		public void RemoveDirectly(int level)
		{
			_innerhash.Remove(level);
		}

		public void Remove(int level)
		{
			if(BeforeRemove!=null)
				BeforeRemove(this,new RemoveHeaderArgs(level));
			_innerhash.Remove(level);
		}
	}

	public class RemoveHeaderArgs : EventArgs
	{
		private int _level;
		public RemoveHeaderArgs(int level):base()
		{
			_level=level;
		}

		public int Level
		{
			get
			{
				return _level;
			}
			set
			{
				_level=value;
			}
		}
	}
}
