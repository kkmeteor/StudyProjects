/*
 * 作者:卢达其
 * 时间:2008.5.14
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 列汇总类型参数管理对象
	/// </summary>
	[Serializable]
	internal class RuntimeFormatArgColSummaryType : RuntimeFormatArgAbstract
	{
		private RuntimeFormat _runtimeFormat = null;

		/// <summary>
		/// 构造参数
		/// </summary>
		/// <param name="rf">运行时格式对象</param>
		public RuntimeFormatArgColSummaryType( RuntimeFormat rf )
		{ 
			this._id = RuntimeFormatServerContext.ArgKeySummaryCols;
			this._runtimeFormat = rf;
		}

		protected override void RefreshTag()
		{
			IConfigXmlItem colsInXml = this._runtimeFormat.GetSubItem( 
				RuntimeFormatServerContext.ArgKeySummaryCols );
			Hashtable summaryCols = new Hashtable();
			this._tag = summaryCols;
			if (colsInXml != null)
			{
				for (int i = 0; i < colsInXml.SubItemCount; i++)
				{
					IConfigXmlItem item = colsInXml.GetSubItem(i);
					string colKey = item.GetProperty(ConfigXmlContext.XmlKeyId).ToString();
					summaryCols[colKey] = item.GetProperty(RuntimeFormatServerContext.XmlKeyOperatorType);
				}
			}
		}
	}
}