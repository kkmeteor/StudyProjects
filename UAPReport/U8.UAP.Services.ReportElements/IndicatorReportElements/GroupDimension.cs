using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class GroupDimension:GridLabel,ISerializable ,IPart ,IFormat,IBDateTime,IInformationSender,IGroupDimensionStyle,IDateTimeDimensionLevel
    {
        protected string _formatstring = "";
        protected bool _bdatetime = false;
        protected string _informationid = "";
        protected bool _usecolumnstyle = false;
        protected DateTimeDimensionLevel _ddlevel = DateTimeDimensionLevel.时间;
        private bool _showyear = true;
        private bool _showWeekRange = false;
        private bool _supportswitch = false;
        public GroupDimension()
            : base()
        {
        }
        public GroupDimension(int x, int y)
            : base(x, y)
        {
        }

        public GroupDimension(DataSource ds)
            : base(ds)
        {
            if (ds.Type == DataType.DateTime)
                _bdatetime = true;
        }

        public override void SetType()
        {
            _type = "GroupDimension";
        }
        public override void SetDefault()
        {
            base.SetDefault();
            _visibleposition = 0;
        }

        protected override Font GetClientFont(ServerFont sf)
        {
            if (!string.IsNullOrEmpty(_informationid))
                sf.UnderLine = true;
            else
                sf.UnderLine = false;
            return base.GetClientFont(sf);
        }

        protected override Color GetInfomationForeColr()
        {
            Color c=_forecolor;
            if (!string.IsNullOrEmpty(_informationid))
                c = Color.Blue;
            return c;
        }

        public GroupDimension(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bdatetime = info.GetBoolean("bDateTime");
            _formatstring = info.GetString("FormatString");
            _informationid = info.GetString("InformationID");
            _usecolumnstyle = info.GetBoolean("UseColumnStyle");
            if (_version >= 100)
            {
                try
                {
                    _ddlevel = (DateTimeDimensionLevel)info.GetValue("DDLevel", typeof(DateTimeDimensionLevel));
                    _showyear = info.GetBoolean("ShowYear");                    
                    _supportswitch = info.GetBoolean("SupportSwitch");
                    _showWeekRange = info.GetBoolean("ShowWeekRange");
                }
                catch { }
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("bDateTime", _bdatetime);
            info.AddValue("FormatString", _formatstring);
            info.AddValue("InformationID", _informationid);
            info.AddValue("UseColumnStyle", _usecolumnstyle);
            info.AddValue("DDLevel", _ddlevel);
            info.AddValue("ShowYear", _showyear);
            info.AddValue("ShowWeekRange", _showWeekRange);
            info.AddValue("SupportSwitch", _supportswitch);
        }
        #region IPart Members
        private IMetrix  _metrix;
        [Browsable(false)]
        public IMetrix  Metrix
        {
            get
            {
                return _metrix;
            }
            set
            {
                _metrix = value;
            }
        }
        [Browsable(false)]
        public PartType PartType
        {
            get { return PartType.Group; }
        }

        #endregion

        #region IFormat 成员

        [DisplayText("U8.UAP.Services.ReportElements.Dis22")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis22")]
        public string FormatString
        {
            get
            {
                return _formatstring;
            }
            set
            {
                _formatstring = value;
            }
        }

        #endregion

        #region IBDateTime Members

        [Browsable(true)]
        [DisplayText("U8.Report.bDateTime")]
        [LocalizeDescription("U8.Report.bDateTime")]
        public bool bDateTime
        {
            get
            {
                return _bdatetime;
            }
            set
            {
                _bdatetime = value;
            }
        }

        #endregion

        #region IInformationSender Members
        [DisplayText("U8.UAP.Report.InformationPublish")]
        [LocalizeDescription("U8.UAP.Report.InformationPublish")]
        public string InformationID
        {
            get
            {
                return _informationid;
            }
            set
            {
                _informationid = value;
            }
        }

        #endregion
        [Browsable(true)]
        [DisplayText("U8.UAP.Report.ShowOnX")]
        [LocalizeDescription("U8.UAP.Report.ShowOnX")]
        public override bool bShowOnX
        {
            get
            {
                return base.bShowOnX;
            }
            set
            {
                base.bShowOnX = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis16")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
        public override DataSource DataSource
        {
            get
            {
                return _datasource;
            }
            set
            {
                base.DataSource = value;
                if (_datasource.Type == DataType.DateTime)
                    _bdatetime = true;
            }
        }

        #region IGroupDimensionStyle Members
        [DisplayText("U8.UAP.Report.GroupUseColumnStyle")]
        [LocalizeDescription("U8.UAP.Report.GroupUseColumnStyle")]
        public bool UseColumnStyle
        {
            get
            {
                return _usecolumnstyle;
            }
            set
            {
                _usecolumnstyle = value;
            }
        }

        #endregion

        [DisplayText("时间维度")]
        [LocalizeDescription("时间维度")]
        public DateTimeDimensionLevel DDLevel
        {
            get
            {
                return _ddlevel;
            }
            set
            {
                _ddlevel = value;
            }
        }

        [DisplayText("U8.UAP.Report.ShowYear")]
        [LocalizeDescription("U8.UAP.Report.ShowYear")]
        public bool ShowYear
        {
            get
            {
                return _showyear;
            }
            set
            {
                _showyear = value;
            }
        }
        [DisplayText("U8.UAP.Report.ShowWeekRange")]
        [LocalizeDescription("U8.UAP.Report.ShowWeekRange")]
        public bool ShowWeekRange
        {
            get
            {
                return _showWeekRange;
            }
            set
            {
                _showWeekRange = value;
            }
        }

        [DisplayText("U8.UAP.Report.SupportSwitch")]
        [LocalizeDescription("U8.UAP.Report.SupportSwitch")]
        public bool SupportSwitch
        {
            get
            {
                return _supportswitch;
            }
            set
            {
                _supportswitch = value;
            }
        }
    }
}
