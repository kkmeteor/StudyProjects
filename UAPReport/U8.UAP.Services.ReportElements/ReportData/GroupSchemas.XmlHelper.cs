using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data;
using UFIDA.U8.UAP.Services.ReportResource;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportData;


namespace UFIDA.U8.UAP.Services.ReportElements
{
    partial class GroupSchemas
    {
        private const string _xmlValueDefaultNoGroupId = "00000000-0000-0000-0000-000000000001";
        private const string _xmlValueZhCn = "zh-CN";
        private const string _xmlValueZhTw = "zh-TW";
        private const string _xmlValueEnUs = "en-US";
        private const string _xmlKeyID = "ID";
        private const string _xmlKeyIDLower = "id"; // Ϊ�˼�����������
        private const string _xmlKeybDefault = "bDefault";
        private const string _xmlKeyShowStyle = "ShowStyle";
        private const string _xmlKeyIsSavedByColumnSetting = "IsSavedByColumnSetting";
        private const string _xmlKeyMulitLangInfo = "MulitLangInfo";
        private const string _xmlKeyLang = "Lang";
        private const string _xmlKeyName = "Name";
        private const string _xmlKeyUnVisibleColumns = "UnVisibleColumns";
        private const string _xmlKeyLayouts = "Layouts";
        private const string _xmlKeySchemaItem = "SchemaItem";
        private const string _xmlKeyItem = "Item";
        private const string _xmlKeyGroupSchemas = "GroupSchemas";
        private const string _xmlKeyGroupSchema = "GroupSchema";
        private const string _xmlKeybShowDetial = "bShowDetail";
        private const string _xmlKeybGroupItemsAhead = "bGroupItemsAhead";
        private const string _xmlKeySwitchItem = "SwitchItem";
        private const string _xmlKeyVersion = "Version";
        private const string _xmlKeyLastUserGuid = "LastUserGuid";
        private const string _xmlKeyDateDimensions = "DateDimensions";
        private const string _xmlKeyDateDimension = "DateDimension";
        private const string _xmlKeyDateDimensionLevel = "Level";
        private const string _xmlKeyQuickSortItems = "QuickSortItems";
        private const string _xmlKeyQuickSortColumnItems = "QuickSortColumnItems";
        private const string _xmlKeyOrder = "Order";
        private const string _xmlKeyPriority = "Priority";
        private const string _xmlKeySortLevel = "Level";
        private const string _xmlKeyIsShowSubTotal = "IsShowSubTotal";
        private const string _xmlKeyCrossRowGroup = "CrossRowGroup";
        private const string _xmlKeyShowNullCrossColumn = "ShowNullCrossColumn";
        private const string _xmlKeyBShowHorizonTotal = "BShowHorizonTotal";

        private string NOGROUPID = "00000000-0000-0000-0000-000000000001";

        private static string[] _localeIds = new string[]{
			GroupSchemas._xmlValueZhCn,
			GroupSchemas._xmlValueZhTw,
			GroupSchemas._xmlValueEnUs,
		};

        //private static void AddSchemaTo(
        //    Report report,
        //    XmlElement xmlRoot,
        //    SectionType detailtype,
        //    string localeid )
        //{
        //    foreach( XmlElement ele in xmlRoot.ChildNodes )
        //    {
        //        GroupSchema gs = GroupSchemas.GetGroupSchemaFromXml(
        //            ele, 
        //            localeid );
        //        report.GroupSchemas.Add( gs );
        //        if( gs.bDefault )
        //            report.CurrentSchemaID = gs.ID ;
        //    }
        //}

