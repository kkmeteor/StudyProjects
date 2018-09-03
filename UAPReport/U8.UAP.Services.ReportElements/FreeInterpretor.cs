using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class FreeInterpretor:BaseInterpretor
    {
        #region constructor
        public FreeInterpretor(Report report, DataHelper datahelper)
            : base(report,datahelper )
        {
        }
        #endregion

        #region base format
        protected override Section AddALoacleSection(XmlElement xec)
        {
            Section section = null;
            string type = xec.GetAttribute("Type");
            switch (type.ToLower())
            {
                case "pageheader":
                    section = new PageHeader();
                    AddOtherControl(xec, section);
                    break;
                case "pagefooter":
                    section = new PageFooter();
                    AddOtherControl(xec, section);
                    break;
                case "pagetitle":
                    section = new PageTitle();
                    AddCalculateControl(xec, section);
                    break;
                case "printpagetitle":
                    section = new PrintPageTitle ();
                    AddOtherControl(xec, section);
                    break;
                case "printpagesummary":
                    section = new PrintPageSummary ();
                    AddOtherControl(xec, section);
                    break;
                case "reportheader":
                    section = new ReportHeader();
                    AddHeaderControl(xec, section);
                    break;
                case "reportsummary":
                    section = new ReportSummary();
                    AddCalculateControl(xec, section);
                    (section as ReportSummary).InitVisibleWidth();
                    break;
                case "groupheader":
                    section = new GroupHeader();
                    AddGroupControl(xec, section);
                    break;
                case "groupsummary":
                    section = new GroupSummary();
                    AddGroupSummaryControl(xec, section);
                    break;
                case "detail":
                    section = new Detail();
                    AddNormalControl(xec, section);
                    break;
            }
            if (section != null)
            {
                ConvertFromLocaleInfo(xec, section);
                //section.Name = "";
                _report.Sections.Add(section);
            }
            return section;
        }

        protected override Cell AddALocaleCell(XmlElement xecc, Section section)
        {
            string type = xecc.GetAttribute("Type");
            if(!section.CanBeParent(type))
                return null;
            if (section is PageTitle && type.ToLower() == "commonlabel")
                type = "Label";
            if (_report.Type == ReportType.FreeReport && type.ToLower() == "superlabel")
                type = "Label";
            Cell cell = CalculateControl(type);
            if (cell == null)
                cell = NormalControl(type);
            if (cell == null)
                cell = GroupControl(type);
            if (cell == null)
                cell = OtherControl(type);
            if (cell != null)
            {
                ConvertFromLocaleInfo(xecc, cell);
                if (cell is Label && (_report.UnderState != ReportStates.Designtime))
                    section.AddALabel(cell as Label);
                else
                    section.Cells.Add(cell);
            }
            return cell;
        }

        protected void AddOtherControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                Cell cell = OtherControl(xecc.GetAttribute("Type"));
                if (cell == null)
                    continue;
                ConvertFromLocaleInfo(xecc, cell);
                if (cell is Label && (_report.UnderState != ReportStates.Designtime))
                    section.AddALabel(cell as Label);
                else
                    section.Cells.Add(cell);
            }
        }

        protected Cell OtherControl(string type)
        {
            Cell cell = null;
            switch (type.ToLower())
            {
                case "commonlabel":
                    cell = new CommonLabel();
                    break;
                case "expression":
                    cell = new Expression();
                    break;
                case "image":
                    cell = new Image();
                    break;
            }
            return cell;
        }

        //header,summary
        private void AddCalculateControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type");
                if (section is PageTitle && type.ToLower() == "commonlabel")
                    type = "Label";
                if (_report.Type == ReportType.FreeReport && type.ToLower() == "superlabel")
                    type = "Label";
                Cell cell = CalculateControl(type);
                if (cell == null)
                    cell = OtherControl(type);
                if (cell == null)
                    continue;
                ConvertFromLocaleInfo(xecc, cell);
                if (cell is Label && (_report.UnderState != ReportStates.Designtime))
                    section.AddALabel(cell as Label);
                else
                    section.Cells.Add(cell);
            }
        }

        private void AddHeaderControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type");
                if (section is PageTitle && type.ToLower() == "commonlabel")
                    type = "Label";
                if (_report.Type == ReportType.FreeReport && type.ToLower() == "superlabel")
                    type = "Label";
                Cell cell = CalculateControl(type);
                if (cell == null)
                    cell = OtherControl(type);
                if (cell == null)
                    cell = NormalControl(type);
                if (cell == null)
                    continue;
                ConvertFromLocaleInfo(xecc, cell);
                if (cell is Label && (_report.UnderState != ReportStates.Designtime))
                    section.AddALabel(cell as Label);
                else
                    section.Cells.Add(cell);
            }
        }

        private Cell CalculateControl(string type)
        {
            Cell cell = null;
            switch (type.ToLower())
            {
                case "algorithmcalculator":
                    cell = new AlgorithmCalculator();
                    break;
                case "decimalalgorithmcalculator":
                    cell = new DecimalAlgorithmCalculator();
                    break;
                case "calculator":
                    cell = new Calculator();
                    break;
                case "superlabel":
                    cell = new SuperLabel();
                    break;
                case "label":
                    cell = new Label();
                    break;
                //case "dbtext"://unit
                //    cell = new DBText();
                //    break;
                case "chart":
                    cell = new Chart();
                    break;
            }
            return cell;
        }

        //group header
        private void AddGroupControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type");
                if (_report.Type == ReportType.FreeReport && type.ToLower() == "superlabel")
                    type = "Label";
                Cell cell = GroupControl(type);
                if (cell == null)
                    cell = CalculateControl(type);
                if (cell == null)
                    cell = NormalControl(type);
                if (cell == null)
                    cell = OtherControl(type);
                if (cell == null)
                    continue;
                ConvertFromLocaleInfo(xecc, cell);
                if (cell is Label && (_report.UnderState != ReportStates.Designtime))
                    section.AddALabel(cell as Label);
                else
                    section.Cells.Add(cell);
            }
        }

        //group summary
        private void AddGroupSummaryControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type");
                Cell cell = CalculateControl(type);
                if (cell == null)
                    cell = NormalControl(type);
                if (cell == null)
                    cell = OtherControl(type);
                if (cell == null)
                    continue;
                ConvertFromLocaleInfo(xecc, cell);
                section.Cells.Add(cell);
            }
        }

        private Cell GroupControl(string type)
        {
            Cell cell = null;
            switch (type.ToLower())
            {
                case "groupobject":
                    cell = new GroupObject();
                    break;
                case "calculategroupobject":
                    cell = new CalculateGroupObject();
                    break;
                case "algorithmgroupobject":
                    cell = new AlgorithmGroupObject();
                    break;
            }
            return cell;
        }

        //normal
        private void AddNormalControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                Cell cell = NormalControl(xecc.GetAttribute("Type"));
                if (cell == null)
                    continue;
                ConvertFromLocaleInfo(xecc, cell);
                section.Cells.Add(cell);
            }
        }

        private Cell SummaryNormalControl(string type)
        {
            Cell cell = null;
            switch (type.ToLower())
            {
                case "commonlabel":
                    cell = new CommonLabel();
                    break;
            }
            return cell;
        }

        private Cell NormalControl(string type)
        {
            Cell cell = null;
            switch (type.ToLower())
            {
                case "commonlabel":
                    cell = new CommonLabel();
                    break;
                case "dbboolean":
                    cell = new DBBoolean();
                    break;
                case "dbimage":
                    cell = new DBImage();
                    break;
                case "dbdecimal":
                    cell = new DBDecimal();
                    break;
                case "calculatecolumn":
                    cell = new CalculateColumn();
                    break;
                case "columnexpression":
                    cell = new ColumnExpression();
                    break;
                case "decimalalgorithmcolumn":
                    cell = new DecimalAlgorithmColumn();
                    break;
                case "algorithmcolumn":
                    cell = new AlgorithmColumn();
                    break;
                case "dbdatetime":
                    cell = new DBDateTime();
                    break;
                case "dbexchangerate":
                    //cell=new GridExchangeRate();
                    break;
                case "dbtext":
                    cell = new DBText();
                    break;
                case "barcode":
                    cell = new BarCode();
                    break;
                case "gridproportiondecimal":
                    cell = new GridProportionDecimal();
                    break;
            }
            return cell;
        }

        protected override void ConvertFrom(System.Xml.XmlElement xe, Cell receiver)
        {
            base.ConvertFrom(xe, receiver);
            if (receiver is IGroupHeader)
                IGroupHeaderFrom(xe, receiver as IGroupHeader);
            if (receiver is IAutoSequence)
                IAutoSequenceFrom(xe, receiver as IAutoSequence);
            if (receiver is Chart)
                IChartFrom(xe, receiver as Chart);
            if (receiver is Gauge)
                IGaugeFrom(xe, receiver as Gauge);
            if (receiver is IInformationSender)
                IInformationSenderFrom(xe, receiver as IInformationSender);
            if (receiver is IGroupDimensionStyle )
                IGroupDimensionStyleFrom(xe, receiver as IGroupDimensionStyle);
        }

        protected void IChartFrom(XmlElement xe, Chart receiver)
        {
            if (xe.HasAttribute("Level"))
                receiver.Level = Convert.ToInt32 (xe.GetAttribute("Level"));
            if (xe.HasAttribute("DataSource"))
                receiver.DataSource = xe.GetAttribute("DataSource");
        }

        protected void IGaugeFrom(XmlElement xe, Gauge receiver)
        {
            if (xe.HasAttribute("GaugeType"))
                receiver.GaugeType = (GaugeType)Convert.ToInt32(xe.GetAttribute("GaugeType"));
            if (xe.HasAttribute("TemplateIndex"))
                receiver.TemplateIndex = Convert.ToInt32(xe.GetAttribute("TemplateIndex"));
            if (xe.HasAttribute("IndicatorName"))
                receiver.IndicatorName = xe.GetAttribute("IndicatorName");
            if (xe.HasAttribute("NeedleColor"))
                receiver.NeedleColor = ConvertToColor(xe.GetAttribute("NeedleColor"));
            if (xe.HasAttribute("NeedleLength"))
                receiver.NeedleLength = Convert.ToInt32(xe.GetAttribute("NeedleLength"));
            if (xe.HasAttribute("TickFontColor"))
                receiver.FontColor  = ConvertToColor(xe.GetAttribute("TickFontColor"));
            if (xe.HasAttribute("LineColor"))
                receiver.LineColor  = ConvertToColor(xe.GetAttribute("LineColor"));
            if (xe.HasAttribute("TickColor"))
                receiver.TickColor  = ConvertToColor(xe.GetAttribute("TickColor"));
            if (xe.HasAttribute("SectionStart"))
                receiver.SectionStart = Convert.ToInt32(xe.GetAttribute("SectionStart"));
            if (xe.HasAttribute("SectionEnd"))
                receiver.SectionEnd = Convert.ToInt32(xe.GetAttribute("SectionEnd"));
            if (xe.HasAttribute("TickStart"))
                receiver.TickStart  = Convert.ToInt32(xe.GetAttribute("TickStart"));
            if (xe.HasAttribute("TickEnd"))
                receiver.TickEnd  = Convert.ToInt32(xe.GetAttribute("TickEnd"));
            if (xe.HasAttribute("TextLoc"))
                receiver.TextLoc  = Convert.ToInt32(xe.GetAttribute("TextLoc"));
            if (xe.HasAttribute("MaxTick"))
                receiver.MaxTick  = Convert.ToDouble  (xe.GetAttribute("MaxTick"));
            if (xe.HasAttribute("MinTick"))
                receiver.MinTick = Convert.ToDouble(xe.GetAttribute("MinTick"));
            if (xe.HasAttribute("GaugeColor"))
                receiver.GaugeColor  = ConvertToColor(xe.GetAttribute("GaugeColor"));
            if (xe.HasAttribute("SemiCircle"))
                receiver.bSemiCircle  = Boolean.Parse(xe.GetAttribute("SemiCircle"));
        }

        protected void IGroupHeaderFrom(XmlElement xe, IGroupHeader receiver)
        {
            if (xe.HasAttribute("bShowNullGroup"))
                receiver.bShowNullGroup = Boolean.Parse(xe.GetAttribute("bShowNullGroup"));
            if (xe.HasAttribute("bHiddenGroup"))
                receiver.bHiddenGroup = Boolean.Parse(xe.GetAttribute("bHiddenGroup"));
            if (xe.HasAttribute("bAloneLine"))
                receiver.bAloneLine = Boolean.Parse(xe.GetAttribute("bAloneLine"));
        }

        protected void IAutoSequenceFrom(XmlElement xe, IAutoSequence receiver)
        {
            if (xe.HasAttribute("bAutoSequence"))
                receiver.bAutoSequence = Boolean.Parse(xe.GetAttribute("bAutoSequence"));
        }

        protected void IInformationSenderFrom(XmlElement xe, IInformationSender  receiver)
        {
            if (xe.HasAttribute("InformationID"))
                receiver.InformationID = xe.GetAttribute("InformationID");
        }
        protected void IGroupDimensionStyleFrom(XmlElement xe, IGroupDimensionStyle receiver)
        {
            if (xe.HasAttribute("UseColumnStyle"))
                receiver.UseColumnStyle = Boolean.Parse(xe.GetAttribute("UseColumnStyle"));
        }
        #endregion

        #region detail 
        public  override SectionType DetailType
        {
            get { return SectionType.Detail; }
        }
        protected override void AddDefaultDetail()
        {
            _report.Sections.Add(new Detail());
        }
        #endregion

    }

    public class IndicatorIntepretor : FreeInterpretor
    {
        public IndicatorIntepretor(Report report, DataHelper datahelper)
            : base(report, datahelper)
        {
        }

        protected override void ConvertFrom(XmlElement xe, Cell receiver)
        {
            base.ConvertFrom(xe, receiver);
        }

        protected override Section AddALoacleSection(XmlElement xec)
        {
            Section section = null;
            string type = xec.GetAttribute("Type");
            switch (type.ToLower())
            {
                case "pageheader":
                    section = new PageHeader();
                    AddOtherControl(xec, section);
                    break;
                case "pagefooter":
                    section = new PageFooter();
                    AddOtherControl(xec, section);
                    break;
                case "indicatordetail":
                    section = new IndicatorDetail ();
                    AddControl(xec, section);
                    break;
                case "reportheader":
                    section = new ReportHeader();
                    AddControl(xec, section);
                    break;
            }
            if (section != null)
            {
                ConvertFromLocaleInfo(xec, section);
                _report.Sections.Add(section);
            }
            return section;
        }

        protected override void HandleReportHeader(XmlElement xec, string name)
        {
            if (name == "IndicatorDetail")
            {
                Section header = _report.Sections[SectionType.ReportHeader];
                if (header != null)
                    AddHeaderFromCommonSection(xec, header);
            }
        }
        private void AddHeaderFromCommonSection(XmlElement xec, Section section)
        {
            ConvertFrom(xec, section);
            Cells cells = section.Cells;
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                Cell cell = cells[xecc.GetAttribute("Name")];
                if (cell != null)
                    ConvertFrom(xecc, cell);
            }
        }
        
        protected override Cell AddALocaleCell(XmlElement xecc, Section section)
        {
            string type = xecc.GetAttribute("Type");
            if (!section.CanBeParent(type))
                return null;
            Cell cell = IndicatorControl(type);
            if (cell != null)
            {
                ConvertFromLocaleInfo(xecc, cell);
                section.Cells.Add(cell);
            }
            return cell;
        }

        private void AddControl(XmlElement xec, Section section)
        {
            Section header = null;
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type=xecc.GetAttribute("Type").ToLower();
                Cell cell = IndicatorControl(type);
                if (cell == null)
                    continue;
                ConvertFromLocaleInfo(xecc, cell);
                if(section.SectionType!= SectionType.ReportHeader && (type == "commonlabel" || type == "expression" || type=="dbtext"))
                {
                    header = _report.Sections[SectionType.ReportHeader];
                    if (header == null)
                    {
                        header = new ReportHeader();
                        header.Height = 140;
                        _report.Sections.Add(header);
                    }
                    header.Cells.Add(cell);
                }
                else
                    section.Cells.Add(cell);
            }
        }

        private Cell IndicatorControl(string type)
        {
            Cell cell = null;
            switch (type.ToLower())
            {
                case "commonlabel":
                    cell = new CommonLabel();
                    break;
                case "image":
                    cell = new Image();
                    break;
                case "expression":
                    cell = new Expression();
                    break;
                case "groupdimension":
                    cell = new GroupDimension ();
                    break;
                case "calculategroupdimension":
                    cell = new CalculateGroupDimension();
                    break;
                case "crossdimension":
                    cell = new CrossDimension ();
                    break;
                case "calculatecrossdimension":
                    cell = new CalculateCrossDimension();
                    break;
                case "indicator":
                    cell = new Indicator();
                    break;
                case "calculateindicator":
                    cell = new CalculateIndicator();
                    break;
                case "chart":
                    cell = new Chart();
                    break;
                case "gauge":
                    cell = new Gauge();
                    break;
                case "indicatormetrix":
                    cell = new IndicatorMetrix();
                    break;
                case "dbtext":
                    cell = new DBText();
                    break;
                case "calculatorindicator":
                    cell = new CalculatorIndicator();
                    break;
                case "gridproportiondecimalindicator":
                    cell = new GridProportionDecimalIndicator();
                    break;
            }
            return cell;
        }

        protected override void DeleteDudentSection(System.Collections.ArrayList alsections)
        {
            int index = _report.Sections.Count - 1;
            while (index >= 0)
            {
                Section s = _report.Sections[index];
                if (!alsections.Contains(s) && s.Type!="ReportHeader")
                    _report.Sections.RemoveAt(index);
                index--;
            }
        }

        #region detail
        public override SectionType DetailType
        {
            get { return SectionType.IndicatorDetail ; }
        }
        protected override void AddDefaultDetail()
        {
            _report.Sections.Add(new IndicatorDetail ());
        }
        #endregion
    }
}
