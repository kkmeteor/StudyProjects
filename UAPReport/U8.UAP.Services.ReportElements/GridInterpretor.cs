using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class GridInterpretor:BaseInterpretor
    {
        #region constructor
        public GridInterpretor(Report report, DataHelper datahelper)
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
                case "reportsummary":
                    bool baddwhendesign = false;
                    if (xec.HasAttribute("bAddWhenDesign"))
                    {
                        baddwhendesign = bool.Parse(xec.GetAttribute("bAddWhenDesign"));
                    }
                    if (!baddwhendesign)
                    {
                        section = new PageFooter();
                        AddOtherControl(xec, section);
                    }
                    else
                    {
                        section = new ReportSummary();
                        AddOtherControl(xec, section);
                        (section as ReportSummary).InitVisibleWidth();
                    }
                    section.Name = "";
                    break;
                case "griddetail":
                    section = new GridDetail();
                    AddDetailControl(xec, section);
                    break;
            }
            if (section != null)
            {
                ConvertFromLocaleInfo(xec, section);
                _report.Sections.Add(section);
                //section.Name = type;
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
                case "gridalgorithmcolumn":
                    cell = new GridAlgorithmColumn();
                    break;
                case "griddatetime":
                    cell = new GridDateTime();
                    break;
                case "gridexchangerate":
                    //cell=new GridExchangeRate();
                    break;
                case "superlabel":
                    cell = new SuperLabel(); 
                    break;
                case "gridproportiondecimal":
                    cell = new GridProportionDecimal();
                    break;
            }
            if (cell != null)
            {
                ConvertFromLocaleInfo(xecc, cell);
                section.Cells.Add(cell);
            }
            return cell;
        }

        private void AddOtherControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                string type = xecc.GetAttribute("Type").ToLower();
                if (type == "commonlabel" ||
                    type == "expression" ||
                    type == "chart" ||
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
                    type == "gridexchangerate" )
                    AddALocaleCell(xecc, section);
            }
        }

        private void AddDetailControl(XmlElement xec, Section section)
        {
            foreach (XmlElement xecc in xec.ChildNodes)//control
            {
                Cell cell = null;
                string type = xecc.GetAttribute("Type").ToLower();
                if (type == "gridlabel" ||
                    type == "gridboolean" ||
                    type == "gridimage" ||
                    type == "griddecimal" ||
                    type == "gridcalculatecolumn" ||
                    type == "gridcolumnexpression" ||
                    type == "griddecimalalgorithmcolumn" ||
                    type == "gridalgorithmcolumn" ||
                    type == "griddatetime" ||
                    type == "gridexchangerate" ||
                    type == "gridproportiondecimal" ||
                    type == "superlabel")
                    cell = AddALocaleCell(xecc, section);
                if (cell != null && _report.UnderState != ReportStates.Designtime)
                    cell.DefaultHeight();
            }
            //SetGroupCells(section);
        }
        #endregion

        #region detail 
        public override SectionType DetailType
        {
            get
            {
                return SectionType.GridDetail;
            }
        }

        private string HeaderLabel
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Title";
                    case "zh-TW":
                        return "˜Ëî}";
                    default:
                        return "±êÌâ";
                }
            }
        }

        protected override void AddDefaultDetail()
        {
            ReportHeader rh = new ReportHeader();
            Cell h = new CommonLabel(100, 40, 280, 80, HeaderLabel );
            h.Name = "HeaderLabel";
            (h as ICenterAlign).CenterAlign = true ;
            h.CaptionAlign = System.Drawing.ContentAlignment.MiddleCenter;
            h.ServerFont.FontSize = 20;
            h.ServerFont.FontName = "ºÚÌå";
            rh.Cells.Add(h);
            _report.Sections.Add(rh);

            GridDetail gd = new GridDetail();
            int left = 8;
            System.Collections.ICollection keys = _report.DataSources.DesignKeys;
            if (keys == null)
                keys = _report.DataSources.Keys;
            foreach (string key in keys )
            {
                DataSource ds = _report.DataSources[key];
                Cell cell = gd.GetDefaultRect(ds);
                if (cell != null)
                {
                    cell.X = left;
                    left += cell.Width;
                    if (_datahelper.bCusName(ds.Name))
                        cell.Visible = false;
                    cell.SetY(DefaultConfigs.SECTIONHEADERHEIGHT + 8);
                    gd.Cells.Add(cell);
                }
            }
            if (gd.Cells.Count > 0)
            {
                (gd as IAutoDesign).AutoDesign(DefaultConfigs.ReportLeft );
                _report.DesignWidth = gd.Width + 300;
            }
            _report.Sections.Add(gd);
            
        }       
        #endregion
               
    }
}
