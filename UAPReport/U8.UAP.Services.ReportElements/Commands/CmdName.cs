using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdName 的摘要说明。
	/// </summary>
	public class CmdName:Command
	{
		public CmdName()
		{
			_commandtype=CommandTypes.Name;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).Name);
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).Name );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetName(_oldstates[i].ToString());
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetName(_newstates[i].ToString());
			}
		}
	}
}
