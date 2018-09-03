using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;


namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class ReportLevelExpand : ISerializable
    {
        private string _name = string.Empty;
        private bool   _isDefault;
        private LevelExpandItemCollection _levelExpandItems;
        [NonSerialized]
        private static ReportLevelExpand _empty;

        public ReportLevelExpand()
        {
            this._levelExpandItems = new LevelExpandItemCollection();
        }

        public static ReportLevelExpand Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new ReportLevelExpand();
                return _empty;
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

        public bool IsDefault
        {
            get
            {
                return _isDefault;
            }
            set
            {
                _isDefault = value;
            }

        }

        public LevelExpandItemCollection LevelExpandItems
        {
            get
            {
                return _levelExpandItems;
            }
            set
            {
                _levelExpandItems = value;
            }
        }

        public void AddLevelExpand(LevelExpandItem item )
        {
            this._levelExpandItems.Add(item);
        }

        public void ClearLevelExpand()
        {
            this._levelExpandItems.Clear();
        }

        protected ReportLevelExpand(SerializationInfo info, StreamingContext context)
　　    {
            this._name = (string)info.GetValue("Name", this._name.GetType() );
            this._isDefault = (bool)info.GetValue("IsDefault", this._isDefault.GetType());

            Type type = Type.GetType("UFIDA.U8.UAP.Services.ReportElements.LevelExpandItemCollection");
            this._levelExpandItems = info.GetValue("LevelExpandItems", type) as LevelExpandItemCollection;
　　    }
　　
        public virtual void GetObjectData(SerializationInfo info,StreamingContext context)
        {
            info.AddValue("Name", this._name, this._name.GetType());
            info.AddValue("IsDefault", this._isDefault, this._isDefault.GetType());
            info.AddValue("LevelExpandItems", this._levelExpandItems, this._levelExpandItems.GetType());
        }

    }
}
