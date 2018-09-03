using System.Collections.Generic;
using System.Linq;
using System.Text;
using UFIDA.U8.UAP.Services.ReportElements;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    ///     将Semirows对象转换为移动报表需要的DSL串
    /// </summary>
    internal class RetrieveRowData2Json
    {
        /// <summary>
        ///     将Semirows对象转换为U易联移动报表需要的Json串
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
                rowBuilder.Append("{\"columns\":");
                rowBuilder.Append("[");
                foreach (Cell cell in cellList)
                {
                    rowBuilder.Append("{");
                    rowBuilder.Append("\"title\":");
                    rowBuilder.Append(string.Format("\"{0}\"", cell.Caption.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")));
                    rowBuilder.Append(",");
                    rowBuilder.Append("\"field\":");
                    rowBuilder.Append(string.Format("\"{0}\"", cell.Name));

                    rowBuilder.Append(",");
                    rowBuilder.Append("\"locked\":");
                    rowBuilder.Append("false");
                    rowBuilder.Append(",");
                    rowBuilder.Append("\"width\":");
                    rowBuilder.Append(string.Format("\"{0}px\"", cell.Width));
                    rowBuilder.Append("},");
                }
                rowBuilder.Remove(rowBuilder.Length - 1, 1);
                rowBuilder.Append("],");
            }
            else //如果该报表属于多列头报表，首先从Detail中取得子列，并对列头进行特殊处理
            {
                var groupCells = new Cells();
                if(report.GroupLevels>0)
                {
                    groupCells =  report.Sections.GetGroupHeader(report.GroupLevels).Cells;
                }

                cellList = new List<Cell>();
                var childCellList4delete = new List<Cell>();
                Dictionary<string, List<Cell>> dicCell = new Dictionary<string, List<Cell>>();
                section = report.Sections[SectionType.Detail];
                foreach (object gridDetailCell in section.Cells)
                {
                    var cell = gridDetailCell as Cell;
                    if (cell != null && cell.Visible)
                    {
                        cellList.Add(gridDetailCell as Cell);
                        foreach (Cell supperCell in supperCellList)
                        {
                            if (cell.Super != null && cell.Super.Name == supperCell.Name)
                            {
                                if (dicCell.ContainsKey(supperCell.Name))
                                {
                                    dicCell[supperCell.Name].Add(cell);
                                }
                                else
                                {
                                    var list = new List<Cell>();
                                    list.Add(cell);
                                    dicCell.Add(supperCell.Name, list);
                                }
                                childCellList4delete.Add(cell);
                            }
                        }
                        childCellList.AddRange(from superCell in supperCellList
                                               where (cell.Super != null && cell.Super.Name == superCell.Name)
                                               select cell);
                    }
                }
                rowBuilder.Append("{\"columns\":");
                rowBuilder.Append("[");
                foreach (Cell cell in cellList)
                {
                    if (string.IsNullOrEmpty(cell.Caption))
                    {
                        foreach (Cell groupcell in groupCells)
                        {
                            if (groupcell.Name == cell.Name)
                                cell.Caption = groupcell.Caption;
                        }
                    }
                    if (childCellList.Contains(cell) && dicCell.ContainsKey(cell.Super.Name))
                    {
                        rowBuilder.Append("{");
                        rowBuilder.Append("\"title\":");
                        rowBuilder.Append(string.Format("\"{0}\",", cell.Super.Caption.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")));
                        rowBuilder.Append("\"columns\":");
                        rowBuilder.Append("[");
                        foreach (var childCell in dicCell[cell.Super.Name])
                        {
                            rowBuilder.Append("{");
                            rowBuilder.Append("\"title\":");
                            rowBuilder.Append(string.Format("\"{0}\"", childCell.Caption.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")));
                            rowBuilder.Append(",");
                            rowBuilder.Append("\"field\":");
                            rowBuilder.Append(string.Format("\"{0}\"", childCell.Name));

                            rowBuilder.Append(",");
                            rowBuilder.Append("\"locked\":");
                            rowBuilder.Append("false");
                            rowBuilder.Append(",");
                            rowBuilder.Append("\"width\":");
                            rowBuilder.Append(string.Format("\"{0}px\"", childCell.Width));
                            rowBuilder.Append("},");
                        }
                        rowBuilder.Remove(rowBuilder.Length - 1, 1);
                        rowBuilder.Append("]");
                        rowBuilder.Append("},");
                        if (dicCell.ContainsKey(cell.Super.Name))
                        {
                            dicCell.Remove(cell.Super.Name);

                        }
                    }
                    else
                    {
                        if (childCellList4delete.Contains(cell))
                            continue;
                        rowBuilder.Append("{");
                        rowBuilder.Append("\"title\":");
                        rowBuilder.Append(string.Format("\"{0}\"", cell.Caption.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")));
                        rowBuilder.Append(",");
                        rowBuilder.Append("\"field\":");
                        rowBuilder.Append(string.Format("\"{0}\"", cell.Name));
                        rowBuilder.Append(",");
                        rowBuilder.Append("\"locked\":");
                        rowBuilder.Append("false");
                        rowBuilder.Append(",");
                        rowBuilder.Append("\"width\":");
                        rowBuilder.Append(string.Format("\"{0}px\"", cell.Width));
                        rowBuilder.Append("},");
                    }
                }
                rowBuilder.Remove(rowBuilder.Length - 1, 1);
                rowBuilder.Append("],");
            }
            string columnsStr = GetColumnsInfoString(obj);
            string tempStr = TransferToRowDataJson(columnsStr, mobileReport);
            rowBuilder.Append(tempStr);
            return rowBuilder.ToString();
        }


        /// <summary>
        ///     在分页加载时，通过移动前端传递过来的列头信息构造数据字符串
        /// </summary>
        /// <param name="columnsInfoString">移动前端传递过来的列头信息字符串</param>
        /// <param name="report">MobileReport对象</param>
        /// <returns>结果DSL串</returns>
        internal static string TransferToRowDataJson(string columnsInfoString, MobileReport report)
        {
            var rowBuilder = new StringBuilder();
            var semirows = new SemiRows();
            if (report != null)
            {
                semirows = report.SemiRows;
            }
            string[] columns = columnsInfoString.Split(',');
            rowBuilder.Append("\"data\":[");
            int j = 0;
            foreach (SemiRow semiRow in semirows)
            {
                rowBuilder.Append("{\"RowNumber\":");
                rowBuilder.Append(string.Format("\"{0}\",", j.ToString()));
                //rowBuilder.Append(string.Format("{\"RowNumber\":\"{0}\"", j.ToString()));
                if (semiRow.SectionType == SectionType.ReportSummary)
                // 如果当前行是总计行
                {
                    rowBuilder.Append("\"level\":\"0\",");
                    for (int i = 0; i < columns.Length; i++)
                    {
                        rowBuilder.Append(string.Format("\"{0}\":", columns[i].ToString()));
                        if (i == 0)
                        {
                            rowBuilder.Append("\"总计\",");
                            continue;
                        }
                        if (semiRow.Contains(columns[i]))
                            rowBuilder.Append(string.Format("\"{0}\",", (semiRow[columns[i]].ToString()).Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\"", "")));
                        else
                            rowBuilder.Append("\"\",");
                        //rowBuilder.Append(string.Format("\"{0}\":\"{1}\"", columns[i].ToString(), semiRow[columns[i]]));
                    }
                    rowBuilder.Remove(rowBuilder.Length - 1, 1);
                    rowBuilder.Append("},");
                }
                else
                // 如果当前行是普通数据行
                {
                    foreach (string t in columns)
                    {
                        if (semiRow.Contains(t))
                            rowBuilder.Append(string.Format("\"{0}\":\"{1}\",", t, semiRow[t].ToString().Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\"","\\\"")));
                        else
                            rowBuilder.Append(string.Format("\"{0}\":\"{1}\",", t, ""));
                    }

                    rowBuilder.Remove(rowBuilder.Length - 1, 1);
                    rowBuilder.Append("},");
                }
                //rowBuilder.Append("}");
                j++;
            }
            if (semirows.Count != 0)
                rowBuilder.Remove(rowBuilder.Length - 1, 1);
            rowBuilder.Append("]}");
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
