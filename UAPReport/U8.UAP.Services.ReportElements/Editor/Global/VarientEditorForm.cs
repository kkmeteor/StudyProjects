using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// VarientEditorForm ��ժҪ˵����
	/// </summary>
	public class VarientEditorForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListBox varlist;
		private System.Windows.Forms.PropertyGrid prop;
		private System.Windows.Forms.Button add;
		private System.Windows.Forms.Button delete;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		/// <summary>
		/// ����������������
		/// </summary>
		private System.ComponentModel.Container components = null;

		public VarientEditorForm()
		{
			//
			// Windows ���������֧���������
			//
			InitializeComponent();

			//
			// TODO: �� InitializeComponent ���ú�����κι��캯������
			//
		}

		/// <summary>
		/// ������������ʹ�õ���Դ��
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

		#region Windows ������������ɵĴ���
		/// <summary>
		/// �����֧������ķ��� - ��Ҫʹ�ô���༭���޸�
		/// �˷��������ݡ�
		/// </summary>
		private void InitializeComponent()
		{
			this.varlist = new System.Windows.Forms.ListBox();
			this.prop = new System.Windows.Forms.PropertyGrid();
			this.add = new System.Windows.Forms.Button();
			this.delete = new System.Windows.Forms.Button();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// varlist
			// 
			this.varlist.ItemHeight = 12;
			this.varlist.Location = new System.Drawing.Point(8, 8);
			this.varlist.Name = "varlist";
			this.varlist.Size = new System.Drawing.Size(160, 256);
			this.varlist.TabIndex = 0;
			this.varlist.SelectedIndexChanged += new System.EventHandler(this.varlist_SelectedIndexChanged);
			// 
			// prop
			// 
			this.prop.CommandsVisibleIfAvailable = true;
			this.prop.LargeButtons = false;
			this.prop.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.prop.Location = new System.Drawing.Point(176, 8);
			this.prop.Name = "prop";
			this.prop.Size = new System.Drawing.Size(280, 256);
			this.prop.TabIndex = 1;
			this.prop.Text = "propertyGrid1";
			this.prop.ViewBackColor = System.Drawing.SystemColors.Window;
			this.prop.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// add
			// 
			this.add.Location = new System.Drawing.Point(8, 272);
			this.add.Name = "add";
			this.add.Size = new System.Drawing.Size(64, 23);
			this.add.TabIndex = 2;
			this.add.Text = "����";
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// delete
			// 
			this.delete.Location = new System.Drawing.Point(80, 272);
			this.delete.Name = "delete";
			this.delete.Size = new System.Drawing.Size(56, 23);
			this.delete.TabIndex = 3;
			this.delete.Text = "ɾ��";
			this.delete.Click += new System.EventHandler(this.delete_Click);
			// 
			// ok
			// 
			this.ok.Location = new System.Drawing.Point(320, 272);
			this.ok.Name = "ok";
			this.ok.Size = new System.Drawing.Size(64, 23);
			this.ok.TabIndex = 4;
			this.ok.Text = "ȷ��";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.Location = new System.Drawing.Point(392, 272);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(64, 23);
			this.cancel.TabIndex = 5;
			this.cancel.Text = "ȡ��";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// VarientEditorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(464, 301);
			this.ControlBox = false;
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.delete);
			this.Controls.Add(this.add);
			this.Controls.Add(this.prop);
			this.Controls.Add(this.varlist);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "VarientEditorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ȫ�ֱ�������";
			this.ResumeLayout(false);

		}
		#endregion

		private GlobalVarients _varients;
		private Hashtable _vhash=new Hashtable();
		public GlobalVarients Varients
		{
			get
			{
				return _varients;
			}
			set
			{
				_varients=value;
				varlist.Items.Clear();
				_vhash.Clear();
				if(value.Count>0)
				{
					foreach(GlobalVarient gv in value)
					{
                        gv.NameChanged -= new EventHandler(gv_NameChanged);
						varlist.Items.Add(gv.Name);
						_vhash.Add(gv.Name,gv);
                        gv.NameChanged += new EventHandler(gv_NameChanged);
					}
					_varients.RegisterVarientEvent();
					varlist.SelectedIndex=0;
					prop.SelectedObject=value[0];
				}
				else
					prop.SelectedObject=null;
			}
		}

        private void gv_NameChanged(object sender, EventArgs e)
        {
            int index = varlist.SelectedIndex;
            varlist.Items.Clear();
            _vhash.Clear();
			foreach(GlobalVarient gv in _varients )
			{
				varlist.Items.Add(gv.Name);
                _vhash.Add(gv.Name, gv);
			}
            varlist.SelectedIndex = index;
        }

		private void add_Click(object sender, System.EventArgs e)
		{
			GlobalVarient gv=new GlobalVarient();
			_varients.Add(gv);
			varlist.Items.Add(gv.Name);
			_vhash.Add(gv.Name,gv);
			varlist.SelectedItem=gv.Name;
            gv.NameChanged += new EventHandler(gv_NameChanged);
//			prop.SelectedObject=gv;
		}

		private void delete_Click(object sender, System.EventArgs e)
		{
			int index=varlist.SelectedIndex;
			if(index>-1)
			{
				GlobalVarient gv=_vhash[varlist.SelectedItem] as GlobalVarient ;
                gv.NameChanged -= new EventHandler(gv_NameChanged);
				_varients.Remove(gv);
				varlist.Items.Remove(gv.Name);
				_vhash.Remove(gv.Name);
				if(varlist.Items.Count>index)
					prop.SelectedObject=_vhash[varlist.SelectedItem];
				else
					varlist.SelectedIndex=varlist.Items.Count-1;
			}
			if(varlist.Items.Count==0)
				prop.SelectedObject=null;
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			this.DialogResult=DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult=DialogResult.Cancel;
			this.Close();
		}

		private void varlist_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(varlist.SelectedItem!=null)
			{
				prop.SelectedObject=_vhash[varlist.SelectedItem];
			}
		}
	}
}
