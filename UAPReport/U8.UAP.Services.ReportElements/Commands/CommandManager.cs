using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CommandManager 的摘要说明。
	/// </summary>
	public class CommandManager
	{
		private Commands _undolist;
		private Commands _redolist;	
		private CommandTypes _commandtype=CommandTypes.None;

		private ICommand _currentcommand;
		private bool _bcommand=false;

		public CommandManager()
		{
			_undolist=new Commands();
			_redolist=new Commands();
		}

		public ICommand CurrentCommand
		{
			get
			{
				return _currentcommand;
			}
			set
			{
				_currentcommand=value;
			}
		}
		
		public CommandTypes CommandType
		{
			get
			{
				return _commandtype;
			}
		}
		public bool CanUndo
		{
			get
			{
				return _undolist.Count>0;
			}
		}
		public bool CanRedo
		{
			get
			{
				return _redolist.Count>0;
			}
		}

		public bool bCommand
		{
			get
			{
				return _bcommand;
			}
			set
			{
				_bcommand=value;
			}
		}

		public void ClearCommand()
		{
			_redolist.Clear();
			_undolist.Clear();
			_bcommand=false;
		}

		public CommandTypes UndoCommand
		{
			get
			{
				if(CanUndo)
					return _undolist[_undolist.Count-1].CommandType;
				else
					return CommandTypes.None;
			}
		}

		public CommandTypes RedoCommand
		{
			get
			{
				if(CanRedo)
					return _redolist[_redolist.Count-1].CommandType;
				else
					return CommandTypes.None;
			}
		}

        public void BeignCommand()
		{
			_commandtype=_currentcommand.CommandType;
		}
		public void CancelCommand()
		{
			if(_currentcommand!=null && _currentcommand.Count>0)
				_currentcommand.Undo();
			_commandtype=CommandTypes.None;
			_currentcommand=null;
		}

		public void EndCommand()
		{
			if(_currentcommand.Count>0)
			{
				for(int i=_redolist.Count-1;i>=0;i--)
				{
                    _undolist.Add(_redolist[i]);
					_undolist.Add(new cmdRedo(_redolist[i]));
				}
				_undolist.Add(_currentcommand);
				_redolist.Clear();
				_bcommand=true;

                _currentcommand.AfterCommand();
                #region autodesign
                
                //ArrayList alp = new ArrayList();
                //if (_currentcommand.CommandType == CommandTypes.AddCell
                //    || _currentcommand.CommandType == CommandTypes.DeleteCell)
                //{
                //    foreach (object o in _currentcommand.Receivers)
                //    {
                //        if (o is Cell && ! alp.Contains((o as Cell).Parent.Name ))
                //        {
                //            alp.Add((o as Cell).Parent.Name);
                //            InvokeAutoDesign((o as Cell).Parent);
                //        }
                //    }
                //}
                //else if(_currentcommand.CommandType== CommandTypes.Move )
                //{
                //    foreach (object o in _currentcommand.Receivers)
                //    {
                //        if (o is Cell && !alp.Contains((o as Cell).Parent.Name))
                //        {
                //            alp.Add((o as Cell).Parent.Name);                            
                //            InvokeAutoDesignWhenMove((o as Cell).Parent );
                //        }
                //    }
                //}
                //else if (_currentcommand.CommandType == CommandTypes.Resize)
                //{
                //    foreach (object o in _currentcommand.Receivers)
                //    {
                //        if (o is Cell && !alp.Contains((o as Cell).Parent.Name))
                //        {
                //            alp.Add((o as Cell).Parent.Name);
                //            InvokeAutoDesign((o as Cell).Parent, (o as Cell).Y, (o as Cell).Height);
                //        }
                //    }
                //}
                #endregion
            }
			_commandtype=CommandTypes.None ;
			_currentcommand=null;
		}

		public void Undo()
		{
			if(CanUndo)
			{
				ICommand command=_undolist.Pop();
				command.Undo();
                //if (command.CommandType != CommandTypes.Redo)
                    _redolist.Add(command);
                //else
                //    _redolist.Add((command as cmdRedo).UndoCommand );
                _bcommand = true;
			}
		}

		public void Redo()
		{
			if(CanRedo)
			{
				ICommand command=_redolist.Pop();
				command.Redo();
				_undolist.Add(command);
				_bcommand=true;
			}
		}
	}
}
