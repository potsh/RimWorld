using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Verse
{
	public static class TranslationFilesCleaner
	{
		private class PossibleDefInjection
		{
			public string suggestedPath;

			public string normalizedPath;

			public bool isCollection;

			public bool fullListTranslationAllowed;

			public string curValue;

			public IEnumerable<string> curValueCollection;

			public FieldInfo fieldInfo;

			public Def def;
		}

		private const string NewlineTag = "NEWLINE";

		private const string NewlineTagFull = "<!--NEWLINE-->";

		[CompilerGenerated]
		private static Action _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Func<char, bool> _003C_003Ef__mg_0024cache1;

		public static void CleanupTranslationFiles()
		{
			LoadedLanguage curLang = LanguageDatabase.activeLanguage;
			LoadedLanguage english = LanguageDatabase.defaultLanguage;
			if (curLang != english)
			{
				IEnumerable<ModMetaData> activeModsInLoadOrder = ModsConfig.ActiveModsInLoadOrder;
				if (activeModsInLoadOrder.Count() != 1 || !activeModsInLoadOrder.First().IsCoreMod)
				{
					Messages.Message("MessageDisableModsBeforeCleaningTranslationFiles".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					LongEventHandler.QueueLongEvent(delegate
					{
						if (curLang.anyKeyedReplacementsXmlParseError || curLang.anyDefInjectionsXmlParseError)
						{
							string value = curLang.lastKeyedReplacementsXmlParseErrorInFile ?? curLang.lastDefInjectionsXmlParseErrorInFile;
							Messages.Message("MessageCantCleanupTranslationFilesBeucaseOfXmlError".Translate(value), MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							english.LoadData();
							curLang.LoadData();
							Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("ConfirmCleanupTranslationFiles".Translate(curLang.FriendlyNameNative), delegate
							{
								LongEventHandler.QueueLongEvent(DoCleanupTranslationFiles, "CleaningTranslationFiles".Translate(), doAsynchronously: true, null);
							}, destructive: true);
							dialog_MessageBox.buttonAText = "ConfirmCleanupTranslationFiles_Confirm".Translate();
							Find.WindowStack.Add(dialog_MessageBox);
						}
					}, null, doAsynchronously: false, null);
				}
			}
		}

		private static void DoCleanupTranslationFiles()
		{
			if (LanguageDatabase.activeLanguage != LanguageDatabase.defaultLanguage)
			{
				try
				{
					try
					{
						CleanupKeyedTranslations();
					}
					catch (Exception arg)
					{
						Log.Error("Could not cleanup keyed translations: " + arg);
					}
					try
					{
						CleanupDefInjections();
					}
					catch (Exception arg2)
					{
						Log.Error("Could not cleanup def-injections: " + arg2);
					}
					try
					{
						CleanupBackstories();
					}
					catch (Exception arg3)
					{
						Log.Error("Could not cleanup backstories: " + arg3);
					}
					Messages.Message("MessageTranslationFilesCleanupDone".Translate(GetActiveLanguageCoreModFolderPath()), MessageTypeDefOf.TaskCompletion, historical: false);
				}
				catch (Exception arg4)
				{
					Log.Error("Could not cleanup translation files: " + arg4);
				}
			}
		}

		private static void CleanupKeyedTranslations()
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage english = LanguageDatabase.defaultLanguage;
			string activeLanguageCoreModFolderPath = GetActiveLanguageCoreModFolderPath();
			string text = Path.Combine(activeLanguageCoreModFolderPath, "CodeLinked");
			string text2 = Path.Combine(activeLanguageCoreModFolderPath, "Keyed");
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			if (directoryInfo.Exists)
			{
				if (!Directory.Exists(text2))
				{
					Directory.Move(text, text2);
					Thread.Sleep(1000);
					directoryInfo = new DirectoryInfo(text2);
				}
			}
			else
			{
				directoryInfo = new DirectoryInfo(text2);
			}
			if (!directoryInfo.Exists)
			{
				Log.Error("Could not find keyed translations folder for the active language.");
			}
			else
			{
				DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(GetEnglishLanguageCoreModFolderPath(), "Keyed"));
				if (!directoryInfo2.Exists)
				{
					Log.Error("English keyed translations folder doesn't exist.");
				}
				else
				{
					FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
					foreach (FileInfo fileInfo in files)
					{
						try
						{
							fileInfo.Delete();
						}
						catch (Exception ex)
						{
							Log.Error("Could not delete " + fileInfo.Name + ": " + ex);
						}
					}
					FileInfo[] files2 = directoryInfo2.GetFiles("*.xml", SearchOption.AllDirectories);
					foreach (FileInfo fileInfo2 in files2)
					{
						try
						{
							string path = new Uri(directoryInfo2.FullName + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(fileInfo2.FullName)).ToString();
							string text3 = Path.Combine(directoryInfo.FullName, path);
							Directory.CreateDirectory(Path.GetDirectoryName(text3));
							fileInfo2.CopyTo(text3);
						}
						catch (Exception ex2)
						{
							Log.Error("Could not copy " + fileInfo2.Name + ": " + ex2);
						}
					}
					List<LoadedLanguage.KeyedReplacement> list = (from x in activeLanguage.keyedReplacements
					where !x.Value.isPlaceholder && !english.HaveTextForKey(x.Key)
					select x.Value).ToList();
					HashSet<LoadedLanguage.KeyedReplacement> writtenUnusedKeyedTranslations = new HashSet<LoadedLanguage.KeyedReplacement>();
					FileInfo[] files3 = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
					foreach (FileInfo fileInfo3 in files3)
					{
						try
						{
							XDocument xDocument = XDocument.Load(fileInfo3.FullName, LoadOptions.PreserveWhitespace);
							XElement xElement = xDocument.DescendantNodes().OfType<XElement>().FirstOrDefault();
							if (xElement != null)
							{
								try
								{
									foreach (XNode item in xElement.DescendantNodes())
									{
										XElement xElement2 = item as XElement;
										if (xElement2 != null)
										{
											foreach (XNode item2 in xElement2.DescendantNodes())
											{
												try
												{
													XText xText = item2 as XText;
													if (xText != null && !xText.Value.NullOrEmpty())
													{
														string value = " EN: " + xText.Value + " ";
														item.AddBeforeSelf(new XComment(value));
														item.AddBeforeSelf(Environment.NewLine);
														item.AddBeforeSelf("  ");
													}
												}
												catch (Exception ex3)
												{
													Log.Error("Could not add comment node in " + fileInfo3.Name + ": " + ex3);
												}
												item2.Remove();
											}
											try
											{
												if (activeLanguage.TryGetTextFromKey(xElement2.Name.ToString(), out string translated))
												{
													if (!translated.NullOrEmpty())
													{
														xElement2.Add(new XText(translated.Replace("\n", "\\n")));
													}
												}
												else
												{
													xElement2.Add(new XText("TODO"));
												}
											}
											catch (Exception ex4)
											{
												Log.Error("Could not add existing translation or placeholder in " + fileInfo3.Name + ": " + ex4);
											}
										}
									}
									bool flag = false;
									foreach (LoadedLanguage.KeyedReplacement item3 in list)
									{
										if (new Uri(fileInfo3.FullName).Equals(new Uri(item3.fileSourceFullPath)))
										{
											if (!flag)
											{
												xElement.Add("  ");
												xElement.Add(new XComment(" UNUSED "));
												xElement.Add(Environment.NewLine);
												flag = true;
											}
											XElement xElement3 = new XElement(item3.key);
											if (item3.isPlaceholder)
											{
												xElement3.Add(new XText("TODO"));
											}
											else if (!item3.value.NullOrEmpty())
											{
												xElement3.Add(new XText(item3.value.Replace("\n", "\\n")));
											}
											xElement.Add("  ");
											xElement.Add(xElement3);
											xElement.Add(Environment.NewLine);
											writtenUnusedKeyedTranslations.Add(item3);
										}
									}
									if (flag)
									{
										xElement.Add(Environment.NewLine);
									}
								}
								finally
								{
									SaveXMLDocumentWithProcessedNewlineTags(xDocument.Root, fileInfo3.FullName);
								}
							}
						}
						catch (Exception ex5)
						{
							Log.Error("Could not process " + fileInfo3.Name + ": " + ex5);
						}
					}
					foreach (IGrouping<string, LoadedLanguage.KeyedReplacement> item4 in from x in list
					where !writtenUnusedKeyedTranslations.Contains(x)
					group x by x.fileSourceFullPath)
					{
						try
						{
							if (File.Exists(item4.Key))
							{
								Log.Error("Could not save unused keyed translations to " + item4.Key + " because this file already exists.");
							}
							else
							{
								XDocument doc = new XDocument(new XElement("LanguageData", new XComment("NEWLINE"), new XComment(" UNUSED "), item4.Select(delegate(LoadedLanguage.KeyedReplacement x)
								{
									string text4 = (!x.isPlaceholder) ? x.value : "TODO";
									return new XElement(x.key, new XText((!text4.NullOrEmpty()) ? text4.Replace("\n", "\\n") : string.Empty));
								}), new XComment("NEWLINE")));
								SaveXMLDocumentWithProcessedNewlineTags(doc, item4.Key);
							}
						}
						catch (Exception ex6)
						{
							Log.Error("Could not save unused keyed translations to " + item4.Key + ": " + ex6);
						}
					}
				}
			}
		}

		private static void CleanupDefInjections()
		{
			string activeLanguageCoreModFolderPath = GetActiveLanguageCoreModFolderPath();
			string text = Path.Combine(activeLanguageCoreModFolderPath, "DefLinked");
			string text2 = Path.Combine(activeLanguageCoreModFolderPath, "DefInjected");
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			if (directoryInfo.Exists)
			{
				if (!Directory.Exists(text2))
				{
					Directory.Move(text, text2);
					Thread.Sleep(1000);
					directoryInfo = new DirectoryInfo(text2);
				}
			}
			else
			{
				directoryInfo = new DirectoryInfo(text2);
			}
			if (!directoryInfo.Exists)
			{
				Log.Error("Could not find def-injections folder for the active language.");
			}
			else
			{
				FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
				foreach (FileInfo fileInfo in files)
				{
					try
					{
						fileInfo.Delete();
					}
					catch (Exception ex)
					{
						Log.Error("Could not delete " + fileInfo.Name + ": " + ex);
					}
				}
				foreach (Type item in GenDefDatabase.AllDefTypesWithDatabases())
				{
					try
					{
						CleanupDefInjectionsForDefType(item, directoryInfo.FullName);
					}
					catch (Exception ex2)
					{
						Log.Error("Could not process def-injections for type " + item.Name + ": " + ex2);
					}
				}
			}
		}

		private static void CleanupDefInjectionsForDefType(Type defType, string defInjectionsFolderPath)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			List<KeyValuePair<string, DefInjectionPackage.DefInjection>> list = (from x in (from x in activeLanguage.defInjections
			where x.defType == defType
			select x).SelectMany((DefInjectionPackage x) => x.injections)
			where !x.Value.isPlaceholder
			select x).ToList();
			Dictionary<string, DefInjectionPackage.DefInjection> dictionary = new Dictionary<string, DefInjectionPackage.DefInjection>();
			foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> item2 in list)
			{
				if (!dictionary.ContainsKey(item2.Value.normalizedPath))
				{
					dictionary.Add(item2.Value.normalizedPath, item2.Value);
				}
			}
			List<PossibleDefInjection> possibleDefInjections = new List<PossibleDefInjection>();
			DefInjectionUtility.ForEachPossibleDefInjection(defType, delegate(string suggestedPath, string normalizedPath, bool isCollection, string str, IEnumerable<string> collection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def)
			{
				if (translationAllowed)
				{
					PossibleDefInjection item = new PossibleDefInjection
					{
						suggestedPath = suggestedPath,
						normalizedPath = normalizedPath,
						isCollection = isCollection,
						fullListTranslationAllowed = fullListTranslationAllowed,
						curValue = str,
						curValueCollection = collection,
						fieldInfo = fieldInfo,
						def = def
					};
					possibleDefInjections.Add(item);
				}
			});
			if (possibleDefInjections.Any() || list.Any())
			{
				List<KeyValuePair<string, DefInjectionPackage.DefInjection>> source = (from x in list
				where !x.Value.injected
				select x).ToList();
				foreach (string item3 in (from x in possibleDefInjections
				select GetSourceFile(x.def)).Concat(from x in source
				select x.Value.fileSource).Distinct())
				{
					try
					{
						XDocument xDocument = new XDocument();
						bool flag = false;
						try
						{
							XElement xElement = new XElement("LanguageData");
							xDocument.Add(xElement);
							xElement.Add(new XComment("NEWLINE"));
							List<PossibleDefInjection> source2 = (from x in possibleDefInjections
							where GetSourceFile(x.def) == item3
							select x).ToList();
							List<KeyValuePair<string, DefInjectionPackage.DefInjection>> source3 = (from x in source
							where x.Value.fileSource == item3
							select x).ToList();
							foreach (string item4 in from x in (from x in source2
							select x.def.defName).Concat(from x in source3
							select x.Value.DefName).Distinct()
							orderby x
							select x)
							{
								try
								{
									IEnumerable<PossibleDefInjection> enumerable = source2.Where((PossibleDefInjection x) => x.def.defName == item4);
									IEnumerable<KeyValuePair<string, DefInjectionPackage.DefInjection>> enumerable2 = source3.Where((KeyValuePair<string, DefInjectionPackage.DefInjection> x) => x.Value.DefName == item4);
									if (enumerable.Any())
									{
										bool flag2 = false;
										foreach (PossibleDefInjection item5 in enumerable)
										{
											if (item5.isCollection)
											{
												IEnumerable<string> englishList = GetEnglishList(item5.normalizedPath, item5.curValueCollection, dictionary);
												bool flag3 = false;
												if (englishList != null)
												{
													int num = 0;
													foreach (string item6 in englishList)
													{
														if (dictionary.ContainsKey(item5.normalizedPath + "." + num))
														{
															flag3 = true;
															break;
														}
														num++;
													}
												}
												if (flag3 || !item5.fullListTranslationAllowed)
												{
													if (englishList != null)
													{
														int num2 = -1;
														foreach (string item7 in englishList)
														{
															num2++;
															string key = item5.normalizedPath + "." + num2;
															string suggestedPath2 = item5.suggestedPath + "." + num2;
															if (!dictionary.TryGetValue(key, out DefInjectionPackage.DefInjection value))
															{
																value = null;
															}
															if (value != null || DefInjectionUtility.ShouldCheckMissingInjection(item7, item5.fieldInfo, item5.def))
															{
																flag2 = true;
																flag = true;
																try
																{
																	if (!item7.NullOrEmpty())
																	{
																		xElement.Add(new XComment(" EN: " + item7.Replace("\n", "\\n") + " "));
																	}
																}
																catch (Exception ex)
																{
																	Log.Error("Could not add comment node in " + item3 + ": " + ex);
																}
																xElement.Add(GetDefInjectableFieldNode(suggestedPath2, value));
															}
														}
													}
												}
												else
												{
													bool flag4 = false;
													if (englishList != null)
													{
														foreach (string item8 in englishList)
														{
															if (DefInjectionUtility.ShouldCheckMissingInjection(item8, item5.fieldInfo, item5.def))
															{
																flag4 = true;
																break;
															}
														}
													}
													if (!dictionary.TryGetValue(item5.normalizedPath, out DefInjectionPackage.DefInjection value2))
													{
														value2 = null;
													}
													if (value2 != null || flag4)
													{
														flag2 = true;
														flag = true;
														try
														{
															string text = ListToLiNodesString(englishList);
															if (!text.NullOrEmpty())
															{
																xElement.Add(new XComment(" EN:\n" + text.Indented() + "\n  "));
															}
														}
														catch (Exception ex2)
														{
															Log.Error("Could not add comment node in " + item3 + ": " + ex2);
														}
														xElement.Add(GetDefInjectableFieldNode(item5.suggestedPath, value2));
													}
												}
											}
											else
											{
												if (!dictionary.TryGetValue(item5.normalizedPath, out DefInjectionPackage.DefInjection value3))
												{
													value3 = null;
												}
												string text2 = (value3 == null || !value3.injected) ? item5.curValue : value3.replacedString;
												if (value3 != null || DefInjectionUtility.ShouldCheckMissingInjection(text2, item5.fieldInfo, item5.def))
												{
													flag2 = true;
													flag = true;
													try
													{
														if (!text2.NullOrEmpty())
														{
															xElement.Add(new XComment(" EN: " + text2.Replace("\n", "\\n") + " "));
														}
													}
													catch (Exception ex3)
													{
														Log.Error("Could not add comment node in " + item3 + ": " + ex3);
													}
													xElement.Add(GetDefInjectableFieldNode(item5.suggestedPath, value3));
												}
											}
										}
										if (flag2)
										{
											xElement.Add(new XComment("NEWLINE"));
										}
									}
									if (enumerable2.Any())
									{
										flag = true;
										xElement.Add(new XComment(" UNUSED "));
										foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> item9 in enumerable2)
										{
											xElement.Add(GetDefInjectableFieldNode(item9.Value.path, item9.Value));
										}
										xElement.Add(new XComment("NEWLINE"));
									}
								}
								catch (Exception ex4)
								{
									Log.Error("Could not process def-injections for def " + item4 + ": " + ex4);
								}
							}
						}
						finally
						{
							if (flag)
							{
								string text3 = Path.Combine(defInjectionsFolderPath, defType.Name);
								Directory.CreateDirectory(text3);
								SaveXMLDocumentWithProcessedNewlineTags(xDocument, Path.Combine(text3, item3));
							}
						}
					}
					catch (Exception ex5)
					{
						Log.Error("Could not process def-injections for file " + item3 + ": " + ex5);
					}
				}
			}
		}

		private static void CleanupBackstories()
		{
			string activeLanguageCoreModFolderPath = GetActiveLanguageCoreModFolderPath();
			string text = Path.Combine(activeLanguageCoreModFolderPath, "Backstories");
			Directory.CreateDirectory(text);
			string path = Path.Combine(text, "Backstories.xml");
			File.Delete(path);
			XDocument xDocument = new XDocument();
			try
			{
				XElement xElement = new XElement("BackstoryTranslations");
				xDocument.Add(xElement);
				xElement.Add(new XComment("NEWLINE"));
				foreach (KeyValuePair<string, Backstory> item in from x in BackstoryDatabase.allBackstories
				orderby x.Key
				select x)
				{
					try
					{
						XElement xElement2 = new XElement(item.Key);
						AddBackstoryFieldElement(xElement2, "title", item.Value.title, item.Value.untranslatedTitle, item.Value.titleTranslated);
						AddBackstoryFieldElement(xElement2, "titleFemale", item.Value.titleFemale, item.Value.untranslatedTitleFemale, item.Value.titleFemaleTranslated);
						AddBackstoryFieldElement(xElement2, "titleShort", item.Value.titleShort, item.Value.untranslatedTitleShort, item.Value.titleShortTranslated);
						AddBackstoryFieldElement(xElement2, "titleShortFemale", item.Value.titleShortFemale, item.Value.untranslatedTitleShortFemale, item.Value.titleShortFemaleTranslated);
						AddBackstoryFieldElement(xElement2, "desc", item.Value.baseDesc, item.Value.untranslatedDesc, item.Value.descTranslated);
						xElement.Add(xElement2);
						xElement.Add(new XComment("NEWLINE"));
					}
					catch (Exception ex)
					{
						Log.Error("Could not process backstory " + item.Key + ": " + ex);
					}
				}
			}
			finally
			{
				SaveXMLDocumentWithProcessedNewlineTags(xDocument, path);
			}
		}

		private static void AddBackstoryFieldElement(XElement addTo, string fieldName, string currentValue, string untranslatedValue, bool wasTranslated)
		{
			if (wasTranslated || !untranslatedValue.NullOrEmpty())
			{
				if (!untranslatedValue.NullOrEmpty())
				{
					addTo.Add(new XComment(" EN: " + untranslatedValue.Replace("\n", "\\n") + " "));
				}
				string text = (!wasTranslated) ? "TODO" : currentValue;
				addTo.Add(new XElement(fieldName, (!text.NullOrEmpty()) ? text.Replace("\n", "\\n") : string.Empty));
			}
		}

		private static string GetActiveLanguageCoreModFolderPath()
		{
			return GetLanguageCoreModFolderPath(LanguageDatabase.activeLanguage);
		}

		private static string GetEnglishLanguageCoreModFolderPath()
		{
			return GetLanguageCoreModFolderPath(LanguageDatabase.defaultLanguage);
		}

		private static string GetLanguageCoreModFolderPath(LoadedLanguage language)
		{
			ModContentPack modContentPack = LoadedModManager.RunningMods.FirstOrDefault((ModContentPack x) => x.IsCoreMod);
			string path = Path.Combine(modContentPack.RootDir, "Languages");
			return Path.Combine(path, language.folderName);
		}

		private static void SaveXMLDocumentWithProcessedNewlineTags(XNode doc, string path)
		{
			File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + doc.ToString().Replace("<!--NEWLINE-->", string.Empty).Replace("&gt;", ">"), Encoding.UTF8);
		}

		private static string ListToLiNodesString(IEnumerable<string> list)
		{
			if (list == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string item in list)
			{
				stringBuilder.Append("<li>");
				if (!item.NullOrEmpty())
				{
					stringBuilder.Append(item.Replace("\n", "\\n"));
				}
				stringBuilder.Append("</li>");
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		private static XElement ListToXElement(IEnumerable<string> list, string name, List<Pair<int, string>> comments)
		{
			XElement xElement = new XElement(name);
			if (list != null)
			{
				int num = 0;
				foreach (string item in list)
				{
					if (comments != null)
					{
						for (int i = 0; i < comments.Count; i++)
						{
							if (comments[i].First == num)
							{
								xElement.Add(new XComment(comments[i].Second));
							}
						}
					}
					XElement xElement2 = new XElement("li");
					if (!item.NullOrEmpty())
					{
						xElement2.Add(new XText(item.Replace("\n", "\\n")));
					}
					xElement.Add(xElement2);
					num++;
				}
				if (comments != null)
				{
					for (int j = 0; j < comments.Count; j++)
					{
						if (comments[j].First == num)
						{
							xElement.Add(new XComment(comments[j].Second));
						}
					}
				}
			}
			return xElement;
		}

		private static string AppendXmlExtensionIfNotAlready(string fileName)
		{
			if (!fileName.ToLower().EndsWith(".xml"))
			{
				return fileName + ".xml";
			}
			return fileName;
		}

		private static string GetSourceFile(Def def)
		{
			if (def.defPackage != null)
			{
				return AppendXmlExtensionIfNotAlready(def.defPackage.fileName);
			}
			return "Unknown.xml";
		}

		private static string TryRemoveLastIndexSymbol(string str)
		{
			int num = str.LastIndexOf('.');
			if (num >= 0 && str.Substring(num + 1).All(char.IsNumber))
			{
				return str.Substring(0, num);
			}
			return str;
		}

		private static IEnumerable<string> GetEnglishList(string normalizedPath, IEnumerable<string> curValue, Dictionary<string, DefInjectionPackage.DefInjection> injectionsByNormalizedPath)
		{
			if (injectionsByNormalizedPath.TryGetValue(normalizedPath, out DefInjectionPackage.DefInjection value) && value.injected)
			{
				return value.replacedList;
			}
			if (curValue == null)
			{
				return null;
			}
			List<string> list = curValue.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				string key = normalizedPath + "." + i;
				if (injectionsByNormalizedPath.TryGetValue(key, out DefInjectionPackage.DefInjection value2) && value2.injected)
				{
					list[i] = value2.replacedString;
				}
			}
			return list;
		}

		private static XElement GetDefInjectableFieldNode(string suggestedPath, DefInjectionPackage.DefInjection existingInjection)
		{
			if (existingInjection == null || existingInjection.isPlaceholder)
			{
				return new XElement(suggestedPath, new XText("TODO"));
			}
			if (existingInjection.IsFullListInjection)
			{
				return ListToXElement(existingInjection.fullListInjection, suggestedPath, existingInjection.fullListInjectionComments);
			}
			XElement xElement = new XElement(suggestedPath);
			if (!existingInjection.injection.NullOrEmpty())
			{
				xElement.Add(new XText(existingInjection.injection.Replace("\n", "\\n")));
			}
			return xElement;
		}
	}
}
