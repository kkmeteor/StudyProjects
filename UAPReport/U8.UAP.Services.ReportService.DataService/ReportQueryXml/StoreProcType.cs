//
// StoreProcType.cs.cs
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
	public class StoreProcType : Altova.Xml.Node
	{
		#region Forward constructors
		public StoreProcType() : base() { SetCollectionParents(); }
		public StoreProcType(XmlDocument doc) : base(doc) { SetCollectionParents(); }
		public StoreProcType(XmlNode node) : base(node) { SetCollectionParents(); }
		public StoreProcType(Altova.Xml.Node node) : base(node) { SetCollectionParents(); }
		#endregion // Forward constructors

		public override void AdjustPrefix()
		{

			for (int i = 0; i < DomChildCount(NodeType.Attribute, "", "Name"); i++)
			{
				XmlNode DOMNode = GetDomChildAt(NodeType.Attribute, "", "Name", i);
				InternalAdjustPrefix(DOMNode, false);
			}

			for (int i = 0; i < DomChildCount(NodeType.Element, "", "Paramter"); i++)
			{
				XmlNode DOMNode = GetDomChildAt(NodeType.Element, "", "Paramter", i);
				InternalAdjustPrefix(DOMNode, true);
				new ParamterType(DOMNode).AdjustPrefix();
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
            StoreProcType parent;
            public StoreProcType Parent
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
			StoreProcType parent;
			public NameEnumerator(StoreProcType par) 
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

		#region Paramter accessor methods
		public int GetParamterMinCount()
		{
			return 0;
		}

		public int ParamterMinCount
		{
			get
			{
				return 0;
			}
		}

		public int GetParamterMaxCount()
		{
			return Int32.MaxValue;
		}

		public int ParamterMaxCount
		{
			get
			{
				return Int32.MaxValue;
			}
		}

		public int GetParamterCount()
		{
			return DomChildCount(NodeType.Element, "", "Paramter");
		}

		public int ParamterCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "Paramter");
			}
		}

		public bool HasParamter()
		{
			return HasDomChild(NodeType.Element, "", "Paramter");
		}

		public ParamterType GetParamterAt(int index)
		{
			return new ParamterType(GetDomChildAt(NodeType.Element, "", "Paramter", index));
		}

		public XmlNode GetStartingParamterCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "Paramter" );
		}

		public XmlNode GetAdvancedParamterCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "Paramter", curNode );
		}

		public ParamterType GetParamterValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new ParamterType( curNode );
		}


		public ParamterType GetParamter()
		{
			return GetParamterAt(0);
		}

		public ParamterType Paramter
		{
			get
			{
				return GetParamterAt(0);
			}
		}

		public void RemoveParamterAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "Paramter", index);
		}

		public void RemoveParamter()
		{
			while (HasParamter())
				RemoveParamterAt(0);
		}

		public void AddParamter(ParamterType newValue)
		{
			AppendDomElement("", "Paramter", newValue);
		}

		public void InsertParamterAt(ParamterType newValue, int index)
		{
			InsertDomElementAt("", "Paramter", index, newValue);
		}

		public void ReplaceParamterAt(ParamterType newValue, int index)
		{
			ReplaceDomElementAt("", "Paramter", index, newValue);
		}
		#endregion // Paramter accessor methods

		#region Paramter collection
        public ParamterCollection	MyParamters = new ParamterCollection( );

        public class ParamterCollection: IEnumerable
        {
            StoreProcType parent;
            public StoreProcType Parent
			{
				set
				{
					parent = value;
				}
			}
			public ParamterEnumerator GetEnumerator() 
			{
				return new ParamterEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class ParamterEnumerator: IEnumerator 
        {
			int nIndex;
			StoreProcType parent;
			public ParamterEnumerator(StoreProcType par) 
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
				return(nIndex < parent.ParamterCount );
			}
			public ParamterType  Current 
			{
				get 
				{
					return(parent.GetParamterAt(nIndex));
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

        #endregion // Paramter collection

        private void SetCollectionParents()
        {
            MyNames.Parent = this; 
            MyParamters.Parent = this; 
	}
}
}
