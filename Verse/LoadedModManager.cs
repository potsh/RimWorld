using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Verse
{
	public static class LoadedModManager
	{
		private static List<ModContentPack> runningMods = new List<ModContentPack>();

		private static Dictionary<Type, Mod> runningModClasses = new Dictionary<Type, Mod>();

		public static List<ModContentPack> RunningModsListForReading => runningMods;

		public static IEnumerable<ModContentPack> RunningMods => runningMods;

		public static IEnumerable<Mod> ModHandles => runningModClasses.Values;

		public static void LoadAllActiveMods()
		{
			XmlInheritance.Clear();
			InitializeMods();
			LoadModContent();
			CreateModClasses();
			List<LoadableXmlAsset> xmls = LoadModXML();
			Dictionary<XmlNode, LoadableXmlAsset> assetlookup = new Dictionary<XmlNode, LoadableXmlAsset>();
			XmlDocument xmlDoc = CombineIntoUnifiedXML(xmls, assetlookup);
			ApplyPatches(xmlDoc, assetlookup);
			ParseAndProcessXML(xmlDoc, assetlookup);
			ClearCachedPatches();
			XmlInheritance.Clear();
		}

		public static void InitializeMods()
		{
			int num = 0;
			foreach (ModMetaData item2 in ModsConfig.ActiveModsInLoadOrder.ToList())
			{
				DeepProfiler.Start("Initializing " + item2);
				try
				{
					if (!item2.RootDir.Exists)
					{
						ModsConfig.SetActive(item2.Identifier, active: false);
						Log.Warning("Failed to find active mod " + item2.Name + "(" + item2.Identifier + ") at " + item2.RootDir);
					}
					else
					{
						ModContentPack item = new ModContentPack(item2.RootDir, num, item2.Name);
						num++;
						runningMods.Add(item);
					}
				}
				catch (Exception arg)
				{
					Log.Error("Error initializing mod: " + arg);
					ModsConfig.SetActive(item2.Identifier, active: false);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}

		public static void LoadModContent()
		{
			for (int i = 0; i < runningMods.Count; i++)
			{
				ModContentPack modContentPack = runningMods[i];
				DeepProfiler.Start("Loading " + modContentPack + " content");
				try
				{
					modContentPack.ReloadContent();
				}
				catch (Exception ex)
				{
					Log.Error("Could not reload mod content for mod " + modContentPack.Identifier + ": " + ex);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}

		public static void CreateModClasses()
		{
			foreach (Type item in typeof(Mod).InstantiableDescendantsAndSelf())
			{
				try
				{
					if (!runningModClasses.ContainsKey(item))
					{
						ModContentPack modContentPack = (from modpack in runningMods
						where modpack.assemblies.loadedAssemblies.Contains(item.Assembly)
						select modpack).FirstOrDefault();
						runningModClasses[item] = (Mod)Activator.CreateInstance(item, modContentPack);
					}
				}
				catch (Exception ex)
				{
					Log.Error("Error while instantiating a mod of type " + item + ": " + ex);
				}
			}
		}

		public static List<LoadableXmlAsset> LoadModXML()
		{
			List<LoadableXmlAsset> list = new List<LoadableXmlAsset>();
			for (int i = 0; i < runningMods.Count; i++)
			{
				ModContentPack modContentPack = runningMods[i];
				DeepProfiler.Start("Loading " + modContentPack);
				try
				{
					list.AddRange(modContentPack.LoadDefs());
				}
				catch (Exception ex)
				{
					Log.Error("Could not load defs for mod " + modContentPack.Identifier + ": " + ex);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			return list;
		}

		public static void ApplyPatches(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
		{
			foreach (PatchOperation item in runningMods.SelectMany((ModContentPack rm) => rm.Patches))
			{
				try
				{
					item.Apply(xmlDoc);
				}
				catch (Exception arg)
				{
					Log.Error("Error in patch.Apply(): " + arg);
				}
			}
		}

		public static XmlDocument CombineIntoUnifiedXML(List<LoadableXmlAsset> xmls, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateElement("Defs"));
			foreach (LoadableXmlAsset xml in xmls)
			{
				if (xml.xmlDoc == null || xml.xmlDoc.DocumentElement == null)
				{
					Log.Error(string.Format("{0}: unknown parse failure", xml.fullFolderPath + "/" + xml.name));
				}
				else
				{
					if (xml.xmlDoc.DocumentElement.Name != "Defs")
					{
						Log.Error(string.Format("{0}: root element named {1}; should be named Defs", xml.fullFolderPath + "/" + xml.name, xml.xmlDoc.DocumentElement.Name));
					}
					IEnumerator enumerator2 = xml.xmlDoc.DocumentElement.ChildNodes.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							XmlNode node = (XmlNode)enumerator2.Current;
							XmlNode xmlNode = xmlDocument.ImportNode(node, deep: true);
							assetlookup[xmlNode] = xml;
							xmlDocument.DocumentElement.AppendChild(xmlNode);
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator2 as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
			return xmlDocument;
		}

		public static void ParseAndProcessXML(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
		{
			XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
			for (int i = 0; i < childNodes.Count; i++)
			{
				if (childNodes[i].NodeType == XmlNodeType.Element)
				{
					LoadableXmlAsset loadableXmlAsset = assetlookup.TryGetValue(childNodes[i]);
					XmlInheritance.TryRegister(childNodes[i], loadableXmlAsset?.mod);
				}
			}
			XmlInheritance.Resolve();
			DefPackage defPackage = new DefPackage("Unknown", string.Empty);
			ModContentPack modContentPack = runningMods.FirstOrDefault();
			modContentPack.AddDefPackage(defPackage);
			IEnumerator enumerator = xmlDoc.DocumentElement.ChildNodes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					XmlNode xmlNode = (XmlNode)enumerator.Current;
					LoadableXmlAsset loadableXmlAsset2 = assetlookup.TryGetValue(xmlNode);
					DefPackage defPackage2 = (loadableXmlAsset2 == null) ? defPackage : loadableXmlAsset2.defPackage;
					Def def = DirectXmlLoader.DefFromNode(xmlNode, loadableXmlAsset2);
					if (def != null)
					{
						def.modContentPack = ((loadableXmlAsset2 == null) ? modContentPack : loadableXmlAsset2.mod);
						defPackage2.AddDef(def);
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
		}

		public static void ClearCachedPatches()
		{
			foreach (ModContentPack runningMod in runningMods)
			{
				foreach (PatchOperation patch in runningMod.Patches)
				{
					try
					{
						patch.Complete(runningMod.Name);
					}
					catch (Exception arg)
					{
						Log.Error("Error in patch.Complete(): " + arg);
					}
				}
				runningMod.ClearPatchesCache();
			}
		}

		public static void ClearDestroy()
		{
			foreach (ModContentPack runningMod in runningMods)
			{
				try
				{
					runningMod.ClearDestroy();
				}
				catch (Exception arg)
				{
					Log.Error("Error in mod.ClearDestroy(): " + arg);
				}
			}
			runningMods.Clear();
		}

		public static T GetMod<T>() where T : Mod
		{
			return GetMod(typeof(T)) as T;
		}

		public static Mod GetMod(Type type)
		{
			if (runningModClasses.ContainsKey(type))
			{
				return runningModClasses[type];
			}
			return (from kvp in runningModClasses
			where type.IsAssignableFrom(kvp.Key)
			select kvp).FirstOrDefault().Value;
		}

		private static string GetSettingsFilename(string modIdentifier, string modHandleName)
		{
			return Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"Mod_{modIdentifier}_{modHandleName}.xml"));
		}

		public static T ReadModSettings<T>(string modIdentifier, string modHandleName) where T : ModSettings, new()
		{
			string settingsFilename = GetSettingsFilename(modIdentifier, modHandleName);
			T target = (T)null;
			try
			{
				if (File.Exists(settingsFilename))
				{
					Scribe.loader.InitLoading(settingsFilename);
					try
					{
						Scribe_Deep.Look(ref target, "ModSettings");
					}
					finally
					{
						Scribe.loader.FinalizeLoading();
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning($"Caught exception while loading mod settings data for {modIdentifier}. Generating fresh settings. The exception was: {ex.ToString()}");
				target = (T)null;
			}
			if (target == null)
			{
				target = new T();
			}
			return target;
		}

		public static void WriteModSettings(string modIdentifier, string modHandleName, ModSettings settings)
		{
			Scribe.saver.InitSaving(GetSettingsFilename(modIdentifier, modHandleName), "SettingsBlock");
			try
			{
				Scribe_Deep.Look(ref settings, "ModSettings");
			}
			finally
			{
				Scribe.saver.FinalizeSaving();
			}
		}
	}
}
