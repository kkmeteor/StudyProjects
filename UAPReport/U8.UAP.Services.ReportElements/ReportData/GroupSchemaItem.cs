using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupSchema 的摘要说明。
	/// </summary>
	[Serializable]
	public class GroupSchemaItem:ICloneable,IDisposable
	{
		private SimpleArrayList  _items;
		private int _level;
        private bool _bsummary;
        private bool _bsolid=false;       
        
		public GroupSchemaItem()
		{
            _items = new SimpleArrayList();            
		}
        public GroupSchemaItem(int level)
            : this()
        {
            _level = level;
        }

        public bool Contains(string name)
        {
            return _items.Contains(name);
        }

		public SimpleArrayList  Items
		{
			get
			{
				return _items;
			}
            set
            {
                _items = value;
            }
		}

        

        public bool bSolid
        {
            get
            {
                return _bsolid;
            }
            set
            {
                _bsolid = value;
            }
        }

        public bool bSummary
        {
            get
            {
                return _bsummary;
            }
            set
            {
                _bsummary = value;
            }
        }

		public int Level
		{
			get
			{
			    return _level;
			}
			set
			{
				_level=value;
			}
		}
		#region ICloneable 成员

		public object Clone()
		{
			GroupSchemaItem gsi=new GroupSchemaItem();
			gsi.Level=this.Level;
            gsi.bSummary = this.bSummary;
			gsi.Items=this.Items.Clone() as SimpleArrayList;            
			return gsi;
		}

		#endregion

        #region IDisposable 成员

        public void Dispose()
        {
            this._items.Clear();
            this._items = null;
        }

        #endregion
    }
}
