using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public partial class DataSourceEditorControl : UserControl
    {
        private DataSource _DataSource;
        private IWindowsFormsEditorService _edSvc;
        private object _object;
        private bool _supportmultichoice;//supportdrag
        private ArrayList _selectedsources;
        private ListViewColumnSorter lvwColumnSorter;

        public event EventHandler SelectedChanged;
        public DataSourceEditorControl()
        {
            InitializeComponent();
        }

        public DataSourceEditorControl(IWindowsFormsEditorService edSvc)
        {
            InitializeComponent();
            _edSvc = edSvc;
            _supportmultichoice = false;
            this.Width = 250;
        }

        public DataSource Source
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _DataSource = value;
            }
        }

        public ArrayList SelectedSources
        {
            get
            {
                _selectedsources = new ArrayList();
                foreach (ListViewItem lvi in datasourcelist.SelectedItems  )
                    _selectedsources.Add(lvi.Tag);
                return _selectedsources;
            }
        }

        public object EditObject
        {
            set
            {
                _object = value;
            }
        }

        public void Init(object editobject, DataSource datasource)
        {
            _supportmultichoice = false;
            _object = editobject;
            _DataSource = datasource;
            Init();
        }

        public void Init(DataSources datasources)
        {
            _supportmultichoice = true;
            datasourcelist.CheckBoxes = false;
            Init(datasources, null,null);
        }

        public void Init()
        {            
            DataSources dss = null;
            if (_object is Cell)
            {
                (_object as Cell).GetReport();
                dss = (_object as Cell).Report.DataSources;
            }
            else if (_object is Report)
                dss = (_object as Report).DataSources;

            string type = "String";
            if (_object is IDecimal)
                type = "Decimal";
            else if (_object is IDateTime)
                type = "DateTime";
            else if (_object is IImage)
                type = "Image";

            Init(dss, _DataSource.Name,type);
        }

        #region resource
        public string DataSource
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "DataSource";
                    case "zh-TW":
                        return "數據源";
                    default:
                        return "数据源";
                }
            }
        }

        #endregion

        private void Init(DataSources dss, string selected,string type)
        {
            if (dss == null)
                return;
            datasourcelist.Columns.Clear();
            datasourcelist.Columns.Add(DataSource, datasourcelist.Width - 20);
            datasourcelist.Items.Clear();

            lvwColumnSorter = new ListViewColumnSorter();
            this.datasourcelist.ListViewItemSorter = lvwColumnSorter;

            ListViewItem checkeditem = null;
            foreach (string key in dss.Keys)
            {
                DataSource ds = dss[key];
                if (!ds.bAppend && TypeValidate(ds.Type,type))
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = ds.Caption;                    
                    lvi.Tag = ds;
                    if (selected != null && (lvi.Tag as DataSource).Name.ToLower() == selected.ToLower())
                    {
                        lvi.Checked = true;
                        checkeditem = lvi;
                    }
                    datasourcelist.Items.Add(lvi);
                }
            }
            if (checkeditem != null)
                datasourcelist.TopItem = checkeditem;
            //datasourcelist_ColumnClick(datasourcelist, new ColumnClickEventArgs(0));
        }

        private bool TypeValidate(DataType dt, string type)
        {
            if (string.IsNullOrEmpty(type) || type == "String")
                return true;
            if (type == "Decimal" && (dt == DataType.Currency ||
                                                dt == DataType.Decimal ||
                                                dt == DataType.Int))
                return true;
            if (type == "DateTime" && dt == DataType.DateTime)
                return true;
            if (type == "Image" && dt == DataType.Image )
                return true;
            return false;
        }

        public void UnRegEvent()
        {
            datasourcelist.DoubleClick -= new EventHandler(datasourcelist_DoubleClick);
            datasourcelist.ItemChecked -= new ItemCheckedEventHandler(datasourcelist_ItemChecked);
            datasourcelist.ColumnClick -= new ColumnClickEventHandler(datasourcelist_ColumnClick);
            datasourcelist.ItemDrag -= new ItemDragEventHandler(datasourcelist_ItemDrag);
        }

        public void RegEvent()
        {
            datasourcelist.DoubleClick += new EventHandler(datasourcelist_DoubleClick);
            datasourcelist.ItemChecked += new ItemCheckedEventHandler(datasourcelist_ItemChecked);
            datasourcelist.ColumnClick += new ColumnClickEventHandler(datasourcelist_ColumnClick);
            datasourcelist.ItemDrag += new ItemDragEventHandler(datasourcelist_ItemDrag);
        }

        private void datasourcelist_ItemDrag(object sender, ItemDragEventArgs e)
        {
            datasourcelist.DoDragDrop((e.Item as ListViewItem).Tag, DragDropEffects.All);
        }

        private void datasourcelist_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            this.datasourcelist.Sort();
        }

        private void datasourcelist_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_edSvc != null)
            {
                if (e.Item.Checked)
                {
                    _DataSource = e.Item.Tag as DataSource;
                    _edSvc.CloseDropDown();    
                }
                else
                    _DataSource = new DataSource("EmptyColumn");
            }
            else if (!_supportmultichoice)
            {
                if (e.Item.Checked)
                {
                    _DataSource = e.Item.Tag as DataSource;
                    datasourcelist.ItemChecked -= new ItemCheckedEventHandler(datasourcelist_ItemChecked);
                    foreach (ListViewItem lvi in datasourcelist.CheckedItems)
                    {
                        if (lvi != e.Item)
                            lvi.Checked = false;
                    }
                    datasourcelist.ItemChecked += new ItemCheckedEventHandler(datasourcelist_ItemChecked);
                }
                else
                    _DataSource = null;
                if (SelectedChanged != null)
                    SelectedChanged(this, null);
            }
        }

        private void datasourcelist_DoubleClick(object sender, EventArgs e)
        {
            if (_edSvc != null && datasourcelist.SelectedItems.Count>0)
            {
                _DataSource = datasourcelist.SelectedItems[0].Tag as DataSource;
                _edSvc.CloseDropDown();   
            }
        }        

        private void DataSourceEditorControl_Load(object sender, EventArgs e)
        {
            RegEvent ();
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            int beginindex = txtsearch.Tag == null ? 0 : (int)txtsearch.Tag;
            int count = datasourcelist.Items.Count;
            ListViewItem item = new ListViewItem();
            for (int i = 0; i < count; i++)
            {
                int index = (beginindex + i + 1) % count;
                item = datasourcelist.Items[index];
                if (item.Text.Contains(txtsearch.Text.Trim()))
                {
                    item.Selected = true;                                       
                    //datasourcelist.TopItem = item;
                    datasourcelist.EnsureVisible(index);
                    txtsearch.Tag = index;
                    return;
                }
            }
        }

        private void txtsearch_TextChanged(object sender, EventArgs e)
        {
            txtsearch.Tag = 0;
        }
    }

    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// 指定按照哪个列排序
        /// </summary>
        private int ColumnToSort;
        /// <summary>
        /// 指定排序的方式
        /// </summary>
        private SortOrder OrderOfSort;
        /// <summary>
        /// 声明CaseInsensitiveComparer类对象，
        /// 参见ms-help://MS.VSCC.2003/MS.MSDNQTR.2003FEB.2052/cpref/html/frlrfSystemCollectionsCaseInsensitiveComparerClassTopic.htm
        /// </summary>
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ListViewColumnSorter()
        {
            // 默认按第一列排序
            ColumnToSort = 0;

            // 排序方式为不排序
            OrderOfSort = SortOrder.Descending;

            // 初始化CaseInsensitiveComparer类对象
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// 重写IComparer接口.
        /// </summary>
        /// <param name="x">要比较的第一个对象</param>
        /// <param name="y">要比较的第二个对象</param>
        /// <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // 将比较对象转换为ListViewItem对象
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            // 比较
            compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

            // 根据上面的比较结果返回正确的比较结果
            if (OrderOfSort == SortOrder.Ascending)
            {
                // 因为是正序排序，所以直接返回结果
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                // 如果是反序排序，所以要取负值再返回
                return (-compareResult);
            }
            else
            {
                // 如果相等返回0
                return 0;
            }
        }

        /// <summary>
        /// 获取或设置按照哪一列排序.
        /// </summary>
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        /// <summary>
        /// 获取或设置排序方式.
        /// </summary>
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }

    }
}
