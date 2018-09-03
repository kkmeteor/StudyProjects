using System;
using System.Linq;
using System.Text;
using System.Globalization;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// 节点名格式化
    /// 功能：解决节点名中有非法字符情况
    /// 规则：
    /// 1、如果非字母开头，会加前缀SNODE_
    /// 2、字符只允许字母、数字、下划线
    /// 3、其他字符，进行Unicode转码，具体格式：_x002C_，如：逗号（，）,格式化为：_x002C_
    ///   遇到两个特殊字符，中间只有一个下划线，如两个逗号（,,）,转义为：_x002C_x002C_
    ///   最后一个字符为非法字符，则没有最后的下划线
    /// </summary>
    public sealed class NodeNameFormatter
    {
        /// <summary>
        /// 数字
        /// </summary>
        private static readonly char[] Numbers =  {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        /// <summary>
        /// 字母（大小写）
        /// </summary>
        private static readonly char[] Letters ={'a','b','c','d','e','f','g','h','i','g','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
        /// <summary>
        /// 字符
        /// </summary>
        private static readonly char[] Symbol = { '_'};
        /// <summary>
        /// 节点前缀
        /// </summary>
        private const string NODE_PREFIX = "SNODE_";
        /// <summary>
        /// 获取原始名
        /// </summary>
        /// <param name="formattedName">已格式化的名称</param>
        /// <returns>原始名</returns>
        public static string Decode(string formattedName)
        {
            //判定为空
            if (string.IsNullOrEmpty(formattedName))
            {
                return string.Empty;
            }
           
            //去掉节点前缀
            if (formattedName.StartsWith(NODE_PREFIX))
            {
                formattedName = formattedName.Substring(NODE_PREFIX.Length);
            }
            StringBuilder builder = new StringBuilder();
            string[] items = formattedName.Split('_');
            bool lastIsAllowed = true;
            for (int i = 0; i < items.Length; i++)
            {
                string item = items[i];
                if (item.StartsWith("x"))
                {
                    int number = 0;
                    if (int.TryParse(item.Substring(1), NumberStyles.HexNumber, null, out number))
                    {
                        lastIsAllowed = false;
                        builder.Append(Convert.ToChar(number));
                    }
                    else
                    {
                        builder.Append(item);
                    }
                }
                else
                {
                    if (i != 0 && lastIsAllowed)
                    {
                        builder.Append('_');
                    }
                    builder.Append(item);
                }
            }

            return builder.ToString();
        }
        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="nodeName">节点名</param>
        /// <returns>编码后的字符串</returns>
        public static string Encode(string nodeName)
        {
            return Encode(nodeName, "x", '_');
        }
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="text">输入字符串</param>
        /// <param name="prefix">前缀</param>
        /// <param name="splitChar">分隔符</param>
        /// <returns>编码后的字符串</returns>
        private static string Encode(string text, string prefix, char splitChar)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            bool lastIsAllowed = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                //如果第一个字符非字母则需要拼写前缀
                if (i == 0 && !CanStart(c))
                {
                    sb.Append(NODE_PREFIX);
                }

                var by = Encoding.Unicode.GetBytes(Convert.ToString(c));


                //如果是允许的字符，则过滤
                if (IsAllowed(c))
                {                    
                    sb.Append(c);
                    lastIsAllowed = true;
                    continue;
                }
                if (lastIsAllowed)
                {
                    sb.Append(splitChar);
                }
                sb.Append(prefix);
                sb.Append(by[1].ToString("X2"));
                sb.Append(by[0].ToString("X2"));
                //最后一个字符不加分隔符
                if (i != text.Length - 1)
                {
                    sb.Append(splitChar);
                }
                lastIsAllowed = false;
            }

            return sb.ToString();
        }
        /// <summary>
        /// 是否可以用此字符开头
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>bool</returns>
        private static bool CanStart(char c)
        {
            return Letters.Contains(c);
        }
        /// <summary>
        /// 是否合法
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>bool</returns>
        private static bool IsAllowed(char c)
        {
            return Numbers.Contains(c) || Letters.Contains(c) || Symbol.Contains(c);
        }        
    }
}
