/*
 * 作者:卢达其
 * 时间:2009.1.16
 */

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 树状态的处理
	/// </summary>
	public class ReportCenterUiStateHandler
	{
		private string[] _dataKeys = new string[]{
			ReportCenterDataService.KeyExpandedNodes,
			ReportCenterDataService.KeyDeletedStaticReports,
			ReportCenterDataService.KeyCustomNodes,
			ReportCenterDataService.KeyCustomStaticReportNames,
			ReportCenterDataService.KeyFindHistory,
			ReportCenterDataService.KeyAllFinalNodes,
			ReportCenterDataService.KeySelectedNode,
			ReportCenterDataService.KeyNodesPostion,
		};
		private Dictionary<string, Hashtable> _hashData = new Dictionary<string, Hashtable>();
		
		public ReportCenterUiStateHandler()
		{
			foreach(string k in this._dataKeys)
				this._hashData[k] = new Hashtable();
		}
		
		public string[] DataKeys
		{
			get { return this._dataKeys; }
		}

		public Dictionary<string, Hashtable> HashData
		{
			get { return this._hashData; }
		}

		public void Xml2Obj(string xml)
		{ 
			if(string.IsNullOrEmpty(xml)
				|| this.IsXmlBefore890(xml))
				return;

			ConfigXmlItem topElement = new ConfigXmlItem();
			topElement.FromXml(xml);
			foreach(string k in this._dataKeys)
				this.Xml2ObjHashtableItems(topElement, k);
		}

		/// <summary>
		/// 是否是890之前的xml数据
		/// </summary>
		private bool IsXmlBefore890(string xml)
		{ 
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			if(doc.DocumentElement.Name != "ConfigXmlItem")
				return true;
			return false;
		}

		private void Xml2ObjHashtableItems(ConfigXmlItem topElement, string key)
		{ 
			Hashtable ht = this._hashData[key];
			ht.Clear();
			ConfigXmlItem nodes = topElement.GetSubItem(key) as ConfigXmlItem;
			for (int i = 0; i < nodes.SubItemCount; i++)
			{
				IConfigXmlItem item = nodes.GetSubItem(i);
				ht.Add(item.GetProperty(ConfigXmlContext.XmlKeyId), item);
			}
		}

		public string Obj2Xml()
		{ 
			//this.Obj2XmlGetSelectedNode();
			//this.Obj2XmlGetAllFinalNodes();
			ConfigXmlItem topElement = new ConfigXmlItem();
			topElement.SetProperty(ConfigXmlContext.XmlKeyId, ReportCenterDataService.KeyReportCenterState);
			foreach(string k in this._dataKeys)
				topElement.SetSubItem(this.Obj2XmlItems(k));

			//System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			//topElement.ToXml(doc, null);
			//doc.Save(@"C:\Documents and Settings\ldq\桌面\test.xml");

			//xml为数据库字段，存入之前'需要转义
			return topElement.ToXml().Replace("'","''");
		}

		private ConfigXmlItem Obj2XmlItems(string key)
		{ 
			Hashtable ht = this._hashData[key];
			ConfigXmlItem item = new ConfigXmlItem();
			item.SetProperty(ConfigXmlContext.XmlKeyId, key);
			foreach (ConfigXmlItem cxi in ht.Values)
				item.SetSubItem(cxi);
			return item;
		}

		//private void Obj2XmlGetSelectedNode()
		//{ 
		//    TreeView tv = this.Context.MyReportViewPart.treeView1;
		//    if (tv.SelectedNode != null)
		//    {
		//        Hashtable data = this._hashData[ReportCenterContext.KeySelectedNode];
		//        data.Clear();
		//        string id = MyReportData.GetTreeNodeId(tv.SelectedNode);
		//        data[id] = new ConfigXmlItem(id);
		//    }
		//}

		//private void Obj2XmlGetAllFinalNodes()
		//{ 
		//    Hashtable data = this._hashData[ReportCenterContext.KeyAllFinalNodes];
		//    data.Clear();
		//    TreeView tv = this.Context.MyReportViewPart.treeView1;
		//    this.GetExendedNodes(tv.Nodes, data);
		//}

		//private void GetExendedNodes(TreeNodeCollection nodes, Hashtable data)
		//{ 
		//    foreach (TreeNode n in nodes)
		//        if (MyReportData.IsCanShowNewInfo(MyReportData.GetTreeNodeTagMeta(n)))
		//        {
		//            string id = MyReportData.GetTreeNodeId(n);
		//            data[id] = new ConfigXmlItem(id);
		//        }
		//        else
		//            this.GetExendedNodes(n.Nodes, data);
		//}
	}
}