using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Data;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    internal class CachePool
    {
        private NameDataSetCollection dataSetObjects = null;
        private U8LoginInfor U8LoginInfor = null;
        ExpandProgram _expandProgram = null;

        public CachePool( U8LoginInfor loginInfo )
        {
            dataSetObjects = new NameDataSetCollection();
            this.U8LoginInfor = loginInfo;
        }

        public LevelExpandDataTable this[LevelExpandEnum levelExpand]
        {
            get
            {
                if (!this.dataSetObjects.Contains(levelExpand))
                    this.Create(levelExpand );
                return this.dataSetObjects[levelExpand];
            }
        }

        private void Create( LevelExpandEnum levelExpand )
        {
  
            string strSQL = string.Empty;
        
            DataSet ds = null;

            if (levelExpand == LevelExpandEnum.AreaClass)
            {
                strSQL = "Select  cDCCode as cCode, cDCName  as 地区 from  DistrictClass";
            }
            else if (levelExpand == LevelExpandEnum.CustClass)
            {
                strSQL = "select  cCCCode as cCode , cCCName as 客户分类 from  CustomerClass";
            }
            else if (levelExpand == LevelExpandEnum.DepLevel)
            {
                strSQL = "Select cDepCode as cCode, cDepName as cname from Department";
            }
            else if (levelExpand == LevelExpandEnum.DispLevel)
            {
                strSQL = "Select  cRdCode as cCode,cRdName as cname  from  Rd_Style";
            }
            else if (levelExpand == LevelExpandEnum.GoodClass)
            {
                strSQL = "Select  cInvCCode as cCode,cInvCname as cname from inventoryClass";
            }
            else if (levelExpand == LevelExpandEnum.GradeLevel)
            {
                strSQL = "Select  cCode as cCode, cCode_name  as cname from  Code";
            }
            else if (levelExpand == LevelExpandEnum.PosLevel)
            {
                strSQL = "Select  cPosCode as cCode, cPosName  as cname  from  Position";
            }
            else if (levelExpand == LevelExpandEnum.ProvClass)
            {
                strSQL = "Select  cVCCode as cCode, cVCName as cname from  vendorClass";
            }
            else if (levelExpand == LevelExpandEnum.SettleLevel)
            {
                strSQL = "Select  cSSCode as cCode, cSSName  as  cname from  SettleStyle";
            }
            else if (levelExpand == LevelExpandEnum.fa_SourceClass)//资金构成分类
            {
                strSQL = "select sCCode as cCode,sCName  as  cname from fa_SourceClass";
            }
            else
            {
                _expandProgram = new ExpandProgram(U8LoginInfor);
                strSQL = _expandProgram.RetriveDifinitionSQL(Convert.ToInt32(levelExpand));
            }
            ds = SqlHelper.ExecuteDataSet( this.U8LoginInfor.UfDataCnnString, strSQL);
           
            LevelExpandDataTable leds = new LevelExpandDataTable( ds.Tables[0] );

            dataSetObjects.Add( levelExpand, leds );
        }
    }
}
