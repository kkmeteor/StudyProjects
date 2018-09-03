using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Data.SqlClient;

using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class LevelExpandItem
    {
        private int _depth ;
        private LevelExpandEnum _expandType;
        private string _columnName = string.Empty;
        private string _codeRule = string.Empty;
        private int    _factDepth = 0;
        ExpandProgram _expandProgram;

        
        public LevelExpandItem()
        {

        }

        public LevelExpandItem(LevelExpandEnum expandType)
        {
            this._expandType = expandType;
        }

        public int Depth
        {
            get
            {
                return _depth;
            }
            set
            {
                _depth = value;
            }
        }

        public LevelExpandEnum ExpandType
        {
            get
            {
                return _expandType;
            }
            set
            {
                _expandType = value;
            }
        }

        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set
            {
                _columnName = value;
            }
        }

        public int FactDepth
        {
            get
            {
                return this._factDepth;
            }
        }

        public string CodeRule
        {
            get
            {
                return this._codeRule;
            }
        }

       
        public void Retrieve( U8LoginInfor login )
        {
            RetrieveFactDepth(login);
            RetrieveCodeRule(login);
        }

        public int RetrieveFactDepth(U8LoginInfor login)
        {
            if (_factDepth <= 0)
            {
                string codeRule = this.RetrieveCodeRule(login);
                if (this._depth > codeRule.Length)
                    _factDepth = codeRule.Length;
                else
                    _factDepth = this._depth;
            }
            return _factDepth;
        }

        

        public string RetrieveCodeRule( U8LoginInfor login)
        {
            if (this._codeRule == string.Empty)
            {
                string strSQL = string.Empty;
                DataSet ds = null;

                if (this._expandType == LevelExpandEnum.AreaClass )
                {
                    strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cAreaClass'";
                }
                else if (this._expandType == LevelExpandEnum.CustClass)
                {
                    strSQL = "select  cValue  from  accinformation where  csysid ='aa' and cname ='cCustClass'";
                }
                else if (this._expandType == LevelExpandEnum.DepLevel)
                {
                    strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cDepLevel'";
                }
                else if (this._expandType == LevelExpandEnum.DispLevel)
                {
                    strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cDispLevel'";
                }
                else if (this._expandType == LevelExpandEnum.GoodClass)
                {
                    strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cGoodClass'";
                }
                else if (this._expandType == LevelExpandEnum.GradeLevel)
                {
                    strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cGradeLevel'";
                }
                else if (this._expandType == LevelExpandEnum.PosLevel)
                {
                    strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cPosLevel'";
                }
                else if (this._expandType == LevelExpandEnum.ProvClass)
                {
                    strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cProvClass'";
                }
                else if (this._expandType == LevelExpandEnum.SettleLevel)
                {
                    strSQL = "select cValue from accinformation where  csysid ='aa' and cname ='cSettleLevel'";
                }
                else if (this._expandType == LevelExpandEnum.fa_SourceClass)//资金构成分类
                {
                    strSQL = "select CODINGRULE cValue from GradeDef_base where keyword='fa_SourceClass'";
                }
                //string strConn = "user id=sa; password=1; database=UFDATA_777_2005; server=zblpwin;";
                // ds = SqlHelper.ExecuteDataSet(strConn, strSQL);
                else
                {
                    _expandProgram = new ExpandProgram(login);
                    strSQL = _expandProgram.RetriveRuleSQL(Convert.ToInt32(_expandType));
                    if (string.IsNullOrEmpty(strSQL))
                        //throw new Exception("No Difinition!"); 
                        return "";
                }
                ds = SqlHelper.ExecuteDataSet(login.UfDataCnnString, strSQL);
                if (ds.Tables[0].Rows.Count == 0)//资金构成分类
                {
                    return "";
                }
                this._codeRule = ds.Tables[0].Rows[0][0].ToString();
            }
            return this._codeRule;
        }


        protected LevelExpandItem(SerializationInfo info, StreamingContext context)
　　    {
          this._depth = (int)info.GetValue("Depth", this._depth.GetType());
          this._expandType = (LevelExpandEnum)info.GetValue("ExpandType", Type.GetType("System.Int32"));
          this._columnName = (string)info.GetValue("ColumnName", this._columnName.GetType());
　　    }
　　
        public virtual void GetObjectData(SerializationInfo info,StreamingContext context)
        {
            info.AddValue("Depth", this._depth, this._depth.GetType());
            info.AddValue("ExpandType", this._expandType, Type.GetType("System.Int32"));
            info.AddValue("ColumnName", this._columnName, this._columnName.GetType());
        }

    }

    public class RuntimeLevelExpandSrv : ILevelExpandTempDBGetDataService
    {
        private ReportLevelExpand _levelexpand;
        private DataHelper _datahelper;

        public RuntimeLevelExpandSrv(ReportLevelExpand levelexpand,DataHelper datahelper)
        {
            _levelexpand = levelexpand;
            _datahelper = datahelper;
        }
        #region ILevelExpandTempDBGetDataService 成员

        public void GetData(System.Collections.Hashtable columnInfo, System.Data.SqlClient.SqlDataReader reader, DataRow dr)
        {
            foreach (string key in columnInfo.Keys)
            {
                string basecolumn = columnInfo[key].ToString();
                string basevalue = reader[basecolumn].ToString();
                dr[basecolumn] = basevalue;
                int index = key.LastIndexOf("_");
                int depth = Convert.ToInt32(key.Substring(index+1));
                LevelExpandItem item = _levelexpand.LevelExpandItems[basecolumn ];
                dr[key] = _datahelper.ExpandData(basevalue , item, depth);
            }
        }

        #endregion
    }
}
