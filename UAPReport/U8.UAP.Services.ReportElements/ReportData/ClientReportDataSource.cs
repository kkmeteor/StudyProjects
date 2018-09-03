using System;
using UFIDA.U8.UAP.Services.BizDAE.Elements;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ClientReportDataSource 的摘要说明。
	/// </summary>
	public class ClientReportDataSource:ReportDataSource
	{
		private static DataSources _dss;
		private ColumnCollection _columncollection;
		public ClientReportDataSource(ColumnCollection datasource):base(datasource)
		{
			_columncollection=datasource;
            _understate = ReportStates.Designtime;
		}

        public ClientReportDataSource(DataSources dss)
            : base(dss)
        {
            _understate = ReportStates.Designtime;
        }

		public override void GenerateDataSources(ColumnCollection datasource)
		{
			base.GenerateDataSources (datasource);
			_dss=_datasources;
		}

		public void ReSetDataSource()
		{
			GenerateDataSources(_columncollection);
		}

		public ColumnCollection ColumnCollection
		{
			get
			{
				return _columncollection;
			}
		}
		public DataSources DSS
		{
			get
			{
				return _dss;
			}
		}

        public bool Contains(string name)
        {
            return _dss.Contains(name);
        }
	}
}
