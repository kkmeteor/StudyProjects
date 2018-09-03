using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using UFIDA.U8.UAP.Services.ReportFilterService;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class RowData:IDisposable,ICloneable,IRelateData,ISerializable,IKeyToObject
    {
        protected  SimpleHashtable _data;
        //[NonSerialized]
        protected SqlDataReader _reader;          
        protected SemiRow _semirow;
        protected bool _bhandled = false;
        protected RowData _minorrow;
        protected SimpleArrayList _columns;

        public RowData()
        {
            _data = new SimpleHashtable();
        }

        public RowData(SqlDataReader reader):this()
        {
            _reader = reader;
            //for (int i = 0; i < reader.FieldCount; i++)
            //    _data.Add(reader.GetName(i), reader.GetValue(i));
        }

        public RowData(RowData rd):this()
        {
            foreach (string key in rd.Keys)
                _data.Add(key, rd[key]);
        }

        protected RowData(SerializationInfo info, StreamingContext context)
        {
            _data = (SimpleHashtable)info.GetValue("Data", typeof(SimpleHashtable));
        }

        public SimpleHashtable InnerData
        {
            get
            {
                return _data;
            }
        }

        public ICollection Keys
        {
            get
            {
                return _data.Keys;
            }
        }

        public bool bHandled
        {
            get
            {
                return _bhandled;
            }
            set
            {
                _bhandled = value;
            }
        }

        public int Level
        {
            get
            {
                if (_semirow != null)
                    return _semirow.Level;
                return 0;
            }
        }

        public SemiRow SemiRow
        {
            get
            {
                return _semirow;
            }
            set
            {
                _semirow = value;
            }
        }

        public RowData MinorRow
        {
            get
            {
                return _minorrow;
            }
            set
            {
                _minorrow = value;
            }
        }

        public SimpleArrayList Columns
        {
            get
            {
                return _columns;
            }
            set
            {
                _columns = value;
            }
        }

        public virtual  object this[string name]
        {
            get
            {
                if (name.ToLower() == "emptycolumn")
                    return "";
                if (_data.Contains(name))
                {
                    return _data[name];
                }
                else if (_reader != null)
                {
                    return _reader[name];
                }
                else
                    return "";
            }
            set
            {
                _data.Add(name, value);
            }
        }

        public bool bClosed
        {
            get
            {
                return _reader == null || _reader.IsClosed || !_reader.HasRows;
            }
        }

        public void Add(string name, object value)
        {
            _data.Add(name, value);
        }

        public void BeforeReaderClosed()
        {
            for (int i = 0; i < _reader.FieldCount; i++)
                _data.Add(_reader.GetName(i), _reader.GetValue(i).ToString());
        }

        #region IDisposable 成员

        public void Dispose()
        {
            _data.Clear();
            _data = null;
            _semirow = null;
            _minorrow = null;
            _reader = null;
        }

        #endregion

        #region ICloneable 成员

        public virtual object Clone()
        {
            return new RowData(this);
        }

        #endregion

        #region IRelateData 成员

        public object GetData(string key)
        {
            // 费用预算产品为了取交叉表的列标题的内容,matfb增加
            if (key.StartsWith("##GetKeys##"))
            {
                return this.Keys;
            }
            return this[key]; 
        }

        #endregion

        #region ISerializable 成员

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //if (_reader != null)
            //{
            //    for (int i = 0; i < _reader.FieldCount; i++)
            //        _data.Add(_reader.GetName(i), _reader.GetValue(i));
            //}
            info.AddValue("Data", _data);
        }

        #endregion
    }
}
