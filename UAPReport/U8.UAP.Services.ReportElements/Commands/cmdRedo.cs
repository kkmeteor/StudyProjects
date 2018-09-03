using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// cmdRedo 的摘要说明。
	/// </summary>
	public class cmdRedo:Command
	{
		private ICommand _undo; 
		public cmdRedo(ICommand undo):base()
		{
			_commandtype=CommandTypes.Redo;
			_undo=undo;
		}

        public ICommand UndoCommand
        {
            get
            {
                return _undo;
            }
        }

		public override void Undo()
		{
			_undo.Redo();
		}

		public override void Redo()
		{
            _undo.Undo();
		}
	}
}
