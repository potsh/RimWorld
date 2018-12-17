using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse.Steam;

namespace Verse
{
	public static class ModLister
	{
		private static List<ModMetaData> mods;

		public static IEnumerable<ModMetaData> AllInstalledMods => mods;

		public static IEnumerable<DirectoryInfo> AllActiveModDirs => from mod in mods
		where mod.Active
		select mod.RootDir;

		static ModLister()
		{
			mods = new List<ModMetaData>();
			RebuildModList();
		}

		public static void EnsureInit()
		{
		}

		public static void RebuildModList()
		{
			string s = "Rebuilding mods list";
			mods.Clear();
			s += "\nAdding mods from mods folder:";
			foreach (string item in from d in new DirectoryInfo(GenFilePaths.CoreModsFolderPath).GetDirectories()
			select d.FullName)
			{
				ModMetaData modMetaData = new ModMetaData(item);
				mods.Add(modMetaData);
				s = s + "\n  Adding " + modMetaData.ToStringLong();
			}
			s += "\nAdding mods from Steam:";
			foreach (WorkshopItem item2 in from it in WorkshopItems.AllSubscribedItems
			where it is WorkshopItem_Mod
			select it)
			{
				ModMetaData modMetaData2 = new ModMetaData(item2);
				mods.Add(modMetaData2);
				s = s + "\n  Adding " + modMetaData2.ToStringLong();
			}
			s += "\nDeactivating not-installed mods:";
			ModsConfig.DeactivateNotInstalledMods(delegate(string log)
			{
				s = s + "\n   " + log;
			});
			if (mods.Count((ModMetaData m) => m.Active) == 0)
			{
				s += "\nThere are no active mods. Activating Core mod.";
				mods.First((ModMetaData m) => m.IsCoreMod).Active = true;
			}
			if (Prefs.LogVerbose)
			{
				Log.Message(s);
			}
		}

		public static int InstalledModsListHash(bool activeOnly)
		{
			int num = 17;
			List<ModMetaData> list = ModsConfig.ActiveModsInLoadOrder.ToList();
			for (int i = 0; i < list.Count(); i++)
			{
				if (!activeOnly || list[i].Active)
				{
					num = num * 31 + list[i].GetHashCode();
					num = num * 31 + i * 2654241;
				}
			}
			return num;
		}

		public static ModMetaData GetModWithIdentifier(string identifier)
		{
			for (int i = 0; i < mods.Count; i++)
			{
				if (mods[i].Identifier == identifier)
				{
					return mods[i];
				}
			}
			return null;
		}

		public static bool HasActiveModWithName(string name)
		{
			for (int i = 0; i < mods.Count; i++)
			{
				if (mods[i].Active && mods[i].Name == name)
				{
					return true;
				}
			}
			return false;
		}
	}
}
