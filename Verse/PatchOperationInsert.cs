using System;
using System.Collections;
using System.Xml;

namespace Verse
{
	public class PatchOperationInsert : PatchOperationPathed
	{
		private enum Order
		{
			Append,
			Prepend
		}

		private XmlContainer value;

		private Order order = Order.Prepend;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = value.node;
			bool result = false;
			IEnumerator enumerator = xml.SelectNodes(xpath).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					result = true;
					XmlNode xmlNode = current as XmlNode;
					XmlNode parentNode = xmlNode.ParentNode;
					if (order == Order.Append)
					{
						for (int i = 0; i < node.ChildNodes.Count; i++)
						{
							parentNode.InsertAfter(parentNode.OwnerDocument.ImportNode(node.ChildNodes[i], deep: true), xmlNode);
						}
					}
					else if (order == Order.Prepend)
					{
						for (int num = node.ChildNodes.Count - 1; num >= 0; num--)
						{
							parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[num], deep: true), xmlNode);
						}
					}
				}
				return result;
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
	}
}
