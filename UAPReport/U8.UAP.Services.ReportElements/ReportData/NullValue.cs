using System;
using System.Text;
using System.Text.RegularExpressions;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// NullValue ��ժҪ˵����
	/// </summary>
	public class NullValue
	{
		public static string String
		{
			get
			{
				return string.Empty;
			}
		}

		public static bool Boolean
		{
			get
			{
				return false;
			}
		}

		public static double Decimal
		{
			get
			{
				return 0;
			}
		}

		public static DateTime DateTime
		{
			get
			{
                return new DateTime(1900, 1, 1);
			}
		}

		public static int Int
		{
			get
			{
				return 0;
			}
		}
	}

    public class ExpressionService
    {
        //������ʹ�á�[������]���������ڼ��ر��ʽʱ�滻����[���롰]������˱��ʽ�е�������֧�֡�[���롰]��
        //���ʽ�е�������֧�֡� ���롰����
        //���ʽ�е�������֧�֡�+������-������*������/��
        //case when �������>0 then ������� else null end
        //DATEPART ( datepart , date )
        //const��ֱ��֧�֣����Կ���ʹ��1+0�����ķ�ʽʵ��
        public static bool CheckIfATrueExpression(string expression)
        {
            bool btrue = (expression.Trim().IndexOfAny(Seperators) != -1);            
            return btrue;
        }

        public static bool IsADigit(string s)
        {
            foreach (char c in s)
            {
                if(!char.IsDigit(c))
                    return false;
            }

            return true;
        }
        private static bool IsNumber(string s)
        {
            try
            {
                decimal d = Convert.ToDecimal(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static  bool IsAConst(string s)
        {
            foreach (char c in s)
            {
                if (c == '\'')
                    return true;
                else if (!char.IsDigit(c))
                {
                    if (c == '.' && IsNumber(s))
                        continue;
                    else
                        return false;
                }
            }

            return true;
        }

        public static char[] Seperators
        {
            get
            {
                return new char[] { '(', ')', '+', '-', '*', '/', ' ', ',', '>', '<', '=' };
            }
        }

        public static bool bSpecialExpression(string expression)
        {
            #region special expression
            expression = expression.ToUpper();
            return expression.Contains("ISNULL(") ||
                expression.Contains("DATEADD(") ||
                expression.Contains("DATEDIFF(") ||
                expression.Contains("DATENAME(") ||
                expression.Contains("DATEPART(") ||
                expression.Contains("DAY(") ||
                expression.Contains("GETDATE(") ||
                expression.Contains("GETUTCDATE(") ||
                expression.Contains("MONTH(") ||
                expression.Contains("YEAR(") ||
                expression.Contains("ABS(") ||
                expression.Contains("DEGREES(") ||
                expression.Contains("RAND(") ||
                expression.Contains("ACOS(") ||
                expression.Contains("EXP(") ||
                expression.Contains("ROUND(") ||
                expression.Contains("ASIN(") ||
                expression.Contains("FLOOR(") ||
                expression.Contains("SIGN(") ||
                expression.Contains("ATAN(") ||
                expression.Contains("LOG(") ||
                expression.Contains("SIN(") ||
                expression.Contains("ATN2(") ||
                expression.Contains("LOG10(") ||
                expression.Contains("SQUARE(") ||
                expression.Contains("CEILING(") ||
                expression.Contains("PI(") ||
                expression.Contains("SQRT(") ||
                expression.Contains("COS(") ||
                expression.Contains("POWER(") ||
                expression.Contains("TAN(") ||
                expression.Contains("COT(") ||
                expression.Contains("RADIANS(") ||
                expression.Contains("ASCII(") ||
                expression.Contains("NCHAR(") ||
                expression.Contains("SOUNDEX(") ||
                expression.Contains("CHAR(") ||
                expression.Contains("PATINDEX(") ||
                expression.Contains("SPACE(") ||
                expression.Contains("CHARINDEX(") ||
                expression.Contains("REPLACE(") ||
                expression.Contains("STR(") ||
                expression.Contains("DIFFERENCE(") ||
                expression.Contains("QUOTENAME(") ||
                expression.Contains("STUFF(") ||
                expression.Contains("LEFT(") ||
                expression.Contains("REPLICATE(") ||
                expression.Contains("SUBSTRING(") ||
                expression.Contains("REVERSE(") ||
                expression.Contains("UNICODE(") ||
                expression.Contains("LOWER(") ||
                expression.Contains("RIGHT(") ||
                expression.Contains("UPPER(") ||
                expression.Contains("LTRIM(") ||
                expression.Contains("RTRIM(") ||
                expression.Contains("CONVERT(") ||
                expression.Contains("CAST(") ||
                expression.Contains("ACCOUNTMONTHCAPTION(") ||//����� add by yanghx2012-6-20
                (expression.Contains("CASE ") && expression.Contains(" WHEN "));
            #endregion
        }

        public static bool bExpressionReallyNotADecimalType(string expression, DataSources dss)
        {
            string[] expressions = SplitExpression(expression);
            foreach (string e in expressions)
            {
                if (e.Trim() != "" && dss.Contains(e.Trim()))
                {
                    DataSource ds = dss[e.Trim()];
                    if (ds.Type != DataType.Currency && ds.Type != DataType.Decimal && ds.Type != DataType.Int)
                        return true;
                }
                else if (e.Trim() != "")
                    return true;
            }
            return false;
        }

        public static bool bExpressionAfterCross(string expression)
        {
            return ((expression.Contains("@@@") && expression.EndsWith("+0"))
                || (expression.StartsWith("[") && expression.EndsWith("]+0"))
                || (expression.Contains("@@@") && expression.EndsWith("+''"))
                || (expression.StartsWith("[") && expression.EndsWith("]+''")));
        }

        public static string[] SplitExpression(string expression)
        {
            return expression.Split(Seperators );
        }

        public static string HandleInvalidDicmal(string value)
        {
            try
            {
                double d = Convert.ToDouble(value.Replace("%",""));
                if (double.IsNaN(d) || double.IsInfinity(d))
                    return "";
                if (value.ToLower().Contains("e"))
                {
                    value = decimal.Parse(value, System.Globalization.NumberStyles.AllowExponent |
                        System.Globalization.NumberStyles.AllowDecimalPoint).ToString();
                    //value = double.Parse(value, System.Globalization.NumberStyles.Float).ToString();
                        
                        
                }
            }
            catch
            {
            }
            return value;
        }


        public static string HandleNullDateTime(string value)
        {
            bool bnulldatetime = value.Contains("1900") && value.Contains(":00");
            return bnulldatetime ? "" : value.Replace(" 0:00:00","");
        }

        public static string HandleNullDateTimeSimple(string value)
        {
            bool bnulldatetime = value.Contains("1900");
            return bnulldatetime ? "" : value;
        }

        public static string HandleDivide(string expression,DataSources dsf)
        {
            expression = expression.Trim();
            if (expression.Contains(@"/"))
            {
                int index = expression.IndexOf("/");
                string expression1 = expression.Substring(0, index + 1).Trim();
                string expression2 = expression.Substring(index + 1).ToLower().Trim();
                if (expression2.StartsWith("("))
                {
                    char[] tmpbracket = expression2.ToCharArray();
                    int count = 0;
                    for (int i = 0; i < tmpbracket.Length; i++)
                    {
                        if (tmpbracket[i] == '(')
                            count++;
                        else if (tmpbracket[i] == ')')
                            count--;
                        if (count == 0)
                        {
                            index = i;
                            break;
                        }
                    }
                    if (count != 0)
                        throw new Exception("���ʽ���岻��ȷ�����Ų�ƥ�䡣");//need resource
                    expression2 = AddCase("(" + HandleDivide(expression2.Substring(1, index - 1), dsf)
                        + ")", dsf) + HandleDivide(expression2.Substring(index + 1), dsf);
                }
                else
                {
                    index = expression2.IndexOfAny(new char[] { '+', '-', '*', '/', ')',',' });
                    if (index != -1)
                    {
                        string expression3 = expression2.Substring(0, index).Trim();
                        expression2 = AddCase(expression3, dsf) + expression2.Substring(index, 1)
                            + HandleDivide(expression2.Substring(index + 1), dsf);
                    }
                    else
                        expression2 = AddCase(expression2,dsf);
                }
                expression = expression1 + expression2;
            }

            return expression;
        }

        private static string AddCase(string expression, DataSources dsf)
        {
            expression = expression.Trim();
            if (dsf.Contains(expression) || expression.StartsWith("("))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("(case when ");
                sb.Append(expression);
                sb.Append(" =0 then null else ");
                sb.Append(expression);
                sb.Append(" end)");
                expression = sb.ToString();
            }
            return expression;
        }
    }
}
