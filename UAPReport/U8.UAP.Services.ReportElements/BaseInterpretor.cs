using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Data;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public abstract class BaseInterpretor
    {
        protected Report _report;
        protected DataHelper _datahelper;
        public BaseInterpretor(Report report, DataHelper datahelper)
        {
            _report = report;
            _datahelper = datahelper;
        }

        public virtual void Interprete(XmlDocument localeformat, XmlDocument commonformat)
        {
            _report.Sections.Clear();
            //sections
            //x,y,w,h,caption
            XmlElement root = localeformat.DocumentElement;//report
            if (root == null)
            {
                AddDefaultDetail();
                _report.Sections.Add(new PageFooter(true));
                return;
            }
            else
            {
                SetReportCaption(root);
                foreach (XmlElement xec in root.ChildNodes)//section
                {
                    AddALoacleSection(xec);
                }

                root = commonformat.DocumentElement;
                SetReportProperty(root);
                ArrayList alsections = new ArrayList();
                foreach (XmlElement xec in root.ChildNodes)//section
                {
                    string name = xec.GetAttribute("Name");
                    Section section = _report.Sections[name];
                    if (section == null && name == "ReportSummary")
                        section = _report.Sections["PageFooter"];
                    if (section != null)
                    {
                        AddCommonSection(xec, section);
                        HandleReportHeader(xec, name);
                    }
                    else
                    {
                        section = AddALoacleSection(xec);
                        if (section != null)
                        {
                            AddCommonSection(xec, section);
                        }
                    }
                    if (section != null)
                        alsections.Add(section);
                }

                DeleteDudentSection(alsections);
                alsections.Clear();
                alsections = null;
            }

        }

        protected virtual void HandleReportHeader(XmlElement xec, string name)
        {

        }

        protected virtual void DeleteDudentSection(ArrayList alsections)
        {
            int index = _report.Sections.Count - 1;
            while (index >= 0)
            {
                Section s = _report.Sections[index];
                if (!alsections.Contains(s))
                    _report.Sections.RemoveAt(index);
                index--;
            }
        }

        #region base format
        protected abstract Section AddALoacleSection(XmlElement xec);
        protected abstract Cell AddALocaleCell(XmlElement xecc, Section section);
        protected virtual void AddCommonSection(XmlElement xec, Section section)
        {
            ConvertFrom(xec, section);
            Cells cells = section.Cells;
            ArrayList alcommons = new ArrayList();
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                Cell cell = cells[xecc.GetAttribute("Name")];
                if (cell != null)
                    ConvertFrom(xecc, cell);
                else
                {
                    cell = AppendCommonCells(xecc, section);
                }
                if (cell != null)
                    alcommons.Add(cell);
            }
            DeleteLocaleOnlyCells(alcommons, cells);
        }

        protected virtual void DeleteLocaleOnlyCells(ArrayList alcommons, Cells cells)
        {
            int index = cells.Count - 1;
            while (index >= 0)
            {
                Cell cell = cells[index];
                if (!alcommons.Contains(cell))
                    cells.Remove(cell);
                index--;
            }
        }

        protected virtual Cell AppendCommonCells(XmlElement xecc, Section section)
        {
            Cell cell = AddALocaleCell(xecc, section);

            if (cell != null)
            {
                ConvertFrom(xecc, cell);
                cell.X = 5;
                cell.RelativeY = 20;
                cell.Caption = "新添加的组件";
            }
            return cell;
        }

        #region cell
        protected Color ConvertToColor(string s)
        {
            try
            {
                if (s.Contains(","))
                {
                    string[] rgb = s.Split(',');
                    return Color.FromArgb(Convert.ToInt32(rgb[0]), Convert.ToInt32(rgb[1]), Convert.ToInt32(rgb[2]));
                }
                else
                {
                    return Color.FromArgb(Convert.ToInt32(s));
                }
            }
            catch
            {
            }
            return Color.Empty;
        }
        protected virtual void ConvertFromLocaleInfo(XmlElement xe, Cell receiver)
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
            if (xe.HasAttribute("Caption"))
                receiver.Caption = xe.GetAttribute("Caption");
            if (xe.HasAttribute("IdentityCaption"))
                receiver.IdentityCaption = xe.GetAttribute("IdentityCaption");
            if (receiver.Type == "CommonLabel")
            {
                if (receiver.Caption.Contains("UFIDA"))
                    receiver.Caption = receiver.Caption.Replace("UFIDA", "yonyou");
                if (receiver.IdentityCaption.Contains("UFIDA"))
                    receiver.IdentityCaption = receiver.IdentityCaption.Replace("UFIDA", "yonyou");
            }
        }

        protected virtual void ConvertFrom(XmlElement xe, Cell receiver)
        {
            CellFrom(xe, receiver);
            if (receiver is IBoolean)
                IBooleanFrom(xe, receiver as IBoolean);
            if (receiver is IMergeStyle)
                IMergeSyleFrom(xe, receiver as IMergeStyle);
            if (receiver is ICalculateColumn)
                ICalculateColumnFrom(xe, receiver as ICalculateColumn);
            if (receiver is ICalculateSequence)
                ICalculateSequenceFrom(xe, receiver as ICalculateSequence);
            if (receiver is ICalculator)
                ICalculatorFrom(xe, receiver as ICalculator);
            if (receiver is IDataSource)
                IDataSourceFrom(xe, receiver as IDataSource);
            if (receiver is IDateTime)
                IDateTimeFrom(xe, receiver as IDateTime);

            if (receiver is IFormat)
                IFormatFrom(xe, receiver as IFormat);

            if (receiver is IDecimal)
                IDecimalFrom(xe, receiver as IDecimal);
            if (receiver is IExchangeRate)
                IExchangeRateFrom(xe, receiver as IExchangeRate);
            if (receiver is IExpression)
                IExpressonFrom(xe, receiver as IExpression);

            if (receiver is IImage)
                IImageFrom(xe, receiver as IImage);
            if (receiver is ISection)
                ISectionFrom(xe, receiver as ISection);
            if (receiver is ISort)
                ISortFrom(xe, receiver as ISort);
            if (receiver is IBDateTime)
                IBDateTimeFrom(xe, receiver as IBDateTime);
            if (receiver is IAlgorithm)
                IAlgorithmFrom(xe, receiver as IAlgorithm);
            if (receiver is IAddWhenDesign)
                IAddWhenDesignFrom(xe, receiver as IAddWhenDesign);
            if (receiver is ILabelType)
                ILabelTypeFrom(xe, receiver as ILabelType);
            if (receiver is IDateTimeDimensionLevel)
                IDateTimeDimensionLevelFrom(xe, receiver as IDateTimeDimensionLevel);
            if (receiver is IUserDefine)
                IUserDefineFrom(xe, receiver as IUserDefine);
            //if (receiver is IGroupHeader )
            //    IGroupHeaderFrom(xe, receiver as IGroupHeader );
            if (receiver is IAlternativeStyle)
                IAlternativeStyleFrom(xe, receiver as IAlternativeStyle);
            if (receiver is IApplyColorStyle)
                IApplyColorStyleFrom(xe, receiver as IApplyColorStyle);
            if (receiver is IBarCode)
                IBarCodeFrom(xe, receiver as IBarCode);
            if (receiver is IIndicatorMetrix)
                IIndicatorMetrixFrom(xe, receiver as IIndicatorMetrix);
            if (receiver is IIndicator)
                IIndicatorFrom(xe, receiver as IIndicator);
            if (receiver is IGap)
                IGapFrom(xe, receiver as IGap);
            if (receiver is IGridCollect)
                IGridCollectFrom(xe, receiver as IGridCollect);
            if (receiver is IGridEvent)
                IGridEventFrom(xe, receiver as IGridEvent);
            if (receiver is ICenterAlign)
                ICenterAlignFrom(xe, receiver as ICenterAlign);
        }
        protected void CellFrom(XmlElement xe, Cell receiver)
        {
            if (xe.HasAttribute("BackColor"))
                receiver.BackColor = ConvertToColor(xe.GetAttribute("BackColor"));

            if (xe.HasAttribute("BorderLeft"))
                receiver.Border.Left = Boolean.Parse(xe.GetAttribute("BorderLeft"));
            if (xe.HasAttribute("BorderRight"))
                receiver.Border.Right = Boolean.Parse(xe.GetAttribute("BorderRight"));
            if (xe.HasAttribute("BorderTop"))
                receiver.Border.Top = Boolean.Parse(xe.GetAttribute("BorderTop"));
            if (xe.HasAttribute("BorderBottom"))
                receiver.Border.Bottom = Boolean.Parse(xe.GetAttribute("BorderBottom"));

            if (xe.HasAttribute("BorderColor"))
                receiver.BorderColor = ConvertToColor(xe.GetAttribute("BorderColor"));
            if (xe.HasAttribute("BorderWidth"))
                receiver.BorderWidth = Convert.ToInt32(xe.GetAttribute("BorderWidth"));

            if (xe.HasAttribute("CaptionAlign"))
                receiver.CaptionAlign = (ContentAlignment)Convert.ToInt32(xe.GetAttribute("CaptionAlign"));

            if (xe.HasAttribute("FontName"))
                receiver.ServerFont.FontName = xe.GetAttribute("FontName");
            if (xe.HasAttribute("FontSize"))
                receiver.ServerFont.FontSize = Convert.ToSingle(xe.GetAttribute("FontSize"));
            if (xe.HasAttribute("FontUnit"))
                receiver.ServerFont.FontUnit = (System.Drawing.GraphicsUnit)Convert.ToInt32(xe.GetAttribute("FontUnit"));
            if (xe.HasAttribute("FontGdiCharSet"))
                receiver.ServerFont.GdiCharSet = byte.Parse(xe.GetAttribute("FontGdiCharSet"));
            if (xe.HasAttribute("FontGdiVerticalFont"))
                receiver.ServerFont.GdiVerticalFont = Boolean.Parse(xe.GetAttribute("FontGdiVerticalFont"));
            if (xe.HasAttribute("FontBold"))
                receiver.ServerFont.Bold = Boolean.Parse(xe.GetAttribute("FontBold"));
            if (xe.HasAttribute("FontItalic"))
                receiver.ServerFont.Italic = Boolean.Parse(xe.GetAttribute("FontItalic"));
            if (xe.HasAttribute("FontStrikethout"))
                receiver.ServerFont.StrikethOut = Boolean.Parse(xe.GetAttribute("FontStrikethout"));
            if (xe.HasAttribute("FontUnderline"))
                receiver.ServerFont.UnderLine = Boolean.Parse(xe.GetAttribute("FontUnderline"));

            if (xe.HasAttribute("ForeColor"))
            {
                string fc = xe.GetAttribute("ForeColor");
                if (fc != "0")
                    receiver.ForeColor = ConvertToColor(fc);
            }
            //			if(xe.HasAttribute("Height"))
            //				receiver.Height=Convert.ToInt32(xe.GetAttribute("Height"));
            //if(xe.HasAttribute("Name"))
            //    receiver.Name=xe.GetAttribute("Name");			
            if (xe.HasAttribute("Visible"))
                receiver.Visible = Boolean.Parse(xe.GetAttribute("Visible"));

            if (xe.HasAttribute("bHidden"))
                receiver.bHidden = Boolean.Parse(xe.GetAttribute("bHidden"));

            if (xe.HasAttribute("KeepPos"))
                receiver.KeepPos = Boolean.Parse(xe.GetAttribute("KeepPos"));
            //			if(xe.HasAttribute("Width"))
            //				receiver.Width=Convert.ToInt32(xe.GetAttribute("Width"));
            //			if(xe.HasAttribute("X"))
            //				receiver.X=Convert.ToInt32(xe.GetAttribute("X"));
            //			if(xe.HasAttribute("Y"))
            //				receiver.Y=Convert.ToInt32(xe.GetAttribute("Y"));
            if (xe.HasAttribute("Z_Order"))
                receiver.Z_Order = Convert.ToInt32(xe.GetAttribute("Z_Order"));
            if (xe.HasAttribute("bControlAuth"))
            {
                receiver.bControlAuth = Convert.ToBoolean(xe.GetAttribute("bControlAuth"));
                //if (receiver.bControlAuth)
                //{
                //    if (this._colsauth.Contains(receiver.Name))
                //        receiver.Visible = false;
                //}
            }

            if (xe.HasAttribute("bSupportLocate"))
            {
                receiver.bSupportLocate = Convert.ToBoolean(xe.GetAttribute("bSupportLocate"));
            }
            if (xe.HasAttribute("bShowOnX"))
            {
                receiver.bShowOnX = Convert.ToBoolean(xe.GetAttribute("bShowOnX"));
            }


            XmlElement xescript = xe.SelectSingleNode("PrepaintEvent") as XmlElement;
            if (xescript != null)
            {
                XmlCDataSection cdata = xescript.FirstChild as XmlCDataSection;
                if (cdata != null)
                {
                    receiver.PrepaintEvent = cdata.Data;
                    if (receiver.PrepaintEvent.Contains("cells["))
                        receiver.PrepaintEvent = receiver.PrepaintEvent.Replace(".Value", ".ToString()");

                    if (_report.UnderState != ReportStates.Designtime && receiver.PrepaintEvent.ToLower().Contains("alltoprevious"))
                        _report.MustShowDetail = true;
                }
            }

        }

        protected void ICenterAlignFrom(XmlElement xe, ICenterAlign receiver)
        {
            if (xe.HasAttribute("CenterAlign"))
                receiver.CenterAlign = Boolean.Parse(xe.GetAttribute("CenterAlign"));
        }

        protected void IGridCollectFrom(XmlElement xe, IGridCollect receiver)
        {
            if (xe.HasAttribute("bSummary"))
                receiver.bSummary = Boolean.Parse(xe.GetAttribute("bSummary"));
            if (xe.HasAttribute("bColumnSummary"))
                receiver.bColumnSummary = Boolean.Parse(xe.GetAttribute("bColumnSummary"));
            if (xe.HasAttribute("bClue"))
                receiver.bClue = Boolean.Parse(xe.GetAttribute("bClue"));
            if (xe.HasAttribute("bCalcAfterCross"))
                receiver.bCalcAfterCross = Convert.ToBoolean((xe.GetAttribute("bCalcAfterCross")));
        }

        protected void IGridEventFrom(XmlElement xe, IGridEvent receiver)
        {
            if (xe.HasAttribute("EventType"))
                receiver.EventType = (EventType)Convert.ToInt32((xe.GetAttribute("EventType")));
            if (xe.HasAttribute("bShowAtReal"))
                receiver.bShowAtReal = Convert.ToBoolean((xe.GetAttribute("bShowAtReal")));
        }

        protected void IAlternativeStyleFrom(XmlElement xe, IAlternativeStyle receiver)
        {
            if (xe.HasAttribute("FontName2"))
                receiver.ServerFont2.FontName = xe.GetAttribute("FontName2");
            if (xe.HasAttribute("FontSize2"))
                receiver.ServerFont2.FontSize = Convert.ToSingle(xe.GetAttribute("FontSize2"));
            if (xe.HasAttribute("FontUnit2"))
                receiver.ServerFont2.FontUnit = (System.Drawing.GraphicsUnit)Convert.ToInt32(xe.GetAttribute("FontUnit2"));
            if (xe.HasAttribute("FontGdiCharSet2"))
                receiver.ServerFont2.GdiCharSet = byte.Parse(xe.GetAttribute("FontGdiCharSet2"));
            if (xe.HasAttribute("FontGdiVerticalFont2"))
                receiver.ServerFont2.GdiVerticalFont = Boolean.Parse(xe.GetAttribute("FontGdiVerticalFont2"));
            if (xe.HasAttribute("FontBold2"))
                receiver.ServerFont2.Bold = Boolean.Parse(xe.GetAttribute("FontBold2"));
            if (xe.HasAttribute("FontItalic2"))
                receiver.ServerFont2.Italic = Boolean.Parse(xe.GetAttribute("FontItalic2"));
            if (xe.HasAttribute("FontStrikethout2"))
                receiver.ServerFont2.StrikethOut = Boolean.Parse(xe.GetAttribute("FontStrikethout2"));
            if (xe.HasAttribute("FontUnderline2"))
                receiver.ServerFont2.UnderLine = Boolean.Parse(xe.GetAttribute("FontUnderline2"));

            if (xe.HasAttribute("BackColor2"))
                receiver.BackColor2 = ConvertToColor(xe.GetAttribute("BackColor2"));
            if (xe.HasAttribute("BorderColor2"))
                receiver.BorderColor2 = ConvertToColor(xe.GetAttribute("BorderColor2"));
            if (xe.HasAttribute("ForeColor2"))
                receiver.ForeColor2 = ConvertToColor(xe.GetAttribute("ForeColor2"));

            if (xe.HasAttribute("bApplyAlternative"))
                receiver.bApplyAlternative = Boolean.Parse(xe.GetAttribute("bApplyAlternative"));
        }

        protected void ILabelTypeFrom(XmlElement xe, ILabelType receiver)
        {
            if (xe.HasAttribute("LabelType"))
                receiver.LabelType = (LabelType)Convert.ToInt32(xe.GetAttribute("LabelType"));
        }

        protected void IBarCodeFrom(XmlElement xe, IBarCode receiver)
        {
            if (xe.HasAttribute("BarCodeType"))
                receiver.Symbology = (Neodynamic.WinControls.BarcodeProfessional.Symbology)Convert.ToInt32(xe.GetAttribute("BarCodeType"));
        }

        protected void IDateTimeDimensionLevelFrom(XmlElement xe, IDateTimeDimensionLevel receiver)
        {
            if (xe.HasAttribute("DDLevel"))
                receiver.DDLevel = (DateTimeDimensionLevel)Convert.ToInt32(xe.GetAttribute("DDLevel"));
            if (xe.HasAttribute("ShowYear"))
                receiver.ShowYear = Boolean.Parse(xe.GetAttribute("ShowYear"));
            if (xe.HasAttribute("ShowWeekRange"))
                receiver.ShowWeekRange = Boolean.Parse(xe.GetAttribute("ShowWeekRange"));
            if (xe.HasAttribute("SupportSwitch"))
                receiver.SupportSwitch = Boolean.Parse(xe.GetAttribute("SupportSwitch"));
        }

        protected void IBooleanFrom(XmlElement xe, IBoolean receiver)
        {
            if (xe.HasAttribute("Checked"))
                receiver.Checked = Boolean.Parse(xe.GetAttribute("Checked"));
        }
        protected void IMergeSyleFrom(XmlElement xe, IMergeStyle receiver)
        {
            if (xe.HasAttribute("bMergeCell"))
                receiver.bMergeCell = Boolean.Parse(xe.GetAttribute("bMergeCell"));
        }

        protected void IApplyColorStyleFrom(XmlElement xe, IApplyColorStyle receiver)
        {
            if (xe.HasAttribute("bApplyColorStyle"))
                receiver.bApplyColorStyle = Boolean.Parse(xe.GetAttribute("bApplyColorStyle"));
        }

        //protected void IGroupHeaderFrom(XmlElement xe, IGroupHeader  receiver)
        //{
        //    if (xe.HasAttribute("bShowNullGroup"))
        //        receiver.bShowNullGroup = Boolean.Parse(xe.GetAttribute("bShowNullGroup"));            
        //}

        protected void IAddWhenDesignFrom(XmlElement xe, IAddWhenDesign receiver)
        {
            if (xe.HasAttribute("bAddWhenDesign"))
                receiver.bAddWhenDesign = Boolean.Parse(xe.GetAttribute("bAddWhenDesign"));
        }

        protected void ICalculateColumnFrom(XmlElement xe, ICalculateColumn receiver)
        {
            if (xe.HasAttribute("Expression"))
            {
                receiver.Expression = xe.GetAttribute("Expression");
                if (ExpressionService.IsADigit(receiver.Expression))
                    receiver.Expression = receiver.Expression + "+0";
            }
        }

        protected void IUserDefineFrom(XmlElement xe, IUserDefine receiver)
        {
            if (xe.HasAttribute("UserDefineItem"))
            {
                receiver.UserDefineItem = xe.GetAttribute("UserDefineItem");
            }
        }

        private bool receiver_CheckExpressionInDataSource(string expression)
        {
            return _report.DataSources.Contains(expression);
        }

        protected void ICalculatorFrom(XmlElement xe, ICalculator receiver)
        {
            if (xe.HasAttribute("Operator"))
                receiver.Operator = (OperatorType)Convert.ToInt32(xe.GetAttribute("Operator"));

            if (_report.UnderState != ReportStates.Designtime)
            {
                if (receiver is IAlgorithm && receiver.Operator == OperatorType.SUM)
                    receiver.Operator = OperatorType.ComplexSUM;

                if (receiver.Operator == OperatorType.AccumulateSUM
                                    || receiver.Operator == OperatorType.ComplexSUM
                                    || receiver.Operator == OperatorType.BalanceSUM)
                    _report.MustShowDetail = true;
            }

            if (xe.HasAttribute("Unit"))
            {
                DataSource ds = _report.DataSources[xe.GetAttribute("Unit")];
                if (ds != null)
                    receiver.Unit = ds;
            }
        }

        protected void ICalculateSequenceFrom(XmlElement xe, ICalculateSequence receiver)
        {
            if (xe.HasAttribute("CalculateIndex"))
                receiver.CalculateIndex = Convert.ToInt32(xe.GetAttribute("CalculateIndex"));
        }
        protected void IDataSourceFrom(XmlElement xe, IDataSource receiver)
        {
            if (xe.HasAttribute("DataSource"))
            {
                DataSource ds = _report.DataSources[xe.GetAttribute("DataSource")];
                if (ds != null)
                    receiver.DataSource = ds;
                else
                {
                    receiver.DataSource = new DataSource("EmptyColumn");
                }
            }
        }
        protected void IDateTimeFrom(XmlElement xe, IDateTime receiver)
        {
        }
        protected void IDecimalFrom(XmlElement xe, IDecimal receiver)
        {
            if (xe.HasAttribute("Precision"))
            {
                receiver.Precision = (PrecisionType)Convert.ToInt32(xe.GetAttribute("Precision"));
                if (_report.UnderState != ReportStates.Designtime)
                {
                    if (receiver.Precision == PrecisionType.None && (receiver is IDataSource && (receiver as IDataSource).DataSource.Type == DataType.Int))
                        receiver.PointLength = 0;
                    else
                        receiver.PointLength = Convert.ToInt32(_datahelper.Precisions(receiver.Precision));
                    if (receiver is IFormat && !string.IsNullOrEmpty((receiver as IFormat).FormatString))
                    {
                        if ((receiver as IFormat).FormatString == "P")
                        {
                            receiver.PointLength = receiver.PointLength + 2;
                        }
                        else
                        {
                            if (receiver.Precision != PrecisionType.None)
                            {
                                int index = (receiver as IFormat).FormatString.IndexOf(".");
                                if (receiver.PointLength >= 0 && index > -1 && !(receiver as IFormat).FormatString.EndsWith("."))
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append((receiver as IFormat).FormatString.Substring(0, index));
                                    sb.Append(".");
                                    for (int i = 1; i <= receiver.PointLength; i++)
                                        sb.Append("0");
                                    (receiver as IFormat).FormatString = sb.ToString();
                                }
                            }
                            else
                            {
                                int index = (receiver as IFormat).FormatString.IndexOf(".");
                                if (index > -1)
                                {
                                    receiver.PointLength = (receiver as IFormat).FormatString.Length - index - 1;
                                    if ((receiver as IFormat).FormatString.Contains("%"))
                                        receiver.PointLength += 1;
                                }
                                else
                                    receiver.PointLength = 0;
                            }
                        }
                    }
                    else if (receiver.Precision != PrecisionType.Source)
                    {
                        //if (receiver.PointLength == -1)
                        //    receiver.FormatString = "0";
                        //else
                        if (receiver.PointLength == 0)
                            receiver.FormatString = "0";
                        else if (receiver.PointLength != -1)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("#,##0.");
                            for (int i = 1; i <= receiver.PointLength; i++)
                                sb.Append("0");
                            receiver.FormatString = sb.ToString();
                        }
                    }
                }
            }
            if (xe.HasAttribute("bShowWhenZero"))
            {
                string old = xe.GetAttribute("bShowWhenZero").ToLower();
                if (old != "true" && old != "false")
                    receiver.bShowWhenZero = (StimulateBoolean)Convert.ToInt32(old);
            }
        }
        protected void IExchangeRateFrom(XmlElement xe, IExchangeRate receiver)
        {
            if (xe.HasAttribute("ExchangeCode"))
                receiver.ExchangeCode = xe.GetAttribute("ExchangeCode");
        }
        protected void IExpressonFrom(XmlElement xe, IExpression receiver)
        {
            if (xe.HasAttribute("Formula"))
                receiver.Formula.FormulaExpression = xe.GetAttribute("Formula");
            if (xe.HasAttribute("FormulaType"))
                receiver.Formula.Type = (FormulaType)Convert.ToInt32(xe.GetAttribute("FormulaType"));
            if (xe.HasAttribute("FormatString"))
                receiver.FormatString = xe.GetAttribute("FormatString");
            if (xe.HasAttribute("bDateTime"))
                receiver.bDate = Convert.ToBoolean(xe.GetAttribute("bDateTime"));
            if (xe.HasAttribute("Precision"))
                receiver.Precision = (PrecisionType)Convert.ToInt32(xe.GetAttribute("Precision"));
        }
        protected void IFormatFrom(XmlElement xe, IFormat receiver)
        {
            if (xe.HasAttribute("FormatString"))
                receiver.FormatString = xe.GetAttribute("FormatString");
        }
        protected void IImageFrom(XmlElement xe, IImage receiver)
        {
            if (xe.HasAttribute("ImageString"))
                receiver.ImageString = xe.GetAttribute("ImageString");
            if (xe.HasAttribute("SizeMode"))
                receiver.SizeMode = (PictureBoxSizeMode)Convert.ToInt32(xe.GetAttribute("SizeMode"));
        }
        protected void ISectionFrom(XmlElement xe, ISection receiver)
        {
            if (xe.HasAttribute("Level"))
                receiver.Level = Convert.ToInt32(xe.GetAttribute("Level"));
        }

        protected void IBDateTimeFrom(XmlElement xe, IBDateTime receiver)
        {
            if (xe.HasAttribute("bDateTime"))
                receiver.bDateTime = Convert.ToBoolean(xe.GetAttribute("bDateTime"));
        }
        protected void ISortFrom(XmlElement xe, ISort receiver)
        {
            if (xe.HasAttribute("Direction"))
                receiver.SortOption.SortDirection = (SortDirection)Convert.ToInt32(xe.GetAttribute("Direction"));
            if (xe.HasAttribute("Priority"))
                receiver.SortOption.Priority = Convert.ToInt32(xe.GetAttribute("Priority"));
        }

        protected void IAlgorithmFrom(XmlElement xe, IAlgorithm receiver)
        {
            XmlElement xealgorithm = xe.SelectSingleNode("Algorithm") as XmlElement;
            if (xealgorithm != null)
            {
                XmlCDataSection cdata = xealgorithm.FirstChild as XmlCDataSection;
                if (cdata != null)
                {
                    receiver.Algorithm = cdata.Data;
                    while (receiver.Algorithm.Contains("rows["))
                    {
                        int beginindex = receiver.Algorithm.IndexOf("rows[");
                        string part1 = receiver.Algorithm.Substring(0, beginindex);
                        int endindex = receiver.Algorithm.IndexOf("]", beginindex);
                        string part3 = receiver.Algorithm.Substring(endindex + 1);
                        receiver.Algorithm = part1 + " previous" + part3;
                    }

                    if (_report.UnderState != ReportStates.Designtime &&
                                (receiver.Algorithm.ToLower().Contains("previous") || receiver.Algorithm.ToLower().Contains("alltoprevious")))
                        _report.MustShowDetail = true;
                }
            }
        }

        protected void IIndicatorFrom(XmlElement xe, IIndicator receiver)
        {
            if (xe.HasAttribute("DetailViewStyle"))
            {
                receiver.DetailCompare = new CompareValue();
                ReadACompareValue("Detail", xe, receiver.DetailCompare);
            }
            if (xe.HasAttribute("TotalViewStyle"))
            {
                receiver.TotalCompare = new CompareValue();
                ReadACompareValue("Total", xe, receiver.TotalCompare);
            }
            if (xe.HasAttribute("SummaryViewStyle"))
            {
                receiver.SummaryCompare = new CompareValue();
                ReadACompareValue("Summary", xe, receiver.SummaryCompare);
            }
        }

        protected void ReadACompareValue(string pre, XmlElement xe, CompareValue cv)
        {
            string expression1 = "";
            string expression2 = "";
            XmlElement xealgorithm = xe.SelectSingleNode(pre + "Expression1") as XmlElement;
            if (xealgorithm != null)
            {
                XmlCDataSection cdata = xealgorithm.FirstChild as XmlCDataSection;
                if (cdata != null)
                    expression1 = cdata.Data;
            }
            xealgorithm = xe.SelectSingleNode(pre + "Expression2") as XmlElement;
            if (xealgorithm != null)
            {
                XmlCDataSection cdata = xealgorithm.FirstChild as XmlCDataSection;
                if (cdata != null)
                    expression2 = cdata.Data;
            }
            cv.Expression1 = expression1;
            cv.Expression2 = expression2;
            if (xe.HasAttribute(pre + "ViewStyle"))
                cv.ViewStyle = (IndicatorViewType)Convert.ToInt32(xe.GetAttribute(pre + "ViewStyle"));
            if (xe.HasAttribute(pre + "Performance"))
                cv.Performance = (IndicatorPerformance)Convert.ToInt32(xe.GetAttribute(pre + "Performance"));

            if (xe.HasAttribute(pre + "FlagOnBadOnly"))
                cv.FlagOnBadOnly = Convert.ToBoolean(xe.GetAttribute(pre + "FlagOnBadOnly"));

            if (xe.HasAttribute(pre + "ScriptID"))
                cv.ScriptID = xe.GetAttribute(pre + "ScriptID");
        }

        protected void IIndicatorMetrixFrom(XmlElement xe, IIndicatorMetrix receiver)
        {
            receiver.Groups = xe.GetAttribute("Groups");
            receiver.Cross = xe.GetAttribute("Cross");
            receiver.Indicators = xe.GetAttribute("Indicators");
            if (xe.HasAttribute("ShowSummary"))
                receiver.ShowSummary = Convert.ToBoolean(xe.GetAttribute("ShowSummary"));
            if (xe.HasAttribute("StyleID"))
                receiver.StyleID = xe.GetAttribute("StyleID");
            if (xe.HasAttribute("PageSize"))
                receiver.PageSize = Convert.ToInt32(xe.GetAttribute("PageSize"));
            if (xe.HasAttribute("BorderStyle"))
                receiver.BorderStyle = (GridBorderStyle)Convert.ToInt32(xe.GetAttribute("BorderStyle"));
        }

        protected void IGapFrom(XmlElement xe, IGap receiver)
        {
            if (xe.HasAttribute("GapHeight"))
                receiver.GapHeight = Convert.ToInt32(xe.GetAttribute("GapHeight"));
        }
        #endregion
        #region report
        protected void SetReportCaption(XmlElement root)
        {
            _report.SelfActions = GetSelfActions(root.GetAttribute("SelfActions"));
            //if(root.HasAttribute("Caption"))
            //	_report.Caption=root.GetAttribute("Caption");
            //if (root.HasAttribute("SelfActions"))
            //    _report.SelfActions = GetSelfActions(root.GetAttribute("SelfActions"));
                //_report.SelfActions = SelfActions.FromString(root.GetAttribute("SelfActions"));
            //else
            //{
            //    _report.SelfActions = null;
            //}
        }

        /// <summary>
        /// 修改版，新增从数据库中读取，保证自定义事件可以升级
        /// 1.获取存在于原数据库format字段中的自定义事件，将其存入数据库独立表中
        /// 2.删除原来存储在format字段中的描述
        /// 3.从数据库表report_selfaction表中读取自定义事件
        /// </summary>
        /// <param name="xmlStr"></param>
        /// <returns></returns>
        private SelfActions GetSelfActions(string xmlStr)
        {
            SelfActions sas;
            if (!string.IsNullOrEmpty(xmlStr))
            {
                sas = SelfActions.FromString(xmlStr);
                _report.SelfActions = sas;
                // 2.12修改，这里主要将format字段中的信息全部复制到表中，不需要关心逻辑
                foreach (SelfAction sa in _report.SelfActions)
                {
                    if (sa != null)
                        SetSelfActionIntoDb(sa);
                }

                //删除原来存储在format字段中的描述
                DeleteSelfActionsFromReportViewFormat();
            }
            sas = GetSelfActionsFromDb();
            return sas;
        }

        /// <summary>
        /// 原代码
        /// </summary>
        /// <param name="sas"></param>
        private void SetSelfActions(SelfActions sas)
        {
            SelfActions tmpsas = _report.SelfActions;
            _report.SelfActions = sas;
            if (tmpsas != null && _report.SelfActions != null)
            {
                foreach (SelfAction sa in _report.SelfActions)
                {
                    SelfAction tmpsa = tmpsas[sa.Name];
                    if (tmpsa != null)
                    {
                        if (_datahelper.LocaleID.ToLower() == "en-us")
                        {
                            sa.EnCaption = tmpsa.EnCaption;
                            sa.EnTip = tmpsa.EnTip;
                        }
                        else if (_datahelper.LocaleID.ToLower() == "zh-tw")
                        {
                            sa.TwCaption = tmpsa.TwCaption;
                            sa.TwTip = tmpsa.TwTip;
                        }
                        else// (_datahelper.LocaleID.ToLower() == "zh-cn")
                        {
                            sa.CnCaption = tmpsa.Caption;
                            sa.CnTip = tmpsa.ToolTip;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 修改版，新增从数据库中读取，保证自定义事件可以升级
        /// 1.获取存在于原数据库format字段中的自定义事件，将其存入数据库独立表中
        /// 2.删除原来存储在format字段中的描述
        /// 3.从数据库表report_selfaction表中读取自定义事件
        /// </summary>
        /// <param name="xmlStr"></param>
        private void SetSelfActions(string xmlStr)
        {
            SelfActions sas = GetSelfActions(xmlStr);
            #region 原代码
            SelfActions tmpsas = _report.SelfActions;
            _report.SelfActions = sas;
            if (tmpsas != null && _report.SelfActions != null)
            {
                foreach (SelfAction sa in _report.SelfActions)
                {
                    SelfAction tmpsa = tmpsas[sa.Name];
                    if (tmpsa != null)
                    {
                        if (_datahelper.LocaleID.ToLower() == "en-us")
                        {
                            sa.EnCaption = tmpsa.EnCaption;
                            sa.EnTip = tmpsa.EnTip;
                        }
                        else if (_datahelper.LocaleID.ToLower() == "zh-tw")
                        {
                            sa.TwCaption = tmpsa.TwCaption;
                            sa.TwTip = tmpsa.TwTip;
                        }
                        else// (_datahelper.LocaleID.ToLower() == "zh-cn")
                        {
                            sa.CnCaption = tmpsa.Caption;
                            sa.CnTip = tmpsa.ToolTip;
                        }
                    }

                }
            }
            #endregion
        }

        /// <summary>
        /// 从数据库表report_selfaction表中读取自定义事件
        /// </summary>
        /// <returns>自定义事件集合</returns>
        private SelfActions GetSelfActionsFromDb()
        {
            var selfActions = new SelfActions();
            string sql = string.Format(@"SELECT * FROM uap_report_selfaction where viewID = '{0}'", _report.ViewID);
            var dateSet = _datahelper.ExecFromMeta(sql);
            var dateTable = dateSet.Tables[0];
            if (dateTable != null)
            {
                foreach (var row in dateTable.Rows)
                {
                    var selfAction = new SelfAction(row as DataRow);
                    selfActions.Add(selfAction);
                }
            }
            return selfActions;
        }

        /// <summary>
        /// 从reportView表中的format字段删除selfactions描述
        /// </summary>
        private void DeleteSelfActionsFromReportViewFormat()
        {
            string sqlformat =
                string.Format(@"SELECT format FROM uap_reportview WHERE ID = '{0}'",
                              _report.ViewID);
            string strFormat = _datahelper.ExecuteScalarFromMeta(sqlformat).ToString();
            var doc = new XmlDocument();
            doc.LoadXml(strFormat);
            if (doc.DocumentElement != null)
            {
                var node = doc.DocumentElement.Attributes.GetNamedItem("SelfActions");
                if (node != null)
                {
                    node.Value = "";
                }
                string result = doc.InnerXml;
                string strSql = "UPDATE dbo.UAP_ReportView SET Format = @format WHERE ID = @viewID";
                var list = new List<SqlParameter>()
                    {
                        new SqlParameter("@format", result),
                        new SqlParameter("@viewID", _report.ViewID)
                    };
                _datahelper.ExecuteScalarFromMeta1(strSql, list);
            }
        }

        /// <summary>
        /// 将用户自定义事件插入DB中
        /// </summary>
        /// <param name="tmpsa"></param>
        private void SetSelfActionIntoDb(SelfAction tmpsa)
        {
            string sql = string.Format(
            @"IF NOT EXISTS (SELECT 1 FROM dbo.UAP_Report_SelfAction  WHERE viewID = N'{13}' AND actionclass = N'{5}' AND name = N'{0}') 
           INSERT INTO dbo.UAP_Report_SelfAction 
           (name ,
            caption ,
            encaption ,
            twcaption ,
            imagestring ,
            actionclass ,
            tooltip ,
            entooltip ,
            twtooltip ,
            bdoubleclickaction ,
            bshowcaption ,
            bneedcontext ,
            ProjectID ,
            viewID,
            cSub_Id)
           VALUES  ( N'{0}' , 
             N'{1}' , 
             N'{2}' ,  
             N'{3}' ,  
             N'{4}' ,  
             N'{5}' ,  
             N'{6}' ,  
             N'{7}' ,  
             N'{8}' ,  
             N'{9}' ,  
             N'{10}' , 
             N'{11}' , 
             N'{12}' , 
             N'{13}' ,
             N'{14}'
        )", tmpsa.Name, tmpsa.Caption, tmpsa.EnCaption,
          tmpsa.TwCaption, tmpsa.ImageString, tmpsa.ActionClass,
          tmpsa.ToolTip, tmpsa.EnTip, tmpsa.TwTip,
          tmpsa.bDoubleClickAction ? 1 : 0,
          tmpsa.bShowCaptionOnToolBar ? 1 : 0,
          tmpsa.bNeedContext ? 1 : 0,
          _report.ProjectID, _report.ViewID,
          _report.SubId.ToLower()=="outu8"?_report.ViewID.Substring(0,2):_report.SubId// 如果是升级上来的老报表这里取不到正确的subId这里需要从viewId取得。
          );
            _datahelper.ExecuteScalarFromMeta(sql);
        }


        protected void SetReportProperty(XmlElement root)
        {
            if (root.HasAttribute("SelfActions"))
            {
                //SetSelfActions(SelfActions.FromString(root.GetAttribute("SelfActions")));
                SetSelfActions(root.GetAttribute("SelfActions"));
            }
            else if (_report.SelfActions == null)
                _report.SelfActions = new SelfActions();

            if (root.HasAttribute("bPageByGroup"))
                _report.bPageByGroup = Convert.ToBoolean(root.GetAttribute("bPageByGroup"));
            if (root.HasAttribute("SolidGroup"))
                _report.SolidGroup = root.GetAttribute("SolidGroup");
            if (root.HasAttribute("SolidGroupStyle"))
                _report.SolidGroupStyle = (SolidGroupStyle)Convert.ToInt32(root.GetAttribute("SolidGroupStyle"));

            if (root.HasAttribute("DesignWidth"))
                _report.DesignWidth = Convert.ToInt32(root.GetAttribute("DesignWidth"));
            if (root.HasAttribute("PageRecords"))
                _report.PageRecords = Convert.ToInt32(root.GetAttribute("PageRecords"));
            //			if(root.HasAttribute("PagingCriterion"))
            //				_report.PagingCriterion=(PagingCriterion)Convert.ToInt32(root.GetAttribute("PagingCriterion"));
            if (root.HasAttribute("GlobalVarients"))
                _report.Varients = GlobalVarients.FromString(root.GetAttribute("GlobalVarients"));
            if (root.HasAttribute("ViewClass"))
                _report.ViewClass = root.GetAttribute("ViewClass");

            if (root.HasAttribute("FreeViewStyle"))
                _report.FreeViewStyle = (FreeViewStyle)Convert.ToInt32(root.GetAttribute("FreeViewStyle"));

            if (root.HasAttribute("SupportDynamicColumn"))
                _report.bSupportDynamicColumn = Convert.ToBoolean(root.GetAttribute("SupportDynamicColumn"));
            if (root.HasAttribute("DynamicColumnVisible"))
                _report.bDynamicColumnVisible = Convert.ToBoolean(root.GetAttribute("DynamicColumnVisible"));
            if (root.HasAttribute("AllowGroup"))
                _report.AllowGroup = Convert.ToBoolean(root.GetAttribute("AllowGroup"));
            if (root.HasAttribute("AllowCross"))
                _report.AllowCross = Convert.ToBoolean(root.GetAttribute("AllowCross"));

            if (root.HasAttribute("bShowWhenZero"))
                _report.bShowWhenZero = Convert.ToBoolean(root.GetAttribute("bShowWhenZero"));

            if (root.HasAttribute("ReportHeaderOption"))
                _report.ReportHeaderOption = (ReportHeaderPrintOption)Convert.ToInt32(root.GetAttribute("ReportHeaderOption"));
            //if (root.HasAttribute("PrintProvider"))
            //    _report.PrintProvider  = (PrintProvider )Convert.ToInt32(root.GetAttribute("PrintProvider"));
            //if (root.HasAttribute("CanSelectProvider"))
            //    _report.CanSelectProvider  = Convert.ToBoolean(root.GetAttribute("CanSelectProvider"));
            //if (root.HasAttribute("FixedRowsPerPage"))
            //    _report.FixedRowsPerPage = Convert.ToInt32(root.GetAttribute("FixedRowsPerPage"));

            if (root.HasAttribute("BorderStyle"))
                _report.BorderStyle = (GridBorderStyle)Convert.ToInt32(root.GetAttribute("BorderStyle"));
            if (root.HasAttribute("BorderColor"))
                _report.BorderColor = ConvertToColor(root.GetAttribute("BorderColor"));

            if (root.HasAttribute("SolidGroup"))
                _report.SolidGroup = root.GetAttribute("SolidGroup");

            if (root.HasAttribute("SolidSort"))
                _report.SolidSort = Convert.ToBoolean(root.GetAttribute("SolidSort"));

            if (root.HasAttribute("FreeColorStyleID"))
                _report.FreeColorStyleID = root.GetAttribute("FreeColorStyleID");

            if (root.HasAttribute("SupportType"))
            {
                if (Convert.ToInt32(root.GetAttribute("SupportType")) == 0)
                    _report.FreeViewStyle = FreeViewStyle.MergeCell;
            }

            if (root.HasAttribute("MustShowDetail"))
            {
                _report.MustShowDetail = Convert.ToBoolean(root.GetAttribute("MustShowDetail"));
            }

            if (root.HasAttribute("HelpName"))
            {
                HelpSetting hs = _report.HelpInfo;
                if (hs.FileName == "")
                {
                    hs.FileName = root.GetAttribute("HelpName");
                    hs.KeyIndex = root.GetAttribute("KeyIndex");
                    hs.KeyWord = root.GetAttribute("KeyWord");
                }
            }

            if (root.HasAttribute("FilterSource"))
            {
                DataSource ds = _report.DataSources[root.GetAttribute("FilterSource")];
                if (ds != null)
                    _report.FilterSource = ds;
                else
                {
                    _report.FilterSource = new DataSource("EmptyColumn");
                }
            }

            XmlElement xeinitevent = root.SelectSingleNode("InitEvent") as XmlElement;
            if (xeinitevent != null)
            {
                XmlCDataSection cdata = xeinitevent.FirstChild as XmlCDataSection;
                if (cdata != null)
                {
                    _report.InitEvent = cdata.Data;
                    if (_report.UnderState != ReportStates.Designtime && _report.InitEvent.ToLower().Contains("alltoprevious"))
                        _report.MustShowDetail = true;
                }
            }

            XmlElement xeinformations = root.SelectSingleNode("Informations") as XmlElement;
            if (xeinformations != null)
            {
                foreach (XmlElement xeinformation in xeinformations.ChildNodes)
                {
                    Information infor = new Information();
                    infor.Name = xeinformation.GetAttribute("Name");
                    infor.InformationHandler = xeinformation.GetAttribute("Handler");
                    _report.Informations.Add(infor);
                }
            }

            //XmlElement xegroupevent = root.SelectSingleNode("GroupFilter") as XmlElement;
            //if (xegroupevent != null)
            //{
            //    XmlCDataSection cdata = xegroupevent.FirstChild as XmlCDataSection;
            //    if (cdata != null)
            //    {
            //        _report.GroupFilter = cdata.Data;
            //    }
            //}

            if (root.HasAttribute("RowFilter"))
            {
                _report.RowFilter = RowFilter.FromString(root.GetAttribute("RowFilter"));
            }

            if (root.HasAttribute("bAdjustPrintWidth"))
                _report.bAdjustPrintWidth = Convert.ToBoolean(root.GetAttribute("bAdjustPrintWidth"));
            if (root.HasAttribute("ReportColorSet"))
                _report.ReportColorSet = root.GetAttribute("ReportColorSet");

            if (root.HasAttribute("Assembly"))
            {
                //byte[] bs = Convert.FromBase64String(root.GetAttribute("Assembly"));
                //if (bs.Length > 0)
                //{
                //    _dynamiccomponent = Assembly.Load(bs);
                //    _tmpassembly = bs;
                //}
            }
        }

        #endregion
        #endregion

        #region detail
        protected abstract void AddDefaultDetail();
        public abstract SectionType DetailType { get; }
        #endregion
    }
}
