using System;
using System.Data;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// ColumnHeader ��ժҪ˵����
    /// </summary>
    [Serializable]
    public class ColumnHeader : Rect, ISort, IDataSource, IMultiHeader, IBDateTime, ISerializable, ICloneable, IDisposable, IDateTimeDimensionLevel, IWithSizable
    {
        #region fields
        protected DataSource _datasource = new DataSource("EmptyColumn");
        protected SortOption _sortoption;
        protected bool _bdatetime = false;
        protected Columns _columns = new Columns();
        protected DataSource _sortsource = new DataSource("EmptyColumn");
        protected string _formatstring = "";
        protected DateTimeDimensionLevel _ddlevel = DateTimeDimensionLevel.ʱ��;
        private bool _showyear = true;
        private bool _showWeekRange = false;
        private bool _supportswitch = false;
        #endregion

        #region constructor
        public ColumnHeader()
            : base()
        {
        }

        public ColumnHeader(int x, int y)
            : base(x, y)
        {
        }

        public ColumnHeader(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
        }

        public ColumnHeader(DataSource ds)
            : base()
        {
            _datasource = ds;
            _name = ds.Name;
            _cncaption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
            _caption = ds.Caption;
        }

        public ColumnHeader(ColumnHeader groupobject)
            : base(groupobject)
        {
            _datasource = groupobject.DataSource;
            _sortoption = groupobject.SortOption;
            _bdatetime = groupobject.bDateTime;
            _sortsource = groupobject.SortSource;
            _formatstring = groupobject.FormatString;
            _ddlevel = groupobject.DDLevel;
            _showyear = groupobject.ShowYear;
            _showWeekRange = groupobject.ShowWeekRange;
            _supportswitch = groupobject.SupportSwitch;
        }

        protected ColumnHeader(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _datasource = (DataSource)info.GetValue("DataSource", typeof(DataSource));
            _sortoption = new SortOption();
            _sortoption.SortDirection = (SortDirection)info.GetValue("Direction", typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
            _bdatetime = info.GetBoolean("bDateTime");
            _sortsource = (DataSource)info.GetValue("SortSource", typeof(DataSource));
            _formatstring = info.GetString("FormatString");
            _ddlevel = (DateTimeDimensionLevel)info.GetValue("DDLevel", typeof(DateTimeDimensionLevel));
            if (_version >= 100)
            {
                try
                {
                    _showyear = info.GetBoolean("ShowYear");
                    _supportswitch = info.GetBoolean("SupportSwitch");
                    _showWeekRange = info.GetBoolean("ShowWeekRange");
                }
                catch
                {
                    ;
                }

            }
        }

        public ColumnHeader(GridLabel gridlabel)
            : base(gridlabel)
        {
            _datasource = gridlabel.DataSource;
            _sortoption = gridlabel.SortOption;
        }
        public ColumnHeader(GridDateTime griddatetime)
            : base(griddatetime)
        {
            _datasource = griddatetime.DataSource;
            _sortoption = griddatetime.SortOption;
            _bdatetime = true;
            _formatstring = griddatetime.FormatString;
            _showyear = griddatetime.ShowYear;
            _showWeekRange = griddatetime.ShowWeekRange;
            _supportswitch = griddatetime.SupportSwitch;
        }
        #endregion

        #region override
        public override void SetType()
        {
            _type = "ColumnHeader";
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _captionalign = System.Drawing.ContentAlignment.MiddleCenter;
            _backcolor = DefaultConfigs.DefaultTitleBackColor;
            _visibleposition = 0;
        }

        [Browsable(false)]
        public override System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Font ClientFont
        {
            get
            {
                return base.ClientFont;
            }
            set
            {
                base.ClientFont = value;
            }
        }

        [Browsable(false)]
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }
        #endregion

        #region property
        [Browsable(false)]
        public override bool bControlAuth
        {
            get
            {
                return _bcontrolauth;
            }
            set
            {
                _bcontrolauth = value;
            }
        }
        #endregion

        #region ISort ��Ա

        [Browsable(true)]
        [DisplayText("U8.Report.SortOption")]
        [LocalizeDescription("U8.Report.SortOption")]
        public SortOption SortOption
        {
            get
            {
                if (_sortoption == null)
                    _sortoption = new SortOption();
                return _sortoption;
            }
            set
            {
                _sortoption = value;
            }
        }

        #endregion

        #region ISerializable ��Ա

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("DataSource", _datasource);
            info.AddValue("Direction", SortOption.SortDirection);
            info.AddValue("Priority", SortOption.Priority);
            info.AddValue("bDateTime", _bdatetime);
            info.AddValue("SortSource", _sortsource);
            info.AddValue("FormatString", _formatstring);
            info.AddValue("DDLevel", _ddlevel);
            info.AddValue("ShowYear", _showyear);
            info.AddValue("ShowWeekRange", _showWeekRange);
            info.AddValue("SupportSwitch", _supportswitch);
        }

        #endregion

        #region ICloneable ��Ա

        public override object Clone()
        {
            return new ColumnHeader(this);
        }

        #endregion

        #region IDisposable ��Ա

        public override void Dispose()
        {
            _datasource = null;
            _columns.Clear();
            _columns = null;
            _sortsource = null;
            _sortoption = null;
            base.Dispose();
        }

        #endregion

        #region IDataSource ��Ա

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis16")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
        public DataSource DataSource
        {
            get
            {
                return _datasource;
            }
            set
            {
                _datasource = value;
                if (_caption == "" && value != null)
                {
                    _caption = _datasource.Caption;
                    OnOtherChanged(null);
                }
            }
        }
        #endregion

        #region IMultiHeader ��Ա

        [Browsable(false)]
        public Columns Columns
        {
            get
            {
                return _columns;
            }
        }

        [DisplayText("����ؼ���")]
        [LocalizeDescription("����ؼ���")]
        public DataSource SortSource
        {
            get
            {
                return _sortsource;
            }
            set
            {
                _sortsource = value;
            }
        }
        #endregion

        #region IMapName ��Ա

        [Browsable(false)]
        public string MapName
        {
            get
            {
                return _datasource.Name;
            }
        }
        #endregion

        #region IBDateTime ��Ա
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

        #region IFormat ��Ա

        [Browsable(true)]
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

        #region IDateTimeDimensionLevel ��Ա
        [DisplayText("ʱ��ά��")]
        [LocalizeDescription("ʱ��ά��")]
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
        #endregion
    }

    public interface IDateTimeDimensionLevel
    {
        DateTimeDimensionLevel DDLevel { get; set; }
        bool ShowYear { get; set; }
        bool SupportSwitch { get; set; }
        bool ShowWeekRange { get; set; }
    }

    public class DateTimeDimensionHelper
    {
        public static string GetExpressionAll(DateTimeDimensionLevel ddlevel, bool showyear, bool showWeekRange, string name, string accid)
        {
            switch (ddlevel)
            {
                case DateTimeDimensionLevel.��:
                    return "Year(" + name + ")";
                case DateTimeDimensionLevel.��:
                    return "dbo.Report_MonthCaption(" + (showyear ? "1" : "0") + ",Year(" + name + ")," + "Month(" + name + "))";
                case DateTimeDimensionLevel.�����:
                    return "dbo.Report_AccountMonthCaption(" + name + "," + (showyear ? "1" : "0") + ",'" + accid + "')";
                case DateTimeDimensionLevel.��:
                    return "dbo.Report_WeekCaption(" + (showyear ? "1" : "0") + "," + (showWeekRange ? "1" : "0") + ",Year(" + name + ")," + "DATEPART( ww , " + name + "))";
                case DateTimeDimensionLevel.��:
                    return "dbo.Report_SeasonCaption(" + (showyear ? "1" : "0") + ",Year(" + name + ")," + "DATEPART( qq , " + name + "))";
                case DateTimeDimensionLevel.Ѯ:
                    return "dbo.Report_XunCaption(" + (showyear ? "1" : "0") + "," + "0" + ",Year(" + name + ")," + "Month(" + name + "),"+ "dbo.GetXun(" + name + "))";
                case DateTimeDimensionLevel.��:
                    return "dbo.Report_DateCaption(" + (showyear ? "1" : "0") + "," + "0" + ",Year(" + name + ")," + "Month(" + name + ")," + "Day(" + name + "))";
                default:
                    return name;
            }
        }

        public static string GetExpressionOnly(DateTimeDimensionLevel ddlevel, bool showyear, string name, string accid)
        {
            switch (ddlevel)
            {
                case DateTimeDimensionLevel.��:
                    return "Year(" + name + ")";
                case DateTimeDimensionLevel.��:
                    return "Month(" + name + ")";
                case DateTimeDimensionLevel.��:
                    return "DAY(" + name + ")";
                case DateTimeDimensionLevel.�����:
                    return "dbo.Report_AccountMonth(" + name + ",'" + accid + "')";
                case DateTimeDimensionLevel.��:
                    return "DATEPART( ww , " + name + ")";
                case DateTimeDimensionLevel.��:
                    return "DATEPART( qq , " + name + ")";
                default:
                    return name;
            }
        }

        public static int DateDimensionToInt(DateTimeDimensionLevel ddlevel, bool showyear, bool supportSwitch, bool showWeekRange)
        {
            int factor = 1;
            if (supportSwitch)
                factor = factor * 10;
            if (showWeekRange)
                factor = factor * 100;
            switch (ddlevel)
            {
                case DateTimeDimensionLevel.ʱ��:
                    return showyear ? 1 * factor : -1 * factor;
                case DateTimeDimensionLevel.��:
                    return showyear ? 3 * factor : -3 * factor;
                case DateTimeDimensionLevel.�����:
                    return showyear ? 5 * factor : -5 * factor;
                case DateTimeDimensionLevel.��:
                    return showyear ? 4 * factor : -4 * factor;
                case DateTimeDimensionLevel.��:
                    return showyear ? 2 * factor : -2 * factor;
                case DateTimeDimensionLevel.Ѯ:
                    return showyear ? 6 * factor : -6 * factor;
                case DateTimeDimensionLevel.��:
                    return showyear ? 7 * factor : -7 * factor;
                default:
                    return 0;
            }
        }

        public static void SetDateDimensionLevel(IDateTimeDimensionLevel cell, int datedimension)
        {
            cell.DDLevel = GetDateDimensionLevel(datedimension);
            cell.ShowYear = ShowYearOrNot(datedimension);
            cell.ShowWeekRange = ShowWeekRangeOrNot(datedimension);
        }

        public static DateTimeDimensionLevel GetDateDimensionLevel(int datedimension)
        {
            if (Math.Abs(datedimension) >= 1000)
                datedimension = datedimension / 1000;
            else if (Math.Abs(datedimension) >= 100)
                datedimension = datedimension / 100;
            else if (Math.Abs(datedimension) >= 10)
                datedimension = datedimension / 10;

            switch (Math.Abs(datedimension))
            {
                case 1:
                    return DateTimeDimensionLevel.ʱ��;
                case 2:
                    return DateTimeDimensionLevel.��;
                case 3:
                    return DateTimeDimensionLevel.��;
                case 4:
                    return DateTimeDimensionLevel.��;
                case 5:
                    return DateTimeDimensionLevel.�����;
                case 6:
                    return DateTimeDimensionLevel.Ѯ;
                case 7:
                    return DateTimeDimensionLevel.��;
                default:
                    return DateTimeDimensionLevel.��;
            }
        }

        public static bool ShowYearOrNot(int datedimension)
        {
            if (datedimension < 0)
                return false;
            else
                return true;
        }
        public static bool ShowWeekRangeOrNot(int datedimension)
        {
            if (System.Math.Abs(datedimension) >= 100)
                return true;
            else
                return false;
        }
        public static bool SupportSwitchOrNot(int datedimension)
        {
            if (System.Math.Abs(datedimension) >= 1000 || (System.Math.Abs(datedimension) < 100 && System.Math.Abs(datedimension) >= 10))
                return true;
            else
                return false;

        }
    }

    public enum DateTimeDimensionLevel
    {
        �� = 0,
        �� = 1,
        ʱ�� = 2,
        �� = 3,
        Ѯ = 6,
        �� = 4,
        ����� = 5,
        �� = 7  //V12.5�޸�matfb�����Ӵ���������date���޸�ԭ������dayΪdatetime����������ʱ���룩
    }
}
