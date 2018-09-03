using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupRow 的摘要说明。
	/// </summary>
	public class GroupRow:Row
	{        
        public GroupRow(SectionLine sl, SemiRow semirow,Graphics gg)
            : base(sl, semirow,null,gg)
        {
        }       

		protected override void DrawHeader(Graphics g,int prex,int width)
		{
            //if (!_bfirst || _rowstate == GroupHeaderRowStates.Normal)
            //    return;
            //Rectangle rect = this.HeaderRect;
            //if (rect.X < width &&
            //    rect.X + rect.Width > 0)
            //{
            //    if (ConvertFromInternalToControl(_left) > prex)//+16
            //        DrawLine(g,prex, rect);
            //}
		}        
    }
}
