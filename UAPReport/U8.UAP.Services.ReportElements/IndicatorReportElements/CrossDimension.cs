using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class CrossDimension:ColumnHeader ,ISerializable,IPart
    {
        public CrossDimension()
            : base()
        {
        }
        public CrossDimension(int x, int y)
            : base(x, y)
        {
        }

        public override void SetType()
        {
            _type = "CrossDimension";
        }
        public override void SetDefault()
        {
            base.SetDefault();
            _visibleposition = 0;
        }

        public CrossDimension(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
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
            get { return PartType.Cross ; }
        }

        #endregion
    }
}
