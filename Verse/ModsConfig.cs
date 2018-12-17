using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Verse
{
	public static class ModsConfig
	{
		private class ModsConfigData
		{
			[LoadAlias("buildNumber")]
			public string version;

			public List<string> activeMods = new List<string>();
		}

		private static ModsConfigData data;

		[CompilerGenerated]
		private static Func<char, bool> _003C_003Ef__mg_0024cache0;

		public static IEnumerable<ModMetaData> ActiveModsInLoadOrder
		{
			get
			{
				ModLister.EnsureInit();
				int i = 0;
				if (i < data.activeMods.Count)
				{
					yield return ModLister.GetModWithIdentifier(data.activeMods[i]);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		static ModsConfig()
		{
			bool flag = false;
			data = DirectXmlLoader.ItemFromXmlFile<ModsConfigData>(GenFilePaths.ModsConfigFilePath);
			if (data.version != null)
			{
				bool flag2 = false;
				int result;
				if (data.version.Contains("."))
				{
					int num = VersionControl.MinorFromVersionString(data.version);
					int num2 = VersionControl.MajorFromVersionString(data.version);
					if (num2 != VersionControl.CurrentMajor || num != VersionControl.CurrentMinor)
					{
						flag2 = true;
					}
				}
				else if (data.version.Length > 0 && data.version.All(char.IsNumber) && int.TryParse(data.version, out result) && result <= 2009)
				{
					flag2 = true;
				}
				if (flag2)
				{
					Log.Message("Mods config data is from version " + data.version + " while we are running " + VersionControl.CurrentVersionStringWithRev + ". Resetting.");
					data = new ModsConfigData();
					flag = true;
				}
			}
			if (!File.Exists(GenFilePaths.ModsConfigFilePath) || flag)
			{
				data.activeMods.Add(ModContentPack.CoreModIdentifier);
				Save();
			}
		}

		public static void DeactivateNotInstalledMods(Action<string> logCallback = null)
		{
			int i;
			for (i = data.activeMods.Count - 1; i >= 0; i--)
			{
				if (!ModLister.AllInstalledMods.Any((ModMetaData m) => m.Identifier == data.activeMods[i]))
				{
					logCallback?.Invoke("Deactivating " + data.activeMods[i]);
					data.activeMods.RemoveAt(i);
				}
			}
		}

		public static void Reset()
		{
			data.activeMods.Clear();
			data.activeMods.Add(ModContentPack.CoreModIdentifier);
			Save();
		}

		public static void Reorder(int modIndex, int newIndex)
		{
			if (modIndex != newIndex)
			{
				data.activeMods.Insert(newIndex, data.activeMods[modIndex]);
				data.activeMods.RemoveAt((modIndex >= newIndex) ? (modIndex + 1) : modIndex);
			}
		}

		public static bool IsActive(ModMetaData mod)
		{
			return data.activeMods.Contains(mod.Identifier);
		}

		public static void SetActive(ModMetaData mod, bool active)
		{
			SetActive(mod.Identifier, active);
		}

		public static void SetActive(string modIdentifier, bool active)
		{
			if (active)
			{
				if (!data.activeMods.Contains(modIdentifier))
				{
					data.activeMods.Add(modIdentifier);
				}
			}
			else if (data.activeMods.Contains(modIdentifier))
			{
				data.activeMods.Remove(modIdentifier);
			}
		}

		public static void SetActiveToList(List<string> mods)
		{
			data.activeMods = (from mod in mods
			where ModLister.GetModWithIdentifier(mod) != null
			select mod).ToList();
		}

		public static void Save()
		{
			data.version = VersionControl.CurrentVersionStringWithRev;
			DirectXmlSaver.SaveDataObject(data, GenFilePaths.ModsConfigFilePath);
		}

		public static void RestartFromChangedMods()
		{
			Find.WindowStack.Add(new Dialog_MessageBox("ModsChanged".Translate(), null, delegate
			{
				GenCommandLine.Restart();
			}));
		}
	}
}
