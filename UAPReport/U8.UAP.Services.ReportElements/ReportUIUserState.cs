using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class ReportUIUserStateInfo
    {
        private string userId;
        private string reportId;
        private PrintSetings printSet;
        private PrintDefaults printDefault;
        private bool includeBackColor;
        private Layouts layout;
        private FilterInfos filterInfo;
        private int extendStatus;
        private FilterType filterFlag = FilterType.FixFilter;
        private bool solutionVisible = true;
        private bool conditionVisible = true;
        private QuickFilterLayout quickFilterLayoutFlag = QuickFilterLayout.LeftRight;
        private bool layoutIsSetted = false;

        /// <summary>
        /// 用户是否已经设置了左右布局还是上下布局
        /// </summary>
        public bool LayoutIsSetted
        {
            get { return layoutIsSetted; }
            set { layoutIsSetted = value; }
        }
        public int ExtendStatus
        {
            get { return extendStatus; }
            set { extendStatus = value; }
        }
        /// <summary>
        /// 0--显示固定条件 1--显示快捷条件
        /// </summary>
        public FilterType FilterFlag
        {
            get { return filterFlag; }
            set { filterFlag = value; }
        }
        

        /// <summary>
        /// 快捷条件是否显示查询方案
        /// </summary>
        public bool SolutionVisible
        {
            get { return solutionVisible; }
            set { solutionVisible = value; }
        }
        /// <summary>
        /// 快捷条件是否显示查询条件
        /// </summary>
        public bool ConditionVisible
        {
            get { return conditionVisible; }
            set { conditionVisible = value; }
        }
        /// <summary>
        /// 0--左右布局 1--上下布局
        /// </summary>
        public QuickFilterLayout QuickFilterLayoutFlag
        {
            get { return quickFilterLayoutFlag; }
            set { quickFilterLayoutFlag = value; }
        }

        private bool bShowChart;

       /// <summary>
       /// 是否显示图表，默认上下显示，12.0增加
       /// </summary>
        public bool BShowChart
        {
            get { return bShowChart; }
            set { bShowChart = value; }
        }


        /// <summary>
        /// 条件信息
        /// </summary>
        public FilterInfos FilterInfo
        {
            get { return filterInfo; }
            set { filterInfo = value; }
        }

        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }
       
        public string ReportId
        {
            get { return reportId; }
            set { reportId = value; }
        }
       
        /// <summary>
        ///打印设置
        /// </summary>
        public PrintSetings PrintSet
        {
            get { return printSet; }
            set { printSet = value; }
        }
        
        /// <summary>
        /// 打印默认按钮
        /// </summary>
        public PrintDefaults PrintDefault
        {
            get {
                if (printDefault == 0)
                    printDefault = PrintDefaults.Print;
                return printDefault;
            }
            set { printDefault = value; }
        }
      
        /// <summary>
        /// 打印时是否包含背景色
        /// </summary>
        public bool IncludeBackColor
        {
            get { return includeBackColor; }
            set { includeBackColor = value; }
        }

        /// <summary>
        /// 布局信息
        /// </summary>
        public Layouts Layout
        {
            get { return layout; }
            set { layout = value; }
        }
    }

    /// <summary>
    /// 0--显示固定条件 1--显示快捷条件
    /// </summary>
    public enum FilterType
    {
        FixFilter = 0,
        QuickFilter = 1
    }
    /// <summary>
    /// 0--左右布局 1--上下布局
    /// </summary>
    public enum QuickFilterLayout
    {
        LeftRight = 0,
        TopBottom = 1
    }


    public enum PrintSetings
    {
        none=0,
        PrintDesignTimeFilter=1,
        PrintRuntimeFilter=2
    }

    public enum PrintDefaults
    {
        Print=1,
        priview=2
    }

    public enum FilterInfos
    {
        DesignTimeFilter=1,
        RuntimeFilter=2,
        QuickFilter=3
    }

    public enum Layouts
    {
        UpDown=1,
        LeftRight=2
    }
}
