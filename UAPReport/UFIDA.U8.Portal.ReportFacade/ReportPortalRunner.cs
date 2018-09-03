using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Text.RegularExpressions;

using UFIDA.U8.Portal.Framework.Actions;
using UFIDA.U8.Portal.Framework.MainFrames;

using UFIDA.U8.Portal.Proxy.editors;
using UFIDA.U8.Portal.Proxy.supports;
using UFIDA.U8.Portal.Proxy.Actions;



using UFIDA.U8.UAP.Services.ReportExhibition;
using UFIDA.U8.UAP.Services.ReportElements;
using UFIDA.U8.UAP.Services.ReportData;

using UFSoft.U8.Framework.Login.UI;


namespace UFIDA.U8.Portal.ReportFacade
{
    public class ReportPortalRunner : AbstractPortalRunner
    {
        private string _staticReportId;
        private string _staticReportName;
        private string _specialname;
        private ReportFormInput ReportFormInput = null;
        private ReportViewControl ReportViewControl = null;
        private IEditorPart ReportEditorPart = null;
        private bool IsOpenReport = false;

        public ReportPortalRunner(NetFormInput input)
            : base(input)
        {
            Trace.WriteLine("ReportPortalRunner::ReportPortalRunner()");
        }

        public override Control CreatePartControl(IEditorPart part, IEditorInput input)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                this.ReportEditorPart = part;
                this.ReportFormInput = input as ReportFormInput;
                sb.Append("CreatePartControl end 1");

                this.ParseStaticReportInfo();
                string reportId = this.GetObjectId();
                sb.Append("CreatePartControl end 2");

                object login = this.ReportFormInput.GetU8Login();
                object rawFilter = null;
                bool isShowUI = false;
                bool isStaticReport = this.ReportFormInput.IsStaticReport();
                int viewType = 1;
                sb.Append("CreatePartControl end 3");
                if (!isStaticReport)
                {
                    rawFilter = this.ReportFormInput.GetRawFilter();
                    isShowUI = this.ReportFormInput.IsShowUI();
                }
                else
                    viewType = this.ReportFormInput.GetViewType();
                sb.Append("CreatePartControl end 4");
                ClientReportContext context = new ClientReportContext(login);
                RemoteDataHelper rdh = new RemoteDataHelper();
                sb.Append("CreatePartControl end 5");
                context.ReportUIUserStateInfo = rdh.GetReportUserState(ClientReportContext.Login,reportId);
                sb.Append("CreatePartControl end 6");
                ClientReportContext.Port = Cryptography.LoginInfo.ProtocolPort["RePt"].ToString();
                ClientReportContext.Protocal = Cryptography.LoginInfo.ProtocolPort["RePr"].ToString();
                sb.Append("CreatePartControl end 7");
                DefaultConfigs.RegChannel(ClientReportContext.Login.AppServer);
                sb.Append("CreatePartControl end 8");

                this.ReportViewControl = new ReportViewControl();
                this.ReportViewControl.CanShowReport += new EventHandler(ReportViewControlShowReport);
                this.ReportViewControl.DoubleClick += new EventHandler(ReportViewControlDoubleClick);
                this.ReportViewControl.CloseContainerEvent += new CloseContainerHandler(ReportViewControl_ReportFilterEvent);
                this.ReportViewControl.UpdataGroupandCrossesToolbarEvent += new RefreshToolbarHandler(ReportViewControl_pdataGroupsToolbarEvent);
                this.ReportViewControl.RefreshViewsToolbarEvent+=new RefreshToolbarHandler(ReportViewControl_RefreshViewsToolbarEvent);
                this.ReportViewControl.RefreshJoinQueryToolbarEvent+=new RefreshToolbarHandler(ReportViewControl_RefreshJoinQueryToolbarEvent);
                //
                // pengzhzh 2012-8-9 
                //
                this.ReportViewControl.ModifyButtons += new System.Action(ButtonChangeModify);

                (this.ReportEditorPart as NetEditor).PageCloseRequest += new PageCloseRequestHandler(ReportPortalRunner_PageCloseRequest);
                sb.Append("CreatePartControl end 9");
                if (isStaticReport)
                    this.ReportViewControl.Initialize(context, reportId, ReportStates.Static, (ReportType)viewType);
                else
                    this.ReportViewControl.Initialize(context, reportId, ReportStates.Browse, rawFilter, isShowUI);

