using System.Windows.Forms;
using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Globalization;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportElements;
using UFIDA.U8.UAP.Services.ReportFilterService;
namespace UFIDA.U8.UAP.Services.DynamicReportComponent.DRC0496aa7350e740ac91da386a4514a81f
{
    public class Global : ITest
    {
        private DataHelper _datahelper;
        public DataHelper DataHelper
        {
            get
            {
                return _datahelper;
            }
            set
            {
                _datahelper = value;
            }
        }
        public void Init(DataHelper datahelper)
        {
            if (datahelper.Global == null)
            {
                Global global = new Global();
                global.DataHelper = datahelper;
                datahelper.Global = global;
            }
        }
        public object ExecuteScalar(string sql, VarientType type)
        {
            DataSet ds = _datahelper.Exec(sql);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                if (type == VarientType.SQL_DateTime)
                    return NullValue.DateTime;
                else if (type == VarientType.SQL_Decimal)
                    return NullValue.Decimal;
                else
                    return NullValue.String;
            }
            else
            {
                return ds.Tables[0].Rows[0][0];
            }
        }
        public object ExecuteScalar(string sql)
        {
            DataSet ds = _datahelper.Exec(sql);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                return null;
            }
            else
            {
                return ds.Tables[0].Rows[0][0];
            }
        }
        public DataSet Execute(string sql)
        {
            return _datahelper.Exec(sql);
        }
        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg, "Message", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
        }
        public void SaveGlobal(PageInfo pi)
        {
        }
        public void RestoreGlobal(PageInfo pi)
        {
            if (pi.GlobalValues == null)
                return;
        }
        public object Test()
        {
            return null;
        }
    }
    public class Cell不良品原因比率 : ICellEvent
    {
        public void Prepaint(Report report, RowData data, Cell innercell, FilterSrv filter, AgileArgs args, DataHelper datahelper, ReportSummaryData reportsummary, RowBalance rowbalance, AccumulateData accumulate, BalanceData balance, object[] others)
        {
            Current current = null;
            int grouplevels = report.GroupLevels;
            int currentindex = -1;
            int startindex = -1;
            Groups groups = null;
            Group currentgroup = null;
            RowData columntodata = null;
            Current previous = new Current(rowbalance);
            if (rowbalance != null)
            {
                currentindex = rowbalance.CurrentIndex;
                startindex = rowbalance.StartIndex;
            }
            SemiRow cells = (data != null ? data.SemiRow : null);
            SemiRow row = cells;
            IKeyToObject nametodata = cells as IKeyToObject;
            StimulateCell cell = new StimulateCell(innercell);
            if (data is Group)
            {
                cell.bInGroup = true;
                currentgroup = data as Group;
                columntodata = currentgroup;
            }
            else if (!(data is ReportSummaryData))
            {
                current = new Current(data);
                columntodata = data;
            }
            else
            {
                cell.bInReportSummary = true;
                columntodata = data;
            }
            if (nametodata == null)
            {
                nametodata = columntodata as IKeyToObject;
            }
            Global global;
            if (datahelper.Global == null)
            {
                global = new Global();
                global.DataHelper = datahelper;
                datahelper.Global = global;
            }
            else
            {
                global = datahelper.Global as Global;
            }
            if (Convert.ToDouble(reportsummary["不良品数量"]) == 0)
            {
                cell.Caption = 0;
                return;
            }
            if (current != null)
            {
                cell.Caption = Convert.ToDouble(current["不良品数量"]) / Convert.ToDouble(reportsummary["不良品数量"]);
            }
            else if (currentgroup != null)
            {
                cell.Caption = Convert.ToDouble(currentgroup["不良品数量"]) / Convert.ToDouble(reportsummary["不良品数量"]);
            }
            else
            {
                cell.Caption = 1;
            }

        }
    }
    public class DataRowsForEx
    {
        private ArrayList _rows;
        public DataRowsForEx(DataTable table)
        {
            _rows = new ArrayList();
            if (table != null)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                    _rows.Add(table.Rows[i]);
            }
        }
        public IEnumerator GetEnumerator()
        {
            return _rows.GetEnumerator();
        }
        public int Length
        {
            get
            {
                return _rows.Count;
            }
        }
        public DataRow this[int index]
        {
            get
            {
                return (Length == 0 ? null : (_rows[index] as DataRow));
            }
        }
    }
    public class Current
    {
        private RowData _rowdata;
        public Current(RowData rowdata)
        {
            _rowdata = rowdata;
        }
        public object this[string name]
        {
            get
            {
                return _rowdata[name];
            }
            set
            {
                _rowdata[name] = value;
            }
        }
        public double Function0
        {
            get
            {
                if (_rowdata["不良品数量"] == DBNull.Value)
                    return (double)NullValue.Decimal;
                else
                    return Convert.ToDouble(_rowdata["不良品数量"]);
            }
            set
            {
                _rowdata["不良品数量"] = value;
            }
        }
    }
}