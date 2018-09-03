/*
 * 作者:卢达其
 * 时间:2008.5.12
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 运行时格式信息通用节点对象,主要应用于存储简单的信息,
	/// 如动态添加的列，列的汇总类型等
	/// </summary>
	[Serializable]
	public class RuntimeFormatInfo : ConfigXmlItem
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