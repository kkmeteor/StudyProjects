using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class IndicatorMetrix:Rect,IMetrix ,IIndicatorMetrix ,IGap,IDrill
    {
        #region fields
        private string _groups;
        private string _indicators;
        private string _cross;
        private int _gapheight=4;
        private string _styleid="";
        private bool _showsummary=true;
        private int _pagesize = 50;
        private GridBorderStyle _borderstyle = GridBorderStyle.VerticalAndHorizontal;

        private IndicatorMetrixPart _gds;
        private IndicatorMetrixPart _is;
        private Cell _cd;

        private int _heightoffsize;
        private int _widthoffsize;

        public event EventHandler ColorStyleChanged;
        public event EventHandler ColorStyleDesigning;
        public event BindPartStyleHandler BindingAPartStyle;
        public event EventHandler BorderStyleChanged;
        #endregion

        #region constructor
        public IndicatorMetrix():base()
		{
            
		}
		public IndicatorMetrix(int x,int y):base(x,y)
		{
            
		}

		public IndicatorMetrix(int x,int y,int width,int height):base(x,y,width,height)
		{
            
		}

		public IndicatorMetrix(IndicatorMetrix im):base(im)
		{
            _groups = im.Groups;
            _indicators = im.Indicators;
            _cross = im.Cross;
            _gapheight = im.GapHeight;
            _showsummary = im.ShowSummary;
            _styleid = im.StyleID;
            _pagesize = im.PageSize;
            _borderstyle = im.BorderStyle;
		}

		protected IndicatorMetrix( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _groups = info.GetString("Groups");
            _indicators = info.GetString("Indicators");
            _cross = info.GetString("Cross");
            _gapheight = info.GetInt32("GapHeight");
            _showsummary = info.GetBoolean("ShowSummary");
            _styleid = info.GetString("StyleID");
            _pagesize = info.GetInt32("PageSize");
            _borderstyle = (GridBorderStyle)info.GetValue("BorderStyle", typeof(GridBorderStyle));
		}
		#endregion

        #region override

        public void HandlePartBorder(Cell part)
        {
            if (_borderstyle == GridBorderStyle.None)
                part.Border.NoneBorder();
            else if (_borderstyle == GridBorderStyle.VerticalAndHorizontal)
                part.Border.AllBorder();
            else
            {
                part.Border.Top = true;
                part.Border.Bottom = true;
                part.Border.Left = false;
                part.Border.Right = false;
            }
            part.BorderColor = _bordercolor;
        }

        [DisplayText("U8.UAP.Report.BorderStyle")]
        [LocalizeDescription("U8.UAP.Report.BorderStyle")]
        public GridBorderStyle BorderStyle
        {
            get
            {
                return _borderstyle;
            }
            set
            {
                if (_borderstyle != value)
                {
                    _borderstyle = value;
                    if (BorderStyleChanged != null)
                        BorderStyleChanged(this, null);
                }
            }
        }

        [DisplayText("U8.UAP.Services.Matrix.MaxRows")]
        [LocalizeDescription("U8.UAP.Services.Matrix.MaxRows")]
        public int PageSize
        {
            get
            {
                return _pagesize;
            }
            set
            {
                if(value >=0)
                    _pagesize = value;
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
                base.BackColor = value;
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
        public override bool bHidden
        {
            get
            {
                return base.bHidden;
            }
            set
            {
                base.bHidden = value;
            }
        }

        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                if (_bordercolor.ToArgb() != value.ToArgb())
                {
                    _bordercolor = value;
                    if (BorderStyleChanged != null)
                        BorderStyleChanged(this, null);
                }
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
        public override bool bSolid
        {
            get
            {
                return base.bSolid;
            }
            set
            {
                base.bSolid = value;
            }
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
        public override string PrepaintEvent
        {
            get
            {
                return base.PrepaintEvent;
            }
            set
            {
                base.PrepaintEvent = value;
            }
        }
        

        public override object Clone()
        {
            return new IndicatorMetrix(this);
        }

        protected override void draw(Graphics g)
        {
            if ((_gds == null || _gds.Count == 0) &&
                (_is == null || _is.Count == 0) &&
                _cd == null)
                base.draw(g);
            else
            {
                int tmpy = _y;
                int tmph=_h;
                int partheight = PartHeight;
                _y += partheight;
                _h -= partheight;
                base.draw(g);
                _y = tmpy;
                _h = tmph;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _groups=null;
            _indicators=null;
            _cross=null;
            _styleid=null;

            if(_gds!=null)
            {
                _gds.Dispose();
                _gds = null;
            }
            if (_is != null)
            {
                _is.Dispose();
                _is = null;
            }
            if (_cd != null)
            {
                _cd = null;
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Groups", Groups);
            info.AddValue("Indicators", Indicators );
            info.AddValue("Cross", Cross );
            info.AddValue("GapHeight", _gapheight);
            info.AddValue("ShowSummary", _showsummary);
            info.AddValue("StyleID", _styleid);
            info.AddValue("PageSize", _pagesize);
            info.AddValue("BorderStyle", _borderstyle);
        }

        public override void SetType()
        {
            _type = "IndicatorMetrix";
        }

        public override void SetDefault()
        {
            //_borderside.AllBorder();
            _h = 100;
            _bordercolor = Color.Silver;
        }

        #endregion

        #region new function
        public void DesignColorStyle()
        {
            if (ColorStyleDesigning != null)
                ColorStyleDesigning(this, null);
        }

        //[Browsable(false)]
        [TypeConverter(typeof(ColorStyleTypeConverter))]
        [Editor(typeof(ColorStyleEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DisplayText("U8.UAP.Report.样式")]
        [LocalizeDescription("U8.UAP.Report.样式")]
        public string StyleID
        {
            get
            {
                return _styleid;
            }
            set
            {
                if (_styleid != value)
                {
                    _styleid = value;
                    if (ColorStyleChanged != null)
                        ColorStyleChanged(this, null);
                }
            }
        }

        [DisplayText("U8.UAP.Report.ShowSummaryAnyWay")]
        [LocalizeDescription("U8.UAP.Report.ShowSummaryAnyWay")]
        public bool ShowSummary
        {
            get
            {
                return !_showsummary;
            }
            set
            {
                _showsummary = !value;
            }
        }

        [DisplayText("U8.UAP.Report.下边距")]
        [LocalizeDescription("U8.UAP.Report.下边距")]
        public int GapHeight
        {
            get
            {
                return _gapheight;
            }
            set
            {
                _gapheight = value;
            }
        }

        [Browsable(false)]
        public int HeightOffsize
        {
            get
            {
                return _heightoffsize;
            }
            set
            {
                _heightoffsize = value;
            }
        }

        [Browsable(false)]
        public int WidthOffsize
        {
            get
            {
                return _widthoffsize;
            }
            set
            {
                _widthoffsize = value;
            }
        }

        [Browsable(false)]
        public string Groups
        {
            get
            {
                return _gds != null ? _gds.Names : _groups;              
            }
            set
            {
                _groups = value;
            }
        }

        [Browsable(false)]
        private ArrayList GroupNames
        {
            get
            {
                string groups = Groups;
                if (groups == null)
                    return null;
                string[] names = groups.Split(',');
                ArrayList al = new ArrayList();
                foreach (string s in names)
                    al.Add(s.Trim());
                return al;
            }
        }

        [Browsable(false)]
        public string Indicators
        {
            get
            {
                return _is != null ? _is.Names : _indicators;
            }
            set
            {
                _indicators = value;
            }
        }

        [Browsable(false)]
        private ArrayList IndicatorNames
        {
            get
            {
                string groups = Indicators ;
                if (groups == null)
                    return null;
                string[] names = groups.Split(',');
                ArrayList al = new ArrayList();
                foreach (string s in names)
                    al.Add(s.Trim());
                return al;
            }
        }

        [Browsable(false)]
        public string Cross
        {
            get
            {
                return _cd != null ? _cd.Name : _cross;
            }
            set
            {
                _cross = value;
            }
        }

        [Browsable(false)]
        private string CrossName
        {
            get
            {
                return Cross;
            }
        }

        [Browsable(false)]
        public IndicatorMetrixPart GroupParts
        {
            get
            {
                return _gds;
            }
        }

        [Browsable(false)]
        public IndicatorMetrixPart IndicatorParts
        {
            get
            {
                return _is;
            }
        }

        [Browsable(false)]
        public Cell CrossPart
        {
            get
            {
                return _cd;
            }
        }

        [Browsable(false)]
        public Cells AllParts
        {
            get
            {
                Cells cells = new Cells();
                if (_gds != null && _gds.Count > 0)
                {
                    foreach (Cell cell in _gds)
                        cells.AddDirectly(cell);
                }
                if (_is != null && _is.Count > 0)
                {
                    foreach (Cell cell in _is)
                        cells.AddDirectly(cell);
                }
                if (_cd != null)
                    cells.AddDirectly(_cd);
                return cells;
            }
        }

        public void InitParts(Cells cells)
        {
            ArrayList al = GroupNames;
            if (al != null)
            {
                foreach (string name in al)
                {
                    Cell cell = cells[name];
                    if (cell != null && cell is IPart)
                        AddAPartFirst(cell as IPart);
                }
            }
            al = IndicatorNames ;
            if (al != null)
            {
                foreach (string name in al)
                {
                    Cell cell = cells[name];
                    if (cell != null && cell is IPart)
                        AddAPartFirst(cell as IPart);
                }
            }
            string cross = Cross;
            if (!string.IsNullOrEmpty(cross))
            {
                Cell cell = cells[cross];
                if (cell != null && cell is IPart)
                    AddAPartFirst(cell as IPart);
            }
            RefreshLayoutByPart();
        }

        public void AddAPart(IPart part)
        {
            AddAPartFirst(part);
            RefreshLayoutByPart();
        }

        private void AddAPartFirst(IPart part)
        {
            switch (part.PartType)
            {
                case PartType.Group:
                    AddAGroupDimension(part);
                    break;
                case PartType.Cross:
                    AddCrossDimension(part);
                    break;
                case PartType.Indicator:
                    AddAIndicator(part);
                    break;
            }
            if (BindingAPartStyle != null)
                BindingAPartStyle(this, part);
        }

        private void AddAGroupDimension(IPart gd)
        {
            if (_gds == null)
                _gds = new IndicatorMetrixPart();
            //if (gd.Metrix != null)
            //    gd.Metrix.RemovePart(gd);
            gd.Metrix = this;
            _gds.AddAPart(gd);
        }

        private void AddAIndicator(IPart ind)
        {
            if (_is == null)
                _is = new IndicatorMetrixPart();
            //if (ind.Metrix != null)
            //    ind.Metrix.RemovePart(ind);
            ind.Metrix = this;
            _is.AddAPart(ind);
        }

        private void AddCrossDimension(IPart cd)
        {
            //if (cd.Metrix != null)
            //    cd.Metrix.RemovePart(cd);
            cd.Metrix = this;
            _cd = (Cell)cd;
        }

        public void RemoveAPart(IPart part)
        {
            RemoveAPartFirst(part);
            RefreshLayoutByPart();
        }

        private void RemoveAPartFirst(IPart part)
        {
            switch (part.PartType)
            {
                case PartType.Group:
                    _gds.RemoveAPart(part);
                    break;
                case PartType.Cross:
                    //part.Metrix = null;
                    _cd = null;
                    _cross = null;
                    break;
                case PartType.Indicator:
                    _is.RemoveAPart(part);
                    break;
            }
        }

        //public void DeleteParts(SelectedCells cells)
        //{
        //    foreach (Cell cell in cells)
        //    {
        //        if (cell == _cd)
        //        {
        //            _cd = null;
        //            _cross = null;
        //        }
        //        else
        //        {
        //            bool bfind = false;
        //            if (_gds != null)
        //            {
        //                foreach (Cell part in _gds)
        //                {
        //                    if (cell == part)
        //                    {
        //                        _gds.RemoveAPart((IPart)part);
        //                        _groups = null;
        //                        bfind = true;
        //                        break;
        //                    }
        //                }
        //            }
        //            if (!bfind && _is != null)
        //            {
        //                foreach (Cell part in _is)
        //                {
        //                    if (cell == part)
        //                    {
        //                        _is.RemoveAPart((IPart)part);
        //                        _indicators  = null;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    RefreshLayout();
        //}

        public void MoveAPart(IPart part)
        {
            RemoveAPartFirst(part);
            AddAPart(part);
        }

        public void RefreshLayoutBySelf()
        {
            if (_h < 80)
                _h = 80;
            int partheight = PartHeight;
            int left = _x;
            int width = 0;
            Cell last = null;
            if (_gds != null)
            {
                foreach (Cell cell in _gds)
                {
                    cell.X = left;
                    cell.RelativeY = _relativey ;
                    cell.Y = _y;
                    cell.Height = partheight;
                    left += cell.Width;
                    width += cell.Width;
                    last = cell;
                }
            }
            partheight = 0;
            if (_cd != null)
            {
                _cd.X = left;
                _cd.Y = _y;
                _cd.RelativeY = _relativey;
                _cd.Width = _w - width;
                last = null;
                partheight = VSIZE;
            }
            if (_is != null)
            {
                foreach (Cell cell in _is)
                {
                    cell.X = left;
                    cell.RelativeY = _relativey +partheight ;
                    cell.Y = _y+partheight ;
                    left += cell.Width;
                    width += cell.Width;
                    last = cell;
                }
            }
            if (last != null)
                last.Width += _w- width;

            OnOtherChanged(null);
        }

        public void RefreshLayoutByPart()
        {
            int partheight = PartHeight;
            int left = _x;
            int width = 0;
            if (_gds != null)
            {
                foreach (Cell cell in _gds)
                {
                    cell.X = left;
                    cell.RelativeY = _relativey;
                    cell.Y = _y;
                    cell.Height = partheight;
                    left += cell.Width;
                    width += cell.Width;
                }
            }
            partheight = 0;
            int iwidth = 0;
            int cwidth = 0;
            if (_cd != null)
            {
                _cd.X = left;
                _cd.Y = _y;
                _cd.RelativeY = _relativey;
                cwidth = _cd.Width;
                partheight = VSIZE;
            }
            if (_is != null)
            {
                foreach (Cell cell in _is)
                {
                    cell.X = left;
                    cell.RelativeY = _relativey + partheight;
                    cell.Y = _y + partheight;
                    left += cell.Width;
                    iwidth += cell.Width;
                }
            }

            if (width + iwidth > 0 || width + cwidth > 0)
            {
                _w = Math.Max(width + iwidth, width + cwidth);
                if (_w == width + iwidth)
                {
                    if (_cd != null)
                        _cd.Width = iwidth;
                }
                else
                {
                    if (_is != null && _is.Count > 0)
                        _is[_is.Count - 1].Width += cwidth - iwidth;
                }
            }
            OnOtherChanged(null);
        }

        private int PartHeight
        {
            get
            {
                int partheight = 0 ;
                if ((_gds != null&& _gds.Count>0)|| _cd != null || (_is != null && _is.Count>0))
                    partheight = VSIZE;
                if (_cd != null && _is != null && _is.Count>0)
                    partheight = VSIZE * 2;
                return partheight;
            }
        }
        #endregion

        #region IDrill Members
        [Browsable(false)]
        [DisplayText("U8.UAP.Report.DrillToReport")]
        [LocalizeDescription("U8.UAP.Report.DrillToReport")]
        [TypeConverter(typeof(DrillDownTypeConverter))]
        [Editor(typeof(DrillDownEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string DrillToReport
        {
            get
            {
                return String.Empty ;
            }
            set
            {
            }
        }
        [Browsable(false)]
        [DisplayText("U8.UAP.Report.DrillToUAPVouch")]
        [LocalizeDescription("U8.UAP.Report.DrillToUAPVouch")]
        [TypeConverter(typeof(DrillDownTypeConverter))]
        [Editor(typeof(DrillDownEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string DrillToVouch
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }

        public event EventHandler ReportDrillDesigning;

        public event EventHandler VouchDrillDesigning;

        public void DesignReportDrill()
        {
            if (ReportDrillDesigning != null)
                ReportDrillDesigning(this, null);
        }

        public void DesignVouchDrill()
        {
            if (VouchDrillDesigning != null)
                VouchDrillDesigning(this, null);
        }
        #endregion
    }

    public delegate void BindPartStyleHandler(IIndicatorMetrix metrix,IPart part);

    public interface IIndicatorMetrix
    {
        string Groups { get;set;}
        string Indicators { get;set;}
        string Cross { get;set;}
        IndicatorMetrixPart GroupParts { get;}
        IndicatorMetrixPart IndicatorParts { get;}
        Cell CrossPart { get;}
        string StyleID { get;set;}
        bool ShowSummary { get;set;}
        int PageSize { get;set;}
        GridBorderStyle BorderStyle { get;set;}
        Color BorderColor { get;set;}
    }

    public interface IMetrix
    {
        void RefreshLayoutByPart();
        void RefreshLayoutBySelf();
        void AddAPart(IPart part);
        void RemoveAPart(IPart part);
        void MoveAPart(IPart part);
        Cells AllParts { get;}
        void InitParts(Cells cells);
        int HeightOffsize { get;set;}
        int WidthOffsize { get;set;}
        void HandlePartBorder(Cell part);
    }

    public interface IPart
    {
        IMetrix Metrix{get;set;}
        PartType PartType { get;}
    }

    public enum PartType
    {
        Group,
        Cross,
        Indicator
    }

    public class IndicatorMetrixPart : IEnumerable,IDisposable
    {
        private ArrayList _parts;
        public IndicatorMetrixPart()
        {
            _parts = new ArrayList();
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _parts.GetEnumerator();
        }

        #endregion

        public int Count
        {
            get
            {
                return _parts.Count;
            }
        }

        public string Names
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _parts.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(",");
                    sb.Append(((Cell)_parts[i]).Name);
                }
                return sb.ToString();
            }
        }

        public Cell this[int index]
        {
            get
            {
                if (index < _parts.Count)
                    return (Cell)_parts[index];
                return null;
            }
        }

        public void AddAPart(IPart  part)
        {
            int index = 0;
            while (index < _parts.Count)
            {
                Cell cell = _parts[index] as Cell;
                if (((Cell)part).X < cell.X)
                {
                    _parts.Insert(index, part);
                    break;
                }
                index++;
            }
            if (index == _parts.Count)
                _parts.Add(part);
        }

        public void RemoveAPart(IPart part)
        {
            _parts.Remove(part);
        }



        #region IDisposable Members

        public void Dispose()
        {
            _parts.Clear();
            _parts = null;
        }

        #endregion
    }
}
