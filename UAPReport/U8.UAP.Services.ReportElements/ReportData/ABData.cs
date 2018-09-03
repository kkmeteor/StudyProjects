using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public abstract class ABCbase
    {
        [NonSerialized]
        protected Hashtable _datas;
        protected Hashtable _rawdatas;
        protected int _baselevel;

        public ABCbase()
        {
        }

        public ABCbase(int grouplevels, bool bshowdetail)
        {
            _datas = new Hashtable();
            for (int i = 1; i < grouplevels; i++)
                _datas.Add(i, null);
            if (bshowdetail)
                _datas.Add(grouplevels , null);
        }

        public ABCbase(ABCbase abc)
        {
            _baselevel = abc.BaseLevel;
            _datas=new Hashtable();
            foreach (int key in abc.Keys)
            {
                Hashtable ht = abc[key];
                if (ht != null)
                    _datas.Add(key, ht.Clone());
                else
                    _datas.Add(key, null);
            }
        }

        public Hashtable this[int level]
        {
            get
            {
                return _datas[level] as Hashtable;
            }
        }

        public int BaseLevel
        {
            get
            {
                return _baselevel;
            }
        }

        public ICollection Keys
        {
            get
            {
                return _datas.Keys;
            }
        }

        public object this[int level,string colname]
        {
            get
            {
                if (_datas.Contains(level))
                    return (_datas[level] as Hashtable)[colname.ToLower()];
                else
                    return null;
            }
        }

        private void Add(int level, Hashtable hash)
        {
            _datas.Add(level, hash);
        }

        public void SaveState()
        {
            if (_datas.Count == 0)
                return;
            _rawdatas = new Hashtable();
            foreach (int key in _datas.Keys)
            {
                Hashtable ht = _datas[key] as Hashtable;
                if (ht != null)
                    _rawdatas.Add(key, ht.Clone());
                else
                    _rawdatas.Add(key, null);
            }
        }

        public void RestoreState()
        {
            _datas = _rawdatas;
        }

        public abstract   void Add(string colname, object value);

        public abstract void StartNext(int fromlevel);

    }

    [Serializable]
    public class AccumulateData : ABCbase 
    {
        public AccumulateData(int grouplevels, bool bshowdetail)
            : base(grouplevels, bshowdetail)
        {
            //if (bshowdetail)
            //    _baselevel = grouplevels;
            //else
            //    _baselevel = grouplevels - 1;
            if (!_datas.Contains(0))
                _datas.Add(0, null);
        }

        public AccumulateData(AccumulateData ad)
            : base(ad)
        {
        }

        public override  void Add(string colname, object value)
        {
            colname = colname.ToLower();            
            for (int level = 0; level <= _datas.Count; level++)
            {
                Hashtable ht = null;
                if (_datas.Contains(level))
                {
                    if (_datas[level] == null)
                    {
                        ht = new Hashtable();
                        _datas[level] = ht;
                    }
                    else
                        ht = _datas[level] as Hashtable;
                }

                if (ht != null)
                {
                    if (ht.Contains(colname))
                    {
                        ht[colname] = UFConvert.ToDouble(ht[colname]) + UFConvert.ToDouble(value);
                    }
                    else
                        ht.Add(colname, value);
                }
            }
        }

        public override  void StartNext(int fromlevel)
        {
            if (fromlevel == 1)
                fromlevel = 2;
            for (int i = fromlevel ; i <= _datas.Count; i++)
            {
                if (_datas[i] != null)
                    (_datas[i] as Hashtable).Clear();
            }
        }
    }

    [Serializable]
    public class BalanceData:ABCbase
    {
        public BalanceData(int grouplevels, bool bshowdetail)
            : base(grouplevels, bshowdetail)
        {
            if (bshowdetail)
                _baselevel = grouplevels-1;
            else
                _baselevel = grouplevels -2;

            if(!_datas.Contains(0))
                _datas.Add(0, null);
        }

        public BalanceData(BalanceData bd)
            : base(bd)
        {
        }

        public override void Add(string colname, object value)
        {
            colname = colname.ToLower();
            
            for (int level = 0; level < _datas.Count; level++)
            {
                Hashtable ht = null;
                if (_datas.Contains(level))
                {
                    if (_datas[level] == null)
                    {
                        ht = new Hashtable();
                        _datas[level] = ht;
                    }
                    else
                        ht = _datas[level] as Hashtable;
                }

                if (ht!=null && level > _baselevel)
                {
                    if (ht.Contains(colname))
                        ht[colname] = value;
                    else
                        ht.Add(colname, value);
                }
            }
        }

        public override  void StartNext(int fromlevel)
        {
            if (fromlevel == _baselevel + 1)
            {
                Hashtable ht = _datas[fromlevel] as Hashtable;
                if (ht != null)
                {
                    for (int i =0; i <= _baselevel; i++)
                    {
                        Hashtable httmp = _datas[i] as Hashtable;
                        foreach (string key in ht.Keys)
                        {
                            if (httmp.Contains(key))
                                httmp[key] = UFConvert.ToDouble(httmp[key]) + UFConvert.ToDouble(ht[key]);
                            else
                                httmp.Add(key, ht[key]);
                        }
                    }
                }
            }

            for (int i = fromlevel; i < _datas.Count; i++)
            {
                if (_datas[i] != null)
                    (_datas[i] as Hashtable).Clear();
            }
        }
    }

    [Serializable]
    public class ComplexData : ABCbase
    {
        public ComplexData(int grouplevels, bool bshowdetail)
            : base(grouplevels, bshowdetail)
        {
            //if (bshowdetail)
            //    _baselevel = grouplevels;
            //else
            //    _baselevel = grouplevels - 1;

            if (!_datas.Contains(0))
                _datas.Add(0, null);
        }

        public ComplexData(ComplexData cd)
            : base(cd)
        {
        }

        public override void Add(string colname, object value)
        {
            colname = colname.ToLower();

            for (int level = 0; level < _datas.Count; level++)
            {
                Hashtable ht = null;
                if (_datas.Contains(level))
                {
                    if (_datas[level] == null)
                    {
                        ht = new Hashtable();
                        _datas[level] = ht;
                    }
                    else
                        ht = _datas[level] as Hashtable;
                }

                if (ht != null)
                {
                    if (ht.Contains(colname))
                        ht[colname] = UFConvert.ToDouble(ht[colname]) + UFConvert.ToDouble(value);
                    else
                        ht[colname] = value;
                }
            }
        }

        public override void StartNext(int fromlevel)
        {
            for (int i = fromlevel; i < _datas.Count; i++)
            {
                if (_datas[i] != null)
                    (_datas[i] as Hashtable).Clear();
            }
        }
    }

    [Serializable]
    public class RowBalance : RowData
    {
        private int _startindex=0;
        private int _currnetindex=-1;
        public RowBalance()
        {
        }

        public RowBalance(RowBalance rb)
            : base(rb)
        {
            _startindex = rb.StartIndex;
            _currnetindex = rb.CurrentIndex;
        }

        protected RowBalance(SerializationInfo info, StreamingContext context):base(info,context )
        {
            _startindex = info.GetInt32("StartIndex");
            _currnetindex = info.GetInt32("CurrentIndex");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("StartIndex", _startindex);
            info.AddValue("CurrentIndex", _currnetindex);
        }

        public int StartIndex
        {
            get
            {
                return _startindex;
            }
            set
            {
                _startindex = value < 0 ? 0 : value;
            }
        }

        public int CurrentIndex
        {
            get
            {
                return _currnetindex;
            }
            set
            {
                _currnetindex = value;
            }
        }
    }

    [Serializable]
    public class PageInfo
    {
        private RowBalance _rowbalance;
        private AccumulateData _accumulate;
        private BalanceData _balance;
        private ComplexData _complex;   
        private int _pageindex;
        private RowData _rawdata;
        private Hashtable _globalvalues;

        public PageInfo()
        {
        }

        public PageInfo(bool bshowdetail, int grouplevels)
        {
            _rowbalance = new RowBalance();
            _accumulate = new AccumulateData(grouplevels, bshowdetail);
            _balance = new BalanceData(grouplevels, bshowdetail);
            _complex = new ComplexData(grouplevels, bshowdetail);
            _pageindex = 0;
        }

        public PageInfo(int pageindex,PageInfo pi)
        {
            _pageindex = pageindex;
            if (pi.GlobalValues != null)
            {
                foreach (string key in pi.GlobalValues.Keys)
                    AddGlobal(key, pi.GlobalValues[key]);
            }
            _rowbalance = new RowBalance(pi.RowBalance );
            pi.Accmulate.RestoreState();
            _accumulate = new AccumulateData(pi.Accmulate );
            pi.Balance.RestoreState();
            _balance = new BalanceData(pi.Balance );
            pi.Complex.RestoreState();
            _complex = new ComplexData(pi.Complex );
            _rawdata = pi.RawData;
        }

        public Hashtable GlobalValues
        {
            get
            {
                return _globalvalues;
            }
        }

        public void AddGlobal(string key, object value)
        {
            if (_globalvalues == null)
                _globalvalues = new Hashtable();
            if (_globalvalues.Contains(key))
                _globalvalues[key] = value;
            else
                _globalvalues.Add(key, value);
        }

        public object GetGlobal(string key)
        {
            return _globalvalues[key];
        }

        public RowData RawData
        {
            get
            {
                return _rawdata;
            }
            set
            {
                _rawdata = value;
            }
        }

        public int PageIndex
        {
            get
            {
                return _pageindex;
            }
            set
            {
                _pageindex = value;
            }
        }

        public RowBalance RowBalance
        {
            get
            {
                return _rowbalance;
            }
        }

        public AccumulateData Accmulate
        {
            get
            {
                return _accumulate;
            }
        }

        public BalanceData Balance
        {
            get
            {
                return _balance;
            }
        }

        public ComplexData Complex
        {
            get
            {
                return _complex;
            }
        }
    }

    [Serializable]
    public class PageInfos
    {
        private Hashtable _pis;
        private PageInfo _globalvalues;
        [NonSerialized]
        private int _appreciateindex;
        [NonSerialized]
        private int _pagebygroupindex=-1;
        public PageInfos()
        {           
        }

        public bool bCalcPageByPage
        {
            get
            {
                return _pis != null;
            }
        }

        public void Add(PageInfo pi)
        {
            if(_pis==null)
                _pis = new Hashtable();

            if (!_pis.Contains(pi.PageIndex))
                _pis.Add(pi.PageIndex, pi);
            else
                _pis[pi.PageIndex] = pi;
        }

        public PageInfo BeginAGlobalValuesCache()
        {
            if(_globalvalues==null)
                _globalvalues = new PageInfo();
            return _globalvalues;
        }

        public int PageByGroupIndex
        {
            get
            {
                return _pagebygroupindex;
            }
            set
            {
                _pagebygroupindex = value;
            }
        }

        public int AppreciateIndex
        {
            get
            {
                return _appreciateindex;
            }
            set
            {
                _appreciateindex = value;
            }
        }

        public PageInfo this[int pageindex]
        {
            get
            {
                if (_pis.Contains(pageindex))
                    return _pis[pageindex] as PageInfo;
                return null;
            }
        }

        public int PageIndexMostNear(int pageindex)
        {
            for (int i = pageindex - 1; i >= 0; i--)
            {
                if(_pis.Contains(i))
                    return i;
            }
            return -1;
        }

        #region bytes
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

        public static PageInfos  FromBytes(Byte[] bs)
        {
            MemoryStream ms = new MemoryStream(bs);
            try
            {
                PageInfos pis;
                BinaryFormatter formatter = new BinaryFormatter();
                pis = (PageInfos)formatter.Deserialize(ms);
                return pis;
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
        #endregion
    }
}
