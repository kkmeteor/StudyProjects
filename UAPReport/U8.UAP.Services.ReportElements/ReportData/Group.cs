using System;
using System.Collections;
using System.Data;
using System.Runtime.Serialization;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Group 的摘要说明。
	/// </summary>
	[Serializable]
	public class Group:RowData,IDisposable,ICloneable
	{
		private int _level;
        protected  Group()
        {
        }

        public Group(int level, SqlDataReader reader)
            : base(reader)
        {
            _level = level;
        }

		public Group(int level,SqlDataReader reader,SimpleArrayList columns):this(level,reader)
		{
            _columns = columns;
		}

        protected Group(SerializationInfo info, StreamingContext context):base(info,context)
        {
            _level = info.GetInt32("Level");
        }

        public Group(Group group):base(group)
        {
            _level = group.Level;
        }

        public override object this[string name]
        {
            get
            {
                if (name.ToLower() == "emptycolumn")
                    return "";
                if (_data.Contains(name))
                {
                    return _data[name];
                }
                else if (_columns==null || _columns.Contains(name))
                {
                    return _reader[name];
                }
                else if (!_minorrow.bClosed)
                {
                    return _minorrow[name];
                }
                else
                    return "";
            }
            set
            {
                _data.Add(name, value);
            }
        }

        public bool Visible
        {
            get
            {
                return true;
            }
        }

        public Groups ChildGroups
        {
            get
            {
                return new Groups();
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Level", _level);
        }

        #region IDisposable 成员
        public void Dispose()
        {
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new Group(this);
        }

        #endregion
    }
}
