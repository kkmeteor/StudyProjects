using System;
using System.Collections.Generic;
using System.Text;
using UFIDA.U8.Portal.Framework.MainFrames;
using UFIDA.U8.UAP.Services.ReportElements;
using UFSoft.U8.Framework.Login.UI;
using UFIDA.U8.Portal.EventBroker;
using System.Windows.Forms;

namespace UFIDA.U8.Portal.ReportFacade
{
    public class PortalViewPart:ViewPart
    {
        private PortalViewControl _pvc;
        public PortalViewPart()
            : base()
        {
            this.SubscribingList.Add("ViewPartCompleted");
        }

        public override System.Windows.Forms.Control CreatePartControl()
        {
            _pvc = new PortalViewControl();
            EventInspector.RegisterScenario(_pvc, Scenario);

            return _pvc;           
        }
        
        public override void ReleaseInnerControl()
        {
            base.ReleaseInnerControl();
            //
            if (_pvc != null)
            {
                _pvc.Release();
                _pvc = null;
            }
        }

        public override bool ReceiveMessage(UFIDA.U8.Portal.Common.Communication.IMessage message)
        {
            //if (message.Type.Equals("ViewPartCompleted"))
            //{
            //    _pvc.BeginInvoke(new OpenViewHandler(InitView));
            //}
            return true;
        }

        private void InitView()
        {
            //ClientReportContext context = new ClientReportContext(this.Site.Advisor.Login.InteropLoginObject);//login
            //ClientReportContext.Port = Cryptography.LoginInfo.ProtocolPort["RePt"].ToString();
            //ClientReportContext.Protocal = Cryptography.LoginInfo.ProtocolPort["RePr"].ToString();
            //DefaultConfigs.RegChannel(ClientReportContext.Login.AppServer);
            //_pvc.OpenView(context, this.InstanceID); 
        }
        
    }

    public delegate void OpenViewHandler(); 
    public delegate void DefaultSettingHandler();
}
