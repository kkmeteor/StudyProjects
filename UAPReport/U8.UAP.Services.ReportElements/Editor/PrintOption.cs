using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public enum PrintProvider
    {
        U8PrintComponent,
        UAPReportPrintComponent
    }

    public enum ReportHeaderPrintOption
    {
        FirstPage,
        EveryPage,
        NotPint
    }

    [TypeConverterAttribute(typeof(PrintOptionTypeConverter))]
    [Serializable]
    public class PrintOption : DisplyTextCustomTypeDescriptor
    {
        private ReportHeaderPrintOption _headeroption = ReportHeaderPrintOption.FirstPage;
        private PrintProvider _provider = PrintProvider.U8PrintComponent;
        private bool _selectproviderruntime = true;
        private int _fixedrowsperpage = 0;

        [DisplayText("U8.Report.HeaderPrintOption")]
        [LocalizeDescription("U8.Report.HeaderPrintOption")]
        public ReportHeaderPrintOption HeaderPrintOption
        {
            get
            {
                return _headeroption;
            }
            set
            {
                _headeroption = value;
            }
        }

        [DisplayText("U8.Report.PrintProvider")]
        [LocalizeDescription("U8.Report.PrintProvider")]
        public PrintProvider PrintProvider
        {
            get
            {
                return _provider;
            }
            set
            {
                _provider = value;
            }
        }

        [DisplayText("U8.Report.CanSelectProvider")]
        [LocalizeDescription("U8.Report.CanSelectProvider")]
        public bool CanSelectProvider
        {
            get
            {
                return _selectproviderruntime;
            }
            set
            {
                _selectproviderruntime = value;
            }
        }

        [DisplayText("U8.Report.FixedRowsPerPage")]
        [LocalizeDescription("U8.Report.FixedRowsPerPage")]
        public int FixedRowsPerPage
        {
            get
            {
                return _fixedrowsperpage;
            }
            set
            {
                _fixedrowsperpage = (value<=0?0:value);
            }
        }
    }

    public class PrintOptionTypeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(PrintOption ))
                return true;

            return base.CanConvertTo(context, destinationType);

        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                value is PrintOption )
            {
                return "";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                return ((Report)context.Instance).PrintOption;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
