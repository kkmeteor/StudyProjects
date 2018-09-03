/*
 * 作者:卢达其
 * 时间:2009.1.13
 * 
 * 890重构“报表中心”:
 * 1.提高加载速度
 * 2.剔除不必要的功能
 */

using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 动态报表和静态报表的基类
	/// </summary>
	public abstract class ReportNode : ReportCenterNode, IReportNode
	{
		private string _subID = null;
		private string _subName = null;

		public abstract string ToPortalMenuCmdString();

		public string SubID
        {
            get { return _subID; }
            set { _subID = value; }
        }
		
		public string SubName
        {
            get { return _subName; }
            set { _subName = value; }
        }

     
	}
}