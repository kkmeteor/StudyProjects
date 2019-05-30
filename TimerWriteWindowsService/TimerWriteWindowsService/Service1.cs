using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimerWriteWindowsService
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer= new Timer( )
        }

        protected override void OnStop()
        {
            this.timer = null;
        }
    }
}
