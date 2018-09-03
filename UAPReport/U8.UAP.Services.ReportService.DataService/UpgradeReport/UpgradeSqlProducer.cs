using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class UpgradeSqlProducer
	{
		private UpgradeSqlProducer()
		{
		}

		public static string GetSqlLevelExpandInforUpgrade( string reportId )
		{
			return string.Format(
				@"Select Name,
				ColumnName=
				CASE 
					WHEN CHARINDEX(N',',Caption)>0 
					AND EXISTS(
					SELECT Name FROM Rpt_FldDef_Base 
					WHERE LocaleID='zh-CN' 
					AND ID=A.ID
					AND ModeEx=0
					AND Expression=SUBSTRING(Caption,CHARINDEX(N',',Caption)+1,LEN(Caption)-CHARINDEX(N',',Caption))
					)
					THEN SUBSTRING(Caption,CHARINDEX(N',',Caption)+1,LEN(Caption)-CHARINDEX(N',',Caption))
					
					WHEN CHARINDEX(N',',Caption)<=0 
					THEN Caption
					ELSE ''
				END
				FROM Rpt_FltDef_Base A
				WHERE ID=N'{0}'
				AND LocaleID=N'zh-CN' 
				AND Flag=1
				AND ModeEx=20",
				reportId );
		}

		public static string GetSqlDirectSaveDataSourceIntoTable( DataSourceInfor dataSourceInfor )
		{
			return string.Format(
				@"INSERT INTO BD_BusinessObjects
				(
					MetaID, 
					Name, 
					ProjectNo, 
					SubNo, 
					MetaInfo, 
					Description
				)
				VALUES
				( N'{0}', N'{1}', N'{2}', N'{3}', N'{4}', N'{5}' )",
				dataSourceInfor.MetaID,
				dataSourceInfor.Name.Replace( "'", "''" ),
				dataSourceInfor.ProjectNo,
				dataSourceInfor.SubNo,
				dataSourceInfor.MetaInfo.Replace( "'", "''" ),
				dataSourceInfor.Description.Replace( "'", "''" ) );
		}

		//	根据VB代码UfHeronReport.clsReportFormat.GetGroupCols中
		//	对cols(i).TpyGroup的赋值获取被作为分组的列名
		public static string GetSqlGroupSchemasItems( string rptId )
		{
			return string.Format( 
				@"SELECT Expression FROM RPT_FldDef_Base 
				WHERE ID=N'{0}' AND LocaleId=N'zh-CN' AND ModeEx=0 AND
				Name IN (
				SELECT DISTINCT Name 
				FROM RPT_FldDef_Base
				WHERE ID=N'{0}' AND ModeEx=3 
				AND OrderEx<>-2 AND LocaleId=N'zh-CN' )",
				rptId );
		}

		public static string GetSqlCustomReportInU861( string cAccId )
		{
			return string.Format(
					@"(
					SELECT UPPER(a.SystemID)+N'[__]'+a.Name AS NewReportID 
                    FROM rpt_glbdef_base a
                    WHERE a.WhereEx like N'RE%'
                    AND IsControlCreate=1 
                    AND Relation=N'heron'
                    AND LocaleId=N'zh-CN'
					AND ISNULL(bNewRpt,0)=1
                    AND EXISTS
                    (SELECT ID FROM rpt_flddef_base b 
                    WHERE  A.ID=B.ID
                    AND b.ModeEx=25
                    AND b.LocaleId=N'zh-CN' )
                    AND (UPPER(a.SystemID)+N'[__]'+a.Name) NOT IN
                    (SELECT ID FROM UFMeta_{0}..UAP_Report)
					)
					UNION
					(
					SELECT CONVERT(NVARCHAR(100),Guid) AS NewReportID 
					FROM Vqd_Rpt_Report 
					WHERE Type ='GeneralReport' 
                    AND ISNULL(bSystem,0)=0
                    AND CONVERT(NVARCHAR(100),Guid) NOT IN
                    (SELECT ID FROM UFMeta_{0}..UAP_Report)
					)",
					cAccId );
		}

		public static string GetSqlMaxColumnLevelCount( string id )
		{
			return string.Format(
				"SELECT MAX(Height) FROM rpt_FldDef_base WHERE id=N'{0}' and localeid='zh-CN' and ModeEx=5",
				id );
		}

		// 计算列时cell的Name取的中文下的Name字段
		// 而非简体下的可能不一样会导致计算列在非简体
		// 下没有该计算列
		public static string GetSqlCaculateColumnName(
			string id,
			string orderEx,
			string topEx )
		{
			return string.Format(
					@"SELECT Name 
					FROM rpt_flddef_base 
					WHERE ID=N'{0}' 
					AND ModeEx=0 
					AND OrderEx=N'{1}' 
					AND TopEx=N'{2}' 
					AND LocaleID='zh-CN'",
					id,orderEx,topEx );
		}

		// 数据引擎目前不支持多语,所以此处只取中文的信息
		public static string GetSqlExportReportColumnInfor( string exportRptDataSourceID )
		{
			return string.Format(
					@"SELECT 
					A.ColName AS ColName, 
					ISNULL(A.SaveType,N'Text') AS ColType, 
					ISNULL(B.Caption,A.ColName) AS ColDescription 
					FROM VQD_Dictionary A
					INNER JOIN VQD_CaptionLang B ON B.ResID=( A.TableName + N'.' + A.ColName ) 
					WHERE A.TableName=N'{0}' AND B.LocalID=N'zh-CN'",
				exportRptDataSourceID );
		}

		public static string GetSqlOldReportInfor( 
			string name, 
			string subId,
			string localeId )
		{
			return string.Format( "select DISTINCT ID,SystemID,Title,IsBaseTable,ClassName,FilterClass,Note from rpt_glbdef_base where name=N'{0}' and SystemID=N'{1}' and LocaleID=N'{2}'" ,
					name ,
					subId ,
					localeId );
		}

		public static string GetSqlOldReportInfor( 
			string id,
			string localeId )
		{
			return string.Format( "select DISTINCT ID,SystemID,Title,IsBaseTable,ClassName,FilterClass,Note from rpt_glbdef_base where id=N'{0}' and LocaleID=N'{1}'" ,
					id,
					localeId );
		}

		public static string GetSqlNewReportInfor( string id )
		{
			return string.Format( "select * from uap_report where id=N'{0}'", id );
		}

		public static string GetSqlIDFieldInZHCN( 
			string reportID,
			string modeEx,
			string id_Field )
		{
			string modeString = string.Empty;
			if( modeEx == "0" )
				modeString = "a.expression=b.expression";
			else if( modeEx == "5" )
				modeString = "a.LeftEx=b.LeftEx and a.TopEx=b.TopEx";
			else if( modeEx == "26" )
				modeString = "a.OrderEx=b.OrderEx";

			return string.Format(
				@"SELECT a.id_field
				FROM rpt_flddef_base a INNER JOIN rpt_flddef_base b 
				ON a.id=b.id AND a.modeex=b.modeex
				AND a.localeid<>b.localeid 
				AND a.id_field<>b.id_field AND {3}
				WHERE a.id=N'{0}' AND a.modeex=N'{1}' AND a.localeid='zh-CN' AND b.id_field=N'{2}'",
				reportID,
				modeEx,
				id_Field,
				modeString );
		}

		public static string GetSqlExportReportReportProperty( string reportID )
		{
			return string.Format( "SELECT * FROM VQD_Rpt_Report WHERE Guid=N'{0}'", reportID );
		}

		public static string GetSqlIsAllTypeBeGridLabel( string reportID )
		{
			return string.Format( 
				@"SELECT DISTINCT iSize 
				FROM rpt_flddef_base 
				WHERE ID=N'{0}'
				AND ModeEx=0 
				AND LocaleId='zh-CN'", 
				reportID );
		}

		public static string GetSqlExportReportLocaleInfor( string reportID )
		{
			return string.Format( "SELECT * FROM VQD_Rpt_ReportLang WHERE ReportID=N'{0}'", reportID );
		}

		public static string GetSqlExportReportViewsInfor( string reportID )
		{
			return string.Format( 
				@"SELECT *
				FROM VQD_Rpt_ReportView A
				INNER JOIN VQD_Rpt_ReportViewLang B 
				ON A.Guid=B.ViewID
				WHERE A.ReportID=N'{0}' 
				ORDER BY A.Guid", 
				reportID );
		}

		public static string GetSqlExportReportDataSourceType( string reportID )
		{
			return string.Format( 
				@"SELECT B.TableName, B.Type 
				FROM VQD_Rpt_Report A INNER JOIN VQD_DataTable B 
				ON A.DataSource=B.TableName
				WHERE Guid=N'{0}'", 
				reportID );
		}

		public static string GetSqlFldDefInfors( 
			string oldReportID, 
			string localeId,
			string modeEx )
		{
			return string.Format(
				@"SELECT * FROM rpt_flddef_base WHERE ID=N'{0}' AND LocaleId=N'{1}' AND ModeEx=N'{2}'",
				oldReportID,
				localeId,
				modeEx );
		}

		// ORDER BY OperatorTypeType保证了ModeEx排在前面,因为此时OrderEx为数字
		// 而Note为字母,这样就使得合计时ModeEx起到优先作用
		public static string GetSqlSummaryColInfor( string oldReportID )
		{
			return string.Format(
				@"SELECT Name,CONVERT(NVARCHAR(100),Note) AS OperatorTypeType 
				FROM rpt_flddef_base 
				WHERE ID=N'{0}' AND LocaleId=N'zh-CN' AND ModeEx=0 AND Note IN(N'AVG',N'MAX',N'MIN',N'SUM')
				UNION
				SELECT Name,CONVERT(NVARCHAR(100),OrderEx) AS OperatorTypeType 
				FROM rpt_flddef_base 
				WHERE ID=N'{0}' AND LocaleId=N'zh-CN' AND ModeEx=4 AND OrderEx<>-1
				ORDER BY OperatorTypeType",
				oldReportID );
		}

		public static string GetSqlAllColumnInfor( string oldReportID, string localeId )
		{
			return string.Format( 
				@"SELECT
				Title.ID_Field as Title_ID_Field,
				Title.Name as Title_Name,
				Title.Expression as Title_Expression,
				Title.CONditiON as Title_CONditiON,
				Title.ModeEx as Title_ModeEx,
				Title.OrderEx as Title_OrderEx,
				Title.TopEx as Title_TopEx,
				0 as Title_LeftEx,
				Title.Width as Title_Width,
				Title.Height as Title_Height,
				Title.Visible as Title_Visible,
				Title.Note as Title_Note,
				Title.nameForeign as Title_nameForeign,
				Title.iColSize as Title_iColSize,
				Title.FormatEx as Title_FormatEx,
				Title.iAlignStyle as Title_iAlignStyle,
				Title.iSize as Title_iSize,
				Title.Name as Column_Name,
				Title.Expression as Column_Expression,
				Title.CONditiON as Column_CONditiON,
				Title.ModeEx as Column_ModeEx,
				Title.OrderEx as Column_OrderEx,
				Title.TopEx as Column_TopEx,
				Title.LeftEx as Column_LeftEx,
				Title.Width as Column_Width,
				Title.Height as Column_Height,
				Title.Visible as Column_Visible,
				Title.Note as Column_Note,
				Title.nameForeign as Column_nameForeign,
				Title.iColSize as Column_iColSize,
				Title.FormatEx as Column_FormatEx,
				Title.iAlignStyle as Column_iAlignStyle,
				Title.iSize as Column_iSize 
				FROM rpt_flddef_base Title
				WHERE ID=N'{0}' and ModeEx=0 and LocaleId=N'{1}'
				and ID_Field not in(
				select ID_Field from rpt_FLDdef_bASE where (name like '%列名%' or name like '%Column name%') and modeex=0 and len(condition)=0) 
				ORDER by Title_OrderEx", 
				oldReportID,
				localeId );
		}

		public static string GetSqlLabelsInforNotInDetailSection( 
			string oldReportID, 
			string localeId,
			string sectionName )
		{ 
			return string.Format( 
					@"SELECT DISTINCT
					ID_Field as Title_ID_Field,Name,Expression,CONditiON,ModeEx,OrderEx,TopEx,LeftEx,
					Width,Height,Visible,Note,nameForeign,iColSize,FormatEx,iAlignStyle,iSize  
					FROM rpt_flddef_base 
					WHERE id=N'{0}' AND LocaleId=N'{1}' 
					AND ModeEx=N'26' AND nameForeign=N'{2}'
					ORDER BY OrderEx",
					oldReportID,
					localeId,
					sectionName );
		}

		//  这条脚本由于历史遗留原因所以很长,且不能修改(与代码的耦合性很强)
		//	union的前一部分是为没有列标题的列信息添加的列标题
		//	union的后一部分是将表中相同列的列信息和列标题合为
		//	查询结果的同一行
		public static string GetSqlColumnsInforInDetailSection(
			string oldReportID, 
			string localeId )
		{ 
			return string.Format( 
				@"((
					select 
					Title.ID_Field as Title_ID_Field,
					Title.ID_Field as BAK_ID_Field,
					Title.Name as Title_Name,
					Title.Expression as Title_Expression,
					Title.CONditiON as Title_CONditiON,
					Title.ModeEx as Title_ModeEx,
					Title.OrderEx as Title_OrderEx,
					Title.TopEx as Title_TopEx,
					Title.LeftEx as Title_LeftEx,
					Title.Width as Title_Width,
					Title.Height as Title_Height,
					Title.Visible as Title_Visible,
					Title.Note as Title_Note,
					Title.nameForeign as Title_nameForeign,
					Title.iColSize as Title_iColSize,
					Title.FormatEx as Title_FormatEx,
					Title.iAlignStyle as Title_iAlignStyle,
					Title.iSize as Title_iSize,
					Title.Name as Column_Name,
					Title.Expression as Column_Expression,
					Title.CONditiON as Column_CONditiON,
					Title.ModeEx as Column_ModeEx,
					Title.OrderEx as Column_OrderEx,
					Title.TopEx as Column_TopEx,
					Title.LeftEx as Column_LeftEx,
					Title.Width as Column_Width,
					Title.Height as Column_Height,
					Title.Visible as Column_Visible,
					Title.Note as Column_Note,
					Title.nameForeign as Column_nameForeign,
					Title.iColSize as Column_iColSize,
					Title.FormatEx as Column_FormatEx,
					Title.iAlignStyle as Column_iAlignStyle,
					Title.iSize as Column_iSize 
					from Rpt_FldDEF_Base Title 
					where 
					localeid=N'{0}' 
					and ModeEx='0'  
					and id=N'{1}'
					and ID_Field not in(
					select ID_Field from rpt_FLDdef_bASE where (name like '%列名%' or name like '%Column name%') and modeex=0 and len(condition)=0) 
					and Title.OrderEx <>all(
					select LeftEx from Rpt_FldDEF_Base B where B.localeid=Title.localeid and B.ModeEx='5' and B.id=Title.id)
					)
					union
					(
					Select 
					ColumnI.ID_Field as Title_ID_Field,
					Title.ID_Field as BAK_ID_Field,
					Title.Name as Title_Name,
					Title.Expression as Title_Expression,
					Title.CONditiON as Title_CONditiON,
					Title.ModeEx as Title_ModeEx,
					Title.OrderEx as Title_OrderEx,
					Title.TopEx as Title_TopEx,
					Title.LeftEx as Title_LeftEx,
					Title.Width as Title_Width,
					Title.Height as Title_Height,
					Title.Visible as Title_Visible,
					Title.Note as Title_Note,
					Title.nameForeign as Title_nameForeign,
					Title.iColSize as Title_iColSize,
					Title.FormatEx as Title_FormatEx,
					Title.iAlignStyle as Title_iAlignStyle,
					Title.iSize as Title_iSize,
					ColumnI.Name as Column_Name,
					ColumnI.Expression as Column_Expression,
					ColumnI.CONditiON as Column_CONditiON,
					ColumnI.ModeEx as Column_ModeEx,
					ColumnI.OrderEx as Column_OrderEx,
					ColumnI.TopEx as Column_TopEx,
					ColumnI.LeftEx as Column_LeftEx,
					ColumnI.Width as Column_Width,
					ColumnI.Height as Column_Height,
					ColumnI.Visible as Column_Visible,
					ColumnI.Note as Column_Note,
					ColumnI.nameForeign as Column_nameForeign,
					ColumnI.iColSize as Column_iColSize,
					ColumnI.FormatEx as Column_FormatEx,
					ColumnI.iAlignStyle as Column_iAlignStyle,
					ColumnI.iSize as Column_iSize 
					From Rpt_FldDEF_Base Title,Rpt_FldDEF_Base ColumnI 
					Where 
					Title.id=N'{1}' 
					and Title.Localeid=N'{0}'  
					and Title.ModeEx='5'  
					and ColumnI.id=N'{1}' 
					and ColumnI.Localeid=N'{0}' 
					and ColumnI.ModeEx='0'  
					and ColumnI.TopEx='0' 
					and Title.LeftEx=ColumnI.OrderEx 
					))
					order by Column_OrderEx,Title_Width,Title_ID_Field", 
					localeId, 
					oldReportID );	
		}
	}
}
