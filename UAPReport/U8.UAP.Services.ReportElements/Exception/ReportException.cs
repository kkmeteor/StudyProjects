using System;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ReportException 的摘要说明。
	/// </summary>
	[Serializable]
    public class ReportException : ApplicationException,ISerializable
	{
        //private string[] _args;
		public ReportException(string msg):base(msg)
		{

		}

        //public ReportException(string msg,string[] args)
        //    : base(msg)
        //{
        //    _args = args;
        //}

        protected ReportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //_args = (string[])info.GetValue("Args", typeof(string[]));
        }

        //public string[] Args
        //{
        //    get
        //    {
        //        return _args;
        //    }
        //}

        #region ISerializable 成员

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            //info.AddValue("Args", _args);
        }

        #endregion
    }

    public class CodeException : Exception
    {
        private string _code;
        public CodeException(string code,string msg)
            : base(msg)
        {
            _code = code;
        }
        public string Code
        {
            get
            {
                return _code;
            }
        }
    }
}
