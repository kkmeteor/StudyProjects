using System;
using System.Drawing;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdMoveX 的摘要说明。
	/// </summary>
	public class CmdMove:Command
	{
		private ArrayList _oldparents;
		private ArrayList _newparents;
        private Hashtable _oldlayouts;
        private Hashtable _newlayouts;
		public CmdMove():base()
		{
			_commandtype=CommandTypes.Move ;
			_oldparents=new ArrayList();
			_newparents=new ArrayList();
            _oldlayouts  = new Hashtable();
            _newlayouts = new Hashtable();
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add(new Point((cell as Cell).X,(cell as Cell).RelativeY ));
			_oldparents.Add ((cell as Cell).Parent);
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add(new Point((cell as Cell).X,(cell as Cell).RelativeY ));
			_newparents.Add((cell as Cell).Parent);
		}

        public override void AfterCommand()
        {
            _oldlayouts.Clear();
            _newlayouts.Clear();
            #region new layout
            for (int i = 0; i < _newparents.Count; i++)
            {
                Section section = _newparents[i] as Section;
                if (!_newlayouts.Contains(section))
                {
                    Hashtable ht = new Hashtable();
                    foreach (Cell cell in section.Cells)
                        ht.Add(cell, cell.Location);
                    _newlayouts.Add(section, ht);
                }
            }
            for (int i = 0; i < _oldparents.Count; i++)
            {
                Section section = _oldparents[i] as Section;
                if (!_newlayouts.Contains(section))
                {
                    Hashtable ht = new Hashtable();
                    foreach (Cell cell in section.Cells)
                        ht.Add(cell, cell.Location);
                    _newlayouts.Add(section, ht);
                }
            }
            #endregion
            InnerUndo();
            #region oldlayout
            for (int i = 0; i < _newparents.Count; i++)
            {
                Section section = _newparents[i] as Section;
                if (!_oldlayouts.Contains(section))
                {
                    Hashtable ht = new Hashtable();
                    foreach (Cell cell in section.Cells)
                        ht.Add(cell, cell.Location);
                    _oldlayouts.Add(section, ht);
                }
            }
            for (int i = 0; i < _oldparents.Count; i++)
            {
                Section section = _oldparents[i] as Section;
                if (!_oldlayouts.Contains(section))
                {
                    Hashtable ht = new Hashtable();
                    foreach (Cell cell in section.Cells)
                        ht.Add(cell, cell.Location);
                    _oldlayouts.Add(section, ht);
                }
            }
            #endregion
            InnerRedo();
            //design
            foreach (Section section in _newlayouts.Keys)
                InvokeAutoDesign(section);
            InvokeMetrixLayout();
        }

        private void InvokeMetrixLayout()
        {
            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                if (cell is IPart)
                    (cell as IPart).Metrix.MoveAPart(cell as IPart);
                else if (cell is IMetrix )
                    (cell as IMetrix ).RefreshLayoutBySelf();
            }
        }

        private void InnerUndo()
        {
            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                Point pt = (Point)_oldstates[i];
                Section oldparent = (Section)_oldparents[i];
                cell.X = pt.X;
                cell.RelativeY = pt.Y;
                cell.Y = oldparent.Y + pt.Y;

                oldparent.Cells.Add(cell);
            }
        }

		public override void Undo()
		{
            foreach (Section section in _oldlayouts.Keys)
            {
                Hashtable ht=_oldlayouts[section] as Hashtable;
                foreach (Cell cell in ht.Keys)
                    cell.SetLocation((Point)ht[cell]);
            }
            InnerUndo();
            foreach (Section section in _oldlayouts.Keys)
            {
                SingleInvokeAutoDesign(section);
            }
            InvokeMetrixLayout();
		}

		public override void Redo()
		{
            foreach (Section section in _newlayouts.Keys)
            {
                Hashtable ht = _newlayouts[section] as Hashtable;
                foreach (Cell cell in ht.Keys)
                    cell.SetLocation((Point)ht[cell]);
            }
            InnerRedo();
            foreach (Section section in _newlayouts.Keys)
            {
                InvokeAutoDesign(section);
            }
            InvokeMetrixLayout();
		}

        private void InnerRedo()
        {
            for (int i = 0; i < _receivers.Count; i++)
            {
                Cell cell = _receivers[i] as Cell;
                Point pt = (Point)_newstates[i];
                Section newparent = (Section)_newparents[i];
                cell.X = pt.X;
                cell.RelativeY = pt.Y;
                cell.Y = newparent.Y + pt.Y;

                newparent.Cells.Add(cell);
            }
        }

        public void SingleInvokeAutoDesign(Section section)
        {
            if (section is IAutoDesign)
            {
                Cells cells = section.Cells;
                section.Cells = new Cells();
                foreach (Cell cell in cells)
                    section.Cells.Add(cell);
                (section as IAutoDesign).AutoDesign();
            }
        }

        private void InvokeAutoDesign(Section section)
        {
            if (section is IAutoDesign)
            {
                Cells cells = section.Cells;
                section.Cells = new Cells();
                foreach (Cell cell in cells)
                    section.Cells.Add(cell);
                cells = new Cells();
                foreach (object ot in _receivers)
                {
                    if (ot is Cell && (ot as Cell).Parent == section)
                        cells.Add(ot as Cell);
                }
                (section as IAutoDesign).AutoDesign(cells);
            }
        }
	}
}
