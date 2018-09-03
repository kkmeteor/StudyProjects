using System;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CalculateGroupObject 的摘要说明。
	/// </summary>
	[Serializable]
	public class CalculateGroupObject:Rect,ISort,ICalculateColumn,IMapName,IGroup,ISerializable,ICloneable,IDisposable,IInformationSender
	{
		#region fields
		protected string _expression="";
		protected SortOption _sortoption ;
        protected string _informationid = "";
		#endregion

		#region constructor
		public CalculateGroupObject():base()
		{
		}

		public CalculateGroupObject(int x,int y):base(x,y)
		{
		}

		public CalculateGroupObject(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public CalculateGroupObject(string name,string expression):base()
		{
			_name=name;
			Expression=expression;
		}

		public CalculateGroupObject(int x,int y,string name,string expression):base(x,y)
		{
			_name=name;
			Expression=expression;
		}

		public CalculateGroupObject(int x,int y,int width,int height,string name,string expression):base(x,y,width,height)
		{
			_name=name;
			Expression=expression;
		}

		public CalculateGroupObject(CalculateGroupObject groupobject):base(groupobject)
		{
			_expression=groupobject.Expression;
			_sortoption =groupobject.SortOption ;
            _informationid = groupobject.InformationID;
		}

		protected CalculateGroupObject( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_expression=info.GetString("Expression");
            _sortoption = new SortOption();
			_sortoption.SortDirection =(SortDirection)info.GetValue("Direction",typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
            if (_version > 91)
                _informationid = info.GetString("InformationID");
		}

		public CalculateGroupObject(GridColumnExpression gridcolumnexpression):base(gridcolumnexpression)
		{
			_expression=gridcolumnexpression.Expression;
            _sortoption =gridcolumnexpression.SortOption;
            
		}

        public CalculateGroupObject(CalculateGroupDimension  gridcolumnexpression)
            : base(gridcolumnexpression)
        {
            _expression = gridcolumnexpression.Expression;
            _sortoption = gridcolumnexpression.SortOption;
            _informationid = gridcolumnexpression.InformationID;
        }

        public CalculateGroupObject(GridCalculateColumn gridcolumnexpression)
            : base(gridcolumnexpression)
        {
            _expression = gridcolumnexpression.Expression;
            _sortoption  = gridcolumnexpression.SortOption ;

        }

        public CalculateGroupObject(IDateTimeDimensionLevel groupobject,string accid)
            : base(groupobject as Rect)
        {
            GroupObject  ch = groupobject as GroupObject ;
            _name = ch.Name + "_Dimension";
            _scriptid = ch.ScriptID;
            _sortoption  = ch.SortOption;
            if (string.IsNullOrEmpty(_prepaintevent))
                Expression = DateTimeDimensionHelper.GetExpressionAll(groupobject.DDLevel,groupobject.ShowYear ,groupobject.ShowWeekRange, ch.DataSource.Name,accid);
            else
                Expression = DateTimeDimensionHelper.GetExpressionOnly(groupobject.DDLevel,groupobject.ShowYear , ch.DataSource.Name,accid );            
        }
		#endregion

		#region override
		public override void SetType()
		{
			_type="CalculateGroupObject";
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
		#endregion

		#region property		
		#endregion

		#region ISort 成员

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

		#region ICalculateColumn 成员

		[Browsable(true)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis19")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis19")]
		[Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public string Expression
		{
			get
			{
				return _expression;
			}
			set
			{
				_expression=value;
			}
		}
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("Expression",_expression);
            info.AddValue("Direction", SortOption.SortDirection);
            info.AddValue("Priority", SortOption.Priority);
            info.AddValue("InformationID", _informationid);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new CalculateGroupObject(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _expression = null;
            _sortoption = null;
            base.Dispose();
		}

		#endregion

		#region IMapName 成员

		[Browsable(false)]
		public string MapName
		{
			get
			{
				return _name ;
			}
		}
		#endregion

        #region IInformationSender Members
        [Browsable(false)]
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
    }
}
