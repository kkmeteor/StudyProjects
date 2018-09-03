/*
 * 作者:卢达其
 * 时间:2008.3.20
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 运行时格式处理异常
	/// </summary>
	public class RuntimeFormatException : Exception
	{
		public RuntimeFormatException(string msg) : base( msg )
		{
 
		}
	}
}

