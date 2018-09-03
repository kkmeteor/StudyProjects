using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CmdDeleteSection 的摘要说明。
	/// </summary>
	public class CmdDeleteSection:Command
	{
		private Sections _sections;
		public CmdDeleteSection(Sections sections):base()
		{
			_sections=sections;
			_commandtype=CommandTypes.DeleteSection ;
		}

		public override void Undo()
		{
            for (int i = 0; i < _receivers.Count; i++)
            {
                Section section = _receivers[i] as Section;
                section.Name = "";
                _sections.Add(section);
            }
		}

		public override void Redo()
		{
			for(int i=0;i<_receivers.Count;i++)
				_sections.Remove(_receivers[i] as Section);
		}
	}
}
