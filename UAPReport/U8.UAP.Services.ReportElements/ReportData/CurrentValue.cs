using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CurrentValue 的摘要说明。
	/// </summary>
	public class CurrentValue
	{
		public static string String(int index)
		{
			return "String"+index.ToString();
		}

		public static DateTime DateTime(int index)
		{
			return System.DateTime.Now.AddDays(index);
		}

		public static double Decimal(int index)
		{
			return index;
		}

		public static int Int(int index)
		{
			return index;
		}
	}
}
