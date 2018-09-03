using System;
using System.Collections;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupSection 的摘要说明。
	/// </summary>
	[Serializable]
	public abstract class GroupSection:Section,ISerializable,ICloneable,IAlternativeStyle
	{
        protected  Color _backcolor2;
        protected Color _bordercolor2;
        protected Color _forecolor2;
        protected ServerFont _serverfont2 = new ServerFont();
        protected bool _bapplyalternative = false;
        protected bool _bapplysecondstyle = false;
        protected bool _balreadysetsecondstyle = false;

		public GroupSection():base()
		{
		}

		public GroupSection(int level):this()
		{
			_level=level;
		}

		public GroupSection(int height,int level):this(level)
		{
			_h=height;
		}

		public GroupSection(GroupSection groupsection):base(groupsection)
		{
            _backcolor2 = groupsection.BackColor2;
            _bordercolor2 = groupsection.BorderColor2;
            _forecolor2 = groupsection.ForeColor2;
            //_serverfont2 = groupsection.ServerFont2;
            _bapplyalternative = groupsection.bApplyAlternative;
		}

		protected GroupSection( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            if (_version > 1)
            {
                _backcolor2 = (Color)info.GetValue("BackColor2", typeof(Color));
                _bordercolor2 = (Color)info.GetValue("BorderColor2", typeof(Color));
                _forecolor2 = (Color)info.GetValue("ForeColor2", typeof(Color));
                _serverfont2 = (ServerFont)info.GetValue("ServerFont2", typeof(ServerFont));
                _bapplyalternative = info.GetBoolean("bApplyAlternative");
            }
		}		


		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("BackColor2", _backcolor2);
            info.AddValue("BorderColor2", _bordercolor2);
            info.AddValue("ForeColor2", _forecolor2);
            info.AddValue("ServerFont2", _serverfont2);
            info.AddValue("bApplyAlternative", _bapplyalternative);
		}

		#endregion

		#region ICloneable 成员

		#endregion

        #region IAlternativeStyle 成员
        [Browsable(false)]
        public bool bAlreadySetSecondStyle
        {
            get
            {
                return _balreadysetsecondstyle;
            }
            set
            {
                _balreadysetsecondstyle = value;
            }
        }
        [Browsable(false)]
        public bool bApplySecondStyle
        {
            get
            {
                return _bapplysecondstyle;
            }
            set
            {
                _bapplysecondstyle = value;
            }
        }

        [Browsable(false)]
        public bool bApplyAlternative
        {
            get
            {
                return _bapplyalternative;
            }
            set
            {
                _bapplyalternative = value;
            }
        }

        [Browsable(false)]
        public Color BackColor2
        {
            get
            {
                return _backcolor2;
            }
            set
            {
                _backcolor2 = value;
            }
        }

        [Browsable(false)]
        public Color BorderColor2
        {
            get
            {
                return _bordercolor2;
            }
            set
            {
                _bordercolor2 = value;
            }
        }

        [Browsable(false)]
        public Color ForeColor2
        {
            get
            {
                return _forecolor2;
            }
            set
            {
                _forecolor2 = value;
            }
        }

        [Browsable(false)]
        public ServerFont ServerFont2
        {
            get
            {
                return _serverfont2;
            }
        }

        #endregion

        protected override void CopySectionStyleToLabel(Cell cell)
        {
            cell.SetBackColor(_backcolor2);
            //cell.SetBorderColor(_bordercolor2);
            cell.SetForeColor(_forecolor2);
            cell.ServerFont=_serverfont2;
        }

        protected override void InterpretSectionCaption()
        {
            base.InterpretSectionCaption();
            _caption = string.Format(_caption, _level.ToString());
        }
    }
}
