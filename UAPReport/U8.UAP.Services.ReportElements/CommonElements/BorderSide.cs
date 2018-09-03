using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// BorderSide 的摘要说明。
	/// </summary>
	/// 
	[Serializable]
	[ComVisibleAttribute(true)]
	[TypeConverterAttribute(typeof(BorderSideTypeConverter))]
	public class BorderSide:ICloneable,ISerializable
	{
		private bool _bLeft=false;
		private bool _bTop=false;
		private bool _bRight=false;
		private bool _bBottom=false;

		public event EventHandler BeforeBorderChanged;
		public event EventHandler AfterBorderChanged;

		public BorderSide()
		{
		}

		public BorderSide(SerializationInfo info, StreamingContext context)
		{
			_bLeft=info.GetBoolean("Left");
			_bTop=info.GetBoolean("Top");
			_bRight=info.GetBoolean("Right");
			_bBottom=info.GetBoolean("Bottom");
		}

		public BorderSide(bool l,bool t,bool r,bool b)
		{
			_bLeft=l;
			_bTop=t;
			_bRight=r;
			_bBottom=b;
		}

		public BorderSide(BorderSide borderside)
		{
			_bLeft=borderside.Left;
			_bTop=borderside.Top;
			_bRight=borderside.Right;
			_bBottom=borderside.Bottom;
		}

		private void OnBeforeBorderChanged()
		{
			if (BeforeBorderChanged!=null)
				BeforeBorderChanged(this,null);
		}

		private void OnAfterBorderChanged()
		{
			if (AfterBorderChanged!=null)
				AfterBorderChanged(this,null);
		}
		
		[DefaultValueAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool Left
		{
			get
			{
				return _bLeft;
			}
			set
			{
				OnBeforeBorderChanged();
				if(_bLeft!=value)
				{
					_bLeft=value;
					OnAfterBorderChanged();
				}
			}
		}

		[DefaultValueAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool Right
		{
			get
			{
				return _bRight;
			}
			set
			{
				OnBeforeBorderChanged();
				if(_bRight!=value)
				{
					_bRight=value;
					OnAfterBorderChanged();
				}
			}
		}

		[DefaultValueAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool Top
		{
			get
			{
				return _bTop;
			}
			set
			{
				OnBeforeBorderChanged();
				if(_bTop!=value)
				{
					_bTop=value;
					OnAfterBorderChanged();
				}
			}
		}

		[DefaultValueAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool Bottom
		{
			get
			{
				return _bBottom;
			}
			set
			{
				OnBeforeBorderChanged();
				if(_bBottom!=value)
				{
					_bBottom=value;
					OnAfterBorderChanged();
				}
			}
		}

		public void AllBorder()
		{
			_bLeft=true;
			_bTop=true;
			_bRight=true;
			_bBottom=true;
		}

		public void NoneBorder()
		{
			_bLeft=false;
			_bTop=false;
			_bRight=false;
			_bBottom=false;
		}

		#region ICloneable 成员

		public object Clone()
		{
			return new BorderSide(this);
		}

		#endregion

		#region ToString-FromString
//		public static string ToString(BorderSide bs)
//		{
//			MemoryStream fs=new MemoryStream();
//			BinaryFormatter formatter = new BinaryFormatter();
//			try 
//			{
//				formatter.Serialize(fs, bs);
//				return Convert.ToBase64String(fs.ToArray());
//			}
//			catch (SerializationException ex) 
//			{
//				throw ex;
//			}
//			finally 
//			{
//				fs.Close();
//			}
//		}
//
//		public static BorderSide FromString(string s)
//		{
//			MemoryStream ms=new MemoryStream(Convert.FromBase64String(s));
//			try 
//			{
//				BinaryFormatter formatter = new BinaryFormatter();
//				return (BorderSide) formatter.Deserialize(ms);
//			}
//			catch (SerializationException ex) 
//			{
//				throw ex;
//			}
//			finally 
//			{
//				ms.Close();
//			}
//		}
		#endregion

		#region ISerializable 成员

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Left",_bLeft);
			info.AddValue("Top",_bTop);
			info.AddValue("Right",_bRight);
			info.AddValue("Bottom",_bBottom);
		}

		#endregion
	}
}
