using System;
using System.Data;
using System.Collections;
using System.Xml;
using System.Windows.Forms;
using UFIDA.U8.UAP.Services.BizDAE.Elements;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ReportDataSource 的摘要说明。
	/// </summary>
	public class ReportDataSource:IDisposable,ICloneable
	{	
        //protected string _datasourceid;
        //protected string _datasourceidextended;
		protected string _functionname;
		protected DataSources _datasources;
        protected ReportStates _understate=ReportStates.Designtime ;

        public ReportDataSource()
        {
            _datasources = new DataSources();
        }

		public ReportDataSource(DataSources datasources):this()
		{
			_datasources=datasources;            
		}
		public ReportDataSource(ColumnCollection datasource):this()
		{
			_datasources=new DataSources();
			if(_understate !=ReportStates.Static
				&& datasource!=null)
			{
				GenerateDataSources(datasource);
			}
		}

        public ReportDataSource(ReportDataSource rds)
        {
            _functionname = rds.FunctionName;
            _understate = rds.UnderState;
            _datasources = (DataSources)rds.DataSources.Clone();
        }

        public ReportDataSource(XmlNode dscommon, XmlNode dslocale):this()
        {
            foreach (XmlElement node in dscommon.ChildNodes)
            {
                DataSource ds = new DataSource();
                ds.Name = node.GetAttribute("Name");
                ds.Type = GetDataType(node.GetAttribute("Type"));
                _datasources.Add(ds);
            }
            foreach (XmlElement node in dslocale.ChildNodes)
            {
                DataSource ds = _datasources[node.GetAttribute("Name")];
                if (ds != null)
                    ds.Caption = node.GetAttribute("Caption");
            }
        }

        //public string DataSourceIDExtended
        //{
        //    get
        //    {
        //        return _datasourceidextended;
        //    }
        //    set
        //    {
        //        _datasourceidextended=value;
        //    }
        //}

        //public string DataSourceID
        //{
        //    get
        //    {
        //        return _datasourceid;
        //    }
        //    set
        //    {
        //        _datasourceid=value;
        //    }
        //}
		public string FunctionName
		{
			get
			{
				return _functionname;
			}
			set
			{
				_functionname=value;
			}
		}
        public ReportStates UnderState
        {
            get
            {
                return _understate;
            }
            set
            {
                _understate = value;
            }
        }

		public virtual void GenerateDataSources(ColumnCollection datasource)
		{
			_datasources.Clear();

            if (_understate == ReportStates.Designtime)
                _datasources.InitDesignKeys();

			DataSource ds;
			for(int i=0;i<datasource.Count;i++)
			{
				TableColumn tc=datasource[i];
				ds=new DataSource();
				ds.Name=tc.Name.Trim();
                //if(tc.Description==null || tc.Description.Trim()=="")
                //    ds.Caption=ds.Name;
                //else
                //    ds.Caption=tc.Description;
                ds.CNCaption = string.IsNullOrEmpty(tc.DescriptionCN)?ds.Name:tc.DescriptionCN ;
                ds.TWCaption = tc.DescriptionTW;
                ds.ENCaption = tc.DescriptionUS;
				ds.Type=GetDataType(tc.DataType);

                if (tc is ERQueryResultColumn && (tc as ERQueryResultColumn).bDEProperty)
                    ds.bDimension = true;
				_datasources.Add(ds);
			}
		}

        public Type ColumnType(string name)
        {
            DataSource ds = _datasources[name];
            if (ds == null)
                return typeof(string);
            if (ds.Type == DataType.DateTime)
                return typeof(DateTime);
            else if (ds.Type == DataType.Decimal || ds.Type == DataType.Currency || ds.Type == DataType.Int)
                return typeof(double);
            else
                return typeof(string);
        }

		private DataType GetDataType(string dt)
		{
			switch(dt.ToLower())
			{
				case "decimal":
				case "int":
					return DataType.Decimal;
				case "datetime":
					return DataType.DateTime;
				default:
					return DataType.String;
			}
		}

		private DataType GetDataType(DataTypeEnum dt)
		{
			switch(dt)
			{
				case DataTypeEnum.Int16:
				case DataTypeEnum.Int32:
				case DataTypeEnum.Int64:
                    return DataType.Int;
                case DataTypeEnum.Decimal:
                case DataTypeEnum.Double:
                case DataTypeEnum.Single:
					return DataType.Decimal;
				case DataTypeEnum.DateTime:
					return DataType.DateTime;
                case DataTypeEnum.Boolean:
                    return DataType.Boolean;
                case DataTypeEnum.Object:
                    return DataType.Image;
                case DataTypeEnum.Text:
                    return DataType.Text;
				default:
					return DataType.String;
			}
		}

        public DataSources DataSources
		{
			get
			{
				return _datasources;
			}
		}

        public void AddColumn(IMapName map)
        {
            if (!(map is IDataSource ) && !_datasources.Contains(map.MapName.Trim()))
            {
                DataType dt = DataType.String;
                if (map is IDecimal)
                    dt = DataType.Decimal;
                else if (map is IDateTime || ((map is IBDateTime ) && (map as IBDateTime ).bDateTime ))
                    dt = DataType.DateTime;
                else if (map is ICalculateColumn && !ExpressionService.bExpressionReallyNotADecimalType ((map as ICalculateColumn).Expression,this.DataSources ))
                    dt = DataType.Decimal;

                //DataSource ds = new DataSource(map.MapName.Trim(), dt);
                //ds.bAppend  = true;
                //if (map is ICalculateColumn && map.MapName.ToLower() != (map as ICalculateColumn).Expression.ToLower())
                //    ds.Tag = (map as ICalculateColumn).Expression;
                //_datasources.Add(ds);
                if (!(map is ICalculateColumn) || map.MapName.ToLower() != (map as ICalculateColumn).Expression.ToLower())
                {
                    DataSource ds = new DataSource(map.MapName.Trim(), dt);
                    ds.bAppend = true;
                    if (map is ICalculateColumn && map.MapName.ToLower() != (map as ICalculateColumn).Expression.ToLower())
                    {
                        ds.Tag = (map as ICalculateColumn).Expression;
                        if (ds.Tag.Contains(@"/"))
                            ds.Tag = "isnull("+ds.Tag +",0)";
                    }
                    _datasources.Add(ds);
                }
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (_datasources != null)
            {
                _datasources.Dispose();
                _datasources = null;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new ReportDataSource(this);
        }

        #endregion
    }
}
