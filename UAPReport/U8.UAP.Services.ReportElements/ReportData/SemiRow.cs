using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class SemiRow : IDisposable, IRow
    {
        private SectionType _type;
        private Hashtable _datas;
        private Hashtable _backcolors;
        private Hashtable _forecolors;
        private Hashtable _visibles;
        private Hashtable _xs;
        private Hashtable _widths;
        private Hashtable _comparevalue1;
        private Hashtable _comparevalue2;
        private Color _backcolor = Color.Empty;
        private Color _forecolor = Color.Empty;
        public SemiRow(int level, SectionType type)
        {
            _type = type;
            _datas = new Hashtable();
            _datas.Add(SemiRow.LevelKey.ToLower(), level);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this._datas != null)
                this._datas.Clear();
            if (this._backcolors != null)
                this._backcolors.Clear();
            if (this._forecolors != null)
                this._forecolors.Clear();
            if (_visibles != null)
                this._visibles.Clear();
            if (_xs != null)
                _xs.Clear();
            if (_widths != null)
                _widths.Clear();
            if (_comparevalue1 != null)
                _comparevalue1.Clear();
            if (_comparevalue2 != null)
                _comparevalue2.Clear();
            this._datas = null;
            this._backcolors = null;
            this._forecolors = null;
            this._visibles = null;
            _xs = null;
            _widths = null;
            _comparevalue1 = null;
            _comparevalue2 = null;
        }

        public static string LevelKey
        {
            get
            {
                return "SemiRowLevel";
            }
        }

        public object this[string key]
        {
            get
            {
                if (_datas.Contains(key.ToLower()))
                    return _datas[key.ToLower()];
                return null;
            }
            set
            {
                Add(key, value);
            }
        }

        public Color BackColor
        {
            get
            {
                return _backcolor;
            }
            set
            {
                _backcolor = value;
            }
        }

        public Color ForeColor
        {
            get
            {
                return _forecolor;
            }
            set
            {
                _forecolor = value;
            }
        }

        public bool bMinorLevel
        {
            get
            {
                return _type == SectionType.None;
            }
        }

        public object CompareValue1s(string key)
        {
            return _comparevalue1[key.ToLower()];
        }

        public object CompareValue2s(string key)
        {
            return _comparevalue2[key.ToLower()];
        }

        //
        // pengzhzh 2012-6-27 _backcolors 优先于 _backcolor
        // 修改代码使单元格的颜色优先于行的颜色
        //

        //public void SetBackColors(Color color)
        //{
        //    if (_backcolors != null)
        //    {
        //        foreach (var key in _backcolors.Keys)
        //        {
        //            _backcolors[key] = color;
        //        }
        //    }
        //}

        //public void SetForeColors(Color color)
        //{
        //    if (_forecolors != null)
        //    {
        //        foreach (var key in _forecolors.Keys)
        //        {
        //            _forecolors[key] = color;
        //        }
        //    }
        //}

        public Color BackColors(string key)
        {
            //if (_backcolor != Color.Empty)
            //    return _backcolor;
            //return Color.FromArgb(Convert.ToInt32(_backcolors[key.ToLower()]));
            //自定义事件中的配色不起作用，只有设置了条件格式才起作用，原因是因为没有进行
            //_backcolors非空判断
            if (_backcolors == null)
            {
                if (_backcolor != null)
                    return _backcolor;
                else return Color.Empty;
            }
            if (_backcolors.ContainsKey(key.ToLower()))
            {
                return Color.FromArgb(Convert.ToInt32(_backcolors[key.ToLower()]));
            }
            return _backcolor;
        }

        public Color ForeColors(string key)
        {
            //if (_forecolor != Color.Empty)
            //    return _forecolor;
            //return Color.FromArgb(Convert.ToInt32(_forecolors[key.ToLower()]));
            if (_forecolors.ContainsKey(key.ToLower()))
            {
                return Color.FromArgb(Convert.ToInt32(_forecolors[key.ToLower()]));
            }
            return _forecolor;
        }

        public bool Visibles(string key)
        {
            return Convert.ToBoolean(_visibles[key.ToLower()]);
        }

        public int Xs(string key)
        {
            return Convert.ToInt32(_xs[key.ToLower()]);
        }

        public int Widths(string key)
        {
            return Convert.ToInt32(_widths[key.ToLower()]);
        }

        public ICollection BackColorKeys
        {
            get
            {
                if (_backcolor != Color.Empty)
                    return Keys;
                if (_backcolors != null)
                    return _backcolors.Keys;
                return null;
            }
        }

        public ICollection ForeColorKeys
        {
            get
            {
                if (_forecolor != Color.Empty)
                    return Keys;
                if (_forecolors != null)
                    return _forecolors.Keys;
                return null;
            }
        }

        public ICollection Keys
        {
            get
            {
                return _datas.Keys;
            }
        }

        public bool Contains(string key)
        {
            return _datas.Contains(key.ToLower());
        }

        public bool CompareValue1Contains(string key)
        {
            return _comparevalue1 != null && _comparevalue1.Contains(key.ToLower());
        }

        public bool CompareValue2Contains(string key)
        {
            return _comparevalue2 != null && _comparevalue2.Contains(key.ToLower());
        }

        public bool BackColorContains(string key)
        {
            if (_backcolor != Color.Empty)
                return true;
            return _backcolors != null && _backcolors.Contains(key.ToLower());
        }

        public bool ForeColorContains(string key)
        {
            if (_forecolor != Color.Empty)
                return true;
            return _forecolors != null && _forecolors.Contains(key.ToLower());
        }

        public bool VisibleContains(string key)
        {
            return _visibles != null && _visibles.Contains(key.ToLower());
        }

        public bool XContains(string key)
        {
            return _xs != null && _xs.Contains(key.ToLower());
        }

        public bool WidthContains(string key)
        {
            return _widths != null && _widths.Contains(key.ToLower());
        }

        /// <summary>
        /// 该行在分组层次中的级别
        /// </summary>
        /// <remarks>
        /// 分组层次级别的基数为1,小于1则表明有错误.
        /// 特别地,无分组时的明细行的级别等于1
        /// </remarks>
        public int Level
        {
            get { return Convert.ToInt32(this[SemiRow.LevelKey]); }
        }

        public SectionType SectionType
        {
            get
            {
                return _type;
            }
        }

        public void Add(string key, object value)
        {
            if (!_datas.Contains(key.ToLower()))
                _datas.Add(key.ToLower(), value);
            else
                _datas[key.ToLower()] = value;
        }

        public void AddCompareValue1(string key, object value)
        {
            if (_comparevalue1 == null)
                _comparevalue1 = new Hashtable();
            if (!_comparevalue1.Contains(key.ToLower()))
                _comparevalue1.Add(key.ToLower(), value);
            else
                _comparevalue1[key.ToLower()] = value;
        }

        public void AddCompareValue2(string key, object value)
        {
            if (_comparevalue2 == null)
                _comparevalue2 = new Hashtable();
            if (!_comparevalue2.Contains(key.ToLower()))
                _comparevalue2.Add(key.ToLower(), value);
            else
                _comparevalue2[key.ToLower()] = value;
        }

        public void AddBackColor(string key, Color color)
        {
            if (_backcolors == null)
                _backcolors = new Hashtable();
            if (!_backcolors.Contains(key.ToLower()))
                _backcolors.Add(key.ToLower(), color.ToArgb());
            else
                _backcolors[key.ToLower()] = color.ToArgb();
        }

        public void AddForeColor(string key, Color color)
        {
            if (_forecolors == null)
                _forecolors = new Hashtable();
            if (!_forecolors.Contains(key.ToLower()))
                _forecolors.Add(key.ToLower(), color.ToArgb());
            else
                _forecolors[key.ToLower()] = color.ToArgb();
        }

        public void AddVisible(string key, bool visible)
        {
            if (_visibles == null)
                _visibles = new Hashtable();
            if (!_visibles.Contains(key.ToLower()))
                _visibles.Add(key.ToLower(), visible);
            else
                _visibles[key.ToLower()] = visible;
        }

        public void AddX(string key, int x)
        {
            if (_xs == null)
                _xs = new Hashtable();
            if (!_xs.Contains(key.ToLower()))
                _xs.Add(key.ToLower(), x);
            else
                _xs[key.ToLower()] = x;
        }

        public void AddWidth(string key, int w)
        {
            if (_widths == null)
                _widths = new Hashtable();
            if (!_widths.Contains(key.ToLower()))
                _widths.Add(key.ToLower(), w);
            else
                _widths[key.ToLower()] = w;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Level: ");
            sb.Append(Level.ToString());
            foreach (string key in this.Keys)
            {
                sb.Append(" ");
                sb.Append(key);
                sb.Append(": ");
                sb.Append(this[key].ToString());
            }
            return sb.ToString();
        }

        public System.Data.DataRow DataRow
        {
            get
            {
                return null;
            }
        }
    }
}
