using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportElements;
using UFIDA.U8.UAP.Services.BizDAE.Elements;
using UFIDA.U8.UAP.Services.BizDAE.ConfigureServices;
using UFIDA.U8.UAP.Services.BizDAE.DBServices;
using UFIDA.U8.UAP.Services.BizDAE.DBServices.QueryServices;
using System.Drawing;
using System.Reflection;

namespace UFIDA.U8.UAP.Services.ReportData
{
    internal class UpgradeFormatService:IUpgradeItem
    {
        private U8LoginInfor _login;
        private string _reportid;//(另存的)销售统计表
        private string _subid;
        private string _oldreportid;//1065
        private Report _report;
        private Section _detailsection;
        private BusinessObject _bo;
        private ColumnCollection _columns;
        private string _classname;
        private string _basereportid;//销售统计表
        private string _cnreportname;
        private string _enreportname;
        private string _twreportname;

        public UpgradeFormatService()
        {
        }

        //leftex   width
        //6--------3
        //6
        //7
        //8
        private bool ToBeContinueRead(int orderex, int leftex, int width)
        {
            if (leftex == orderex)
            {
                if (width == 1)
                    return false;
                else
                    return true;
            }
            else if (leftex < orderex)
                return true;
            else
                return false;
        }

        private void FillLocaleCaption(SqlDataReader reader,SqlDataReader localereader,SimpleHashtable captions,string localeid)
        {
            string caption = null;
            int left = -1;
            int width = -1;
            string name = null;
            string nameforeign = null;
            while (reader.Read())
            {
                object orderex1 = reader["orderex"];
                if (orderex1 == DBNull.Value)
                    continue;
                int orderex = Convert.ToInt32(orderex1);
                string condition = reader["condition"].ToString().Trim();
                condition = ReplaceCondition(condition);
                condition = HandleCondition(condition, localeid );
                string key = GetKey(reader["expression"].ToString().Trim(), condition);
                name = reader["name"].ToString().Trim();
                nameforeign = reader["nameforeign"].ToString().Trim();
                captions.Add(key, (nameforeign == "" ? name : nameforeign));

                bool b = ToBeContinueRead(orderex, left, width);
                if (!b)
                {
                    if (left == orderex)
                    {
                        captions.Add(key, caption);
                        left = -1;
                    }
                }
                else
                {
                    if (left == orderex)
                    {
                        captions.Add(key+"_Super", caption);
                        left = -1;
                    }

                    while (localereader.Read())
                    {
                        int leftex = Convert.ToInt32(localereader["leftex"]);
                        int widthex = Convert.ToInt32(localereader["width"]);
                        int topex = Convert.ToInt32(localereader["topex"]);
                        name = localereader["name"].ToString().Trim();
                        nameforeign = localereader["nameforeign"].ToString().Trim();
                        caption = (nameforeign == "" ? name : nameforeign);

                        if (leftex < orderex)
                            continue;
                        else if (leftex > orderex)
                        {
                            left = leftex;
                            width = widthex;
                            break;
                        }
                        else
                        {
                            if (widthex == 1)
                            {
                                captions.Add(key, caption);
                                left = -1;
                                break;
                            }
                            else
                            {
                                captions.Add(key + "_Super", caption );
                            }
                        }
                    }
                }
            }
        }

        private string GetKey(string expression, string condition)
        {
            if (!string.IsNullOrEmpty(expression))
                return expression;
            else
                return condition;
        }
        private void UpgradeFormat(UpgradeReportMetaWrapper urmw, bool defaultshowdetail)
        {
            UpgradeFormat(urmw, defaultshowdetail, false);        
        }

