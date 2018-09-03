using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 发布模式信息
	/// </summary>
	internal class PublishDataMode : ICloneable
	{
		private ReportDataPublishType		_publishType		= ReportDataPublishType.StaticReport;
		private ReportDataEMailAffixType	_eMailAffixType		= ReportDataEMailAffixType.Excel;

		public object Clone()
        {
            return this.MemberwiseClone();
        }

		public ReportDataPublishType PublishType
		{
			get{ return _publishType; }
			set{ _publishType = value; }
		}
		
		public ReportDataEMailAffixType EMailAffixType
		{
			get{ return _eMailAffixType; }
			set{ _eMailAffixType = value; }
		}
	}
}
