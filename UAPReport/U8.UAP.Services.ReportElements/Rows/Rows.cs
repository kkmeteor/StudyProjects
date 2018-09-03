using System;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Rows 的摘要说明。
	/// </summary>
	[Serializable]
	public class Rows : CollectionBase,IDisposable
	{
        private bool _bautoheight = false;
        private int _minordetailcount = 0;
        private int _printsummarycount = 0;
        private bool _nogroupheader = false;
        //disply
        [NonSerialized]
        private SimpleHashtable _parentrows=new SimpleHashtable();
        public void AppendSimpleRows(Section section)
        {
            if (section.SectionLines.Count == 0)
                section.AsignToSectionLines();
            foreach (SectionLine sl in section.SectionLines)
            {
                if (sl.Cells.Count > 0)
                    Add(new Row(sl));
            }
        }

        public void AppendFrom(Report report, SemiRows semirows)
        {
            ShowStyle ss = HandleNoGroupHeader(report,ShowStyle.NoGroupHeader);            
            AppendSemiRows(report, semirows, ss , null);
        }

        public void AppendFrom(Report report, SemiRowsContainer semirowscontainer)
        {
            ShowStyle ss = HandleNoGroupHeader(report, ShowStyle.NoGroupHeader);        
            SemiRows semirows = semirowscontainer.GetABlock();
            AppendSemiRows(report, semirows, ss, null);
        }

        public void AppendFrom(Report report, SemiRowsContainer semirowscontainer, ShowStyle showstyle)
        {
            SemiRows semirows = semirowscontainer.GetABlock();
            showstyle  = HandleNoGroupHeader(report, showstyle );        
            AppendSemiRows(report, semirows, showstyle,null);
        }

        public void AppendFrom(Section section)
        {
            AppendFrom(section, null);
        }

        public void AppendFrom(Section section,int containerwidth)
        {
            AppendFrom(section, null,containerwidth );
        }

        public void AppendFrom(Section section, System.Drawing.Graphics g)
        {
            AppendFrom(section, g, -1);
        }

        //print
        public void AppendFrom(Section section, System.Drawing.Graphics g,int containerwidth)
        {
            if (section == null)
                return;
            foreach (SectionLine sl in section.SectionLines)
            {
                if (sl.Cells.Count > 0)
                    Add(new Row(sl, g,containerwidth ));
            }
        }
        
        public void AppendFromWait(Report report,SemiRowsContainer semirowscontainer,ShowStyle showstyle,System.Drawing.Graphics g)
        {
            SemiRows semirows = semirowscontainer.GetABlockWait();
            showstyle = HandleNoGroupHeader(report, showstyle); 
            AppendSemiRows(report, semirows,showstyle  ,g );//report.FreeViewStyle == FreeViewStyle.MergeCell ?ShowStyle.NoGroupHeader :ShowStyle.Normal
        }

        private ShowStyle  HandleNoGroupHeader(Report report,ShowStyle showstyle)
        {
            if (showstyle  == ShowStyle.NoGroupHeader)
            {
                _nogroupheader = true;
                GroupSummary gs = report.Sections.GetGroupSummary(1);
                if (gs == null || gs.Cells.Count == 0)
                {
                    _nogroupheader = false;
                    showstyle  = ShowStyle.Normal;
                }
            }
            return showstyle;
        }

        private void AppendSemiRows(Report report,SemiRows semirows,ShowStyle showstyle,System.Drawing.Graphics g)
        {
            if (semirows != null)
            {
                Sections sections = report.Sections;
                Detail detail = sections[SectionType.Detail] as Detail;
                int grouplevels=report.GroupLevels;
                foreach (SemiRow semirow in semirows)
                {
                    Section section = null;
                    Section section1 = null;
                    switch (semirow.SectionType)
                    {
                        case SectionType.GroupHeader:
                            section = sections.GetGroupHeader(semirow.Level);
                            if ((!report.bShowDetail || detail.Cells.Count == 0) && semirow.Level == grouplevels)//detail.Cells.Count == 0
                            {
                                ApplyDetailStyle(section, _minordetailcount, detail, (detail as IAlternativeStyle).bApplyAlternative);
                                _minordetailcount++;
                            }
                            else
                            {
                                _minordetailcount = 0;
                                //if (showstyle == ShowStyle.NoGroupHeader)
                                //    ApplyDetailStyle(section, 0, detail, false);
                            }
                            break;
                        case SectionType.GroupSummary:
                            section = sections.GetGroupSummary(semirow.Level);
                            if ((!report.bShowDetail || detail.Cells.Count == 0) && semirow.Level == grouplevels)
                            {
                                _minordetailcount++;
                                ApplyDetailStyle(section, _minordetailcount, detail, (detail as IAlternativeStyle).bApplyAlternative);
                                _minordetailcount++;
                            }
                            else
                            {
                                _minordetailcount = 0;
                                //if (showstyle == ShowStyle.NoGroupHeader)
                                //    ApplyDetailStyleToGroupLabel(section, detail);
                            }
                            break;
                        case SectionType.ReportSummary:
                            section = sections[SectionType.ReportSummary];
                            break;
                        default:
                            if (report.bShowDetail)
                            {
                                section = detail;
                                (section as IAlternativeStyle).bApplySecondStyle = (_minordetailcount % 2 == 1);
                            }
                            else
                            {
                                section = sections.GetGroupHeader(semirow.Level);
                                ApplyDetailStyle(section, _minordetailcount, detail, (detail as IAlternativeStyle).bApplyAlternative);
                                section1 = sections.GetGroupSummary(semirow.Level);
                                ApplyDetailStyle(section1, _minordetailcount, detail, (detail as IAlternativeStyle).bApplyAlternative);
                            }
                            _minordetailcount++;
                            break;
                    }
                    AppendFrom(section, semirow,showstyle ,report.BaseID ,g);
                    AppendFrom(section1, semirow,showstyle ,report.BaseID,g );
                }
            }
        }

        private void ApplyDetailStyleToGroupLabel(Section section, Detail detail)
        {
            if (section != null)
            {
                (section as IAlternativeStyle).bApplyAlternative = false;
                (section as IAlternativeStyle).bApplySecondStyle = false;
                if (!(section as IAlternativeStyle).bAlreadySetSecondStyle)
                {
                    (section as IAlternativeStyle).bAlreadySetSecondStyle = true;
                    foreach (Cell cell in section.Cells)
                    {
                        if (cell is CommonLabel && (cell as ILabelType).LabelType == LabelType.GroupLabel)
                        {
                            cell.SetBackColor(detail.BackColor);
                            cell.SetForeColor(detail.ForeColor);
                            //cell.SetBorderColor(detail.BorderColor);
                            cell.ServerFont = detail.ServerFont ;
                        }
                    }
                }
            }
        }

        private void ApplyDetailStyle(Section section, int minorcount, Detail detail, bool bapplyalternative)
        {
            if (section != null)
            {
                (section as IAlternativeStyle).bApplyAlternative = bapplyalternative;
                (section as IAlternativeStyle).bApplySecondStyle = (minorcount % 2 == 1);
                if (!(section as IAlternativeStyle).bAlreadySetSecondStyle)
                {
                    (section as IAlternativeStyle).bAlreadySetSecondStyle = true;
                    section.BackColor = detail.BackColor;
                    //section.BorderColor = detail.BorderColor;
                    section.ForeColor = detail.ForeColor;
                    section.ServerFont = detail.ServerFont;
                    section.ApplyColorStyle();

                    if (bapplyalternative)
                    {
                        (section as IAlternativeStyle).BackColor2 = detail.BackColor2;
                        //(section as IAlternativeStyle).BorderColor2 = detail.BorderColor2 ;
                        (section as IAlternativeStyle).ForeColor2 = detail.ForeColor2;
                        //(section as IAlternativeStyle).ServerFont2.From(detail.ServerFont2);
                    }
                }
            }
        }

        private void AppendFrom(Section section, SemiRow semirow,ShowStyle showstyle,string baseid,System.Drawing.Graphics g)
        {
            if (section == null || (_nogroupheader && section.Type=="GroupHeader"))
                return;
            foreach (SectionLine sl in section.SectionLines)
            {
                if (sl.Cells.Count > 0)
                {
                    if (section is GroupHeader)
                    {
                        GroupHeaderRow row = new GroupHeaderRow(sl, semirow,g);
                        SetRowState(showstyle, row);
                        _parentrows.Add(row.Level.ToString(), row);
                        if (row.Level > 1)
                            row.ParentRow = _parentrows[Convert.ToString(row.Level - 1)] as GroupHeaderRow;
                        Add(row);
                    }
                    else if (section is GroupSummary)
                    {
                        GroupRow row = new GroupRow(sl, semirow,g);
                        SetRowState(showstyle, row);
                        row.ParentRow = _parentrows[Convert.ToString(row.Level)] as GroupHeaderRow;
                        Add(row);
                    }
                    else
                    {
                        Row row = new Row(sl, semirow,baseid,g );
                        SetRowState(showstyle, row);
                        row.ParentRow = _parentrows[Convert.ToString(row.Level-1)] as GroupHeaderRow;
                        Add(row);
                    }
                }
            }
        }

        private void SetRowState(ShowStyle showstyle,Row row)
        {
            if (showstyle == ShowStyle.NoGroupSummary)
                row.RowState = GroupHeaderRowStates.Expanded;
            else if (showstyle == ShowStyle.Normal)
                row.RowState = GroupHeaderRowStates.Normal;
            else
                row.RowState = GroupHeaderRowStates.Classical;
        }
        protected override void OnClear()
        {
            base.OnClear();
            _printsummarycount = 0;
            _minordetailcount = 0;
        }

		public Row this[int index]
		{
			get {  return List[index] as Row;  }
		}

        public bool bAutoHeight
        {
            set
            {
                _bautoheight = value;
                _nogroupheader = true;
            }
        }

		public void Add( Row value )  
		{
            if (_bautoheight)
                value.bAutoHeight = true;

            if (value.InArea == "PrintPageSummary")
            {
                _printsummarycount++;
                List.Add(value);
            }
            else if (_printsummarycount == 0)
                List.Add(value);
            else
                Insert(Count - _printsummarycount, value);
		}

		public int IndexOf( Row value )  
		{
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, Row value )  
		{
            if (_bautoheight)
                value.bAutoHeight = true;
			List.Insert( index, value );
		}

		public void Remove( Row value )  
		{
			List.Remove( value );
		}

		public bool Contains( Row value )  
		{
			return( List.Contains( value ) );
		}

		#region IDisposable 成员

		public void Dispose()
		{
			foreach(Row value in List)
			{
				if(value!=null)
					value.Dispose();
			}
			this.Clear();
		}

		#endregion
	}
}
