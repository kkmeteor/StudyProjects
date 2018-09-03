using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// GroupSchema ��ժҪ˵����
    /// </summary>
    [Serializable]
    public class GroupSchema : ICloneable, IDisposable
    {
        private string[] _localeIds = new string[] { 
			"zh-CN", "zh-TW", "en-US",
		};

        private ShowStyle _showStyle = ShowStyle.Normal;
        private GroupSchemaItems _schemaitems;
        private bool _bdefault;
        private string _id;
        private Hashtable _names = new Hashtable();
        //private string _currentLocaleId;
        private DateTime _createTime = DateTime.Now;
        private bool _bshowdetail = false;
        private bool _bgroupitemsahead = true;
        private string _switchitem = string.Empty;
        private QuickSortSchema _sortSchema;
        //11.0 add
        private QuickSortSchema _columnSortSchema;//������ʱ��ʹ��
        private GroupSchema _crossRowGroup;//������Ԫ�ص���������
        private bool _bShowCrossNullColumn = true;//�Ƿ���ʾ��������ݶ�Ϊ�յĽ�����
        private bool _bShowHorizonTotal = true;//�Ƿ�������
        // end 

        private Dictionary<string, int> _datedimensions;//positive is to show year;negative is not to show year;
        private string  _guidVersion;
        private string _lastUserGuid;
        private bool _bShowSubTotal = true;
        public readonly String NOGROUPID = "00000000-0000-0000-0000-000000000001";
        public static String NOGROUPCROSSID = "00000000-0000-0000-0000-000000000001";
        public GroupSchema()
        {
            _id = Guid.NewGuid().ToString();
            _schemaitems = new GroupSchemaItems();
            foreach (string localeId in this._localeIds)
                this._names[localeId.ToUpper()] = "";
            //foreach (string localeId in this._localeIds)
            //    this._names[localeId.ToUpper()] = this.GetDefaultName(localeId);
            _datedimensions = new Dictionary<string, int>();
        }

        public GroupSchema(string switchitem, Dictionary<string, int> datedimensions)
            : this()
        {
            _switchitem = switchitem;
            foreach (string key in datedimensions.Keys)
                _datedimensions.Add(key, datedimensions[key]);
        }

        private string GetDefaultName(string localeId)
        {
            return string.Format(
                "{0}({1})",
                String4Report.GetString("δ��������", localeId),
                Util.GetTimeExtension(this._createTime, localeId));
        }

        //������ĵ�ʱ��
        public string GuidVersion
        {
            get { return _guidVersion; }
            set { _guidVersion = value; }
        }

        //��������������Ա��ʶ(Guid)
        public string LastUserGuid
        {
            get { return _lastUserGuid; }
            set { _lastUserGuid = value; }
        }
        //��ǰ�����Ƿ�С��
        public bool bShowSubTotal
        {
            get
            {
                if (this.ShowStyle == ShowStyle.NoGroupSummary)//�۵�չ����ԶΪtrue
                    return true;
                return _bShowSubTotal; 
            }
            set { _bShowSubTotal = value; }
        }

        public bool IsEnbleSubTotal
        {
            get
            {
                if (this.ID == NOGROUPID)
                {
                    return false;
                }
                else if (this.SchemaItems.Count == 1 && !this.bShowDetail)
                    return false;
                else
                    return true;
            }
        }

        public bool bShowCrossNullColumn
        {
            get { return _bShowCrossNullColumn; }
            set { _bShowCrossNullColumn = value; }
        }

        public Dictionary<string, int> DateDimensions
        {
            get
            {
                return _datedimensions;
            }
        }

        public GroupSchema CrossRowGroup
        {
            get { return _crossRowGroup; }
            set { _crossRowGroup = value; }
        }

        public string SwitchItem
        {
            get
            {
                return _switchitem;
            }
            set
            {
                _switchitem = value;
            }
        }

        /// <summary>
        /// �����չ�ַ�ʽ
        /// </summary>
        public ShowStyle ShowStyle
        {
            get { return this._showStyle; }
            set { this._showStyle = value; }
        }

        /// <summary>
        /// �Ƿ�������
        /// </summary>
        public bool BShowHorizonTotal
        {
            get { return _bShowHorizonTotal; }
            set { _bShowHorizonTotal = value; }
        }

        public bool FromCrossItem(GroupSchemaItem gsi)
        {
            bool b = false;
            foreach (string name in gsi.Items)
            {
                string[] ss = System.Text.RegularExpressions.Regex.Split(name, "____");
                int level = Convert.ToInt32(ss[0]);
                if (level > 0)
                    b = true;
                GroupSchemaItem g = this[level];
                if (g == null)
                {
                    g = new GroupSchemaItem(level);
                    this.SchemaItems.AddByLevel(g);
                }
                g.Items.Add(ss[1]);
            }
            return b;
        }

        public bool CrossContain(string cross)
        {
            foreach (GroupSchemaItem gsi in this.SchemaItems)
            {
                foreach (string name in gsi.Items)
                {
                    string[] ss = System.Text.RegularExpressions.Regex.Split(name, "____");
                    if (cross.ToLower() == ss[1].ToLower())
                        return true;
                }
            }
            return false;
        }

        public bool bNoneGroup
        {
            get
            {
                return _id == "00000000-0000-0000-0000-000000000001";
            }
        }

        public bool bShowDetail
        {
            get
            {
                return _bshowdetail;
            }
            set
            {
                _bshowdetail = value;
            }
        }
        public bool bGroupItemsAhead
        {
            get
            {
                return _bgroupitemsahead;
            }
            set
            {
                _bgroupitemsahead = value;
            }
        }

        public void CrossHandle()
        {
            if (this[1] == null)
                this.SchemaItems.Add(new GroupSchemaItem(1));
            if (this[2] == null)
                this.SchemaItems.Add(new GroupSchemaItem(2));
            if (this[3] == null)
                this.SchemaItems.Add(new GroupSchemaItem(3));
        }


        public GroupSchemaItem this[int level]
        {
            get
            {
                // GroupSchemaItem.Level��ʼֵΪ1
                for (int i = 0; i < _schemaitems.Count; i++)
                {
                    GroupSchemaItem gsi = _schemaitems[i];
                    if (gsi.Level == level)
                        return gsi;
                }
                return null;
            }
        }

        public bool Contains(Cell cell)
        {
            bool b = false;
            for (int i = 0; i < _schemaitems.Count; i++)
            {
                GroupSchemaItem gsi = _schemaitems[i];
                b = gsi.Contains(cell.Name) || (cell is IMapName && gsi.Contains((cell as IMapName).MapName));
                if (b)
                    return b;
            }
            return b;
        }

        public bool Contains(string name)
        {
            bool b = false;
            for (int i = 0; i < _schemaitems.Count; i++)
            {
                GroupSchemaItem gsi = _schemaitems[i];
                b = gsi.Contains(name);
                if (b)
                    return b;
            }
            return b;
        }

        public GroupSchemaItems SchemaItems
        {
            get
            {
                return _schemaitems;
            }
            set
            {
                _schemaitems = value;
            }
        }

        public string Name
        {
            get
            {
                string localeid = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                return this.GetName(localeid);
            }
            set
            {
                string localeid = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                this.SetName(localeid, value);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public string GetNameWithEmpty(string localeId)
        {
            string name = null;
            if (string.IsNullOrEmpty(localeId))
                localeId = "zh-CN";
            name = this._names[localeId.ToUpper()].ToString();
            return name;
        }

        public string GetName(string localeId)
        {
            string name = null;
            if (string.IsNullOrEmpty(localeId))
                localeId = "zh-CN";

            name = this._names[localeId.ToUpper()].ToString();
            if (string.IsNullOrEmpty(name))
            {
                name = this._names["ZH-CN"].ToString();
                if (string.IsNullOrEmpty(name))
                {
                    name = this._names["ZH-TW"].ToString();
                    if (string.IsNullOrEmpty(name))
                    {
                        name = this._names["EN-US"].ToString();
                    }
                }
            }
            if (string.IsNullOrEmpty(name))
                return this.GetDefaultName(localeId);
            return name;
        }

        public void SetName(string localeId, string name)
        {
            if (string.IsNullOrEmpty(localeId))
                this._names[this._localeIds[0].ToUpper()] = name;
            else
                this._names[localeId.ToUpper()] = name;
        }

        public bool bDefault
        {
            get
            {
                return _bdefault;
            }
            set
            {
                _bdefault = value;
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

        public QuickSortSchema SortSchema
        {
            get
            {
                //if (_sortSchema == null)
                //{
                //    _sortSchema = new QuickSortSchema();
                //}
                return _sortSchema;
            }
            set { _sortSchema = value; }
        }

        /// <summary>
        /// ����ʱ���з����������
        /// </summary>
        public QuickSortSchema ColumnSortSchema
        {
            get
            {
                //if (_columnSortSchema == null)
                //{
                //    _columnSortSchema = new QuickSortSchema();
                //}
                return _columnSortSchema;
            }
            set { _columnSortSchema = value; }
        }

        public bool IsCross
        {
            get
            {
                if (this.SchemaItems.Count >= 3)
                {
                    if (this.SchemaItems[2].Items[0].Contains("____"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        #region ICloneable ��Ա

        public object Clone()
        {
            GroupSchema gs = new GroupSchema();
            gs.ID = this.ID;
            gs.Name = this.Name;
            gs.bDefault = this.bDefault;
            gs.bGroupItemsAhead = this.bGroupItemsAhead;
            gs.bShowSubTotal = this.bShowSubTotal;

            //************2012-5-17 add by yanghx
            gs.bShowDetail = this.bShowDetail;
            gs.ShowStyle = this.ShowStyle;
            //**************

            gs.SchemaItems = this.SchemaItems.Clone() as GroupSchemaItems;
            //gs.CurrentLocaleId = this.CurrentLocaleId;
            gs._names = this._names.Clone() as Hashtable;
            foreach (string key in this.DateDimensions.Keys)
                gs.DateDimensions.Add(key, this.DateDimensions[key]);
            //��������
            if (this.SortSchema != null)
            {
                gs.SortSchema = new QuickSortSchema();
                foreach (QuickSortItem sortItem in this.SortSchema.QuickSortItems)
                {
                    QuickSortItem newItem=new QuickSortItem(sortItem.Level,sortItem.Name,sortItem.SortDirection,sortItem.Priority);
                    gs.SortSchema.QuickSortItems.Add(newItem);
                }
            }
            //����������
            if (this.ColumnSortSchema != null && this.ColumnSortSchema.QuickSortItems!=null)
            {
                gs.ColumnSortSchema = new QuickSortSchema();
                foreach (QuickSortItem sortItem in this.ColumnSortSchema.QuickSortItems)
                {
                    QuickSortItem newItem = new QuickSortItem(sortItem.Level, sortItem.Name, sortItem.SortDirection, sortItem.Priority);
                    gs.ColumnSortSchema.QuickSortItems.Add(newItem);
                }
            }
            //11.1�����з��鸴��
            if (this.CrossRowGroup != null &&
                (this.CrossRowGroup.SchemaItems != null || this.CrossRowGroup.SortSchema != null))
                gs.CrossRowGroup = this.CrossRowGroup.Clone() as GroupSchema;
            gs.BShowHorizonTotal = this.BShowHorizonTotal;
            return gs;
        }

        #endregion

        #region IDisposable ��Ա

        public void Dispose()
        {
            foreach (GroupSchemaItem item in this._schemaitems)
            {
                item.Dispose();
            }
            this._schemaitems.Clear();
            _schemaitems = null;
            _datedimensions.Clear();
            _datedimensions = null;
        }

        #endregion
    }
}
