using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [TypeConverterAttribute(typeof(SortOptionTypeConverter))]
    [Serializable]
    public class SortOption : DisplyTextCustomTypeDescriptor
    {
        private SortDirection _direction = SortDirection.None;
        private int _priority = 0;

        [DisplayText("U8.UAP.Services.ReportElements.Dis15")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis15")]
        public SortDirection SortDirection
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value;
            }
        }

        [DisplayText("U8.Report.SortPriority")]
        [LocalizeDescription("U8.Report.SortPriority")]
        public int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = value;
            }
        }
    }

    public class SortOptionTypeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(SortOption ))
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
                value is SortOption )
            {
                return Enum.GetName(typeof(SortDirection),(value as SortOption).SortDirection);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                return ((ISort)context.Instance).SortOption ;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
