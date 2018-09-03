using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdAddSection 的摘要说明。
	/// </summary>
	public class CmdAddSection:Command
	{
		private Sections _sections;
		public CmdAddSection(Sections sections):base()
		{
			_sections=sections;
			_commandtype=CommandTypes.AddSection;
		}

		public override void Undo()
		{
			for(int i=0;i<_receivers.Count;i++)
				_sections.Remove(_receivers[i] as Section);
		}

		public override void Redo()
		{
            for (int i = 0; i < _receivers.Count; i++)
            {
                Section section=_receivers[i] as Section;
                section.Name = "";
                _sections.Add(section );
            }
		}
	}
}
