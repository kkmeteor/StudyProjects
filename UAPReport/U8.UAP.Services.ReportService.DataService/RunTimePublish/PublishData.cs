/*
 * 作者:卢达其
 * 时间:2009.2.11
 * 
 * 890重构报表发布功能
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 报表发布信息对象
	/// </summary>
	public class PublishData : ReportCenterNode, ICloneable
	{
		public const int DataMaintainDaysDefault = 7;
        public const int DataMaintainVersionsDefault = 3;
		public const string EmailContextSeparator = "[#eemms#]";

		private int	_reportViewType	= 1;
		private int	_dataMaintainDays = PublishData.DataMaintainDaysDefault;
        private int _dataMaintainVersions = PublishData.DataMaintainVersionsDefault;
		private bool _isDataSpeciafied = false;//对象数据已经填充
		private bool _bPortalInfo = false;
		private bool _bPhoneMessageInfo = false;
		private bool _bCoverOldData = false;
        private bool _bAutoDelete = false;
		private bool _bRefused = false;
        private bool _bVersionDeleteControl = false;
        // 报表数据为空时不通知，V13.0增加
        private bool _noticeCondition = false;
        public bool NoticeCondition
        {
            get { return _noticeCondition; }
            set { _noticeCondition = value; }
        }
		private string	_localeID		= string.Empty;
		private string	_publisherID	= string.Empty;
		private string	_reportViewID	= string.Empty;
		private string	_filterXml		= string.Empty;
		private string	_emailTitle		= string.Empty;
		private string	_emailContext	= string.Empty;
		private string	_name			= string.Empty;
		private string  _groupSchemaId	= string.Empty;
		private ProduceByType _produceBy = ProduceByType.ReportCenter;
		
		private ReportDataStaticDeleteType _staticDeleteType = ReportDataStaticDeleteType.Automatic;
		private PublishDataMode			_publishDataMode = new PublishDataMode();
		private PublishDataTime			_publishDataTime = new PublishDataTime();
		private List<U8User>			_publishDataReceivers = new List<U8User>();

		private U8LoginInfor _loginInfo = null;

		public PublishData(IOpenReportCenterNode iors) : this(null, iors)
		{
		}

		public PublishData(string id, IOpenReportCenterNode iors) 
		{
			this.ID = id;
			this.OpenEngine = iors;
		}

		public override object Clone()
		{
			PublishData pd = this.MemberwiseClone() as PublishData;
			pd._publishDataMode = this._publishDataMode.Clone() as PublishDataMode;
			pd._publishDataTime = this._publishDataTime.Clone() as PublishDataTime;
			pd._publishDataReceivers = new List<U8User>();
			foreach(U8User u in this._publishDataReceivers)
				pd._publishDataReceivers.Add(u);
			return pd;
		}

		/// <summary>
		/// 手动删除与数据库DataMaintainDays字段是否等于0对应
		/// </summary>
		public ReportDataStaticDeleteType StaticDeleteType
		{ 
			get { return this._staticDeleteType; }
			set { this._staticDeleteType = value; }
		}

		public bool bCoverOldData
		{
			get { return this._bCoverOldData; }
			set { this._bCoverOldData = value; }
		}
        public bool bAutoDelete
        {
            get { return this._bAutoDelete; }
            set { this._bAutoDelete = value; }
        }

		public bool bPortalInfo
		{
			get { return this._bPortalInfo; }
			set { this._bPortalInfo = value; }
		}

		public bool bPhoneMessageInfo
		{
			get { return this._bPhoneMessageInfo; }
			set { this._bPhoneMessageInfo = value; }
		}

		public ProduceByType ProduceBy
		{
			get { return this._produceBy; }
			set { this._produceBy = value; }
		}

		public bool IsDataSpeciafied
		{
			get { return this._isDataSpeciafied; }
			set { this._isDataSpeciafied = value; }
		}

		public bool bRefused
		{
			get { return this._bRefused; }
			set { this._bRefused = value; }
		}

		public U8LoginInfor LogInfo
		{
			get { return this._loginInfo; }
			set { this._loginInfo = value; }
		}

		public string GroupSchemaId
		{
			get{ return this._groupSchemaId; }
			set{ this._groupSchemaId= value; }
		}

		public string Name
		{
			get{ return this._name; }
			set{ this._name = value; }
		}

		public override string Text
		{
			get
			{
				if(string.IsNullOrEmpty(base.Text))
					return this.Name;
				return base.Text; 
			}
			set{ base.Text = value; }
		}

		public override string ToString()
		{
			return this.Name;
		}
		
		public int DataMaintainDays
		{
			get{ return this._dataMaintainDays; }
			set{ this._dataMaintainDays = value; }
		}

        public int DataMaintainVersions
        {
            get { return this._dataMaintainVersions; }
            set { this._dataMaintainVersions = value; }
        }
        public bool bVersionDeleteControl
        {
            get { return this._bVersionDeleteControl; }
            set { this._bVersionDeleteControl = value; }
        }
		public string EmailTitle
		{
			get{ return this._emailTitle; }
			set{ this._emailTitle = value; }
		}

		public string EmailContext
		{
			get{ return this._emailContext; }
			set{ this._emailContext = value; }
		}

		public string FilterXml
		{
			get{ return this._filterXml; }
			set{ this._filterXml = value; }
		}

		public string LocaleID
		{
			get{ return _localeID; }
			set{ _localeID = value; }
		}

		public string PublisherID
		{
			get{ return _publisherID; }
			set{ _publisherID = value; }
		}

		public string ReportViewID
		{
			get{ return _reportViewID; }
			set{ _reportViewID = value; }
		}

		public int ReportViewType
		{
			get{ return _reportViewType; }
			set{ _reportViewType = value; }
		}

		public ReportDataPublishType PublishType
		{
			get{ return _publishDataMode.PublishType; }
			set{ _publishDataMode.PublishType = value; }
		}
		
		public ReportDataEMailAffixType EMailAffixType
		{
			get{ return _publishDataMode.EMailAffixType; }
			set{ _publishDataMode.EMailAffixType = value; }
		}

		public ReportDataPublishTimeType PublishTimeType
		{
			get{ return _publishDataTime.PublishTimeType; }
			set{ _publishDataTime.PublishTimeType = value; }
		}

		public ReportDataGivenTimeType GivenTimeType		
		{
			get{ return _publishDataTime.GivenTimeType; }
			set{ _publishDataTime.GivenTimeType = value; }
		}

		public ReportDataCycleTimeType CycleTimeType
		{
			get{ return _publishDataTime.CycleTimeType; }
			set{ _publishDataTime.CycleTimeType = value; }
		}

		public DateTime AbsoluteTime		
		{
			get{ return _publishDataTime.AbsoluteTime; }
			set{ _publishDataTime.AbsoluteTime = value; }
		}

		public DateTime FirstExecuteTime		
		{
			get{ return _publishDataTime.FirstExecuteTime; }
			set{ _publishDataTime.FirstExecuteTime = value; }
		}

		public DateTime CreateTime
		{
			get{ return _publishDataTime.CreateTime; }
			set{ _publishDataTime.CreateTime = value; }
		}

		public DateTime ModifyTime
		{
			get{ return _publishDataTime.ModifyTime; }
			set{ _publishDataTime.ModifyTime = value; }
		}

		public List<U8User> Receivers
		{
			get{ return _publishDataReceivers; }
			set{ _publishDataReceivers = value; }
		}

		public bool Retrieve(SqlTransaction tr)
		{ 
			this.PublisherID = this._loginInfo.UserID;
			this.LocaleID = this._loginInfo.LocaleID;

			SqlCommand cmd = this.GetCmdRetrieve(this);
			DataSet ds = null;
			if(tr != null)
				ds = SqlHelper.ExecuteDataSet(tr, cmd);
			else
				ds = SqlHelper.ExecuteDataSet(this._loginInfo.UfMetaCnnString, cmd);
			if(ds.Tables[0].Rows.Count == 0)
				return false;

			// 返回的表顺序依据存储过程"UAP_Report_PublishmentRetrieve"
			DataTable[] dts = new DataTable[]{
				ds.Tables[0],
				ds.Tables[1],
				ds.Tables[2],
				ds.Tables[3],
			};
			this.RetrieveFrom(dts);
			return true;
		}

		public void RetrieveFrom(DataTable[] dts)
		{ 
			this.FillPublishData(dts[0], this);
			this.FillPublishLangInfo(dts[1], this);
			this.FillReceivers(dts[2], this);
			this.FillReportViewType(dts[3], this);
		}

		public void Update(SqlTransaction tr)
		{ 
			SqlCommand cmd	= new SqlCommand("UAP_Report_PublishmentUpdate");
			cmd.CommandType	= CommandType.StoredProcedure;
			
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ID",				SqlDbType.NVarChar,	100,	this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@PublishType",		SqlDbType.Int,		(object)this.PublishType ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@EMailAffixType",	SqlDbType.Int,		(object)this.EMailAffixType ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@GivenTimeType",	SqlDbType.Int,		(object)this.GivenTimeType ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@CycleTimeType",	SqlDbType.Int,		(object)this.CycleTimeType ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@AbsoluteTime",	SqlDbType.DateTime,	this.AbsoluteTime ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FirstExecuteTime",SqlDbType.DateTime, this.FirstExecuteTime ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@PublisherID",		SqlDbType.NVarChar, 50,		this.PublisherID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ModifyTime",		SqlDbType.DateTime, this.ModifyTime ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@PublishTimeType",	SqlDbType.Int,		(object)this.PublishTimeType ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterXml" ,		SqlDbType.NText,	this.FilterXml ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReceiverList",	SqlDbType.NVarChar, 4000,	this.GetReceiverList(this) ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@EmailContext",	SqlDbType.NText, this.WrapEmailContext()));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@CreateTime",		SqlDbType.DateTime, DateTime.Now));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportViewID" ,	SqlDbType.NVarChar, 100,	this.ReportViewID) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@Name",			SqlDbType.NVarChar, 256, this.Name));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleID",		SqlDbType.NVarChar, 10, this.LogInfo.LocaleID));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@bPortalInfo",		SqlDbType.Bit, this.bPortalInfo));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@bPhoneMessageInfo", SqlDbType.Bit, this.bPhoneMessageInfo));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@bCoverOldData",	SqlDbType.Bit, this.bCoverOldData));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ProduceBy",		SqlDbType.NVarChar, 100, (int)this.ProduceBy));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@GroupSchemaId",	SqlDbType.NVarChar, 100, this.GroupSchemaId));

			int dataMaintainDays =0;
            int dataMaintainVersions = 0;
            if (this.StaticDeleteType == ReportDataStaticDeleteType.Automatic && this.bAutoDelete)
            {
                if (this.bVersionDeleteControl)
                    dataMaintainVersions = this.DataMaintainVersions;
                else
                    dataMaintainDays = this.DataMaintainDays;
            }
			cmd.Parameters.Add(SqlHelper.GetParameter("@DataMaintainDays",      SqlDbType.Int, (object)dataMaintainDays));
            cmd.Parameters.Add(SqlHelper.GetParameter("@DataMaintainVersions",  SqlDbType.Int, (object)dataMaintainVersions));
            cmd.Parameters.Add(SqlHelper.GetParameter("@bAutoDelete",           SqlDbType.Bit, this.bAutoDelete));
            cmd.Parameters.Add(SqlHelper.GetParameter("@bNoticeCondition", SqlDbType.Bit, this.NoticeCondition));

			SqlHelper.ExecuteNonQuery(tr, cmd);
		}

		private string WrapEmailContext()
		{
			return string.Format(
				"{0}{1}{2}",
				this.EmailTitle,
				PublishData.EmailContextSeparator,
				this.EmailContext);
		}

		private void SplitEmailContext(string contextString)
		{
			if (!string.IsNullOrEmpty(contextString))
			{
				string[] emailContexts = contextString.Split(
				   new string[] { PublishData.EmailContextSeparator },
				   StringSplitOptions.None);
				this.EmailTitle = emailContexts[0];
				this.EmailContext = emailContexts[1];
			}
		}

		public void Delete(SqlTransaction tr)
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_PublishmentDelete" );
			cmd.CommandType	= CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@PublishID", SqlDbType.NVarChar, 100, this.ID));
			cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 10, this.LogInfo.cAccId));
			cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, this.LogInfo.cYear));
			SqlHelper.ExecuteNonQuery(tr, cmd);
		}

		private SqlCommand GetCmdRetrieve(PublishData pd)
		{
			SqlCommand cmd	= new SqlCommand("UAP_Report_PublishmentRetrieve");
			cmd.CommandType	= CommandType.StoredProcedure;
			cmd.Parameters.Add( SqlHelper.GetParameter( "@UserID",			SqlDbType.NVarChar,	100, pd.PublisherID));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@PublishID",		SqlDbType.NVarChar,	100, pd.ID));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleID",		SqlDbType.NVarChar,	10, pd.LocaleID));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",		SqlDbType.NVarChar,	3, this.LogInfo.cAccId));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",		SqlDbType.NVarChar,	4, this.LogInfo.cYear));
			return cmd;
		}

		private void FillPublishData(DataTable table, PublishData pd)
		{
			if (table.Rows.Count > 0)
			{
				DataRow dr = table.Rows[0];
				pd.PublishType = (ReportDataPublishType)SqlHelper.GetIntFrom(dr["PublishType"]);
				pd.PublishTimeType = (ReportDataPublishTimeType)SqlHelper.GetIntFrom(dr["PublishTimeType"]);
				pd.EMailAffixType = (ReportDataEMailAffixType)SqlHelper.GetIntFrom(dr["EMailAffixType"]);
				pd.GivenTimeType = (ReportDataGivenTimeType)SqlHelper.GetIntFrom(dr["GivenTimeType"]);
				pd.CycleTimeType = (ReportDataCycleTimeType)SqlHelper.GetIntFrom(dr["CycleTimeType"]);
				pd.ProduceBy = (ProduceByType)SqlHelper.GetIntFrom(dr["ProduceBy"]);

				pd.AbsoluteTime = SqlHelper.GetDataTimeFrom(dr["AbsoluteTime"], "2009-9-19 12:00");
				pd.CreateTime = SqlHelper.GetDataTimeFrom(dr["CreateTime"], "2009-9-19 12:00");
				pd.ModifyTime = SqlHelper.GetDataTimeFrom(dr["ModifyTime"], "2009-9-19 12:00");
				pd.FirstExecuteTime = SqlHelper.GetDataTimeFrom(dr["FirstExecuteTime"], "2009-9-19 12:00");

				pd.bCoverOldData = SqlHelper.GetBooleanFrom(dr["bCoverOldData"], false);
				pd.bPortalInfo = SqlHelper.GetBooleanFrom(dr["bPortalInfo"], false);
				pd.bPhoneMessageInfo = SqlHelper.GetBooleanFrom(dr["bPhoneMessageInfo"], false);

				pd.ID = dr["ID"].ToString();
				pd.PublisherID = dr["PublisherID"].ToString();
				pd.ReportViewID = dr["ReportViewID"].ToString();
				pd.FilterXml = SqlHelper.GetStringFrom(dr["FilterXml"]);
				pd.GroupSchemaId = SqlHelper.GetStringFrom(dr["GroupSchemaId"]);
				
				pd.DataMaintainDays = SqlHelper.GetIntFrom(dr["DataMaintainDays"]);
                pd.DataMaintainVersions = SqlHelper.GetIntFrom(dr["DataMaintainVersions"]);
                pd.bAutoDelete = SqlHelper.GetBooleanFrom(dr["bAutoDelete"],false);
                pd.NoticeCondition = SqlHelper.GetBooleanFrom(dr["bNoticeCondition"], false);
				if (pd.DataMaintainDays == 0 && pd.DataMaintainVersions == 0)
				{
					pd.StaticDeleteType = ReportDataStaticDeleteType.ByHand;
					pd.DataMaintainDays = PublishData.DataMaintainDaysDefault;
                    pd.DataMaintainVersions = PublishData.DataMaintainVersionsDefault;
				}
                else if (pd.DataMaintainDays == 0)
                {
                    pd.bVersionDeleteControl = true;
                }
                else if (pd.DataMaintainDays != 0 &&pd.bCoverOldData==false )
                {
                    pd.bAutoDelete = true;                
                }

				this.SplitEmailContext(SqlHelper.GetStringFrom(dr["EmailContext"], string.Empty));
			}
		}

		private void FillPublishLangInfo(DataTable table, PublishData pd)
		{
			if (table.Rows.Count > 0)
				pd.Name = table.Rows[0]["Name"].ToString();
		}

		private void FillReceivers(DataTable table, PublishData pd)
		{
			pd.Receivers.Clear();
			foreach (DataRow dr in table.Rows)
			{
				U8User user = PublishDataService.GetU8user(dr);
				pd.Receivers.Add(user);

				//判断当前操作员是否拒绝接受该发布
				if(user.ID == this._loginInfo.UserID)
					pd.bRefused = SqlHelper.GetBooleanFrom(dr["bRefused"], false);
			}
		}

		private void FillReportViewType(DataTable table, PublishData pd)
		{
			if (table.Rows.Count > 0)
				pd.ReportViewType = Convert.ToInt32(table.Rows[0]["ViewType"]);
		}

		private string GetReceiverList(PublishData pd)
		{
			string receiverList = string.Empty;
			foreach(U8User user in pd.Receivers)
				receiverList += ( user.ID + ";" );
			return receiverList;
		}
	}
}
