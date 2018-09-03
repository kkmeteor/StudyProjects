using System;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CanceledException ��ժҪ˵����
	/// </summary>
    [Serializable]
    public class CanceledException : ApplicationException,ISerializable 
	{
		public CanceledException():base()
		{			
		}
		public CanceledException(string msg):base(msg)
		{			
		}

        protected CanceledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
		{
		}

        public override string Message
        {
            get
            {
                return "";
            }
        }

        #region ISerializable ��Ա

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
	}

    public class InvalidExpressionException : ApplicationException
    {
    }

}
