using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdResizeSection 的摘要说明。
	/// </summary>
	public class CmdResizeSection:Command
	{
		public CmdResizeSection():base()
		{
			_commandtype=CommandTypes.ResizeSection;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Cell).Height);
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Cell).Height);
		}

		public override void Undo()
		{
			(_receivers[0] as Section).Height=Convert.ToInt32(_oldstates[0]);
		}

		public override void Redo()
		{
			(_receivers[0] as Section).Height=Convert.ToInt32(_newstates[0]);
		}

	}

}
