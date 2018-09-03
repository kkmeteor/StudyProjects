using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// ICalculator ��ժҪ˵����
    /// </summary>
    public interface ICalculator
    {
        OperatorType Operator { get; set; }
        DataSource Unit { get; set; }
    }

    public enum OperatorType
    {//-1                                  
        [EnumItemDescAttribute("zh-CN", "�ۼ�ֵ"),
       EnumItemDescAttribute("zh-TW", "��Ӌֵ"),
       EnumItemDescAttribute("en-US", "SUM")]
        SUM = 0,
        [EnumItemDescAttribute("zh-CN", "ƽ��ֵ"),
       EnumItemDescAttribute("zh-TW", "ƽ��ֵ"),
       EnumItemDescAttribute("en-US", "AVG")]
        //ƽ��ֵ
        AVG,
        [EnumItemDescAttribute("zh-CN", "���ֵ"),
       EnumItemDescAttribute("zh-TW", "���ֵ"),
       EnumItemDescAttribute("en-US", "MAX")]
        //���ֵ
        MAX,
        [EnumItemDescAttribute("zh-CN", "��Сֵ"),
       EnumItemDescAttribute("zh-TW", "��Сֵ"),
       EnumItemDescAttribute("en-US", "MIN")]
        //��Сֵ
        MIN,
        [EnumItemDescAttribute("zh-CN", "���һ�е�ֵ"),
       EnumItemDescAttribute("zh-TW", "������һ�е�ֵ"),
       EnumItemDescAttribute("en-US", "BalanceSUM")]
        //���һ�е�ֵ
        BalanceSUM,
        [EnumItemDescAttribute("zh-CN", "��Ȩƽ��"),
       EnumItemDescAttribute("zh-TW", "�ә�ƽ��"),
       EnumItemDescAttribute("en-US", "ExpressionSUM")]
        //��Ȩƽ��
        ExpressionSUM,
        [EnumItemDescAttribute("zh-CN", "�ۼ����"),
       EnumItemDescAttribute("zh-TW", "��Ӌ���"),
       EnumItemDescAttribute("en-US", "AccumulateSUM")]
        //�ۼ����
        AccumulateSUM,
        [EnumItemDescAttribute("zh-CN", "���ӻ���"),
       EnumItemDescAttribute("zh-TW", "�}�s�R��"),
       EnumItemDescAttribute("en-US", "ComplexSUM")]
        //���ӻ���
        ComplexSUM,
    }

    public class OperatorTypeConvertor : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);

        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            OperatorType name = OperatorType.SUM;
            if (value.GetType() == typeof(string))
            {
                string[] ps = Enum.GetNames(typeof(OperatorType));
                foreach (string p in ps)
                {
                    if (EnumItemDescAttribute.GetDisplayValue(p, typeof(OperatorType), Thread.CurrentThread.CurrentUICulture) == value.ToString())
                    {
                        name = (OperatorType)Enum.Parse(typeof(OperatorType), p);
                        break;
                    }
                }
            }
            return name;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {

                string[] ps = Enum.GetNames(typeof(OperatorType));
                foreach (string p in ps)
                {
                    if (value.ToString() == p)
                    {
                        return EnumItemDescAttribute.GetDisplayValue(value.ToString(), typeof(OperatorType), Thread.CurrentThread.CurrentUICulture);
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);

        }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] ps = Enum.GetNames(typeof(OperatorType));
            string[] names = new string[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                names[i] = EnumItemDescAttribute.GetDisplayValue(ps[i], typeof(OperatorType), Thread.CurrentThread.CurrentUICulture);
            }
            System.ComponentModel.TypeConverter.StandardValuesCollection svc = new System.ComponentModel.TypeConverter.StandardValuesCollection(names);
            return svc;
        }
    }
}
