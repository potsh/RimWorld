using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Verse
{
	public static class XmlInheritance
	{
		private class XmlInheritanceNode
		{
			public XmlNode xmlNode;

			public XmlNode resolvedXmlNode;

			public ModContentPack mod;

			public XmlInheritanceNode parent;

			public List<XmlInheritanceNode> children = new List<XmlInheritanceNode>();
		}

		private static Dictionary<XmlNode, XmlInheritanceNode> resolvedNodes = new Dictionary<XmlNode, XmlInheritanceNode>();

		private static List<XmlInheritanceNode> unresolvedNodes = new List<XmlInheritanceNode>();

		private static Dictionary<string, List<XmlInheritanceNode>> nodesByName = new Dictionary<string, List<XmlInheritanceNode>>();

		private const string NameAttributeName = "Name";

		private const string ParentNameAttributeName = "ParentName";

		private const string InheritAttributeName = "Inherit";

		private static HashSet<string> tempUsedNodeNames = new HashSet<string>();

		public static void TryRegisterAllFrom(LoadableXmlAsset xmlAsset, ModContentPack mod)
		{
			if (xmlAsset.xmlDoc != null)
			{
				XmlNodeList childNodes = xmlAsset.xmlDoc.DocumentElement.ChildNodes;
				for (int i = 0; i < childNodes.Count; i++)
				{
					if (childNodes[i].NodeType == XmlNodeType.Element)
					{
						TryRegister(childNodes[i], mod);
					}
				}
			}
		}

		public static void TryRegister(XmlNode node, ModContentPack mod)
		{
			XmlAttribute xmlAttribute = node.Attributes["Name"];
			XmlAttribute xmlAttribute2 = node.Attributes["ParentName"];
			if (xmlAttribute != null || xmlAttribute2 != null)
			{
				List<XmlInheritanceNode> value = null;
				if (xmlAttribute != null && nodesByName.TryGetValue(xmlAttribute.Value, out value))
				{
					for (int i = 0; i < value.Count; i++)
					{
						if (value[i].mod == mod)
						{
							if (mod == null)
							{
								Log.Error("XML error: Could not register node named \"" + xmlAttribute.Value + "\" because this name is already used.");
							}
							else
							{
								Log.Error("XML error: Could not register node named \"" + xmlAttribute.Value + "\" in mod " + mod.ToString() + " because this name is already used in this mod.");
							}
							return;
						}
					}
				}
				XmlInheritanceNode xmlInheritanceNode = new XmlInheritanceNode();
				xmlInheritanceNode.xmlNode = node;
				xmlInheritanceNode.mod = mod;
				unresolvedNodes.Add(xmlInheritanceNode);
				if (xmlAttribute != null)
				{
					if (value != null)
					{
						value.Add(xmlInheritanceNode);
					}
					else
					{
						value = new List<XmlInheritanceNode>();
						value.Add(xmlInheritanceNode);
						nodesByName.Add(xmlAttribute.Value, value);
					}
				}
			}
		}

		public static void Resolve()
		{
			ResolveParentsAndChildNodesLinks();
			ResolveXmlNodes();
		}

		public static XmlNode GetResolvedNodeFor(XmlNode originalNode)
		{
			if (originalNode.Attributes["ParentName"] != null)
			{
				if (resolvedNodes.TryGetValue(originalNode, out XmlInheritanceNode value))
				{
					return value.resolvedXmlNode;
				}
				if (unresolvedNodes.Any((XmlInheritanceNode x) => x.xmlNode == originalNode))
				{
					Log.Error("XML error: XML node \"" + originalNode.Name + "\" has not been resolved yet. There's probably a Resolve() call missing somewhere.");
				}
				else
				{
					Log.Error("XML error: Tried to get resolved node for node \"" + originalNode.Name + "\" which uses a ParentName attribute, but it is not in a resolved nodes collection, which means that it was never registered or there was an error while resolving it.");
				}
			}
			return originalNode;
		}

		public static void Clear()
		{
			resolvedNodes.Clear();
			unresolvedNodes.Clear();
			nodesByName.Clear();
		}

		private static void ResolveParentsAndChildNodesLinks()
		{
			for (int i = 0; i < unresolvedNodes.Count; i++)
			{
				XmlAttribute xmlAttribute = unresolvedNodes[i].xmlNode.Attributes["ParentName"];
				if (xmlAttribute != null)
				{
					unresolvedNodes[i].parent = GetBestParentFor(unresolvedNodes[i], xmlAttribute.Value);
					if (unresolvedNodes[i].parent != null)
					{
						unresolvedNodes[i].parent.children.Add(unresolvedNodes[i]);
					}
				}
			}
		}

		private static void ResolveXmlNodes()
		{
			List<XmlInheritanceNode> list = (from x in unresolvedNodes
			where x.parent == null || x.parent.resolvedXmlNode != null
			select x).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				ResolveXmlNodesRecursively(list[i]);
			}
			for (int j = 0; j < unresolvedNodes.Count; j++)
			{
				if (unresolvedNodes[j].resolvedXmlNode == null)
				{
					Log.Error("XML error: Cyclic inheritance hierarchy detected for node \"" + unresolvedNodes[j].xmlNode.Name + "\". Full node: " + unresolvedNodes[j].xmlNode.OuterXml);
				}
				else
				{
					resolvedNodes.Add(unresolvedNodes[j].xmlNode, unresolvedNodes[j]);
				}
			}
			unresolvedNodes.Clear();
		}

		private static void ResolveXmlNodesRecursively(XmlInheritanceNode node)
		{
			if (node.resolvedXmlNode != null)
			{
				Log.Error("XML error: Cyclic inheritance hierarchy detected for node \"" + node.xmlNode.Name + "\". Full node: " + node.xmlNode.OuterXml);
			}
			else
			{
				ResolveXmlNodeFor(node);
				for (int i = 0; i < node.children.Count; i++)
				{
					ResolveXmlNodesRecursively(node.children[i]);
				}
			}
		}

		private static XmlInheritanceNode GetBestParentFor(XmlInheritanceNode node, string parentName)
		{
			XmlInheritanceNode xmlInheritanceNode = null;
			if (nodesByName.TryGetValue(parentName, out List<XmlInheritanceNode> value))
			{
				if (node.mod == null)
				{
					for (int i = 0; i < value.Count; i++)
					{
						if (value[i].mod == null)
						{
							xmlInheritanceNode = value[i];
							break;
						}
					}
					if (xmlInheritanceNode == null)
					{
						for (int j = 0; j < value.Count; j++)
						{
							if (xmlInheritanceNode == null || value[j].mod.loadOrder < xmlInheritanceNode.mod.loadOrder)
							{
								xmlInheritanceNode = value[j];
							}
						}
					}
				}
				else
				{
					for (int k = 0; k < value.Count; k++)
					{
						if (value[k].mod != null && value[k].mod.loadOrder <= node.mod.loadOrder && (xmlInheritanceNode == null || value[k].mod.loadOrder > xmlInheritanceNode.mod.loadOrder))
						{
							xmlInheritanceNode = value[k];
						}
					}
					if (xmlInheritanceNode == null)
					{
						for (int l = 0; l < value.Count; l++)
						{
							if (value[l].mod == null)
							{
								xmlInheritanceNode = value[l];
								break;
							}
						}
					}
				}
			}
			if (xmlInheritanceNode == null)
			{
				Log.Error("XML error: Could not find parent node named \"" + parentName + "\" for node \"" + node.xmlNode.Name + "\". Full node: " + node.xmlNode.OuterXml);
				return null;
			}
			return xmlInheritanceNode;
		}

		private static void ResolveXmlNodeFor(XmlInheritanceNode node)
		{
			if (node.parent == null)
			{
				node.resolvedXmlNode = node.xmlNode;
			}
			else if (node.parent.resolvedXmlNode == null)
			{
				Log.Error("XML error: Internal error. Tried to resolve node whose parent has not been resolved yet. This means that this method was called in incorrect order.");
				node.resolvedXmlNode = node.xmlNode;
			}
			else
			{
				CheckForDuplicateNodes(node.xmlNode, node.xmlNode);
				XmlNode xmlNode = node.parent.resolvedXmlNode.CloneNode(deep: true);
				RecursiveNodeCopyOverwriteElements(node.xmlNode, xmlNode);
				node.resolvedXmlNode = xmlNode;
			}
		}

		private static void RecursiveNodeCopyOverwriteElements(XmlNode child, XmlNode current)
		{
			XmlAttribute xmlAttribute = child.Attributes["Inherit"];
			if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "false")
			{
				while (current.HasChildNodes)
				{
					current.RemoveChild(current.FirstChild);
				}
				IEnumerator enumerator = child.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						XmlNode node = (XmlNode)enumerator.Current;
						XmlNode newChild = current.OwnerDocument.ImportNode(node, deep: true);
						current.AppendChild(newChild);
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			else
			{
				current.Attributes.RemoveAll();
				XmlAttributeCollection attributes = child.Attributes;
				for (int i = 0; i < attributes.Count; i++)
				{
					XmlAttribute node2 = (XmlAttribute)current.OwnerDocument.ImportNode(attributes[i], deep: true);
					current.Attributes.Append(node2);
				}
				List<XmlElement> list = new List<XmlElement>();
				XmlNode xmlNode = null;
				IEnumerator enumerator2 = child.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						XmlNode xmlNode2 = (XmlNode)enumerator2.Current;
						if (xmlNode2.NodeType == XmlNodeType.Text)
						{
							xmlNode = xmlNode2;
						}
						else if (xmlNode2.NodeType == XmlNodeType.Element)
						{
							list.Add((XmlElement)xmlNode2);
						}
					}
				}
				finally
				{
					IDisposable disposable2;
					if ((disposable2 = (enumerator2 as IDisposable)) != null)
					{
						disposable2.Dispose();
					}
				}
				if (xmlNode != null)
				{
					for (int num = current.ChildNodes.Count - 1; num >= 0; num--)
					{
						XmlNode xmlNode3 = current.ChildNodes[num];
						if (xmlNode3.NodeType != XmlNodeType.Attribute)
						{
							current.RemoveChild(xmlNode3);
						}
					}
					XmlNode newChild2 = current.OwnerDocument.ImportNode(xmlNode, deep: true);
					current.AppendChild(newChild2);
				}
				else if (!list.Any())
				{
					bool flag = false;
					IEnumerator enumerator3 = current.ChildNodes.GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							XmlNode xmlNode4 = (XmlNode)enumerator3.Current;
							if (xmlNode4.NodeType == XmlNodeType.Element)
							{
								flag = true;
								break;
							}
						}
					}
					finally
					{
						IDisposable disposable3;
						if ((disposable3 = (enumerator3 as IDisposable)) != null)
						{
							disposable3.Dispose();
						}
					}
					if (!flag)
					{
						IEnumerator enumerator4 = current.ChildNodes.GetEnumerator();
						try
						{
							while (enumerator4.MoveNext())
							{
								XmlNode xmlNode5 = (XmlNode)enumerator4.Current;
								if (xmlNode5.NodeType != XmlNodeType.Attribute)
								{
									current.RemoveChild(xmlNode5);
								}
							}
						}
						finally
						{
							IDisposable disposable4;
							if ((disposable4 = (enumerator4 as IDisposable)) != null)
							{
								disposable4.Dispose();
							}
						}
					}
				}
				else
				{
					for (int j = 0; j < list.Count; j++)
					{
						XmlElement xmlElement = list[j];
						if (xmlElement.Name == "li")
						{
							XmlNode newChild3 = current.OwnerDocument.ImportNode(xmlElement, deep: true);
							current.AppendChild(newChild3);
						}
						else
						{
							XmlElement xmlElement2 = current[xmlElement.Name];
							if (xmlElement2 != null)
							{
								RecursiveNodeCopyOverwriteElements(xmlElement, xmlElement2);
							}
							else
							{
								XmlNode newChild4 = current.OwnerDocument.ImportNode(xmlElement, deep: true);
								current.AppendChild(newChild4);
							}
						}
					}
				}
			}
		}

		private static void CheckForDuplicateNodes(XmlNode node, XmlNode root)
		{
			tempUsedNodeNames.Clear();
			IEnumerator enumerator = node.ChildNodes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					XmlNode xmlNode = (XmlNode)enumerator.Current;
					if (xmlNode.NodeType == XmlNodeType.Element && !(xmlNode.Name == "li"))
					{
						if (tempUsedNodeNames.Contains(xmlNode.Name))
						{
							Log.Error("XML error: Duplicate XML node name " + xmlNode.Name + " in this XML block: " + node.OuterXml + ((node == root) ? string.Empty : ("\n\nRoot node: " + root.OuterXml)));
						}
						else
						{
							tempUsedNodeNames.Add(xmlNode.Name);
						}
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			tempUsedNodeNames.Clear();
			IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					XmlNode xmlNode2 = (XmlNode)enumerator2.Current;
					if (xmlNode2.NodeType == XmlNodeType.Element)
					{
						CheckForDuplicateNodes(xmlNode2, root);
					}
				}
			}
			finally
			{
				IDisposable disposable2;
				if ((disposable2 = (enumerator2 as IDisposable)) != null)
				{
					disposable2.Dispose();
				}
			}
		}
	}
}
