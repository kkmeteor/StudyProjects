using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public class XMLTransfer
    {
        public static string GetSignedToken(string userToken)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(userToken);
            var node = doc.SelectSingleNode("ufsoft/data/SignedToken");
            if (node != null)
            {
                return node.Attributes["value"].Value;
            }
            else
                return "获取Token失败！";
        }
    }
}
