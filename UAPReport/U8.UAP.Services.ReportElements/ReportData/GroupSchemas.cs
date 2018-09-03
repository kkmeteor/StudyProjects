using System;
using System.Collections;
using System.Xml;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupSchemas ��ժҪ˵����
	/// </summary>
	[Serializable]
	public partial class GroupSchemas : CollectionBase,ICloneable,IDisposable
	{
		private GroupSchema _default = null;
        private bool _bcross = false;        

        public GroupSchemas()
        {
        }

        public GroupSchemas(bool bcross)
        {
            _bcross = bcross;            
        }

		public GroupSchema this[int index]
		{
			get {  return List[index] as GroupSchema;  }
		}

        public bool bCross
        {
            get
            {
                return _bcross;
            }
            set
            {
                _bcross = value;
            }
        }

        
		public GroupSchema this[string id]
		{
			get 
			{  
				for(int i=0;i<Count;i++)
				{
					if(this[i].ID.ToLower()==id.ToLower())
						return this[i];  
				}
				return null;
			}
		}

        public GroupSchema Default
        {
            get 
            {
                if (this._default != null)
                    return this._default;
                else if (this.Count > 0)
                    return this[0];
                return null;
            }
			set { this._default = value; }
        }

        public GroupSchema GetByName(string name)
		{
			for(int i=0;i<Count;i++)
			{
				if(this[i].Name.ToLower()==name.ToLower())
					return this[i];  
			}
			return null;
        }

        public bool Contains(string groupschemaid)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].ID.ToLower() == groupschemaid.ToLower())
                    return true;
            }
            return false;
        }

        private void BeforeAdd(GroupSchema value)
        {
            if (_bcross)
            {
                value.CrossHandle();
            }
        }

		public int Add( GroupSchema value )  
		{
            BeforeAdd(value);
			return( List.Add( value ) );
		}

        public void Append(int index, GroupSchema value)
        {
            BeforeAdd(value);
                this.List.Insert(index, value);
        }

		public int IndexOf( GroupSchema value )  
		{
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, GroupSchema value )  
		{
            BeforeAdd(value);
			List.Insert( index, value );
		}

		public void Remove( GroupSchema value )  
		{
			List.Remove( value );
		}

		public bool Contains( GroupSchema value )  
		{
			return( List.Contains( value ) );
		}

		/// <summary>
		/// �����������
		/// </summary>
		/// <param name="report">����ṹ����</param>
		/// <param name="schemaXmlDoc">�洢����ṹ������Ϣ��XmlDocument</param>
		/// <param name="detailtype">��Ӧ�ı�������</param>
		/// <param name="localeid">��ǰ����������</param>
		public static void Initialize(
			Report report,
			XmlDocument schemaXmlDoc,
			SectionType detailtype,
			string localeid )
		{
			report.GroupSchemas = GroupSchemas.GetSchemas(schemaXmlDoc, localeid);
			
			// ���õ�ǰ����ID
			if(report.CurrentSchema == null)
				report.CurrentSchemaID = report.GroupSchemas[0].ID;
        }

        public static void InitializeCross(
            Report report,
            XmlDocument schemaXmlDoc,
            string localeid)
        {
            report.CrossSchemas  = GroupSchemas.GetSchemas(schemaXmlDoc, localeid,true);

            // ���õ�ǰ����ID
            if (report.CurrentCrossSchema  == null)
                report.CurrentCrossID  = report.CrossSchemas[0].ID;
        }

		public static GroupSchemas GetSchemas(
			string xml,
			string localeid)
		{
			XmlDocument doc = null;
			if (!string.IsNullOrEmpty(xml))
			{
				doc = new XmlDocument();
				doc.LoadXml(xml);
			}
			return GroupSchemas.GetSchemas(doc, localeid);
		}
        public static GroupSchemas GetSchemasNoSetDefaultSchema(
            string xml,
            string localeid)
        {
            XmlDocument schemaXmlDoc = null;
            if (!string.IsNullOrEmpty(xml))
            {
                schemaXmlDoc = new XmlDocument();
                schemaXmlDoc.LoadXml(xml);
            }

            GroupSchemas gss = new GroupSchemas(false);
            if (schemaXmlDoc != null)
            {
                XmlElement root = schemaXmlDoc.DocumentElement;
                if (root != null)
                    foreach (XmlElement ele in root.ChildNodes)
                        gss.Add(GroupSchemas.GetGroupSchemaFromXml(ele, localeid));
            }

            // ���û���޷������Ҫ���
            if (gss[GroupSchemas._xmlValueDefaultNoGroupId] == null)
            {
                if (false)
                    gss.Add(GroupSchemas.GetDefaultCrossSchema());
                else
                    gss.Add(GroupSchemas.GetDefaultGroupSchema());
            }           
            return gss;
        }

        public static GroupSchemas GetSchemas(
            XmlDocument schemaXmlDoc,
            string localeid)
        {
            return GetSchemas(schemaXmlDoc, localeid, false);
        }
		/// <summary>
		/// ��XML��ȡ���鼯�϶���
		/// </summary>
		public static GroupSchemas GetSchemas(
			XmlDocument schemaXmlDoc,
			string localeid,bool bcross)
		{
			GroupSchemas gss = new GroupSchemas(bcross);
			if (schemaXmlDoc != null)
			{
				XmlElement root = schemaXmlDoc.DocumentElement;
				if (root != null)
					foreach (XmlElement ele in root.ChildNodes)
						gss.Add(GroupSchemas.GetGroupSchemaFromXml(ele, localeid));
			}
			
			// ���û���޷������Ҫ���
            if (gss[GroupSchemas._xmlValueDefaultNoGroupId] == null)
            {
                if (bcross)
                    gss.Add(GroupSchemas.GetDefaultCrossSchema());
                else
                gss.Add(GroupSchemas.GetDefaultGroupSchema());
            }

			// �����û��Ĭ�Ϸ���,ȡ��һ������ΪĬ�Ϸ���
			GroupSchema defaultSchema = gss[0];
			foreach(GroupSchema gs in gss)
				if( gs.bDefault )
				{
					defaultSchema = gs;
					break;
				}
			defaultSchema.bDefault = true;
			gss.Default = defaultSchema;
			return gss;
        }

		#region ICloneable ��Ա

		public object Clone()
		{
			GroupSchemas gss=new GroupSchemas();
			foreach(GroupSchema gs in this.InnerList)
				gss.Add(gs.Clone() as GroupSchema);
			return gss;
		}

		#endregion

        #region IDisposable ��Ա

        public void Dispose()
        {
            foreach (GroupSchema gs in this.InnerList)
            {
                gs.Dispose();
            }
            this.Clear();
        }

        #endregion
    }
}
