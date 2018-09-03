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
    public class CalculatorIndicator : Calculator , ISerializable, IIndicator
    {
        private CompareValue _summarycompare = null;

        public CalculatorIndicator()
            : base()
        {
        }
        public CalculatorIndicator(int x, int y)
            : base(x, y)
        {
        }

        public CalculatorIndicator(Indicator indicator)
            : base(indicator)
        {
            _summarycompare = indicator.SummaryCompare;
        }

        public CalculatorIndicator(CalculateIndicator indicator)
            : base(indicator)
        {
            _summarycompare = indicator.SummaryCompare;
        }

        public CalculatorIndicator(CalculatorIndicator indicator)
            : base(indicator)
        {
            _summarycompare = indicator.SummaryCompare==null?null:indicator.SummaryCompare.Clone();
        }

        public CalculatorIndicator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _summarycompare = (CompareValue)info.GetValue("SummaryCompare", typeof(CompareValue));
        }

        public override void SetType()
        {
            _type = "CalculatorIndicator";
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _visible = false;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SummaryCompare", _summarycompare);
        }

        public override object Clone()
        {
            return new CalculatorIndicator(this);
        }

        protected override void draw(System.Drawing.Graphics g)
        {
            System.Drawing.Image image = null;
            if (_understate == ReportStates.Browse)
                image = AnalysisIndicator(CompareValue);

            base.draw(g);

            if (image != null)
                DrawCompareImage(g, image);
        }

        [DisplayText("U8.UAP.Report.详细设计")]
        [LocalizeDescription("U8.UAP.Report.详细设计")]
        public CompareValue CompareValue
        {
            get
            {
                return _summarycompare;
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
                return null;
            }
            set
            {
                ;
            }
        }

        [Browsable(false)]
        public CompareValue TotalCompare
        {
            get
            {
                return null;
            }
            set
            {
                ;
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
            _summarycompare = null;
        }
    }
}
