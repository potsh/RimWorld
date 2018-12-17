using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class LoadedLanguage
	{
		public class KeyedReplacement
		{
			public string key;

			public string value;

			public string fileSource;

			public int fileSourceLine;

			public string fileSourceFullPath;

			public bool isPlaceholder;
		}

		public string folderName;

		public LanguageInfo info;

		private LanguageWorker workerInt;

		private LanguageWordInfo wordInfo = new LanguageWordInfo();

		private bool dataIsLoaded;

		public List<string> loadErrors = new List<string>();

		public List<string> backstoriesLoadErrors = new List<string>();

		public bool anyKeyedReplacementsXmlParseError;

		public string lastKeyedReplacementsXmlParseErrorInFile;

		public bool anyDefInjectionsXmlParseError;

		public string lastDefInjectionsXmlParseErrorInFile;

		public bool anyError;

		public Texture2D icon = BaseContent.BadTex;

		public Dictionary<string, KeyedReplacement> keyedReplacements = new Dictionary<string, KeyedReplacement>();

		public List<DefInjectionPackage> defInjections = new List<DefInjectionPackage>();

		public Dictionary<string, List<string>> stringFiles = new Dictionary<string, List<string>>();

		public const string OldKeyedTranslationsFolderName = "CodeLinked";

		public const string KeyedTranslationsFolderName = "Keyed";

		public const string OldDefInjectionsFolderName = "DefLinked";

		public const string DefInjectionsFolderName = "DefInjected";

		public const string LanguagesFolderName = "Languages";

		public const string PlaceholderText = "TODO";

		public string FriendlyNameNative
		{
			get
			{
				if (info == null || info.friendlyNameNative.NullOrEmpty())
				{
					return folderName;
				}
				return info.friendlyNameNative;
			}
		}

		public string FriendlyNameEnglish
		{
			get
			{
				if (info == null || info.friendlyNameEnglish.NullOrEmpty())
				{
					return folderName;
				}
				return info.friendlyNameEnglish;
			}
		}

		public IEnumerable<string> FolderPaths
		{
			get
			{
				foreach (ModContentPack runningMod in LoadedModManager.RunningMods)
				{
					string langDirPath = Path.Combine(runningMod.RootDir, "Languages");
					string myDirPath = Path.Combine(langDirPath, folderName);
					DirectoryInfo myDir = new DirectoryInfo(myDirPath);
					if (myDir.Exists)
					{
						yield return myDirPath;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_010b:
				/*Error near IL_010c: Unexpected return in MoveNext()*/;
			}
		}

		public LanguageWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (LanguageWorker)Activator.CreateInstance(info.languageWorkerClass);
				}
				return workerInt;
			}
		}

		public LoadedLanguage(string folderPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			folderName = directoryInfo.Name;
		}

		public void TryLoadMetadataFrom(string folderPath)
		{
			if (info == null)
			{
				string filePath = Path.Combine(folderPath.ToString(), "LanguageInfo.xml");
				info = DirectXmlLoader.ItemFromXmlFile<LanguageInfo>(filePath, resolveCrossRefs: false);
				if (info.friendlyNameNative.NullOrEmpty())
				{
					FileInfo fileInfo = new FileInfo(Path.Combine(folderPath.ToString(), "FriendlyName.txt"));
					if (fileInfo.Exists)
					{
						info.friendlyNameNative = GenFile.TextFromRawFile(fileInfo.ToString());
					}
				}
				if (info.friendlyNameNative.NullOrEmpty())
				{
					info.friendlyNameNative = folderName;
				}
				if (info.friendlyNameEnglish.NullOrEmpty())
				{
					info.friendlyNameEnglish = folderName;
				}
			}
		}

		public void LoadData()
		{
			if (!dataIsLoaded)
			{
				dataIsLoaded = true;
				DeepProfiler.Start("Loading language data: " + folderName);
				try
				{
					foreach (string folderPath in FolderPaths)
					{
						string localFolderPath = folderPath;
						LongEventHandler.ExecuteWhenFinished(delegate
						{
							if (icon == BaseContent.BadTex)
							{
								FileInfo fileInfo = new FileInfo(Path.Combine(localFolderPath.ToString(), "LangIcon.png"));
								if (fileInfo.Exists)
								{
									icon = ModContentLoader<Texture2D>.LoadItem(fileInfo.FullName).contentItem;
								}
							}
						});
						DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(folderPath.ToString(), "CodeLinked"));
						if (directoryInfo.Exists)
						{
							loadErrors.Add("Translations aren't called CodeLinked any more. Please rename to Keyed: " + directoryInfo);
						}
						else
						{
							directoryInfo = new DirectoryInfo(Path.Combine(folderPath.ToString(), "Keyed"));
						}
						if (directoryInfo.Exists)
						{
							FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
							foreach (FileInfo file in files)
							{
								LoadFromFile_Keyed(file);
							}
						}
						DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(folderPath.ToString(), "DefLinked"));
						if (directoryInfo2.Exists)
						{
							loadErrors.Add("Translations aren't called DefLinked any more. Please rename to DefInjected: " + directoryInfo2);
						}
						else
						{
							directoryInfo2 = new DirectoryInfo(Path.Combine(folderPath.ToString(), "DefInjected"));
						}
						if (directoryInfo2.Exists)
						{
							DirectoryInfo[] directories = directoryInfo2.GetDirectories("*", SearchOption.TopDirectoryOnly);
							foreach (DirectoryInfo directoryInfo3 in directories)
							{
								string name = directoryInfo3.Name;
								Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(name);
								if (typeInAnyAssembly == null && name.Length > 3)
								{
									typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(name.Substring(0, name.Length - 1));
								}
								if (typeInAnyAssembly == null)
								{
									loadErrors.Add("Error loading language from " + folderPath + ": dir " + directoryInfo3.Name + " doesn't correspond to any def type. Skipping...");
								}
								else
								{
									FileInfo[] files2 = directoryInfo3.GetFiles("*.xml", SearchOption.AllDirectories);
									foreach (FileInfo file2 in files2)
									{
										LoadFromFile_DefInject(file2, typeInAnyAssembly);
									}
								}
							}
						}
						EnsureAllDefTypesHaveDefInjectionPackage();
						DirectoryInfo directoryInfo4 = new DirectoryInfo(Path.Combine(folderPath.ToString(), "Strings"));
						if (directoryInfo4.Exists)
						{
							DirectoryInfo[] directories2 = directoryInfo4.GetDirectories("*", SearchOption.TopDirectoryOnly);
							foreach (DirectoryInfo directoryInfo5 in directories2)
							{
								FileInfo[] files3 = directoryInfo5.GetFiles("*.txt", SearchOption.AllDirectories);
								foreach (FileInfo file3 in files3)
								{
									LoadFromFile_Strings(file3, directoryInfo4);
								}
							}
						}
						wordInfo.LoadFrom(folderPath);
					}
				}
				catch (Exception arg)
				{
					Log.Error("Exception loading language data. Rethrowing. Exception: " + arg);
					throw;
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}

		private void LoadFromFile_Strings(FileInfo file, DirectoryInfo stringsTopDir)
		{
			string text;
			try
			{
				text = GenFile.TextFromRawFile(file.FullName);
			}
			catch (Exception ex)
			{
				loadErrors.Add("Exception loading from strings file " + file + ": " + ex);
				return;
			}
			string text2 = file.FullName;
			if (stringsTopDir != null)
			{
				text2 = text2.Substring(stringsTopDir.FullName.Length + 1);
			}
			text2 = text2.Substring(0, text2.Length - Path.GetExtension(text2).Length);
			text2 = text2.Replace('\\', '/');
			List<string> list = new List<string>();
			foreach (string item in GenText.LinesFromString(text))
			{
				list.Add(item);
			}
			if (stringFiles.TryGetValue(text2, out List<string> value))
			{
				foreach (string item2 in list)
				{
					value.Add(item2);
				}
			}
			else
			{
				stringFiles.Add(text2, list);
			}
		}

		private void LoadFromFile_Keyed(FileInfo file)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			try
			{
				foreach (DirectXmlLoaderSimple.XmlKeyValuePair item in DirectXmlLoaderSimple.ValuesFromXmlFile(file))
				{
					DirectXmlLoaderSimple.XmlKeyValuePair current = item;
					if (keyedReplacements.ContainsKey(current.key) || dictionary.ContainsKey(current.key))
					{
						loadErrors.Add("Duplicate keyed translation key: " + current.key + " in language " + folderName);
					}
					else
					{
						dictionary.Add(current.key, current.value);
						dictionary2.Add(current.key, current.lineNumber);
					}
				}
			}
			catch (Exception ex)
			{
				loadErrors.Add("Exception loading from translation file " + file + ": " + ex);
				dictionary.Clear();
				dictionary2.Clear();
				anyKeyedReplacementsXmlParseError = true;
				lastKeyedReplacementsXmlParseErrorInFile = file.Name;
			}
			foreach (KeyValuePair<string, string> item2 in dictionary)
			{
				string text = item2.Value;
				KeyedReplacement keyedReplacement = new KeyedReplacement();
				if (text == "TODO")
				{
					keyedReplacement.isPlaceholder = true;
					text = string.Empty;
				}
				keyedReplacement.key = item2.Key;
				keyedReplacement.value = text;
				keyedReplacement.fileSource = file.Name;
				keyedReplacement.fileSourceLine = dictionary2[item2.Key];
				keyedReplacement.fileSourceFullPath = file.FullName;
				keyedReplacements.Add(item2.Key, keyedReplacement);
			}
		}

		public void LoadFromFile_DefInject(FileInfo file, Type defType)
		{
			DefInjectionPackage defInjectionPackage = (from di in defInjections
			where di.defType == defType
			select di).FirstOrDefault();
			if (defInjectionPackage == null)
			{
				defInjectionPackage = new DefInjectionPackage(defType);
				defInjections.Add(defInjectionPackage);
			}
			defInjectionPackage.AddDataFromFile(file, out bool xmlParseError);
			if (xmlParseError)
			{
				anyDefInjectionsXmlParseError = true;
				lastDefInjectionsXmlParseErrorInFile = file.Name;
			}
		}

		private void EnsureAllDefTypesHaveDefInjectionPackage()
		{
			foreach (Type item in GenDefDatabase.AllDefTypesWithDatabases())
			{
				if (!defInjections.Any((DefInjectionPackage x) => x.defType == item))
				{
					defInjections.Add(new DefInjectionPackage(item));
				}
			}
		}

		public bool HaveTextForKey(string key, bool allowPlaceholders = false)
		{
			if (!dataIsLoaded)
			{
				LoadData();
			}
			if (key == null)
			{
				return false;
			}
			if (!keyedReplacements.TryGetValue(key, out KeyedReplacement value))
			{
				return false;
			}
			return allowPlaceholders || !value.isPlaceholder;
		}

		public bool TryGetTextFromKey(string key, out string translated)
		{
			if (!dataIsLoaded)
			{
				LoadData();
			}
			if (key == null)
			{
				translated = key;
				return false;
			}
			if (!keyedReplacements.TryGetValue(key, out KeyedReplacement value) || value.isPlaceholder)
			{
				translated = key;
				return false;
			}
			translated = value.value;
			return true;
		}

		public bool TryGetStringsFromFile(string fileName, out List<string> stringsList)
		{
			if (!dataIsLoaded)
			{
				LoadData();
			}
			if (!stringFiles.TryGetValue(fileName, out stringsList))
			{
				stringsList = null;
				return false;
			}
			return true;
		}

		public string GetKeySourceFileAndLine(string key)
		{
			if (!keyedReplacements.TryGetValue(key, out KeyedReplacement value))
			{
				return "unknown";
			}
			return value.fileSource + ":" + value.fileSourceLine;
		}

		public Gender ResolveGender(string str, string fallback = null)
		{
			return wordInfo.ResolveGender(str, fallback);
		}

		public void InjectIntoData_BeforeImpliedDefs()
		{
			if (!dataIsLoaded)
			{
				LoadData();
			}
			foreach (DefInjectionPackage defInjection in defInjections)
			{
				try
				{
					defInjection.InjectIntoDefs(errorOnDefNotFound: false);
				}
				catch (Exception arg)
				{
					Log.Error("Critical error while injecting translations into defs: " + arg);
				}
			}
		}

		public void InjectIntoData_AfterImpliedDefs()
		{
			if (!dataIsLoaded)
			{
				LoadData();
			}
			int num = loadErrors.Count;
			foreach (DefInjectionPackage defInjection in defInjections)
			{
				try
				{
					defInjection.InjectIntoDefs(errorOnDefNotFound: true);
					num += defInjection.loadErrors.Count;
				}
				catch (Exception arg)
				{
					Log.Error("Critical error while injecting translations into defs: " + arg);
				}
			}
			BackstoryTranslationUtility.LoadAndInjectBackstoryData(FolderPaths, backstoriesLoadErrors);
			num += backstoriesLoadErrors.Count;
			if (num != 0)
			{
				anyError = true;
				Log.Warning("Translation data for language " + LanguageDatabase.activeLanguage.FriendlyNameEnglish + " has " + num + " errors. Generate translation report for more info.");
			}
		}

		public override string ToString()
		{
			return info.friendlyNameEnglish;
		}
	}
}
