using System;
using System.Collections;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Labels 的摘要说明。
	/// </summary>
	[Serializable]
	public class Labels:CollectionBase,ICloneable,IDisposable
	{
		public event EventHandler BeforeAdd;
		public Label this[int index]
		{
			get {  return List[index] as Label;  }
		}

		public int Add( Label value )  
		{
            //value.SetRuntimeHeight(value.Caption );
			value.KeepPos=true;
			AddArgs e=new AddArgs();
			if(BeforeAdd!=null)
				BeforeAdd(value,e);
			if(!e.bAlreadyAdd)
			{
				for(int i=0;i<Count;i++)
				{
					if(value.X<this[i].X)
						return Insert(i,value);
				}
				return( List.Add( value ) );
			}
			else
				return -1;
		}

        public void AutoLayout(int x)
        {
            Labels ls = new Labels();
            foreach (Label l in this)
            {
                if(l.Visible)
                    ls.AddByVisiblePosition(l);
            }
            this.Clear();
            for (int i=0;i<ls.Count;i++)
            {
                Label l = ls[i];
                if (i == 0)
                    l.X = x;
                else
                    l.X = ls[i - 1].X + ls[i - 1].Width;
                if (l is SuperLabel)
                    (l as SuperLabel).Labels.AutoLayout(l.X);
                this.InnerList.Add(l);
            }
            ls.Clear();
        }

        public void AddByVisiblePosition(Label value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (value.VisiblePosition < this[i].VisiblePosition)
                {
                    Insert(i, value);
                    return;
                }
            }
            InnerList.Add(value);
        }

		public int MetaHeight
		{
			get
			{
				if(Count==0)
					return 0;
				int top=Int32.MaxValue;
				int bottom=0;
				foreach(Label label in List)
				{
					if(label.Y <top)
						top=label.Y;
					if(label.Y+label.MetaHeight >bottom)
						bottom=label.Y+label.MetaHeight ;
				}
				return bottom-top;
			}
		}

		public int ExpandHeight
		{
			get
			{
				if(Count==0)
					return 0;
				int top=Int32.MaxValue;
				int bottom=0;
				foreach(Label label in List)
				{
					if(label.Y <top)
						top=label.Y;
					if(label.Y+label.ExpandHeight >bottom)
						bottom=label.Y+label.ExpandHeight ;
				}
				return bottom-top;
			}
		}

		public int IndexOf( Label value )  
		{
			return( List.IndexOf( value ) );
		}

		public int Insert( int index, Label value )  
		{
			List.Insert( index, value );
			return index;
		}

		public void Remove( Label value )  
		{
			List.Remove( value );
		}

		public bool Contains( Label value )  
		{
			return( List.Contains( value ) );
		}

        public void AdjustRuntimeHeight()
        {
            int height=0;
            foreach (Label label in List)
            {
                if (!(label is SuperLabel) &&  label.RuntimeHeight > height)
                    height = label.RuntimeHeight;
            }
            foreach (Label label in List)
            {
                if (!(label is SuperLabel))
                    label.AdjustRuntimeHeight(height);
            }
        }
		#region ICloneable 成员

		public object Clone()
		{
			Labels ls=new Labels();
			foreach(Label l in List)
			{
				ls.Add(l.Clone() as Label);
			}
			return ls;
		}

		#endregion

        #region IDisposable 成员

        public void Dispose()
        {
            foreach (Label label in List)
            {
                label.Dispose();
            }
            this.Clear();
        }

        #endregion
    }
}
