using System;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdFont 的摘要说明。
	/// </summary>
	public class CmdFont:Command
	{
		public CmdFont()
		{
			_commandtype=CommandTypes.Font;
		}

		protected override void AddOldState(object cell)
		{
            _oldstates.Add((cell as Cell).ClientFont);
		}

		public override void UpdateState(object cell)
		{
            _newstates.Add((cell as Cell).ClientFont);
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
                (_receivers[i] as Cell).SetClientFont((Font)_oldstates[i]);
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
                (_receivers[i] as Cell).SetClientFont((Font)_newstates[i]);
			}
		}
	}
}
