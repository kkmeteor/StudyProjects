using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class LevelExpandData
    {
        private string          _filterCode;
        private DataTable       RawDataTable = null;
        private LevelExpandItem _levelExpandItem = null;

        public LevelExpandData(LevelExpandDataTable dataTable,LevelExpandItem leveExpandItem )
        {
            this.RawDataTable = dataTable.RawDataTable;
            this._levelExpandItem = leveExpandItem;
        }

        internal string FilterCode
        {
            set
            {
                _filterCode = value;
            }
        }

        public LevelExpandItem LevelExpandItem
        {
            get
            {
                return _levelExpandItem;
            }
            set
            {
                _levelExpandItem = value;
            }

        }

        private int GetLevelLength(int level)
        {
            int nLen = 0;
            for (int i = 0; i < level; i++)
            {
                nLen += Convert.ToInt32( this.LevelExpandItem.CodeRule.Substring(i, 1));
            }
            return nLen;
        }

   
        public string GetValue(int level)
        {
            string  strValue = string.Empty;
            int     nLen = GetLevelLength( level );

            if ( this._filterCode.Length>=nLen )
            {
                string  codeValue = _filterCode.Substring( 0, nLen );

                DataRow[] rows = this.RawDataTable.Select(string.Format("CCode='{0}'", codeValue));

                if ( rows.Length > 0 )
                    strValue = rows[0][1].ToString();
            }

            return strValue;
        }
    }
}
