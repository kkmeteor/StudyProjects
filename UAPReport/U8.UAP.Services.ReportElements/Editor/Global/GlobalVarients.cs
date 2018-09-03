using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GlobalVarients 的摘要说明。
	/// </summary>
	[Serializable]
	[Editor(typeof(VarientEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public class GlobalVarients:CollectionBase,ICloneable,IDisposable
	{
        [NonSerialized]
        private DataHelper _datahelper;
        public DataHelper DataHelper
        {
            get
            {
                return _datahelper;
            }
            set
            {
                _datahelper = value;
            }
        }
		public GlobalVarient this[int index]
		{
			get
			{
				return this.InnerList[index] as GlobalVarient;
			}
		}

		public void Add(GlobalVarient value)
		{
			if(value.Name.Trim()=="")
			{
				CalcName(value);
			}
			UnRegisterVarientEvent(value);
			RegisterVarientEvent(value);
			this.InnerList.Add(value);
		}

		public void RegisterVarientEvent()
		{
			foreach(GlobalVarient gv in this)
				RegisterVarientEvent(gv);
		}

		public void UnRegisterVarientEvent()
		{
			foreach(GlobalVarient gv in this)
				UnRegisterVarientEvent(gv);
		}
		private void RegisterVarientEvent(GlobalVarient value)
		{
			value.NameCheck+=new VarientNameCheckHandler(value_NameCheck);
			value.Validate+=new VarientValidateHandler(value_Validate);
		}

		private void UnRegisterVarientEvent(GlobalVarient value)
		{
			value.NameCheck-=new VarientNameCheckHandler(value_NameCheck);
			value.Validate-=new VarientValidateHandler(value_Validate);
		}

		private void CalcName(GlobalVarient value)
		{
			int count = 1;
			string name = "Var" + count.ToString();
			while (FindName(name))
			{
				count ++;
				name = "Var" + count.ToString();
			}
			value.Name=name;
		}

		private bool FindName(string name)
		{
			foreach(GlobalVarient var in this)
			{
				if(var.Name.ToLower().Trim()==name.ToLower().Trim())
					return true;
			}
			return false;
		}		

		public void Remove(GlobalVarient value)
		{
			this.InnerList.Remove(value);
		}

		public bool Contains(GlobalVarient value)
		{
			return this.InnerList.Contains(value);
		}

		public override string ToString()
		{
			MemoryStream fs=new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			try 
			{
				formatter.Serialize(fs, this);
				return Convert.ToBase64String(fs.ToArray());
			}
			catch (SerializationException ex) 
			{
				throw ex;
			}
			finally 
			{
				fs.Close();
			}
		}

		public static GlobalVarients FromString(string s)
		{
			MemoryStream ms=new MemoryStream(Convert.FromBase64String(s));
			try 
			{
				GlobalVarients gvs;
				BinaryFormatter formatter = new BinaryFormatter();
				gvs= (GlobalVarients) formatter.Deserialize(ms);
				return gvs;
			}
			catch (SerializationException ex) 
			{
				throw ex;
			}
			finally 
			{
				ms.Close();
			}
		}

		private string value_NameCheck(string name)
		{
			if(name.Trim()=="")
                return UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx8", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
			if(FindName(name))
                return UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx9", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
			return "";
		}

		private string value_Validate(string name, VarientType type, string value)
		{
			GlobalVarients gvs=new GlobalVarients();
			gvs.Add(new GlobalVarient(name,type,value));
			
			ScriptHelper sh=new ScriptHelper();
            sh.AddScript(sh.VarientString(gvs));
            sh.References.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportData.dll");
            sh.References.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportElements.dll");
            sh.References.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportFilterService.dll");
			sh.OutputAssembly=null;
			CompilerResults cr= sh.Compile();

			if(cr.Errors.Count>0)
                return UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx10", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);

			try
			{
                ITest test = ((ITest)ScriptHelper.FindInterface(cr.CompiledAssembly, "UFIDA.U8.UAP.Services.DynamicReportComponent.DRCtmp.Global"));
                test.DataHelper = _datahelper;
                test.Test();
			}
			catch
			{
                return UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx11", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
			}
			return "";
		}

        private string ExecutePath
        {
            get
            {
                string referencepath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                if (!referencepath.EndsWith(@"\"))
                    referencepath += @"\";

                if (!referencepath.ToLower().EndsWith(@"\uap\"))
                    referencepath += @"UAP\";

                return referencepath;
            }
        }

		#region ICloneable 成员

		public object Clone()
		{
			GlobalVarients gvs=new GlobalVarients();
			foreach(GlobalVarient gv in this)
				gvs.Add(gv.Clone() as GlobalVarient);
			return gvs;
		}

		#endregion

        #region string builder
        /*
        internal string HeaderString()
        {
            StringBuilder sbscript = new StringBuilder();
            sbscript.Append("using System.Windows.Forms;\r\n");
            sbscript.Append("using System;\r\n");
            sbscript.Append("using System.Data;\r\n");
            sbscript.Append("using System.Drawing;\r\n");
            sbscript.Append("using System.Collections;\r\n");
            sbscript.Append("using System.Data.SqlClient;\r\n");
            sbscript.Append("using System.Text;\r\n");
            sbscript.Append("using System.Globalization;\r\n");
            sbscript.Append("using UFIDA.U8.UAP.Services.ReportData;\r\n");
            sbscript.Append("using UFIDA.U8.UAP.Services.ReportElements;\r\n");
            sbscript.Append("using UFIDA.U8.UAP.Services.ReportFilterService;\r\n");
            return sbscript.ToString();
        }

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
                        break;
                }
            }
            sbscript.Append("}\r\n");
            return sbscript.ToString();
        }
        */
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            _datahelper = null ;
            this.InnerList.Clear();
        }

        #endregion
    }
}
