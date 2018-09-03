using System;
using System.Collections.Generic;
using System.Text;

using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class LevelExpandSrv
    {
        private CachePool       CachePool = null;
        private U8LoginInfor    u8LoginInfor = null;
        private ExpandProgram _expandProgram = null;

        public LevelExpandSrv( U8LoginInfor loginInfo )
        {
            this.CachePool = new CachePool( loginInfo );
            this.u8LoginInfor = loginInfo;
        }

        public LevelExpandData GetLevelExpandData( string codeValue,LevelExpandItem levelExpandItem )
        {
            LevelExpandData data = new LevelExpandData(this.CachePool[levelExpandItem.ExpandType], levelExpandItem);
            data.FilterCode = codeValue;
            levelExpandItem.Retrieve( this.u8LoginInfor );
           
            return data;
        }

        public int GetFactLevelExpandDepth( LevelExpandItem levelExpandItem )
        {
            return levelExpandItem.RetrieveFactDepth(this.u8LoginInfor );
        }

        public string GetLevelItemCaption( LevelExpandEnum levelExpandType )
        {
           // string localeId = this.U8LoginInfor.LocaleID;
            string caption = string.Empty;

            if ( levelExpandType==LevelExpandEnum.AreaClass )
                caption = "U8.SO.ReportServer.clssetdataprovider.00086";
            else if (levelExpandType == LevelExpandEnum.CustClass)
                caption = "U8.GL.ZWSQL.CODEACC.00766";
            else if (levelExpandType == LevelExpandEnum.DepLevel)
                caption = "U8.BPM.DSS.U8GDP.SaleOrderLabDept001";
            else if (levelExpandType == LevelExpandEnum.DispLevel)
                caption = "U8.CW.GL.ZZTool.frmPzYr.MenuPop.menuSFLB.Caption";
            else if (levelExpandType == LevelExpandEnum.GoodClass)
                caption = "U8.AA.ARCHIVE.FIELD.cinvcls";
            else if (levelExpandType == LevelExpandEnum.GradeLevel)
                caption = "U8.AA.ARCHIVE.code";
            else if (levelExpandType == LevelExpandEnum.PosLevel)
                caption = "U8.AA.ARCHIVE.FIELD.ref_cposcode";
            else if (levelExpandType == LevelExpandEnum.ProvClass)
                caption = "U8.AA.ARCHIVE.FIELD.cvencls";
            else if (levelExpandType == LevelExpandEnum.SettleLevel)
                caption = "U8.AA.EAI.XML.Operation.Dir.0014";
            else if (levelExpandType == LevelExpandEnum.fa_SourceClass)//资金构成分类
                caption = "U8.UAP.Services.ReportExhibition.ReportExpandWindow.资金构成分类";
            else
            {
                _expandProgram = new ExpandProgram(u8LoginInfor);
                caption = _expandProgram.RetriveItemCaption(Convert.ToInt32(levelExpandType));
            }
           
            return caption;
            
        }

        /*
        public int GetFactLevelExpandDepth( LevelExpandItem levelExpandItem )
        {
            int depth = 0;
            DataSet ds = new DataSet();

            if (levelExpand == LevelExpandEnum.Customer)
            {
                strSQL = "select  cValue  from  accinformation where  csysid ='aa' and cname ='cCustClass'";
            }
            else if (levelExpand == LevelExpandEnum.Department)
            {
                strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cDepLevel'";
            }
            SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn);
            adapter.Fill(ds);
            conn.Close();

            string levelRule = ds.Tables[0].Rows[0][0].ToString();

            depth = levelRule.Length;

            if (levelExpandItem.Depth < depth)
                depth = levelExpandItem.Depth;

            return depth;
        }
         */
    }
}
