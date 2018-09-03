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
	/// 一个表格列的格式信息
	/// </summary>
	[Serializable]
	public class RuntimeFormatGridColumn : ConfigXmlItem
	{
		protected override void InitAfterSettedContext()
		{
            this.PropertyMap.Add(RuntimeFormatServerContext.XmlKeyIsVisible, ConfigXmlContext.TypeKeyBool);
            this.PropertyMap.Add(RuntimeFormatServerContext.XmlKeyIsMerge, ConfigXmlContext.TypeKeyBool);
			this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeyWidth, ConfigXmlContext.TypeKeyInt );
			//this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeySpanX, ConfigXmlContext.TypeKeyInt );
			//this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeySpanY, ConfigXmlContext.TypeKeyInt );
			//this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeyOriginX, ConfigXmlContext.TypeKeyInt );
			//this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeyOriginY, ConfigXmlContext.TypeKeyInt );
			//this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeyVisiblePosition, ConfigXmlContext.TypeKeyInt );
			this.PropertyMap.Add( RuntimeFormatServerContext.XmlKeyTextHAlign, ConfigXmlContext.TypeKeyInt );
		}

		/// <summary>
		/// 当前对象名称
		/// </summary>
		public override string Name 
		{
			get { return this.GetType().Name; }
		}
	}
}
