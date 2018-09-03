using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class CalculateGroupDimension:GridColumnExpression ,ISerializable,IPart,IInformationSender,IGroupDimensionStyle
    {
        protected string _informationid = "";
        protected bool _usecolumnstyle = false;
        public CalculateGroupDimension()
            : base()
        {
        }
        public CalculateGroupDimension(int x, int y)
            : base(x, y)
        {
        }

        public CalculateGroupDimension(IDateTimeDimensionLevel groupobject,string accid)
            : base(groupobject as Rect )
        {
            GroupDimension ch = groupobject as GroupDimension;
            _name = ch.Name;
            _scriptid = ch.ScriptID;
            _sortoption  = ch.SortOption;
            _informationid = ch.InformationID;
            _usecolumnstyle = ch.UseColumnStyle;
            _eventtype = ch.EventType;
            _bshowatreal = ch.bShowAtReal;
            if (string.IsNullOrEmpty(_prepaintevent) || _eventtype == ReportElements.EventType.OnTitle)
                Expression = DateTimeDimensionHelper.GetExpressionAll(groupobject.DDLevel, groupobject.ShowYear,groupobject.ShowWeekRange, ch.DataSource.Name, accid);
            else
                Expression = DateTimeDimensionHelper.GetExpressionOnly(groupobject.DDLevel,groupobject.ShowYear , ch.DataSource.Name,accid);            
        }

        public override void SetType()
        {
            _type = "CalculateGroupDimension";
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
            Color c = _forecolor;
            if (!string.IsNullOrEmpty(_informationid))
                c = Color.Blue;
            return c;
        }

        public CalculateGroupDimension(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _informationid = info.GetString("InformationID");
            _usecolumnstyle = info.GetBoolean("UseColumnStyle");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("InformationID", _informationid);
            info.AddValue("UseColumnStyle", _usecolumnstyle);
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
    }
}
