using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
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
	[Editor(typeof(ActionEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public class SelfActions:CollectionBase,ICloneable,ICollectonEditTypes
	{
        [NonSerialized]
        private string _designlocaleid;

        public string DesignLocaleID
        {
            set
            {
                _designlocaleid = value;
            }
        }

		public SelfAction this[int index]
		{
			get
			{
				return this.InnerList[index] as SelfAction;
			}
		}

		public SelfAction this[string name]
		{
			get
			{
				for(int i=0;i<Count;i++)
				{
					if(this[i].Name.ToLower().Trim()==name.ToLower().Trim())
						return this[i];
				}
				return null;
			}
		}

        public SelfAction UseAllDataAction
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].bUseAllData)
                        return this[i];
                }
                return null;
            }
        }

		public void Add(SelfAction value)
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
			foreach(SelfAction gv in this)
				RegisterVarientEvent(gv);
		}

		public void UnRegisterVarientEvent()
		{
			foreach(SelfAction gv in this)
				UnRegisterVarientEvent(gv);
		}
		private void RegisterVarientEvent(SelfAction value)
		{
			value.NameCheck+=new NameCheckHandler(value_NameCheck);
			value.bDoubleClickActionEvent+=new EventHandler(value_bDoubleClickActionEvent);
			value.CaptionCheck+=new NameCheckHandler(value_CaptionCheck);
            value.DesignLocaleID += new DesignLocaleIDFatcher(value_DesignLocaleID);
		}

        private string value_DesignLocaleID()
        {
            return _designlocaleid;
        }
		private void UnRegisterVarientEvent(SelfAction value)
		{
			value.NameCheck-=new NameCheckHandler(value_NameCheck);
			value.bDoubleClickActionEvent-=new EventHandler(value_bDoubleClickActionEvent);
			value.CaptionCheck-=new NameCheckHandler(value_CaptionCheck);
            value.DesignLocaleID-= new DesignLocaleIDFatcher(value_DesignLocaleID);
		}

		private void CalcName(SelfAction value)
		{
			int count = 1;
			string name = "Action" + count.ToString();
			while (FindName(name))
			{
				count ++;
				name = "Action" + count.ToString();
			}
			value.Name=name;
			value.Caption=name;
		}

		private bool FindName(string name)
		{
			foreach(SelfAction var in this)
			{
				if(var.Name.ToLower().Trim()==name.ToLower().Trim())
					return true;
			}
			return false;
		}		

		private bool FindCaption(string caption)
		{
			foreach(SelfAction var in this)
			{
				if(var.Caption.ToLower().Trim()==caption.ToLower().Trim())
					return true;
			}
			return false;
		}		

		public void Remove(SelfAction value)
		{
			this.InnerList.Remove(value);
		}

		public bool Contains(SelfAction value)
		{
			return this.InnerList.Contains(value);
		}

		public bool Contains(string caption)
		{
			return this[caption]!=null;
		}

		public SelfAction DoubleClickAction
		{
			get
			{
				for(int i=0;i<Count;i++)
				{
					if(this[i].bDoubleClickAction)
						return this[i];
				}
				return null;
			}
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

		public static SelfActions FromString(string s)
		{
			MemoryStream ms=new MemoryStream(Convert.FromBase64String(s));
			try 
			{
				SelfActions sas;
				BinaryFormatter formatter = new BinaryFormatter();
				sas= (SelfActions) formatter.Deserialize(ms);
				return sas;
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

		private string value_CaptionCheck(string name)
		{
			if(name.Trim()=="")
                return UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx12", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
			if(FindCaption(name))
                return UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Tx13", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
			return "";
		}

		private void value_bDoubleClickActionEvent(object sender, EventArgs e)
		{
			foreach(SelfAction var in this)
			{
				var.bDoubleClickAction=false;
			}
		}

		#region ICloneable 成员

		public object Clone()
		{
			SelfActions gvs=new SelfActions();
			foreach(SelfAction gv in this)
				gvs.Add(gv.Clone() as SelfAction);
            gvs.DesignLocaleID = _designlocaleid;
			return gvs;
		}

		#endregion


        #region ICollectonEditTypes Members


        public ICollectionEditType AddNew()
        {
            SelfAction sa = new SelfAction();
            Add(sa);
            return sa;
        }

        public void Remove(ICollectionEditType ice)
        {
            this.InnerList.Remove(ice);
        }

        #endregion
    }

    public delegate  string DesignLocaleIDFatcher();

    [Serializable]
    [Editor(typeof(ActionEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class Informations : CollectionBase,ICollectonEditTypes,ICloneable 
    {
        public Information this[int index]
        {
            get
            {
                return InnerList[index] as Information;
            }
        }

        public Information this[string name]
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].Name.ToLower().Trim() == name.ToLower().Trim())
                        return this[i];
                }
                return null;
            }
        }

        public void Add(Information value)
        {
            this.InnerList.Add(value);
        }

        public bool Contains(string name)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].Name.ToLower().Trim() == name.ToLower().Trim())
                    return true;
            return false;
        }

        private bool FindName(string name)
        {
            foreach (Information  var in this)
            {
                if (var.Name.ToLower().Trim() == name.ToLower().Trim())
                    return true;
            }
            return false;
        }

        #region ICollectonEditTypes Members

        public void RegisterVarientEvent()
        {
            
        }

        public void UnRegisterVarientEvent()
        {
            
        }

        public ICollectionEditType AddNew()
        {
            Information infor = new Information();
            int count = 1;
            string name = "Information" + count.ToString();
            while (FindName(name))
            {
                count++;
                name = "Information" + count.ToString();
            }
            infor.Name = name;

            Add(infor);
            return infor;
        }

        public void Remove(ICollectionEditType ice)
        {
            this.InnerList.Remove(ice);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            Informations infors = new Informations();
            foreach (ICloneable info in this)
            {
                infors.Add(info.Clone() as Information);
            }
            return infors;
        }

        #endregion

        public string InstanceInformation(string id, string value)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Information infor = this[id];
                if (infor != null)
                    return string.Format(infor.InformationHandler, value);
            }
            return null;
        }
    }

    public interface ICollectonEditTypes:ICollection ,IEnumerable ,IList 
    {
        void RegisterVarientEvent();
        void UnRegisterVarientEvent();
        ICollectionEditType AddNew();
        void Remove(ICollectionEditType ice);
    }
}
