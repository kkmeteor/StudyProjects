using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
   [Serializable]
    public class ExpandProgram
    {
       U8LoginInfor _login = null;      


       public ExpandProgram(object u8login)
       {
           _login = new U8LoginInfor(u8login);
       }      

       public ExpandProgram(U8LoginInfor u8login)
       {
           _login = u8login;
       }

       public string RetriveItemCaption(int levelExpandValue)
       {
           string str = string.Empty;
           string sql = string.Empty;
           if (_login.LocaleID.ToLower() == "zh-cn")
               sql = string.Format(@"select typeCaptionCN as typeCaption from uap_report_Expandrule where isShow=1 and typeID={0}",levelExpandValue);
           if (_login.LocaleID.ToLower() == "zh-tw")
               sql = string.Format(@"select typeCaptiontw as typeCaption from uap_report_Expandrule where isShow=1 and typeID={0}",levelExpandValue);
           if (_login.LocaleID.ToLower() == "en-us")
               sql = string.Format(@"select typeCaptionen as typeCaption from uap_report_Expandrule where isShow=1 and typeID={0}", levelExpandValue);
           str = SqlHelper.ExecuteScalar(_login.UfMetaCnnString, sql).ToString() ;
              return str;       
       }

       public string RetriveRuleSQL(int levelExpandValue)
       {
           string str = string.Empty;
           str = Convert.ToString(SqlHelper.ExecuteScalar(_login.UfMetaCnnString,
               string.Format(@"select ruleSQL from uap_report_Expandrule 
                                where isShow=1 and typeID={0}", levelExpandValue)));
           return str;     
       
       }

       public string RetriveDifinitionSQL(int levelExpandValue)
       {
           string str = string.Empty;
           str = SqlHelper.ExecuteScalar(_login.UfMetaCnnString,
               string.Format(@"select difinitionSQL from uap_report_Expandrule 
                                where isShow=1 and typeID={0}", levelExpandValue)).ToString();
           return str;        
       }

       public SqlDataReader RetriveItemCaptions()
       { 
         string currentLocale = "zh-CN";
            if (!string.IsNullOrEmpty( _login.LocaleID))
            {
                currentLocale = _login.LocaleID;
            }
            string sql = "";
            if (currentLocale.ToLower() == "zh-cn")
                sql = "select typeID,typeCaptionCN as typeCaption from uap_report_Expandrule where isShow=1";
            if (currentLocale.ToLower() == "zh-tw")
                sql = "select typeID,typeCaptiontw as typeCaption from uap_report_Expandrule where isShow=1";
            if (currentLocale.ToLower() == "en-us")
                sql = "select typeID,typeCaptionen as typeCaption from uap_report_Expandrule where isShow=1";
            SqlDataReader dr = SqlHelper.ExecuteReader(_login.UfMetaCnnString, sql);
            return dr;       
       }
    }
}
