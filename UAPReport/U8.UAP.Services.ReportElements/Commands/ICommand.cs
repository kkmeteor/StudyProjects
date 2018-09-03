using System;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ICommand 的摘要说明。
	/// </summary>
	public interface ICommand
	{
		CommandTypes CommandType{get;}
		int Count{get;}
		void AddReceiver(object cell);
		void UpdateState(object cell);
		void Redo();
		void Undo();
        void AfterCommand();
	}

	public enum CommandTypes
	{
		None,
		AddCell,
		DeleteCell,
		Move,                    //x,y
		Resize,                  //x,y,w,h
		ResizeSection,            //h
		AddSection,
		DeleteSection,
		Redo,
		Name,
		Caption,
		BackColor,
		ForeColor,
		Border,
		BorderWidth,
		BorderColor,
		CaptionAlign,
		Font,
		ChangeZorder,
		Reference,
		SizeModeChange
	}

    public enum PropertyType
    {
        IdentityCaption,
        KeepPos,
        ControlAuth,
        Visible
    }

    public class PropertyChangeArgs : EventArgs
    {
        private PropertyType _type;
        public PropertyChangeArgs(PropertyType type)
        {
            _type = type;
        }

        public PropertyType Type
        {
            get
            {
                return _type;
            }
        }
    }
}
