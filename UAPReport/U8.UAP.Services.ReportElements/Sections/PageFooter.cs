using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// Detail 的摘要说明。
    /// </summary>
    [Serializable]
    public class PageFooter : Section, ISerializable, ICloneable, IAutoSequence
    {
        protected bool _bautosequence = false;
        public PageFooter()
            : base()
        {
        }
        
        public PageFooter(bool needAddDefaultFooter)
            : base()
        {
            try
            {
                if (needAddDefaultFooter)
                {
                    XmlDocument doc = new XmlDocument();
                    string filePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\DefaultFooterConfig.xml";
                    if(!System.IO.File.Exists(filePath))
                        filePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\uap\DefaultFooterConfig.xml";
                        doc.Load(filePath);
                  
                  

                    Cell f = null;
                    XmlElement root = doc.DocumentElement;
                    foreach (XmlElement xec in root.ChildNodes)
                    {
                        string type = xec.GetAttribute("Type").ToLower();
                        if (type == "commonlabel")
                        {
                            f = new CommonLabel();
                            f.CaptionAlign = System.Drawing.ContentAlignment.MiddleRight;
                        }
                        else
                        {
                            f = new Expression();
                            Formula formula = new Formula();
                            formula.Type = FormulaType.Common;
                            formula.FormulaExpression = xec.GetAttribute("Caption").ToLower();
                            (f as Expression).Formula = formula;
                            CaptionAlign = System.Drawing.ContentAlignment.MiddleLeft;
                        }
                        ConvertFromLocaleInfo(xec, f);
                        this.Cells.Add(f);

                    }

                }
            }
            catch { }
        }

       
      
        public PageFooter(int height)
            : base(height)
        {
        }

        public PageFooter(PageFooter pagefooter)
            : base(pagefooter)
        {
            _bautosequence = pagefooter.bAutoSequence;
        }

        protected PageFooter(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bautosequence = info.GetBoolean("bAutoSequence");
        }

        private void ConvertFromLocaleInfo(XmlElement xe, Cell receiver)
        {
            receiver.Name = xe.GetAttribute("Name");
            if (xe.HasAttribute("X"))
                receiver.X = Convert.ToInt32(xe.GetAttribute("X"));
            if (xe.HasAttribute("Y"))
                receiver.Y = Convert.ToInt32(xe.GetAttribute("Y"));
            if (xe.HasAttribute("Width"))
                receiver.Width = Convert.ToInt32(xe.GetAttribute("Width"));
            if (xe.HasAttribute("Height"))
                receiver.Height = Convert.ToInt32(xe.GetAttribute("Height"));
            switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
            {
                case "en-US":
                    if (xe.HasAttribute("CaptionEN"))
                        receiver.Caption = xe.GetAttribute("CaptionEN");
                    break;
                case "zh-TW":
                    if (xe.HasAttribute("CaptionTW"))
                        receiver.Caption = xe.GetAttribute("CaptionTW");
                    break;
                default:
                    if (xe.HasAttribute("Caption"))
                        receiver.Caption = xe.GetAttribute("Caption");
                    break;
            }
            if (xe.HasAttribute("IdentityCaption"))
                receiver.IdentityCaption = receiver.Caption;

            if (receiver.Type == "CommonLabel")
            {
                if (receiver.Caption.Contains("UFIDA"))
                    receiver.Caption = receiver.Caption.Replace("UFIDA", "yonyou");
                if (receiver.IdentityCaption.Contains("UFIDA"))
                    receiver.IdentityCaption = receiver.IdentityCaption.Replace("UFIDA", "yonyou");
            }

        }    

        protected override void SetOrderID()
        {
            _orderid = 9;
        }

        protected override void SetSectionType()
        {
            _sectiontype = SectionType.PageFooter;
        }

        public override void SetType()
        {
            _type = "PageFooter";
            _x = 0;
        }

        protected override string CaptionID
        {
            get { return "U8.UAP.Services.ReportElements.Tx18"; }
        }

        public override Cell GetDefaultRect(DataSource ds)
        {
            return null;
        }

        public override States GetDefaultState(DataType type)
        {
            return States.Arrow;
        }

        public override bool CanBeParent(string type)
        {
            type = type.ToLower();
            if (type == "commonlabel" ||
                type == "expression" ||
                type == "commonexpression" ||
                type == "filterexpression" ||
                type == "image")
                return true;
            else
                return false;
        }

        [Browsable(false)]
        public override int VisibleWidth
        {
            get
            {
                if (_bautosequence)
                {
                    int max = 0;
                    if (this._sectionlines.Count == 0)
                        this.AsignToSectionLines();
                    for (int i = 0; i < _sectionlines.Count; i++)
                    {
                        int tmp = this.CalcVisibleWidth(_sectionlines[i].Cells);
                        if (tmp > max)
                            max = tmp;
                    }
                    return max;
                }
                else
                    return base.VisibleWidth;
            }
        }


        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("bAutoSequence", _bautosequence);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new PageFooter(this);
        }

        #endregion

        #region IAutoSequence 成员
        [Browsable(false)]
        [DisplayText("U8.UAP.Services.Report.bAutoSequence")]
        [LocalizeDescription("U8.UAP.Services.Report.bAutoSequence")]
        public bool bAutoSequence
        {
            get
            {
                return _bautosequence;
            }
            set
            {
                _bautosequence = value;
            }
        }

        #endregion
    }
}
