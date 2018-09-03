/*
 * ����:¬����
 * ʱ��:2009.1.13
 * 
 * 890�ع����������ġ�:
 * 1.��߼����ٶ�
 * 2.�޳�����Ҫ�Ĺ���
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
	/// ��̬����;�̬����Ļ���
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