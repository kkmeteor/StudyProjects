using System;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdBackColor 的摘要说明。
	/// </summary>
	public class CmdBackColor:Command
	{
		public CmdBackColor()
		{
			_commandtype=CommandTypes.BackColor;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).BackColor );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).BackColor );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetBackColor((Color)_oldstates[i]);
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetBackColor((Color)_newstates[i]);
			}
		}

	}
}
