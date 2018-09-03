using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Commands 的摘要说明。
	/// </summary>
	public class Commands:CollectionBase
	{
		private const int UNDOCOUNT=200;
		public ICommand this[int index]
		{
			get {  return List[index] as ICommand;  }
		}

		public int Add( ICommand value )  
		{
			if(Count>UNDOCOUNT)
				Remove(this[0]);
			return( List.Add( value ) );
		}

		public int IndexOf( ICommand value )  
		{
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, ICommand value )  
		{
			List.Insert( index, value );
		}

		public void Remove( ICommand value )  
		{
			List.Remove( value );
		}

		public bool Contains( ICommand value )  
		{
			return( List.Contains( value ) );
		}

		public ICommand Pop()
		{
			ICommand command=this[Count-1];
			Remove(command);
			return command;
		}
	}
}
