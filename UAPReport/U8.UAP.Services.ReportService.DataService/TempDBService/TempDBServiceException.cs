using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ת�����ݵ���ʱ��ʱ���ִ���,�����������쳣
	/// </summary>
	public class TempDBServiceException : Exception
	{
		private string _exMsg = string.Empty;
		private string _msg = string.Empty;
		private Exception _causeException = null;

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="e">Դ�쳣</param>
		/// <param name="_exMsg">���ӵĴ�����Ϣ</param>
		public TempDBServiceException( Exception e, string exMsg )
			: this( e.Message, exMsg )
		{
			this._causeException = e;
		}

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="msg">������Ϣ</param>
		/// <param name="_exMsg">���ӵĴ�����Ϣ</param>
		public TempDBServiceException( string msg, string exMsg )
		{
			this._exMsg = exMsg;
			this._msg = msg;
		}

		/// <summary>
		/// �쳣Դ
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
		/// �쳣�����Ϣ
		/// </summary>
		public override string Message
		{
			get 
			{ 
				string m = string.Format( 
					"������ת�Ƶ���ʱ��ʱ�������󣬴�����Ϣ:\r\n{0}\r\n���Ӵ�����Ϣ:\r\n{1}",
					this._msg,
					this._exMsg );
				if( this._causeException != null )
					m += "\r\n���ö�ջ:\r\n" + this._causeException.StackTrace;
				return m;
			}
		}
	}
}
