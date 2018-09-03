using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Drawing;
using System.Data;
using UFIDA.U8.UAP.Services.ReportData;


namespace UFIDA.U8.UAP.Services.ReportElements
{

    /// <summary>
    /// ���汨�����,���콻���ѯ���,������ʱ��,�����췵�ؽ��汨���Cells
    /// </summary>

    class CrossService
    {
        private Report _report;
        private CrossObj _crossObj;
        private U8LoginInfor _login;
        private string[] _separators = new string[] { "@@@" };

        public CrossService()
        {
        }

        /// <summary>
        /// �������汨�����ʱ��
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="cnnStr"></param>
        /// <param name="retCells"></param>
        /// <param name="sErrMsg"></param>
        /// <returns></returns>
        public bool CrossCalc(Report rep, U8LoginInfor login, out string sErrMsg)
        {
            bool bRet = true;
            sErrMsg = "";
            _report = rep;
            string cnnStr = login.UfDataCnnString;
            _login = login;
            string sTemp = "";
            string rowGroup = "", rowGroupbyFlds = "", headColGroup = "", SelectSortFlds = "", headColOrder = "", strSql = "";
            try
            {
                int rowGroupCount;
                int headColGroupCount;

                string srcTableName = rep.CrossTable;
                bool hascalcch = false;
                StringBuilder sb2 = new StringBuilder();
                sb2.Append(" (select *");

                Cells chs = rep.Sections[SectionType.CrossRowHeader].Cells;
                rowGroup = GetFlds(chs, true, out rowGroupbyFlds, out rowGroupCount);

                foreach (Cell c in chs)
                {
                    if (c is ICalculateColumn && !rep.DataSources.RawContains((c as IMapName).MapName))
                    {
                        hascalcch = true;
                        sb2.Append(",");
                        //������齻��ļ򵥼�����isnull���� 
                        if (c is ICalculator)
                        {
                            (c as ICalculateColumn).Expression = ReportEngine.HandleExpression(_report.DataSources, (c as ICalculateColumn).Expression, "", (c as ICalculator).Operator.ToString().ToLower() != "avg" ? true : false);
                        }
                        // end
                        sb2.Append(ReportEngine.HandleExpression(rep.DataSources, (c as ICalculateColumn).Expression, "", false));
                        sb2.Append(" as [");
                        sb2.Append((c as IMapName).MapName);
                        sb2.Append("] ");
                    }
                }

                chs = rep.Sections[SectionType.CrossColumnHeader].Cells;
                Cells cds = rep.Sections[SectionType.CrossDetail].Cells;
                CheckCrossDetail(cds);

                headColGroup = GetFlds(chs, false, out sTemp, out headColGroupCount);
                if (headColGroupCount == 0)
                    throw new Exception(NoCrossColumn);

                //ֻ�е��з�������Ϊ�յ�ʱ��ȡĬ���������
                if (this._report.CurrentCrossSchema.ColumnSortSchema == null || 
                    this._report.CurrentCrossSchema.ColumnSortSchema.QuickSortItems.Count==0)
                {
                    headColOrder = GetOrders(chs, true, out SelectSortFlds);
                }
                else
                {
                    #region ����������
                    foreach (QuickSortItem item in _report.CurrentCrossSchema.ColumnSortSchema.QuickSortItems)
                    {
                            if (_report.DataSources[item.Name] != null)
                            {
                                string sortName = string.Empty;
                                if (_report.DataSources[item.Name].IsADecimal)
                                    sortName = string.Format("sum([{0}]) as [{0}]", item.Name);
                                else
                                    sortName = string.Format("min([{0}]) as [{0}]", item.Name);
                                SelectSortFlds = SelectSortFlds + "," + sortName;
                                //�������
                                if (item.SortDirection == SortDirection.Ascend)
                                {
                                    headColOrder = headColOrder + string.Format(",[{0}] asc", item.Name);
                                }
                                else if (item.SortDirection == SortDirection.Descend)
                                {
                                    headColOrder = headColOrder + string.Format(",[{0}] desc", item.Name);
                                }
                            }
                    }

                    if (headColOrder.Length > 0)
                    {
                        headColOrder = headColOrder.Remove(0, 1);
                        headColOrder = "order by" + headColOrder;
                    }
                    #endregion
                }

                #region add by cwh,to handle calculate columnheader

                foreach (Cell c in chs)
                {
                    if (c is ICalculateColumn && !rep.DataSources.RawContains((c as IMapName).MapName))
                    {
                        hascalcch = true;
                        sb2.Append(",");
                        //������齻��ļ򵥼�����isnull����
                        if( c is ICalculator)
                        {
                        (c as ICalculateColumn).Expression = ReportEngine.HandleExpression(_report.DataSources, (c as ICalculateColumn).Expression, "",true);
                        }
                        // end
                        sb2.Append(ReportEngine.HandleExpression(rep.DataSources, (c as ICalculateColumn).Expression, "", false));
                        sb2.Append(" as [");
                        sb2.Append((c as IMapName).MapName);
                        sb2.Append("] ");
                    }
                }
                if (hascalcch)
                {
                    sb2.Append(" from ");
                    sb2.Append(srcTableName);
                    sb2.Append(" ) A");
                    srcTableName = sb2.ToString();
                }
                #endregion

                _crossObj = new CrossObj(rep);
                _crossObj.HeadColGroupCount = headColGroupCount;
                string selectsql = "";
                //10.1 add ��ʹ�ÿձ�����Ȼ��insert into ��ֹ��������ʱ����sysobjects 
                strSql = GetCrossSql(srcTableName, cnnStr, rowGroup, rowGroupbyFlds, headColGroup, SelectSortFlds, headColOrder, ref selectsql);
                SqlHelper.ExecuteNonQuery(cnnStr, strSql);
                string s = getColFlds(cnnStr, _report.BaseTable);
                strSql = "insert into " + _report.BaseTable + "(" + s + ")" + "select " + s + " from (" + selectsql + ") ax ";
                SqlDataReader dr = SqlHelper.ExecuteReader(cnnStr, strSql);
                
                DropCrossNullCulumn(rep, login);
                DropCrossEmptyCulumn(rep, login);
            }
            catch (SqlException ex)
            {
                bRet = false;
                if (ex.Number == 1702 || ex.Number == 1701)
                    sErrMsg = SqlExceptionMsg;
                else
                    sErrMsg = ex.Message;
            }
            catch (Exception ex)
            {
                bRet = false;
                sErrMsg = ex.Message;
                System.Diagnostics.Trace.WriteLine("rowGroup=" + rowGroup);
                System.Diagnostics.Trace.WriteLine("headColGroup=" + headColGroup);
                System.Diagnostics.Trace.WriteLine("strSql=" + strSql);
            }
            return bRet;
        }

