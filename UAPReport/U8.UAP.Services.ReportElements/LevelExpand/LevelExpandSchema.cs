using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;


namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class LevelExpandSchema:ISerializable,IDisposable
    {
        private ReportLevelExpandCollection _reportLevelExpands;
        private LevelExpandItemCollection _designTimeLevelExpandItems;
        private ReportLevelExpand _currentReportLevelExpand;

        public LevelExpandSchema()
        {
            _reportLevelExpands = new ReportLevelExpandCollection();
            _designTimeLevelExpandItems = new LevelExpandItemCollection();

        }

        public ReportLevelExpand CurrentReportLevelExpand
        {
            get
            {
                if (_currentReportLevelExpand == null)
                {
                    foreach (ReportLevelExpand item in this._reportLevelExpands)
                    {
                        if (item.IsDefault)
                        {
                            _currentReportLevelExpand = item;
                        }
                    }
                    if (_currentReportLevelExpand == null && this._reportLevelExpands.Count > 0)
                        _currentReportLevelExpand = this._reportLevelExpands[0];
                }
                return _currentReportLevelExpand;
            }
            set
            {
                _currentReportLevelExpand = value;
            }


        }

        public ReportLevelExpandCollection ReportLevelExpands
        {
            get
            {
                return _reportLevelExpands;
            }
            set
            {
                _reportLevelExpands = value;
            }
        }

        public LevelExpandItemCollection DesignTimeLevelExpandItems
        {
            get
            {
                return _designTimeLevelExpandItems;
            }
        }

        public void AddDesignTimeExpandItem( LevelExpandItem item )
        {
            this._designTimeLevelExpandItems.Add(item);
        }

        public void AddReportLevelExpand( ReportLevelExpand item )
        {
            this._reportLevelExpands.Add(item);
        }

        protected LevelExpandSchema( SerializationInfo info, StreamingContext context )
　　    {
          Type type = Type.GetType("UFIDA.U8.UAP.Services.ReportElements.ReportLevelExpandCollection");
          this._reportLevelExpands = info.GetValue("ReportLevelExpands",type ) as ReportLevelExpandCollection;

          type = Type.GetType("UFIDA.U8.UAP.Services.ReportElements.LevelExpandItemCollection");
          this._designTimeLevelExpandItems = info.GetValue("LevelExpandItemCollection", type) as LevelExpandItemCollection;
　　    }
　　
        public virtual void GetObjectData(SerializationInfo info,StreamingContext context)
        {
            info.AddValue("ReportLevelExpands", this._reportLevelExpands, this._reportLevelExpands.GetType());
            info.AddValue("LevelExpandItemCollection", this._designTimeLevelExpandItems, this._designTimeLevelExpandItems.GetType());
        }

        public string Serialize()
        {
            string strObject = string.Empty;
            strObject = SerializeHelper.Serialize(this);
            return strObject;
        }

        public override string ToString()
        {
            string xml = string.Empty;
            try
            {
                LevelExpandSchemaDoc doc = new LevelExpandSchemaDoc();
                LevelExpandSchemaType xmlLevelExpandSchema = new LevelExpandSchemaType();
                doc.SetRootElementName("", "LevelExpandSchema");

                DesignTimeExpandItemsType xmlDesignTimeExpandItems = new DesignTimeExpandItemsType();
                foreach (LevelExpandItem lei in this._designTimeLevelExpandItems)
                {
                    LevelExpandItemType xmlLevelExpandItem = new LevelExpandItemType();
                    xmlLevelExpandItem.AddDepth(new Altova.Types.SchemaLong(lei.Depth));
                    xmlLevelExpandItem.AddLevelExpandType(new Altova.Types.SchemaLong((long)lei.ExpandType));
                    xmlLevelExpandItem.AddColumnName(new Altova.Types.SchemaString(lei.ColumnName));
                    xmlDesignTimeExpandItems.AddLevelExpandItem(xmlLevelExpandItem);
                }
                xmlLevelExpandSchema.AddDesignTimeExpandItems(xmlDesignTimeExpandItems);

                ReportLevelExpandsType xmlReportLevelExpands = new ReportLevelExpandsType();
                foreach (ReportLevelExpand rle in this._reportLevelExpands)
                {
                    ReportLevelExpandType xmlReportLevelExpand = new ReportLevelExpandType();
                    xmlReportLevelExpand.AddIsDefault(new Altova.Types.SchemaBoolean(rle.IsDefault));
                    xmlReportLevelExpand.AddName(new Altova.Types.SchemaString(rle.Name));

                    foreach (LevelExpandItem lei in rle.LevelExpandItems)
                    {
                        LevelExpandItemType xmlLevelExpandItem = new LevelExpandItemType();

                        xmlLevelExpandItem.AddDepth(new Altova.Types.SchemaLong(lei.Depth));
                        xmlLevelExpandItem.AddLevelExpandType(new Altova.Types.SchemaLong((long)lei.ExpandType));
                        xmlLevelExpandItem.AddColumnName(new Altova.Types.SchemaString(lei.ColumnName));

                        xmlReportLevelExpand.AddLevelExpandItem(xmlLevelExpandItem);
                    }
                    xmlReportLevelExpands.AddReportLevelExpand(xmlReportLevelExpand);
                }
                xmlLevelExpandSchema.AddReportLevelExpands(xmlReportLevelExpands);
                xml = doc.SaveToString(xmlLevelExpandSchema);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            return xml;
        }

        public void LoadFromString( string xml )
        {
            if( string.IsNullOrEmpty(xml))
                return;
            try
            {
                LevelExpandSchemaDoc doc = new LevelExpandSchemaDoc();
                LevelExpandSchemaType xmlLevelExpandSchema = new LevelExpandSchemaType(doc.LoadFromString(xml));

                int count = xmlLevelExpandSchema.GetDesignTimeExpandItems().GetLevelExpandItemCount();
                for (int i = 0; i < count; i++)
                {
                    LevelExpandItem lei = new LevelExpandItem();
                    lei.ColumnName = xmlLevelExpandSchema.GetDesignTimeExpandItems().GetLevelExpandItemAt(i).ColumnName.Value;
                    lei.Depth = (int)xmlLevelExpandSchema.GetDesignTimeExpandItems().GetLevelExpandItemAt(i).Depth.Value;
                    lei.ExpandType = (LevelExpandEnum)xmlLevelExpandSchema.GetDesignTimeExpandItems().GetLevelExpandItemAt(i).LevelExpandType.Value;
                    this._designTimeLevelExpandItems.Add(lei);
                }

                count = xmlLevelExpandSchema.GetReportLevelExpands().GetReportLevelExpandCount();
                for (int i = 0; i < count; i++)
                {
                    ReportLevelExpand rle = new ReportLevelExpand();
                    ReportLevelExpandType xmlReportLevelExpand = xmlLevelExpandSchema.GetReportLevelExpands().GetReportLevelExpandAt(i);

                    rle.IsDefault = xmlReportLevelExpand.IsDefault.Value;
                    rle.Name = xmlReportLevelExpand.Name.Value;

                    int itemCount = xmlReportLevelExpand.GetLevelExpandItemCount();
                    for (int j = 0; j < itemCount; j++)
                    {
                        LevelExpandItem lei = new LevelExpandItem();
                        lei.ColumnName = xmlReportLevelExpand.GetLevelExpandItemAt(j).ColumnName.Value;
                        lei.Depth = (int)xmlReportLevelExpand.GetLevelExpandItemAt(j).Depth.Value;
                        lei.ExpandType = (LevelExpandEnum)xmlReportLevelExpand.GetLevelExpandItemAt(j).LevelExpandType.Value;

                        rle.AddLevelExpand(lei);
                    }

                    this._reportLevelExpands.Add(rle);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.Write(e.Message);
            }
            
        }

        public static LevelExpandSchema Deserialize(string objectString)
        {
            LevelExpandSchema obj = null;
            if (objectString == string.Empty)
                obj = new LevelExpandSchema();
            else
                obj = SerializeHelper.Deserialize(objectString) as LevelExpandSchema;
            return obj;
        }

        #region IDisposable 撹埀

        public void Dispose()
        {
            if (_currentReportLevelExpand != null)
            {
                _currentReportLevelExpand = null;
            }
            if (_reportLevelExpands != null)
            {
                _reportLevelExpands.Clear();
                _reportLevelExpands = null;
            }
            if (_designTimeLevelExpandItems != null)
            {
                _designTimeLevelExpandItems.Clear();
                _designTimeLevelExpandItems = null;
            }            
        }

        #endregion
    }
}
