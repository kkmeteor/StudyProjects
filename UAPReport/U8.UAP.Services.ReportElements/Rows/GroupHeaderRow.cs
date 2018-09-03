using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupHeaderRow 的摘要说明。
	/// </summary>
	public class GroupHeaderRow:GroupRow
	{
        public GroupHeaderRow(SectionLine sl, SemiRow semirow,Graphics gg)
            : base(sl, semirow,gg)
        {
            if ((sl.Section as IGroupHeader).bAloneLine)
                _baloneline = true;
        }

        protected override void DrawPlus(Graphics g, int prex, int width)
		{
            //if (_rowstate == GroupHeaderRowStates.Normal)
            //    return;
            //if (_bfirst)
            //{
            //    Rectangle rect = this.CrossRect;
            //    Rectangle newrect = new Rectangle(prex, rect.Y, rect.X + 16 - prex, rect.Height);
            //    if (newrect.X < width &&
            //        newrect.X + newrect.Width > 0)
            //    {

            //        using (Pen pen = new Pen(DefaultConfigs.LineColor))
            //        using (Pen cpen = new Pen(DefaultConfigs.CrossColor))
            //        {
            //            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            //            pen.DashPattern = new float[] { 1, 1 };
            //            if (_level != 1)
            //            {
            //                g.DrawLine(pen, prex, newrect.Y + 4, rect.X, newrect.Y + 4);
            //            }
            //            using (Brush brush = new SolidBrush(Color.White))
            //            {
            //                g.FillRectangle(brush, rect);
            //            }
            //            g.DrawRectangle(cpen, rect);
            //            g.DrawLine(cpen, rect.X, rect.Y + 4, rect.X + 8, rect.Y + 4);
            //            if (_rowstate == GroupHeaderRowStates.Collapsed)
            //                g.DrawLine(cpen, rect.X + 4, rect.Y, rect.X + 4, rect.Y + 8);
            //            g.DrawLine(pen, rect.X + 8, rect.Y + 4, rect.X + 24, rect.Y + 4);
            //        }
            //    }
            //}
		}

        protected override void DrawLine(Graphics g, int prex, Rectangle rect)
		{
            if (_rowstate != GroupHeaderRowStates.Normal)
            {
                base.DrawLine(g,prex, rect);
            }
		}

        //public override void DrawBack(Graphics g)
        //{
        //    //if (_rowstate == GroupHeaderRowStates.Normal )
        //    //    return;
        //    base.DrawBack(g);
        //}
		
		public override Rectangle CrossRect
		{
			get
			{
				Rectangle rect =this.HeaderRect;
				return new Rectangle(rect.X-16,rect.Y+rect.Height/2-4,8,8);
			}
		}

        public bool bInThisRow(RuntimeGroup group)
        {            
            bool bin = bInAGroup(group);
            if (!bin)
                return false;

            GroupHeaderRow parent = this;
            for (int i = group.Level -1 ; i >0 ; i--)
            {
                parent = parent.ParentRow ;
                bin = parent.bInAGroup(group.UpperGroup(i));
                if (!bin)
                    return false;
            }
            return true;
        }

        public bool bInAGroup(RuntimeGroup group)
        {
            if (group.Level != this.Level)
                return false;
            for (int i = 0; i < group.Count; i++)
            {
                RuntimeGroupItem item = group[i];
                if (_cells[item.CellName].Caption != item.Value)
                    return false;
            }
            return true;
        }
    }

    public enum GroupHeaderRowStates
	{
		Expanded,
		Collapsed,
		Normal,
        Classical
	}
}
