using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class ReportSummaryData :RowData //,ISerializable
    {
        public ReportSummaryData(SqlDataReader reader)
            : base(reader)
        {
            for (int i = 0; i < _reader.FieldCount; i++)
                _data.Add(_reader.GetName(i), _reader.GetValue(i));
        }

        protected ReportSummaryData(SerializationInfo info, StreamingContext context):base(info,context)
        {
            
        }

        public int Level
        {
            get
            {
                return 0;
            }
        }

        public new Double this[string mapname]
        {
            get
            {
                double d = 0;
                try
                {
                    d = Convert.ToDouble(_data[mapname.ToLower()]);
                }
                catch
                {
                }

                return d;
            }
        }

        //#region ISerializable ³ÉÔ±

        //public void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        //#endregion
    }
}
