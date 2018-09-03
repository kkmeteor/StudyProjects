using System;
using System.Data;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportData
{
	[Serializable]
	public class SimpleViewCollection : CollectionBase
	{
		public SimpleViewCollection(){}

		public SimpleView DefaultView
		{
			get
			{
				for( int i = 0; i < this.Count; i++ )
				{
					if( this[i].IsDefault )
						return this[i];
				}

                if (this.Count > 0)
                    return this[0];
				return null;
			}
		}

		public SimpleView this[ string viewid ]
		{
			get
			{
				for(int i=0;i<Count;i++)
				{
					if( this[i].ID.ToLower()== viewid.ToLower() )
						return this[i];
				}
				return null;
			}
		}

		public SimpleView this[int index]
		{
			get{ return (SimpleView)( base.List[index] ); }
		}
	
		public bool Contains( string viewid )
		{
			return this[viewid]!= null;
		}

		public void Add( SimpleView item )
		{
			item.Parent = this;
			this.List.Add( item );
		}

		internal void Remove( SimpleView item )
		{
			this.List.Remove(item);
		}
	}
}
