using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.IO;
using UFIDA.U8.UAP.Services.ReportResource;
using Infragistics.UltraChart.Design;
using Infragistics.UltraChart.Resources.Appearance;
using Infragistics.UltraChart.Shared.Styles;
using System.Reflection;
using Infragistics.UltraChart.Resources;
using Infragistics.Win.UltraWinChart;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class ChartService
    {
        protected Report _report;

        public ChartService(Report report)
        {
            _report = report;
        }

        public string DefaultID(int level)
        {
            return GetSchemaAmongBy(level, null).ID;
        }

        public ChartSchemaItemAmong GetSchemaAmongBy(int level, string id)
        {
            return _report.ChartSchemas.CurrentGroupChart[level, id];
        }

        public ChartSchemaItemAmong AddASchema(int level)
        {
            return _report.ChartSchemas.CurrentGroupChart.AddAChart(level);
        }

        public ChartSchemaItemAmong AddASchema(int level, string id)
        {
            return _report.ChartSchemas.CurrentGroupChart.AddAChart(level, id);
        }

        #region prepair design
        public void InitializeChartWizard(int level, string id, ChartWizardPane chartwizard)
        {
            Hashtable ht = new Hashtable();
            Hashtable hts = new Hashtable();
            ArrayList alsource = new ArrayList();


            ChartSchemaItemAmong among = _report.ChartSchemas.CurrentGroupChart[level, id];
            int bdtgroup = InitDataSourceList(ht, alsource, among.Source, level, hts);

            //是否当前分组第一次创建
            if (((ChartSchemaItem)_report.ChartSchemas.CurrentGroupChart.Items[level]).Count == 1
                && string.IsNullOrEmpty(among.ChartXml))
            {
                chartwizard.FirstCurrentLevelNewSchema = true;
            }
            else
            {
                chartwizard.FirstCurrentLevelNewSchema = false;
            }
            if (!string.IsNullOrEmpty(among.ChartXml))
            {
                chartwizard.bNewSchema = false;
                using (StringReader sR = new StringReader(among.ChartXml))
                {
                    (chartwizard.Component as Infragistics.Win.UltraWinChart.UltraChart).LoadPreset(sR, true);
                }
            }
            else
            {
                chartwizard.bNewSchema = true;
            }
            chartwizard.OrderType = among.OrderType;
            chartwizard.TopRank = among.TopRank;

            chartwizard.SetDataSourceList(ht, alsource, bdtgroup, hts);
        }
        private void AddToHash(Hashtable ht, Cell cell, ArrayList alname, ArrayList alsource, Hashtable hts)
        {
            if (string.IsNullOrEmpty(cell.Caption.Trim()))
                return;
            if (!ht.Contains(cell.Caption))
            {
                if ((cell is Calculator && (cell as Calculator).Operator != OperatorType.AccumulateSUM &&
                    (cell as Calculator).Operator != OperatorType.BalanceSUM))
                {
                    ht.Add(cell.Caption, null);
                    hts.Add(ht.Count, cell.Caption);
                }
                else if (cell is AlgorithmCalculator)
                {
                    if (cell is IBDateTime && (cell as IBDateTime).bDateTime)
                    {
                        ht.Add(cell.Caption, "datetime");
                        hts.Add(ht.Count, cell.Caption);
                    }
                    else if (cell is DecimalAlgorithmCalculator)
                    {
                        ht.Add(cell.Caption, null);
                        hts.Add(ht.Count, cell.Caption);
                    }
                }
            }
            if (alname != null && alname.Contains(cell.Name.ToLower()))
                alsource.Add(cell.Caption);
        }
        private void AddToHash(Hashtable ht, Cell cell, ArrayList alname, ArrayList alsource)
        {
            if (string.IsNullOrEmpty(cell.Caption.Trim()))
                return;
            if (!ht.Contains(cell.Caption))
            {
                if ((cell is Calculator && (cell as Calculator).Operator != OperatorType.AccumulateSUM &&
                    (cell as Calculator).Operator != OperatorType.BalanceSUM))
                {
                    ht.Add(cell.Caption, null);
                }
                else if (cell is AlgorithmCalculator)
                {
                    if (cell is IBDateTime && (cell as IBDateTime).bDateTime)
                        ht.Add(cell.Caption, "datetime");
                    else if (cell is DecimalAlgorithmCalculator)
                        ht.Add(cell.Caption, null);
                }
            }
            if (alname != null && alname.Contains(cell.Name.ToLower()))
                alsource.Add(cell.Caption);
        }

        protected virtual int InitDataSourceList(Hashtable ht, ArrayList alsource, ArrayList alname, int level, Hashtable hts)
        {
            bool bdtgroup = false;
            GroupHeader gh = _report.Sections.GetGroupHeader(level);
            GroupSummary gs = _report.Sections.GetGroupSummary(level);
            Cell groupcell = null;
            if (gh != null)
            {
                foreach (Cell cell in gh.Cells)
                {
                    if (cell is IGroup)
                    {
                        if (groupcell != null)
                        {
                            groupcell = null;
                            break;
                        }
                        else
                            groupcell = cell;
                    }
                }

                if (groupcell != null && (groupcell is IBDateTime) && (groupcell as IBDateTime).bDateTime)
                    bdtgroup = true;

                foreach (Cell cell in gh.Cells)
                {
                    AddToHash(ht, cell, alname, alsource, hts);
                }
            }
            if (gs != null)
            {
                foreach (Cell cell in gs.Cells)
                {
                    AddToHash(ht, cell, alname, alsource, hts);
                }
            }
            return bdtgroup ? 1 : 0;
        }
        #endregion

        #region design ok
        public void InitializeChart(int level, string id, Infragistics.Win.UltraWinChart.UltraChart mychat)
        {
            if (_report.ChartSchemas.CurrentGroupChart.Contains(level))
            {
                using (StringReader sR = new StringReader(_report.ChartSchemas.CurrentGroupChart[level, id].ChartXml))
                {
                    mychat.LoadPreset(sR, true);
                }
                ChartStyleHelper.InitChartWithSolidProperty(mychat);
            }
        }

        public void SaveSettings(ChartWizardPane chartwizard, ChartSchemaItemAmong among, int level)
        {
            string xml = "";
            using (StringWriter sW = new StringWriter())
            {
                (chartwizard.Component as Infragistics.Win.UltraWinChart.UltraChart).SavePreset(sW, "Preset", "Preset", Infragistics.Win.UltraWinChart.WinChartHelper.PresetType.All);
                xml = sW.ToString();
            }
            among.ChartXml = xml;
            among.Source = GetDataSourceList(level, chartwizard.GetDataSourceList());
            among.OrderType = chartwizard.OrderType;
            among.TopRank = chartwizard.TopRank;
        }

        protected virtual ArrayList GetDataSourceList(int level, ArrayList alsource)
        {
            ArrayList alname = new ArrayList();
            GroupHeader gh = _report.Sections.GetGroupHeader(level);
            GroupSummary gs = _report.Sections.GetGroupSummary(level);
            foreach (string key in alsource)
            {
                Cell cell = gh.Cells.GetByCaption(key);
                if (cell != null)
                    alname.Add(cell.Name.ToLower());
                else if (gs != null)
                {
                    cell = gs.Cells.GetByCaption(key);
                    if (cell != null)
                        alname.Add(cell.Name.ToLower());
                }
            }
            return alname;
        }
        #endregion

        #region chart data
        protected void FillToHashs(SimpleHashtable simplehash, SimpleHashtable complexhash, Cell cell, string columncaption)
        {
            if (cell.PrepaintEvent.Trim().Contains("if(currentgroup==null)".Trim()))
                cell.PrepaintEvent = "";
            if (!string.IsNullOrEmpty(cell.PrepaintEvent))
            {
                complexhash.Add(cell.Name, columncaption);
            }
            else if (cell is Calculator && (cell as Calculator).Operator == OperatorType.ExpressionSUM)
            {
                complexhash.Add(cell.Name, columncaption);
            }
            else if (cell is GridDecimal && (cell as GridDecimal).Operator == OperatorType.ExpressionSUM)
            {
                complexhash.Add(cell.Name, columncaption);
            }
            else if (cell is GridCalculateColumn && (cell as GridCalculateColumn).Operator == OperatorType.ExpressionSUM)
            {
                complexhash.Add(cell.Name, columncaption);
            }

            else if (cell is AlgorithmCalculator)
            {
                if (cell is IBDateTime && (cell as IBDateTime).bDateTime)
                {
                    complexhash.Add(cell.Name, "D___" + columncaption);
                }
                else if (cell is DecimalAlgorithmCalculator)
                {
                    complexhash.Add(cell.Name, columncaption);
                }
            }
            else if (cell is IMapName)
            {
                simplehash.Add((cell as IMapName).MapName, columncaption);
            }
        }
        void FindCellsToFill4SChartSource(SimpleHashtable simplehash, SimpleHashtable complexhash, int level, ArrayList alname, Cells cells)
        {
            int _currentlevel = level;
            //GroupHeader gh = _report.Sections.GetGroupHeader(_currentlevel);
            //GroupSummary gs = _report.Sections.GetGroupSummary(_currentlevel);

            ArrayList alsort = new ArrayList();
            foreach (string key in alname)
            {
                Cell cell = cells[key];
                if (cell != null)
                {
                    SortCell(alsort, cell);
                }
            }
            foreach (Cell cell in alsort)
                FillToHashs(simplehash, complexhash, cell, cell.Caption);
        }
        protected virtual void FindCellsToFill(SimpleHashtable simplehash, SimpleHashtable complexhash, int level, ArrayList alname)
        {
            int _currentlevel = level;
            GroupHeader gh = _report.Sections.GetGroupHeader(_currentlevel);
            GroupSummary gs = _report.Sections.GetGroupSummary(_currentlevel);
            ArrayList alsort = new ArrayList();
            foreach (string key in alname)
            {
                Cell cell = gh.Cells[key];
                if (cell != null)
                {
                    SortCell(alsort, cell);
                }
                else if (gs != null)
                {
                    cell = gs.Cells[key];
                    if (cell != null)
                    {
                        SortCell(alsort, cell);
                    }
                }
            }

            foreach (Cell cell in alsort)
                FillToHashs(simplehash, complexhash, cell, cell.Caption);
        }
        protected void SortCell(ArrayList al, Cell cell)
        {
            int count = al.Count;
            for (int i = 0; i < count; i++)
            {
                Cell c = al[i] as Cell;
                if (cell.CrossIndex < c.CrossIndex)
                {
                    al.Insert(i, cell);
                    return;
                }
            }
            al.Add(cell);
        }
        /// <summary>
        /// 原先的dependgroupdid==currentgoupid
        /// </summary>
        /// <param name="level"></param>
        /// <param name="id"></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual DataTable GetDataSource(int level, string id, object source, Infragistics.UltraChart.Shared.Styles.ChartType type)
        {
            int _currentlevel = level;
            RuntimeGroup group = source as RuntimeGroup;
            ChartSchemaItemAmong csi = _report.ChartSchemas.CurrentGroupChart[_currentlevel, id];
            ArrayList alname = csi.Source;
            SimpleHashtable simplehash = new SimpleHashtable();
            SimpleHashtable complexhash = new SimpleHashtable();


            FindCellsToFill(simplehash, complexhash, _currentlevel, alname);


            string wherestring = "";
            if (group != null)
                wherestring = group.GetFilterString();
            if (wherestring != "")
                wherestring = " where " + wherestring;

            DataTable dt = null;
            if (complexhash.Count > 0)
            {
                ReportEngine engine = DefaultConfigs.GetRemoteEngine(ClientReportContext.Login, ReportStates.Browse);
                dt = engine.ComplexChartData(_report.CacheID,
                    simplehash,
                    complexhash,
                    _currentlevel, wherestring);
                engine = null;
            }
            else
            {
                if (simplehash.Count == 0)
                    throw new Exception(U8ResService.GetResStringEx("U8.UAP.Report.ChartDataSourceRemoved"));
                StringBuilder sb = new StringBuilder();
                sb.Append("select ");
                if (csi.TopRank != 0)
                {
                    sb.Append(" top ");
                    sb.Append(csi.TopRank);
                    sb.Append(" ");
                }
                ArrayList altmp = _report.GroupStructure[_currentlevel] as ArrayList;
                foreach (string key in altmp)
                {
                    if (sb.Length > 7)
                        sb.Append("+ N' ' + ");

                    int bconvert = -1;
                    DataSource ds = _report.DataSources[key];
                    if (ds != null && ds.Type == DataType.DateTime)
                    {
                        sb.Append("convert(nvarchar(10),");
                        bconvert = 1;
                    }
                    else if (ds != null && ds.Type != DataType.String)
                    {
                        sb.Append("convert(nvarchar(100),");
                        bconvert = 2;
                    }

                    sb.Append("[");
                    sb.Append(key);
                    sb.Append("]");

                    if (bconvert == 1)
                        sb.Append(",120)");
                    else if (bconvert == 2)
                        sb.Append(")");
                }
                string orderkey = null;
                foreach (string key in simplehash.Keys)
                {
                    orderkey = simplehash[key].ToString();
                    sb.Append(",isnull([");
                    sb.Append(key);
                    sb.Append("],0)");
                    sb.Append(" as [");
                    sb.Append(simplehash[key].ToString());
                    sb.Append("]");
                }
                sb.Append("  from ");
                sb.Append(_report.BaseTableInTemp);
                sb.Append("_");
                sb.Append(_currentlevel.ToString());
                sb.Append(wherestring);
                if (csi.OrderType != 0)
                {
                    sb.Append(" order by [");
                    sb.Append(orderkey);
                    sb.Append(csi.OrderType == 1 ? "] asc " : "] desc ");
                }
                RemoteDataHelper rdh = null;
                if (_report.bWebOrOutU8)//ReportStates.WebBrowse
                    rdh = DefaultConfigs.GetRemoteHelper();
                else
                    rdh = new RemoteDataHelper();
                dt = rdh.GetChartData(ClientReportContext.Login.UfDataCnnString, sb.ToString(), System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            }

            CheckData(dt.Rows.Count, type);
            return dt;
        }

        protected bool bMaxMinOrAvg(OperatorType type)
        {
            return type == OperatorType.MAX || type == OperatorType.MIN || type == OperatorType.AVG;//|| type == OperatorType.AVG;被注释导致取平均列名重复 2011720被放开
        }

        protected void HandleMaxMinAvg(Cells cells)
        {
            int count = _report.Sections.Count - 1;
            #region handle max,min,avg
            int index = cells.Count - 1;
            while (index >= 0)
            {
                Cell cell = cells[index];
                if ((cell is IGridCollect && (cell as IGridCollect).bSummary && bMaxMinOrAvg((cell as IGridCollect).Operator)))
                {
                    if (cell is IDataSource)// || (cell is ICalculateColumn && (cell as IMapName).
                    {
                        if (cell is GridDecimal)
                        {
                            //string caption = GetCaptionBeforeChangeType(cell);
                            string caption = cell.Caption;
                            GridCalculateColumn cnew = new GridCalculateColumn(cell as GridDecimal);
                            cnew.Expression = cnew.Expression + "+ 0";
                            cnew.SetMapName(cnew.Name + "__MaxMinAvg");
                            cnew.Caption = caption;
                            cells.RemoveAt(index);
                            cells.Insert(index, cnew);
                        }
                    }
                }
                index--;
            }
            #endregion
        }

        protected void AddAggregateStrings(string op, string expression, string asname, int precision, bool bsingle, ref string minor, ref string upper)
        {
            #region minor aggregate string
            StringBuilder sbminor = new StringBuilder();
            sbminor.Append(minor);
            if (sbminor.Length > 0)
                sbminor.Append(",");
            if (expression != null)
            {
                if (!bsingle)
                {
                    if (!ExpressionService.bSpecialExpression(expression))
                    {
                        if (precision != -1)
                        {
                            sbminor.Append("Convert(Decimal(38,");
                            sbminor.Append(precision.ToString());
                            sbminor.Append("),");
                        }
                        sbminor.Append(ReportEngine.HandleExpression(_report.DataSources, expression, op + "(", true));

                        if (precision != -1)
                            sbminor.Append(")");
                        sbminor.Append(" as [");
                        sbminor.Append(asname);
                        sbminor.Append("]");
                    }
                    else
                    {
                        sbminor.Append("(");
                        sbminor.Append(op);
                        sbminor.Append("(");
                        if (precision != -1)
                        {
                            sbminor.Append("Convert(Decimal(38,");
                            sbminor.Append(precision.ToString());
                            sbminor.Append("),");
                        }

                        sbminor.Append(ReportEngine.HandleExpression(_report.DataSources, expression, "", op.ToLower() != "avg" ? true : false));

                        if (precision != -1)
                            sbminor.Append(")");
                        sbminor.Append(")");
                        sbminor.Append(") as [");
                        sbminor.Append(asname);
                        sbminor.Append("]");
                    }
                }
                else//single
                {
                    sbminor.Append("(");
                    sbminor.Append(op);
                    sbminor.Append("(");
                    if (precision != -1)
                    {
                        sbminor.Append("Convert(Decimal(38,");
                        sbminor.Append(precision.ToString());
                        sbminor.Append("),");
                    }

                    sbminor.Append("[");
                    sbminor.Append(expression);
                    sbminor.Append("]");

                    if (precision != -1)
                        sbminor.Append(")");
                    sbminor.Append(")");
                    sbminor.Append(") as [");
                    sbminor.Append(asname);
                    sbminor.Append("]");
                }
            }
            else
            {
                sbminor.Append("(");
                sbminor.Append(op);
                sbminor.Append("(");
                if (precision != -1)
                {
                    sbminor.Append("Convert(Decimal(38,");
                    sbminor.Append(precision.ToString());
                    sbminor.Append("),");
                }
                sbminor.Append("[");
                sbminor.Append(asname);
                sbminor.Append("]");
                if (precision != -1)
                    sbminor.Append(")");
                sbminor.Append(")");
                sbminor.Append(") as [");
                sbminor.Append(asname);
                sbminor.Append("]");
            }
            minor = sbminor.ToString();
            #endregion

            #region upper aggregate string
            sbminor = new StringBuilder();
            sbminor.Append(upper);
            if (sbminor.Length > 0)
                sbminor.Append(",");

            sbminor.Append("(");
            sbminor.Append(op);
            sbminor.Append("(");
            if (precision != -1)
            {
                sbminor.Append("Convert(Decimal(38,");
                sbminor.Append(precision.ToString());
                sbminor.Append("),");
            }
            sbminor.Append("[");
            sbminor.Append(asname);
            sbminor.Append("]");
            if (precision != -1)
                sbminor.Append(")");
            sbminor.Append(")");
            sbminor.Append(") as [");
            sbminor.Append(asname);
            sbminor.Append("]");

            upper = sbminor.ToString();
            #endregion
        }

        public virtual DataTable GetChartDataByChartID(string id, object source, Infragistics.UltraChart.Shared.Styles.ChartType type)
        {
            int _currentlevel = 1;
            RuntimeGroup group = source as RuntimeGroup;
            ChartSchemaItemAmong csi = _report.ChartSchemas.CurrentGroupChart[_currentlevel, id];
            ArrayList alname = csi.Source;
            SimpleHashtable simplehash = new SimpleHashtable();
            SimpleHashtable complexhash = new SimpleHashtable();


            Cells cells = _report.GridDetailCells.Clone() as Cells;
            HandleMaxMinAvg(cells);
            FindCellsToFill4SChartSource(simplehash, complexhash, _currentlevel, alname, cells);
            string minor = string.Empty;
            string upper = string.Empty;
            if (complexhash.Count > 0)
            {
                ArrayList calcuCells = new ArrayList();
                foreach (Cell c in cells)
                {
                    if (c is GridDecimal || c is GridAlgorithmColumn)
                    {
                        calcuCells.Add(c.Name);
                    }
                }
                simplehash = new SimpleHashtable();
                SimpleHashtable complexhash2 = new SimpleHashtable();
                FindCellsToFill4SChartSource(simplehash, complexhash2, _currentlevel, calcuCells, cells);
            }
            foreach (string key in simplehash.Keys)
            {
                Cell cell = cells.GetBySource(key);
                //取运行时期的(cell as ICalculator).Operator.ToString()，需要修改一下
                ICalculator ic = _report.RuntimeGetSummaryCell(cell.Name) as ICalculator;
                string oper = string.Empty;
                if (ic != null)
                    oper = ic.Operator.ToString();
                else oper = (cell as ICalculator).Operator.ToString();
                PrecisionHelper ph = null;
                string dataName = string.Empty;
                string express = null;
                bool bsingle = true;
                if (cell is IDataSource)
                {
                    dataName = (cell as IDataSource).DataSource.Name;
                }
                else if (cell is IMapName)
                {
                    dataName = (cell as IMapName).MapName;

                }
                if (cell is ICalculateColumn)
                {
                    express = (cell as ICalculateColumn).Expression;
                    bsingle = false;
                }
                ph = new PrecisionHelper(cell is IGroup, cell.Name, oper, dataName, express, bsingle, cell is IDecimal);


                int precision = -1;
                //if (_precisions.Contains(key))
                //    precision = Convert.ToInt32(_precisions[key]);
                AddAggregateStrings(ph.OperType, ph.Expression, ph.AsName, precision, ph.bSingleColumn, ref minor, ref upper);
            }
           
            string groupbyStr = string.Empty;
            GroupSchema groupSchema = _report.GroupSchemas[csi.DataDependGroupId];
            if (groupSchema == null)
                throw new Exception("当前图表依赖的分组不存在");
            string groupStr = this.GetGrouplevle1Str(groupSchema, ref groupbyStr);
            string gouplevle1Str = this.CreaeGroupLevel1Table(minor, groupStr, groupbyStr);

            string wherestring = "";
            if (group != null)
                wherestring = group.GetFilterString();
            if (wherestring != "")
                wherestring = " where " + wherestring;
            DataTable dt = null;


            if (complexhash.Count > 0)
            {
                ReportEngine engine = DefaultConfigs.GetRemoteEngine(ClientReportContext.Login, ReportStates.Browse);
                dt = engine.ComplexChartDataByOtherGroup(_report.CacheID,
                    simplehash,
                    complexhash,
                    _currentlevel, wherestring, gouplevle1Str, groupSchema, alname);
                engine = null;
            }
            else
            {
                if (simplehash.Count == 0)
                    throw new Exception(U8ResService.GetResStringEx("U8.UAP.Report.ChartDataSourceRemoved"));
                StringBuilder sb = new StringBuilder();
                sb.Append("select ");
                if (csi.TopRank != 0)
                {
                    sb.Append(" top ");
                    sb.Append(csi.TopRank);
                    sb.Append(" ");
                }
                SimpleArrayList altmp = groupSchema.SchemaItems[0].Items;
                foreach (string key in altmp)
                {
                    if (sb.Length > 7)
                        sb.Append("+ N' ' + ");

                    int bconvert = -1;
                    DataSource ds = _report.DataSources[key];
                    if (ds != null && ds.Type == DataType.DateTime)
                    {
                        sb.Append("convert(nvarchar(10),");
                        bconvert = 1;
                    }
                    else if (ds != null && ds.Type != DataType.String)
                    {
                        sb.Append("convert(nvarchar(100),");
                        bconvert = 2;
                    }

                    sb.Append("[");
                    sb.Append(key);
                    sb.Append("]");

                    if (bconvert == 1)
                        sb.Append(",120)");
                    else if (bconvert == 2)
                        sb.Append(")");
                }
                string orderkey = null;
                foreach (string key in simplehash.Keys)
                {
                    orderkey = simplehash[key].ToString();
                    sb.Append(",isnull([");
                    sb.Append(key);
                    sb.Append("],0)");
                    sb.Append(" as [");
                    sb.Append(simplehash[key].ToString());
                    sb.Append("]");
                }
                sb.Append("  from (");
                sb.Append(gouplevle1Str);
                sb.Append(" ) as group_1");
                //sb.Append("_");
                //sb.Append(_currentlevel.ToString());
                sb.Append(wherestring);
                if (csi.OrderType != 0)
                {
                    sb.Append(" order by [");
                    sb.Append(orderkey);
                    sb.Append(csi.OrderType == 1 ? "] asc " : "] desc ");
                }
                RemoteDataHelper rdh = null;
                if (_report.bWebOrOutU8)//ReportStates.WebBrowse
                    rdh = DefaultConfigs.GetRemoteHelper();
                else
                    rdh = new RemoteDataHelper();
                dt = rdh.GetChartData(ClientReportContext.Login.UfDataCnnString, sb.ToString(), System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            }
            CheckData(dt.Rows.Count, type);
            return dt;
        }

        #endregion

        #region 新图形问题
        int cellPointLength = 0;
        protected string CreaeGroupLevel1Table(string minor, string group, string groupby)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(group);
            if (!string.IsNullOrEmpty(minor))
                sb.Append(",");
            sb.Append(minor);
            string colpart2 = sb.ToString();

            sb = new StringBuilder();
            sb.Append("select ");
            string targetTableCols;
            //if (targetTableColss.Length > 1)
            targetTableCols = colpart2;

            sb.Append(targetTableCols);
            //sb.Append(" into ");
            //sb.Append("temp1");
            sb.Append(" from ");
            if (_report.Type == ReportType.CrossReport && _report.RealViewType != ReportType.CrossReport)//交叉方案
            {
                sb.Append(_report.CrossTable);
            }
            else
            {
                sb.Append(_report.BaseTableInTemp);
            }
            //sb.Append(_report.BaseTable);
            //sb.Append(" where 1 = 0 ");
            if (!string.IsNullOrEmpty(group))
            {
                sb.Append(" group by ");
                sb.Append(groupby);
            }
            return sb.ToString();
        }

        protected string GetGrouplevle1Str(GroupSchema group, ref string groupbyStr)
        {

            string haveGroup = string.Empty;
            foreach (string item in group.SchemaItems[0].Items)
            {
                Cell c = _report.GridDetailCells[item];
                if (c == null)
                    c = _report.GridDetailCells.GetBySource(item);
                if (c == null)
                    break;
                IGroup go = null;
                if (c is GroupDimension)
                {
                    go = new GroupObject(c as GroupDimension);
                    AddGroupString(ref haveGroup, null, (go as IMapName).MapName, ref groupbyStr);
                    cellPointLength = -1;//数字型不应该被格式化保持原数据
                }
                else if (c is CalculateGroupDimension)
                {
                    go = new CalculateGroupObject(c as CalculateGroupDimension);
                    AddGroupString(ref haveGroup, (go as ICalculateColumn).Expression, (go as IMapName).MapName, ref  groupbyStr);
                }
                else if (c is GridDateTime)
                {
                    go = new GroupObject(c as GridDateTime);
                    AddGroupString(ref haveGroup, null, (go as IMapName).MapName, ref groupbyStr);
                }
                else if (c is GridLabel)
                {
                    go = new GroupObject(c as GridLabel);
                    AddGroupString(ref haveGroup, null, (go as IMapName).MapName, ref groupbyStr);
                }
                else if (c is GridDecimal)
                {
                    go = new GroupObject(c as GridDecimal);
                    cellPointLength = (c as GridDecimal).PointLength;
                    AddGroupString(ref haveGroup, null, (go as IMapName).MapName, ref groupbyStr);
                }
                else if (c is GridColumnExpression)
                {
                    go = new CalculateGroupObject(c as GridColumnExpression);
                    AddGroupString(ref haveGroup, (go as ICalculateColumn).Expression, (go as IMapName).MapName, ref groupbyStr);
                }
                else if (c is GridCalculateColumn)
                {
                    go = new CalculateGroupObject(c as GridCalculateColumn);
                    AddGroupString(ref haveGroup, (go as ICalculateColumn).Expression, (go as IMapName).MapName, ref groupbyStr);
                }
            }
            return haveGroup;

        }

        private void AddGroupString(ref string haveGroup, string expression, string asname, ref string groupbyStr)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbnoas = new StringBuilder();
            DataSource ds = _report.DataSources[asname];

            if (!string.IsNullOrEmpty(haveGroup))
            {
                sb.Append(haveGroup);
                sb.Append(",");

                sbnoas.Append(groupbyStr);
                sbnoas.Append(",");
            }
            HandleLevel0String(ds, expression, asname, sb, sbnoas);
            haveGroup = sb.ToString();
            groupbyStr = sbnoas.ToString();
        }

        private void HandleLevel0String(DataSource ds, String expression, string asname, StringBuilder sb, StringBuilder sbnoas)
        {
            if (expression != null)
            {
                string ep = ReportEngine.HandleExpression(_report.DataSources, expression, "", false);
                if (ds != null && ds.IsADecimal)
                {
                    sb.Append(ep);
                    sb.Append(" as ");
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append(ep);
                }
                else
                {
                    sb.Append("isnull(");
                    sb.Append(ep);
                    sb.Append(",'') as ");
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append("isnull(");
                    sbnoas.Append(ep);
                    sbnoas.Append(",'')");
                }
            }
            else
            {
                DataType dt = DataType.String;
                if (ds != null)
                    dt = ds.Type;


                if (dt == DataType.DateTime)
                {
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append("[");
                    sbnoas.Append(asname);
                    sbnoas.Append("]");
                }
                else if (ds != null && ds.IsADecimal)
                {
                    if (cellPointLength >= 0)
                    {
                        StringBuilder sbtmp = new StringBuilder();
                        sbtmp.Append("convert(decimal(38,");
                        sbtmp.Append(cellPointLength.ToString());

                        sbtmp.Append("), [");
                        sbtmp.Append(asname);
                        sbtmp.Append("])");
                        sb.Append(sbtmp.ToString());
                        sb.Append(" as ");
                        sb.Append("[");
                        sb.Append(asname);
                        sb.Append("]");
                        sbnoas.Append(sbtmp.ToString());
                    }
                    else
                    {
                        sb.Append("[");
                        sb.Append(asname);
                        sb.Append("]");

                        sbnoas.Append("[");
                        sbnoas.Append(asname);
                        sbnoas.Append("]");
                    }
                    cellPointLength = 0;//每次使用完清空
                }
                else
                {
                    sb.Append("isnull([");
                    sb.Append(asname);
                    sb.Append("],'') as ");
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append("isnull([");
                    sbnoas.Append(asname);
                    sbnoas.Append("],'')");
                }
            }
        }

        #endregion

        protected void CheckData(int count, Infragistics.UltraChart.Shared.Styles.ChartType type)
        {
            if (count == 0)
                throw new CodeException("ChartNoData", String4Report.GetString("没有任何数据"));

            if (count < 2 &&
                (type == Infragistics.UltraChart.Shared.Styles.ChartType.HeatMapChart ||
                type == Infragistics.UltraChart.Shared.Styles.ChartType.HeatMapChart3D))
                throw new Exception(String4Report.GetString("热点图至少需要2行数据"));
        }

        public void SetDefaultID(int level, string id)
        {
            _report.ChartSchemas.CurrentGroupChart.SetDefaultID(level, id);
        }

        public void DeleteChart(int level, string id)
        {
            _report.ChartSchemas.CurrentGroupChart.DeleteAChart(level, id);
        }

        public bool Contains(int level)
        {
            return _report.ChartSchemas.CurrentGroupChart.Contains(level);
        }

        public SchemaIdentitys GetSchemasInLevel(int level)
        {
            SchemaIdentitys sis = new SchemaIdentitys();
            if (_report.ChartSchemas.CurrentGroupChart.Contains(level))
            {
                sis.DefaultID = _report.ChartSchemas.CurrentGroupChart.DefaultID(level);
                ICollection ids = _report.ChartSchemas.CurrentGroupChart.IDs(level);
                foreach (string id in ids)
                    sis.Identitys.Add(new SchemaIdentity(id, _report.ChartSchemas.CurrentGroupChart[level, id].Caption));
            }
            return sis;
        }

        public Hashtable GetSchemasAllLevel()
        {
            int levels = _report.GroupLevels;
            Hashtable ht = new Hashtable();
            for (int i = 1; i <= levels; i++)
                ht.Add(i, GetSchemasInLevel(i));
            return ht;
        }

        public virtual Hashtable GetSchemasAllLevel(int level)
        {
            return GetSchemasAllLevel();
        }
    }

    public class CrossChartService : ChartService
    {
        private SimpleHashtable _captiontoname;
        private SimpleHashtable _runtimecaptiontodesignname;
        private SimpleHashtable _designtimecaptiontodesignname;

        public CrossChartService(Report report)
            : base(report)
        {
            _captiontoname = new SimpleHashtable();
            _runtimecaptiontodesignname = new SimpleHashtable();
            _designtimecaptiontodesignname = new SimpleHashtable();

            Section pagetitle = report.Sections[SectionType.PageTitle];
            Section groupsummary = report.Sections.GetGroupSummary(1);
            if (groupsummary == null)
            {
                groupsummary = report.Sections[SectionType.ReportSummary];
            }
            foreach (Cell cell in pagetitle.Cells)
            {
                if (!cell.Name.Contains("SuperLabel"))
                {
                    if (!CheckSummary(groupsummary, cell.Name))
                        continue;
                }
                if (cell is Label)
                {
                    Label l = cell as Label;
                    if (l.DesignCaption != "____")
                    {
                        if (l is SuperLabel)
                        {
                            foreach (Label sl in (l as SuperLabel).Labels)
                            {
                                if (!string.IsNullOrEmpty(sl.DesignCaption) && sl.DesignCaption != "____")
                                    AddTo(l.Caption + "____" + sl.Caption, sl.Name, sl.Caption, sl.DesignName, sl.DesignCaption);
                            }
                        }
                        else if (!string.IsNullOrEmpty(l.DesignCaption))//x
                        {
                            AddTo(l.Caption, l.Name, l.Caption, l.DesignName, l.DesignCaption);
                        }
                    }
                }
            }
        }
        private bool CheckSummary(Section summary, string name)
        {
            return summary != null && summary.Cells[name] != null;
        }

        private void AddTo(string columnname, object source, string runtimecaption, string designname, string designcaption)
        {
            if (!_captiontoname.Contains(columnname))
                _captiontoname.Add(columnname, source);
            if (runtimecaption != null && !_runtimecaptiontodesignname.Contains(runtimecaption))
                _runtimecaptiontodesignname.Add(runtimecaption, designname);
            if (designcaption != null && !_designtimecaptiontodesignname.Contains(designcaption))
                _designtimecaptiontodesignname.Add(designcaption, designname);
        }

        protected override int InitDataSourceList(Hashtable ht, ArrayList alsource, ArrayList alname, int level, Hashtable hts)
        {
            foreach (string key in _designtimecaptiontodesignname.Keys)
            {
                ht.Add(key, null);
                hts.Add(ht.Count, key);
                if (alname != null && alname.Contains(_designtimecaptiontodesignname[key].ToString().ToLower()))
                    alsource.Add(key);
            }

            return 2;
        }

        protected override ArrayList GetDataSourceList(int level, ArrayList alsource)
        {
            ArrayList al = new ArrayList();
            foreach (string key in alsource)
                al.Add(_designtimecaptiontodesignname[key].ToString().ToLower());
            return al;
        }

        protected override void FindCellsToFill(SimpleHashtable simplehash, SimpleHashtable complexhash, int level, ArrayList alname)
        {
            GroupSummary gs = _report.Sections.GetGroupSummary(level);
            ArrayList alsort = new ArrayList();
            foreach (string key in _captiontoname.Keys)
            {
                string cn = key;
                string newcn = key;
                if (cn.Contains("____"))
                {
                    string[] cns = cn.Split(new string[] { "____" }, StringSplitOptions.RemoveEmptyEntries);
                    cn = cns[1];
                    //newcn = cns[0];
                }
                if (_runtimecaptiontodesignname.Contains(cn) &&
                    (alname.Contains(_runtimecaptiontodesignname[cn].ToString().ToLower()) ||
                      alname.Contains(_runtimecaptiontodesignname[cn].ToString()))
                    )
                {
                    Cell cell = gs.Cells[_captiontoname[key].ToString()];
                    if (cell != null)
                    {
                        cell.Caption = newcn;
                        SortCell(alsort, cell);
                    }
                }
            }
            foreach (Cell cell in alsort)
                FillToHashs(simplehash, complexhash, cell, cell.Caption);
        }
        void FindCellsToFill4SChartSource(SimpleHashtable simplehash, SimpleHashtable complexhash, int level, ArrayList alname, Cells cells)
        {
           Cells summary = _report.Sections[SectionType.ReportSummary].Cells;
            ArrayList alsort = new ArrayList();
            foreach (string key in _captiontoname.Keys)
            {
                string cn = key;
                string newcn = key;
                if (cn.Contains("____"))
                {
                    string[] cns = cn.Split(new string[] { "____" }, StringSplitOptions.RemoveEmptyEntries);
                    cn = cns[1];
                    //newcn = cns[0];
                }
                if (_runtimecaptiontodesignname.Contains(cn) &&
                    (alname.Contains(_runtimecaptiontodesignname[cn].ToString().ToLower()) ||
                      alname.Contains(_runtimecaptiontodesignname[cn].ToString()))
                    )
                {
                    Cell cell = summary[_captiontoname[key].ToString()];
                    if (cell != null)
                    {
                        cell.Caption = newcn;
                        SortCell(alsort, cell);
                    }
                }
            }
            foreach (Cell cell in alsort)
                FillToHashs(simplehash, complexhash, cell, cell.Caption);
        }
        public override DataTable GetChartDataByChartID(string id, object source, Infragistics.UltraChart.Shared.Styles.ChartType type)
        {
            int _currentlevel = 1;
            RuntimeGroup group = source as RuntimeGroup;
            ChartSchemaItemAmong csi = _report.ChartSchemas.CurrentGroupChart[_currentlevel, id];
            ArrayList alname = csi.Source;
            SimpleHashtable simplehash = new SimpleHashtable();
            SimpleHashtable complexhash = new SimpleHashtable();

            Cells cells = _report.GridDetailCells.Clone() as Cells;
            HandleMaxMinAvg(cells);
            FindCellsToFill4SChartSource(simplehash, complexhash, _currentlevel, alname, _report.GridDetailCells);
            string minor = string.Empty;
            string upper = string.Empty;

            //foreach (string key in simplehash.Keys)
            //{
            //    Cell cell = cells.GetBySource(key);
            //    //取运行时期的(cell as ICalculator).Operator.ToString()，需要修改一下
            //    PrecisionHelper ph = new PrecisionHelper(cell is IGroup, cell.Name, (cell as ICalculator).Operator.ToString(), (cell as IDataSource).DataSource.Name, null, true, cell is IDecimal);
            //    int precision = -1;
            //    //if (_precisions.Contains(key))
            //    //    precision = Convert.ToInt32(_precisions[key]);
            //    AddAggregateStrings(ph.OperType, ph.Expression, ph.AsName, precision, ph.bSingleColumn, ref minor, ref upper);
            //}

            foreach (string key in simplehash.Keys)
            {
                //Cell cell = cells.GetBySource(key);
                //if (cell == null)
                //{
                //    cell=cell
                //}
                //取运行时期的(cell as ICalculator).Operator.ToString()，需要修改一下
                ICalculator ic = _report.RuntimeGetSummaryCell(key) as ICalculator;
                string oper = string.Empty;
                if (ic != null)
                    oper = ic.Operator.ToString();
                else oper = ic.Operator.ToString();
                PrecisionHelper ph = null;
                string dataName = string.Empty;
                string express = null;
                bool bsingle = true;
                if (ic is IDataSource)
                {
                    dataName = (ic as IDataSource).DataSource.Name;
                }
                else if (ic is IMapName)
                {
                    dataName = (ic as IMapName).MapName;

                }
                if (ic is ICalculateColumn)
                {
                    express = (ic as ICalculateColumn).Expression;
                    bsingle = false;
                }
                ph = new PrecisionHelper(ic is IGroup, key, oper, dataName, express, bsingle, ic is IDecimal);
                int precision = -1;
                AddAggregateStrings(ph.OperType, ph.Expression, ph.AsName, precision, ph.bSingleColumn, ref minor, ref upper);
            }

            string groupbyStr = string.Empty;
            GroupSchema groupSchema = _report.GroupSchemas[csi.DataDependGroupId];
            if (groupSchema == null)
                throw new Exception("当前图表依赖的分组不存在");
            string groupStr = this.GetGrouplevle1Str(groupSchema, ref groupbyStr);
            //string gouplevle1Str = this.CreaeGroupLevel1Table(minor, groupStr, groupbyStr);
            string gouplevle1Str = this.CreaeGroupLevel1Table(minor, groupStr, groupbyStr);

            string wherestring = "";
            if (group != null)
                wherestring = group.GetFilterString();
            if (wherestring != "")
                wherestring = " where " + wherestring;

            DataTable dt = null;
            if (complexhash.Count > 0)
            {
                ReportEngine engine = DefaultConfigs.GetRemoteEngine(ClientReportContext.Login, ReportStates.Browse);
                dt = engine.ComplexChartDataWithDependId(_report.CacheID,
                    simplehash,
                    complexhash,
                    _currentlevel, wherestring, csi.DataDependGroupId);
                engine = null;
            }
            else
            {
                if (simplehash.Count == 0)
                    throw new Exception(U8ResService.GetResStringEx("U8.UAP.Report.ChartDataSourceRemoved"));
                StringBuilder sb = new StringBuilder();
                sb.Append("select ");
                if (csi.TopRank != 0)
                {
                    sb.Append(" top ");
                    sb.Append(csi.TopRank);
                    sb.Append(" ");
                }
                SimpleArrayList altmp = groupSchema.SchemaItems[0].Items;
                foreach (string key in altmp)
                {
                    if (sb.Length > 7)
                        sb.Append("+ N' ' + ");

                    int bconvert = -1;
                    DataSource ds = _report.DataSources[key];
                    if (ds != null && ds.Type == DataType.DateTime)
                    {
                        sb.Append("convert(nvarchar(10),");
                        bconvert = 1;
                    }
                    else if (ds != null && ds.Type != DataType.String)
                    {
                        sb.Append("convert(nvarchar(100),");
                        bconvert = 2;
                    }

                    sb.Append("[");
                    if (_report.RealViewType == ReportType.CrossReport)
                    {
                        string mapName = key;
                        if (_report.GridDetailCells[key] is IMapName)
                            mapName = (_report.GridDetailCells[key] as IMapName).MapName;
                        sb.Append(mapName);
                    }
                    else
                    sb.Append(key);
                    sb.Append("]");

                    if (bconvert == 1)
                        sb.Append(",120)");
                    else if (bconvert == 2)
                        sb.Append(")");
                }
                string orderkey = null;
                foreach (string key in simplehash.Keys)
                {
                    orderkey = simplehash[key].ToString();
                    sb.Append(",isnull([");
                    sb.Append(key);
                    sb.Append("],0)");
                    sb.Append(" as [");
                    sb.Append(simplehash[key].ToString());
                    sb.Append("]");
                }
                sb.Append("  from (");
                sb.Append(gouplevle1Str);
                //sb.Append(_report.MinorAggregateString);
                sb.Append(" ) as group_1");
                sb.Append(wherestring);
                if (csi.OrderType != 0)
                {
                    sb.Append(" order by [");
                    sb.Append(orderkey);
                    sb.Append(csi.OrderType == 1 ? "] asc " : "] desc ");
                }
                RemoteDataHelper rdh = null;
                if (_report.bWebOrOutU8)//ReportStates.WebBrowse
                    rdh = DefaultConfigs.GetRemoteHelper();
                else
                    rdh = new RemoteDataHelper();
                dt = rdh.GetChartData(ClientReportContext.Login.UfDataCnnString, sb.ToString(), System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            }

            CheckData(dt.Rows.Count, type);
            return dt;
        }
    }

    public class IndicatorChartService : ChartService
    {
        protected Hashtable _captiontoname;
        public IndicatorChartService(Report report, Hashtable captiontoname)
            : base(report)
        {
            _captiontoname = captiontoname;
        }

        protected override int InitDataSourceList(Hashtable ht, ArrayList alsource, ArrayList alname, int level, Hashtable hts)
        {
            SimpleHashtable ctn = _captiontoname[level] as SimpleHashtable;
            bool bcross = (bool)ctn["__bcross__"];
            foreach (string key in ctn.Keys)
            {
                ht.Add(key, null);
                hts.Add(ht.Count, key);
                if (alname != null && alname.Contains(ctn[key].ToString().ToLower()))
                    alsource.Add(key);
            }

            return bcross ? 2 : 0;
        }

        protected override ArrayList GetDataSourceList(int level, ArrayList alsource)
        {
            SimpleHashtable ctn = _captiontoname[level] as SimpleHashtable;
            ArrayList al = new ArrayList();
            foreach (string key in alsource)
                al.Add(ctn[key].ToString().ToLower());
            return al;
        }

        public override DataTable GetDataSource(int level, string id, object source, Infragistics.UltraChart.Shared.Styles.ChartType type)
        {
            SimpleHashtable ctn = _captiontoname[level] as SimpleHashtable;
            DataTable sourcetable = (source as DataTable).Copy();
            ChartSchemaItemAmong csi = _report.ChartSchemas.CurrentGroupChart[level, id];
            ArrayList alname = csi.Source;
            ArrayList invalidcolumns = new ArrayList();
            string orderkey = null;
            foreach (DataColumn dc in sourcetable.Columns)
            {
                string cn = dc.ColumnName;
                string newcn = dc.ColumnName;
                if (cn.Contains("____"))
                {
                    string[] cns = cn.Split(new string[] { "____" }, StringSplitOptions.RemoveEmptyEntries);
                    cn = cns[1];
                    newcn = cns[0];
                }

                if (dc.DataType == typeof(decimal) && ctn.Contains(cn) && !alname.Contains(ctn[cn].ToString().ToLower()))
                    invalidcolumns.Add(dc);
                else if (dc.ColumnName != newcn)
                    dc.ColumnName = newcn;

                if (csi.TopRank != 0 && ctn.Contains(cn) && alname.Contains(ctn[cn].ToString().ToLower()))
                    orderkey = dc.ColumnName;
            }
            foreach (DataColumn dc in invalidcolumns)
                sourcetable.Columns.Remove(dc);

            CheckData(sourcetable.Rows.Count, type);
            if (csi.TopRank != 0)
            {
                DataRow[] drs = sourcetable.Select("", "[" + orderkey + (csi.OrderType == 1 ? "] asc" : "] desc"));
                sourcetable = sourcetable.Clone();
                for (int i = 0; i < csi.TopRank && i < drs.Length; i++)
                {
                    DataRow dr = sourcetable.NewRow();
                    foreach (DataColumn dc in sourcetable.Columns)
                        dr[dc.ColumnName] = drs[i][dc.ColumnName];
                    sourcetable.Rows.Add(dr);
                }
            }

            return sourcetable;
        }

        /// <summary>
        /// 12.0新加
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataTable GetChartDataByChartID(string id)
        {
            DataTable dt = new DataTable();
            return dt;

        }

        public override Hashtable GetSchemasAllLevel(int level)
        {
            Hashtable ht = new Hashtable();
            ht.Add(level, GetSchemasInLevel(level));
            return ht;
        }
    }

    public class ChartStyleHelper
    {
        protected static ChartTextCollection GetTextCollection(UltraChart chart)
        {
            ChartTextCollection _TColl = null;

            try
            {
                ChartType chartType;
                switch (chart.ChartType)
                {
                    case ChartType.StackBarChart:
                        chartType = ChartType.BarChart;
                        break;
                    case ChartType.StackColumnChart:
                        chartType = ChartType.ColumnChart;
                        break;
                    case ChartType.HeatMapChart3D:
                    case ChartType.PieChart3D:
                    case ChartType.Composite:
                        return null;
                    default:
                        chartType = chart.ChartType;
                        break;

                }

                PropertyInfo appProp = chart.GetType().GetProperty(chartType.ToString());
                if (appProp != null)
                {
                    object app = appProp.GetValue(chart, BindingFlags.GetProperty, null, null, null);

                    if (app != null)
                    {
                        PropertyInfo ctxProp = app.GetType().GetProperty("ChartText");
                        if (ctxProp != null)
                        {
                            object ctx = ctxProp.GetValue(app, BindingFlags.GetProperty, null, null, null);
                            if (ctx != null)
                            {
                                _TColl = (ChartTextCollection)ctx;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return _TColl;

        }

        protected static ChartTextAppearance GetDataLabels(UltraChart chart, ChartTextCollection col)
        {
            ChartTextAppearance _DataLabels = null;

            if (col != null)
            {
                // look for on chart text appearance where wild card is specified.
                foreach (ChartTextAppearance ap in col)
                {
                    if (ap.Match((int)ItemWildCard.Row, (int)ItemWildCard.Column))
                    {
                        _DataLabels = ap;
                        break;
                    }
                }
                if (_DataLabels != null) _DataLabels.SetComponent((IChartComponent)chart);
            }
            return _DataLabels;
        }

        public static void SetChartTextFormat(UltraChart chart, int precision)
        {
            ChartTextCollection col = GetTextCollection(chart);
            ChartTextAppearance DatApp = GetDataLabels(chart, col);
            if (DatApp == null)
                return;
            DatApp.ItemFormatString = "<DATA_VALUE:0.###############################>";
            //DatApp.Visible = true;
        }

        public static void InitChartWithSolidProperty(UltraChart MyChart)
        {
            //MyChart.Border.Thickness = 0;
            MyChart.PieChart.Labels.Format = PieLabelFormat.None;
            MyChart.PieChart.OthersCategoryPercent = 0;
            MyChart.PieChart3D.Labels.Format = PieLabelFormat.None;
            MyChart.PieChart3D.OthersCategoryPercent = 0;
            MyChart.DoughnutChart.Labels.Format = PieLabelFormat.None;
            MyChart.DoughnutChart.OthersCategoryPercent = 0;
            MyChart.DoughnutChart3D.Labels.Format = PieLabelFormat.None;
            MyChart.DoughnutChart3D.OthersCategoryPercent = 0;
            MyChart.Tooltips.Format = TooltipStyle.None;//.Custom;
            string type = MyChart.ChartType.ToString().ToLower();
            if (type.IndexOf("bar") != -1 || type.IndexOf("column") != -1)
                MyChart.Axis.X.Labels.ItemFormat = AxisItemLabelFormat.None;
            MyChart.Axis.Y.Labels.ItemFormatString = "<DATA_VALUE:0.00>";
            //if(MyChart.ChartType== ChartType.PieChart || MyChart.ChartType== ChartType.PieChart3D)
            //    MyChart.Tooltips.FormatString = "<DATA_VALUE>";
            //else
            //    MyChart.Tooltips.FormatString = "<SERIES_LABEL>:<DATA_VALUE>";

            //SetChartTextFormat(MyChart, 0);
            #region anti-collision
            MyChart.Axis.X.Labels.Layout.BehaviorCollection.AddRange(GetCustomLayoutBehaviors());
            MyChart.Axis.X.Labels.SeriesLabels.Layout.BehaviorCollection.AddRange(GetCustomLayoutBehaviors());
            MyChart.Axis.Y.Labels.Layout.BehaviorCollection.AddRange(GetCustomLayoutBehaviors());

            MyChart.Axis.X.Labels.Layout.Behavior = AxisLabelLayoutBehaviors.Auto;
            MyChart.Axis.X.Labels.SeriesLabels.Layout.Behavior = AxisLabelLayoutBehaviors.Auto;
            MyChart.Axis.Y.Labels.Layout.Behavior = AxisLabelLayoutBehaviors.Auto;
            #endregion

            SetDefaultAppearance(MyChart);
        }

        private static void SetDefaultAppearance(UltraChart chart)
        {
            ChartType type = chart.ChartType;
            if (type.ToString().ToLower().IndexOf("bar") > -1)
                SetBarDefaultAppearance(chart);
            else if (type.ToString().ToLower().IndexOf("column") > -1)
                SetColumnDefaultAppearance(chart);
            else
                SetOtherAppearance(chart);
        }

        private static void SetColumnDefaultAppearance(UltraChart chart)
        {
            chart.Axis.X.Labels.ItemFormat = AxisItemLabelFormat.None;
            SetOtherAppearance(chart);
        }

        private static void SetOtherAppearance(UltraChart chart)
        {
            //this.ultraChart1.Axis.X.Labels.ItemFormat = AxisItemLabelFormat.None;
            chart.Axis.X2.Labels.ItemFormat = AxisItemLabelFormat.None;
            chart.Axis.Y.Labels.ItemFormatString = "<DATA_VALUE:0.00>";
            chart.Axis.Y2.Labels.ItemFormatString = "<DATA_VALUE:0.00>";
            chart.PieChart.Labels.LeaderLinesVisible = false;
            chart.PieChart.Labels.Format = PieLabelFormat.None;
            chart.DoughnutChart.Labels.LeaderLinesVisible = false;
            chart.DoughnutChart.Labels.Format = PieLabelFormat.None;
            //UpdateChartText();
        }

        private static void SetBarDefaultAppearance(UltraChart chart)
        {
            chart.Axis.Y.Labels.ItemFormat = AxisItemLabelFormat.None;
            chart.Axis.Y2.Labels.ItemFormat = AxisItemLabelFormat.None;
            chart.Axis.X.Labels.ItemFormatString = "<DATA_VALUE:0.00>";
            chart.Axis.X2.Labels.ItemFormatString = "<DATA_VALUE:0.00>";
            chart.PieChart.Labels.LeaderLinesVisible = false;
            chart.PieChart.Labels.Format = PieLabelFormat.None;
            chart.DoughnutChart.Labels.LeaderLinesVisible = false;
            chart.DoughnutChart.Labels.Format = PieLabelFormat.None;
            //UpdateChartText();
        }

        private static AxisLabelLayoutBehavior[] GetCustomLayoutBehaviors()
        {
            // scale fonts down to 8pt if necessary.
            FontScalingAxisLabelLayoutBehavior fontScaling1 = new FontScalingAxisLabelLayoutBehavior();
            fontScaling1.MaximumSize = -1f;
            fontScaling1.MinimumSize = 8f;
            fontScaling1.EnableRollback = false;

            // if collisions are detected, try wrapping the text
            WrapTextAxisLabelLayoutBehavior wrapText1 = new WrapTextAxisLabelLayoutBehavior();
            wrapText1.EnableRollback = true;

            // try rotating to 30 degrees
            RotateAxisLabelLayoutBehavior rotation1 = new RotateAxisLabelLayoutBehavior();
            rotation1.RotationAngle = 30f;
            rotation1.EnableRollback = true;

            // failing that, try rotating to 60 degrees
            RotateAxisLabelLayoutBehavior rotation2 = new RotateAxisLabelLayoutBehavior();
            rotation2.RotationAngle = 60f;
            rotation2.EnableRollback = true;

            // try staggering the labels
            StaggerAxisLabelLayoutBehavior stagger1 = new StaggerAxisLabelLayoutBehavior();
            stagger1.EnableRollback = true;

            // since none of the above worked, scale the fonts down to 6pt
            FontScalingAxisLabelLayoutBehavior fontScaling2 = new FontScalingAxisLabelLayoutBehavior();
            fontScaling2.MaximumSize = -1f;
            fontScaling2.MinimumSize = 6f;
            fontScaling2.EnableRollback = false;

            // if collisions are still detected, just truncate the labels.
            ClipTextAxisLabelLayoutBehavior clipText1 = new ClipTextAxisLabelLayoutBehavior();
            clipText1.EnableRollback = false;

            return new AxisLabelLayoutBehavior[] { fontScaling1, wrapText1, rotation1, rotation2, stagger1, fontScaling2, clipText1 };
        }
    }

}

