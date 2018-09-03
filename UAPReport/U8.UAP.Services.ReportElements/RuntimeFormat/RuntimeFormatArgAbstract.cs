/*
 * ����:¬����
 * ʱ��:2008.4.7
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ��������ʱ��ʽ���ݲ�����ز���������
	/// </summary>
	[Serializable]
	internal abstract class RuntimeFormatArgAbstract : ObjectWithArgs, IRuntimeFormatArg
	{
		protected string _id = null;
		protected object _tag = null;

		/// <summary>
		/// ���ݲ��������ʶ
		/// </summary>
		public string Id 
		{
			get { return this._id; } 
		}
		
		/// <summary>
		/// ���ݲ�������ֵ
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
		/// ÿ�λ�ȡ֮ǰ��ˢ������,�Ա�֤ʵʱͬ��
		/// </summary>
		protected abstract void RefreshTag();
	}
}