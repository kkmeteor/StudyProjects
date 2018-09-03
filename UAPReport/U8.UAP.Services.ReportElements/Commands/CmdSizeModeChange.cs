using System;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdReference 的摘要说明。
	/// </summary>
	public class CmdSizeModeChange:Command
	{
		public CmdSizeModeChange()
		{
			_commandtype=CommandTypes.SizeModeChange;
		}

		protected override void AddOldState(object cell)
		{
			_oldstates.Add((cell as Image).ImageString );
			_oldstates.Add((cell as Image).Width );
			_oldstates.Add((cell as Image).Height );
			_oldstates.Add((cell as Image).SizeMode );
		}

		public override void UpdateState(object cell)
		{
			_newstates.Add((cell as Image).ImageString );
			_newstates.Add((cell as Image).Width );
			_newstates.Add((cell as Image).Height );
			_newstates.Add((cell as Image).SizeMode );
		}

		public override void Undo()
		{
			Image image=_receivers[0] as Image ;
			image.SetBack(_oldstates[0].ToString(),(int)_oldstates[1],(int)_oldstates[2],(PictureBoxSizeMode)_oldstates[3]);
		}

		public override void Redo()
		{
			Image image=_receivers[0] as Image ;
			image.SetBack(_newstates[0].ToString(),(int)_newstates[1],(int)_newstates[2],(PictureBoxSizeMode)_newstates[3]);
		}
	}
}
