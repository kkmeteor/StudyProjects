using System;
using System.Data;
//using UFIDA.U8.UAP.Services.ExFormatUpdateService;
using UFIDA.U8.UAP.Services.BizDAE.Elements;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class UpgradeExportReport : UpgradeReport
	{
		//private string	_UfDataCnnString	= string.Empty;
		//private string	_ExDataSourceID		= string.Empty;
		//private string	_CommonFormat		= string.Empty;
		//private string	_LocaleFormat		= string.Empty;
		//private string	_ClassName			= string.Empty;

		//public UpgradeExportReport( 
		//    string exportRptID, 
		//    string UfDataCnnString,
		//    string UfMetaCnnString ) 
		//    : base()
		//{
		//    this.ID					= exportRptID;
		//    this.SubProjectID		= "EX";
		//    this._UfDataCnnString	= UfDataCnnString;
		//    this.DbConnString		= UfMetaCnnString;

		//    FillData();
		//}

		//private void FillData()
		//{
		//    FillReportProperty();
		//    FillReportLocaleInfor();
		//    FillDataSourceBO();
		//    FillViews();
		//}

		//#region Private Method

		//private void FillReportProperty()
		//{
		//    DataSet ds = GetDataSet( UpgradeSqlProducer.GetSqlExportReportReportProperty( this.ID ) );
		//    DataRow dr = ds.Tables[0].Rows[0];
			
		//    _ExDataSourceID		= SqlHelper.GetStringFrom( dr[ "DataSource" ] );
		//    this.CreatedTime	= SqlHelper.GetDataTimeFrom( dr[ "CreateDate" ], "2006-9-19" );
		//    _ClassName			= GetClassName( SqlHelper.GetStringFrom( dr[ "Code" ] ));
			
		//    // 升级出口自定义报表时，ClassName为空,FilterClass="clsExVoucher"
		//    if (ReportDataFacade._IsUpgradeCustomReport)
		//    {
		//        this.ClassName = string.Empty;
		//        this.FilterClass = "UFSoftEX.clsExVoucher";
		//    }
		//    else
		//    {
		//        this.ClassName = _ClassName;
		//        this.FilterClass = _ClassName;
		//    }
		//}

		//private void FillReportLocaleInfor()
		//{
		//    DataSet ds = GetDataSet( UpgradeSqlProducer.GetSqlExportReportLocaleInfor( this.ID ) );
			
		//    for( int i = 0; i < ds.Tables[0].Rows.Count; i++ )
		//    {
		//        ReportLocaleInfo reportLocaleInfo	= new ReportLocaleInfo();
		//        DataRow dr							= ds.Tables[0].Rows[i];
		//        reportLocaleInfo.Name				= SqlHelper.GetStringFrom( dr[ "Name" ] );
		//        reportLocaleInfo.LocaleID			= SqlHelper.GetStringFrom( dr[ "LocalID" ] );
		//        reportLocaleInfo.Description		= SqlHelper.GetStringFrom( dr[ "Description" ] );
		//        this.ReportLocaleInfos.Add( reportLocaleInfo );
		//    }
		//}
		
		//private void FillDataSourceBO()
		//{
		//    string DataSourceName = string.Empty;
		//    string DataSourceType = "VIEW";

		//    DataSet ds = GetDataSet( UpgradeSqlProducer.GetSqlExportReportDataSourceType( this.ID ) );
		//    DataRow dr = ds.Tables[0].Rows[0];
		//    DataSourceName = SqlHelper.GetStringFrom( dr[ "TableName" ] );
		//    DataSourceType = SqlHelper.GetStringFrom( dr[ "Type" ] );
			
		//    ds = GetDataSet( UpgradeSqlProducer.GetSqlExportReportColumnInfor( this._ExDataSourceID ) );
		//    ConvertDataToBO convertDataToBO	= new ConvertDataToBO(
		//        ds.Tables[0],
		//        "EX",
		//        string.Format( "出口报表:{0}", this.ReportLocaleInfos[ "zh-CN" ].Name, this.ID ) );

		//    if ( DataSourceType.Trim().ToUpper() == "PROCEDURE" )
		//        this.DataSourceInfo.DataSourceBO = convertDataToBO.GetBusinessObject( true, _ClassName );
		//    else
		//        this.DataSourceInfo.DataSourceBO = convertDataToBO.GetBusinessObject( "SELECT * FROM " + DataSourceName );

		//    this.DataSourceID = this.DataSourceInfo.DataSourceBO.MetaID;

		//    if ( ReportDataFacade._IsUpgradeCustomReport )
		//    {
		//        this.DataSourceInfo.IsShouldSave	= true;
		//        this.DataSourceInfo.MetaID			= this.DataSourceID;
		//        this.DataSourceInfo.Name			= this.DataSourceInfo.DataSourceBO.Name;
		//        this.DataSourceInfo.ProjectNo		= this.DataSourceInfo.DataSourceBO.ProjectNo;
		//        this.DataSourceInfo.SubNo			= this.DataSourceInfo.DataSourceBO.SubNo;
		//        this.DataSourceInfo.Description		= this.DataSourceInfo.DataSourceBO.Description;
		//        this.DataSourceInfo.MetaInfo		= this.DataSourceInfo.DataSourceBO.Serialize();
		//    }
		//}

		//private void FillViews()
		//{
		//    string					lastViewID				= string.Empty;
		//    ReportViewLocaleInfo	reportViewLocaleInfo	= null;
		//    UpgradeReportView		upgradeReportView		= null;
		//    ExFormatUpdate			exFormatUpdate			= new ExFormatUpdate( this._UfDataCnnString );

		//    QueryFunction qf = (QueryFunction)this.DataSourceInfo.DataSourceBO.Functions[0];
		//    ColumnCollection columns = qf.QuerySettings[0].QueryResultTable.Columns;

		//    DataSet ds = GetDataSet( UpgradeSqlProducer.GetSqlExportReportViewsInfor( this.ID ) );
		//    for( int i = 0; i < ds.Tables[0].Rows.Count; i++ )
		//    {
		//        DataRow dr = ds.Tables[0].Rows[i];
		//        string guid = SqlHelper.GetStringFrom( dr[ "Guid" ] );
		//        if( lastViewID != guid )
		//        {
		//            lastViewID						= guid;
		//            exFormatUpdate.Update( lastViewID, columns );
			        
		//            upgradeReportView				= new UpgradeReportView( this );
		//            upgradeReportView.ID			= lastViewID;
		//            upgradeReportView.ViewType		= 2;
		//            upgradeReportView.CommonFormat  = exFormatUpdate.CommonFormat;
		//            this.ReportViews.Add( upgradeReportView );
		//        }
				
		//        string localeId                     = SqlHelper.GetStringFrom( dr[ "LocalID" ] );
		//        reportViewLocaleInfo				= new ReportViewLocaleInfo();
		//        reportViewLocaleInfo.Name           = GetViewNameBy( dr[ "Caption" ], localeId );
		//        reportViewLocaleInfo.LocaleID       = localeId;
		//        reportViewLocaleInfo.LocaleFormat   = exFormatUpdate.LocalFormat[ localeId ].ToString();
		//        upgradeReportView.ViewLocaleInfos.Add( reportViewLocaleInfo );
		//    }
		//}

		//private string GetViewNameBy( object dc, string localeID )
		//{
		//    string strAdded = string.Empty;
		//    if ( ReportDataFacade._IsUpgradeCustomReport )
		//    {
		//        switch ( localeID.ToUpper() )
		//        {
		//            case "ZH-CN":
		//                strAdded = "(原861视图)";
		//                break;
		//            case "ZH-TW":
		//                strAdded = "(原861視圖)";
		//                break;
		//            case "EN-US":
		//                strAdded = "(View of U861)";
		//                break;
		//            default:
		//                break;
		//        }
		//    }

		//    return SqlHelper.GetStringFrom( dc ) + strAdded;
		//}

		//private DataSet GetDataSet( string sql )
		//{
		//    return SqlHelper.ExecuteDataSet( this._UfDataCnnString, sql );
		//}

		//private string GetClassName( string reportCode )
		//{ 
		//    switch( reportCode.Trim().ToUpper() )
		//    {
		//        case "PROFITANALYSE_FOREIGNFARE":
		//        case "PROFITANALYSE_PROFIT":
		//        case "盈亏分析明细":
		//            return "UFSoftEX.clsExReport";
				
		//        case "ORDEREXESTATUS":
		//        case "ORDERSTATISTICS":
		//        case "CONSIGNMENTSTATISTICS":
		//        case "INVOICESTATISTICS":
		//        case "TOTALDETAIL":
		//        case "INCOMECOSTFAREANALYSE":
		//        case "CURRENTSTOCKQUERY":
		//            return "UFSoftEX.clsStatisticsReport";
				
		//        default:
		//            return "UFSoftEX.clsOpenReport";
		//    }
		//}

		//#endregion
	}
}
