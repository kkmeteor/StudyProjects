using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class ResourceReportException: ApplicationException,ISerializable
    {
        private string _resourceId;
        private string _message="";
        private string[] _args;

        public  ResourceReportException( string resourceId )
        {
            this._resourceId = resourceId;
        }

        public ResourceReportException(string resourceId,string message ):this( resourceId)
        {
            this._message = message;
        }

        public ResourceReportException(string resourceId, string[] args)
            : this(resourceId)
        {
            _args = args;
        }

        public override string Message
        {
            get
            {
				//string resource = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx(_resourceId);
                string resource = String4Report.GetString(_resourceId);
				if (_args == null)
                    return _message + resource;
                else
                    return string.Format(resource, _args);
            }
        }

        protected ResourceReportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
		{
            _resourceId = info.GetString("ResourceId");
            _message = info.GetString("InnerMessage");
            _args = (string[])info.GetValue("Args", typeof(string[]));
		}

        #region ISerializable ≥…‘±

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ResourceId", _resourceId);
            info.AddValue("InnerMessage", _message);
            info.AddValue("Args", _args);
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
