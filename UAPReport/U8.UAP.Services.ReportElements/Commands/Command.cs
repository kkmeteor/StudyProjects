using System;
using System.Data;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Command 的摘要说明。
	/// </summary>
	public abstract class Command:ICommand
	{
		protected ArrayList _receivers;
		protected ArrayList _oldstates;
		protected ArrayList _newstates;
		protected CommandTypes _commandtype;

		public Command()
		{
			_receivers=new ArrayList();
			_oldstates=new ArrayList();
			_newstates=new ArrayList();
		}
		#region ICommand 成员

		public CommandTypes CommandType
		{
			get
			{
				return _commandtype;
			}
		}

        public void AddReceiver(object cell)
		{
			if(!_receivers.Contains(cell))
			{
				_receivers.Add(cell);
				AddOldState(cell);
			}
		}

		public int Count
		{
			get
			{
				return _receivers.Count;
			}
		}
		
		protected virtual void AddOldState(object cell)
		{
		}	

		public virtual void UpdateState(object cell)
		{
		}

        public virtual void AfterCommand()
        {
        }

		public abstract void Redo();
		public abstract void Undo();	

		#endregion
	}
}
