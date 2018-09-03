/*
 * ����:¬����
 * ʱ��:2008.3.20
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Data;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// һ������ʽ��Ϣ
	/// </summary>
	[Serializable]
	public class RuntimeFormat : ConfigXmlItem
	{
        private string _outu8filterstring = null;
        private string _outu8formatclass = null;
        private bool _bempty = false;
		private List<IRuntimeFormatArg> _runtimeFormatArg = new List<IRuntimeFormatArg>();

		/// <summary>
		/// ���캯��
		/// </summary>
		public RuntimeFormat()
		{
			Hashtable ht = new Hashtable();
			ht.Add( ConfigXmlContext.ArgKeyAssemblyName, "UFIDA.U8.UAP.Services.ReportElements" );
			ht.Add( ConfigXmlContext.ArgKeyTypeNameSpace, "UFIDA.U8.UAP.Services.ReportElements" );
			this._runtimeFormatArg.Add( new RuntimeFormatArgDynamicDataSourceCol( this ));
			this._runtimeFormatArg.Add( new RuntimeFormatArgColSummaryType( this ));
			this.Context = ht;
		}

		protected override void InitAfterSettedContext()
		{
			this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeyIsSavedByEndUser, ConfigXmlContext.TypeKeyBool );
		}

        public bool bEmpty
        {
            get
            {
                return _bempty;
            }
            set
            {
                _bempty = value;
            }
        }

        public string OutU8FormatClass
        {
            get
            {
                return _outu8formatclass;
            }
            set
            {
                _outu8formatclass = value;
            }
        }

        public string OutU8FilterString
        {
            get
            {
                return _outu8filterstring;
            }
            set
            {
                _outu8filterstring = value;
            }
        }

		/// <summary>
		/// ��ǰ��������
		/// </summary>
		public override string Name 
		{
			get { return this.GetType().Name; }
		}

		/// <summary>
		/// ��ȡ��ز��������ݲ���,�綯̬��ӵ�����Դ����,�����е����õ�
		/// </summary>
		public Hashtable GetData()
		{
			Hashtable data = new Hashtable();
			for (int i = 0; i < this._runtimeFormatArg.Count; i++)
				data.Add( this._runtimeFormatArg[i].Id, this._runtimeFormatArg[i].Tag );
			return data;
		}

		/// <summary>
		/// ��ȡһ�����������ʱ��ʽ
		/// </summary>
		public RuntimeFormatGroupFormat GetGroup(string id)
		{
			return this.GetSubItem(id) as RuntimeFormatGroupFormat;
		}

		/// <summary>
		/// ��ȡһ��չ����ʽ������ʱ��ʽ
		/// </summary>
		public RuntimeFormatGroupStyle GetStyle(
			string groupId, 
			string styleMode)
		{ 
			RuntimeFormatGroupFormat group = this.GetGroup( groupId );
			if( group != null )
				return (RuntimeFormatGroupStyle)group.GetSubItem( styleMode );
			return null;
		}

		/// <summary>
		/// ��ȡһ�����鼶�������ʱ��ʽ
		/// </summary>
		public RuntimeFormatGroupLevel GetLevel(
			string groupId,
			string styleMode,
			string levelKey )
		{ 
			RuntimeFormatGroupStyle style = this.GetStyle( groupId, styleMode );
			if( style != null )
				return (RuntimeFormatGroupLevel)style.GetSubItem( levelKey );
			return null;
		}
		
		/// <summary>
		/// ��ȡһ���е�����ʱ��ʽ
		/// </summary>
		public RuntimeFormatGridColumn GetColumn(
			string groupId,
			string styleMode,
			string levelKey,
			string colName)
		{ 
			RuntimeFormatGroupLevel gLevel = this.GetLevel( groupId, styleMode, levelKey );
			if( gLevel != null )
				return gLevel.GetColumn(colName);
			return null;
		}

		/// <summary>
		/// ��ֱ���޸����ݿ�ķ�ʽ���һ�����������ʱ��ʽ
		/// </summary>
		public static void RemoveGroupStyle(
			string viewId, 
			string groupId,
			string ufMetaCnnString)
		{
            RemoteDataHelper rdh = DefaultConfigs.GetRemoteHelper();
            rdh.RemoveGroupStyle(viewId, groupId, ufMetaCnnString);
		}
	}
}