using System;
using System.Data;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ColumnExpression 的摘要说明。
	/// </summary>
	[Serializable]
	public class ColumnExpression:Rect,ISort,ICalculateColumn,IMapName,ISerializable,ICloneable,IDisposable
	{
		#region fields
		protected string _expression="";
		protected SortOption _sortoption ;
		#endregion

		#region constructor
		public ColumnExpression():base()
		{
		}

		public ColumnExpression(int x,int y):base(x,y)
		{
		}

		public ColumnExpression(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public ColumnExpression(string name,string expression):base()
		{
			_name=name;
			Expression=expression;
		}

		public ColumnExpression(int x,int y,string name,string expression):base(x,y)
		{
			_name=name;
			Expression=expression;
		}

		public ColumnExpression(int x,int y,int width,int height,string name,string expression):base(x,y,width,height)
		{
			_name=name;
			Expression=expression;
		}

		public ColumnExpression(ColumnExpression columnexpression):base(columnexpression)
		{
			_name=columnexpression.Name ;
			_expression=columnexpression.Expression;
            _sortoption = columnexpression.SortOption;
		}

		protected ColumnExpression( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_expression=info.GetString("Expression");
            _sortoption = new SortOption();
			_sortoption.SortDirection =(SortDirection)info.GetValue("Direction",typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
		}

		public ColumnExpression(GridColumnExpression gridcolumnexpression):base(gridcolumnexpression)
		{
			_name=gridcolumnexpression.Name;
			_expression=gridcolumnexpression.Expression;
            _sortoption =gridcolumnexpression.SortOption ;
		}

        public ColumnExpression(Rect gridcolumnexpression)
            : base(gridcolumnexpression)
        {
        }
		#endregion

		#region override
		public override void SetType()
		{
			_type="ColumnExpression";
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
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new ColumnExpression(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
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
	}
}
