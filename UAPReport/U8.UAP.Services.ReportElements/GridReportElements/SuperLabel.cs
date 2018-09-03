using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// SuperLabel 的摘要说明。
	/// </summary>
	[Serializable]
    public class SuperLabel : Label, ISerializable, IDisposable, IWithSizable
	{
		#region fields
		private Labels _labels=new Labels();
		private bool _btop=false;
        private Cells _cells;
		#endregion

		#region contructor
		public SuperLabel():base()
		{
			_keeppos=true;
		}

		public SuperLabel(int x,int y):base(x,y)
		{
			_keeppos=true;
		}

		public SuperLabel(SuperLabel superlabel):base(superlabel)
		{
			_labels=superlabel.Labels.Clone() as Labels;
		}

		public SuperLabel(IMultiHeader mh):base(mh as Rect)
		{
			_keeppos=true;
		}

		public SuperLabel(SerializationInfo info,StreamingContext context):base(info,context)
		{
			_btop=info.GetBoolean("bTop");
			_labels=(Labels)info.GetValue("Labels",typeof(Labels));
		}
		#endregion 

		#region override
		public override void SetType()
		{
			_type="SuperLabel";			
		}
        
		public override void SetDefault()
		{
			_backcolor=DefaultConfigs.DefaultTitleBackColor;
            _captionalign = ContentAlignment.MiddleCenter;
            _labeltype = LabelType.DetailLabel;
            _visibleposition = 0;
        }

        [Browsable(false)]
        public override System.Drawing.ContentAlignment CaptionAlign
        {
            get
            {
                return base.CaptionAlign;
            }
            set
            {
                //base.CaptionAlign = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Font ClientFont
        {
            get
            {
                return base.ClientFont;
            }
            set
            {
                base.ClientFont = value;
            }
        }

        [Browsable(false)]
        public override LabelType LabelType
        {
            get
            {
                return base.LabelType;
            }
            set
            {
                base.LabelType = value;
            }
        }

        [Browsable(false)]
        public override  int VisiblePosition
        {
            get
            {
                return _visibleposition;
            }
            set
            {
                _visibleposition = value;
                if (_visibleposition == -1)
                {
                    foreach (Label label in _labels)
                        label.VisiblePosition = -1;
                }
            }
        }
		#endregion

		#region property
        public override void SetRuntimeHeight(Graphics g,string s)
        {
            base.SetRuntimeHeight(g,s);
            foreach (Label label in _labels)
                label.SetRuntimeHeight(g,label.Caption);
        }

        public void SetRuntimeHeight(int height)
        {
            if (height > this.RuntimeHeight)
                _runtimeheight = height;
        }

		[Browsable(false)]
		public override int MetaHeight
		{
			get
			{
				return  _h+_labels.MetaHeight;
			}
		}

		[Browsable(false)]
		public override int ExpandHeight
		{
			get
			{
				return  this.RuntimeHeight +_labels.ExpandHeight;
			}
        }

        [Browsable(false)]
        public override int VisibleWidth
        {
            get
            {
                if(!_visible)
                    return 0;
                if (_labels.Count == 0)
                    return _w;
                int wtmp = 0;
                foreach (Label label in _labels)
                    wtmp += label.VisibleWidth;
                return wtmp;
            }
        }

		[Browsable(false)]
		public Labels Labels
		{
			get
			{
				_labels.BeforeAdd-=new EventHandler(_labels_BeforeAdd);
				_labels.BeforeAdd+=new EventHandler(_labels_BeforeAdd);
				return _labels;
			}
		}

		[Browsable(false)]
		public bool bTop
		{
			get
			{
				return _btop;
			}
			set
			{
				_btop=value;
			}
		}

        [Browsable(false)]
        public Cells Cells
        {
            get
            {
                if (_cells == null)
                    _cells = new Cells();
                return _cells;
            }
        }
		#endregion

		#region method
		public Label GetByName(string name)
		{
			foreach(Label label in _labels)
			{
				if(label.Name.ToLower()==name.ToLower())
				{
					return label;
				}
				if(label is SuperLabel)
				{
					Label tmp=(label as SuperLabel ).GetByName(name);
					if(tmp!=null)
						return tmp;
				}
			}
			return null;
		}

        public int CalcWidth()
        {

            if (_labels.Count == 0)
                return this.Width;
            int w = 0;
            for (int i = 0; i < _labels.Count; i++)
            {
                if (!_labels[i].Visible)
                    continue;
                _labels[i].Y = _y + this.Height;
                if (i > 0)
                {
                    int index=i-1;
                    while (index >= 0)
                    {
                        if (!_labels[index].Visible)
                        {
                            index--;
                            continue;
                        }
                        _labels[i].X = _labels[index].X + _labels[index].Width;
                        break;
                    }
                    if(index<0)
                        _labels[i].X = _x;
                }
                else
                {
                    _labels[i].X = _x;
                }
                
                int subw = 0;
                if (_labels[i] is SuperLabel)
                    subw = (_labels[i] as SuperLabel).CalcWidth();
                else
                    subw = _labels[i].Width;
                w += subw;
            }
            return w;
        }

        public void AutoResize()
        {
            int w = 0;
            foreach (Label l in _labels)
            {
                l.SetY(_y + _h);
                if (l is SuperLabel)
                    (l as SuperLabel).AutoResize();
                w += l.Width;
            }
            if (w != 0)
            {
                this.X = _labels[0].X;
                this.Width = w;
            }
        }

		public void AutoLayOut()
		{
			int w=0;
			if(_labels.Count>0)
			{
				_labels[0].X=this._x;
				_labels[0].SetY(_y+_h);
				if(_labels[0] is SuperLabel)
					(_labels[0] as SuperLabel).AutoLayOut();
				w+=_labels[0].Width;
			}
			for(int i=1;i<_labels.Count;i++)
			{
				_labels[i].X=_labels[i-1].X+_labels[i-1].Width;
				_labels[i].SetY(_y+_h);
				if(_labels[i] is SuperLabel)
					(_labels[i] as SuperLabel).AutoLayOut();
				w+=_labels[i].Width;
			}
			if(w!=0)
				_w=w;
		}

		private void _labels_BeforeAdd(object sender, EventArgs e)
		{
			Label label=sender as Label;
			AddArgs aa=e as AddArgs;
			int i=0;
			while (i<_labels.Count)
			{
				Label lab=_labels[i];
				if(label.bUnder(lab) && lab is SuperLabel )
				{
                    if (label.LabelType != LabelType.DetailLabel)
                        (lab as SuperLabel).LabelType = label.LabelType;
					(lab as SuperLabel).Labels.Add(label);
					aa.bAlreadyAdd=true;
                    i++;
//					return;
				}
				else if(lab.bUnder(label) && label is SuperLabel)
				{
                    if (lab.LabelType != LabelType.DetailLabel)
                        (label  as SuperLabel).LabelType = lab.LabelType;
					(label as SuperLabel).Labels.Add(lab);
					_labels.Remove(lab);
				}
                else
				    i++;
			}
        }

        public override bool bUnder(Cell cell)
        {
            //if (_y == cell.Y && _x == cell.X && _x + _w == cell.X + cell.Width)
            //    return false;
            //else
            //    return _y >= cell.Y && _x >= cell.X && _x + _w <= cell.X + cell.Width;

            return false;
        }

		public override object Clone()
		{
			return new SuperLabel(this);
		}
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("bTop",_btop);
			info.AddValue("Labels",_labels);
		}

		#endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            if (_labels != null)
            {
                _labels.Dispose();
                _labels = null;
            }
            base.Dispose();
        }

        #endregion
    }
}
