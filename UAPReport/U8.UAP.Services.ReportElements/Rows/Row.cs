using System;
using System.Drawing;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using UFIDA.U8.UAP.Services.ReportFilterService;
using System.Threading;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Row 的摘要说明。
	/// </summary>
	public class Row:IDisposable,IRow
    {
        #region fields & event
        protected Cells _cells;
		protected int _y;
		protected bool _bautoheight;
		protected int _level;
		protected string _area;
		protected int _metaheight;
        protected int _expandheight;
        protected int _height = -1;
		protected int _pageindex;
		protected int _ybase;
        protected int _xbase;

		protected SectionLine _sectionline;
        
        protected int _pagesinpage=1;
        protected int _pages = 1;
        protected int _pageoffsize = 1;

		protected bool _blastinpage;
		protected bool _bfirst;
		protected int _x;
		protected int _right;
        protected GroupHeaderRowStates _rowstate = GroupHeaderRowStates.Normal ;
        private GroupHeaderRow _upperrow;
        private int _baseid=-1;
        protected Graphics _g;

        protected int _containerwidth = -1;

        protected Report _report;
        protected PrintPage _printpage;
        protected bool _bminorlevel;
        protected bool _baloneline = false;
        protected bool _bdrawlastline = false;

		public event ActiveCellHandler bActiveCell;
        public event ActiveRowHandler bActiveRow;
        public event HasActiveCellHandler hasActiveCell;
        #endregion

        #region constructor
        protected Row(Graphics g)
		{
			_cells=new Cells();
            _cells.UnderState = ReportStates.Browse;
            _g = g;
        }

        public Row(SectionLine sl,Graphics g):this(g)
        {
            _sectionline = sl;
            _level = sl.Section.Level;
            _area = sl.Section.Type;
            _bfirst = sl.Index == 0;
            AddCells(null);
        }

        public Row(SectionLine sl, Graphics g,int containerwidth):this(g)
        {
            _containerwidth = containerwidth;
            _sectionline = sl;
            _level = sl.Section.Level;
            _area = sl.Section.Type;
            _bfirst = sl.Index == 0;
            AddCells(null);
        }

        public Row(SectionLine sl, SemiRow semirow,string baseid,Graphics g):this(g)
        {
            if(baseid!=null && semirow.Contains(baseid))
                _baseid = Convert.ToInt32(semirow[baseid]);
            _bminorlevel = semirow.bMinorLevel;
            _sectionline = sl;
            _level = semirow.Level;
            _area = sl.Section.Type ;
            _bfirst = sl.Index == 0;
            AddCells(semirow);
        }

        public Row(SectionLine sl)
        {
            //_bautoheight = true;
            _cells = sl.Cells;
            _cells.UnderState = ReportStates.Browse;
            _level = sl.Section.Level;
            _area = sl.Section.Type;
            _sectionline = sl;
            _x = _cells.X;
            _ybase = _cells.Y;
            _expandheight = _cells.ExpandHeight;
            _metaheight = _cells.MetaHeight;
        }
        #endregion
        public static Graphics CreateGraphics()
        {
            Bitmap bitmap = new Bitmap(1, 1);
            return Graphics.FromImage(bitmap);
        }
        private void AddCells(SemiRow semirow)
        {   
            ArrayList superlabels = new ArrayList();
            Graphics g = null;
            try
            {
                if (_g == null)
                    g = CreateGraphics();
                foreach (Cell ctmp in _sectionline.Cells)
                {
                    Cell cell = ctmp.Clone() as Cell;

                    #region handler alternative style
                    if (_sectionline.Section is IAlternativeStyle)
                    {
                        IAlternativeStyle ias = _sectionline.Section as IAlternativeStyle;
                        if (ias.bApplyAlternative && ias.bApplySecondStyle)
                        {
                            //if (cell is CommonLabel && (cell as CommonLabel).LabelType != LabelType.SummaryLabel)
                            //    continue;
                            //if (cell is Label)
                            //    continue;
                            if (//!(cell is CommonLabel && (cell as CommonLabel).LabelType != LabelType.SummaryLabel) &&
                                cell.Type != "Label" && cell.Type != "SuperLabel")
                            {
                                if(!(cell is IApplyColorStyle) || ( cell as IApplyColorStyle).bApplyColorStyle)
                                    cell.SetAlternativeStyle(ias);
                            }
                        }
                    }
                    #endregion

                    if (semirow != null && semirow.Contains(cell.Name))
                    {
                        if (cell is IImage)
                            (cell as IImage).ImageString = semirow[cell.Name].ToString();
                        else
                        {
                            cell.Caption = semirow[cell.Name].ToString();

                            try
                            {
                                if (!string.IsNullOrEmpty(cell.Caption) && (cell.VisiblePosition!=-1 || cell is Indicator || cell is CalculateIndicator || cell is CalculatorIndicator) && cell is IDecimal  && cell is IFormat && !string.IsNullOrEmpty((cell as IFormat).FormatString))
                                    cell.Caption = Convert.ToDouble(cell.Caption).ToString((cell as IFormat).FormatString);
                            }
                            catch
                            {
                            }
                        }
                    }

                    if (semirow != null)
                    {
                        if (semirow.BackColorContains(cell.Name))
                            cell.BackColor = semirow.BackColors(cell.Name);
                        if (semirow.ForeColorContains(cell.Name))
                            cell.ForeColor = semirow.ForeColors(cell.Name);
                        if (semirow.VisibleContains(cell.Name))
                            cell.Visible = semirow.Visibles(cell.Name);
                        if (semirow.XContains(cell.Name))
                            cell.X = semirow.Xs(cell.Name);
                        if (semirow.WidthContains(cell.Name))
                            cell.Width = semirow.Widths(cell.Name);
                        if (cell is IIndicator)
                        {
                            if (semirow.CompareValue1Contains(cell.Name))
                                (cell as IIndicator).CompareValue.Expression1 = semirow.CompareValue1s(cell.Name).ToString();
                            if (semirow.CompareValue2Contains(cell.Name))
                                (cell as IIndicator).CompareValue.Expression2 = semirow.CompareValue2s(cell.Name).ToString(); 
                        }
                    }
                    if (cell.Visible)
                    {
                        cell.SetRuntimeHeight(_g == null ? g : _g, cell.Caption);
                        _cells.CalcRuntimeHeight(cell);
                    }

                    if ((cell is ICenterAlign) && (cell as ICenterAlign).CenterAlign)
                    {
                        if (_containerwidth != -1)
                        {
                            if (_containerwidth > cell.Width)
                                cell.X = (_containerwidth - cell.Width) / 2;
                            else
                                cell.X = 0;
                            ctmp.X = cell.X;
                        }
                    }

                    _cells.AddByVisiblePosition(cell);

                    if (cell.Visible)
                    {
                        if (_sectionline.Section is IAutoSequence && (_sectionline.Section as IAutoSequence).bAutoSequence)
                        {
                            if (cell is SuperLabel)
                            {
                                cell.Width = (cell as SuperLabel).CalcWidth();
                                if (cell.Width == 0)
                                    cell.Visible = false;
                                else
                                    superlabels.Add(cell);
                            }
                        }
                        else if (cell is SuperLabel)// && cell.Visible)
                        {
                            cell.Width = (cell as SuperLabel).CalcWidth();
                            superlabels.Add(cell);
                        }
                    }
                }
            }
            finally
            {
                if (g != null)
                    g.Dispose();
            }
            _x = _cells.X;
            _ybase = _cells.Y;   
            _expandheight = _cells.ExpandHeight;
            _metaheight = _cells.MetaHeight;

            _cells.AutoLayOut();

            foreach (Cell sp in superlabels)
            {
                HandleSuperLabel(sp);
            }
        }

        private void HandleSuperLabel(Cell cell)
        {
            SuperLabel superlabel = cell as SuperLabel;
            int mh = superlabel.ExpandHeight;
            int rh = this._expandheight;
            int height = 0;
            int oldheight = superlabel.RuntimeHeight;
            superlabel.bTop = true;
            //if (!superlabel.KeepPos)
            //{
            superlabel.AutoHeightY  = superlabel.Y;
            if (mh <= rh)
            {
                superlabel.SetRuntimeHeight(rh - (mh - oldheight));
                height = superlabel.RuntimeHeight - superlabel.Height;
            }

            //}            
            ReleaseASuperLabel(superlabel, height);
        }
		private void ReleaseASuperLabel(SuperLabel superlabel,int height)
		{
			foreach(Label label in superlabel.Labels)
			{
                superlabel.Labels.AdjustRuntimeHeight();
                if (!label.Visible)
                    continue;
     		    label.AutoHeightY =label.Y+height;
				label.Super=superlabel;
				if(label is SuperLabel)
				{
					ReleaseASuperLabel(label as SuperLabel,height);
				}
                _cells.AddByVisiblePosition(label);
                //_cells.AddDirectly(label);
			}
        }

        #region property
        public bool bDrawLastLine
        {
            get
            {
                return _bdrawlastline;
            }
            set
            {
                _bdrawlastline = value;
            }
        }

        public bool bAloneLine
        {
            get
            {
                return _baloneline;
            }
            set
            {
                _baloneline = value;
            }
        }

        public int BaseID
        {
            get
            {
                return _baseid;
            }
        }
        public GroupHeaderRow ParentRow
        {
            get
            {
                return _upperrow;
            }
            set
            {
                _upperrow = value;
            }
        }

        public int PageIndex
		{
			get
			{
				return _pageindex;
			}
		}

        public int PagesInPage
        {
            get
            {
                return _pagesinpage;
            }
            set
            {
                _pagesinpage = value;
            }
        }

		public Cells Cells
		{
			get
			{
				return _cells;
			}
		}

    	public int Y
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

		public int Height
		{
			get
			{
                if (_height != -1)
                    return _height;
                else if (!_bautoheight)
                    return _metaheight;
                else
                    return _expandheight;
			}
            set
            {
                _height = value;
            }
		}

        public bool bAutoHeight
        {
            set
            {
                _bautoheight = value;
            }
        }

		public int Level
		{
			get
			{
				return _level;
			}
		}

		public int Left
		{
			get
			{
				return _left;
			}
		}

        public int Right
        {
            get
            {
                Cell lastcell=_cells[_cells.Count-1];
                return lastcell.X + lastcell.Width;
            }
        }


		public string InArea
		{
			get
			{
				return _area;
			}
		}

		internal Section Section
		{
			get
			{
                return _sectionline.Section;
			}
		}

		public bool bLastInPage
		{
			get
			{
				return _blastinpage;
			}
		}

		public bool bFirst
		{
			get
			{
				return _bfirst;
			}
        }
        #endregion
        
		public void SetXBase(int x,int right,int pageindex,int pagesinpage,int pageoffsize,int pages,Report report,PrintPage printpage)
		{
			_xbase=x;
            _pagesinpage = pagesinpage;
            _pageoffsize = pageoffsize;
            _pageindex = pageindex;
            _pages = pages;
            _right = right;
            _report = report;
            _printpage  = printpage  ;
		}
		#region convert
        //protected int ConvertFromControlToInternal(int x)
        //{
        //    return x+_xbase;
        //}

        protected int ConvertFromInternalToControl(int x)
        {
            return x - _xbase;
        }

        //protected int ExpandScale(int value)
        //{
        //    if (_bprint)
        //        return value * 15 / 14;
        //    else
        //        return value;
        //}
		#endregion

		public virtual void DrawBack(Graphics g ,bool drawback)
		{
            //Color back = Color.Empty ;
            //if (bActiveRow != null && bActiveRow(this) && hasActiveCell !=null && !hasActiveCell())
            //{
            //    back = Color.Blue;//.Gainsboro;
            //    using (SolidBrush sbback = new SolidBrush(back))
            //    {
            //        int x = ConvertFromInternalToControl(_x);
            //        int right = ConvertFromInternalToControl(_right);
            //        if (drawback)
            //        {
            //            if (this.RowState == GroupHeaderRowStates.Classical)
            //            {
            //                int tx = ConvertFromInternalToControl(Section.X);
            //                int tright = ConvertFromInternalToControl(Section.Width);
            //                g.FillRectangle(sbback, tx, _y + 1, tright - tx, Height - 2);
            //            }
            //            else
            //                g.FillRectangle(sbback, x, _y + 1, right - x, Height - 2);
            //        }
            //    }
            //}
		}
		protected int _left;
		public virtual void Draw(Graphics g,int prex,int width,int height,int y,bool bclapse)
		{
			_left=_x;
			this._y=y;		
            DrawBack(g,true );
			prex=ConvertFromInternalToControl(prex);
			for(int i=0;i<_cells.Count;i++)
			{
				Cell c=_cells[i];	
			
				if(bclapse && this.CrossRect!=Rectangle.Empty)
					DrawPlus(g,prex, width);
				if(bclapse && this.HeaderRect!=Rectangle.Empty)
					DrawHeader(g,prex,width);

				DrawCell(c,width,height,y,g);
			}

		}

		public virtual void Draw(Graphics g,int width,int height,int y,bool drawback)
		{
			this._y=y;	
            DrawBack(g,drawback );
			for(int i=0;i<_cells.Count;i++)
			{
				Cell c=_cells[i];
                if (_bdrawlastline && c.Border.Left )
                    c.Border.Bottom = true;
				DrawCell(c,width,height,y,g);
			}
            if (_bdrawlastline)
            {
                DrawLastLine(g);
            }
		}
        private void DrawLastLine(Graphics g)
        {
            if (_cells.Count > 0)
            {
                Cell cell = _cells[0];
                if (cell.Border.Left)
                {
                    using (Brush brushdark = new SolidBrush(cell.BorderColor ))
                    using (Pen pendark = new Pen(brushdark, 1))
                    {
                        int x = ConvertFromInternalToControl(cell.X);
                        pendark.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        g.DrawLine(pendark, 0, _y + this.Height , x , _y + this.Height );
                    }
                }
            }
        }

		private void DrawCell(Cell c,int width,int height,int y,Graphics g)
		{
            if (!c.Visible)
                return;
            if(c is Expression)
            {
                string formula=(c as Expression).Formula.FormulaExpression.ToLower();
                if( formula=="page()")
                {
                    int pageindex = _pageindex+1;//* _pagesinpage + _pageoffsize;
                    formula = formula.Replace("page()", pageindex.ToString());
                    c.Caption = formula;
                }
                else if ( formula=="pages()")
                {
                    int pages = _pages * _pagesinpage;
                    formula = formula.Replace("pages()", pages.ToString());
                    c.Caption = formula;
                }
                else if (formula=="grouppage()")
                {
                    c.Caption = (_printpage==null? "" : Convert.ToString((_printpage.GroupPageIndex-1) * _pagesinpage + _pageoffsize));
                }
                else if (formula=="grouppages()")
                {
                    c.Caption = (_printpage==null? "" : Convert.ToString(_printpage.GroupPages * _pagesinpage));
                }
                else if(c.GetStateType()=="PrintExpression")
                {
                    object o = (_printpage==null ? "": _printpage.GetValue(c));
                    if (o != null)
                        c.Caption = o.ToString();
                    else
                        c.Caption = "";
                }
            }
            else if (c is Chart)
            {
                #region chart
                Chart chart = c as Chart;
                if (chart.bNullChart)
                {
                    try
                    {
                        Infragistics.Win.UltraWinChart.UltraChart mychart = new Infragistics.Win.UltraWinChart.UltraChart();
                        chart.MyChart = mychart;
                        ChartService cs = null;
                        if (_report.Type == ReportType.IndicatorReport)
                            cs = new IndicatorChartService(_report, chart.CaptionToName);
                        else
                            cs = new ChartService(_report);
                        cs.InitializeChart(chart.Level, null, mychart);
                        if (chart.Data == null)
                        {
                            RuntimeGroup group = null;
                            if (chart.Level > 1)
                            {
                                group = AddARuntimeGroup(_report);
                                GroupHeaderRow pr = (this as GroupRow).ParentRow;
                                while (pr != null)
                                {
                                    group.AddAUpperGroup(pr.AddARuntimeGroup(_report));
                                    pr = pr.ParentRow;
                                }
                            }
                            mychart.Data.DataSource = cs.GetDataSource(chart.Level, null, group, mychart.ChartType);
                        }
                        else
                            mychart.Data.DataSource = cs.GetDataSource(chart.Level, null, chart.Data, mychart.ChartType);

                        mychart.Data.DataBind();
                    }
                    catch(Exception  e)
                    {

                        chart.Caption = e.Message;
                        chart.bExceptionChart = true;
                    }
                }
                #endregion
            }
            int x = ConvertFromInternalToControl(c.X);
            if (width != -1 && (x >= width || x + c.Width <= 0))
				return;
						
			int h;
			int ycor=y;
            if (c is SuperLabel)
            {
                if (_bautoheight)
                    h = c.RuntimeHeight;
                else
                    h = c.Height;
            }
            else
            {
                if (_bautoheight)
                    h = c.ExpandHeight;
                else
                    h = c.MetaHeight;
            }

            int celly = c.Y;
            if (c is Label && _bautoheight)
            {
                if (celly < (c as Label).AutoHeightY )
                    celly  = (c as Label).AutoHeightY  ;
            }

            //if(! (c is SubReport))
            //{
			if(c.KeepPos)
				ycor+=celly  -_ybase;
			else
				h=this.Height;
            //}
            c.bActive = false;
            c.bInActiveRow = false;
            if (bActiveCell != null && bActiveCell(c))
                c.bActive = true;
            if (bActiveRow != null && bActiveRow(this) && hasActiveCell != null && hasActiveCell())//!hasActiveCell()
                c.bInActiveRow = true;


			DrawIt(c,g,x,ycor,width,height,h);
		}

        protected RuntimeGroup AddARuntimeGroup(Report report)
        {
            RuntimeGroup group = new RuntimeGroup(this.Level);
            //Section header = report.Sections.GetGroupHeader(this.Level);
            foreach (string key in report.CurrentSchema[this.Level].Items)
            {
                //Cell ctmp = this.Section.Cells[key];
                Cell ctmp = this.Cells[key];
                //Cell ctmp = header.Cells[key];
                if (ctmp != null && ctmp is IMapName)
                {
                    group.AddAItem(new RuntimeGroupItem(key, (ctmp as IMapName).MapName, ctmp.Caption));
                }
            }
            return group;
        }

		protected virtual void DrawIt(Cell c,Graphics g,int x,int y,int width,int height,int h)
		{
            c.draw(g, x, y, h);
		}

		protected virtual void DrawPlus(Graphics g,int prex,int width)
		{
		}

		protected virtual void DrawHeader(Graphics g,int prex,int width)
		{
//            if(!_bfirst || _rowstate==GroupHeaderRowStates.Normal )
//                return;
//            Rectangle rect=this.HeaderRect;
//            if(rect.X<width &&
//                rect.X+rect.Width>0)
//            {
//                if(_level!=1 && ConvertFromInternalToControl(_left)>prex+16)
//                    DrawLine(g,prex,rect);
//////				using (Brush brush=new SolidBrush(DefaultConfigs.RowHeaderColor ))
////				using (Pen pen=new Pen(DefaultConfigs.LineColor))
////				{
////					g.FillRectangle(brush,new Rectangle(rect.X,rect.Y+1,rect.Width,rect.Height-1));
////					g.DrawLine(pen  ,rect.X,rect.Y,rect.X+rect.Width,rect.Y);
////					g.DrawLine(Pens.White ,rect.X,rect.Y+1,rect.X+rect.Width,rect.Y+1);
////					g.DrawLine(Pens.White ,rect.X,rect.Y,rect.X,rect.Y+rect.Height);
////					g.DrawLine(pen,rect.X,rect.Y+rect.Height,rect.X+rect.Width,rect.Y+rect.Height);
////					g.DrawLine(pen,rect.X+rect.Width,rect.Y,rect.X+rect.Width,rect.Y+rect.Height);
////				}
//            }
		}

		protected virtual void DrawLine(Graphics g,int prex,Rectangle rect)
		{
			using(Pen pen=new Pen(DefaultConfigs.LineColor ))
			{
				pen.DashStyle=System.Drawing.Drawing2D.DashStyle.Custom ;
				pen.DashPattern=new float[]{1,1};
				g.DrawLine(pen,prex,rect.Y+rect.Height/2,rect.X+12,rect.Y+rect.Height/2);
			}
		}

		public virtual PositionType OnClick(Point pt)
		{
			if(HeaderRect.Contains(pt))
				return PositionType.Header;
			else if(CrossRect.Contains(pt))
				return PositionType.Cross;
			return PositionType.Other;
		}

		public Rectangle HeaderRect
		{
			get
			{
				if(_level>0)
				{
					int left=ConvertFromInternalToControl(_left);
					return new Rectangle(left-16,_y,16,this.Height);
				}
				else
					return Rectangle.Empty;
			}
		}

		public bool Contains(Point pt)
		{
            if (this.RowState == GroupHeaderRowStates.Classical)
            {
                int tx = ConvertFromInternalToControl(Section.X);
                int tright = ConvertFromInternalToControl(Section.Width);
                return (pt.Y >= _y &&
                    pt.Y <= _y + this.Height &&
                    pt.X >= tx &&
                    pt.X<=tright );   
            }
            else
            {
                return (pt.Y >= _y &&
                    pt.Y <= _y + this.Height &&
                    pt.X >= ConvertFromInternalToControl(_left));                   
            }
		}

        public Cell CellAt(int index)
        {
            return _cells[index];
        }

		public Cell CellAt(Point pt)
		{
			if(!Contains(pt))
				return null;
			for(int i=0;i<_cells.Count;i++)
			{
				Cell cell = _cells[i];
				int left=ConvertFromInternalToControl(cell.X);
                int top = cell.RealY;
                int height = cell.RuntimeHeight;
                //if (_bautoheight)
                //    height = cell.ExpandHeight;
                //else
                //    height = cell.ru.MetaHeight;
				if(pt.X >= left &&
					pt.X<= left + cell.Width &&
                    pt.Y>= top &&
                    pt.Y<=top+height 
                    )
				{
//					if(cell is SuperLabel)
					return cell;
				}
			}
			return null;
		}

		public virtual Rectangle CrossRect
		{
			get
			{
				return Rectangle.Empty;
			}
		}

        public GroupHeaderRowStates RowState
        {
            get
            {
                return _rowstate;
            }
            set
            {
                _rowstate = value;
            }
        }


		#region IDisposable 成员

		public void Dispose()
		{
			if(_cells !=null)
			{
				_cells.Dispose();
				_cells=null;
			}
		}

		#endregion

        #region ISerializable 成员

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Cells", _cells);
            info.AddValue("bAutoHeight", _bautoheight);
            info.AddValue("Level", _level);
            info.AddValue("Area", _area);
            info.AddValue("Height", Height);
            info.AddValue("PageIndex", _pageindex);
            info.AddValue("bLastInPage", _blastinpage);
            info.AddValue("bFirst", _bfirst);
            info.AddValue("X", _x);
            info.AddValue("Right", _right);
        }

        #endregion

        #region IRow 成员

        public object this[string key]
        {
            get
            {
                Cell cell = this.Cells[key];
                if (cell != null)
                    return cell.Caption;
                else
                    return null;
            }
            set
            {
            }
        }

        public bool bMinorLevel
        {
            get
            {
                return _bminorlevel;
            }
        }

        #endregion
    }

	public delegate bool ActiveCellHandler(Cell cell);
    public delegate bool ActiveRowHandler(Row row);
    public delegate bool HasActiveCellHandler();
	public enum PositionType
	{
		Header,
		Cross,
		Other,
		None
	}

    public interface IKeyToObject
    {
        object this[string key] { get; set; }
        int Level { get; }
    }

    public interface IRow:IKeyToObject 
    {
        bool bMinorLevel { get;}
    }

    public class PrintPages
    {
        private ArrayList _printpages;
        private PrintPage _currentpage;
        private PrintPage _lastpage;
        private bool _bgrouping;
        private SimpleArrayList _getdatakeys;
        private SimpleArrayList _pagesumkeys;
        private SimpleArrayList _pageaccsumkeys;
        private SimpleArrayList _groupsumkeys;
        private int _groupcount;

        public event GroupEventHanlder OnGroupEvent;

        public PrintPages()
        {
            _printpages = new ArrayList();
            _groupcount = -1;
            _bgrouping = false;
        }

        public SimpleArrayList GetDataKeys
        {
            get
            {
                return _getdatakeys;
            }
        }

        public SimpleArrayList GroupSumKeys
        {
            get
            {
                return _groupsumkeys;
            }
        }

        public PrintPage this[int index]
        {
            get
            {
                if (index < _printpages.Count)
                    return _printpages[index] as PrintPage;
                return null;
            }
        }

        private void AddKey(ref SimpleArrayList list, string key)
        {
            if (list == null)
                list = new SimpleArrayList();
            list.Add(key);
        }

        public void AddAKey(Cell cell)
        {
            if (cell is Expression)
            {
                string key = null;
                string expression=(cell as Expression).Formula.FormulaExpression.ToLower();
                if (expression.StartsWith("getdata("))
                {
                    key = (cell as Expression).GetExpressionKey();
                    AddKey(ref _getdatakeys, key);
                }
                else if (expression.StartsWith("pagesum("))
                {
                    key = (cell as Expression).GetExpressionKey();
                    AddKey(ref _pagesumkeys , key);
                }
                else if (expression.StartsWith("pageaccsum("))
                {
                    key = (cell as Expression).GetExpressionKey();
                    AddKey(ref _pageaccsumkeys, key);
                }
                else if (expression.StartsWith("groupsum("))
                {
                    key = (cell as Expression).GetExpressionKey();
                    AddKey(ref _groupsumkeys, key);
                }
                else if (expression.StartsWith("groupaccsum("))
                {
                    key = (cell as Expression).GetExpressionKey();
                    AddKey(ref _groupsumkeys, key);
                }
                else if (expression.StartsWith("grouppageaccsum("))
                {
                    key = (cell as Expression).GetExpressionKey();
                    AddKey(ref _groupsumkeys, key);
                }
            }
        }

        public bool HasKey
        {
            get
            {
                return (_getdatakeys != null && _getdatakeys.Count > 0)
                    || (_groupsumkeys != null && _groupsumkeys.Count > 0)
                    || (_pageaccsumkeys != null && _pageaccsumkeys.Count > 0)
                    || (_pagesumkeys != null && _pagesumkeys.Count > 0);
            }
        }

        public int GroupCount
        {
            get
            {
                return _groupcount;
            }
        }

        public void HandleARow(IRow semirow)
        {
            if (!semirow.bMinorLevel)
                return;
            if (_currentpage == null)
            {
                _groupcount++;
                _currentpage = new PrintPage(_printpages.Count, _groupcount);
                _printpages.Add(_currentpage);
                SetData(semirow);
            }
            //page sum
            if (_pagesumkeys != null)
            {
                foreach (string key in _pagesumkeys)
                    _currentpage.AddPageSum(key, semirow[key]);
            }
            //page acc sum
            if (_pageaccsumkeys != null)
            {
                foreach (string key in _pageaccsumkeys)
                    _currentpage.AddPageAccSum(key, semirow[key]);
            }
            //group page acc sum
            if (_groupsumkeys != null)
            {
                foreach (string key in _groupsumkeys)
                    _currentpage.AddGroupPageAccSum(key, semirow[key]);
            }
        }

        public void OnPage(IRow semirow)
        {
            PrintPage lastpage = _currentpage;
            if (lastpage != null)
                lastpage.EndPage();
            _currentpage  = new PrintPage(_printpages.Count,_groupcount );
            _printpages.Add(_currentpage);
            TransformDataFromLastWhenOnPage(lastpage);
            if (semirow is SemiRow)
            {
                _currentpage.SemiRow = semirow as SemiRow;
            }
            if (!semirow.bMinorLevel)
                return;
            //grouppageaccsum from the last page in this group
            if (_groupsumkeys != null)
            {
                foreach (string key in _groupsumkeys)
                    _currentpage.AddGroupPageAccSum(key, semirow[key]);
            }
            //pagesum from 0 start
            if (_pagesumkeys != null)
            {
                foreach (string key in _pagesumkeys)
                    _currentpage.AddPageSum(key, semirow[key]);
            }
            //pageaccsum from the last page
            if (_pageaccsumkeys != null)
            {
                foreach (string key in _pageaccsumkeys)
                    _currentpage.AddPageAccSum(key, semirow[key]);
            }
        }

        private void TransformDataFromLastWhenOnPage(PrintPage lastpage)
        {
            if (lastpage != null)
            {
                //getdata from the last group
                if (_getdatakeys != null)
                {
                    foreach (string key in _getdatakeys)
                        _currentpage.AddData(key, lastpage.GetData(key));
                }
                //groupaccsum from the last group
                if (_groupsumkeys != null)
                {
                    foreach (string key in _groupsumkeys)
                        _currentpage.AddGroupAccSum(key, lastpage.GetGroupAccSum(key));
                }
                //grouppageaccsum from the last page in this group
                if (_groupsumkeys != null)
                {
                    foreach (string key in _groupsumkeys)
                        _currentpage.AddGroupPageAccSum (key, lastpage.GetGroupPageAccSum(key));
                }
                //pageaccsum from the last page
                if (_pageaccsumkeys != null)
                {
                    foreach (string key in _pageaccsumkeys)
                        _currentpage.AddPageAccSum(key, lastpage.GetPageAccSum(key));
                }
            }
        }

        public bool bWaitingForEndGroup
        {
            get
            {
                return _bgrouping ;
            }
        }

        public void OnBeginGroup(IRow semirow)
        {
            _bgrouping = true;
            _lastpage = _currentpage;
            if (_lastpage != null)
                _lastpage.EndPage();

            if (semirow != null)
            {
                _groupcount++;
                _currentpage = new PrintPage(_printpages.Count, _groupcount);
                _printpages.Add(_currentpage);
            }
            else
            {
                _currentpage = null;
                return;
            }
            //getdata from 0 start
            SetData(semirow);
        }

        public void OnGrouping(IRow semirow)
        {
            //getdata from 0 start
            SetData(semirow);
        }

        private void SetData(IRow semirow)
        {
            if (_getdatakeys != null)
            {
                foreach (string key in _getdatakeys)
                {
                    object o1 = semirow[key];
                    object o2 =_currentpage.GetData(key);
                    if(o1!=null && o2==null)
                        _currentpage.AddData(key, o1);
                }
            }
        }

        public void OnEndGroup(IRow semirow)
        {
            if (_lastpage != null)
            {
                int count = 0;
                int first = 0;
                for (int i = _printpages.Count - 1; i >= 0; i--)
                {
                    PrintPage tmppage = _printpages[i] as PrintPage;
                    if (tmppage == _currentpage)
                        continue;
                    if (tmppage.GroupIndex != _lastpage.GroupIndex)
                        break;
                    count++;
                    //setgroupsums=grouppageaccsum
                    if (_groupsumkeys != null)
                    {
                        foreach (string key in _groupsumkeys)
                        {
                            tmppage.AddGroupSum(key, _lastpage.GetGroupPageAccSum(key));
                            tmppage.AddGroupAccSum(key, _lastpage.GetGroupPageAccSum(key));
                        }
                    }
                    first = i;
                }
                //setgrouppages
                for (int i = first ; i < first + count; i++)
                {
                    PrintPage tmppage = _printpages[i] as PrintPage;
                    tmppage.GroupPageIndex = i - first + 1;
                    tmppage.GroupPages = count;
                }
                TransformFromLastPageWhenOnGroup();
            }

            if (semirow == null)
                return;

            if (semirow is SemiRow)
            {
                _currentpage.SemiRow = semirow as SemiRow;
            }
            if (!semirow.bMinorLevel)
                return;
            //getdata from 0 start
            SetData(semirow);
            //grouppageaccsum from 0 start
            if (_groupsumkeys != null)
            {
                foreach (string key in _groupsumkeys)
                    _currentpage.AddGroupPageAccSum(key, semirow[key]);
            }
            //pagesum from 0 start
            if (_pagesumkeys != null)
            {
                foreach (string key in _pagesumkeys)
                    _currentpage.AddPageSum(key, semirow[key]);
            }
            //pageaccsum from the last page
            if (_pageaccsumkeys != null)
            {
                foreach (string key in _pageaccsumkeys)
                    _currentpage.AddPageAccSum(key, semirow[key]);
            }
            _bgrouping = false;
        }

        private void TransformFromLastPageWhenOnGroup()
        {
            if (_lastpage != null && _currentpage!=null)
            {
                //groupaccsum from the last group
                if (_groupsumkeys  != null)
                {
                    foreach (string key in _groupsumkeys)
                        _currentpage.AddGroupAccSum(key, _lastpage.GetGroupAccSum(key));
                }
                //pageaccsum from the last page
                if (_pageaccsumkeys != null)
                {
                    foreach (string key in _pageaccsumkeys)
                        _currentpage.AddPageAccSum(key, _lastpage.GetPageAccSum(key));
                }
            }
        }

        public void OnGroup(IRow semirow)
        {
            OnBeginGroup(semirow);
            OnEndGroup(semirow);
            if (OnGroupEvent != null)
                OnGroupEvent(_lastpage, semirow==null?null:_currentpage);
        }
    }

    public class PrintPage
    {
        private int _pageindex;
        private int _groupindex;
        private int _grouppageindex;
        private int _grouppages;
        private SimpleHashtable _getdatas;
        private SimpleHashtable _pagesums;
        private SimpleHashtable _pageaccsums;
        private SimpleHashtable _groupsums;
        private SimpleHashtable _groupaccsums;
        private SimpleHashtable _grouppageaccsums;
        private SemiRow _semirow;
        
        public PrintPage(int index,int groupindex)
        {
            _pageindex = index;
            _groupindex = groupindex;
        }

        public SemiRow SemiRow
        {
            set
            {
                _semirow = value;
            }
        }

        public void EndPage()
        {
            if (_semirow != null)
            {
                //_pagesums
                if (_pagesums != null)
                {
                    foreach (string key in _pagesums.Keys)
                        _semirow.Add("pagesum(\"" + key + "\")", _pagesums[key]);
                }
                //_pageaccsums
                if (_pageaccsums != null)
                {
                    foreach (string key in _pageaccsums.Keys)
                        _semirow.Add("pageaccsum(\"" + key + "\")", _pageaccsums[key]);
                }
                //_grouppageaccsums
                if (_grouppageaccsums != null)
                {
                    foreach (string key in _grouppageaccsums.Keys)
                        _semirow.Add("grouppageaccsum(\"" + key + "\")", _grouppageaccsums[key]);
                }
            }
        }

        public int GroupPageIndex
        {
            get
            {
                return _grouppageindex;
            }
            set
            {
                _grouppageindex = value;
            }
        }

        public int GroupPages
        {
            get
            {
                return _grouppages;
            }
            set
            {
                _grouppages = value;
            }
        }

        public void AddData(string key, object value)
        {
            if (_getdatas == null)
                _getdatas = new SimpleHashtable();
            _getdatas.Add(key, value);
        }

        private void AppendAValue(ref SimpleHashtable values, string key, object value)
        {
            if (values == null)
                values = new SimpleHashtable();
            if (values.Contains(key))
            {
                double d1 = 0;
                if (values[key] == null || values[key].ToString() == "")
                    d1 = 0;
                else
                    d1 = Convert.ToDouble(values[key]);

                double d2 = 0;
                if (value == null || value.ToString() == "")
                    d2 = 0;
                else
                    d2 = Convert.ToDouble(value);
                
                values.Add(key, d1 + d2);
                
            }
            else
                values.Add(key, value);
        }

        public void AddPageSum(string key, object value)
        {
            AppendAValue(ref _pagesums, key, value);
        }

        public void AddPageAccSum(string key, object value)
        {
            AppendAValue(ref _pageaccsums, key, value);
        }

        public void AddGroupSum(string key, object value)
        {
            AppendAValue(ref _groupsums, key, value);
        }

        public void AddGroupAccSum(string key, object value)
        {
            AppendAValue(ref _groupaccsums, key, value);
        }

        public void AddGroupPageAccSum(string key, object value)
        {
            AppendAValue(ref _grouppageaccsums, key, value);
        }

        private object GetAValue(SimpleHashtable hash, string key)
        {
            if(hash!=null && hash.Contains(key))
                return hash[key];
            return null;
        }

        public object GetData(string key)
        {
            return GetAValue(_getdatas, key);
        }

        public object GetPageSum(string key)
        {
            return GetAValue(_pagesums, key);
        }

        public object GetPageAccSum(string key)
        {
            return GetAValue(_pageaccsums , key);
        }

        public object GetGroupSum(string key)
        {
            return GetAValue(_groupsums , key);
        }

        public object GetGroupAccSum(string key)
        {
            return GetAValue(_groupaccsums , key);
        }

        public object GetGroupPageAccSum(string key)
        {
            return GetAValue(_grouppageaccsums , key);
        }

        public object  GetValue(Cell cell)
        {
            if (cell is Expression)
            {
                string key = (cell as Expression).GetExpressionKey();
                string expression = (cell as Expression).Formula.FormulaExpression.ToLower();
                if (expression.StartsWith ("getdata("))
                {
                    return GetAValue(_getdatas, key);
                }
                else if (expression.StartsWith("pagesum("))
                {
                    return GetAValue(_pagesums , key);
                }
                else if (expression.StartsWith("pageaccsum("))
                {
                    return GetAValue(_pageaccsums , key);
                }
                else if (expression.StartsWith("groupsum("))
                {
                    return GetAValue(_groupsums , key);
                }
                else if (expression.StartsWith("groupaccsum("))
                {
                    return GetAValue(_groupaccsums , key);
                }
                else if (expression.StartsWith("grouppageaccsum("))
                {
                    return GetAValue(_grouppageaccsums , key);
                }
            }
            return null;
        }

        public int GroupIndex
        {
            get
            {
                return _groupindex;
            }
            set
            {
                _groupindex = value;
            }
        }
        
    }
    public delegate void GroupEventHanlder(PrintPage oldpage,PrintPage newpage);
}
