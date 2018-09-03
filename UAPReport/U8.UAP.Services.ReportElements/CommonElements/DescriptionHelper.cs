using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using UFIDA.U8.UAP.Services.ReportResource ;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// LocallizeDescription 的摘要说明。
	/// </summary>
	[Serializable]
	[AttributeUsageAttribute(AttributeTargets.All)]
	public class LocalizeDescription:DescriptionAttribute
	{
		public LocalizeDescription(string description) : base(description)
		{
		}

		private bool replaced = false;
		public override string Description
		{
			get
			{
				if (!replaced)
				{
					replaced = true;
					base.DescriptionValue = U8ResService.GetResString(base.Description,System.Threading.Thread.CurrentThread.CurrentUICulture.Name);//ResourceHelper.GetString(base.Description);
				}
				return base.Description;
			}
		}
	}
}
