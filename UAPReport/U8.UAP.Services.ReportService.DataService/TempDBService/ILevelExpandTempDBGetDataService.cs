using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ��ȡ����չ����Ϣ����Ľӿ�
	/// </summary>
	public interface ILevelExpandTempDBGetDataService
	{
		/// <summary>
		/// �˷�����DataRow���������뼶��չ����Ϣ
		/// </summary>
		/// <param name="columnInfo">
		/// �봫�ݸ�ReportDataFacade.LevelExpandInfo2TempDB()
		/// ��columnInfo����ͬһ����
		/// </param>
		/// <param name="reader">
		/// Դ��������Ϣ,�ӿڵ�ʵ���߽�ʹ��reader��ǰ����Ϣ
		/// ����ó�����չ������Ϣ
		/// </param>
		/// <param name="dr">ʹ��DataRow���س�����չ������Ϣ</param>
		void GetData( 
			Hashtable columnInfo, 
			SqlDataReader reader,
			DataRow dr );
	}
}
