using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// SerializeHelper ��ժҪ˵����
	/// </summary>
	public class SerializeHelper
	{
		public SerializeHelper()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}
		public static string  Serialize( object serializedObject)
		{
			string strObject = string.Empty;
            IFormatter formatter = new SoapFormatter();
           
			MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, serializedObject);	
			byte[] bytData = ms.ToArray();
            strObject = System.Text.UTF8Encoding.UTF8.GetString(bytData, 0, bytData.Length);
            ms.Close();
			return strObject;
		}

		public static object Deserialize( string objectString )
		{
			object obj = null;
            byte[] bytData = System.Text.UTF8Encoding.UTF8.GetBytes( objectString );
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
            
            MemoryStream ms = new MemoryStream( bytData );
			obj = formatter.Deserialize( ms );
			ms.Close();
			return obj;
		}
	}
}
