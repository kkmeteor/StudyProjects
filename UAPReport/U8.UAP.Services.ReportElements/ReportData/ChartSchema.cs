using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UFIDA.U8.UAP.Services.ReportResource;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class ChartSchemas
    {
        private Hashtable _schemas;//deferent groups
        [NonSerialized]
        private ChartSchema _current;

        public ChartSchemas()
        {
            _schemas = new Hashtable();
        }

        public bool Contains(string id)
        {
            return _schemas.Contains(id);
        }

        private void CheckGroupLevels(int levels)
        {
            if (levels > 0)
            {
                ArrayList al = new ArrayList();
                foreach (int level in _current.Items.Keys)
                {
                    if ((_current.Items[level] as ChartSchemaItem).Level > levels)
                        al.Add(level);
                }
                foreach (int level in al)
                    _current.DeleteChart(level);
            }
        }

        public int GetMaxLevel()
        {
            int maxLevel = 1;

            foreach (DictionaryEntry chartSchema in _schemas)
            {
                foreach (DictionaryEntry item in (chartSchema.Value as ChartSchema).Items)
                {
                    maxLevel = (item.Value as ChartSchemaItem).Level;
                    if (maxLevel > 1)
                        return maxLevel;
                }
            }
            return maxLevel;
        }

        public void SetCurrentGroupChart(string id,int levels)
        {
            if (_schemas.Contains(id))
            {
                _current = _schemas[id] as ChartSchema;
                CheckGroupLevels(levels);
            }
            else
            {
                _current = new ChartSchema(id);
                _schemas.Add(id, _current);
            }
        }

        public ChartSchema CurrentGroupChart
        {
            get
            {
                if (_current == null)
                    throw new Exception("please call SetCurrentGroupChart(string id) firstly......");
                return _current;
            }
            set
            {
                _current = value;
            }
        }

        #region To / From Strings
        public string ToStrings()
        {
            using(MemoryStream fs = new MemoryStream())            
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, this);
                    return Convert.ToBase64String(fs.ToArray());
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
        }

        public static ChartSchemas FromStrings(string s)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(s)))
            {
                try
                {
                    ChartSchemas report;
                    BinaryFormatter formatter = new BinaryFormatter();
                    report = (ChartSchemas)formatter.Deserialize(ms);
                    return report;
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
        }

        public static ChartSchemas FromBytes(byte[] b)
        {
            using (MemoryStream ms = new MemoryStream(b))
            {
                try
                {
                    ChartSchemas report;
                    BinaryFormatter formatter = new BinaryFormatter();
                    report = (ChartSchemas)formatter.Deserialize(ms);
                    return report;
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
        }
        #endregion
    }

    [Serializable]
    public class ChartSchema
    {
        private string _id;//groupid
        private Hashtable _items;//deferent levels
        [NonSerialized]
        private ChartSchemaItem  _current;
        public ChartSchema(string id)
        {
            _id = id;
            _items = new Hashtable();
        }

        #region private
        private ChartSchemaItem CurrentLevelChart
        {
            get
            {
                if (_current == null)
                    throw new Exception("please call SetCurrentLevelChart(int level) firstly......");
                return _current;
            }
            set
            {
                _current = value;
            }
        }
        
        private void SetCurrentLevelChart(int level)
        {
            if (_items.Contains(level))
                _current = _items[level] as ChartSchemaItem;
            else
            {
                _current = new ChartSchemaItem(level);
                _items.Add(level, _current);
            }
        }

        private string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        private void SetCaptionPostfix(ChartSchemaItemAmong csia)
        {
            ICollection ids = this.IDs(this.CurrentLevelChart.Level);
            int index = 1;
            bool bfind = false;
            while(true )
            {
                bfind = false;
                foreach (string id in ids)
                {
                    ChartSchemaItemAmong tmp = this.CurrentLevelChart[id];
                    if (tmp.Caption == csia.Caption + " " + index.ToString())
                    {
                        bfind = true;
                        index++;
                        break;
                    }
                }
                if (!bfind)
                {
                    csia.Caption += " " + index.ToString();
                    break;
                }
            }
        }

        #endregion

        #region public
        public ChartSchemaItemAmong AddAChart(int level)
        {
            this.SetCurrentLevelChart(level);
            ChartSchemaItemAmong csia = new ChartSchemaItemAmong();
            csia.Caption = U8ResService.GetResStringEx("U8.Report.ChartSchema");
            SetCaptionPostfix(csia);
            this.CurrentLevelChart.Add(csia );
            return csia;
        }

        public ChartSchemaItemAmong AddAChart(int level,string id)
        {
            this.SetCurrentLevelChart(level);
            ChartSchemaItemAmong csia = new ChartSchemaItemAmong(this[level, id]);
            csia.Caption = U8ResService.GetResStringEx("U8.Report.ChartSchema");
            SetCaptionPostfix(csia);
            this.CurrentLevelChart.Add(csia);
            return csia;
        }

        public void DeleteChart(int level)
        {
            if (_current!=null && _current.Level == level)
                _current = null;
            _items.Remove(level);
        }

        public void DeleteAChart(int level, string id)
        {
            this.SetCurrentLevelChart(level);
            this.CurrentLevelChart.Remove(id);
            if (this.CurrentLevelChart.Count == 0)
                _items.Remove(level);
        }

        public Hashtable Items
        {
            get
            {
                return _items;
            }
        }

        public ICollection IDs(int level)
        {
            this.SetCurrentLevelChart(level);
            return this.CurrentLevelChart.IDs;
        }

        public string DefaultID(int level)
        {
            this.SetCurrentLevelChart(level);
            return this.CurrentLevelChart.DefaultID;
        }

        public void SetDefaultID(int level, string id)
        {
            this.SetCurrentLevelChart(level);
            this.CurrentLevelChart.DefaultID = id;
        }

        public ChartSchemaItemAmong this[int level,string id]
        {
            get
            {
                if (!Contains(level))
                    throw new Exception(U8ResService.GetResStringEx("U8.Report.ChartNotDefine"));
                return (_items[level] as ChartSchemaItem)[id] ;
            }
        }

        public bool Contains(int level)
        {
            return _items.Contains(level);
        }
        #endregion
    }

    [Serializable]
    public class ChartSchemaItem:ISerializable
    {
        private int _level;
        private Hashtable _items;//deferent schemas
        private string _defaultid;

        private ChartSchemaItem()
        {
        }

        protected  ChartSchemaItem(SerializationInfo info, StreamingContext context)
        {
            try
            {
                int version = info.GetInt32("Version");
                _level = info.GetInt32("Level");
                if (version == 1)
                {
                    _items = new Hashtable();
                    ChartSchemaItemAmong sia = new ChartSchemaItemAmong();
                    sia.ChartXml= info.GetString("ChartXML");
                    sia.Source= (ArrayList)info.GetValue("Source", typeof(ArrayList));
                    sia.OrderType= info.GetInt32("OrderType");
                    sia.TopRank = info.GetInt32("TopRank");
                    sia.Caption = U8ResService.GetResStringEx("U8.Report.ChartSchema") + " 1";
                    _defaultid = sia.ID;
                    _items.Add(sia.ID, sia);
                }
                else if (version == 2)
                {
                    _items = (Hashtable)info.GetValue("Items", typeof(Hashtable));
                    _defaultid = info.GetString("DefaultID");
                }
            }
            catch
            {
            }
        }

        public ChartSchemaItem(int level)
        {
            _level = level;
            _items = new Hashtable();
        }

        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }

        public string DefaultID
        {
            get
            {
                return _defaultid;
            }
            set 
            {
                _defaultid = value;
            }
        }

        public ICollection IDs
        {
            get
            {
                return _items.Keys;
            }
        }

        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

        public Hashtable Items
        {
            get
            {
                return _items  ;
            }
            set
            {
                _items  = value;
            }
        }

        public ChartSchemaItemAmong DefaultSchemaChart
        {
            get
            {
                if (string.IsNullOrEmpty(_defaultid))
                {
                    ChartSchemaItemAmong tmp = null;
                    foreach (string key in _items.Keys)
                    {
                        tmp = this[key];
                        break;
                    }
                    return tmp;
                }
                else
                {
                    return this[_defaultid];
                }
            }
        }

        public ChartSchemaItemAmong this[string id]
        {
            get
            {
                if(string.IsNullOrEmpty(id))
                    return DefaultSchemaChart; 
                else
                    return _items[id] as ChartSchemaItemAmong ;
            }
        }

        public void Add(ChartSchemaItemAmong csia)
        {
            _items.Add(csia.ID, csia);
        }

        public void Remove(string id)
        {
            _items.Remove(id);
        }

        #region ISerializable 成员

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", 2);
            info.AddValue("Level", _level);
            info.AddValue("Items", _items );
            info.AddValue("DefaultID", _defaultid);
        }

        #endregion
    }

    [Serializable]
    public class ChartSchemaItemAmong : ISerializable
    {
        private string _id;
        private string _caption;
        private string _encaption;
        private string _twcaption;
        private string _chartxml;
        private ArrayList _source;
        private int _ordertype = 0;//0-not order 1-asc -1 desc
        private int _toprank = 0;//0-not top n- top n

        #region 12.0 为了任意布局添加的
        private Point _position;

        public Point Position
        {
            get { return _position; }
            set { _position = value; }
        }
        private int _width;

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }
        private int _height;

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private string _dataDependGroupId;

        public string DataDependGroupId
        {
            get { return _dataDependGroupId; }
            set { _dataDependGroupId = value; }
        }
        private bool showData = true;

        public bool ShowData
        {
            get { return showData; }
            set { showData = value; }
        }
        private bool showPercent = true;

        public bool ShowPercent
        {
            get { return showPercent; }
            set { showPercent = value; }
        }
    
        #endregion
        public ChartSchemaItemAmong()
        {
            _id = Guid.NewGuid().ToString();
            _caption = "";
        }
      

        public ChartSchemaItemAmong(ChartSchemaItemAmong csia):this()
        {
            _id = Guid.NewGuid().ToString();
            _caption = "";
            _chartxml = csia.ChartXml;
            if (csia.Source==null )
                throw new ResourceReportException("请保存当前方案后复制!");
            _source = csia.Source.Clone() as ArrayList ;
            _ordertype = csia.OrderType;
            _toprank = csia.TopRank;
        }

        public ChartSchemaItemAmong(string caption,string chartxml,ArrayList source,int ordertype,int toprank):this()
        {
            _caption = caption;
            _chartxml = chartxml;
            _source = source;
            _ordertype = ordertype;
            _toprank = toprank;
        }

        protected  ChartSchemaItemAmong(SerializationInfo info, StreamingContext context)
        {
            int version = info.GetInt32("Version");
            if (version >= 2)
            {
                _id = info.GetString ("ID");
                _caption = info.GetString("Caption");
                _encaption = info.GetString("EnCaption");
                _twcaption = info.GetString("TwCaption");
                _chartxml = info.GetString("ChartXML");
                _source = (ArrayList)info.GetValue("Source", typeof(ArrayList));
                _ordertype = info.GetInt32("OrderType");
                _toprank = info.GetInt32("TopRank");
            }
            if (version ==3)
            {
                _dataDependGroupId = info.GetString("DataDependGroupId");
                _position = (Point)info.GetValue("Position", typeof(Point));
                _width = info.GetInt32("Width");
                _height = info.GetInt32("Height");
            }

        }

        public string Caption
        {
            get
            {
                if(System.Threading.Thread.CurrentThread.CurrentUICulture.Name=="zh-TW")
                    return (string.IsNullOrEmpty(_twcaption)?_caption:_twcaption );
                else if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                    return (string.IsNullOrEmpty(_encaption) ? _caption : _encaption);
                else 
                    return _caption;
            }
            set
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "zh-TW")
                {
                    _twcaption = value;
                    if (string.IsNullOrEmpty(_caption))
                        _caption = value;
                }
                else if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                {
                    _encaption = value;
                    if (string.IsNullOrEmpty(_caption))
                        _caption = value;
                }
                else
                    _caption=value;
            }
        }

        public int OrderType
        {
            get
            {
                return _ordertype;
            }
            set
            {
                _ordertype = value;
            }
        }

        public int TopRank
        {
            get
            {
                return _toprank;
            }
            set
            {
                _toprank = value;
            }
        }

        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public ArrayList Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        public string ChartXml
        {
            get
            {
                return _chartxml;
            }
            set
            {
                _chartxml = value;
            }
        }

        #region ISerializable 成员

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", 3);
            info.AddValue("ID", _id);
            info.AddValue("Caption", _caption);
            info.AddValue("EnCaption", _encaption);
            info.AddValue("TwCaption", _twcaption);
            info.AddValue("ChartXML", _chartxml);
            info.AddValue("Source", _source);
            info.AddValue("OrderType", _ordertype);
            info.AddValue("TopRank", _toprank);
            info.AddValue("Position", _position);
            info.AddValue("Width", _width);
            info.AddValue("Height", _height);
            info.AddValue("DataDependGroupId", _dataDependGroupId);
        }

        #endregion
    }
}