        /// <summary>
        /// ɾ�����������ȫ��Ϊ�յ���
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="login"></param>
        /// <param name="sErrMsg"></param>
        private void DropCrossNullCulumn(Report rep, U8LoginInfor login)
        {
            string column = string.Empty;
            //������ͼ����֧�ָù���
            //if (rep.RealViewType == ReportType.CrossReport)
            //    return;
            //if (!rep.CurrentCrossSchema.bShowCrossNullColumn)
            //{
                //���ұ�������Ϊ�յ���   �и�ʽΪ�� ����Ԫ��@@@һ������@@@��������
                string columnName = string.Format("SELECT name FROM dbo.syscolumns WHERE id=OBJECT_ID('{0}') and  name like '%@@@'", rep.BaseTable);
                DataTable dt = SqlHelper.ExecuteDataSet(login.TempDBCnnString, columnName).Tables[0];
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    column = SqlHelper.GetStringFrom(dt.Rows[row][0]);
                    //�б���Ϊ��
                    if (!string.IsNullOrEmpty(column))
                    {
                        //null���
                        //string drop = string.Format("IF not EXISTS (select [{0}] from {1} where isnull([{0}],'0')!='0') alter table {1} drop column [{0}];", 
                            // " IF not EXISTS (select [{0}] from {1} where [{0}] !='0')      alter table {1} drop column [{0}]",
                        string drop = string.Format("IF not EXISTS (select [{0}] from {1} where [{0}] is not null) alter table {1} drop column [{0}];", 
                            column, rep.BaseTable);
                        SqlHelper.ExecuteNonQuery(login.TempDBCnnString, drop);

                        

                    }
                }
            //}
        }
              /// <summary>
        /// ɾ�����������ȫ��Ϊ�յ���
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="login"></param>
        /// <param name="sErrMsg"></param>
        private void DropCrossEmptyCulumn(Report rep, U8LoginInfor login)
        {
            string column = string.Empty;
            //������ͼ����֧�ָù���
            //if (rep.RealViewType == ReportType.CrossReport)
            //    return;
            //if (!rep.CurrentCrossSchema.bShowCrossNullColumn)
            //{
                //���ұ�������Ϊ�յ���   �и�ʽΪ�� ����Ԫ��@@@һ������@@@��������
                string columnName = string.Format("SELECT name FROM dbo.syscolumns WHERE id=OBJECT_ID('{0}') and  name like '%@@@'", rep.BaseTable);
                DataTable dt = SqlHelper.ExecuteDataSet(login.TempDBCnnString, columnName).Tables[0];
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    column = SqlHelper.GetStringFrom(dt.Rows[row][0]);
                    //�б���Ϊ��
                    if (!string.IsNullOrEmpty(column))
                    {
                        //�����null���
                        string drop = string.Format("IF not EXISTS (select [{0}] from {1} where rtrim(ltrim(convert(NVARCHAR(4000),[{0}])))!='') alter table {1} drop column [{0}];",
                            column, rep.BaseTable);
                        SqlHelper.ExecuteNonQuery(login.TempDBCnnString, drop);
                    }
                }
            //}
        }
                  
        private string getColFlds(string cnnStr, string basetable)
        {
            string sql = "select name from tempdb..syscolumns where id=object_id('" + basetable + "')";
            SqlDataReader dr = SqlHelper.ExecuteReader(cnnStr, sql);
            StringBuilder sb = new StringBuilder();
            while (dr.Read())
            {
                sb.Append("[");
                sb.Append(dr.GetString(0).Replace("]", "]]"));
                sb.Append("]");
                sb.Append(",");
            }

            string s = sb.ToString();
            if (s.EndsWith(",[" + _report.BaseID + "],"))
                sql = s.Replace(",[" + _report.BaseID + "],", "");
            else
            {
                s = s.Replace("[" + _report.BaseID + "],", "");
                sql = s.Substring(0, s.Length - 1);
            }

            return sql;
        }
        public void NotCrossCalc(Report rep)
        {
            _report = rep;
            string rowGroupbyFlds = "";
            int rowGroupCount;
            Cells chs = rep.Sections[SectionType.CrossRowHeader].Cells;
            GetFlds(chs, true, out rowGroupbyFlds, out rowGroupCount);
        }

        private void CheckCrossDetail(Cells cells)
        {
            int index = cells.Count - 1;
            while (index >= 0)
            {
                Cell cell = cells[index];
                if (cell is IDataSource && ((cell as IDataSource).DataSource.IsEmpty || !_report.DataSources.Contains((cell as IDataSource).DataSource.Name)))
                    cells.Remove(cell);
                if (cell is IDataSource && (cell as IDataSource).DataSource.Type == DataType.Boolean)
                    cells.Remove(cell);
                index--;
            }
            if (cells.Count == 0)
                throw new Exception(NoCrossDetail);
        }

        private string NoCrossDetail
        {
            get
            {
                switch (_login.LocaleID.ToLower())
                {
                    case "zh-tw":
                        return "�]�н����Y��";
                    case "en-us":
                        return "No cross detail data";
                    default:
                        return "û�н�������";
                }
            }
        }

        private string NoCrossColumn
        {
            get
            {
                switch (_login.LocaleID.ToLower())
                {
                    case "zh-tw":
                        return "�]�н����И��}�Y��";
                    case "en-us":
                        return "No cross column header data";
                    default:
                        return "û�н����б�������";
                }
            }
        }

        //CREATE TABLE failed because column 'SystemID@@@976' in table 'UFTmpTable04431872_a5f1_43da_a8da_63839fb69b50' exceeds the maximum of 1024 columns.
        private string SqlExceptionMsg
        {
            get
            {
                switch (_login.LocaleID.ToLower())
                {
                    case "zh-tw":
                        return "����Y���Д�������ϵ�y֧Ԯ�Ĺ�����Ո�sС��ԃ��������ԇ";
                    case "en-us":
                        return "The result columns after cross operation have exceeded the maxium of columns system can support, please shrink the data scope and try again";
                    default:
                        return "����������������ϵͳ֧�ֵķ�Χ������С��ѯ��Χ������";
                }
            }
        }
        /// <summary>
        /// ��ý�����Cells����
        /// </summary>
        /// <param name="rep">Report����</param>
        /// <param name="cnnStr">���ݿ����Ӵ�</param>
        /// <param name="retCells">���ؽ�����Cells����</param>
        /// <param name="sErrMsg">����ʱ������Ϣ</param>
        /// <returns>��ȷ����true������false</returns>
        public bool GetCrossCells(Report rep, U8LoginInfor login, out Cells retCells, out string sErrMsg)
        {
            bool bRet = true;
            sErrMsg = "";
            retCells = null;
            _report = rep;
            string strSql = "";
            string cnnStr = login.UfDataCnnString;
            _login = login;
            try
            {
                int rowGroupCount = GetRowGroupCount();
                _crossObj = new CrossObj(rep);
                _crossObj.HeadColGroupCount = rep.Sections[SectionType.CrossColumnHeader].Cells.Count;
                strSql = "select top 0 * from " + _report.BaseTable;
                SqlDataReader dr = SqlHelper.ExecuteReader(cnnStr, strSql);
                retCells = GetCells(dr, rowGroupCount);
            }
            catch (Exception ex)
            {
                bRet = false;
                sErrMsg = ex.Message;
                System.Diagnostics.Trace.WriteLine("strSql=" + strSql);
            }
            return bRet;
        }

        private int GetRowGroupCount()
        {
            int count = 0;
            Cells cells = _report.Sections[SectionType.CrossRowHeader].Cells;
            foreach (Cell cell in cells)
            {
                if (cell is IAlgorithm)
                    continue;

                if (cell is IDataSource && (cell as IDataSource).DataSource.IsEmpty)
                    continue;
                count++;
            }
            return count;
        }

        private int AddARowHeaderCell(Cell cell, Cells retCells, int nLeft)
        {
            DataSource ds = null;
            if (cell is GridCalculateColumn)
            {
                ds = new DataSource((cell as IMapName).MapName, DataType.Decimal);
                _report.DataSources.Add(ds);
                cell = new GridDecimal(cell as IMapName, ds);
            }
            else if (cell is GridColumnExpression)
            {
                ds = new DataSource((cell as IMapName).MapName, DataType.String);
                _report.DataSources.Add(ds);
                cell = new GridLabel(cell as IMapName, ds);
            }
            if (nLeft == Int32.MinValue)
                nLeft = cell.X;
            cell.X = nLeft;
            nLeft += cell.Width;
            retCells.AddDirectly(cell);
            return nLeft;
        }
        /// <summary>
        /// ���콻�汨���Cells
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="rowGroupCount">�з����и���</param>
        /// <returns></returns>
        private Cells GetCells(SqlDataReader dr, int rowGroupCount)
        {
            Cells retCells = new Cells();
            int totalCrossFldCount = 0;
            int nLeft = Int32.MinValue;
            int nTop = DefaultConfigs.SECTIONHEADERHEIGHT + 10;// _report.Sections[SectionType.CrossRowHeader].Cells[0].Y;            
            string sTemp = "";

            totalCrossFldCount = dr.FieldCount - rowGroupCount - 1;// ��1��ʾȥ��baseid�ֶ�
            if (_crossObj.ColSummaryNum > 0)//�Ƿ���ͳ���ֶ�
            {
                totalCrossFldCount -= _crossObj.ColSummaryNum;
            }
            int count = _report.Sections[SectionType.CrossRowHeader].Cells.Count;
            for (int i = 0; i < count; i++)
            {
                Cell tmp = _report.Sections[SectionType.CrossRowHeader].Cells[i];
                if ((tmp is IGridEvent) && (tmp as IGridEvent).bShowAtReal)
                    continue;
                nLeft = AddARowHeaderCell(tmp, retCells, nLeft);
            }
            //nLeft = _report.Sections[SectionType.CrossRowHeader].Cells[count - 1].X + _report.Sections[SectionType.CrossRowHeader].Cells[count - 1].Width;
            if (nLeft == Int32.MinValue)
                nLeft = 0;
            int[] arrSuperLeft = new int[_crossObj.HeadColGroupCount];
            int[] arrSubSupperLeft = new int[_crossObj.HeadColGroupCount];
            for (int i = 0; i < arrSuperLeft.Length; i++)
            {
                arrSuperLeft[i] = nLeft;
                arrSubSupperLeft[i] = nLeft;
            }
            int[] arrCrossIndex = new int[_crossObj.CrossFldsCount];//ÿ������������
            int headColCrossIndex = -1;//�����б�ͷ��������
            int serial;//�����ֶε����У������������ֶεĵڼ���                
            string cellName = "", cellCaption = "", fldName = "";
            int horizonSubtoTotalCount = 0;
            for (int i = 0; i < totalCrossFldCount; i++)
            {
                serial = i % _crossObj.CrossFldsCount;
                arrCrossIndex[serial] = Convert.ToInt32((i / _crossObj.CrossFldsCount)) + _crossObj.ArrSumNum[serial];
                cellName = _crossObj.ArrFldName[serial] + "__" + arrCrossIndex[serial].ToString();
                fldName = dr.GetName(i + rowGroupCount);
                
                //��һ�������ʱ����ʾ�з���ֵ
                cellCaption = _crossObj.CrossFldsCount == 1 ? fldName.Substring(dr.GetName(i + rowGroupCount).LastIndexOf("@@@") + 3) : _crossObj.ArrFldCaption[serial];
                // Modify by matfb V12.0���ֽ����������������
                if (_crossObj.CrossFldsCount != 1)
                {
                    string name = fldName.Substring(0, fldName.LastIndexOf("@@@"));
                    for (int j = 0; j < _crossObj.ArrFldName.Length; j++)
                    {
                        if (_crossObj.ArrFldName[j] == name)
                        {
                            cellCaption = _crossObj.ArrFldCaption[j];
                            break;
                        }
                    }
                }
                
                CreateColumns(retCells, cellName, cellCaption, fldName, ref nLeft, nTop, serial, arrCrossIndex[serial], false);
                if (_crossObj.HeadColGroupCount > 1 && _crossObj.ArrbColumnSummary[serial] == true)//������ͳ����
                {
                    _crossObj.ArrSumExp[serial] += "isnull([" + fldName + "],0) +";
                    if (serial == _crossObj.CrossFldsCount - 1 && //�������һ�������й������
                        _report.CurrentCrossSchema.BShowHorizonTotal)//������ʾ�������
                    {
                        for (int k = 1; k < _crossObj.HeadColGroupCount; k++)
                        {
                            arrSubSupperLeft[k] = nLeft;
                            if ((!CompareHeadColVal(dr.GetName(i + rowGroupCount), dr.GetName(i + rowGroupCount + 1), k)))
                            {
                                for (int j = 0; j < _crossObj.CrossFldsCount; j++)
                                {
                                    if (_crossObj.ArrbColumnSummary[j] == true)
                                    {
                                        horizonSubtoTotalCount++;
                                        _crossObj.ArrSumExp[j] = _crossObj.ArrSumExp[j] + "0";
                                        _crossObj.ArrSumNum[j] += 1;
                                        arrCrossIndex[j] += 1;
                                        cellName = _crossObj.ArrFldName[j] + "__" + (arrCrossIndex[j]).ToString();
                                        fldName = cellName;//С������ֱ����cellName����
                                        cellCaption = _crossObj.ColSummaryNum > 1 ? _crossObj.ArrFldCaption[j] : GetResString("С��");
                                        CreateSubTotalColumn(retCells, cellName, cellCaption, fldName, ref  nLeft, nTop, j, _crossObj.ArrSumExp[j]);
                                        if (k == _crossObj.HeadColGroupCount - 1) _crossObj.ArrSumExp[j] = "";//���                            
                                    }
                                }
                                if (_crossObj.ColSummaryNum > 1)
                                {
                                    headColCrossIndex++;
                                    cellCaption = GetResString("С��");
                                    AddSuperLabel(k, retCells, cellCaption, arrSubSupperLeft[k], nLeft, -2, CrossColumnType.CrossSubTotal);//headColCrossIndex
                                }
                            }
                        }
                    }
                }
                arrSubSupperLeft[0] = nLeft;

                //����supperLabel
                if ((_crossObj.CrossFldsCount > 1 || _crossObj.HeadColGroupCount > 1))
                {
                    for (int j = 1; j <= _crossObj.HeadColGroupCount; j++)
                    {
                        if (!(_crossObj.CrossFldsCount == 1 && j == _crossObj.HeadColGroupCount))//һ�������ֶβ���ʾ�ֶ���
                        {
                            if (!CompareHeadColVal(dr.GetName(i + rowGroupCount), dr.GetName(i + rowGroupCount + 1), j))
                            {
                                headColCrossIndex++;
                                sTemp = dr.GetName(i + rowGroupCount);
                                string superCaption = (sTemp.Split(_separators, StringSplitOptions.None))[j];
                                AddSuperLabel(j, retCells, superCaption, arrSuperLeft[j - 1], arrSubSupperLeft[j - 1], headColCrossIndex, CrossColumnType.CrossDetail);
                                arrSuperLeft[j - 1] = nLeft;
                            }
                        }
                    }
                }
            }
            //if (horizonSubtoTotalCount>0   &&
               if(_crossObj.ColSummaryNum > 0 && //�Ƿ��кϼ��ֶ�
                  _report.CurrentCrossSchema.BShowHorizonTotal)//������ʾ�������
            {
                int j = 0;
                for (int i = 0; i < _crossObj.CrossFldsCount; i++)
                {
                    if (_crossObj.ArrbColumnSummary[i] == true)//���ܴ����ַ��Ľ����ֶΣ�û�кϼ��У�����j����
                    {
                        cellName = _crossObj.ArrFldName[i];//�ϼ�cellName��Ϊ�ֶ���
                        fldName = dr.GetName(j + rowGroupCount + totalCrossFldCount);
                        cellCaption = _crossObj.ColSummaryNum > 1 ? _crossObj.ArrFldCaption[i] : GetResString("�ϼ�");
                        CreateColumns(retCells, cellName, cellCaption, fldName, ref  nLeft, nTop, i, (Convert.ToInt32((i / _crossObj.CrossFldsCount)) + _crossObj.ArrSumNum[i] + 1), true);
                        j++;
                    }

                }
                if (_crossObj.ColSummaryNum > 1)
                {
                    headColCrossIndex++;
                    string superCaption = GetResString("�ϼ�");
                    AddSuperLabel(1, retCells, superCaption, arrSuperLeft[0], nLeft, -1, CrossColumnType.CrossTotal);//headColCrossIndex
                    arrSuperLeft[0] = nLeft;
                }
            }
            count = _report.Sections[SectionType.CrossRowHeader].Cells.Count;
            for (int i = 0; i < count; i++)
            {
                Cell tmp = _report.Sections[SectionType.CrossRowHeader].Cells[i];
                if ((tmp is IGridEvent) && (tmp as IGridEvent).bShowAtReal)
                    nLeft = AddARowHeaderCell(tmp, retCells, nLeft);
            }
            return retCells;
        }

        /// <summary>
        /// ���ӷ����б���
        /// </summary>
        /// <param name="superCaption">�����б���</param>
        /// <param name="nStartLeft">��ʼ��߽�λ��</param>
        /// <param name="nEndLeft">��ֹ�ұ߽�λ��</param>
        /// <param name="headColCrossIndex">���������˳��</param>
        protected virtual void AddSuperLabel(int level, Cells retCells, string superCaption, int nStartLeft, int nEndLeft, int headColCrossIndex, CrossColumnType ct)
        {
            Cell ch = _report.Sections[SectionType.CrossColumnHeader].Cells[level - 1];
            if (ch is IBDateTime && (ch as IBDateTime).bDateTime)
                superCaption = HandleDateTimeCaption(superCaption, (ch as IFormat).FormatString);

            SuperLabel sl = new SuperLabel();
            sl.Caption = superCaption.Trim().Length > 0 ? superCaption : GetResString("����");//���ڽ�����Ϊ��ֵ��һ���ԡ�������������//superCaption;
            //sl.Caption = superCaption.Trim().Length > 0 ? superCaption : string.Empty;//���ڽ�����Ϊ��ֵ��һ���ԡ�������������//superCaption;
            sl.Name += "SuperLabel" + (headColCrossIndex == -1 ? "100000" : headColCrossIndex.ToString());
            sl.CrossIndex = headColCrossIndex;
            if (_crossObj.ColSummaryNum > 0)
                sl.LabelType = LabelType.SummaryLabel;
            sl.X = nStartLeft;
            sl.Width = nEndLeft - nStartLeft;
            sl.PrepaintEvent = ch.PrepaintEvent;
            sl.ScriptID = string.IsNullOrEmpty(ch.PrepaintEvent) ? ch.ScriptIDOnly : ch.ScriptID;
            sl.CrossColumnType = ct;
            sl.Tag = ch.Caption;
            retCells.AddDirectly(sl);
        }

        /// <summary>
        /// ���ڵ�һ���з���ֵ�Ƿ���ͬ,����ͬ����������С��
        /// </summary>
        /// <param name="PreFldName"></param>
        /// <param name="NextFldName"></param>
        /// <param name="iCompareLevel"> �����еıȽϲ���</param>
        /// <returns></returns>
        private bool CompareHeadColVal(string PreFldName, string NextFldName, int iCompareLevel)
        {
            bool bRet = false;
            string[] preArr = PreFldName.Split(_separators, StringSplitOptions.None);
            string[] nextArr = NextFldName.Split(_separators, StringSplitOptions.None);
            if (preArr.Length > iCompareLevel && nextArr.Length > iCompareLevel)
            {
                string pre = "", next = "";
                for (int i = 1; i <= iCompareLevel; i++)
                {
                    pre += preArr[i];
                    next += nextArr[i];
                }
                if (pre == next)
                    bRet = true;
            }
            return bRet;
        }

        /// <summary>
        /// ����С����
        /// </summary>
        /// <param name="cellName">Cell ��Nameֵ</param>
        /// <param name="fldName">�����ѯ������ֶ���</param>
        /// <param name="nLeft"></param>
        /// <param name="nTop"></param>
        /// <param name="serial"></param>        
        /// <param name="exp">С�Ʊ��ʽ</param>
        private void CreateSubTotalColumn(Cells retCells, string cellName, string cellCaption, string fldName, ref int nLeft, int nTop, int serial, string exp)
        {
            Cells chs = _report.Sections[SectionType.CrossColumnHeader].Cells;
            Cell hc = chs[chs.Count - 1];
            string headerevent = null;
            if (_crossObj.CrossFldsCount == 1)
            {
                headerevent = hc.PrepaintEvent;
            }

            Cell cell = _report.Sections[SectionType.CrossDetail].Cells[serial];
            string designname = cell.Name;
            GridCalculateColumn ch;
            if (cell is GridDecimal)
                ch = new GridCalculateColumn(cell as GridDecimal);
            else
                ch = new GridCalculateColumn(cell as GridCalculateColumn);
            ch.Name = cellName;
            ch.OldName = _crossObj.ArrOldCellName[serial];
            ch.Expression = exp;
            ch.bSummary = (cell as IGridCollect).bColumnSummary;

            (ch as IGridCollect).Operator = (cell as IGridCollect).Operator;
            //(ch as IGridCollect).bAutoAnalysis = false;

            ch.X = nLeft;
            ch.Width = cell.Width;
            nLeft += ch.Width;
            ch.Y = cell.Y;
            ch.Height = cell.Height;
            ch.Caption = cellCaption;
            ch.CaptionAlign = cell.CaptionAlign;
            //ch.CrossIndex = _Number;

            if (!string.IsNullOrEmpty(headerevent))
            {
                ch.PrepaintEvent = headerevent;
                ch.EventType = EventType.OnTitle;
                ch.ScriptID = hc.ScriptID;
            }
            else
            {
                ch.PrepaintEvent = cell.PrepaintEvent;
                ch.ScriptID = string.IsNullOrEmpty(cell.PrepaintEvent) ? cell.ScriptIDOnly : cell.ScriptID;
                (ch as IGridEvent).EventType = (cell as IGridEvent).EventType;
            }
            //up 2 rows is newly added

            //(ch as IDecimal).bShowWhenZero = (cell as IDecimal).bShowWhenZero;
            //(ch as IDecimal).Precision = (cell as IDecimal).Precision;
            //(ch as IDecimal).PointLength = (cell as IDecimal).PointLength;
            //(ch as IDecimal).FormatString = (cell as IDecimal).FormatString;
            //_dataSources.AddColumn(ch as IMapName);
            CreateDataSource(fldName, _crossObj.ArrCrossType[serial]);
            ch.CrossColumnType = CrossColumnType.CrossSubTotal;
            ch.CrossIndex = -2;
            ch.Tag = _crossObj.ArrFldCaption[serial] + "++++" + designname;

            retCells.AddDirectly(ch);
        }

        private string HandleDateTimeCaption(string cellCaption, string formatstring)
        {
            if (!bSummaryCaption(cellCaption))
            {
                try
                {
                    if (formatstring.Trim() != "")
                        cellCaption = Convert.ToDateTime(cellCaption).ToString(formatstring);
                    else
                        cellCaption = Convert.ToDateTime(cellCaption).ToShortDateString();
                }
                catch
                {
                }
            }
            return cellCaption;
        }
        /// <summary>
        /// ���콻����
        /// </summary>
        /// <param name="cellName"></param>
        /// <param name="fldName"></param>
        /// <param name="nLeft"></param>
        /// <param name="nTop"></param>
        /// <param name="serial"></param>
        /// <param name="crossIndex"></param>
        /// <param name="bTotal"></param>
        private void CreateColumns(Cells retCells, string cellName, string cellCaption, string fldName, ref int nLeft, int nTop, int serial, int crossIndex, bool bTotal)
        {
            Cells chs = _report.Sections[SectionType.CrossColumnHeader].Cells;
            Cell ch = chs[chs.Count - 1];
            string headerevent = null;
            if (_crossObj.CrossFldsCount == 1)
            {
                if (ch is IBDateTime && (ch as IBDateTime).bDateTime)
                    cellCaption = HandleDateTimeCaption(cellCaption, (ch as IFormat).FormatString);
                headerevent = ch.PrepaintEvent;
            }

            Cell cell = _report.Sections[SectionType.CrossDetail].Cells[serial];
            if (_crossObj.ArrCrossType[serial] == DataType.Decimal)// || (cell is IDataSource && (cell as IDataSource).DataSource.IsADecimal ))
            {
                GridCalculateColumn gl;
                string designname = cell.Name;
                if (cell is Indicator)
                    gl = new CalculateIndicator(cell as Indicator);
                else if (cell is CalculateIndicator)
                    gl = new CalculateIndicator(cell as CalculateIndicator);
                else if (cell is GridDecimal)
                    gl = new GridCalculateColumn(cell as GridDecimal);// GridDecimal();
                else if (cell is GridCalculateColumn)
                    gl = new GridCalculateColumn(cell as GridCalculateColumn);// GridDecimal();
                else //if(cell is GridLabel)
                    gl = new GridCalculateColumn(cell as GridLabel);

                //gl.DataSource = CreateDataSource(fldName, _crossObj.ArrCrossType[serial]);
                CreateDataSource(fldName, _crossObj.ArrCrossType[serial]);
                if (bTotal && cell is ICalculateColumn && cell is IGridCollect && (cell as IGridCollect).Operator == OperatorType.ExpressionSUM)
                    gl.Expression = (cell as ICalculateColumn).Expression;
                else
                    gl.Expression = "[" + fldName + "]+0";//�����������Ŀ�ķ�ֹ���Ʋ��Ϸ����ֶ���
                gl.Caption = cellCaption.Trim().Length > 0 ? cellCaption : GetResString("����");//���ڽ�����Ϊ��ֵ��һ���ԡ�������������
                //gl.Caption = cellCaption.Trim().Length > 0 ? cellCaption : string.Empty;//���ڽ�����Ϊ��ֵ��һ���ԡ�������������
                gl.CaptionAlign = _crossObj.ArrCaptionAlign[serial];
                gl.Name = cellName;
                gl.OldName = _crossObj.ArrOldCellName[serial];

                gl.CrossIndex = crossIndex;
                gl.X = nLeft;
                gl.Y = nTop;
                gl.Width = _crossObj.ArrFldWidth[serial];
                gl.Height = _crossObj.ArrFldHeight[serial];

                //if (gl.Operator == OperatorType.AccumulateSUM ||
                //    gl.Operator == OperatorType.BalanceSUM ||
                //    gl.Operator == OperatorType.ExpressionSUM)
                //    gl.Operator = OperatorType.SUM;
                if (gl.Operator == OperatorType.AccumulateSUM ||
                    gl.Operator == OperatorType.BalanceSUM)
                    gl.Operator = OperatorType.SUM;

                if (!string.IsNullOrEmpty(headerevent))
                {
                    gl.PrepaintEvent = headerevent;
                    gl.EventType = EventType.OnAll;
                    gl.ScriptID = ch.ScriptID;
                }
                else
                {
                    gl.PrepaintEvent = _crossObj.ArrPrepaintEvent[serial];
                    gl.EventType = _crossObj.ArrEventType[serial];
                    gl.ScriptID = string.IsNullOrEmpty(gl.PrepaintEvent) ? cell.ScriptIDOnly : cell.ScriptID;
                }

                //Cell cell = _report.Sections[SectionType.CrossDetail].Cells[serial];
                //(gl as GridCalculateColumn).bShowWhenZero = (cell as IDecimal).bShowWhenZero;
                //(gl as GridCalculateColumn).Precision = (cell as IDecimal).Precision;
                //(gl as GridCalculateColumn).PointLength = (cell as IDecimal).PointLength;
                //(gl as GridCalculateColumn).FormatString = (cell as IDecimal).FormatString;
                //if(_crossObj.CrossFldsCount ==1)
                //    gl.CrossPaintEvent  = ch.PrepaintEvent;  //�����¼�
                gl.CrossColumnType = bTotal ? CrossColumnType.CrossTotal : CrossColumnType.CrossDetail;

                if (!bTotal)
                {
                    if ((chs.Count == 1 && _crossObj.CrossFldsCount == 1) || _crossObj.CrossFldsCount >= 2)
                        //    gl.Tag = cell.Caption + "++++" + designname;//ch.Caption
                        //else if (_crossObj.CrossFldsCount == 2)
                        gl.Tag = _crossObj.ArrFldCaption[serial] + "++++" + designname;
                    else
                        gl.Tag = "____";

                    if (gl is IIndicator)
                    {
                        (gl as IIndicator).TotalCompare = null;
                    }
                }
                else
                {
                    gl.CrossIndex = -1;
                    gl.Tag = "____";
                    if (gl is IIndicator)
                    {
                        (gl as IIndicator).SummaryCompare = null;
                        (gl as IIndicator).DetailCompare = null;
                    }
                }

                retCells.AddDirectly(gl);
            }
            else
            {
                GridColumnExpression gl = new GridColumnExpression();
                //gl.DataSource = CreateDataSource(fldName, _crossObj.ArrCrossType[serial]);
                CreateDataSource(fldName, _crossObj.ArrCrossType[serial]);
                gl.Expression = "[" + fldName + "]+''";
                gl.Caption = cellCaption;
                gl.CaptionAlign = _crossObj.ArrCaptionAlign[serial];
                gl.Name = cellName;
                gl.OldName = _crossObj.ArrOldCellName[serial];

                gl.CrossIndex = bTotal ? -1 : crossIndex;
                gl.X = nLeft;
                gl.Y = nTop;
                gl.Width = _crossObj.ArrFldWidth[serial];
                gl.Height = _crossObj.ArrFldHeight[serial];

                if (!string.IsNullOrEmpty(headerevent))
                {
                    gl.PrepaintEvent = headerevent;
                    gl.EventType = EventType.OnAll ;
                    gl.ScriptID = ch.ScriptID;
                }
                else
                {
                    gl.PrepaintEvent = _crossObj.ArrPrepaintEvent[serial];
                    gl.EventType = _crossObj.ArrEventType[serial];
                    gl.ScriptID = string.IsNullOrEmpty(gl.PrepaintEvent) ? cell.ScriptIDOnly : cell.ScriptID;
                }

                //if (_crossObj.CrossFldsCount == 1)
                //    gl.CrossPaintEvent  = ch.PrepaintEvent;//�����¼�
                gl.CrossColumnType = bTotal ? CrossColumnType.CrossTotal : CrossColumnType.CrossDetail;
                retCells.AddDirectly(gl);
            }
            nLeft += _crossObj.ArrFldWidth[serial];

        }

        /// <summary>
        /// ��������Դ
        /// </summary>
        /// <param name="fldName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private void CreateDataSource(string fldName, DataType type)
        {
            DataSource ds = new DataSource();
            ds.Caption = fldName;
            ds.Name = fldName;
            ds.Type = type;
            _report.DataSources.Add(ds);
        }

        /// <summary>
        /// ��ȡ�ֶΣ�����ֶ�ʹ�ö���ƴ��
        /// </summary>
        /// <param name="tCells"></param>
        /// <returns></returns>
        private string GetFlds(Cells tCells, bool bRowGroup, out string rowGroupbyFlds, out int cellCount)
        {
            string sRet = "";
            string fldName = "";
            bool bAddGroupbyFlds = true;
            rowGroupbyFlds = "";
            Cell cell;
            int i = 0;
            while (i < tCells.Count)
            {
                cell = tCells[i];
                i++;
                if (cell is IDataSource && (cell as IDataSource).DataSource.IsEmpty)
                {
                    if (!bRowGroup)
                    {
                        i--;
                        tCells.RemoveAt(i);
                    }

                    continue;
                }

                if (cell is IDataSource &&
                    (((cell as IDataSource).DataSource.Name.ToLower() == _report.BaseID.ToLower()) || !_report.DataSources.Contains((cell as IDataSource).DataSource.Name)))
                {
                    i--;
                    tCells.RemoveAt(i);
                    continue;
                }

                if (!(cell is IMapName))
                    continue;

                fldName = (cell as IMapName).MapName;
                bAddGroupbyFlds = true;
                if (bRowGroup)
                {
                    if (cell is IAlgorithm)
                    {
                        continue;
                    }
                    if (cell is IDecimal)
                    {
                        if (cell is IDataSource && !(cell as IDataSource).DataSource.IsADecimal)
                        {
                            (cell as IGridCollect).bClue = true;
                            (cell as IGridCollect).bColumnSummary = false;
                            (cell as IGridCollect).bSummary = false;
                        }
                        // V12.5 �޸ģ������ֵ����Ϊ������ͼ�����飩�б��⣬Ҳ���ܲ���ϼ�����
                        if (!(cell as IGridCollect).bClue && !_report.Sections[SectionType.CrossRowHeader].Cells.Contains(cell))
                        {
                            bAddGroupbyFlds = false;
                            fldName = (cell as IMapName).MapName;
                            //if (!(cell as IGridCollect).bClue)
                            fldName = "SUM(isnull([" + fldName + "],0)) AS [" + fldName + "]";
                            //else
                            //    fldName = "MAX(isnull([" + fldName + "],0)) AS [" + fldName + "]";
                            sRet = sRet + fldName + ",";
                        }

                        //else
                        //{
                        //bAddGroupbyFlds = false;
                        ////if (cell is IDataSource)
                        ////    fldName = (cell as IDataSource).DataSource.Name;
                        ////else
                        ////    fldName = ReportEngine.HandleExpression(_report.DataSources,(cell as ICalculateColumn).Expression,"");
                        ////if (!(cell as IGridCollect).bClue)
                        ////    fldName = "SUM(" + fldName + ") AS " + (cell as IMapName).MapName;
                        ////else
                        ////    fldName = "MAX(" + fldName + ") AS " + (cell as IMapName).MapName; 

                        //fldName = (cell as IMapName).MapName;
                        //if (!(cell as IGridCollect).bClue)
                        //    fldName = "SUM(isnull([" + fldName + "],0)) AS [" + fldName + "]";
                        //else
                        //    fldName = "MAX(isnull([" + fldName + "],0)) AS [" + fldName + "]";
                        //sRet = sRet + fldName + ",";
                        //}
                    }
                }
                if (bAddGroupbyFlds)
                {
                    rowGroupbyFlds = rowGroupbyFlds + " [" + fldName + "],";
                    sRet = sRet + " [" + fldName + "],";
                }
            }
            if (rowGroupbyFlds.Length > 0)
                rowGroupbyFlds = rowGroupbyFlds.Substring(0, rowGroupbyFlds.Length - 1);//ȥ����󶺺�
            if (sRet.Length > 0)
                sRet = sRet.Substring(0, sRet.Length - 1);//ȥ����󶺺�
            cellCount = tCells.Count;
            return sRet;
        }

        /// <summary>
        /// ���û��������з���������µ���
        /// to use in V11.0 
        /// </summary>
        /// <param name="tCells"></param>
        /// <param name="bHeadCol"></param>
        /// <param name="SelectSortFlds"></param>
        /// <returns>��ȡʵ�ʵ��з�������</returns>
        private string GetColumnOrders(Cells tCells, bool bHeadCol, out string SelectSortFlds)
        {
            string sRet = "";
            string SortSourceName = "";
            SelectSortFlds = "";
            string sort = "";

            for (int i = 0; i < tCells.Count; i++)
            {
                Cell cell = tCells[i];
                if (cell is IDataSource && (cell as IDataSource).DataSource.IsEmpty)
                    continue;
                switch ((cell as ISort).SortOption.SortDirection)
                {
                    case SortDirection.Ascend:
                        sort = " ASC ";
                        break;
                    case SortDirection.Descend:
                        sort = " DESC ";
                        break;
                    default:
                        if (bHeadCol)
                            sort = " ASC ";//���ڽ����б���������򣬷����������Ͻ����ѯ���˳����ȷ
                        else
                            sort = "";
                        break;
                }
                if (sort.Length > 0)
                {
                    bool bspecialsort = true;
                    SortSourceName = (cell as IMultiHeader).SortSource.Name;
                    if (SortSourceName == "" || SortSourceName == "EmptyColumn")
                    {
                        SortSourceName = (cell as IMapName).MapName;
                        bspecialsort = false;
                    }
                    if (bHeadCol && !bspecialsort)
                        SelectSortFlds += ",[" + SortSourceName + "] ";
                    else
                        SelectSortFlds += ",min(" + SortSourceName + ") as " + SortSourceName;
                    sRet = sRet + SortSourceName + sort + ",";
                }
            }
            if (sRet.Length > 0)
                sRet = " Order by " + RemoveLastComma(sRet);
            return sRet;
        }

        /// <summary>
        /// ��ȡ�����ֶ�
        /// </summary>
        /// <param name="tCells"></param>
        /// <param name="SortSourceName"></param>
        /// <returns></returns>
        private string GetOrders(Cells tCells, bool bHeadCol, out string SelectSortFlds)
        {
            string sRet = "";
            string SortSourceName = "";
            SelectSortFlds = "";
            string sort = "";
            for (int i = 0; i < tCells.Count; i++)
            {
                Cell cell = tCells[i];
                if (cell is IDataSource && (cell as IDataSource).DataSource.IsEmpty)
                    continue;
                switch ((cell as ISort).SortOption.SortDirection)
                {
                    case SortDirection.Ascend:
                        sort = " ASC ";
                        break;
                    case SortDirection.Descend:
                        sort = " DESC ";
                        break;
                    default:
                        if (bHeadCol)
                            sort = " ASC ";//���ڽ����б���������򣬷����������Ͻ����ѯ���˳����ȷ
                        else
                            sort = "";
                        break;
                }
                if (sort.Length > 0)
                {
                    bool bspecialsort = true;
                    SortSourceName = (cell as IMultiHeader).SortSource.Name;
                    if (SortSourceName == "" || SortSourceName == "EmptyColumn")
                    {
                        SortSourceName = (cell as IMapName).MapName;
                        bspecialsort = false;
                    }
                    if (bHeadCol && !bspecialsort)
                        SelectSortFlds += ",[" + SortSourceName + "] ";
                    else
                        SelectSortFlds += ",min(" + SortSourceName + ") as " + SortSourceName;
                    sRet = sRet + SortSourceName + sort + ",";
                }
            }
            if (sRet.Length > 0)
                sRet = " Order by " + RemoveLastComma(sRet);
            return sRet;
        }

        /// <summary>
        /// ȥ�����һ������
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private string RemoveLastComma(string val)
        {
            if (val.Length > 0)
                val = val.Substring(0, val.Length - 1);//ȥ����󶺺�     
            return val;
        }

        /// <summary>
        /// ��ý�����ѯSql��
        /// </summary>
        /// <param name="cnnStr">���ݿ����Ӵ�</param>
        /// <param name="srcTableName">����Դ����</param>
        ///<param name="srcTableName">����Ŀ�ı���</param>
        /// <param name="rowGroup">�з�������</param>
        /// <param name="headColGroup">��ͷ�з�������</param>
        /// <param name="colSummaryNum">�ϼ�����Ŀ</param>
        /// <returns>���ؽ���Sql��</returns>
        private string GetCrossSql(string srcTableName, string cnnStr, string rowGroup, string rowGroupbyFlds, string headColGroup, string SelectSortFlds, string headColOrder, ref string selectsql)
        {
            string strSql = "";
            string headColGroupVal = "";
            //string srcTableName = _report.CrossTable;
            string descTableName = _report.BaseTable;
            string headColGroupNotNull = "";
            SqlDataReader dr;
            
            StringBuilder sb2 = new StringBuilder();
            ////sb.Append("select " + rowGroup);        //��ѯ��ǰ���
            //headColGroupNotNull = "isnull(" + headColGroup.Replace(",", ",'')+'@@@'+isnull(") + ",'')";
            //headColGroupNotNull = "Convert(nvarchar(300),isnull(" + headColGroup.Replace(",", ",''))+'@@@'+Convert(nvarchar(300),isnull(") + ",''))";
            headColGroupNotNull = "isnull(Convert(nvarchar(300)," + headColGroup.Replace(",", ",121),'')+'@@@'+isnull(Convert(nvarchar(300),") + ",121),'')";
            //2011-4-27 strSql = "SELECT  " + headColGroupNotNull + SelectSortFlds + " from " + srcTableName + " Group by " + headColGroup + headColOrder;
            strSql = "if COL_LENGTH('" + srcTableName + "','ufreport_cross_order_fld') is not null select 1";
            try
            {
                dr = SqlHelper.ExecuteReader(cnnStr, strSql);
                strSql = " SELECT  " + headColGroupNotNull + SelectSortFlds + " from " + srcTableName + " Group by " + headColGroup + headColOrder;
                while (dr.Read())
                {
                    strSql = " SELECT  " + headColGroupNotNull + SelectSortFlds + " from " + srcTableName + " Group by " + headColGroup + ",ufreport_cross_order_fld order by ufreport_cross_order_fld asc";
                }
            }
            catch (SqlException ex)
            {
                strSql = " SELECT  " + headColGroupNotNull + SelectSortFlds + " from " + srcTableName + " Group by " + headColGroup + headColOrder;
            }
            //2011-4-27
            dr = SqlHelper.ExecuteReader(cnnStr, strSql);
            int kk = 0;
            bool bhasempty = false;
            while (dr.Read())
            {
                kk++;
                headColGroupVal = dr.GetValue(0).ToString();
                if (headColGroupVal == "" && bhasempty)
                    continue;
                if (headColGroupVal == "")
                    bhasempty = true;
                if (sb2.Length > 0)
                    sb2.Append(",");
                sb2.Append( GetCrossFlds(1, headColGroupNotNull, headColGroupVal));
                if (kk > 500)
                    throw new Exception(UFIDA.U8.UAP.Services.ReportResource.String4Report.GetString(
                    "�򿪱���ʧ�ܣ������й��࣬���������ý��淽����", _login.LocaleID));
            }
            if (_crossObj.ColSummaryNum > 0)
            {
                if (sb2.Length > 0)
                    sb2.Append(",");
                sb2.Append(GetCrossFlds(2, headColGroup, headColGroupVal)); //��ѯ��β   
            }            

            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append(rowGroup);
            if (!string.IsNullOrEmpty(rowGroup) && sb2.Length>0)
                sb.Append(",");
            sb.Append(sb2.ToString());

            StringBuilder sb1 = new StringBuilder(sb.ToString());
            sb.Append(",IDENTITY(INT,0,1) as ");//��������Է�Ӱ����㽻���е�˳��;��0��ʼ���
            sb.Append(_report.BaseID);
            sb.Append(" Into ");
            sb.Append(descTableName);
            sb.Append(" from ");
            sb1.Append(" from ");
            sb.Append(srcTableName);
            sb1.Append(srcTableName);
            sb.Append(" where 1=0 ");
            if (rowGroupbyFlds.Trim().Length > 0)
            {
                sb.Append(" group by " + rowGroupbyFlds);
                sb1.Append(" group by " + rowGroupbyFlds);
            }
            selectsql = sb1.ToString();
            return sb.ToString();
        }
        /// <summary>
        /// �������н�����
        /// </summary>
        /// <param name="crossFlds">������ֶ���</param>
        /// <param name="iFlag">�������ͣ�1����������ֶα�־��2���������ͳ���ֶα�־</param>
        /// <param name="funFlag">�ۺϺ������㷽ʽ</param>
        /// <param name="headColGroup">��ͷ�з�������</param>
        /// <param name="headColGroupVal">��ͷ�з�����ֵ</param>
        /// <returns>���ض��н��洮</returns>
        private string GetCrossFlds(int iFlag, string headColGroup, string headColGroupVal)
        {
            string sRet = "";
            for (int i = 0; i < _crossObj.ArrFldName.Length; i++)
            {
                if (sRet.Length > 0 && !sRet.EndsWith(","))
                    sRet += ",";
                sRet += CreateCrossCol(_crossObj.ArrFldName[i], iFlag, _crossObj.ArrFunFlag[i], _crossObj.ArrbColumnSummary[i], headColGroup, headColGroupVal);
            }
            if (sRet.EndsWith(","))
                sRet = sRet.Substring(0, sRet.Length - 1);
            return sRet;
        }

        /// <summary>
        /// ���쵥�������ֶν����д�
        /// </summary>
        /// <param name="numFld">���������ֶ�</param>
        /// <param name="iFlag">�������ͣ�1����������ֶα�־��2���������ͳ���ֶα�־</param>
        /// <param name="funFlag"><�ۺϺ������㷽ʽ/param>
        /// <param name="headColGroup">��ͷ�з�������</param>
        /// <param name="headColGroupVal">��ͷ�з�����ֵ</param>
        /// <returns>���ص��н��洮</returns>
        private string CreateCrossCol(string numFld, int iFlag, string funFlag, bool bColumnSummary, string headColGroup, string headColGroupVal)
        {
            string sRet = "";
            string headColGroupVal1 = headColGroupVal.IndexOf(']', 0) > 0 ? headColGroupVal.Replace("]", "]]") : headColGroupVal;
            if (iFlag == 1)
                sRet = funFlag + "(CASE " + headColGroup + " WHEN '" + headColGroupVal + "' THEN " + (_report.DataSources.Contains(numFld) ? "[" + numFld + "]" : ReportEngine.HandleExpression(_report.DataSources, numFld, "", false)) + " ELSE Null END) AS [" + numFld + "@@@" + headColGroupVal1 + "]";
            else if (iFlag == 2 && funFlag.Length > 0)
                if (bColumnSummary)
                {
                    sRet = funFlag + "(" + (_report.DataSources.Contains(numFld) ? "[" + numFld + "]" : ReportEngine.HandleExpression(_report.DataSources, numFld, "", false)) + ") AS [" + numFld + "]";//cwhҪ��ϼ�ֱ�����ֶ���[" + funFlag + "@@@" + numFld + "]";
                }
            return sRet;
        }


        /// <summary>
        /// ������Դ����
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string GetResString(string text)
        {
            string sRet = "";
            string localeId = _login.LocaleID.ToLower();
            if (localeId == "zh-cn")
                return text;
            switch (text)
            {
                case "С��":
                    if (localeId == "zh-tw")
                        sRet = "СӋ";
                    else
                        sRet = "SubTotal";
                    break;
                case "�ϼ�":
                    if (localeId == "zh-tw")
                        sRet = "��Ӌ";
                    else
                        sRet = "Summary";
                    break;
                case "����":
                    if (localeId == "zh-tw")
                        sRet = "����";
                    else
                        sRet = "Other";
                    break;
            }
            return sRet;
        }

        private bool bSummaryCaption(string caption)
        {
            return caption == GetResString("С��") || caption == GetResString("�ϼ�");
        }
    }

    internal class CrossObj
    {
        //˵�������й������ֶΰ����Է���д��Ŀ�ļ򻯴���
        public string[] ArrFunFlag;             //ͳ�ƾۺϺ���
        public string[] ArrSumExp;              //С��ͳ�Ʊ��ʽ         
        public int[] ArrSumNum;                 //ͳ���и��� 
        public bool[] ArrbColumnSummary;        //�Ƿ���ҪС��

        public int HeadColGroupCount = 1;       //�����б�ͷ����

        public string[] ArrFldName;             //�����ֶ����ݿ���        
        public string[] ArrFldCaption;          //��������ʾ
        public string[] ArrOldCellName;         //ԭʼ��CellName
        public DataType[] ArrCrossType;         //�����ֶ���������

        public int ColSummaryNum = 0;             //ͳ���ֶθ���
        public int CrossFldsCount = 0;          //�����ֶθ���

        public int[] ArrFldWidth;               //�����ֶο��
        public int[] ArrFldHeight;              //�����ֶθ߶�
        public ContentAlignment[] ArrCaptionAlign;//������뷽ʽ

        public string[] ArrPrepaintEvent;        //
        public EventType[] ArrEventType;          // 


        /// <summary>
        /// 
        /// </summary>        
        public CrossObj(Report rep)
        {
            CrossFldsCount = rep.Sections[SectionType.CrossDetail].Cells.Count;
            ArrFunFlag = new string[CrossFldsCount];
            ArrSumExp = new string[CrossFldsCount];
            ArrSumNum = new int[CrossFldsCount];
            ArrbColumnSummary = new bool[CrossFldsCount];

            ArrFldName = new string[CrossFldsCount];
            ArrFldCaption = new string[CrossFldsCount];
            ArrOldCellName = new string[CrossFldsCount];
            ArrCrossType = new DataType[CrossFldsCount];

            ArrFldWidth = new int[CrossFldsCount];
            ArrFldHeight = new int[CrossFldsCount];
            ArrCaptionAlign = new ContentAlignment[CrossFldsCount];

            ArrPrepaintEvent = new string[CrossFldsCount];
            ArrEventType = new EventType[CrossFldsCount];

            for (int i = 0; i < CrossFldsCount; i++)
            {
                Cell cell = rep.Sections[SectionType.CrossDetail].Cells[i];
                if (cell is ICalculateColumn)//���Ǽ���Ľ����У�ֱ��ʹ�ñ��ʽ
                    ArrFldName[i] = (cell as ICalculateColumn).Expression;
                else
                    ArrFldName[i] = (cell as IMapName).MapName;
                ArrOldCellName[i] = cell.Name;
                ArrFldCaption[i] = cell.Caption;
                ArrCrossType[i] = DataType.String;

                ArrFldWidth[i] = cell.Width;
                ArrFldHeight[i] = cell.Height;
                ArrCaptionAlign[i] = cell.CaptionAlign;

                ArrPrepaintEvent[i] = cell.PrepaintEvent;
                ArrEventType[i] = (cell as IGridEvent).EventType;

                ArrFunFlag[i] = "";
                ArrbColumnSummary[i] = false;
                if (cell is IDecimal)
                {
                    if (!(cell is IDataSource) || (cell as IDataSource).DataSource.IsADecimal)
                        ArrCrossType[i] = DataType.Decimal;
                    else
                    {
                        (cell as IGridCollect).bSummary = false;
                        (cell as IGridCollect).bColumnSummary = false;
                    }
                    if ((cell as IGridCollect).bColumnSummary)////�Ƿ���С��
                    {
                        ArrFunFlag[i] = GetFunFlag((cell as IGridCollect).Operator);
                        ArrbColumnSummary[i] = true;
                        ColSummaryNum++;
                    }
                    else
                    {
                        if (ArrCrossType[i] != DataType.Decimal)
                            ArrFunFlag[i] = "Max";
                        else
                            ArrFunFlag[i] = GetFunFlag((cell as IGridCollect).Operator);
                    }
                }
                else
                {
                    ArrFunFlag[i] = "Max";//�ַ�
                }
            }
        }

        /// <summary>
        /// ��ȡͳ�ƺ������ַ�
        /// </summary>
        /// <param name="type">��������</param>
        /// <returns></returns>
        private string GetFunFlag(OperatorType type)
        {
            string sRet = "";
            switch (type)
            {
                case OperatorType.AVG:
                    sRet = "AVG";
                    break;
                case OperatorType.MAX:
                    sRet = "Max";
                    break;
                case OperatorType.MIN:
                    sRet = "Min";
                    break;
                default:
                    sRet = "Sum";
                    break;
            }
            return sRet;
        }

    }
}
