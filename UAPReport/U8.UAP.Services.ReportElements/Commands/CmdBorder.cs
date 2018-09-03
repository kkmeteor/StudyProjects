using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdBorder 的摘要说明。
	/// </summary>
	public class CmdBorder:Command
	{
		public CmdBorder()
		{
			_commandtype=CommandTypes.Border;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).Border.Clone() );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).Border.Clone() );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).Border  =(BorderSide)_oldstates[i];
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).Border =(BorderSide)_newstates[i];
			}
		}
	}
}