                this.ReportFormInput.Text = "Loading";
                sb.Append("CreatePartControl end 10");
                (this.ReportEditorPart as NetEditor).Refresh(this.ReportFormInput);
                
                return this.ReportViewControl;
            }
            catch (CanceledException)
            {
                CloseContainer();
                return null;
            }
            catch (Exception ex)
            {
                UFIDA.U8.UAP.Services.ReportExhibition.ReportViewControl.ShowInformationMessageBox(ex);

                UFIDA.U8.UAP.Services.ReportExhibition.ReportViewControl.ShowInformationMessageBox(ex.InnerException);
                System.Diagnostics.Trace.WriteLine("ReportPortalRunner.CreatePartControl Log:" + sb);
                CloseContainer();
                return null;
            }
        }

        /// <summary>
        /// 解析经过包装的静态报表Id，以获取正确的静态报表id和名称
        /// 报表的规则请参见
        /// UFIDA.U8.UAP.Services.ReportData.StaticReport.GetWrappedStaticReportId()
        /// </summary>
        private void ParseStaticReportInfo()
        {
            this._staticReportId = null;
            this._staticReportName = null;
            string id = this.ReportFormInput.GetReportId();
            char compartChar = StaticReport.WrappedIdCompartChar;
            if (id.Contains(compartChar.ToString()))
            {
                string[] infos = id.Split(compartChar);
                this._staticReportId = infos[0];
                this._staticReportName = infos[1];
            }
        }
        private void  ReportViewControl_pdataGroupsToolbarEvent()
        {
            AbstractFormInput.ActionHashtable actions = UserToolbarStateManager.ReportActionsState[this.ReportViewControl.ClientReportContext.GetReportId()];
            NetAction acGroups = actions["GroupAndCross"] as NetAction;
            acGroups.Actions.Clear();
            StateButtons buttons = UserToolbarStateManager.ReportButtonState[this.ReportViewControl.ClientReportContext.GetReportId()];
            StateButton stGroups = buttons["GroupAndCross"];
            IActionDelegate actionDelegate = new ReportActionDelegate(this.ReportViewControl);
            foreach (StateButton subButton in stGroups.Buttons)
            {
                if (subButton.Key == string.Empty)
                    return;
                NetAction subAction = new NetAction(subButton.Key, subButton.Key == "ViewStyle" ? null : actionDelegate);
                subAction.Text = subButton.Caption;
                subAction.IsEnabled = subButton.IsEnable;
                subAction.IsVisible = subButton.Visible;
                subAction.ToolTipText = subButton.ToolTip;
                subAction.Image = subButton.Image;
                subAction.SetGroup = subButton.SetGroup;
                subAction.Catalog = subButton.Category;
                subAction.Style = subButton.ButtonStyle;
                subAction.IsChecked = subButton.IsChecked;
                subAction.Tag = stGroups;
                subButton.VisibleChanged -= new VisibleChangedHandler(StateButtonVisibleChanged);
                subButton.VisibleChanged += new VisibleChangedHandler(StateButtonVisibleChanged);
                acGroups.Actions.Add(subAction);

            }
        }
        private void ReportViewControl_RefreshJoinQueryToolbarEvent()
        {
            AbstractFormInput.ActionHashtable actions = UserToolbarStateManager.ReportActionsState[this.ReportViewControl.ClientReportContext.GetReportId()];
            NetAction joinQueryAcs = actions["JoinQuery"] as NetAction;
            joinQueryAcs.Actions.Clear();
            StateButtons buttons = UserToolbarStateManager.ReportButtonState[this.ReportViewControl.ClientReportContext.GetReportId()];
            StateButton joinQuerySbs = buttons["JoinQuery"];
            IActionDelegate actionDelegate = new ReportActionDelegate(this.ReportViewControl);
            foreach (StateButton subButton in joinQuerySbs.Buttons)
            {
                if (subButton.Key == string.Empty)
                    return;
                NetAction subAction = new NetAction(subButton.Key, subButton.Key == "ViewStyle" ? null : actionDelegate);
                subAction.Text = subButton.Caption;
                subAction.IsEnabled = subButton.IsEnable;
                subAction.IsVisible = subButton.Visible;
                subAction.ToolTipText = subButton.ToolTip;
                subAction.Image = subButton.Image;
                subAction.SetGroup = subButton.SetGroup;
                subAction.Catalog = subButton.Category;
                subAction.Style = subButton.ButtonStyle;
                subAction.IsChecked = subButton.IsChecked;
                subAction.Tag = subButton;
                subButton.VisibleChanged -= new VisibleChangedHandler(StateButtonVisibleChanged);
                subButton.VisibleChanged += new VisibleChangedHandler(StateButtonVisibleChanged);
                joinQueryAcs.Actions.Add(subAction);

            }
        }
        private void ReportViewControl_RefreshViewsToolbarEvent()
        {
            AbstractFormInput.ActionHashtable actions = UserToolbarStateManager.ReportActionsState[this.ReportViewControl.ClientReportContext.GetReportId()];
            NetAction viewAcs = actions["View"] as NetAction;
            viewAcs.Actions.Clear();
            StateButtons buttons = UserToolbarStateManager.ReportButtonState[this.ReportViewControl.ClientReportContext.GetReportId()];
            StateButton viewSbs = buttons["View"];
            IActionDelegate actionDelegate = new ReportActionDelegate(this.ReportViewControl);
            foreach (StateButton subButton in viewSbs.Buttons)
            {
                if (subButton.Key == string.Empty)
                    return;
                NetAction subAction = new NetAction(subButton.Key, subButton.Key == "ViewStyle" ? null : actionDelegate);
                subAction.Text = subButton.Caption;
                subAction.IsEnabled = subButton.IsEnable;
                subAction.IsVisible = subButton.Visible;
                subAction.ToolTipText = subButton.ToolTip;
                subAction.Image = subButton.Image;
                subAction.SetGroup = subButton.SetGroup;
                subAction.Catalog = subButton.Category;
                subAction.Style = subButton.ButtonStyle;
                subAction.IsChecked = subButton.IsChecked;
                subAction.Tag = subButton;
                subButton.VisibleChanged -= new VisibleChangedHandler(StateButtonVisibleChanged);
                subButton.VisibleChanged += new VisibleChangedHandler(StateButtonVisibleChanged);
                viewAcs.Actions.Add(subAction);

            }
        }

        private string GetObjectId()
        {
            if (string.IsNullOrEmpty(this._staticReportId))
            {
                string id = this.ReportFormInput.GetReportId();
                if (id.Contains("_@!@_"))
                {
                    try
                    {
                        string[] ids = Regex.Split(id, "_@!@_");
                        _specialname = ids[1];
                        return ids[2];
                    }
                    catch
                    {
                    }
                }
                return id;
            }
            return this._staticReportId;
        }

        private string GetObjectName()
        {
            if (!string.IsNullOrEmpty(_specialname))
                return _specialname;
            else if (!string.IsNullOrEmpty(this._staticReportName))
                return this._staticReportName;
            return this.ReportViewControl.Report.Name;
        }

        private void CloseContainer()
        {
            (this.ReportEditorPart as NetEditor).CloseContainer(this.ReportFormInput);
            //if (this.ReportViewControl != null)
            //{
            //    this.ReportViewControl.Release();
            //    this.ReportViewControl = null;
            //}
        }

        private void ReportPortalRunner_PageCloseRequest(IEditorInput input)
        {
            if (this.ReportFormInput == input)
            {
                if (this.ReportViewControl != null)
                {
                    (this.ReportEditorPart as NetEditor).PageCloseRequest -= new PageCloseRequestHandler(ReportPortalRunner_PageCloseRequest);
                    if (this.ReportViewControl != null)
                    {
                        //保存用户页面布局
                        this.ReportViewControl.SaveSaveReportUserState();
                        this.ReportViewControl.Release();
                        this.ReportViewControl.Dispose();
                        this.ReportViewControl = null;
                        
                    }
                }
            }
        }

        void ReportViewControl_ReportFilterEvent()
        {
            CloseContainer();
        }

        private void ReportViewControlShowReport(object sender, EventArgs e)
        {
            ReportViewControl rvc = sender as ReportViewControl;

            if (rvc != null && rvc.StateManager != null)
            {
                //this.ReportFormInput.FormCode = rvc.Report.ViewID;//add by yanghx 
                rvc.ReloadViewRefButtions();
                rvc.StateManager.SetButtonsState();//.SetPageButtonsVisible();
            }
            
            this.CreateToolBar(rvc == null ? null : rvc.StateManager);
            
            //必须在创建完toolbar后才能创建表头，否则会出现重叠现象
            if (rvc != null && this.ReportViewControl != null)
            {
                this.ReportViewControl.ShowHeader();
            }
        }

        //
        // pengzhzh 2012-8-9 刷新所有按钮
        //
        void ButtonChangeModify()
        {
            try
            {
                int count = this.ReportFormInput.Actions.Count;
                if (count > 0)
                {
                    this.ReportFormInput.FireActionsChange(ButtonChangeType.Modify);
                }
            }
            catch { }
        }


        private void CreateToolBar(UIStateManager manager)
        {
            if (manager == null || this.ReportViewControl == null || this.ReportViewControl.Report == null)
            {
                int count = this.ReportFormInput.Actions.Count;
                for (int i = 0; i < count; i++)
                {
                    NetAction action = this.ReportFormInput.Actions[i] as NetAction;
                    //action.Style = (int)Proxy.UFPortalProxy.ToolButtonSyle.DropDown;
                    //if(action.Id!="Help")
                    //    action.IsVisible = false;
                    action.IsEnabled = false;
                }
                if (count > 0)
                    this.ReportFormInput.FireActionsChange(ButtonChangeType.Modify);
                return;
            }
            this.ReportFormInput.Actions.Clear();
            
            IActionDelegate actionDelegate = new ReportActionDelegate(this.ReportViewControl);
            foreach (StateButton sb in manager.CurrentState.Buttons)
            {
                NetAction action = new NetAction(sb.Key, actionDelegate);
                action.Text = sb.Caption;
                action.IsEnabled = sb.IsEnable;
                action.ToolTipText = sb.ToolTip;
                action.Image = sb.Image;
                action.Tag = null;
                action.Catalog = sb.Category;
                action.RowSpan = sb.RowSpan;
                action.ActionType = (sb.Category == "编辑" ? NetAction.NetActionType.Edit : NetAction.NetActionType.Normal);
                action.Style = sb.ButtonStyle;
                action.SetGroup = sb.SetGroup;
                action.IsChecked = sb.IsChecked;
                action.IsVisible = sb.Visible;
                sb.Tag = action;
                sb.VisibleChanged -= new VisibleChangedHandler(StateButtonVisibleChanged);
                sb.VisibleChanged += new VisibleChangedHandler(StateButtonVisibleChanged);

                AddSubButtons(sb, actionDelegate, action);
                this.ReportFormInput.Actions.Add(sb.Key, action);
                if (!UserToolbarStateManager.ReportActionsState.ContainsKey(this.ReportViewControl.ClientReportContext.GetReportId()))
                {
                    UserToolbarStateManager.ReportActionsState.Add(this.ReportViewControl.ClientReportContext.GetReportId(), this.ReportFormInput.Actions);
                }
                else
                {
                    UserToolbarStateManager.ReportActionsState[this.ReportViewControl.ClientReportContext.GetReportId()] = this.ReportFormInput.Actions;
                }
            }
            this.ReportFormInput.FormInputExtend.IsSmallButton = manager.CurrentState.Buttons.IsSmallButton;
            this.ReportFormInput.FireActionsChange(ButtonChangeType.Add);
            this.ReportFormInput.Text = this.GetObjectName();
            (this.ReportEditorPart as NetEditor).Refresh(this.ReportFormInput);

            this.ReportViewControl.Focus();
        }

        private void AddSubButtons(StateButton sb, IActionDelegate actionDelegate, NetAction action)
        {
            foreach (StateButton subButton in sb.Buttons)
            {
                NetAction subaction = AddASubButton(subButton, actionDelegate, action);
                AddSubButtons(subButton, actionDelegate, subaction);
            }
        }

        private NetAction AddASubButton(StateButton subButton, IActionDelegate actionDelegate, NetAction action)
        {
            if (subButton.Key == string.Empty)
                return null;

            NetAction subAction = new NetAction(subButton.Key, subButton.Key == "ViewStyle" ? null : actionDelegate);
            subAction.Text = subButton.Caption;
            subAction.IsEnabled = subButton.IsEnable;
            subAction.IsVisible = subButton.Visible;
            subAction.ToolTipText = subButton.ToolTip;
            subAction.Image = subButton.Image;
            subAction.SetGroup = subButton.SetGroup;
            subAction.Catalog = subButton.Category;
            subAction.Style = subButton.ButtonStyle;
            subAction.IsChecked = subButton.IsChecked;
            if (subAction.Id == this.ReportViewControl.CurrentID)
                subAction.IsChecked = true;
            subAction.Tag = action;


            subButton.VisibleChanged -= new VisibleChangedHandler(StateButtonVisibleChanged);
            subButton.VisibleChanged += new VisibleChangedHandler(StateButtonVisibleChanged);

            action.Actions.Add(subAction);
            return subAction;
        }

        private void StateButtonVisibleChanged(StateButton button)
        {
            if (this.ReportFormInput.Actions[button.Key] != null)
            {
                this.ReportFormInput.Actions[button.Key].IsEnabled = button.Visible;
                this.ReportFormInput.FireActionsChange(ButtonChangeType.Modify);
            }
        }

        private void ReportViewControlDoubleClick(object sender, EventArgs e)
        {
            SelfAction dbc = this.ReportViewControl.Report.SelfActions.DoubleClickAction;
            if (dbc != null)
                this.ReportViewControl.OnSelfAction(dbc.Caption);
        }
    }

    public class ReportPortalPreLoadRunner
    {
        public ReportPortalPreLoadRunner()
        {

        }

        public void PreLoadReport(object login)
        {
            System.Diagnostics.Trace.WriteLine("Report preload begin");
            ParameterizedThreadStart ts = new ParameterizedThreadStart(InnerPreLoad);
            Thread t = new Thread(ts);
            t.ApartmentState = ApartmentState.STA;
            t.Start(login);
        }

        private void InnerPreLoad(object login)
        {
            try
            {
                ClientReportContext context = new ClientReportContext(login);
                ClientReportContext.Port = Cryptography.LoginInfo.ProtocolPort["RePt"].ToString();
                ClientReportContext.Protocal = Cryptography.LoginInfo.ProtocolPort["RePr"].ToString();
                DefaultConfigs.RegChannel(ClientReportContext.Login.AppServer);
                ReportEngine re = DefaultConfigs.GetRemoteEngine(ClientReportContext.Login, ReportStates.Browse);
                re.PreLoad();

                new ReportViewControl();
                new GridReportControl();

                AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinGrid.v10.2");
                AppDomain.CurrentDomain.Load("Infragistics2.Shared.v10.2");
                AppDomain.CurrentDomain.Load("Infragistics2.Win.v10.2");
                AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinDock.v10.2");
                AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinDataSource.v10.2");
                AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinChart.v10.2");
                AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinToolbars.v10.2");
                AppDomain.CurrentDomain.Load("Infragistics2.Win.SupportDialogs.v10.2");

                //AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinGrid.v6.3");
                //AppDomain.CurrentDomain.Load("Infragistics2.Shared.v6.3");
                //AppDomain.CurrentDomain.Load("Infragistics2.Win.v6.3");
                //AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinDock.v6.3");
                //AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinDataSource.v6.3");
                //AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinChart.v6.3");

                //AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinGrid.v6.1");
                //AppDomain.CurrentDomain.Load("Infragistics2.Shared.v6.1");
                //AppDomain.CurrentDomain.Load("Infragistics2.Win.v6.1");
                //AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinDataSource.v6.1");
                //AppDomain.CurrentDomain.Load("Infragistics2.Win.UltraWinToolbars.v6.1");

                AppDomain.CurrentDomain.Load("UFIDA.U8.UAP.UI.Runtime.Common");
                AppDomain.CurrentDomain.Load("UFIDA.U8.UAP.UI.Runtime.View");
                AppDomain.CurrentDomain.Load("UFIDA.U8.UAP.UI.Runtime.Controller");
                AppDomain.CurrentDomain.Load("UFIDA.U8.UAP.UI.Runtime.Model");
                AppDomain.CurrentDomain.Load("UFIDA.U8.UAP.UI.Runtime.List");
                AppDomain.CurrentDomain.Load("UFIDA.U8.UAP.UI.Runtime.BusinessObject");

                System.Diagnostics.Trace.WriteLine("Report preload end");
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }
        }
    }
}
