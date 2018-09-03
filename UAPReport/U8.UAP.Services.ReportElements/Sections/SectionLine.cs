using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// SectionLine 的摘要说明。
	/// </summary>
	public class SectionLine:IDisposable,ICloneable
	{
		private Cells _cells;
		private Section _section;
		public SectionLine(Section section)
		{
			_section=section;
			_cells=new Cells();
		}

		public SectionLine(SectionLine sectionline)
		{
			_section=sectionline.Section;
			_cells=sectionline.Cells.Clone() as Cells;
		}

		public Section Section
		{
			get
			{
				return _section;
			}
		}

        public int Index
        {
            get
            {
                return _section.SectionLines.IndexOf(this);
            }
        }

		public Cells Cells
		{
			get
			{
				return _cells;
			}
		}

		public bool ShouldContains(Cell value)
		{
            for(int i=0;i<_cells.Count;i++)
            {
			    if((value.Y>=_cells[i].Y && value.Y<_cells[i].Y+_cells[i].Height)||
				    (_cells[i].Y>=value.Y && _cells[i].Y<value.Y+value.Height))
				    return true;
            }
            return false;
		}

		public int Right
		{
			get
			{
				return _cells.Right ;
			}
		}

		public int X
		{
			get
			{
				return _cells.X ;
			}
		}

		public int Y
		{
			get
			{
				return _cells.Y;
			}
		}


		#region IDisposable 成员

		public void Dispose()
		{
			_section=null;
			if(_cells!=null)
			{
				_cells.Dispose();
				_cells=null;
			}
		}

		#endregion

		#region ICloneable 成员

		public object Clone()
		{
			return new SectionLine(this);
		}

		#endregion
	}
}
