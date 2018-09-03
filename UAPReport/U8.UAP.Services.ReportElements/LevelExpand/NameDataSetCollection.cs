using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    class NameDataSetCollection
    {
        private Hashtable DataSetHashTable;

        public NameDataSetCollection()
        {
            this.DataSetHashTable = new Hashtable();
        }

        public bool Contains( LevelExpandEnum levelExpand )
        {
            return DataSetHashTable.ContainsKey(levelExpand );
        }

        public LevelExpandDataTable this[LevelExpandEnum levelExpand]
        {
            get
            {
                if (!this.DataSetHashTable.Contains(levelExpand))
	            {
            		 return null;
	            }
                return this.DataSetHashTable[levelExpand] as LevelExpandDataTable;
            }
        }

        public void Add(LevelExpandEnum levelExpand, LevelExpandDataTable dataSet)
        {
            if (!this.DataSetHashTable.Contains(levelExpand))
                this.DataSetHashTable.Add(levelExpand, dataSet);
        }
      
    }
}
