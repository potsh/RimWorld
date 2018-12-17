using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public static class Translator
	{
		public static bool CanTranslate(this string key)
		{
			return LanguageDatabase.activeLanguage.HaveTextForKey(key);
		}

		public static string TranslateWithBackup(this string key, string backupKey)
		{
			if (key.TryTranslate(out string result))
			{
				return result;
			}
			if (key.TryTranslate(out backupKey))
			{
				return result;
			}
			return key.Translate();
		}

		public static bool TryTranslate(this string key, out string result)
		{
			if (key.NullOrEmpty())
			{
				result = key;
				return false;
			}
			if (LanguageDatabase.activeLanguage == null)
			{
				Log.Error("No active language! Cannot translate from key " + key + ".");
				result = key;
				return true;
			}
			if (LanguageDatabase.activeLanguage.TryGetTextFromKey(key, out result))
			{
				return true;
			}
			result = key;
			return false;
		}

		public static string Translate(this string key)
		{
			if (key.TryTranslate(out string result))
			{
				return result;
			}
			LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out result);
			if (Prefs.DevMode)
			{
				return PseudoTranslated(result);
			}
			return result;
		}

		[Obsolete("Use TranslatorFormattedStringExtensions")]
		public static string Translate(this string key, params object[] args)
		{
			if (key.NullOrEmpty())
			{
				return key;
			}
			if (LanguageDatabase.activeLanguage != null)
			{
				if (!LanguageDatabase.activeLanguage.TryGetTextFromKey(key, out string translated))
				{
					LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out translated);
					if (Prefs.DevMode)
					{
						translated = PseudoTranslated(translated);
					}
				}
				string result = translated;
				try
				{
					result = string.Format(translated, args);
					return result;
				}
				catch (Exception ex)
				{
					Log.ErrorOnce("Exception translating '" + translated + "': " + ex, Gen.HashCombineInt(key.GetHashCode(), 394878901));
					return result;
				}
			}
			Log.Error("No active language! Cannot translate from key " + key + ".");
			return key;
		}

		public static bool TryGetTranslatedStringsForFile(string fileName, out List<string> stringList)
		{
			if (!LanguageDatabase.activeLanguage.TryGetStringsFromFile(fileName, out stringList) && !LanguageDatabase.defaultLanguage.TryGetStringsFromFile(fileName, out stringList))
			{
				Log.Error("No string files for " + fileName + ".");
				return false;
			}
			return true;
		}

		private static string PseudoTranslated(string original)
		{
			if (!Prefs.DevMode)
			{
				return original;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in original)
			{
				string text = null;
				switch (c)
				{
				case 'a':
					text = "à";
					break;
				case 'b':
					text = "þ";
					break;
				case 'c':
					text = "ç";
					break;
				case 'd':
					text = "ð";
					break;
				case 'e':
					text = "è";
					break;
				case 'f':
					text = "Ƒ";
					break;
				case 'g':
					text = "ğ";
					break;
				case 'h':
					text = "ĥ";
					break;
				case 'i':
					text = "ì";
					break;
				case 'j':
					text = "ĵ";
					break;
				case 'k':
					text = "к";
					break;
				case 'l':
					text = "ſ";
					break;
				case 'm':
					text = "ṁ";
					break;
				case 'n':
					text = "ƞ";
					break;
				case 'o':
					text = "ò";
					break;
				case 'p':
					text = "ṗ";
					break;
				case 'q':
					text = "q";
					break;
				case 'r':
					text = "ṟ";
					break;
				case 's':
					text = "ș";
					break;
				case 't':
					text = "ṭ";
					break;
				case 'u':
					text = "ù";
					break;
				case 'v':
					text = "ṽ";
					break;
				case 'w':
					text = "ẅ";
					break;
				case 'x':
					text = "ẋ";
					break;
				case 'y':
					text = "ý";
					break;
				case 'z':
					text = "ž";
					break;
				default:
					text = string.Empty + c;
					break;
				}
				stringBuilder.Append(text);
			}
			return stringBuilder.ToString();
		}
	}
}
