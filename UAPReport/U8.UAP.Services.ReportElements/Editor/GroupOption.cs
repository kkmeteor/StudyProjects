using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public enum SolidGroupStyle
    {
        FixOnFirstGroup,
        FixOnLastGroup
    }

    [TypeConverterAttribute(typeof(GroupOptionTypeConverter))]
    [Serializable]
    public class GroupOption : DisplyTextCustomTypeDescriptor
    {
        private bool _bpagebygroup = false;
        private string _solidgroup="";
        private SolidGroupStyle _solidstyle = SolidGroupStyle.FixOnFirstGroup ;

        [DisplayText("U8.UAP.Services.ReportElements.Dis37")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis37")]
        public bool PageByGroup
        {
            get
            {
                return _bpagebygroup  ;
            }
            set
            {
                _bpagebygroup   = value;
            }
        }

        [DisplayText("U8.Report.SolidGroup")]
        [LocalizeDescription("U8.Report.SolidGroup")]
        public string SolidGroup
        {
            get
            {
                return _solidgroup ;
            }
            set
            {
                _solidgroup   = value;
            }
        }

        [DisplayText("U8.Report.SolidStyle")]
        [LocalizeDescription("U8.Report.SolidStyle")]
        public SolidGroupStyle SolidGroupStyle
        {
            get
            {
                return _solidstyle  ;
            }
            set
            {
                _solidstyle   = value;
            }
        }
    }

    public class GroupOptionTypeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(GroupOption ))
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
                value is GroupOption )
            {
                return "";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                return ((Report)context.Instance).GroupOption;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
