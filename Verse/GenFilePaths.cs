using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GenFilePaths
	{
		private static string saveDataPath = null;

		private static string coreModsFolderPath = null;

		public const string SoundsFolder = "Sounds/";

		public const string TexturesFolder = "Textures/";

		public const string StringsFolder = "Strings/";

		public const string DefsFolder = "Defs/";

		public const string PatchesFolder = "Patches/";

		public const string BackstoriesPath = "Backstories";

		public const string SavedGameExtension = ".rws";

		public const string ScenarioExtension = ".rsc";

		public const string ExternalHistoryFileExtension = ".rwh";

		private const string SaveDataFolderCommand = "savedatafolder";

		private static readonly string[] FilePathRaw = new string[73]
		{
			"Ž",
			"ž",
			"Ÿ",
			"¡",
			"¢",
			"£",
			"¤",
			"¥",
			"¦",
			"§",
			"\u00a8",
			"©",
			"ª",
			"À",
			"Á",
			"Â",
			"Ã",
			"Ä",
			"Å",
			"Æ",
			"Ç",
			"È",
			"É",
			"Ê",
			"Ë",
			"Ì",
			"Í",
			"Î",
			"Ï",
			"Ð",
			"Ñ",
			"Ò",
			"Ó",
			"Ô",
			"Õ",
			"Ö",
			"Ù",
			"Ú",
			"Û",
			"Ü",
			"Ý",
			"Þ",
			"ß",
			"à",
			"á",
			"â",
			"ã",
			"ä",
			"å",
			"æ",
			"ç",
			"è",
			"é",
			"ê",
			"ë",
			"ì",
			"í",
			"î",
			"ï",
			"ð",
			"ñ",
			"ò",
			"ó",
			"ô",
			"õ",
			"ö",
			"ù",
			"ú",
			"û",
			"ü",
			"ý",
			"þ",
			"ÿ"
		};

		private static readonly string[] FilePathSafe = new string[73]
		{
			"%8E",
			"%9E",
			"%9F",
			"%A1",
			"%A2",
			"%A3",
			"%A4",
			"%A5",
			"%A6",
			"%A7",
			"%A8",
			"%A9",
			"%AA",
			"%C0",
			"%C1",
			"%C2",
			"%C3",
			"%C4",
			"%C5",
			"%C6",
			"%C7",
			"%C8",
			"%C9",
			"%CA",
			"%CB",
			"%CC",
			"%CD",
			"%CE",
			"%CF",
			"%D0",
			"%D1",
			"%D2",
			"%D3",
			"%D4",
			"%D5",
			"%D6",
			"%D9",
			"%DA",
			"%DB",
			"%DC",
			"%DD",
			"%DE",
			"%DF",
			"%E0",
			"%E1",
			"%E2",
			"%E3",
			"%E4",
			"%E5",
			"%E6",
			"%E7",
			"%E8",
			"%E9",
			"%EA",
			"%EB",
			"%EC",
			"%ED",
			"%EE",
			"%EF",
			"%F0",
			"%F1",
			"%F2",
			"%F3",
			"%F4",
			"%F5",
			"%F6",
			"%F9",
			"%FA",
			"%FB",
			"%FC",
			"%FD",
			"%FE",
			"%FF"
		};

		public static string SaveDataFolderPath
		{
			get
			{
				if (saveDataPath == null)
				{
					if (GenCommandLine.TryGetCommandLineArg("savedatafolder", out string value))
					{
						value.TrimEnd('\\', '/');
						if (value == string.Empty)
						{
							value = string.Empty + Path.DirectorySeparatorChar;
						}
						saveDataPath = value;
						Log.Message("Save data folder overridden to " + saveDataPath);
					}
					else
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(UnityData.dataPath);
						if (UnityData.isEditor)
						{
							saveDataPath = Path.Combine(directoryInfo.Parent.ToString(), "SaveData");
						}
						else if (UnityData.platform == RuntimePlatform.OSXPlayer || UnityData.platform == RuntimePlatform.OSXEditor || UnityData.platform == RuntimePlatform.OSXDashboardPlayer)
						{
							DirectoryInfo parent = Directory.GetParent(UnityData.persistentDataPath);
							string path = Path.Combine(parent.ToString(), "RimWorld");
							if (!Directory.Exists(path))
							{
								Directory.CreateDirectory(path);
							}
							saveDataPath = path;
						}
						else
						{
							saveDataPath = Application.persistentDataPath;
						}
					}
					DirectoryInfo directoryInfo2 = new DirectoryInfo(saveDataPath);
					if (!directoryInfo2.Exists)
					{
						directoryInfo2.Create();
					}
				}
				return saveDataPath;
			}
		}

		public static string ScenarioPreviewImagePath
		{
			get
			{
				if (!UnityData.isEditor)
				{
					return Path.Combine(ExecutableDir.FullName, "ScenarioPreview.jpg");
				}
				return Path.Combine(Path.Combine(Path.Combine(ExecutableDir.FullName, "PlatformSpecific"), "All"), "ScenarioPreview.jpg");
			}
		}

		private static DirectoryInfo ExecutableDir => new DirectoryInfo(UnityData.dataPath).Parent;

		public static string CoreModsFolderPath
		{
			get
			{
				if (coreModsFolderPath == null)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(UnityData.dataPath);
					DirectoryInfo directoryInfo2 = (!UnityData.isEditor) ? directoryInfo.Parent : directoryInfo;
					coreModsFolderPath = Path.Combine(directoryInfo2.ToString(), "Mods");
					if (UnityData.isDebugBuild)
					{
						DirectoryInfo directoryInfo3 = new DirectoryInfo(coreModsFolderPath);
						if (!directoryInfo3.Exists)
						{
							coreModsFolderPath = Path.Combine(directoryInfo.Parent.Parent.ToString(), "RimWorld/Assets/Mods");
						}
					}
					DirectoryInfo directoryInfo4 = new DirectoryInfo(coreModsFolderPath);
					if (!directoryInfo4.Exists)
					{
						directoryInfo4.Create();
					}
				}
				return coreModsFolderPath;
			}
		}

		public static string ConfigFolderPath => FolderUnderSaveData("Config");

		private static string SavedGamesFolderPath => FolderUnderSaveData("Saves");

		private static string ScenariosFolderPath => FolderUnderSaveData("Scenarios");

		private static string ExternalHistoryFolderPath => FolderUnderSaveData("External");

		public static string ScreenshotFolderPath => FolderUnderSaveData("Screenshots");

		public static string DevOutputFolderPath => FolderUnderSaveData("DevOutput");

		public static string ModsConfigFilePath => Path.Combine(ConfigFolderPath, "ModsConfig.xml");

		public static string ConceptKnowledgeFilePath => Path.Combine(ConfigFolderPath, "Knowledge.xml");

		public static string PrefsFilePath => Path.Combine(ConfigFolderPath, "Prefs.xml");

		public static string KeyPrefsFilePath => Path.Combine(ConfigFolderPath, "KeyPrefs.xml");

		public static string LastPlayedVersionFilePath => Path.Combine(ConfigFolderPath, "LastPlayedVersion.txt");

		public static string DevModePermanentlyDisabledFilePath => Path.Combine(ConfigFolderPath, "DevModeDisabled");

		public static string BackstoryOutputFilePath => Path.Combine(DevOutputFolderPath, "Fresh_Backstories.xml");

		public static string TempFolderPath => Application.temporaryCachePath;

		public static IEnumerable<FileInfo> AllSavedGameFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(SavedGamesFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
				where f.Extension == ".rws"
				orderby f.LastWriteTime descending
				select f;
			}
		}

		public static IEnumerable<FileInfo> AllCustomScenarioFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(ScenariosFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
				where f.Extension == ".rsc"
				orderby f.LastWriteTime descending
				select f;
			}
		}

		public static IEnumerable<FileInfo> AllExternalHistoryFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(ExternalHistoryFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
				where f.Extension == ".rwh"
				orderby f.LastWriteTime descending
				select f;
			}
		}

		private static string FolderUnderSaveData(string folderName)
		{
			string text = Path.Combine(SaveDataFolderPath, folderName);
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			return text;
		}

		public static string FilePathForSavedGame(string gameName)
		{
			return Path.Combine(SavedGamesFolderPath, gameName + ".rws");
		}

		public static string AbsPathForScenario(string scenarioName)
		{
			return Path.Combine(ScenariosFolderPath, scenarioName + ".rsc");
		}

		public static string ContentPath<T>()
		{
			if (typeof(T) == typeof(AudioClip))
			{
				return "Sounds/";
			}
			if (typeof(T) == typeof(Texture2D))
			{
				return "Textures/";
			}
			if (typeof(T) == typeof(string))
			{
				return "Strings/";
			}
			throw new ArgumentException();
		}

		public static string FolderPathRelativeToDefsFolder(string fullFolderPath, ModContentPack mod)
		{
			fullFolderPath = Path.GetFullPath(fullFolderPath).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			string text = Path.GetFullPath(Path.Combine(mod.RootDir, "Defs/")).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			if (!fullFolderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				fullFolderPath += Path.DirectorySeparatorChar;
			}
			if (!text.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				text += Path.DirectorySeparatorChar;
			}
			if (!fullFolderPath.StartsWith(text))
			{
				Log.Error("Can't get relative path. Path \"" + fullFolderPath + "\" does not start with \"" + text + "\".");
				return null;
			}
			if (fullFolderPath == text)
			{
				return string.Empty;
			}
			string text2 = fullFolderPath.Substring(text.Length);
			while (text2.StartsWith("/") || text2.StartsWith("\\"))
			{
				if (text2.Length == 1)
				{
					return string.Empty;
				}
				text2 = text2.Substring(1);
			}
			return text2;
		}

		public static string SafeURIForUnityWWWFromPath(string rawPath)
		{
			string text = rawPath;
			for (int i = 0; i < FilePathRaw.Length; i++)
			{
				text = text.Replace(FilePathRaw[i], FilePathSafe[i]);
			}
			return "file:///" + text;
		}
	}
}
