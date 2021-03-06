//
// ParamterType.cs.cs
//
// This file was generated by XMLSpy 2005 Enterprise Edition.
//
// YOU SHOULD NOT MODIFY THIS FILE, BECAUSE IT WILL BE
// OVERWRITTEN WHEN YOU RE-RUN CODE GENERATION.
//
// Refer to the XMLSpy Documentation for further details.
// http://www.altova.com/xmlspy
//


using System;
using System.Collections;
using System.Xml;
using UFIDA.U8.UAP.Services.ReportData.Altova.Types;

namespace UFIDA.U8.UAP.Services.ReportData.ReportQueryXml
{
	public class ParamterType : Altova.Xml.Node
	{
		#region Forward constructors
		public ParamterType() : base() { SetCollectionParents(); }
		public ParamterType(XmlDocument doc) : base(doc) { SetCollectionParents(); }
		public ParamterType(XmlNode node) : base(node) { SetCollectionParents(); }
		public ParamterType(Altova.Xml.Node node) : base(node) { SetCollectionParents(); }
		#endregion // Forward constructors

		public override void AdjustPrefix()
		{

			for (int i = 0; i < DomChildCount(NodeType.Attribute, "", "Name"); i++)
			{
				XmlNode DOMNode = GetDomChildAt(NodeType.Attribute, "", "Name", i);
				InternalAdjustPrefix(DOMNode, false);
			}

			for (int i = 0; i < DomChildCount(NodeType.Attribute, "", "Type"); i++)
			{
				XmlNode DOMNode = GetDomChildAt(NodeType.Attribute, "", "Type", i);
				InternalAdjustPrefix(DOMNode, false);
			}

			for (int i = 0; i < DomChildCount(NodeType.Attribute, "", "Size"); i++)
			{
				XmlNode DOMNode = GetDomChildAt(NodeType.Attribute, "", "Size", i);
				InternalAdjustPrefix(DOMNode, false);
			}

			for (int i = 0; i < DomChildCount(NodeType.Attribute, "", "Value"); i++)
			{
				XmlNode DOMNode = GetDomChildAt(NodeType.Attribute, "", "Value", i);
				InternalAdjustPrefix(DOMNode, false);
			}
		}


		#region Name accessor methods
		public int GetNameMinCount()
		{
			return 0;
		}

		public int NameMinCount
		{
			get
			{
				return 0;
			}
		}

		public int GetNameMaxCount()
		{
			return 1;
		}

		public int NameMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetNameCount()
		{
			return DomChildCount(NodeType.Attribute, "", "Name");
		}

