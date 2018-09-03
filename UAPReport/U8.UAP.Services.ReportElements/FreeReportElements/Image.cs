using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Image 的摘要说明。
	/// </summary>
	[Serializable]
	public class Image:Rect,IImage,ISerializable,ICloneable,IDisposable
	{
		#region fields
		protected System.Drawing.Image _image;
		protected string _imagestring="";
		System.Windows.Forms.PictureBoxSizeMode _sizemode=PictureBoxSizeMode.StretchImage;
		#endregion
         
		#region constructor
		public Image():base()
		{
		}

		public Image(int x,int y):base(x,y)
		{
		}

		public Image(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public Image(Image image):base(image)
		{
			_sizemode=image.SizeMode;
			ImageString=image.ImageString;
		}

		protected Image( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_sizemode=(PictureBoxSizeMode)info.GetValue("SizeMode",typeof(PictureBoxSizeMode));
			ImageString=info.GetString("ImageString");
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="Image";			
		}
        
		[Browsable(false)]
		public override Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[Browsable(false)]
        public override Font ClientFont
		{
			get
			{
                return base.ClientFont;
			}
			set
			{
                base.ClientFont = value;
			}
		}

        [Browsable(false)]
        public override ContentAlignment CaptionAlign
        {
            get
            {
                return base.CaptionAlign;
            }
            set
            {
                base.CaptionAlign = value;
            }
        }


		public override void SetDefault()
		{
			base.SetDefault ();
			_w=80;
			_h=80;
			_borderside.NoneBorder();
			_keeppos=true;
		}

		public override void SetRuntimeHeight(Graphics g,string s)
		{
			_runtimeheight=_h;
		}


		protected override void draw(Graphics g)
		{
            if (_image == null || _imagestring == null || _imagestring.Length == 0)
                base.draw(g);
            else
            {
                int ow = _w;
                int oh = _h;
                DrawImage(g);
                DrawBorder(g);
                DrawFocus(g);
                _w = ow;
                _h = oh;
            }
		}

        private void DrawImage(Graphics g)
        {
            switch (_sizemode)
            {
                case PictureBoxSizeMode.AutoSize:
                    #region autosize
                    _w = _image.Width;
                    _h = _image.Height;
                    g.DrawImage(_image, new Rectangle(_x, _y, _w, _h), new Rectangle(0, 0, _w, _h), GraphicsUnit.Pixel);
                    #endregion
                    break;
                case PictureBoxSizeMode.CenterImage:
                    #region centerimage
                    DrawBackGround(g);
                    if (_image.Width >= _w && _image.Height >= _h)
                    {
                        g.DrawImage(_image, new Rectangle(_x, _y, _w, _h), new Rectangle((_image.Width - _w) / 2,
                                                                        (_image.Height - _h) / 2,
                                                                        Math.Min(_w, _image.Width),
                                                                        Math.Min(_h, _image.Height)), GraphicsUnit.Pixel);
                    }
                    else if (_image.Width <= _w && _image.Height <= _h)
                    {
                        g.DrawImage(_image, new Rectangle(_x+(_w - _image.Width) / 2,
                                                                        _y+(_h - _image.Height) / 2,
                                                                        Math.Min(_w, _image.Width),
                                                                        Math.Min(_h, _image.Height)),
                                                                        new Rectangle(0, 0, _image.Width, _image.Height), GraphicsUnit.Pixel);
                    }
                    else if (_image.Width >= _w)
                    {
                        g.DrawImage(_image, new Rectangle(_x,_y+(_h - _image.Height) / 2,
                                                                        Math.Min(_w, _image.Width),
                                                                        Math.Min(_h, _image.Height)),
                                                                        new Rectangle((_image.Width - _w) / 2, 0, Math.Min(_w, _image.Width),
                                                                        Math.Min(_h, _image.Height)), GraphicsUnit.Pixel);
                    }
                    else//image.Height>=_h
                    {
                        g.DrawImage(_image, new Rectangle(_x+(_w - _image.Width) / 2,_y,
                                                                        Math.Min(_w, _image.Width),
                                                                        Math.Min(_h, _image.Height)),
                                                                        new Rectangle(0, (_image.Height - _h) / 2, Math.Min(_w, _image.Width),
                                                                        Math.Min(_h, _image.Height)), GraphicsUnit.Pixel);
                    }
                    #endregion
                    break;
                case PictureBoxSizeMode.Normal:
                    g.DrawImage(_image, new Rectangle(_x, _y, _w, _h), new Rectangle(0, 0, _w, _h), GraphicsUnit.Pixel);
                    break;
                case PictureBoxSizeMode.StretchImage:
                    g.DrawImage(_image, new Rectangle(_x, _y, _w, _h), new Rectangle(0, 0, _image.Width, _image.Height), GraphicsUnit.Pixel);
                    break;
                default://PictureBoxSizeMode.Zoom:
                    #region zoom
                    DrawBackGround(g);
                    double rate = Math.Max(Convert.ToDouble(_image.Width) / Convert.ToDouble(_w), Convert.ToDouble(_image.Height) / Convert.ToDouble(_h));
                    int witdh = Convert.ToInt32(Convert.ToDouble(_image.Width) / rate);
                    int height = Convert.ToInt32(Convert.ToDouble(_image.Height) / rate);
                    g.DrawImage(_image, new Rectangle(_x+(_w - witdh) / 2,
                                                                    _y+(_h - height) / 2,
                                                                    witdh,
                                                                    height),
                                                                    new Rectangle(0, 0, _image.Width, _image.Height), GraphicsUnit.Pixel);
                    #endregion
                    break;
            }
        }

		#endregion

		#region IImage 成员
        [Browsable(false)]
        public System.Drawing.Image MyImage
        {
            get
            {
                return _image;
            }
        }

		[DisplayText("U8.UAP.Services.ReportElements.Dis17")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis17")]
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
				if(_imagestring!=value)
				{
					OnBeforeSizeModeChanged();
					_imagestring=value;
					if(value!=null && value.Length>0)
					{
                        try
                        {
                            byte[] imageByte = Convert.FromBase64String(value);
                            _image = System.Drawing.Image.FromStream(new MemoryStream(imageByte));
                        }
                        catch
                        {
                            _imagestring = null;
                        }
						OnSizeChange();
					}
				}
			}
		}        

		[DisplayText("U8.UAP.Services.ReportElements.Dis26")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis26")]
		public System.Windows.Forms.PictureBoxSizeMode SizeMode
		{
			get
			{
				return _sizemode;
			}
			set
			{
				if(_sizemode!=value)
				{
					OnBeforeSizeModeChanged();
					_sizemode=value;
					OnSizeChange();
				}
			}
		}

		public void SetBack(string imagestring,int w,int h,PictureBoxSizeMode sizemode)
		{
			_imagestring=imagestring;
			_w=w;
			_h=h;
			_sizemode=sizemode;
		}

		protected void OnSizeChange()
		{
			if(_image!=null && _sizemode==PictureBoxSizeMode.AutoSize)
			{				
				_w=_image.Width;
				_h=_image.Height;				
			}
			OnAfterSizeModeChanged();
		}

		protected void OnBeforeSizeModeChanged()
		{
			if(BeforeSizeModeChanged!=null)
				BeforeSizeModeChanged(this,EventArgs.Empty);
		}

		protected void OnAfterSizeModeChanged()
		{
			if(AfterSizeModeChanged!=null)
				AfterSizeModeChanged(this,EventArgs.Empty);
		}
		public event EventHandler BeforeSizeModeChanged;
		public event EventHandler AfterSizeModeChanged;
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("ImageString",_imagestring);
			info.AddValue("SizeMode",_sizemode);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new Image(this);
		}

		#endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            _image = null;
            _imagestring = null;
            base.Dispose();
        }

        #endregion

        public static System.Drawing.Image GetImage(string imagestring)
        {
            System.Drawing.Image image = null;
            try
            {
                byte[] imageByte = Convert.FromBase64String(imagestring);
                image = System.Drawing.Image.FromStream(new MemoryStream(imageByte));
            }
            catch
            {
                
            }
            return image;
        }
    }

    [Serializable]
    public class BarCode : Rect, ISerializable, ICloneable, IDataSource, IMapName, IDisposable, IBarCode
    {
        #region fields
        protected DataSource _datasource = new DataSource("EmptyColumn");
        protected Neodynamic.WinControls.BarcodeProfessional.Symbology _symbology = Neodynamic.WinControls.BarcodeProfessional.Symbology.Code39;
        protected Neodynamic.WinControls.BarcodeProfessional.BarcodeProfessional _barcode;
        #endregion

        #region constructor
        public BarCode()
            : base()
        {
        }
        public BarCode(int x, int y)
            : base(x, y)
        {
        }

        public BarCode(BarCode dbtext)
            : base(dbtext)
        {
            _datasource = dbtext.DataSource;
            _symbology = dbtext._symbology;
            _barcode = dbtext._barcode;
        }

        protected BarCode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _datasource = (DataSource)info.GetValue("DataSource", typeof(DataSource));
            _symbology = (Neodynamic.WinControls.BarcodeProfessional.Symbology)info.GetValue("SymboLogy", typeof(Neodynamic.WinControls.BarcodeProfessional.Symbology));
            _barcode = (Neodynamic.WinControls.BarcodeProfessional.BarcodeProfessional)info.GetValue("BarCODE", typeof(Neodynamic.WinControls.BarcodeProfessional.BarcodeProfessional)); 

        }
        #endregion

        #region override
        public override void SetType()
        {
            _type = "BarCode";
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _w = 184;
            _h = 64;
        }

        [Browsable(false)]
        public override string Caption
        {
            get
            {
                return base.Caption;
            }
            set
            {
                base.Caption = value;
            }
        }

        protected override void draw(Graphics g)
        {
            try
            {
                if (_barcode == null)
                    _barcode = new Neodynamic.WinControls.BarcodeProfessional.BarcodeProfessional();

                _barcode.AddChecksum = false;
                if (_symbology == Neodynamic.WinControls.BarcodeProfessional.Symbology.Code128)
                {
                    _barcode.Code128CharSet = Neodynamic.WinControls.BarcodeProfessional.Code128.B;
                   
                    _barcode.AddChecksum = true;
                }
                //else if (_symbology == Neodynamic.WinControls.BarcodeProfessional.Symbology.Code16k)
                //    _barcode.Code16kMode = Neodynamic.WinControls.BarcodeProfessional.Code16k.Mode0;
                _barcode.Symbology = _symbology;
                //_barcode.AntiAlias = true;
                _barcode.Code = _caption == "" ? "123456789" : _caption;//_caption;
                System.Drawing.Image image = _barcode.Image;
                //_barcode.im.DrawOnCanvas(g, new PointF(_x, _y));
                _w = image.Width;
                _h = image.Height;
                g.DrawImage(image, new Rectangle(_x, _y, _w, _h), new Rectangle(0, 0, _w, _h), GraphicsUnit.Pixel);
            }
            catch
            {
                base.draw(g);
            }
        }
        #endregion

        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("DataSource", _datasource);
            info.AddValue("SymboLogy", _symbology);
            info.AddValue("BarCODE", _barcode);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new BarCode(this);
        }

        #endregion

        #region IDataSource 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis16")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
        public DataSource DataSource
        {
            get
            {
                return _datasource;
            }
            set
            {
                _datasource = value;
            }
        }
        #endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            _datasource = null;
            if (_barcode != null)
                _barcode.Dispose();
            _barcode = null;
            base.Dispose();
        }

        #endregion

        #region IMapName 成员

        [Browsable(false)]
        public string MapName
        {
            get
            {
                return _datasource.Name;
            }
        }

        #endregion

        #region IBarCode 成员

        [Browsable(true)]
        
        [DisplayText("U8.UAP.Report.BarCodeType")]
        [LocalizeDescription("U8.UAP.Report.BarCodeType")]
        public Neodynamic.WinControls.BarcodeProfessional.Symbology Symbology
        {
            get
            {
                return _symbology;
            }
            set
            {
                _symbology = value;
                OnOtherChanged(null);
            }
        }

        #endregion
    }

}
