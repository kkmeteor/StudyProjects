//
// SchemaExceptions.cs
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


namespace UFIDA.U8.UAP.Services.ReportData.Altova.Types
{
	/// <summary>
	/// Base class for all exceptions thrown in the context of schema-types handling.
	/// </summary>
	public class SchemaTypeException : AltovaException 
	{
		public SchemaTypeException(string text) 
			: base( text )
		{
		}

		public SchemaTypeException(Exception other) 
			: base( other )
		{
		}
	}

	/// <summary>
	/// The given string could not be parsed to the value of the given schema-type.
	/// There are formating (or other) errors.
	/// </summary>
	public class StringParseException : SchemaTypeException 
	{
		int position;

		public StringParseException(string text, int newposition) 
			: base( text )
		{
			position = newposition;
		}

		public StringParseException(Exception other) 
			: base( other )
		{
		}
	}

	/// <summary>
	/// This exception is thrown when trying to convert a string not containing a valid number to a number.
	/// </summary>
	public class NotANumberException : SchemaTypeException 
	{
		public NotANumberException(string text) 
			: base( text )
		{
		}

		public NotANumberException(Exception other) 
			: base( other )
		{
		}
	}

	/// <summary>
	/// Two schema-types are not compatible and can't be converted from one to the other regardless the contained value.
	/// </summary>
	public class TypesIncompatibleException : SchemaTypeException 
	{
		protected ISchemaType object1;
		protected ISchemaType object2;

		public TypesIncompatibleException(ISchemaType newobj1, ISchemaType newobj2) 
			: base("Incompatible schema-types")
		{
			object1 = newobj1;
			object2 = newobj2;
		}

		public TypesIncompatibleException(Exception other) 
			: base( other )
		{
		}
	}

	/// <summary>
	/// The two schema-types may be compatible, but the contained value cannot be 
	/// converted to the other schema-type.
	/// </summary>
	public class ValuesNotConvertableException : SchemaTypeException 
	{
		protected ISchemaType object1;
		protected ISchemaType object2;

		public ValuesNotConvertableException(ISchemaType newobj1, ISchemaType newobj2) 
			: base("Values could not be converted")
		{
			object1 = newobj1;
			object2 = newobj2;
		}

		public ValuesNotConvertableException(Exception other) 
			: base( other )
		{
		}
	}
}