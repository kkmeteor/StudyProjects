/*
 * 作者:卢达其
 * 时间:2007.6.22
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
	/// 该类实现如果相关业务没启用，则该标准报表在“我的报表”中不可见
	/// </summary>
	class StandardReportMenuRuleHandler
	{
		private Dictionary<string, List<DataRow>> _infoUaMenurule = null;
		private Dictionary<string, List<DataRow>> _infoInUfDataAccInformation = null;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="infoUaMenurule">
		/// 从UfSystem数据库中返回的表UA_Menurule的一些信息,
		/// 表中的字段为:cValue,cName,cMenu_Id
		/// </param>
		/// <param name="infoInUfDataAccInformation">
		/// 从UfData数据库中返回的表AccInformation的一些信息,
		/// 表中的字段为:cValue,cName,cSysID
		/// </param>
		public void Init( 
			DataTable infoUaMenurule,
			DataTable infoInUfDataAccInformation)
		{ 
			//两张表的信息都可能有能有一个关键值对应多条记录的情况，需要
			//对每一个都进行判断
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
		/// 判断当前DataRow对应的报表是否可见
		/// </summary>
		/// <param name="dr">包含报表信息的DataRow对象</param>
		/// <returns>报表可见,返回true;否则返回false</returns>
		/// <remarks>
		/// 由于门户左树菜单可能只控制父节点的可见性，其所有子节点将与其保持一致
		/// 而“我的报表”没有父节点的信息。考察到父节点的MenuId为子节点MenuId的
		/// 子集，所以此处采用递减子节点MenuId的方法来其父节点的可见性;
		/// 可见性的判断过程为:
		/// 根据menuId在UA_Menurule表中获取cValue1,cName;然后根据
		/// cName在AccInformation获取cValue2,如果cValue1=cValue2则
		/// 表明当前报表不可见.需要注意的是:
		/// 库存的一些报表的cName比较特殊,如：'ST,bBatch',此时要分解出
		/// cSysID="ST"和cName="bBatch"来获取cValue2
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
                            System.Diagnostics.Trace.WriteLine("dr[cName]" + dr["cName"] + "dr[cValue]" + dr["cValue"] + "由于相关业务规则不能显示");
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
				sb.Append( "\"我的报表\"判断标准报表是否显示时出错.\r\n错误原因:" );
				sb.AppendLine( e.Message );
				sb.Append( "当前MenuId=" );
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
		/// cSysID不为空,则cSysID也要相等
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
