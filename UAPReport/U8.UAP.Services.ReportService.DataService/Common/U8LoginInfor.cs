using U8Login;
using System.Text;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportData
{
    [Serializable]
	public class U8LoginInfor
	{
		#region Parameter

		private string	_LocaleID			= "zh-CN";
		private string	_UfDataCnnString	= string.Empty;
		private string	_UfMetaCnnString	= string.Empty;
		private string	_UfSystCnnString	= string.Empty;
		private string	_tempdbCnnString	= string.Empty;
        private string _appserver;
        private string _usertoken;
        private string _userid = "ASUSER";
        private string _caccid="001";
        private string _year="2009";
        private string _taskid;
        private string _subid="OutU8";
        private int _iyear;
        private int _imonth;
        private int _iday;
        private string _time;
        private string _date;
        private string _accdbname;
		[NonSerialized]
		private object	_U8Login			= null;
		[NonSerialized]
		private clsLogin	_U8LoginClass	= null;

		#endregion

        #region encrypt
        //private void writeObject(ObjectOutputStream out)

        #endregion

        #region Constructor

        public U8LoginInfor()
        {
        }

		public U8LoginInfor( object u8login )
		{
			if( u8login != null )
			{
				_U8Login		= u8login;
				_U8LoginClass	= (clsLogin)u8login;
				Setting();
			}
		}

		#endregion

		#region Property

        public string TaskID
        {
            get{ return _taskid; }
            set
            {
                _taskid = value;
            }
        }

        public string SubID
        {
            get{ return _subid; }
            set
            {
                _subid = value;
            }
        }

		public object U8Login
		{
			get{ return _U8Login; }
		}

		public clsLogin U8LoginClass
		{
			get{ return this._U8LoginClass; }
		}

		public string AppServer
		{
			get{ return _appserver ; }
            set{ _appserver = value; }
		}

        public string UserToken
        {
            get{ return _usertoken; }
            set
            {
                _usertoken = value;
            }
        }

		public string UserID
		{
			get{ return _userid ; }
            set{ _userid = value; }
		}

		public string LocaleID
		{
			get
            {
                if (_LocaleID.ToLower() == "en-us")
                    return "en-US";
                else if (_LocaleID.ToLower() == "zh-tw")
                    return "zh-TW";
                else
                    return "zh-CN"; 
            }
            set{ _LocaleID = value; }
		}
        public string Encrypte(string value)
        {
            return Encrypt.ToEncrypt("AAAA", value);
        }
        public static string Decrypte(string value)
        {
            return Encrypt.ToDecrypt("AAAA", value);
        }

		public string UfDataCnnString
		{
            //get{ return _UfDataCnnString; }
            //set{ _UfDataCnnString = value; }
            get
            {
                return Decrypte(this._UfDataCnnString);
            }
            set
            {
                _UfDataCnnString = Encrypte(value); 
            }
		}

		// 因为定时执行时没有login对象
		// 而要执行定时操作要使用UfMetaCnnString
		// 此时会这样使用构造函数U8LoginInfor(null)
		// 之后设置UfMetaCnnString
		public string UfMetaCnnString
		{
            set 
            {
                _UfMetaCnnString = Encrypte(value); 
            }
            get
            {
                return Decrypte(this._UfMetaCnnString);
            }
            //get{ return _UfMetaCnnString; }
            //set { _UfMetaCnnString = value; }
		}
		
		public string UfSystCnnString
		{
            //get{ return _UfSystCnnString; }
            //set{ _UfSystCnnString = value; }
            get
            {
                return Decrypte(this._UfSystCnnString);
            }
            set
            {
                _UfSystCnnString = Encrypte(value);
            }
		}

		public string TempDBCnnString
		{
            //get{ return _tempdbCnnString; }
            //set{ _tempdbCnnString = value; }
            get
            {
                return Decrypte(this._tempdbCnnString);
            }
            set
            {
                _tempdbCnnString = Encrypte(value);
            }
		}

		public string cAccId
		{
			get{ return _caccid ; }
            set{ _caccid = value; }
		}
		
		public string cYear
		{
			get{ return _year ; }
            set{ _year = value; }
		}

        public int Year
        {
            get
            {
                return _iyear;
            }
        }
        public int Month
        {
            get
            {
                return _imonth;
            }
        }

        public int Day
        {
            get
            {
                return _iday;
            }
        }

        public string Time
        {
            get
            {
                return _time;
            }
        }

        public string Date
        {
            get
            {
                return _date;
            }
        }

        public int AccountYear
        {
            get
            {
                if (Month == 12 && Day >= 25)
                    return Year + 1;
                else
                    return Year;
            }
        }

        public int AccountMonth
        {
            get
            {
                if (Month == 12 && Day >= 25)
                    return 1;
                else if (Day >= 25)
                    return Month + 1;
                else
                    return Month;
            }
        }
		
		#endregion

		#region Public Method

		/// <summary>
		/// 运行时生成的权限号要添加到登录信息的缓存中
		/// </summary>
		public void AddAuth2Cache( string authId )
		{
            bool result = false;
            if(_U8LoginClass!=null)
                result = this._U8LoginClass.AddAuthToCache(authId);
            if (result)
                Trace.WriteLine("成功添加权限缓存:" + authId);
            else
                Trace.WriteLine("添加权限缓存失败:" + authId);
		}

    	public void ChangeLocaleIdWhenSwitchLang( string localeId )
		{ 
			string oldLocaleID	= this._LocaleID;
			this._LocaleID		= localeId;
			ReplaceLocaleId( ref _UfDataCnnString, oldLocaleID, localeId );
			ReplaceLocaleId( ref _UfSystCnnString, oldLocaleID, localeId );
			ReplaceLocaleId( ref _UfMetaCnnString, oldLocaleID, localeId );
		}        

		#endregion

		#region Private Method
        //cyear-----权限（begin year）
        //iyear-----登录年（取数据库名）
		private void Setting()
		{
			string UFDBName = _U8LoginClass.UfDbName;
			if( UFDBName == null || UFDBName == string.Empty )
				return;

            this.UfDataCnnString = HandleConnectionString(_U8LoginClass.UFDataConnstringForNet);
			this.UfMetaCnnString	= HandleConnectionString(_U8LoginClass.UFMetaConnstringForNet);
            string metaname = "UFMeta_" + _U8LoginClass.get_cAcc_Id();
            this.UfSystCnnString = Decrypte(this._UfMetaCnnString).Replace(metaname, "UFSystem");
            this.TempDBCnnString = Decrypte(this._UfMetaCnnString).Replace(metaname, "tempdb");
			this._LocaleID			= _U8LoginClass.LanguageRegion;
            //this._appserver			= _U8LoginClass.cServer.ToString().Split(':')[0].Trim();
            this._appserver         = _U8LoginClass.cServer.ToString().Trim();
            this._usertoken			= _U8LoginClass.userToken;
            this._caccid			= _U8LoginClass.get_cAcc_Id();
            this._year = _U8LoginClass.cBeginYear;
            this._userid			= _U8LoginClass.cUserId;
            this._taskid = _U8LoginClass.get_TaskId();
            this._subid = _U8LoginClass.cSub_Id;
            this._iyear = _U8LoginClass.CurDate.Year;
            this._imonth = _U8LoginClass.CurDate.Month;
            this._iday = _U8LoginClass.CurDate.Day;
            this._time  = _U8LoginClass.CurDate.ToLongTimeString();
            this._date = _U8LoginClass.CurDate.ToShortDateString();
            object dbname=null;
            _U8LoginClass.GetAccInfo(401, ref dbname);
            _accdbname = dbname.ToString();
		}

        public string AccDbName
        {
            get
            {
                return _accdbname;
            }
        }

		private void ReplaceLocaleId( 
			ref string cnnString, 
			string oldLocaleId,
			string newLocaleId )
		{
			if( cnnString != null && cnnString != string.Empty )
				cnnString = cnnString.Replace( GetLangName( oldLocaleId ), GetLangName( newLocaleId ) );
		}

		private string GetLangName( string localeId )
		{
			string localeIdUpper = localeId.ToUpper();
			switch( localeIdUpper )
			{
				default:
				case "ZH-CN":
					return "Simplified Chinese";
				case "ZH-TW":
					return "Traditional Chinese";
				case "EN-US":
					return "English";
			}
		}

        private string HandleConnectionString(string connstring)
        {
            int index = connstring.ToLower().IndexOf("connect timeout=");
            string s1 = connstring.Substring(0, index);
            string s2 = connstring.Substring(index);
            index = s2.IndexOf(";");
            s2 = s2.Substring(index + 1);
            StringBuilder sb = new StringBuilder();
            sb.Append(s1);
            sb.Append("Connect Timeout=7200;");
            sb.Append(s2);
            sb.Append("max pool size=500;Application Name = UFReportService" + System.Guid.NewGuid().ToString());
            return sb.ToString();
        }

        #region old
        //private string GetUfDataCnnStringNET( 
        //    string ufDataString, 
        //    ref string dataBase )
        //{
        //    string			s		= ufDataString;
        //    StringBuilder	sb		= new StringBuilder();
        //    string[]		sall	= s.Split( ';' );
        //    for( int i=0; i < sall.Length; i++ )
        //    {
        //        if( sall[i] == null || sall[i] == string.Empty )
        //            continue;

        //        int equalindex	= sall[i].IndexOf( "=" );
        //        string sheader	= sall[i].Substring( 0, equalindex );
        //        if( sheader.ToLower() == "persist security info" ||
        //            sheader.ToLower() == "data source" ||
        //            sheader.ToLower() == "user id" ||
        //            sheader.ToLower() == "pwd" ||
        //            sheader.ToLower() == "current language" )
        //        {
        //            if( sb.Length > 0 )
        //                sb.Append( ";" );
        //            sb.Append( sall[i] );
        //        }
        //        else if( sheader.ToLower()=="initial catalog" )
        //        {
        //            if( sb.Length > 0 )
        //                sb.Append( ";" );
        //            sb.Append( sall[i] );
					
        //            string[] temp1 = sall[i].Split( '=' );
        //            dataBase = temp1[1];
        //        }
        //        else if( sheader.ToLower()=="connect timeout" )
        //        {
        //            if( sb.Length > 0 )
        //                sb.Append( ";" );
        //            sb.Append( "Connect Timeout=600" );
        //        }
        //        else if( sheader.ToLower()=="password" )
        //        {
        //            if( sb.Length > 0 )
        //                sb.Append( ";" );
        //            sb.Append( "Password=" );
        //            sb.Append( sall[i].Substring( equalindex +1, sall[i].Length-equalindex -1 ));
        //        }
        //    }
        //    sb.Append(";max pool size=500;Application Name = UFReportService"+System.Guid.NewGuid().ToString());//;min pool size=10

        //    return sb.ToString();
        //}
        #endregion
        #endregion
    }
}
