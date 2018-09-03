using System;
using System.ComponentModel; 
using System.Windows.Forms;
using System.Drawing ;
using System.Globalization;
using System.Windows.Forms.Design ;
using System.Drawing.Design;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// 用来制订显示属性页里面字段的名称
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DisplayTextAttribute : Attribute
	{
		private string _displayText = "";
		public DisplayTextAttribute(string displayText)
		{
			_displayText = displayText;
		}

		public string DisplayText
		{
			get
			{
				if(this._displayText.Substring(0,1).ToLower()=="u")
                    return U8ResService.GetResString(this._displayText, System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
				else
					return this._displayText;
			}
		}
		public override string ToString()
		{
			if(this._displayText.Substring(0,1).ToLower()=="u")
                return U8ResService.GetResString(this._displayText, System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
			else
				return this._displayText;
		}

	}
	public class DisplyTextPropertyDescriptor : PropertyDescriptor
	{
		private PropertyDescriptor basePropertyDescriptor; 
  
		public DisplyTextPropertyDescriptor(PropertyDescriptor basePropertyDescriptor) : base(basePropertyDescriptor)
		{
			this.basePropertyDescriptor = basePropertyDescriptor;
		}
		public override bool CanResetValue(object component)
		{
			return basePropertyDescriptor.CanResetValue(component);
		}
		public override Type ComponentType
		{
			get { return basePropertyDescriptor.ComponentType; }
		}
		public override string DisplayName
		{
			get 
			{
				string svalue  = "";
				foreach(Attribute attribute in this.basePropertyDescriptor.Attributes)
				{
					if (attribute is DisplayTextAttribute )
					{
						svalue = attribute.ToString();
						break;
					}
				}
				if (svalue == "") return this.basePropertyDescriptor.Name;
				else return svalue;
			}
		}
		public override string Description
		{
			get
			{
				return this.basePropertyDescriptor.Description;
			}
		}
		public override object GetValue(object component)
		{
			return this.basePropertyDescriptor.GetValue(component);
		}
		public override bool IsReadOnly
		{
			get { return this.basePropertyDescriptor.IsReadOnly; }
		}
		public override string Name
		{
			get { return this.basePropertyDescriptor.Name; }
		}
		public override Type PropertyType
		{
			get { return this.basePropertyDescriptor.PropertyType; }
		}
		public override void ResetValue(object component)
		{
			this.basePropertyDescriptor.ResetValue(component);
		}
		public override bool ShouldSerializeValue(object component)
		{
			return this.basePropertyDescriptor.ShouldSerializeValue(component);
		}
		public override void SetValue(object component, object value)
		{
			this.basePropertyDescriptor.SetValue(component, value);
			System.Attribute[] attrs = this.AttributeArray;
			for(int i=0;i<attrs.Length;i++)
			{
				if(attrs[i] is BrowsableAttribute)
				{
					attrs[i] =new BrowsableAttribute(false);
				}
			}
		
			//this.basePropertyDescriptor.Attributes.
			//			
			//			if(VoucherOrArch.IsVoucher)
			//				return;
			//			string svalue="";
			//			bool b = false;
			//			foreach(Attribute attribute in this.basePropertyDescriptor.Attributes)
			//			{
			//				if (attribute is DisplayTextAttribute )
			//				{
			//					svalue = attribute.ToString();
			//					if(svalue == "是否有表体" )
			//					{
			//						b=true;
			//					}
			//				}
			//			}
			//			if(!b)
			//				return;
			//			foreach(Attribute attribute in this.basePropertyDescriptor.Attributes)
			//			{
			//				if (attribute is BrowsableAttribute)
			//				{
			//					((Browsable
			//					break;
			//				}
			//			}
		}
	}

	[Serializable]
	public class DisplyTextCustomTypeDescriptor : ICustomTypeDescriptor
	{
		public String GetClassName()
		{
			return TypeDescriptor.GetClassName(this,true);
		}
		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this,true);
		}
		public String GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}
		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}
		public EventDescriptor GetDefaultEvent() 
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}
		public PropertyDescriptor GetDefaultProperty() 
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}
		public object GetEditor(Type editorBaseType) 
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}
		public EventDescriptorCollection GetEvents(Attribute[] attributes) 
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}
		public EventDescriptorCollection GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}
		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection baseProps = TypeDescriptor.GetProperties(this, attributes, true);
			PropertyDescriptorCollection descriptorList = new PropertyDescriptorCollection(null);
			foreach( PropertyDescriptor oProp in baseProps )
			{
				descriptorList.Add(new DisplyTextPropertyDescriptor(oProp));
			}
			return descriptorList;
		}
		public PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection baseProps = TypeDescriptor.GetProperties(this, true);
			PropertyDescriptorCollection descriptorList = new PropertyDescriptorCollection(null);
			foreach( PropertyDescriptor oProp in baseProps )
			{
				descriptorList.Add(new DisplyTextPropertyDescriptor(oProp));
			}
			return descriptorList;
		}
		public object GetPropertyOwner(PropertyDescriptor pd) 
		{
			return this;
		}
	}
}
