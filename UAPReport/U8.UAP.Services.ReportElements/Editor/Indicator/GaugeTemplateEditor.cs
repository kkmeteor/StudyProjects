using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class GaugeTemplateEditor : UITypeEditor
    {
        public GaugeTemplateEditor()
        {
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal ;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            // get the editor service.
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (edSvc == null)
            {
                // uh oh.
                return value;
            }
            if ((int)value < 1)
            {
                NormalGaugeDesign ngd = new NormalGaugeDesign(context.Instance as Gauge);
                if(ngd.ShowDialog()== DialogResult.OK)
                    return 1+(int)value;
            }
            //else
            //{
            //    TemplateGaugeDesign tgd = new TemplateGaugeDesign();
            //    tgd.TemplateIndex = (int)value;
            //    if(tgd.ShowDialog()== DialogResult.OK)
            //        return tgd.TemplateIndex;
            //}
            return value;
        }
    }

    public class GaugeTemplateTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(int))
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
                value is int)
            {
                return "";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                Gauge d = (Gauge)context.Instance;
                return d.TemplateIndex;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
