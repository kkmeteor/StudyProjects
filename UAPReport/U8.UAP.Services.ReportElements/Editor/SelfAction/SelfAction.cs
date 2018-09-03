using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GlobalVarient 的摘要说明。
	/// </summary>
	[Serializable]
	public class SelfAction:DisplyTextCustomTypeDescriptor,ISerializable,ICloneable,ICollectionEditType
	{
		private string _name="";
		private string _caption="";
		private string _imagestring="";
		private string _actionclass;
		private string _tooltip="";
		private bool _bdoubleclickaction=false;
		private object _tag;
		private bool _bshowcaption=false;
        private bool _bneedcontext = true;
		public event NameCheckHandler NameCheck;
		public event NameCheckHandler CaptionCheck;
		public event EventHandler bDoubleClickActionEvent;
        public event DesignLocaleIDFatcher DesignLocaleID;
        private string _encaption;
        private string _twcaption;
        private string _entip;
        private string _twtip;
        private System.Data.DataRow dataRow;

		public SelfAction()
		{			
		}

		public SelfAction(string name,string caption)
		{			
			_name=name;
			_caption=caption;
		}

		public SelfAction(SelfAction sa)
		{			
			_name=sa.Name;
			_caption=sa.CnCaption;
			_imagestring=sa.ImageString;
			_actionclass=sa.ActionClass;
			_tooltip=sa.CnTip;
			_bdoubleclickaction=sa.bDoubleClickAction;
			_bshowcaption=sa.bShowCaptionOnToolBar;
            _bneedcontext = sa.bNeedContext;
            _encaption = sa.EnCaption;
            _twcaption = sa.TwCaption;
            _entip = sa.EnTip;
            _twtip = sa.TwTip;            
		}

		private SelfAction(SerializationInfo info, StreamingContext context)
		{
            int version = 1;
            try
            {
                version = info.GetInt32("Version");
            }
            catch
            {
            }
			_name=info.GetString("Name");
			_caption=info.GetString("Caption");
			_imagestring=info.GetString("ImageString");
			_actionclass=info.GetString("ActionClass");
			_tooltip=info.GetString("ToolTip");
			_bdoubleclickaction=info.GetBoolean("bDoubleClickAction");
			_bshowcaption=info.GetBoolean("bShowCaption");
            if (version > 1)
            {
                _bneedcontext = info.GetBoolean("bNeedContext");
            }
            if (version > 2)
            {
                _encaption = info.GetString("EnCaption");
                _entip = info.GetString("EnTip");
                _twcaption = info.GetString("TwCaption");
                _twtip = info.GetString("TwTip");
            }
		}

        public SelfAction(System.Data.DataRow dataRow)
        {
            // TODO: Complete member initialization
            this.dataRow = dataRow;
            _name = dataRow["Name"].ToString();
            _caption = dataRow["caption"].ToString();
            _imagestring = dataRow["imagestring"].ToString();
            _actionclass = dataRow["actionclass"].ToString();
            _tooltip = dataRow["tooltip"].ToString();
            _bdoubleclickaction = (bool)dataRow["bdoubleclickaction"];
            _bshowcaption = (bool)dataRow["bshowcaption"];
            _bneedcontext = (bool)dataRow["bneedcontext"];
            _encaption = dataRow["encaption"].ToString();
            _twcaption = dataRow["twcaption"].ToString();
            _entip = dataRow["entooltip"].ToString();
            _twtip = dataRow["twtooltip"].ToString();          
        }

        [Browsable(false)]
        public bool bUseAllData
        {
            get
            {
                return _name.ToLower().StartsWith("alldataforaction_");
            }
        }

		[Browsable(false)]
		public System.Drawing.Image Image
		{
			get
			{
				if(_imagestring!=null && _imagestring.Length>0)
				{
					byte[] imageByte=Convert.FromBase64String(_imagestring);
					return System.Drawing.Image.FromStream(new MemoryStream(imageByte));
				}
				else
					return null;
			}
		}



        [Browsable(false)]
        public string EnCaption
        {
            get
            {
                return _encaption;
            }
            set
            {
                _encaption = value;
            }
        }

        [Browsable(false)]
        public string CnCaption
        {
            get
            {
                return _caption ;
            }
            set
            {
                _caption = value;
            }
        }

        [Browsable(false)]
        public string TwCaption
        {
            get
            {
                return _twcaption ;
            }
            set
            {
                _twcaption  = value;
            }
        }

        [Browsable(false)]
        public string EnTip
        {
            get
            {
                return _entip ;
            }
            set
            {
                _entip  = value;
            }
        }

        [Browsable(false)]
        public string CnTip
        {
            get
            {
                return _tooltip ;
            }
            set
            {
                _tooltip  = value;
            }
        }

        [Browsable(false)]
        public string TwTip
        {
            get
            {
                return _twtip ;
            }
            set
            {
                _twtip  = value;
            }
        }

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if(_name.ToLower().Trim()!=value.ToLower().Trim())
				{
					string bvalid="";
					if(NameCheck!=null)
						bvalid=NameCheck(value);
					if(bvalid!="")
                        MessageBox.Show(bvalid, String4Report.GetString("Report"));
					else
						_name=value;					
				}
			}
		}

		public bool bDoubleClickAction
		{
			get
			{
				return _bdoubleclickaction;
			}
			set
			{
				if(value && bDoubleClickActionEvent!=null)
					bDoubleClickActionEvent(this,EventArgs.Empty);
				_bdoubleclickaction=value;
			}
		}

        private string GetDesignLocaleID()
        {
            if (DesignLocaleID != null)
                return DesignLocaleID();
            else
                return System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        }

		public string Caption
		{
			get
			{
                string localeid = GetDesignLocaleID();
                if (localeid  == "en-US")
                {
                    return string.IsNullOrEmpty(_encaption) ? _caption : _encaption;
                }
                else if (localeid == "zh-TW")
                {
                    return string.IsNullOrEmpty(_twcaption ) ? _caption : _twcaption ;
                }
                else
                {
                    return _caption;
                }
			}
			set
			{
				if(Caption.ToLower().Trim()!=value.ToLower().Trim())
				{
					string bvalid="";
					if(CaptionCheck!=null)
						bvalid=CaptionCheck(value);
                    if (bvalid != "")
                        MessageBox.Show(bvalid, String4Report.GetString("Report"));
                    else
                    {
                        string localeid = GetDesignLocaleID();
                        if (localeid == "en-US")
                        {
                            _encaption = value;
                            if (string.IsNullOrEmpty(_caption))
                                _caption = value;
                        }
                        else if (localeid == "zh-TW")
                        {
                            _twcaption = value;
                            if (string.IsNullOrEmpty(_caption))
                                _caption = value;
                        }
                        else
                        {
                            _caption = value;
                        }
                    }
				}
			}
		}
        //U8.UAP.Services.ReportElements.Dis17
		[DisplayText("Image")]
		[LocalizeDescription("Image")]
		[TypeConverter(typeof(ImageTypeConverter))]
		[Editor(typeof(ImageEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public virtual string ImageString
		{
			get
			{
				return _imagestring;
			}
			set
			{
				_imagestring=value;
			}
		}


		public string ActionClass
		{
			get
			{
				return _actionclass;
			}
			set
			{
				_actionclass=value;
			}
		}

		public string ToolTip
		{
            get
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                {
                    return string.IsNullOrEmpty(_entip ) ? _tooltip  : _entip ;
                }
                else if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "zh-TW")
                {
                    return string.IsNullOrEmpty(_twtip ) ? _tooltip  : _twtip ;
                }
                else
                {
                    return _tooltip ;
                }
            }
            set
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                {
                    _entip  = value;
                    if (string.IsNullOrEmpty(_tooltip ))
                        _tooltip  = value;
                }
                else if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "zh-TW")
                {
                    _twtip  = value;
                    if (string.IsNullOrEmpty(_tooltip))
                        _tooltip = value;
                }
                else
                {
                    _tooltip  = value;
                }
            }
		}

		[Browsable(false)]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag=value;
			}
		}

		public bool bShowCaptionOnToolBar
		{
			get
			{
				return _bshowcaption;
			}
			set
			{
				_bshowcaption=value;
			}
		}

        public bool bNeedContext
        {
            get
            {
                return _bneedcontext;
            }
            set
            {
                _bneedcontext = value;
            }
        }

		#region ISerializable 成员

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
            info.AddValue("Version", 30);
			info.AddValue("Name",_name);
			info.AddValue("Caption",_caption);
			info.AddValue("ImageString",_imagestring);
			info.AddValue("ActionClass",_actionclass);
			info.AddValue("ToolTip",_tooltip);
			info.AddValue("bDoubleClickAction",_bdoubleclickaction);
			info.AddValue("bShowCaption",_bshowcaption);
            info.AddValue("bNeedContext", _bneedcontext);
            info.AddValue("EnCaption", _encaption);
            info.AddValue("TwCaption", _twcaption);
            info.AddValue("EnTip", _entip);
            info.AddValue("TwTip", _twtip);
		}

		#endregion

		#region ICloneable 成员

		public object Clone()
		{
			return new SelfAction(this);
		}

		#endregion

    }

	public delegate string NameCheckHandler(string name);

    [Serializable]
    public class Information:DisplyTextCustomTypeDescriptor,ISerializable,ICollectionEditType,ICloneable
    {
        private string _name="";
        private string _informationhandler="";

        public Information()
        {
        }

        public Information(Information infor)
        {
            _name = infor.Name;
            _informationhandler = infor.InformationHandler;
        }

        protected Information(SerializationInfo info, StreamingContext context)
        {
            _name = info.GetString("Name");
            _informationhandler = info.GetString("InformationHandler");
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis3")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis3")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [DisplayText("U8.UAP.Report.InformationHandler")]
        [LocalizeDescription("U8.UAP.Report.InformationHandler")]
        public string InformationHandler
        {
            get
            {
                return _informationhandler;
            }
            set
            {
                _informationhandler = value;
            }
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", 890);
            info.AddValue("Name", _name);
            info.AddValue("InformationHandler", _informationhandler);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new Information(this);
        }

        #endregion
    }

    public interface ICollectionEditType
    {
        string Name { get;set;}
    }
}
