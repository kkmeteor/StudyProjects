using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;
using UFIDA.U8.UAP.Services.ReportFilterService;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class IndicatorBuilder
    {
        protected  SemiRowsContainerPerhaps4Matrix _semirowscontainer;
        protected  SimpleHashtable _cellnametocolumnname;
        protected  string _viewid;
        protected  Report _currentreport;
        
        protected  IndicatorBuilder()
        {
        }
        public IndicatorBuilder(string viewid,IndicatorMetrix matrix,SemiRowsContainerPerhaps4Matrix semirowscontainer)
        {
            _viewid = viewid;
            _semirowscontainer = semirowscontainer;            
        }

        public virtual object BuildResult
        {
            get
            {
                return null;
            }
        }

        public virtual void HandleHeader(Cells cells)
        {
        }

        public string ViewID
        {
            get
            {
                return _viewid;
            }
        }
        public virtual void BuildIndicatorMetrix(Report report, bool bdrilldefined)
        {
            _currentreport = report;
            foreach (Section section in report.Sections)
            {
                section.AsignToSectionLines();
            }
            if (bdrilldefined)
            {
                CreateNameToColumnMap();
                _semirowscontainer.SetDrillTag(new DrillData(_viewid, report.ViewID), _cellnametocolumnname);
            }
        }

        public virtual void BuildRows()
        {
        }

        public virtual void EndBuild()
        {
        }

        protected void HandleSummaryRow(SemiRows semirows)
        {
            if (!_currentreport.bShowSummary && semirows.Count > 0)
            {
                int summaryindex = semirows.Count - 1;
                if (semirows[summaryindex].SectionType == SectionType.ReportSummary)
                    semirows.RemoveAt(summaryindex);
            }
        }

        protected void CreateNameToColumnMap()
        {
            _cellnametocolumnname = new SimpleHashtable();
            foreach (Section section in _currentreport.Sections)
            {
                if (section is GroupHeader)
                {
                    foreach (Cell cell in section.Cells)
                    {
                        if (cell is IGroup)
                            _cellnametocolumnname.Add(cell.Name, (cell as IMapName).MapName);
                    }
                }
                else if (section is Detail)
                {
                    foreach (Cell cell in section.Cells)
                    {
                        if (cell is GroupDimension || cell is CalculateGroupDimension )
                            _cellnametocolumnname.Add(cell.Name, (cell as IMapName).MapName);
                    }
                }
            }
        }
    }

    public class IndicatorDetailBuilder : IndicatorBuilder
    {
        private Cells _currentcells;
        private IMetrix _currentmetrix; 
        private SectionLines _currentlines;        
        private SectionLine _currentline;
        private SectionLine _newline;
        private int _currentx;
        private int _currenty;
        private int _beginy;        
        private Detail _detail;        
        private Rows _rows;
        private new SemiRowsContainer _semirowscontainer;
        
        private DataTable _currenttable;
        private DataSet _data;
        private SimpleHashtable _captiontoname;
        private SimpleHashtable _designcaptiontodesignname;
        private int _rowsheight=0;        
        
        private DrillData _chartdrilldata;

        public IndicatorDetailBuilder(string viewid,IndicatorDetail indicatordetail,Detail detail,SemiRowsContainer semirowscontainer)
        {
            _viewid = viewid;
            indicatordetail.AsignToSectionLines();
            _currentcells = indicatordetail.Cells;
            _currentlines = indicatordetail.SectionLines;
            
            _detail = detail;
            _detail.UnderState = ReportStates.Browse;
            _semirowscontainer = semirowscontainer;
            _rows = new Rows();

            for (int i = 0; i < _currentlines.Count; i++)
            {
                _newline = new SectionLine(_detail);
                _detail.SectionLines.Add(_newline);
            }
            
            _data = new DataSet();
        }

        public override void BuildIndicatorMetrix(Report report,bool hasdrilldefined)
        {
            BuildTheLastMetrix();

            _currentmetrix = _currentcells[report.ViewID] as IMetrix;
            _currentreport = report;
            Section pagetitle = report.Sections[SectionType.PageTitle];
            if (bChartDataSource)
                CreateDataTable(pagetitle);
            if (hasdrilldefined)
            {
                if ((_currentmetrix as Cell).Visible || _currenttable != null)
                {
                    CreateNameToColumnMap();
                    if (_currenttable != null)
                        _chartdrilldata = new DrillData(_viewid, report.ViewID);
                }
            }

            if ((_currentmetrix as Cell).Visible)
            {
                int index = FindTheLine(_currentmetrix as Cell);
                _currentline = _currentlines[index];

                _currentx = (_currentmetrix as Cell).X + bRightAMetrix(_currentmetrix as Cell, _currentline);
                _currenty = (_currentmetrix as Cell).Y + bUnderAMetrix(_currentmetrix as Cell, _currentline);
                _beginy = _currenty;
                
                _newline = _detail.SectionLines[index];

                foreach (Section section in report.Sections)
                {
                    section.AsignToSectionLines();
                    //if (section is GroupHeader)
                    //{
                    //    foreach (Cell cell in section.Cells)
                    //        cell.Border.AllBorder();
                    //}
                }
                
                _rows.AppendFrom(pagetitle);
            }
        }

        public override void BuildRows()
        {
            SemiRows semirows = _semirowscontainer.GetABlock();
            HandleSummaryRow(semirows);
            if(_currenttable!=null)
                FillDataTable(semirows );
            if ((_currentmetrix as Cell).Visible)
            {
                if ((_currentmetrix as Cell).Tag == null)
                    (_currentmetrix as Cell).Tag = new SemiRowsContainerPerhaps4Matrix(100);//(_currentmetrix as IIndicatorMetrix).PageSize
                ((SemiRowsContainer)(_currentmetrix as Cell).Tag).AddSemiRows(semirows);
                if (_rowsheight == Int32.MaxValue)
                    return;
                _rows.AppendFrom(_currentreport, semirows);
                _rowsheight = RowsHeight;
                if (_rowsheight >= (_currentmetrix as Cell).Height)
                    _rowsheight = Int32.MaxValue;
            }
        }        

        private int RowsHeight
        {
            get
            {
                int height = 0;
                for (int i = 0; i < _rows.Count; i++)
                {
                    Row r = _rows[i];
                    r.bAutoHeight = true;
                    height += r.Height;
                }
                return height;
            }
        }

        public override  void EndBuild()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(ClientReportContext.LocaleID);
            BuildTheLastMetrix();
            bool bonlychartorgauge = bOnlyOneChartOrGaugeVisible;
            for (int i = 0; i < _currentlines.Count; i++)
            {
                SectionLine sl = _currentlines[i];
                SectionLine newline = _detail.SectionLines[i];
                foreach (Cell cell in sl.Cells)
                {
                    if (!cell.Visible)
                        continue;
                    if (cell is IMetrix)
                        continue;
                    if (cell is Chart)
                        (cell as Chart).Data = _data.Tables[(cell as Chart).DataSource];
                    if (cell is Gauge)
                        (cell as Gauge).Indicator = _currentcells[(cell as Gauge).IndicatorName ] as CalculatorIndicator ;
                    int yoffsize = bUnderAMetrix(cell, sl);
                    cell.SetY(cell.Y+ yoffsize);
                    int xoffsize = bRightAMetrix(cell, sl);
                    cell.X+=xoffsize;
                    AddCellToNewLine(newline,cell);

                    if (!bonlychartorgauge && cell is IGap && (cell as IGap).GapHeight > 0 && bLastOneIntheLine(cell, sl))
                        AddCellToNewLine(newline,GetAGap(cell as IGap));
                }
            }
        }

        private bool bOnlyOneChartOrGaugeVisible
        {
            get
            {
                int visiblecount=0;
                Cell vc = null;
                for (int i = 0; i < _currentcells.Count; i++)
                {
                    Cell cell = _currentcells[i];
                    if (cell.Visible)
                    {
                        visiblecount++;
                        vc = cell;
                    }
                    if (visiblecount > 1)
                        return false;
                }
                if (visiblecount == 1 && (vc is Chart || vc is Gauge))
                    return true;
                return false;
            }
        }

        private Cell GetAGap(IGap sourcecell)
        {
            return GetAGap(sourcecell, (sourcecell as Cell).X, (sourcecell as Cell).Y + (sourcecell as Cell).Height-1);
        }

        private Cell GetAGap(IGap sourcecell, int x, int y)
        {
            CommonLabel cl = new CommonLabel(sourcecell as Cell);
            cl.Border.NoneBorder();
            cl.Height = sourcecell.GapHeight;
            cl.X = x;
            cl.SetY(y);
            cl.Width = 2;
            cl.Tag = "NoMore";
            //cl.BackColor = Color.Red;
            return cl;
        }

        private Cell MoreLabel(bool bmore,Cell sourcecell,int x, int y,int w,int h)
        {
            CommonLabel cl = new CommonLabel(sourcecell);
            cl.X = x;
            cl.SetY(y);
            cl.Width = w;
            cl.Height = h;
            cl.CaptionAlign = ContentAlignment.TopLeft;
            if (bmore)
            {
                cl.Caption = U8ResService.GetResStringEx("U8.UAP.Report.ViewAll");
                _currentreport.RowsCount = (sourcecell.Tag as SemiRowsContainerPerhaps4Matrix).RowsCount;
                _currentreport.PageRecords = (sourcecell as IIndicatorMetrix).PageSize;
                cl.Report = _currentreport;
                (sourcecell.Tag as SemiRowsContainerPerhaps4Matrix).SetDrillTag(new DrillData(_viewid, _currentreport.ViewID), _cellnametocolumnname);
                cl.Tag = sourcecell.Tag ;
            }
            else
            {
                if(sourcecell.Tag is SemiRowsContainer)
                    (sourcecell.Tag as SemiRowsContainer).Canceled();
                cl.Tag = "NoMore";
                
            }

            cl.Border.Left = false;
            cl.Border.Bottom = false;
            cl.Border.Right = false;

            cl.ServerFont.UnderLine = true;
            cl.ForeColor = Color.Blue;
            
            //cl.BackColor = Color.Red;
            return cl;
        }

        private bool bChartDataSource
        {
            get
            {
                foreach (Cell cell in _currentcells)
                {
                    if (cell is Chart && (cell as Chart).DataSource.ToLower().Trim() == (_currentmetrix as Cell).Name.ToLower().Trim())
                        return true;
                }
                return false;
            }
        }

        private void FillDataTable(SemiRows semirows)
        {
            foreach (SemiRow semirow in semirows)
            {
                if (semirow.Level == 0)
                    continue;
                DataRow dr= _currenttable.NewRow();
                _currenttable.Rows.Add(dr);

                string groupkey = null;
                foreach (string key in _captiontoname.Keys)
                {
                    object o = _captiontoname[key];
                    if (o != null)
                    {
                        if (o is string)
                        {
                            string v = semirow[o.ToString()].ToString();
                            if (v == "")
                                dr[key] = DBNull.Value;
                            else
                                dr[key] = v;
                        }
                        else if (o is SimpleArrayList)
                        {
                            SimpleArrayList sal = o as SimpleArrayList;
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in sal)
                            {
                                if (sb.Length > 0)
                                    sb.Append(" ");
                                sb.Append(semirow[s].ToString());
                            }
                            groupkey = sb.ToString();
                            dr[key] = groupkey;
                        }
                    }
                }
                if (_cellnametocolumnname != null)
                {
                    DrillData dd = new DrillData(_viewid, ((Cell)_currentmetrix).Name);
                    foreach (string key in _cellnametocolumnname.Keys)
                        dd.Add(_cellnametocolumnname[key].ToString(), semirow[key]);

                    _chartdrilldata.Add(groupkey, dd);
                }
            }
        }

        private void CreateDataTable(Section pagetitle)//from pagetitle,designcaption=null, to be x; ____ to be none; a___b to be two new columns; others to be a new column
        {
            _captiontoname = new SimpleHashtable();
            _designcaptiontodesignname  = new SimpleHashtable();
            _designcaptiontodesignname.Add("__bcross__", _currentreport.Type == ReportType.CrossReport);
            _currenttable = new DataTable((_currentmetrix as Cell).Name);
            AddATableColumn("X", new SimpleArrayList(), typeof(string),null,null);
            _data.Tables.Add(_currenttable);

            foreach (Cell cell in pagetitle.Cells)
            {
                if (cell is Label)
                {
                    Label l = cell as Label;
                    if (l.DesignCaption != "____")
                    {
                        if (l is SuperLabel)
                        {
                            foreach (Label sl in (l as SuperLabel).Labels)
                            {
                                if(!string.IsNullOrEmpty(sl.DesignCaption) && sl.DesignCaption!="____") 
                                    AddATableColumn(l.Caption  + "____" + sl.Caption, sl.Name, typeof(decimal), sl.Caption, sl.DesignName);
                                //AddATableColumn(l.DesignCaption + "____" + sl.DesignCaption, sl.Name, typeof(decimal), sl.DesignCaption, sl.DesignName);
                            }
                        }
                        else if (string.IsNullOrEmpty(l.DesignCaption))//x
                        {
                            SimpleArrayList sal = _captiontoname["X"] as SimpleArrayList;
                            sal.Add(l.Name);
                        }
                        else
                        {
                            AddATableColumn(l.Caption,l.Name,typeof(decimal), l.Caption ,l.DesignName );
                            //AddATableColumn(l.DesignCaption, l.Name, typeof(decimal), l.DesignCaption, l.DesignName);
                        }
                    }
                }
            }
        }

        private void AddATableColumn(string columnname,object source,Type type,string designcaption,string designname)
        {
            if (!_captiontoname.Contains(columnname))
            {
                _currenttable.Columns.Add(new DataColumn(columnname, type));
                _captiontoname.Add(columnname, source);
            }
            if (designcaption!=null && !_designcaptiontodesignname.Contains(designcaption))
                _designcaptiontodesignname.Add(designcaption, designname);
        }

        private void BuildTheLastMetrix()
        {
            if (_currentmetrix != null)
            {
                if ((_currentmetrix as Cell).Visible)
                {
                    AddToNewLinesFromRows();
                    _rows.Clear();
                }

                if (_captiontoname != null)
                {
                    foreach (Cell cell in _currentcells)
                    {
                        if (cell is Chart && (cell as Chart).DataSource.ToLower().Trim() == (_currentmetrix as Cell).Name.ToLower().Trim())
                        {
                            Hashtable h = new Hashtable();
                            h.Add((cell as Chart).Level, _designcaptiontodesignname );
                            (cell as Chart).CaptionToName = h;

                            (cell as Chart).DrillTag = _chartdrilldata;
                        }
                    }
                    _captiontoname.Clear();
                }
                
                _captiontoname = null;
                _currenttable = null;
                _designcaptiontodesignname = null;
                _rowsheight = 0;
                _cellnametocolumnname = null;
                _chartdrilldata = null;
            }
        }

        private void AddToNewLinesFromRows()
        {
            int beginx = _currentx;
            int realheight = 0;
            foreach (Row row in _rows)
            {
                row.bAutoHeight = true;
                if (_rowsheight==Int32.MaxValue && realheight + row.Height > ((Cell)_currentmetrix).Height - 24)
                {//(_currentmetrix as Cell).X
                    AddCellToNewLine(MoreLabel(true,_currentmetrix as Cell,beginx , _currenty, _currentx - beginx, ((Cell)_currentmetrix).Height - (_currenty - _beginy)));
                    break;
                }
                _currentx = beginx;
                DrillData dd=null;
                if (row.InArea == "GroupHeader" || row.InArea == "Detail" || row.InArea == "GroupSummary")
                    dd = new DrillData(_viewid, ((Cell)_currentmetrix).Name);
                foreach (Cell cell in row.Cells)
                {
                    if (!cell.Visible)
                        continue;
                    //if(cell is CommonLabel || row.InArea=="GroupHeader")
                    //    cell.Border.AllBorder();
                    if (cell.Super == null)
                    {
                        cell.X = _currentx;
                        cell.SetY(_currenty);
                        if (!(cell is SuperLabel))
                        {
                            cell.SetRuntimeHeight(row.Height);
                            cell.Height = row.Height;
                        }
                        else
                            cell.Tag = 0;
                        _currentx += cell.Width;
                    }
                    else
                    {
                        cell.X = cell.Super.X + (int)cell.Super.Tag ;
                        cell.SetY(cell.Super.Y + cell.Super.RuntimeHeight);
                        cell.Super.Tag =(int)cell.Super.Tag + cell.Width;
                    }
                    AddCellToNewLine(cell);

                    if (_cellnametocolumnname != null && dd!=null && _cellnametocolumnname.Contains(cell.Name))
                    {
                        dd.Add(_cellnametocolumnname[cell.Name].ToString(), cell.Caption);                        
                    }
                    cell.DrillTag = dd;
                }
                _currenty += row.Height;
                realheight += row.Height;
            }
            if (_rowsheight != Int32.MaxValue)//(_currentmetrix as Cell).X
                AddCellToNewLine(MoreLabel(false,_currentmetrix as Cell, beginx , _currenty, _currentx - beginx, ((Cell)_currentmetrix).Height - (_currenty - _beginy)));

            _currentmetrix.WidthOffsize = (_currentx - beginx - ((Cell)_currentmetrix).Width > 0) ? _currentx - beginx - ((Cell)_currentmetrix).Width : 0;
            //_currentmetrix.HeightOffsize = (_currenty - _beginy - ((Cell)_currentmetrix).Height > 0) ? _currenty - _beginy - ((Cell)_currentmetrix).Height : 0;

            if ((_currentmetrix as IGap).GapHeight > 0 && bLastOneIntheLine(_currentmetrix as Cell, _currentline))//(_currentmetrix as Cell).X
                AddCellToNewLine(GetAGap(_currentmetrix as IGap,beginx  , _beginy + (_currentmetrix as Cell).Height ));//_currenty
        }

        private void AddCellToNewLine(Cell cell)
        {
            AddCellToNewLine(_newline, cell);
        }

        private void AddCellToNewLine(SectionLine newline,Cell cell)
        {
            cell.KeepPos = true;
            newline.Cells.Add (cell);
            newline.Cells.CalcRuntimeHeight(cell);
            newline.Section.Cells.Add(cell);
        }

        private int FindTheLine(Cell metrix)
        {
            for (int i = 0; i < _currentlines.Count; i++)
            {
                SectionLine sl = _currentlines[i];
                foreach (Cell cell in sl.Cells)
                    if (cell == metrix)
                        return i;
            }
            return -1;
        }

        private bool bLastOneIntheLine(Cell cell, SectionLine sl)
        {
            foreach (Cell c in sl.Cells)
            {
                if (c.Y >= cell.Y +cell.Height  && ((cell.X >= c.X && cell.X < c.X + c.Width) || (cell.X + cell.Width > c.X && cell.X + cell.Width <= c.X + c.Width)))
                    return false;
            }
            return true;
        }

        private int bUnderAMetrix(Cell cell,SectionLine sl)
        {
            int offsize = 0;
            foreach (Cell c in sl.Cells)
            {
                if (c is IMetrix && cell.Y>=c.Y+c.Height  && ((cell.X >= c.X && cell.X < c.X + c.Width) || (cell.X+cell.Width  > c.X && cell.X+cell.Width  <= c.X + c.Width)))
                    offsize += (c as IMetrix).HeightOffsize;
            }
            return offsize;
        }

        //private int bRightAMetrix(Cell cell, SectionLine sl)
        //{
        //    int offsize = 0;
        //    foreach (Cell c in sl.Cells)
        //    {
        //        if (c is IMetrix && cell.X >= c.X + c.Width && ((cell.Y>=c.Y && cell.Y<c.Y+c.Height )||(cell.Y+cell.Height >c.Y && cell.Y+cell.Height<=c.Y+c.Height )))
        //            offsize += (c as IMetrix).WidthOffsize;
        //    }
        //    return offsize;
        //}
        private int bRightAMetrix(Cell cell, SectionLine sl)
        {
            int offsize = 0;
            foreach (Cell c in sl.Section.Cells)
            {
                if (c is IMetrix && cell.X >= c.X + c.Width)// && ((cell.Y >= c.Y && cell.Y < c.Y + c.Height) || (cell.Y + cell.Height > c.Y && cell.Y + cell.Height <= c.Y + c.Height)))
                    offsize += (c as IMetrix).WidthOffsize;
            }
            return offsize;
        }
    }

    //chart: hashtable->drilldata
    //matrix: arraylist->drilldata
    public class DrillData:IRelateData
    {
        private string _viewid;
        private string _matrixname;
        private SimpleHashtable _data;
        private object _tag;

        public DrillData(string viewid,string matrixname)
        {
            _viewid = viewid;
            _matrixname = matrixname;
            _data = new SimpleHashtable();
        }

        public void Add(string columnname, object value)
        {
            _data.Add(columnname, value);
        }

        public string ViewID
        {
            get
            {
                return _viewid;
            }
            set
            {
                _viewid = value;
            }
        }

        public string MatrixName
        {
            get
            {
                return _matrixname;
            }
            set
            {
                _matrixname = value;
            }
        }

        public object Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }

        #region IRelateData Members

        public object GetData(string key)
        {
            return _data[key];
        }

        #endregion


    }

    public interface IDrill
    {
        string DrillToReport { get;set;}
        string DrillToVouch { get;set;}
        event EventHandler ReportDrillDesigning;
        event EventHandler VouchDrillDesigning;
        void DesignReportDrill();
        void DesignVouchDrill();
    }
}
