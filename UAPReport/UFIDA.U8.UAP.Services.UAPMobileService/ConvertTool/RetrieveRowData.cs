using System.Collections.Generic;
using System.Linq;
using System.Text;
using UFIDA.U8.UAP.Services.ReportElements;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    ///     将Semirows对象转换为移动报表需要的DSL串
    /// </summary>
    internal class RetrieveRowData
    {
        /// <summary>
        ///     将Semirows对象转换为移动报表需要的DSL串
        /// </summary>
        /// <param name="obj">MobileReport对象</param>
        /// <returns></returns>
        public static string TransferToRowData(object obj)
        {
            bool isMutiReport = false;
            var rowBuilder = new StringBuilder();
            var cellList = new List<Cell>();
            var supperCellList = new List<Cell>();
            var childCellList = new List<Cell>();
            var mobileReport = obj as MobileReport;
            if (mobileReport == null)
            {
                return null;
            }
            Report report = mobileReport.Report;
            // 这里取pageTitle的一个对象来组成报表标题区域的值
            Section section = report.Sections[SectionType.PageTitle];
            foreach (object gridDetailCell in section.Cells)
            {
                var cell = gridDetailCell as Cell;
                if (cell == null || !cell.Visible)
                    continue;
                if (cell is SuperLabel)
                {
                    isMutiReport = true; //如果列头存在superLable类型，则为多列头格式报表
                    supperCellList.Add(cell);
                }
                cellList.Add(gridDetailCell as Cell);
            }
            // 下面代码生成报表数据和列的信息，列的显示名称和列的值都需要通过cell来描述
            // 如果该报表为简单单列头的报表，直接做出标题结构。
            if (!isMutiReport)
            {
                rowBuilder.Append("<row>");
                foreach (Cell cell in cellList)
                {
                    rowBuilder.Append(string.Format(@"<column>{0}</column>", cell.Caption));
                }
                rowBuilder.Append("</row>");
            }
            else //如果该报表属于多列头报表，首先从Detail中取得子列，并对列头进行特殊处理
            {
                cellList = new List<Cell>();
                section = report.Sections[SectionType.Detail];
                foreach (object gridDetailCell in section.Cells)
                {
                    var cell = gridDetailCell as Cell;
                    if (cell != null && cell.Visible)
                    {
                        cellList.Add(gridDetailCell as Cell);
                        childCellList.AddRange(from superCell in supperCellList
                                               where (cell.Super != null && cell.Super.Name == superCell.Name)
                                               select cell);
                    }
                }
                rowBuilder.Append("<row>");
                foreach (Cell cell in cellList)
                {
                    rowBuilder.Append(!childCellList.Contains(cell)
                                          ? string.Format(@"<column>{0}</column>", cell.Caption)
                                          : string.Format(@"<column>{0}</column>", cell.Super.Caption));
                }
                rowBuilder.Append("</row>");
                rowBuilder.Append("<row>");
                foreach (Cell cell in cellList)
                {
                    if (!childCellList.Contains(cell))
                        rowBuilder.Append(string.Format(@"<column>{0}</column>", string.Empty));
                    else
                    {
                        rowBuilder.Append(string.Format(@"<column>{0}</column>", cell.Caption));
                    }
                }
                rowBuilder.Append("</row>");
            }
            string columnsStr = GetColumnsInfoString(obj);
            string tempStr = TransferToRowData(columnsStr, mobileReport);
            rowBuilder.Append(tempStr);
            return rowBuilder.ToString();
        }


        /// <summary>
        ///     在分页加载时，通过移动前端传递过来的列头信息构造数据字符串
        /// </summary>
        /// <param name="columnsInfoString">移动前端传递过来的列头信息字符串</param>
        /// <param name="report">MobileReport对象</param>
        /// <returns>结果DSL串</returns>
        internal static string TransferToRowData(string columnsInfoString, MobileReport report)
        {
            var rowBuilder = new StringBuilder();
            var semirows = new SemiRows();
            if (report != null)
            {
                semirows = report.SemiRows;
            }
            string[] columns = columnsInfoString.Split(',');
            foreach (SemiRow semiRow in semirows)
            {
                rowBuilder.Append("<row>");
                if (semiRow.SectionType == SectionType.ReportSummary)
                    // 如果当前行是总计行
                {
                    rowBuilder.Append(@"<column>总计</column>");
                    for (int i = 0; i < columns.Length; i++)
                    {
                        if (i == 0) continue;
                        rowBuilder.Append(string.Format(@"<column>{0}</column>", semiRow[columns[i]]));
                    }
                }
                else
                    // 如果当前行是普通数据行
                {
                    foreach (string t in columns)
                    {
                        rowBuilder.Append(string.Format(@"<column>{0}</column>", semiRow[t]));
                    }
                }
                rowBuilder.Append("</row>");
            }
            return rowBuilder.ToString();
        }

        /// <summary>
        ///     第一次加载报表对象时将报表列头信息缓存起来
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetColumnsInfoString(object obj)
        {
            var rowBuilder = new StringBuilder();
            var cellList = new List<Cell>();
            var mobileReport = obj as MobileReport;
            if (mobileReport != null)
            {
                Report report = mobileReport.Report;
                // 这里取pageTitle的一个对象来组成报表标题区域的值
                Section section = report.Sections[SectionType.PageTitle];
                if (DslTransfer.IsMutiReport(report))
                    section = report.Sections[SectionType.Detail];
                foreach (object gridDetailCell in section.Cells)
                {
                    var cell = gridDetailCell as Cell;
                    if (cell != null && cell.Visible)
                    {
                        cellList.Add(gridDetailCell as Cell);
                    }
                }
            }
            foreach (Cell cell in cellList)
            {
                rowBuilder.Append(cell.Name);
                rowBuilder.Append(",");
            }
            rowBuilder.Remove(rowBuilder.Length - 1, 1);
            return rowBuilder.ToString();
        }
    }
}
