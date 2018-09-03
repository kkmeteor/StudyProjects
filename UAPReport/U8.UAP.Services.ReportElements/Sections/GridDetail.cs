using System;
using System.Drawing;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Detail 的摘要说明。
	/// </summary>
	[Serializable]
    public class GridDetail : Section, IAutoDesign, ISerializable, ICloneable
	{
		public GridDetail():base()
		{
		}

		public GridDetail(int height):base(height)
		{
		}

		public GridDetail(GridDetail GridDetail):base(GridDetail)
		{
		}

		protected GridDetail( SerializationInfo info, StreamingContext context ):base(info,context)
		{
		}

		protected override void SetOrderID()
		{
			_orderid=5;
		}

		protected override void SetSectionType()
		{
			_sectiontype=SectionType.GridDetail;
		}

		public override void SetType()
		{
			_type="GridDetail";
			_x=0;
        }

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx17"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            switch (ds.Type )
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return new GridDecimal(ds);
                case DataType.DateTime:
                    return new GridDateTime(ds);
                //case DataType.Image:
                //    return new GridImage(ds);
                default:
                    return new GridLabel(ds);
            }
        }

        public override States GetDefaultState(DataType type)
        {
            switch (type)
            {
                case DataType.Currency:
                case DataType.Decimal:
                case DataType.Int:
                    return States.GridDecimal;
                case DataType.DateTime:
                    return States.GridDateTime;
                //case DataType.Image:
                //    return States.GridImage;
                default:
                    return States.GridLabel;
            }
        }

		public override bool CanBeParent(string type)
		{
			type=type.ToLower();
			if(type=="superlabel" ||
				type=="gridlabel" ||
				type=="gridboolean" ||
				type=="gridimage" ||
				type=="griddecimalalgorithmcolumn" ||
				type=="gridalgorithmcolumn" ||
				type=="griddecimal" ||
				type=="gridcalculatecolumn" ||
				type=="gridcolumnexpression" ||
                type == "griddatetime" ||
                type == "gridproportiondecimal" ||
				type=="gridexchangerate" )
				return true;
			else
				return false;
        } 

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new GridDetail(this);
		}

		#endregion

        #region IAutoDesign 成员

        public void AutoDesign()
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesign();
        }

        public void AutoDesign(int x)
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesign(x);
        }

        public void AutoDesign(Cells cs)
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesign(cs);
        }

        public void AutoDesign(int y, int height)
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesign(y,height );
        }

        public void AutoDesignSuperLabel()
        {
            AutoDesignService ads = new AutoDesignService(_cells);
            ads.AutoDesignSuperLabel();
        }

        #endregion
    }

    public class AutoDesignService
    {
        private Cells _cells;
        public AutoDesignService(Cells cells)
        {
            _cells = cells;
        }

        public void AutoDesign()
        {
            AutoDesign(Int32.MaxValue);
        }

        public void AutoDesign(int x)
        {
            Cell cfirst = null;
            if (_cells.Count > 0)
            {
                cfirst = FirstNotSuper(_cells);
                if (cfirst == null)
                    return;
                int height = cfirst.Height;
                int left = (x == Int32.MaxValue ? cfirst.X : x);
                int top = cfirst.RelativeY;
                foreach (Cell cell in _cells)
                {
                    if (cell is SuperLabel)
                        continue;
                    cell.SetLocation(new Point(left, top));
                    cell.SetSize(new Size(cell.Width, height));
                    left += cell.Width;
                }
                _cells.ReSetXRight();
            }
        }

        private Cell FirstNotSuper(Cells cells)
        {
            Cell cfirst = null;
            int i = 0;
            while (i < cells.Count)
            {
                cfirst = cells[i];
                if (!(cfirst is SuperLabel))
                    break;
                cfirst = null;
                i++;
            }
            return cfirst;
        }

        public void AutoDesign(Cells cs)
        {
            if (_cells.Count > 0)
            {
                int index = 0;
                int count = _cells.Count;
                Cell cell = FirstNotSuper(_cells);
                if (cell == null)
                    return;
                Cell c = FirstNotSuper(cs);
                if (c == null)
                    return;
                int x = c.X;
                int beginx = cell.X;
                int left = Int32.MaxValue;
                int top = c.RelativeY;
                while (index < count)
                {
                    if (index > _cells.Count - 1)
                        break;
                    cell = _cells[index];
                    if (cell is SuperLabel)
                    {
                        index++;
                        continue;
                    }
                    if (cs.Contains(cell))
                    {
                        _cells.Remove(cell);
                        continue;
                    }
                    if (x <= cell.X)
                    {
                        if (left == Int32.MaxValue)
                            left = x;
                        foreach (Cell ct in cs)
                        {
                            if (ct is SuperLabel)
                                continue;
                            _cells.Remove(ct);
                            _cells.Insert(index, ct);
                            ct.SetLocation(new Point(left, top));
                            left += ct.Width;
                            index++;
                        }
                        cell.SetLocation(new Point(left, top));
                        left += cell.Width;
                        index++;
                        x = Int32.MaxValue;
                    }
                    else
                    {
                        if (left == Int32.MaxValue)
                            left = cell.X;
                        cell.SetLocation(new Point(left, top));
                        left += cell.Width;
                        index++;
                    }
                }
                if (x != Int32.MaxValue)
                {
                    if (left == Int32.MaxValue)
                        left = beginx;
                    foreach (Cell ct in cs)
                    {
                        if (ct is SuperLabel)
                            continue;
                        _cells.Remove(ct);
                        _cells.AddDirectly(ct);
                        ct.SetLocation(new Point(left, top));
                        left += ct.Width;
                    }
                }
                _cells.ReSetXRight();
            }
        }

        public void AutoDesign(int y, int height)
        {
            Cell cfirst = null;
            if (_cells.Count > 0)
            {
                cfirst = FirstNotSuper(_cells);
                int left = cfirst.X;
                foreach (Cell cell in _cells)
                {
                    height = cell.Height;
                    if (height != 24)
                        height = 24;
                    cell.SetSize(new Size(cell.Width, height));
                    if (cell is SuperLabel)
                        continue;
                    cell.SetLocation(new Point(left, y));
                    //cell.SetSize(new Size(cell.Width, height));
                    left += cell.Width;
                }
                _cells.ReSetXRight();
            }
        }

        public void AutoDesignSuperLabel()
        {
            foreach (Cell cell in _cells)
            {
                if (cell is SuperLabel)
                    SetSuper(cell as SuperLabel);
            }
        }

        private int SetSuper(SuperLabel sl)
        {
            int width=0;
            foreach (Cell cell in sl.Cells)
            {
                if (cell is SuperLabel)
                    width+=SetSuper(cell as SuperLabel);
                else
                    width+=cell.Width;
            }
            if (sl.Cells.Count > 0)
            {
                sl.X = sl.Cells[0].X;
                sl.Width = width;
            }
            return sl.Width;
        }

        public void SetSuperCells()
        {
        }
    }
}
