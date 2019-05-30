using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EventStudy1.EventHandler;

namespace EventStudy1
{
    public class EventTest
    {
        private int value;
        public delegate void NumberManipulationHandler();

        public event NumberManipulationHandler ChangeNumber;
        protected virtual void OnNumberChanged()
        {
            if (ChangeNumber == null)
                Console.WriteLine("event not fire");
            else
                ChangeNumber();
        }
        public EventTest()
        {
            int n = 5;
            SetValue(n);
        }

        public void SetValue(int n)
        {
            if (value != n)
            {
                value = n;
                OnNumberChanged();
            }
        }
    }
    /***********订阅器类***********/

    public class subscribEvent
    {
        public void printf()
        {
            Console.WriteLine("event fire");
            Console.ReadKey(); /* 回车继续 */
        }
    }
    class Program
    {
        private static  void Log(string log)
        {
            Console.WriteLine(log);
        }
        static void Main(string[] args)
        {
            //EventTest e = new EventTest(); /* 实例化对象,第一次没有触发事件 */
            //subscribEvent v = new subscribEvent(); /* 实例化对象 */
            //e.ChangeNumber += new EventTest.NumberManipulationHandler(v.printf); /* 注册 */
            //e.SetValue(7);
            //e.SetValue(11);
            Boiler boiler = new Boiler(100, 16);
            FileLogger fileLogger = new FileLogger("c:\\boiler.txt");
            EventHandler eventHandler = new EventHandler();
            eventHandler.BoilerEventLog += Log;
            eventHandler.BoilerEventLog += fileLogger.Logger;
            eventHandler.LogProcess();
            eventHandler.LogProcess(boiler);
            fileLogger.Close();
        }
    }
}
