using System;
using System.Data;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportFilterService;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// RuntimeRows 的摘要说明。
	/// </summary>
	public class RuntimeRows
	{
		private ArrayList _array;
		private Cell _activecell;
		private Row _activerow;
        private bool _b4matrix = false;
		public RuntimeRows()
		{
			_array=new ArrayList();
		}

		public void Add(int y,Row row)
		{
			row.bActiveCell-=new ActiveCellHandler(row_bActiveCell);
			row.bActiveCell+=new ActiveCellHandler(row_bActiveCell);
            row.bActiveRow -= new ActiveRowHandler(row_bActiveRow);
            row.bActiveRow += new ActiveRowHandler(row_bActiveRow);
            row.hasActiveCell -= new HasActiveCellHandler(row_hasActiveCell);
            row.hasActiveCell += new HasActiveCellHandler(row_hasActiveCell);
			_array.Add(new RuntimeRow(y,row));
		}

        private bool row_hasActiveCell()
        {
            return _activecell != null;
        }

        private bool row_bActiveRow(Row row)
        {
            return _activerow == row && _b4matrix ;
        }

        public bool b4Matrix
        {
            set
            {
                _b4matrix = value;
            }
        }

		public void Clear()
		{
			_array.Clear();
		}

		public Cell ActiveCell
		{
			get
			{
				return _activecell;
			}
			set
			{
				_activecell=value;
			}
		}

		public Row ActiveRow
		{
			get
			{
				return _activerow;
			}
			set
			{
				_activerow=value;
			}
		}

		public int Count
		{
			get
			{
				return _array.Count;
			}
		}

		public Row this[int index]
		{
			get
			{
				if(index<Count)
					return (_array[index] as RuntimeRow).Row;
				else
					return null;
			}
		}

		public Row this[string y]
		{
			get
			{
				int iy=Convert.ToInt32(y);
				for(int i=0;i<_array.Count;i++)
				{
					if((_array[i] as RuntimeRow).Y==iy)
						return this[i];
				}
				return null;
			}
		}

		private bool row_bActiveCell(Cell cell)
		{
			return cell==_activecell;
		}
	}

	public class RuntimeRow
	{
		private int _y;
		private Row _row;
		public RuntimeRow(int y,Row row)
		{
			_y=y;
			_row=row;
		}
		public int Y
		{
			get
			{
				return _y;
			}
		}
		public Row Row
		{
			get
			{
				return _row;
			}
		}
	}

    public class RuntimeGroup:IRelateData
    {
        private int _level;
        private ArrayList _items;
        private Hashtable _uppergroups;
        public RuntimeGroup(int level)
        {
            _level = level;
            _items = new ArrayList();
            _uppergroups = new Hashtable();
        }

        public RuntimeGroup UpperGroup(int level)
        {
            return _uppergroups[level] as RuntimeGroup;
        }

        public void AddAUpperGroup(RuntimeGroup group)
        {
            _uppergroups.Add(group.Level, group);
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

        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

        public bool Contains(string key)
        {
            foreach (RuntimeGroupItem item in _items)
            {
                if (item.CellName.ToLower() == key.ToLower()
                    || item.ColumnName.ToLower() == key.ToLower())
                    return true;
            }
            foreach (int level in _uppergroups.Keys)
            {
                if ((_uppergroups[level] as RuntimeGroup).Contains(key))
                    return true;
            }
            return false;
        }

        public object this[string key]
        {
            get
            {
                foreach (RuntimeGroupItem item in _items)
                {
                    if (item.CellName.ToLower() == key.ToLower()
                        || item.ColumnName.ToLower() == key.ToLower())
                        return item.Value ;
                }
                foreach (int level in _uppergroups.Keys)
                {
                    object o = (_uppergroups[level] as RuntimeGroup)[key];
                    if (o!=null)
                        return o;
                }
                return null;
            }
        }

        public RuntimeGroupItem this[int index]
        {
            get
            {
                return _items[index] as RuntimeGroupItem;
            }
        }

        public void AddAItem(RuntimeGroupItem item)
        {
            this._items.Add(item);
        }

        public string GetFilterString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (RuntimeGroupItem item in _items)
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.Append("isnull(convert(nvarchar(300),[");
                sb.Append(item.ColumnName);
                sb.Append("]),'')='");
                sb.Append(item.Value);
                sb.Append("'");
            }
            foreach (int key in _uppergroups.Keys)
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.Append((_uppergroups[key] as RuntimeGroup).GetFilterString());
            }
            return sb.ToString();
        }
        #region IRelateData 成员

        public object GetData(string key)
        {
            return this[key];
        }

        #endregion
    }

    public class RuntimeGroupItem
    {
        private string _cellname;
        private string _columnname;
        private string _value;

        public RuntimeGroupItem(string cellname, string columnname, string value)
        {
            _cellname = cellname;
            _columnname = columnname;
            _value = value;
        }

        public string CellName
        {
            get
            {
                return _cellname;
            }
            set
            {
                _cellname = value;
            }
        }

        public string ColumnName
        {
            get
            {
                return _columnname;
            }
            set
            {
                _columnname = value;
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }
}
