using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Section 的摘要说明。
	/// </summary>
	[Serializable]
	public abstract class Section:Rect,ISection,ICloneable,ISerializable,IDisposable
	{
		#region fields
		protected int _orderid;	
		protected int _level;
		protected SectionType _sectiontype;		
		protected SectionLines _sectionlines;
		protected Cells _cells;		
		protected bool _bexpand=true;
        protected int _autoleft = -1;
        
		#endregion

		#region event
		public event EventHandler BottomChanged;
		public event EventHandler HeightChanged;
		#endregion

		#region constructor
		public Section():base()
		{	
			_cells=new Cells();
			_sectionlines=new SectionLines();
			SetOrderID();
			SetSectionType();
		}

		public Section(int height):this()
		{
			_h=height;
		}

		public Section(Section section):base(section)
		{			
			_orderid=section.OrderID;
			_level=section.Level;
			_sectiontype=section.SectionType;
            _cells = section.Cells.Clone() as Cells;
			//_cells=new Cells();
			_sectionlines=new SectionLines();
			_x=0;
		}

		protected Section( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_orderid=info.GetInt32("OrderID");
			_sectiontype=(SectionType)info.GetValue("SectionType",typeof(SectionType));
			_cells=(Cells)info.GetValue("Cells",typeof(Cells));
			_x=0;
			_level=info.GetInt32("Level");
            _sectionlines = new SectionLines();
            if (!ClientReportContext.bInServerProcess)
                InterpretSectionCaption();
		}
		#endregion
       
		#region override
        public override void SetDefault()
        {
            _borderside.NoneBorder();
            _x = 0;
            _h = 140;
        }

        protected abstract string CaptionID { get;}

        protected override void InerpretCaption()
        {
        }

        protected virtual void InterpretSectionCaption()
        {
            _caption = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString(CaptionID, ClientReportContext.LocaleID);
        }

        [Browsable(false)]
        public override bool bSupportLocate
        {
            get
            {
                return base.bSupportLocate;
            }
            set
            {
                base.bSupportLocate = value;
            }
        }

        [Browsable(false)]
        public override bool bControlAuth
        {
            get
            {
                return base.bControlAuth;
            }
            set
            {
                base.bControlAuth = value;
            }
        }

        [Browsable(false)]
        public int AutoLeft
        {
            get
            {
                return _autoleft;
            }
            set
            {
                _autoleft = value;
            }
        }

        [Browsable(false)]
        public override string PrepaintEvent
        {
            get
            {
                return _prepaintevent ;
            }
            set
            {
            }
        }

        
		[DisplayText("U8.UAP.Services.ReportElements.Dis44")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis44")]
		public int DesignHeight
		{
			get
			{
				return _h;
			}
			set
			{
				if(value < this.HeaderHeight+this.InnerHeight +YOFFSIZE*2 )
					return ;
				_h=value;
				if(HeightChanged!=null)
					HeightChanged(this,EventArgs.Empty);
			}
		}

        [Browsable(false)]
        public override BorderSide Border
        {
            get
            {
                return base.Border;
            }
            set
            {
                base.Border = value;
            }
        }

		[Browsable(false)]
		public override Color BorderColor
		{
			get
			{
				return base.BorderColor;
			}
			set
			{
                _bordercolor = value;
			}
		}

        [Browsable(false)]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                _backcolor = value;
            }
        }

		[Browsable(false)]
		public override int BorderWidth
		{
			get
			{
				return base.BorderWidth;
			}
			set
			{
				base.BorderWidth = value;
			}
		}

		[Browsable(false)]
		public override Size Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				base.Size = value;
			}
		}

		[Browsable(false)]
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		[Browsable(false)]
		public override string Caption
		{
			get
			{
                if (!ClientReportContext.bInServerProcess)
                    InterpretSectionCaption();
				return _caption ;
			}
			set
			{
				//base.Caption = value;
			}
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
                _forecolor = value;
			}
		}


		[Browsable(false)]
		public override bool KeepPos
		{
			get
			{
				return base.KeepPos;
			}
			set
			{
				base.KeepPos = value;
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

        [Browsable(false)]
        public override string IdentityCaption
        {
            get
            {
                return base.IdentityCaption;
            }
            set
            {
                base.IdentityCaption = value;
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
		public override Point Location
		{
			get
			{
				return new Point(_x,_y);
			}
			set
			{
				
			}
		}

		[Browsable(false)]
		public override int RelativeY
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override void SetLocation(Point pt)
		{
			
		}

		[Browsable(false)]
		public override int Y
		{
			get
			{
				return _y;
			}
			set
			{
				_y=value;
			}
		}

        public override bool contains(Point pt)
        {
            int h = _h;
            if (!_bexpand)
                h = DefaultConfigs.SECTIONHEADERHEIGHT ;
            return new Rectangle(_x, _y, Int32.MaxValue, h).Contains(pt);
        }


		public override Rectangle getRects()
		{
			if(_bexpand)
				return base.getRects ();
			else
                return new Rectangle(_x, _y, _w, DefaultConfigs.SECTIONHEADERHEIGHT);
		}

		[Browsable(false)]
		public override int Height
		{
			get
			{
				if(_bexpand)
					return _h;
				else
                    return DefaultConfigs.SECTIONHEADERHEIGHT;
			}
			set
			{
				_h = value;
			}
		}

		protected override void draw(Graphics g) 
		{
			DrawGrid(g);
			DrawNormalHeader(g);
		}
		
		public override void ResizeHeight(int dy)
		{
            this.Cells.CalcHeight();
            int minheight = this.Cells.Bottom -_y + YOFFSIZE * 2;
            if (dy - _y < minheight)
                dy = _y + minheight;
//            if(_h>InnerHeight+HeaderHeight+YOFFSIZE*2 && dy-_y<InnerHeight+HeaderHeight+YOFFSIZE*2)
//                return;
////				_h=SECTIONHEADERHEIGHT;
//            else if(_h<=InnerHeight+HeaderHeight+YOFFSIZE*2 && dy-_y<_h)
//                return;
//            else
//            {
				_h = dy - _y;
				if(BottomChanged!=null)
					BottomChanged(this,EventArgs.Empty);
            //}
		}

		#endregion

		#region public
        public abstract States GetDefaultState(DataType type);
        public abstract Cell GetDefaultRect(DataSource ds);

		[Browsable(false)]
		public int HeaderHeight
		{
			get
			{
                return DefaultConfigs.SECTIONHEADERHEIGHT;
			}
        }
        [Browsable(false)]
        public override ReportStates UnderState
        {
            get
            {
                return base.UnderState;
            }
            set
            {
                base.UnderState = value;
                _cells.UnderState = value;
            }
        }

        [Browsable(false)]
        public override bool Visible
        {
            get
            {
                return _visible ;
            }
            set
            {
                
            }
        }

        [Browsable(false)]
        public override bool bHidden
        {
            get
            {
                return _bhidden ;
            }
            set
            {

            }
        }

        [Browsable(false)]
        public override bool bSolid
        {
            get
            {
                return _bsolid ;
            }
            set
            {

            }
        }

		[Browsable(false)]
		public bool bExpand
		{
			get
			{
				return _bexpand;
			}
			set
			{
				_bexpand=value;
			}
		}

		public void DrawActiveHeader(Graphics g,Point pt)
		{
			int ox=_x;
			int oy=_y;
			_x=pt.X;
			_y=pt.Y;
			
			DrawHeader(g,SystemColors.ActiveCaption);
			DrawHeaderText(g,true);
			_x=ox;
			_y=oy;
		}

		private void DrawHeader(Graphics g,Color color)
		{
            DrawBackGround(g, new Rectangle(_x, _y, _w, DefaultConfigs.SECTIONHEADERHEIGHT), color);
			using (Brush b=new SolidBrush(color))
			{
                //g.FillRectangle(b, _x, _y, _w, DefaultConfigs.SECTIONHEADERHEIGHT);

				Color backlight = ControlPaint.Light(SystemColors.Control );
				Color backlightlight = ControlPaint.LightLight(SystemColors.Control );
				Color backdark = ControlPaint.Dark(SystemColors.Control );
				Color backdarkdark = ControlPaint.DarkDark(SystemColors.Control );

				using (Brush brushlight = new SolidBrush(backlight))
				using (Brush brushdark = new SolidBrush(backdark))
				using (Pen penlight = new Pen(brushlight, 1))
				using (Pen pendark = new Pen(brushdark, 1))
				using (Brush brushlightlight = new SolidBrush(backlightlight))
				using (Brush brushdarkdark = new SolidBrush(backdarkdark))
				using (Pen penlightlight = new Pen(brushlightlight, 1))
				using (Pen pendarkdark = new Pen(brushdarkdark, 1))
				{
					g.DrawLine(pendark,_x , _y , _x+_w-2, _y );
                    g.DrawLine(pendark, _x, _y, _x, _y + DefaultConfigs.SECTIONHEADERHEIGHT - 2);
					g.DrawLine(pendarkdark, _x +1, _y+1, _x +_w-3,_y+ 1);
                    g.DrawLine(pendarkdark, _x + 1, _y + 1, _x + 1, _y + DefaultConfigs.SECTIONHEADERHEIGHT - 3);

                    g.DrawLine(penlightlight, _x, _y + DefaultConfigs.SECTIONHEADERHEIGHT - 1, _x + _w - 1, _y + DefaultConfigs.SECTIONHEADERHEIGHT - 1);
                    g.DrawLine(penlightlight, _x + _w - 1, _y + DefaultConfigs.SECTIONHEADERHEIGHT - 1, _x + _w - 1, _y);
                    g.DrawLine(penlight, _x + 1, _y + DefaultConfigs.SECTIONHEADERHEIGHT - 2, _x + _w - 2, _y + DefaultConfigs.SECTIONHEADERHEIGHT - 2);
                    g.DrawLine(penlight, _x + _w - 2, _y + DefaultConfigs.SECTIONHEADERHEIGHT - 2, _x + _w - 2, _y + 1);
				}
			}
		}

		private void DrawNormalHeader(Graphics g)
		{
			DrawHeader(g,SystemColors.InactiveCaption );
			DrawHeaderText(g,false);
		}

		private const int PLUSLEFT=4;
		private void DrawHeaderText(Graphics g,bool bactivate)
		{
			Color headertextcolor;
            if (bactivate)
                headertextcolor = SystemColors.ActiveCaptionText;
            else
                headertextcolor = Color.Olive ;//SystemColors.InactiveCaptionText;
			g.FillRectangle(Brushes.White,_x+PLUSLEFT,_y+6,12,12);
			g.DrawRectangle(Pens.Black,_x+PLUSLEFT,_y+6,12,12);
			g.DrawLine(Pens.Black,_x+PLUSLEFT+2,_y+12,_x+PLUSLEFT+10,_y+12);
			if(!_bexpand)
				g.DrawLine(Pens.Black,_x+PLUSLEFT+6,_y+8,_x+PLUSLEFT+6,_y+16);
			using (Brush b=new SolidBrush(headertextcolor))
			{
                g.DrawString(this.Caption, GetClientFont(new ServerFont()), b, new Rectangle(_x + PLUSLEFT + 14, _y + 6, _w - PLUSLEFT - 14, DefaultConfigs.SECTIONHEADERHEIGHT - 6));
			}
		}

		private bool _bshowgrid=true;
		private int _gridsize=8;
		[Browsable(false)]
		public int GridSize
		{
			set
			{
				_gridsize=value;
			}
		}
		[Browsable(false)]
		public bool bShowGrid
		{
			set
			{
				_bshowgrid=value;
			}
		}

		private void DrawGrid(Graphics g)
		{
			using (Brush brush=new SolidBrush(Color.White ))
			{
				g.FillRectangle(brush , getRects());
			}
			
			if(_bshowgrid)
			{
				using (Pen gpen=new Pen(Color.Black))
				{
					gpen.DashStyle=System.Drawing.Drawing2D.DashStyle.Custom ;
					gpen.DashPattern=new float[]{1,_gridsize -1};
                    for (int i = DefaultConfigs.SECTIONHEADERHEIGHT + _gridsize; i < Height; i = i + _gridsize)
					{
						int y=(_y+i)/_gridsize * _gridsize;
						g.DrawLine(gpen,_x,y,_x+_w ,y);
					}
				}
			}
		}

		public bool HeaderContains(Point pt)
		{
            return (new Rectangle(_x, _y, _w, DefaultConfigs.SECTIONHEADERHEIGHT)).Contains(pt);
		}

		public bool PlusContains(Point pt)
		{
			return (new Rectangle(_x+PLUSLEFT,_y+6,12,12)).Contains(pt);
		}

		public virtual void AddALabel(Label label)
		{	
			//label.Parent=this;
            label.RelativeY = label.Y - _y;
			_cells.AddALabel(label);
            //_cells.CalcHeight();
		}
		
		private const int YOFFSIZE=16;
        public void AdjustHeight()
        {
            //_h = InnerHeight + SECTIONHEADERHEIGHT + YOFFSIZE * 2;
            //InitRelativeY();
        }

        //private void InitRelativeY()
        //{
        //    foreach(Cell cell in _cells)
        //    {
        //        cell.RelativeY=SECTIONHEADERHEIGHT+YOFFSIZE + cell.Y-InnerY; 
        //    }

        //}

        public void GridDetailAutoLayOutAtRuntime()
        {
            if (_cells.Count == 0)
                return;

            int top = DefaultConfigs.SECTIONHEADERHEIGHT + 10;
            int left = DefaultConfigs.ReportLeft;
            for(int index = 0;index<_cells.Count;index++)
            {
                Cell cell = _cells[index];
                cell.SetY(top);
                if (cell is SuperLabel)
                    continue;
                cell.X=left;
                left += cell.Width;
            }
            foreach (Cell cell in _cells)
            {
                if (cell.Super != null && cell.Super.X > cell.X)
                {
                    cell.Super.X = cell.X ;//-1
                }
            }
        }

        public void PageTitleAutoLayOutAtRuntime()
        {
            if (_cells.Count == 0)
                return;
            int index = 0;

            Cell cell = _cells[index];
            int top = cell.Y;
            if (cell  is SuperLabel)
                (cell as SuperLabel).AutoResize ();
            for (int i = index + 1; i < _cells.Count; i++)
            {
                cell = _cells[i];
                cell.X = _cells[index].X + _cells[index].Width;
                cell.SetY(top);
                if (cell is SuperLabel)
                    (cell as SuperLabel).AutoResize ();
                index = i;
            }

        }

        public void AutoLayOutAtRuntimeAll()
        {
                if (_cells.Count == 0)
                    return;
                int index = 0;
                Cell cell = _cells[index];
                int top = cell.RelativeY;
                cell.X = DefaultConfigs.ReportLeft;
                if (cell is SuperLabel)
                    (cell as SuperLabel).AutoLayOut();
                for (int i = index + 1; i < _cells.Count; i++)
                {
                    cell = _cells[i];
                    cell.X = _cells[index].X + _cells[index].Width;
                    cell.SetY(top);
                    if (cell is SuperLabel)
                        (cell as SuperLabel).AutoLayOut();
                    index = i;
                }
        }

        public void AutoLayOut()
        {
            AutoLayOutAtDesigntime();
        }

        public void AutoLayOutAtDesigntime()
        {
            foreach (Cell cell in _cells)
            {
                cell.Y = _y + cell.RelativeY;
            }
        }

		public void AsignToSectionLines()
		{
            _sectionlines.Clear();
			if(_cells.Count==0)
			{
				SectionLine sl=new SectionLine(this);
				_sectionlines.Add(sl);
				return;
			}
            ArrayList al = new ArrayList();
			foreach(Cell value in _cells)
			{
				bool badded=false;
                al.Clear();
                SectionLine slto = null;
				foreach(SectionLine sl in _sectionlines)
				{
					if(sl.ShouldContains(value))
					{
                        if (!badded)
                        {
                            sl.Cells.Add(value);
                            slto = sl;
                            badded = true;
                        }
                        else
                        {
                            RemoveFromTo(sl,slto);
                            al.Add(sl);
                        }
					}
				}
				if(!badded)
				{
					SectionLine sl=new SectionLine(this);
					sl.Cells.Add(value);
					_sectionlines.Add(sl);
				}
                else if (al.Count > 0)
                {
                    for (int i = 0; i < al.Count; i++)
                        _sectionlines.Remove((SectionLine)al[i]);
                }
			}
		}

        private void RemoveFromTo(SectionLine sl, SectionLine slto)
        {
            foreach (Cell cell in sl.Cells)
                slto.Cells.Add(cell);
        }

        [Browsable(false)]
		public Cells Cells
		{
			get
			{
                if (_understate == ReportStates.Designtime)
                {
                    _cells.BeforeAdd -= new EventHandler(_cells_BeforeAdd);
                    _cells.BeforeAdd += new EventHandler(_cells_BeforeAdd);
                }
				return _cells;
			}
			set
			{
				_cells=value;
			}
		}

		[Browsable(false)]
		public int Level
		{
			get
			{
				return _level;
			}
			set
			{
				_level=value;
			}
		}

		[Browsable(false)]
		public SectionLines SectionLines
		{
			get
			{
				return _sectionlines;
			}
		}

		[Browsable(false)]
		public override int X
		{
			get
			{
				return _cells.X;
			}
			set
			{
				_x=value;
			}
		}

		[Browsable(false)]
		public int InnerY
		{
			get
			{
				return _cells.Y;
			}
			set
			{
			}
		}

		[Browsable(false)]
		public int InnerHeight
		{
			get
			{
				return _cells.Height;
			}
		}
		
		[Browsable(false)]
		public override int Width
		{
			get
			{
				return _cells.Right ;
			}
			set
			{
				_w=value;
			}
		}

        protected int CalcVisibleWidth(Cells cells)
        {
            int left = (_autoleft!=-1?_autoleft :0);
            foreach (Cell cell in cells)
            {
                if (left == 0)
                {
                    left = cell.X;
                }
                if (cell.Visible)
                {
                    cell.X = left;
                    left += cell.VisibleWidth;
                }
            }
            return left;
        }


        [Browsable(false)]
        public override  int VisibleWidth
        {
            get
            {
                return _cells.VisibleRight;
            }
        }

        public void Update(Cell cell)
        {
            Cell ctmp = _cells[cell.Name];
            if (ctmp != null)
            {
                ctmp.BackColor = cell.BackColor;
                ctmp.Caption = cell.Caption.ToString();
                ctmp.ForeColor = cell.ForeColor;
            }
        }

		#endregion

		#region abstract
		protected abstract void SetOrderID();
		protected abstract void SetSectionType();
		public abstract bool CanBeParent(string type);
		#endregion

		#region ISection 成员

		[Browsable(false)]
		public int OrderID
		{
			get
			{
				return _orderid;
			}
		}

		[Browsable(false)]
		public SectionType SectionType
		{
			get
			{
				return _sectiontype;
			}
		}
		#endregion

		#region ICloneable 成员
        //public object DeepClone()
        //{
        //    Section section=this.Clone() as Section;
        //    //foreach(Cell cell in _cells)
        //    //{
        //    //    Cell ctmp=(Cell)cell.Clone() ;
        //    //    ctmp.Parent=null;
        //    //    section.Cells.Add(ctmp);
        //    //}
        //    return section;
        //}
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("OrderID",_orderid);
			info.AddValue("SectionType", _sectiontype);
			info.AddValue("Cells",_cells);
			info.AddValue("Level",_level);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
			if(_sectionlines!=null)
			{
				_sectionlines.Dispose();
				_sectionlines=null;
			}
			if(_cells!=null)
			{
				_cells.Dispose();
				_cells=null;
			}
            base.Dispose();
		}

		#endregion

        public void ApplyColorStyle()
        {
            if (this.SectionType == SectionType.ReportHeader)
            {
                foreach (Cell cell in _cells)
                {
                    if (cell.Type=="Label")
                        CopySectionStyleToCell(cell);
                }
            }
            else
            {
                foreach (Cell cell in _cells)
                {
                    if (cell is IApplyColorStyle)
                    {
                        if((cell as IApplyColorStyle).bApplyColorStyle )
                            CopySectionStyleToCell(cell);
                    }
                    else if((cell.Type=="Label" || cell.Type=="SuperLabel") && this is IAlternativeStyle ) //(cell is Label && this is IAlternativeStyle)
                        CopySectionStyleToLabel(cell);
                    else
                        CopySectionStyleToCell(cell);
                }
            }
        }

        protected  virtual  void CopySectionStyleToLabel(Cell cell)
        {
        }

        private void CopySectionStyleToCell(Cell cell)
        {
            cell.SetBackColor(_backcolor);
            //cell.SetBorderColor(_bordercolor);
            cell.SetForeColor(_forecolor);
            cell.ServerFont=_serverfont;
            if (cell is SuperLabel)
            {
                foreach (Label l in (cell as SuperLabel).Labels)
                    CopySectionStyleToCell(l);
            }
        }

		private void _cells_BeforeAdd(object sender, EventArgs e)
		{
			Cell cell =sender as Cell;
			AddArgs aa=e as AddArgs;
			if(cell.Parent!=null)
			{
				if(cell.Parent!=this )
				{
					cell.Parent.Cells.Remove(cell);
					cell.Parent=this;
					cell.RelativeY=cell.Y-_y;
				}
				else if( cell.Parent.Cells.Contains(cell))
					aa.bAlreadyAdd=true;
			}
			else
			{
				cell.Parent=this;
				cell.RelativeY=cell.Y-_y;
			}
            //cell.GetReport();
            //if (cell.Report != null && !cell.Report.bFree)
            //{
                if (cell is IApplyColorStyle)
                {
                    if ((cell as IApplyColorStyle).bApplyColorStyle)
                        CopySectionStyleToCell(cell);
                }
                else if ((cell.Type == "Label" || cell.Type == "SuperLabel") && this is IAlternativeStyle)
                    CopySectionStyleToLabel(cell);
                else if (_sectiontype != SectionType.ReportHeader || cell.Type == "Label")
                    CopySectionStyleToCell(cell);

            //}
		}
	}
}
