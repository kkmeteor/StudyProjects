using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UFIDA.U8.UAP.Services.ReportElements;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    ///     将报表结构转换为DSL
    /// </summary>
    public class DslTransfer
    {
        /// <summary>
        ///     转换为DSL
        /// </summary>
        /// <param name="obj">报表对象</param>
        /// <returns>DSL字符串</returns>
        public static string TransferToTableSchema(object obj)
        {
            bool _isMutiReport = false;
            var dslsb = new StringBuilder();
            var cellList = new List<Cell>();
            var mobileReport = obj as MobileReport;
            var semirows = new SemiRows();
            var supperCellList = new List<SuperCell>();
            var childCellList = new List<Cell>();
            Hashtable ht = new Hashtable();
            string headerRow = "1";
            if (mobileReport != null)
            {
                Report report = mobileReport.Report;
                if (report != null)
                {
                    // 这里取pageTitle的一个对象来组成报表标题区域的值
                    Section section = report.Sections[SectionType.PageTitle];
                    foreach (object gridDetailCell in section.Cells)
                    {
                        var cell = gridDetailCell as Cell;
                        if (cell != null && cell.Visible)
                        {
                            if (cell is SuperLabel)
                            {
                                headerRow = "2";
                                _isMutiReport = true;
                                supperCellList.Add(new SuperCell(cell, 0, 0));
                            }
                            cellList.Add(gridDetailCell as Cell);
                            ht.Add(cell.Name, cell.Caption);
                        }
                    }
                    if (_isMutiReport) //如果出现多表头，则从Detail中取得子列头，并且进行拼接处理
                    {
                        cellList = new List<Cell>();
                        section = report.Sections[SectionType.Detail];
                        foreach (object gridDetailCell in section.Cells)
                        {
                            var cell = gridDetailCell as Cell;
                            if (cell.Visible)
                            {
                                if (ht.Contains(cell.Name))
                                    cell.Caption = ht[cell.Name].ToString();
                                cellList.Add(gridDetailCell as Cell);
                            }
                        }
                        foreach (SuperCell cell in supperCellList)
                        {
                            foreach (Cell gridDetailCell in cellList)
                            {
                                if (gridDetailCell.Super != null && gridDetailCell.Super.Name == cell.Cell.Name)
                                {
                                    cell.Span += 1;
                                    cell.ChildList.Add(gridDetailCell);
                                }
                            }
                        }
                    }
                    dslsb.Append(
                        string.Format(
                            @"<table id=""report"" width=""fill"" height=""fill"" bindfield=""sss"" column=""{0}"" onDownRefresh=""nextPage""",
                            cellList.Count));
                    dslsb.Append(
                        string.Format(
                            @" fixedColumn=""{0}"" headerRow=""{1}"" columnWidth=""60dp"" rowHeight=""50dp"" style=""default"">",
                            1, headerRow));
                    //dslsb.Append("\n");
                    dslsb.Append("<property>");
                    // 下面代码是用来生成报表列的样式

                    #region 详细描述行列

                    dslsb.Append("<columns>");
                    var sort = new SortedList();
                    foreach (Cell cell in cellList)
                    {
                        sort.Add(cell.Location.X, cell);
                    }

                    int i = 0;
                    foreach (Cell cell in sort.Values)
                    {
                        var tc = new TitleColumn(cell);
                        tc.Col = i.ToString();
                        dslsb.Append(tc.ColumnsToXmlFormat());
                        foreach (SuperCell superCell in supperCellList)
                        {
                            if (superCell.ChildList.Contains(cell))
                            {
                                if (superCell.Col > i || superCell.Col == 0)
                                    superCell.Col = i;
                            }
                        }
                        i++;
                    }
                    dslsb.Append("</columns>");
                    if (_isMutiReport)
                    {
                        dslsb.Append("<merges>");
                        bool isSucced = false;
                        //对每一个supper来说，col为该superCell的col值，而span为该superCell的子cell的个数
                        foreach (SuperCell superCell in supperCellList)
                        {
                            if (superCell.ChildList.Count != 0)
                            {
                                dslsb.Append(string.Format(@"<merge row=""0""  col=""{0}""  span=""{1}""/>",
                                                           superCell.Col,
                                                           superCell.Span));
                                isSucced = true;
                            }
                        }
                        dslsb.Append("</merges>");
                        if (!isSucced)
                        {
                            throw new Exception("该报表数据分组层级太多，移动端无法展现！");
                        }
                    }

                    #endregion

                    dslsb.Append("</property>");
                    dslsb.Append("</table>");
                    return dslsb.ToString();
                }
            }
            return string.Empty;
        }

        public static bool IsMutiReport(Report report)
        {
            Section section = report.Sections[SectionType.PageTitle];
            return section.Cells.Cast<object>().OfType<Cell>().Where(cell => cell.Visible).OfType<SuperLabel>().Any();
        }
    }

    /// <summary>
    ///     内部类，父标签
    /// </summary>
    internal class SuperCell
    {
        public SuperCell(Cell cell, int col, int span)
        {
            Cell = cell;
            Col = col;
            Span = span;
            if (ChildList == null)
                ChildList = new List<Cell>();
        }

        public Cell Cell { get; set; }
        public List<Cell> ChildList { get; set; }
        public int Col { get; set; }
        public int Span { get; set; }
    }
}
