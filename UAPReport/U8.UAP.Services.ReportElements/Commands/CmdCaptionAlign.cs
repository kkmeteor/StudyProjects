using System;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdCaptionAlign 的摘要说明。
	/// </summary>
	public class CmdCaptionAlign:Command
	{
		public CmdCaptionAlign()
		{
			_commandtype=CommandTypes.CaptionAlign;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).CaptionAlign );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).CaptionAlign );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetCaptionAlign((ContentAlignment)_oldstates[i]);
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetCaptionAlign((ContentAlignment)_newstates[i]);
			}
		}
	}
}
