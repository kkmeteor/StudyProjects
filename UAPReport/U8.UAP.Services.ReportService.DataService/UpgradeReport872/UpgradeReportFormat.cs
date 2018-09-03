/*
 * ����:�ż���
 * ʱ��:2008.3.17
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.BizDAE.Elements;
using UFIDA.U8.UAP.Services.BizDAE.DBServices.QueryServices;
using UFIDA.U8.UAP.Services.BizDAE.ConfigureServices;

namespace UFIDA.U8.UAP.Services.ReportData
{
    class UpgradeReportFormat
    {
        private string _datastring;
        private string _metastring;
        private string _reportid;
        public UpgradeReportFormat(string datastring,string metastring, string reportid)
        {
            _datastring = datastring;
            _metastring = metastring;
            _reportid = reportid;
        }
        public bool Upgrade()
        {
            ColumnCollection oldcolumns=null;
            BusinessObject BO = null;
            object dsid=SqlHelper.ExecuteScalar(_metastring, "select DataSourceID from uap_report where id='"+_reportid+"'");
            if ( dsid!= null)
            {
                oldcolumns=((QueryFunction)BO.Functions[0]).QuerySettings[0].QueryResultTable.Columns;
                //((QueryFunction)BO.Functions[0]).QuerySettings[0].QueryResultTable.Columns=new ColumnCollection();
            }
            else
            {
                //new bo
            }
            
            ColumnCollection newcolumns=((QueryFunction)BO.Functions[0]).QuerySettings[0].QueryResultTable.Columns;
            //��ȡ�ϱ�������,�����е�˳��
            SqlDataReader reader = SqlHelper.ExecuteReader(_datastring, "select ,, from rpt order by orderid");
            while (reader.Read())
            {
                string columnname=reader["ColumnName"].ToString();
                TableColumn column=AlreadyInColumns(columnname,oldcolumns);
                if(column!=null)
                {
                    newcolumns.Add(column);
                }
                else//��������ӽ�ȥ
                {
                    DataTypeEnum dt=GetDataType();
                    string desc=null;
                    column=new TableColumn(columnname,dt,desc);
                    newcolumns.Add(column);
                }
            }
            ConfigureServiceProxy c = new ConfigureServiceProxy();
            c.UpdateBusinessObject(BO);

            UpgradeReport ur=new UpgradeReport();
            ur.DataSourceID = BO.MetaID;
            
           
            //����BusinessObject���󣬲�����Save����
            //����Report���󣬲�����Save����
            //reportengine
            //Report report=reportengine.DesignReport()
            //reportengine.UpgradeSave(report,ref commonxml,ref cnxml,ref twxml,ref enxml);
            //if rpt_id��meta����UAP_Report������
            //BusinessObject BO = new BusinessObject();
            //else rpt_id��meta����UAP_Report�����
            //ConfigureServiceProxy proxy =new ConfigureServiceProxy();
            //proxy.GetBusinessObject()

            ur.Save();
            return true;
        }

        private TableColumn AlreadyInColumns(string name,ColumnCollection columns)
        {
            if (columns == null)
                return null;
            foreach (TableColumn column in columns)
            {
                if (column.Name.ToLower() == name.ToLower())
                    return column ;
            }
            return null;
        }

        private DataTypeEnum GetDataType()
        {
            return DataTypeEnum.String;
        }
    }
}

