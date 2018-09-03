using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class MendWhenUpgrading
	{
		private bool		_IsFilledData		= false;
		private string		_ReportID			= string.Empty;
		private string		_LocaleID			= string.Empty;
		private string		_UfDataConnString	= string.Empty;
		
		private DataColumnInforCollection _DataColumnInfors		= new DataColumnInforCollection();
		private DataColumnInforCollection _SummaryTypeInfors	= new DataColumnInforCollection();

		#region Constructor

		public MendWhenUpgrading(
			string reportID,
			string localeID,
			string ufDataCnnString )
		{
			_ReportID = reportID;
			_LocaleID = localeID;
			_UfDataConnString = ufDataCnnString;
		}

		#endregion

		#region Public Method
		
		#region Static Method

		public static string GetFieldNote(
			string ufDataCnnString,
			string reportId )
		{ 
			string note	= string.Empty;
			string sql	= UpgradeSqlProducer.GetSqlOldReportInfor( reportId, "zh-CN" );
			DataSet ds	= SqlHelper.ExecuteDataSet( ufDataCnnString, sql );
			DataRow dr	= SqlHelper.GetDataRowFrom( 0 , ds );
			if( dr != null )
				return SqlHelper.GetStringFrom( dr[ "Note" ] );

			return string.Empty;
		}

		public static string GetSqlBaseTableOldId( 
			string ufDataCnnString,
			string reportId,
			string subId )
		{
			string note	= GetFieldNote( ufDataCnnString, reportId );
			if( note != string.Empty )
			{
				string sql = UpgradeSqlProducer.GetSqlOldReportInfor( note, subId, "zh-CN" );
				DataSet ds	= SqlHelper.ExecuteDataSet( ufDataCnnString, sql );
				DataRow dr	= SqlHelper.GetDataRowFrom( 0 , ds );
				if( dr != null )
					return SqlHelper.GetStringFrom( dr[ "ID" ] );
			}
			
			return string.Empty;
		}

		public static ReportDefinition GetSqlBaseReportDefinition( 
			string ufDataCnnString,
			string reportId )
		{
			ReportDefinition rdf = new ReportDefinition();
			ReportDefinition.DbConnString = ufDataCnnString;
			rdf.ID = reportId;
			rdf.LocaleID = "zh-CN";
			if( rdf.Retrieve( true ) )
				return rdf;
			
			return null;
		}

		#endregion

		// ������ʱcell��Nameȡ�������µ�Name�ֶ�
		// ���Ǽ����µĿ��ܲ�һ���ᵼ�¼������ڷǼ���
		// ��û�иü�����
		public string GetCaculateColumnName(
			string id,
			string orderEx,
			string topEx )
		{ 
			string sql = UpgradeSqlProducer.GetSqlCaculateColumnName( id, orderEx, topEx );
			DataSet ds = SqlHelper.ExecuteDataSet( _UfDataConnString, sql );
			DataRow dr = SqlHelper.GetDataRowFrom( 0, ds );
			if( dr != null )
				return SqlHelper.GetStringFrom( dr[0] );

			return string.Empty;
		}

		// �ѱ��ʽ��ԭ����������ر�ʾ��
		// ����Դ�ȱ�ʶ�滻Ϊ�����޹صı�ʶ
		public void ReplaceNameDescription( ref string stringToReplace )
		{
			if( stringToReplace == string.Empty )
				return;

			if( ! _IsFilledData )
				PrepareDataColumnInfor();

			for( int i=0; i < _DataColumnInfors.Count; i++ )
			{
				string name			= _DataColumnInfors[i].Name;
				string description	= _DataColumnInfors[i].Description;
				if( name != string.Empty && description != string.Empty )
					stringToReplace = stringToReplace.Replace( description, name );
			}

			stringToReplace = stringToReplace.Replace( "��", "(" );
			stringToReplace = stringToReplace.Replace( "��", ")" );
		}

		// �ж��Ƿ��ǰٷֱ�
		public bool IsPercentColumn( string columnDescription )
		{
			if( ! _IsFilledData )
				PrepareDataColumnInfor();

			DataColumnInfor col = _DataColumnInfors[ columnDescription ];
			if( col != null )
				return col.IsPercentColumn;

			return false;
		}

		// �ж��Ƿ��Ƿ���ٷֱ�
		public bool IsPercentGroupColumn( string columnDescription )
		{
			if( ! _IsFilledData )
				PrepareDataColumnInfor();

			DataColumnInfor col = _DataColumnInfors[ columnDescription ];
			if( col != null )
				return col.IsPercentGroupColumn;

			return false;
		}

		//	�ٷֱ�
		public string GetPercentExpression( string columnKey )
		{
			return string.Format( 

@"double dsum = 0;
for( int i = 0; i < rows.Count; i++ )
	dsum += rows[i].{0};
if( dsum == 0 )
	return 0;
else
	return current.{0} / dsum;",
					   
			columnKey );
		}

		//	����ٷֱ�
		public string GetPercentGroupExpression( string columnKey )
		{
			return string.Format(

@"double dsum = 0;
for( int i = startindex; i <= endindex; i++ )
	dsum += rows[i].{0};
if( dsum == 0 )
	return 0;
else
	return current.{0} / dsum;",
			
			columnKey );
		}

		public bool IsSummaryColumn( string columnName )
		{
			if( ! _IsFilledData )
				PrepareDataColumnInfor();

			DataColumnInfor col = _SummaryTypeInfors[ columnName ];
			if( col != null )
				return col.IsSummaryColumn;

			return false;
		}

		public string GetOperatorType( string columnName )
		{
			if( ! _IsFilledData )
				PrepareDataColumnInfor();

			DataColumnInfor col = _SummaryTypeInfors[ columnName ];
			if( col != null )
				return col.OperatorType;

			return string.Empty;
		}

		public int GetPrecision( string name )
		{
			if( ! _IsFilledData )
				PrepareDataColumnInfor();

			DataColumnInfor col = _DataColumnInfors[ name ];
			if( col != null )
				return col.Precision;

			return -1;
		}

		#endregion

		#region Private Method

		private void PrepareDataColumnInfor()
		{
			string sql = UpgradeSqlProducer.GetSqlFldDefInfors( _ReportID, _LocaleID, "0" );
			DataSet ds = SqlHelper.ExecuteDataSet( _UfDataConnString, sql );
			if( ds != null )
			{
				for( int i=0; i < ds.Tables[0].Rows.Count; i++ )
				{
					DataColumnInfor col = new DataColumnInfor();
					DataRow dr		= ds.Tables[0].Rows[i];
					col.Name		= SqlHelper.GetStringFrom( dr[ "Expression" ] );
					col.Description = SqlHelper.GetStringFrom( dr[ "Name" ] );
					col.FormatEx	= SqlHelper.GetStringFrom( dr[ "FormatEx" ] );
					
					if( col.Name == string.Empty )
						col.Name = col.Description;
					
					_DataColumnInfors.Add( col );
				}
				
				_DataColumnInfors.Sort();
			}

			sql = UpgradeSqlProducer.GetSqlSummaryColInfor( _ReportID );
			ds	= SqlHelper.ExecuteDataSet( _UfDataConnString, sql );
			if( ds != null )
			{
				for( int i=0; i < ds.Tables[0].Rows.Count; i++ )
				{
					DataColumnInfor col = new DataColumnInfor();
					DataRow dr			= ds.Tables[0].Rows[i];
					col.Name			= SqlHelper.GetStringFrom( dr[ "Name" ] );
					col.OperatorType	= SqlHelper.GetStringFrom( dr[ "OperatorTypeType" ] );
					
					// �ⲽ������GetSqlSummaryColInfor��ORDER BY OperatorTypeType��Ӧ
					if( _SummaryTypeInfors[ col.Name ] == null )
						_SummaryTypeInfors.Add( col );
				}
			}

			_IsFilledData = true;
		}

		#region Private Class: DataColumnInfor

		private class DataColumnInfor
		{
			public DataColumnInfor(){}

			public bool		IsPercentColumn			= false; //	�жϵ�ǰ���Ƿ��ǰٷֱ�
			public bool		IsPercentGroupColumn	= false; //	�жϵ�ǰ���Ƿ��Ƿ���ٷֱ�
			public bool		IsSummaryColumn			= false;

			public string Name			= string.Empty;
			public string Description	= string.Empty;

			public int		Precision	= -1;

			private string _FormatEx	= string.Empty;
			public string FormatEx
			{
				get{ return _FormatEx; }
				set
				{
					_FormatEx = value;

					//	����PERCENT��PERCENTGROUPʱ��ʾ��Ϊ�ٷֱȻ����ٷֱ�
					switch( _FormatEx.ToUpper() )
					{
						case "PERCENT":
							IsPercentColumn = true;
							break;
						case "PERCENTGROUP":
							IsPercentGroupColumn = true;
							break;
						case "MONEY":
							Precision = 9;
							break;
						default:
							break;
					}
				}
			}

			// 861�ĺϼ�����ָʾ���������:
			// 1.ModeEx=0ʱNoteֵ����Ϊavg,min,max,sum
			// 2.ModeEx=4ʱOrderEx��ֵ����Ϊ-1,0,1,2
			// ����Ϊ870�ĺϼ�ö��
			//public enum OperatorType
			//{
			//    SUM			= 0,
			//    AVG			= 1,
			//    MAX			= 2,
			//    MIN			= 3,
			//    BalanceSUM	= 4,
			//    ExpressionSUM	= 5,
			//    AccumulateSUM	= 6
			//}
			private string _OperatorType = "0";
			public string OperatorType
			{
				get{ return _OperatorType; }
				set
				{
					IsSummaryColumn	= true;
					switch( value.ToUpper() )
					{
						case "AVG":
							_OperatorType = "1";
							break;
						case "MIN":
							_OperatorType = "3";
							break;
						case "MAX":
							_OperatorType = "2";
							break;
						case "SUM":
						case "0":
							_OperatorType = "0";
							break;
						case "1":
							_OperatorType = "5";
							break;
						case "2":
							_OperatorType = "4";
							break;
						default:
							IsSummaryColumn	= false;
							break;
					}
				}
			}
		}

		private class DataColumnInforCollection : CollectionBase
		{
			private DataColumnComparer compare = new DataColumnComparer();

			public DataColumnInfor this[ int index ] 
			{
				get{ return ( ( DataColumnInfor ) List[index] ); }
			}

			public DataColumnInfor this[ string columnKey ] 
			{
				get
				{ 
					for( int i=0; i < this.Count; i++ )
						if( this[i].Name == columnKey )
							return this[i];

					return null; 
				}
			}

			public void Add( DataColumnInfor dataColumnInfor )
			{
				List.Add( dataColumnInfor );
			}

			public void Sort()
			{
				this.InnerList.Sort( compare );
			}

			// ʹ�ü��ϰ�DataColumnInfor.Description�ĳ��ȷ�������
			// Ŀ��Ϊ�˶��б��ʽ��������ȷ���滻�������ɵ������б��ʽ
			// ������������Դ��Description������Name
			private class DataColumnComparer : IComparer
			{
				public int Compare(object x, object y)
				{
					DataColumnInfor xX = x as DataColumnInfor;
					DataColumnInfor yY = y as DataColumnInfor;
					if( xX != null && yY != null )
					{
						if( xX.Description.Length < yY.Description.Length )
							return 1;
						else if( xX.Description.Length > yY.Description.Length )
							return -1;
					}

					return 0;
				}
			}
		}

		#endregion

		#endregion
	}
}
