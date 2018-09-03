using System;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdForeColor 的摘要说明。
	/// </summary>
	public class CmdForeColor:Command
	{
		public CmdForeColor()
		{
			_commandtype=CommandTypes.ForeColor;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).ForeColor );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).ForeColor );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetForeColor((Color)_oldstates[i]);
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetForeColor((Color)_newstates[i]);
			}
		}
	}
}
