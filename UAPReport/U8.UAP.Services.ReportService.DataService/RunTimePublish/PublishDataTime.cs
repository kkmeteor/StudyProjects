using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 发布的时间信息
	/// </summary>
	internal class PublishDataTime : ICloneable
	{	
		private ReportDataPublishTimeType		_publishTimeType	= ReportDataPublishTimeType.Immediately;
		private ReportDataGivenTimeType			_givenTimeType		= ReportDataGivenTimeType.CycleTime;
		private ReportDataCycleTimeType			_cycleTimeType		= ReportDataCycleTimeType.EveryDay;
		
		private DateTime		_absoluteTime = DateTime.Now;
		private DateTime		_firstExecuteTime = DateTime.Now;
		private DateTime		_createTime = DateTime.Now;
		private DateTime		_modifyTime = DateTime.Now;

		public object Clone()
        {
            return this.MemberwiseClone();
        }

		public ReportDataPublishTimeType PublishTimeType
		{
			get{ return _publishTimeType; }
			set{ _publishTimeType = value; }
		}

		public ReportDataGivenTimeType GivenTimeType		
		{
			get{ return _givenTimeType; }
			set{ _givenTimeType = value; }
		}

		public ReportDataCycleTimeType CycleTimeType
		{
			get{ return _cycleTimeType; }
			set{ _cycleTimeType = value; }
		}

		public System.DateTime AbsoluteTime		
		{
			get{ return _absoluteTime; }
			set{ _absoluteTime = value; }
		}

		public DateTime FirstExecuteTime		
		{
			get{ return _firstExecuteTime; }
			set{ _firstExecuteTime = value; }
		}

		public DateTime CreateTime
		{
			get{ return _createTime; }
			set{ _createTime = value; }
		}

		public DateTime ModifyTime
		{
			get{ return _modifyTime; }
			set{ _modifyTime = value; }
		}
	}
}
