using Infragistics.UltraChart.Shared.Styles;
using Infragistics.Win.UltraWinChart;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// U易联报表图表专用
    /// </summary>
    [Serializable]
    public class MobileChart
    {
        private DataTable _dataTable;

        public DataTable DataTable
        {
            get { return _dataTable; }
            set { _dataTable = value; }
        }
        private ChartType _chartType;

        public ChartType ChartType
        {
            get { return _chartType; }
            set { _chartType = value; }
        }


        public MobileChart(ChartType chartType, DataTable dataTable)
        {
            this.ChartType = chartType;
            this.DataTable = dataTable;
        }
    }
}
