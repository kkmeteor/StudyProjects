using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 转移数据到临时表时出现错误,将引发此类异常
	/// </summary>
	public class TempDBServiceException : Exception
	{
		private string _exMsg = string.Empty;
		private string _msg = string.Empty;
		private Exception _causeException = null;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="e">源异常</param>
		/// <param name="_exMsg">附加的错误信息</param>
		public TempDBServiceException( Exception e, string exMsg )
			: this( e.Message, exMsg )
		{
			this._causeException = e;
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="msg">错误信息</param>
		/// <param name="_exMsg">附加的错误信息</param>
		public TempDBServiceException( string msg, string exMsg )
		{
			this._exMsg = exMsg;
			this._msg = msg;
		}

		/// <summary>
		/// 异常源
		/// </summary>
		public Exception RootException
		{
			get 
			{ 
				Exception e = this._causeException;
				if( e is TempDBServiceException )
					return (e as TempDBServiceException).RootException;
				return e; 
			}
		}

		/// <summary>
		/// 异常输出信息
		/// </summary>
		public override string Message
		{
			get 
			{ 
				string m = string.Format( 
					"将数据转移到临时表时发生错误，错误信息:\r\n{0}\r\n附加错误信息:\r\n{1}",
					this._msg,
					this._exMsg );
				if( this._causeException != null )
					m += "\r\n调用堆栈:\r\n" + this._causeException.StackTrace;
				return m;
			}
		}
	}
}
