using System;
using System.Drawing;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// MetaCells 的摘要说明。
	/// </summary>
	[Serializable]
	public class Sections : CollectionBase,IDisposable,ICloneable
	{
        protected ReportStates _understate=ReportStates.Designtime;
        public ReportStates UnderState
        {
            get
            {
                return _understate;
            }
            set
            {
                _understate = value;
                foreach (Section section in List)
                    section.UnderState = value;
            }
        }
		#region common
		public Section this[int index]
		{
			get
			{  
				return List[index] as Section; 
			}
		}

        public Section this[string name, SectionType type]
        {
            get
            {
                if (name.ToLower() == "reportsummary")
                    return this[type];
                else
                    return this[name];
            }
        }

		public Section this[string name]
		{
			get
			{  
				for(int i=0;i<Count;i++)
				{
					if(this[i].Name.ToLower()==name.ToLower())
						return this[i];
				}
				return null;
			}
		}

        public Cells GetCellsWithSameIdentityCaption(string ic)
        {
            Cells cells = new Cells();
            foreach (Section section in this)
            {
                foreach (Cell cell in section.Cells)
                {
                    if (cell.IdentityCaption.ToLower() == ic.ToLower())
                        cells.AddDirectly(cell);
                }
            }
            return cells;
        }

		public Section this[SectionType sectiontype]
		{
			get
			{
				if(sectiontype==SectionType.GroupHeader||sectiontype==SectionType.GroupSummary)
					return null;
				for(int i=0;i<Count;i++)
				{
					if(this[i].SectionType==sectiontype ||
						(sectiontype==SectionType.Detail && (this[i].SectionType==SectionType.GridDetail || this[i].SectionType==SectionType.CrossDetail )))
						return this[i];
				}
				return null;
			}
		}

		public GroupHeader GetGroupHeader(int level)
		{
			for(int i=0;i<Count;i++)
			{
				if(this[i] is GroupHeader && (this[i] as GroupHeader).Level==level)
					return this[i] as GroupHeader;
			}
			return null;
		}

		public GroupSummary GetGroupSummary(int level)
		{
			for(int i=0;i<Count;i++)
			{
				if(this[i] is GroupSummary && (this[i] as GroupSummary).Level==level)
					return this[i] as GroupSummary;
			}
			return null;
		}

		public int Add(Section value)
		{
            value.UnderState = _understate;
			if(value.Name=="")
				return AddAtDesigntime(value);
			else
				return AddAtRuntime(value);
		}

		public int IndexOf( Section value )  
		{
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, Section value )  
		{
			List.Insert( index, value );
		}

		public void Remove( Section value )  
		{
			if(value==null)
				return;
            if (value.SectionType == SectionType.Detail)
                return;
			if(value is GroupHeader)
			{
				int levels=GroupLevels;
				int level=value.Level;
				for(int i=level+1;i<=levels;i++)
				{
					GroupHeader gh=this.GetGroupHeader(i);
					gh.Level--;
					gh.Name="GroupHeader-"+gh.Level.ToString();
                    gh.Caption = "U8.UAP.Services.ReportElements.Sections.级分组区";
                    GroupSummary gs = this.GetGroupSummary (i);
					gs.Level--;
					gs.Name="GroupSummary-"+gs.Level.ToString();
                    gs.Caption = "U8.UAP.Services.ReportElements.Sections.级分组汇总区";
				}
			}
			List.Remove( value );
		}

		public bool Contains( Section value )  
		{
			return( List.Contains( value ) );
		}
		#endregion

		#region Runtime
		private int AddAtRuntime( Section value )  
		{			
//			for(int i=0;i<Count;i++)
//			{
//				if(value.Y<this[i].Y)
//				{
//					Insert(i,value);
//					return i;
//				}
//			}
			return( List.Add( value ) );
		}

		public int GroupLevels
		{
			get
			{
				int grouplevel=0;
				for(int i=0;i<Count;i++)
				{
					if(this[i] is GroupHeader && (this[i] as GroupHeader).Level>grouplevel)
						grouplevel=(this[i] as GroupHeader).Level;
				}
				return grouplevel;
			}
		}	

		public int X
		{
			get
			{
				int _x=Int32.MaxValue;
				foreach(Section value in List)
				{
					if(value.X <_x)
						_x=value.X ;					
				}
				return _x;
			}
		}

		public int Right
		{
			get
			{
				int _right=Int32.MinValue;
				foreach(Section value in List)
				{
					if(value.Width>_right)
						_right=value.Width;
				}
				return _right;
			}
		}

        public void AutoLayOutAtRuntimeAll()
        {
            foreach (Section section in List)
            {
                if (section.SectionType != SectionType.ReportHeader &&
                    section.SectionType != SectionType.PageHeader &&
                    section.SectionType != SectionType.PageFooter &&
                    section.SectionType != SectionType.PrintPageSummary &&
                    section.SectionType != SectionType.PrintPageTitle)
                    section.AutoLayOutAtRuntimeAll();
            }
        }
		#endregion

		#region Designtime
		public Section this[Point pt]
		{
			get
			{
				for(int i=0;i<Count;i++)
				{
					if(this[i].getRects().Contains(pt))
						return this[i];
				}
				return null;
			}
		}
	
		private int AddAtDesigntime( Section value )  
		{
			if(this[value.SectionType]!=null)
				return -1;
			if(value is GroupSection)
				calcText(value);
			else
				value.Name=value.Type;
			int index=getInsertPos(value);
			if (index!=-1)
			{
				Insert(index,value);				
			}
			else
			{
				index= List.Add(value);
			}
			calcPosition();
			return index;
		}

		public void calcPosition()
		{
            if (_understate != ReportStates.Designtime)
                return;
			if(Count>0)
				this[0].Y=0;
			for(int i=1;i<Count;i++)
			{
				Section section=this[i];
				Section presection=this[i-1];
				section.Y=presection.Y+presection.Height;
			}
		}

		private int getInsertPos(Section c)
		{
			for(int i=0;i<this.Count;i++)
			{
				if(c is GroupSummary)
				{
					if(c.OrderID <=this[i].OrderID )
						return i;  //addto i
				}
				else
				{
					if(c.OrderID <this[i].OrderID )
						return i;  //addto i
				}
			}
			return -1;//addto the tail
		}

		private void calcText(Section c)
		{
			string text;
			string name;
			int Num=1;
			if(c is GroupHeader )
			{
				name="GroupHeader-"+Num.ToString();
                text = "U8.UAP.Services.ReportElements.Sections.级分组区";
				while (FindSection(SectionType.GroupHeader ,name))
				{
					Num++;
					name="GroupHeader-"+Num.ToString();
                    text = "U8.UAP.Services.ReportElements.Sections.级分组区";					
				}
				c.Name=name;
				c.Caption =text;
				c.Level=Num;
			}
			else
			{
				name="GroupSummary-"+Num.ToString();
                //text = Num.ToString() + UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Sections.级分组汇总区", ServerReportContext.ReportSession.LocaleId);
                text = "U8.UAP.Services.ReportElements.Sections.级分组汇总区";
                while (FindSection(SectionType.GroupSummary ,name))
				{
					Num++;
					name="GroupSummary-"+Num.ToString();
                    text = "U8.UAP.Services.ReportElements.Sections.级分组汇总区";
                    //text = Num.ToString() + UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Sections.级分组汇总区", ServerReportContext.ReportSession.LocaleId);					
				}
				c.Name=name;
                c.Level = Num;
				c.Caption =text;				
			}
		}

		private bool FindSection(SectionType type,string name)
		{
			for(int i=0;i<this.Count;i++)
			{
				if(this[i].SectionType ==type && this[i].Name ==name)
					return true;
			}
			return false;
		}

		public int TotalHeight
		{
			get
			{
				if(Count>0)
				{
					Section section=this[Count-1];
					return section.Y+section.Height;
				}
				else
					return 0;
			}
		}
		#endregion

		#region IDisposable 成员

		public void Dispose()
		{
			foreach(Section value in List)
			{
				if(value!=null)
				{
					value.Dispose();
				}
			}
			this.Clear();
		}

		#endregion

		#region ICloneable 成员

		public object Clone()
		{
			Sections sections=new Sections();
            sections.UnderState = _understate;
			foreach(Section section in this.InnerList)
				sections.Add(section.Clone() as Section);//DeepClone
			return sections;
		}

		#endregion
	}
}
