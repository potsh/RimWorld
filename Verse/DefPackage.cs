using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Verse
{
	public class DefPackage
	{
		public string fileName = "NamelessPackage";

		public string relFolder = string.Empty;

		public List<Def> defs = new List<Def>();

		public DefPackage(string name, string relFolder)
		{
			fileName = name;
			this.relFolder = relFolder;
		}

		public List<Def>.Enumerator GetEnumerator()
		{
			return defs.GetEnumerator();
		}

		public void AddDef(Def def)
		{
			def.defPackage = this;
			defs.Add(def);
		}

		public void RemoveDef(Def def)
		{
			if (def == null)
			{
				throw new ArgumentNullException("def");
			}
			if (!defs.Contains(def))
			{
				throw new InvalidOperationException("Package " + this + " cannot remove " + def + " because it doesn't contain it.");
			}
			defs.Remove(def);
			if (def.defPackage == this)
			{
				def.defPackage = null;
			}
		}

		public void SaveIn(ModContentPack mod)
		{
			string fullFolderPath = GetFullFolderPath(mod);
			string text = Path.Combine(fullFolderPath, fileName);
			XDocument xDocument = new XDocument();
			XElement xElement = new XElement("DefPackage");
			xDocument.Add(xElement);
			try
			{
				foreach (Def def in defs)
				{
					XElement content = DirectXmlSaver.XElementFromObject(def, def.GetType());
					xElement.Add(content);
				}
				DirectXmlSaveFormatter.AddWhitespaceFromRoot(xElement);
				SaveOptions options = SaveOptions.DisableFormatting;
				xDocument.Save(text, options);
				Messages.Message("Saved in " + text, MessageTypeDefOf.PositiveEvent, historical: false);
			}
			catch (Exception ex)
			{
				Messages.Message("Exception saving XML: " + ex.ToString(), MessageTypeDefOf.NegativeEvent, historical: false);
				throw;
			}
		}

		public override string ToString()
		{
			return relFolder + "/" + fileName;
		}

		public string GetFullFolderPath(ModContentPack mod)
		{
			return Path.GetFullPath(Path.Combine(Path.Combine(mod.RootDir, "Defs/"), relFolder));
		}

		public static string UnusedPackageName(string relFolder, ModContentPack mod)
		{
			string fullPath = Path.GetFullPath(Path.Combine(Path.Combine(mod.RootDir, "Defs/"), relFolder));
			int num = 1;
			string text;
			do
			{
				text = "NewPackage" + num.ToString() + ".xml";
				num++;
			}
			while (File.Exists(Path.Combine(fullPath, text)));
			return text;
		}
	}
}
