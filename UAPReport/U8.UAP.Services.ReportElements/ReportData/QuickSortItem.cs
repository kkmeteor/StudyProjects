using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class QuickSortItem
    {
        private SortDirection   _sortDirection;
        private int             _level;
        private string          _name;
        private int _priority;

        public  QuickSortItem()
        {
        }

        public QuickSortItem(int level, string name, SortDirection direction,int priority)
        {
            this._level = level;
            this._name = name;
            this._sortDirection = direction;
            _priority = priority;
        }

        public SortDirection SortDirection
        {
            get
            {
                if (_level > 0 && _sortDirection == SortDirection.None)
                    _sortDirection = SortDirection.Ascend;
                return _sortDirection;
            }
            set
            {
                _sortDirection = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
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

        public int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = value;
            }
        }
    }
}
