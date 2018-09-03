using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// ICalculator 的摘要说明。
    /// </summary>
    public interface ICalculator
    {
        OperatorType Operator { get; set; }
        DataSource Unit { get; set; }
    }

    public enum OperatorType
    {//-1                                  
        [EnumItemDescAttribute("zh-CN", "累计值"),
       EnumItemDescAttribute("zh-TW", "累計值"),
       EnumItemDescAttribute("en-US", "SUM")]
        SUM = 0,
        [EnumItemDescAttribute("zh-CN", "平均值"),
       EnumItemDescAttribute("zh-TW", "平均值"),
       EnumItemDescAttribute("en-US", "AVG")]
        //平均值
        AVG,
        [EnumItemDescAttribute("zh-CN", "最大值"),
       EnumItemDescAttribute("zh-TW", "最大值"),
       EnumItemDescAttribute("en-US", "MAX")]
        //最大值
        MAX,
        [EnumItemDescAttribute("zh-CN", "最小值"),
       EnumItemDescAttribute("zh-TW", "最小值"),
       EnumItemDescAttribute("en-US", "MIN")]
        //最小值
        MIN,
        [EnumItemDescAttribute("zh-CN", "最后一行的值"),
       EnumItemDescAttribute("zh-TW", "累最後一行的值"),
       EnumItemDescAttribute("en-US", "BalanceSUM")]
        //最后一行的值
        BalanceSUM,
        [EnumItemDescAttribute("zh-CN", "加权平均"),
       EnumItemDescAttribute("zh-TW", "加權平均"),
       EnumItemDescAttribute("en-US", "ExpressionSUM")]
        //加权平均
        ExpressionSUM,
        [EnumItemDescAttribute("zh-CN", "累计求和"),
       EnumItemDescAttribute("zh-TW", "累計求和"),
       EnumItemDescAttribute("en-US", "AccumulateSUM")]
        //累计求和
        AccumulateSUM,
        [EnumItemDescAttribute("zh-CN", "复杂汇总"),
       EnumItemDescAttribute("zh-TW", "複雜匯總"),
       EnumItemDescAttribute("en-US", "ComplexSUM")]
        //复杂汇总
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
