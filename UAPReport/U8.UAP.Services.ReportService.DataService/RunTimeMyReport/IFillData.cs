/*
 * ����:¬����
 * ʱ��:2009.3.9
 *
 */

using System;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �������
	/// </summary>
	public interface IFillData
	{
		void FillData(DataRow dr);
	}
}