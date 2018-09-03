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
	/// 一个表格分组的格式信息
	/// </summary>
	[Serializable]
	public class RuntimeFormatGroupFormat : ConfigXmlItem
	{
		/// <summary>
		/// 当前对象名称
		/// </summary>
		public override string Name 
		{
			get { return this.GetType().Name; }
		}
	}
}