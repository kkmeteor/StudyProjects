using System;
using System.Collections.Generic;
using System.Text;
using UFIDA.U8.MA.Component.Framework;
using UFIDA.U8.MERP.MerpContext;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public class ReportComponet : IMAService
    {
        private const string COMPONENT_NAME = "WA00030";

        /// <summary>
        /// 显式声明无参数构造方法，移动调用可能用得上
        /// </summary>
        public ReportComponet()
        {

        }
        /// <summary>
        /// 调用请求入口
        /// </summary>
        /// <param name="token"></param>
        /// <param name="requestXml"></param>
        /// <param name="responseXml"></param>
        /// <returns></returns>
        public string GetServiceMsg(ref string token, string requestXml, ref string responseXml)
        {
            //注册ActionHandler
            if (!ActionDispatcher.IsRegistered(COMPONENT_NAME))
            {
                var actions = new Dictionary<string, Type>
                    {
                        {"getReportList", typeof (GetReportListActionHandler)},
                        {"getReport", typeof (OpenReportActionHandler)},
                        {"markReport",typeof(MarkReportActionHandler)},
                        {"unmarkReport",typeof(UnMarkReportActionHandler)},
                        {"getMarkReportList",typeof(GetMarkReportListActionHandler)},
                        {"addDownloadTask",typeof(AddDownloadTaskActionHandler)},
                        {"getDownloadTaskList",typeof(GetDownloadTaskListActionHandler)},
                        {"deleteDownloadTask",typeof(DeleteDownloadTaskActionHandler)}
                    };
                ActionDispatcher.Register(COMPONENT_NAME, actions);
            }
            //执行
            string result = ActionDispatcher.Execute(COMPONENT_NAME, token, requestXml, ref responseXml);
            //声明字符集   
            //实例化utf-8字符集编码方式
            //Encoding utf8 = Encoding.GetEncoding("UTF-8");
            //result = Encoding.GetEncoding("UTF-8").GetString(Encoding.GetEncoding("GB2312").GetBytes(result));
            return result;
        }
    }
}