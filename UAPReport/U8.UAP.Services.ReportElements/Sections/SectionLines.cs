using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// SectionLines 的摘要说明。
	/// </summary>
	public class SectionLines:CollectionBase,IDisposable
	{
		public SectionLine this[int index]
		{
			get {  return List[index] as SectionLine;  }
		}

		public int Add( SectionLine value )  
		{
			for(int i=0;i<Count;i++)
			{
                SectionLine sl = this[i];
				if(value.Cells.Count>0 && sl.Cells.Count>0 && value.Cells[0].Y <sl.Cells[0].Y )
				{
					Insert(i,value);
					return i;
				}
			}
			return( List.Add( value ) );
		}

		public int IndexOf( SectionLine value )  
		{			
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, SectionLine value )  
		{
			if(index>=Count)
				List.Add(value);
			else
				List.Insert( index, value );
		}

		public void AddAfter(SectionLine a,SectionLine b)
		{
			int index=IndexOf(a);
			Insert(index+1,b);
		}

		public void Remove( SectionLine value )  
		{
			List.Remove( value );
		}

		public bool Contains( SectionLine value )  
		{
			return( List.Contains( value ) );
		}

		public int X
		{
			get
			{
				int _x=Int32.MaxValue;
				foreach(SectionLine value in List)
				{
					if(value.X <_x)
						_x=value.X ;					
				}
				return _x;
			}
		}

		public int Right
		{
			get
			{
				int _right=Int32.MinValue;
				foreach(SectionLine value in List)
				{
					if(value.Right >_right)
						_right=value.Right ;
				}
				return _right;
			}
		}
		#region IDisposable 成员

		public void Dispose()
		{
			foreach(SectionLine value in List)
			{
				if(value!=null)
				{
					value.Dispose();
				}
			}
			this.Clear();
		}

		#endregion
	}
}
