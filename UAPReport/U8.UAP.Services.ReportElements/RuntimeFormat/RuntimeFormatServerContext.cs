/*
 * 作者:卢达其
 * 时间:2008.3.12
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 表格报表运行格式环境信息
	/// </summary>
	public class RuntimeFormatServerContext
	{
		public const string XmlKeyPosition = "Position";
		//public const string XmlKeyRuntimeFormatGroupLevel = "GroupLevel";
		//public const string XmlKeyRuntimeFormatGroupStyle = "GroupStyle";
		//public const string XmlKeyRuntimeFormatGroupFormat = "GroupFormat";
		//public const string XmlKeyRuntimeFormat = "RuntimeFormat";
		public const string XmlKeyIsSavedByEndUser = "IsSavedByEndUser";
		public const string XmlKeyTextHAlign = "TextHAlign";
        public const string XmlKeyIsVisible = "IsVisible";
        public const string XmlKeyIsMerge = "IsMerge";
		public const string XmlKeyWidth = "Width";
		//public const string XmlKeySpanX = "SpanX";
		//public const string XmlKeySpanY = "SpanY";
		//public const string XmlKeyOriginX = "OriginX";
		//public const string XmlKeyOriginY = "OriginY";
		//public const string XmlKeyVisiblePosition = "VisiblePosition";
		public const string XmlKeyOperatorType = "OperatorType";
		public const string XmlKeyIsFoldRow = "IsFoldRow";//自动折行

		public const string ArgKeySummaryCols = "SummaryCols";
		public const string ArgKeyDymanicAddedCols = "DymanicAddedCols";
		public const string ArgKeyAddingCols = "ArgKeyAddingCols";
		public const string ArgKeyCurrentGroupId = "ArgKeyCurrentGroupId";
		public const string ArgKeyCurrentStyle = "ArgKeyCurrentStyle";
        public const string ArgKeyMergeAllCells = "IsMergeAllCells";//整表合并
        public const string ArgKeyAutoColumnWidthFit = "AutoColumnWidthFit";//自适应列宽
	}
}