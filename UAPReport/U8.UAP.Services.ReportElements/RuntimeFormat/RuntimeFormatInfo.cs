/*
 * ����:¬����
 * ʱ��:2008.5.12
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ����ʱ��ʽ��Ϣͨ�ýڵ����,��ҪӦ���ڴ洢�򵥵���Ϣ,
	/// �綯̬��ӵ��У��еĻ������͵�
	/// </summary>
	[Serializable]
	public class RuntimeFormatInfo : ConfigXmlItem
	{
		/// <summary>
		/// ��ǰ��������
		/// </summary>
		public override string Name 
		{
			get { return this.GetType().Name; }
		}
	}
}