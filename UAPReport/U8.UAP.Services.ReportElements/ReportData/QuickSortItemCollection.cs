using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class QuickSortItemCollection:CollectionBase
    {
        public QuickSortItemCollection()
        {

        }

        public QuickSortItem this[int index]
        {
            get
            {
                 return this.List[index] as QuickSortItem;
            }
        }

        public QuickSortItem this[string name]
        {
            get
            {
                QuickSortItem qsi = null;
                foreach(QuickSortItem item in this)
                {
                    if (item.Name.ToLower() == name.ToLower())
                    {
                        qsi = item;
                        break;
                    }
                }
                return qsi;
            }
        }

        public void Insert(int index,QuickSortItem item)
        {
            this.List.Insert(index, item);
        }

        public void Add(QuickSortItem item )
        {
            this.List.Add( item );
        }

        public void Remove(QuickSortItem item)
        {
            this.List.Remove(item);
        }

        public bool Contains(QuickSortItem item)
        {
            bool exists = false;
            if (this[item.Name] != null)
                exists = true;

            return exists;
        }

    }
}
