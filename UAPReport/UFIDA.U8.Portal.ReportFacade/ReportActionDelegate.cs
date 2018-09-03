using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using UFIDA.U8.Portal.Proxy.Actions;
using System.Diagnostics;

using UFIDA.U8.Portal.Common.Core;
using UFIDA.U8.Portal.Framework.Actions;
using UFIDA.U8.Portal.Framework.MainFrames;
using UFIDA.U8.UAP.Services.ReportExhibition;
using UFIDA.U8.Portal.Proxy;
using UFIDA.U8.Portal.Proxy.editors;
using UFIDA.U8.UAP.Services.ReportElements;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.Portal.ReportFacade
{
    internal class ReportActionDelegate : IActionDelegate
    {
        private ReportViewControl ReportViewControl;
        private string _reportId = string.Empty;

        public ReportActionDelegate(ReportViewControl reportViewControl)
        {
            this.ReportViewControl = reportViewControl;
            _reportId = this.ReportViewControl.ClientReportContext.GetReportId();
        }

        public void Run(IAction action)
        {
            System.Threading.Thread.Sleep(100);
            Trace.WriteLine("Run action");

            // 当前运行时对象已经不存在,不能进行任何操作
            if (!this.ReportViewControl.IsActionGoOn())
                return;
            //if (action.Tag == null)
            //{
            this.ReportViewControl.ReleaseFloatForm(action.Id);
            if (action.Id.Equals("PrintParent"))
            {

                this.SetPrintControl(this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.IncludeBackColor);
                if (this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.PrintDefault == PrintDefaults.Print)
                {
                    this.ReportViewControl.Print();
                }
                else
                    this.ReportViewControl.PrintView();
            }
            if (action.Id.Equals("Print"))
            {
                this.SetPrintControl(false);
                this.ReportViewControl.Print();
            }
            else if (action.Id.Equals("PrintView"))
            {
                this.SetPrintControl(false);
                this.ReportViewControl.PrintView();
            }
            if (action.Id.Equals("PrintInclueBackColor"))
            {
                this.SetPrintControl(true);
                this.ReportViewControl.Print();
            }
            else if (action.Id.Equals("PrintViewInclueBackColor"))
            {
                this.SetPrintControl(true);
                this.ReportViewControl.PrintView();
            }
            else if (action.SetGroup == "Print")
            {
                HandlePrint(action);
            }
            else if (action.Id.Equals("SaveAs"))
                this.ReportViewControl.SaveAs();
            else if (action.Id.Equals("Save"))
                this.ReportViewControl.Save();
            else if (action.Id.Equals("Output"))
                this.ReportViewControl.OutPut();
            else if (action.Id.Equals("Group"))
                this.ReportViewControl.ShowGroupSchema();
            else if (action.Id.Equals("Cross"))
                this.ReportViewControl.ShowCrossSchema();
            else if (action.Id.Equals("Expand") || action.Id.Equals("MenuKeyExpandCollapseRows"))
                this.ReportViewControl.ShowLevelExpand();
            else if (action.Id.Equals("Sort"))
                this.ReportViewControl.ShowQuickSort();
            else if (action.Id.Equals("Location"))
                this.ReportViewControl.ShowLocation();
            else if (action.Id.Equals("ReFilter"))
                this.ReportViewControl.ShowReFilter();
            //else if (action.Id.Equals("Column"))
            //    this.ReportViewControl.ShowColumnSetting();
            else if (action.Id.Equals("Format"))
                this.ReportViewControl.ShowFormat();
            else if (action.Id.Equals("AutoHeight"))
            {
                this.ReportViewControl.ChangeStyle(action.IsChecked);
                this.HandleMorsetButton(action);
                //处理折行情况
                IAction autoMergeAllCell = UserToolbarStateManager.FindActionByKey(_reportId, "AutoMergeAllCell");
                if (autoMergeAllCell!=null && autoMergeAllCell.IsVisible && autoMergeAllCell.IsChecked)
                {
                    this.ReportViewControl.AutoMergeAllCells(autoMergeAllCell.IsChecked);
                    this.HandleMorsetButton(autoMergeAllCell);
                }
            }
            else if (action.Id.Equals("AutoColumnFit"))
            {
                this.ReportViewControl.AutoColumnFit(action.IsChecked);
            }
            else if (action.Id == "AutoMergeAllCell")
            {
                this.ReportViewControl.AutoMergeAllCells(action.IsChecked);
                //this.HandleMorsetButton(action);
            }
            else if (action.Id.Equals("AutoPageFit"))
            {
                action.IsChecked = !action.IsChecked;
                this.ReportViewControl.AutoPageFit(action.IsChecked);
            }
            else if (action.Id.Equals("Query"))
                this.ReportViewControl.ShowQuery();
            else if (action.Id.Equals("First"))
                this.ReportViewControl.FirstPage();
            else if (action.Id.Equals("Previous"))
                this.ReportViewControl.PreviousPage();
            else if (action.Id.Equals("Next"))
                this.ReportViewControl.NextPage();
            else if (action.Id.Equals("Last"))
                this.ReportViewControl.LastPage();
            else if (action.Id.Equals("Help"))
                this.ReportViewControl.ShowHelp();
            else if (action.Id.Equals("Return"))
                this.ReportViewControl.NoneCross();
            else if (action.Id.Equals("ReportMapping"))
            {
                if (action.IsChecked)
                {
                    this.ReportViewControl.ShowReportDiagram();
                }
                else
                {
                    this.ReportViewControl.HideReportDiagram();
                }
                HandleTopCheckBoxButton(action);
            }
            else if (action.Id.Equals("Publish"))
                this.ReportViewControl.Publish();
            else if (action.Id.Equals(ContextMenuMetaAssigner.MenuKeySubTotal)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeyTotal))
            {
                this.ReportViewControl.SwithTotalRowVisible(action.Id);
                HandleTopCheckBoxButton(action);
            }
            //else if (action.Id.Equals("FilterValue"))
            //    this.ReportViewControl.ShowFileterValue();
            //分组
            else if (action.Id.Equals("PlanStyle"))
            {
                this.ReportViewControl.ChangeViewStyle("Plat");
                HangleGroupsStyle(action);
            }
            else if (action.Id.Equals("Collapse"))
            {
                this.ReportViewControl.ChangeViewStyle("Collapse");
                HangleGroupsStyle(action);
            }
            else if (action.SetGroup == "NogroupCross" || action.SetGroup == "Group")
            {
                this.ReportViewControl.GroupSchemaShowCtrl_GroupSchemaChangeEvent(false, action.Id);
                HandleGroup(action);
                this.ReportViewControl.SetOcxCurrentGroup(action.Text);
            }
            else if (action.SetGroup == "Cross")
            {
                this.ReportViewControl.GroupSchemaShowCtrl_GroupSchemaChangeEvent(true, action.Id);
                HandleGroup(action);
                this.ReportViewControl.SetOcxCurrentGroup(action.Text);
            }
            //列格式
            else if (action.Id.Equals(ContextMenuMetaAssigner.MenuKeyAlignLeft)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeyAlignCenter)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeyAlignRight)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeyFixCol)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeyUnFixCol)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeyHideCol)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeyUnHideCol)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeySumDec)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeySumAccumulate)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeySumAverage)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeySumMaximum)
                || action.Id.Equals(ContextMenuMetaAssigner.MenuKeySumMinimum)
                || action.Id.Equals("MenuKeySumLastValue")
                || action.Id.Equals("MenuKeySumWeightedAve")
                || action.Id.Equals("MenuKeyExpandCollapseRows")
                || action.Id.Equals("MenuKeySumWeightedAve")//汇总行使用表达式 = 加权平均（用于类似加权平均价的计算）
                || action.Id.Equals("MenuKeySumLastValue")//最后一行值=余额汇总方案 
                || action.Id.Equals("GroupStatStyle"))
            {
                string cmdId = action.Id;
                if (action.Id.Equals("MenuKeySumWeightedAve"))
                {
                    cmdId = ContextMenuMetaAssigner.MenuKeySumExp;
                }
                else if (action.Id.Equals("MenuKeySumLastValue"))
                {
                    cmdId = ContextMenuMetaAssigner.MenuKeySumBal;
                }
                else if (action.Id.Equals("GroupStatStyle"))
                {
                    cmdId = ContextMenuMetaAssigner.MenuKeySumNo;
                }
                //if (ReportViewControl.GetSelectedColumnsCount() <= 0)
                //{
                //    action.IsChecked = !action.IsChecked;
                //    return;
                //}
                this.ReportViewControl.HandlerColumn(cmdId, action.IsChecked);
                this.HandleColumnStyle(action);

            }
            else if (action.Id.Equals(ContextMenuMetaAssigner.MenuKeyFontColor))
            {
                this.ReportViewControl.ApplyFontColorStyle2Report();
            }
            else if (action.Id.Equals(ContextMenuMetaAssigner.MenuKeyExtendDataSource))
            {
                this.ReportViewControl.ExtendDataSource();
            }
            else if (action.SetGroup == "View")
            {
                this.ReportViewControl.ChangeView(action.Id);
                this.ReportViewControl.SetOcxCurrentView(action.Text);
            }
            else if (action.Id.Equals("ResetOriginalLayout"))
            {
                this.ReportViewControl.ResetOriginalLayout();
            }
            else if (action.Id.Equals("filterstyleexcel"))
            {
                this.ReportViewControl.FilterExcelStyle();
                HandleFilterstyle(action);
            }
            else if (action.Id.Equals("filterstylerow"))
            {
                this.ReportViewControl.FilterRowStyle();
                HandleFilterstyle(action);
            }
            else if (action.Id.Equals("filterstylenone"))
            {
                this.ReportViewControl.FilterNoneStyle(true);
                HandleFilterstyle(action);
            }
            else if (action.SetGroup == "JoinQuery")
                this.ReportViewControl.OnJointAction(action.Id);
            else if (action.SetGroup == "Extend")
            {
                this.ReportViewControl.OnSelfAction(action.Id); 
            }
            else if (action.Id == "collectToDesktop")
            {
                //this.ReportViewControl.CollectToDesktop();
            }
            else if (action.Id == "SaveAsView" || action.Id == "SaveAsReport")//保存视图
            {
                this.ReportViewControl.SaveAs(action.Id);
            }
            //else if (action.Id == "InclueBackColor")
            //{
            //    action.IsChecked = !action.IsChecked;
            //    //this.ReportViewControl.SetInclueBackColorState(action.IsChecked);
            //}
            //else if (action.Id == "QuickFilter")
            ////|| action.Id == "FixedFilter"
            ////|| action.Id == "RuntimeFilter")//快捷条件
            //{
            //    if (action.IsChecked)
            //    {
            //        this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.FilterInfo = FilterInfos.QuickFilter;
            //        //this.ReportViewControl.ShowFileterInfo(action.Id);
            //        this.ReportViewControl.TopBottomLayout();
            //    }
            //    else
            //    {
            //        this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.FilterInfo = FilterInfos.DesignTimeFilter;
            //        this.ReportViewControl.LeftRightLayout();
            //        //this.ReportViewControl.HideOCX();
            //    }
            //    HandleQuickQueryaction(action);
            //}
            else if (action.Catalog == "ColorFormat")
            {
                this.ReportViewControl.ColorFormat(action.Id);
            }
            else if (action.SetGroup == "solution")
            {
                this.ReportViewControl.ShowQueryNoUI(action.Id, action.Text);
            }
            else if (action.Id == "filterSolutionDeskTop")
            {
                this.ReportViewControl.PublishFilterSolution2Desktop();
            }
            else if (action.Id == "QueryParent")
            {
                this.ReportViewControl.ShowQuery();
            }

            #region 布局

            //显示固定条件
            else if (action.Id == "FixFilter")
            {
                //action.IsChecked = true;
                //this.ReportViewControl.ShowFixFilter();
                HandleLayoutStyle(action);
            }
            //显示快捷条件
            else if (action.Id == "QuickFilter")
            {
                //action.IsChecked = true;
                //this.ReportViewControl.ShowQuickFilter();
                HandleLayoutStyle(action);
            }
            //显示查询方案
            else if (action.Id == "OcxShowSchema")
            {
                //action.IsChecked = !action.IsChecked;
                //this.ReportViewControl.ShowQuerySchema(action.IsChecked);
                HandleLayoutStyle(action);

            }
            //显示查询条件
            else if (action.Id == "OcxShowFilter")
            {
                //action.IsChecked = !action.IsChecked;
                //this.ReportViewControl.ShowQueryFilter(action.IsChecked);
                HandleLayoutStyle(action);
            }
            //左右布局
            else if (action.Id == "LeftRightLayout")
            {
                //action.IsChecked = true;
                //this.ReportViewControl.LeftRightLayout();
                HandleLayoutStyle(action);
            }
            //上下布局
            else if (action.Id == "TopBottomLayout")
            {
                //action.IsChecked = true;
                //this.ReportViewControl.TopBottomLayout();
                HandleLayoutStyle(action);
            }
            #endregion
            else if (action.Id == "ChartDesign")
            {
                action.IsChecked = true;
                //this.ReportViewControl.TopBottomLayout();
                
                ReportViewControl.ChartDesign();
            }
            else if (action.Id == "ShowChart")
            {
                ReportViewControl.ShowChart(action.IsChecked);
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.BShowChart = action.IsChecked;
            }
            else if (action.Id == "ChartOutput")
            {
                ReportViewControl.OutputChart();
            }
            //    if( parentAction.Id=="View")
            //    {
            //        if (action.Id == "Plat" ||
            //           action.Id == "Collapse" ||
            //            action.Id == "MergeCell")
            //        {
            //            CheckThisID(action, parentAction);
            //        }
            //        else //changeview
            //        {
            //            this.ReportViewControl.ChangeView(action.Id);
            //        }
            //    }
            //    else if (parentAction.Id == "SelfDefine")
            //    {
            //        this.ReportViewControl.OnSelfAction(action.Id);
            //    }
            //    else if (action.Id.Equals("ResetOriginalLayout"))
            //        this.ReportViewControl.ResetOriginalLayout();

            //    else if (action.Id.Equals("filterstyleexcel"))
            //        this.ReportViewControl.FilterExcelStyle();
            //    else if (action.Id.Equals("filterstylerow"))
            //        this.ReportViewControl.FilterRowStyle();
            //    else if (action.Id.Equals("filterstylenone"))
            //        this.ReportViewControl.FilterNoneStyle();
            //    else if(parentAction.Id=="ReportJoint"||parentAction.Id=="VoucherJoint")
            //      this.ReportViewControl.OnJointAction(action.Id);
            //}
        }

        private void HandleAutoMerge()
        {
            IAction merge = UserToolbarStateManager.FindActionByKey(_reportId, "AutoMergeAllCell");
            if (merge != null)
            {
                this.ReportViewControl.AutoMergeAllCells(merge.IsChecked);
            }
        }
        private void SetPrintControl(bool bIncludeBackColor)
        {
            if (this.ReportViewControl.Report.PrintOption.CanSelectProvider)
            {
                this.ReportViewControl.Report.PrintOption.PrintProvider = (bIncludeBackColor == true ? PrintProvider.UAPReportPrintComponent : PrintProvider.U8PrintComponent);
            }
        }

        private void CheckThisID(IAction action, NetAction parentAction)
        {
            action.IsChecked = true;
            ArrayList pas = parentAction.Actions;
            for (int i = 0; i < pas.Count; i++)
            {
                NetAction na = pas[i] as NetAction;
                if (na.Id != action.Id &&
                   (na.Id == "MergeCell" ||
                   na.Id == "Collapse" ||
                   na.Id == "Plat"))
                    na.IsChecked = false;
            }
            this.ReportViewControl.ChangeViewStyle(action.Id);
        }

        public void SelectionChanged(IAction action, ISelection selection)
        {

        }

        /// <summary>
        /// 处理过滤的互斥
        /// </summary>
        /// <param name="action"></param>
        private void HandleFilterstyle(IAction action)
        {
            AbstractFormInput.ActionHashtable actions = UserToolbarStateManager.ReportActionsState[this.ReportViewControl.ClientReportContext.GetReportId()];
            NetAction filterstyle = actions["filterstyle"] as NetAction;
            foreach (NetAction ac in filterstyle.Actions)
            {
                if (action.Id == ac.Id)
                {
                    ac.IsChecked = true;
                    continue;
                }
                ac.IsChecked = false;
            }
        }

        /// <summary>
        /// 处理列的互斥
        /// </summary>
        /// <param name="action"></param>
        private void HandleColumnStyle(IAction action)
        {
            AbstractFormInput.ActionHashtable actions = UserToolbarStateManager.ReportActionsState[this.ReportViewControl.ClientReportContext.GetReportId()];
            NetAction columnStyle = actions["ColumnStyle"] as NetAction;
            if (action.Id == ContextMenuMetaAssigner.MenuKeyAlignLeft ||
               action.Id == ContextMenuMetaAssigner.MenuKeyAlignCenter ||
               action.Id == ContextMenuMetaAssigner.MenuKeyAlignRight)
            {
                foreach (NetAction ac in columnStyle.Actions)
                {
                    if (ac.Id == ContextMenuMetaAssigner.MenuKeyAlignLeft ||
                        ac.Id == ContextMenuMetaAssigner.MenuKeyAlignCenter ||
                        ac.Id == ContextMenuMetaAssigner.MenuKeyAlignRight)
                    {
                        ac.IsChecked = false;
                    }
                    if (action.Id == ac.Id)
                    {
                        action.IsChecked = true;
                    }
                }
            }
            if (action.Id == ContextMenuMetaAssigner.MenuKeyFixCol ||
               action.Id == ContextMenuMetaAssigner.MenuKeyUnFixCol)
            {
                foreach (NetAction ac in columnStyle.Actions)
                {
                    if (ac.Id == ContextMenuMetaAssigner.MenuKeyFixCol ||
                        ac.Id == ContextMenuMetaAssigner.MenuKeyUnFixCol)
                    {
                        ac.IsChecked = false;
                    }
                    if (action.Id == ac.Id)
                    {
                        action.IsChecked = true;
                    }
                }
            }
        }


        /// <summary>
        /// 处理分组的互斥
        /// </summary>
        /// <param name="action"></param>
        private void HandleGroup(IAction action)
        {
            StateButtons buttons = UserToolbarStateManager.ReportButtonState[this.ReportViewControl.ClientReportContext.GetReportId()];
            StateButton groupAndCross = buttons["GroupAndCross"];
            foreach (StateButton bt in groupAndCross.Buttons)
            {
                if (bt.Key == action.Id)
                {
                    bt.IsChecked = true;
                }
                else
                {
                    bt.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// 处理分组的样式的互斥
        /// </summary>
        /// <param name="action"></param>
        private void HangleGroupsStyle(IAction action)
        {
            StateButtons buttons = UserToolbarStateManager.ReportButtonState[this.ReportViewControl.ClientReportContext.GetReportId()];
            StateButton groupAndCross = buttons["GroupAndCross"];
            foreach (StateButton bt in groupAndCross.Buttons)
            {
                if (action.Id == bt.Key)
                {
                    bt.IsChecked = true;
                    continue;
                }
                else if (bt.Key == "PlanStyle" ||
                         bt.Key == "Collapse")
                {
                    bt.IsChecked = false;
                }
            }
        }
        /// <summary>
        /// 处理布局按钮的样式
        /// </summary>
        /// <param name="action"></param>
        private void HandleLayoutStyle(IAction action)
        {
            try
            {
                StateButtons buttons = UserToolbarStateManager.ReportButtonState[this.ReportViewControl.ClientReportContext.GetReportId()];
                StateButton layouts = buttons["Layouts"];
                //foreach (StateButton bt in groupAndCross.Buttons)
                //{

                //StateButton Layouts = new StateButton("Layouts", "布局", true, "布局", Properties.Resources.filterFormat, "Layouts");
                //StateButton FixFilter = new StateButton("FixFilter", "显示固定条件", true, "显示固定条件", null, "Layouts");
                //StateButton QuickFilter = new StateButton("QuickFilter", "显示快捷条件", true, "显示固定条件", null, "Layouts");
                //StateButton line1 = new StateButton("line1", "-", true, "", null, "Layouts");
                //StateButton OcxShowSchema = new StateButton("OcxShowSchema", "显示查询方案", true, "显示查询方案", null, "Layouts");
                //StateButton OcxShowFilter = new StateButton("OcxShowFilter", "显示查询条件", true, "显示查询条件", null, "Layouts");
                //StateButton line2 = new StateButton("line2", "-", true, "", null, "Layouts");
                //StateButton LeftRightLayout = new StateButton("LeftRightLayout", "左右布局", true, "左右布局", null, "Layouts");
                //StateButton TopBottomLayout = new StateButton("TopBottomLayout", "上下布局", true, "上下布局", null, "Layouts");
                string reportId = this.ReportViewControl.ClientReportContext.GetReportId();
                AbstractFormInput.ActionHashtable actions = UserToolbarStateManager.ReportActionsState[reportId];
               
                NetAction layoutsbuttons = actions["Layouts"] as NetAction;
                IAction btnFixFilter = UserToolbarStateManager.FindActionByKey(reportId, "FixFilter");
                IAction btnQuickFilter = UserToolbarStateManager.FindActionByKey(reportId, "QuickFilter");
                IAction btnOcxShowSchema = UserToolbarStateManager.FindActionByKey(reportId, "OcxShowSchema");
                IAction btnOcxShowFilter = UserToolbarStateManager.FindActionByKey(reportId, "OcxShowFilter");
                IAction btnLeftRightLayout = UserToolbarStateManager.FindActionByKey(reportId, "LeftRightLayout");
                IAction btnTopBottomLayout = UserToolbarStateManager.FindActionByKey(reportId, "TopBottomLayout");

                //显示固定条件
                if (action.Id == "FixFilter")
                {
                    btnFixFilter.IsChecked = true;
                    btnFixFilter.IsEnabled = true;
                    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.FilterFlag = FilterType.FixFilter;

                    btnQuickFilter.IsChecked = false;
                    btnQuickFilter.IsEnabled = true;

                    btnOcxShowSchema.IsChecked = false;
                    btnOcxShowSchema.IsEnabled = false;
                    //this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.SolutionVisible = false;

                    btnOcxShowFilter.IsChecked = false;
                    btnOcxShowFilter.IsEnabled = false;
                    //this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.ConditionVisible = false;

                    btnLeftRightLayout.IsChecked = false;
                    btnLeftRightLayout.IsEnabled = false;
                    //this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.QuickFilterLayoutFlag = QuickFilterLayout.LeftRight;

                    btnTopBottomLayout.IsChecked = false;
                    btnTopBottomLayout.IsEnabled = false;
                }
                else if (action.Id == "QuickFilter")
                {
                    btnQuickFilter.IsChecked = true;
                    btnQuickFilter.IsEnabled = true;
                    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.FilterFlag = FilterType.QuickFilter;

                    btnFixFilter.IsChecked = false;
                    btnFixFilter.IsEnabled = true;

                    btnQuickFilter.IsEnabled = true;
                    btnOcxShowSchema.IsEnabled = true;
                    //默认应该显示一个，现在默认显示solution
                    //btnOcxShowSchema.IsChecked = this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.SolutionVisible;
                    btnOcxShowSchema.IsChecked = true;

                    btnOcxShowFilter.IsEnabled = true;

                    btnOcxShowFilter.IsChecked = this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.ConditionVisible;
                    btnLeftRightLayout.IsEnabled = true;
                    btnTopBottomLayout.IsEnabled = true;

                    if (this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.QuickFilterLayoutFlag == QuickFilterLayout.LeftRight)
                    {
                        btnLeftRightLayout.IsChecked = true;
                    }
                    else
                    {
                        btnTopBottomLayout.IsChecked = true;
                    }
                        if (this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.ConditionVisible == false &&
                            this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.SolutionVisible == false)
                            this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.SolutionVisible = true;
                }
                else if (action.Id == "OcxShowSchema")
                {
                    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.SolutionVisible = action.IsChecked;
                }
                else if (action.Id == "OcxShowFilter")
                {
                    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.ConditionVisible = action.IsChecked;
                }
                else if (action.Id == "LeftRightLayout")
                {
                    btnLeftRightLayout.IsChecked = true;
                    btnTopBottomLayout.IsChecked = false;
                    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.QuickFilterLayoutFlag = QuickFilterLayout.LeftRight;
                    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.LayoutIsSetted = true;
                }
                else if (action.Id == "TopBottomLayout")
                {
                    btnTopBottomLayout.IsChecked = true;
                    btnLeftRightLayout.IsChecked = false;
                    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.QuickFilterLayoutFlag = QuickFilterLayout.TopBottom;
                    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.LayoutIsSetted = true;
                }

                this.ReportViewControl.HandlerLayout(action.Id, action.IsChecked);
            }
            catch(Exception ex)
            {

                Logger logger = Logger.GetLogger("ReportFacade");
                logger.Error(ex);
            }

        }
        /// <summary>
        /// 处理视图的互斥
        /// </summary>
        /// <param name="action"></param>
        private void HandleView(IAction action)
        {
            AbstractFormInput.ActionHashtable actions = UserToolbarStateManager.ReportActionsState[this.ReportViewControl.ClientReportContext.GetReportId()];
            NetAction columnStyle = actions["View"] as NetAction;
            foreach (NetAction ac in columnStyle.Actions)
            {
                if (action.Id == ac.Id)
                {
                    action.IsChecked = true;
                    continue;
                }
                else if (ac.SetGroup == "View")
                {
                    ac.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// 处理打印情况
        /// </summary>
        /// <param name="action"></param>
        private void HandlePrint(IAction action)
        {
            string reportId = this.ReportViewControl.ClientReportContext.GetReportId();
            AbstractFormInput.ActionHashtable actions = UserToolbarStateManager.ReportActionsState[reportId];
            string id = action.Id.Substring(7);
            IAction bSetAction = UserToolbarStateManager.FindActionByKey(reportId, id);
            string defaultText = "(" + String4Report.GetString("U8.UAP.Report.默认") + ")";

            NetAction columnStyle = actions["PrintParent"] as NetAction;
            foreach (NetAction ac in columnStyle.Actions)
            {
                if (ac.Text.Contains(defaultText))
                {
                    ac.Text = ac.Text.Replace(defaultText, string.Empty);
                }
            }
            if (bSetAction != null)
            {
                bSetAction.Text = bSetAction.Text + defaultText;
            }
            IAction setPrintDefault = UserToolbarStateManager.FindActionByKey(reportId, "SetPrintDefault");
            if (setPrintDefault != null)
            {
                foreach (NetAction ac in ((NetAction)setPrintDefault).Actions)
                {
                    if (ac.Id != action.Id)
                    {
                        ac.IsChecked = false;
                    }
                    else ac.IsChecked = true;
                }
            }
            SetPrintUserUIState(action);
        }

        /// <summary>
        /// 设置用户状态
        /// </summary>
        /// <param name="action"></param>
        private void SetPrintUserUIState(IAction action)
        {
            //if (action.Id == "InclueBackColor")
            //{
            //    this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.IncludeBackColor = action.IsChecked;
            //}
            if (action.Id == "InclueDesignFilter")
            {
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.PrintSet = PrintSetings.PrintDesignTimeFilter;
            }
            else if (action.Id == "IncludQueryFilter")
            {
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.PrintSet = PrintSetings.PrintRuntimeFilter;
            }
            else if (action.Id == "DefaultPrint")
            {
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.PrintDefault = PrintDefaults.Print;
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.IncludeBackColor = false;
            }
            if (action.Id == "DefaultPrintView")
            {
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.PrintDefault = PrintDefaults.priview;
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.IncludeBackColor = false;
            }
            else if (action.Id == "DefaultPrintInclueBackColor")
            {
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.PrintDefault = PrintDefaults.Print;
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.IncludeBackColor = true;
            }
            if (action.Id == "DefaultPrintViewInclueBackColor")
            {
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.PrintDefault = PrintDefaults.priview;
                this.ReportViewControl.ClientReportContext.ReportUIUserStateInfo.IncludeBackColor = true;
            }
        }

        /// <summary>
        /// 处理条件信息
        /// </summary>
        /// <param name="action"></param>
        private void HandleQuickQueryaction(IAction action)
        {
            StateButton button = UserToolbarStateManager.FindStateButtonByKey(this._reportId, action.Id);
            if (button != null)
                button.IsChecked = action.IsChecked;
        }

        private void HandleTopCheckBoxButton(IAction action)
        {
            StateButtons buttons = UserToolbarStateManager.ReportButtonState[this.ReportViewControl.ClientReportContext.GetReportId()];
            buttons[action.Id].IsChecked = action.IsChecked;
        }

        private void HandleMorsetButton(IAction action)
        {
            string reportId = this.ReportViewControl.ClientReportContext.GetReportId();
            UserToolbarStateManager.SetStateButtonCheckState(reportId, action.Id, action.IsChecked);
        }
    }
}
