using System;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// SelectedCells 的摘要说明。
	/// </summary>
	[Serializable]
	public class SelectedCells:ICloneable,ISerializable
	{
		private ArrayList _cells;
		private ArrayList _xcells;
		private ArrayList _ycells;

		public SelectedCells()
		{
			_cells=new ArrayList();
			_xcells=new ArrayList();
			_ycells=new ArrayList();
		}

		protected SelectedCells(SerializationInfo info,StreamingContext context)
		{
			_cells=(ArrayList)info.GetValue("Cells",typeof(ArrayList));
		}

		private SelectedCells(bool bclone)
		{
			_cells=new ArrayList();
		}

		private void Add(Cell value)
		{
			_cells.Add(value);
		}

		public IEnumerator GetEnumerator()
		{
			return _cells.GetEnumerator();
		}

		public bool Contains(Cell value)
		{
			return _cells.Contains(value);
		}

		public Cell this[int index]
		{
			get
			{
				if(index>Count-1)
					return null;
				return _cells[index] as Cell;
			}
		}

		public int Count
		{
			get
			{
				return _cells.Count;
			}
		}

		public Cell XCells(int index)
		{
			if(index>Count-1)
				return null;
			return _xcells[index] as Cell;
		}

		public Cell YCells(int index)
		{
			if(index>Count-1)
				return null;
			return _ycells[index] as Cell;
		}

		public void Remove(Cell value)
		{
			if(!_cells.Contains(value))
				return ;
			_cells.Remove(value);
			_xcells.Remove(value);
			_ycells.Remove(value);
		}

		public void AddSelected(Cell value)
		{
			if(_cells.Contains(value))
				return ;			
			AddToXCells(value);
			AddToYCells(value);
			AddToCells(value);
		}

		public void Clear()
		{
			_cells.Clear();
			_xcells.Clear();
			_ycells.Clear();
		}

		private void AddToCells(Cell value)
		{
			_cells.Add(value);
		}

		private void AddToXCells(Cell value)
		{
			for(int i=0;i<Count;i++)
			{
				if(value.X<XCells(i).X)
				{
					_xcells.Insert(i,value);
					return;
				}
			}
			_xcells.Add(value);
		}

		private void AddToYCells(Cell value)
		{
			for(int i=0;i<Count;i++)
			{
				if(value.Y<YCells(i).Y)
				{
					_ycells.Insert(i,value);
					return;
				}
			}
			_ycells.Add(value);
		}
		#region ICloneable 成员

		public object Clone()
		{
			SelectedCells cells=new SelectedCells(true);
			for(int i=0;i<Count;i++)
			{
				cells.Add((this[i] as ICloneable).Clone() as Cell);
			}
			return cells;
		}

		#endregion

		#region ISerializable 成员

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Cells",_cells);
		}

		#endregion
	}
}
