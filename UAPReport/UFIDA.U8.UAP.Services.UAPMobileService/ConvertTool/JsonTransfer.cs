using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using UFIDA.U8.UAP.Services.ReportElements;
using System.Collections;
using System.Xml;
using System.Web.Script.Serialization;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public class JsonTransfer
    {

        public static string ReadJson(string jsonText, string param = "")
        {
            //jsonText = @"{""input"":""value"",""output"":""result""}";
            JsonTextReader reader = new JsonTextReader(new StringReader(jsonText));
            JObject obj = JObject.Parse(jsonText);
            return obj[param].ToString();
            //return reader.ValueType.ToString() + reader.Value.ToString();
        }
        public static string[] ParseJsonToStrings(string jsonText)
        {
            jsonText = @"{""input"" : ""value"", ""output"" : ""result""}";
            JObject jo = JObject.Parse(jsonText);
            string[] values = jo.Properties().Select(item => item.Value.ToString()).ToArray();
            return values;
        }
        /// <summary>
        /// 把dataset数据转换成json的格式
        /// </summary>
        /// <param name="ds">dataset数据集</param>
        /// <returns>json格式的字符串</returns>
        public static string GetJsonByDataset(DataSet ds)
        {
            if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
            {
                //如果查询到的数据为空则返回标记ok:false
                return "{\"ok\":false}";
            }
            var sb = new StringBuilder();
            sb.Append("{\"ok\":true,");
            foreach (DataTable dt in ds.Tables)
            {
                sb.Append(string.Format("\"{0}\":[", dt.TableName));

                foreach (DataRow dr in dt.Rows)
                {
                    sb.Append("{");
                    for (int i = 0; i < dr.Table.Columns.Count; i++)
                    {
                        sb.AppendFormat("\"{0}\":\"{1}\",", dr.Table.Columns[i].ColumnName.Replace("\"", "\\\"").Replace("\'", "\\\'"), ObjToStr(dr[i]).Replace("\"", "\\\"").Replace("\'", "\\\'")).Replace(Convert.ToString((char)13), "\\r\\n").Replace(Convert.ToString((char)10), "\\r\\n");
                    }
                    sb.Remove(sb.ToString().LastIndexOf(','), 1);
                    sb.Append("},");
                }

                sb.Remove(sb.ToString().LastIndexOf(','), 1);
                sb.Append("],");
            }
            sb.Remove(sb.ToString().LastIndexOf(','), 1);
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 将object转换成为string
        /// </summary>
        /// <param name="ob">obj对象</param>
        /// <returns></returns>
        public static string ObjToStr(object ob)
        {
            if (ob == null)
            {
                return string.Empty;
            }
            else
                return ob.ToString();
        }


        internal static string ReportListToJson(MA.Component.Framework.ActionResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"resresult\":{");
            sb.Append("\"flag\": \"0\",");
            sb.Append("\"desc\": \"查询报表列表信息成功！\",");
            sb.Append("\"resdata\": ");
            sb.Append(string.Format("{0}", result.ResultData));
            sb.Append("}}");
            return sb.ToString();
            //return string.Format(@"{""resresult"": {""flag"": "{{0}}",""desc"": "{1}",""resdata"": "{{2}}"}",result.Flag,result.Description,result.ResultData);
        }

        /// <summary>
        /// 将移动报表对象列信息装换为json格式字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string TransferToTableSchema(object obj)
        {
            string result = "";
            bool _isMutiReport = false;
            var jsonsb = new StringBuilder();
            var cellList = new List<Cell>();
            var mobileReport = obj as MobileReport;
            var semirows = new SemiRows();
            var supperCellList = new List<SuperCell>();
            var childCellList = new List<Cell>();
            Hashtable ht = new Hashtable();
            string headerRow = "1";
            if (mobileReport != null)
            {
                Report report = mobileReport.Report;
                if (report != null)
                {
                    // 这里取pageTitle的一个对象来组成报表标题区域的值
                    Section section = report.Sections[SectionType.PageTitle];
                    foreach (object gridDetailCell in section.Cells)
                    {
                        var cell = gridDetailCell as Cell;
                        if (cell != null && cell.Visible)
                        {
                            if (cell is SuperLabel)
                            {
                                headerRow = "2";
                                _isMutiReport = true;
                                supperCellList.Add(new SuperCell(cell, 0, 0));
                            }
                            cellList.Add(gridDetailCell as Cell);
                            ht.Add(cell.Name, cell.Caption);
                        }
                    }
                    if (_isMutiReport) //如果出现多表头，则从Detail中取得子列头，并且进行拼接处理
                    {
                        cellList = new List<Cell>();
                        section = report.Sections[SectionType.Detail];
                        foreach (object gridDetailCell in section.Cells)
                        {
                            var cell = gridDetailCell as Cell;
                            if (cell.Visible)
                            {
                                if (ht.Contains(cell.Name))
                                    cell.Caption = ht[cell.Name].ToString();
                                cellList.Add(gridDetailCell as Cell);
                            }
                        }
                        foreach (SuperCell cell in supperCellList)
                        {
                            foreach (Cell gridDetailCell in cellList)
                            {
                                if (gridDetailCell.Super != null && gridDetailCell.Super.Name == cell.Cell.Name)
                                {
                                    cell.Span += 1;
                                    cell.ChildList.Add(gridDetailCell);
                                }
                            }
                        }
                    }
                    //jsonsb.Append(
                    //    string.Format(
                    //        @"<table id=""report"" width=""fill"" height=""fill"" bindfield=""sss"" column=""{0}"" onDownRefresh=""nextPage""",
                    //        cellList.Count));
                    //jsonsb.Append(
                    //    string.Format(
                    //        @" fixedColumn=""{0}"" headerRow=""{1}"" columnWidth=""60dp"" rowHeight=""50dp"" style=""default"">",
                    //        1, headerRow));
                    ////dslsb.Append("\n");
                    //jsonsb.Append("<property>");
                    // 下面代码是用来生成报表列的样式

                    #region 详细描述行列

                    jsonsb.Append("<columns>");
                    var sort = new SortedList();
                    foreach (Cell cell in cellList)
                    {
                        sort.Add(cell.Location.X, cell);
                    }

                    int i = 0;
                    foreach (Cell cell in sort.Values)
                    {
                        var tc = new TitleColumn(cell);
                        tc.Col = i.ToString();
                        jsonsb.Append(tc.ColumnsToXmlFormat());
                        foreach (SuperCell superCell in supperCellList)
                        {
                            if (superCell.ChildList.Contains(cell))
                            {
                                if (superCell.Col > i || superCell.Col == 0)
                                    superCell.Col = i;
                            }
                        }
                        i++;
                    }
                    //jsonsb.Append("</columns>");
                    if (_isMutiReport)
                    {
                        jsonsb.Append("<merges>");
                        bool isSucced = false;
                        //对每一个supper来说，col为该superCell的col值，而span为该superCell的子cell的个数
                        foreach (SuperCell superCell in supperCellList)
                        {
                            if (superCell.ChildList.Count != 0)
                            {
                                jsonsb.Append(string.Format(@"<merge row=""0""  col=""{0}""  span=""{1}""/>",
                                                           superCell.Col,
                                                           superCell.Span));
                                isSucced = true;
                            }
                        }
                        jsonsb.Append("</merges>");
                        if (!isSucced)
                        {
                            throw new Exception("该报表数据分组层级太多，移动端无法展现！");
                        }
                    }

                    #endregion

                    //jsonsb.Append("</property>");
                    //jsonsb.Append("</table>");
                    return jsonsb.ToString();
                }
            }
            return string.Empty;
            return result;
        }

        public static string ReferListToJson(string result)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"resresult\":{");
            sb.Append("\"flag\": \"0\",");
            sb.Append("\"desc\": \"调用成功！\",");
            sb.Append("\"resdata\": ");
            sb.Append(string.Format("{0}", result));
            sb.Append("}}");
            return sb.ToString();
        }

        /// <summary>  
        /// json字符串转换为Xml对象  
        /// </summary>  
        /// <param name="sJson"></param>  
        /// <returns></returns>  
        public static XmlDocument Json2Xml(string sJson)
        {
            //XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(sJson), XmlDictionaryReaderQuotas.Max);  
            //XmlDocument doc = new XmlDocument();  
            //doc.Load(reader);  

            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            Dictionary<string, object> Dic = (Dictionary<string, object>)oSerializer.DeserializeObject(sJson);
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDec;
            xmlDec = doc.CreateXmlDeclaration("1.0", "gb2312", "yes");
            doc.InsertBefore(xmlDec, doc.DocumentElement);
            XmlElement nRoot = doc.CreateElement("root");
            doc.AppendChild(nRoot);
            foreach (KeyValuePair<string, object> item in Dic)
            {
                string key = Transfer(item.Key);
                XmlElement element = doc.CreateElement(key);
                KeyValue2Xml(element, item);
                nRoot.AppendChild(element);
            }
            return doc;
        }

        public static string Transfer(string p)
        {
            string result = p.Replace("(", "LXX");
            result = result.Replace(")", "RXX");
            result = result.Replace(",", "DDH");
            return result;
        }

        internal static string VTransfer(string p)
        {
            string result = p.Replace("LXX", "(");
            result = result.Replace("RXX", ")");
            result = result.Replace("DDH", ",");
            return result;
        }

        private static void KeyValue2Xml(XmlElement node, KeyValuePair<string, object> Source)
        {
            object kValue = Source.Value;
            if (kValue.GetType() == typeof(Dictionary<string, object>))
            {
                foreach (KeyValuePair<string, object> item in kValue as Dictionary<string, object>)
                {
                    string key = Transfer(item.Key);
                    XmlElement element = node.OwnerDocument.CreateElement(key);
                    KeyValue2Xml(element, item);
                    node.AppendChild(element);
                }
            }
            else if (kValue.GetType() == typeof(object[]))
            {
                object[] o = kValue as object[];
                for (int i = 0; i < o.Length; i++)
                {
                    XmlElement xitem = node.OwnerDocument.CreateElement("Item");
                    KeyValuePair<string, object> item = new KeyValuePair<string, object>("Item", o[i]);
                    KeyValue2Xml(xitem, item);
                    node.AppendChild(xitem);
                }

            }
            else
            {
                XmlText text = node.OwnerDocument.CreateTextNode(kValue.ToString());
                node.AppendChild(text);
            }
        }

    }
}
