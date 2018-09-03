using System;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupSchemaItems 的摘要说明。
	/// </summary>
	[Serializable]
	public class GroupSchemaItems:CollectionBase,ICloneable
	{
		public GroupSchemaItem this[int index]
		{
			get {  return List[index] as GroupSchemaItem;  }
		}

		public GroupSchemaItem GetByLevel( int level )
		{
			for( int i=0; i<Count; i++ )
			{
				if(this[i].Level==level)
					return this[i];
			}
			return null;
		}

        public void AddByLevel(GroupSchemaItem value)
        {
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                if (this[i].Level > value.Level)
                {
                    Insert(i, value);
                    return;
                }
            }
            Add(value);
        }

		public int Add( GroupSchemaItem value )  
		{
			return( List.Add( value ) );
		}

		public int IndexOf( GroupSchemaItem value )  
		{
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, GroupSchemaItem value )  
		{
			List.Insert( index, value );
		}

		public void Remove( GroupSchemaItem value )  
		{
			List.Remove( value );
		}

		public bool Contains( GroupSchemaItem value )  
		{
			return( List.Contains( value ) );
		}
		#region ICloneable 成员

		public object Clone()
		{
			GroupSchemaItems gsis=new GroupSchemaItems();
			foreach(GroupSchemaItem gsi in this.InnerList)
				gsis.Add(gsi.Clone() as GroupSchemaItem);
			return gsis;
		}

		#endregion
	}
}
