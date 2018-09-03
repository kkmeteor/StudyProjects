/*
 * ����:¬����
 * ʱ��:2007.8.20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// print
	/// </summary>
	partial class Report
    {
		//#region visible
		///// <summary>
		///// ��ǰ���ɼ��ķ�����,���������
		///// </summary>
		//private ArrayList _unVisibleColumnsGroup = new ArrayList();
		
		///// <summary>
		///// ������������ڲ��ɼ��е�ֵ�Թ�ϵ
		///// </summary>
		//private Hashtable _sectionAndUnVisibleColumnMap = new Hashtable();
		//#region for print
		//public void RefreshCellVisibleStateFromGroupSchemaWhenPrint()
		//{
		//    if (this.Type == ReportType.FreeReport)
		//        return;
		//    ShowStyle style = this.CurrentSchema.ShowStyle;
		//    this.CurrentSchema.ShowStyle= ShowStyle.NoGroupHeader;
		//    try
		//    {
		//        int grouplevels = this.GroupLevels;
		//        for (int i = 1; i <= grouplevels; i++)
		//        {
		//            Section gh = this.Sections.GetGroupHeader(i);
		//            foreach (Cell cell in gh.Cells)
		//            {
		//                foreach (Section s in this.Sections)
		//                {
		//                    Cell c = s.Cells[cell.Name];
		//                    if (c != null)
		//                    {
		//                        c.Width = cell.Width;
		//                        c.Visible = cell.Visible;
		//                    }
		//                }
		//            }
		//        }
		//        Section detail = this.Sections[SectionType.Detail];
		//        if (detail != null)
		//        {
		//            foreach (Cell cell in detail.Cells)
		//            {
		//                foreach (Section s in this.Sections)
		//                {
		//                    Cell c = s.Cells[cell.Name];
		//                    if (c != null)
		//                    {
		//                        c.Width = cell.Width;
		//                        c.Visible = cell.Visible;
		//                    }
		//                }
		//            }
		//        }
		//    }
		//    finally
		//    {
		//        this.CurrentSchema.ShowStyle = style;
		//    }
		//}
		//#endregion

		///// <summary>
		///// ���ݷ�����Ϣ�����õ���Ŀ�ɼ���ˢ��Report�����״̬
		///// �˷���Ӧ��ֻ���ڱ�񱨱��ʱ�����.���ò��ɼ��еĹ���:
		///// 1.���ȸ��ݸ�������ԵĲ��ɼ��м������ø������еĲ��ɼ���,
		///// ͬʱ�ռ����ɼ��ķ�����ͷ��������;
		///// 2.Ȼ��ͬ�������������벻�ɼ��ķ�����ͬ����Cell�Ŀɼ���,
		///// �Լ�ƽ��չ��ʱ,���������Ŀɼ�������ϸ��������Ϊ׼
		///// </summary>
		//public void RefreshCellVisibleStateFromGroupSchema()
		//{
		//    // �����ǰ������Ϣ����ͨ����Ŀ���ñ����,
		//    // ��ȡ���ʱ�Ŀɼ������
		//    if( !this.CurrentSchema.IsSavedByColumnSetting )
		//        return;
		//    this._unVisibleColumnsGroup.Clear();
		//    this._sectionAndUnVisibleColumnMap.Clear();
		//    this.CollectInfoToArrays();
		//    foreach( Section sec in this._sectionAndUnVisibleColumnMap.Keys )
		//    {
		//        this.RefreshSectionCellVisibleState(
		//            sec,
		//            this._sectionAndUnVisibleColumnMap[sec] as SimpleArrayList );
		//    }
		//    this.SyncAllCellsWithSameName( this._unVisibleColumnsGroup );
		//}

		//private void SyncAllCellsWithSameName( ArrayList unvisibleColumns )
		//{ 
		//    foreach( string cellName in this._unVisibleColumnsGroup )
		//        foreach( Section sec in this.Sections )
		//            foreach( Cell cell in sec.Cells )
		//                if( cellName.ToLower() == cell.Name.ToLower())
		//                    cell.Visible = false;
		//}

		//private void CollectInfoToArrays()
		//{
		//    // ������ɼ���
		//    int levelsCount = this.GroupLevels;
		//    for( int i=0; i < levelsCount; i++ )
		//        this.AddInfoFromGroup( i+1 );

		//    // ��ϸ���ɼ���
		//    SimpleArrayList unvisibleCols = this.CurrentSchema.GetUnvisibleColumn( levelsCount +1 );
		//    Section sec = this.Sections[SectionType.Detail];
		//    this._sectionAndUnVisibleColumnMap.Add( sec, unvisibleCols );
		//    if( this.CurrentSchema.ShowStyle != ShowStyle.NoGroupSummary )
		//        foreach( string cellName in unvisibleCols )
		//            if( this.IsCalculator( cellName ) )
		//                this._unVisibleColumnsGroup.Add( cellName );
		//}

		//private void AddInfoFromGroup( int lev )
		//{ 
		//    SimpleArrayList unvisibleCols = this.CurrentSchema.GetUnvisibleColumn( lev );
		//    Section sec = this.Sections.GetGroupHeader( lev );
		//    if( sec != null )
		//    {
		//        foreach( string cellName in unvisibleCols )
		//            if( this.IsGroupItem( cellName, sec ) )
		//                this._unVisibleColumnsGroup.Add( cellName );
		//        this._sectionAndUnVisibleColumnMap.Add( sec, unvisibleCols );
		//    }
		//    sec = this.Sections.GetGroupSummary( lev );
		//    if( sec != null )
		//        this._sectionAndUnVisibleColumnMap.Add( sec, unvisibleCols );
			
		//}

		///// <summary>
		///// ����Ѿ���ʾ�������Ŀ,���չ��ʱ�еĿɼ�����ȫ
		///// �ɷ���Ԫ�����е�UnvisibleColumns����
		///// </summary>
		//private void RefreshSectionCellVisibleState(
		//    Section sec,
		//    SimpleArrayList unvisibleCols )
		//{
		//    if( sec != null )
		//    {
		//        foreach( Cell cell in sec.Cells )
		//        {
		//            cell.Visible = true;
		//            if (unvisibleCols != null
		//                && unvisibleCols.Contains(cell.Name))
		//                cell.Visible = false;
		//            this.SetPageTitleVisible( cell );
		//        }
		//    }
		//}

		///// <summary>
		///// ͬ������PageTitleͬ��Cell�Ŀɼ���
		///// </summary>
		//private void SetPageTitleVisible( Cell currentCell )
		//{
		//    Section pageTitleSection = this.Sections[SectionType.PageTitle];
		//    if( pageTitleSection != null )
		//    { 
		//        Cell pc = pageTitleSection.Cells[currentCell.Name];
		//        if( pc != null )
		//            pc.Visible = currentCell.Visible;
		//    }
		//}

		//private bool IsGroupItem( string cellName, Section secGroup )
		//{ 
		//    if( secGroup != null )
		//    {
		//        Cell cell = secGroup.Cells[cellName];
		//        if( cell != null && cell is IGroup )
		//                return true;
		//    }
		//    return false;
		//}

		//private bool IsCalculator( string cellName )
		//{ 
		//    int levelsCount = this.GroupLevels;
		//    for (int i = 1; i <= levelsCount +1; i++)
		//    {
		//        Section sec = this.Sections.GetGroupSummary( i+1 );
		//        if( sec != null )
		//        {
		//            Cell cell = sec.Cells[cellName];
		//            if( cell != null && cell is ICalculator )
		//                return true;
		//        }
		//    }
		//    return false;
		//}
		//#endregion

        #region unit helper
        private int InchesOfHandredToPixels(int inches, Graphics g)
        {
            return Convert.ToInt32((float)inches / (float)100 * g.DpiY );
        }

        private int PixelsToInchesOfHandred(int pixels, Graphics g)
        {
            return Convert.ToInt32((float)pixels / g.DpiX * (float)100);
        }

        public void TransformFromInchOfHandredToPixel(Graphics g)
        {
            foreach (Section section in this.Sections)
            {
                section.AsignToSectionLines();
                if (!this.bFree  && bSectionNeedTransformUnit(section))
                {
                    foreach (Cell cell in section.Cells)
                        CellToPixel(cell, g);
                    section.Cells.CalcHeight();
                }
            }
            this.SetVisibleWidth();
        }

        private bool bSectionNeedTransformUnit(Section section)
        {
            return section.SectionType == SectionType.Detail ||
                section.SectionType == SectionType.GroupHeader ||
                section.SectionType == SectionType.GroupSummary ||
                section.SectionType == SectionType.PageTitle ||
                section.SectionType == SectionType.ReportSummary;
        }

        private void CellToPixel(Cell cell, Graphics g)
        {
            cell.X = InchesOfHandredToPixels(cell.X,g);
            cell.SetY(InchesOfHandredToPixels(cell.Y,g));
            cell.Width = InchesOfHandredToPixels(cell.Width,g);
            cell.Height = InchesOfHandredToPixels(cell.Height,g);
            if (cell is SuperLabel)
            {
                foreach (UFIDA.U8.UAP.Services.ReportElements.Label label in (cell as SuperLabel).Labels)
                {
                    CellToPixel(label,g);
                }
            }
        }

        public void TransformFromPixelToInchOfHandred(Graphics g)
        {
			//if (this.Type != ReportType.FreeReport)
			//    RefreshCellVisibleStateFromGroupSchemaWhenPrint();
            foreach (Section section in this.Sections)
            {
                section.AsignToSectionLines();
                if (!this.bFree  && bSectionNeedTransformUnit(section))
                {
                    foreach (Cell cell in section.Cells)
                        CellToInchesOfHandred(cell, g);
                    section.Cells.CalcHeight();
                }
            }
            this.SetVisibleWidth();
        }

        private void CellToInchesOfHandred(Cell cell, Graphics g)
        {
            cell.X = PixelsToInchesOfHandred(cell.X,g);
            cell.SetY(PixelsToInchesOfHandred(cell.Y, g));
            cell.Width = PixelsToInchesOfHandred(cell.Width, g);
            cell.Height = PixelsToInchesOfHandred(cell.Height, g);
            if (cell is SuperLabel)
            {
                foreach (UFIDA.U8.UAP.Services.ReportElements.Label label in (cell as SuperLabel).Labels)
                {
                    CellToInchesOfHandred(label, g);
                }
            }
        }
        #endregion
    }
}
