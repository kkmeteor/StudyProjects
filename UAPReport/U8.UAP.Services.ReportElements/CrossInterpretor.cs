using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class CrossInterpretor:GridInterpretor
    {
        #region constructor
        public CrossInterpretor(Report report, DataHelper datahelper)
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
                case "printpagetitle":
                    section = new PrintPageTitle();
                    AddOtherControl(xec, section);
                    break;
                case "printpagesummary":
                    section = new PrintPageSummary();
                    AddOtherControl(xec, section);
                    break;
                case "pageheader":
                    section = new PageHeader();
                    AddOtherControl(xec, section);
                    break;
                case "pagefooter":
                    section = new PageFooter();
                    AddOtherControl(xec, section);
                    break;
                case "reportheader":
                    section = new ReportHeader();
                    AddHeaderControl(xec, section);
                    break;
                case "crossdetail":
                    section = new CrossDetail();
                    AddDetailControl(xec, section);
                    break;
                case "crossrowheader":
                    section = new CrossRowHeader();
                    AddRowControl(xec, section);
                    break;
                case "crosscolumnheader":
                    section = new CrossColumnHeader();
                    AddColumnControl(xec, section);
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
            Cell cell = null;
            string type = xecc.GetAttribute("Type");
            if (!section.CanBeParent(type))
                return null;
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
                case "algorithmcalculator":
                    cell = new AlgorithmCalculator();
                    break;
                case "columnheader":
                    cell = new ColumnHeader();
                    break;
                case "calculatecolumnheader":
                    cell = new CalculateColumnHeader();
                    break;
                case "algorithmcolumnheader":
                    cell = new AlgorithmColumnHeader();
                    break;
                case "gridlabel":
                    cell = new GridLabel();
                    break;
                case "gridboolean":
                    cell = new GridBoolean();
                    break;
                case "gridimage":
                    //cell=new GridImage();
                    break;
                case "griddecimal":
                    cell = new GridDecimal();
                    break;
                case "gridcalculatecolumn":
                    cell = new GridCalculateColumn();
                    break;
                case "gridcolumnexpression":
                    cell = new GridColumnExpression();
                    break;
                case "griddecimalalgorithmcolumn":
                    cell = new GridDecimalAlgorithmColumn();
                    break;
                case "griddatetime":
                    cell = new GridDateTime();
                    break;
                case "gridexchangerate":
                    //cell=new GridExchangeRate();
                    break;
            }
            if (cell != null)
            {
                ConvertFromLocaleInfo(xecc, cell);
                section.Cells.Add(cell);
            }
            return cell;
        }

        protected override void ConvertFrom(XmlElement xe, Cell receiver)
        {
            base.ConvertFrom(xe, receiver);
            if (receiver is IMultiHeader)
                IMultiHeaderFrom(xe, receiver as IMultiHeader);
        }

        private void IMultiHeaderFrom(XmlElement xe, IMultiHeader receiver)
        {
            if (xe.HasAttribute("SortSource"))
            {
                DataSource ds = _report.DataSources[xe.GetAttribute("SortSource")];
                if (ds != null)
                    receiver.SortSource = ds;
                else
                {
                    receiver.SortSource = new DataSource("EmptyColumn");
                }
            }
        }

        private void AddOtherControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type").ToLower();
                if (type == "commonlabel" ||
                    type == "expression" ||
                    type == "image" ||
                    type == "algorithmcalculator")
                    AddALocaleCell(xecc, section);
            }
        }

        private void AddHeaderControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type").ToLower();
                if (type == "commonlabel" ||
                    type == "expression" ||
                    type == "chart" ||
                    type == "image" ||
                    type == "algorithmcalculator" ||
                    type == "gridlabel" ||
                    type == "gridboolean" ||
                    type == "gridimage" ||
                    type == "griddecimal" ||
                    type == "gridcalculatecolumn" ||
                    type == "gridcolumnexpression" ||
                    type == "griddecimalalgorithmcolumn" ||
                    type == "gridalgorithmcolumn" ||
                    type == "griddatetime" ||
                    type == "gridexchangerate")
                    AddALocaleCell(xecc, section);
            }
        }

        private void AddDetailControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type").ToLower();
                if (type == "griddecimal" ||
                    type == "gridlabel" ||
                    type == "gridcalculatecolumn" ||
                    type == "griddecimalalgorithmcolumn")
                    AddALocaleCell(xecc, section);
            }
        }

        private void AddColumnControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type").ToLower();
                if (type == "columnheader" ||
                    type == "calculatecolumnheader" ||
                    type == "algorithmcolumnheader")
                    AddALocaleCell(xecc, section);
            }
        }

        private void AddRowControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type").ToLower();
                if (type == "gridlabel" ||
                    type == "gridboolean" ||
                    type == "gridimage" ||
                    type == "griddecimal" ||
                    type == "gridcalculatecolumn" ||
                    type == "gridcolumnexpression" ||
                    type == "griddatetime" ||
                    type == "griddecimalalgorithmcolumn")
                    AddALocaleCell(xecc, section);
            }
        }
        #endregion

        #region detail
        protected override void AddDefaultDetail()
        {
            _report.Sections.Add(new CrossRowHeader());
            _report.Sections.Add(new CrossColumnHeader());
            _report.Sections.Add(new CrossDetail());
        }

        public  override SectionType DetailType
        {
            get
            {
                return SectionType.CrossRowHeader;
            }
        }
        #endregion

        public override void Interprete(XmlDocument localeformat, XmlDocument commonformat)
        {
            base.Interprete(localeformat, commonformat);
            
            if (_report.Sections[SectionType.CrossDetail].Cells.Count == 1)
            {
                string m_prepaintevent = _report.Sections[SectionType.CrossDetail].Cells[0].PrepaintEvent;
                if (m_prepaintevent.Trim().Length > 0)
                {
                    try
                    {
                    int m_count = _report.Sections[SectionType.CrossColumnHeader].Cells.Count-1;
                    _report.Sections[SectionType.CrossColumnHeader].Cells[m_count].PrepaintEvent = "";
                    _report.Sections[SectionType.CrossDetail].Cells[0].PrepaintEvent = "";
                    }
                    catch(Exception e)
                     {
                         ;
                    }
                }

            }

        }

    }
}
