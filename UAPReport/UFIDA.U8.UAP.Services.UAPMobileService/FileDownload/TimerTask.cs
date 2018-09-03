using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public static class TimerTask
    {
        public static long Time { get; set; }
        public static int Status { get; set; }
        private static Timer _timer;
        private static Stopwatch stopwatch = new Stopwatch();
        public static List<string> TokenList = new List<string>(); 
        public static string Token;
        private static MobileReportDownloadEngine engine;
        public static MobileReportDownloadEngine Engine
        {
            get { return engine ?? (engine = new MobileReportDownloadEngine()); }
            set { engine = value; }
        }
        public static void CreatTimerTask(string token)
        {
            if(!TokenList.Contains(token))
                TokenList.Add(token);
            Token = token;
            _timer = new Timer(Time);
            _timer.Elapsed += OnTimedEvent;
            _timer.Start();
            stopwatch.Start(); //  开始监视代码运行时间
            Status = 1;
        }

        // 定时任务
        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            var loginInfo = TokenTransfer.GetLoginInfo(Token);
            if (!Engine.Busy)
            {
                stopwatch.Reset();
                Engine.CreatDownLoadEngine(loginInfo);
            }
            if (string.IsNullOrEmpty(loginInfo.UfDataCnnString))
            {
                Status = 0;
                _timer.Close();
            }
            stopwatch.Stop(); //  停止监视计时

            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double seconds = timespan.TotalSeconds;  //  总秒数
            if (seconds > 60)
            {
                // 如果报表下载任务超过60S，则销毁任务
                Engine.DestroyTask();
                stopwatch.Reset();
            }
            stopwatch.Start(); //  开始监视计时
        }
    }
}
