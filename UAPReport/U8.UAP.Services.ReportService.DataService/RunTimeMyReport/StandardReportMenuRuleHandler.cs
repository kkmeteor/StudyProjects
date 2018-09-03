/*
 * ����:¬����
 * ʱ��:2007.6.22
 */

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ����ʵ��������ҵ��û���ã���ñ�׼�����ڡ��ҵı����в��ɼ�
	/// </summary>
	class StandardReportMenuRuleHandler
	{
		private Dictionary<string, List<DataRow>> _infoUaMenurule = null;
		private Dictionary<string, List<DataRow>> _infoInUfDataAccInformation = null;

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="infoUaMenurule">
		/// ��UfSystem���ݿ��з��صı�UA_Menurule��һЩ��Ϣ,
		/// ���е��ֶ�Ϊ:cValue,cName,cMenu_Id
		/// </param>
		/// <param name="infoInUfDataAccInformation">
		/// ��UfData���ݿ��з��صı�AccInformation��һЩ��Ϣ,
		/// ���е��ֶ�Ϊ:cValue,cName,cSysID
		/// </param>
		public void Init( 
			DataTable infoUaMenurule,
			DataTable infoInUfDataAccInformation)
		{ 
			//���ű����Ϣ������������һ���ؼ�ֵ��Ӧ������¼���������Ҫ
			//��ÿһ���������ж�
			this._infoUaMenurule = this.GetInfo(infoUaMenurule, "cMenu_Id");
			this._infoInUfDataAccInformation = this.GetInfo(infoInUfDataAccInformation, "cName");
		}

		private Dictionary<string, List<DataRow>> GetInfo(DataTable dt, string field)
		{
		    Dictionary<string, List<DataRow>> info = new Dictionary<string, List<DataRow>>();
		    foreach (DataRow dr in dt.Rows)
		    {
		        string key = this.GetWrappedKey(SqlHelper.GetStringFrom(dr[field]));
		        if(!info.ContainsKey(key))
		            info.Add(key, new List<DataRow>()); 
		        info[key].Add(dr);
		    }
		    return info;
		}

		private string GetWrappedKey(string key)
		{ 
			return key.ToLower();
		}

		/// <summary>
		/// �жϵ�ǰDataRow��Ӧ�ı����Ƿ�ɼ�
		/// </summary>
		/// <param name="dr">����������Ϣ��DataRow����</param>
		/// <returns>����ɼ�,����true;���򷵻�false</returns>
		/// <remarks>
		/// �����Ż������˵�����ֻ���Ƹ��ڵ�Ŀɼ��ԣ��������ӽڵ㽫���䱣��һ��
		/// �����ҵı���û�и��ڵ����Ϣ�����쵽���ڵ��MenuIdΪ�ӽڵ�MenuId��
		/// �Ӽ������Դ˴����õݼ��ӽڵ�MenuId�ķ������丸�ڵ�Ŀɼ���;
		/// �ɼ��Ե��жϹ���Ϊ:
		/// ����menuId��UA_Menurule���л�ȡcValue1,cName;Ȼ�����
		/// cName��AccInformation��ȡcValue2,���cValue1=cValue2��
		/// ������ǰ�����ɼ�.��Ҫע�����:
		/// ����һЩ�����cName�Ƚ�����,�磺'ST,bBatch',��ʱҪ�ֽ��
		/// cSysID="ST"��cName="bBatch"����ȡcValue2
		/// </remarks>
		public bool IsReportVisible(string menuId)
		{
			try
			{
				string key = this.GetWrappedKey(menuId);
				if (this._infoUaMenurule.ContainsKey(key))
				{
					List<DataRow> drs = this._infoUaMenurule[key];
					foreach (DataRow dr in drs)
                        if (!this.IsVisible(dr, menuId))
                        {
                            System.Diagnostics.Trace.WriteLine("dr[cName]" + dr["cName"] + "dr[cValue]" + dr["cValue"] + "�������ҵ���������ʾ");
                            return false;
                        }
				}
				
				if(menuId.Length > 2)
				{
					string nextMenuId = menuId.Substring(0, menuId.Length -1);
					return this.IsReportVisible(nextMenuId);
				}
				return true;
			}
			catch( Exception e ) 
			{ 
				StringBuilder sb = new StringBuilder();
				sb.Append( "\"�ҵı���\"�жϱ�׼�����Ƿ���ʾʱ����.\r\n����ԭ��:" );
				sb.AppendLine( e.Message );
				sb.Append( "��ǰMenuId=" );
				sb.Append( menuId );
				Trace.WriteLine( sb.ToString());
				return true; 
			}
		}

		private bool IsVisible(DataRow menuRuleRow, string menuId)
		{
			string cName = SqlHelper.GetStringFrom(menuRuleRow["cName"]);
			string cValue1 = SqlHelper.GetStringFrom(menuRuleRow["cValue"]);
			string cSysID = this.GetcSysID(ref cName);

			string key = this.GetWrappedKey(cName);
			if (this._infoInUfDataAccInformation.ContainsKey(key))
			{
				List<DataRow> drs = this._infoInUfDataAccInformation[key];
				foreach (DataRow dr in drs)
				{ 
					string cValue2 = this.GetValue2(cName, cSysID, dr);
					if(cValue1.ToLower() == cValue2.ToLower())
						return false;
				}
			}
			return true;
		}

		private string GetcSysID( ref string cName )
		{
			if( cName.IndexOf( "," ) != -1 )
			{
				string[] ss = cName.Split( ',' );
				cName = ss[1];
				return ss[0];
			}
			return string.Empty;
		}

		/// <summary>
		/// cSysID��Ϊ��,��cSysIDҲҪ���
		/// </summary>
		private string GetValue2(
			string cName,
			string cSysID,
			DataRow dr)
		{ 
			string sid = SqlHelper.GetStringFrom( dr["cSysID"] );
			if(string.IsNullOrEmpty(cSysID) 
				|| sid.ToLower() == cSysID.ToLower())
				return SqlHelper.GetStringFrom(dr["cValue"]);
			return "cValue2";
		}
	}
}