        /// <summary>
        /// ��XML��GroupSchemaԪ�ع���һ���������
        /// </summary>
        private static GroupSchema GetGroupSchemaFromXml(
            XmlElement groupSchemaElement,
            string localeid)
        {
            GroupSchema gs = new GroupSchema();
            GroupSchemas.SetGroupSchemaProperty(localeid, gs, groupSchemaElement);

            int groupLevel = 0;
            foreach (XmlElement ele in groupSchemaElement.ChildNodes)
            {
                switch (ele.Name)
                {
                    case GroupSchemas._xmlKeyMulitLangInfo:
                        GroupSchemas.SetGroupSchemaName(ele, gs);
                        break;
                    case GroupSchemas._xmlKeySchemaItem:
                        groupLevel++;
                        gs.SchemaItems.Add(GroupSchemas.GetGroupSchemaItem(ele, groupLevel));
                        break;
                    case GroupSchemas._xmlKeyDateDimensions:
                        SetDateDimensions(ele, gs);
                        break;
                    case GroupSchemas._xmlKeyQuickSortItems:
                        SetQuickSortItems(ele, gs, false);
                        break;
                    case GroupSchemas._xmlKeyQuickSortColumnItems:
                        SetQuickSortItems(ele, gs, true);
                        break;
                    case GroupSchemas._xmlKeyCrossRowGroup:
                        gs.CrossRowGroup = GetGroupSchemaFromXml(ele, localeid);
                        break;
                    default:
                        break;
                }
            }
            return gs;
        }

