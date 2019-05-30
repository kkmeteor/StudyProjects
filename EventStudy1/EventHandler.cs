using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStudy1
{
    public class EventHandler
    {
        private Boiler b;
        public delegate void BoilerLogHandler(string status);
        public event BoilerLogHandler BoilerEventLog;
        public void LogProcess()
        {
            string remarks = "O. K";
            Boiler b = new Boiler(100, 12);
            int t = b.GetTemp();
            int p = b.GetPresure();
            if (t > 150 || t < 80 || p < 12 || p > 15)
            {
                remarks = "Need Maintenance";
            }
            OnBoilerEventLog("Logging Info:\n");
            OnBoilerEventLog("Temparature " + t + "\nPressure: " + p);
            OnBoilerEventLog("\nMessage: " + remarks);
        }
        public void LogProcess(Boiler b1)
        {
            string remarks = "O. K";
             b = b1;
            int t = b.GetTemp();
            int p = b.GetPresure();
            if (t > 150 || t < 80 || p < 12 || p > 15)
            {
                remarks = "Need Maintenance";
            }
            OnBoilerEventLog("Logging Info:\n");
            OnBoilerEventLog("Temparature " + t + "\nPressure: " + p);
            OnBoilerEventLog("\nMessage: " + remarks);
        }
        protected void OnBoilerEventLog(string message)
        {
            if (BoilerEventLog != null)
            {
                BoilerEventLog(message);
            }
        }
    }
}
