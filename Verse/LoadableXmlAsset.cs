using System;
using System.IO;
using System.Xml;

namespace Verse
{
	public class LoadableXmlAsset
	{
		public string name;

		public string fullFolderPath;

		public XmlDocument xmlDoc;

		public ModContentPack mod;

		public DefPackage defPackage;

		public string FullFilePath => fullFolderPath + Path.DirectorySeparatorChar + name;

		public LoadableXmlAsset(string name, string fullFolderPath, string contents)
		{
			this.name = name;
			this.fullFolderPath = fullFolderPath;
			try
			{
				xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(contents);
			}
			catch (Exception ex)
			{
				Log.Warning("Exception reading " + name + " as XML: " + ex);
				xmlDoc = null;
			}
		}

		public override string ToString()
		{
			return name;
		}
	}
}
