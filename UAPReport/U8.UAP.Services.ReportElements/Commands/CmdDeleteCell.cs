using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdDelete 的摘要说明。
	/// </summary>
	public class CmdDeleteCell:Command
	{
        private Hashtable _hashs;
		public CmdDeleteCell():base()
		{
			_commandtype=CommandTypes.DeleteCell;
            _hashs = new Hashtable();
		}

        public override void AfterCommand()
        {
            ArrayList metrixs = new ArrayList();
            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                if (cell is IMetrix )
                    metrixs.Add(cell);
            }
            foreach (IMetrix im in metrixs)
            {
                foreach (Cell cp in im.AllParts)
                {
                    if (!_receivers.Contains(cp))
                    {
                        cp.Parent.Cells.Remove(cp);
                        _receivers.Add(cp);
                    }
                }
            }

            _hashs.Clear();
            //undo
            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                cell.Parent.Cells.AddDirectly (cell);
                if (!_hashs.Contains(cell.Parent))
                    _hashs.Add(cell.Parent, new Hashtable());
            }
            foreach (Section section in _hashs.Keys)
            {
                Hashtable hash = _hashs[section] as Hashtable;
                foreach (Cell cell in section.Cells)
                    hash.Add(cell, cell.X);
            }

            //redo
            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                cell.Parent.Cells.Remove(cell);
            }

            //autodesign
            foreach (Section section in _hashs.Keys)
                InvokeAutoDesign(section);

            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                if (cell is IPart)
                    (cell as IPart).Metrix.RemoveAPart(cell as IPart);
            }
        }

		public override void Undo()
		{
            foreach (Section section in _hashs.Keys)
            {
                Hashtable hash = _hashs[section] as Hashtable;
                foreach (Cell cell in hash.Keys)
                    cell.X = (int)hash[cell];
            }

            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                cell.Parent.Cells.Add(cell);
            }
            
            foreach (Section section in _hashs.Keys)
                InvokeAutoDesign(section);

            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                if (cell is IPart)
                    (cell as IPart).Metrix.AddAPart(cell as IPart);
            }
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				Cell cell=_receivers[i] as Cell;
				cell.Parent.Cells.Remove(cell);
			}

            foreach (Section section in _hashs.Keys)
                InvokeAutoDesign(section);

            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                if (cell is IPart)
                    (cell as IPart).Metrix.RemoveAPart(cell as IPart);
            }
		}

        public void InvokeAutoDesign(Section section)
        {
            if (section is IAutoDesign)
            {
                (section as IAutoDesign).AutoDesign();
            }
        }
	}
}
