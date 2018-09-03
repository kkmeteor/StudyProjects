using System;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdBorderColor 的摘要说明。
	/// </summary>
	public class CmdBorderColor:Command
	{
		public CmdBorderColor()
		{
			_commandtype=CommandTypes.BorderColor;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).BorderColor );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).BorderColor );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetBorderColor((Color)_oldstates[i]);
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
