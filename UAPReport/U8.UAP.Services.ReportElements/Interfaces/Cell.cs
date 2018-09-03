using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Cell 的摘要说明。
	/// </summary>
	public interface Cell:Drawing
	{
	}

	public interface IGroup
	{
	}

    public interface IGap
    {
        int GapHeight { get;set;}
    }

    public interface IWithSizable
    {
    }

    public interface ICenterAlign
    {
        bool CenterAlign { get; set; }
    }

    public interface IBarCode
    {
        Neodynamic.WinControls.BarcodeProfessional.Symbology Symbology { get;set;}
    }

    public interface IApplyColorStyle
    {
        bool bApplyColorStyle { get;set;}
    }

    public interface ILabelType
    {
        LabelType LabelType { get;set;}
    }

    public interface IUserDefine
    {
        string UserDefineItem { get;set;}
    }

    public interface IInformationSender
    {
        string InformationID { get;set;}
        //string InformationTaker { get;set;}
    }

    public interface IGroupDimensionStyle
    {
        bool UseColumnStyle { get;set;}
    }

	public enum DrawingType
	{
		PageHeader,
		PageTitle,
		ReportHeader,
		GroupHeader,
		Detail,
		GroupSummary,
		PageSummary,
		ReportSummary,
		PageFooter,
		CrossDetail,
		DBBoolean,
		DBDecimal,
		DBDateTime,
		DBText,
		ExchangeRate,
		DBImage,
		Image,
		Label,
		SuperLabel,
		SubReport,
		Line
	}

	public enum ReportStates
	{
		Browse,
		Preview,
		Print,
		PrintPreview,
		Static,
		Designtime,
		WebBrowse,
        OutU8,
        CrossView
	}

	public enum ReportType
	{
	    None = 0,
	    GridReport,
	    FreeReport,
	    CrossReport,
	    IndicatorReport,
	    BpmReport,
        MetrixReport
	}

    public  enum ShowStyle
    {
        Normal,
        NoGroupHeader,
        NoGroupSummary,
        None
    }
}
