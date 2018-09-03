using System;
using System.Collections;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Groups 的摘要说明。
	/// </summary>
	[Serializable]
	public class Groups:CollectionBase,IDisposable 
	{
        private int _levels;
		public Group this[int index]
		{
			get
			{
				return this.InnerList[index] as Group;
			}
		}

        public int Levels
        {
            get
            {
                return _levels;
            }
            set
            {
                _levels = value;
            }
        }

		public void Add(Group group)
		{
			this.InnerList.Add(group);
		}

		public int IndexOf(Group group)
		{
			return this.InnerList.IndexOf(group);
		}

		public void Insert(int index,Group group)
		{
			this.InnerList.Insert(index,group);
		}

		public void Remove(Group group)
		{
			this.InnerList.Remove(group);
		}

		public bool Contains(Group group)
		{
			return this.InnerList.Contains(group);
		}		

        #region IDisposable 成员

        public void Dispose()
        {
            foreach (Group group in this.InnerList)
            {
                if (group != null)
                {
                    group.Dispose();
                }
            }
            this.InnerList.Clear();
        }

        #endregion

        #region bytes
        public byte[] ToBytes()
        {
            MemoryStream fs = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
                return fs.ToArray();
            }
            catch (SerializationException ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
        }

        public static Groups FromBytes(Byte[] bs)
        {
            MemoryStream ms = new MemoryStream(bs);
            try
            {
                Groups groups;
                BinaryFormatter formatter = new BinaryFormatter();
                groups = (Groups)formatter.Deserialize(ms);
                return groups;
            }
            catch (SerializationException ex)
            {
                throw ex;
            }
            finally
            {
                ms.Close();
            }
        }
        #endregion
    }
}
