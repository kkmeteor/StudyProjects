using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class LevelExpandItemCollection:CollectionBase
    {
        public LevelExpandItemCollection()
        {

        }

        public LevelExpandItem this[int index]
        {
            get
            {
                return this.List[index] as LevelExpandItem;
            }
        }

        public LevelExpandItem this[string name]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    LevelExpandItem item = this[i];
                    if (item.ColumnName.ToLower() == name.ToLower())
                        return item;
                }
                return null;
            }
        }

        public void Add(LevelExpandItem item)
        {
            this.List.Add( item );
        }

        public void Remove(LevelExpandItem item)
        {
            this.List.Remove( item );
        }

        public void RemoveAll()
        {
            this.List.Clear();
        }

        public LevelExpandItemCollection Clone()
        {
            string strObject = SerializeHelper.Serialize( this);
            LevelExpandItemCollection obj = SerializeHelper.Deserialize(strObject) as LevelExpandItemCollection;
            return obj;
        }
    }
}
