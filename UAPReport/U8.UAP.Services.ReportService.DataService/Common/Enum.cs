using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	#region For ReportMeta

	public enum PersistentObjectState
	{
		New,
		View,
		Delete
	}

	public enum ReportDataExceptionEnum
	{
		ExistedReportName
	}

	public enum ReportDataSourceTypeEnum
	{
		ER,
		SQL,
		StoreProc,
		Custom
	}

	public enum RunTimeReportType
	{
		StandardReport,
		CustomReport,
		StaticReport
	}

	#endregion

	#region For RuntimePublishment.

	public enum ProduceByType
	{ 
		ReportCenter = 0,
		U8Portal,
		U8Task
	}

	public enum ReportDataPublishSettingType
	{ 
		New = 0,
		ModifyPerReport,
		ModifySpecifiedPublish
	}

	public enum ReportDataPublishType
	{ 
		StaticReport = 0, 
		EMail, 
		Both 
	}
	
	public enum ReportDataEMailAffixType
	{ 
		Excel = 0, 
		Html 
	}
	
	public enum ReportDataPublishTimeType
	{ 
		Immediately = 0,
		GivenTime, 
		Both,
	}
	
	public enum ReportDataGivenTimeType
	{ 
		AbsoluteTime = 0,
		CycleTime,
		Both,
        RefreshDataWhenQuery
	}
	
	public enum ReportDataCycleTimeType
	{ 
		EveryDay = 0,
		EveryWeek, 
		EveryMonth,
		EveryOur,
	}

	public enum ReportDataStaticDeleteType
	{ 
		Automatic = 0,
		ByHand
	}

	#endregion 

	#region For RuntimeDataToTempTable.

	public enum DataSourceType
	{
		None,
		FromSql,
		FromSqlExcuteBySqlCommand,
		FromStoreProcedure
	}

	#endregion

	/// <summary>
	/// 因为数据库里也沿用此枚举名称,所以不要随便改变
	/// </summary>
	[Serializable]
	public enum SaveAsType
	{
		SaveAsReport,
		SaveAsView
	}

	[Serializable]
	public enum RunItemType
	{
		Report,
		ReportView,
		StaticReport,
		PublishData
	}
}
