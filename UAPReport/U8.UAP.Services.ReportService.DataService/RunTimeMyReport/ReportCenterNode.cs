/*
 * 作者:卢达其
 * 时间:2009.1.13
 * 
 * 890重构“报表中心”:
 * 1.提高加载速度
 * 2.剔除不必要的功能
 */

using System;
using System.Xml.Serialization;
using System.Drawing;
using System.Runtime.InteropServices;
using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class ReportCenterNode : IReportCenterNode, ICloneable
	{
		public const string KeyImageAffixNew = "New";
		public const string KeyImageAffixLink = "Link";
		public const string KeyImageAffixMissing = "Missing";

		private bool _isNew = false;
		private bool _isLink = false;
		private bool _isMissing = false;

		private string _id = null;
        private string _text = null;
        private string _imageKey = null;
		private string _imageKeyOpen = null;

		private Image _image = null;
        private Image _imageOpen = null;
		private Reports _reports = new Reports();
		private IOpenReportCenterNode _openEngine = null;
        private string _solutionId = null;

		public virtual object Clone()
		{
			return this.MemberwiseClone();
		}

		public IOpenReportCenterNode OpenEngine 
		{
			get { return this._openEngine;}
			set { this._openEngine = value;}
		}

		public bool IsNew 
		{
			get { return this._isNew;}
			set { this._isNew = value;}
		}

		public bool IsLink 
		{
			get { return this._isLink;}
			set { this._isLink = value;}
		}

		public bool IsMissing 
		{
			get { return this._isMissing;}
			set { this._isMissing = value;}
		}

		public Reports Reports
		{
			get{ return this._reports; }
		}

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public virtual string Text
        {
            get { return _text; }
            set { _text = value; }
        }

		public virtual string ImageKey
        {
            get { return ReportNode.GetImageKay(_imageKey, this); }
            set { _imageKey = value; }
        }

		public string ImageKeyRaw
        {
            get { return this._imageKey; }
        }

		public string ImageKeyOpen
        {
            get 
			{ 
				string key = this._imageKeyOpen;
				if(string.IsNullOrEmpty(key))
					key = this._imageKey;
				return ReportNode.GetImageKay(key, this); 
			}
            set { _imageKeyOpen = value; }
        }

		public Image Image
        {
            get { return _image; }
			set { _image = value;  }
        }

        public Image ImageOpen
        {
            get { return _imageOpen; }
			set { _imageOpen = value; }
        }
        /// <summary>
        /// 查询方案的ID,只有收藏时候使用
        /// </summary>
        public string SolutionId
        {
            get { return _solutionId; }
            set { _solutionId = value; }
        }
		/// <summary>
		/// 图标键值规则:假设源键值为DynamicReport,
		/// 当其为新产生的节点，则键值为DynamicReportNew;
		/// 当其为自定义节点，则键值为DynamicReportLink;
		/// 以上都不满足时，则键值为DynamicReport
		/// </summary>
		public static string GetImageKay(string key, IReportCenterNode ircn)
		{
			if (ircn.IsLink)
			{
				if(ircn.IsMissing)
					return key + ReportNode.KeyImageAffixMissing;
				return key + ReportNode.KeyImageAffixLink;
			}

			if(ircn.IsNew)
				return key + ReportNode.KeyImageAffixNew;
			return key;
		}

		public virtual void Open()
		{
			if(this.OpenEngine != null)
				this.OpenEngine.Open(this);
		}
	}
}