		public int NameCount
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "Name");
			}
		}

		public bool HasName()
		{
			return HasDomChild(NodeType.Attribute, "", "Name");
		}

		public SchemaString GetNameAt(int index)
		{
			return new SchemaString(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "Name", index)));
		}

		public XmlNode GetStartingNameCursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "Name" );
		}

		public XmlNode GetAdvancedNameCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "Name", curNode );
		}

		public SchemaString GetNameValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaString( curNode.Value );
		}


		public SchemaString GetName()
		{
			return GetNameAt(0);
		}

		public SchemaString Name
		{
			get
			{
				return GetNameAt(0);
			}
		}

		public void RemoveNameAt(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "Name", index);
		}

		public void RemoveName()
		{
			while (HasName())
				RemoveNameAt(0);
		}

		public void AddName(SchemaString newValue)
		{
			AppendDomChild(NodeType.Attribute, "", "Name", newValue.ToString());
		}

		public void InsertNameAt(SchemaString newValue, int index)
		{
			InsertDomChildAt(NodeType.Attribute, "", "Name", index, newValue.ToString());
		}

		public void ReplaceNameAt(SchemaString newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "Name", index, newValue.ToString());
		}
		#endregion // Name accessor methods

		#region Name collection
        public NameCollection	MyNames = new NameCollection( );

        public class NameCollection: IEnumerable
        {
            ParamterType parent;
            public ParamterType Parent
			{
				set
				{
					parent = value;
				}
			}
			public NameEnumerator GetEnumerator() 
			{
				return new NameEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class NameEnumerator: IEnumerator 
        {
			int nIndex;
			ParamterType parent;
			public NameEnumerator(ParamterType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.NameCount );
			}
			public SchemaString  Current 
			{
				get 
				{
					return(parent.GetNameAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // Name collection

		#region Type2 accessor methods
		public int GetType2MinCount()
		{
			return 0;
		}

		public int Type2MinCount
		{
			get
			{
				return 0;
			}
		}

		public int GetType2MaxCount()
		{
			return 1;
		}

		public int Type2MaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetType2Count()
		{
			return DomChildCount(NodeType.Attribute, "", "Type");
		}

		public int Type2Count
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "Type");
			}
		}

		public bool HasType2()
		{
			return HasDomChild(NodeType.Attribute, "", "Type");
		}

		public SchemaString GetType2At(int index)
		{
			return new SchemaString(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "Type", index)));
		}

		public XmlNode GetStartingType2Cursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "Type" );
		}

		public XmlNode GetAdvancedType2Cursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "Type", curNode );
		}

		public SchemaString GetType2ValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaString( curNode.Value );
		}


		public SchemaString GetType2()
		{
			return GetType2At(0);
		}

		public SchemaString Type2
		{
			get
			{
				return GetType2At(0);
			}
		}

		public void RemoveType2At(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "Type", index);
		}

		public void RemoveType2()
		{
			while (HasType2())
				RemoveType2At(0);
		}

		public void AddType2(SchemaString newValue)
		{
			AppendDomChild(NodeType.Attribute, "", "Type", newValue.ToString());
		}

		public void InsertType2At(SchemaString newValue, int index)
		{
			InsertDomChildAt(NodeType.Attribute, "", "Type", index, newValue.ToString());
		}

		public void ReplaceType2At(SchemaString newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "Type", index, newValue.ToString());
		}
		#endregion // Type2 accessor methods

		#region Type2 collection
        public Type2Collection	MyType2s = new Type2Collection( );

        public class Type2Collection: IEnumerable
        {
            ParamterType parent;
            public ParamterType Parent
			{
				set
				{
					parent = value;
				}
			}
			public Type2Enumerator GetEnumerator() 
			{
				return new Type2Enumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class Type2Enumerator: IEnumerator 
        {
			int nIndex;
			ParamterType parent;
			public Type2Enumerator(ParamterType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.Type2Count );
			}
			public SchemaString  Current 
			{
				get 
				{
					return(parent.GetType2At(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // Type2 collection

		#region Size accessor methods
		public int GetSizeMinCount()
		{
			return 0;
		}

		public int SizeMinCount
		{
			get
			{
				return 0;
			}
		}

		public int GetSizeMaxCount()
		{
			return 1;
		}

		public int SizeMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetSizeCount()
		{
			return DomChildCount(NodeType.Attribute, "", "Size");
		}

		public int SizeCount
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "Size");
			}
		}

		public bool HasSize()
		{
			return HasDomChild(NodeType.Attribute, "", "Size");
		}

		public SchemaInt GetSizeAt(int index)
		{
			return new SchemaInt(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "Size", index)));
		}

		public XmlNode GetStartingSizeCursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "Size" );
		}

		public XmlNode GetAdvancedSizeCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "Size", curNode );
		}

		public SchemaInt GetSizeValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaInt( curNode.Value );
		}


		public SchemaInt GetSize()
		{
			return GetSizeAt(0);
		}

		public SchemaInt Size
		{
			get
			{
				return GetSizeAt(0);
			}
		}

		public void RemoveSizeAt(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "Size", index);
		}

		public void RemoveSize()
		{
			while (HasSize())
				RemoveSizeAt(0);
		}

		public void AddSize(SchemaInt newValue)
		{
			AppendDomChild(NodeType.Attribute, "", "Size", newValue.ToString());
		}

		public void InsertSizeAt(SchemaInt newValue, int index)
		{
			InsertDomChildAt(NodeType.Attribute, "", "Size", index, newValue.ToString());
		}

		public void ReplaceSizeAt(SchemaInt newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "Size", index, newValue.ToString());
		}
		#endregion // Size accessor methods

		#region Size collection
        public SizeCollection	MySizes = new SizeCollection( );

        public class SizeCollection: IEnumerable
        {
            ParamterType parent;
            public ParamterType Parent
			{
				set
				{
					parent = value;
				}
			}
			public SizeEnumerator GetEnumerator() 
			{
				return new SizeEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class SizeEnumerator: IEnumerator 
        {
			int nIndex;
			ParamterType parent;
			public SizeEnumerator(ParamterType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.SizeCount );
			}
			public SchemaInt  Current 
			{
				get 
				{
					return(parent.GetSizeAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // Size collection

		#region Value2 accessor methods
		public int GetValue2MinCount()
		{
			return 0;
		}

		public int Value2MinCount
		{
			get
			{
				return 0;
			}
		}

		public int GetValue2MaxCount()
		{
			return 1;
		}

		public int Value2MaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetValue2Count()
		{
			return DomChildCount(NodeType.Attribute, "", "Value");
		}

		public int Value2Count
		{
			get
			{
				return DomChildCount(NodeType.Attribute, "", "Value");
			}
		}

		public bool HasValue2()
		{
			return HasDomChild(NodeType.Attribute, "", "Value");
		}

		public SchemaString GetValue2At(int index)
		{
			return new SchemaString(GetDomNodeValue(GetDomChildAt(NodeType.Attribute, "", "Value", index)));
		}

		public XmlNode GetStartingValue2Cursor()
		{
			return GetDomFirstChild( NodeType.Attribute, "", "Value" );
		}

		public XmlNode GetAdvancedValue2Cursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Attribute, "", "Value", curNode );
		}

		public SchemaString GetValue2ValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new SchemaString( curNode.Value );
		}


		public SchemaString GetValue2()
		{
			return GetValue2At(0);
		}

		public SchemaString Value2
		{
			get
			{
				return GetValue2At(0);
			}
		}

		public void RemoveValue2At(int index)
		{
			RemoveDomChildAt(NodeType.Attribute, "", "Value", index);
		}

		public void RemoveValue2()
		{
			while (HasValue2())
				RemoveValue2At(0);
		}

		public void AddValue2(SchemaString newValue)
		{
			AppendDomChild(NodeType.Attribute, "", "Value", newValue.ToString());
		}

		public void InsertValue2At(SchemaString newValue, int index)
		{
			InsertDomChildAt(NodeType.Attribute, "", "Value", index, newValue.ToString());
		}

		public void ReplaceValue2At(SchemaString newValue, int index)
		{
			ReplaceDomChildAt(NodeType.Attribute, "", "Value", index, newValue.ToString());
		}
		#endregion // Value2 accessor methods

		#region Value2 collection
        public Value2Collection	MyValue2s = new Value2Collection( );

        public class Value2Collection: IEnumerable
        {
            ParamterType parent;
            public ParamterType Parent
			{
				set
				{
					parent = value;
				}
			}
			public Value2Enumerator GetEnumerator() 
			{
				return new Value2Enumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class Value2Enumerator: IEnumerator 
        {
			int nIndex;
			ParamterType parent;
			public Value2Enumerator(ParamterType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.Value2Count );
			}
			public SchemaString  Current 
			{
				get 
				{
					return(parent.GetValue2At(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // Value2 collection

        private void SetCollectionParents()
        {
            MyNames.Parent = this; 
            MyType2s.Parent = this; 
            MySizes.Parent = this; 
            MyValue2s.Parent = this; 
	}
}
}
