using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.CodeDom.Compiler;
using System.Text;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// Script 的摘要说明。
    /// </summary>
    public class ScriptHelper : IDisposable
    {
        #region fields
        private DataSources _datasourcefunctions;
        private StringBuilder _script;
        private ScriptLanguage _language = ScriptLanguage.CSharp;
        private ArrayList _references;
        private string _outputassembly;
        private Report _report;
        //private ArrayList _alpre;
        private string _namespaceprefix;
        private ArrayList _scriptids;
        #endregion

        #region Constructor
        public ScriptHelper()
        {
            _references = new ArrayList();
            _references.Add("System.Windows.Forms.dll");
            _references.Add("System.dll");
            _references.Add("System.Data.dll");
            _references.Add("System.Xml.dll");
            _references.Add("System.Drawing.dll");
            _script = new StringBuilder();
            _scriptids = new ArrayList();
        }
        public ScriptHelper(Report report)
            : this()
        {
            _datasourcefunctions = new DataSources();
            _report = report;
            //_alpre = new ArrayList();
        }

        public ScriptHelper(string namespaceprefix)
        {
            _namespaceprefix = namespaceprefix;
        }
        #endregion

        #region property
        public string NameSpacePrefix
        {
            get
            {
                if (string.IsNullOrEmpty(_namespaceprefix))
                    return "UFIDA.U8.UAP.Services.DynamicReportComponent.DRC" + (_report == null ? "tmp" : ClassName(_report.ViewID));
                else
                    return _namespaceprefix;
            }
        }

        public string OutputAssembly
        {
            get
            {
                return _outputassembly;
            }
            set
            {
                _outputassembly = value;
            }
        }

        public ScriptLanguage Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        public ArrayList References
        {
            get
            {
                return _references;
            }
        }

        public DataSources DSFunctions
        {
            get
            {
                return _datasourcefunctions;
            }
        }

        public void AddAKey(string name)
        {
            if (!string.IsNullOrEmpty(_namespaceprefix))
                return;
            if (!_datasourcefunctions.Contains(name))
                _datasourcefunctions.Add(name, "Function" + _datasourcefunctions.Count.ToString());
        }

        //private void AddReportString()
        //{
        //    _script.Append(ReportString("", ScriptType.None));
        //}

        public void AddCellString(Section section, Cell cell)
        {
            if (!string.IsNullOrEmpty(_namespaceprefix))
                return;

            if (cell is SuperLabel)
                SuperLabelScript(_script, cell as SuperLabel);
            else
            {
                //if (!_alpre.Contains(cell.Name.ToLower()))
                //{
                _script.Append(CellString(cell, cell.PrepaintEvent, ScriptType.PreEvent));
                //    if (cell.PrepaintEvent != "")
                //        _alpre.Add(cell.Name.ToLower());
                //}
            }
            if (cell is IExpression)
                _script.Append(CellString(cell, (cell as IExpression).Formula.FormulaExpression, ScriptType.Expression));
            if (cell is IAlgorithm)
                _script.Append(CellString(cell, (cell as IAlgorithm).Algorithm, ScriptType.Algorithm));
            if (cell is IIndicator)
                _script.Append(CellString(cell, "IIndicator", ScriptType.Algorithm));

            //if (cell is ICalculator && ((cell as ICalculator).Operator == OperatorType.AccumulateSUM
            //                            || (cell as ICalculator).Operator == OperatorType.BalanceSUM
            //                            || (cell as ICalculator).Operator == OperatorType.ExpressionSUM)
            //    )
            //{
            //    _script.Append(CalculatorString(cell as ICalculator));
            //}
        }

        private void SuperLabelScript(StringBuilder sbscript, SuperLabel superlabel)
        {
            sbscript.Append(CellString(superlabel, superlabel.PrepaintEvent, ScriptType.PreEvent));
            foreach (Label label in superlabel.Labels)
            {
                if (label is SuperLabel)
                    SuperLabelScript(sbscript, label as SuperLabel);
                else
                    sbscript.Append(CellString(label, label.PrepaintEvent, ScriptType.PreEvent));
            }
        }

        public void AddScript(string script)
        {
            if (!string.IsNullOrEmpty(_namespaceprefix))
                return;
            _script.Append(script);
        }
        #endregion

        #region compile
        public CompilerResults Compile()
        {
            string script = GetScript();
            Logger logger = Logger.GetLogger("ScriptHelper");
            logger.Info(script);
            if (string.IsNullOrEmpty(script))
                return null;

            CodeDomProvider provider = null;

            switch (_language)
            {
                case ScriptLanguage.VBDotNet:
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                case ScriptLanguage.CSharp:
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
            }

            ICodeCompiler compiler = provider.CreateCompiler();
            CompilerParameters parms = new CompilerParameters();
            CompilerResults results= new CompilerResults(new TempFileCollection());

            // Configure parameters
            parms.GenerateExecutable = false;
            parms.IncludeDebugInformation = false;
            parms.TreatWarningsAsErrors = false;
            parms.GenerateInMemory = true;
            if (_outputassembly != null)
                parms.OutputAssembly = _outputassembly;

            for (int i = 0; i < _references.Count; i++)
                parms.ReferencedAssemblies.Add(_references[i].ToString());

            //System.Diagnostics.Trace.WriteLine(script);
            try
            {
                results = compiler.CompileAssemblyFromSource(parms, script);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return results;
        }

        public static object FindInterface(System.Reflection.Assembly DLL, string TypeName)
        {
            //			foreach(Type t in DLL.GetTypes())
            //			{
            //				if (t.GetInterface(InterfaceName, true) != null)
            //					return DLL.CreateInstance(t.FullName);
            //			}
            foreach (Type t in DLL.GetTypes())
            {
                if (t.FullName == TypeName)
                    return DLL.CreateInstance(TypeName);
            }
            return null;
        }

        private string GetScript()
        {
            if (_report != null &&
                //string.IsNullOrEmpty(_report.GroupFilter) &&
                string.IsNullOrEmpty(_report.RowFilter.MapKeys) &&
                string.IsNullOrEmpty(_report.InitEvent) &&
                _report.Varients.Count == 0 &&
                _script.Length == 0)
                return null;

            StringBuilder sbscript = new StringBuilder();
            sbscript.Append(HeaderString());
            sbscript.Append("namespace ");
            sbscript.Append(NameSpacePrefix);
            sbscript.Append("\r\n{\r\n");
            sbscript.Append(ReportString("", ScriptType.None));
            sbscript.Append(_script.ToString());
            sbscript.Append(DataString());
            sbscript.Append("}");
            return sbscript.ToString();
        }
        #endregion

        #region script builder
        //private string DateDimensionCaptionHandlerString()
        //{
        //    if (_report.Type == ReportType.CrossReport)
        //    {
        //        StringBuilder sbscript = new StringBuilder();
        //        sbscript.Append("public class CellDateDimensionCaptionHandler");//scriptid:DateDimensionCaptionHandler
        //        sbscript.Append(":ICellEvent\r\n");
        //        sbscript.Append("{\r\n");
        //        sbscript.Append("public void Prepaint(Report report, RowData data, Cell innercell, FilterSrv filter, AgileArgs args, DataHelper datahelper, ReportSummaryData reportsummary, RowBalance  rowbalance, AccumulateData accumulate, BalanceData balance,object[] others)\r\n");
        //        sbscript.Append("{\r\n");
        //        innercell.Caption=innercell.UnFormatValue.Replace
        //        sbscript.Append("\r\n}");
        //        sbscript.Append("\r\n}\r\n");
        //        return sbscript.ToString();
        //    }
        //    return "";
        //}

        internal string HeaderString()
        {
            StringBuilder sbscript = new StringBuilder();
            sbscript.Append("using System.Windows.Forms;\r\n");
            sbscript.Append("using System;\r\n");
            sbscript.Append("using System.Data;\r\n");
            sbscript.Append("using System.Drawing;\r\n");
            sbscript.Append("using System.Collections;\r\n");
            sbscript.Append("using System.Data.SqlClient;\r\n");
            sbscript.Append("using System.Diagnostics;\r\n");
            sbscript.Append("using System.Text;\r\n");
            sbscript.Append("using System.IO;\r\n");
            sbscript.Append("using System.Globalization;\r\n");
            sbscript.Append("using UFIDA.U8.UAP.Services.ReportData;\r\n");
            sbscript.Append("using UFIDA.U8.UAP.Services.ReportElements;\r\n");
            sbscript.Append("using UFIDA.U8.UAP.Services.ReportFilterService;\r\n");
            return sbscript.ToString();
        }

        private string ReportString(string s, ScriptType st)
        {
            if (_report == null)
                return "";
            StringBuilder sbscript = new StringBuilder();
            ScriptType sttmp = st;

            if (st == ScriptType.None)
            {
                s = _report.InitEvent;
                sttmp = ScriptType.InitEvent;
            }
            if (s != "" && sttmp == ScriptType.InitEvent)
            {
                sbscript.Append("public class ReportInitEvent_Script");
                sbscript.Append(":IReportInitEvent\r\n");
                sbscript.Append("{\r\n");
                sbscript.Append("public void Init(Report report, FilterSrv filter, AgileArgs args, DataHelper datahelper,object[] others)\r\n");
                sbscript.Append("{\r\n");
                sbscript.Append("   Global global;\r\n");
                sbscript.Append("   if (datahelper.Global == null)\r\n");
                sbscript.Append("   {\r\n");
                sbscript.Append("       global = new Global();\r\n");
                sbscript.Append("       global.DataHelper = datahelper;\r\n");
                sbscript.Append("       datahelper.Global = global;\r\n");
                sbscript.Append("   }\r\n");
                sbscript.Append("   else\r\n");
                sbscript.Append("   {\r\n");
                sbscript.Append("       global = datahelper.Global as Global;\r\n");
                sbscript.Append("   }\r\n");
                sbscript.Append(s);
                sbscript.Append("\r\n}");
                sbscript.Append("\r\n}\r\n");
            }

            //if (st == ScriptType.None)
            //{
            //    s = _report.GroupFilter ;
            //    sttmp = ScriptType.GroupFilter ;
            //}
            //if (s != "" && sttmp == ScriptType.GroupFilter )
            //{
            //    sbscript.Append("public class ReportGroupFilter_Script");
            //    sbscript.Append(":IGroupFilter\r\n");
            //    sbscript.Append("{\r\n");
            //    sbscript.Append("public bool GroupFilter(SemiRow cells,FilterSrv filter, AgileArgs args, DataHelper datahelper,object[] others)\r\n");
            //    sbscript.Append("{\r\n");
            //    sbscript.Append("   Global global;\r\n");
            //    sbscript.Append("   if (datahelper.Global == null)\r\n");
            //    sbscript.Append("   {\r\n");
            //    sbscript.Append("       global = new Global();\r\n");
            //    sbscript.Append("       global.DataHelper = datahelper;\r\n");
            //    sbscript.Append("       datahelper.Global = global;\r\n");
            //    sbscript.Append("   }\r\n");
            //    sbscript.Append("   else\r\n");
            //    sbscript.Append("   {\r\n");
            //    sbscript.Append("       global = datahelper.Global as Global;\r\n");
            //    sbscript.Append("   }\r\n");
            //    sbscript.Append("   return (\r\n");
            //    sbscript.Append(s);
            //    sbscript.Append("   );\r\n");
            //    sbscript.Append("\r\n}");
            //    sbscript.Append("\r\n}\r\n");
            //} 

            if (st == ScriptType.None)
            {
                s = _report.RowFilter.MapKeys;
                sttmp = ScriptType.MapKeys;
            }
            if (s != "" && sttmp == ScriptType.MapKeys)
            {
                sbscript.Append("public class MapKeys_Script");
                sbscript.Append(":IMapKeys\r\n");
                sbscript.Append("{\r\n");
                sbscript.Append("public string MapKeys(RowData data, FilterSrv filter, AgileArgs args, DataHelper datahelper,object[] others)\r\n");
                sbscript.Append("{\r\n");
                sbscript.Append("   Current current = new Current(data);\r\n");
                sbscript.Append("   Global global;\r\n");
                sbscript.Append("   if (datahelper.Global == null)\r\n");
                sbscript.Append("   {\r\n");
                sbscript.Append("       global = new Global();\r\n");
                sbscript.Append("       global.DataHelper = datahelper;\r\n");
                sbscript.Append("       datahelper.Global = global;\r\n");
                sbscript.Append("   }\r\n");
                sbscript.Append("   else\r\n");
                sbscript.Append("   {\r\n");
                sbscript.Append("       global = datahelper.Global as Global;\r\n");
                sbscript.Append("   }\r\n");
                sbscript.Append(" return string.Format(\u0022");
                sbscript.Append(_report.RowFilter.FilterString);
                sbscript.Append("\u0022,");
                sbscript.Append(s);
                sbscript.Append(");\r\n}");
                sbscript.Append("\r\n}\r\n");
            }

            sbscript.Append(VarientString(_report.Varients));

            return sbscript.ToString();
        }

        internal string DataString()
        {
            if (_report == null)
                return "";
            StringBuilder sb = new StringBuilder();

            sb.Append("public class DataRowsForEx\r\n");
            sb.Append("{\r\n");
            sb.Append("    private ArrayList _rows;\r\n");
            sb.Append("    public DataRowsForEx(DataTable table)\r\n");
            sb.Append("    {\r\n");
            sb.Append("       _rows=new ArrayList();\r\n");
            sb.Append("       if(table!=null)\r\n");
            sb.Append("       {\r\n");
            sb.Append("           for(int i=0;i<table.Rows.Count;i++)\r\n");
            sb.Append("               _rows.Add(table.Rows[i]);\r\n");
            sb.Append("       }\r\n");
            sb.Append("    }\r\n");
            sb.Append("    public IEnumerator GetEnumerator ()\r\n");
            sb.Append("    {\r\n");
            sb.Append("         return _rows.GetEnumerator();\r\n");
            sb.Append("    }\r\n");
            sb.Append("    public int Length\r\n");
            sb.Append("    {\r\n");
            sb.Append("        get\r\n");
            sb.Append("        {\r\n");
            sb.Append("            return _rows.Count;\r\n");
            sb.Append("        }\r\n");
            sb.Append("    }\r\n");
            sb.Append("    public DataRow this[int index]\r\n");
            sb.Append("    {\r\n");
            sb.Append("        get\r\n");
            sb.Append("        {\r\n");
            sb.Append("            return (Length==0?null:(_rows[index] as DataRow));\r\n");
            sb.Append("        }\r\n");
            sb.Append("    }\r\n");
            sb.Append("}\r\n");

            sb.Append("public class Current\r\n");
            sb.Append("{\r\n");
            sb.Append("private RowData _rowdata;\r\n");
            sb.Append("public Current(RowData rowdata)\r\n");
            sb.Append("{\r\n");
            sb.Append("_rowdata=rowdata;\r\n");
            sb.Append("}\r\n");

            sb.Append("public object this[string name]\r\n");
            sb.Append("{\r\n");
            sb.Append("get\r\n");
            sb.Append("{\r\n");
            sb.Append("return _rowdata[name];\r\n");
            sb.Append("}\r\n");
            sb.Append("set\r\n");
            sb.Append("{\r\n");
            sb.Append("_rowdata[name]=value;\r\n");
            sb.Append("}\r\n");
            sb.Append("}\r\n");

            foreach (string name in _datasourcefunctions.Keys)
            {
                DataSource ds = _report.DataSources[name];
                switch (ds.Type)
                {
                    case DataType.Boolean:
                        sb.Append("public bool ");
                        Data1(sb, ds.Name);
                        sb.Append("return (bool)NullValue.Boolean;\r\n");
                        sb.Append("else\r\n");
                        sb.Append("return Convert.ToBoolean");
                        Data2(sb, ds.Name);
                        break;
                    case DataType.Currency:
                    case DataType.Decimal:
                        sb.Append("public double ");
                        Data1(sb, ds.Name);
                        sb.Append("return (double)NullValue.Decimal;\r\n");
                        sb.Append("else\r\n");
                        sb.Append("return Convert.ToDouble");
                        Data2(sb, ds.Name);
                        break;
                    case DataType.Int:
                        sb.Append("public int ");
                        Data1(sb, ds.Name);
                        sb.Append("return (int)NullValue.Int;\r\n");
                        sb.Append("else\r\n");
                        sb.Append("return Convert.ToInt32");
                        Data2(sb, ds.Name);
                        break;
                    case DataType.DateTime:
                        sb.Append("public DateTime ");
                        Data1(sb, ds.Name);
                        sb.Append("return (DateTime)NullValue.DateTime;\r\n");
                        sb.Append("else\r\n");
                        sb.Append("return Convert.ToDateTime");
                        Data2(sb, ds.Name);
                        break;
                    default:    // DataType.String:
                        sb.Append("public string ");
                        Data1(sb, ds.Name);
                        sb.Append("return (string)NullValue.String;\r\n");
                        sb.Append("else\r\n");
                        sb.Append("return Convert.ToString");
                        Data2(sb, ds.Name);
                        break;
                }
            }
            sb.Append("}\r\n");
            return sb.ToString();
        }

        internal void Data1(StringBuilder sb, string name)
        {
            sb.Append(_datasourcefunctions.GetString(name).ToString());
            sb.Append("{\r\n");
            sb.Append("get\r\n");
            sb.Append("{\r\n");
            sb.Append("if(_rowdata[\u0022");
            sb.Append(name);
            sb.Append("\u0022]==DBNull.Value)\r\n");
        }

        internal void Data2(StringBuilder sb, string name)
        {
            sb.Append("(_rowdata[\u0022");
            sb.Append(name);
            sb.Append("\u0022]);\r\n");
            sb.Append("}\r\n");
            sb.Append("set\r\n");
            sb.Append("{\r\n");
            sb.Append("_rowdata[\u0022");
            sb.Append(name);
            sb.Append("\u0022]=value;\r\n");
            sb.Append("}\r\n");
            sb.Append("}\r\n");
        }

        //internal string ObjectString(object o, string s, ScriptType st)
        //{
        //    if (o is Report)
        //    {
        //        return ReportString(s, st);
        //    }
        //    else if (o is GlobalVarients)
        //    {
        //        return VarientString(o as GlobalVarients);
        //    }
        //    else//cell
        //    {
        //        return CellString(o as Cell, s, st);
        //    }
        //}

        internal string VarientString(GlobalVarients gvs)
        {
            StringBuilder sbscript = new StringBuilder();
            sbscript.Append("public class Global:ITest\r\n");
            sbscript.Append("{\r\n");
            sbscript.Append("	private DataHelper _datahelper;\r\n");
            sbscript.Append("	public DataHelper DataHelper\r\n");
            sbscript.Append("	{\r\n");
            sbscript.Append("	    get\r\n");
            sbscript.Append("	    {\r\n");
            sbscript.Append("	        return _datahelper;\r\n");
            sbscript.Append("	    }\r\n");
            sbscript.Append("	    set\r\n");
            sbscript.Append("	    {\r\n");
            sbscript.Append("	        _datahelper=value;\r\n");
            sbscript.Append("	    }\r\n");
            sbscript.Append("	}\r\n");
            sbscript.Append("	public void Init(DataHelper datahelper)\r\n");
            sbscript.Append("	{\r\n");
            sbscript.Append("       if (datahelper.Global == null)\r\n");
            sbscript.Append("       {\r\n");
            sbscript.Append("           Global global = new Global();\r\n");
            sbscript.Append("           global.DataHelper = datahelper;\r\n");
            sbscript.Append("           datahelper.Global = global;\r\n");
            sbscript.Append("       }\r\n");
            sbscript.Append("	}\r\n");
            sbscript.Append("	public object ExecuteScalar(string sql,VarientType type)\r\n");
            sbscript.Append("	{\r\n");
            sbscript.Append("		DataSet ds=_datahelper.Exec(sql);\r\n");
            sbscript.Append("		if(ds==null || ds.Tables.Count==0 || ds.Tables[0].Rows.Count==0)\r\n");
            sbscript.Append("		{\r\n");
            sbscript.Append("			if(type==VarientType.SQL_DateTime)\r\n");
            sbscript.Append("				return NullValue.DateTime;\r\n");
            sbscript.Append("			else if(type==VarientType.SQL_Decimal)\r\n");
            sbscript.Append("				return NullValue.Decimal;\r\n");
            sbscript.Append("			else\r\n");
            sbscript.Append("				return NullValue.String;\r\n");
            sbscript.Append("		}\r\n");
            sbscript.Append("		else\r\n");
            sbscript.Append("		{\r\n");
            sbscript.Append("			return ds.Tables[0].Rows[0][0];\r\n");
            sbscript.Append("		}\r\n");
            sbscript.Append("	}\r\n");
            sbscript.Append("	public object ExecuteScalar(string sql)\r\n");
            sbscript.Append("	{\r\n");
            sbscript.Append("		DataSet ds=_datahelper.Exec(sql);\r\n");
            sbscript.Append("		if(ds==null || ds.Tables.Count==0 || ds.Tables[0].Rows.Count==0)\r\n");
            sbscript.Append("		{\r\n");
            sbscript.Append("			return null;\r\n");
            sbscript.Append("		}\r\n");
            sbscript.Append("		else\r\n");
            sbscript.Append("		{\r\n");
            sbscript.Append("			return ds.Tables[0].Rows[0][0];\r\n");
            sbscript.Append("		}\r\n");
            sbscript.Append("	}\r\n");
            sbscript.Append("	public DataSet Execute(string sql)\r\n");
            sbscript.Append("	{\r\n");
            sbscript.Append("		return _datahelper.Exec(sql);\r\n");
            sbscript.Append("	}\r\n");
            sbscript.Append("	public void ShowMessage(string msg)\r\n");
            sbscript.Append("	{\r\n");
            sbscript.Append("		MessageBox.Show(msg, \u0022Message\u0022, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);\r\n");
            sbscript.Append("	}\r\n");
            sbscript.Append("	public void SaveGlobal(PageInfo pi)\r\n");
            sbscript.Append("	{\r\n");
            foreach (GlobalVarient gv in gvs)
            {
                sbscript.Append("		pi.AddGlobal(\u0022");
                sbscript.Append(gv.Name);
                sbscript.Append("\u0022,this.");
                sbscript.Append(gv.Name);
                sbscript.Append(");\r\n");
            }
            sbscript.Append("	}\r\n");
            sbscript.Append("	public void RestoreGlobal(PageInfo pi)\r\n");
            sbscript.Append("	{\r\n");
            sbscript.Append("	    if(pi.GlobalValues==null)\r\n");
            sbscript.Append("	        return;\r\n");
            foreach (GlobalVarient gv in gvs)
            {
                sbscript.Append("this.Set_");
                sbscript.Append(gv.Name);
                sbscript.Append("(pi.GetGlobal(\u0022");
                sbscript.Append(gv.Name);
                sbscript.Append("\u0022));\r\n");
            }
            sbscript.Append("	}\r\n");
            sbscript.Append("	public object Test()\r\n");
            sbscript.Append("	{\r\n");
            if (gvs.Count > 0)
            {
                sbscript.Append("		return ");
                sbscript.Append(gvs[0].Name);
                sbscript.Append(";\r\n");

            }
            else
            {
                sbscript.Append("		return null;\r\n");
            }
            sbscript.Append("	}\r\n");
            foreach (GlobalVarient gv in gvs)
            {
                switch (gv.Type)
                {
                    case VarientType.DateTime:
                        sbscript.Append("private DateTime _");
                        sbscript.Append(gv.Name);
                        sbscript.Append("= NullValue.DateTime;\r\n");
                        sbscript.Append("public DateTime ");
                        sbscript.Append(gv.Name);
                        sbscript.Append("\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	get\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		if(_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("==NullValue.DateTime && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=null && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=string.Empty)\r\n");
                        sbscript.Append("			_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToDateTime(\u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022);\r\n");
                        sbscript.Append("		return _");
                        sbscript.Append(gv.Name);
                        sbscript.Append(";\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("	set\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=value;\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("}\r\n");

                        sbscript.Append("public void Set_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("(object value)\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToDateTime(value);\r\n");
                        sbscript.Append("}\r\n");
                        break;
                    case VarientType.Decimal:
                        sbscript.Append("private double _");
                        sbscript.Append(gv.Name);
                        sbscript.Append("= NullValue.Decimal;\r\n");
                        sbscript.Append("public double ");
                        sbscript.Append(gv.Name);
                        sbscript.Append("\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	get\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		if(_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("==NullValue.Decimal && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=null && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=string.Empty)\r\n");
                        sbscript.Append("			_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToDouble(\u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022);\r\n");
                        sbscript.Append("		return _");
                        sbscript.Append(gv.Name);
                        sbscript.Append(";\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("	set\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=value;\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("}\r\n");

                        sbscript.Append("public void Set_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("(object value)\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToDouble(value);\r\n");
                        sbscript.Append("}\r\n");
                        break;
                    case VarientType.SQL_DateTime:
                        sbscript.Append("private DateTime _");
                        sbscript.Append(gv.Name);
                        sbscript.Append("= NullValue.DateTime;\r\n");
                        sbscript.Append("public DateTime ");
                        sbscript.Append(gv.Name);
                        sbscript.Append("\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	get\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		if(_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("==NullValue.DateTime && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=null && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022!=string.Empty)\r\n");
                        sbscript.Append("			_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToDateTime(ExecuteScalar(\u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022,VarientType.SQL_DateTime));\r\n");
                        sbscript.Append("		return _");
                        sbscript.Append(gv.Name);
                        sbscript.Append(";\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("	set\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=value;\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("}\r\n");

                        sbscript.Append("public void Set_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("(object value)\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToDateTime(value);\r\n");
                        sbscript.Append("}\r\n");
                        break;
                    case VarientType.SQL_Decimal:
                        sbscript.Append("private double _");
                        sbscript.Append(gv.Name);
                        sbscript.Append("= NullValue.Decimal;\r\n");
                        sbscript.Append("public double ");
                        sbscript.Append(gv.Name);
                        sbscript.Append("\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	get\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		if(_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("==NullValue.Decimal && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=null && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=string.Empty)\r\n");
                        sbscript.Append("			_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToDouble(ExecuteScalar(\u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022,VarientType.SQL_Decimal));\r\n");
                        sbscript.Append("		return _");
                        sbscript.Append(gv.Name);
                        sbscript.Append(";\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("	set\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=value;\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("}\r\n");

                        sbscript.Append("public void Set_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("(object value)\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToDouble(value);\r\n");
                        sbscript.Append("}\r\n");
                        break;
                    case VarientType.SQL_String:
                        sbscript.Append("private string _");
                        sbscript.Append(gv.Name);
                        sbscript.Append("= NullValue.String;\r\n");
                        sbscript.Append("public string ");
                        sbscript.Append(gv.Name);
                        sbscript.Append("\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	get\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		if(_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("==NullValue.String && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=null && \u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022 !=string.Empty)\r\n");
                        sbscript.Append("			_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToString(ExecuteScalar(\u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022,VarientType.SQL_String));\r\n");
                        sbscript.Append("		return _");
                        sbscript.Append(gv.Name);
                        sbscript.Append(";\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("	set\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=value;\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("}\r\n");

                        sbscript.Append("public void Set_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("(object value)\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToString(value);\r\n");
                        sbscript.Append("}\r\n");
                        break;
                    case VarientType.String:
                        sbscript.Append("private string _");
                        sbscript.Append(gv.Name);
                        sbscript.Append("= NullValue.String;\r\n");
                        sbscript.Append("public string ");
                        sbscript.Append(gv.Name);
                        sbscript.Append("\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	get\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		if(_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("==NullValue.String)\r\n");
                        sbscript.Append("			_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=\u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022;\r\n");
                        sbscript.Append("		return _");
                        sbscript.Append(gv.Name);
                        sbscript.Append(";\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("	set\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=value;\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("}\r\n");

                        sbscript.Append("public void Set_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("(object value)\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=Convert.ToString(value);\r\n");
                        sbscript.Append("}\r\n");
                        break;
                    case VarientType.Object:
                        sbscript.Append("private object _");
                        sbscript.Append(gv.Name);
                        sbscript.Append("= null;\r\n");
                        sbscript.Append("public object ");
                        sbscript.Append(gv.Name);
                        sbscript.Append("\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	get\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		if(_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("==null)\r\n");
                        sbscript.Append("			_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=\u0022");
                        sbscript.Append(gv.Value);
                        sbscript.Append("\u0022;\r\n");
                        sbscript.Append("		return _");
                        sbscript.Append(gv.Name);
                        sbscript.Append(";\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("	set\r\n");
                        sbscript.Append("	{\r\n");
                        sbscript.Append("		_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=value;\r\n");
                        sbscript.Append("	}\r\n");
                        sbscript.Append("}\r\n");

                        sbscript.Append("public void Set_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("(object value)\r\n");
                        sbscript.Append("{\r\n");
                        sbscript.Append("	_");
                        sbscript.Append(gv.Name);
                        sbscript.Append("=value;\r\n");
                        sbscript.Append("}\r\n");
                        break;
                }
            }
            sbscript.Append("}\r\n");
            return sbscript.ToString();
        }

        private bool BeforeAddCellString(string scriptid)
        {
            if (_scriptids.Contains(scriptid))
                return true;
            _scriptids.Add(scriptid);
            return false;
        }

        internal string CellString(Cell cell, string s, ScriptType st)
        {
            if (s == null || s == "")
                return "";

            StringBuilder sbscript = new StringBuilder();
            #region preevent
            if (st == ScriptType.PreEvent)
            {
                if (BeforeAddCellString(cell.ScriptID))
                    return "";
                if (!(cell is IGridEvent) || ((cell as IGridEvent).EventType != EventType.BothContentAndSummary && (cell as IGridEvent).EventType != EventType.OnAll) || !(cell is Calculator))
                {
                    sbscript.Append("public class Cell");
                    sbscript.Append(ClassName(cell.ScriptID));
                    sbscript.Append(":ICellEvent\r\n");
                    sbscript.Append("{\r\n");
                    sbscript.Append("public void Prepaint(Report report, RowData data, Cell innercell, FilterSrv filter, AgileArgs args, DataHelper datahelper, ReportSummaryData reportsummary, RowBalance  rowbalance, AccumulateData accumulate, BalanceData balance,object[] others)\r\n");
                    sbscript.Append("{\r\n");
                    sbscript.Append("   Current current=null;\r\n");
                    sbscript.Append("   int grouplevels=report.GroupLevels;\r\n");
                    sbscript.Append("   int currentindex=-1;\r\n");
                    sbscript.Append("   int startindex=-1;\r\n");
                    sbscript.Append("   Groups groups=null;\r\n");
                    sbscript.Append("   Group currentgroup=null;\r\n");
                    sbscript.Append("   RowData columntodata=null;\r\n");
                    sbscript.Append("   Current previous=new Current(rowbalance);\r\n");
                    sbscript.Append("   if(rowbalance !=null )\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       currentindex=rowbalance.CurrentIndex;\r\n");
                    sbscript.Append("       startindex=rowbalance.StartIndex;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   SemiRow cells=(data!=null?data.SemiRow:null);\r\n");
                    sbscript.Append("   SemiRow row=cells;\r\n");
                    sbscript.Append("   IKeyToObject nametodata=cells as IKeyToObject;\r\n");
                    sbscript.Append("   StimulateCell cell=new StimulateCell(innercell);\r\n");
                    sbscript.Append("   if(data is Group)\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       cell.bInGroup=true;\r\n");
                    sbscript.Append("       currentgroup=data as Group;\r\n");
                    sbscript.Append("       columntodata=currentgroup;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   else if(!(data is ReportSummaryData))\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       current=new Current(data);\r\n");
                    sbscript.Append("       columntodata=data;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   else\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       cell.bInReportSummary=true;\r\n");
                    sbscript.Append("       columntodata=data;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   if(nametodata==null)\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       nametodata=columntodata as IKeyToObject;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   Global global;\r\n");
                    sbscript.Append("   if (datahelper.Global == null)\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       global = new Global();\r\n");
                    sbscript.Append("       global.DataHelper = datahelper;\r\n");
                    sbscript.Append("       datahelper.Global = global;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   else\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       global = datahelper.Global as Global;\r\n");
                    sbscript.Append("   }\r\n");
                    foreach (string name in _datasourcefunctions.Keys)
                    {
                        s = s.Replace("current." + name, "current." + _datasourcefunctions.GetString(name).ToString());
                        s = s.Replace("previous." + name, "previous." + _datasourcefunctions.GetString(name).ToString());
                    }
                    sbscript.Append(s);
                    sbscript.Append("\r\n}");
                    sbscript.Append("\r\n}\r\n");
                }
            }
            #endregion
            //if (st == ScriptType.Expression)
            //{
            //    IExpression ie = cell as IExpression;
            //    if (ie.Formula.Type == FormulaType.Filter)
            //    {
            //        sbscript.Append("public class Filter");
            //        sbscript.Append(ClassName(cell.Name));
            //        sbscript.Append(":IFltSrv\r\n");
            //        sbscript.Append("{\r\n");
            //        sbscript.Append("public object GetValue(FilterSrv filter)\r\n");
            //        sbscript.Append("{\r\n");

            //        sbscript.Append("return filter.");
            //        sbscript.Append(s);
            //        sbscript.Append(";");

            //        //--------------------
            //        //					sbscript.Append("return \u0022");
            //        //sbscript.Append(ie.Formula);
            //        //					sbscript.Append("\u0022;");
            //        //--------------------
            //        sbscript.Append("\r\n}");
            //        sbscript.Append("\r\n}\r\n");
            //    }
            //}
            if (st == ScriptType.Algorithm)
            {
                if (s == "IIndicator" && cell is IIndicator)
                {
                    HandleIndicatorScript(cell as IIndicator, sbscript);
                }
                else
                {
                    if (BeforeAddCellString(cell.ScriptID))
                        return "";
                    #region algorithm
                    IAlgorithm ia = cell as IAlgorithm;
                    sbscript.Append("public class Algorithm");
                    sbscript.Append(ClassName(cell.ScriptID));
                    sbscript.Append(":IAlgorithmSrv\r\n");
                    sbscript.Append("{\r\n");
                    sbscript.Append("public object GetValue(int grouplevels, int currentindex, int startindex, RowData data, FilterSrv filter, AgileArgs args, DataHelper datahelper, Cell innercell, DataSources datasources, ReportSummaryData reportsummary, RowBalance  rowbalance, AccumulateData accumulate, BalanceData balance,object[] others)\r\n");
                    sbscript.Append("{\r\n");
                    sbscript.Append("   DataRowsForEx dataRows=new DataRowsForEx(others==null?null:others[0] as DataTable);\r\n");
                    sbscript.Append("   ReportSummaryData reportsummarydata=reportsummary;\r\n");
                    sbscript.Append("   Groups groups=null;\r\n");
                    sbscript.Append("   StimulateCell cell=new StimulateCell(innercell);\r\n");
                    sbscript.Append("   Current current=null;\r\n");
                    sbscript.Append("   Group currentgroup=null;\r\n");
                    sbscript.Append("   RowData columntodata=null;\r\n");
                    sbscript.Append("   Current previous=new Current(rowbalance);\r\n");
                    sbscript.Append("   if(rowbalance !=null )\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       currentindex=rowbalance.CurrentIndex;\r\n");
                    sbscript.Append("       startindex=rowbalance.StartIndex;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   if(data is Group)\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       currentgroup=data as Group;\r\n");
                    sbscript.Append("       columntodata=currentgroup;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   else\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       current=new Current(data);\r\n");
                    sbscript.Append("       columntodata=data;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   Global global;\r\n");
                    sbscript.Append("   if (datahelper.Global == null)\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       global = new Global();\r\n");
                    sbscript.Append("       global.DataHelper = datahelper;\r\n");
                    sbscript.Append("       datahelper.Global = global;\r\n");
                    sbscript.Append("   }\r\n");
                    sbscript.Append("   else\r\n");
                    sbscript.Append("   {\r\n");
                    sbscript.Append("       global = datahelper.Global as Global;\r\n");
                    sbscript.Append("   }\r\n");
                    foreach (string name in _datasourcefunctions.Keys)
                    {
                        s = s.Replace("current." + name, "current." + _datasourcefunctions.GetString(name).ToString());
                        s = s.Replace("previous." + name, "previous." + _datasourcefunctions.GetString(name).ToString());
                    }
                    sbscript.Append(s);
                    //sbscript.Append(";");
                    sbscript.Append("\r\n}");
                    sbscript.Append("\r\n}\r\n");
                    #endregion
                }
            }

            //if (st == ScriptType.MapKeys)
            //{
            //    sbscript.Append("public class MapKeys");
            //    sbscript.Append(cell.Name);
            //    sbscript.Append(":IMapKeys\r\n");
            //    sbscript.Append("{\r\n");
            //    sbscript.Append("public string MapKeys(RowData data, FilterSrv filter, AgileArgs args, DataHelper datahelper)\r\n");
            //    sbscript.Append("{\r\n");
            //    sbscript.Append("   ReportData rows = new ReportData(reportdata);\r\n");
            //    sbscript.Append("   Current current=null;\r\n");
            //    sbscript.Append("   Global global;\r\n");
            //    sbscript.Append("   if (datahelper.Global == null)\r\n");
            //    sbscript.Append("   {\r\n");
            //    sbscript.Append("       global = new Global();\r\n");
            //    sbscript.Append("       global.DataHelper = datahelper;\r\n");
            //    sbscript.Append("       datahelper.Global = global;\r\n");
            //    sbscript.Append("   }\r\n");
            //    sbscript.Append("   else\r\n");
            //    sbscript.Append("   {\r\n");
            //    sbscript.Append("       global = datahelper.Global as Global;\r\n");
            //    sbscript.Append("   }\r\n");
            //    sbscript.Append(" return string.Format(\u0022");
            //    sbscript.Append((cell as SubReport).RowFilter.FilterString);
            //    sbscript.Append("\u0022,");
            //    //sbscript.Append(report.RowFilter.MapKeys.Replace(",","+\u0022,\u0022+"));
            //    sbscript.Append(s);
            //    sbscript.Append(");\r\n}");
            //    sbscript.Append("\r\n}\r\n");
            //}
            return sbscript.ToString();
        }

        private void HandleIndicatorScript(IIndicator indicator, StringBuilder sb)
        {
            if (indicator.DetailCompare != null)
            {
                if (indicator.DetailCompare.bExpression1Script)
                    sb.Append(CompareValueScript(indicator.DetailCompare.ScriptID, indicator.DetailCompare.Expression1Script));
                if (indicator.DetailCompare.bExpression2Script)
                    sb.Append(CompareValueScript(indicator.DetailCompare.ScriptID + "_2", indicator.DetailCompare.Expression2Script));
            }
            if (indicator.SummaryCompare != null)
            {
                if (indicator.SummaryCompare.bExpression1Script)
                    sb.Append(CompareValueScript(indicator.SummaryCompare.ScriptID, indicator.SummaryCompare.Expression1Script));
                if (indicator.SummaryCompare.bExpression2Script)
                    sb.Append(CompareValueScript(indicator.SummaryCompare.ScriptID + "_2", indicator.SummaryCompare.Expression2Script));
            }
            if (indicator.TotalCompare != null)
            {
                if (indicator.TotalCompare.bExpression1Script)
                    sb.Append(CompareValueScript(indicator.TotalCompare.ScriptID, indicator.TotalCompare.Expression1Script));
                if (indicator.TotalCompare.bExpression2Script)
                    sb.Append(CompareValueScript(indicator.TotalCompare.ScriptID + "_2", indicator.TotalCompare.Expression2Script));
            }
        }

        private string CompareValueScript(string scriptid, string s)
        {
            StringBuilder sbscript = new StringBuilder();
            sbscript.Append("public class Algorithm");
            sbscript.Append(scriptid);
            sbscript.Append(":IAlgorithmSrv\r\n");
            sbscript.Append("{\r\n");
            sbscript.Append("public object GetValue(int grouplevels, int currentindex, int startindex, RowData data, FilterSrv filter, AgileArgs args, DataHelper datahelper, Cell innercell, DataSources datasources, ReportSummaryData reportsummary, RowBalance  rowbalance, AccumulateData accumulate, BalanceData balance,object[] others)\r\n");
            sbscript.Append("{\r\n");
            sbscript.Append("   DataRowsForEx dataRows=new DataRowsForEx(others==null?null:others[0] as DataTable);\r\n");
            sbscript.Append("   ReportSummaryData reportsummarydata=reportsummary;\r\n");
            sbscript.Append("   Groups groups=null;\r\n");
            sbscript.Append("   StimulateCell cell=new StimulateCell(innercell);\r\n");
            sbscript.Append("   SemiRow cells=(data!=null?data.SemiRow:null);\r\n");
            sbscript.Append("   Current current=null;\r\n");
            sbscript.Append("   Group currentgroup=null;\r\n");
            sbscript.Append("   Current previous=new Current(rowbalance);\r\n");
            sbscript.Append("   if(rowbalance !=null )\r\n");
            sbscript.Append("   {\r\n");
            sbscript.Append("       currentindex=rowbalance.CurrentIndex;\r\n");
            sbscript.Append("       startindex=rowbalance.StartIndex;\r\n");
            sbscript.Append("   }\r\n");
            sbscript.Append("   if(data is Group)\r\n");
            sbscript.Append("       currentgroup=data as Group;\r\n");
            //sbscript.Append("   else\r\n");
            sbscript.Append("   current=new Current(data);\r\n");
            sbscript.Append("   Global global;\r\n");
            sbscript.Append("   if (datahelper.Global == null)\r\n");
            sbscript.Append("   {\r\n");
            sbscript.Append("       global = new Global();\r\n");
            sbscript.Append("       global.DataHelper = datahelper;\r\n");
            sbscript.Append("       datahelper.Global = global;\r\n");
            sbscript.Append("   }\r\n");
            sbscript.Append("   else\r\n");
            sbscript.Append("   {\r\n");
            sbscript.Append("       global = datahelper.Global as Global;\r\n");
            sbscript.Append("   }\r\n");
            foreach (string name in _datasourcefunctions.Keys)
            {
                s = s.Replace("current." + name, "current." + _datasourcefunctions.GetString(name).ToString());
                s = s.Replace("previous." + name, "previous." + _datasourcefunctions.GetString(name).ToString());
            }
            sbscript.Append(s);
            sbscript.Append("\r\n}");
            sbscript.Append("\r\n}\r\n");
            return sbscript.ToString();
        }

        public static string ClassName(string name)
        {
            return name.Replace("+", "")
                .Replace("-", "")
            .Replace("*", "")
            .Replace("/", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace(",", "")
            .Replace(" ", "")
            .Replace("%", "")
            .Replace("{", "")
            .Replace("}", "");
        }

        #region calculatorstring waste
        //internal string CalculatorString(ICalculator calculator)
        //{
        //    StringBuilder sbscript = new StringBuilder();
        //    if (calculator.Operator == OperatorType.ExpressionSUM)
        //    {
        //        #region expressionsum
        //        sbscript.Append("public class Algorithm");
        //        sbscript.Append(ClassName((calculator as Cell).Name));
        //        sbscript.Append(":IAlgorithmSrv\r\n");
        //        sbscript.Append("{\r\n");
        //        sbscript.Append("public object GetValue(int grouplevels, int currentindex, int startindex, RowData data, FilterSrv filter, AgileArgs args, DataHelper datahelper, Cell innercell, DataSources datasources, ReportSummaryData reportsummary, RowBalance  rowbalance, AccumulateData accumulate, BalanceData balance,object[] others)\r\n");
        //        sbscript.Append("{\r\n");
        //        if (calculator is ICalculateColumn)
        //        {
        //            sbscript.Append("   string colname=\u0022");
        //            sbscript.Append((calculator as IMapName).MapName);
        //            sbscript.Append("\u0022;\r\n");
        //            sbscript.Append("   Global global;\r\n");
        //            sbscript.Append("   if (datahelper.Global == null)\r\n");
        //            sbscript.Append("   {\r\n");
        //            sbscript.Append("       global = new Global();\r\n");
        //            sbscript.Append("       global.DataHelper = datahelper;\r\n");
        //            sbscript.Append("       datahelper.Global = global;\r\n");
        //            sbscript.Append("   }\r\n");
        //            sbscript.Append("   else\r\n");
        //            sbscript.Append("   {\r\n");
        //            sbscript.Append("       global = datahelper.Global as Global;\r\n");
        //            sbscript.Append("   }\r\n");

        //            sbscript.Append("   string expression=\u0022");
        //            sbscript.Append((calculator as ICalculateColumn).Expression);
        //            sbscript.Append("\u0022;\r\n");
        //            sbscript.Append("   string[] exp=ExpressionService.SplitExpression(expression);\r\n");
        //            sbscript.Append("   double[] sub=new double[exp.Length];\r\n");
        //            sbscript.Append("   for (int i = 0; i < exp.Length; i++)\r\n");
        //            sbscript.Append("   {\r\n");
        //            sbscript.Append("       sub[i] = 0;\r\n");
        //            sbscript.Append("       if (exp[i].Trim() == String.Empty  || !datasources.Contains(exp[i].Trim()))\r\n");
        //            sbscript.Append("           continue;\r\n");
        //            sbscript.Append("       sub[i]=Convert.ToDouble(data[exp[i].Trim()]);\r\n");                    
        //            sbscript.Append("    }\r\n");

        //            string expression = (calculator as ICalculateColumn).Expression.ToLower();
        //            string[] exp = ExpressionService.SplitExpression(expression);
        //            Hashtable htexp = new Hashtable();
        //            for (int i = 0; i < exp.Length; i++)
        //            {
        //                if (exp[i].Trim() == String.Empty || !_datasourcefunctions.Contains(exp[i].Trim()))
        //                    continue;
        //                if (!htexp.Contains(exp[i].Trim()))
        //                {
        //                    htexp.Add(exp[i].Trim(), "sub[" + i.ToString() + "]");
        //                }
        //            }

        //            foreach (string name in _datasourcefunctions.Keys)
        //            {
        //                if (htexp.Contains(name.ToLower().Trim()))
        //                    expression = expression.Replace(name.ToLower(), htexp[name.ToLower().Trim()].ToString());
        //            }

        //            sbscript.Append("return ");
        //            sbscript.Append(expression);
        //            sbscript.Append(";\r\n");
        //        }
        //        else
        //        {
        //            sbscript.Append("return 0;\r\n");
        //        }
        //        sbscript.Append("\r\n}");
        //        sbscript.Append("\r\n}\r\n");
        //        #endregion
        //    }       
        //    return sbscript.ToString();
        //}
        #endregion

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            this._outputassembly = null;
            if (this._references != null)
            {
                this._references.Clear();
                this._references = null;
            }
        }

        #endregion
    }

    public enum ScriptType
    {
        None,
        MapKeys,
        InitEvent,
        PreEvent,
        Algorithm,
        Expression,
        GroupFilter,
        Indicator
    }

    public enum ScriptLanguage
    {
        VBDotNet,
        CSharp
    }

    public class StimulateCell
    {
        private Cell _cell;
        private bool _bingroup = false;
        private bool _binreportsummary = false;

        public StimulateCell(Cell cell)
        {
            _cell = cell;
        }

        public bool bTitle
        {
            get
            {
                return _cell.Type == "Label" || _cell.Type == "SuperLabel";
            }
        }

        public bool bContent
        {
            get
            {
                return !bTitle && !bSummary;
            }
        }

        public bool bSummary
        {
            get
            {
                return _cell is Calculator;
            }
        }

        public bool bInGroup
        {
            get
            {
                return _bingroup;
            }
            set
            {
                _bingroup = value;
            }
        }

        public bool bInReportSummary
        {
            get
            {
                return _binreportsummary;
            }
            set
            {
                _binreportsummary = value;
            }
        }

        public string CellName
        {
            get
            {
                return _cell.Name;
            }
        }

        public Cell InnerCell
        {
            get
            {
                return _cell;
            }
        }

        public CrossColumnType CrossColumnType
        {
            get
            {
                return _cell.CrossColumnType;
            }
        }

        public int CrossIndex
        {
            get
            {
                return _cell.CrossIndex;
            }
        }

        public string Name
        {
            get
            {
                return _cell.Name;
            }
        }

        public bool Visible
        {
            get
            {
                return _cell.Visible;
            }
            set
            {
                _cell.Visible = value;
            }
        }

        public int X
        {
            get
            {
                return _cell.X;
            }
            set
            {
                _cell.X = value;
            }
        }

        public int Width
        {
            get
            {
                return _cell.Width;
            }
            set
            {
                _cell.Width = value;
            }
        }

        public object Value
        {
            get
            {
                return Caption;
            }
            set
            {
                Caption = value;
            }
        }

        public object Caption
        {
            get
            {
                return _cell.Caption;
            }
            set
            {
                if (value == null)
                    _cell.Caption = "";
                else
                    _cell.Caption = value.ToString();
            }
        }

        public System.Drawing.Color BackColor
        {
            get
            {
                return _cell.BackColor;
            }
            set
            {
                _cell.BackColor = value;
            }
        }

        public System.Drawing.Color ForeColor
        {
            get
            {
                return _cell.ForeColor;
            }
            set
            {
                _cell.ForeColor = value;
            }
        }
    }

    #region interface
    public interface IFltSrv
    {
        object GetValue(FilterSrv filter);
    }

    public interface IBusinessSrv
    {
        object GetValue(object businesssrv);
    }

    public interface IAlgorithmSrv
    {
        object GetValue(int grouplevels, int currentindex, int startindex, RowData data, FilterSrv filter, AgileArgs args, DataHelper datahelper, Cell innercell, DataSources datasources, ReportSummaryData reportsummary, RowBalance rowbalance, AccumulateData accumulate, BalanceData balance, object[] others);
    }

    public interface IMapKeys
    {
        string MapKeys(RowData data, FilterSrv filter, AgileArgs args, DataHelper datahelper, object[] others);
    }

    //public interface ISubMapKeys
    //{
    //    string MapKeys(IReportData reportdata, int currentindex, Group currentgroup, FilterSrv filter, AgileArgs args, DataHelper datahelper);
    //}

    public interface IReportInitEvent
    {
        void Init(Report report, FilterSrv filter, AgileArgs args, DataHelper datahelper, object[] others);
    }

    //public interface IGroupFilter
    //{
    //    bool GroupFilter(SemiRow cells, FilterSrv filter, AgileArgs args, DataHelper datahelper, object[] others);
    //}

    public interface ICellEvent
    {
        void Prepaint(Report report, RowData data, Cell innercell, FilterSrv filter, AgileArgs args, DataHelper datahelper, ReportSummaryData reportsummary, RowBalance rowbalance, AccumulateData accumulate, BalanceData balance, object[] others);
    }

    public interface ITest
    {
        object Test();
        DataHelper DataHelper { get; set; }
        void Init(DataHelper datahelper);
        void SaveGlobal(PageInfo pi);
        void RestoreGlobal(PageInfo pi);
    }
    #endregion
}
