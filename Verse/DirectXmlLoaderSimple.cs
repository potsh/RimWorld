using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Verse
{
	public static class DirectXmlLoaderSimple
	{
		public struct XmlKeyValuePair
		{
			public string key;

			public string value;

			public int lineNumber;
		}

		public static IEnumerable<XmlKeyValuePair> ValuesFromXmlFile(FileInfo file)
		{
			XDocument doc = XDocument.Load(file.FullName, LoadOptions.SetLineInfo);
			using (IEnumerator<XElement> enumerator = doc.Root.Elements().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					XElement element = enumerator.Current;
					string key = element.Name.ToString();
					string value2 = element.Value;
					value2 = value2.Replace("\\n", "\n");
					yield return new XmlKeyValuePair
					{
						key = key,
						value = value2,
						lineNumber = ((IXmlLineInfo)element).LineNumber
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_015e:
			/*Error near IL_015f: Unexpected return in MoveNext()*/;
		}
	}
}
