using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// cmdAdd 的摘要说明。
	/// </summary>
	public class CmdAddCell:Command
	{
        private int _x = Int32.MaxValue;
        private ArrayList _indexs;
        private Hashtable _hashs;
		public CmdAddCell():base()
		{
			_commandtype=CommandTypes.AddCell;
            _indexs = new ArrayList();
            _hashs = new Hashtable();
		}

		public override void Undo()
		{
            _indexs.Clear();
            _hashs.Clear();
            Section section=(_receivers[0] as Cell ).Parent ;
            for (int i = 0; i < _receivers.Count; i++)
            {
                int index = section.Cells.IndexOf(_receivers[i] as Cell);
                _indexs.Add(index);
                _hashs.Add(index, _receivers[i]);
            }
            _indexs.Sort();

			for(int i=0;i<_receivers.Count;i++)
			{
				Cell cell=_receivers[i] as Cell;
				section.Cells.Remove(cell);                
			}
            InvokeAutoDesignWithX(section );
            if (_receivers[0] is IPart)
                (_receivers[0] as IPart).Metrix.RemoveAPart(_receivers[0] as IPart);
		}

        public override void AfterCommand()
        {
            Section section = (_receivers[0] as Cell).Parent;
            foreach (Cell cell in section.Cells)
            {
                if (cell != (_receivers[0] as Cell))
                {
                    _x = cell.X;
                    break;
                }
            }
            InvokeAutoDesign(section);
            if (_receivers[0] is IPart )
                (_receivers[0] as IPart).Metrix.AddAPart(_receivers[0] as IPart);
        }

    	public override void Redo()
		{
            foreach (int index in _indexs)
            {
                Cell cell = _hashs[index] as Cell;
                cell.Parent.Cells.Insert(index, cell);
            }

            InvokeAutoDesign((_receivers[0] as Cell).Parent);
            if (_receivers[0] is IPart)
                (_receivers[0] as IPart).Metrix.AddAPart(_receivers[0] as IPart);
		}

        public void InvokeAutoDesign(Section section)
        {
            if (section is IAutoDesign)
            {
                (section as IAutoDesign).AutoDesign();
            }
        }

        public void InvokeAutoDesignWithX(Section section)
        {
            if (section is IAutoDesign)
            {
                (section as IAutoDesign).AutoDesign(_x);
            }
        }
	}
}
