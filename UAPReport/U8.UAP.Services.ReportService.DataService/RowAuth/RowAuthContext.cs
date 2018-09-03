/*
 * 作者:卢达其
 * 时间:2009.7.28
 * 
 * 环境信息对象
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using UFSoft.U8.Framework.SecurityCommon;
using U8Login;

namespace UFIDA.U8.UAP.Services.ReportData
{
	class RowAuthContext
	{
		private clsLogin	_u8LoginClass	= null;
		private ContentAuth _contentAuth = null;
        private string _subId = string.Empty;
        private string _cSubId = string.Empty;

		public const string NoAuthExpression = "1=2";//某些情况下_clsRowAuth返回此表达式表示无权

		public clsLogin U8LoginClass
		{
			get { return this._u8LoginClass; }
			set { this._u8LoginClass = value; }
		}

		public ContentAuth ContentAuth
		{
			get { return this._contentAuth; }
			set { this._contentAuth = value; }
		}

        public string SubId
        {
            get { return this._subId; }
            set { this._subId = value; }
        }

        public string CSubId
        {
            get { return _cSubId; }
            set { _cSubId = value; }
        }
	}
}
