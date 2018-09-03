using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IGridCollect 的摘要说明。
	/// </summary>
	public interface IGridCollect:ICalculator,ICalculateSequence
	{
		bool bSummary{get;set;}
        bool bClue { get;set;}
        bool bColumnSummary { get;set;}
        bool bCalcAfterCross { get;set;}
	}

    public interface IGridEvent
    {
        EventType EventType { get;set;}
        bool bShowAtReal { get;set;}
    }

    public enum EventType
    {
        OnTitle,
        OnContent,
        OnSummary,
        BothContentAndSummary,
        OnAll
    }
}
