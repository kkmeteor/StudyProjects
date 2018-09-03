using System;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Threading;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// IDecimal 的摘要说明。
    /// </summary>
    public interface IDecimal : IFormat
    {
        PrecisionType Precision { get; set; }
        StimulateBoolean bShowWhenZero { get; set; }
        int PointLength { get; set; }
    }

    public enum StimulateBoolean
    {
        None,
        True,
        False
    }

    public enum PrecisionType
    {
        [EnumItemDescAttribute("zh-CN", "无"),
       EnumItemDescAttribute("zh-TW", "無"),
       EnumItemDescAttribute("en-US", "None")]
        None,
        [EnumItemDescAttribute("zh-CN", "金额"),
       EnumItemDescAttribute("zh-TW", "金額"),
       EnumItemDescAttribute("en-US", "Money")]
        Money,                    //金额
        [EnumItemDescAttribute("zh-CN", "重量"),
       EnumItemDescAttribute("zh-TW", "重量"),
       EnumItemDescAttribute("en-US", "Weight")]
        Weight,                   //重量
        [EnumItemDescAttribute("zh-CN", "体积"),
       EnumItemDescAttribute("zh-TW", "體積"),
       EnumItemDescAttribute("en-US", "Volume")]
        Volume,                   //体积
        [EnumItemDescAttribute("zh-CN", "比率"),
       EnumItemDescAttribute("zh-TW", "比率"),
       EnumItemDescAttribute("en-US", "Rate")]
        Rate,                     //比率
        [EnumItemDescAttribute("zh-CN", "数量"),
       EnumItemDescAttribute("zh-TW", "數量"),
       EnumItemDescAttribute("en-US", "Quantity")]
        Quantity,                  //数量
        [EnumItemDescAttribute("zh-CN", "件数"),
       EnumItemDescAttribute("zh-TW", "件數"),
       EnumItemDescAttribute("en-US", "PieceNum")]
        PieceNum,                 //件数
        [EnumItemDescAttribute("zh-CN", "换算率"),
       EnumItemDescAttribute("zh-TW", "換算率"),
       EnumItemDescAttribute("en-US", "ExchangeRate")]
        ExchangeRate,             //换算率
        [EnumItemDescAttribute("zh-CN", "税率"),
       EnumItemDescAttribute("zh-TW", "稅率"),
       EnumItemDescAttribute("en-US", "TaxRate")]
        TaxRate,                  //税率
        [EnumItemDescAttribute("zh-CN", "存货单价"),
       EnumItemDescAttribute("zh-TW", "存貨單價"),
       EnumItemDescAttribute("en-US", "InventoryPrice")]
        InventoryPrice,//存货单价
        [EnumItemDescAttribute("zh-CN", "成本金额"),
       EnumItemDescAttribute("zh-TW", "成本金額"),
       EnumItemDescAttribute("en-US", "CostMoney")]
        CostMoney,//成本金额
        [EnumItemDescAttribute("zh-CN", "成本数量"),
       EnumItemDescAttribute("zh-TW", "成本數量"),
       EnumItemDescAttribute("en-US", "CostQuantity")]
        CostQuantity,//成本数量
        [EnumItemDescAttribute("zh-CN", "开票单价"),
       EnumItemDescAttribute("zh-TW", "開票單價"),
       EnumItemDescAttribute("en-US", "BillPrice")]
        BillPrice,                 //开票单价
        [EnumItemDescAttribute("zh-CN", "数据源"),
       EnumItemDescAttribute("zh-TW", "數據源"),
       EnumItemDescAttribute("en-US", "Source")]
        Source,                    //数据源确定
        [EnumItemDescAttribute("zh-CN", "总账数量"),
       EnumItemDescAttribute("zh-TW", "總帳數量"),
       EnumItemDescAttribute("en-US", "GLQuantity")]
        GLQuantity,//总账数量
        [EnumItemDescAttribute("zh-CN", "总账单价"),
       EnumItemDescAttribute("zh-TW", "總帳單價"),
       EnumItemDescAttribute("en-US", "GLPrice")]
        GLPrice     //总账单价
    }
    public class PrecisionTypeConvertor : TypeConverter
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
            PrecisionType name = PrecisionType.None;
            if (value.GetType() == typeof(string))
            {
                string[] ps = Enum.GetNames(typeof(PrecisionType));
                foreach (string p in ps)
                {
                    if (EnumItemDescAttribute.GetDisplayValue(p, typeof(PrecisionType), Thread.CurrentThread.CurrentUICulture) == value.ToString())
                    {
                        name = (PrecisionType)Enum.Parse(typeof(PrecisionType), p);
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

                string[] ps = Enum.GetNames(typeof(PrecisionType));
                foreach (string p in ps)
                {
                    if (value.ToString() == p)
                    {
                        return EnumItemDescAttribute.GetDisplayValue(value.ToString(), typeof(PrecisionType), Thread.CurrentThread.CurrentUICulture);
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
            string[] ps = Enum.GetNames(typeof(PrecisionType));
            string[] names = new string[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                names[i] = EnumItemDescAttribute.GetDisplayValue(ps[i], typeof(PrecisionType), Thread.CurrentThread.CurrentUICulture);
            }
            System.ComponentModel.TypeConverter.StandardValuesCollection svc = new System.ComponentModel.TypeConverter.StandardValuesCollection(names);
            return svc;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnumItemDescAttribute : Attribute
    {
        private CultureInfo _culture;
        private string _description;

        public CultureInfo Culture
        {
            get { return _culture; }
        }

        public string Description
        {
            get { return _description; }
        }

        public EnumItemDescAttribute(CultureInfo culture, string description)
        {
            _culture = culture;
            _description = description;
        }

        public EnumItemDescAttribute(string cultureName, string description)
            : this(CultureInfo.GetCultureInfo(cultureName), description)
        {
        }
        public EnumItemDescAttribute(int cultureId, string description)
            : this(CultureInfo.GetCultureInfo(cultureId), description)
        {
        }

        private static readonly Dictionary<Type, Dictionary<object, Dictionary<CultureInfo, EnumItemDescAttribute>>> _cache =
            new Dictionary<Type, Dictionary<object, Dictionary<CultureInfo, EnumItemDescAttribute>>>();

        public static Dictionary<CultureInfo, EnumItemDescAttribute> GetDisplayValues(object value, Type enumType)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                throw new NotSupportedException("enumType is not a Enum");
            }
            if (value == null || !Enum.IsDefined(enumType, value))
            {
                throw new ArgumentException("value is not defined in " + enumType.FullName);
            }

            return GetDisplayValuesImp(value, enumType);
        }

        public static string GetDisplayValue(object value, Type enumType, CultureInfo culture)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                throw new NotSupportedException("enumType is not a Enum");
            }
            if (value == null || !Enum.IsDefined(enumType, value))
            {
                throw new ArgumentException("value is not defined in " + enumType.FullName);
            }

            if (culture == null)
            {
                return value.ToString();
            }

            Dictionary<CultureInfo, EnumItemDescAttribute> disc = GetDisplayValuesImp(value, enumType);
            if (disc.ContainsKey(culture))
                return disc[culture].Description;
            return value.ToString();
        }

        private static Dictionary<CultureInfo, EnumItemDescAttribute> GetDisplayValuesImp(object value, Type enumType)
        {
            if (!_cache.ContainsKey(enumType))
            {
                Dictionary<object, Dictionary<CultureInfo, EnumItemDescAttribute>> displayValues =
                    new Dictionary<object, Dictionary<CultureInfo, EnumItemDescAttribute>>();
                foreach (Enum item in Enum.GetValues(enumType))
                {
                    Dictionary<CultureInfo, EnumItemDescAttribute> descriptions =
                        new Dictionary<CultureInfo, EnumItemDescAttribute>();
                    FieldInfo enumField = enumType.GetField(item.ToString());
                    if (enumField != null)
                    {
                        foreach (EnumItemDescAttribute desc in enumField.GetCustomAttributes(typeof(EnumItemDescAttribute), true))
                        {
                            descriptions.Add(desc.Culture, desc);
                        }
                    }
                    //如果没有设置多语信息,自己加一些多语信息
                    if (descriptions.Count == 0)
                    {
                        descriptions.Add(CultureInfo.CurrentUICulture, new EnumItemDescAttribute(CultureInfo.CurrentUICulture, enumField.Name));
                    }

                    displayValues.Add(enumField.Name, descriptions);
                }
                _cache.Add(enumType, displayValues);
            }
            return _cache[enumType][value];
        }
    }
}