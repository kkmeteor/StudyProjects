using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class LevelExpandDataTable
    {
        private DataTable   _rawDataTable;

        public LevelExpandDataTable( DataTable dataTable )
        {
            this._rawDataTable = dataTable;
        }

        public DataTable RawDataTable
        {
            get
            {
                return _rawDataTable;
            }
        }
    }
}
