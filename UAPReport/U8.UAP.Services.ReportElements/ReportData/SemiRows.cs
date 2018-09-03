using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class SemiRows : CollectionBase
    {
        public SemiRow this[int index]
        {
            get
            {
                return this.InnerList[index] as SemiRow ;
            }
        }

        public void Add(SemiRow semirow)
        {
            this.InnerList.Add(semirow);
        }

    }
}
