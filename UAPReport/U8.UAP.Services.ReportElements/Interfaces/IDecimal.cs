using System;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Threading;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// IDecimal ��ժҪ˵����
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
        [EnumItemDescAttribute("zh-CN", "��"),
       EnumItemDescAttribute("zh-TW", "�o"),
       EnumItemDescAttribute("en-US", "None")]
        None,
        [EnumItemDescAttribute("zh-CN", "���"),
       EnumItemDescAttribute("zh-TW", "���~"),
       EnumItemDescAttribute("en-US", "Money")]
        Money,                    //���
        [EnumItemDescAttribute("zh-CN", "����"),
       EnumItemDescAttribute("zh-TW", "����"),
       EnumItemDescAttribute("en-US", "Weight")]
        Weight,                   //����
        [EnumItemDescAttribute("zh-CN", "���"),
       EnumItemDescAttribute("zh-TW", "�w�e"),
       EnumItemDescAttribute("en-US", "Volume")]
        Volume,                   //���
        [EnumItemDescAttribute("zh-CN", "����"),
       EnumItemDescAttribute("zh-TW", "����"),
       EnumItemDescAttribute("en-US", "Rate")]
        Rate,                     //����
        [EnumItemDescAttribute("zh-CN", "����"),
       EnumItemDescAttribute("zh-TW", "����"),
       EnumItemDescAttribute("en-US", "Quantity")]
        Quantity,                  //����
        [EnumItemDescAttribute("zh-CN", "����"),
       EnumItemDescAttribute("zh-TW", "����"),
       EnumItemDescAttribute("en-US", "PieceNum")]
        PieceNum,                 //����
        [EnumItemDescAttribute("zh-CN", "������"),
       EnumItemDescAttribute("zh-TW", "�Q����"),
       EnumItemDescAttribute("en-US", "ExchangeRate")]
        ExchangeRate,             //������
        [EnumItemDescAttribute("zh-CN", "˰��"),
       EnumItemDescAttribute("zh-TW", "����"),
       EnumItemDescAttribute("en-US", "TaxRate")]
        TaxRate,                  //˰��
        [EnumItemDescAttribute("zh-CN", "�������"),
       EnumItemDescAttribute("zh-TW", "��؛�΃r"),
       EnumItemDescAttribute("en-US", "InventoryPrice")]
        InventoryPrice,//�������
        [EnumItemDescAttribute("zh-CN", "�ɱ����"),
       EnumItemDescAttribute("zh-TW", "�ɱ����~"),
       EnumItemDescAttribute("en-US", "CostMoney")]
        CostMoney,//�ɱ����
        [EnumItemDescAttribute("zh-CN", "�ɱ�����"),
       EnumItemDescAttribute("zh-TW", "�ɱ�����"),
       EnumItemDescAttribute("en-US", "CostQuantity")]
        CostQuantity,//�ɱ�����
        [EnumItemDescAttribute("zh-CN", "��Ʊ����"),
       EnumItemDescAttribute("zh-TW", "�_Ʊ�΃r"),
       EnumItemDescAttribute("en-US", "BillPrice")]
        BillPrice,                 //��Ʊ����
        [EnumItemDescAttribute("zh-CN", "����Դ"),
       EnumItemDescAttribute("zh-TW", "����Դ"),
       EnumItemDescAttribute("en-US", "Source")]
        Source,                    //����Դȷ��
        [EnumItemDescAttribute("zh-CN", "��������"),
       EnumItemDescAttribute("zh-TW", "��������"),
       EnumItemDescAttribute("en-US", "GLQuantity")]
        GLQuantity,//��������
        [EnumItemDescAttribute("zh-CN", "���˵���"),
       EnumItemDescAttribute("zh-TW", "�����΃r"),
       EnumItemDescAttribute("en-US", "GLPrice")]
        GLPrice     //���˵���
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
                    //���û�����ö�����Ϣ,�Լ���һЩ������Ϣ
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