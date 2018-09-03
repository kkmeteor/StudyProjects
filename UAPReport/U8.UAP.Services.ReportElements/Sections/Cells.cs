using System;
using System.Runtime.Serialization;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// Cells 的摘要说明。
    /// </summary>
    [Serializable]
    public class Cells : CollectionBase, IDisposable, ICloneable, ISerializable
    {
        private int _x = Int32.MaxValue;
        private int _right = Int32.MinValue;
        private int _y = Int32.MaxValue;
        private int _bottom = Int32.MinValue;
        private int _expandbottom = Int32.MinValue;

        private ReportStates _understate = ReportStates.Designtime;
        public Cells()
            : base()
        {
        }
        protected Cells(SerializationInfo info, StreamingContext context)
            : base()
        {
            _understate = (ReportStates)info.GetValue("UnderState", typeof(ReportStates));
            int count = info.GetInt32("Count");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                    Add((Cell)info.GetValue("Cell" + i.ToString(), typeof(Cell)));
            }
        }

        internal event EventHandler BeforeAdd;
        public Cell this[int index]
        {
            get
            {
                if (index >= Count)
                    return null;
                return List[index] as Cell;
            }
        }

        public Cell this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }

        public Cell GetByGroupKeyWhenCheckGroup(string mapname)
        {
            Cell c = null;
            SimpleArrayList mapnames = new SimpleArrayList();
            foreach (Cell cell in this)
            {
                if (cell.Name.ToLower() == mapname.ToLower() || ((cell is IMapName) && (cell as IMapName).MapName.ToLower() == mapname.ToLower()))
                {
                    if (cell is GridProportionDecimal)//占比列不能作为分组 add by yanghx 2012-6-30
                    {
                        continue;
                    }
                    c = cell;
                    if (c is IMapName && (c as IMapName).MapName.ToLower() != mapname.ToLower())
                        mapnames.Add((c as IMapName).MapName);
                    if (c is IGridCollect && (c as IGridCollect).bSummary)
                        (c as IGridCollect).bSummary = false;
                    break;
                }
                else if (cell is IGridCollect && (cell as IGridCollect).bSummary && (cell as IGridCollect).Operator == OperatorType.ExpressionSUM && ExpressionSumContains(cell, mapname))
                    (cell as IGridCollect).bSummary = false;
            }
            foreach (string mp in mapnames)
            {
                foreach (Cell cell in this)
                {
                    if (cell.Name.ToLower() == mp.ToLower() || ((cell is IMapName) && (cell as IMapName).MapName.ToLower() == mp.ToLower()))
                    {
                        if (cell is IGridCollect && (cell as IGridCollect).bSummary)
                            (cell as IGridCollect).bSummary = false;
                    }
                    else if (cell is IGridCollect && (cell as IGridCollect).bSummary && (cell as IGridCollect).Operator == OperatorType.ExpressionSUM && ExpressionSumContains(cell, mp))
                        (cell as IGridCollect).bSummary = false;
                }
            }
            return c;
        }

        private bool ExpressionSumContains(Cell cell, string mapname)
        {
            if (cell is ICalculateColumn)
            {
                string expression = (cell as ICalculateColumn).Expression;
                string[] expressions = ExpressionService.SplitExpression(expression);
                foreach (string e in expressions)
                {
                    if (e.Trim().ToLower() == mapname.ToLower())
                        return true;
                }
            }
            return false;
        }

        public Cell GetByGroupKey(string mapname)
        {
            Cell cell = GetByName(mapname);
            if (cell != null)
                return cell;
            cell = GetBySource(mapname);
            return cell;
        }

        public void SetRawSuper()
        {
            foreach (Cell cell in this)
                cell.Super = null;

            foreach (Cell cell in this)
            {
                foreach (Cell super in this)
                {
                    if ((super is SuperLabel) && cell.bUnder(super))
                        cell.Super = super as SuperLabel;
                }
            }
        }

        public void SetSuper()
        {
            foreach (Cell cell in this)
            {
                if (cell is SuperLabel)
                {
                    SetSuper(cell as SuperLabel);
                }
            }
        }

        private void SetSuper(SuperLabel sl)
        {
            foreach (Label l in sl.Labels)
            {
                l.Super = sl;
                if (l is SuperLabel)
                    SetSuper(l as SuperLabel);
            }
        }

        public Cell GetBySource(string name)
        {
            for (int i = 0; i < Count; i++)
            {
                Cell c = this[i];
                if (c is IMapName && (c as IMapName).MapName.ToLower() == name.ToLower())
                    return c;
            }
            return null;
        }

        public Cell GetByCaption(string caption)
        {
            for (int i = 0; i < Count; i++)
            {
                Cell c = this[i];
                if (c.Caption.ToLower() == caption.ToLower())
                    return c;
            }
            return null;
        }

        private Cell GetByName(string name)
        {
            for (int i = 0; i < Count; i++)
            {
                Cell c = this[i];
                if (c.Name.ToLower() == name.ToLower())
                    return c;
                if (c is SuperLabel)
                {
                    c = (c as SuperLabel).GetByName(name);
                    if (c != null)
                        return c;
                }
            }
            return null;
        }

        public void AutoLayOut()
        {
            int left = DefaultConfigs.ReportLeft;
            if (Count > 0 && this[0].VisiblePosition > -1)
            {
                for (int i = 0; i < Count; i++)
                {
                    Cell cell = this[i];
                    if (cell.Visible)
                    {
                        cell.X = left;
                        left += cell.Width;
                        if (cell is SuperLabel)
                        {
                            (cell as SuperLabel).Labels.AutoLayout(cell.X);
                        }
                    }
                }
            }
        }

        public void AddByVisiblePosition(Cell value)
        {
            value.UnderState = _understate;
            for (int i = 0; i < Count; i++)
            {
                if (value.VisiblePosition < this[i].VisiblePosition)
                {
                    Insert(i, value);
                    return;
                }
            }
            bool bSubLabelVisible = true;

            if (!string.IsNullOrEmpty(Convert.ToString(value as UFIDA.U8.UAP.Services.ReportElements.SuperLabel)))
            {
                Labels lbs = (value as UFIDA.U8.UAP.Services.ReportElements.SuperLabel).Labels;
                foreach (Label lb in lbs)
                {
                    bSubLabelVisible = lb.Visible;
                    if (bSubLabelVisible == true)
                        break;
                }
            }

            if (bSubLabelVisible == false)
                value.Visible = false;
            InnerList.Add(value);

        }

        public void AddByY(Cell value)
        {
            value.UnderState = _understate;
            for (int i = 0; i < Count; i++)
            {
                if (value.Y < this[i].Y)
                {
                    Insert(i, value);
                    return;
                }
            }
            InnerList.Add(value);
        }

        public int Add(Cell value)
        {
            if (value.Width < 2)
                return -1;
            value.UnderState = _understate;
            if (value.X < _x)
                _x = value.X;
            if (value.X + value.Width > _right)
                _right = value.X + value.Width;
            if (value.Y < _y)
                _y = value.Y;

            //if (_understate == ReportStates.Designtime)
            //{
            //    if (value.Y + value.Height  > _bottom)
            //        _bottom = value.Y + value.Height;
            //}
            //else
            //{
            if (value.Y + value.MetaHeight > _bottom)
                _bottom = value.Y + value.MetaHeight;
            if (value.Y + value.ExpandHeight > _expandbottom)
                _expandbottom = value.Y + value.ExpandHeight;
            //}

            //---------------------
            //int height;
            //if (_understate != ReportStates.Designtime)
            //    height=value.MetaHeight;
            //else
            //    height=value.Height;
            //if(value.Y+height  >_bottom)
            //    _bottom=value.Y+height ;
            //----------------------
            AddArgs aa = new AddArgs();
            if (BeforeAdd != null)
                BeforeAdd(value, aa);
            if (aa.bAlreadyAdd)
                return -1;
            //if (_understate != ReportStates.Designtime)
            //{
            for (int i = 0; i < Count; i++)
            {
                Cell c = this[i];
                if (value.X < c.X)
                {
                    Insert(i, value);
                    return i;
                }
                else if (value.X == c.X)
                {
                    if (value.Y <= c.Y)
                    {
                        Insert(i, value);
                        return i;
                    }
                }
            }
            //}
            //else
            //{
            //    for(int i=0;i<Count;i++)
            //    {
            //        if(value.Z_Order < this[i].Z_Order )
            //        {
            //            Insert(i,value);
            //            return i;
            //        }
            //    }
            //}
            return (List.Add(value));
        }

        public void AddDirectly(Cell value)
        {
            value.UnderState = _understate;
            List.Add(value);
        }

        public void AddALabel(Label label)
        {
            label.UnderState = _understate;
            bool badded = false;
            ArrayList removes = new ArrayList();
            foreach (Cell c in this.InnerList)
            {
                if (label is SuperLabel && c.bUnder(label) && c is Label)
                {
                    removes.Add(c);
                }
                if (c is SuperLabel && label.bUnder(c))
                {
                    if (label.LabelType != LabelType.DetailLabel)
                        (c as SuperLabel).LabelType = label.LabelType;
                    (c as SuperLabel).Labels.Add(label);
                    badded = true;
                    break;
                }
            }
            if (removes.Count > 0)
            {
                for (int i = 0; i < removes.Count; i++)
                {
                    Label ctmp = removes[i] as Label;
                    this.Remove(ctmp);

                    if (ctmp.LabelType != LabelType.DetailLabel)
                        (label as SuperLabel).LabelType = ctmp.LabelType;
                    (label as SuperLabel).Labels.Add(ctmp);
                }
                this.Add(label);
            }
            else if (!badded)
            {
                this.Add(label);
            }
        }

        public void CalcHeight()
        {
            _x = Int32.MaxValue;
            _right = Int32.MinValue;
            _y = Int32.MaxValue;
            _bottom = Int32.MinValue;
            foreach (Cell value in List)
            {
                if (!value.Visible)
                    continue;
                if (value.X < _x)
                    _x = value.X;
                if (value.X + value.Width > _right)
                    _right = value.X + value.Width;
                if (value.Y < _y)
                    _y = value.Y;

                //if (_understate == ReportStates.Designtime)
                //{
                //    if (value.Y + value.Height > _bottom)
                //        _bottom = value.Y + value.Height;
                //}
                //else
                //{
                if (value.Y + value.MetaHeight > _bottom)
                    _bottom = value.Y + value.MetaHeight;
                if (value.Y + value.ExpandHeight > _expandbottom)
                    _expandbottom = value.Y + value.ExpandHeight;
                //}

                //int height;
                //if (_understate != ReportStates.Designtime)
                //    height=value.MetaHeight;
                //else
                //    height=value.Height;
                //if(value.Y+height  >_bottom)
                //    _bottom=value.Y+height ;
            }
        }

        public void CalcRuntimeHeight(Cell value)
        {
            if (value.RelativeY < _y)
                _y = value.RelativeY;
            if (value.RelativeY + value.MetaHeight > _bottom)
                _bottom = value.RelativeY + value.MetaHeight;
            if (value.RelativeY + value.ExpandHeight > _expandbottom)
                _expandbottom = value.RelativeY + value.ExpandHeight;
        }

        public int IndexOf(Cell value)
        {
            return (List.IndexOf(value));
        }

        public void Insert(int index, Cell value)
        {
            List.Insert(index, value);
        }

        public void Remove(Cell value)
        {
            if (Contains(value))
                List.Remove(value);
        }

        public void RemoveCell(Cell value)
        {
            int i = 0;
            while (i < Count)
            {
                if (this[i].Name.ToLower() == value.Name.ToLower())
                {
                    List.Remove(this[i]);
                    break;
                }
                i++;
            }
        }

        public bool Contains(Cell value)
        {
            return (List.Contains(value));
        }

        public bool Contains(string name)
        {
            if (GetByName(name) == null)
                return false;
            else
                return true;
        }

        public void ReSetXRight()
        {
            if (Count > 0)
            {
                _right = this[Count - 1].X + this[Count - 1].Width;
                _x = this[0].X;
            }
        }
        public int VisibleRight
        {
            get
            {
                if (Count > 0)
                {
                    int i = Count - 1;
                    while (i >= 0 && !this[i].Visible)
                    {
                        i--;
                    }
                    if (i >= 0)
                        return this[i].X + this[i].Width;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }


        public int Right
        {
            get
            {
                if (Count > 0)
                {
                    //if (_understate != ReportStates.Designtime)
                    //{
                    //    return this[Count - 1].X + this[Count - 1].Width ;
                    //}
                    //else
                    //    return _right;
                    return Math.Max(_right, this[Count - 1].X + this[Count - 1].Width);
                }
                else
                    return 0;
            }
        }

        public int X
        {
            get
            {
                if (Count > 0)
                {
                    if (_understate != ReportStates.Designtime)
                    {
                        return this[0].X;
                    }
                    else
                        return _x;
                }
                else
                    return 0;
            }
        }

        public int Y
        {
            get
            {
                if (Count == 0)
                    return 0;
                return _y;
            }
        }

        public int Bottom
        {
            get
            {
                if (Count == 0)
                    return 0;
                return _bottom;
            }
        }

        public int Height
        {
            get
            {
                if (Count == 0)
                    return 0;
                return _bottom - _y;
            }
        }

        public int MetaHeight
        {
            get
            {
                if (_bottom == Int32.MinValue)
                    return 0;
                return _bottom - _y;
            }
        }

        public int ExpandHeight
        {
            get
            {
                if (_expandbottom == Int32.MinValue)
                    return 0;
                return _expandbottom - _y;
            }
        }

        public ReportStates UnderState
        {
            get
            {
                return _understate;
            }
            set
            {
                _understate = value;
                foreach (Cell cell in List)
                    cell.UnderState = value;
            }
        }
        #region IDisposable 成员

        public void Dispose()
        {
            foreach (Cell value in List)
            {
                if (value != null)
                {
                    ((IDisposable)value).Dispose();
                }
            }
            this.Clear();
        }

        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            Cells cells = new Cells();
            foreach (Cell value in List)
            {
                cells.Add(value.Clone() as Cell);
            }
            return cells;
        }

        #endregion

        #region ISerializable 成员

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", 1);
            info.AddValue("UnderState", _understate);
            info.AddValue("Count", Count);
            for (int i = 0; i < Count; i++)
                info.AddValue("Cell" + i.ToString(), List[i]);
        }

        #endregion
    }
}
