using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// 移动报表列对象
    /// </summary>
    public class TitleColumn
    {
        private string _caption;
        public string Caption
        {
            get
            {
                return @"" + this._caption + "";
            }
            set { this._caption = value; }
        }

        public string Name { get; set; }
        private string _col;
        public string Col
        {
            get
            {
                return @"" + this._col + "";
            }
            set { this._col = value; }
        }

        private string _row;
        public string Row
        {
            get
            {
                return @"" + this._row + "";
            }
            set
            {
                this._row = value;
            }
        }
        private string _width;
        public string Width
        {
            get
            {
                return @"" + this._width + "";
            }
            set { this._width = value; }
        }

        private string _backGroud;
        private ReportElements.Cell cell;


        public string BackGround
        {
            get
            {
                return @"" + this._backGroud + "";
            }
            set { this._backGroud = value; }
        }

        public TitleColumn(ReportElements.Cell cell)
        {

            this.cell = cell;
            this.Caption = cell.Caption;
            this.Name = cell.Name;
            this.BackGround = cell.BackColor.Name.ToUpper();
            this.Width = (Convert.ToInt32(cell.Width * 0.68)).ToString(CultureInfo.InvariantCulture);
            this.Width += "dp";
        }

        public string ColumnsToXmlFormat()
        {
            //return string.Format(@"<column col=""{0}"" width=""{1}""  background=""{2}""/>", Col, Width, BackGround);
            return string.Format(@"<column col=""{0}"" width=""{1}""/>", Col, Width);
        }

        public string ColumnCellToXmlFormat()
        {
            return string.Format(@"<column col=""{0}""  name=""{1}""  background=""{2}""/>\n", Col, Caption, BackGround);
        }
    }
}
