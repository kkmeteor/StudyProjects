using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
    [Serializable]
    public class ReportDataException : Exception
	{
		private string	_Message;
		
		private ReportDataException(){}

		public new string Message
		{
			set{ _Message = value; }
			get{ return _Message; }
		}

		internal static void ThrowCnnStringEmptyException( string source )
		{
			ReportDataException e = new ReportDataException();
            e.Message = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportData.ReportDataException.数据库连接串为空");
			e.Source	= source;
			throw e;
		}

		internal static void ThrowException( string message, string source )
		{
			ReportDataException e = new ReportDataException();
			e.Message	= message;
			e.Source	= source;
			throw e;
		}
	}
}
