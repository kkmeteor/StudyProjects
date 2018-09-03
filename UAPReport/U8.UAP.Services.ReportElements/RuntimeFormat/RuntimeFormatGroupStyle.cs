/*
 * ����:¬����
 * ʱ��:2008.3.20
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// һ����������һ����ʽ�ĸ�ʽ��Ϣ
	/// </summary>
	[Serializable]
	public class RuntimeFormatGroupStyle : ConfigXmlItem
	{
		/// <summary>
		/// Ĭ�ϲ�����
		/// </summary>
		public RuntimeFormatGroupStyle()
		{
			this.SetProperty( RuntimeFormatServerContext.XmlKeyIsFoldRow, false );
		}

		/// <summary>
		/// ��ǰ��������
		/// </summary>
		public override string Name 
		{
			get { return this.GetType().Name; }
		}

		protected override void InitAfterSettedContext()
		{
			this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeyIsFoldRow, ConfigXmlContext.TypeKeyBool );
		}

		public RuntimeFormatGroupLevel GetLevel( string levelKey )
		{ 
			return (RuntimeFormatGroupLevel)this.GetSubItem( levelKey );
		}
	}
}
