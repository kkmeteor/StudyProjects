using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdReference 的摘要说明。
	/// </summary>
	public class CmdReference:Command
	{
		public CmdReference()
		{
			_commandtype=CommandTypes.Reference;
		}

		protected override void AddOldState(object sg)
		{
			_oldstates.Add((sg as Report).Sections.Clone() );
			_oldstates.Add((sg as Report).GroupSchemas.Clone());
		}

		public override void UpdateState(object sg)
		{
			_receivers[0]=sg;
			_newstates.Add((sg as Report).Sections.Clone() );
			_newstates.Add((sg as Report).GroupSchemas.Clone() );
		}

		public override void Undo()
		{
			Report report=_receivers[0] as Report ;
			report.Sections=_oldstates[0] as Sections;
			report.GroupSchemas=_oldstates[1] as GroupSchemas;
		}

		public override void Redo()
		{
			Report report=_receivers[0] as Report ;
			report.Sections=_newstates[0] as Sections;
			report.GroupSchemas=_newstates[1] as GroupSchemas;
		}
	}
}
