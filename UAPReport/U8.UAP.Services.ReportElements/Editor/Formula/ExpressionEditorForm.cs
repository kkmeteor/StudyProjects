using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.CodeDom.Compiler;
using System.Text;
using UFIDA.U8.UAP.Services.ExpressionDesigner;
using UFIDA.U8.UAP.Services.ReportFunctionsSrv;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ExpressionEditorControl 的摘要说明。
	/// </summary>
	public class ExpressionEditorForm: System.Windows.Forms.Form
	{
		/// <summary> 
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;
		private ExpressionDesigner.ExpressionDesigner _designer;
		private string _source;
		private object _object;
		private ScriptType _type;

		public ExpressionEditorForm()
		{
			// 该调用是 Windows.Forms 窗体设计器所必需的。
			InitializeComponent();

			// TODO: 在 InitializeComponent 调用后添加任何初始化
			_designer=new UFIDA.U8.UAP.Services.ExpressionDesigner.ExpressionDesigner();
			_designer.DesignerButtonOKClicked+=new DesignerButtonOKClickedHandle(_designer_DesignerButtonOKClicked);
			_designer.DesignerButtonCancelClicked+=new DesignerButtonCancelClickedHandle(_designer_DesignerButtonCancelClicked);
			_designer.BeforeShowIntelliSenceForm+=new DesignerShowIntelliSenceFormEventHandle(_designer_BeforeShowIntelliSenceForm);
			_designer.DesignerCompileSupport+=new DesignerCompileSupportHandle(_designer_DesignerCompileSupport);
			_designer.Dock=DockStyle.Fill;
			this.Controls.Add(_designer);
		}

		/// <summary> 
		/// 清理所有正在使用的资源。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region 组件设计器生成的代码
		/// <summary> 
		/// 设计器支持所需的方法 - 不要使用代码编辑器 
		/// 修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ExpressionEditorForm));
			// 
			// ExpressionEditorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(944, 629);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(640, 432);
			this.Name = "ExpressionEditorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "公式设计器";

		}
		#endregion

		public string Source
		{
			get
			{
				return _source;
			}
			set
			{
				_source=value;
			}
		}

		public object EditObject
		{
			set
			{
				_object=value;
			}
		}

		public ScriptType ScriptType
		{
			get
			{
				return _type;
			}
			set
			{
				_type=value;
			}
		}

		public ExpressionDesigner.ExpressionDesigner.EditType Type
		{
			set
			{
				U8LoginInfor login=ClientReportContext.Login;
				_designer.InitDesigner(
					login.UfDataCnnString ,
					value,
					_source,
					GenerateNodeCollection(),
					(int)this.ScriptType );
			}
		}

		private NodeCollection GenerateNodeCollection()
		{
            Report report=null;
            if (_object is Report)
            {
                report = _object as Report;
            }
            else if (_object is RowFilter)
            {
                report = (_object as RowFilter).Parent;
            }
            else if (_object is Cell)
            {
                (_object as Cell).GetReport();
                report = (_object as Cell).Report;
            }
            else
            {
                return null;
            }
            DataSources dss = report.DataSources;
            NodeCollection dc = new NodeCollection(UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.AA.AppServerConfig.DsiCode", System.Threading.Thread.CurrentThread.CurrentUICulture.Name));
			if(dss==null)
				return dc;
			foreach(string name in dss.Keys)
			{
				DataSource ds=dss[name];
				dc.Add(new NodeLeaf(ds.Name,GetNodeType(ds.Type ),ds.Caption));//add type
			}
			return dc;
		}

		private NodeLeaf.Type GetNodeType(DataType type)
		{
			switch(type)
			{
				case DataType.Boolean:
					return NodeLeaf.Type.Boolean;
				case DataType.Currency :
					return NodeLeaf.Type.Currency;
				case DataType.DateTime :
					return NodeLeaf.Type.DateTime;
				case DataType.Decimal :
					return NodeLeaf.Type.Decimal;
				case DataType.Image :
					return NodeLeaf.Type.Image;
				case DataType.Int :
					return NodeLeaf.Type.Int;
				default:
					return NodeLeaf.Type.String;
			}
		}

		private void _designer_DesignerButtonCancelClicked(object sender, EventArgs e)
		{
			this.DialogResult=DialogResult.Cancel;
			this.Close();
		}

		private void _designer_BeforeShowIntelliSenceForm(object sender, DesignerShowIntelliSenceFromArgs e)
		{
			//
		}

		private void _designer_DesignerButtonOKClicked(object sender, DesignerCompilerArgs e)
		{
			_source=e.ResultString;
			this.DialogResult=DialogResult.OK;
			this.Close();
		}

		private void _designer_DesignerCompileSupport(object sender, DesignerCompilerArgs e)
		{
            if (e.EditType != ExpressionDesigner.ExpressionDesigner.EditType.Script)
                return;
            ScriptHelper sh = null;
            Report report=null;
            string reportinitstring = null;
            string reportrowfiltermapkeys = null;

            if (_object is Report)
            {
                report = _object as Report;
                reportinitstring = report.InitEvent;
                report.InitEvent = e.ResultString;
                sh = new ScriptHelper(report);
            }
            else if (_object is RowFilter)
            {
                report = (_object as RowFilter).Parent;
                sh = new ScriptHelper(report);
                reportrowfiltermapkeys = (_object as RowFilter).MapKeys;
                (_object as RowFilter).MapKeys = e.ResultString;
            }
            else
            {
                #region cell
                (_object as Cell).GetReport();
                report = (_object as Cell).Report;

                foreach (Section section in report.Sections)
                {
                    foreach (Cell cell in section.Cells)
                    {
                        if (cell is IMapName)
                        {
                            IMapName map = cell as IMapName;
                            if (!report.DataSources.Contains(map.MapName.Trim()))
                            {
                                DataType dt = DataType.String;
                                if (map is IDecimal)
                                    dt = DataType.Decimal;
                                else if (map is IDateTime)
                                    dt = DataType.DateTime;

                                report.DataSources.Add(new DataSource(map.MapName.Trim(), dt));
                            }
                        }
                    }
                }

                sh = new ScriptHelper(report);
                string cellstring = sh.CellString(_object as Cell , e.ResultString, _type);

                AddScriptKey(e.ResultString, report, sh);

                foreach (string name in sh.DSFunctions.Keys)
                {
                    cellstring = cellstring.Replace("current." + name, "current." + sh.DSFunctions.GetString(name).ToString());
                    cellstring = cellstring.Replace("previous." + name, "previous." + sh.DSFunctions.GetString(name).ToString());
                }
                sh.AddScript(cellstring);
                #endregion
            }

            sh.References.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportData.dll");
            sh.References.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportElements.dll");
            sh.References.Add(ExecutePath + "UFIDA.U8.UAP.Services.ReportFilterService.dll");
            //sh.References.Add(ExecutePath + "UFSoft.U8.Report.Exhibition.dll");
            //sh.References.Add(ExecutePath + "UFSoft.U8.Ex.Filter.dll");
			sh.OutputAssembly=null;
			CompilerResults cr= sh.Compile();

			if(cr.Errors.Count>0)
			{
				e.IsCompileSucceed=false;
				for(int i=0;i<cr.Errors.Count;i++)
				{
					cr.Errors[i].Line-=18;
				}
				e.Error=cr.Errors;
			}
			else
			{
				e.IsCompileSucceed=true;
			}
            if (reportinitstring != null)
                (_object as Report).InitEvent = reportinitstring;
            if (reportrowfiltermapkeys != null)
                (_object as RowFilter).MapKeys = reportrowfiltermapkeys;
        }

        private void AddScriptKey(string keystring,Report report,ScriptHelper sh)
        {
            if (string.IsNullOrEmpty(keystring))
                return;
            foreach (string key in report.DataSources.Keys)
            {
                if (keystring.ToLower().Contains(key.ToLower()))
                {
                    sh.AddAKey(key);
                    keystring = keystring.ToLower().Replace(key.ToLower(), " ");
                }
            }
        }

        private string ExecutePath
        {
            get
            {
                if (ClientReportContext.Login.SubID == "OutU8")
                {
                    return System.Environment.SystemDirectory + @"\UFCOMSQL\UAP\";
                }
                else
                {
                    string referencepath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                    if (!referencepath.EndsWith(@"\"))
                        referencepath += @"\";

                    if (!referencepath.ToLower().EndsWith(@"\uap\"))
                        referencepath += @"UAP\";

                    return referencepath;
                }
            }
        }
    }
}
