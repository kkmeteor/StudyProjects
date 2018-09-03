/*
 * 作者:卢达其
 * 时间:2008.4.7
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 管理运行时格式数据参数相关操作抽象定义
	/// </summary>
	[Serializable]
	internal abstract class RuntimeFormatArgAbstract : ObjectWithArgs, IRuntimeFormatArg
	{
		protected string _id = null;
		protected object _tag = null;

		/// <summary>
		/// 数据参数对象标识
		/// </summary>
		public string Id 
		{
			get { return this._id; } 
		}
		
		/// <summary>
		/// 数据参数对象值
		/// </summary>
		public object Tag 
		{
			get 
			{ 
				this.RefreshTag();
				return this._tag; 
			} 
		}

		protected override string[] ArgsRequired
		{
			get 
			{ 
				return new string[]{ };
			}
		}

		/// <summary>
		/// 每次获取之前先刷新数据,以保证实时同步
		/// </summary>
		protected abstract void RefreshTag();
	}
}