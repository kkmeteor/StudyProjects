using System;
using System.Collections;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// MetaCells 的摘要说明。
	/// </summary>
    [Serializable]
	public class DataSources : IDisposable,ICloneable
    {
        [NonSerialized]
        private ArrayList _designkeys;

        private ArrayList _keys;
        private Hashtable _dss;
        public DataSources()
        {
            _keys = new ArrayList();
            _dss = new Hashtable();
        }

        public DataSources(DataSources dss):this()
        {
            foreach (string key in dss.Keys)
                _keys.Add(key);
            foreach (string key in dss.Keys)
                _dss.Add(key.ToLower(), dss[key].Clone());
        }

        public void InitDesignKeys()
        {
            _designkeys = new ArrayList();
        }

        public ICollection DesignKeys
        {
            get
            {
                return _designkeys;
            }
        }

        public DataSource this[int index]
        {
            get
            {
                int i = 0;
                foreach (string key in this.Keys)
                {
                    if (i == index)
                        return this[key];
                    i++;
                }
                return null;
            }
        }

        public ICollection Keys
        {
            get
            {
                return _keys ;
            }
        }

        public int Count
        {
            get
            {
                return _keys.Count;
            }
        }

        public DataSource this[string datasource]
		{
			get
			{
                if (Contains(datasource))
                    return _dss[datasource.ToLower()] as DataSource ;
				return null;
			}
		}

		public DataSource GetByCaption( string caption )
		{
			for (int i = 0; i < _keys.Count; i++)
				if( this[i].Caption.ToLower() == caption.ToLower() )
					return this[i];
			return null;
		}

        public string GetString(string key)
        {
            if (Contains(key))
                return _dss[key.ToLower()].ToString();
            return null;
        }

        public void Add(string key, object value)
        {
            if (Contains(key))
                return;
            _dss.Add(key.ToLower(), value);

            if (_designkeys != null)
                _designkeys.Add(key);

            for (int i = 0; i < _keys.Count; i++)
            {
                if (key.Length > _keys[i].ToString().Length)
                {
                    _keys.Insert(i, key);
                    return;
                }
            }
            _keys.Add(key);
        }

		public void Add( DataSource value )  
		{
            //string key = value.Name.Trim();
            string key = value.Name;//由于其他地方都没加trim因此该地方添加会导致不一致
            Add(key, value);           
		}

		public void Remove( DataSource value )  
		{
            if (Contains(value.Name))
            {
                _dss.Remove(value.Name.ToLower());
                _keys.Remove(value.Name);
            }
		}

        public void Remove(string name)
        {
            if (Contains(name))
            {
                _dss.Remove(name.ToLower());
                _keys.Remove(name);
            }
        }

        public bool Contains(DataSource value)
        {
            return _dss.Contains(value.Name.ToLower());
        }

		public bool Contains( string value )  
		{
            return _dss.Contains(value.ToLower());
		}

        public bool RawContains(string value)
        {
            DataSource ds = this[value];
            if (ds == null)
                return false;
            else
                return ! ds.bAppend;
        }

        public void Clear()
        {
            _keys.Clear();
            _dss.Clear();
            if (_designkeys != null)
                _designkeys.Clear();
        }

        public byte[] ToBytes()
        {
            MemoryStream fs = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
                return fs.ToArray();
            }
            catch (SerializationException ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
        }

        public static DataSources FromBytes(Byte[] bs)
        {
            MemoryStream ms = new MemoryStream(bs);
            try
            {
                DataSources dss;
                BinaryFormatter formatter = new BinaryFormatter();
                dss = (DataSources)formatter.Deserialize(ms);
                return dss;
            }
            catch (SerializationException ex)
            {
                throw ex;
            }
            finally
            {
                ms.Close();
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (_dss != null)
            {
                this._dss.Clear();
                _dss = null;
            }
            if (_keys != null)
            {
                _keys.Clear();
                _keys = null;
            }
            if (_designkeys != null)
            {
                _designkeys.Clear();
                _designkeys = null;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new DataSources(this);
        }

        #endregion


    }
}