        private void UpgradeFormat(UpgradeReportMetaWrapper urmw, bool defaultshowdetail, bool IsBak)
        {
            string tablename = string.Empty;
            if (IsBak)
                tablename = "rpt_flddef_base_bak";
            else
                tablename = "rpt_flddef_base";

            _report = new Report();
            _report.ViewID = _subid + "[__]" + _reportid;//_basereportid;
			//_report.ViewID = Guid.NewGuid().ToString();
            string[] crossdetailitems=null;
            string crosscolumnheaderitem = null;

            if (_report.ViewID.ToLower() == "sa[__]货龄分析(按发票)")//SA[__]货龄分析(按发票)
                _report.bSupportDynamicColumn = true;

            #region init bo
            string datasourceid = null;
            string olddatasourceid = null;
            string fuctionname = null;
            string projectNO = "U8CUSTDEF";
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfMetaCnnString, "select top 1 datasourceid,functionname,HelpFileName,HelpIndex,HelpKeyWord,MappingMenuId,b.ProjectNo from uap_report a LEFT JOIN BD_BusinessObjects b ON a.DataSourceID=b.MetaID  where subid='" + _subid + "' and id='" + _subid + "[__]" + _basereportid + "'"))
            {
                while (reader.Read())
                {
                    datasourceid = reader["DataSourceID"].ToString();
                    fuctionname = reader["FunctionName"].ToString();
                    try
                    {
                        projectNO = reader["ProjectNo"].ToString();
                    }
                    catch
                    {
                    }
                    urmw.SetArgument(UpgradeReportMetaWrapper.ReportHelpFileName, reader["HelpFileName"].ToString());
                    urmw.SetArgument(UpgradeReportMetaWrapper.ReportHelpIndex, reader["HelpIndex"].ToString());
                    urmw.SetArgument(UpgradeReportMetaWrapper.ReportHelpKeyWord, reader["HelpKeyWord"].ToString());
                    //string mappingMenuId = reader["MappingMenuId"].ToString();
                    //if (!string.IsNullOrEmpty(mappingMenuId))// 如果有对应的左树菜单,则级为系统报表
                    //{
                    //    if(!urmw.Contains(UpgradeReportMetaWrapper.MappingMenuId))
                    //        urmw.SetArgument(UpgradeReportMetaWrapper.MappingMenuId, mappingMenuId);
                    //    //urmw.SetArgument(UpgradeReportMetaWrapper.ReportIsSystem, true);
                    //    //urmw.SetArgument(UpgradeReportMetaWrapper.ViewIsSystem, true);
                    //}
                }
                reader.Close();
            }
            if (datasourceid == null)
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, string.Format("select top 1 orderex from {1} where modeex=22 and id='{0}'",_oldreportid,tablename )))
                {
                    while (reader.Read())
                    {
                        string helpfilename = _subid;
                        if (_subid.ToLower() == "pu")
                            helpfilename = "CG";
                        string helpindex = reader["orderex"].ToString();
                        urmw.SetArgument(UpgradeReportMetaWrapper.ReportHelpFileName, helpfilename );
                        urmw.SetArgument(UpgradeReportMetaWrapper.ReportHelpIndex, helpindex );
                        urmw.SetArgument(UpgradeReportMetaWrapper.ReportHelpKeyWord, helpindex );
                    }
                    reader.Close();
                }
            }
            //if (_report.ViewID == "SA[__]发货明细表")
            //    datasourceid = null;
            if (!string.IsNullOrEmpty(datasourceid))
            {
                //try
                //{
                //    ConfigureServiceProxy proxy = new ConfigureServiceProxy(_login.AppServer, _login.UfMetaCnnString);
                //    proxy.DeleteBusinessObject(datasourceid);
                //}
                //catch
                //{
                //}
                olddatasourceid = datasourceid;
                datasourceid = null;
            }
            if (String.IsNullOrEmpty(datasourceid ))
            {//new bo
                #region new bo
                fuctionname="ReportFunction";
               // _bo = new BusinessObject(Guid.NewGuid().ToString(), "U8CUSTDEF", _subid, _reportid + "数据源");
                _bo = new BusinessObject(Guid.NewGuid().ToString(), projectNO, _subid, _reportid + "数据源");
                _bo.Name = _bo.MetaID;
                if (!string.IsNullOrEmpty(olddatasourceid))
                    _bo.MetaID = olddatasourceid;
                QueryFunction qf = new QueryFunction(fuctionname,fuctionname );
                _bo.Functions.Add(qf);
                if (string.IsNullOrEmpty(_classname))
                {
                    SQLQuerySetting sqlqs = new SQLQuerySetting();
                    sqlqs.DataSourceType = DataSourceTypeEnum.Script;
                    qf.QuerySettings.Add(sqlqs);
                }
                else
                {
                    CustomQuerySetting cqs = new CustomQuerySetting();
                    cqs.DllType = "Com";
                    cqs.Content = _classname;
                    qf.QuerySettings.Add(cqs);
                }
                _columns = qf.QuerySettings[0].QueryResultTable.Columns;
                #endregion
            }
            else
            {//get bo
                #region get bo
                ConfigureServiceProxy proxy = new ConfigureServiceProxy(_login.AppServer, _login.UfMetaCnnString);
                _bo = proxy.GetBusinessObject(datasourceid);
                proxy.LanguageId = _login.LocaleID ;
                QueryFunction qf=(QueryFunction)_bo.Functions[fuctionname ];
                if (qf == null)
                {
                    qf = (QueryFunction)_bo.Functions[0];
                    qf.Name = fuctionname;
                }
                _columns = qf.QuerySettings[0].QueryResultTable.Columns;

                
                #endregion
            }

            #region handle sql
            if (String.IsNullOrEmpty(_classname))
            {
                #region getsqlscript
                StringBuilder sb = new StringBuilder();
                string orderby = "ORDER BY ID_Field";
                if(IsBak)
                    orderby = "";
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                    string.Format("SELECT Expression FROM {1} WHERE id=N'{0}' AND ModeEx=24 AND LocaleId=N'zh-CN' order by orderex", _oldreportid, tablename)))
                {
                    while (reader.Read())
                    {
                        sb.Append(reader["Expression"].ToString());
                    }
                    reader.Close();
                }
                string sql = sb.ToString();
                HandleIfCrossReport(ref sql, ref crossdetailitems, ref crosscolumnheaderitem);
                #endregion
                SQLQuerySetting sqlqs = ((QueryFunction)_bo.Functions[fuctionname]).QuerySettings[0] as SQLQuerySetting;
                DispartSqlSelect dispartSqlSelect = new DispartSqlSelect();
                dispartSqlSelect.Dispart(sql);
                sqlqs.Script.SelectPart = dispartSqlSelect.SelectPart;
                sqlqs.Script.GroupByPart = dispartSqlSelect.GroupByPart;
                sqlqs.Script.HavingPart = dispartSqlSelect.HavingPart;
                sqlqs.Script.OrderByPart = dispartSqlSelect.OrderByPart;
                sqlqs.Script.WherePart = dispartSqlSelect.WherePart;
            }
            else if(bCGSpecial )
            {
                crosscolumnheaderitem="HLPeriod";
                crossdetailitems=new string[]{"Money1"};
            }
            #endregion
            #endregion

            #region init report & cross handle
            if (crosscolumnheaderitem == null)
            {
                _report.Type = ReportType.GridReport;
                _detailsection = new GridDetail();
            }
            else
            {
                _report.Type = ReportType.CrossReport;
                _detailsection = new CrossRowHeader();
                
                #region columnheader
                Section crosscolumnheader = new CrossColumnHeader();
                TableColumn column = GetTableColumn(crosscolumnheaderitem);
                if (column == null)
                {
                    column = new TableColumn();
                    column.Name = crosscolumnheaderitem;
                    column.DataType = DataTypeEnum.String;
                    column.DescriptionCN = crosscolumnheaderitem;
                    column.DescriptionTW = crosscolumnheaderitem;
                    column.DescriptionUS = crosscolumnheaderitem;
                    _columns.Add(column);
                }
                DataSource ds = GetDataSource(column);
                Cell cell = crosscolumnheader.GetDefaultRect(ds);
                cell.X = 10;
                cell.SetY(DefaultConfigs.SECTIONHEADERHEIGHT + 4);
                crosscolumnheader.Cells.AddDirectly (cell);
                _report.Sections.Add(crosscolumnheader);
                #endregion

                #region crossdetail
                Section crossdetail = new CrossDetail();
                int order = 0;
                foreach (string detailitem in crossdetailitems)
                {
                    column = GetTableColumn(detailitem);
                    if (column == null)
                    {
                        column = new TableColumn();
                        column.Name = detailitem ;
                        column.DataType = DataTypeEnum.Decimal;
                        column.DescriptionCN = detailitem;
                        column.DescriptionTW = detailitem;
                        column.DescriptionUS = detailitem;
                        _columns.Add(column);
                    }
                    ds = GetDataSource(column);
                    cell = crossdetail.GetDefaultRect(ds);
                    cell.X = 10+order++;
                    cell.SetY(DefaultConfigs.SECTIONHEADERHEIGHT + 10);//cell.Y + height);
                    //height = cell.Height;
                    if (cell is IGridCollect)
                    {
                        (cell as IGridCollect).bColumnSummary = false;
                        (cell as IGridCollect).bSummary = true;
                        (cell as IGridCollect).Operator = OperatorType.SUM;
                    }
                    crossdetail.Cells.AddDirectly (cell);
                }
                _report.Sections.Add(crossdetail);
                #endregion
            }
            _report.Sections.Add(_detailsection);
            #endregion

            #region detail
            #region twcaptions
            SimpleHashtable twcaptions = new SimpleHashtable();
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                "select name,nameforeign,expression,condition,orderex from "+tablename+" where id=" + _oldreportid + " and modeex=0 and topex<>1  and localeid='zh-TW' order by orderex asc"))
            using (SqlDataReader twreader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                "select name,nameforeign,expression,condition,isnull(leftex,0) as leftex,isnull(topex,0) as topex,isnull(width,0) as width from "+tablename+ " where id=" + _oldreportid + " and modeex=5 and localeid='zh-TW' order by leftex,topex asc"))
            {
                FillLocaleCaption(reader, twreader, twcaptions,"zh-TW");
                twreader.Close();
                reader.Close();
            }
            #endregion
            #region encaptions
            SimpleHashtable encaptions = new SimpleHashtable();
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                "select name,nameforeign,expression,condition,orderex from " + tablename + " where id=" + _oldreportid + " and modeex=0 and topex<>1  and localeid='en-US' order by orderex asc"))
            using (SqlDataReader enreader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                "select name,nameforeign,expression,condition,isnull(leftex,0) as leftex,isnull(topex,0) as topex,isnull(width,0) as width from " + tablename + " where id=" + _oldreportid + " and modeex=5 and localeid='en-US' order by leftex,topex asc"))
            {
                FillLocaleCaption(reader, enreader, encaptions,"en-US");
                enreader.Close();
                reader.Close();
            }
            #endregion

            //int defaultwidth = 96;
            int defaultheight = 24;
            SimpleHashtable htsuper = new SimpleHashtable();
            object olevels = SqlHelper.ExecuteScalar(_login.UfDataCnnString, "select max(topex) from " + tablename + " where id=" + _oldreportid + " and modeex=5 and localeid='zh-CN'");
            int levels = ((olevels == null || olevels is DBNull) ? 1 : (int)olevels);
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                "select name,nameforeign,expression,condition,orderex,topex,width,isize,Visible,FormatEx,note,iColSize from " + tablename + " where id=" + _oldreportid + " and modeex=0 and topex<>1  and localeid='zh-CN' order by orderex asc"))
            using (SqlDataReader cnreader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                "select name,nameforeign, isnull(leftex,0) as leftex,isnull(topex,0) as topex,isnull(width,0) as width,isnull(height,0) as height from " + tablename + " where id=" + _oldreportid + " and modeex=5 and localeid='zh-CN' order by leftex,topex asc"))
            {
                while (reader.Read())
                {
                    #region column base infor
                    string name = reader["Name"].ToString().Trim();
                    string nameforeign = reader["nameforeign"].ToString().Trim();
                    string expression = reader["Expression"].ToString().Trim();
                    //if (_subid.ToUpper() == "SA" && (_reportid == "发票日报" || _basereportid == "发票日报") && expression == "默认")
                    //    continue;
                    string condition = reader["Condition"].ToString().Trim();
                    condition = ReplaceCondition(condition );
                    condition = HandleCondition(condition,"zh-CN");
                    string key = GetKey(expression, condition);
                    int orderex = Convert.ToInt32(reader["OrderEx"]);
                    int topex = Convert.ToInt32(reader["TopEx"]);
                    int isize = Convert.ToInt32(reader["iSize"]);
                    int visible = Convert.ToInt32(reader["Visible"]);
                    int width = Convert.ToInt32(reader["iColSize"]);
                    string formatestring=reader["FormatEx"].ToString().Trim();
                    string note = reader["Note"].ToString().Trim();
                    if (isize != 100 && isize!=0)
                    {
                        if (note.ToLower() == "sum" || note.ToLower() == "avg")
                            isize = 0;
                        if (formatestring.Contains(".") || formatestring.Contains("0") || formatestring.ToUpper().Contains("U8") || bDecimalFormat(formatestring))
                            isize = 0;
                    }
                    Cell cell = null;
                    TableColumn column = null;
                    if (!string.IsNullOrEmpty(condition))
                    {//calculate column
                        #region calculate column
                        if (condition.IndexOf("PREV_", StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            #region Prev_
                            cell = new GridDecimalAlgorithmColumn();
                            (cell as GridDecimalAlgorithmColumn).Algorithm = GetPrevAlgorithm(condition);
                            #endregion
                        }
                        else if (condition.IndexOf("IIF(", StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            #region IIF
                            cell = new GridAlgorithmColumn();
                            (cell as GridAlgorithmColumn).Algorithm = GetIIFAlgorithm(condition);
                            #endregion
                        }
                        else
                        {
                            #region calculate column
                            cell = new GridCalculateColumn();
                            (cell as GridCalculateColumn).Expression = condition;
                            #endregion
                        }
                        #region name & default caption
                        //cell.Name = (expression == "" ? "Cell" + orderex.ToString() + topex.ToString() : expression);
                        cell.Name = (expression == "" ? name : expression);
                        cell.Caption = nameforeign == "" ? name : nameforeign;
                        cell.ENCaption = encaptions.Contains(key) ? encaptions[key].ToString() : cell.Caption ;
                        cell.TWCaption = twcaptions.Contains(key) ? twcaptions[key].ToString() : cell.Caption ;
                        #endregion
                        #endregion
                    }
                    else
                    {//datasource
                        #region datasource column
                        #region cross items dont add to detail
                        if (crosscolumnheaderitem != null && crossdetailitems != null)
                        {
                            if (expression.ToLower() == crosscolumnheaderitem.Trim().ToLower())
                                continue;
                            else
                            {
                                bool bcontinue = false;
                                foreach (string detailitem in crossdetailitems)
                                {
                                    if (detailitem.Trim().ToLower() == expression.ToLower())
                                    {
                                        bcontinue = true;
                                        break;
                                    }
                                }
                                if (bcontinue)
                                    continue;
                            }
                        }
                        #endregion
                        column = GetTableColumn(expression);
                        DataSource ds = null;
                        if (column == null)
                        {
                            column = new TableColumn();
                            column.Name = expression;
                            column.DataType = GetDAEDataType(isize);
                            _columns.Add(column);
                        }

                        column.DescriptionCN  = nameforeign == "" ? name : nameforeign;
                        column.DescriptionUS = encaptions.Contains(key) ? encaptions[key].ToString() : column.DescriptionCN ;
                        column.DescriptionTW = twcaptions.Contains(key) ? twcaptions[key].ToString() : column.DescriptionCN ;

                        ds = GetDataSource(column);
                        cell = _detailsection.GetDefaultRect(ds);
                        #endregion
                    }

                    cell.Visible = (visible == 0 ? false : true);

                    if (cell is IFormat)
                        SetPrecisionAndFormatString(cell as IFormat, formatestring);
                    if (cell is IGridCollect)
                        (cell as IGridCollect).bSummary = false ;
                    
                    if (cell is ICalculator && note != "")
                        SetOperatorType(cell as ICalculator, note);

                    if (cell is IDecimal)
                    {
                        (cell as IDecimal).bShowWhenZero = StimulateBoolean.False ;
                        if(isize == 100)
                            (cell as IDecimal).Precision = PrecisionType.ExchangeRate;
                    }                    

                    cell.Width = ConvertVBTiwpToPixel(width,false);
                    cell.SetY( DefaultConfigs.SECTIONHEADERHEIGHT+10  + (levels-1)*defaultheight);
                    _detailsection.Cells.AddDirectly(cell);
                    #endregion

                    #region superlabel & caption   
                    string cncaption = null;
                    while (SetCaption(orderex,cnreader,ref cncaption))
                    {
                        int titletopex = Convert.ToInt32(cnreader["TopEx"]);
                        int childsize = Convert.ToInt32(cnreader["Width"]);
                        int height = Convert.ToInt32(cnreader["Height"]);
                        if (childsize > 1)
                        {
                            #region super
                            int titletopex1 = Convert.ToInt32(cnreader["TopEx"]);
                            SuperLabel sl = new SuperLabel();
                            sl.Name = "SuperLabel" + orderex.ToString() + titletopex1.ToString();
                            sl.SetY( DefaultConfigs.SECTIONHEADERHEIGHT+10  + (titletopex1 - 1) * defaultheight);
                            if (cncaption != null)
                            {
                                sl.Caption = cncaption;
                                sl.ENCaption = encaptions.Contains(key + "_Super") ? encaptions[key + "_Super"].ToString() : cncaption;
                                sl.TWCaption = twcaptions.Contains(key + "_Super") ? twcaptions[key + "_Super"].ToString() : cncaption;
                            }
                            htsuper.Add(titletopex1.ToString(), sl);
                            if (htsuper.Contains(Convert.ToString(titletopex1 - 1)))
                                ((SuperLabel)htsuper[Convert.ToString(titletopex1 - 1)]).Cells.AddDirectly (sl);
                            _detailsection.Cells.AddDirectly (sl);
                            while (SetCaption(orderex, cnreader, ref cncaption))
                            {
                                int titletopex2 = Convert.ToInt32(cnreader ["TopEx"]);
                                childsize = Convert.ToInt32(cnreader ["Width"]);
                                height = Convert.ToInt32(cnreader["Height"]);
                                if (childsize > 1)
                                {
                                    sl = new SuperLabel();
                                    sl.Name = "SuperLabel" + orderex.ToString() + titletopex2.ToString();
                                    sl.SetY( DefaultConfigs.SECTIONHEADERHEIGHT+10  + (titletopex2 - 1) * defaultheight);
                                    if (cncaption != null)
                                    {
                                        sl.Caption = cncaption;
                                        sl.ENCaption = encaptions.Contains(key + "_Super") ? encaptions[key + "_Super"].ToString() : cncaption;
                                        sl.TWCaption = twcaptions.Contains(key + "_Super") ? twcaptions[key + "_Super"].ToString() : cncaption;
                                    }
                                    htsuper.Add(titletopex2.ToString(), sl);
                                    if (htsuper.Contains(Convert.ToString(titletopex2 - 1)))
                                        ((SuperLabel)htsuper[Convert.ToString(titletopex2 - 1)]).Cells.AddDirectly (sl);
                                    _detailsection.Cells.AddDirectly(sl);
                                }
                                else
                                {
                                    if (cncaption != null)
                                    {
                                        cell.Caption = cncaption;
                                        cell.ENCaption = encaptions.Contains(key) ? encaptions[key].ToString() : cncaption;
                                        cell.TWCaption = twcaptions.Contains(key) ? twcaptions[key].ToString() : cncaption;
                                    }
                                    if (column != null)
                                    {
                                        if (cncaption != null)
                                        {
                                            column.DescriptionCN = cncaption;
                                            column.DescriptionUS  = encaptions.Contains(key) ? encaptions[key].ToString() : cncaption;
                                            column.DescriptionTW  = twcaptions.Contains(key) ? twcaptions[key].ToString() : cncaption;
                                        }
                                    }
                                    if (htsuper.Contains(Convert.ToString(titletopex2 - 1)))
                                        ((SuperLabel)htsuper[Convert.ToString(titletopex2 - 1)]).Cells.AddDirectly(cell);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            if (cncaption != null)
                            {
                                cell.Caption = cncaption;
                                cell.ENCaption = encaptions.Contains(key) ? encaptions[key].ToString() : cncaption;
                                cell.TWCaption = twcaptions.Contains(key) ? twcaptions[key].ToString() : cncaption;
                            }
                            if (column != null)
                            {
                                if (cncaption != null)
                                {
                                    column.DescriptionCN = cncaption;
                                    column.DescriptionUS = encaptions.Contains(key) ? encaptions[key].ToString() : cncaption;
                                    column.DescriptionTW = twcaptions.Contains(key) ? twcaptions[key].ToString() : cncaption;
                                }
                            }
                            if (htsuper.Contains(Convert.ToString(titletopex - 1)))
                                ((SuperLabel)htsuper[Convert.ToString(titletopex - 1)]).Cells.AddDirectly(cell);
                        }
                    }
                    #endregion
                }
                reader.Close();
                cnreader.Close();
            }

            #region add topex=1 and modeex=0 to tablecolumns
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                "select expression,orderex,isize,formatex,note from "+tablename+" where id=" + _oldreportid + " and modeex=0 and topex=1  and localeid='zh-CN' order by orderex asc"))
            {
                while (reader.Read())
                {
                    string expression = reader["Expression"].ToString().Trim();
                    if (expression == "")
                        continue;
                    int orderex = Convert.ToInt32(reader["OrderEx"]);
                    int isize = Convert.ToInt32(reader["iSize"]);
                    string formatex = reader["formatex"].ToString();
                    string note = reader["note"].ToString();
                    TableColumn column = GetTableColumn(expression);
                    if (column == null)
                    {
                        column = new TableColumn();
                        column.Name = expression;
                        column.DataType = GetDAEDataType(isize);
                        _columns.Add(column);
                    }

                    column.DescriptionCN = GetTop1Caption("zh-CN", orderex,IsBak);
                    column.DescriptionTW = GetTop1Caption("zh-TW", orderex,IsBak);
                    if (column.DescriptionTW == null)
                        column.DescriptionTW = column.DescriptionCN;
                    column.DescriptionUS  = GetTop1Caption("en-US", orderex,IsBak);
                    if (column.DescriptionUS == null)
                        column.DescriptionUS = column.DescriptionCN;

                    string columnname = column.Name.ToLower();
                    if (_report.ViewID == "ST[__]货位存量查询")
                    {
                        if (columnname == "cbatchproperty1" || 
                            columnname == "cbatchproperty2" ||
                            columnname == "cbatchproperty3" ||
                            columnname == "cbatchproperty4" ||
                            columnname == "cbatchproperty5" )
                            column.DataType = DataTypeEnum.Decimal;
                        else if(columnname == "cbatchproperty10")                        
                            column.DataType = DataTypeEnum.DateTime;
                    }
                    else if (_report.ViewID == "SA[__]发货明细表")
                    {
                        if (columnname == "cdefine1")
                            column.DataType = DataTypeEnum.String;
                    }

                    if (!_detailsection.Cells.Contains(column.Name))
                    {
                        Cell cell = _detailsection.GetDefaultRect(GetDataSource(column));
                        cell.Caption = column.DescriptionCN;
                        cell.ENCaption = column.DescriptionUS;
                        cell.TWCaption = column.DescriptionTW;
                        cell.Visible = false;

                        if (cell is IFormat)
                            SetPrecisionAndFormatString(cell as IFormat, formatex);
                        if (cell is IGridCollect)
                            (cell as IGridCollect).bSummary = false;
                        if (cell is ICalculator && note != "")
                            SetOperatorType(cell as ICalculator, note);
                        if (cell is IDecimal)
                            (cell as IDecimal).bShowWhenZero = StimulateBoolean.False;

                        cell.SetY(DefaultConfigs.SECTIONHEADERHEIGHT + 10 + (levels - 1) * defaultheight);
                        _detailsection.Cells.AddDirectly(cell);
                    }
                }
                reader.Close();
            }

            //try
            //{
            //    if (_report.ViewID == "ST[__]货位存量查询")
            //    {
            //        _columns["cBatchProperty1"].DataType = DataTypeEnum.Decimal;
            //        _columns["cBatchProperty2"].DataType = DataTypeEnum.Decimal;
            //        _columns["cBatchProperty3"].DataType = DataTypeEnum.Decimal;
            //        _columns["cBatchProperty4"].DataType = DataTypeEnum.Decimal;
            //        _columns["cBatchProperty5"].DataType = DataTypeEnum.Decimal;
            //        _columns["cBatchProperty10"].DataType = DataTypeEnum.DateTime;
            //    }
            //    else if (_report.ViewID == "SA[__]发货明细表")
            //    {
            //        _columns["cdefine1"].DataType = DataTypeEnum.String ;
            //    }
            //}
            //catch
            //{
            //}
            #endregion

            #region cross remove last handle
            if (crosscolumnheaderitem != null
                && !bCGSpecial)
            {
                Cell lastcell = _detailsection.Cells[_detailsection.Cells.Count - 1];
                if (lastcell != null && lastcell is IDataSource)
                {
                    _detailsection.Cells.Remove(lastcell);
                    TableColumn lastcolumn = GetTableColumn((lastcell as IDataSource).DataSource.Name);
                    if (lastcolumn != null)
                        _columns.Remove(lastcolumn);
                }
            }

            if (_report.ViewID.ToLower() == "sa[__]货龄分析(按发票)")
            {
                TableColumn column = GetTableColumn("货龄0-20_金额");
                if (column != null)
                    _columns.Remove(column);
                column = GetTableColumn("货龄0-20_金额比率");
                if (column != null)
                    _columns.Remove(column);
                column = GetTableColumn("货龄21以上_金额");
                if (column != null)
                    _columns.Remove(column);
                column = GetTableColumn("货龄21以上_金额比率");
                if (column != null)
                    _columns.Remove(column);
                Cell cell = _detailsection.Cells["货龄0-20_金额比率"];
                if (cell != null)
                    _detailsection.Cells.Remove(cell);
            }
            else if (bCGSpecial)
            {//iAPrice,数量,1至30天,31天以上
                TableColumn column = GetTableColumn("1至30天");
                if (column != null)
                    _columns.Remove(column);
                column = GetTableColumn("31天以上");
                if (column != null)
                    _columns.Remove(column);
                Cell cell = _detailsection.Cells["1至30天"];
                if (cell != null)
                    _detailsection.Cells.Remove(cell);
                cell = _detailsection.Cells["31天以上"];
                if (cell != null)
                    _detailsection.Cells.Remove(cell);
                cell = _detailsection.Cells["iAPrice"];
                if (cell != null && cell is IGridCollect)
                    (cell as IGridCollect).bClue = true;
                cell = _detailsection.Cells["数量"];
                if (cell != null && cell is IGridCollect)
                    (cell as IGridCollect).bClue = true;
                cell = _detailsection.Cells["件数"];
                if (cell != null && cell is IGridCollect)
                    (cell as IGridCollect).bClue = true;
                cell = _detailsection.Cells["ConverRate"];
                if (cell != null && cell is IGridCollect)
                    (cell as IGridCollect).bClue = true;
            }
            #endregion

            (_detailsection as IAutoDesign).AutoDesign(10);
            (_detailsection as IAutoDesign).AutoDesignSuperLabel();

            #region handle sum
            using(SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                "select name,nameforeign,orderex from "+tablename+" where id=" + _oldreportid + " and modeex=4 and localeid='zh-CN' and orderex<>-1"))
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString().Trim();
                    string nameforeign = reader["nameforeign"].ToString().Trim();
                    string caption = (nameforeign == "" ? name : nameforeign);
                    int orderex = Convert.ToInt32(reader["orderEx"]);
                    Cell cell = _detailsection.Cells.GetByCaption(caption);
                    if (cell == null)
                        cell = _detailsection.Cells[name];
                    if (cell != null && cell is IGridCollect )
                    {
                        switch (orderex)
                        {
                            case 0:
                                (cell as IGridCollect).bSummary = true;
                                break;
                            case 1:
                                (cell as IGridCollect).bSummary = true;
                                (cell as IGridCollect).Operator = OperatorType.ExpressionSUM;
                                break;
                            case 2:
                                (cell as IGridCollect).bSummary = true;
                                (cell as IGridCollect).Operator = OperatorType.BalanceSUM;
                                break;
                        }
                    }
                }
                reader.Close();
            }
            #endregion

            #region handle group
            GroupSchema gs = new GroupSchema();
            gs.ID = Guid.NewGuid().ToString();
            gs.SetName("zh-CN", "缺省分组");
            gs.SetName("zh-TW", "缺省分組");
            gs.SetName("en-US", "Default Group");
            gs.bDefault = true;
            GroupSchemaItem gsi = null;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(@"select A.orderex,A.isize,A.Note,B.Expression from {1} A inner join {1} B on A.Name=B.Name and A.ID=B.ID and A.LocaleID=B.LocaleID where A.Modeex=3 and A.orderex<>-2 and B.Modeex=0 and B.TopEx<>1 and A.localeid='zh-CN' and A.ID={0} order by A.orderex", _oldreportid,tablename)))
            {
                while (reader.Read())
                {
                    int orderex = Convert.ToInt32(reader["orderEx"]);
                    string expression=reader["Expression"].ToString();
                    if (orderex == -1)
                    {
                        gsi = new GroupSchemaItem();
                        gsi.Items.Add(expression);
                        gs.SchemaItems.Add(gsi);
                        gsi = null;
                    }
                    else if (orderex == 0)
                    {
                        if (gsi == null)
                            gsi = new GroupSchemaItem();
                        gsi.Items.Add(expression);
                        if (reader["Note"].ToString().ToLower().Trim() == "grouphold")
                        {
                            if (_report.SolidGroup != "")
                                _report.SolidGroup += ",";
                            _report.SolidGroup += expression;
                        }
                    }
                    #region isize
                    //if (cell != null)
                    //{
                    //    if (orderex == 0)
                    //    {
                    //        gsi.Items.Add(cell.Name);
                    //    }
                    //    else if (orderex == -1 && cell is IGridCollect)
                    //    {
                    //        int isize = Convert.ToInt32(reader["isize"]);
                    //        if (isize == 1)
                    //        {
                    //            (cell as IGridCollect).bSummary = true;
                    //            (cell as IGridCollect).Operator = OperatorType.AccumulateSUM;
                    //        }
                    //        else
                    //        {
                    //            (cell as IGridCollect).bSummary = true;
                    //            (cell as IGridCollect).Operator = OperatorType.SUM;
                    //        }
                    //    }
                    //}
                    #endregion
                }
                if(gsi!=null)
                    gs.SchemaItems.Add(gsi);
                reader.Close();
            }
            if (gs.SchemaItems.Count > 0)
            {
                string viewid=_report.ViewID.ToLower();
                if ( viewid == "sa[__]销售综合统计表")//
                {
                    if(gs.SchemaItems.Count==2)
                        gs.SchemaItems[1].Items.Add("单据类型");
                }
                else if (viewid == "sa[__]销售收入明细账" )//
                {
                    if (gs.SchemaItems.Count == 3)
                        gs.SchemaItems[2].Items.Add("SBVID");
                }
                else if (viewid == "sa[__]销售明细账")
                {
                    if (gs.SchemaItems.Count == 4)
                        gs.SchemaItems[3].Items.Add("SBVID");
                }
                else if (viewid == "sa[__]发货明细表")
                {
                    if (gs.SchemaItems.Count == 2)
                        gs.SchemaItems[1].Items.Add("DLID");
                }
                gs.bShowDetail = defaultshowdetail;
                gs.bGroupItemsAhead = true;
                _report.GroupSchemas.Add(gs);
            }

            #region solidgroup
            //if ((_subid.ToLower() == "ar" && _reportid == "应收总账表")
            //    || (_subid.ToLower() == "ap" && _reportid == "应付总账表"))
            //    _report.InitEvent = "report.Args.Add(\"SolidGroup1\",\"\");\r\nreport.Args.Add(\"SolidSort\",1);";
            //else if ((_subid.ToLower() == "ar" && _reportid == "应收明细账")
            //    || (_subid.ToLower() == "ap" && _reportid == "应付明细账"))
            //    _report.InitEvent = "report.Args.Add(\"SolidGroup1\",\"vid\");\r\nreport.Args.Add(\"SolidSort\",1);";
            //else if ((_subid.ToLower() == "ar" && _reportid == "应收对账单")
            //    || (_subid.ToLower() == "ap" && _reportid == "应付对账单"))
            //    _report.InitEvent = "report.Args.Add(\"SolidGroup1\",\"vtype,vid,ccancelno,exchname,csysid,dRDate\");\r\nreport.Args.Add(\"SolidSort\",1);";
            #endregion
            #endregion

            #region handle levelexpand
            ReportLevelExpand rle=new ReportLevelExpand();
            rle.Name = "缺省级次展开";
            rle.IsDefault=true;           
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(
                @"Select Name,issingle,itop,
				ColumnName=
				CASE 
					WHEN CHARINDEX(N',',Caption)>0 
					AND EXISTS(
					SELECT Name FROM {1} 
					WHERE LocaleID='zh-CN' 
					AND ID=A.ID
					AND ModeEx=0
					AND Expression=SUBSTRING(Caption,CHARINDEX(N',',Caption)+1,LEN(Caption)-CHARINDEX(N',',Caption))
					)
					THEN SUBSTRING(Caption,CHARINDEX(N',',Caption)+1,LEN(Caption)-CHARINDEX(N',',Caption))
					
					WHEN CHARINDEX(N',',Caption)<=0 
					THEN Caption
					ELSE ''
				END
				FROM Rpt_FltDef_Base A
				WHERE ID=N'{0}'
				AND LocaleID=N'zh-CN' 
				AND Flag=1
				AND ModeEx=20",
                _oldreportid,tablename )))
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString().Trim();
                    string columnname = reader["columnname"].ToString().Trim();
                    int issingle = Convert.ToInt32(reader["issingle"]);
                    int itop = Convert.ToInt32(reader["itop"]);
                    LevelExpandEnum type = GetLevelExpandType(name);
                    LevelExpandItem lei = new LevelExpandItem(type);
                    lei.ColumnName = columnname;
                    _report.ExpandSchema.DesignTimeLevelExpandItems.Add(lei);
                    if (issingle == 1)
                    {
                        lei = new LevelExpandItem(type);
                        lei.ColumnName = columnname;
                        lei.Depth = itop;
                        rle.AddLevelExpand(lei);
                    }
                }
                reader.Close();
            }
            if (rle.LevelExpandItems.Count > 0)
                _report.ExpandSchema.ReportLevelExpands.Add(rle);
            #endregion
            #endregion

            #region selfaction
            if (_subid.ToLower() != "qm" && _report.ViewID.ToLower() != "pu[__]供应商存货价格分析")
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                    string.Format(
                    @"select langid,name,caption,isnull(showontoolbar,1) as showontoolbar,isnull(doubleclick,0) as doubleclick,tooltip,actionclass from Rpt_CustomEvents_Base where reportname='{0}' and csub_id='{1}'",
                    _basereportid, _subid)))
                {
                    while (reader.Read())
                    {
                        string name = reader["name"].ToString();
                        SelfAction sa = _report.SelfActions[name];
                        if (sa == null)
                        {
                            sa = new SelfAction();
                            sa.Name = name;
                            sa.ActionClass = reader["actionclass"].ToString();
                            sa.bDoubleClickAction = Convert.ToBoolean(reader["doubleclick"]);
                            sa.bShowCaptionOnToolBar = Convert.ToBoolean(reader["showontoolbar"]);
                            _report.SelfActions.Add(sa);
                        }
                        string localid = reader["langid"].ToString().ToLower();
                        string caption = reader["caption"].ToString();
                        //if(caption.Trim()=="")
                        //    caption=name;
                        if (localid == "zh-cn")
                        {
                            sa.CnCaption = caption;
                            sa.CnTip = reader["tooltip"].ToString();
                        }
                        else if (localid == "en-us")
                        {
                            sa.EnCaption = caption;
                            sa.EnTip = reader["tooltip"].ToString();
                        }
                        else
                        {
                            sa.TwCaption = caption;
                            sa.TwTip = reader["tooltip"].ToString();
                        }
                    }
                    reader.Close();
                }
            }
            #endregion

            #region othersection
            int irh = 0, iph = 0, ipf1 = 0,ipf2=0;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(
                @"select Name,topex from {1} where id='{0}' and modeex=25 and localeid='zh-CN' order by topex", _oldreportid,tablename)))
            {
                while (reader.Read())
                {
                    string name = reader["Name"].ToString();
                    Section section=null;
                    switch (name)
                    {
                        case "标题区域"://reportheader
                            if (irh == 0)
                            {
                                irh++;
                                section = _report.Sections[SectionType.ReportHeader];
                                if (section == null)
                                {
                                    section = new ReportHeader();
                                    _report.Sections.Add(section);
                                }
                            }
                            break;
                        case "页标题区域"://pageheader
                            if (iph == 0)
                            {
                                iph++;
                                section = new PageHeader();
                                _report.Sections.Add(section);
                            }
                            break;
                        case "正文区域":
                            break;
                        case "页脚注区域"://pagefooter
                            if (ipf1 == 0)
                            {
                                ipf1++;
                                section = _report.Sections[SectionType.PageFooter];
                                if (section == null)
                                {
                                    section = new PageFooter();
                                    _report.Sections.Add(section);
                                }
                            }
                            break;
                        case "脚注区域":
                            if (ipf2 == 0)
                            {
                                ipf2++;
                                section = _report.Sections[SectionType.PageFooter];
                                if (section == null)
                                {
                                    section = new PageFooter();
                                    _report.Sections.Add(section);
                                }
                            }
                            break;
                        default :
                            if (name.StartsWith("[分组]"))
                            {
                                if (name.EndsWith("_标题"))//reportheader
                                {
                                    //_report.bPageByGroup = true;
                                    section = _report.Sections[SectionType.PrintPageTitle ];
                                    if (section == null)
                                    {
                                        section = new PrintPageTitle ();
                                        _report.Sections.Add(section);
                                    }
                                }
                                else if (name.EndsWith("_脚注"))//pagefooter
                                {
                                    section = _report.Sections[SectionType.PrintPageSummary  ];
                                    if (section == null)
                                    {
                                        section = new PrintPageSummary ();
                                        _report.Sections.Add(section);
                                    }
                                }
                            }
                            break;
                    }
                    if (section != null)
                    {
                        AddLabels(name, section);                        
                    }
                }
                reader.Close();
            }
            #endregion
        }

        #region private functions

        private string ReplaceCondition(string condition)
        {
            return condition.Replace("[", "").Replace("]", "").Replace("（", "(").Replace("）", ")");
        }
        private string GetTop1Caption(string localeid, int orderex,bool IsBak)
        {
            string tablename = string.Empty;
            if (IsBak)
                tablename = "rpt_flddef_base_bak";
            else
                tablename = "rpt_flddef_base";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(
                @"SELECT top 1 name,nameforeign	FROM {3} 
				WHERE id=N'{0}' AND LocaleId=N'{1}' 
				AND ModeEx=5 AND orderex=N'{2}' ",
                _oldreportid, localeid, orderex,tablename)))
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    string nameforeign = reader["nameforeign"].ToString();
                    return nameforeign == "" ? name : nameforeign;
                }
                reader.Close();
            }
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(
                @"SELECT top 1 name,nameforeign	FROM {3} 
				WHERE id=N'{0}' AND LocaleId=N'{1}' 
				AND ModeEx=0 and topex=1 AND orderex=N'{2}' ",
                _oldreportid, localeid, orderex,tablename)))
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    string nameforeign = reader["nameforeign"].ToString();
                    return nameforeign  == "" ? name : nameforeign ;
                }
                reader.Close();
            }
            return null;
        }

        private bool bCGSpecial
        {
            get
            {
                return (_subid.ToLower() == "pu" && _reportid == "采购货龄综合分析");
            }
        }

        private bool bSALocaleSpecial
        {
            get
            {
                return (_subid.ToLower() == "sa" && (_reportid == "委托代销统计表" || _reportid == "销售增长分析"));
            }
        }
        private bool bPULocaleSpecial
        {
            get
            {
                return (_subid.ToLower() == "pu" && (_reportid == "采购成本分析"));
            }
        }

        private string HandleCondition(string condition,string localeid)
        {
            if (bSALocaleSpecial)
                condition = condition.ToLower().Replace("period-beginning", "period&beginning");
            else if (bPULocaleSpecial)
                condition = condition.ToLower().Replace("increase-decrease", "increase&decrease").Replace("standard/sales", "standard&sales");
            StringBuilder sb = new StringBuilder();
            char[] seps = new char[] { '+', '-', '*', '/', '(', ')' };
            while (true)
            {
                int index = condition.IndexOfAny(seps);
                string rname = null;
                if (index == -1)
                {
                    if (bSALocaleSpecial)
                        condition = condition.Replace("period&beginning", "period-beginning");
                    else if (bPULocaleSpecial)
                        condition = condition.Replace("increase&decrease", "increase-decrease").Replace("standard&sales", "standard/sales");
                    rname = ReplaceName(condition, localeid);
                    sb.Append(string.IsNullOrEmpty(rname) ? condition : rname);
                    break;
                }
                string c1 = condition.Substring(0, index).Trim();
                if (bSALocaleSpecial)
                    c1 = c1.Replace("period&beginning", "period-beginning");
                else if (bPULocaleSpecial)
                    c1 = c1.Replace("increase&decrease", "increase-decrease").Replace("standard&sales", "standard/sales");
                rname = ReplaceName(c1, localeid);
                sb.Append(string.IsNullOrEmpty(rname) ? c1 : rname);
                sb.Append(condition.Substring(index, 1));
                condition = condition.Substring(index + 1).Trim();
            }
            return sb.ToString();
        }

        private string ReplaceName(string condition,string localeid)
        {
            string rname = null;
            string rcondition = null;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, "select top 1 expression,condition from rpt_flddef_base where name='" + condition + "' and id=" + _oldreportid + " and modeex=0 and localeid='"+localeid +"'"))
            {
                while (reader.Read())
                {
                    rname = reader["expression"].ToString();
                    rcondition = ReplaceCondition(reader["condition"].ToString().Trim());
                }
                reader.Close();
            }
            if (string.IsNullOrEmpty(rname) && !string.IsNullOrEmpty(rcondition))
                rname = "("+HandleCondition(rcondition,localeid )+")";
            return rname;
        }

        private int tmpcnleft=-1;
        private string tmpcncaption=null;
        private bool SetCaption(int leftex,SqlDataReader cnreader, ref string cncaption)
        {
            bool bfind = false;
            cncaption = null;
            string name=null;
            string nameforeign=null;
            #region cncaption
            if (tmpcnleft==leftex )
            {
                cncaption = tmpcncaption;
                tmpcnleft = -1;
                bfind = true;
            }
            else if (tmpcnleft<leftex)
            {
                while (cnreader.Read())
                {
                    name = cnreader["name"].ToString();
                    nameforeign = cnreader["nameforeign"].ToString();
                    tmpcnleft =cnreader["leftex"]==DBNull.Value ? leftex: Convert.ToInt32(cnreader["leftex"]);
                    tmpcncaption = nameforeign == "" ? name : nameforeign;
                    if (tmpcnleft == leftex)
                    {
                        cncaption = tmpcncaption;
                        tmpcnleft = -1;
                        bfind = true;
                        break;
                    }
                    else if (tmpcnleft > leftex)
                        break;
                }
            }
            #endregion
            return bfind;
        }

        private int ConvertVBTiwpToPixel(int value,bool bhorizontal)
        {
            if (bhorizontal)
                return Convert.ToInt32(value / 56 * 3.6);
            else
                return value / 56 * 4;
        }

        private string GetEnSectionName(string sectionname)
        {
            switch (sectionname)
            {
                case "正文区域":
                    return "Body region";
                case "标题区域":
                    return "Header region";
                case "脚注区域":
                    return "Footnote area";
                case "页标题区域":
                    return "Page title area";
                case "页脚注区域":
                    return "Page footnote area";
                case "正文区":
                    return "Body region";
                case "页尾区":
                    return "Page footer region";
                case "分组脚注区":
                    return "Grouping footer region";
                case "脚注区":
                    return "Footer region";
                case "标题区":
                    return "Header region";
                case "分组标题区":
                    return "Grouping header region";
                default:// "页眉区":
                    return "Page header region";
            }
        }

        private string GetTWSectionName(string sectionname)
        {
            switch (sectionname)
            {
                case "正文区域":
                    return "正文區域";
                case "标题区域":
                    return "標題區域";
                case "脚注区域":
                    return "腳注區域";
                case "页标题区域":
                    return "頁標題區域";
                case "页脚注区域":
                    return "頁腳注區域";
                case "正文区":
                    return "正文區";
                case "页尾区":
                    return "頁尾區";
                case "分组脚注区":
                    return "分組腳注區";
                case "脚注区":
                    return "腳注區";
                case "标题区":
                    return "標題區";
                case "分组标题区":
                    return "分組標題區";
                default:// "页眉区":
                    return "頁眉區";
            }
        }

        //private bool bLabelsSpecial
        //{
        //    get
        //    {
        //        return (_subid.ToLower() == "mo" && (_reportid == "MO04013"));//补料申请单明细表
        //    }
        //}

        private string GetNameForeignFilter(string sectionname)
        {
            //if (bLabelsSpecial)
            //    return "";
            //else

                return " AND nameForeign='" + sectionname + "'";
        }

        private void AddLabels(string sectionname,Section section)
        {
            //if (sectionname.StartsWith("[分组]") && sectionname.EndsWith("_标题"))
            //    bpagebygrouplabel = true;
            int basey = DefaultConfigs.SECTIONHEADERHEIGHT + 10;
            if (section.Tag != null)
                basey = (int)section.Tag;
            int nexty = basey;
            int index = 0;
            SimpleArrayList orderal = new SimpleArrayList();
            SimpleHashtable ens = new SimpleHashtable();
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(
                @"SELECT Name,Expression, FormatEx ,orderex 
				FROM rpt_flddef_base 
				WHERE id=N'{0}' AND LocaleId=N'{1}' 
				AND ModeEx=N'26' {2} and width>0 
				ORDER BY OrderEx,topex",
                _oldreportid,
                "en-US",
                GetNameForeignFilter(GetEnSectionName(sectionname)))))
            {
                while (reader.Read())
                {
                    string formatex = reader["FormatEx"].ToString();
                    if (formatex.Trim() == "只打印")//隐含,只显示,显示/打印
                        continue;
                    int orderex = Convert.ToInt32(reader["orderex"]);
                    if (orderal.Contains(orderex.ToString()))
                        continue;
                    orderal.Add(orderex.ToString());
                    string name = reader["name"].ToString();
                    string expression = reader["expression"].ToString();
                    ens.Add(index.ToString(), (expression == "" ? name : expression));
                    index++;
                }
            }
            index = 0;
            orderal.Clear();
            SimpleHashtable tws = new SimpleHashtable();
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(
                @"SELECT Name,Expression , FormatEx ,orderex 
				FROM rpt_flddef_base 
				WHERE id=N'{0}' AND LocaleId=N'{1}' 
				AND ModeEx=N'26' {2} and width>0 
				ORDER BY OrderEx,topex",
                _oldreportid,
                "zh-TW",
                GetNameForeignFilter(GetTWSectionName(sectionname)))))
            {
                while (reader.Read())
                {
                    string formatex = reader["FormatEx"].ToString();
                    if (formatex.Trim() == "只打印")//隐含,只显示,显示/打印
                        continue;
                    int orderex = Convert.ToInt32(reader["orderex"]);
                    if (orderal.Contains(orderex.ToString()))
                        continue;
                    orderal.Add(orderex.ToString());
                    string name = reader["name"].ToString();
                    string expression = reader["expression"].ToString();
                    tws.Add(index.ToString(), expression == "" ? name : expression);
                    index++;
                }
            }
            index = 0;
            orderal.Clear();
            
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(
                @"SELECT Name,Expression,CONditiON,OrderEx,TopEx,LeftEx,
				Width,Height,Visible,Note,iAlignStyle,FormatEx
				FROM rpt_flddef_base 
				WHERE id=N'{0}' AND LocaleId=N'{1}' 
				AND ModeEx=N'26' {2} and width>0 
				ORDER BY OrderEx,topex",
                _oldreportid,
                "zh-CN",
                GetNameForeignFilter(sectionname))))
            {
                //int iexwidth = 160;
                //Hashtable htopex = new Hashtable();
                //int ileftno = 0;
                while (reader.Read())
                {
                    string formatex = reader["FormatEx"].ToString();
                    if (formatex.Trim() == "只打印")//隐含,只显示,显示/打印
                        continue;

                    int orderex = Convert.ToInt32(reader["orderex"]);
                    if (orderal.Contains(orderex.ToString()))
                        continue;
                    orderal.Add(orderex.ToString());
                    int topex = Convert.ToInt32(reader["topex"]);
                    int leftex = Convert.ToInt32(reader["leftex"]);
                    //if (!htopex.Contains(topex))
                    //{
                    //    htopex.Add(topex, 0);
                    //}
                    //else
                    //{
                    //    ileftno = int.Parse(htopex[topex].ToString());
                    //    ileftno++;
                    //    htopex.Remove(topex);
                    //    leftex = leftex + iexwidth * ileftno;
                    //    htopex.Add(topex, ileftno);
                    //}
                    //int width = Convert.ToInt32(reader["width"]) + iexwidth;
                    int width = Convert.ToInt32(reader["width"]);
                    int height = Convert.ToInt32(reader["height"]);
                    string align = reader["ialignstyle"].ToString();
                    string[] condition = reader["Condition"].ToString().Split(',');
                    string[] notes = reader["Note"].ToString().Split(';');

                    string name = reader["name"].ToString();
                    string expression = reader["expression"].ToString();
                    string cncaption = (expression == "" ? name : expression);
                    string encaption = "";
                    if (ens.Contains(index.ToString()))
                        encaption = ens[index.ToString()].ToString();
                    string twcaption = "";
                    if (tws.Contains(index.ToString()))
                        twcaption = tws[index.ToString()].ToString();

                    string type = "";
                    //string sFormatString = "";
                    //string FormulaTypeSource = "";
                    if (notes.Length >= 3)
                    {
                        type = notes[0].Trim();
                        //sFormatString = notes[1].Trim();
                        //sFormatString = sFormatString.Replace("yyyy-mm-dd", "yyyy-MM-dd");
                        //sFormatString = sFormatString.Replace("YYYY-MM-DD", "yyyy-MM-dd");
                    }

                    Cell cell = HandleALabel(type, reader["Expression"].ToString(), orderex, topex, leftex, cncaption, encaption, twcaption);
                    if (cell != null)
                    {
                        cell.Name = "Label" + orderex.ToString() + topex.ToString() + leftex.ToString();
                        cell.CaptionAlign = GetAlign(align);
                        cell.KeepPos = true;
                        cell.Border = new BorderSide(false, false, false, false);
                        if (condition.Length > 7)
                            cell.ServerFont = GetServerFont(condition);

                        int labely = basey + ConvertVBTiwpToPixel(topex, false);
                        int labelheight = ConvertVBTiwpToPixel(height, false);

                        if (cell is ICenterAlign && (cell as ICenterAlign).CenterAlign)
                        {
                            cell.ServerFont.Bold = false;
                            cell.ServerFont.FontSize = 15;
                            cell.ServerFont.FontName = "黑体";

                            int delta = labelheight - 45;
                            if (delta > 0)
                                delta = 0;
                            labelheight = 45;
                            labely += delta;
                        }
                        cell.X = ConvertVBTiwpToPixel(leftex, true);
                        
                        cell.SetY(labely);
                        if (cell.Width != 500)
                            cell.Width = ConvertVBTiwpToPixel(width, true);
                        if (cell.Width > 500)
                            cell.Width = 500;
                        if (cell is Expression)
                            cell.X += 15;
                        else
                            cell.Width += 15;

                        cell.Height = labelheight;

                        if (cell.Y + cell.Height > nexty)
                            nexty = cell.Y + cell.Height;
                        //if (cell is IPageByGroupLabel)
                        //    (cell as IPageByGroupLabel).bPageByGroupLabel = bpagebygrouplabel;

                        section.Cells.AddDirectly(cell);
                    }
                    index++;
                }
                reader.Close();
            }
            section.Tag = nexty;
        }

        private Cell HandleALabel(string type, string expression,int orderex,int topex,int leftex,string cncaption,string encaption,string twcaption)
        {
            switch (type.ToLower())
            {
                case "公式":
                    return HandleInnerFormula(expression, orderex, topex);
                case "SQL查询":
                    Cell cell = new CommonLabel ();
                    cell.Caption = cncaption;// GetLabelCaption("zh-CN", 26, orderex, topex, leftex);
                    cell.ENCaption = encaption;// GetLabelCaption("en-US", 26, orderex, topex, leftex);
                    cell.TWCaption = twcaption;// GetLabelCaption("zh-TW", 26, orderex, topex, leftex);
                    cell.PrepaintEvent = "cell.Caption=global.ExecuteScalar(\u0022"+expression+"\u0022);";
                    return cell;
                case "表达式":
                    return null;
                default :// "文本":
                    cell = new CommonLabel();
                    cell.Caption = cncaption;// GetLabelCaption("zh-CN", 26, orderex, topex, leftex);
                    cell.ENCaption = encaption;// GetLabelCaption("en-US", 26, orderex, topex, leftex);
                    cell.TWCaption = twcaption;// GetLabelCaption("zh-TW", 26, orderex, topex, leftex);
                    //if (cell.ENCaption == null)
                    //    cell.ENCaption = cell.Caption;
                    //if (cell.TWCaption == null)
                    //    cell.TWCaption = cell.Caption;
                    return cell;
            }
        }

        private ServerFont GetServerFont(string[] condition)
        {
            ServerFont sf = new ServerFont();
            try
            {
                sf.FontName = condition[3].Trim();
                sf.FontSize = Convert.ToSingle(condition[4]);
                sf.Bold = Boolean.Parse(condition[0].Trim());
                sf.Italic = Boolean.Parse(condition[2].Trim());
                sf.StrikethOut = Boolean.Parse(condition[5].Trim());
                sf.UnderLine = Boolean.Parse(condition[6].Trim());
            }
            catch (Exception ex)
            {
                var log = UFIDA.U8.UAP.Services.ReportResource.Logger.GetLogger("kkmeteor");
                log.Info(ex.Message);
                log.Close();
            }
            return sf;
        }

        private ContentAlignment GetAlign(string align)
        {
            switch (align)
            {
                case "2":
                    return ContentAlignment.MiddleCenter ;
                case "3":
                    return ContentAlignment.MiddleRight   ;
                default:
                    return ContentAlignment.MiddleLeft ;
            }
        }

        private Cell HandleInnerFormula(string expression,int orderex,int topex)
        {
            //Sum,GroupSum,PageSum,AccGroupSum,AccPageSum
            //GetFilterValue,GetGroupValue
            //GetReportName,GetSubTitle,GetUserName,GetCopritionName,Date,Month,Year,Day,AccountMonth,AccountYear,Time,Page,Pages,GroupPage,GroupPages
            Cell cell=null;
            if (expression == "GetReportName()" || expression=="GetSubTitle()")
            {
                cell = new CommonLabel();
                (cell as ICenterAlign).CenterAlign = true; 
                cell.Caption = _cnreportname;
                cell.ENCaption = _enreportname;
                cell.TWCaption = _twreportname;
                if (_subid.ToLower() == "pu" && (_reportid == "请购单执行统计表"))
                    cell.Width = 500;
            }
            else if (expression == "GetUserName()" ||
                expression == "GetCopritionName()" ||
                expression == "Date()" ||
                expression == "Month()" ||
                expression == "Year()" ||
                expression == "Day()" ||
                expression == "AccountMonth()" ||
                expression == "AccountYear()" ||
                expression == "Time()" ||
                expression == "Page()" ||
                expression == "Pages()")
            {
                cell = new Expression();
                cell.Caption = expression;
                cell.ENCaption = expression;
                cell.TWCaption = expression;
                (cell as Expression).Formula.Type = FormulaType.Common;
                (cell as Expression).Formula.FormulaExpression = expression;
            }
            else if (expression == "GroupPage()")
            {
                cell = new Expression();
                cell.Caption = "GroupPage()";
                cell.ENCaption = "GroupPage()";
                cell.TWCaption = "GroupPage()";
                (cell as Expression).Formula.Type = FormulaType.Print ;
                (cell as Expression).Formula.FormulaExpression = "GroupPage()";
            }
            else if (expression == "GroupPages()")
            {
                cell = new Expression();
                cell.Caption = "GroupPages()";
                cell.ENCaption = "GroupPages()";
                cell.TWCaption = "GroupPages()";
                (cell as Expression).Formula.Type = FormulaType.Print ;
                (cell as Expression).Formula.FormulaExpression = "GroupPages()";
            }
            else if (expression.StartsWith("GetFilterValue("))
            {
                cell = new Expression();
                cell.Caption = ConvertToNewFilterFuction(expression);
                cell.ENCaption = cell.Caption;
                cell.TWCaption = cell.Caption;
                (cell as Expression).Formula.Type = FormulaType.Filter;
                (cell as Expression).Formula.FormulaExpression = cell.Caption;
            }
            else if (expression.StartsWith("GetGroupValue("))//cellname,runtime from semirow
            {//consider funciton area
                expression = expression.Replace("GetGroupValue(", "GetData(");
                cell = new Expression();
                cell.Caption = expression;
                cell.ENCaption = expression;
                cell.TWCaption = expression;
                (cell as Expression).Formula.Type = FormulaType.Print ;
                (cell as Expression).Formula.FormulaExpression = expression;
            }//Sum,GroupSum,PageSum,AccGroupSum,AccPageSum---user cellname,runtime from semirow
            else if (expression.StartsWith("Sum(") ||
                expression.StartsWith("GroupSum(") ||
                expression.StartsWith("PageSum("))
            {
                if (expression.StartsWith("Sum("))
                    expression = expression.Replace("Sum(", "PageSum(");
                cell = new Expression();
                cell.Caption = expression;
                cell.ENCaption = expression;
                cell.TWCaption = expression;
                (cell as Expression).Formula.Type = FormulaType.Print ;
                (cell as Expression).Formula.FormulaExpression = expression;
            }
            else if (expression.StartsWith("AccGroupSum(") ||
                expression.StartsWith("AccPageSum("))
            {
                if (expression.StartsWith("AccGroupSum("))
                    expression = expression.Replace("AccGroupSum(", "GroupAccSum(");
                if (expression.StartsWith("AccPageSum("))
                    expression = expression.Replace("AccPageSum(", "PageAccSum(");
                cell = new Expression();
                cell.Caption = expression;
                cell.ENCaption = expression;
                cell.TWCaption = expression;
                (cell as Expression).Formula.Type = FormulaType.Print ;
                (cell as Expression).Formula.FormulaExpression = expression;
            }
            return cell;
        }

        private string GetLabelCaption(string localeid, int modeex,int orderex,int topex,int leftex)
        {
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString,
                string.Format(
                @"SELECT top 1 name,Expression	FROM rpt_flddef_base 
				WHERE id=N'{0}' AND LocaleId=N'{1}' 
				AND ModeEx=N'{2}' AND topex=N'{3}' and orderex=N'{4}' and leftex=N'{5}'",
                _oldreportid, localeid, modeex, topex, orderex,leftex )))
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    string expression = reader["expression"].ToString();
                    return expression == "" ? name : expression;
                }
                reader.Close();
            }
            return "";
        }

        //private Cell HandleExpression()
        //{
        //    //label,IIF
        //}

        #region filter expression
        private string ConvertToNewFilterFuction(string sExpression)
        {
            string sReturnVal = string.Empty;

            //	GetFilterValue(*,0,0)转换为GetValue1("*")
            //	GetFilterValue(*,0,1)转换为GetValue2("*")
            //	GetFilterValue(*,2,0)转换为GetName1("*")
            //	GetFilterValue(*,2,1)转换为GetName2("*")
            string[] sTemp1 = sExpression.Split('(');
            if (sTemp1.Length >= 2)
            {
                string[] sTemp2 = sTemp1[1].Split(',');
                if (sTemp2.Length >= 3)
                    sReturnVal = GetFilterFunction(sTemp2);
            }
            return sReturnVal;
        }

        //	GetFilterValue(*,0,0)转换为GetValue1("*")
        //	GetFilterValue(*,0,1)转换为GetValue2("*")
        //	GetFilterValue(*,2,0)转换为GetName1("*")
        //	GetFilterValue(*,2,1)转换为GetName2("*")
        private string GetFilterFunction(string[] sTemp2)
        {
            // 防止类似"1  )"的情况
            string functionIndex = "1";
            bool bTemp1 = (sTemp2[2] == "1)");
            bool bTemp2 = (sTemp2[2].IndexOf("1") != -1);
            bool bTemp3 = (sTemp2[2].IndexOf(")") != -1);
            if (bTemp1 || (bTemp2 && bTemp3))
                functionIndex = "2";
            else
                functionIndex = "1";
            string filterValueString = "";
            string functionName = "GetValue"; //string.Empty;
            string functionName1 = ""; //string.Empty;
            if (sTemp2[1].Trim() == "2" && (sTemp2[2].Trim() == "0)" || sTemp2[2].Trim() == "1)"))
                functionName1 = "GetName";
                filterValueString = GetFilterValueString(sTemp2[0],ref functionName );
                if (functionName1 != "")
                    functionName = functionName1;
            return string.Format("{0}{1}(\"{2}\")", functionName, functionIndex, filterValueString);
        }

        private string GetFilterValueString(string note,ref string functionName)
        {
            string filtervaluestring = string.Empty;
            try
            {
                note = note.Replace("[", string.Empty);
                note = note.Replace("]", string.Empty);
                //-------------
                filtervaluestring = note;
                //-------------
                string sql = string.Format(
                    "select top 1 Name,Expression,modeex from Rpt_FltDEF_Base where ID=(select id from rpt_glbdef_base where systemid=N'{0}' and name=N'{1}' and localeid='zh-CN') and Note=N'{2}' and localeid=N'zh-CN'",
                    _subid,
                    _basereportid ,
                    note);
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, sql))
                {
                    while (reader.Read())
                    {
                        filtervaluestring = reader["name"].ToString();
                        string modeex=reader["Modeex"].ToString();
                        if (modeex=="12" || ( modeex == "1" && !reader["Expression"].ToString().StartsWith("select ", StringComparison.OrdinalIgnoreCase)))
                            functionName = "GetName";
                        else
                            functionName = "GetValue";
                    }
                    reader.Close();
                }
            }
            catch
            {
            }
            return filtervaluestring;
        }
        #endregion

        private string GetPrevAlgorithm(string expression)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("if(currentindex==0)");
            sb.AppendLine("     return ");
            sb.Append(PrevHandle(expression, true));
            sb.Append(";");
            sb.AppendLine("else");
            sb.AppendLine("     return ");
            sb.Append(PrevHandle(expression, false));
            sb.Append(";");
            return sb.ToString();
        }

        private string PrevHandle(string expression, bool bfirst)
        {
            StringBuilder sb = new StringBuilder();
            int index = expression.IndexOfAny(new char[] { '+', '-', '*', '/', '(', ')' });
            if (index == -1)
            {
                AppendPre(sb, expression, bfirst);
                return sb.ToString();
            }
            while(index>-1)
            {
                AppendPre(sb,expression.Substring(0, index).Trim(),bfirst);                
                sb.Append(expression.Substring(index, 1));
                expression = expression.Substring(index + 1);
                index = expression.IndexOfAny(new char[] { '+', '-', '*', '/', '(', ')' });
                if(index==-1)
                    AppendPre(sb, expression, bfirst);
            }
            return sb.ToString();
        }

        private void AppendPre(StringBuilder sb,string pre, bool bfirst)
        {
            if (pre == "" || IsAConst(pre))
                sb.Append(pre);
            else if (pre.IndexOf("PREV_") != -1)
            {
                if (bfirst)
                    sb.Append("0");//sb.Append(pre.Replace("PREV_", "current."));
                else
                    sb.Append(pre.Replace("PREV_", "previous."));
            }
            else
            {
                sb.Append("current.");
                sb.Append(pre);
            }
        }

        private string GetIIFAlgorithm(string expression)
        {
            //begin with "IIF(" and end with ")" 
            string value = expression.Substring(4, expression.Length - 5);
            value = value.Replace(" OR ", " || ");
            value = value.Replace(" Or ", " || ");
            value = value.Replace(" or ", " || ");
            value = value.Replace(" AND ", " && ");
            value = value.Replace(" And ", " && ");
            value = value.Replace(" and ", " && ");
            value = value.Replace("<>", "!=");

            // 先代替含有"="的运算符
            value = value.Replace("<=", "<<");
            value = value.Replace(">=", ">>");
            value = value.Replace("!=", "!!");

            value = value.Replace("=", "==");

            // 把已经代替含有"="的运算符换回来
            value = value.Replace("<<", "<=");
            value = value.Replace(">>", ">=");
            value = value.Replace("!!", "!=");

            string[] keys = value.Split(',');
            if (keys == null || keys.Length != 3)
                return "return 0;";

            for (int i = 0; i < keys.Length; i++)
            {
                if (!IsAConst (keys[i].Trim()))
                    keys[i] = "current." + keys[i].Trim();
            }

            return string.Format(
                "return {0} ? {1} : {2};",
                keys[0].Trim(),
                keys[1].Trim(),
                keys[2].Trim());
        }

        private bool IsAConst(string s)
        {
            foreach (char c in s)
            {
                if (c == '\'')
                    return true;
                else if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        private void HandleIfCrossReport(ref string script,ref string[] crossdetailitems,ref string crosscolumnheaderitem)
        {
            int indexTRANSFORM = script.IndexOf("TRANSFORM");
            int indexPIVOT = script.IndexOf("PIVOT");
            int indexSELECT = script.IndexOf("SELECT");

            string sTemp = string.Empty;
            if (indexTRANSFORM != -1
                && indexPIVOT != -1)
            {
                sTemp = script.Substring(indexTRANSFORM + 9, indexPIVOT - indexTRANSFORM - 9).Trim();
                crossdetailitems = sTemp.Split(',');
                crosscolumnheaderitem = script.Substring(indexPIVOT + 5, indexSELECT - indexPIVOT - 5).Trim();
                script = script.Substring(indexSELECT, script.Length - indexSELECT).Trim();
            }
        }

        private LevelExpandEnum GetLevelExpandType(string name)
        {
            switch (name)
            {
                case "按存货分类展开":
                    return  LevelExpandEnum.GoodClass ;
                case "按客户分类展开":
                    return LevelExpandEnum.CustClass;
                case "按供应商分类展开":
                    return LevelExpandEnum.ProvClass;
                case "按部门展开":
                    return LevelExpandEnum.DepLevel;
                case "按科目编码展开":
                    return LevelExpandEnum.GradeLevel;
                case "按地区分类展开":
                    return LevelExpandEnum.AreaClass;
                case "按收发类别展开":
                    return LevelExpandEnum.DispLevel;
                case "按结算方式展开":
                    return LevelExpandEnum.SettleLevel;
                case "按货位展开":
                    return LevelExpandEnum.PosLevel;
                default:
                    return LevelExpandEnum.AreaClass;
            }
        }

        private void SetOperatorType(ICalculator cell,string note)
        {
            switch (note.ToUpper())
            {
                case "SUM":
                    //if(cell is IGridCollect)
                    //    (cell as IGridCollect).bSummary = true;
                    cell.Operator = OperatorType.SUM;
                    break;
                case "MAX":
                    //if (cell is IGridCollect)
                    //    (cell as IGridCollect).bSummary = true;
                    cell.Operator = OperatorType.MAX;
                    break;
                case "MIN":
                    //if (cell is IGridCollect)
                    //    (cell as IGridCollect).bSummary = true;
                    cell.Operator = OperatorType.MIN;
                    break;
                case "AVG":
                    //if (cell is IGridCollect)
                    //    (cell as IGridCollect).bSummary = true;
                    cell.Operator = OperatorType.AVG;
                    break;
            }
        }

        private bool bDecimalFormat(string formatstring)
        {
            bool b = false;
            switch (formatstring.ToUpper())
            {
                case "MONEY":
                    b = true;
                    break;
                case "QUANTITY":
                    b = true;
                    break;
                case "NUM":
                    b = true;
                    break;
                case "PERCENT":
                case "PERCENTGROUP":
                    b = true;
                    break;
            }
            return b;
        }

        private void SetPrecisionAndFormatString(IFormat format,string formatstring)
        {
            PrecisionType precision = PrecisionType.None;
            switch (formatstring.ToUpper())
            {
                case "U8存货单价显示格式":
                    precision = PrecisionType.InventoryPrice;
                    formatstring = "";
                    break;
                case "MONEY":
                    precision = PrecisionType.InventoryPrice;
                    formatstring = "";
                    break;
                case "QUANTITY":
                    precision = PrecisionType.Quantity;
                    formatstring = "";
                    break;
                case "NUM":
                    precision = PrecisionType.PieceNum;
                    formatstring = "";
                    break;
                case "PERCENT":
                case "PERCENTGROUP":
                    formatstring = "P";
                    if (format is IGridEvent)
                    {
                        string mapname=(format as IMapName).MapName;
                        StringBuilder sb=new StringBuilder();
                        sb.Append("if(Convert.ToDouble(reportsummary[\"");
                        sb.Append(mapname);
                        sb.AppendLine("\"])==0)");
                        sb.AppendLine("{");
                        sb.AppendLine("    cell.Caption = 0;");
                        sb.AppendLine("    return;");
                        sb.AppendLine("}");
                        sb.AppendLine("if (current != null)");
                        sb.AppendLine("{");
                        sb.Append("    cell.Caption = Convert.ToDouble(current[\"");
                        sb.Append(mapname);
                        sb.Append("\"]) / Convert.ToDouble(reportsummary[\"");
                        sb.Append(mapname);
                        sb.AppendLine("\"]);");
                        sb.AppendLine("}");
                        sb.AppendLine("else if (currentgroup != null)");
                        sb.AppendLine("{");
                        sb.Append("    cell.Caption = Convert.ToDouble(currentgroup[\"");
                        sb.Append(mapname);
                        sb.Append("\"]) / Convert.ToDouble(reportsummary[\"");
                        sb.Append(mapname);
                        sb.AppendLine("\"]);");
                        sb.AppendLine("}");
                        sb.AppendLine("else");
                        sb.AppendLine("{");
                        sb.AppendLine("    cell.Caption = 1;");
                        sb.AppendLine("}");
                        (format as IGridEvent).EventType = EventType.BothContentAndSummary;
                        (format as Cell).PrepaintEvent = sb.ToString();
                    }
                    break;
            }
            if (format is IDecimal)
                (format as IDecimal).Precision = precision;
            if (formatstring  != "")
            {
                formatstring = formatstring.Replace("yyyy-mm-dd", "yyyy-MM-dd");
                formatstring = formatstring.Replace("YYYY-MM-DD", "yyyy-MM-dd");
                format.FormatString = formatstring;
            }
        }

        private DataTypeEnum GetDAEDataType(int isize)
        {
            switch (isize)
            {
                case 100:
                case 0:
                    return DataTypeEnum.Decimal;
                case 1:
                    return DataTypeEnum.String;
                case 2:
                    return DataTypeEnum.DateTime;
                case 3:
                    return DataTypeEnum.Boolean;
                case 4:
                    return DataTypeEnum.Object;
                default:
                    return DataTypeEnum.String;
            }
        }

        private object GetColumnLocaleDescription(string localeid,int orderex,int topex)
        {
            StringBuilder sb=new StringBuilder();
            sb.Append("SELECT top 1 case when nameforeign=null then Name else nameforeign end FROM rpt_FldDef_base WHERE ");
            sb.Append(" OrderEx=");
            sb.Append(orderex.ToString());
            sb.Append(" AND TopEx=");
            sb.Append(topex.ToString());
            sb.Append(" AND ModeEx=0");
            sb.Append(" and id='");
            sb.Append(_oldreportid);
            sb.Append("' and localeid='");
            sb.Append(localeid);
            sb.Append("'");
            return SqlHelper.ExecuteScalar(_login.UfDataCnnString, sb.ToString());
        }

        private TableColumn GetTableColumn(string name)
        {
            foreach (TableColumn column in _columns)
            {
                if (column.Name.ToLower() == name.ToLower())
                    return column;
            }
            return null;
        }

        private DataSource GetDataSource(TableColumn column)
        {
            DataSource ds = new DataSource();
            ds.Name = column.Name.Trim();
            if (column.DescriptionCN == null || column.DescriptionCN.Trim() == "")
                ds.Caption = ds.Name;
            else
                ds.Caption = column.DescriptionCN;
            ds.TWCaption = column.DescriptionTW;
            ds.ENCaption = column.DescriptionUS;
            ds.Type = GetDataType(column.DataType);
            return ds;
        }

        private DataType GetDataType(DataTypeEnum dt)
        {
            switch (dt)
            {
                case DataTypeEnum.Int16:
                case DataTypeEnum.Int32:
                case DataTypeEnum.Int64:
                    return DataType.Int;
                case DataTypeEnum.Decimal:
                case DataTypeEnum.Double:
                case DataTypeEnum.Single:
                    return DataType.Decimal;
                case DataTypeEnum.DateTime:
                    return DataType.DateTime;
                case DataTypeEnum.Boolean:
                    return DataType.Boolean;
                case DataTypeEnum.Object:
                    return DataType.Image;
                default:
                    return DataType.String;
            }
        }

        private object GetLocaleReportName(string localeid)
        {
            return SqlHelper.ExecuteScalar(_login.UfDataCnnString, "select top 1 Title from rpt_glbdef_base where localeid='" + localeid + "' and id=" + _oldreportid);
        }

        private void RaiseExceptionIfSpecialID(string reportId)
        {
            if (reportId.IndexOf("SA[__]货龄分析") != -1
                || reportId.IndexOf("ST[__]业务类型汇总表") != -1
                || reportId.IndexOf("ST[__]收发存汇总表") != -1
                || reportId.IndexOf("ST[__]供货单位收发存汇总表") != -1)
            {
                throw new Exception("此报表为特殊报表，不能进行升级，关键字：" + reportId);
            }
        }
        #endregion

        #region IUpgradeItem 成员

        public void SetMeta(UpgradeReportMetaWrapper urmw)
        { 
            SetMeta(urmw, false);
        }        
        public void SetMeta(UpgradeReportMetaWrapper urmw, bool IsBak)
        {
            string sql=string.Empty;
            //if(IsBak)
            //    sql = "select top 1 ID,IsBaseTable,ClassName,FilterClass,FilterID,Title,Note,isnull(defaultshowdetail,0) as defaultshowdetail,isnull(MappingMenuId,'') as MappingMenuId,systemId from rpt_glbdef_base_bak where localeid='zh-CN' and systemid='" + _subid + "' and name='" + _reportid + "'"; 
            //else
                sql = "select top 1 ID,IsBaseTable,ClassName,FilterClass,FilterID,Title,Note,isnull(defaultshowdetail,0) as defaultshowdetail,isnull(MappingMenuId,'') as MappingMenuId,systemId from rpt_glbdef_base where localeid='zh-CN' and systemid='" + _subid + "' and name='" + _reportid + "'";
            bool defaultshowdetail =false;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, sql))
            {
                while (reader.Read())
                {
                    _oldreportid = reader["ID"].ToString();
                    _classname = reader["ClassName"].ToString();
                    bool bsystem = Boolean.Parse(reader["IsBaseTable"].ToString());
                    _cnreportname  = reader["Title"].ToString();
                    string filterclass = reader["FilterClass"].ToString();
                    string filterid = reader["FilterID"].ToString();
                    string note = reader["Note"].ToString().Trim();
                    defaultshowdetail = ((int)reader["defaultshowdetail"]) == 0 ? false : true;
                    string systemId = reader["systemId"].ToString();
                    string rootReportId = string.Empty;
                    if(note!="")
                        rootReportId=systemId + "[__]" + note;


                    #region userdefine report
                    // 这种情况下表明其是自定义报表
                    if (note != string.Empty && note.ToLower() != _reportid.ToLower() )
                        _basereportid = note;

                    if (string.IsNullOrEmpty(_basereportid))
                        _basereportid  = _reportid ;
                    //RaiseExceptionIfSpecialID(string.Format("{0}[__]{1}", _subid.ToUpper(), _basereportid));
                    #endregion

                    string mappingmenuid = reader["MappingMenuId"].ToString();
					if (!string.IsNullOrEmpty(mappingmenuid))
					{
						if(mappingmenuid == "NotShowInReportCenter")
							urmw.SetArgument(UpgradeReportMetaWrapper.Description, mappingmenuid);
						else
							urmw.SetArgument(UpgradeReportMetaWrapper.MappingMenuId, mappingmenuid);
					}

                    urmw.SetArgument(UpgradeReportMetaWrapper.ReportNameCN, _cnreportname  );
                    urmw.SetArgument(UpgradeReportMetaWrapper.ReportClassName, _classname );
                    urmw.SetArgument(UpgradeReportMetaWrapper.ReportFilterClass, filterclass );
                    urmw.SetArgument(UpgradeReportMetaWrapper.ReportFilterId, filterid );
                    urmw.SetArgument(UpgradeReportMetaWrapper.ReportIsSystem, bsystem);
                    urmw.SetArgument(UpgradeReportMetaWrapper.ViewIsSystem, bsystem);
                    urmw.SetArgument(UpgradeReportMetaWrapper.RootReportId, rootReportId);
                }
                reader.Close();
            }

            object ln = GetLocaleReportName("en-US");
            _enreportname=(ln==null?_cnreportname :ln.ToString());
            urmw.SetArgument(UpgradeReportMetaWrapper.ReportNameEN, _enreportname );
            if (ln == null)
                urmw.SetArgument(UpgradeReportMetaWrapper.IsNeedExpandEN, true);
            ln = GetLocaleReportName("zh-TW");
            _twreportname = (ln == null ? _cnreportname : ln.ToString());
            urmw.SetArgument(UpgradeReportMetaWrapper.ReportNameTW , _twreportname );
            if (ln == null)
                urmw.SetArgument(UpgradeReportMetaWrapper.IsNeedExpandTW, true);

            string commonxml=null;
            string cnxml=null;
            string enxml=null;
            string twxml=null;
            UpgradeFormat(urmw, defaultshowdetail, IsBak);
            ReportEngine re = new ReportEngine(_login, ReportStates.Designtime);
            re.UpgradeSave(_report, new ReportDataSource(_columns).DataSources , ref commonxml, ref cnxml, ref twxml, ref enxml);

            urmw.SetArgument(UpgradeReportMetaWrapper.ViewId , _report.ViewID );
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewBlandScape, _report.bLandScape );
            //urmw.SetArgument(UpgradeReportMetaWrapper.ViewColumns , _report.bLandScape);
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewGroupSchemas , _report.GroupSchemas.ToXml().InnerXml );
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewLevelExpand , _report.ExpandSchema.ToString());
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewPageMargins, _report.PageMargins.ConvertToString());
            //urmw.SetArgument(UpgradeReportMetaWrapper.ViewPaperType, _report );
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewViewType, Convert.ToInt32(_report.Type) );

            urmw.SetArgument(UpgradeReportMetaWrapper.ReportDataSourceBO,_bo );
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewCommonFormat, commonxml);
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewLocaleFormatCN , cnxml );
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewLocaleFormatTW , twxml );
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewLocaleFormatEN , enxml );
        }

        public void DeliverEnvironmentInfos(Hashtable infos)
        {
            _login = (U8LoginInfor)infos[Upgrade872Controller.InfoKeyLoginInfo];
            _reportid = infos[Upgrade872Controller.InfoKeyReportName861 ].ToString();
            _subid = infos[Upgrade872Controller.InfoKeyReportSubId ].ToString();            
        }

        #endregion
    }

    internal class UpgradeFormatServiceOutU8 : IUpgradeItem
    {
        private U8LoginInfor _login;
        private string _reportid;//(另存的)销售统计表

        public UpgradeFormatServiceOutU8()
        {
        }

        #region IUpgradeItem 成员
        public void SetMeta(UpgradeReportMetaWrapper urmw)
        {
        }

        public void SetMeta(UpgradeReportMetaWrapper urmw,string filterstring,string classname)
        {            
            string commonxml = null;
            string cnxml = null;
            #region commonxml
//            commonxml = @"<Report Name='Report' Type='1'>
//  <Section Name='ReportHeader' Type='ReportHeader'>
//    <Control Name='HeaderLabel' Type='CommonLabel'  bControlAuth='False'/>
//  </Section>
//  <Section Name='GridDetail' Type='GridDetail'>
//    <Control Name='GridLabel1' Type='GridLabel' bControlAuth='False' DataSource='A'/>
//    <Control Name='GridDateTime1' Type='GridDateTime'  bControlAuth='False' DataSource='D' FormatString=''/>
//    <Control Name='SuperLabel1' Type='SuperLabel'  bControlAuth='False'/>
//    <Control Name='GridDecimal1' Type='GridDecimal' bControlAuth='False' Operator='0' DataSource='E' Precision='0' bShowWhenZero='0' FormatString='' bSummary='True'/>
//    <Control Name='GridDecimal2' Type='GridDecimal' bControlAuth='False' Operator='0' DataSource='F' Precision='0' bShowWhenZero='0' FormatString='' bSummary='True'/>
//    <Control Name='GridCalculateColumn1' Type='GridCalculateColumn' bControlAuth='False' Expression='E+F' CalculateIndex='0' Operator='0' Precision='0' bShowWhenZero='0' FormatString='' bSummary='True'/>
//  </Section>
//  <DataSource>
//    <Column Name='A' Type='String'/>
//    <Column Name='D' Type='DateTime'/>
//    <Column Name='E' Type='Decimal'/>
//  </DataSource>
//</Report>";
            #endregion

            #region cnxml
//            cnxml = @"<Report>
//  <Section Name='ReportHeader' Type='ReportHeader' Height='140' Caption='报表标题区' IdentityCaption=''>
//    <Control Name='HeaderLabel' Type='CommonLabel' X='132' Y='42' Width='280' Height='80' Caption='标题' IdentityCaption='' />
//  </Section>
//  <Section Name='GridDetail' Type='GridDetail' Height='140' Caption='明细区' IdentityCaption=''>
//    <Control Name='GridLabel1' Type='GridLabel' X='40' Y='86' Width='96' Height='24' Caption='A' IdentityCaption='' />
//    <Control Name='GridDateTime1' Type='GridDateTime' X='136' Y='86' Width='96' Height='24' Caption='D' IdentityCaption='' />
//    <Control Name='SuperLabel1' Type='SuperLabel' X='226' Y='56' Width='206' Height='24' Caption='EF' IdentityCaption='' />
//    <Control Name='GridDecimal1' Type='GridDecimal' X='232' Y='86' Width='96' Height='24' Caption='E' IdentityCaption='' />
//    <Control Name='GridDecimal2' Type='GridDecimal' X='328' Y='86' Width='96' Height='24' Caption='F' IdentityCaption='' />
//    <Control Name='GridCalculateColumn1' Type='GridCalculateColumn' X='424' Y='86' Width='96' Height='24' Caption='E+F' IdentityCaption='' />
//  </Section>
//  <DataSource>
//    <Column Name='A' Caption='A'/>
//    <Column Name='D' Caption='D'/>
//    <Column Name='E' Caption='E'/>
//  </DataSource>
//</Report>";
            #endregion

            //object oh = Activator.CreateInstance("U8DRP_GeneralQueryC", "UFIDA.U8.DRP.Lib.Business.U8DRP_GeneralQueryC").Unwrap();
            //Hashtable ht = oh.GetType().InvokeMember("GetQueryHeadDetail",
            //            BindingFlags.InvokeMethod,
            //            null,
            //            oh,
            //            new object[] { _login.UserToken, filterstring, "", errstring }) as Hashtable;

            string[] classtocreate=classname.Split(',');
            object oh = Activator.CreateInstance(classtocreate[1], classtocreate[0]).Unwrap();
            Hashtable ht=oh.GetType().InvokeMember("GetFormat",
                        BindingFlags.InvokeMethod,
                        null,
                        oh,
                        new object[] { _login.UserToken, filterstring }) as Hashtable ;
            if (ht == null)
                throw new Exception("设计格式出错，返回hashtable=null");
            commonxml = ht["CommonFormat"].ToString();
            cnxml = ht["LocaleFormat"].ToString();
            string viewid = _reportid;
            if (ht.Contains("DynamicFormat") && Convert.ToBoolean(ht["DynamicFormat"]))
                viewid = viewid + "_DynamicFormat";

            if (ht.Contains("DefaultGroup") && !string.IsNullOrEmpty(ht["DefaultGroup"].ToString()))
            {
                GroupSchemas gss=new GroupSchemas();
                GroupSchema gs = new GroupSchema();
                gs.ID = Guid.NewGuid().ToString();
                gs.SetName("zh-CN", "缺省分组");
                gs.SetName("zh-TW", "缺省分組");
                gs.SetName("en-US", "Default Group");
                gs.bDefault = true;
                GroupSchemaItem gsi = null;

                string[] dgs = ht["DefaultGroup"].ToString().Split(',');
                foreach (string dg in dgs)
                {
                    if (!string.IsNullOrEmpty(dg.Trim()))
                    {
                        gsi = new GroupSchemaItem();
                        gsi.Items.Add(dg.Trim());
                        gs.SchemaItems.Add(gsi);
                    }
                }
                        
                gs.bShowDetail = true;
                gs.bGroupItemsAhead = true;
                gss.Add(gs);
                urmw.SetArgument(UpgradeReportMetaWrapper.ViewGroupSchemas, gss.ToXml().InnerXml);
            }

            urmw.SetArgument(UpgradeReportMetaWrapper.ReportNameCN, _reportid );
            //urmw.SetArgument(UpgradeReportMetaWrapper.ReportClassName, _classname);
            //urmw.SetArgument(UpgradeReportMetaWrapper.ReportFilterClass, filterclass);
            //urmw.SetArgument(UpgradeReportMetaWrapper.ReportFilterId, filterid);
            urmw.SetArgument(UpgradeReportMetaWrapper.ReportIsSystem, false);
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewIsSystem, false);

            urmw.SetArgument(UpgradeReportMetaWrapper.ViewId, viewid  );
            //urmw.SetArgument(UpgradeReportMetaWrapper.ViewBlandScape, _report.bLandScape);
            //urmw.SetArgument(UpgradeReportMetaWrapper.ViewGroupSchemas, _report.GroupSchemas.ToXml().InnerXml);
            //urmw.SetArgument(UpgradeReportMetaWrapper.ViewLevelExpand, _report.ExpandSchema.ToString());
            //urmw.SetArgument(UpgradeReportMetaWrapper.ViewPageMargins, _report.PageMargins.ConvertToString());
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewViewType, 1);

            urmw.SetArgument(UpgradeReportMetaWrapper.ViewCommonFormat, commonxml);
            urmw.SetArgument(UpgradeReportMetaWrapper.ViewLocaleFormatCN, cnxml);
        }

        public void DeliverEnvironmentInfos(Hashtable infos)
        {
            _login = (U8LoginInfor)infos[Upgrade872Controller.InfoKeyLoginInfo];
            _reportid = infos[Upgrade872Controller.InfoKeyReportName861].ToString();
            //_subid = infos[Upgrade872Controller.InfoKeyReportSubId].ToString();
        }

        #endregion
    }
}
