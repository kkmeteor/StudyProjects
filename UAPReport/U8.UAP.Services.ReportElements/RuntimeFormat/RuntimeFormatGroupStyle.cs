/*
 * 作者:卢达其
 * 时间:2008.3.20
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 一个表格分组中一个样式的格式信息
	/// </summary>
	[Serializable]
	public class RuntimeFormatGroupStyle : ConfigXmlItem
	{
		/// <summary>
		/// 默认不折行
		/// </summary>
		public RuntimeFormatGroupStyle()
		{
			this.SetProperty( RuntimeFormatServerContext.XmlKeyIsFoldRow, false );
		}

		/// <summary>
		/// 当前对象名称
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
