using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class ReportLevelExpandCollection : CollectionBase
    {
        public ReportLevelExpandCollection()
        {

        }

        public ReportLevelExpand this[int index]
        {
            get
            {
                return this.List[index] as ReportLevelExpand;
            }
        }

        public void Add( ReportLevelExpand item )
        {
            this.List.Add( item );
        }

        public bool Contains(ReportLevelExpand item)
        {
            return this.List.Contains(item);
        }
    }
}
