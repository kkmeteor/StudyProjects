using System;
using System.Drawing;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdResize 的摘要说明。
	/// </summary>
	public class CmdResize:Command
	{
        //private ArrayList _oldparents;
        //private ArrayList _newparents;
		public CmdResize():base()
		{
			_commandtype=CommandTypes.Resize;
            //_oldparents=new ArrayList();
            //_newparents=new ArrayList();
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add(new Rectangle((cell as Cell).X,(cell as Cell).RelativeY,(cell as Cell).Width,(cell as Cell).Height ));
            //_oldparents.Add ((cell as Cell).Parent);
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add(new Rectangle((cell as Cell).X,(cell as Cell).RelativeY,(cell as Cell).Width,(cell as Cell).Height ));
            //_newparents.Add ((cell as Cell).Parent);
		}

        public override void AfterCommand()
        {
            Cell cell = _receivers[0] as Cell;
            if(!(cell is SuperLabel))
                InvokeAutoDesign(cell.Parent, cell.RelativeY, cell.Height);
            InvokeMetrixLayout();
        }

        private void InvokeMetrixLayout()
        {
            Cell cell = _receivers[0] as Cell;
            if (cell is IPart)
                (cell as IPart).Metrix.RefreshLayoutByPart();
            else if (cell is IMetrix )
                (cell as IMetrix ).RefreshLayoutBySelf();
        }

		public override void Undo()
		{
            //for(int i=0;i<_receivers.Count;i++)
            //{
            int i=0;
				Cell cell=_receivers[i] as Cell;
				Rectangle rect=(Rectangle)_oldstates[i];
                Section oldparent = cell.Parent;
				cell.X=rect.X;
				cell.RelativeY=rect.Y;
				cell.Y=oldparent.Y+rect.Y;
				cell.Width=rect.Width;
				cell.Height=rect.Height;
				
                //oldparent.Cells.Add(cell);
            //}
                if (!(cell is SuperLabel))
                    InvokeAutoDesign(cell.Parent, cell.RelativeY, cell.Height);
                InvokeMetrixLayout();
		}

		public override void Redo()
		{
            //for(int i=0;i<_receivers.Count;i++)
            //{
            int i = 0;
				Cell cell=_receivers[i] as Cell;
				Rectangle rect=(Rectangle)_newstates[i];
                Section newparent = cell.Parent;//(Section)_newparents[i];
				cell.X=rect.X;
				cell.RelativeY=rect.Y;
				cell.Y=newparent.Y+rect.Y;
				cell.Width=rect.Width;
				cell.Height=rect.Height;

                //newparent.Cells.Add(cell);
            //}
                if (!(cell is SuperLabel))
                    InvokeAutoDesign(cell.Parent, cell.RelativeY, cell.Height);
                InvokeMetrixLayout();
		}

        private void InvokeAutoDesign(Section section, int y, int height)
        {
            if (section is IAutoDesign)
            {
                (section as IAutoDesign).AutoDesign(y, height);
            }
        }

	}
}
