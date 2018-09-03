using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdCaption 的摘要说明。
	/// </summary>
	public class CmdCaption:Command
	{
		public CmdCaption()
		{
			_commandtype=CommandTypes.Caption;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).Caption );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).Caption );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetCaption(_oldstates[i].ToString());
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetCaption(_newstates[i].ToString());
			}
		}
	}
}
