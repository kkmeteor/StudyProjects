using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

using Infragistics.UltraChart.Core.ColorModel;
using Infragistics.UltraChart.Core;
using Infragistics.UltraChart.Core.Layers;
using Infragistics.UltraChart.Core.Primitives;
using Infragistics.UltraChart.Data;
using Infragistics.UltraChart.Resources;   
using Infragistics.UltraChart.Resources.Appearance;
using Infragistics.UltraChart.Resources.Editor;
using Infragistics.UltraChart.Shared.Styles;
using Infragistics.UltraChart.Core.Util;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Gauge layer for UltraChart. When added to the custom layer collection of 
	/// the Ultrachart it will render aa a gauge chart. Gauge is normally a chart with
	/// radial axis. A numeric value is charted by showing a needle at an angular position.
	/// </summary>
	public class GaugeLayer : ILayer
	{
		/// <summary>
		/// Field storage of Properties
		/// </summary>
		protected Rectangle innerBounds = new Rectangle(0,0,0,0);

		/// <summary>
		///Numeric ruler for mapping purposes 
		/// </summary>
		private NumericRuler _Ruler;
		private Hashtable _Labels = new Hashtable();

		/// <summary>
		/// Default constructor for gauge layer.
		/// </summary>
		public GaugeLayer() 
		{
			// Create a ruler.
			this._Ruler = new NumericRuler();
		}

		/// <summary>
		/// Layer's main piece of code that does the drawing. This method calculates and 
		/// populates a scene graph with various primitive that constitue the chart.
		/// </summary>
		/// <param name="scene">Collection of Scene items (primitives) to hold items that draw chart.</param>
		public void FillSceneGraph(SceneGraph scene)
		{
		
			//Draw the axes, use the settings from the y axis properties.
			AxisAppearance YApp = (AxisAppearance) this.ChartComponent.GetChartAppearance(ChartAppearanceTypes.AxisY);

			// Check if appearance and Y-axis appearance are defined.
			if (_Appearance!= null && YApp != null) 
			{

				#region Initialization

				// initilize the variable. 
				// Minimum value a needle can have.
				double min = 0;
				// Maximum value a needle can have.
				double max = 100;
				// Tick increments.
				double delta = 10;

				// Check to see if the Gauge should assume size and layout automatically
				// centering the dial and assuming available width as its size.
				if (this._Appearance.Layout==DialLayout.Automatic) 
				{
					this._Appearance.Radius =  Math.Min(this.innerBounds.Width/3,this.innerBounds.Height/3);
					this._Appearance.Center = new Point(this.innerBounds.X+this.innerBounds.Width/2,this.innerBounds.Y+this.innerBounds.Height/2);
				}

				// check for axis settings
				// If cutom rage is specified use custom range.
				if (YApp.RangeType == AxisRangeType.Custom) 
				{
					min = YApp.RangeMin;
					max = YApp.RangeMax;
				}
				else 
				{
					// Automatic: add up the specified sections.
					if (this._Appearance.Sections.Count>0) 
					{
						min = 0;
						max = 0;
						foreach(GaugeSection sec in this._Appearance.Sections) 
						{
							max+= sec.Value;
						}
					}
				}
			
				// Assume and calculate the default increment.
				delta = (max - min)/10;

				// check for y-axis appearance for specified data interval.
				if (YApp.TickmarkStyle == AxisTickStyle.DataInterval) 
				{
					// Actual value is specified.
					delta  = YApp.TickmarkInterval;
				}
				else 
				{
					// Convert the percentage value in actual value.
					delta = (max - min)*YApp.TickmarkPercentage /100;
				}

				// set up ruler with value. This will be used for measurement purposes.
				_Ruler.Maximum = max;
				_Ruler.Minimum = min;

				// Check the direction of the dial ticks.
				if (this.Appearance.Direction == Direction.RightToLeft) 
				{
					_Ruler.MapMinimum = _Appearance.StartAngle;
					_Ruler.MapMaximum = _Appearance.EndAngle;
				}
				else 
				{
					_Ruler.MapMinimum = _Appearance.EndAngle;
					_Ruler.MapMaximum = _Appearance.StartAngle;
				}

				// copy scoll-scale.
				_Ruler.Scale  = YApp.ScrollScale.Scale;
				_Ruler.Scroll = YApp.ScrollScale.Scroll;

				#endregion

				#region Draw dial and sections
				// start from the minimum
				double d_i = (double)_Ruler.WindowMinimum;

				// calculate various radii.
				int r1 =this._Appearance.TickStart*this._Appearance.Radius/100;
				int r2 =this._Appearance.TickEnd*this._Appearance.Radius/100;
				int r3 =this._Appearance.TextLoc*this._Appearance.Radius/100;

				// draw dial or background.
				Ellipse dial = new Ellipse(this._Appearance.Center, this._Appearance.Radius);
				dial.PE = this.Appearance.DialPE;

				// add dial background.
				scene.Add(dial);

				// draw the sections
				// variable to hold present value.
				double presentVal = 0;
				// start the section from the window minimum.
				double lastVal = (double)_Ruler.WindowMinimum;

				// foreach section, draw a section.
				foreach(GaugeSection sec in this._Appearance.Sections) 
				{
					presentVal = lastVal + sec.Value;

					// section start angle.
					int ang0 = -(int)_Ruler.Map(lastVal);
					// section end angle.
					int ang1 = -(int)_Ruler.Map(presentVal);

					// create the wedge
					Wedge w = new Wedge(this._Appearance.Center, sec.EndWidth*this._Appearance.Radius/100, ang0, (ang1-ang0));
					w.PE = sec.PE;
					w.RadiusInner = Math.Max(0, Math.Min(w.Radius - 1, sec.StartWidth*this._Appearance.Radius/100));
					scene.Add(w);

					lastVal = presentVal;
				}
				#endregion

				#region Draw axis' tick marks on dial
				// sanity check for increment. Without this it will go into infinite loop.
				if (delta < 2*double.Epsilon) delta = 5*double.Epsilon;

				// loop thru and add the items.
				while(d_i< (double)_Ruler.WindowMaximum + 2* double.Epsilon + delta) 
				{
					// convert the tickmark value to angle
					int ang = (int)_Ruler.Map(d_i);

					// see if major grid lines are visible.
					if (YApp.MajorGridLines.Visible) 
					{
						// Convert polar co-ordinates into cartiesian.
						// In simple words: Convert a pair of (radius, angle) in a point (x and y).
						Point p1 = Infragistics.UltraChart.Core.Util.Geometry.AngularToCartesian(this._Appearance.Center, r1, -Geometry.DegreeToRadian(ang));
						Point p2 = Infragistics.UltraChart.Core.Util.Geometry.AngularToCartesian(this._Appearance.Center, r2, -Geometry.DegreeToRadian(ang));

						// Draw the line for tick marks. Use Y-axis's properties to color and style it.
						Line l = new Line(p1, p2);
						l.PE.Stroke = YApp.MajorGridLines.Color;
						l.PE.StrokeOpacity = YApp.MajorGridLines.Color.A;
						l.lineStyle.DrawStyle = YApp.MajorGridLines.DrawStyle;
						l.PE.StrokeWidth = YApp.MajorGridLines.Thickness;

						// add to scene.
						scene.Add(l);
					}

					// see if minor grid lines are visible. If yes draw them half 
					// a tick far from major grid line. It will use Y-axis's minor
					// grid lines appearance for color and style.
					if (YApp.MinorGridLines.Visible) 
					{
						if (d_i+delta/2 < (double)_Ruler.WindowMaximum ) 
						{
							// convert the tickmark value to angle
							int ang1 = (int)_Ruler.Map(d_i+delta/2);

							int tfp = Math.Abs((r2 - r1) /4);
							// Convert a pair of (radius, angle) in a point (x and y).
							Point p1 = Infragistics.UltraChart.Core.Util.Geometry.AngularToCartesian(this._Appearance.Center, r1+tfp, -Geometry.DegreeToRadian(ang1));
							Point p2 = Infragistics.UltraChart.Core.Util.Geometry.AngularToCartesian(this._Appearance.Center, r2-tfp, -Geometry.DegreeToRadian(ang1));

							// draw a minor tick line
							Line l = new Line(p1, p2);
							l.PE.Stroke = YApp.MinorGridLines.Color;
							l.PE.StrokeOpacity = YApp.MinorGridLines.Color.A;
							l.lineStyle.DrawStyle = YApp.MinorGridLines.DrawStyle;
							l.PE.StrokeWidth = YApp.MinorGridLines.Thickness;

							// add to scene.
							scene.Add(l);
						}
					}

					// see if labels are visible.
					if (YApp.Labels.Visible) 
					{

						// Draw the labels: Convert the angle and radius into point location for the label.
						Point p3 = Infragistics.UltraChart.Core.Util.Geometry.AngularToCartesian(this._Appearance.Center, r3, -Geometry.DegreeToRadian(ang));
						_Labels["DATA_VALUE"] = d_i;
					
						// Use the label formatter and use Y-axis label format.
						Text t = new Text(p3, LabelFormatter.replaceKeywords(_Labels, YApp.Labels.ItemFormatString), YApp.Labels.LabelStyle.Copy());
						t.labelStyle.VerticalAlign = StringAlignment.Center;
						t.labelStyle.HorizontalAlign = StringAlignment.Center;

						// Use custom orienation as we need to rotate them to
						// place them on angular axis.
						t.labelStyle.Orientation = TextOrientation.Custom;

						// rotate with respect to present tick angle.
						t.labelStyle.RotationAngle = ang -90;

						// add to scene.
						scene.Add(t);
					}
			
					// increment current value of tick by the data interval: delta.
					d_i += delta;
				}

				// See if axis line is visible.
				if (YApp.Visible) 
				{
					// create new line style
					LineStyle ls = new LineStyle();

					// use y-axis's draw style.
					ls.DrawStyle = YApp.LineDrawStyle;
				
					// draw an arc that looks takes place of axis line.
					Arc el = new Arc(this._Appearance.Center, (r1+r2)/2, (float)this._Appearance.StartAngle, -(float)Math.Abs(this._Appearance.EndAngle-this._Appearance.StartAngle), ls);
					// use the axes line's color and thickness.
					el.PE.Stroke = YApp.LineColor;
					el.PE.StrokeOpacity = YApp.LineColor.A;
					el.PE.StrokeWidth = YApp.LineThickness;

					// add to scene.
					scene.Add(el);
				}

				#endregion

				#region Draw needles
				// sort needles according to needle length. shortest comes on the top.
				double[] ar = new Double[this.Appearance.Needles.Count];
				for(int i =0; i <this.Appearance.Needles.Count; i++) 
				{
					// store the length of needle in temporary array.
					ar[i] = this.Appearance.Needles[i].Length;
				}
			
				// sort the order. This function takes the length of 
				// needle array and get the sorted order.
				int[] order = null;
				if (ar.Length > 0)
				{
					order = MiscFunctions.GetSortedOrderDouble(ar);
				}
				// draw the needles on the dials
				for(int i =0; i <this.Appearance.Needles.Count; i++) 
				{
					// get n-th needle.
					Needle nd = this.Appearance.Needles[order[i]];

					// depending upon needle's present value find out the
					// angle at which it should be inclined. 
					int theta_i = (int)_Ruler.Map(nd.Value);

					// convert, angle of needle and its length into a point location.
					Point p = Infragistics.UltraChart.Core.Util.Geometry.AngularToCartesian(this._Appearance.Center, nd.Length*this._Appearance.Radius/100, Geometry.DegreeToRadian( - theta_i));

					// draw a line from the center of dial to location of needle's head.
					Line l = new Line(this._Appearance.Center, p);
					l.lineStyle.EndStyle = LineCapStyle.ArrowAnchor;
					l.lineStyle.StartStyle = LineCapStyle.RoundAnchor;

					// attach the paint element from needle to the line primitive.
					l.PE = nd.PE;

					// add to scene.
					scene.Add(l);
				}
				#endregion
			}
		}

		private GaugeAppearance _Appearance;
		/// <summary>
		/// Gauge appearance property. This holds all the things that
		/// can be done to change the look of the gauge layer.
		/// </summary>
		public  GaugeAppearance Appearance
		{
			get { return _Appearance; }
			set { _Appearance = value; }
		}

		#region ILayer Members
		/// <summary>
		/// Get the inner bounds of the layer.
		/// </summary>
		/// <returns>A rectangle which specifies the inner bounds of layers.</returns>
		public Rectangle GetInnerBounds()
		{
			return this.innerBounds;
		}

		/// <summary>
		/// Get the invalid message. Upon validating data for the layer, a 
		/// layer can publish on what is wrong with data. 
		/// </summary>
		/// <returns>a invalid data message</returns>
		public string GetDataInvalidMessage()
		{
			return "Gauge Layer: Data Invalid.";
		}

		private Hashtable _Grid = new Hashtable();
		/// <summary>
		/// Holds axes references.
		/// </summary>
		public  Hashtable Grid
		{
			get { return _Grid; }
			set { _Grid = value; }
		}

		private string _LayerID;
		/// <summary>
		/// Layer id. User to uniquely identifies in the deck of layers.
		/// </summary>
		public  string LayerID
		{
			get { return _LayerID; }
			set { _LayerID = value; }
		}

		private ChartCore _ChartCore;
		/// <summary>
		/// Reference to chart core.
		/// </summary>
		public  ChartCore ChartCore
		{
			get { return _ChartCore; }
			set { _ChartCore = value; }
		}

		private IChartData _ChartData;
		/// <summary>
		/// Reference to chart data.
		/// </summary>
		public  IChartData ChartData
		{
			get { return _ChartData; }
			set { _ChartData = value; }
		}

		private IColorModel _ChartColorModel;
		/// <summary>
		/// Reference to color model.
		/// </summary>
		public  IColorModel ChartColorModel
		{
			get { return _ChartColorModel; }
			set { _ChartColorModel = value; }
		}

		private bool _Visible;
		/// <summary>
		/// If this layer is visible.
		/// </summary>
		public  bool Visible
		{
			get { return _Visible; }
			set { _Visible = value; }
		}

		private IChartComponent _ChartComponent;
		/// <summary>
		/// Reference to chart component.
		/// </summary>
		public  IChartComponent ChartComponent
		{
			get { return _ChartComponent; }
			set { _ChartComponent = value; }
		}

		private Rectangle _OuterBound = new Rectangle(0,0,0,0);
		/// <summary>
		/// Outer bounds of layer.
		/// </summary>
		public  Rectangle OuterBound
		{
			get { return _OuterBound; }
			set 
			{
				_OuterBound = value; 
				CalculateInnerBounds();
			}
		}
		/// <summary>
		/// Calculate the innerbounds upon setting of outerbound.
		/// </summary>
		protected void CalculateInnerBounds() 
		{
			this.innerBounds = new Rectangle(this._OuterBound.X, this._OuterBound.Y, this._OuterBound.Width, this._OuterBound.Height);
		}
		#endregion
	}


	/// <summary>
	/// Direction of the gauge ticks.
	/// </summary>
	public enum Direction 
	{
		LeftToRight,
		RightToLeft
	}

	/// <summary>
	/// How should the gauge layer do the layout of dial and the needles. 
	/// This is used in places where the user want to draw more than one
	/// dial on a bigger dial. When using manual layout, the developer should
	/// GaugeAppearance with Radius and Center of the Gauge.
	/// </summary>
	public enum DialLayout 
	{
		Automatic,
		Manual
	}

	#region Gauge Appearance Class
	/// <summary>
	/// A class to hold the all the appearance items related to Gauge layer.
	/// </summary>
	public class GaugeAppearance 
	{
		private Point _Center;
		/// <summary>
		/// Gets/sets the center point location of the gauge. All needles
		/// are drawn from this location.
		/// </summary>
		public  Point Center
		{
			get { return _Center; }
			set { _Center = value; }
		}

		private int _Radius;
		/// <summary>
		/// Gets/sets the radius of the gauge. Larger values draw large gauge
		/// chart. User need to adjust Y-axis's tick-interval to accomodate 
		/// more ticks on large gauge.
		/// </summary>
		public  int Radius
		{
			get { return _Radius; }
			set { _Radius = value; }
		}

		private double _StartAngle = -45;
		/// <summary>
		/// Gets/sets the start angle of gauge dial.
		/// </summary>
		public  double StartAngle
		{
			get { return _StartAngle; }
			set { _StartAngle = value; }
		}

		private double _EndAngle = 180;
		/// <summary>
		/// Gets sets end angle of gauge dial.
		/// </summary>
		public  double EndAngle
		{
			get { return _EndAngle; }
			set { _EndAngle = value; }
		}

		private int _TickStart = 70;
		/// <summary>
		/// Gets/sets the tick start percentage. Considering center 
		/// of the dial as 0 and edge of the dial as 100: Tick start
		/// specifyies where to start putting the tick at. TickEnd 
		/// specifies where to end it.
		/// </summary>
		public  int TickStart
		{
			get { return _TickStart; }
			set { _TickStart = value; }
		}

		private int _TickEnd = 90;
		/// <summary>
		/// Gets/sets the tick end percentage. Considering center 
		/// of the dial as 0 and edge of the dial as 100: Tick end
		/// specifies where to end tick at. TickStart specifies 
		/// where to start it.
		/// </summary>
		public  int TickEnd
		{
			get { return _TickEnd; }
			set { _TickEnd = value; }
		}

		private int _TextLoc = 94;
		/// <summary>
		/// Gets/sets the label location percentage. Considering 
		/// center  of the dial as 0 and edge of the dial as 100: 
		/// TextLoc specifies where to place the label w.r.t. 
		/// center.
		/// </summary>
		public  int TextLoc
		{
			get { return _TextLoc; }
			set { _TextLoc = value; }
		}

		private Direction _Direction;
		/// <summary>
		/// Gets/sets the direction of gauge ticks which direction 
		/// specifies the increaing value of gauge ticks. 
		/// </summary>
		public  Direction Direction
		{
			get { return _Direction; }
			set { _Direction = value; }
		}

		private NeedleCollection _Needles = new NeedleCollection();
		/// <summary>
		/// Gets the collection of needles this gauge has. A Gauge can
		/// have more than one needle of various colors/shapes to indicate
		/// various values on same dial.
		/// </summary>
		public  NeedleCollection Needles
		{
			get { return _Needles; }
		}

		/// <summary>
		/// Gets the collection of gauge sections. Please see GaugeSection class'
		/// documentation on what a gauge section is. 
		/// </summary>
		private GaugeSectionCollection _Sections = new GaugeSectionCollection();
		public  GaugeSectionCollection Sections
		{
			get { return _Sections; }
		}

		private PaintElement _DialPE = new PaintElement(Color.White, Color.Blue, GradientStyle.Elliptical);
		/// <summary>
		/// Paint Element associated with Gauge's dial.
		/// </summary>
		[TypeConverter(typeof(PaintElementConverter))]
		public  PaintElement DialPE
		{
			get { return _DialPE; }
			set { _DialPE = value; }
		}

		private DialLayout _Layout = DialLayout.Automatic;
		/// <summary>
		/// Gets/sets how the dial should be layed out. Default is Automatic. Other ways 
		/// to lay dial out is "Manual". In this case developer must provide with point
		/// location indicating the center of dial. Developer must also provide with 
		/// radius of the dial. In case of "Automatic" it fills all the available area of
		/// chart and centers.
		/// </summary>
		public  DialLayout Layout
		{
			get { return _Layout; }
			set { _Layout = value; }
		}
	}

	#endregion

	#region Gauge Needle Class

	/// <summary>
	/// A class holding properties related to a Gauge needle, its look, 
	/// length and current value.
	/// </summary>
	public class Needle 
	{
		/// <summary>
		/// Create an instance of Gauge chart needle with default value = 0.
		/// </summary>
		public Needle() {}
		/// <summary>
		/// Create an instance of Gauge chart needle with given value.
		/// </summary>
		/// <param name="val">Value indicating current position of the needle.</param>
		public Needle(double val) 
		{
			this._Value = val;
		}
		/// <summary>
		/// Create an instance of Gauge chart needle with given value 
		/// </summary>
		/// <param name="val">Value indicating current position of the needle.</param>
		/// <param name="pe">Paint element decide on how the needle looks. 
		/// Since gauge layer draws a line using this paint element. Only values that
		/// effect the look of the needle are, StrokWidth, Stroke and Fill.</param>
		public Needle(double val, PaintElement pe) :this(val)
		{
			this._PE = pe;
		}

		private double _Value;
		/// <summary>
		/// Gets/sets present value of the needle.
		/// </summary>
		public  double Value
		{
			get { return _Value; }
			set { _Value = value; }
		}

		private PaintElement _PE = new PaintElement();
		/// <summary>
		/// Paint element decide on how the needle looks. 
		/// Since gauge layer draws a line using this paint element. Only values that
		/// effect the look of the needle are, StrokWidth, Stroke and Fill.
		/// </summary>
		[TypeConverter(typeof(PaintElementConverter))]
		public  PaintElement PE
		{
			get { return _PE; }
			set { _PE = value; }
		}

		private int _Length = 90;
		/// <summary>
		/// Percentatge length of the needle. Considering 
		/// center of the dial as 0 and edge of the dial as 100: 
		/// Length specifies how long the needle should be w.r.t 
		/// radius of the dial.
		/// </summary>
		public  int Length
		{
			get { return _Length; }
			set { _Length = value; }
		}
	}

	#endregion

	#region Gauge Needle Collection Class
	/// <summary>
	/// Collection of Needles. Gauge layer can draw dial with more than one needle.
	/// </summary>
	public class NeedleCollection : CollectionBase
	{
		/// <summary>
		/// Gets or sets the element at the specified index.
		/// In C#, this property is the indexer for the Needle Collection class
		/// <param name="value">The zero-based index of the element to get or set.</param>
		/// </summary>
		public Needle this[ int index ]
		{
			get
			{
				return( (Needle) List[index] );
			}
			set
			{
				List[index] = value;
			}
		}
		/// <summary>
		/// Adds an Needle to the end of the Needle Collection.
		/// </summary>
		/// <param name="value">A needle</param>
		/// <returns>The Needle Collection index at which the value has been added.</returns>
		public int Add( Needle value )  
		{
			return( List.Add( value ) );
		}
		/// <summary>
		/// Searches for the specified Needle and returns the zero-based index of the first occurrence within the entire NeedleCollection.
		/// </summary>
		/// <param name="value">Needle to search</param>
		/// <returns>The zero-based index of the first occurrence of given Needle within the entire Needle Collection, if found; otherwise, -1</returns>
		public int IndexOf( Needle value )  
		{
			return( List.IndexOf( value ) );
		}
		/// <summary>
		/// Inserts an element into the Needle Collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The Needle to insert.</param>
		public void Insert( int index, Needle value )  
		{
			List.Insert( index, value );
		}
		/// <summary>
		/// Removes the first occurrence of a specific Needle from the Needle Collection.
		/// </summary>
		/// <param name="value">A Needle</param>
		public void Remove( Needle value )  
		{
			List.Remove( value );
		}

		/// <summary>
		/// Determines whether the NeedleCollection contains a specific Needle.
		/// </summary>
		/// <param name="value">A Needle</param>
		/// <returns>true if the NeedleCollection contains the specified Needle; otherwise, false.</returns>
		public bool Contains( Needle value )  
		{
			// If value is not of type Needle, this will return false.
			return( List.Contains( value ) );
		}


		/// <summary>
		/// Performs additional custom processes before inserting a new element into the Needle Collection instance.
		/// </summary>
		/// <param name="index">The zero-based index at which to insert value. </param>
		/// <param name="value">The new value of the Needle at index. </param>
		protected override void OnInsert( int index, Object value )  
		{
			if ( value.GetType() != typeof(Needle) )
				throw new ArgumentException( "value must be of type Needle.", "value" );
		}
		/// <summary>
		/// Performs additional custom processes before removing from the Needle Collection instance.
		/// </summary>
		/// <param name="index">The zero-based index at which to insert value. </param>
		/// <param name="value">The value of the Needle to remove. </param>
		protected override void OnRemove( int index, Object value )  
		{
			if ( value.GetType() != typeof(Needle) )
				throw new ArgumentException( "value must be of type Needle.", "value" );
		}
		/// <summary>
		/// Performs additional custom processes before setting a value of new Needle into the Needle Collection instance.
		/// </summary>
		/// <param name="index">The zero-based index at which to insert value. </param>
		/// <param name="oldValue">The old value of the Needle at index. </param>
		/// <param name="newValue">The new value of the Needle at index. </param>
		protected override void OnSet( int index, Object oldValue, Object newValue )
		{
			if ( newValue.GetType() != typeof(Needle) )
				throw new ArgumentException( "newValue must be of type Needle.", "newValue" );
		}

		/// <summary>
		/// Performs additional custom processes when validating a Needle.
		/// </summary>
		/// <param name="value">The Needle to validate.</param>
		protected override void OnValidate( Object value )  
		{
			if ( value.GetType() != typeof(Needle) )
				throw new ArgumentException( "value must be of type Needle." );
		}
	}
	#endregion

	#region Gauge Section Class
	/// <summary>
	/// A class representing Gauge Section. On a Gauge there are sections that 
	/// have special meanings.  These sections are colored differently and range
	/// from certain values. For example, in a temperature gauge a gauge 
	/// section can indicate the cold temperatures with a blue color, normal
	/// temperatures with green and higher or dangerous temperatures with red.
	/// These sections stack up in a gauge collection to form gauge sections.
	/// </summary>
	public class GaugeSection 
	{

		/// <summary>
		/// Create a instance of Gauge Section with default values.
		/// </summary>
		public GaugeSection()  {}
	
		/// <summary>
		/// Create a instance of Gauge Section with given value.
		/// </summary>
		public GaugeSection(double val) 
		{
			this._Value = val;
		}
		/// <summary>
		/// Create a instance of Gauge Section with given value and color (paint element).
		/// </summary>
		public GaugeSection(double val, PaintElement pe) :this(val)
		{
			this._PE = pe;
		}

		private double _Value;
		/// <summary>
		/// Value of present section.
		/// </summary>
		public  double Value
		{
			get { return _Value; }
			set { _Value = value; }
		}

		private PaintElement _PE = new PaintElement();
		/// <summary>
		/// Gets/sets paint element that holds color or other painting information related to this section.
		/// </summary>
		[TypeConverter(typeof(PaintElementConverter)),NotifyParentProperty(true)]
		public  PaintElement PE
		{
			get { return _PE; }
			set { _PE = value; }
		}

		private int _StartWidth = 40;
		/// <summary>
		/// Gets/sets the section start percentage. Considering center 
		/// of the dial as 0 and edge of the dial as 100: start width
		/// specifyies where to start drawing the section at. 
		/// </summary>
		public  int StartWidth
		{
			get { return _StartWidth; }
			set 
			{ 
				if (value <= 0 || value >= this.EndWidth)
				{
					throw new ArgumentOutOfRangeException("StartWidth", value, "StartWidth must be a value greater than zero and less than EndWidth.");
				}
				_StartWidth = value; 
			}
		}

		private int _EndWidth = 80;
		/// <summary>
		/// Gets/sets the section end percentage. Considering center 
		/// of the dial as 0 and edge of the dial as 100: end width
		/// specifyies where to end drawing the section. 
		/// </summary>
		public  int EndWidth
		{
			get { return _EndWidth; }
			set 
			{
				if (value <= 0 || value <= this.StartWidth)
				{
					throw new ArgumentOutOfRangeException("EndWidth", value, "EndWidth must be a value greater than zero and greater than StartWidth");
				}
				_EndWidth = value; 
			}
		}
	}
	#endregion

	#region Gauge Section Collection Class
	/// <summary>
	/// Collection of GaugeSections. Gauge layer can draw dial with more than one GaugeSection.
	/// </summary>
	public class GaugeSectionCollection : CollectionBase
	{
		/// <summary>
		/// Gets or sets the element at the specified index.
		/// In C#, this property is the indexer for the GaugeSection Collection class
		/// <param name="value">The zero-based index of the element to get or set.</param>
		/// </summary>
		public GaugeSection this[ int index ]
		{
			get
			{
				return( (GaugeSection) List[index] );
			}
			set
			{
				List[index] = value;
			}
		}
		/// <summary>
		/// Adds an GaugeSection to the end of the GaugeSection Collection.
		/// </summary>
		/// <param name="value">A GaugeSection</param>
		/// <returns>The GaugeSection Collection index at which the value has been added.</returns>
		public int Add( GaugeSection value )  
		{
			return( List.Add( value ) );
		}
		/// <summary>
		/// Searches for the specified GaugeSection and returns the zero-based index of the first occurrence within the entire GaugeSectionCollection.
		/// </summary>
		/// <param name="value">GaugeSection to search</param>
		/// <returns>The zero-based index of the first occurrence of given GaugeSection within the entire GaugeSection Collection, if found; otherwise, -1</returns>
		public int IndexOf( GaugeSection value )  
		{
			return( List.IndexOf( value ) );
		}
		/// <summary>
		/// Inserts an element into the GaugeSection Collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The GaugeSection to insert.</param>
		public void Insert( int index, GaugeSection value )  
		{
			List.Insert( index, value );
		}
		/// <summary>
		/// Removes the first occurrence of a specific GaugeSection from the GaugeSection Collection.
		/// </summary>
		/// <param name="value">A GaugeSection</param>
		public void Remove( GaugeSection value )  
		{
			List.Remove( value );
		}

		/// <summary>
		/// Determines whether the GaugeSectionCollection contains a specific GaugeSection.
		/// </summary>
		/// <param name="value">A GaugeSection</param>
		/// <returns>true if the GaugeSectionCollection contains the specified GaugeSection; otherwise, false.</returns>
		public bool Contains( GaugeSection value )  
		{
			// If value is not of type GaugeSection, this will return false.
			return( List.Contains( value ) );
		}


		/// <summary>
		/// Performs additional custom processes before inserting a new element into the GaugeSection Collection instance.
		/// </summary>
		/// <param name="index">The zero-based index at which to insert value. </param>
		/// <param name="value">The new value of the GaugeSection at index. </param>
		protected override void OnInsert( int index, Object value )  
		{
			if ( value.GetType() != typeof(GaugeSection) )
				throw new ArgumentException( "value must be of type GaugeSection.", "value" );
		}
		/// <summary>
		/// Performs additional custom processes before removing from the GaugeSection Collection instance.
		/// </summary>
		/// <param name="index">The zero-based index at which to insert value. </param>
		/// <param name="value">The value of the GaugeSection to remove. </param>
		protected override void OnRemove( int index, Object value )  
		{
			if ( value.GetType() != typeof(GaugeSection) )
				throw new ArgumentException( "value must be of type GaugeSection.", "value" );
		}
		/// <summary>
		/// Performs additional custom processes before setting a value of new GaugeSection into the GaugeSection Collection instance.
		/// </summary>
		/// <param name="index">The zero-based index at which to insert value. </param>
		/// <param name="oldValue">The old value of the GaugeSection at index. </param>
		/// <param name="newValue">The new value of the GaugeSection at index. </param>
		protected override void OnSet( int index, Object oldValue, Object newValue )
		{
			if ( newValue.GetType() != typeof(GaugeSection) )
				throw new ArgumentException( "newValue must be of type GaugeSection.", "newValue" );
		}

		/// <summary>
		/// Performs additional custom processes when validating a GaugeSection.
		/// </summary>
		/// <param name="value">The GaugeSection to validate.</param>
		protected override void OnValidate( Object value )  
		{
			if ( value.GetType() != typeof(GaugeSection) )
				throw new ArgumentException( "value must be of type GaugeSection." );
		}
	}
	#endregion
}