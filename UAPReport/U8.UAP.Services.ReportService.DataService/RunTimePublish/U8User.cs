namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// U8操作员信息
	/// </summary>
	public class U8User
	{
		private string	_id			= string.Empty;
		private string	_name		= string.Empty;
		private string	_eMail		= string.Empty;
		private string	_department	= string.Empty;
		private string	_phoneNo	= string.Empty;

		public U8User() { }
		public U8User(string id)
		{
			this._id = id;
		}

		public string ID
		{
			get{ return _id; }
			set{ _id = value; }
		}

		public string PhoneNo
		{
			get{ return this._phoneNo; }
			set{ this._phoneNo = value; }
		}
		
		public string Name
		{
			get{ return _name; }
			set{ _name = value; }
		}

		public string EMail
		{
			get{ return _eMail; }
			set{ _eMail = value; }
		}

		public string Department
		{
			get{ return _department; }
			set{ _department = value; }
		}
	}
}
