using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public static class LanguageDatabase
	{
		private static List<LoadedLanguage> languages = new List<LoadedLanguage>();

		public static LoadedLanguage activeLanguage;

		public static LoadedLanguage defaultLanguage;

		public static readonly string DefaultLangFolderName = "English";

		private static readonly List<string> SupportedAutoSelectLanguages = new List<string>
		{
			"Arabic",
			"ChineseSimplified",
			"ChineseTraditional",
			"Czech",
			"Danish",
			"Dutch",
			"English",
			"Estonian",
			"Finnish",
			"French",
			"German",
			"Hungarian",
			"Italian",
			"Japanese",
			"Korean",
			"Norwegian",
			"Polish",
			"Portuguese",
			"PortugueseBrazilian",
			"Romanian",
			"Russian",
			"Slovak",
			"Spanish",
			"SpanishLatin",
			"Swedish",
			"Turkish",
			"Ukrainian"
		};

		public static IEnumerable<LoadedLanguage> AllLoadedLanguages => languages;

		public static void SelectLanguage(LoadedLanguage lang)
		{
			Prefs.LangFolderName = lang.folderName;
			LongEventHandler.QueueLongEvent(delegate
			{
				PlayDataLoader.ClearAllPlayData();
				PlayDataLoader.LoadAllPlayData();
			}, "LoadingLongEvent", doAsynchronously: true, null);
		}

		public static void Clear()
		{
			languages.Clear();
			activeLanguage = null;
		}

		public static void LoadAllMetadata()
		{
			foreach (ModContentPack runningMod in LoadedModManager.RunningMods)
			{
				string path = Path.Combine(runningMod.RootDir, "Languages");
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				if (directoryInfo.Exists)
				{
					DirectoryInfo[] directories = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
					foreach (DirectoryInfo langDir in directories)
					{
						LoadLanguageMetadataFrom(langDir);
					}
				}
			}
			defaultLanguage = languages.FirstOrDefault((LoadedLanguage la) => la.folderName == DefaultLangFolderName);
			activeLanguage = languages.FirstOrDefault((LoadedLanguage la) => la.folderName == Prefs.LangFolderName);
			if (activeLanguage == null)
			{
				Prefs.LangFolderName = DefaultLangFolderName;
				activeLanguage = languages.FirstOrDefault((LoadedLanguage la) => la.folderName == Prefs.LangFolderName);
			}
			if (activeLanguage == null || defaultLanguage == null)
			{
				Log.Error("No default language found!");
				defaultLanguage = languages[0];
				activeLanguage = languages[0];
			}
		}

		private static LoadedLanguage LoadLanguageMetadataFrom(DirectoryInfo langDir)
		{
			LoadedLanguage loadedLanguage = languages.FirstOrDefault((LoadedLanguage lib) => lib.folderName == langDir.Name);
			if (loadedLanguage == null)
			{
				loadedLanguage = new LoadedLanguage(langDir.ToString());
				languages.Add(loadedLanguage);
			}
			loadedLanguage?.TryLoadMetadataFrom(langDir.ToString());
			return loadedLanguage;
		}

		public static string SystemLanguageFolderName()
		{
			if (SteamManager.Initialized)
			{
				string text = SteamApps.GetCurrentGameLanguage().CapitalizeFirst();
				if (SupportedAutoSelectLanguages.Contains(text))
				{
					return text;
				}
			}
			string text2 = Application.systemLanguage.ToString();
			if (SupportedAutoSelectLanguages.Contains(text2))
			{
				return text2;
			}
			return DefaultLangFolderName;
		}
	}
}