        private static void SetGroupSchemaProperty(
            string localeid,
            GroupSchema gs,
            XmlElement groupSchemaElement)
        {
            gs.ID = GroupSchemas.GetAtrribute(groupSchemaElement, GroupSchemas._xmlKeyID);
            gs.bDefault = Boolean.Parse(GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeybDefault,
                "False"));
            string s = GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeybShowDetial,
                "False");
            if (s == "1")
                s = "True";
            else if (s == "0")
                s = "False";
            gs.bShowDetail = Boolean.Parse(s);
            //����Ƿ���ʾС��
            string showSubTotal = GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeyIsShowSubTotal);
            bool bShowSubTotal=false;
            if(!string.IsNullOrEmpty(showSubTotal))
            {
                if (showSubTotal.ToLower() == "true")
                    bShowSubTotal = true;
            }

            gs.bShowSubTotal =bShowSubTotal;
            gs.bGroupItemsAhead = Boolean.Parse(GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeybGroupItemsAhead,
                "True"));
            int showStyle = Convert.ToInt32(GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeyShowStyle,
                "2"));
            gs.ShowStyle = (ShowStyle)showStyle;
            if (gs.ShowStyle == ShowStyle.NoGroupSummary) //�۵�չ��Ĭ����ʾС�ƣ����۵���һ��Ԫ��
            {
                gs.bShowSubTotal = true;
            }
            gs.SwitchItem = GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeySwitchItem,
                "");
            //modifytime��ֵ
            gs.GuidVersion= GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeyVersion,
                "");
            gs.LastUserGuid = GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeyLastUserGuid,
                "");
            gs.bShowCrossNullColumn = Boolean.Parse(GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeyShowNullCrossColumn,
                "True"));
            gs.BShowHorizonTotal = Boolean.Parse(GroupSchemas.GetAtrribute(
                groupSchemaElement,
                GroupSchemas._xmlKeyBShowHorizonTotal,
                "True"));
        }

        private static string GetAtrribute(
            XmlElement ele,
            string atrrName)
        {
            return GroupSchemas.GetAtrribute(ele, atrrName, string.Empty);
        }

        private static string GetAtrribute(
            XmlElement ele,
            string atrrName,
            string valWhenEmpty)
        {
            XmlAttribute attr = ele.Attributes[atrrName];
            if (attr != null)
                return attr.Value;
            return valWhenEmpty;
        }

        private static GroupSchemaItem GetGroupSchemaItem(
            XmlElement ele,
            int groupLevel)
        {
            GroupSchemaItem gsi = new GroupSchemaItem();
            gsi.Level = groupLevel;
            foreach (XmlElement n in ele.ChildNodes)
            {
                switch (n.Name)
                {
                    case GroupSchemas._xmlKeyItem:
                        string cellName = n.GetAttribute(GroupSchemas._xmlKeyName);
                        gsi.Items.Add(cellName);
                        break;
                    default:
                        break;
                }
            }
            return gsi;
        }

        private static void SetDateDimensions(XmlElement xe, GroupSchema gs)
        {
            foreach (XmlElement n in xe.ChildNodes)
            {
                string key = n.GetAttribute(GroupSchemas._xmlKeyName);
                int value = Convert.ToInt32(n.GetAttribute(GroupSchemas._xmlKeyDateDimensionLevel));
                gs.DateDimensions.Add(key, value);
            }
        }

        private static void SetQuickSortItems(XmlElement xe, GroupSchema gs, bool isColumnSort)
        {

            if (xe == null)
                return;
            QuickSortSchema sortSchema = new QuickSortSchema();
            foreach (XmlElement n in xe.ChildNodes)
            {
                string key = n.GetAttribute(GroupSchemas._xmlKeyName);
                string order = n.GetAttribute(GroupSchemas._xmlKeyOrder);
                int level = Convert.ToInt32(n.GetAttribute(GroupSchemas._xmlKeySortLevel));
                int priority = Convert.ToInt32(n.GetAttribute(GroupSchemas._xmlKeyPriority));
                SortOption so = new SortOption();
                so.SortDirection = (SortDirection)Enum.Parse(typeof(SortDirection), order);
                so.Priority = priority;
                sortSchema.Add(key, so, level);
            }
            if (isColumnSort)
            {
                gs.ColumnSortSchema = sortSchema;
            }
            else
            {
                gs.SortSchema = sortSchema;
            }
        }

        private static void SetGroupSchemaName(
            XmlElement ele,
            GroupSchema gs)
        {
            foreach (string localeId in GroupSchemas._localeIds)
            {
                string path = string.Format(
                    "{0}[@{1}='{2}']",
                    GroupSchemas._xmlKeyLang,
                    GroupSchemas._xmlKeyIDLower,
                    localeId);
                XmlNode node = ele.SelectSingleNode(path);
                if (node != null)
                    gs.SetName(
                        localeId,
                        node.Attributes[GroupSchemas._xmlKeyName].Value);
            }
        }

        /// <summary>
        /// ����һ��"�޷���"�ķ������
        /// </summary>
        private static GroupSchema GetDefaultGroupSchema()
        {
            GroupSchema gs = new GroupSchema();
            gs.ID = GroupSchemas._xmlValueDefaultNoGroupId;
            gs.bDefault = false;
            //gs.CurrentLocaleId = localeid;
            gs.SetName(GroupSchemas._xmlValueZhCn, "�޷�����չ��");
            gs.SetName(GroupSchemas._xmlValueZhTw, "�o�ֽM�չ�F");
            gs.SetName(GroupSchemas._xmlValueEnUs, "No group item");
            gs.bShowDetail = true;
            return gs;
        }

        private static GroupSchema GetDefaultCrossSchema()
        {
            GroupSchema gs = new GroupSchema();
            gs.ID = GroupSchemas._xmlValueDefaultNoGroupId;
            gs.bDefault = false;
            //gs.CurrentLocaleId = localeid;
            gs.SetName(GroupSchemas._xmlValueZhCn, "�޽��淽��");
            gs.SetName(GroupSchemas._xmlValueZhTw, "�o���淽��");
            gs.SetName(GroupSchemas._xmlValueEnUs, "No cross schema");
            gs.bShowDetail = true;
            return gs;
        }

        public XmlDocument ToXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement(GroupSchemas._xmlKeyGroupSchemas);
            doc.AppendChild(root);
            for (int i = 0; i < this.Count; i++)
                this.AddGroupSchemaElement(this[i], doc, root);
            return doc;
        }

        private void InitSort(GroupSchema gs)
        {
            QuickSortSchema sortSchema = new QuickSortSchema();
            int Priority = 1;
            if (gs.SchemaItems.Count < 1)
                return;
            if (gs.SchemaItems.Count < 3)
            {
                foreach (GroupSchemaItem group in gs.SchemaItems)
                {
                    foreach (string name in group.Items)
                    {
                        QuickSortItem item = new QuickSortItem();
                        item.Level = group.Level;
                        item.Priority = Priority;
                        Priority++;
                        item.Name = name;
                        item.SortDirection = SortDirection.Descend;
                        sortSchema.QuickSortItems.Add(item);
                    }
                }
            }
            else
            {
                gs.SortSchema = sortSchema;
                GroupSchemaItem column = gs.SchemaItems[1];
                foreach (string name in column.Items)
                {
                    QuickSortItem item = new QuickSortItem();
                    item.Level = 2;
                    item.Priority = Priority;
                    Priority++;
                    item.Name = name;
                    item.SortDirection = SortDirection.Descend;
                    sortSchema.QuickSortItems.Add(item);
                }
                gs.ColumnSortSchema = sortSchema;
            }
        }

        private void AddGroupSchemaElement(
            GroupSchema gs,
            XmlDocument doc,
            XmlElement parent)
        {
           // InitSort(gs);
            XmlElement xeGs = this.AddSubElement(doc, parent, GroupSchemas._xmlKeyGroupSchema);
            xeGs.SetAttribute(GroupSchemas._xmlKeyID, gs.ID);
            xeGs.SetAttribute(GroupSchemas._xmlKeybDefault, gs.bDefault.ToString());
            xeGs.SetAttribute(GroupSchemas._xmlKeyIsShowSubTotal, gs.bShowSubTotal.ToString());
            xeGs.SetAttribute(GroupSchemas._xmlKeybShowDetial, gs.bShowDetail.ToString());
            xeGs.SetAttribute(GroupSchemas._xmlKeybGroupItemsAhead, gs.bGroupItemsAhead.ToString());
            xeGs.SetAttribute(GroupSchemas._xmlKeyShowStyle, Convert.ToString((int)gs.ShowStyle));
            xeGs.SetAttribute(GroupSchemas._xmlKeySwitchItem, gs.SwitchItem);
            xeGs.SetAttribute(GroupSchemas._xmlKeyVersion, gs.GuidVersion);
            xeGs.SetAttribute(GroupSchemas._xmlKeyLastUserGuid, gs.LastUserGuid);
            xeGs.SetAttribute(GroupSchemas._xmlKeyShowNullCrossColumn, gs.bShowCrossNullColumn.ToString());//�����Ƿ���ʾ
            xeGs.SetAttribute(GroupSchemas._xmlKeyBShowHorizonTotal, gs.BShowHorizonTotal.ToString());//�����Ƿ���ʾ
            this.AddMulitLangInfoElement(gs, doc, xeGs);
            this.AddSchemaItem(gs, doc, xeGs);
            //�����������
            this.AddSortItems(gs, doc, xeGs);
            this.AddColumnSortItems(gs, doc, xeGs);
            this.AddDateDimensions(doc, xeGs, gs);

            //��ӽ�����Ϣ CrossRowGroupĬ��ֵһ��ҪΪNULLҪ���������ѭ��
            if (gs.CrossRowGroup != null)
            {
                AddCrossRowGroup(gs.CrossRowGroup, doc, xeGs);
            } 
        }

        private void AddCrossRowGroup(GroupSchema gs, XmlDocument doc, XmlElement parent)
        {
            XmlElement xeGs = this.AddSubElement(doc, parent, GroupSchemas._xmlKeyCrossRowGroup);
            xeGs.SetAttribute(GroupSchemas._xmlKeyID, gs.ID);
            xeGs.SetAttribute(GroupSchemas._xmlKeybDefault, gs.bDefault.ToString());
            xeGs.SetAttribute(GroupSchemas._xmlKeyIsShowSubTotal, gs.bShowSubTotal.ToString());
            xeGs.SetAttribute(GroupSchemas._xmlKeybShowDetial, gs.bShowDetail.ToString());
            xeGs.SetAttribute(GroupSchemas._xmlKeybGroupItemsAhead, gs.bGroupItemsAhead.ToString());
            xeGs.SetAttribute(GroupSchemas._xmlKeyShowStyle, Convert.ToString((int)gs.ShowStyle));
            xeGs.SetAttribute(GroupSchemas._xmlKeySwitchItem, gs.SwitchItem);
            xeGs.SetAttribute(GroupSchemas._xmlKeyVersion, gs.GuidVersion);
            xeGs.SetAttribute(GroupSchemas._xmlKeyLastUserGuid, gs.LastUserGuid);
            this.AddMulitLangInfoElement(gs, doc, xeGs);
            this.AddSchemaItem(gs, doc, xeGs);
            //�����������
            this.AddSortItems(gs, doc, xeGs);
            this.AddColumnSortItems(gs, doc, xeGs);
            this.AddDateDimensions(doc, xeGs, gs);

        }

        /// <summary>
        /// �ѷ����������Ϣ��ӷ����� create by yanghx 2012-2-24
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="doc"></param>
        /// <param name="xeGs"></param>
        private void AddSortItems(GroupSchema gs, XmlDocument doc, XmlElement parent)
        {
            if (gs.SortSchema != null)
            {
                XmlElement xeSortItems = this.AddSubElement(doc, parent, GroupSchemas._xmlKeyQuickSortItems);
                foreach (QuickSortItem item in gs.SortSchema.QuickSortItems)
                {
                    XmlElement sort = this.AddSubElement(doc, xeSortItems, GroupSchemas._xmlKeyItem);
                    sort.SetAttribute(GroupSchemas._xmlKeyName, item.Name);
                    sort.SetAttribute(GroupSchemas._xmlKeyOrder, item.SortDirection.ToString());
                    sort.SetAttribute(GroupSchemas._xmlKeySortLevel, item.Level.ToString());
                    sort.SetAttribute(GroupSchemas._xmlKeyPriority, item.Priority.ToString());
                }
            }
        }

        /// <summary>
        /// ��ӽ����е�����
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="doc"></param>
        /// <param name="xeGs"></param>
        private void AddColumnSortItems(GroupSchema gs, XmlDocument doc, XmlElement parent)
        {
            if (gs.ColumnSortSchema != null)
            {
                XmlElement xeSortColumnItems = this.AddSubElement(doc, parent, GroupSchemas._xmlKeyQuickSortColumnItems);
                foreach (QuickSortItem item in gs.ColumnSortSchema.QuickSortItems)
                {
                    XmlElement sort = this.AddSubElement(doc, xeSortColumnItems, GroupSchemas._xmlKeyItem);
                    sort.SetAttribute(GroupSchemas._xmlKeyName, item.Name);
                    sort.SetAttribute(GroupSchemas._xmlKeyOrder, item.SortDirection.ToString());
                    sort.SetAttribute(GroupSchemas._xmlKeySortLevel, item.Level.ToString());
                    sort.SetAttribute(GroupSchemas._xmlKeyPriority, item.Priority.ToString());
                }
            }
        }

        private void AddDateDimensions(XmlDocument doc, XmlElement parent, GroupSchema gs)
        {
            XmlElement xe = this.AddSubElement(doc, parent, GroupSchemas._xmlKeyDateDimensions);
            foreach (string key in gs.DateDimensions.Keys)
            {
                XmlElement xec = this.AddSubElement(doc, xe, GroupSchemas._xmlKeyDateDimension);
                xec.SetAttribute(GroupSchemas._xmlKeyName, key);
                xec.SetAttribute(GroupSchemas._xmlKeyDateDimensionLevel, gs.DateDimensions[key].ToString());
            }
        }

        private XmlElement AddSubElement(
            XmlDocument doc,
            XmlElement parent,
            string subElementName)
        {
            return parent.AppendChild(
                doc.CreateElement(subElementName)) as XmlElement;
        }

        private void AddMulitLangInfoElement(
            GroupSchema gs,
            XmlDocument doc,
            XmlElement parent)
        {
            XmlElement xeMli = this.AddSubElement(doc, parent, GroupSchemas._xmlKeyMulitLangInfo);
            foreach (string localeid in GroupSchemas._localeIds)
                this.AddLangElement(doc, xeMli, localeid, gs);
        }

        private void AddLangElement(
            XmlDocument doc,
            XmlElement parent,
            string id,
            GroupSchema gs)
        {
            XmlElement xeL = this.AddSubElement(doc, parent, GroupSchemas._xmlKeyLang);
            xeL.SetAttribute(GroupSchemas._xmlKeyIDLower, id);
            xeL.SetAttribute(GroupSchemas._xmlKeyName, gs.GetNameWithEmpty(id));
        }

        private void AddSchemaItem(
            GroupSchema gs,
            XmlDocument doc,
            XmlElement parent)
        {
            for (int i = 0; i < gs.SchemaItems.Count; i++)
            {
                XmlElement xeSi = this.AddSubElement(doc, parent, GroupSchemas._xmlKeySchemaItem);
                this.AddSchemaItemItem(gs.SchemaItems[i], doc, xeSi);
            }
        }

        private void AddSchemaItemItem(
            GroupSchemaItem item,
            XmlDocument doc,
            XmlElement parent)
        {
            for (int i = 0; i < item.Items.Count; i++)
            {
                XmlElement xeSii = this.AddSubElement(doc, parent, GroupSchemas._xmlKeyItem);
                xeSii.SetAttribute(GroupSchemas._xmlKeyName, item.Items[i].ToString());
            }
        }

        #region �����õ�
        /// <summary>
        /// ��ȡĳһ�������ʱ�򣬼���������
        /// </summary>
        /// <param name="viewID"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        private XmlDocument GetGroupSchemasWithXlock(string viewID, SqlTransaction sqlTransaction)
        {
            string sql = string.Format("SELECT GroupSchemas FROM UAP_ReportView with(XLOCK) WHERE ID=N'{0}'", viewID);
            DataSet ds = SqlHelper.ExecuteDataSet(sqlTransaction, sql);
            string gs = ds.Tables[0].Rows[0]["GroupSchemas"].ToString();
            XmlDocument xml = new XmlDocument();
            if (!gs.Equals(string.Empty))
                xml.LoadXml(gs);
            return xml;
        }

        /// <summary>
        /// groupSchemas:ÿ�α���ʱֻ��һ������������version�ж��ǲ������µģ�����state�ж�����ɾ�ġ�
        /// </summary>
        /// <param name="viewID"></param>
        /// <param name="groupSchemas"></param>
        public string SaveGroupSchemasWithLock(U8LoginInfor login,string viewID, string groupSchemas, string actionType)
        {
            //1��һ���µ����ӣ���������
            SqlConnection cnn = new SqlConnection(login.UfMetaCnnString);
            cnn.Open();
            SqlTransaction tran = cnn.BeginTransaction();
            string result = string.Empty;
            try
            {
                //21 ���Ȼ�ȡ��ǰ���ݿ��groupSchemas��������Ҫ�����groupSchemas
                XmlDocument doc = this.GetGroupSchemasWithXlock(viewID, tran); //tran
                GroupSchemas haveSaveGroupSchemas = GroupSchemas.GetSchemas(doc, "");
                GroupSchemas toSaveGroupSchemas = GroupSchemas.GetSchemasNoSetDefaultSchema(groupSchemas, "");
                //3Ȼ��ÿһ��groupSchema���Ա�
                if (toSaveGroupSchemas.Count <= 0)
                    return toSaveGroupSchemas.ToXml().InnerXml;
                GroupSchema gs = toSaveGroupSchemas[0];//Ҫ���µ�
                foreach (GroupSchema group in toSaveGroupSchemas)
                {
                    if (group.ID != NOGROUPID)
                    {
                        gs = group;
                    }
                }
                GroupSchema oldGs = GetGroupSchemaById(haveSaveGroupSchemas, gs.ID);//���ݿ����Ѿ��е�
                switch (actionType.ToLower().Trim())
                {
                    case "delete":
                        if (oldGs == null)
                        {
                            throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Delete.Ex", login.LocaleID));
                            //throw new Exception(String4Report.GetString("����ʧ�ܣ������Ѿ�ɾ��"));
                        }
                        haveSaveGroupSchemas.Remove(oldGs);
                        break;
                    case "add":
                        if (HaveSameName(haveSaveGroupSchemas, gs))
                        {
                            throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Add.Ex", login.LocaleID));
                            //throw new Exception(String4Report.GetString("���������ķ���/���棬���ٴδ򿪱�������"));
                        }
                        gs.GuidVersion = Guid.NewGuid().ToString();
                        haveSaveGroupSchemas.Add(gs);
                        SetOtherSchemaBDefault(haveSaveGroupSchemas, gs);
                        break;
                    case "modify":
                        if (oldGs == null)
                        {
                            throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Modify.Ex1", login.LocaleID));
                            //throw new Exception(String4Report.GetString("����ʧ�ܣ������Ѿ�ɾ��"));
                        }
                        //����ͬһ���û��ڲ�������У�鲢��
                        if (string.IsNullOrEmpty(oldGs.LastUserGuid ) || oldGs.LastUserGuid == gs.LastUserGuid)
                        {
                            if (HaveSameName(haveSaveGroupSchemas, gs))
                            {
                                throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Modify.Ex2", login.LocaleID));
                                //throw new Exception(String4Report.GetString("���������ķ���/���棬���ٴδ򿪱�������"));
                            }
                            haveSaveGroupSchemas.Remove(oldGs);
                            gs.GuidVersion = Guid.NewGuid().ToString();
                            haveSaveGroupSchemas.Add(gs);
                            SetOtherSchemaBDefault(haveSaveGroupSchemas, gs);
                        }
                        //����ͬһ���û��ڲ�����У�鲢��
                        else
                        {
                            if (oldGs.GuidVersion == gs.GuidVersion)
                            {
                                if (HaveSameName(haveSaveGroupSchemas, gs))
                                {
                                    throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Modify.Ex2", login.LocaleID));
                                    //throw new Exception(String4Report.GetString("���������ķ���/���棬���ٴδ򿪱�������"));
                                }
                                haveSaveGroupSchemas.Remove(oldGs);
                                gs.GuidVersion = Guid.NewGuid().ToString();
                                haveSaveGroupSchemas.Add(gs);
                                SetOtherSchemaBDefault(haveSaveGroupSchemas, gs);
                            }
                            else
                            {
                                throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Modify.Ex3", login.LocaleID));
                                //throw new Exception(String4Report.GetString("����ʧ�ܣ������Ѿ��޸�"));
                            }
                        }
                        break;
                    default:
                        break;
                }
                result = haveSaveGroupSchemas.ToXml().InnerXml;
                string sql = "UPDATE UAP_ReportView SET GroupSchemas = @GroupSchemas WHERE ID=@ViewID";
                SqlCommand command = new SqlCommand(sql, cnn);
                command.Parameters.Add(new SqlParameter("GroupSchemas", result));
                command.Parameters.Add(new SqlParameter("ViewID", viewID));

                SqlHelper.ExecuteNonQuery(tran, command);
                tran.Commit();
            }
            catch (Exception e)
            {
                tran.Rollback();
                throw e;
            }
            finally
            {
                if (cnn.State == ConnectionState.Open)
                    cnn.Close();
            }
            return result;
        }

        private XmlDocument GetCrossSchemasWithXLock(string viewID, SqlTransaction sqlTransaction)
        {
            string sql = string.Format("SELECT PreservedField FROM UAP_ReportView with(XLOCK) WHERE ID=N'{0}'", viewID);
            DataSet ds = SqlHelper.ExecuteDataSet(sqlTransaction, sql);
            string gs = ds.Tables[0].Rows[0]["PreservedField"].ToString();
            XmlDocument xml = new XmlDocument();
            if (!gs.Equals(string.Empty))
                xml.LoadXml(gs);

            return xml;
        }

        public string SaveCrossSchemasWithLock(U8LoginInfor login,string viewID, string crossSchemas, string actionType)
        {
            //1��һ���µ����ӣ���������
            SqlConnection cnn = new SqlConnection(login.UfMetaCnnString);
            cnn.Open();
            SqlTransaction tran = cnn.BeginTransaction();
            string result = string.Empty;
            try
            {
                //21 ���Ȼ�ȡ��ǰ���ݿ��groupSchemas��������Ҫ�����groupSchemas
                XmlDocument doc = this.GetCrossSchemasWithXLock(viewID, tran); //tran
                GroupSchemas haveSaveCrossSchemas = GroupSchemas.GetSchemas(doc, "");
                GroupSchemas toSaveCrossSchemas = GroupSchemas.GetSchemasNoSetDefaultSchema(crossSchemas, "");
                //3Ȼ��ÿһ��groupSchema���Ա�
                if (toSaveCrossSchemas.Count <= 0)
                    return toSaveCrossSchemas.ToXml().InnerXml;
                GroupSchema cs = toSaveCrossSchemas[0];//Ҫ���µ�
                foreach (GroupSchema group in toSaveCrossSchemas)
                {
                    if (group.ID != NOGROUPID)
                    {
                        cs = group;
                    }
                }
                GroupSchema oldGs = GetGroupSchemaById(haveSaveCrossSchemas, cs.ID);//���ݿ����Ѿ��е�
                switch (actionType.ToLower().Trim())
                {
                    case "delete":
                        if (oldGs == null)
                        {
                            throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Delete.Ex", login.LocaleID));//"û��Ȩ��" 
                            //throw new Exception(String4Report.GetString("����ʧ�ܣ������Ѿ�ɾ��"));
                        }
                        haveSaveCrossSchemas.Remove(oldGs);
                        break;
                    case "add":
                        if (HaveSameName(haveSaveCrossSchemas, cs))
                        {
                            throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Add.Ex", login.LocaleID));
                            //throw new Exception(String4Report.GetString("���������ķ���/���棬���ٴδ򿪱�������"));
                        }
                        cs.GuidVersion = Guid.NewGuid().ToString();
                        haveSaveCrossSchemas.Add(cs);
                        SetOtherSchemaBDefault(haveSaveCrossSchemas, cs);
                        break;
                    case "modify":
                        if (oldGs == null)
                        {
                            throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Delete.Ex", login.LocaleID));//"û��Ȩ��" 
                            //throw new Exception(String4Report.GetString("����ʧ�ܣ������Ѿ�ɾ��"));
                        }
                        //����ͬһ���û��ڲ�������У�鲢��
                        if (string.IsNullOrEmpty(oldGs.LastUserGuid) || oldGs.LastUserGuid == cs.LastUserGuid)
                        {
                            if (HaveSameName(haveSaveCrossSchemas, cs))
                            {
                                throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Modify.Ex2", login.LocaleID));
                                //throw new Exception(String4Report.GetString("���������ķ���/���棬���ٴδ򿪱�������"));
                            }
                            haveSaveCrossSchemas.Remove(oldGs);
                            cs.GuidVersion = Guid.NewGuid().ToString();
                            haveSaveCrossSchemas.Add(cs);
                            SetOtherSchemaBDefault(haveSaveCrossSchemas, cs);
                        }
                        //����ͬһ���û��ڲ�������У�鲢��
                        else
                        {
                            if (oldGs.GuidVersion == cs.GuidVersion)
                            {
                                if (HaveSameName(haveSaveCrossSchemas, cs))
                                {
                                    throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Modify.Ex2", login.LocaleID));
                                    //throw new Exception(String4Report.GetString("���������ķ���/���棬���ٴδ򿪱�������"));
                                }
                                haveSaveCrossSchemas.Remove(oldGs);
                                cs.GuidVersion = Guid.NewGuid().ToString();
                                haveSaveCrossSchemas.Add(cs);
                                SetOtherSchemaBDefault(haveSaveCrossSchemas, cs);
                            }
                            else
                            {
                                throw new ResourceReportException(String4Report.GetString("U8.UAP.Services.ReportData.ReportDataFacade.Modify.Ex3", login.LocaleID));
                                //throw new Exception(String4Report.GetString("����ʧ�ܣ������Ѿ��޸�"));
                            }
                        }
                        break;
                    default:
                        break;
                }
                result = haveSaveCrossSchemas.ToXml().InnerXml;
                string sql = "UPDATE UAP_ReportView SET PreservedField = @CrossSchemas WHERE ID=@ViewID";
                SqlCommand command = new SqlCommand(sql, cnn);
                command.Parameters.Add(new SqlParameter("CrossSchemas", result));
                command.Parameters.Add(new SqlParameter("ViewID", viewID));

                SqlHelper.ExecuteNonQuery(tran, command);
                tran.Commit();
            }
            catch (Exception e)
            {
                tran.Rollback();
                throw e;
            }
            finally
            {
                if (cnn.State == ConnectionState.Open)
                    cnn.Close();
            }
            return result;
        }

        private bool HaveSameName(GroupSchemas groupSchemas, GroupSchema group)
        {
            foreach (GroupSchema gs in groupSchemas)
            {
                if (gs.Name.Equals(group.Name) && gs.ID != group.ID)
                    return true;
            }
            return false;
        }
        private void SetOtherSchemaBDefault(GroupSchemas haveSaveGroupSchemas, GroupSchema gs)
        {
            if (gs.bDefault)
            {
                for (int i = 0; i < haveSaveGroupSchemas.Count; i++)
                {
                    haveSaveGroupSchemas[i].bDefault = haveSaveGroupSchemas[i].ID == gs.ID ? true : false;
                }
            }
        }
        private GroupSchema GetGroupSchemaById(GroupSchemas groupSchemas, string id)
        {
            GroupSchema schema = null;
            foreach (GroupSchema gs in groupSchemas)
            {
                if (gs.ID == id)
                    return gs;
            }
            return schema;
        }
        #endregion
    }
}
