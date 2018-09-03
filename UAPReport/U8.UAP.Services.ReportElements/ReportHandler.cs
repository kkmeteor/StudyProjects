using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Xml;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Runtime.Remoting.Lifetime;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.BizDAE.Elements;
using UFIDA.U8.UAP.Services.ReportResource;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UFIDA.U8.ExpressionEditor.Runtime;
using System.Runtime.InteropServices;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    internal class ReportHandler
    {

        Logger logger = Logger.GetLogger("ReportHandler");
        int i1 = 0;
        #region fields
        protected Report _report;
        protected U8LoginInfor _login;
        protected DataHelper _datahelper;
        protected ReportDataSource _datasource;
        protected ScriptHelper _scripthelper;
        protected Assembly _assembly;
        protected byte[] _assemblybytes;
        protected SemiRows _tmprows;
        protected PageInfos _pageinfos;
        protected PageInfo _pageinfo;
        protected DataTable _datatable;
        protected SemiRowsContainerOnServer _semirowcontainer;

        protected SimpleHashtable _sumcalculators;
        // V13.0由于多列头下计算列将计算类型设置为最小值或其他非普通合计方式时，会造成多次汇总该列，因此需要将该列单独处理，避免汇总两次造成异常。
        // add by matfb
        protected SimpleHashtable _sumedcalculators = new SimpleHashtable();
        protected SimpleHashtable _precisions;
        protected SimpleHashtable _normalcolumns;
        protected ArrayList _avgcalculators;
        protected ArrayList _maxcalculators;
        protected ArrayList _mincalculators;

        protected int _defaultpageindex = 0;
        protected int cellPointLength = 0;//-1 to 0
        private IList<MethodInfo> methodColorSet = new List<MethodInfo>();
        private IList<object> objColorSet = new List<object>();
        private XmlDocument xmlColorSet = null;
        List<int> _getColorSetMethodErrorHashCode = new List<int>();
        #endregion

        #region event
        public event AfterCrossHandler AfterCross;
        public event CheckCanceledHandler CheckCanceled;
        private void OnAfterCross()
        {
            if (AfterCross != null)
            {
                AfterCross();
            }
        }

        protected void OnCheckCanceled()
        {
            if (CheckCanceled != null)
                CheckCanceled();
        }
        #endregion

        protected ReportHandler()
        {
        }

        public ReportHandler(Report report, U8LoginInfor login, DataHelper datahelper, ReportDataSource datasource, SemiRowsContainerOnServer semirowcontainer, byte[] assemblybytes, PageInfos pageinfos)
        {
            _report = report;
            _login = login;
            _datahelper = datahelper;
            _datasource = datasource;
            _report.DataSources = datasource.DataSources;
            _semirowcontainer = semirowcontainer;
            _assemblybytes = assemblybytes;
            _scripthelper = new ScriptHelper(_report);
            if (_assemblybytes != null)
                _assembly = Assembly.Load(_assemblybytes);
            _pageinfos = pageinfos;
            //加载shangpingfeng的条件表达式
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private U8Login.clsLogin GetLoginInfo(U8LoginInfor u8Login)
        {
            U8Login.clsLogin vbLogin;
            vbLogin = new U8Login.clsLogin();
            string pSubId = u8Login.SubID;
            string pAccId = string.Empty;
            string pYearId = string.Empty;
            string pUserId = string.Empty;
            string pPassword = string.Empty;
            string pDate = string.Empty;
            string cSrv = string.Empty;
            string cSerial = string.Empty;
            bool val = true;
            val = vbLogin.ConstructLogin(u8Login.UserToken);
            return vbLogin;
        }

        void InitCurrentAsmDir()
        {

        }

        static string GetU8Path()
        {
            string subKey = @"Software\UFSoft\WF\V8.700\Install\CurrentInstPath", sValue = "";
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine)
                {
                    using (Microsoft.Win32.RegistryKey key2 = key.OpenSubKey(subKey))
                    {
                        if ((key2 != null) && (key2.GetValue(sValue) != null))
                        {
                            return (string)key2.GetValue(sValue);
                        }
                    }
                }
                return GetInstallPath();
            }
            catch
            {
                return GetInstallPath();
            }

        }

        static string GetInstallPath()
        {
            string referencepath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            if (!referencepath.EndsWith(@"\"))
                referencepath += @"\";

            if (referencepath.ToLower().EndsWith(@"\uap\"))
            {
                //
                // 如果是从uap目录下执行 去掉路径中的"uap\"
                //
                referencepath.Remove(referencepath.Length - 4);
            }
            return referencepath;
        }

        [ThreadStatic]
        private static string _currentAsmDir = "";
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var index = args.Name.IndexOf(',');
            string sAsmName = "";
            if (index > 0)
                sAsmName = args.Name.Substring(0, index) + ".dll";
            else
                sAsmName = args.Name + ".dll";

            if (sAsmName.StartsWith("UFIDA.U8.ExpressionEditor"))
            {
                if (string.IsNullOrEmpty(_currentAsmDir))
                {
                    _currentAsmDir = System.IO.Path.Combine(GetU8Path(), @"ExpressionEditor\Bin");
                }
                String asmPath = System.IO.Path.Combine(_currentAsmDir, sAsmName);
                if (System.IO.File.Exists(asmPath))
                {
                    return System.Reflection.Assembly.LoadFrom(asmPath);
                }
            }
            return null;
        }

        #region open
        public void OpenCrossReport(bool bcross, string authstring, ReportLevelExpand levelexpand, ShowStyle showstyle)
        {
            #region before open
            GroupSchema gs = this._report.CurrentCrossSchema.CrossRowGroup;
            if (gs != null && gs.SchemaItems.Count > 0)
            {
                this._report.GroupSchemas.Add(gs);
                this._report.CurrentSchemaID = gs.ID;
                this._report.bShowDetail = gs.bShowDetail;
            }
            #endregion
            CrossCalc(bcross, authstring);
            OpenReport(authstring, levelexpand, showstyle);

            #region after open
            if (this._report.GroupSchemas != null && this._report.GroupSchemas.Contains(gs))
            {
                this._report.GroupSchemas.Remove(gs);
            }
            this._report.CurrentSchemaID = this._report.CurrentCrossSchema.ID;
            #endregion
            //InitExpand(levelexpand);
            //CheckGroup(authstring);
            //ReallyInterprete(authstring);
            //InnerCreateReport(showstyle == ShowStyle.None ? _report.CurrentSchema.ShowStyle : showstyle);
        }

        public void OpenReport(string authstring, ReportLevelExpand levelexpand, ShowStyle showstyle)
        {
            CheckGroup(authstring);
            ReallyInterprete(authstring);
            InnerCreateReport(showstyle == ShowStyle.None ? _report.CurrentSchema.ShowStyle : showstyle);
        }

        public void SetDefaultPageIndex(int pageindex)
        {
            _defaultpageindex = pageindex;
        }
        #endregion
        #region ColorSet
        //
        // pengzhzh 2012-07-06 判断xml中是否包含可用的colorset
        // modify by matfb V12.5
        private bool ConatainsColorSet(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            try
            {
                XmlNodeList xnlist = doc.SelectNodes(@"rows/row[@enable='1']");
                return (xnlist != null && xnlist.Count > 0);
            }
            catch
            {
                return false;
            }

        }
        private bool HasEXP(string reportColor)
        {
            Regex regex = new Regex("\\$Exp\\([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\\)");
            MatchCollection matchs = regex.Matches(reportColor);
            if (matchs != null && matchs.Count > 0)
                return true;
            else
                return false;
        }
        private IList<MethodInfo> getColorSetMethod(SemiRow row = null)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            System.Diagnostics.Trace.WriteLine("getColorSetMethod start");
            //根据
            if (string.IsNullOrEmpty(_report.ReportColorSet))
                return null;
            //if (methodColorSet != null)
            //    return methodColorSet;
            int hashCode = _report.ReportColorSet.GetHashCode();
            if (_getColorSetMethodErrorHashCode.Contains(hashCode))//已经出过错的不进行检查
                return null;
            //if (methodColorSet != null)
            //    return methodColorSet;
            if (!HasEXP(_report.ReportColorSet))
            {
                if (methodColorSet.Count > 0)
                {
                    return methodColorSet;
                }
            }

            List<StringBuilder> sbinfoList = new List<StringBuilder>();
            xmlColorSet = new XmlDocument();
            xmlColorSet.LoadXml(_report.ReportColorSet);
            //获取vblogin add by yanghx
            U8Login.clsLogin vbLogin = this.GetLoginInfo(_login);

            XmlNodeList xnlist = xmlColorSet.SelectNodes(@"rows/row[@enable='1']");
            string stemp = "";
            //正序
            //foreach (XmlNode n in xnlist)
            //{
            //    stemp=n.Attributes["condition"].Value;
            //    if (!stemp.StartsWith("if("))
            //        stemp = "if" + stemp;
            //    sbinfo.AppendLine(stemp);
            //    sbinfo.AppendLine("return " + n.Attributes["rowno"].Value + ";");
            //}
            //倒序
            XmlNode n = null;
            //for (int i = xnlist.Count - 1; i >= 0; i--)
            //{
            //    n = xnlist[i];
            //    stemp = n.Attributes["condition"].Value;
            //    if (!stemp.StartsWith("if("))
            //        stemp = "if" + stemp;
            //    sbinfo.AppendLine(stemp);
            //    sbinfo.AppendLine("return " + n.Attributes["rowno"].Value + ";");
            //}
            //sbinfo.AppendLine("return -1;");

            //
            // pengzhzh修改2012-6-5 修改输出事件
            // 原输出事件不支持多个(返回一个int，现修改为返回List<int>）
            //

            for (int i = xnlist.Count - 1; i >= 0; i--)
            {
                //
                // pengzhzh 2012-06-11
                // 添加try-catch模块 其中一个条件格式运行时出错
                // 其他的格式条件不收影响
                //

                //
                // pengzhzh 2012-6-21
                // 将一个返回List<int>的函数改成多个返回int的函数
                //
                StringBuilder sbinfo = new StringBuilder();
                sbinfo.AppendLine("int result = -1;");
                sbinfo.AppendLine("try { ");

                n = xnlist[i];
                stemp = n.Attributes["condition"].Value;
                if (row != null)
                {
                    //
                    // 处理条件 如果是编辑公式，在这里处理条件 pengzhzh
                    //
                    Regex regex = new Regex("\\$Exp\\([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\\)");
                    MatchCollection matchs = regex.Matches(stemp);
                    //
                    // 倒序解析和替换编辑公式 pengzhzh
                    //
                    if (matchs != null && matchs.Count > 0)
                    {
                        for (int j = matchs.Count - 1; j >= 0; --j)
                        {
                            Match match = matchs[j];
                            int index = match.Index;
                            string sGuid = stemp.Substring(index + 5, 36);

                            Dictionary<string, Type> argsDic = RuntimeService.RequestArguments(sGuid, vbLogin);
                            Dictionary<string, object> argsDicTmp = new Dictionary<string, object>();
                            //对每个argsdic进行赋值
                            if (argsDic != null)
                            {
                                foreach (KeyValuePair<string, Type> item in argsDic)
                                {
                                    argsDicTmp.Add(item.Key, item.Value);
                                    object value = row[item.Key];
                                    if (value == null || value.ToString() == string.Empty)
                                    {
                                        argsDicTmp[item.Key] = null;
                                        break;
                                    }
                                    if (item.Value != null && row.Contains(item.Key))
                                    {
                                        //
                                        // 数据类型分支
                                        //
                                        if (typeof(DateTime) == item.Value)
                                        {
                                            argsDicTmp[item.Key] = Convert.ToDateTime(row[item.Key]);
                                        }
                                        else if (typeof(decimal) == item.Value)
                                        {
                                            argsDicTmp[item.Key] = Convert.ToDecimal(row[item.Key]);
                                        }
                                        else if (typeof(double) == item.Value)
                                        {

                                            argsDicTmp[item.Key] = Convert.ToDouble(row[item.Key]);


                                        }
                                        else if (typeof(Single) == item.Value)
                                        {
                                            argsDicTmp[item.Key] = Convert.ToSingle(row[item.Key]);
                                        }
                                        else if (typeof(Int16) == item.Value)
                                        {
                                            argsDicTmp[item.Key] = Convert.ToInt16(row[item.Key]);
                                        }
                                        else if (typeof(Int32) == item.Value)
                                        {
                                            argsDicTmp[item.Key] = Convert.ToInt32(row[item.Key]);
                                        }
                                        else if (typeof(Int64) == item.Value)
                                        {
                                            argsDicTmp[item.Key] = Convert.ToInt64(row[item.Key]);
                                        }
                                        else // string and other reference types
                                        {
                                            argsDicTmp[item.Key] = Convert.ToString(row[item.Key]);
                                        }
                                    }
                                }
                            }
                            object result = RuntimeService.ExcuteExpression(sGuid, argsDicTmp, vbLogin);
                            //
                            // 设置格式条件公式替换的值 如果不是数值类型则加上双引号
                            //
                            #region sresult
                            string sResult;
                            if (null == result)
                            {
                                //
                                // null当做string.Empty处理 报表semirow中没有null 都是""
                                //
                                sResult = "\"\"";
                            }
                            else
                            {
                                Type tResult = result.GetType();
                                if (tResult == typeof(decimal) || tResult == typeof(double) || tResult == typeof(Single)
                                    || tResult == typeof(Int16) || tResult == typeof(Int32) || tResult == typeof(Int64))
                                {
                                    sResult = result.ToString();
                                }
                                else
                                {
                                    sResult = string.Format("{0}", result);
                                }
                            }
                            #endregion
                            stemp = stemp.Remove(index, match.Length);
                            stemp = stemp.Insert(index, sResult);
                        }
                    }
                }

                //
                // 2012-6-27 如果条件为空串 默认为true
                //
                if (string.IsNullOrEmpty(stemp))
                {
                    stemp = "(true)";
                }

                if (!stemp.StartsWith("if("))
                    stemp = "if" + stemp;
                sbinfo.AppendLine(stemp);
                sbinfo.AppendLine(string.Format("result = {0};", n.Attributes["rowno"].Value));

                sbinfo.AppendLine(" } catch { }");
                sbinfo.AppendLine(" return result;");
                sbinfoList.Add(sbinfo);
            }
            //sbinfo.AppendLine("return list;");

            methodColorSet.Clear();
            objColorSet.Clear();
            foreach (StringBuilder sBuilder in sbinfoList)
            {
                ICodeCompiler vcodeCompiler = new Microsoft.CSharp.CSharpCodeProvider().CreateCompiler();
                CompilerParameters vcompara = new CompilerParameters();
                vcompara.GenerateExecutable = false;
                vcompara.GenerateInMemory = true;

                StringBuilder vsb = new StringBuilder();
                vsb.AppendLine("using System.Windows.Forms;");
                vsb.AppendLine("using System;");
                vsb.AppendLine("using System.Data;");
                vsb.AppendLine("using System.Drawing;");
                vsb.AppendLine("using System.Collections;");
                vsb.AppendLine("using System.Data.SqlClient;");
                vsb.AppendLine("using System.Diagnostics;");
                vsb.AppendLine("using System.Text;");
                vsb.AppendLine("using System.IO;");
                vsb.AppendLine("using System.Collections.Generic;");
                vsb.AppendLine("using System.Globalization;");
                vsb.AppendLine("using UFIDA.U8.UAP.Services.ReportData;");
                vsb.AppendLine("using UFIDA.U8.UAP.Services.ReportElements;");
                vsb.AppendLine("using UFIDA.U8.UAP.Services.ReportFilterService;");
                vsb.AppendLine("public class MethodColorSet");
                vsb.AppendLine("{");
                vsb.AppendLine("public int getEffectRow(RowData data, SemiRow semirow)");
                vsb.AppendLine("{");
                //vsb.AppendLine("return 2;");
                vsb.AppendLine(sBuilder.ToString());
                vsb.AppendLine("}");
                //
                // pengzhzh 2012-07-31 解释Include
                //
                vsb.AppendLine("bool Include(object o, object v)");
                vsb.AppendLine("{");
                vsb.AppendLine("Type t = o.GetType();");
                vsb.AppendLine("if (typeof(double) == t)");
                vsb.AppendLine("{ return Convert.ToDouble(o) == Convert.ToDouble(v); }");
                vsb.AppendLine("else if (typeof(DateTime) == t)");
                vsb.AppendLine("{ return Convert.ToDateTime(o) == Convert.ToDateTime(v); }");
                vsb.AppendLine("else");
                vsb.AppendLine("{ return o.ToString().Contains(v.ToString()); }");
                vsb.AppendLine("}");

                vsb.AppendLine("}");

                ArrayList references = new ArrayList();
                references.Add("System.Windows.Forms.dll");
                references.Add("System.dll");
                references.Add("System.Data.dll");
                references.Add("System.Xml.dll");
                references.Add("System.Drawing.dll");
                references.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportData.dll");
                references.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportElements.dll");
                references.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportFilterService.dll");
                for (int i = 0; i < references.Count; i++)
                    vcompara.ReferencedAssemblies.Add(references[i].ToString());
                try
                {
                    CompilerResults rst = vcodeCompiler.CompileAssemblyFromSource(vcompara, vsb.ToString());
                    Assembly clsColorSet = rst.CompiledAssembly;
                    object obj = clsColorSet.CreateInstance("MethodColorSet");
                    objColorSet.Add(obj);
                    MethodInfo method = obj.GetType().GetMethod("getEffectRow");
                    //methodColorSet = method;
                    methodColorSet.Add(method);
                }
                catch (Exception e)
                {
                    //objColorSet = null;
                    //methodColorSet = null;
                    System.Diagnostics.Debug.WriteLine(e.InnerException);
                    //return null;
                    if (!_getColorSetMethodErrorHashCode.Contains(hashCode))
                    {
                        _getColorSetMethodErrorHashCode.Add(hashCode);
                    }
                }
            }
            watch.Stop();
            System.Diagnostics.Trace.WriteLine("getColorSetMethod stop");
            System.Diagnostics.Trace.WriteLine("getColorSetMethod 耗时"+ watch.ElapsedMilliseconds);
            return methodColorSet;
        }

        //public Dictionary<string, string> GetDictionary(RowData data, SemiRow semirow, U8Login.clsLogin vbLogin)
        //{
        //if (string.IsNullOrEmpty(_report.ReportColorSet))
        //{
        //    return null;
        //}
        //Dictionary<string, string> dic = new Dictionary<string, string>();
        //xmlColorSet = new XmlDocument();
        //xmlColorSet.LoadXml(_report.ReportColorSet);

        //XmlNodeList xnlist = xmlColorSet.SelectNodes(@"rows/row[@enable='1']");
        //string stemp = "";
        //XmlNode n = null;

        //for (int i = xnlist.Count - 1; i >= 0; i--)
        //{

        //    n = xnlist[i];
        //    stemp = n.Attributes["condition"].Value;
        //    //
        //    // 处理条件 如果是编辑公式，在这里处理条件 pengzhzh
        //    //
        //    Regex regex = new Regex("\\$Exp\\([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\\)");
        //    MatchCollection matchs = regex.Matches(stemp);
        //    //
        //    // 倒序解析和替换编辑公式 pengzhzh
        //    //
        //    if (matchs != null && matchs.Count > 0)
        //    {
        //        for (int j = matchs.Count - 1; j >= 0; --j)
        //        {
        //            Match match = matchs[j];
        //            int index = match.Index;
        //            string sGuid = stemp.Substring(index + 5, 36);

        //            Dictionary<string, object> argsDic = RuntimeService.RequestArguments(sGuid, vbLogin);
        //            //argsDic 

        //            //添加到dic

        //        }
        //    }
        //}

        //return dic;
        //}

        #endregion
        #region cross handle
        private void CrossCalc(bool bcross, string authstring)
        {
            this.FilterCrossPointByAuth(authstring);

            Cells rowheadercells = _report.Sections[SectionType.CrossRowHeader].Cells;
            Section reportheader = _report.Sections[SectionType.ReportHeader];
            Cells reportheadercells = null;
            if (reportheader != null)
                reportheadercells = reportheader.Cells;
            if (reportheadercells != null)
            {
                foreach (Cell cell in reportheadercells)
                {
                    if (cell is IMapName)
                    {
                        Cell ctorow = cell.Clone() as Cell;
                        ctorow.bHidden = true;
                        rowheadercells.AddDirectly(ctorow);
                    }
                }
            }

            CrossService cs = new CrossService();
            string sErrMsg = "";
            Cells cells;
            if (_report.Sections[SectionType.CrossColumnHeader].Cells.Count > 0)
            {
                if (bcross)
                {
                    _report.CrossTable = _report.BaseTable;
                    //_report.BaseTable = CustomDataSource.GetATableName();
                    _report.BaseTable = CustomDataSource.GetATableNameWithTaskId(this._login.TaskID);
                    if (cs.CrossCalc(_report, _login, out sErrMsg) == false)
                        throw new Exception(sErrMsg);//错误处理
                }
                else
                    cs.NotCrossCalc(_report);

                if (cs.GetCrossCells(_report, _login, out cells, out sErrMsg) == false)
                    throw new Exception(sErrMsg);//错误处理
            }
            else
                cells = new Cells();

            Section columnheaders = _report.Sections[SectionType.CrossColumnHeader];
            Section crossdetail = _report.Sections[SectionType.CrossDetail];
            _report.Sections.Remove(_report.Sections[SectionType.CrossRowHeader]);
            _report.Sections.Remove(columnheaders);
            _report.Sections.Remove(crossdetail);

            foreach (Cell c in columnheaders.Cells)
            {
                _report.DataSources.Remove((c as IMapName).MapName);
            }
            foreach (Cell c in crossdetail.Cells)
            {
                DataSource ds = _report.DataSources[(c as IMapName).MapName];
                if (ds != null)
                    ds.Caption = "";
            }
            Section sect = new GridDetail();
            sect.Cells = cells;
            _report.Sections.Add(sect);

            //AdaptDataSource(null, null, _report.BaseTable);
            OnAfterCross();
        }

        /// <summary>
        /// 应用交叉点权限
        /// </summary>
        private void FilterCrossPointByAuth(string authstring)
        {
            ArrayList authCols = this._report.GetAuthCols(authstring);
            Cells points = this._report.Sections[SectionType.CrossDetail].Cells;
            if (points.Count == 0)
                throw new Exception(String4Report.GetString("格式设置错误,请重新设置格式后重试.", this._login.LocaleID));
            ArrayList removeingCells = new ArrayList();
            foreach (Cell c in points)
                if (this._report.NoAuthOrNotValidCusItem(c, authCols, this._datahelper))
                    removeingCells.Add(c);
            foreach (Cell c in removeingCells)
                points.Remove(c);
            if (points.Count == 0)
                throw new Exception(String4Report.GetString("您无权查询该表数据!(字段权限)", this._login.LocaleID));
        }
        #endregion

        #region interprete format
        public void ReallyInterprete(string authstring)
        {
            ArrayList colsauth = _report.GetAuthCols(authstring);
            HandleMaxMinAvg();

            #region circle
            _sumcalculators = new SimpleHashtable();
            _precisions = new SimpleHashtable();
            _normalcolumns = new SimpleHashtable();

            _avgcalculators = new ArrayList();
            _maxcalculators = new ArrayList();
            _mincalculators = new ArrayList();

            ArrayList invalidsource = new ArrayList();
            foreach (string key in _report.DataSources.Keys)
            {
                if (key.Contains("@@@"))
                    invalidsource.Add(key);
                else if (_report.NoAuthOrNotValidCusItem(key, colsauth, _datahelper))
                    invalidsource.Add(key);
            }
            foreach (string key in invalidsource)
                _report.DataSources.Remove(key);

            int count = _report.Sections.Count - 1;
            while (count >= 0)
            {
                Section section = _report.Sections[count];
                if (section.SectionType == SectionType.PrintPageSummary || section.SectionType == SectionType.PrintPageTitle)
                {
                    count--;
                    continue;
                }

                if (!_report.bShowDetail && section.SectionType == SectionType.Detail)
                {
                    _report.Sections.RemoveAt(count);
                }
                else
                {
                    int index = section.Cells.Count - 1;
                    while (index >= 0)
                    {
                        Cell cell = section.Cells[index];
                        #region //占比列处理
                        //如果分组的数据源和占比列的数据源一样，则移除占比列
                        if (cell is GridProportionDecimal &&
                            this._report.CurrentSchema != null &&
                            this._report.CurrentSchema.SchemaItems != null &&
                            this._report.CurrentSchema.SchemaItems.Count > 0)
                        {
                            foreach (GroupSchemaItem gsi in this._report.CurrentSchema.SchemaItems)
                            {
                                foreach (string item in gsi.Items)
                                {
                                    Cell group = section.Cells[item];
                                    if (group != null)
                                    {
                                        if (group is IMapName && cell is IMapName)
                                        {
                                            if ((group as IMapName).MapName == (cell as IMapName).MapName)
                                            {
                                                section.Cells.RemoveAt(index);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                        }// end 占比列处理
                        #endregion
                        else if (cell is IMetrix)
                            (cell as IMetrix).InitParts(section.Cells);
                        else if (_report.UnderState != ReportStates.Designtime
                            && _report.NoAuthOrNotValidCusItem(cell, colsauth, _datahelper))
                            section.Cells.RemoveAt(index);
                        else if (_report.UnderState != ReportStates.Designtime
                            && (cell is ICalculateColumn && (cell as ICalculateColumn).Expression.Trim() == ""))
                            section.Cells.RemoveAt(index);
                        else
                        {
                            #region summary mode
                            if (_report.UnderState != ReportStates.Designtime && !_report.bShowDetail)
                            {
                                if (section.SectionType != SectionType.PageHeader && section.SectionType != SectionType.PageFooter)
                                {
                                    if (section.SectionType == SectionType.ReportHeader || _report.Type == ReportType.FreeReport)
                                    {
                                        if (cell is ILabelType)
                                        {
                                            if (cell is SuperLabel)
                                            {
                                                SuperLabelSummary(cell as SuperLabel);
                                                if ((cell as SuperLabel).Labels.Count == 0)
                                                {
                                                    section.Cells.RemoveAt(index);
                                                    index--;
                                                    continue;
                                                }
                                            }
                                            else if ((cell as ILabelType).LabelType == LabelType.DetailLabel)
                                            {
                                                section.Cells.RemoveAt(index);
                                                index--;
                                                continue;
                                            }
                                        }
                                        else if (!(cell is IGroup || cell is Calculator || cell is DecimalAlgorithmCalculator || cell is AlgorithmCalculator || cell is Expression || cell is Image))
                                        {
                                            section.Cells.RemoveAt(index);
                                            index--;
                                            continue;
                                        }
                                    }
                                    else if (!_report.bIndicator)
                                    {
                                        if (cell is IDataSource && (cell as IDataSource).DataSource.Name.ToLower() == _report.BaseID.ToLower() && cell is IGridCollect && (cell as IGridCollect).bSummary)
                                            (cell as IGridCollect).bSummary = false;
                                        if (!(_report.CurrentSchema != null && _report.CurrentSchema.Contains(cell)) &&
                                           !((cell is IGridCollect) && (cell as IGridCollect).bSummary)
                                            && !(cell is SuperLabel)
                                            //&& !((cell is SuperLabel) && (cell as SuperLabel).LabelType == LabelType.SummaryLabel)
                                            )
                                        {
                                            section.Cells.RemoveAt(index);
                                            index--;
                                            continue;
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region many other things to do here
                            if (cell is IMapName)
                            {
                                //addcolumn
                                _datasource.AddColumn(cell as IMapName);
                            }
                            #endregion
                        }
                        index--;
                    }
                }

                count--;
            }

            count = _report.Sections.Count - 1;
            while (count >= 0)
            {
                Section section = _report.Sections[count];
                int index = section.Cells.Count - 1;
                while (index >= 0)
                {
                    bool hasRemove = false;
                    Cell cell = section.Cells[index];
                    //items
                    if (!AddItems(cell, section))
                    {
                        section.Cells.RemoveAt(index);
                        hasRemove = true;
                    }
                    //script
                    _scripthelper.AddCellString(section, cell);
                    if (cell.bHidden)
                    {
                        if (!hasRemove)
                            section.Cells.RemoveAt(index);
                        if (cell is IMapName)
                        {
                            DataSource dshidden = _report.DataSources[(cell as IMapName).MapName];
                            if (dshidden != null)
                            {
                                dshidden.CNCaption = "";
                                dshidden.ENCaption = "";
                                dshidden.TWCaption = "";
                                dshidden.Caption = "";
                            }
                        }
                    }
                    if (cell is IUserDefine && !string.IsNullOrEmpty((cell as IUserDefine).UserDefineItem))
                        // 当caption和数据源caption不同时，说明是自定义的名字，不取自定义项的名字
                        //if (cell is IUserDefine && !string.IsNullOrEmpty((cell as IUserDefine).UserDefineItem)
                        // && (cell is IDataSource) && cell.Caption == (cell as IDataSource).DataSource.Caption)
                        cell.Caption = _datahelper.CusDefineInfo(cell.Caption, (cell as IUserDefine).UserDefineItem);
                    index--;
                }
                count--;
            }

            if (!string.IsNullOrEmpty(_report.RowFilter.FilterString))
            {
                AddScriptKey(null, _report.RowFilter.FilterString, null);
            }

            foreach (string key in _sumcalculators.Keys)
            {
                if (_sumedcalculators.Contains(key))
                    continue;
                PrecisionHelper ph = _sumcalculators[key] as PrecisionHelper;
                int precision = -1;
                if (_precisions.Contains(key))
                    precision = Convert.ToInt32(_precisions[key]);
                AddAggregateStrings(ph.OperType, ph.Expression, ph.AsName, precision, ph.bSingleColumn);
            }
            foreach (string key in _normalcolumns.Keys)
            {
                PrecisionHelper ph = _normalcolumns[key] as PrecisionHelper;
                int precision = -1;
                if (_precisions.Contains(key))
                    precision = Convert.ToInt32(_precisions[key]);
                AddDetailStrings(ph.bGroupItem, ph.GroupName, ph.Expression, ph.AsName, precision, ph.bDecimal);
            }
            #endregion
        }

        protected virtual void HandleMaxMinAvg()
        {
        }

        protected void AddSortItem(IMapName map, int level)
        {
            string mapname = map.MapName;
            _report.SortSchema.Add(mapname, (map as ISort).SortOption, level);
        }

        protected virtual bool AddItems(Cell cell, Section section)
        {
            #region add columns & addkey
            string keystring = null;
            if (cell is IDataSource)
            {
                if (!(cell is IGroup) && (cell as IDataSource).DataSource.Name.ToLower() == "emptycolumn")
                {
                    if (cell is IGridCollect && (cell as IGridCollect).bSummary)
                        (cell as IGridCollect).bSummary = false;
                    return true;
                }
                if (!_datasource.DataSources.Contains((cell as IDataSource).DataSource.Name))//add//RawContains
                    return false;

                DataType dt = (cell as IDataSource).DataSource.Type;
                if ((cell is IDecimal && dt != DataType.Int && dt != DataType.Decimal && dt != DataType.Currency) ||
                    (cell is IDateTime && dt != DataType.DateTime))
                {
                    int index = section.Cells.IndexOf(cell);
                    string caption = GetCaptionBeforeChangeType(cell);
                    SortOption sortOption = null;
                    try
                    {
                        if (cell is ISort)
                            sortOption = (cell as ISort).SortOption;
                    }
                    catch
                    {
                        ;
                    }
                    section.Cells.RemoveAt(index);
                    if (_report.Type == ReportType.FreeReport)
                        cell = new DBText(cell as IDataSource);
                    else
                        cell = new GridLabel(cell as IDataSource);
                    cell.Caption = caption;
                    if (sortOption != null)
                    {
                        if (cell is ISort)
                            (cell as ISort).SortOption = sortOption;
                    }
                    section.Cells.Insert(index, cell);
                }

                if (!_normalcolumns.Contains((cell as IDataSource).DataSource.Name))
                {
                    _normalcolumns.Add((cell as IDataSource).DataSource.Name, new PrecisionHelper(cell is IGroup, cell.Name, null, (cell as IDataSource).DataSource.Name, null, true, cell is IDecimal));
                    //AddDetailStrings(cell,null, (cell as IDataSource).DataSource.Name, (cell is IDecimal) ? (cell as IDecimal).PointLength : -1, cell is IDecimal);

                    #region allcolumns
                    //_allcolumns.Add((cell as IDataSource).DataSource.Name);
                    #endregion
                }
                if (cell is IDecimal)
                    AddPrecision((cell as IDataSource).DataSource.Name, (cell as IDecimal).PointLength);

            }
            else if (cell is ICalculateColumn)
            {
                if (!_normalcolumns.Contains((cell as IMapName).MapName))
                {
                    bool hasalgorithmcolumn = false;
                    #region allcolumns
                    if (!ExpressionService.bExpressionAfterCross((cell as ICalculateColumn).Expression))
                    {
                        string expression = ReportEngine.ReplaceAppend(_datasource.DataSources, (cell as ICalculateColumn).Expression.ToLower(), 0);
                        if (!ExpressionService.bSpecialExpression(expression.Replace("isnull", "")))
                        {
                            string[] expressions = ExpressionService.SplitExpression(expression);
                            for (int i = 0; i < expressions.Length; i++)
                            {
                                if (expressions[i].Trim() == "" || ExpressionService.IsAConst(expressions[i].Trim()))
                                    continue;
                                DataSource ds = _datasource.DataSources[expressions[i].Trim()];
                                if (ds == null || (cell is IDecimal && ds.Type != DataType.Int && ds.Type != DataType.Decimal && ds.Type != DataType.Currency))
                                    return false;
                                //if (!ds.bAppend )
                                //    _allcolumns.Add(expressions[i].Trim());
                                //else 
                                if (ds.bAppend && ds.Tag == null)
                                    hasalgorithmcolumn = true;
                            }
                        }
                    }
                    #endregion
                    if (!hasalgorithmcolumn)
                    {
                        _normalcolumns.Add((cell as IMapName).MapName, new PrecisionHelper(cell is IGroup, cell.Name, null, (cell as IMapName).MapName, (cell as ICalculateColumn).Expression, false, cell is IDecimal));

                        //AddDetailStrings(cell,(cell as ICalculateColumn).Expression, (cell as IMapName).MapName, (cell is IDecimal) ? (cell as IDecimal).PointLength : -1, cell is IDecimal);
                    }
                    else if (cell is ICalculator
                        && (cell as ICalculator).Operator != OperatorType.AccumulateSUM
                        && (cell as ICalculator).Operator != OperatorType.ComplexSUM
                        && (cell as ICalculator).Operator != OperatorType.BalanceSUM)
                    {
                        //throw new Exception("计算列 " + cell.Name + " 不支持所定义类型的汇总方式，请修改。");//need resource
                        (cell as ICalculator).Operator = OperatorType.ComplexSUM;
                        _report.MustShowDetail = true;
                    }
                }
                if (cell is IDecimal)
                    AddPrecision((cell as IMapName).MapName, (cell as IDecimal).PointLength);
                //if (cell is ICalculator && (cell as ICalculator).Operator == OperatorType.ExpressionSUM)
                //    keystring = (cell as ICalculateColumn).Expression;
            }
            else if (cell is IAlgorithm)
            {
                if (cell is IMapName)
                {
                    _report.ScriptColumns.Add(cell as IMapName);
                    #region addrowbalancecolumns
                    AnalysPrevious((cell as IAlgorithm).Algorithm, "previous.");
                    AnalysPrevious((cell as IAlgorithm).Algorithm, "previous[\u0022");
                    #endregion

                    //if (cell is IGridCollect && (cell as IGridCollect).bSummary && (cell as IGridCollect).Operator == OperatorType.SUM)
                    //    (cell as IGridCollect).Operator = OperatorType.ComplexSUM;
                }

                keystring = (cell as IAlgorithm).Algorithm;
            }
            AddScriptKey(cell, keystring, cell.PrepaintEvent);
            #endregion

            #region sum,max,min,avg items
            if (section.SectionType != SectionType.ReportHeader && cell is IGridCollect && (cell as IGridCollect).bSummary)
            {
                if (cell is ICalculateColumn)
                    return AddCalculators((cell as IGridCollect).Operator, (cell as ICalculateColumn).Expression, cell as IMapName);
                else
                    AddCalculators((cell as IGridCollect).Operator, (cell as IMapName).MapName, cell as IMapName);
            }
            else if (cell is Calculator)
            {
                DataSource dscalc = _datasource.DataSources[(cell as IMapName).MapName];
                if (dscalc != null && dscalc.Type != DataType.Int && dscalc.Type != DataType.Decimal && dscalc.Type != DataType.Currency)
                {
                    return false;
                }
                return AddCalculators((cell as ICalculator).Operator, (cell as ICalculateColumn).Expression, cell as IMapName);
            }
            else if (cell is AlgorithmCalculator)
            {
                _report.ScriptCalculators.Add(cell.Name);
            }
            #endregion
            return true;
        }

        private void AnalysPrevious(string algorithm, string flag)
        {
            int pindex = algorithm.IndexOf(flag);
            while (pindex != -1)
            {
                pindex = algorithm.IndexOfAny(new char[] { '\u0022', '.' }, pindex);
                algorithm = algorithm.Substring(pindex + 1).Trim();
                pindex = algorithm.IndexOfAny(new char[] { '\u0022', ' ', ')', '+', '-', '*', '/' });
                string column = algorithm.Substring(0, pindex).Trim();
                if (_datasource.DataSources.Contains(column))
                    _report.RowBalanceColumns.Add(column);
                algorithm = algorithm.Substring(pindex + 1).Trim();
                pindex = algorithm.IndexOf(flag);
            }
        }

        private void AddPrecision(string mapname, int pointlength)
        {
            if (!_precisions.Contains(mapname))
                _precisions.Add(mapname, pointlength);
            else
            {
                int pl = Convert.ToInt32(_precisions[mapname]);
                if (pointlength > pl)
                    _precisions[mapname] = pointlength;
            }
        }

        protected bool AddCalculators(OperatorType oper, string key, IMapName map)
        {
            return AddCalculators(oper, key, map, null);
        }

        protected bool bSingle(IMapName map)
        {
            return map is IDataSource || (map is ICalculateColumn && (map as ICalculateColumn).Expression.ToLower() == map.MapName.ToLower());
        }


        protected bool AddCalculators(OperatorType oper, string key, IMapName map, SimpleArrayList indicatorkeys)
        {
            switch (oper)
            {
                case OperatorType.SUM:
                    if (!_sumcalculators.Contains(map.MapName))
                        _sumcalculators.Add(map.MapName, new PrecisionHelper(false, null, "SUM", map.MapName, key, bSingle(map), true));

                    AddPrecision(map.MapName, (map as IDecimal).PointLength);
                    AddIndicatorKey(oper, indicatorkeys, map);
                    break;
                case OperatorType.AVG:
                    if (!_avgcalculators.Contains(map.MapName.ToLower()))
                    {
                        _avgcalculators.Add(map.MapName.ToLower());
                        AddAggregateStrings("AVG", key, map.MapName, (map as IDecimal).PointLength, bSingle(map));
                    }
                    AddIndicatorKey(oper, indicatorkeys, map);
                    break;
                case OperatorType.MAX:
                    if (!_maxcalculators.Contains(map.MapName.ToLower()))
                    {
                        _maxcalculators.Add(map.MapName.ToLower());
                        AddAggregateStrings("MAX", key, map.MapName, (map as IDecimal).PointLength, bSingle(map));
                    }
                    AddIndicatorKey(oper, indicatorkeys, map);
                    break;
                case OperatorType.MIN:
                    if (!_mincalculators.Contains(map.MapName.ToLower()))
                    {
                        _mincalculators.Add(map.MapName.ToLower());
                        AddAggregateStrings("MIN", key, map.MapName, (map as IDecimal).PointLength, bSingle(map));
                    }
                    AddIndicatorKey(oper, indicatorkeys, map);
                    break;
                case OperatorType.ComplexSUM:
                    _report.ComplexColumns.Add(map.MapName);
                    break;
                case OperatorType.AccumulateSUM:
                    _report.AccumulateColumns.Add(map.MapName);
                    //if(!_report.bShowDetail)
                    //    AddAggregateStrings("SUM", key, map.MapName, (map as IDecimal).PointLength);
                    break;
                case OperatorType.BalanceSUM:
                    _report.BalanceColumns.Add(map.MapName);
                    //if (!_report.bShowDetail)
                    //    AddAggregateStrings("SUM", key, map.MapName, (map as IDecimal).PointLength);
                    break;
                case OperatorType.ExpressionSUM:
                    if (map is ICalculateColumn)
                    {
                        string[] expression = ExpressionService.SplitExpression((map as ICalculateColumn).Expression);
                        for (int i = 0; i < expression.Length; i++)
                        {
                            if (expression[i].Trim() != "" && _datasource.DataSources.RawContains(expression[i].Trim()))
                            {
                                if (ExpressionInGroup(expression[i].Trim()))
                                    return false;
                                if (!_sumcalculators.Contains(expression[i].Trim()))
                                {
                                    _sumcalculators.Add(expression[i].Trim(), new PrecisionHelper(false, null, "SUM", expression[i].Trim(), null, true, true));
                                    //AddAggregateStrings("SUM", null, expression[i].Trim(), (map as IDecimal).PointLength);
                                }
                                AddPrecision(expression[i].Trim(), (map as IDecimal).PointLength);
                            }
                        }
                    }
                    AddIndicatorKey(OperatorType.SUM, indicatorkeys, map);
                    //_report.ScriptCalculators.Add(map.MapName);
                    //if (!_report.bShowDetail)
                    //    AddAggregateStrings("SUM", key, map.MapName, (map as IDecimal).PointLength);
                    break;
            }
            return true;
        }

        protected bool ExpressionInGroup(string exp)
        {
            foreach (GroupSchemaItem gsitem in _report.CurrentSchema.SchemaItems)
            {
                foreach (string item in gsitem.Items)
                {
                    if (item.Trim() == exp)
                        return true;
                }
            }
            return false;
        }

        protected void AddIndicatorKey(OperatorType oper, SimpleArrayList indicatorkeys, IMapName map)
        {
            if (indicatorkeys != null)
            {
                switch (oper)
                {
                    case OperatorType.SUM:
                        foreach (string ikey in indicatorkeys)
                        {
                            if (!_sumcalculators.Contains(ikey))
                                _sumcalculators.Add(ikey, new PrecisionHelper(false, null, "SUM", ikey, null, true, true));
                            AddPrecision(ikey, (map as IDecimal).PointLength);
                        }
                        break;
                    case OperatorType.AVG:
                        foreach (string ikey in indicatorkeys)
                        {
                            if (!_avgcalculators.Contains(ikey.ToLower()))
                            {
                                _avgcalculators.Add(ikey.ToLower());
                                AddAggregateStrings("AVG", null, ikey, (map as IDecimal).PointLength, bSingle(map));
                            }
                        }
                        break;
                    case OperatorType.MAX:
                        foreach (string ikey in indicatorkeys)
                        {
                            if (!_maxcalculators.Contains(ikey.ToLower()))
                            {
                                _maxcalculators.Add(ikey.ToLower());
                                AddAggregateStrings("MAX", null, ikey, (map as IDecimal).PointLength, bSingle(map));
                            }
                        }
                        break;
                    case OperatorType.MIN:
                        foreach (string ikey in indicatorkeys)
                        {
                            if (!_mincalculators.Contains(ikey.ToLower()))
                            {
                                _mincalculators.Add(ikey.ToLower());
                                AddAggregateStrings("MIN", null, ikey, (map as IDecimal).PointLength, bSingle(map));
                            }
                        }
                        break;
                }
            }
        }

        protected void AddScriptKey(Cell cell, string keystring, string script)
        {
            AddScriptKey(null, cell, keystring, script);
        }

        protected void AddScriptKey(SimpleArrayList sal, Cell cell, string keystring, string script)
        {
            if (string.IsNullOrEmpty(keystring) && string.IsNullOrEmpty(script))
                return;
            foreach (string key in _report.DataSources.Keys)
            {
                if (keystring != null && (keystring.ToLower().Contains("\"" + key.ToLower() + "\"") || keystring.ToLower().Contains("." + key.ToLower())))
                {
                    _scripthelper.AddAKey(key);
                    if (sal != null && _report.DataSources[key].IsADecimal)
                        sal.Add(key);
                    if (_report.DataSources.RawContains(key) && !_normalcolumns.Contains(key))
                    {
                        bool bdecimal = _report.DataSources[key].Type == DataType.Currency ||
                            _report.DataSources[key].Type == DataType.Decimal ||
                            _report.DataSources[key].Type == DataType.Int;
                        _normalcolumns.Add(key, new PrecisionHelper(false, key, null, key, null, true, bdecimal));
                        //AddDetailStrings(cell,null, key, (cell != null && cell is IDecimal) ? (cell as IDecimal).PointLength : -1, bdecimal);
                        #region allcolumns
                        //_allcolumns.Add(key);
                        #endregion
                    }
                    keystring = keystring.ToLower().Replace(key.ToLower(), " ");
                }
                if (script != null && (script.ToLower().Contains("\"" + key.ToLower() + "\"") || script.ToLower().Contains("." + key.ToLower())))
                {
                    _scripthelper.AddAKey(key);
                    if (sal != null && _report.DataSources[key].IsADecimal)
                        sal.Add(key);
                    if (_report.DataSources.RawContains(key) && !_normalcolumns.Contains(key.ToLower()))
                    {
                        bool bdecimal = _report.DataSources[key].Type == DataType.Currency ||
                            _report.DataSources[key].Type == DataType.Decimal ||
                            _report.DataSources[key].Type == DataType.Int;
                        _normalcolumns.Add(key, new PrecisionHelper(false, key, null, key, null, true, bdecimal));
                        //AddDetailStrings(cell,null, key, (cell is IDecimal) ? (cell as IDecimal).PointLength : -1, bdecimal);

                        #region allcolumns
                        //_allcolumns.Add(key);
                        #endregion
                    }
                    script = script.ToLower().Replace(key.ToLower(), " ");
                }
            }
        }

        protected virtual void AddCalculatorIndicatorKey(Cell cell, string key)
        {

        }

        private void AddDetailStrings(bool bgroupitem, string groupname, string expression, string asname, int precision, bool bdecimal)
        {
            if (!_report.bShowDetail)
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append(_report.DetailString);
            if (sb.Length > 0)
                sb.Append(",");


            bgroupitem = bgroupitem || (groupname != null && _report.CurrentSchema.Contains(groupname));

            if (bgroupitem)
            {
                #region bgroupitem
                DataSource ds = _report.DataSources[asname];
                //if (ds != null && (ds.Type == DataType.DateTime || ds.IsADecimal))
                //{
                //    if (expression != null)
                //    {
                //        sb.Append("(");
                //        sb.Append(ReportEngine.HandleExpression(_report.DataSources, expression, "A.",false));
                //        sb.Append(") as [");
                //        sb.Append(asname);
                //        sb.Append("]");
                //    }
                //    else
                //    {
                //        sb.Append("A.");
                //        sb.Append("[");
                //        sb.Append(asname);
                //        sb.Append("] as [");
                //        sb.Append(asname);
                //        sb.Append("]");
                //    }
                //}
                if (ds != null && (ds.Type == DataType.DateTime))
                {
                    if (expression != null)
                    {
                        sb.Append("(");
                        sb.Append(ReportEngine.HandleExpression(_report.DataSources, expression, "A.", false));
                        sb.Append(") as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                    else
                    {
                        sb.Append("A.");
                        sb.Append("[");
                        sb.Append(asname);
                        sb.Append("] as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                }
                else if (ds != null && (ds.IsADecimal))
                {
                    if (expression != null)
                    {
                        sb.Append("(");
                        sb.Append(ReportEngine.HandleExpression(_report.DataSources, expression, "A.", false));
                        sb.Append(") as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                    else
                    {
                        sb.Append("convert(decimal(38,");
                        if (precision <= 0)
                            sb.Append("0");
                        else
                            sb.Append(precision.ToString());
                        sb.Append("), A.[");
                        sb.Append(asname);
                        sb.Append("]) as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                }

                else
                {
                    if (expression != null)
                    {
                        sb.Append("isnull((");
                        sb.Append(ReportEngine.HandleExpression(_report.DataSources, expression, "A.", false));
                        sb.Append("),'') as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                    else
                    {
                        sb.Append("isnull(A.");
                        sb.Append("[");
                        sb.Append(asname);
                        sb.Append("],'') as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                }
                #endregion
            }
            else
            {
                #region not bgroupitem
                if (!bdecimal)
                {
                    #region !decimal
                    if (expression != null)
                    {
                        sb.Append("(");
                        sb.Append(ReportEngine.HandleExpression(_report.DataSources, expression, "A.", false));
                        sb.Append(") as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                    else
                    {
                        sb.Append("A.");
                        sb.Append("[");
                        sb.Append(asname);
                        sb.Append("] as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                    #endregion
                }
                else
                {
                    #region decimal
                    if (expression != null)
                    {
                        sb.Append("(");
                        if (precision != -1)
                        {
                            sb.Append("Convert(Decimal(38,");
                            sb.Append(precision.ToString());
                            sb.Append("),");
                        }
                        sb.Append(ReportEngine.HandleExpression(_report.DataSources, expression, "A.", true));
                        if (precision != -1)
                            sb.Append(")");
                        sb.Append(") as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                    else
                    {
                        sb.Append("(");
                        if (precision != -1)
                        {
                            sb.Append("Convert(Decimal(38,");
                            sb.Append(precision.ToString());
                            sb.Append("),");
                        }
                        sb.Append("A.[");
                        sb.Append(asname);
                        sb.Append("]");
                        if (precision != -1)
                            sb.Append(")");
                        sb.Append(") as [");
                        sb.Append(asname);
                        sb.Append("]");
                    }
                    #endregion
                }
                #endregion
            }
            _report.DetailString = sb.ToString();
        }

        private void AddAggregateStrings(string op, string expression, string asname, int precision, bool bsingle)
        {
            #region minor aggregate string
            StringBuilder sbminor = new StringBuilder();
            sbminor.Append(_report.MinorAggregateString);
            if (sbminor.Length > 0)
                sbminor.Append(",");
            if (expression != null)
            {
                if (!bsingle)
                {
                    if (GroupLevels > 0 && !ExpressionService.bSpecialExpression(expression))
                    {
                        if (precision != -1)
                        {
                            sbminor.Append("Convert(Decimal(38,");
                            sbminor.Append(precision.ToString());
                            sbminor.Append("),");
                        }
                        sbminor.Append(ReportEngine.HandleExpression(_report.DataSources, expression, op + "(", true));

                        if (precision != -1)
                            sbminor.Append(")");
                        sbminor.Append(" as [");
                        sbminor.Append(asname);
                        sbminor.Append("]");
                    }
                    else
                    {
                        sbminor.Append("(");
                        sbminor.Append(op);
                        sbminor.Append("(");
                        if (precision != -1)
                        {
                            sbminor.Append("Convert(Decimal(38,");
                            sbminor.Append(precision.ToString());
                            sbminor.Append("),");
                        }

                        sbminor.Append(ReportEngine.HandleExpression(_report.DataSources, expression, "", op.ToLower() != "avg" ? true : false));

                        if (precision != -1)
                            sbminor.Append(")");
                        sbminor.Append(")");
                        sbminor.Append(") as [");
                        sbminor.Append(asname);
                        sbminor.Append("]");
                    }
                    _sumedcalculators.Add(asname, 0);
                }
                else//single
                {
                    sbminor.Append("(");
                    sbminor.Append(op);
                    sbminor.Append("(");
                    if (precision != -1)
                    {
                        sbminor.Append("Convert(Decimal(38,");
                        sbminor.Append(precision.ToString());
                        sbminor.Append("),");
                    }

                    sbminor.Append("[");
                    sbminor.Append(expression);
                    sbminor.Append("]");

                    if (precision != -1)
                        sbminor.Append(")");
                    sbminor.Append(")");
                    sbminor.Append(") as [");
                    sbminor.Append(asname);
                    sbminor.Append("]");
                }
            }
            else
            {
                sbminor.Append("(");
                sbminor.Append(op);
                sbminor.Append("(");
                if (precision != -1)
                {
                    sbminor.Append("Convert(Decimal(38,");
                    sbminor.Append(precision.ToString());
                    sbminor.Append("),");
                }
                sbminor.Append("[");
                sbminor.Append(asname);
                sbminor.Append("]");
                if (precision != -1)
                    sbminor.Append(")");
                sbminor.Append(")");
                sbminor.Append(") as [");
                sbminor.Append(asname);
                sbminor.Append("]");
            }
            _report.MinorAggregateString = sbminor.ToString();
            #endregion

            #region upper aggregate string
            sbminor = new StringBuilder();
            sbminor.Append(_report.UpperAggregateString);
            if (sbminor.Length > 0)
                sbminor.Append(",");

            sbminor.Append("(");
            sbminor.Append(op);
            sbminor.Append("(");
            if (precision != -1)
            {
                sbminor.Append("Convert(Decimal(38,");
                sbminor.Append(precision.ToString());
                sbminor.Append("),");
            }
            sbminor.Append("[");
            sbminor.Append(asname);
            sbminor.Append("]");
            if (precision != -1)
                sbminor.Append(")");
            sbminor.Append(")");
            sbminor.Append(") as [");
            sbminor.Append(asname);
            sbminor.Append("]");

            _report.UpperAggregateString = sbminor.ToString();
            #endregion
        }

        private void HandleLevel0String(DataSource ds, String expression, string asname, StringBuilder sb, StringBuilder sbnoas)
        {
            if (expression != null)
            {
                string ep = ReportEngine.HandleExpression(_report.DataSources, expression, "", false);
                if (ds != null && ds.IsADecimal)
                {
                    sb.Append(ep);
                    sb.Append(" as ");
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append(ep);
                }
                else
                {
                    sb.Append("isnull(");
                    sb.Append(ep);
                    sb.Append(",'') as ");
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append("isnull(");
                    sbnoas.Append(ep);
                    sbnoas.Append(",'')");
                }
            }
            else
            {
                DataType dt = DataType.String;
                if (ds != null)
                    dt = ds.Type;
                //if (dt == DataType.DateTime || (ds != null && ds.IsADecimal))
                //{
                //    sb.Append("[");
                //    sb.Append(asname);
                //    sb.Append("]");

                //    sbnoas.Append("[");
                //    sbnoas.Append(asname);
                //    sbnoas.Append("]");
                //}

                if (dt == DataType.DateTime)
                {
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append("[");
                    sbnoas.Append(asname);
                    sbnoas.Append("]");
                }
                else if (ds != null && ds.IsADecimal)
                {
                    if (cellPointLength >= 0)
                    {
                        StringBuilder sbtmp = new StringBuilder();
                        sbtmp.Append("convert(decimal(38,");
                        sbtmp.Append(cellPointLength.ToString());

                        sbtmp.Append("), [");
                        sbtmp.Append(asname);
                        sbtmp.Append("])");
                        sb.Append(sbtmp.ToString());
                        sb.Append(" as ");
                        sb.Append("[");
                        sb.Append(asname);
                        sb.Append("]");
                        sbnoas.Append(sbtmp.ToString());
                    }
                    else
                    {
                        sb.Append("[");
                        sb.Append(asname);
                        sb.Append("]");

                        sbnoas.Append("[");
                        sbnoas.Append(asname);
                        sbnoas.Append("]");
                    }
                    cellPointLength = 0;//每次使用完清空
                }
                else
                {
                    sb.Append("isnull([");
                    sb.Append(asname);
                    sb.Append("],'') as ");
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append("isnull([");
                    sbnoas.Append(asname);
                    sbnoas.Append("],'')");
                }
            }
        }

        private void AddGroupString(int level, string expression, string asname)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbnoas = new StringBuilder();
            DataSource ds = _report.DataSources[asname];
            if (level == GroupLevels)
            {
                #region minor group string
                if (_report.GroupStrings.Contains(level))
                {
                    sb.Append(_report.GroupStrings[level].ToString());
                    sb.Append(",");

                    sbnoas.Append(_report.GroupByStrings[level].ToString());
                    sbnoas.Append(",");
                }
                else if (_report.GroupStrings.Contains(0))
                {
                    sb.Append(_report.GroupStrings[0].ToString());
                    sb.Append(",");

                    sbnoas.Append(_report.GroupByStrings[0].ToString());
                    sbnoas.Append(",");
                }
                //else if (_report.GroupStrings.Contains(level - 1))
                //{
                //    sb.Append(_report.GroupStrings[level - 1].ToString());
                //    sb.Append(",");

                //    sbnoas.Append(_report.GroupByStrings[level - 1].ToString());
                //    sbnoas.Append(",");
                //}

                HandleLevel0String(ds, expression, asname, sb, sbnoas);

                if (_report.GroupStrings.Contains(level))
                {
                    _report.GroupStrings[level] = sb.ToString();
                    _report.GroupByStrings[level] = sbnoas.ToString();
                }
                else
                {
                    _report.GroupStrings.Add(level, sb.ToString());
                    _report.GroupByStrings.Add(level, sbnoas.ToString());
                }

                #endregion
            }
            else
            {
                #region upper group string
                if (_report.GroupStrings.Contains(level))
                {
                    sb.Append(_report.GroupStrings[level].ToString());
                    sb.Append(",");

                    sbnoas.Append(_report.GroupByStrings[level].ToString());
                    sbnoas.Append(",");
                }
                else if (_report.GroupStrings.Contains(level - 1))
                {
                    sb.Append(_report.GroupStrings[level - 1].ToString());
                    sb.Append(",");

                    sbnoas.Append(_report.GroupByStrings[level - 1].ToString());
                    sbnoas.Append(",");
                }

                StringBuilder sb0 = new StringBuilder();
                StringBuilder sbnoas0 = new StringBuilder();
                if (_report.GroupStrings.Contains(0))
                {
                    sb0.Append(_report.GroupStrings[0].ToString());
                    sb0.Append(",");

                    sbnoas0.Append(_report.GroupByStrings[0].ToString());
                    sbnoas0.Append(",");
                }
                HandleLevel0String(ds, expression, asname, sb0, sbnoas0);

                if (_report.GroupStrings.Contains(0))
                {
                    _report.GroupStrings[0] = sb0.ToString();
                    _report.GroupByStrings[0] = sbnoas0.ToString();
                }
                else
                {
                    _report.GroupStrings.Add(0, sb0.ToString());
                    _report.GroupByStrings.Add(0, sbnoas0.ToString());
                }

                DataType dt = DataType.String;
                if (ds != null)
                    dt = ds.Type;
                if (dt == DataType.DateTime || (ds != null && ds.IsADecimal))
                {
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append("[");
                    sbnoas.Append(asname);
                    sbnoas.Append("]");
                }
                else
                {
                    sb.Append("isnull([");
                    sb.Append(asname);
                    sb.Append("],'') as ");
                    sb.Append("[");
                    sb.Append(asname);
                    sb.Append("]");

                    sbnoas.Append("isnull([");
                    sbnoas.Append(asname);
                    sbnoas.Append("],'')");
                }

                if (_report.GroupStrings.Contains(level))
                {
                    _report.GroupStrings[level] = sb.ToString();
                    _report.GroupByStrings[level] = sbnoas.ToString();
                }
                else
                {
                    _report.GroupStrings.Add(level, sb.ToString());
                    _report.GroupByStrings.Add(level, sbnoas.ToString());
                }
                #endregion
            }
        }

        protected bool bMaxMinOrAvg(OperatorType type)
        {
            return type == OperatorType.MAX || type == OperatorType.MIN || type == OperatorType.AVG;//|| type == OperatorType.AVG;被注释导致取平均列名重复 2011720被放开
        }

        private void SuperLabelSummary(SuperLabel sl)
        {
            int index = sl.Labels.Count - 1;
            while (index >= 0)
            {
                Label l = sl.Labels[index];
                if (l.LabelType == LabelType.DetailLabel)
                {
                    sl.Labels.RemoveAt(index);
                    sl.Width -= l.Width;
                }
                else if (l is SuperLabel)
                    SuperLabelSummary(l as SuperLabel);
                index--;
            }
        }
        #endregion

        #region group handle
        public void CheckGroup(string authstring)
        {
            _report.CheckGroup(authstring, _datahelper, ReportStates.Designtime);
        }
        #endregion

        #region deeply formate handle
        protected SuperLabel AppendSuper(Cell cell)
        {
            Cell super = cell.Super;
            if (super != null)
            {
                SuperLabel sl = new SuperLabel();
                sl.Name = cell.Name + "_Super";
                sl.Caption = super.Caption;
                sl.X = cell.X;
                sl.SetY(cell.Y + sl.Height);
                sl.Width = cell.Width;
                sl.CaptionAlign = ContentAlignment.MiddleCenter;
                return sl;
            }
            return null;
        }

        protected virtual void InnerCreateReport(ShowStyle style)
        {
            BaseHandleBeforeFormalize();

            #region formalize format
            int grouplevels = 0;
            if (_report.Type != ReportType.FreeReport)
            {
                #region not free
                //change grid to free & generate group structure & add to script back columns
                //remove from script columns
                GridDetail gd = _report.Sections[SectionType.GridDetail] as GridDetail;
                Cells cells = null;
                if (gd != null)
                {
                    _report.Sections.Remove(gd);
                    cells = gd.Cells;
                }
                else
                    cells = new Cells();

                //set super---和图表相关！
                cells.SetRawSuper();
                if (gd != null && !_report.CurrentSchema.bGroupItemsAhead)
                    gd.GridDetailAutoLayOutAtRuntime();
                //}
                PageTitle pt = new PageTitle();
                pt.bAutoSequence = true;
                pt.UnderState = ReportStates.Browse;
                _report.Sections.Add(pt);

                bool hassummary = false;
                foreach (Cell cell in cells)
                {
                    if (cell is IGridCollect && (cell as IGridCollect).bSummary)
                    {
                        hassummary = true;
                        break;
                    }
                }

                Detail dtmp = new Detail();
                dtmp.UnderState = ReportStates.Browse;
                dtmp.bAutoSequence = true;
                _report.Sections.Add(dtmp);

                ReportSummary rs = null;
                if (hassummary)
                {
                    rs = new ReportSummary();
                    rs.bAutoSequence = true;
                    rs.UnderState = ReportStates.Browse;
                    _report.Sections.Add(rs);
                }

                //CheckGroup(cells);

                grouplevels = _report.CurrentSchema.SchemaItems.Count;
                int left = 0;
                int top = DefaultConfigs.SECTIONHEADERHEIGHT;
                left = 1000000 * (0 - grouplevels);
                for (int i = 0; i < grouplevels; i++)
                {
                    GroupSchemaItem gsi = _report.CurrentSchema.SchemaItems[i];
                    GroupHeader groupheader = new GroupHeader(i + 1);//gsi.Level;
                    groupheader.bAutoSequence = true;
                    groupheader.UnderState = ReportStates.Browse;
                    _report.Sections.Add(groupheader);
                    if (hassummary)
                    {
                        GroupSummary groupsummary = new GroupSummary(i + 1);//gsi.Level
                        groupsummary.bAutoSequence = true;
                        groupsummary.UnderState = ReportStates.Browse;
                        _report.Sections.Add(groupsummary);
                        groupheader.Summary = groupsummary;
                        groupsummary.Header = groupheader;
                    }
                }

                string xtitle = "";
                bool bfirst = true;
                for (int i = 0; i < grouplevels; i++)
                {
                    GroupSchemaItem gsi = _report.CurrentSchema.SchemaItems[i];
                    ArrayList al = new ArrayList();
                    #region add a group
                    bfirst = true;
                    foreach (string key in gsi.Items)
                    {
                        Cell c = cells.GetByGroupKey(key);
                        if (c != null)
                        {
                            cells.RemoveCell(c);
                            if (_report.CurrentSchema.bGroupItemsAhead)
                            {
                                //c.AppendSuperCaption();
                                c.X = left;
                                c.SetY(top);
                            }
                        }
                        else
                        {
                            c = new GridLabel(_report.DataSources[key]);
                            c.X = left;
                            c.SetY(top);
                        }
                        _report.HandleGridCellBorder(c);
                        c.CaptionAlign = ContentAlignment.MiddleLeft;
                        #region grouplabel
                        if (_report.CurrentSchema.bGroupItemsAhead)
                        {
                            SuperLabel sl = AppendSuper(c);
                            if (sl != null)
                            {
                                _report.Sections[SectionType.PageTitle].AddALabel(sl);
                            }
                        }

                        Label grouplabel = new Label(c);
                        grouplabel.LabelType = LabelType.GroupLabel;
                        grouplabel.X = c.X;
                        grouplabel.SetY(c.Y);
                        if (c is IGridEvent && (c as IGridEvent).EventType != EventType.OnTitle && (c as IGridEvent).EventType != EventType.OnAll)
                        {
                            grouplabel.PrepaintEvent = "";
                            grouplabel.ScriptID = null;
                        }
                        grouplabel.CaptionAlign = ContentAlignment.MiddleCenter;
                        _report.Sections[SectionType.PageTitle].AddALabel(grouplabel);
                        #endregion

                        if (!(c is IGroupDimensionStyle) || !(c as IGroupDimensionStyle).UseColumnStyle)
                            c.BackColor = Color.White;

                        IGroup go = null;
                        if (c is GroupDimension)
                        {
                            go = new GroupObject(c as GroupDimension);
                            AddGroupString(i + 1, null, (go as IMapName).MapName);
                            cellPointLength = -1;//数字型不应该被格式化保持原数据
                        }
                        else if (c is CalculateGroupDimension)
                        {
                            go = go = new CalculateGroupObject(c as CalculateGroupDimension);
                            AddGroupString(i + 1, (go as ICalculateColumn).Expression, (go as IMapName).MapName);
                        }
                        else if (c is GridDateTime)
                        {
                            go = new GroupObject(c as GridDateTime);
                            AddGroupString(i + 1, null, (go as IMapName).MapName);
                        }
                        else if (c is GridLabel)
                        {
                            go = new GroupObject(c as GridLabel);
                            AddGroupString(i + 1, null, (go as IMapName).MapName);
                        }
                        else if (c is GridDecimal)
                        {
                            go = new GroupObject(c as GridDecimal);
                            cellPointLength = (c as GridDecimal).PointLength;
                            AddGroupString(i + 1, null, (go as IMapName).MapName);
                        }
                        else if (c is GridColumnExpression)
                        {
                            go = new CalculateGroupObject(c as GridColumnExpression);
                            AddGroupString(i + 1, (go as ICalculateColumn).Expression, (go as IMapName).MapName);
                        }
                        else if (c is GridCalculateColumn)
                        {
                            go = new CalculateGroupObject(c as GridCalculateColumn);
                            AddGroupString(i + 1, (go as ICalculateColumn).Expression, (go as IMapName).MapName);
                        }

                        //else if (c is GridAlgorithmColumn)
                        //{
                        //    go = new AlgorithmGroupObject(c as GridAlgorithmColumn);

                        //    AddGroupString(i + 1, null, (go as IMapName).MapName);
                        //    _report.ScriptColumns.Remove((go as Cell).Name);
                        //    //when group items,must calc sourcecolumns from algorithm
                        //    string sourcecolumn = (c.Tag as LevelExpandItem).ColumnName;
                        //    scriptbackcolumns.Add((go as IMapName).MapName, sourcecolumn);

                        //}
                        if (go != null)
                        {
                            AddSortItem(go as IMapName, i + 1);
                            al.Add((go as IMapName).MapName);
                            if (c is IGridEvent && (c as IGridEvent).EventType != EventType.OnContent && (c as IGridEvent).EventType != EventType.BothContentAndSummary && (c as IGridEvent).EventType != EventType.OnAll)
                            {
                                (go as Cell).PrepaintEvent = "";
                                (go as Cell).ScriptID = null;
                            }

                            if (i == 0)
                            {
                                if (xtitle != "")
                                    xtitle += " ";
                                xtitle += (go as Cell).Caption;
                            }
                            (go as Cell).X = c.X;
                            (go as Cell).Border.Bottom = false;
                            GroupHeader groupheader = _report.Sections.GetGroupHeader(i + 1);//gsi.Level
                            groupheader.Cells.Add(go as Cell);

                            CommonLabel summarylabel = null;
                            if (hassummary)
                            {
                                summarylabel = new CommonLabel(c);
                                summarylabel.bApplyColorStyle = (c is IGroupDimensionStyle && (c as IGroupDimensionStyle).UseColumnStyle) ? false : true;
                                if (c is IInformationSender)
                                    summarylabel.InformationID = (c as IInformationSender).InformationID;
                                summarylabel.PrepaintEvent = "";
                                summarylabel.ScriptID = null;
                                if (bfirst)
                                    summarylabel.Caption = String4Report.GetString("小计", _login.LocaleID);
                                else
                                {
                                    summarylabel.Caption = SetCaption(i + 1);
                                }
                                summarylabel.LabelType = LabelType.SummaryLabel;
                                summarylabel.X = c.X;
                                summarylabel.SetY(c.Y);
                                groupheader.Summary.Cells.Add(summarylabel);
                            }

                            for (int j = 1; j <= grouplevels; j++)
                            {
                                groupheader = _report.Sections.GetGroupHeader(j);
                                if (j > i + 1)//gsi.Level
                                {
                                    summarylabel = new CommonLabel(c);
                                    summarylabel.bApplyColorStyle = false;//(c is IGroupDimensionStyle && (c as IGroupDimensionStyle).UseColumnStyle) ? false : true;
                                    summarylabel.PrepaintEvent = "";
                                    summarylabel.ScriptID = null;
                                    summarylabel.X = c.X;
                                    summarylabel.Caption = SetCaption(j);
                                    UnableTopBottonLine(summarylabel);
                                    summarylabel.LabelType = LabelType.GroupLabel;
                                    groupheader.Cells.Add(summarylabel);
                                }

                                if (hassummary && j != i + 1)//gsi.Level
                                {
                                    summarylabel = new CommonLabel(c);
                                    summarylabel.bApplyColorStyle = (c is IGroupDimensionStyle && (c as IGroupDimensionStyle).UseColumnStyle) ? false : (j > i ? false : true);
                                    if (c is IInformationSender)
                                        summarylabel.InformationID = (c as IInformationSender).InformationID;
                                    summarylabel.PrepaintEvent = "";
                                    summarylabel.ScriptID = null;
                                    summarylabel.X = c.X;
                                    summarylabel.SetY(c.Y);
                                    summarylabel.Caption = SetCaption(j);
                                    if (j > i + 1)//gsi.Level
                                    {
                                        UnableTopBottonLine(summarylabel);
                                    }
                                    summarylabel.LabelType = (j > i + 1) ? LabelType.GroupLabel : LabelType.OtherLabel;
                                    groupheader.Summary.Cells.Add(summarylabel);
                                }
                            }

                            #region detaillabel
                            summarylabel = new CommonLabel(c);
                            summarylabel.bApplyColorStyle = false;//(c is IGroupDimensionStyle && (c as IGroupDimensionStyle).UseColumnStyle) ? false : true;
                            summarylabel.PrepaintEvent = "";
                            summarylabel.ScriptID = null;
                            summarylabel.Caption = SetCaption(grouplevels + 1);
                            UnableTopBottonLine(summarylabel);
                            summarylabel.X = c.X;
                            summarylabel.SetY(c.Y);
                            summarylabel.LabelType = LabelType.GroupLabel;
                            _report.Sections[SectionType.Detail].Cells.Add(summarylabel);
                            #endregion

                            #region reportsummmarylabel
                            if (hassummary)
                            {
                                if (i == 0 && bfirst)//pagesummary......reportsummary
                                {
                                    summarylabel = new CommonLabel(c);
                                    summarylabel.bApplyColorStyle = true;//(c is IGroupDimensionStyle && (c as IGroupDimensionStyle).UseColumnStyle) ? false : true;
                                    summarylabel.PrepaintEvent = "";
                                    summarylabel.ScriptID = null;
                                    summarylabel.Caption = SummaryLabelCaption;
                                    //summarylabel.Tag = "ReportSummaryTotal";
                                    summarylabel.X = c.X;
                                    summarylabel.SetY(c.Y);
                                    summarylabel.LabelType = LabelType.SummaryLabel;
                                    _report.Sections[SectionType.ReportSummary].Cells.Add(summarylabel);
                                }
                                else
                                {
                                    summarylabel = new CommonLabel(c);
                                    summarylabel.bApplyColorStyle = true;//(c is IGroupDimensionStyle && (c as IGroupDimensionStyle).UseColumnStyle) ? false : true;
                                    summarylabel.PrepaintEvent = "";
                                    summarylabel.ScriptID = null;
                                    summarylabel.X = c.X;
                                    summarylabel.SetY(c.Y);
                                    summarylabel.Caption = "";
                                    summarylabel.LabelType = LabelType.SummaryLabel;
                                    _report.Sections[SectionType.ReportSummary].Cells.Add(summarylabel);
                                }
                            }
                            #endregion
                        }
                        left += c.Width;
                        bfirst = false;
                    }
                    #endregion
                    _report.GroupStructure.Add(i + 1, al);
                }

                Cells labels = _report.Sections[SectionType.PageTitle].Cells;
                Hashtable htsupers = new Hashtable();
                if (labels != null)
                {
                    int index = 0;
                    while (index < labels.Count)
                    {
                        Cell l = labels[index];
                        if (l is SuperLabel && l.Name.EndsWith("_Super"))
                        {
                            if (!htsupers.ContainsKey(l.Caption))
                                htsupers.Add(l.Caption, l);
                            else
                            {
                                SuperLabel pre = htsupers[l.Caption] as SuperLabel;
                                pre.Width += l.Width;
                                foreach (Label ll in (l as SuperLabel).Labels)
                                    pre.Labels.Add(ll);

                                labels.Remove(l);
                                continue;
                            }
                        }
                        index++;
                    }
                }



                Detail detail = _report.Sections[SectionType.Detail] as Detail;
                foreach (Cell cell in cells)
                {
                    Cell datacell = null;
                    int flag = 0;
                    _report.HandleGridCellBorder(cell);
                    #region type
                    switch (cell.Type.ToLower())
                    {
                        case "groupdimension":
                        case "gridlabel":
                            cell.CaptionAlign = ContentAlignment.MiddleLeft;
                            datacell = cell;//new DBText(cell as GridLabel);
                            break;
                        case "griddecimalalgorithmcolumn":
                            datacell = cell;//new DecimalAlgorithmColumn(cell as GridDecimalAlgorithmColumn);
                            flag = 4;
                            break;
                        case "gridalgorithmcolumn":
                            cell.CaptionAlign = ContentAlignment.MiddleLeft;
                            datacell = cell;//new AlgorithmColumn(cell as GridAlgorithmColumn);
                            break;
                        case "gridboolean":
                            cell.CaptionAlign = ContentAlignment.MiddleLeft;
                            datacell = cell;//new DBBoolean(cell as GridBoolean);
                            break;
                        case "gridimage":
                            //datacell=new DBImage(cell as GridImage );
                            break;
                        case "griddecimal":
                            datacell = cell;//new DBDecimal(cell as GridDecimal);
                            flag = 1;
                            break;
                        case "gridproportiondecimal":
                            datacell = cell;//new DBDecimal(cell as GridDecimal);
                            flag = 1;
                            break;
                        case "gridproportiondecimalindicator":
                            datacell = cell;
                            flag = 1;
                            break;
                        case "indicator":
                            datacell = cell;//new DBDecimal(cell as GridDecimal);
                            flag = 1;
                            break;
                        case "gridcalculatecolumn":
                            datacell = cell;//new CalculateColumn(cell as GridCalculateColumn);
                            flag = 2;
                            break;
                        case "calculateindicator":
                            datacell = cell;//new CalculateColumn(cell as GridCalculateColumn);
                            flag = 2;
                            break;
                        case "calculategroupdimension":
                        case "gridcolumnexpression":
                            cell.CaptionAlign = ContentAlignment.MiddleLeft;
                            datacell = cell;//new ColumnExpression(cell as GridColumnExpression);
                            break;
                        case "griddatetime":
                            cell.CaptionAlign = ContentAlignment.MiddleLeft;
                            datacell = cell;//new DBDateTime(cell as GridDateTime);
                            break;
                        case "gridexchangerate":
                            //datacell=new DBExchangeRate(cell);
                            break;
                        case "superlabel":
                            flag = 3;
                            break;
                    }
                    #endregion
                    AddLabel(cell);
                    string preeventstring = cell.PrepaintEvent;
                    #region AddToDetail
                    if (datacell != null)
                    {
                        if (cell is IGridEvent && (cell as IGridEvent).EventType != EventType.OnContent && (cell as IGridEvent).EventType != EventType.BothContentAndSummary && (cell as IGridEvent).EventType != EventType.OnAll)
                        {
                            (datacell as Cell).PrepaintEvent = "";
                            datacell.ScriptID = null;
                        }
                        datacell.SetY(top);
                        detail.Cells.Add(datacell);
                        if (cell is ISort)
                            AddSortItem(cell as IMapName, -1);
                    }
                    #endregion
                    if (hassummary)
                    {
                        GroupSummary gs = null;
                        CommonLabel clabel = null;
                        Calculator calc = null;
                        if (flag == 0)
                        {
                            #region flag=0
                            for (int k = 1; k <= grouplevels; k++)
                            {
                                clabel = new CommonLabel(cell);
                                clabel.bApplyColorStyle = true;
                                clabel.PrepaintEvent = "";
                                clabel.ScriptID = null;
                                clabel.Caption = SetCaption(k);
                                clabel.SetY(top);
                                clabel.LabelType = LabelType.DetailLabel;
                                _report.Sections.GetGroupSummary(k).Cells.Add(clabel);
                            }

                            clabel = new CommonLabel(cell);
                            clabel.bApplyColorStyle = true;
                            clabel.PrepaintEvent = "";
                            clabel.ScriptID = null;
                            //clabel.Tag = "ReportSummaryTotal";
                            if (bfirst)
                            {
                                clabel.Caption = SummaryLabelCaption;
                            }
                            else
                            {
                                clabel.Caption = "";
                            }
                            clabel.LabelType = LabelType.DetailLabel;
                            clabel.SetY(top);
                            rs.Cells.Add(clabel);
                            bfirst = false;
                            #endregion
                        }
                        else if (flag == 1)
                        {
                            #region flag=1
                            if (cell is IGridCollect && (cell as IGridCollect).bSummary)
                            {
                                if (cell is Indicator)
                                    calc = new CalculatorIndicator(cell as Indicator);
                                else if (cell is GridProportionDecimal)
                                    calc = new Calculator(cell as GridProportionDecimal);
                                else if (cell is GridProportionDecimalIndicator)
                                    calc = new Calculator(cell as GridProportionDecimalIndicator);
                                else
                                    calc = new Calculator(cell as GridDecimal);
                                if (cell.Super != null)
                                    calc.Caption = cell.Super.Caption + " - " + calc.Caption;
                                if (cell is IGridEvent && (cell as IGridEvent).EventType != EventType.OnSummary && (cell as IGridEvent).EventType != EventType.BothContentAndSummary && (cell as IGridEvent).EventType != EventType.OnAll)
                                {
                                    (calc as Cell).PrepaintEvent = "";
                                    calc.ScriptID = null;
                                }
                                else
                                    (calc as Cell).PrepaintEvent = preeventstring;
                                calc.SetY(top);
                                //calc.Tag = "ReportSummaryTotal";
                                rs.Cells.Add(calc);

                                for (int k = 1; k <= grouplevels; k++)
                                {
                                    gs = _report.Sections.GetGroupSummary(k);
                                    if (cell is Indicator)
                                    {
                                        (cell as Indicator).SummaryCompare = (cell as Indicator).DetailCompare == null ? (cell as Indicator).TotalCompare : (cell as Indicator).DetailCompare;
                                        calc = new CalculatorIndicator(cell as Indicator);
                                    }
                                    else
                                        calc = new Calculator(cell as GridDecimal);
                                    if (cell.Super != null)
                                        calc.Caption = cell.Super.Caption + " - " + calc.Caption;
                                    if (cell is IGridEvent && (cell as IGridEvent).EventType != EventType.OnSummary && (cell as IGridEvent).EventType != EventType.BothContentAndSummary && (cell as IGridEvent).EventType != EventType.OnAll)
                                    {
                                        (calc as Cell).PrepaintEvent = "";
                                        calc.ScriptID = null;
                                    }
                                    else
                                        (calc as Cell).PrepaintEvent = preeventstring;
                                    calc.SetY(top);
                                    gs.Cells.Add(calc);
                                }
                            }
                            else
                            {
                                for (int k = 1; k <= grouplevels; k++)
                                {
                                    gs = _report.Sections.GetGroupSummary(k);
                                    clabel = new CommonLabel(cell);
                                    clabel.bApplyColorStyle = true;
                                    clabel.PrepaintEvent = "";
                                    clabel.ScriptID = null;
                                    clabel.Caption = SetCaption(k);
                                    clabel.SetY(top);
                                    clabel.LabelType = LabelType.DetailLabel;
                                    gs.Cells.Add(clabel);
                                }

                                clabel = new CommonLabel(cell);
                                clabel.bApplyColorStyle = true;
                                clabel.PrepaintEvent = "";
                                clabel.ScriptID = null;
                                //clabel.Tag = "ReportSummaryTotal";
                                if (bfirst)
                                {
                                    clabel.Caption = SummaryLabelCaption;
                                }
                                else
                                    clabel.Caption = "";
                                clabel.SetY(top);
                                clabel.LabelType = LabelType.DetailLabel;
                                rs.Cells.Add(clabel);
                                bfirst = false;
                            }
                            #endregion
                        }
                        else if (flag == 2)
                        {
                            #region flag=2
                            if (cell is IGridCollect && (cell as IGridCollect).bSummary)
                            {
                                if (cell is CalculateIndicator)
                                    calc = new CalculatorIndicator(cell as CalculateIndicator);
                                else
                                    calc = new Calculator(cell as GridCalculateColumn);
                                if (cell.Super != null)
                                    calc.Caption = cell.Super.Caption + " - " + calc.Caption;
                                if (cell is IGridEvent && (cell as IGridEvent).EventType != EventType.OnSummary && (cell as IGridEvent).EventType != EventType.BothContentAndSummary && (cell as IGridEvent).EventType != EventType.OnAll)
                                {
                                    (calc as Cell).PrepaintEvent = "";
                                    calc.ScriptID = null;
                                }
                                else
                                    (calc as Cell).PrepaintEvent = preeventstring;
                                calc.SetY(top);
                                //calc.Tag = "ReportSummaryTotal";
                                rs.Cells.Add(calc);

                                for (int k = 1; k <= grouplevels; k++)
                                {
                                    gs = _report.Sections.GetGroupSummary(k);
                                    if (cell is CalculateIndicator)
                                    {
                                        (cell as CalculateIndicator).SummaryCompare = (cell as CalculateIndicator).DetailCompare == null ? (cell as CalculateIndicator).TotalCompare : (cell as CalculateIndicator).DetailCompare;
                                        calc = new CalculatorIndicator(cell as CalculateIndicator);
                                    }
                                    else
                                        calc = new Calculator(cell as GridCalculateColumn);
                                    if (cell.Super != null)
                                        calc.Caption = cell.Super.Caption + " - " + calc.Caption;
                                    if (cell is IGridEvent && (cell as IGridEvent).EventType != EventType.OnSummary && (cell as IGridEvent).EventType != EventType.BothContentAndSummary && (cell as IGridEvent).EventType != EventType.OnAll)
                                    {
                                        (calc as Cell).PrepaintEvent = "";
                                        calc.ScriptID = null;
                                    }
                                    else
                                        (calc as Cell).PrepaintEvent = preeventstring;
                                    calc.SetY(top);
                                    gs.Cells.Add(calc);
                                }


                            }
                            else
                            {
                                for (int k = 1; k <= grouplevels; k++)
                                {
                                    gs = _report.Sections.GetGroupSummary(k);
                                    clabel = new CommonLabel(cell);
                                    clabel.bApplyColorStyle = true;
                                    clabel.PrepaintEvent = "";
                                    clabel.ScriptID = null;
                                    clabel.Caption = SetCaption(k);
                                    clabel.SetY(top);
                                    clabel.LabelType = LabelType.DetailLabel;
                                    gs.Cells.Add(clabel);
                                }

                                clabel = new CommonLabel(cell);
                                clabel.bApplyColorStyle = true;
                                clabel.PrepaintEvent = "";
                                clabel.ScriptID = null;
                                //clabel.Tag = "ReportSummaryTotal";
                                if (bfirst)
                                {

                                    clabel.Caption = SummaryLabelCaption;
                                }
                                else
                                    clabel.Caption = "";
                                clabel.SetY(top);
                                clabel.LabelType = LabelType.DetailLabel;
                                rs.Cells.Add(clabel);
                                bfirst = false;
                            }
                            #endregion
                        }
                        else if (flag == 4)
                        {
                            #region falg=4
                            if (cell is IGridCollect && (cell as IGridCollect).bSummary)
                            {
                                for (int k = 1; k <= grouplevels; k++)
                                {
                                    gs = _report.Sections.GetGroupSummary(k);
                                    calc = new Calculator(cell as GridDecimalAlgorithmColumn);
                                    if (cell.Super != null)
                                        calc.Caption = cell.Super.Caption + " - " + calc.Caption;
                                    if (cell is IGridEvent && (cell as IGridEvent).EventType != EventType.OnSummary && (cell as IGridEvent).EventType != EventType.BothContentAndSummary && (cell as IGridEvent).EventType != EventType.OnAll)
                                    {
                                        (calc as Cell).PrepaintEvent = "";
                                        calc.ScriptID = null;
                                    }
                                    else
                                        (calc as Cell).PrepaintEvent = preeventstring;
                                    calc.SetY(top);
                                    gs.Cells.Add(calc);
                                }

                                calc = new Calculator(cell as GridDecimalAlgorithmColumn);
                                if (cell.Super != null)
                                    calc.Caption = cell.Super.Caption + " - " + calc.Caption;
                                if (cell is IGridEvent && (cell as IGridEvent).EventType != EventType.OnSummary && (cell as IGridEvent).EventType != EventType.BothContentAndSummary && (cell as IGridEvent).EventType != EventType.OnAll)
                                {
                                    (calc as Cell).PrepaintEvent = "";
                                    calc.ScriptID = null;
                                }
                                else
                                    (calc as Cell).PrepaintEvent = preeventstring;
                                calc.SetY(top);
                                //calc.Tag = "ReportSummaryTotal";
                                rs.Cells.Add(calc);
                            }
                            else
                            {
                                for (int k = 1; k <= grouplevels; k++)
                                {
                                    gs = _report.Sections.GetGroupSummary(k);
                                    clabel = new CommonLabel(cell);
                                    clabel.bApplyColorStyle = true;
                                    clabel.PrepaintEvent = "";
                                    clabel.ScriptID = null;
                                    clabel.Caption = SetCaption(k);
                                    clabel.SetY(top);
                                    clabel.LabelType = LabelType.DetailLabel;
                                    gs.Cells.Add(clabel);
                                }
                                clabel = new CommonLabel(cell);
                                clabel.bApplyColorStyle = true;
                                clabel.PrepaintEvent = "";
                                clabel.ScriptID = null;
                                //clabel.Tag = "ReportSummaryTotal";
                                if (bfirst)
                                {
                                    clabel.Caption = SummaryLabelCaption;
                                }
                                else
                                    clabel.Caption = "";
                                clabel.SetY(top);
                                clabel.LabelType = LabelType.DetailLabel;
                                rs.Cells.Add(clabel);
                                bfirst = false;
                            }
                            #endregion
                        }
                    }
                    if (datacell is IIndicator)
                        (datacell as IIndicator).SummaryCompare = null;
                }
                //if (!_report.bShowDetail)
                //    _report.Sections[SectionType.Detail].Cells.Clear();
                //LayOut
                if (pt != null)
                    CheckSuperLabel(pt.Cells);
                //AdjustLayOut();
                if (_report.CurrentSchema.bGroupItemsAhead)
                    _report.Sections.AutoLayOutAtRuntimeAll();
                else
                    _report.Sections[SectionType.PageTitle].PageTitleAutoLayOutAtRuntime();
                #endregion
            }
            else
            {
                #region free
                //generate group structure & add to script back columns
                int index = _report.Sections.Count - 1;
                while (index >= 0)
                {
                    Section section = _report.Sections[index];
                    if (section is GroupHeader)
                    {
                        bool bnull = true;
                        foreach (Cell cell in section.Cells)
                        {
                            if (cell is IGroup)
                            {
                                bnull = false;
                                break;
                            }
                        }
                        if (bnull)
                        {
                            _report.Sections.Remove(section);
                            _report.Sections.Remove(_report.Sections.GetGroupSummary(section.Level));
                            index--;
                        }
                    }

                    index--;
                }
                grouplevels = GroupLevels;
                for (int i = 1; i <= grouplevels; i++)
                {
                    GroupHeader gh = _report.Sections.GetGroupHeader(i);
                    ArrayList al = new ArrayList();
                    GroupSchemaItem item = new GroupSchemaItem();
                    item.Level = i;
                    foreach (Cell cellinheader in gh.Cells)
                    {
                        if (cellinheader is IGroup)
                        {
                            if (cellinheader is ICalculateColumn)
                            {
                                AddGroupString(i, (cellinheader as ICalculateColumn).Expression, (cellinheader as IMapName).MapName);
                            }
                            else
                            {
                                AddGroupString(i, null, (cellinheader as IMapName).MapName);
                            }
                            AddSortItem(cellinheader as IMapName, i);
                            al.Add((cellinheader as IMapName).MapName);
                            item.Items.Add(cellinheader.Name);
                        }
                        else if (cellinheader is ISort)
                        {
                            AddSortItem(cellinheader as IMapName, -1);
                        }
                        if (cellinheader is IMapName &&
                           (_report.BalanceColumns.Contains((cellinheader as IMapName).MapName) ||
                           _report.ComplexColumns.Contains((cellinheader as IMapName).MapName) ||
                           _report.AccumulateColumns.Contains((cellinheader as IMapName).MapName)))
                        {
                            if (gh.TmpCells == null)
                                gh.TmpCells = new SequenceCells();
                            gh.TmpCells.Add(cellinheader);
                        }
                    }
                    _report.GroupStructure.Add(i, al);
                    _report.CurrentSchema.SchemaItems.Add(item);
                }
                //自由报表合并单元格--1
                if (_report.CurrentSchema != null)
                    _report.CurrentSchema.ShowStyle = ShowStyle.Normal;
                Section detail = _report.Sections[SectionType.Detail];
                if (detail != null)
                {
                    foreach (Cell cellinheader in detail.Cells)
                    {
                        if (cellinheader is ISort)
                            AddSortItem(cellinheader as IMapName, -1);
                    }
                }
                #endregion
            }

            #region page by group
            if (grouplevels == 0 ||
                (grouplevels == 1 && !_report.bShowDetail))
                _report.bPageByGroup = false;
            #endregion
            #endregion

            SortReport(false, style);
        }

        protected int GroupLevels
        {
            get
            {
                //考虑了交叉行分组
                if (_report.CurrentSchemaID != null &&
                    !_report.bFree &&
                    _report.CrossSchemas.Contains(_report.CurrentSchemaID))
                {
                    if (_report.CurrentCrossSchema != null && _report.CurrentCrossSchema.CrossRowGroup != null)
                    {
                        return _report.CurrentCrossSchema.CrossRowGroup.SchemaItems.Count;
                    }
                    else return _report.GroupLevels;
                }
                if (!_report.bFree && _report.CurrentSchema != null)
                    return _report.CurrentSchema.SchemaItems.Count;
                return _report.GroupLevels;
            }
        }

        protected virtual void UnableTopBottonLine(Cell cell)
        {
            cell.Border.Top = false;
            cell.Border.Bottom = false;
        }

        protected virtual string SummaryLabelCaption
        {
            get
            {
                return "";
            }
        }

        protected virtual void BaseHandleBeforeFormalize()
        {
            #region compile
            if (_assembly == null)// || _report.Type == ReportType.CrossReport)
            {
                Compile(false);
            }
            #endregion

            #region report Init Event
            if (!string.IsNullOrEmpty(_report.InitEvent))
            {
                try
                {
                    IReportInitEvent ice = ((IReportInitEvent)ScriptHelper.FindInterface(_assembly,
                        _scripthelper.NameSpacePrefix + ".ReportInitEvent_Script"));
                    ice.Init(_report, _report.FltSrv, _report.Args, _datahelper, null);
                    if (!string.IsNullOrEmpty(_report.RowFilter.FilterString) && _report.RowFilter.FilterString.ToLower().Contains("cell94"))
                        _report.RowFilter.FilterString = "";
                }
                catch
                {
                }
            }
            #endregion

            #region rowfilter.filterstring
            if (!string.IsNullOrEmpty(_report.RowFilter.MapKeys))
            {
                try
                {
                    IMapKeys ice = ((IMapKeys)ScriptHelper.FindInterface(_assembly, _scripthelper.NameSpacePrefix + ".MapKeys_Script"));
                    _report.RowFilter.FilterString = ice.MapKeys(null, _report.FltSrv, _report.Args, _datahelper, null);
                }
                catch
                {
                    _report.RowFilter.FilterString = "";
                }
            }
            #endregion
        }

        protected void AddLabel(Cell cell)
        {
            bool bsingle = true;
            if (cell is SuperLabel || cell.Super != null)
                bsingle = false;
            Label label = null;
            if (cell is SuperLabel)
            {
                label = (cell as ICloneable).Clone() as Label;
                //				label.BackColor =DefaultConfigs.DefaultTitleBackColor;
            }
            else
                label = new Label(cell);
            if (label != null)
            {
                if (cell is IGridEvent && (cell as IGridEvent).EventType != EventType.OnTitle && (cell as IGridEvent).EventType != EventType.OnAll)
                {
                    label.PrepaintEvent = "";
                    label.ScriptID = null;
                }

                if (cell is IGridCollect && (cell as IGridCollect).bSummary)
                    label.LabelType = LabelType.SummaryLabel;

                //label.DefaultHeight();
                if (label.Tag != null)
                {
                    string design = label.Tag.ToString();
                    if (design.Contains("++++"))
                    {
                        string[] designs = design.Split(new string[] { "++++" }, StringSplitOptions.RemoveEmptyEntries);
                        if (designs.Length > 1)
                        {
                            label.DesignCaption = designs[0];
                            label.DesignName = designs[1];
                        }
                        else if (designs.Length == 1)
                        {
                            if (design.StartsWith("++++"))
                            {
                                label.DesignName = designs[0];
                            }
                            else
                            {
                                label.DesignCaption = designs[0];
                            }
                        }
                    }
                    else
                        label.DesignCaption = design;
                }
                else if (_report.Type == ReportType.GridReport)
                {
                    if (cell is IGridCollect)
                    {
                        if ((cell as IGridCollect).bSummary)
                        {
                            label.DesignCaption = label.Caption;
                            label.DesignName = label.Name;
                        }
                        else
                            label.DesignCaption = "____";
                    }
                }

                label.SetY(DefaultConfigs.SECTIONHEADERHEIGHT);
                label.CaptionAlign = ContentAlignment.MiddleLeft;
                if (bsingle)
                    _report.Sections[SectionType.PageTitle].Cells.Add(label);
                else
                    _report.Sections[SectionType.PageTitle].AddALabel(label);
            }
        }

        private void CheckSuperLabel(Cells cells)
        {
            int i = 0;
            while (i < cells.Count)
            {
                Cell cell = cells[i];
                if (cell is SuperLabel && CheckASuperLabel(cell as SuperLabel))
                    cells.RemoveAt(i);
                else
                    i++;
            }
        }

        private bool CheckASuperLabel(SuperLabel sl)
        {
            int i = 0;
            while (i < sl.Labels.Count)
            {
                Label l = sl.Labels[i];
                if (l is SuperLabel && CheckASuperLabel(l as SuperLabel))
                    sl.Labels.RemoveAt(i);
                else
                    i++;
            }
            return sl.Labels.Count == 0;
        }

        public virtual void SortReport(bool bonlyindex, ShowStyle style)
        {
            System.Diagnostics.Trace.WriteLine("Prepair data begin");
            PrepairData(bonlyindex, style);
            System.Diagnostics.Trace.WriteLine("Prepair data end");
            InnerPageTo(_defaultpageindex, _report.RowsCount - 1, style);
        }
        #endregion

        #region generate results


        /// <summary>
        /// report的排序内容从分组取
        /// </summary>
        private void SetReportSortFromGroupOrCross()
        {
            //提前赋值
            //自由视图不做处理
            //V12.5开发过程中，由于支持问题，这里先放开，有问题再看。
            //if (_report.Type == ReportType.FreeReport)
            //    return;
            if (_report.CurrentSchema.SortSchema != null &&
                _report.CurrentSchema.SortSchema.QuickSortItems.Count > 0)
            {
                _report.SortSchema.Clear();
                foreach (QuickSortItem item in _report.CurrentSchema.SortSchema.QuickSortItems)
                {
                    if (_report.DataSources.Contains(item.Name))
                    {
                        QuickSortItem newItem = new QuickSortItem(item.Level, item.Name, item.SortDirection, item.Priority);
                        _report.SortSchema.QuickSortItems.Add(newItem);
                    }
                }
            }
        }


        protected void PrepairData(int flag, ShowStyle style)
        {
            string wherestring = WhereString;

            #region calc in database
            int grouplevels = GroupLevels;
            StringBuilder sb = new StringBuilder();
            if (flag != 1)//从分组来的，report.sortschema已经赋值了
            {
                SetReportSortFromGroupOrCross();
            }
            if (_report.bShowDetail)//显示明细
            {
                #region baseid
                //if (!_report.BaseTable.Contains("UFReport.."))
                if (flag < 2 && _report.BaseTable.Contains(".."))
                {
                    string basetablename = _report.BaseTableNoneHeader;
                    sb.Append(" IF not EXISTS (SELECT * FROM dbo.syscolumns WHERE id = OBJECT_ID(N'[");
                    sb.Append(basetablename);
                    sb.Append("]') AND NAME=N'");
                    sb.Append(_report.BaseID);
                    sb.Append("')");
                    sb.Append(" alter table ");
                    sb.Append(basetablename);
                    sb.Append(" add ");
                    sb.Append(_report.BaseID);
                    sb.Append(" int identity(0,1) not null ; \r\n");
                    SqlHelper.ExecuteNoneQueryNoneTransaction(_login.TempDBCnnString, sb.ToString());
                    sb = new StringBuilder();
                }
                #endregion
                if (flag != 1)//(!bonlyindex)
                {
                    #region minor
                    if (grouplevels > 0)
                    {
                        #region edit by 马腾飞 先创建表结构，再执行插入操作
                        #region 原代码逻辑
                        //AddDropStringBeforeCreate(_report.BaseTableInTemp + "_" + grouplevels.ToString());
                        //sb.Append("select ");
                        //if (_report.GroupStrings[grouplevels] == null && _report.bFree)
                        //    throw new Exception(String4Report.GetString("您无权查询该表数据!(字段权限)", this._login.LocaleID));
                        //sb.Append(_report.GroupStrings[grouplevels].ToString());
                        //if (!string.IsNullOrEmpty(_report.MinorAggregateString))
                        //    sb.Append(",");
                        //sb.Append(_report.MinorAggregateString);
                        //sb.Append(" into ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_");
                        //sb.Append(grouplevels.ToString());
                        ////sb.Append(GetTargetTableCol(grouplevels, wherestring));
                        //sb.Append(" from ");
                        //sb.Append(_report.BaseTable);
                        //sb.Append(wherestring);
                        //sb.Append(" group by ");
                        //sb.Append(_report.GroupByStrings[grouplevels].ToString());
                        ////Ron 2011623
                        //if (GetGroupFilterString().Length > 0)
                        //{
                        //    sb.Append(" having ");
                        //    //sb.Append(GetGroupFilterString().Replace("sum.", "sum(").Replace(",", "),"));
                        //    sb.Append(HandleSumInGroupFilter(GetGroupFilterString()));
                        //}//Ron 2011623
                        ////sb.Append("\r\n  ;  \r\n");
                        //SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                        //sb = new StringBuilder();
                        #endregion
                        AddDropStringBeforeCreate(_report.BaseTableInTemp + "_" + grouplevels.ToString());
                        if (_report.GroupStrings[grouplevels] == null && _report.bFree)
                            throw new Exception(String4Report.GetString("您无权查询该表数据!(字段权限)", this._login.LocaleID));

                        string targetTableCols = this.GetTargetTableCol(grouplevels);
                        string targetTableName = this.GetTargetTableName(grouplevels);
                        string sourceTableName = _report.BaseTable;
                        string whereExpression = wherestring;
                        string groupByExpression = _report.GroupByStrings[grouplevels].ToString();
                        if (GetGroupFilterString().Length > 0)
                        {
                            groupByExpression = groupByExpression + " having " +
                                                HandleSumInGroupFilter(GetGroupFilterString());
                        }
                        this.ExecuteInsertNewTable(new string[] { targetTableCols }, targetTableName, sourceTableName, whereExpression,
                                                   groupByExpression, null);
                        #endregion
                        sb = new StringBuilder();
                    }
                    #endregion

                    #region upper
                    for (int i = grouplevels - 1; i > 0; i--)
                    {
                        #region edit by 马腾飞 先创建表结构，在执行插入操作
                        #region 原代码逻辑
                        //AddDropStringBeforeCreate(_report.BaseTableInTemp + "_" + i.ToString());
                        //sb.Append("select ");
                        //sb.Append(_report.GroupStrings[i].ToString());
                        //if (!string.IsNullOrEmpty(_report.UpperAggregateString))
                        //    sb.Append(",");
                        //sb.Append(_report.UpperAggregateString);
                        //sb.Append(" into ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_");
                        //sb.Append(i.ToString());
                        //sb.Append(" from ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_");
                        //sb.Append(Convert.ToString(i + 1));
                        //sb.Append(" group by ");
                        //sb.Append(_report.GroupByStrings[i].ToString());
                        ////sb.Append("\r\n  ;  \r\n");
                        //SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                        //sb = new StringBuilder();
                        #endregion
                        AddDropStringBeforeCreate(_report.BaseTableInTemp + "_" + i.ToString());

                        string targetTableCols = _report.GroupStrings[i].ToString();
                        if (!string.IsNullOrEmpty(_report.UpperAggregateString))
                        {
                            targetTableCols = targetTableCols + "," + _report.UpperAggregateString;
                        }
                        string targetTableName = this.GetTargetTableName(i);
                        string sourceTableName = this.GetTargetTableName(i + 1);
                        string groupByExpression = _report.GroupByStrings[i].ToString();
                        this.ExecuteInsertNewTable(new string[] { targetTableCols }, targetTableName, sourceTableName, null, groupByExpression, null);
                        sb = new StringBuilder();
                        #endregion

                    }
                    #endregion
                }

                #region check complexpage
                if (flag != 3 && bComplexPage)
                {
                    if (sb.Length > 0)
                    {
                        SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                        sb = new StringBuilder();
                    }
                    ComplexHandle();
                }
                #endregion

                #region index
                if (flag < 2)
                {
                    if (!_report.bPageByGroup)
                    {
                        #region not page by group
                        StringBuilder sbtmp = new StringBuilder();
                        sbtmp.Append("select count(*) from ");
                        sbtmp.Append(_report.BaseTable);
                        sbtmp.Append(wherestring);
                        _report.RowsCount = Convert.ToInt32(SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbtmp.ToString()));
                        if (grouplevels > 0)
                        {
                            sbtmp = new StringBuilder();
                            sbtmp.Append("select count(*) from ");
                            sbtmp.Append(_report.BaseTableInTemp);
                            sbtmp.Append("_1");
                            _report.GroupsCount = Convert.ToInt32(SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbtmp.ToString()));
                        }
                        else
                        {
                            _report.GroupsCount = -1;
                        }

                        AddDropStringBeforeCreate(_report.BaseTableInTemp + "_index");
                        #region edit by 马腾飞 先创建表结构，在执行插入操作
                        #region 原代码逻辑
                        //sb.Append(" select identity(int,0,1) as index__id,* ");
                        //sb.Append(" into ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_index from (");
                        //sb.Append(" select top ");
                        //sb.Append(_report.RowsCount.ToString());
                        //sb.Append(" ");
                        //sb.Append(_report.BaseID);
                        //sb.Append(" +0 as  ");
                        //sb.Append(_report.BaseID);
                        //if (grouplevels > 0)
                        //{
                        //    sb.Append(",");
                        //    sb.Append(_report.GroupStrings[grouplevels].ToString());
                        //}
                        //sb.Append(GetCaluculateStringOnSort());

                        //sb.Append(" from ");
                        //sb.Append(_report.BaseTable);
                        //sb.Append(wherestring);

                        //if (_report.SortSchema.QuickSortItems.Count > 0)
                        //{
                        //    sb.Append(" order by ");
                        //    sb.Append(_report.SortSchema.GetSortString());
                        //}
                        //sb.Append(") A ");
                        ////sb.Append("\r\n  ;  \r\n");
                        //SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                        #endregion


                        sb.Append(" select identity(int,0,1) as index__id,* ");
                        sb.Append(" into ");
                        sb.Append(_report.BaseTableInTemp);
                        sb.Append("_index from (");
                        sb.Append(" select top ");
                        sb.Append(_report.RowsCount.ToString());
                        sb.Append(" ");
                        sb.Append(_report.BaseID);
                        sb.Append(" +0 as  ");
                        sb.Append(_report.BaseID);
                        if (grouplevels > 0)
                        {
                            sb.Append(",");
                            sb.Append(_report.GroupStrings[grouplevels].ToString());
                        }
                        sb.Append(GetCaluculateStringOnSort());
                        sb.Append(" from ");
                        sb.Append(_report.BaseTable);
                        sb.Append(")A where 1=0");
                        SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());

                        sb = new StringBuilder();
                        sb.Append("insert into ");
                        sb.Append(_report.BaseTableInTemp);
                        sb.Append("_index ");
                        sb.Append("  select * from ( select top ");
                        sb.Append(_report.RowsCount.ToString());
                        sb.Append(" ");
                        sb.Append(_report.BaseID);
                        sb.Append(" +0 as  ");
                        sb.Append(_report.BaseID);
                        if (grouplevels > 0)
                        {
                            sb.Append(",");
                            sb.Append(_report.GroupStrings[grouplevels].ToString());
                        }
                        sb.Append(GetCaluculateStringOnSort());
                        sb.Append(" from ");
                        sb.Append(_report.BaseTable);
                        sb.Append(wherestring);

                        if (_report.SortSchema.QuickSortItems.Count > 0)
                        {
                            sb.Append(" order by ");
                            sb.Append(_report.SortSchema.GetSortString());
                        }
                        sb.Append(")a");
                        SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                        sb = new StringBuilder();
                        #endregion


                        #endregion
                    }
                    else
                    {
                        #region page by group
                        if (sb.Length > 0)
                        {
                            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                            sb = new StringBuilder();
                        }
                        string orderby = string.Empty;
                        StringBuilder sbtmp = new StringBuilder();
                        sbtmp.Append("select count(*) from ");
                        sbtmp.Append(_report.BaseTableInTemp);
                        sbtmp.Append("_1");
                        _report.RowsCount = Convert.ToInt32(SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbtmp.ToString()));
                        _report.GroupsCount = _report.RowsCount;
                        AddDropStringBeforeCreate(_report.BaseTableInTemp + "_index");
                        #region edit by 马腾飞 先创建表结构，在执行插入操作
                        #region 原代码
                        //sb.Append(" select identity(int,0,1) as index__id,* ");
                        //sb.Append(" into ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_index from ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_1");
                        //orderby = _report.SortSchema.GetGroupSortStringWithPrefix(1, "");

                        //if (!string.IsNullOrEmpty(orderby) && orderby.Trim().Length > 0)
                        //{
                        //    sb.Append(" order by ");
                        //    sb.Append(orderby);
                        //}
                        #endregion
                        string colpart1 = "identity(int,0,1) as index__id";
                        string colpart2 = "*";
                        var targetTableCols = new[] { colpart1, colpart2 };
                        string targetTableName = _report.BaseTableInTemp + "_index";
                        string sourceTableName = _report.BaseTableInTemp + "_1";
                        string orderByExpression = string.Empty;
                        if (!string.IsNullOrEmpty(orderby) && orderby.Trim().Length > 0)
                            orderByExpression = " order by " + _report.SortSchema.GetGroupSortStringWithPrefix(1, "");

                        this.ExecuteInsertNewTable(targetTableCols, targetTableName, sourceTableName, null, null, orderByExpression);
                        #endregion
                        #endregion
                    }
                }
                #endregion
            }
            else//不显示明细
            {
                if (flag != 1 && flag != 3)//(!bonlyindex)
                {
                    #region minor
                    if (grouplevels > 0)
                    {
                        AddDropStringBeforeCreate(_report.BaseTableInTemp + "_" + grouplevels.ToString());

                        #region edit by 马腾飞 先创建表结构，在执行插入操作
                        #region 原代码
                        //sb.Append("select identity(int,0,1) as ");
                        //sb.Append(_report.BaseID);
                        //sb.Append(",");
                        //sb.Append(_report.GroupStrings[grouplevels].ToString());
                        //if (!string.IsNullOrEmpty(_report.MinorAggregateString))
                        //    sb.Append(",");
                        //sb.Append(_report.MinorAggregateString);
                        //sb.Append(" into ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_");
                        //sb.Append(grouplevels.ToString());
                        //sb.Append(" from ");
                        //sb.Append(_report.BaseTable);
                        //sb.Append(wherestring);
                        //sb.Append(" group by ");
                        //sb.Append(_report.GroupByStrings[grouplevels].ToString());
                        ////Ron 2011623
                        //if (GetGroupFilterString().Length > 0)
                        //{
                        //    sb.Append(" having ");
                        //    //sb.Append(GetGroupFilterString().Replace("sum.", "sum(").Replace(",", "),"));
                        //    sb.Append(HandleSumInGroupFilter(GetGroupFilterString()));
                        //}//Ron 2011623
                        ////sb.Append("\r\n  ;  \r\n");
                        //SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                        #endregion
                        sb = new StringBuilder();
                        sb.Append("identity(int,0,1) as ");
                        sb.Append(_report.BaseID);
                        string colpart1 = sb.ToString();
                        sb = new StringBuilder();
                        sb.Append(_report.GroupStrings[grouplevels]);
                        if (!string.IsNullOrEmpty(_report.MinorAggregateString))
                            sb.Append(",");
                        sb.Append(_report.MinorAggregateString);
                        string colpart2 = sb.ToString();
                        var targetTableCols = new[] { colpart1, colpart2 };
                        string targetTableName = _report.BaseTableInTemp + "_" + grouplevels.ToString();
                        string sourceTableName = _report.BaseTable;
                        string whereExpression = wherestring;
                        string groupByExpression = _report.GroupByStrings[grouplevels].ToString();
                        if (GetGroupFilterString().Length > 0)
                        {
                            groupByExpression = groupByExpression + " having " + HandleSumInGroupFilter(GetGroupFilterString());
                        }
                        this.ExecuteInsertNewTable(targetTableCols, targetTableName, sourceTableName, whereExpression, groupByExpression);
                        #endregion
                        sb = new StringBuilder();
                    }
                    #endregion

                    #region upper

                    for (int i = grouplevels - 1; i > 0; i--)
                    {
                        AddDropStringBeforeCreate(_report.BaseTableInTemp + "_" + i.ToString());

                        #region edit by 马腾飞 先创建表结构，在执行插入操作

                        #region 原代码

                        //sb.Append("select ");
                        //sb.Append(_report.GroupStrings[i].ToString());
                        //if (!string.IsNullOrEmpty(_report.UpperAggregateString))
                        //    sb.Append(",");
                        //sb.Append(_report.UpperAggregateString);
                        //sb.Append(" into ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_");
                        //sb.Append(i.ToString());
                        //sb.Append(" from ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_");
                        //sb.Append(Convert.ToString(i + 1));
                        //sb.Append(" group by ");
                        //sb.Append(_report.GroupByStrings[i].ToString());
                        ////sb.Append("\r\n  ;  \r\n");
                        //SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());

                        #endregion

                        sb = new StringBuilder();
                        sb.Append(_report.GroupStrings[i].ToString());
                        if (!string.IsNullOrEmpty(_report.UpperAggregateString))
                            sb.Append(",");
                        sb.Append(_report.UpperAggregateString);
                        string targetTableCols = sb.ToString();
                        string targetTableName = _report.BaseTableInTemp + "_" + i.ToString();
                        string sourceTableName = _report.BaseTableInTemp + "_" + Convert.ToString(i + 1);
                        string groupByExpression = _report.GroupByStrings[i].ToString();
                        this.ExecuteInsertNewTable(new string[] { targetTableCols }, targetTableName, sourceTableName, null, groupByExpression);
                        #endregion





































                        sb = new StringBuilder();
                    }
                    #endregion
                }

                #region check complexpage
                if (flag != 3 && bComplexPage)
                {
                    if (sb.Length > 0)
                    {
                        SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                        sb = new StringBuilder();
                    }
                    ComplexHandle();
                }
                #endregion

                #region index
                if (flag < 2)
                {
                    if (!_report.bPageByGroup)
                    {
                        #region not page by group
                        if (grouplevels > 0)
                        {
                            StringBuilder sbtmp = new StringBuilder();
                            sbtmp.Append("select count(*) from ");
                            sbtmp.Append(_report.BaseTableInTemp);
                            sbtmp.Append("_");
                            sbtmp.Append(grouplevels.ToString());
                            _report.RowsCount = Convert.ToInt32(SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbtmp.ToString()));

                            sbtmp = new StringBuilder();
                            sbtmp.Append("select count(*) from ");
                            sbtmp.Append(_report.BaseTableInTemp);
                            sbtmp.Append("_1");
                            _report.GroupsCount = Convert.ToInt32(SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbtmp.ToString()));

                            AddDropStringBeforeCreate(_report.BaseTableInTemp + "_index");
                            #region edit by 马腾飞
                            #region 原代码逻辑
                            //sb.Append(" select identity(int,0,1) as index__id,* ");
                            //sb.Append(" into ");
                            //sb.Append(_report.BaseTableInTemp);
                            //sb.Append("_index from (");
                            //sb.Append(" select top ");
                            //sb.Append(_report.RowsCount.ToString());
                            //sb.Append(" ");
                            //sb.Append(_report.BaseID);
                            //sb.Append(" +0 as  ");
                            //sb.Append(_report.BaseID);
                            //sb.Append(", ");
                            ////sb.Append(_report.GroupStrings[grouplevels].ToString());
                            //sb.Append(GetGroupString(grouplevels));
                            //sb.Append(" from ");
                            //sb.Append(_report.BaseTableInTemp);
                            //sb.Append("_");
                            //sb.Append(grouplevels.ToString());

                            //if (_report.SortSchema != null && _report.SortSchema.QuickSortItems.Count > 0)
                            //{
                            //    if (_report.SortSchema.GetGroupSortString(_report).Length > 0)
                            //    {
                            //        sb.Append(" order by ");
                            //        sb.Append(_report.SortSchema.GetGroupSortString(_report));
                            //    }
                            //}
                            //sb.Append(") A ");
                            ////sb.Append("\r\n  ;  \r\n");
                            //SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                            #endregion

                            sb.Append(" select identity(int,0,1) as index__id,* ");
                            sb.Append(" into ");
                            sb.Append(_report.BaseTableInTemp);
                            sb.Append("_index from (");
                            sb.Append(" select top ");
                            sb.Append(_report.RowsCount.ToString());
                            sb.Append(" ");
                            sb.Append(_report.BaseID);
                            sb.Append(" +0 as  ");
                            sb.Append(_report.BaseID);
                            sb.Append(", ");
                            sb.Append(GetGroupString(grouplevels));

                            sb.Append(" from ");
                            sb.Append(_report.BaseTableInTemp);
                            sb.Append("_");
                            sb.Append(grouplevels.ToString());
                            sb.Append(")A where 1=0");
                            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());

                            sb = new StringBuilder();
                            sb.Append("insert into ");
                            sb.Append(_report.BaseTableInTemp);
                            sb.Append("_index ");
                            sb.Append("  select * from (select top ");
                            sb.Append(_report.RowsCount.ToString());
                            sb.Append(" ");
                            sb.Append(_report.BaseID);
                            sb.Append(" +0 as  ");
                            sb.Append(_report.BaseID);
                            sb.Append(", ");
                            sb.Append(GetGroupString(grouplevels));
                            sb.Append(" from ");
                            sb.Append(_report.BaseTableInTemp);
                            sb.Append("_");
                            sb.Append(grouplevels.ToString());

                            sb.Append(wherestring);

                            if (_report.SortSchema != null && _report.SortSchema.QuickSortItems.Count > 0)
                            {
                                if (_report.SortSchema.GetGroupSortString(_report).Length > 0)
                                {
                                    sb.Append(" order by ");
                                    sb.Append(_report.SortSchema.GetGroupSortString(_report));
                                }
                            }
                            sb.Append(")a");
                            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                            #endregion
                            sb = new StringBuilder();
                        }
                        else
                        {
                            _report.RowsCount = 0;
                            _report.GroupsCount = -1;
                        }
                        #endregion
                    }
                    else
                    {
                        #region page by group
                        if (sb.Length > 0)
                        {
                            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                            sb = new StringBuilder();
                        }

                        StringBuilder sbtmp = new StringBuilder();
                        sbtmp.Append("select count(*) from ");
                        sbtmp.Append(_report.BaseTableInTemp);
                        sbtmp.Append("_1");
                        _report.RowsCount = Convert.ToInt32(SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbtmp.ToString()));
                        _report.GroupsCount = _report.RowsCount;

                        AddDropStringBeforeCreate(_report.BaseTableInTemp + "_index");
                        #region edity by 马腾飞
                        #region 原代码逻辑
                        //sb.Append(" select identity(int,0,1) as index__id,* ");
                        //sb.Append(" into ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_index from ");
                        //sb.Append(_report.BaseTableInTemp);
                        //sb.Append("_1");
                        //string orderby = string.Empty;
                        //orderby = _report.SortSchema.GetGroupSortStringWithPrefix(1, "");
                        //if (!string.IsNullOrEmpty(orderby) && orderby.Trim().Length > 0)
                        //{
                        //    sb.Append(" order by ");
                        //    sb.Append(orderby);
                        //}
                        #endregion 原代码逻辑
                        sb.Append(" select identity(int,0,1) as index__id,* ");
                        sb.Append(" into ");
                        sb.Append(_report.BaseTableInTemp);
                        sb.Append("_index from ");
                        sb.Append(_report.BaseTableInTemp);
                        sb.Append("_1 where 1=0");
                        SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());

                        sb = new StringBuilder();
                        sb.Append("insert into ");
                        sb.Append(_report.BaseTableInTemp);
                        sb.Append("_index select * from ");
                        sb.Append(_report.BaseTableInTemp);
                        sb.Append("_1");
                        string orderby = string.Empty;
                        orderby = _report.SortSchema.GetGroupSortStringWithPrefix(1, "");
                        if (!string.IsNullOrEmpty(orderby) && orderby.Trim().Length > 0)
                        {
                            sb.Append(" order by ");
                            sb.Append(orderby);
                        }

                        #endregion edity by 马腾飞























                        #endregion
                    }
                }
                #endregion
            }
            OnCheckCanceled();
            if (sb.Length > 0)
                SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
            #endregion

            #region indicatordetail
            ArrayList metrixs = new ArrayList();
            Section rh = _report.Sections[SectionType.IndicatorDetail];
            #endregion

            #region summarydata
            if (!string.IsNullOrEmpty(_report.UpperAggregateString))
            {
                sb = new StringBuilder();
                AppendSummaryDataSQLString(sb, grouplevels, wherestring);
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, sb.ToString()))
                {
                    if (reader.Read())
                    {
                        _report.SummaryData = new ReportSummaryData(reader);
                    }
                    reader.Close();
                }
                if (rh != null)
                {
                    foreach (Cell cell in rh.Cells)
                    {
                        if (cell is CalculatorIndicator)
                        {
                            PayASingleCell(cell, _report.SummaryData, null);
                            PreEvent(cell, _report.SummaryData, null);
                        }
                    }
                    foreach (Cell cell in rh.Cells)
                    {
                        if (cell is CalculatorIndicator && (cell as CalculatorIndicator).CompareValue != null)
                        {
                            if ((cell as CalculatorIndicator).CompareValue.bExpression1Script)
                            {
                                object o = CalcColumnScript(cell, _report.SummaryData, (cell as CalculatorIndicator).CompareValue.ScriptID);
                                (cell as CalculatorIndicator).CompareValue.Expression1 = o.ToString();
                            }
                            if ((cell as CalculatorIndicator).CompareValue.bExpression2Script)
                            {
                                object o = CalcColumnScript(cell, _report.SummaryData, (cell as CalculatorIndicator).CompareValue.ScriptID + "_2");
                                (cell as CalculatorIndicator).CompareValue.Expression2 = o.ToString();
                            }
                        }
                    }
                }
            }
            #endregion

            #region reportheader
            //if(rh==null)
            //    rh = _report.Sections[SectionType.ReportHeader];
            Section header = _report.Sections[SectionType.ReportHeader];
            if (rh != null || header != null)
            {
                RowData row = null;
                StringBuilder sbheader = new StringBuilder();
                sbheader.Append("select top 1 ");
                sbheader.Append(string.IsNullOrEmpty(_report.DetailString) ? "*" : _report.DetailString);
                //sbheader.Append("*");//891cwh 注释 10.1还原
                sbheader.Append(" from ");
                sbheader.Append(_report.BaseTable);
                sbheader.Append(" A");
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, sbheader.ToString()))
                {
                    if (reader.Read())
                        row = new RowData(reader);
                    else
                        row = new RowData();
                    if (rh != null)
                    {
                        foreach (Cell cell in rh.Cells)
                        {
                            if (cell is CalculatorIndicator)
                                continue;
                            if (cell is IMetrix)
                            {
                                PreEvent(cell, null, null);
                                metrixs.Add(cell);
                            }
                        }
                    }
                    if (header != null)
                    {
                        foreach (Cell cell in header.Cells)
                        {
                            PayASingleCell(cell, row, null);
                            PreEvent(cell, row, null);
                        }
                    }
                    reader.Close();
                }
            }
            #endregion

            #region pageheader
            PageHeader ph = _report.Sections[SectionType.PageHeader] as PageHeader;
            if (ph != null)
            {
                foreach (Cell cell in ph.Cells)
                {
                    PayASingleCell(cell, null, null);
                    PreEvent(cell, null, null);
                }
            }
            #endregion

            #region pagefooter
            PageFooter pf = _report.Sections[SectionType.PageFooter] as PageFooter;
            if (pf != null)
            {
                foreach (Cell cell in pf.Cells)
                {
                    PayASingleCell(cell, null, null);
                    PreEvent(cell, null, null);
                }
            }
            #endregion

            #region printpagetitle
            PrintPageTitle ppt = _report.Sections[SectionType.PrintPageTitle] as PrintPageTitle;
            if (ppt != null)
            {
                foreach (Cell cell in ppt.Cells)
                {
                    PayASingleCell(cell, null, null);
                    PreEvent(cell, null, null);
                }
            }
            #endregion

            #region printpagesummary
            PrintPageSummary pps = _report.Sections[SectionType.PrintPageSummary] as PrintPageSummary;
            if (pps != null)
            {
                foreach (Cell cell in pps.Cells)
                {
                    PayASingleCell(cell, null, null);
                    PreEvent(cell, null, null);
                }
            }
            #endregion

            #region pagetitle
            PageTitle pt = _report.Sections[SectionType.PageTitle] as PageTitle;
            if (pt != null)
            {
                foreach (Cell cell in pt.Cells)
                {
                    PreEvent(cell, null, null);
                    if (cell is SuperLabel)
                    {
                        foreach (Label l in (cell as SuperLabel).Labels)
                            PreEvent(l, null, null);
                    }


                    //if (!_report.bFree && cell is Label && !string.IsNullOrEmpty((cell as Label).DesignCaption) && (cell as Label).DesignCaption != "____")
                    //    (cell as Label).DesignCaption = cell.Caption;
                }
            }
            else if (_report.Type != ReportType.IndicatorReport)
            {
                if (_report.Type != ReportType.FreeReport)
                {
                    string caption = String4Report.GetString("CurrentView", _login.LocaleID);
                    if (!_report.bShowDetail)
                        caption += String4Report.GetString("NoDetail", _login.LocaleID);
                    caption += String4Report.GetString("NoData", _login.LocaleID);
                    pt = new PageTitle();
                    Label label = new Label(DefaultConfigs.ReportLeft, DefaultConfigs.SECTIONHEADERHEIGHT, caption);
                    label.Name = "NoCaption";
                    pt.Cells.Add(label);
                    _report.Sections.Add(pt);
                    Detail detail = new Detail();
                    detail.Cells.Add(new DBText(label));
                    _report.Sections.Add(detail);
                }
                else if (!_report.bShowDetail && grouplevels == 0)
                {
                    Section detail = _report.Sections[SectionType.Detail];
                    if (detail == null || detail.Cells.Count == 0)
                    {
                        string caption = String4Report.GetString("CurrentView", _login.LocaleID)
                            + String4Report.GetString("NoDetail", _login.LocaleID)
                            + String4Report.GetString("NoData", _login.LocaleID);
                        pt = new PageTitle();
                        Label label = new Label(DefaultConfigs.ReportLeft, DefaultConfigs.SECTIONHEADERHEIGHT, caption);
                        label.Name = "NoCaption";
                        pt.Cells.Add(label);
                        _report.Sections.Add(pt);
                    }
                }
            }
            #endregion

            #region solid sort
            if (_report.RowBalanceColumns.Count > 0 || _report.BalanceColumns.Count > 0)
                _report.SolidSort = true;
            #endregion

            #region report going
            System.Diagnostics.Trace.WriteLine("Report going");
            //_report.CurrentSchema.ShowStyle = style;
            //删除groupschemas中交叉的行分组
            //返回给前端前需要删除，由于后面又用到，所以要再添加
            bool bRemove = false;
            string currentGroupid = _report.CurrentSchemaID;
            if (_report.CurrentCrossSchema != null &&
                !_report.CurrentCrossSchema.bNoneGroup)
            {
                _report.CurrentSchemaID = _report.CurrentCrossSchema.ID;
            }
            if (_report.CurrentCrossSchema != null &&
                _report.CurrentCrossSchema.CrossRowGroup != null &&
                _report.GroupSchemas.Contains(_report.CurrentCrossSchema.CrossRowGroup))
            {
                _report.GroupSchemas.Remove(_report.CurrentCrossSchema.CrossRowGroup);
                _report.CurrentSchemaID = _report.CurrentCrossSchema.ID;
                bRemove = true;
            }
            this.ChangeGridDetailCaption2RunTime();
            OnReportGoing(_report);
            if (bRemove)
            {
                _report.GroupSchemas.Add(_report.CurrentCrossSchema.CrossRowGroup);
                _report.CurrentSchemaID = currentGroupid;
            }
            #endregion

            HandleIndicatorDetail(metrixs);
        }

        /// <summary>
        /// edit by 马腾飞
        /// 重载方法，有时创建中间表执行identity函数创建标识列,该函数只支持select into操作
        /// 增加对使用identity函数的支持
        /// </summary>
        /// <param name="targetTableColss">目标表的列字符串（第一部分为identity函数，第二部分为原表的列）</param>
        /// <param name="targetTableName">目标表的名称</param>
        /// <param name="sourceTableName">原表的名称</param>
        /// <param name="whereExpression">where条件</param>
        /// <param name="groupByExpression">groupBy条件</param>
        /// <param name="orderByExpression">orderBy条件可选参数</param>
        protected void ExecuteInsertNewTable(
            string[] targetTableColss,
            string targetTableName,
            string sourceTableName,
            string whereExpression,
            string groupByExpression,
            string orderByExpression = null)
        {
            var sb = new StringBuilder();
            sb.Append("select ");
            // 根据传递参数不同执行不同的插入操作
            // 只传递一个参数的不需要执行identity函数操作
            string targetTableCols;
            if (targetTableColss.Length > 1)
                targetTableCols = targetTableColss[0] + "," + targetTableColss[1];
            else
                targetTableCols = targetTableColss[0];
            sb.Append(targetTableCols);
            sb.Append(" into ");
            sb.Append(targetTableName);
            sb.Append(" from ");
            sb.Append(sourceTableName);
            sb.Append(" where 1 = 0 ");
            if (!string.IsNullOrEmpty(groupByExpression))
            {
                sb.Append(" group by ");
                sb.Append(groupByExpression);
            }
            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
            sb = new StringBuilder();
            sb.Append("insert into ");
            sb.Append(targetTableName);
            sb.Append(" select ");
            targetTableCols = targetTableColss.Length > 1 ? targetTableColss[1] : targetTableColss[0];
            sb.Append(targetTableCols);
            sb.Append(" from ");
            sb.Append(sourceTableName);
            if (!string.IsNullOrEmpty(whereExpression))
                sb.Append(whereExpression);
            if (!string.IsNullOrEmpty(groupByExpression))
            {
                sb.Append(" group by ");
                sb.Append(groupByExpression);
            }
            if (!string.IsNullOrEmpty(orderByExpression))
                sb.Append(orderByExpression);
            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
        }

        /// <summary>
        /// SQL优化新增方法，获取目标表名
        /// </summary>
        /// <param name="grouplevels">分组层级</param>
        /// <returns>目标表名称</returns>
        private string GetTargetTableName(int grouplevels)
        {
            return _report.BaseTableInTemp + "_" + grouplevels.ToString();
        }

        /// <summary>
        /// SQL优化新增方法，获取目标表列名集合
        /// </summary>
        /// <param name="groupLevel">分组层级</param>
        /// <returns>目标表列明字符串</returns>
        private string GetTargetTableCol(int groupLevels)
        {
            string result = _report.GroupStrings[groupLevels].ToString();
            if (!string.IsNullOrEmpty(_report.MinorAggregateString))
                result = result + "," + _report.MinorAggregateString;
            return result;
        }


































        private void ChangeGridDetailCaption2RunTime()
        {
            if (_report.GridDetailCells != null)
            {
                foreach (Cell cell in _report.GridDetailCells)
                {
                    if (cell is IUserDefine && !string.IsNullOrEmpty((cell as IUserDefine).UserDefineItem))
                        //&& (cell is IDataSource) && cell.Caption == (cell as IDataSource).DataSource.Caption)
                        cell.Caption = _datahelper.CusDefineInfo(cell.Caption, (cell as IUserDefine).UserDefineItem);
                }
            }
        }
        protected string GetGroupString(int levels)
        {
            StringBuilder sbgroup = new StringBuilder();
            for (int m = 1; m <= levels; m++)
            {
                ArrayList al = _report.GroupStructure[m] as ArrayList;
                for (int k = 0; k < al.Count; k++)
                {
                    string colname = al[k].ToString();
                    if (sbgroup.Length > 0)
                        sbgroup.Append(",");
                    sbgroup.Append("[");
                    sbgroup.Append(colname);
                    sbgroup.Append("]");
                }
            }
            return sbgroup.ToString();
        }

        protected string HandleSumInGroupFilter(string filter)
        {
            filter = filter.ToLower();
            int index1 = filter.IndexOf("sum.");
            if (index1 == -1)
                return filter;

            StringBuilder sbfilter = new StringBuilder();
            while (index1 >= 0)
            {
                sbfilter.Append(filter.Substring(0, index1));
                sbfilter.Append("sum(");
                filter = filter.Substring(index1 + 4);

                int index2 = filter.IndexOfAny(new char[] { ',', '+', '-', '*', '/', '<', '>', '=' });
                if (index2 == -1)
                {
                    sbfilter.Append(filter);
                    sbfilter.Append(")");
                    break;
                }
                else
                {
                    sbfilter.Append(filter.Substring(0, index2));
                    sbfilter.Append(")");
                    filter = filter.Substring(index2);
                }

                index1 = filter.IndexOf("sum.");
                if (index1 == -1)
                {
                    sbfilter.Append(filter);
                    break;
                }
            }
            return sbfilter.ToString();
        }


        protected virtual void AppendSummaryDataSQLString(StringBuilder sb, int grouplevels, string wherestring)
        {
            if (grouplevels > 0)
            {
                sb.Append("select ");
                sb.Append(_report.UpperAggregateString);
                sb.Append(" from ");
                sb.Append(_report.BaseTableInTemp);
                sb.Append("_1");
            }
            else
            {
                sb.Append("select ");
                sb.Append(_report.MinorAggregateString);
                sb.Append(" from ");
                sb.Append(_report.BaseTable);
                sb.Append(wherestring);
            }
        }

        protected virtual void PrepairData(bool bonlyindex, ShowStyle style)
        {
            PrepairData(bonlyindex ? 1 : 0, style);
        }

        protected virtual void HandleIndicatorDetail(ArrayList metrixs)
        {
        }

        //private void CheckU8LoginClass()
        //{
        //    if (this._login.U8LoginClass == null)
        //    {
        //        this._login.U8LoginClass = GetLoginInfo(this._login);
        //    }
        //}

        //lastindex isn't pageindex ,it's dataindex
        public void InnerPageTo(int pageindex, int lastindex, ShowStyle style)
        {
            _tmprows = new SemiRows();
            if (_pageinfos == null)
                _pageinfos = new PageInfos();

            if (_report.Type == ReportType.FreeReport)
                style = ShowStyle.Normal;
            else if (style == ShowStyle.Normal && pageindex != -1)
                style = ShowStyle.NoGroupHeader;

            //methodColorSet = getColorSetMethod();
            int tmplevel = GroupLevels;
            int nearpage = -1;
            if (bCalcPageByPage)
            {
                if (pageindex <= 0)
                {
                    nearpage = -1;
                    _pageinfo = new PageInfo(_report.bShowDetail, tmplevel);
                }
                else
                {
                    nearpage = _pageinfos.PageIndexMostNear(pageindex);
                    _pageinfo = new PageInfo(pageindex, _pageinfos[nearpage]);
                }
                _pageinfos.Add(_pageinfo);

                if (_report.bPageByGroup && nearpage >= 0)
                    _pageinfos.PageByGroupIndex = nearpage;
            }

            RestoreGlobal();

            int level = 1;
            int levels;
            if (_report.bShowDetail)
                levels = tmplevel + 1;
            else
                levels = tmplevel;

            if (levels > 0)
            {
                SqlDataReader[] readers = new SqlDataReader[levels];
                SimpleArrayList[] columns = new SimpleArrayList[levels];
                try
                {
                    #region get readers
                    int startindex = -1;
                    int endindex = -1;
                    if (_report.bPageByGroup)
                    {
                        if (!_pageinfos.bCalcPageByPage)
                        {
                            startindex = pageindex == -1 ? 0 : pageindex;
                            endindex = pageindex == -1 ? lastindex : pageindex;
                        }
                        else
                        {
                            startindex = nearpage + 1;
                            endindex = pageindex == -1 ? lastindex : pageindex;
                            _pageinfos.AppreciateIndex = pageindex == -1 ? 0 : pageindex;
                        }

                    }
                    else
                    {
                        if (!_pageinfos.bCalcPageByPage)
                        {
                            startindex = (pageindex == -1 ? 0 : pageindex) * _report.PageRecords;
                            endindex = startindex + (pageindex == -1 ? lastindex : (_report.PageRecords - 1));
                        }
                        else
                        {
                            startindex = pageindex == -1 ? 0 : ((nearpage + 1) * _report.PageRecords);
                            endindex = pageindex == -1 ? lastindex : (pageindex * _report.PageRecords + _report.PageRecords - 1);
                            _pageinfos.AppreciateIndex = pageindex == -1 ? 0 : (pageindex * _report.PageRecords);
                        }
                    }

                    StringBuilder sb = null;
                    string wherestring = WhereString;
                    for (int i = 1; i <= levels; i++)
                    {
                        if (i == levels)
                        {
                            if (_report.bShowDetail)
                            {
                                #region detail
                                sb = new StringBuilder();
                                if (!_report.bPageByGroup)
                                {
                                    #region not page by group
                                    AppendMinorSQLStringWithDetail(sb, startindex, endindex, wherestring);
                                    #endregion
                                }
                                else
                                {
                                    #region page by group
                                    sb.Append("select A.");
                                    sb.Append(_report.BaseID);
                                    if (!string.IsNullOrEmpty(_report.DetailString))
                                    {
                                        sb.Append(",");
                                        sb.Append(_report.DetailString);
                                    }
                                    sb.Append(" from ");
                                    if (!string.IsNullOrEmpty(wherestring))
                                    {
                                        sb.Append("(");
                                        sb.Append(" select * from ");
                                        sb.Append(_report.BaseTable);
                                        sb.Append(wherestring);
                                        sb.Append(")");
                                    }
                                    else
                                        sb.Append(_report.BaseTable);
                                    sb.Append(" A inner join ");
                                    sb.Append(_report.BaseTableInTemp);
                                    sb.Append("_index B on ");
                                    string orderby = string.Empty;
                                    StringBuilder sbtmp = new StringBuilder();
                                    ArrayList al = _report.GroupStructure[1] as ArrayList;
                                    for (int k = 0; k < al.Count; k++)
                                    {
                                        string colname = al[k].ToString();
                                        //sbtmp.Append(" isnull(A.[");
                                        //sbtmp.Append(colname);
                                        //sbtmp.Append("],'')=isnull(B.[");
                                        //sbtmp.Append(colname);
                                        //sbtmp.Append("],'') and ");
                                        AddJoinOnString(colname, sbtmp, "B");
                                    }
                                    string tmpstring = sbtmp.ToString();
                                    sb.Append(tmpstring.Substring(0, tmpstring.Length - 4));

                                    sb.Append(" where B.index__id>=");
                                    sb.Append(startindex.ToString());
                                    sb.Append(" and B.index__id<=");
                                    sb.Append(endindex.ToString());

                                    orderby = _report.SortSchema.GetSortStringWithPrefix("A");
                                    if (!string.IsNullOrEmpty(orderby) && orderby.Trim().Length > 0)
                                    {
                                        sb.Append(" order by ");
                                        sb.Append(orderby);
                                    }
                                    #endregion
                                }
                                OnCheckCanceled();
                                readers[0] = SqlHelper.ExecuteReader(_login.UfDataCnnString, sb.ToString());
                                #endregion
                            }
                            else
                            {
                                #region minor
                                sb = new StringBuilder();
                                if (!_report.bPageByGroup)
                                {
                                    #region not page by group
                                    AppendMinorSQLStringWithNoDetail(sb, i, startindex, endindex, wherestring);
                                    #endregion
                                }
                                else
                                {
                                    #region page by group
                                    sb.Append("select A.* from ");
                                    sb.Append(_report.BaseTableInTemp);
                                    sb.Append("_");
                                    sb.Append(i.ToString());
                                    sb.Append(" A inner join ");
                                    sb.Append(_report.BaseTableInTemp);
                                    sb.Append("_index B on ");
                                    StringBuilder sbtmp = new StringBuilder();
                                    ArrayList al = _report.GroupStructure[1] as ArrayList;
                                    for (int k = 0; k < al.Count; k++)
                                    {
                                        string colname = al[k].ToString();
                                        //sbtmp.Append(" isnull(A.[");
                                        //sbtmp.Append(colname);
                                        //sbtmp.Append("],'')=isnull(B.[");
                                        //sbtmp.Append(colname);
                                        //sbtmp.Append("],'') and ");
                                        AddJoinOnString(colname, sbtmp, "B");
                                    }
                                    string tmpstring = sbtmp.ToString();
                                    sb.Append(tmpstring.Substring(0, tmpstring.Length - 4));

                                    sb.Append(" where B.index__id>=");
                                    sb.Append(startindex.ToString());
                                    sb.Append(" and B.index__id<=");
                                    sb.Append(endindex.ToString());

                                    string orderby = string.Empty;
                                    orderby = _report.SortSchema.GetGroupSortStringWithPrefix(i, "A");
                                    if (!string.IsNullOrEmpty(orderby) && orderby.Trim().Length > 0)
                                    {
                                        sb.Append(" order by ");
                                        sb.Append(orderby);
                                    }
                                    #endregion
                                }
                                OnCheckCanceled();
                                readers[0] = SqlHelper.ExecuteReader(_login.UfDataCnnString, sb.ToString());
                                #endregion
                            }
                            //if(_report.bShowDetail)
                            //    columns[0] = GetColumns(readers[0]);
                        }
                        else
                        {
                            #region other
                            sb = new StringBuilder();
                            if (!_report.bPageByGroup)
                            {
                                #region not page by group
                                StringBuilder sbgroup = new StringBuilder();
                                StringBuilder sbtmp = new StringBuilder();
                                for (int m = 1; m <= i; m++)
                                {
                                    ArrayList al = _report.GroupStructure[m] as ArrayList;
                                    for (int k = 0; k < al.Count; k++)
                                    {
                                        string colname = al[k].ToString();
                                        //sbtmp.Append(" isnull(A.[");
                                        //sbtmp.Append(colname);
                                        //sbtmp.Append("],'')=isnull(B.[");
                                        //sbtmp.Append(colname);
                                        //sbtmp.Append("],'') and ");
                                        AddJoinOnString(colname, sbtmp, "B");

                                        if (sbgroup.Length > 0)
                                            sbgroup.Append(",");
                                        sbgroup.Append("[");
                                        sbgroup.Append(colname);
                                        sbgroup.Append("]");
                                    }
                                }

                                sb.Append("select A.* from ");
                                sb.Append(_report.BaseTableInTemp);
                                sb.Append("_");
                                sb.Append(i.ToString());
                                sb.Append(" A inner join (select ");
                                sb.Append(sbgroup.ToString());
                                //sb.Append(_report.GroupStrings[i].ToString());
                                sb.Append(" from ");
                                sb.Append(_report.BaseTableInTemp);
                                sb.Append("_index ");
                                sb.Append(" where index__id>=");
                                sb.Append(startindex.ToString());
                                sb.Append(" and index__id<=");
                                sb.Append(endindex.ToString());
                                sb.Append(" group by ");
                                //sb.Append(_report.GroupByStrings[i].ToString());
                                sb.Append(sbgroup.ToString());
                                sb.Append(") B on ");

                                string tmpstring = sbtmp.ToString();
                                sb.Append(tmpstring.Substring(0, tmpstring.Length - 4));
                                string orderby = _report.SortSchema.GetGroupSortStringWithPrefix(i, "B");
                                if (orderby != "")
                                {
                                    sb.Append(" order by ");
                                    sb.Append(orderby);
                                }
                                #endregion
                            }
                            else
                            {
                                #region page by group

                                sb.Append("select A.* from ");
                                sb.Append(_report.BaseTableInTemp);
                                sb.Append("_");
                                sb.Append(i.ToString());
                                sb.Append(" A inner join ");
                                sb.Append(_report.BaseTableInTemp);
                                sb.Append("_index B on ");
                                StringBuilder sbtmp = new StringBuilder();
                                ArrayList al = _report.GroupStructure[1] as ArrayList;
                                for (int k = 0; k < al.Count; k++)
                                {
                                    string colname = al[k].ToString();
                                    //sbtmp.Append(" isnull(A.[");
                                    //sbtmp.Append(colname);
                                    //sbtmp.Append("],'')=isnull(B.[");
                                    //sbtmp.Append(colname);
                                    //sbtmp.Append("],'') and ");
                                    AddJoinOnString(colname, sbtmp, "B");
                                }
                                string tmpstring = sbtmp.ToString();
                                sb.Append(tmpstring.Substring(0, tmpstring.Length - 4));

                                sb.Append(" where B.index__id>=");
                                sb.Append(startindex.ToString());
                                sb.Append(" and B.index__id<=");
                                sb.Append(endindex.ToString());

                                string orderby = _report.SortSchema.GetGroupSortStringWithPrefix(i, "A");
                                if (orderby != "")
                                {
                                    sb.Append(" order by ");
                                    sb.Append(orderby);
                                }

                                #endregion
                            }

                            OnCheckCanceled();
                            readers[i] = SqlHelper.ExecuteReader(_login.UfDataCnnString, sb.ToString());
                            #endregion
                            if (_report.bShowDetail)
                                columns[i] = GetColumns(readers[i]);
                        }
                    }

                    #endregion
                    RowData[] keynodes = new RowData[levels];
                    if (bReadToTable)
                    {
                        _datatable = new DataTable();
                        columns[0] = GetColumns(readers[0]);
                        foreach (string key in columns[0])
                            _datatable.Columns.Add(key);
                        while (readers[0].Read())
                        {
                            DataRow dr = _datatable.NewRow();
                            foreach (string key in columns[0])
                                dr[key] = readers[0][key].ToString();
                            _datatable.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        InnerHandle(readers, keynodes, columns, levels, level, style);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    for (int i = 0; i < readers.Length; i++)
                    {
                        if (readers[i] != null)
                        {
                            readers[i].Close();
                            readers[i] = null;
                        }
                        if (columns[i] != null)
                        {
                            columns[i].Clear();
                            columns[i] = null;
                        }
                    }
                }
            }
            //reportsummary
            //if (_report.bShowSummary && bLastPage(pageindex)
            if (
                //bLastPage(pageindex)&& 
                _report.Sections[SectionType.ReportSummary] != null)
            {
                SemiRow semirow = AddASemiRow(null, null, 0, levels, _report.SummaryData, SectionType.ReportSummary, style);
                if (semirow != null)
                    _tmprows.Add(semirow);
            }
            OnEndAll(_tmprows);

            if (pageindex != -1)
                OnCachePageInfos();
        }

        protected virtual void AppendMinorSQLStringWithDetail(StringBuilder sb, int startindex, int endindex, string wherestring)
        {
            sb.Append("select B.index__id,A.");
            sb.Append(_report.BaseID);
            if (!string.IsNullOrEmpty(_report.DetailString))
            {
                sb.Append(",");
                sb.Append(_report.DetailString);
            }
            sb.Append(" from ");
            if (!string.IsNullOrEmpty(wherestring))
            {
                sb.Append("(");
                sb.Append(" select * from ");
                sb.Append(_report.BaseTable);
                sb.Append(wherestring);
                sb.Append(")");
            }
            else
                sb.Append(_report.BaseTable);
            sb.Append(" A inner join ");
            sb.Append(_report.BaseTableInTemp);
            sb.Append("_index B on A.");
            sb.Append(_report.BaseID);
            sb.Append("=B.");
            sb.Append(_report.BaseID);
            sb.Append(" where B.index__id>=");
            sb.Append(startindex.ToString());
            sb.Append(" and B.index__id<=");
            sb.Append(endindex.ToString());
            sb.Append(" order by B.index__id");
        }

        protected virtual void AppendMinorSQLStringWithNoDetail(StringBuilder sb, int i, int startindex, int endindex, string wherestring)
        {
            sb.Append("select B.index__id,A.* from ");
            sb.Append(_report.BaseTableInTemp);
            sb.Append("_");
            sb.Append(i.ToString());
            sb.Append(" A inner join ");
            sb.Append(_report.BaseTableInTemp);
            sb.Append("_index B on A.");
            sb.Append(_report.BaseID);
            sb.Append("=B.");
            sb.Append(_report.BaseID);
            sb.Append(" where B.index__id>=");
            sb.Append(startindex.ToString());
            sb.Append(" and B.index__id<=");
            sb.Append(endindex.ToString());
            sb.Append(" order by B.index__id");
        }
        //tmplevel=1 first ;level=0 last
        private void InnerHandle(SqlDataReader[] readers, RowData[] keynodes, SimpleArrayList[] columns, int levels, int tmplevel, ShowStyle style)
        {
            OnCheckCanceled();
            RowData firstrow = null;
            if (readers.Length > 0 && readers[0] != null)
            {
                if (readers[0].Read())
                {
                    if (_report.bShowDetail)
                        firstrow = new RowData(readers[0]);
                    else
                        firstrow = new Group(levels, readers[0]);

                    if (_pageinfos.bCalcPageByPage)
                        _pageinfo.RowBalance.CurrentIndex++;
                }
            }
            while (tmplevel > 0)
            {
                int level = tmplevel % levels;
                OnCheckCanceled();

                if (keynodes[level] != null)
                {
                    #region keynodes[level] != null
                    if (level == 0)
                    {
                        AddASemiRow(keynodes, null, levels, levels, keynodes[level], SectionType.None, style);
                    }
                    else
                    {
                        if (!keynodes[level].bHandled)
                        {
                            if (style == ShowStyle.NoGroupSummary)
                            {
                                if (_pageinfos.bCalcPageByPage)
                                    keynodes[level].SemiRow = AddASemiRow(keynodes, null, level, levels, keynodes[level], SectionType.GroupHeader, style);
                                else
                                    AddASemiRow(keynodes, null, level, levels, keynodes[level], SectionType.None, style);
                            }
                            else if (style == ShowStyle.Normal)
                                keynodes[level].SemiRow = AddASemiRow(keynodes, null, level, levels, keynodes[level], SectionType.GroupHeader, style);

                            keynodes[level].bHandled = true;
                            tmplevel++;
                            continue;
                        }
                        else
                        {
                            if (style == ShowStyle.NoGroupHeader)
                                AddASemiRow(keynodes, null, level, levels, keynodes[level], SectionType.None, style);
                            else if (style == ShowStyle.Normal)
                                AddASemiRow(keynodes, keynodes[level].SemiRow, level, levels, keynodes[level], SectionType.GroupSummary, style);
                            else if (_pageinfos.bCalcPageByPage)//NoGroupSummary
                                AddASemiRow(keynodes, keynodes[level].SemiRow, level, levels, keynodes[level], SectionType.GroupSummary, style);
                        }

                        if (_pageinfos.bCalcPageByPage)
                        {
                            _pageinfo.Balance.StartNext(tmplevel);
                            _pageinfo.Accmulate.StartNext(tmplevel);
                            _pageinfo.Complex.StartNext(tmplevel);
                        }
                    }

                    //rowbalance 
                    if (level == 0 && bAllToPrevious)
                    {
                        keynodes[level].BeforeReaderClosed();
                        foreach (string key in keynodes[level].Keys)
                            _pageinfo.RowBalance.Add(key, keynodes[level][key]);
                    }

                    keynodes[level].SemiRow = null;
                    keynodes[level].MinorRow = null;
                    keynodes[level].Columns = null;
                    keynodes[level] = null;
                    #endregion
                }
                if ((level == 0 && firstrow != null)
                    || (readers[level] != null && readers[level].Read()))
                {
                    OnCheckCanceled();
                    RowData data = null;

                    if (level == 0 && firstrow != null)
                    {
                        data = firstrow;
                        firstrow = null;
                    }
                    else
                    {
                        #region read a row
                        if (_report.bShowDetail && level == 0)
                            data = new RowData(readers[level]);
                        else
                            data = new Group(level, readers[level], columns[level]);

                        if (level == 0 && _pageinfos.bCalcPageByPage)
                            _pageinfo.RowBalance.CurrentIndex++;
                        if (tmplevel == 1 && _report.bPageByGroup && _pageinfos.bCalcPageByPage)
                            _pageinfos.PageByGroupIndex++;
                        #endregion
                    }

                    if (level > 0)
                    {
                        if (firstrow != null)
                            data.MinorRow = firstrow;
                        else
                            data.MinorRow = keynodes[0];
                    }

                    keynodes[level] = data;
                    RowData upperrow = null;

                    if (level == 0 && _pageinfos.bCalcPageByPage && _pageinfo.RawData != null)
                    {
                        int dl = DiffirenceLevel(tmplevel, _pageinfo.RawData, data);
                        if (dl != -1)
                        {
                            for (int i = tmplevel - 1; i >= dl; i--)
                            {
                                _pageinfo.Complex.StartNext(dl);
                                _pageinfo.Accmulate.StartNext(dl);
                                _pageinfo.Balance.StartNext(dl);
                            }
                            if (dl == 1)
                                _pageinfo.RowBalance.StartIndex = _pageinfo.RowBalance.CurrentIndex;
                        }
                        _pageinfo.RawData = null;
                    }
                    else if (tmplevel == levels - 1 && _pageinfos.bCalcPageByPage && _pageinfo.RawData == null)
                        _pageinfo.RowBalance.StartIndex = _pageinfo.RowBalance.CurrentIndex;

                    if (tmplevel > 1)
                        upperrow = keynodes[tmplevel - 1];
                    if (!bInAGroup(tmplevel, upperrow, data))
                    {
                        tmplevel--;
                    }
                }
                else if (readers[level] != null)
                {
                    if (level == 0 && _pageinfos.bCalcPageByPage)
                    {
                        RowData tmprow = null;
                        //if (tmplevel > 0)
                        tmprow = keynodes[level];
                        if (tmprow != null)
                            tmprow.BeforeReaderClosed();
                        _pageinfo.RawData = tmprow;
                        _pageinfo.Balance.SaveState();
                        _pageinfo.Accmulate.SaveState();
                        _pageinfo.Complex.SaveState();
                    }

                    readers[level].Close();
                    readers[level] = null;
                    tmplevel--;
                }
                else
                    tmplevel--;
                //throw new Exception("当前分组下出现异常，level=" + tmplevel.ToString());
            }
        }

        private int DiffirenceLevel(int level, RowData upperrow, RowData currentrow)
        {
            if (upperrow != null && level > 1)
            {
                for (int m = 1; m < level; m++)
                {
                    ArrayList al = _report.GroupStructure[m] as ArrayList;
                    foreach (string key in al)
                    {
                        if (upperrow[key].ToString() != currentrow[key].ToString())
                            return m;
                    }
                }
            }
            return -1;
        }

        private bool bInAGroup(int level, RowData upperrow, RowData currentrow)
        {
            if (upperrow != null && level > 1)
            {
                for (int m = 1; m < level; m++)
                {
                    ArrayList al = _report.GroupStructure[m] as ArrayList;
                    foreach (string key in al)
                    {
                        DataSource ds = _report.DataSources[key];
                        bool bnotinagroup = true;
                        string us = null;
                        string cs = null;
                        if (ds.Type == DataType.DateTime)
                        {
                            object o1 = upperrow[key];
                            object o2 = currentrow[key];
                            if (o1 != DBNull.Value)
                            {
                                DateTime dt1 = Convert.ToDateTime(upperrow[key]);
                                us = dt1.ToShortDateString() + dt1.TimeOfDay.ToString();
                            }
                            else
                                us = "";
                            if (o2 != DBNull.Value)
                            {
                                DateTime dt2 = Convert.ToDateTime(currentrow[key]);
                                cs = dt2.ToShortDateString() + dt2.TimeOfDay.ToString();
                            }
                            else
                                cs = "";
                        }
                        else
                        {
                            us = upperrow[key].ToString().Trim();
                            cs = currentrow[key].ToString().Trim();
                        }
                        us = ExpressionService.HandleNullDateTime(us.ToLower());
                        cs = ExpressionService.HandleNullDateTime(cs.ToLower());
                        bnotinagroup = (us != cs);

                        if (bnotinagroup)
                            return false;
                    }
                }
            }
            return true;
        }

        private SemiRow AddASemiRow(RowData[] keynodes, SemiRow headerrow, int level, int levels, RowData data, SectionType type, ShowStyle style)
        {
            if(i1%1000==0&&i1>1)
                logger.Info("已经增加了" + i1.ToString() + "000行SemiRow");
            if (style == ShowStyle.Normal && type == SectionType.GroupSummary)
            {
                if (headerrow != null)
                {
                    GroupHeader gh = _report.Sections.GetGroupHeader(level);
                    if (gh.TmpCells != null)
                    {
                        foreach (Cell cell in gh.TmpCells)
                        {
                            cell.OldCaption = cell.Caption;
                            ScriptCalculator(cell, data, headerrow);
                            if (!string.IsNullOrEmpty(cell.PrepaintEvent) || !cell.bScriptIDEmpty)
                                PreEvent(cell, data, headerrow);
                            cell.Caption = cell.OldCaption;
                        }
                    }
                }
                headerrow = null;
            }
            OnCheckCanceled();
            Section[] sections = new Section[2];
            if (level == 0)
            {
                sections[0] = _report.Sections[type];
            }
            else if (level == levels)
            {
                if (_report.bShowDetail)
                {
                    sections[0] = _report.Sections[SectionType.Detail];
                    foreach (IMapName map in _report.ScriptColumns)
                        data.Add(map.MapName, CalcColumnScript(map as Cell, data));
                }
                else
                {
                    sections[0] = _report.Sections.GetGroupHeader(level);
                    sections[1] = _report.Sections.GetGroupSummary(level);
                }

                if (_pageinfos.bCalcPageByPage)
                {
                    //rowbalance                        
                    foreach (string key in _report.RowBalanceColumns)
                        _pageinfo.RowBalance.Add(key, data[key]);

                    //accumulate
                    foreach (string key in _report.AccumulateColumns)
                        _pageinfo.Accmulate.Add(key, data[key]);

                    //balance
                    foreach (string key in _report.BalanceColumns)
                        _pageinfo.Balance.Add(key, data[key]);

                    //complex
                    foreach (string key in _report.ComplexColumns)
                        _pageinfo.Complex.Add(key, data[key]);
                }
            }
            else
            {
                if (type == SectionType.GroupHeader)
                    sections[0] = _report.Sections.GetGroupHeader(level);
                else if (type == SectionType.GroupSummary)
                    sections[0] = _report.Sections.GetGroupSummary(level);
                else //sectiontype.None
                {
                    sections[0] = _report.Sections.GetGroupHeader(level);
                    sections[1] = _report.Sections.GetGroupSummary(level);
                    if (style == ShowStyle.NoGroupHeader && sections[1] == null)
                        sections[0] = null;
                }
            }
            if ((sections[0] == null || sections[0].Cells.Count == 0) && (sections[1] == null || sections[1].Cells.Count == 0))
                return null;
            SemiRow semirow = null;
            if (level > 0 && _pageinfos.bCalcPageByPage && keynodes != null &&
                ((level < levels && _tmprows.Count < levels - 1 && !_report.bPageByGroup && _pageinfo.RowBalance.CurrentIndex <= _pageinfos.AppreciateIndex) ||
                (_report.bPageByGroup && _pageinfos.PageByGroupIndex < _pageinfos.AppreciateIndex)))
                return semirow;
            if (headerrow != null)
                semirow = headerrow;
            else
            {
                SectionType t = type;
                if (level != levels && type == SectionType.None)
                {
                    if (style == ShowStyle.NoGroupHeader)
                        t = SectionType.GroupSummary;
                    else if (style == ShowStyle.NoGroupSummary)
                        t = SectionType.GroupHeader;
                }
                semirow = new SemiRow(level, t);
            }
            //------------------
            SequenceCells sequencecells = new SequenceCells();
            SequenceCells eventcells = new SequenceCells();
            ArrayList indicatorcells = null;
            InitIndicatorCells(ref indicatorcells);
            for (int i = 0; i <= 1; i++)
            {
                if (sections[i] != null)
                {
                    foreach (Cell cell in sections[i].Cells)
                    {
                        cell.OldCaption = cell.Caption;
                        if ((cell is Calculator || cell is AlgorithmCalculator) &&
                            _report.ScriptCalculators.Contains(cell.Name))
                            sequencecells.Add(cell);
                        else
                        {
                            if (level == 0
                                && cell is CommonLabel
                                && cell.Caption != "")
                                semirow.Add(cell.Name, cell.Caption);
                            else if (style == ShowStyle.NoGroupHeader
                                && level < levels
                                && cell is CommonLabel)
                            {
                                if (semirow.Contains(cell.Name))
                                {
                                    if (cell.Caption.Trim() != "")
                                        semirow.Add(cell.Name, "(" + cell.Caption + ")" + semirow[cell.Name].ToString());
                                    else
                                        //
                                        // pengzhzh 在小计行前面加一个空格会影响条件格式
                                        //
                                        //semirow.Add(cell.Name, " " + semirow[cell.Name].ToString());
                                        semirow.Add(cell.Name, semirow[cell.Name].ToString());
                                }
                            }
                            else
                            {
                                PayASingleCell(cell, data, semirow);
                            }
                        }
                        if (!string.IsNullOrEmpty(cell.PrepaintEvent) || !cell.bScriptIDEmpty)
                            eventcells.Add(cell);
                        else
                            cell.Caption = cell.OldCaption;

                        if (cell is IIndicator && (cell as IIndicator).CompareValue != null && (cell as IIndicator).CompareValue.bScript)
                            indicatorcells.Add(cell);
                    }
                }
            }
            foreach (Cell cell in sequencecells)
            {
                ScriptCalculator(cell, data, semirow);
            }
            AddBaseID(level, levels, semirow, data);
            if (style == ShowStyle.NoGroupHeader)
            {
                for (int i = 1; i < level; i++)
                {
                    GroupHeader gh = _report.Sections.GetGroupHeader(i);
                    foreach (Cell cell in gh.Cells)
                    {
                        if (cell is IMapName)
                        {
                            cell.OldCaption = cell.Caption;
                            PayASingleCell(cell, data, semirow);
                            if (!string.IsNullOrEmpty(cell.PrepaintEvent) || !cell.bScriptIDEmpty)
                                eventcells.Add(cell);
                            else
                                cell.Caption = cell.OldCaption;
                        }
                    }
                }
            }
            foreach (Cell cell in eventcells)
            {
                PreEvent(cell, data, semirow);
                cell.Caption = cell.OldCaption;
            }
            //if (!string.IsNullOrEmpty(_report.ReportColorSet)) preColorSet(data, semirow);
            //
            // pengzhzh 2012-07-06 当ReportColorSet为<rows /> 这样不包含colorset的时候
            // 同空串一样处理 提高效率
            //
            if (!string.IsNullOrEmpty(_report.ReportColorSet)
            && ConatainsColorSet(_report.ReportColorSet))
            {
                preColorSet(data, semirow);
            }

            HandleIndicatorCells(indicatorcells, data, semirow);
            //------------------
            if (level == 0 || headerrow != null)
                return semirow;
            if (!_pageinfos.bCalcPageByPage ||
                (!_report.bPageByGroup && _pageinfo.RowBalance.CurrentIndex >= _pageinfos.AppreciateIndex) ||
                (_report.bPageByGroup && _pageinfos.PageByGroupIndex >= _pageinfos.AppreciateIndex))
            {
                if (!_report.bPageByGroup && style != ShowStyle.NoGroupHeader && keynodes != null && _pageinfos.bCalcPageByPage
                    //&& _pageinfo.RowBalance.CurrentIndex > 0
                    && level == levels
                    && _tmprows.Count < level - 1
                    && _pageinfo.RowBalance.CurrentIndex == _pageinfos.AppreciateIndex)
                {
                    for (int i = 1; i < level; i++)
                    {
                        if (keynodes[i].SemiRow != null)
                            AddASemiRow(keynodes[i].SemiRow, style, sections[0]);
                        else
                            keynodes[i].SemiRow = AddASemiRow(null, null, i, levels, keynodes[i], _pageinfos.bCalcPageByPage ? SectionType.GroupHeader : type, style);//type//bNeedAddToHeader?SectionType.GroupHeader:type
                    }
                }
                if (!_report.bPageByGroup ||
                    !_pageinfos.bCalcPageByPage ||
                    _pageinfos.PageByGroupIndex >= _pageinfos.AppreciateIndex)
                {
                    AddASemiRow(semirow, style, sections[0]);
                }
            }
            i1++;
            return semirow;
        }

        protected virtual void InitIndicatorCells(ref ArrayList indicatorcells)
        {
        }

        protected virtual void HandleIndicatorCells(ArrayList indicatorcells, RowData data, SemiRow semirow)
        {
        }

        protected virtual void AddBaseID(int level, int levels, SemiRow semirow, RowData data)
        {
            if (_report.bShowDetail && level == levels)
                semirow.Add(_report.BaseID, data[_report.BaseID]);
        }

        private int _invalidlevel = -1;
        private bool bValidRow(SemiRow semirow, ShowStyle style, Section section)
        {
            if (_report.Type != ReportType.FreeReport || style != ShowStyle.Normal)
                return true;
            if (semirow.Level > 0)
            {
                if (_invalidlevel != -1)
                {
                    if (semirow.Level < _invalidlevel || (section.SectionType == SectionType.GroupHeader && semirow.Level == _invalidlevel))
                        _invalidlevel = -1;
                    else
                        return false;
                }
                if (section.SectionType == SectionType.GroupHeader)
                {
                    GroupHeader gh = section as GroupHeader;
                    if (gh.bShowNullGroup)
                        return true;
                    else
                    {
                        foreach (Cell cell in gh.Cells)
                        {
                            if (cell is IGroup && semirow[cell.Name].ToString().Trim() != "")
                                return true;
                        }
                        _invalidlevel = semirow.Level;
                        return false;
                    }
                }
            }
            return true;
        }
        private void AddASemiRow(SemiRow semirow, ShowStyle style, Section section)
        {
            if (!bValidRow(semirow, style, section))
                return;
            if (style == ShowStyle.NoGroupSummary
                && _pageinfos.bCalcPageByPage)
            {
                if (semirow.Level == 1 && _tmprows.Count >= 100)
                {
                    OnSemiRowsGoing(_tmprows);
                    _tmprows = new SemiRows();
                }
                _tmprows.Add(semirow);
            }
            else
            {
                _tmprows.Add(semirow);
                if (_tmprows.Count == 100)
                {
                    OnSemiRowsGoing(_tmprows);
                    _tmprows = new SemiRows();
                }
            }
        }

        private void PayASingleCell(Cell cell, RowData data, SemiRow semirow)
        {
            if (cell is Calculator)//ICalculator
            {
                if (((cell as ICalculator).Operator == OperatorType.BalanceSUM)
                    || ((cell as ICalculator).Operator == OperatorType.ComplexSUM)
                    || ((cell as ICalculator).Operator == OperatorType.AccumulateSUM)
                    || ((cell as ICalculator).Operator == OperatorType.ExpressionSUM))
                {
                    CalcScriptValue(cell, data);
                }
                else
                {
                    cell.Caption = data[(cell as IMapName).MapName].ToString();
                }
                AddToSemiRow(cell, semirow);
            }
            else if (cell is IMapName)
            {
                if (cell is IImage)
                {
                    object oi = data[(cell as IMapName).MapName];
                    if (oi == DBNull.Value)
                        cell.Caption = "";
                    else
                    {
                        try
                        {
                            cell.Caption = Convert.ToBase64String((byte[])oi);
                        }
                        catch
                        {
                            cell.Caption = "";
                        }
                    }
                }
                else
                    cell.Caption = data[(cell as IMapName).MapName].ToString();
                AddToSemiRow(cell, semirow);
            }
            else if (cell is Expression)
            {
                #region Expression
                Expression expression = cell as Expression;
                if (expression.Formula.Type == FormulaType.Filter)
                {
                    #region filter
                    try
                    {
                        string filterfunc = expression.Formula.FormulaExpression;
                        FilterSrv fs = (_report.FltSrv == null ? new FilterSrv() : _report.FltSrv);
                        string funcname = null;
                        string funckey = null;
                        int blankindex = filterfunc.IndexOf("(");
                        funcname = filterfunc.Substring(0, blankindex).Trim();
                        blankindex = filterfunc.IndexOf("\"");
                        int blankindex2 = filterfunc.LastIndexOf("\"");
                        funckey = filterfunc.Substring(blankindex + 1, blankindex2 - blankindex - 1).Trim();
                        switch (funcname)
                        {
                            case "GetValue1":
                                cell.Caption = fs.GetValue1(funckey);
                                break;
                            case "GetValue2":
                                cell.Caption = fs.GetValue2(funckey);
                                break;
                            case "GetName1":
                                cell.Caption = fs.GetName1(funckey);
                                break;
                            case "GetName2":
                                cell.Caption = fs.GetName2(funckey);
                                break;
                        }
                        //object o = ((IFltSrv)ScriptHelper.FindInterface(_assembly, _scripthelper.NameSpacePrefix + ".Filter" + ScriptHelper.ClassName(expression.Name))).GetValue(_report.FltSrv == null ? new FilterSrv() : _report.FltSrv);
                        //if (o != null)
                        //    cell.Caption = o.ToString();
                        //else
                        //    cell.Caption = "";
                    }
                    catch
                    {
                        cell.Caption = "";
                    }
                    #endregion
                }
                else if (expression.Formula.Type == FormulaType.Common)
                {
                    #region Common Formula
                    string formula = expression.Formula.FormulaExpression.ToLower();
                    switch (formula)
                    {
                        case "getusername()":
                            cell.Caption = _datahelper.UserName;
                            break;
                        case "getcopritionname()":
                            cell.Caption = _datahelper.CompanyInfo("unit name");
                            break;
                        case "date()":
                            cell.Caption = _datahelper.Date;
                            break;
                        case "month()":
                            cell.Caption = _datahelper.Month.ToString();
                            break;
                        case "year()":
                            cell.Caption = _datahelper.Year.ToString();
                            break;
                        case "day()":
                            cell.Caption = _datahelper.Day.ToString();
                            break;
                        case "accountmonth()":
                            cell.Caption = _datahelper.AccountMonth.ToString();
                            break;
                        case "accountyear()":
                            cell.Caption = _datahelper.AccountYear.ToString();
                            break;
                        case "time()":
                            cell.Caption = _datahelper.Time;
                            break;
                    }
                    #endregion
                }
                //else if (expression.Formula.Type == FormulaType.Business)
                //    cell.Caption = expression.Formula.FormulaExpression;
                else if (expression.Formula.Type == FormulaType.UserDefine)
                    #region User Define
                    cell.Caption = _datahelper.CusDefineInfo(cell.Caption, expression.Formula.FormulaExpression);
                    #endregion

                #endregion
                AddToSemiRow(cell, semirow);
            }
            else if (cell is IAlgorithm)
            {
                CalcScriptValue(cell, data);
                AddToSemiRow(cell, semirow);
            }
        }
        protected virtual Color ConvertToColor(string s)
        {
            if (s == "")
                return Color.Empty;
            try
            {
                if (s.Contains(","))
                {
                    string[] rgb = s.Split(',');
                    return Color.FromArgb(Convert.ToInt32(rgb[0]), Convert.ToInt32(rgb[1]), Convert.ToInt32(rgb[2]));
                }
                else
                {
                    return Color.FromArgb(Convert.ToInt32(s));
                }
            }
            catch
            {
            }
            return Color.Empty;
        }
        protected virtual string getAttibute(XmlNode xnode, string attr)
        {
            if (attr == "") return "";
            string s = "";
            try
            {
                s = xnode.Attributes[attr].Value.Trim();
            }
            catch
            {
            }
            return s;
        }
        protected virtual void preColorSet(RowData data, SemiRow semirow)
        {
            if (semirow == null)
                return;
            if (xmlColorSet == null)
            {
                getColorSetMethod(semirow);
            }
            if (xmlColorSet == null)
            {
                return;
            }
            //
            // pengzhzh做相应修改 支持多个输出事件
            //
            //int rowno = -1;
            List<int> rownos = new List<int>();
            object[] vparams = { data, semirow };
            try
            {
                //
                //pengzhzh 2012-06-14 修改getColorSetMethod重载 多一个参数
                //
                methodColorSet = getColorSetMethod(semirow);
                if (methodColorSet != null && methodColorSet.Count > 0 && objColorSet != null
                    && objColorSet.Count == methodColorSet.Count)
                {
                    //rowno = int.Parse(methodColorSet.Invoke(objColorSet, vparams).ToString());
                    //rownos.AddRange((List<int>)methodColorSet.Invoke(objColorSet, vparams));
                    int i = 0;
                    foreach (MethodInfo m in methodColorSet)
                    {
                        try
                        {
                            int index = -1;
                            index = (int)m.Invoke(objColorSet[i], vparams);
                            if (index >= 0)
                            {
                                rownos.Add(index);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                        ++i;
                    }
                }
            }
            catch
            {
                //rowno = -1;
            }
            //if (rowno >= 0)
            if (rownos.Count > 0)
            {
                foreach (int rowno in rownos)
                {
                    XmlNode xnode = xmlColorSet.SelectNodes(@"rows/row[@rowno='" + rowno.ToString() + "']")[0];
                    string effectrow = getAttibute(xnode, "effectrow");
                    if (effectrow != "")
                    {
                        if (effectrow.IndexOf("reportsummary", 0) < 0 && semirow.SectionType == SectionType.ReportSummary)
                            continue;
                        if (effectrow.IndexOf("groupsummary", 0) < 0 &&
                            (semirow.SectionType == SectionType.GroupSummary || semirow.SectionType == SectionType.GroupHeader))
                            continue;
                        if (effectrow.IndexOf("content", 0) < 0 && semirow.SectionType == SectionType.None)
                            continue;
                    }
                    string effectkey = getAttibute(xnode, "effectcol");
                    Color colorFore = ConvertToColor(getAttibute(xnode, "forecolor"));
                    Color colorBack = ConvertToColor(getAttibute(xnode, "backcolor"));
                    if (effectkey == "" || effectkey == "@allcolumns@")
                    {
                        if (colorFore != Color.Empty)
                        {
                            semirow.ForeColor = colorFore;
                            //
                            // 修改原先设置的color
                            //
                            //semirow.SetForeColors(colorFore);

                        }
                        if (colorBack != Color.Empty)
                        {
                            semirow.BackColor = colorBack;
                            //
                            // 修改原先设置的color
                            //
                            //semirow.SetBackColors(colorBack);
                        }
                        //
                        // pengzhzh 2012-9-24 修改配色顺序问题
                        //
                        if (semirow.Keys != null)
                        {
                            foreach (var key in semirow.Keys)
                            {
                                semirow.AddForeColor(key.ToString(), colorFore);
                                semirow.AddBackColor(key.ToString(), colorBack);
                            }
                        }
                    }
                    else
                    {
                        if (semirow.Contains(effectkey))
                        {
                            if (colorBack != Color.Empty) semirow.AddBackColor(effectkey, colorBack);
                            if (colorFore != Color.Empty) semirow.AddForeColor(effectkey, colorFore);
                        }
                    }
                }
            }
        }
        public void PreEvent(Cell cell, RowData data, SemiRow semirow)
        {
            if (_assembly == null)
                return;
            //if (semirow != null && (cell is CommonLabel))
            //    return;
            if (!string.IsNullOrEmpty(cell.PrepaintEvent) || !cell.bScriptIDEmpty)
            {
                Color oldbackcolor = cell.BackColor;
                Color oldforecolor = cell.ForeColor;
                string oldcaption = cell.Caption;
                bool oldvisible = cell.Visible;
                int oldx = cell.X;
                int oldwidth = cell.Width;
                SemiRow sr = null;
                if (data != null)
                {
                    sr = data.SemiRow;
                    data.SemiRow = semirow;
                }
                try
                {
                    object ice = ScriptHelper.FindInterface(_assembly, _scripthelper.NameSpacePrefix + ".Cell" + ScriptHelper.ClassName(cell.ScriptID));
                    if (ice != null && ice is ICellEvent)
                        (ice as ICellEvent).Prepaint(_report, data, cell, _report.FltSrv, _report.Args, _datahelper, _report.SummaryData, (_pageinfo != null ? _pageinfo.RowBalance : null), (_pageinfo != null ? _pageinfo.Accmulate : null), (_pageinfo != null ? _pageinfo.Balance : null), null);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine("Error in preevent: " + e.Message);
                }
                finally
                {
                    if (data != null)
                        data.SemiRow = sr;
                }

                if (semirow != null)
                {
                    if (cell.BackColor.ToArgb() != oldbackcolor.ToArgb())
                    {
                        semirow.AddBackColor(cell.Name, cell.BackColor);
                        cell.BackColor = oldbackcolor;
                    }
                    if (cell.ForeColor.ToArgb() != oldforecolor.ToArgb())
                    {
                        semirow.AddForeColor(cell.Name, cell.ForeColor);
                        cell.ForeColor = oldforecolor;
                    }
                    if (cell.Visible != oldvisible)
                    {
                        semirow.AddVisible(cell.Name, cell.Visible);
                        cell.Visible = oldvisible;
                    }
                    if (cell.X != oldx)
                    {
                        semirow.AddX(cell.Name, cell.X);
                        cell.X = oldx;
                    }
                    if (cell.Width != oldwidth)
                    {
                        semirow.AddWidth(cell.Name, cell.Width);
                        cell.Width = oldwidth;
                    }
                    if (cell.Caption != null && cell.Caption.ToLower() != oldcaption.ToLower())
                    {
                        try
                        {
                            if (cell is IDecimal && cell.Caption.Trim() != "" && (cell as IDecimal).PointLength != -1)
                                cell.Caption = Convert.ToDouble(cell.Caption).ToString("N" + (cell as IDecimal).PointLength.ToString());
                        }
                        catch
                        {
                        }
                        AddToSemiRow(cell, semirow);
                    }
                }
            }

            if (cell is SuperLabel)
            {
                foreach (Label l in (cell as SuperLabel).Labels)
                    PreEvent(l, data, semirow);
            }
        }

        private void AddToSemiRow(Cell cell, SemiRow semirow)
        {
            try
            {
                cell.Caption = ExpressionService.HandleNullDateTime(cell.Caption);
                if (cell is IDecimal && cell.Caption.Trim() != "")
                    cell.Caption = ExpressionService.HandleInvalidDicmal(cell.Caption);
                if (cell.Caption.Trim() != "" && _datahelper.NotBeSummaryTitle(cell.Caption))
                {
                    if (cell is IDecimal && Convert.ToDouble(cell.Caption) == 0 && ((cell as IDecimal).bShowWhenZero == StimulateBoolean.False || ((cell as IDecimal).bShowWhenZero == StimulateBoolean.None && !_report.bShowWhenZero)))
                    {
                        cell.Caption = "";
                    }
                    else if (cell is IFormat && !string.IsNullOrEmpty((cell as IFormat).FormatString))
                    {
                        if (cell is IDecimal && _report.Type == ReportType.FreeReport)
                            cell.Caption = Convert.ToDouble(cell.Caption).ToString((cell as IFormat).FormatString);
                        else if (cell is IDateTime || (cell is IBDateTime && (cell as IBDateTime).bDateTime))
                        {
                            try
                            {
                                cell.Caption = Convert.ToDateTime(cell.Caption).ToString((cell as IFormat).FormatString);
                            }
                            catch
                            {

                            }
                        }
                    }
                    else if (cell is IDateTime || (cell is IBDateTime && (cell as IBDateTime).bDateTime))
                    {
                        try
                        {
                            cell.Caption = Convert.ToDateTime(cell.Caption).ToShortDateString();
                        }
                        catch
                        {
                        }
                    }
                    // V13.0新增，如果是日期格式作为交叉点，这里需要进行格式转换。--matfb
                    else if (cell is GridColumnExpression
                        && ExpressionService.bExpressionAfterCross(((GridColumnExpression)cell).Expression)
                        && cell.Caption.Length == 18
                        && (cell.Caption.Contains("AM") || cell.Caption.Contains("PM")))
                    {
                        try
                        {
                            cell.Caption = Convert.ToDateTime(cell.Caption).ToString("yyyy-MM-dd hh:mm:ss");
                        }
                        catch
                        {
                        }
                    }
                }
            }

            catch
            {
            }
            if (semirow != null)
                semirow.Add(cell.Name, cell.Caption);
        }

        private void CalcScriptValue(Cell cell, RowData data)
        {
            try
            {
                object o = null;
                if (cell is Calculator && (cell as ICalculator).Operator == OperatorType.AccumulateSUM)
                    o = AccumulateSum(data, (cell as IMapName).MapName);
                else if (cell is Calculator && (cell as ICalculator).Operator == OperatorType.BalanceSUM)
                    o = BalanceSum(data, (cell as IMapName).MapName);
                else if (cell is Calculator && (cell as ICalculator).Operator == OperatorType.ComplexSUM)
                    o = ComplexSum(data, (cell as IMapName).MapName);
                else if (cell is Calculator && (cell as ICalculator).Operator == OperatorType.ExpressionSUM)
                    o = ExpressionSum(data, (cell as ICalculateColumn).Expression);
                else
                {
                    IAlgorithmSrv ias = ((IAlgorithmSrv)ScriptHelper.FindInterface(_assembly, _scripthelper.NameSpacePrefix + ".Algorithm" + ScriptHelper.ClassName(cell.ScriptID)));
                    o = ias.GetValue(GroupLevels, -1, -1, data, _report.FltSrv, _report.Args, _datahelper, cell, _report.DataSources, _report.SummaryData, (_pageinfo != null ? _pageinfo.RowBalance : null), (_pageinfo != null ? _pageinfo.Accmulate : null), (_pageinfo != null ? _pageinfo.Balance : null),
                        _datatable == null ? null : (new object[] { _datatable }));
                }
                if (o != null)
                    cell.Caption = o.ToString();
                else
                    cell.Caption = "";
                if (cell is IDecimal && (cell as IDecimal).PointLength != -1 && cell.Caption != "")
                {
                    cell.Caption = Convert.ToDouble(cell.Caption).ToString("N" + (cell as IDecimal).PointLength.ToString());
                }
            }
            catch (Exception e)
            {
                cell.Caption = "";
                System.Diagnostics.Trace.WriteLine("Error in CalcScriptValue: " + e.Message);
            }
        }

        public void ScriptCalculator(Cell cell, RowData data, SemiRow semirow)
        {
            CalcScriptValue(cell, data);
            AddToSemiRow(cell, semirow);
        }

        private object CalcColumnScript(Cell cell, RowData data)
        {
            return CalcColumnScript(cell, data, ScriptHelper.ClassName(cell.ScriptID));
        }

        protected object CalcColumnScript(Cell cell, RowData data, string name)
        {
            try
            {
                IAlgorithmSrv ias = ((IAlgorithmSrv)ScriptHelper.FindInterface(_assembly, _scripthelper.NameSpacePrefix + ".Algorithm" + name));
                object o = ias.GetValue(GroupLevels, -1, -1, data, _report.FltSrv, _report.Args, _datahelper, cell, _report.DataSources, _report.SummaryData, (_pageinfo != null ? _pageinfo.RowBalance : null), (_pageinfo != null ? _pageinfo.Accmulate : null), (_pageinfo != null ? _pageinfo.Balance : null),
                    _datatable == null ? null : (new object[] { _datatable }));
                if (o != null)
                    o = o.ToString();
                else
                    o = "";
                if (cell is IDecimal && (cell as IDecimal).PointLength != -1 && o.ToString() != "")
                {
                    o = Convert.ToDouble(o).ToString("N" + (cell as IDecimal).PointLength.ToString());
                }
                return o;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("Error in CalcColumnScript: " + e.Message);
                return "";
            }
        }

        private object AccumulateSum(RowData data, string mapname)
        {
            return _pageinfo.Accmulate[GetLevel(data), mapname];
        }

        private object BalanceSum(RowData data, string mapname)
        {
            return _pageinfo.Balance[GetLevel(data), mapname];
        }

        private object ComplexSum(RowData data, string mapname)
        {
            return _pageinfo.Complex[GetLevel(data), mapname];
        }

        private object ExpressionSum(RowData row, string expression)
        {
            try
            {
                expression = expression.Trim();
                char[] seps = new char[] { '+', '-', '*', '/', '(', ')' };
                Stack data = new Stack();
                Stack ops = new Stack();
                object preop = null;
                while (true)
                {
                    int index = expression.IndexOfAny(seps);
                    if (index == -1)
                    {
                        if (_report.DataSources.Contains(expression))
                            data.Push(row[expression]);
                        else if (expression.Trim() != "")
                            data.Push(expression);
                        break;
                    }

                    if (index == 0)
                    {
                        #region op
                        string op = expression.Substring(index, 1);
                        HandleAOperator(op, preop, data, ops);
                        #endregion
                    }
                    else
                    {
                        #region data
                        string exp1 = expression.Substring(0, index).Trim();
                        if (_report.DataSources.Contains(exp1))
                            data.Push(row[exp1]);
                        else
                            data.Push(exp1);

                        string op = expression.Substring(index, 1);
                        HandleAOperator(op, preop, data, ops);
                        #endregion
                    }
                    expression = expression.Substring(index + 1).Trim();
                }
                preop = null;
                if (ops.Count > 0)
                    preop = ops.Pop();
                while (preop != null)
                {
                    data.Push(CalcOnce(preop.ToString(), Convert.ToDouble(data.Pop()), Convert.ToDouble(data.Pop())));
                    preop = null;
                    if (ops.Count > 0)
                        preop = ops.Pop();
                }
                if (data.Count > 0)
                    return data.Pop();
                else
                    return 0;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("ExpressionSUM error:" + e.Message);
                return 0;
            }
        }

        private void HandleAOperator(string op, object preop, Stack data, Stack ops)
        {
            if (op == "(")
                ops.Push(op);
            else if (op == ")")
            {
                preop = null;
                if (ops.Count > 0)
                    preop = ops.Pop();
                while (preop != null && preop.ToString() != "(")
                {
                    data.Push(CalcOnce(preop.ToString(), Convert.ToDouble(data.Pop()), Convert.ToDouble(data.Pop())));
                    preop = null;
                    if (ops.Count > 0)
                        preop = ops.Pop();
                }
            }
            else if (ops.Count > 0)
            {
                preop = ops.Pop();
                while (preop != null && !PriorTo(op, preop.ToString()))
                {
                    data.Push(CalcOnce(preop.ToString(), Convert.ToDouble(data.Pop()), Convert.ToDouble(data.Pop())));
                    preop = null;
                    if (ops.Count > 0)
                        preop = ops.Pop();
                }
                if (preop != null && PriorTo(op, preop.ToString()))
                    ops.Push(preop);
                ops.Push(op);
            }
            else
                ops.Push(op);
        }

        private bool bOperator(string key)
        {
            return (key == "*" || key == "/" || key == "+" || key == "-" || key == "(" || key == ")");
        }

        private bool PriorTo(string op, string preop)
        {
            if (preop == "(")
                return true;
            return (op == "*" || op == "/") && (preop == "+" || preop == "-");
        }

        private double CalcOnce(string op, double d1, double d2)
        {
            switch (op)
            {
                case "+":
                    return d1 + d2;
                case "-":
                    return d2 - d1;
                case "*":
                    return d1 * d2;
                default:
                    return d2 / d1;
            }
        }

        private int GetLevel(RowData data)
        {
            int level;
            int grouplevels = GroupLevels;
            if (data == null || data is ReportSummaryData)
                level = 0;// (grouplevels == 0 ? 0 : 1);
            else
            {
                Group currentgroup = data as Group;
                if (grouplevels == 0)
                    level = grouplevels;
                else
                    level = currentgroup.Level;
            }
            return level;
        }

        private SimpleArrayList GetColumns(SqlDataReader reader)
        {
            if (reader != null)
            {
                SimpleArrayList al = new SimpleArrayList();
                DataTable dt = reader.GetSchemaTable();
                foreach (DataRow dr in dt.Rows)
                    al.Add(dr["ColumnName"].ToString());
                return al;
            }
            else
                return null;
        }

        private bool bLastPage(int pageindex)
        {
            if (pageindex == -1)
                return true;
            if (_report.bPageByGroup)
                return (pageindex == (_report.RowsCount == 0 ? 1 : _report.RowsCount) - 1);
            else
                return pageindex == ((_report.RowsCount / _report.PageRecords + (_report.RowsCount % _report.PageRecords == 0 ? 0 : 1)) - 1);
        }

        private string SetCaption(int level)
        {
            return "";
            //StringBuilder sb = new StringBuilder();
            //for (int i = 1; i <= level; i++)
            //{
            //    sb.Append(" ");
            //}
            //return sb.ToString();
        }

        protected string WhereString
        {
            get
            {
                string wherestring = "";
                if (NeedHandleGroupFilter || !string.IsNullOrEmpty(_report.RowFilter.FilterString) || !string.IsNullOrEmpty(_report.SolidFilter) || !string.IsNullOrEmpty(_report.InformationString))
                {
                    if (NeedHandleGroupFilter)
                        wherestring = _report.GroupFilter.ToLower().Replace("sum.", "");
                    if (!string.IsNullOrEmpty(_report.RowFilter.FilterString))
                        wherestring = (wherestring == "" ? _report.RowFilter.FilterString : wherestring + " and " + _report.RowFilter.FilterString);
                    if (!string.IsNullOrEmpty(_report.SolidFilter))
                        wherestring = (wherestring == "" ? _report.SolidFilter : wherestring + " and " + _report.SolidFilter);
                    if (!string.IsNullOrEmpty(_report.InformationString))
                        wherestring = (wherestring == "" ? _report.InformationString : wherestring + " and " + _report.InformationString);
                    wherestring = " where " + wherestring;
                }
                return wherestring;
            }
        }

        protected virtual bool NeedHandleGroupFilter
        {
            get
            {
                return _report.GroupLevels == 0 && !string.IsNullOrEmpty(_report.GroupFilter);
            }
        }

        private string GetCaluculateStringOnSort()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _report.SortSchema.QuickSortItems.Count; i++)
            {
                QuickSortItem qsi = _report.SortSchema.QuickSortItems[i];
                if (qsi.Level <= 0)
                {
                    DataSource ds = _report.DataSources[qsi.Name];
                    if (ds != null && ds.Tag != null)
                    {
                        sb.Append(",");
                        sb.Append(ReportEngine.HandleExpression(_report.DataSources, ds.Tag, "", false));
                        sb.Append(" as [");
                        sb.Append(ds.Name);
                        sb.Append("]");
                    }
                }
            }
            return sb.ToString();
        }

        private void AddDropStringBeforeCreate(string tablename)
        {
            StringBuilder sb = new StringBuilder();
            tablename = TableNameNoneHeader(tablename);
            sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
            sb.Append(tablename);
            sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)");
            sb.Append(" drop table ");
            sb.Append(tablename);
            //sb.Append("\r\n  ;  \r\n");
            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.TempDBCnnString, sb.ToString());
        }

        public static string TableNameNoneHeader(string tablename)
        {
            int index = tablename.IndexOf("..");
            if (index == -1)
                return tablename;
            else
                return tablename.Substring(index + 2);
        }

        private void ComplexHandle()
        {
            //string newtable = CustomDataSource.GetATableName();
            string newtable = CustomDataSource.GetATableNameWithTaskId(this._login.TaskID);
            StringBuilder sb = new StringBuilder();
            int grouplevels = GroupLevels;
            //generate tables add groupfilter
            #region base
            //sb.Append(" select A.* ");
            //sb.Append(" into ");
            //sb.Append(newtable);
            //sb.Append(" from ");
            //sb.Append(_report.BaseTable);
            //sb.Append(" A inner join ");
            //sb.Append(_report.BaseTableInTemp);
            //sb.Append("_1 sum on ");
            //sb.Append(GetComplexOnString());
            //sb.Append(" where ");
            //sb.Append(GetGroupFilterString());
            //Ron 2011623
            sb.Append(" select A.* ");
            sb.Append(" into ");
            sb.Append(newtable);
            sb.Append(" from ");
            sb.Append(_report.BaseTable);
            sb.Append(" A inner join ");
            sb.Append(_report.BaseTableInTemp);
            sb.Append("_" + grouplevels.ToString() + " sum on ");
            sb.Append(GetComplexOnString(grouplevels));
            //Ron 2011623
            #endregion
            #region level 1
            sb.Append(" select sum.* ");
            sb.Append(" into ");
            sb.Append(newtable);
            sb.Append("_1 from ");
            sb.Append(_report.BaseTableInTemp);
            //Ron 2011623
            sb.Append("_1 sum ");
            //sb.Append("_1 sum where ");
            //sb.Append(GetGroupFilterString());
            //Ron 2011623

            #endregion
            #region other level
            for (int level = 2; level <= grouplevels; level++)
            {
                sb.Append(" select A.* ");
                sb.Append(" into ");
                sb.Append(newtable);
                sb.Append("_");
                sb.Append(level.ToString());
                sb.Append(" from ");
                sb.Append(_report.BaseTableInTemp);
                sb.Append("_");
                sb.Append(level.ToString());
                sb.Append(" A inner join ");
                sb.Append(_report.BaseTableInTemp);
                sb.Append("_1 sum on ");
                sb.Append(GetComplexOnString());
                //Ron 2011623
                //sb.Append(" where ");
                //sb.Append(GetGroupFilterString());
                //Ron 2011623
            }
            #endregion
            //delete rawtables
            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
            DropTableAfterComplexHandle(_report.BaseTable, grouplevels, _report.CacheID);
            _report.BaseTable = newtable;
        }

        protected void DropTableAfterComplexHandle(string basetable, int grouplevels, string cacheid)
        {
            Thread t = new Thread(delegate()
            {
                try
                {
                    RemoteDataHelper rdh = new RemoteDataHelper();
                    rdh.DropFromDB(_login.TempDBCnnString, rdh.BaseAndLevelsString(basetable, grouplevels, cacheid));
                }
                catch
                {
                }
            }
                     );
            t.Start();
        }

        protected virtual string GetGroupFilterString()
        {
            //sum(column1)>100
            //sum.column1>100(ok)
            return _report.GroupFilter;
        }
        private string GetComplexOnString(int level)
        {
            StringBuilder sbc = new StringBuilder();
            ArrayList al = new ArrayList(level);
            for (int k = 1; k <= level; k++)
            {
                for (int m = 0; m < (_report.GroupStructure[k] as ArrayList).Count; m++)
                {
                    al.Add((_report.GroupStructure[k] as ArrayList)[m].ToString());
                }
            }
            for (int k = 0; k < al.Count; k++)
            {
                string colname = al[k].ToString();
                AddJoinOnString(colname, sbc, "sum");
            }
            string tmpc = sbc.ToString();
            return tmpc.Substring(0, tmpc.Length - 4);
        }

        private string GetComplexOnString()
        {
            StringBuilder sbc = new StringBuilder();
            ArrayList al = _report.GroupStructure[1] as ArrayList;
            for (int k = 0; k < al.Count; k++)
            {
                string colname = al[k].ToString();
                //sbc.Append(" isnull(A.[");
                //sbc.Append(colname);
                //sbc.Append("],'')=isnull(sum.[");
                //sbc.Append(colname);
                //sbc.Append("],'') and ");
                AddJoinOnString(colname, sbc, "sum");
            }
            string tmpc = sbc.ToString();
            return tmpc.Substring(0, tmpc.Length - 4);
        }

        private void AddJoinOnString(string colname, StringBuilder sb, string b)
        {
            DataSource ds = _report.DataSources[colname];
            DataType dt = DataType.String;
            if (ds != null)
                dt = ds.Type;
            if (dt == DataType.DateTime || (ds != null && ds.IsADecimal))
            {
                sb.Append("((");
                sb.Append(" A.[");
                sb.Append(colname);
                sb.Append("] is null and ");
                sb.Append(b);
                sb.Append(".[");
                sb.Append(colname);
                sb.Append("] is null) or ");
                sb.Append(" A.[");
                sb.Append(colname);
                sb.Append("]=");
                sb.Append(b);
                sb.Append(".[");
                sb.Append(colname);
                sb.Append("]) and ");
            }
            else
            {
                sb.Append(" isnull(A.[");
                sb.Append(colname);
                sb.Append("],'')=isnull(");
                sb.Append(b);
                sb.Append(".[");
                sb.Append(colname);
                sb.Append("],'') and ");
            }
        }

        protected void OnReportGoing(Report report)
        {
            report.State = 1;
            _semirowcontainer.AddReport(report);
            if (report.ViewID != "preloadview")
                OnCache();
        }

        private void OnSemiRowsGoing(SemiRows semirows)
        {
            _semirowcontainer.AddSemiRows(semirows);
        }

        private void OnEndAll(SemiRows semirows)
        {
            if (semirows != null)
                OnSemiRowsGoing(semirows);
            EndContainer();
        }

        protected virtual void EndContainer()
        {
            _semirowcontainer.EndAll();
        }
        #endregion

        #region compile
        public void NeedReCompile()
        {
            _assembly = null;
        }
        private void Compile(bool bsave)
        {
            System.Diagnostics.Trace.WriteLine("Begin compile");
            // 为了兼容移动报表，这里从注册表取得运行位置。
            _scripthelper.References.Add(GetU8Path() + "\\UAP\\UFIDA.U8.UAP.Services.ReportData.dll");
            _scripthelper.References.Add(GetU8Path() + "\\UAP\\UFIDA.U8.UAP.Services.ReportElements.dll");
            _scripthelper.References.Add(GetU8Path() + "\\UAP\\UFIDA.U8.UAP.Services.ReportFilterService.dll");
            //_scripthelper.References.Add(ExecutePath + "UFSoft.U8.Report.Exhibition.dll");
            //_scripthelper.References.Add(ExecutePath + "UFSoft.U8.Ex.Filter.dll");
            _scripthelper.OutputAssembly = ReportEngine.CacheFileName(_report.CacheID) + "as.rc";
            CompilerResults cr = _scripthelper.Compile();
            if (cr != null)
            {
                if (cr.Errors.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < cr.Errors.Count; i++)
                    {
                        sb.Append((i + 1).ToString());
                        sb.Append("：");
                        sb.Append(cr.Errors[i].ErrorText);
                        sb.Append("\r\n");
                    }
                    throw new ReportException("Compile error: \r\n" + sb.ToString());
                }
                _assembly = cr.CompiledAssembly;
                _assemblybytes = null;
            }
            else if (bsave)
            {
                _scripthelper.OutputAssembly = null;
            }
            System.Diagnostics.Trace.WriteLine("End compile");
        }

        private string ExecutePath
        {
            get
            {
                string referencepath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                if (!referencepath.EndsWith(@"\"))
                    referencepath += @"\";

                if (!referencepath.ToLower().EndsWith(@"\uap\"))
                {
                    if (_report.SubId == "OutU8")
                        referencepath += @"WWW\UAP\";
                    else
                        referencepath += @"UAP\";
                }
                //referencepath = referencepath.ToLower().Replace(@"\appserver\bin\uap\", @"\uap\");
                return referencepath;
            }
        }
        #endregion

        #region cache handle
        protected virtual void OnCache()
        {
            try
            {
                string filename = ReportEngine.CacheFileName(_report.CacheID) + "rt.rc";
                //System.Threading.Thread.Sleep(100);
                using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    byte[] reportbytes = _report.ToBytesForCache();
                    fs.Write(reportbytes, 0, reportbytes.Length);
                    logger.Info("reportbytes写入文件 filename = " + filename);
                    fs.Close();
                }

                #region save assembly to db
                if (_report.bShowDetail && _report.Type != ReportType.CrossReport && _report.UnderState != ReportStates.Static && _assemblybytes == null && !string.IsNullOrEmpty(_scripthelper.OutputAssembly) && File.Exists(_scripthelper.OutputAssembly))
                {
                    _assemblybytes = File.ReadAllBytes(_scripthelper.OutputAssembly);
                    RemoteDataHelper rdh = new RemoteDataHelper();
                    rdh.SaveAssemblyString(_login.UfMetaCnnString, _report.ViewID, Convert.ToBase64String(_assemblybytes));
                }
                #endregion

                if (_assemblybytes != null)
                {
                    filename = ReportEngine.CacheFileName(_report.CacheID) + "as.rc";
                    using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        fs.Write(_assemblybytes, 0, _assemblybytes.Length);
                        logger.Info("_assemblybytes写入文件 filename = " + filename);
                        fs.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Info("报表缓存写入失败：" + ex.Message);
                logger.Error(ex);
            }
        }

        protected virtual void OnCachePageInfos()
        {
            string filename = ReportEngine.CacheFileName(_report.CacheID) + "pi.rc";
            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                SaveGlobal();
                byte[] reportbytes = _pageinfos.ToBytes();
                fs.Write(reportbytes, 0, reportbytes.Length);
                fs.Close();
            }
        }

        private void ReadCache(string cacheid)
        {
            logger.Info("begin ReadCache,cacheid = " + cacheid);
            string filename = ReportEngine.CacheFileName(cacheid);
            logger.Info("begin ReadCache,filename = " + filename);
            byte[] b = null;
            using (System.IO.FileStream fs = new System.IO.FileStream(filename + "rt.rc", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                logger.Info("begin ReadCache,filename = " + filename + "rt.rc");
                b = new Byte[fs.Length];
                fs.Read(b, 0, b.Length);
                fs.Close();
            }
            if (b != null)
                _report = Report.FromBytes(b);

            _datasource = new ReportDataSource(_report.DataSources);
            _scripthelper = new ScriptHelper(_report);

            if (File.Exists(filename + "as.rc"))
            {
                logger.Info("begin ReadCache,filename = " + filename + "as.rc");
                using (System.IO.FileStream fs = new System.IO.FileStream(filename + "as.rc", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    b = new Byte[fs.Length];
                    fs.Read(b, 0, b.Length);
                    fs.Close();
                }
                if (b != null)
                    _assembly = Assembly.Load(b);
            }

            if (File.Exists(filename + "pi.rc"))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filename + "pi.rc", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    b = new Byte[fs.Length];
                    fs.Read(b, 0, b.Length);
                    fs.Close();
                }
                if (b != null)
                {
                    _pageinfos = PageInfos.FromBytes(b);
                    //RestoreGlobal();
                }
            }
        }

        protected virtual void RestoreGlobal()
        {
            if (_report.Varients.Count > 0 && _assembly != null)
            {
                ITest itest = (ITest)ScriptHelper.FindInterface(_assembly, _scripthelper.NameSpacePrefix + ".Global");
                if (itest != null)
                {
                    itest.Init(_datahelper);
                    PageInfo pi = _pageinfo;
                    if (!_pageinfos.bCalcPageByPage)
                        pi = _pageinfos.BeginAGlobalValuesCache();
                    _datahelper.Global.RestoreGlobal(pi);
                }
            }
        }

        protected virtual void SaveGlobal()
        {
            if (_report.Varients.Count > 0 && _assembly != null)
            {
                ITest itest = (ITest)ScriptHelper.FindInterface(_assembly, _scripthelper.NameSpacePrefix + ".Global");
                if (itest != null)
                {
                    itest.Init(_datahelper);
                    PageInfo pi = _pageinfo;
                    if (!_pageinfos.bCalcPageByPage)
                        pi = _pageinfos.BeginAGlobalValuesCache();
                    _datahelper.Global.SaveGlobal(pi);
                }
            }
        }
        #endregion

        #region savecheck
        public void SaveCheck()
        {
            //InitExpand(null);
            CheckGroup(null);
            ReallyInterprete(null);
            Compile(true);

            #region double aggregation operation
            foreach (string key in _avgcalculators)
            {
                if (_sumcalculators.Contains(key) || _maxcalculators.Contains(key) || _mincalculators.Contains(key))
                    throw new ResourceReportException("U8.UAP.Report.DoubleMapName", new string[] { key });
            }
            foreach (string key in _maxcalculators)
            {
                if (_sumcalculators.Contains(key) || _mincalculators.Contains(key))
                    throw new ResourceReportException("U8.UAP.Report.DoubleMapName", new string[] { key });
            }
            foreach (string key in _mincalculators)
            {
                if (_sumcalculators.Contains(key))
                    throw new ResourceReportException("U8.UAP.Report.DoubleMapName", new string[] { key });
            }
            #endregion
        }
        #endregion

        #region property
        public byte[] AssemblyBytes
        {
            get
            {
                return _assemblybytes;
            }
        }
        public string OutputAssembly
        {
            get
            {
                if (_scripthelper != null)
                    return _scripthelper.OutputAssembly;
                return null;
            }
        }
        protected bool bComplexPage
        {
            get
            {
                return !string.IsNullOrEmpty(_report.GroupFilter) && GroupLevels > 0;
            }
        }

        private bool bCalcPageByPage
        {
            get
            {
                return _report.AccumulateColumns.Count > 0
                               || _report.ComplexColumns.Count > 0
                               || _report.BalanceColumns.Count > 0
                               || _report.RowBalanceColumns.Count > 0
                               || bAllToPrevious
                               || bGlobalToPage;
            }
        }

        private bool bGlobalToPage
        {
            get
            {
                return _report.Args != null && _report.Args.Contains("GlobalToPage");
            }
        }

        private bool bAllToPrevious
        {
            get
            {
                return _report.Args != null && _report.Args.Contains("AllToPrevious");
            }
        }

        private bool bReadToTable
        {
            get
            {
                return _report.Args != null && _report.Args.Contains("ReadToTable");
            }
        }
        #endregion

        protected string GetCaptionBeforeChangeType(Cell cell)
        {
            string caption = cell.Caption;
            if (cell is IUserDefine && !string.IsNullOrEmpty((cell as IUserDefine).UserDefineItem))
                // && (cell is IDataSource) && cell.Caption == (cell as IDataSource).DataSource.Caption)
                caption = _datahelper.CusDefineInfo(caption, (cell as IUserDefine).UserDefineItem);
            return caption;
        }
    }

    internal class GridReportHandler : ReportHandler
    {
        protected GridReportHandler()
        {
        }

        public GridReportHandler(Report report, U8LoginInfor login, DataHelper datahelper, ReportDataSource datasource, SemiRowsContainerOnServer semirowcontainer, byte[] assemblybytes, PageInfos pageinfos)
            : base(report, login, datahelper, datasource, semirowcontainer, assemblybytes, pageinfos)
        {
        }

        protected override void HandleMaxMinAvg()
        {
            int count = _report.Sections.Count - 1;
            #region handle max,min,avg
            while (count >= 0)
            {
                Section section = _report.Sections[count];
                int index = section.Cells.Count - 1;
                while (index >= 0)
                {
                    Cell cell = section.Cells[index];
                    if ((cell is IGridCollect && (cell as IGridCollect).bSummary && bMaxMinOrAvg((cell as IGridCollect).Operator)))
                    {
                        if (cell is IDataSource)// || (cell is ICalculateColumn && (cell as IMapName).
                        {
                            if (cell is GridDecimal)
                            {
                                string caption = GetCaptionBeforeChangeType(cell);

                                GridCalculateColumn cnew = new GridCalculateColumn(cell as GridDecimal);
                                cnew.Expression = cnew.Expression + "+ 0";
                                cnew.SetMapName(cnew.Name + "__MaxMinAvg");
                                cnew.Caption = caption;
                                section.Cells.RemoveAt(index);
                                section.Cells.Insert(index, cnew);
                            }
                        }
                    }
                    index--;
                }
                count--;
            }
            #endregion
        }
    }

    internal class FreeReportHandler : ReportHandler
    {
        protected FreeReportHandler()
        {
        }

        public FreeReportHandler(Report report, U8LoginInfor login, DataHelper datahelper, ReportDataSource datasource, SemiRowsContainerOnServer semirowcontainer, byte[] assemblybytes, PageInfos pageinfos)
            : base(report, login, datahelper, datasource, semirowcontainer, assemblybytes, pageinfos)
        {
        }
    }

    internal class IndicatorReportHandler : ReportHandler
    {
        protected IndicatorReportHandler()
        {
        }

        public IndicatorReportHandler(Report report, U8LoginInfor login, DataHelper datahelper, ReportDataSource datasource, SemiRowsContainerOnServer semirowcontainer, byte[] assemblybytes, PageInfos pageinfos)
            : base(report, login, datahelper, datasource, semirowcontainer, assemblybytes, pageinfos)
        {
        }

        protected override bool AddItems(Cell cell, Section section)
        {
            if (cell is IPart || cell is IIndicator || cell is IMetrix)
            {
                string keystring = null;
                if (cell is IAlgorithm)
                    keystring = (cell as IAlgorithm).Algorithm;
                AddScriptKey(cell, keystring, cell.PrepaintEvent);
                SimpleArrayList sal = null;
                if (cell is CalculatorIndicator)
                    sal = new SimpleArrayList();
                if (cell is IIndicator)
                {
                    if ((cell as IIndicator).DetailCompare != null)
                        AddScriptKey(sal, cell, (cell as IIndicator).DetailCompare.Expression1Script, (cell as IIndicator).DetailCompare.Expression2Script);
                    if ((cell as IIndicator).TotalCompare != null)
                        AddScriptKey(sal, cell, (cell as IIndicator).TotalCompare.Expression1Script, (cell as IIndicator).TotalCompare.Expression2Script);
                    if ((cell as IIndicator).SummaryCompare != null)
                        AddScriptKey(sal, cell, (cell as IIndicator).SummaryCompare.Expression1Script, (cell as IIndicator).SummaryCompare.Expression2Script);
                }

                if (sal != null)
                    AddCalculators((cell as ICalculator).Operator, (cell as ICalculateColumn).Expression, cell as IMapName, sal);

                if (cell is IPart)
                    return false;
                return true;
            }
            else
                return base.AddItems(cell, section);
        }

        protected override void InnerCreateReport(ShowStyle style)
        {
            BaseHandleBeforeFormalize();
            PrepairData(false, style);
        }

        protected override void PrepairData(bool bonlyindex, ShowStyle style)
        {
            base.PrepairData(2, style);
        }

        protected override void HandleIndicatorDetail(ArrayList metrixs)
        {
            //Code here
            foreach (IIndicatorMetrix metrix in metrixs)
            {
                Report report = new Report();
                report.Name = this._report.Name;
                report.ViewID = ((Cell)metrix).Name;
                report.GroupSchemas = new GroupSchemas();
                report.BaseTable = _report.BaseTable;
                report.UnderState = ReportStates.Browse;
                report.bShowSummary = metrix.ShowSummary;
                report.bShowWhenZero = _report.bShowWhenZero;
                report.ColorStyleID = metrix.StyleID;
                report.Type = ReportType.MetrixReport;
                report.BorderStyle = metrix.BorderStyle;
                report.BorderColor = metrix.BorderColor;
                report.Informations = _report.Informations;
                report.SubId = _report.SubId;

                try
                {
                    if (_report.FltSrv != null && _report.FltSrv.Contains("RowsToReturn"))
                        metrix.PageSize = Convert.ToInt32(_report.FltSrv.GetValue1("RowsToReturn"));
                }
                catch
                {
                }

                if (_report.PageRecords == 0)
                    report.PageRecords = 0;
                else
                    report.PageRecords = metrix.PageSize;

                report.GroupFilter = _report.GroupFilter;
                report.SolidFilter = _report.SolidFilter;
                report.RowFilter.FilterString = _report.RowFilter.FilterString;
                report.InformationString = _report.InformationString;

                GroupSchema gs = null;
                GroupSchemaItem gsi = null;
                CrossRowHeader crh = null;
                CrossColumnHeader cch = null;
                CrossDetail cd = null;
                GridDetail gd = null;
                if (metrix.CrossPart != null)
                {
                    report.bShowDetail = true;
                    report.Type = ReportType.CrossReport;
                    crh = new CrossRowHeader();
                    cch = new CrossColumnHeader();
                    cd = new CrossDetail();
                    report.Sections.Add(crh);
                    report.Sections.Add(cch);
                    report.Sections.Add(cd);

                    cch.Cells.Add(metrix.CrossPart);
                }
                else
                {
                    report.Type = ReportType.GridReport;
                    report.bShowDetail = false;
                    gd = new GridDetail();
                    report.Sections.Add(gd);
                    gs = new GroupSchema();
                    gs.ID = "MetrixGroup";
                    gsi = new GroupSchemaItem();
                    gs.SchemaItems.Add(gsi);
                }

                if (metrix.GroupParts != null)
                {
                    foreach (Cell cell in metrix.GroupParts)
                    {
                        if (!cell.Visible)
                            continue;
                        if (report.Type == ReportType.GridReport)
                        {
                            gd.Cells.Add(cell);
                            gsi.Items.Add(cell.Name);
                        }
                        else
                        {
                            (cell as Cell).Height = (cell as Cell).Height / 2;
                            if (cell is GroupDimension)
                            {
                                if ((cell as GroupDimension).bDateTime)
                                    crh.Cells.Add(new GridDateTime(cell as GroupDimension));
                                else
                                    crh.Cells.Add(cell);
                            }
                            else//calculategroupdimension
                            {
                                crh.Cells.Add(cell);
                            }
                        }
                    }
                }
                if (metrix.IndicatorParts != null)
                {
                    foreach (Cell cell in metrix.IndicatorParts)
                    {
                        if (report.Type == ReportType.GridReport)
                        {
                            gd.Cells.Add(cell);
                        }
                        else
                        {
                            cd.Cells.Add(cell);
                        }
                    }
                }

                if (gs != null)
                {
                    report.GroupSchemas.Add(gs);
                    report.CurrentSchemaID = gs.ID;
                }

                MetrixReportHandler mrh = new MetrixReportHandler(report, _login, _datahelper, (ReportDataSource)_datasource.Clone(), _semirowcontainer, _assembly, _scripthelper.NameSpacePrefix);
                mrh.CheckCanceled += new CheckCanceledHandler(mrh_CheckCanceled);
                if (report.Type == ReportType.CrossReport)
                    mrh.OpenCrossReport(true, null, null, ShowStyle.NoGroupHeader);
                else
                    mrh.OpenReport(null, null, ShowStyle.NoGroupHeader);
            }
            EndContainer();
        }

        private void mrh_CheckCanceled()
        {
            OnCheckCanceled();
        }

        protected override void OnCache()
        {
        }
        protected override void OnCachePageInfos()
        {
        }
        protected override void RestoreGlobal()
        {
        }
        protected override void SaveGlobal()
        {
        }
    }

    internal class MetrixReportHandler : ReportHandler
    {
        protected MetrixReportHandler()
        {
        }
        public MetrixReportHandler(Report report, U8LoginInfor login, DataHelper datahelper, ReportDataSource datasource, SemiRowsContainerOnServer semirowcontainer, Assembly assembly, string namespaceprefix)
        {
            _report = report;
            _login = login;
            _datahelper = datahelper;
            _datasource = datasource;
            _report.DataSources = datasource.DataSources;
            _semirowcontainer = semirowcontainer;
            _assembly = assembly;
            _scripthelper = new ScriptHelper(namespaceprefix);
        }

        public override void SortReport(bool bonlyindex, ShowStyle style)
        {
            PrepairData(bonlyindex, style);
            InnerPageTo(-1, -1, style);
        }
        protected override bool NeedHandleGroupFilter
        {
            get
            {
                if (_report.bShowDetail)
                    return !string.IsNullOrEmpty(_report.GroupFilter);
                else
                    return false;
            }
        }

        protected override string GetGroupFilterString()
        {
            return _report.GroupFilter.ToLower().Replace("sum.", "");
        }
        protected string GroupFilterWhereString
        {
            get
            {
                string where = GetGroupFilterString();
                if (!string.IsNullOrEmpty(where))
                    where = " where " + where;
                return where;
            }
        }

        protected override void PrepairData(bool bonlyindex, ShowStyle style)
        {
            if (_report.bShowDetail)
            {
                StringBuilder sbcount = new StringBuilder();
                sbcount.Append("select count(*) from ");
                sbcount.Append(_report.BaseTable);
                sbcount.Append(" A ");
                sbcount.Append(WhereString);
                _report.RowsCount = (int)SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbcount.ToString());
            }
            else
            {

                if (bComplexPage)
                {
                    #region ComplexHandle

                    string targetTableCols = _report.GroupStrings[1].ToString();
                    if (!string.IsNullOrEmpty(_report.MinorAggregateString))
                    {
                        targetTableCols = targetTableCols + "," + _report.MinorAggregateString;
                    }
                    //string newtable = CustomDataSource.GetATableName();
                    string newtable = CustomDataSource.GetATableNameWithTaskId(this._login.TaskID);

                    ExecuteInsertNewTable(new string[] { targetTableCols }, newtable, _report.BaseTable, null, _report.GroupByStrings[1].ToString());
                    #region 旧代码逻辑
                    StringBuilder sb = new StringBuilder();
                    //sb.Append("select ");
                    //sb.Append(_report.GroupStrings[1].ToString());//grouplevels
                    //if (!string.IsNullOrEmpty(_report.MinorAggregateString))
                    //    sb.Append(",");
                    //sb.Append(_report.MinorAggregateString);
                    //sb.Append(" into ");
                    //sb.Append(newtable);
                    //sb.Append(" from ");
                    //sb.Append(_report.BaseTable);
                    //sb.Append(WhereString);
                    //sb.Append(" group by ");
                    //sb.Append(_report.GroupByStrings[1].ToString());//grouplevels
                    //SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                    #endregion
                    DropTableAfterComplexHandle(_report.BaseTable, 0, _report.CacheID);
                    _report.BaseTable = newtable;
                    #endregion
                    #region rowscount
                    StringBuilder sbcount = new StringBuilder();
                    sbcount.Append("select count(*) from ");
                    sbcount.Append(_report.BaseTable);
                    sbcount.Append(" A ");
                    sbcount.Append(GroupFilterWhereString);
                    _report.RowsCount = (int)SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbcount.ToString());
                    #endregion
                }
                else
                {
                    StringBuilder sbcount = new StringBuilder();
                    sbcount.Append("select count(*) from (");
                    sbcount.Append("select count(*) as b from ");
                    sbcount.Append(_report.BaseTable);
                    sbcount.Append(" A ");
                    sbcount.Append(WhereString);
                    sbcount.Append(" group by ");
                    sbcount.Append(_report.GroupByStrings[1].ToString());//grouplevels
                    sbcount.Append(") B");
                    _report.RowsCount = (int)SqlHelper.ExecuteScalar(_login.UfDataCnnString, sbcount.ToString());
                }
            }
            base.PrepairData(3, style);
        }

        protected override void AppendMinorSQLStringWithDetail(StringBuilder sb, int startindex, int endindex, string wherestring)
        {
            sb.Append("select ");
            if (_report.PageRecords != 0)
            {
                sb.Append(" top ");
                sb.Append(_report.PageRecords.ToString());
                sb.Append(" ");
            }
            if (!string.IsNullOrEmpty(_report.DetailString))
                sb.Append(_report.DetailString);
            else
                sb.Append("*");
            sb.Append(" from ");
            sb.Append(_report.BaseTable);
            sb.Append(" A ");
            sb.Append(wherestring);
            if (_report.SortSchema.QuickSortItems.Count > 0)
            {
                sb.Append(" order by ");
                sb.Append(_report.SortSchema.GetSortString());
            }
        }

        protected override void AppendMinorSQLStringWithNoDetail(StringBuilder sb, int i, int startindex, int endindex, string wherestring)
        {
            if (bComplexPage)
            {
                sb.Append("select ");
                if (_report.PageRecords != 0)
                {
                    sb.Append(" top ");
                    sb.Append(_report.PageRecords.ToString());
                    sb.Append(" ");
                }
                sb.Append(" * from ");
                sb.Append(_report.BaseTable);
                sb.Append(GroupFilterWhereString);
                sb.Append(" order by ");
                sb.Append(_report.SortSchema.GetSortStringForMatrix());
            }
            else
            {
                sb.Append("select ");
                if (_report.PageRecords != 0)
                {
                    sb.Append(" top ");
                    sb.Append(_report.PageRecords.ToString());
                    sb.Append(" ");
                }
                sb.Append(_report.GroupStrings[1].ToString());//grouplevels
                if (!string.IsNullOrEmpty(_report.MinorAggregateString))
                    sb.Append(",");
                sb.Append(_report.MinorAggregateString);
                sb.Append(" from ");
                sb.Append(_report.BaseTable);
                sb.Append(wherestring);
                sb.Append(" group by ");
                sb.Append(_report.GroupByStrings[1].ToString());//grouplevels
                sb.Append(" order by ");
                sb.Append(_report.SortSchema.GetSortStringForMatrix());
            }
        }

        protected override void AppendSummaryDataSQLString(StringBuilder sb, int grouplevels, string wherestring)
        {
            base.AppendSummaryDataSQLString(sb, 0, wherestring);
        }

        protected override void HandleIndicatorCells(ArrayList indicatorcells, RowData data, SemiRow semirow)
        {
            data.SemiRow = semirow;
            foreach (IIndicator indicator in indicatorcells)
            {
                if (indicator.CompareValue.bExpression1Script)
                {
                    object o = CalcColumnScript(indicator as Cell, data, indicator.CompareValue.ScriptID);
                    semirow.AddCompareValue1((indicator as Cell).Name, o);
                }
                if (indicator.CompareValue.bExpression2Script)
                {
                    object o = CalcColumnScript(indicator as Cell, data, indicator.CompareValue.ScriptID + "_2");
                    semirow.AddCompareValue2((indicator as Cell).Name, o);
                }
            }
        }

        protected override void InitIndicatorCells(ref ArrayList indicatorcells)
        {
            indicatorcells = new ArrayList();
        }

        protected override string SummaryLabelCaption
        {
            get
            {
                return String4Report.GetString("总计", _login.LocaleID);
            }
        }

        protected override void UnableTopBottonLine(Cell cell)
        {
            //base.UnableTopBottonLine(cell);
        }

        protected override void AddBaseID(int level, int levels, SemiRow semirow, RowData data)
        {
        }

        protected override void EndContainer()
        {

        }
        protected override void BaseHandleBeforeFormalize()
        {
        }
        protected override void OnCache()
        {
        }
        protected override void OnCachePageInfos()
        {
        }
        protected override void RestoreGlobal()
        {
        }
        protected override void SaveGlobal()
        {
        }

    }
}
