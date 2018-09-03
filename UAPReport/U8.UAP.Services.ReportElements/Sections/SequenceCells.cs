using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CalculatorCells 的摘要说明。
	/// </summary>
	public class SequenceCells:CollectionBase
	{
		internal Cell this[int index]
		{
			get
			{
				return this.InnerList[index] as Cell;
			}
		}

		public void Add(Cell value)
		{
            if (!(value is ICalculateSequence))
            {
                this.InnerList.Add(value);
                return;
            }
			for(int i=0;i<Count;i++)
			{
                Cell cell = this[i];
                if (!(cell is ICalculateSequence))
                    continue;
				if((value as ICalculateSequence).CalculateIndex < (cell  as ICalculateSequence).CalculateIndex)
				{
					this.InnerList.Insert(i,value);
					return ;
				}
			}
			this.InnerList.Add(value);
		}

		internal Cell GetByName(string name)
		{
			for(int i=0;i<Count;i++)
			{
				if(this[i].Name.ToLower()==name.ToLower())
				{
					return this[i];
				}
			}
			return null;
		}

        internal Cell GetByMap(string mapname)
        {
            for (int i = 0; i < Count; i++)
            {
                Cell mc = this[i];

                if (mc is Calculator && (mc as Calculator).MapName.ToLower() == mapname.ToLower())
                    return mc;
            }
            return null;
        }

        internal bool Contains(string mapname)
        {
            return GetByMap(mapname) != null;
        }
	}

    [Serializable ]
    public class SequenceMaps : CollectionBase
    {
        internal IMapName this[int index]
        {
            get
            {
                return this.InnerList[index] as IMapName;
            }
        }

        public void Add(IMapName value)
        {
            if (!(value is ICalculateSequence))
                return;
            for (int i = 0; i < Count; i++)
            {
                if ((value as ICalculateSequence).CalculateIndex < (this[i] as ICalculateSequence).CalculateIndex)
                {
                    this.InnerList.Insert(i, value);
                    return;
                }
            }
            this.InnerList.Add(value);
        }

        internal IMapName  this[string mapname]
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].MapName .ToLower() == mapname.ToLower())
                    {
                        return this[i];
                    }
                }
                return null;
            }
        }

        public bool Contains(string mapname)
        {
            return this[mapname] != null;
        }

        public void Remove(IMapName map)
        {
            this.InnerList.Remove(map);
        }

        public void Remove(string mapname)
        {
            IMapName map = this[mapname];
            if (map != null)
                Remove(map);
        }
    }

    [Serializable]
    public class SimpleHashtable:ICloneable
    {
        private Hashtable _hash;
        private ArrayList _keys;
        public SimpleHashtable()
        {
            _hash = new Hashtable();
            _keys = new ArrayList();
        }

        public SimpleHashtable(SimpleHashtable sh):this()
        {
            foreach (string key in sh.Keys)
            {
                _hash.Add(key, sh[key]);
                _keys.Add(key);
            }
        }

        public int Count
        {
            get
            {
                return _keys.Count;
            }
        }

        public void Remove(string  key)
        {
            if (Contains(key))
            {
                key = key.ToLower();
                _hash.Remove(key);
                _keys.Remove(key);
            }
        }

        public Hashtable InnerHash
        {
            get
            {
                return _hash;
            }
        }

        public ICollection Keys
        {
            get
            {
                return _keys;
            }
        }

        public object this[string key]
        {
            get
            {
                if (Contains(key))
                    return _hash[key.ToLower()];
                return null;
            }
            set
            {
                if (Contains(key))
                    _hash[key.ToLower()] = value;
            }
        }

        public void Add(string key, object value)
        {
            key = key.ToLower();
            if (!Contains(key))
            {
                _hash.Add(key, value);
                _keys.Add(key);
            }
            else
            {
                _hash[key.ToLower()] = value;
            }
        }

        public bool Contains(string key)
        {
            return _hash.Contains(key.ToLower());
        }

        public void Clear()
        {
            _hash.Clear();
            _keys.Clear();
        }

        #region ICloneable 成员

        public object Clone()
        {
            return new SimpleHashtable(this);
        }

        #endregion
    }

    [Serializable]
    public class SimpleArrayList:IEnumerable
    {
        private ArrayList _list;
        public SimpleArrayList()
        {
            _list = new ArrayList();
        }

        public string this[int index]
        {
            get
            {
                return _list[index].ToString();
            }
        }

        public int IndexOf(string key)
        {
            if(Contains(key))
            {
                key = key.ToLower().Trim();
                return _list.IndexOf(key);
            }           
            
            return -1;
        }

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public void Add(string key)
        {
            key = key.ToLower().Trim();
            if (key == "emptycolumn" || key=="")
                return;
            if (!Contains(key))
                _list.Add(key);
        }

        public void Insert(int index, string key)
        {
            if (!Contains(key))
                _list.Insert(index, key.ToLower().Trim());
        }

		public void Remove( string key )
        {
            if (this.Contains( key ))
				_list.Remove( key.ToLower().Trim() );
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public bool Contains(string key)
        {
            return _list.Contains(key.ToLower().Trim());
        }

        public void Clear()
        {
            _list.Clear();
        }

        public object Clone()
        {
            SimpleArrayList al = new SimpleArrayList();
            foreach (string key in this)
                al.Add(key);
            return al;
        }


        #region IEnumerable 成员

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }
}
