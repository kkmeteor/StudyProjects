using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Drawing;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class CalculateIndicator:GridCalculateColumn,ISerializable,IPart ,IIndicator 
    {
        private CompareValue _detailcompare = null;
        private CompareValue _totalcompare = null;
        private CompareValue _summarycompare = null;

        public CalculateIndicator()
            : base()
        {
        }
        public CalculateIndicator(int x, int y)
            : base(x, y)
        {
        }

        public CalculateIndicator(Indicator indicator):base(indicator)
        {
            _totalcompare = indicator.TotalCompare;
            _summarycompare = indicator.SummaryCompare;
            _detailcompare = indicator.DetailCompare;
        }

        public CalculateIndicator(CalculateIndicator indicator):base(indicator)
        {
            _totalcompare = indicator.TotalCompare == null ? null : indicator.TotalCompare.Clone();
            _summarycompare = indicator.SummaryCompare == null ? null : indicator.SummaryCompare.Clone();
            _detailcompare = indicator.DetailCompare == null ? null : indicator.DetailCompare.Clone();
        }

        public CalculateIndicator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _totalcompare = (CompareValue)info.GetValue("TotalCompare", typeof(CompareValue));
            _summarycompare = (CompareValue)info.GetValue("SummaryCompare", typeof(CompareValue));
            _detailcompare = (CompareValue)info.GetValue("DetailCompare", typeof(CompareValue));
        }

        public override void SetType()
        {
            _type = "CalculateIndicator";
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _visibleposition = 0;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("TotalCompare", _totalcompare);
            info.AddValue("SummaryCompare", _summarycompare);
            info.AddValue("DetailCompare", _detailcompare);
        }

        public override object Clone()
        {
            return new CalculateIndicator(this);
        }

        protected override void draw(System.Drawing.Graphics g)
        {
            System.Drawing.Image image = null;
            if (_understate == ReportStates.Browse)
                image = AnalysisIndicator(CompareValue);

            base.draw(g);

            if (image != null)
                DrawCompareImage(g, image );
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
            get { return PartType.Indicator ; }
        }

        #endregion

        [DisplayText("U8.UAP.Report.详细设计")]
        [LocalizeDescription("U8.UAP.Report.详细设计")]
        public CompareValue CompareValue
        {
            get
            {
                if (_detailcompare != null)
                    return _detailcompare;
                else if (_summarycompare != null)
                    return _summarycompare;
                else
                    return _totalcompare;
            }
            set
            {
                ;
            }
        }

        [Browsable(false)]
        public CompareValue DetailCompare
        {
            get
            {
                return _detailcompare;
            }
            set
            {
                _detailcompare = value;
            }
        }

        [Browsable(false)]
        public CompareValue TotalCompare
        {
            get
            {
                return _totalcompare;
            }
            set
            {
                _totalcompare = value;
            }
        }

        [Browsable(false)]
        public CompareValue SummaryCompare
        {
            get
            {
                return _summarycompare;
            }
            set
            {
                _summarycompare = value;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_summarycompare != null)
                _summarycompare.Dispose();
            if (_totalcompare != null)
                _totalcompare.Dispose();
            if (_detailcompare != null)
                _detailcompare.Dispose();
            _detailcompare = null;
            _summarycompare = null;
            _totalcompare = null;
        }
    }
}
