using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdBorderWidth ��ժҪ˵����
	/// </summary>
	public class CmdBorderWidth:Command
	{
		public CmdBorderWidth()
		{
			_commandtype=CommandTypes.BorderWidth ;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).BorderWidth );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).BorderWidth );
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetBorderWidth((int)_oldstates[i]);
			}
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
			{
				(_receivers[i] as Cell).SetBorderWidth((int)_newstates[i]);
			}
		}
	}
}
