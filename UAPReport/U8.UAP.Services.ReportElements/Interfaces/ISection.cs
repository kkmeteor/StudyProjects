using System;
using System.Data;
using System.Collections;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// IBound 的摘要说明。
	/// </summary>
	public interface ISection
	{		
		int OrderID{get;}
		SectionType SectionType{get;}
		int Level{get;set;}

		event EventHandler BottomChanged;
		event EventHandler HeightChanged;
    }

    public interface IAutoSequence
    {
        bool bAutoSequence { get;set;}
    }

    public interface IAlternativeStyle
    {
        Color BackColor2 { get;set;}
        Color BorderColor2 { get;set;}
        Color ForeColor2 { get;set;}
        ServerFont  ServerFont2 { get;}
        bool bApplyAlternative { get;set;}
        bool bApplySecondStyle { get;set;}
        bool bAlreadySetSecondStyle { get;set;}
    }

    public interface IAutoDesign
    {
        void AutoDesign();
        void AutoDesign(int x);
        void AutoDesign(Cells cells);
        void AutoDesign(int y,int height);
        void AutoDesignSuperLabel();
    }

    public interface IAddWhenDesign
    {
        bool bAddWhenDesign { get;set;}
    }

	public enum SectionType
	{
		PageHeader,
		PageTitle,
		ReportHeader,
		GroupHeader,
		Detail,
		GroupSummary,
		PrintPageSummary,
		ReportSummary,
		PageFooter,
		GridDetail,
		CrossRowHeader,
		CrossColumnHeader,
		CrossDetail,
        PrintPageTitle,
        IndicatorDetail,
        None
	}
}
