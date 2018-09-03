using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UFIDA.U8.UAP.Services.ReportData;


namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class DataHelper : ISerializable, IDisposable
    {
        #region fields
        private U8LoginInfor _login;
        private Hashtable _htprecision;
        private ReportStates _understate;
        private Hashtable _htcompany;
        private Hashtable _htcusdefine;
        private LevelExpandSrv _levelexpandsrv;
        private Hashtable _htcusname;

        private string _username = "";
        private int _year;
        private int _month;
        private int _day;
        private int _accountmonth;
        private int _accountyear;
        private string _date;
        private string _time;

        private ITest _global;
        #endregion

        #region constructor
        public DataHelper(U8LoginInfor login)
        {
            _login = login;
            _year = _login.Year;
            _month = _login.Month;
            _day = _login.Day;
            _date = _login.Date;
            _time = _login.Time;
            _accountmonth = _login.AccountMonth;
            _accountyear = _login.AccountYear;
            _levelexpandsrv = new LevelExpandSrv(_login);
        }

        public DataHelper(U8LoginInfor login, ReportStates understate)
            : this(login)
        {
            _understate = understate;
        }

        protected DataHelper(SerializationInfo info, StreamingContext context)
        {
            _htprecision = (Hashtable)info.GetValue("Precision", typeof(Hashtable));
            _htcompany = (Hashtable)info.GetValue("Company", typeof(Hashtable));
            _htcusdefine = (Hashtable)info.GetValue("CusDefine", typeof(Hashtable));
            _htcusname = (Hashtable)info.GetValue("CusName", typeof(Hashtable));
            _understate = ReportStates.Static;

            _username = info.GetString("UserName");
            _year = info.GetInt32("Year");
            _month = info.GetInt32("Month");
            _day = info.GetInt32("Day");
            _accountmonth = info.GetInt32("AccountMonth");
            _accountyear = info.GetInt32("AccountYear");
            _date = info.GetString("Date");
            _time = info.GetString("Time");
        }
        #endregion

        #region precision
        public string Precisions(PrecisionType pt)
        {
            if (_login.SubID == "OutU8")
                return "-1";
            try
            {
                if (_htprecision == null || _htprecision.Count == 0)
                    SetPrecision();
                switch (pt)
                {
                    case PrecisionType.BillPrice:
                        return _htprecision["BillPrice"].ToString();
                    case PrecisionType.InventoryPrice:
                        return _htprecision["InventoryPrice"].ToString();
                    case PrecisionType.ExchangeRate:
                        return _htprecision["ExchangeRate"].ToString();
                    case PrecisionType.Money:
                        return _htprecision["Money"].ToString();
                    case PrecisionType.PieceNum:
                        return _htprecision["PieceNum"].ToString();
                    case PrecisionType.Quantity:
                        return _htprecision["Quantity"].ToString();
                    case PrecisionType.Rate:
                        return _htprecision["Rate"].ToString();
                    case PrecisionType.TaxRate:
                        return _htprecision["TaxRate"].ToString();
                    case PrecisionType.Volume:
                        return _htprecision["Volume"].ToString();
                    case PrecisionType.Weight:
                        return _htprecision["Weight"].ToString();
                    case PrecisionType.CostMoney:
                        return _htprecision["CostMoney"].ToString();
                    case PrecisionType.CostQuantity:
                        return _htprecision["CostQuantity"].ToString();
                    case PrecisionType.GLQuantity:
                        return _htprecision["GLQuantity"].ToString();
                    case PrecisionType.GLPrice:
                        return _htprecision["GLPrice"].ToString();
                    case PrecisionType.Source:
                        return "-1";
                    default:
                        return "2";
                }
            }
            catch
            {
                return "2";
            }
        }

        private void SetPrecision()
        {
            try
            {
                _htprecision = new Hashtable();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("select cname,case when cvalue is null then cdefault else cvalue end as cvalue from accinformation with (nolock) where cName='iStrsPriDecDgt' or cName='iStrsQuanDecDgt' or cName='iNumDecDgt' or cName='iExchRateDecDgt' or cname='iRateDecDgt' or cname='iBillPrice' or cname='iWeightDecDgt' or cname='iVolumeDecDgt' or cname='iQuanDecDgt' or cname='iPriceDecDgt';");
                sb.Append("select cid,cvalue from accinformation with (nolock) where csysid='CA' and (cid='15' or cid='16')");
                DataSet ds = Exec(sb.ToString());
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i]["cname"].ToString() == "iStrsPriDecDgt")
                        _htprecision.Add("InventoryPrice", ds.Tables[0].Rows[i]["cvalue"]);
                    else if (ds.Tables[0].Rows[i]["cname"].ToString() == "iBillPrice")
                        _htprecision.Add("BillPrice", ds.Tables[0].Rows[i]["cvalue"]);
                    else if (ds.Tables[0].Rows[i]["cname"].ToString() == "iStrsQuanDecDgt")
                        _htprecision.Add("Quantity", ds.Tables[0].Rows[i]["cvalue"]);
                    else if (ds.Tables[0].Rows[i]["cname"].ToString() == "iNumDecDgt")
                        _htprecision.Add("PieceNum", ds.Tables[0].Rows[i]["cvalue"]);
                    else if (ds.Tables[0].Rows[i]["cname"].ToString() == "iExchRateDecDgt")
                        _htprecision.Add("ExchangeRate", ds.Tables[0].Rows[i]["cvalue"]);
                    else if (ds.Tables[0].Rows[i]["cname"].ToString() == "iWeightDecDgt")
                        _htprecision.Add("Weight", ds.Tables[0].Rows[i]["cvalue"]);
                    else if (ds.Tables[0].Rows[i]["cname"].ToString() == "iVolumeDecDgt")
                        _htprecision.Add("Volume", ds.Tables[0].Rows[i]["cvalue"]);
                    else if (ds.Tables[0].Rows[i]["cname"].ToString() == "iQuanDecDgt")
                        _htprecision.Add("GLQuantity", ds.Tables[0].Rows[i]["cvalue"]);
                    else if (ds.Tables[0].Rows[i]["cname"].ToString() == "iPriceDecDgt")
                        _htprecision.Add("GLPrice", ds.Tables[0].Rows[i]["cvalue"]);
                    else //iRateDecDgt
                        _htprecision.Add("TaxRate", ds.Tables[0].Rows[i]["cvalue"]);
                }
                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                {
                    if (ds.Tables[1].Rows[i]["cid"].ToString() == "15")
                        _htprecision.Add("CostMoney", ds.Tables[1].Rows[i]["cvalue"]);
                    else if (ds.Tables[1].Rows[i]["cid"].ToString() == "16")
                        _htprecision.Add("CostQuantity", ds.Tables[1].Rows[i]["cvalue"]);
                }
                _htprecision.Add("Money", 2);
                //_htprecision.Add("Weight", 6);
                //_htprecision.Add("Volume", 6);
                _htprecision.Add("Rate", 4);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("err when get precision info: " + ex.Message);
            }
        }
        #endregion

        #region companyinfo
        public string CompanyInfo(string key)
        {
            if (_login.SubID == "OutU8")
                return "";
            if (_htcompany == null || _htcompany.Count == 0)
                GetCompanyInfo();
            if (_htcompany.Contains(key.ToLower()))
                return _htcompany[key.ToLower()].ToString();
            else
                return "";
        }

        private void GetCompanyInfo()
        {
            try
            {
                _htcompany = new Hashtable();
                DataSet ds = Exec("select top 1 cunitname, cunitaddr,cunitzap, cunitnameen,cunitaddress1en,cunitaddress2en,cunitaddress3en,cunitaddress4en,cunittel,cunitfax,cunitemail,cunittaxno from ufsystem..ua_account where cacc_id = substring(db_name(),8,3)");

                DataRow dr = ds.Tables[0].Rows[0];
                _htcompany.Add("unit name", dr["cunitname"].ToString());
                _htcompany.Add("unit address", dr["cunitaddr"].ToString());
                _htcompany.Add("unit zip code", dr["cunitzap"].ToString());
                _htcompany.Add("unit name in english", dr["cunitnameen"].ToString());
                _htcompany.Add("unit address in english part one", dr["cunitaddress1en"].ToString());
                _htcompany.Add("unit address in english part two", dr["cunitaddress2en"].ToString());
                _htcompany.Add("unit address in english part three", dr["cunitaddress3en"].ToString());
                _htcompany.Add("unit address in english part four", dr["cunitaddress4en"].ToString());
                _htcompany.Add("unit telephone", dr["cunittel"].ToString());
                _htcompany.Add("unit fax number", dr["cunitfax"].ToString());
                _htcompany.Add("unit email", dr["cunitemail"].ToString());
                _htcompany.Add("unit tax number", dr["cunittaxno"].ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("err when get company info: " + ex.Message);
            }
        }
        #endregion

        #region cusdefine
        public string CusDefineInfo(string caption, string key)
        {
            if (_login.SubID == "OutU8")
                return caption;
            string keycaption = CusDefineInfo(key);
            string headercaption = null;
            // 判断是否是供应商==
            if (caption.IndexOf("CCUSDEFINE", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "CCUSDEFINE";

            else if (caption.IndexOf("CVENDEFINE", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "CVENDEFINE";

            // 判断是否是存货自定义项
            else if (caption.IndexOf("CINVDEFINE", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "CINVDEFINE";

            // 判断是否是存货自由项
            else if (caption.IndexOf("CFREE", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "CFREE";
            // 判断是否是联系人
            else if (caption.IndexOf("CCONDEFINE", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "CCONDEFINE";

            // 判断是否是序列号属性
            else if (caption.IndexOf("CSNDEFINE", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "CSNDEFINE";

            // 判断是否批次属性cBatchProperty
            else if (caption.IndexOf("CBATCHPROPERTY", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "CBATCHPROPERTY";

            // 判断是否是单据头或单据体
            else if (caption.IndexOf("CDEFINE", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "CDEFINE";

            // 判断是否是单据头或单据体
            else if (caption.IndexOf("DDEFINE_", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "DDEFINE_";
            else if (caption.IndexOf("DEFINE_", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "DEFINE_";

            // 判断是否存货自定义项
            else if (caption.IndexOf("DINVDEFINE_", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "DINVDEFINE_";
            else if (caption.IndexOf("INVDEFINE_", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "INVDEFINE_";

            else if (caption.IndexOf("DINVFREE_", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "DINVFREE_";
            // 判断是否存货自由项
            else if (caption.IndexOf("INVFREE_", StringComparison.OrdinalIgnoreCase) != -1)
                headercaption = "INVFREE_";

            if (headercaption != null)
                return caption.Substring(0, caption.IndexOf(headercaption, StringComparison.OrdinalIgnoreCase)) + keycaption;
            else
                return string.IsNullOrEmpty(keycaption) ? caption : keycaption;

        }

        public string CusDefineInfo(string key)
        {
            if (_login.SubID == "OutU8")
                return "";
            if (_htcusdefine == null || _htcusdefine.Count == 0)
                GetCusDefine();
            if (_htcusdefine.Contains(key.ToLower()))
                return _htcusdefine[key.ToLower()].ToString();
            else
                return "";
        }

        private void GetCusDefine()
        {
            try
            {
                _htcusdefine = new Hashtable();
                DataSet ds = Exec("select cVer,cCaption from UA_Caption where cLocaleID='" + _login.LocaleID + "'");
                DataTable dt = ds.Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _htcusdefine.Add(dt.Rows[i]["cVer"].ToString().ToLower(), dt.Rows[i]["cCaption"]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("err when get cus define: " + ex.Message);
            }
        }

        #region userdefine service
        public string GetUserDefineCaption(string columnDataSourceExpression)
        {
            if (_login.SubID == "OutU8")
                return "";
            string userdefineheader = null;
            int index = GetUserDefineIndex(columnDataSourceExpression, ref userdefineheader);
            if (index == -1)
                return string.Empty;

            string userDefineKey = string.Empty;
            // 判断是否是客户
            if (userdefineheader.IndexOf("CCUSDEFINE") != -1)
                userDefineKey = "@客户.自定义项" + index.ToString();
            // 判断是否是客户扩展自定义项
            if (userdefineheader.IndexOf("CCDEFINE") != -1)
            {
                userDefineKey = "@客户.扩展自定义项" + index.ToString();
                return userDefineKey;
            }
            
            // 判断是否是供应商==
            if (userdefineheader.IndexOf("CVENDEFINE") != -1)
                userDefineKey = "@供应商.自定义项" + index.ToString();

            // 判断是否是存货自定义项
            else if (userdefineheader.IndexOf("CINVDEFINE") != -1)
                userDefineKey = "@存货.自定义项" + index.ToString();

            // 判断是否是存货自由项
            else if (userdefineheader.IndexOf("CFREE") != -1)
                userDefineKey = "@存货.自由项" + index.ToString();

            // 判断是否是联系人
            else if (userdefineheader.IndexOf("CCONDEFINE") != -1)
                userDefineKey = "@联系人.自定义项" + index.ToString();

            // 判断是否是序列号属性
            else if (userdefineheader.IndexOf("CSNDEFINE") != -1)
                userDefineKey = "@序列号.属性" + index.ToString();

            // 判断是否批次属性cBatchProperty
            else if (userdefineheader.IndexOf("CBATCHPROPERTY") != -1)
                userDefineKey = "@批次.属性" + index.ToString();

            // 判断是否是单据头或单据体
            else if (userdefineheader.IndexOf("CDEFINE") != -1)
            {
                if (index > 21)
                {
                    index -= 21;
                    userDefineKey = "@单据体.自定义项" + index.ToString();
                }
                else
                    userDefineKey = "@单据头.自定义项" + index.ToString();
            }
            #region _判断
            // 判断是否是单据头或单据体
            else if (userdefineheader.IndexOf("DDEFINE_") != -1
           || userdefineheader.IndexOf("DEFINE_") != -1)
            {
                if (index > 21)
                {
                    index -= 21;
                    userDefineKey = "@单据体.自定义项" + index.ToString();
                }
                else
                    userDefineKey = "@单据头.自定义项" + index.ToString();
            }

            // 判断是否存货自定义项
            else if (userdefineheader.IndexOf("INVDEFINE_") != -1
           || userdefineheader.IndexOf("DINVDEFINE_") != -1)
            {
                userDefineKey = "@存货.自定义项" + index.ToString();
            }

            // 判断是否存货自由项
            else if (userdefineheader.IndexOf("INVFREE_") != -1
           || userdefineheader.IndexOf("DINVFREE_") != -1)
            {
                userDefineKey = "@存货.自定义项" + index.ToString();
            }
            #endregion
            return userDefineKey;
        }

        private int GetUserDefineIndex(string expression, ref string userdefineheader)
        {
            if (expression.Length >= 2
                && char.IsDigit(expression, expression.Length - 1)
                && char.IsDigit(expression, expression.Length - 2))
            {
                userdefineheader = expression.Substring(0, expression.Length - 2).ToUpper();
                return System.Convert.ToInt32(expression.Substring(expression.Length - 2));
            }
            else if (expression.Length >= 1
                && char.IsDigit(expression, expression.Length - 1))
            {
                userdefineheader = expression.Substring(0, expression.Length - 1).ToUpper();
                return System.Convert.ToInt32(expression.Substring(expression.Length - 1));
            }

            return -1;
        }
        #endregion

        #endregion

        #region cusname

        public bool bNotCusNameOrHasCusName(string key)
        {
            if (_login.SubID == "OutU8")
                return true;
            if (_htcusname == null || _htcusname.Count == 0)
                GetCusName();
            if (!_htcusname.Contains(key.ToLower()) || _htcusname[key.ToLower()].ToString() != "")
                return true;
            else
                return false;
        }

        public bool bCusName(string name)
        {
            if (_login.SubID == "OutU8")
                return false;
            if (_htcusname == null || _htcusname.Count == 0)
                GetCusName();
            return _htcusname.Contains(name.ToLower());
        }

        private void GetCusName()
        {
            try
            {
                _htcusname = new Hashtable();
                DataSet ds = Exec("select distinct cdicdbname,citemname from userdef");
                DataTable dt = ds.Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _htcusname.Add(dt.Rows[i]["cdicdbname"].ToString().ToLower(), dt.Rows[i]["citemname"].ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("err when get cus name: " + ex.Message);
            }
        }
        #endregion

        #region resources
        public U8LoginInfor Login
        {
            get
            {
                return _login;
            }
        }

        public string LocaleID
        {
            get
            {
                return _login.LocaleID;
            }
        }

        public bool NotBeSummaryTitle(string caption)
        {
            return caption.ToLower() != SubTotal.ToLower() &&
                caption.ToLower() != Summary.ToLower() &&
                caption.ToLower() != Total.ToLower();
        }

        public string SubTotal
        {
            get
            {
                if (_login.LocaleID.ToLower() == "en-us")
                    return "Subtotal";
                else if (_login.LocaleID.ToLower() == "zh-tw")
                    return "小計";
                else
                    return "小计";
            }
        }

        public string Summary
        {
            get
            {
                if (_login.LocaleID.ToLower() == "en-us")
                    return "Summary";
                else if (_login.LocaleID.ToLower() == "zh-tw")
                    return "合計";
                else
                    return "合计";
            }
        }

        public string Total
        {
            get
            {
                if (_login.LocaleID.ToLower() == "en-us")
                    return "Total";
                else if (_login.LocaleID.ToLower() == "zh-tw")
                    return "總計";
                else
                    return "总计";
            }
        }
        #endregion

        #region other
        public string UserName
        {
            get
            {
                if (_login.SubID == "OutU8")
                    return "";
                try
                {
                    if (_username == null || _username == "")
                    {
                        DataSet ds = Exec("select cuser_name from UFSystem.dbo.UA_User where cuser_id='" + _login.UserID + "'");
                        _username = ds.Tables[0].Rows[0][0].ToString();
                    }
                    return _username;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("err when get get username: " + ex.Message);
                }
                return "";
            }
        }

        public string Date
        {
            get
            {
                return _date;
            }
        }

        public string Time
        {
            get
            {
                //return _time;
                return DateTime.Now.ToString();
            }
        }

        public int Year
        {
            get
            {
                return _year;
            }
        }

        public int Month
        {
            get
            {
                return _month;
            }
        }

        public int Day
        {
            get
            {
                return _day;
            }
        }

        public int AccountYear
        {
            get
            {
                return _accountyear;
            }
        }

        public int AccountMonth
        {
            get
            {
                return _accountmonth;
            }
        }

        public ITest Global
        {
            get
            {
                return _global;
            }
            set
            {
                _global = value;
            }
        }
        #endregion

        #region exec
        public void ExecuteNonQuery(string sql)
        {
            SqlHelper.ExecuteNonQuery(_login.UfDataCnnString, sql);
        }

        public object ExecuteScalar(string sql)
        {
            return SqlHelper.ExecuteScalar(_login.UfDataCnnString, sql);
        }

        public DataSet Exec(string sql)
        {
            DataSet ds = SqlHelper.ExecuteDataSet(_login.UfDataCnnString, sql);
            return ds;
        }

        public DataSet ExecFromMeta(string sql)
        {
            DataSet ds = SqlHelper.ExecuteDataSet(_login.UfMetaCnnString, sql);
            return ds;
        }

        public object ExecuteScalarFromMeta(string sql)
        {
            return SqlHelper.ExecuteScalar(_login.UfMetaCnnString, sql);
        }
        internal void ExecuteScalarFromMeta1(string strSql, List<SqlParameter> parameters)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = strSql;
                cmd.CommandType = CommandType.Text;
                foreach (var sqlParameter in parameters)
                {
                    cmd.Parameters.Add(sqlParameter);
                }
                SqlHelper.ExecuteNonQuery(_login.UfMetaCnnString, cmd);
            }
        }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", 1);
            info.AddValue("Precision", _htprecision);
            info.AddValue("Company", _htcompany);
            info.AddValue("CusDefine", _htcusdefine);
            info.AddValue("CusName", _htcusname);

            info.AddValue("UserName", _username);
            info.AddValue("Year", _year);
            info.AddValue("Month", _month);
            info.AddValue("Day", _day);
            info.AddValue("Date", _date);
            info.AddValue("Time", _time);
            info.AddValue("AccountYear", _accountyear);
            info.AddValue("AccountMonth", _accountmonth);
        }
        #endregion

        #region ToFile()/FromFile()
        public void ToFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
            }
            catch (SerializationException e)
            {
                throw e;
            }
            finally
            {
                fs.Close();
            }

        }

        public static DataHelper FromFile(string filename)
        {
            DataHelper datahelper = null;

            FileStream fs = new FileStream(filename, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                datahelper = (DataHelper)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                throw e;
            }
            finally
            {
                fs.Close();
            }
            return datahelper;
        }
        #endregion

        #region expand data
        public string ExpandData(string codeValue, LevelExpandItem levelExpandItem, int depth)
        {
            LevelExpandData led = _levelexpandsrv.GetLevelExpandData(codeValue, levelExpandItem);
            return led.GetValue(depth);
        }

        public int ExpandDepth(LevelExpandItem levelExpandItem)
        {
            return _levelexpandsrv.GetFactLevelExpandDepth(levelExpandItem);
        }

        public string ExpandCaption(LevelExpandEnum type)
        {
            return _levelexpandsrv.GetLevelItemCaption(type);
        }
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (_htprecision != null)
                _htprecision.Clear();
            if (_htcompany != null)
                _htcompany.Clear();
            if (_htcusdefine != null)
                _htcusdefine.Clear();
            //_levelexpandsrv ;
            if (_htcusname != null)
                _htcusname.Clear();
        }

        #endregion

    }
}
