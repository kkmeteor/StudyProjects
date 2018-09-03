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
	/// ��̬����Դ�����ݲ����������
	/// </summary>
	[Serializable]
	internal class RuntimeFormatArgDynamicDataSourceCol : RuntimeFormatArgAbstract
	{
		private RuntimeFormat _runtimeFormat = null;

		/// <summary>
		/// �������
		/// </summary>
		/// <param name="rf">����ʱ��ʽ����</param>
		public RuntimeFormatArgDynamicDataSourceCol( RuntimeFormat rf )
		{ 
			this._id = RuntimeFormatServerContext.ArgKeyDymanicAddedCols;
			this._runtimeFormat = rf;
		}

		protected override void RefreshTag()
		{
			IConfigXmlItem colsInXml = this._runtimeFormat.GetSubItem( 
				RuntimeFormatServerContext.ArgKeyDymanicAddedCols );
			ArrayList addedCols = new ArrayList();
			this._tag = addedCols;
			if (colsInXml != null)
			{
				for (int i = 0; i < colsInXml.SubItemCount; i++)
				{
					IConfigXmlItem item = colsInXml.GetSubItem(i);
					string colKey = item.GetProperty(ConfigXmlContext.XmlKeyId).ToString();
					addedCols.Add(colKey);
				}
			}
		}
	}
}