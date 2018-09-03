using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class ViewLevelExpandSrv
	{
		private string _ReportId		= string.Empty;
		private string _UfDataCnnString = string.Empty;

		public ViewLevelExpandSrv( string reportId, string ufDataCnnString ) 
		{ 
			_ReportId = reportId;
			_UfDataCnnString = ufDataCnnString;
		}

		public string GetLevelExpand()
		{
			DataTable dt = GetLevelExpandInfor();
			if( dt == null )
				return string.Empty;

			return GetLevelExpandFromDataTable( dt );
		}

		public string GetLevelExpandFromDataTable( DataTable dt )
		{ 
			bool isRealHaveItem = false;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( "<LevelExpandSchema></LevelExpandSchema>" );
			XmlNode itemsNode = doc.CreateElement( "DesignTimeExpandItems" );
			doc.DocumentElement.AppendChild( itemsNode );
			foreach (DataRow dr in dt.Rows)
			{
				string name				= SqlHelper.GetStringFrom( dr[ "Name" ] );
				string columnName		= SqlHelper.GetStringFrom( dr[ "ColumnName" ] );
				string levelExpandType	= string.Empty;
				if (columnName != string.Empty)
				{
					isRealHaveItem	= true;
					levelExpandType = GetLevelExpandType( name );
					if( levelExpandType == string.Empty )
						continue;

					XmlElement node	= doc.CreateElement( "LevelExpandItem" );
					node.SetAttribute( "LevelExpandType", levelExpandType );
					node.SetAttribute( "ColumnName", columnName );
					node.SetAttribute( "Depth", "2" );
					itemsNode.AppendChild( node );
				}
			}

			if (isRealHaveItem)
			{
				doc.DocumentElement.AppendChild( doc.CreateElement( "ReportLevelExpands" ) );
				return doc.OuterXml;
			}

			return string.Empty;
		}

		private string GetLevelExpandType( string name )
		{
			switch (name)
			{
				case "按存货分类展开":
					return "0";
				case "按客户分类展开":
					return "1";
				case "按供应商分类展开":
					return "2";
				case "按部门展开":
					return "3";
				case "按科目编码展开":
					return "4";
				case "按地区分类展开":
					return "5";
				case "按收发类别展开":
					return "6";
				case "按结算方式展开":
					return "7";
				case "按货位展开":
					return "8";
				default:
					return string.Empty;
			}
		}

		private DataTable GetLevelExpandInfor()
		{
			string sql = UpgradeSqlProducer.GetSqlLevelExpandInforUpgrade( _ReportId );
			DataSet ds = SqlHelper.ExecuteDataSet( _UfDataCnnString, sql );
			DataRow dr = SqlHelper.GetDataRowFrom( 0, ds );
			if( dr != null )
				return ds.Tables[0];
			return null;
		}
	}
}
