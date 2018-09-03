using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdChangeZorder 的摘要说明。
	/// </summary>
	public class CmdChangeZorder:Command
	{
		public CmdChangeZorder()
		{
			_commandtype=CommandTypes.ChangeZorder;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).Z_Order );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).Z_Order );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetZOrder((int)_oldstates[i]);
				(_receivers[i] as Cell).Parent.Cells.Remove((_receivers[i] as Cell));
				(_receivers[i] as Cell).Parent.Cells.Add((_receivers[i] as Cell));
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetZOrder((int)_newstates[i]);
				(_receivers[i] as Cell).Parent.Cells.Remove((_receivers[i] as Cell));
				(_receivers[i] as Cell).Parent.Cells.Add((_receivers[i] as Cell));
			}
		}
	}
}
