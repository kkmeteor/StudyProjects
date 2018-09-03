using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class QuickSortSchema : ICloneable
    {
        private QuickSortItemCollection _quickSortItems;
        private int _pageSize = 0;

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public QuickSortSchema()
        {
            _quickSortItems = new QuickSortItemCollection();
        }

        public void Clear()
        {
            _quickSortItems.Clear();
        }

        public QuickSortItemCollection QuickSortItems
        {
            get
            {
                return _quickSortItems;
            }
        }

        public bool Contains(string name)
        {
            foreach (QuickSortItem item in _quickSortItems)
            {
                if (item.Name.ToLower() == name.ToLower())
                    return true;
            }
            return false;
        }

        public void Add(string name, SortOption direction, int level)
        {
            QuickSortItem item = new QuickSortItem(level, name, direction.SortDirection, direction.Priority);
            AddQuickSortItem(item);
        }

        public void InsertToFirst(QuickSortItem item)
        {
            if (item.SortDirection == SortDirection.None)
                return;
            this._quickSortItems.Insert(0, item);
        }

        public void AddQuickSortItem(QuickSortItem item)
        {
            if (item.SortDirection == SortDirection.None)
                return;
            bool exists = this._quickSortItems.Contains(item);
            if (!exists)
            {
                //int i = 0;
                //bool badded = false;
                //if (item.Level <= 0)
                //{
                //    while (i < _quickSortItems.Count)
                //    {
                //        QuickSortItem tmp = _quickSortItems[i];
                //        if (tmp.Level <= 0 && item.Priority < tmp.Priority)
                //        {
                //            this._quickSortItems.Insert(i, item);
                //            badded = true;
                //            break;
                //        }
                //        i++;
                //    }
                //}
                //else
                //{
                //    while (i < _quickSortItems.Count)
                //    {
                //        QuickSortItem tmp = _quickSortItems[i];
                //        if (tmp.Level <= 0 || item.Level < tmp.Level || (item.Level == tmp.Level && item.Priority < tmp.Priority))
                //        {
                //            this._quickSortItems.Insert(i, item);
                //            badded = true;
                //            break;
                //        }
                //        i++;
                //    }
                //}
                //if (!badded)
                //    this._quickSortItems.Add(item);
                //----------------------10.0
                //if (item.Level > 0)
                //    item.Priority -= 100000;
                //int i = 0;
                //bool badd = false;
                //while (i < _quickSortItems.Count)
                //{
                //    QuickSortItem tmp = _quickSortItems[i];
                //    //if (tmp.Level <= 0 && item.Priority <= tmp.Priority)
                //    if (item.Priority <= tmp.Priority)
                //    {
                //        this._quickSortItems.Insert(i, item);
                //        badd = true;
                //        break;
                //    }
                //    i++;
                //}
                //if (!badd)
                //    this._quickSortItems.Add(item);
                try
                {
                    if (_quickSortItems.Count == 0)//没有数据时直接插入
                        this._quickSortItems.Add(item);
                    else
                    {
                        AddItemByLevel(item);
                    }
                }
                catch
                {
                    ;
                }
            }
        }

        private void AddItemByLevel(QuickSortItem item)
        {
            int i = 0;
            int pos = 0;
            int poe = 0;
            bool badd = false;
            int level = item.Level;
            QuickSortItem tmp = null;
            if (level <= 0)
            {
                while (i < this._quickSortItems.Count)
                {
                    tmp = this._quickSortItems[i];
                    if (tmp.Level <= 0)
                    {
                        pos = i;
                        break;
                    }
                    i++;
                }
                while (i < _quickSortItems.Count)
                {
                    tmp = _quickSortItems[i];
                    if (item.Priority < tmp.Priority)
                    {
                        this._quickSortItems.Insert(i, item);
                        badd = true;
                        break;
                    }
                    i++;
                }
                if (!badd)
                    this._quickSortItems.Add(item);
            }
            else
            {
                if (level < _quickSortItems[0].Level)
                {
                    this._quickSortItems.Insert(0, item);
                    return;
                }
                if (level == _quickSortItems[0].Level)
                {
                    while (i < this._quickSortItems.Count)
                    {
                        tmp = this._quickSortItems[i];
                        if (tmp.Level != level || i + 1 == this._quickSortItems.Count)
                        {
                            pos = i;
                            break;
                        }
                        i++;
                    }
                    i = 0;
                    while (i <= pos)
                    {
                        tmp = _quickSortItems[i];
                        if (item.Priority < tmp.Priority)
                        {
                            this._quickSortItems.Insert(i, item);
                            badd = true;
                            break;
                        }
                        i++;
                    }
                    if (!badd)
                    {
                        if (tmp.Level != level)
                        {
                            this._quickSortItems.Insert(pos, item);
                        }
                        else
                        {
                            this._quickSortItems.Insert(pos + 1, item);
                        }
                    }
                       
                    return;
                }

                if (level > _quickSortItems[0].Level)
                {
                    while (i < this._quickSortItems.Count)
                    {
                        tmp = this._quickSortItems[i];
                        if (tmp.Level == level || tmp.Level == 0 || i + 1 == this._quickSortItems.Count)
                        {
                            pos = i;
                            break;
                        }
                        i++;
                    }
                    while (i < this._quickSortItems.Count)
                    {
                        tmp = this._quickSortItems[i];
                        if (tmp.Level > level || tmp.Level == 0 || i + 1 == this._quickSortItems.Count)
                        {
                            poe = i;
                            break;
                        }
                        i++;
                    }
                    i = pos;
                    while (i <= poe)
                    {
                        tmp = _quickSortItems[i];
                        if (item.Priority < tmp.Priority && item.Level == tmp.Level)
                        {
                            this._quickSortItems.Insert(i, item);
                            badd = true;
                            break;
                        }
                        i++;
                    }
                    if (!badd)
                        this._quickSortItems.Insert(poe + 1, item);
                }
            }
        }
        //倒着加
        public void AddAfterGroup(QuickSortItem item)
        {
            //是否是一层分组的情况，或者只有明细的情况，
            //因为一层分组的情况下，分组和明细是平级的关系，可以随便排序

            if (item.SortDirection == SortDirection.None)
                return;
            bool badd = false;
            bool bdel = false;
            int i = 0;
            while (i < _quickSortItems.Count)
            {
                QuickSortItem tmp = _quickSortItems[i];
                if (tmp.Name.ToLower() == item.Name.ToLower())
                {
                    if (tmp.Level > 0)
                    {
                        tmp.SortDirection = item.SortDirection;
                        badd = true;
                        break;
                    }
                    else
                    {
                        _quickSortItems.RemoveAt(i);
                        bdel = true;
                        break;
                    }
                }
                i++;
            }
            if (bdel || !badd)
            {
                i = 0;
                while (i < _quickSortItems.Count)
                {
                    QuickSortItem tmp = _quickSortItems[i];
                    //if (tmp.Level <=0)
                    //    break;
                    if (tmp.Level < 0)//小于0即是默认排序或者日期维度等现在都是正序
                        break;
                    i++;
                }
                _quickSortItems.Insert(i, item);
            }
        }

        public string GetSortStringForMatrix()
        {
            bool brank = false;
            for (int i = 0; i < _quickSortItems.Count; i++)
            {
                if (_quickSortItems[i].Level <= 0)
                    brank = true;
            }
            if (brank)
                return GetRankSortString();
            else
                return GetGroupSortString();
        }

        private string GetRankSortString()
        {
            StringBuilder sbsort = new StringBuilder();
            for (int i = 0; i < _quickSortItems.Count; i++)
            {
                QuickSortItem item = _quickSortItems[i];
                if (item.Level > 0)
                    continue;
                if (sbsort.Length > 0)
                    sbsort.Append(",");
                sbsort.Append(GetAItemString(item, ""));
            }
            return sbsort.ToString();
        }

        public string GetGroupSortString()
        {
            StringBuilder sbsort = new StringBuilder();
            for (int i = 0; i < _quickSortItems.Count; i++)
            {
                QuickSortItem item = _quickSortItems[i];
                //if (item.Level <=0)
                if (item.Level < 0)
                    continue;
                if (sbsort.Length > 0)
                    sbsort.Append(",");
                sbsort.Append(GetAItemString(item, ""));
            }
            return sbsort.ToString();
        }



        public string GetGroupSortStringWithPrefix(int level, string prefix)
        {
            StringBuilder sbsort = new StringBuilder();
            for (int i = 0; i < _quickSortItems.Count; i++)
            {
                QuickSortItem item = _quickSortItems[i];
                if (item.Level > level || item.Level <= 0)
                    //if (item.Level > level)
                    continue;
                if (level == 1 && item.Level < 0)
                    continue;
                if (level > 1 && item.Level <= 0)
                    continue;

                if (sbsort.Length > 0)
                    sbsort.Append(",");
                sbsort.Append(GetAItemString(item, prefix));
            }
            return sbsort.ToString();
        }

        public string GetSortStringWithPrefix(string prefix)
        {
            StringBuilder sbsort = new StringBuilder();

            for (int i = 0; i < _quickSortItems.Count; i++)
            {
                if (sbsort.Length > 0)
                    sbsort.Append(",");
                QuickSortItem item = _quickSortItems[i];
                sbsort.Append(GetAItemString(item, prefix));
            }
            return sbsort.ToString();
        }

        public string GetSortString()
        {
            StringBuilder sbsort = new StringBuilder();

            for (int i = 0; i < _quickSortItems.Count; i++)
            {
                if (sbsort.Length > 0)
                    sbsort.Append(",");
                QuickSortItem item = _quickSortItems[i];
                sbsort.Append(GetAItemString(item, ""));
            }
            return sbsort.ToString();
        }

        private string GetAItemString(QuickSortItem item, string b)
        {
            StringBuilder sbsort = new StringBuilder();
            if (!string.IsNullOrEmpty(b))
            {
                sbsort.Append(b);
                sbsort.Append(".");
            }
            sbsort.Append("[");
            sbsort.Append(item.Name);
            sbsort.Append("]");
            if (item.SortDirection == SortDirection.Ascend)
                sbsort.Append(" asc");
            else
                sbsort.Append(" desc");
            return sbsort.ToString();
        }



        #region ICloneable 成员
        public object Clone()
        {
            QuickSortSchema sort = new QuickSortSchema();
            foreach (QuickSortItem item in this.QuickSortItems)
            {
                sort.QuickSortItems.Add(new QuickSortItem(item.Level, item.Name, item.SortDirection, item.Priority));
            }
            return sort;
        }
        #endregion

        #region 11.0 add
        public string GetGroupSortString(Report report)
        {

            Cells designCells = report.GridDetailCells;
            string realMapName = string.Empty;
            StringBuilder sbsort = new StringBuilder();
            for (int i = 0; i < _quickSortItems.Count; i++)
            {
                QuickSortItem item = _quickSortItems[i];
                //if (item.Level <=0)
                if (item.Level < 0)
                    continue;

                //根据item找到真正的name
                Cell cell = designCells.GetBySource(item.Name);
                realMapName = this.GetMapname(cell, report);
                if (string.IsNullOrEmpty(realMapName))
                {
                    if (report.DataSources[item.Name] != null)
                    {
                        realMapName = report.DataSources[item.Name].Name;
                    }
                    if (string.IsNullOrEmpty(realMapName))
                    continue;
                }
               
                //end
                if (sbsort.Length > 0)
                    sbsort.Append(",");
                sbsort.Append(GetAItemString(item, "", realMapName));
            }
            return sbsort.ToString();
        }
        private string GetMapname(Cell cell,Report report)
        {
            if (cell == null)
                return null;
            int level = report.GroupLevels;
            for (int i = 1; i <= report.GroupLevels; i++)
            {
               Section sec= report.Sections.GetGroupHeader(i);
               if (sec.Cells[cell.Name] != null && sec.Cells[cell.Name] is IMapName)
               {
                   return (sec.Cells[cell.Name] as IMapName).MapName;
               }
            }
            if (report.Sections[SectionType.Detail] == null)
                return null;
            Cells detailCells = report.Sections[SectionType.Detail].Cells;
            if (detailCells[cell.Name] != null && detailCells[cell.Name] is IMapName)
            {
                return (detailCells[cell.Name] as IMapName).MapName;
            }
            return null;
        }

        private string GetAItemString(QuickSortItem item, string b, string mapName)
        {
            StringBuilder sbsort = new StringBuilder();
            if (!string.IsNullOrEmpty(b))
            {
                sbsort.Append(b);
                sbsort.Append(".");
            }
            sbsort.Append("[");
            sbsort.Append(mapName);
            sbsort.Append("]");
            if (item.SortDirection == SortDirection.Ascend)
                sbsort.Append(" asc");
            else
                sbsort.Append(" desc");
            return sbsort.ToString();
        }
        #endregion
    }

}
