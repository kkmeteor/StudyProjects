using System;
using System.Windows.Forms;
using System.Runtime.Serialization;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GlobalVarient 的摘要说明。
	/// </summary>
	[Serializable]
	public class GlobalVarient:ISerializable,ICloneable
	{
		private string _name="";
		private VarientType _type;
		private string _value;
		public event VarientNameCheckHandler NameCheck;
		public event VarientValidateHandler Validate;
        public event EventHandler NameChanged;
		public GlobalVarient()
		{			
		}

		public GlobalVarient(string name,VarientType type,string value)
		{			
			_name=name;
			_type=type;
			_value=value;
		}

		public GlobalVarient(GlobalVarient gv)
		{			
			_name=gv.Name ;
			_type=gv.Type ;
			_value=gv.Value;
		}

		private GlobalVarient(SerializationInfo info, StreamingContext context)
		{
			_name=info.GetString("Name");
			_type=(VarientType)info.GetValue("Type",typeof(VarientType));
			_value=info.GetString("Value");
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if(_name.ToLower().Trim()!=value.ToLower().Trim())
				{
					string bvalid="";
					if(NameCheck!=null)
						bvalid=NameCheck(value);
                    if (bvalid != "")
                        MessageBox.Show(bvalid,String4Report.GetString("Report"));
                    else
                    {
                        _name = value;
                        if (NameChanged != null)
                            NameChanged(this, null);
                    }
				}
			}
		}

		public VarientType Type
		{
			get
			{
				return _type;
			}
			set
			{
				if(_type !=value)
				{
					string bvalid="";
					if(Validate!=null)
						bvalid=Validate(_name,value,_value);
					if(bvalid!="")
                        MessageBox.Show(bvalid, String4Report.GetString("Report"));
					else
						_type=value;
				}
			}
		}

		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				if(_value!=value)
				{
					string bvalid="";
					if(Validate!=null)
						bvalid=Validate(_name,_type,value);
					if(bvalid!="")
                        MessageBox.Show(bvalid, String4Report.GetString("Report"));
					else
						_value=value;
				}
				
			}
		}
		#region ISerializable 成员

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
            info.AddValue("Version", 1);
			info.AddValue("Name",_name);
			info.AddValue("Type",_type);
			info.AddValue("Value",_value);
		}

		#endregion

		#region ICloneable 成员

		public object Clone()
		{
			return new GlobalVarient(this);
		}

		#endregion
	}

	public delegate string VarientNameCheckHandler(string name);
	public delegate string VarientValidateHandler(string name,VarientType type,string value);

	public enum VarientType
	{
		Object,
		Decimal,
		DateTime,
		String,
		SQL_Decimal,
		SQL_DateTime,
		SQL_String
	}
}
