using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Verse
{
	public static class BackstoryTranslationUtility
	{
		public const string BackstoriesFolder = "Backstories";

		public const string BackstoriesFileName = "Backstories.xml";

		private static IEnumerable<XElement> BackstoryTranslationElements(IEnumerable<string> folderPaths, List<string> loadErrors)
		{
			foreach (string folderPath in folderPaths)
			{
				string localFolderPath = folderPath;
				FileInfo fi = new FileInfo(Path.Combine(localFolderPath.ToString(), "Backstories/Backstories.xml"));
				if (!fi.Exists)
				{
					break;
				}
				XDocument doc;
				try
				{
					doc = XDocument.Load(fi.FullName);
				}
				catch (Exception ex)
				{
					loadErrors?.Add("Exception loading backstory translation data from file " + fi + ": " + ex);
					yield break;
				}
				using (IEnumerator<XElement> enumerator2 = doc.Root.Elements().GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						XElement element = enumerator2.Current;
						yield return element;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01c4:
			/*Error near IL_01c5: Unexpected return in MoveNext()*/;
		}

		public static void LoadAndInjectBackstoryData(IEnumerable<string> folderPaths, List<string> loadErrors)
		{
			foreach (XElement item in BackstoryTranslationElements(folderPaths, loadErrors))
			{
				string text = "[unknown]";
				try
				{
					text = item.Name.ToString();
					string text2 = GetText(item, "title");
					string text3 = GetText(item, "titleFemale");
					string text4 = GetText(item, "titleShort");
					string text5 = GetText(item, "titleShortFemale");
					string text6 = GetText(item, "desc");
					if (!BackstoryDatabase.TryGetWithIdentifier(text, out Backstory bs, closestMatchWarning: false))
					{
						throw new Exception("Backstory not found matching identifier " + text);
					}
					if (text2 == bs.title && text3 == bs.titleFemale && text4 == bs.titleShort && text5 == bs.titleShortFemale && text6 == bs.baseDesc)
					{
						throw new Exception("Backstory translation exactly matches default data: " + text);
					}
					if (text2 != null)
					{
						bs.SetTitle(text2, bs.titleFemale);
						bs.titleTranslated = true;
					}
					if (text3 != null)
					{
						bs.SetTitle(bs.title, text3);
						bs.titleFemaleTranslated = true;
					}
					if (text4 != null)
					{
						bs.SetTitleShort(text4, bs.titleShortFemale);
						bs.titleShortTranslated = true;
					}
					if (text5 != null)
					{
						bs.SetTitleShort(bs.titleShort, text5);
						bs.titleShortFemaleTranslated = true;
					}
					if (text6 != null)
					{
						bs.baseDesc = text6;
						bs.descTranslated = true;
					}
				}
				catch (Exception ex)
				{
					loadErrors.Add("Couldn't load backstory " + text + ": " + ex.Message + "\nFull XML text:\n\n" + item.ToString());
				}
			}
		}

		public static List<string> MissingBackstoryTranslations(LoadedLanguage lang)
		{
			List<KeyValuePair<string, Backstory>> list = BackstoryDatabase.allBackstories.ToList();
			List<string> list2 = new List<string>();
			foreach (XElement item in BackstoryTranslationElements(lang.FolderPaths, null))
			{
				try
				{
					string text = item.Name.ToString();
					string modifiedIdentifier = BackstoryDatabase.GetIdentifierClosestMatch(text, closestMatchWarning: false);
					bool flag = list.Any((KeyValuePair<string, Backstory> x) => x.Key == modifiedIdentifier);
					KeyValuePair<string, Backstory> backstory = list.Find((KeyValuePair<string, Backstory> x) => x.Key == modifiedIdentifier);
					if (flag)
					{
						list.RemoveAt(list.FindIndex((KeyValuePair<string, Backstory> x) => x.Key == backstory.Key));
						string text2 = GetText(item, "title");
						string text3 = GetText(item, "titleFemale");
						string text4 = GetText(item, "titleShort");
						string text5 = GetText(item, "titleShortFemale");
						string text6 = GetText(item, "desc");
						if (text2.NullOrEmpty())
						{
							list2.Add(text + ".title missing");
						}
						if (flag && !backstory.Value.titleFemale.NullOrEmpty() && text3.NullOrEmpty())
						{
							list2.Add(text + ".titleFemale missing");
						}
						if (text4.NullOrEmpty())
						{
							list2.Add(text + ".titleShort missing");
						}
						if (flag && !backstory.Value.titleShortFemale.NullOrEmpty() && text5.NullOrEmpty())
						{
							list2.Add(text + ".titleShortFemale missing");
						}
						if (text6.NullOrEmpty())
						{
							list2.Add(text + ".desc missing");
						}
					}
					else
					{
						list2.Add("Translation doesn't correspond to any backstory: " + text);
					}
				}
				catch (Exception ex)
				{
					list2.Add("Exception reading " + item.Name + ": " + ex.Message);
				}
			}
			foreach (KeyValuePair<string, Backstory> item2 in list)
			{
				list2.Add("Missing backstory: " + item2.Key);
			}
			return list2;
		}

		public static List<string> BackstoryTranslationsMatchingEnglish(LoadedLanguage lang)
		{
			List<string> list = new List<string>();
			foreach (XElement item in BackstoryTranslationElements(lang.FolderPaths, null))
			{
				try
				{
					string text = item.Name.ToString();
					if (BackstoryDatabase.allBackstories.TryGetValue(BackstoryDatabase.GetIdentifierClosestMatch(text), out Backstory value))
					{
						string text2 = GetText(item, "title");
						string text3 = GetText(item, "titleFemale");
						string text4 = GetText(item, "titleShort");
						string text5 = GetText(item, "titleShortFemale");
						string text6 = GetText(item, "desc");
						if (!text2.NullOrEmpty() && text2 == value.untranslatedTitle)
						{
							list.Add(text + ".title '" + text2.Replace("\n", "\\n") + "'");
						}
						if (!text3.NullOrEmpty() && text3 == value.untranslatedTitleFemale)
						{
							list.Add(text + ".titleFemale '" + text3.Replace("\n", "\\n") + "'");
						}
						if (!text4.NullOrEmpty() && text4 == value.untranslatedTitleShort)
						{
							list.Add(text + ".titleShort '" + text4.Replace("\n", "\\n") + "'");
						}
						if (!text5.NullOrEmpty() && text5 == value.untranslatedTitleShortFemale)
						{
							list.Add(text + ".titleShortFemale '" + text5.Replace("\n", "\\n") + "'");
						}
						if (!text6.NullOrEmpty() && text6 == value.untranslatedDesc)
						{
							list.Add(text + ".desc '" + text6.Replace("\n", "\\n") + "'");
						}
					}
				}
				catch (Exception ex)
				{
					list.Add("Exception reading " + item.Name + ": " + ex.Message);
				}
			}
			return list;
		}

		private static string GetText(XElement backstory, string fieldName)
		{
			XElement xElement = backstory.Element(fieldName);
			if (xElement == null || xElement.Value == "TODO")
			{
				return null;
			}
			return xElement.Value.Replace("\\n", "\n");
		}
	}
}
