using System;
using System.Drawing;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp ;
using System.Runtime.Remoting.Channels.Http ;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Activation;

using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// DefaultStyleSchema 的摘要说明。
	/// </summary>
	public class DefaultConfigs
	{
        private static string _cachepath;
        private static string _url = null;

        public static int ReportLeft
        {
            get
            {
                return 2;
            }
        }

        public static int SECTIONHEADERHEIGHT
        {
            get
            {
                return 24;
            }
        }

        public static string CachePath
        {
            get
            {
                return _cachepath;
            }
            set
            {
                _cachepath = value;
            }
        }

		public static Color NormalSelect
		{
			get
			{
				return SystemColors.Desktop;
			}
		}

		public static int DefaultMargin
		{
			get
			{
				return 80;
			}
		}

		public static Color LastSelect
		{
			get
			{
				return SystemColors.HotTrack;
			}
		}

		public static Color DefaultTitleBackColor
		{
			get
			{
                return Color.White;
			}
		}

		public static Color CrossColor
		{
			get
			{
				return Color.Black;
			}
		}

		public static Color SelectedItemColor
		{
			get
			{
				return Color.FromArgb(0x46,0x66,0x9f);
			}
		}

        public static int FontShadow
        {
            get
            {
                return 1;
            }
        }

        public static int LineShadow
        {
            get
            {
                return 1;
            }
        }

		public static Color GroupItemColor
		{
			get
			{
				return Color.FromArgb(0xfd,0xff,0xed);
			}
		}

		public static Color LineColor
		{
			get
			{
                return Color.FromArgb(0x9f,0x9f,0x9f);
			}
		}

		public static Color ReportBackColor
		{
			get
			{
				return Color.White;
			}
		}

		public static Color RowHeaderColor
		{
			get
			{
				return SystemColors.Control;
			}
        }

        public static RemoteDataHelper  GetRemoteHelper()
        {
            try
            {
                return (RemoteDataHelper)Activator.GetObject(typeof(RemoteDataHelper), _url);
            }
            catch (TargetInvocationException e)
            {
                throw new ReportException(LanError + e.InnerException.Message + CheckLan);
            }
            catch (Exception e)
            {
                throw new ReportException(LanError + e.Message + CheckLan);
            }
        }

        public static ReportEngine GetRemoteEngine(U8LoginInfor login, ReportStates state)
        {
            try
            {
                object[] attrs = new object[] { new UrlAttribute(_url) };
                object[] objs = new object[] { login,state };
                return (ReportEngine)Activator.CreateInstance(typeof(ReportEngine), objs, attrs);
            }
            catch (TargetInvocationException e)
            {
                throw new ReportException(LanError + ((e.InnerException is TargetInvocationException)?e.InnerException.InnerException.Message:e.InnerException.Message) + CheckLan);
            }
            catch (Exception e)
            {
                throw new ReportException(LanError + e.Message + CheckLan);
            }
        }

        #region resource
        private static string LanError
        {
            get
            {
                string LanguageID = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                if (LanguageID == "zh-CN")
                    return "网络错误：";
                else if (LanguageID == "zh-TW")
                    return "網路錯誤：";
                else //LanguageID != "en-US"
                    return "Net work error:";
            }
        }

        private static string CheckLan
        {
            get
            {
                string LanguageID = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                if (LanguageID == "zh-CN")
                    return " 造成该错误的原因可能是由于您的网络突然中断、网络设置变更或报表服务未启用等。请检查或重新登录U8";
                else if (LanguageID == "zh-TW")
                    return " 造成該錯誤的原因可能是由於您的網路突然中斷、網路設置變更或報表服務未啟用等。請檢查或重新登錄U8";
                else //LanguageID != "en-US"
                    return " Creates this wrong reason possibly is because your network sudden stop, the network establishment change or the UFReportService has not start and so on. Please inspect or registers U8 again";
            }
        }
        #endregion

        public static void RegChannel(string appserver)
        {
            RegChannel(false, appserver);
        }

        public static void RegChannel(bool bweb,string appserver)
        {
            try
            {
                string basepath = "";
                if (bweb)
                    basepath = System.Environment.SystemDirectory + @"\UFCOMSQL\ReportConfig.xml";
                else
                {
                    basepath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                    if (!basepath.EndsWith(@"\"))
                        basepath += @"\";
                    if (!basepath.ToLower().EndsWith(@"\uap\"))
                        basepath += @"uap\";
                    basepath = basepath.ToLower().Replace(@"\uap\", @"\UFCOMSQL\");
                    basepath += @"ReportConfig.xml";
                }
                bool bhttp = false;
                string port = "9023";
                bool blocalecalc = false;
                bool buseipaddress = true;
                string bindto = "";
                string channelname = "";
                int clientport = 0;

                if (System.IO.File.Exists(basepath ))
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(basepath);
                    XmlElement root = doc.DocumentElement;
                    root = root.SelectSingleNode("Settings") as XmlElement;
                    buseipaddress = Convert.ToInt32(root.GetAttribute("buseipaddress")) == 1 ? true : false;
                    bindto = root.GetAttribute("bindto").Trim();
                    port = root.GetAttribute("port").Trim();
                    clientport = Convert.ToInt32(root.GetAttribute("clientport").Trim());
                    blocalecalc = Convert.ToInt32(root.GetAttribute("blocalecalc")) == 1 ? true : false;
                }

                if (blocalecalc && !bweb )
                {
                    appserver = "localhost";
                    buseipaddress = true;
                    bindto = "127.0.0.1";
                    channelname = "UFClientReportServiceChannel";
                }
                else
                {
                    appserver = appserver.Split(':')[0].Trim();
                    bhttp = ClientReportContext.Protocal.ToLower() == "http";
                    port = ClientReportContext.Port;
                    channelname = "UFReportServiceChannel";
                }

                bool breged = false;                
                IChannel[] channels = ChannelServices.RegisteredChannels;
                lock (channels.SyncRoot)
                {
                    foreach (IChannel eachChannel in channels)
                    {
                        if (eachChannel.ChannelName == channelname)
                        {
                            breged = true;
                            break;
                        }
                    }

                    if (!breged)
                    {
                        BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                        BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
                        serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                        IDictionary props = new Hashtable();
                        props["name"] = channelname;
                        props["port"] = clientport;
                        props["rejectRemoteRequests"] = false;
                        props["useIpAddress"] = buseipaddress;
                        if (bindto.Trim() != "")
                        {
                            props["useIpAddress"] = true;
                            props["bindTo"] = bindto.Trim();
                        }

                        string protocol = @"tcp://";
                        port = ":" + port;
                        if (bhttp)
                        {
                            HttpChannel channel = new HttpChannel(props, clientProvider, serverProvider);
                            ChannelServices.RegisterChannel(channel);

                            WebProxy wp = GlobalProxySelection.Select as WebProxy;
                            wp.Credentials = CredentialCache.DefaultCredentials;
                            FieldInfo clientChannelFieldInfo = typeof(HttpChannel).GetField("_clientChannel", BindingFlags.Instance | BindingFlags.NonPublic);
                            HttpClientChannel clientChannel = (HttpClientChannel)clientChannelFieldInfo.GetValue(channel);
                            FieldInfo proxyObjectFieldInfo = typeof(HttpClientChannel).GetField("_proxyObject", BindingFlags.Instance | BindingFlags.NonPublic);
                            proxyObjectFieldInfo.SetValue(clientChannel, wp);
                            protocol = @"http://";
                            System.Diagnostics.Trace.WriteLine("Channel uri: " + ((ChannelDataStore)channel.ChannelData).ChannelUris[0]);
                            //System.Windows.Forms.MessageBox.Show("Client uri: " + ((ChannelDataStore)channel.ChannelData).ChannelUris[0]);
                        }
                        else
                        {
                            TcpChannel channel = new TcpChannel(props, clientProvider, serverProvider);
                            ChannelServices.RegisterChannel(channel);
                            System.Diagnostics.Trace.WriteLine("Channel uri: " + ((ChannelDataStore)channel.ChannelData).ChannelUris[0]);
                            //System.Windows.Forms.MessageBox.Show ("Client uri: " + ((ChannelDataStore)channel.ChannelData).ChannelUris[0]);
                        }

                        _url = protocol + appserver + port + "/ReportEngine";
                        System.Diagnostics.Trace.WriteLine(_url);
                        RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
                        //System.Diagnostics.Trace.WriteLine("注册成功");
                    }
                }
            }
            catch (Exception e)
            {
                throw new ReportException(LanError + e.Message + CheckLan);
                //System.Diagnostics.Trace.WriteLine("注册失败：" + e.Message);
            }
        }
    }

    public class ParallelCenter
    {
        private static Hashtable _parallelitems;
        private static ArrayList _waitinglist;
        private static Hashtable _servicinglist;
        private static int _servicingcount=0;
        private static int _max;
        private static int _waitinglimit;
        private static bool _enable = false;
        private static bool _includeall = false;
        private static int _defaultlevel = 1;
        private static System.Timers.Timer _timer;
        static ParallelCenter()
        {
            _parallelitems = new Hashtable();
            _waitinglist = new ArrayList();
            _servicinglist = new Hashtable();
            _timer = new System.Timers.Timer();
            _timer.Interval = 10000;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            _timer.Start();
        }

        private static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_enable)
                return;
            lock (_parallelitems.SyncRoot)
            {
                if (_waitinglist.Count > 0 && _servicingcount < _max)
                {
                    ActivateAWaitingRequest();
                }
            }
        }

        //s to get the heighest level when waiting for  a too long time
        public static int WaitingLimit
        {
            get
            {
                return _waitinglimit;
            }
            set
            {
                _waitinglimit = value;
            }
        }

        public static bool Enable
        {
            set
            {
                _enable = value;
            }
        }

        public static bool IncludeAll
        {
            set
            {
                _includeall = value;
            }
        }

        public static int DefaultLevel
        {
            set
            {
                _defaultlevel = value;
            }
        }

        public static string WaitingList()
        {
            StringBuilder sb = new StringBuilder();
            lock (_waitinglist.SyncRoot)
            {
                for(int i=0;i<_waitinglist.Count;i++)
                {
                    Request r = _waitinglist[i] as Request;
                    sb.Append(r.ID );
                    sb.Append(" 优先级：");
                    sb.AppendLine(r.Level.ToString());
                }
            }
            return sb.ToString();
        }

        public static  string ServicingList()
        {
            StringBuilder sb=new StringBuilder();
            lock (_servicinglist.SyncRoot)
            {
                foreach (string key in _servicinglist.Keys)
                {
                    int count = (int)_servicinglist[key];
                    string s = key;
                    if (count > 1)
                        s += ": "+count.ToString();
                    sb.AppendLine(s);
                }
            }
            return sb.ToString();
        }

        //<=0 is not limited
        public static int Max
        {
            get
            {
                return _max;
            }
            set
            {
                _max = value;
            }
        }

        public static void AddAParallelItem(string id, ParallelItem item)
        {
            if (id != null && !_parallelitems.Contains(id.ToLower()))
                _parallelitems.Add(id.ToLower(), item);
        }
        
        public static Request SubmitARequest(string id)
        {
            if (!_enable)
                return null;
            lock (_parallelitems.SyncRoot)
            {
                if (NeedWait(id))
                {
                    int level = _defaultlevel;
                    if(_parallelitems.Contains(id.ToLower()))
                        level=(_parallelitems[id.ToLower()] as ParallelItem).Level;
                    Request r = new Request(id, level);
                    _waitinglist.Add(r);
                    return r;
                }
                else
                {
                    AddToServicingList(id);
                }
            }
            return null;
        }

        public static void CommitARequest(string id,Guid guid)
        {
            if (!_enable)
                return;
            lock (_parallelitems.SyncRoot)
            {
                EndAService(id,guid);
                ActivateAWaitingRequest();
            }
        }

        private static void ActivateAWaitingRequest()
        {
            ActivateAWaitingRequest(TheFirstToActivate);
        }

        private static void ActivateAWaitingRequest(Request r)
        {
            if (r != null)
            {
                _waitinglist.Remove(r);
                if (!r.bActive)
                {
                    r.Activate();
                    AddToServicingList(r.ID);
                }
            }
        }

        private static void EndAService(string id,Guid guid)
        {
            if (!_servicinglist.Contains(id.ToLower()) && guid != Guid.Empty)
            {
                System.Diagnostics.Trace.WriteLine("Add -----------------------" + id);
                ActivateAWaitingRequest(FindRequest(guid));
            }

            if (_servicinglist.Contains(id.ToLower()))
            {
                int count = Convert.ToInt32(_servicinglist[id.ToLower()]);
                if (count > 1)
                    _servicinglist[id.ToLower()] = count - 1;
                else
                    _servicinglist.Remove(id.ToLower());
                _servicingcount--;
            }
        }

        private static void AddToServicingList(string id)
        {
            if (_servicinglist.Contains(id.ToLower()))
                _servicinglist[id.ToLower()] = Convert.ToInt32(_servicinglist[id.ToLower()]) + 1;
            else
                _servicinglist.Add(id.ToLower(), 1);
            _servicingcount++;
        }

        private static Request FindRequest(Guid guid)
        {
            for (int i = 0; i < _waitinglist.Count; i++)
            {
                Request r=_waitinglist[i] as Request;
                if (r.Guid == guid)
                    return r;
            }
            return null;
        }

        private static Request TheFirstToActivate
        {
            get
            {
                int maxlevel = Int32.MaxValue;
                Request ther=null;
                for (int i = 0; i < _waitinglist.Count; i++)
                {
                    Request r = _waitinglist[i] as Request;
                    bool benough=MultiInstanceEnough(r.ID);
                    TimeSpan ts = DateTime.Now - r.Time;
                    if (_waitinglimit >= 10 && ts.Seconds > _waitinglimit && !benough)
                    {
                        ther = r;
                        break;
                    }
                    if (r.Level < maxlevel && !benough )
                    {
                        maxlevel = r.Level;
                        ther = r;
                    }
                }
                return ther;
            }
        }

        private static bool MultiInstanceEnough(string id)
        {
            if (_parallelitems.Contains(id.ToLower()) && _servicinglist.Contains(id.ToLower()))
            {
                int count = (int)_servicinglist[id.ToLower()];
                ParallelItem pi = _parallelitems[id.ToLower()] as ParallelItem;
                if (pi.Limit > 0 && count >= pi.Limit)
                    return true;
            }
            return false;
        }

        private static bool NeedWait(string id)
        {
            if (_includeall || _parallelitems.Contains(id.ToLower()))
            {
                if (_max > 0 && _servicingcount >= _max)
                    return true;
                if (MultiInstanceEnough(id))
                    return true;
            }
            return false;
        }
    }

    public class ParallelItem
    {
        private string _id;
        private int _level;
        private int _limit;
        public ParallelItem()
        {
        }
        public ParallelItem(string id, int level, int limit)
        {
            _id = id;
            _level = level;
            _limit = limit;
        }
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }
        public int Limit
        {
            get
            {
                return _limit;
            }
            set
            {
                _limit = value;
            }
        }
    }

    public class Request
    {
        private Guid  _guid;
        private string _id;
        private int _level;
        private DateTime _time;
        private ManualResetEvent _request;
        public Request(string id, int level)
        {
            _guid = Guid.NewGuid();
            _id = id;
            _level = level;
            _request = new ManualResetEvent(false);
            _time = DateTime.Now;
        }

        public Guid Guid
        {
            get
            {
                return _guid;
            }
        }

        public void Wait()
        {
            _request.WaitOne();
        }

        public bool bActive
        {
            get
            {
                return _request.WaitOne(0, false);
            }
        }

        public void Activate()
        {
            _request.Set();
        }

        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }

        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
        }
    }

    public class UFConvert
    {
        public static  double ToDouble(object value)
        {
            try
            {
                if (value == null || value.ToString() == "" ||
                    value == DBNull.Value)
                    return 0;
                else
                    return Convert.ToDouble(value);
            }
            catch
            {
                return 0;
            }
        }
    }
}
