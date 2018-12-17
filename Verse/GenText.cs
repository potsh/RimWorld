using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse.Grammar;

namespace Verse
{
	public static class GenText
	{
		private const int SaveNameMaxLength = 40;

		private const char DegreeSymbol = '°';

		private static StringBuilder tmpSb = new StringBuilder();

		private static StringBuilder tmpSbForCapitalizedSentences = new StringBuilder();

		private static StringBuilder tmpStringBuilder = new StringBuilder();

		public static string Possessive(this Pawn p)
		{
			if (p.gender == Gender.Female)
			{
				return "Proher".Translate();
			}
			return "Prohis".Translate();
		}

		public static string PossessiveCap(this Pawn p)
		{
			if (p.gender == Gender.Female)
			{
				return "ProherCap".Translate();
			}
			return "ProhisCap".Translate();
		}

		public static string ProObj(this Pawn p)
		{
			if (p.gender == Gender.Female)
			{
				return "ProherObj".Translate();
			}
			return "ProhimObj".Translate();
		}

		public static string ProObjCap(this Pawn p)
		{
			if (p.gender == Gender.Female)
			{
				return "ProherObjCap".Translate();
			}
			return "ProhimObjCap".Translate();
		}

		public static string ProSubj(this Pawn p)
		{
			if (p.gender == Gender.Female)
			{
				return "Proshe".Translate();
			}
			return "Prohe".Translate();
		}

		public static string ProSubjCap(this Pawn p)
		{
			if (p.gender == Gender.Female)
			{
				return "ProsheCap".Translate();
			}
			return "ProheCap".Translate();
		}

		public static string AdjustedFor(this string text, Pawn p, string pawnSymbol = "PAWN")
		{
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(RulePackDefOf.DynamicWrapper);
			request.Rules.Add(new Rule_String("RULE", text));
			request.Rules.AddRange(GrammarUtility.RulesForPawn(pawnSymbol, p));
			return GrammarResolver.Resolve("r_root", request);
		}

		public static string AdjustedForKeys(this string text, List<string> outErrors = null, bool resolveKeys = true)
		{
			outErrors?.Clear();
			if (text.NullOrEmpty())
			{
				return text;
			}
			int num = 0;
			while (true)
			{
				num++;
				if (num > 500000)
				{
					Log.Error("Too many iterations.");
					outErrors?.Add("The parsed string caused an infinite loop");
					break;
				}
				int num2 = text.IndexOf("{Key:");
				if (num2 < 0)
				{
					break;
				}
				int num3 = num2;
				while (text[num3] != '}')
				{
					num3++;
					if (num3 >= text.Length)
					{
						outErrors?.Add("Mismatched braces");
						return text;
					}
				}
				string text2 = text.Substring(num2 + 5, num3 - (num2 + 5));
				KeyBindingDef namedSilentFail = DefDatabase<KeyBindingDef>.GetNamedSilentFail(text2);
				string str = text.Substring(0, num2);
				if (namedSilentFail != null)
				{
					str = ((!resolveKeys) ? (str + "placeholder") : (str + namedSilentFail.MainKeyLabel));
				}
				else
				{
					str += "error";
					if (outErrors != null)
					{
						string text3 = "Could not find key '" + text2 + "'";
						string text4 = BackCompatibility.BackCompatibleDefName(typeof(KeyBindingDef), text2);
						if (text4 != text2)
						{
							text3 = text3 + " (hint: it was renamed to '" + text4 + "')";
						}
						outErrors.Add(text3);
					}
				}
				str += text.Substring(num3 + 1);
				text = str;
			}
			return text;
		}

		public static string LabelIndefinite(this Pawn pawn)
		{
			if (pawn.Name != null && !pawn.Name.Numerical)
			{
				return pawn.LabelShort;
			}
			return pawn.KindLabelIndefinite();
		}

		public static string LabelDefinite(this Pawn pawn)
		{
			if (pawn.Name != null && !pawn.Name.Numerical)
			{
				return pawn.LabelShort;
			}
			return pawn.KindLabelDefinite();
		}

		public static string KindLabelIndefinite(this Pawn pawn)
		{
			return Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(pawn.KindLabel, pawn.gender);
		}

		public static string KindLabelDefinite(this Pawn pawn)
		{
			return Find.ActiveLanguageWorker.WithDefiniteArticlePostProcessed(pawn.KindLabel, pawn.gender);
		}

		public static string RandomSeedString()
		{
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(RulePackDefOf.SeedGenerator);
			return GrammarResolver.Resolve("r_seed", request).ToLower();
		}

		public static string Shorten(this string s)
		{
			if (s.NullOrEmpty() || s.Length <= 4)
			{
				return s;
			}
			s = s.Trim();
			string[] array = s.Split(' ');
			string text = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					text += " ";
				}
				if (array[i].Length > 2)
				{
					text = text + array[i].Substring(0, 1) + array[i].Substring(1, array[i].Length - 2).WithoutVowels() + array[i].Substring(array[i].Length - 1, 1);
				}
			}
			return text;
		}

		private static string WithoutVowels(this string s)
		{
			string vowels = "aeiouy";
			return new string((from c in s
			where !vowels.Contains(c)
			select c).ToArray());
		}

		public static string MarchingEllipsis(float offset = 0f)
		{
			switch (Mathf.FloorToInt(Time.realtimeSinceStartup + offset) % 3)
			{
			case 0:
				return ".";
			case 1:
				return "..";
			case 2:
				return "...";
			default:
				throw new Exception();
			}
		}

		public static void SetTextSizeToFit(string text, Rect r)
		{
			Text.Font = GameFont.Small;
			float num = Text.CalcHeight(text, r.width);
			if (num > r.height)
			{
				Text.Font = GameFont.Tiny;
			}
		}

		public static string TrimEndNewlines(this string s)
		{
			return s.TrimEnd('\r', '\n');
		}

		public static string Indented(this string s, string indentation = "    ")
		{
			if (s.NullOrEmpty())
			{
				return s;
			}
			return indentation + s.Replace("\r", string.Empty).Replace("\n", "\n" + indentation);
		}

		public static string ReplaceFirst(this string source, string key, string replacement)
		{
			int num = source.IndexOf(key);
			if (num < 0)
			{
				return source;
			}
			return source.Substring(0, num) + replacement + source.Substring(num + key.Length);
		}

		public static int StableStringHash(string str)
		{
			if (str == null)
			{
				return 0;
			}
			int num = 23;
			int length = str.Length;
			for (int i = 0; i < length; i++)
			{
				num = num * 31 + str[i];
			}
			return num;
		}

		public static string StringFromEnumerable(IEnumerable source)
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerator enumerator = source.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					stringBuilder.AppendLine("• " + current.ToString());
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
			return stringBuilder.ToString();
		}

		public static IEnumerable<string> LinesFromString(string text)
		{
			string[] lineSeparators = new string[2]
			{
				"\r\n",
				"\n"
			};
			string[] array = text.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);
			int num = 0;
			string doneStr2;
			while (true)
			{
				if (num >= array.Length)
				{
					yield break;
				}
				string str = array[num];
				doneStr2 = str.Trim();
				if (!doneStr2.StartsWith("//"))
				{
					doneStr2 = doneStr2.Split(new string[1]
					{
						"//"
					}, StringSplitOptions.None)[0];
					if (doneStr2.Length != 0)
					{
						break;
					}
				}
				num++;
			}
			yield return doneStr2;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static string GetInvalidFilenameCharacters()
		{
			return new string(Path.GetInvalidFileNameChars()) + "/\\{}<>:*|!@#$%^&*?";
		}

		public static bool IsValidFilename(string str)
		{
			if (str.Length > 40)
			{
				return false;
			}
			Regex regex = new Regex("[" + Regex.Escape(GetInvalidFilenameCharacters()) + "]");
			return !regex.IsMatch(str);
		}

		public static string SanitizeFilename(string str)
		{
			return string.Join("_", str.Split(GetInvalidFilenameCharacters().ToArray(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
		}

		public static bool NullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

		public static string SplitCamelCase(string Str)
		{
			return Regex.Replace(Str, "(?<a>(?<!^)((?:[A-Z][a-z])|(?:(?<!^[A-Z]+)[A-Z0-9]+(?:(?=[A-Z][a-z])|$))|(?:[0-9]+)))", " ${a}");
		}

		public static string CapitalizedNoSpaces(string s)
		{
			string[] array = s.Split(' ');
			StringBuilder stringBuilder = new StringBuilder();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.Length > 0)
				{
					stringBuilder.Append(char.ToUpper(text[0]));
				}
				if (text.Length > 1)
				{
					stringBuilder.Append(text.Substring(1));
				}
			}
			return stringBuilder.ToString();
		}

		public static string RemoveNonAlphanumeric(string s)
		{
			tmpSb.Length = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (char.IsLetterOrDigit(s[i]))
				{
					tmpSb.Append(s[i]);
				}
			}
			return tmpSb.ToString();
		}

		public static bool EqualsIgnoreCase(this string A, string B)
		{
			return string.Compare(A, B, ignoreCase: true) == 0;
		}

		public static string WithoutByteOrderMark(this string str)
		{
			return str.Trim().Trim('\ufeff');
		}

		public static bool NamesOverlap(string A, string B)
		{
			A = A.ToLower();
			B = B.ToLower();
			string[] array = A.Split(' ');
			string[] source = B.Split(' ');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (TitleCaseHelper.IsUppercaseTitleWord(text) && source.Contains(text))
				{
					return true;
				}
			}
			return false;
		}

		public static string CapitalizeFirst(this string str)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			if (char.IsUpper(str[0]))
			{
				return str;
			}
			if (str.Length == 1)
			{
				return str.ToUpper();
			}
			return char.ToUpper(str[0]) + str.Substring(1);
		}

		public static string UncapitalizeFirst(this string str)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			if (char.IsLower(str[0]))
			{
				return str;
			}
			if (str.Length == 1)
			{
				return str.ToLower();
			}
			return char.ToLower(str[0]) + str.Substring(1);
		}

		public static string ToNewsCase(string str)
		{
			string[] array = str.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (text.Length >= 2)
				{
					if (i == 0)
					{
						array[i] = text[0].ToString().ToUpper() + text.Substring(1);
					}
					else
					{
						array[i] = text.ToLower();
					}
				}
			}
			return string.Join(" ", array);
		}

		public static string ToTitleCaseSmart(string str)
		{
			string[] array = str.MergeMultipleSpaces(leaveMultipleSpacesAtLineBeginning: false).Trim().Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if ((i == 0 || i == array.Length - 1 || TitleCaseHelper.IsUppercaseTitleWord(text)) && !text.NullOrEmpty())
				{
					string str2 = text[0].ToString().ToUpper();
					string str3 = text.Substring(1);
					array[i] = str2 + str3;
				}
			}
			return string.Join(" ", array);
		}

		public static string CapitalizeSentences(string input, bool capitalizeFirstSentence = true)
		{
			if (input.NullOrEmpty())
			{
				return input;
			}
			if (input.Length == 1)
			{
				return input.ToUpper();
			}
			bool flag = capitalizeFirstSentence;
			tmpSbForCapitalizedSentences.Length = 0;
			for (int i = 0; i < input.Length; i++)
			{
				if (flag && char.IsLetterOrDigit(input[i]))
				{
					tmpSbForCapitalizedSentences.Append(char.ToUpper(input[i]));
					flag = false;
				}
				else
				{
					tmpSbForCapitalizedSentences.Append(input[i]);
				}
				if (input[i] == '\r' || input[i] == '\n' || input[i] == '.' || input[i] == '!' || input[i] == '?' || input[i] == ':')
				{
					flag = true;
				}
			}
			return tmpSbForCapitalizedSentences.ToString();
		}

		public static string CapitalizeAsTitle(string str)
		{
			return Find.ActiveLanguageWorker.ToTitleCase(str);
		}

		public static string ToCommaList(this IEnumerable<string> items, bool useAnd = false)
		{
			if (items != null)
			{
				string text = null;
				string text2 = null;
				int num = 0;
				StringBuilder stringBuilder = new StringBuilder();
				IList<string> list = items as IList<string>;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						string text3 = list[i];
						if (!text3.NullOrEmpty())
						{
							if (text2 == null)
							{
								text2 = text3;
							}
							if (text != null)
							{
								stringBuilder.Append(text + ", ");
							}
							text = text3;
							num++;
						}
					}
				}
				else
				{
					foreach (string item in items)
					{
						if (!item.NullOrEmpty())
						{
							if (text2 == null)
							{
								text2 = item;
							}
							if (text != null)
							{
								stringBuilder.Append(text + ", ");
							}
							text = item;
							num++;
						}
					}
				}
				switch (num)
				{
				case 0:
					return "NoneLower".Translate();
				case 1:
					return text;
				default:
					if (useAnd)
					{
						if (num == 2)
						{
							return text2 + " " + "AndLower".Translate() + " " + text;
						}
						stringBuilder.Append("AndLower".Translate() + " " + text);
						return stringBuilder.ToString();
					}
					stringBuilder.Append(text);
					return stringBuilder.ToString();
				}
			}
			return string.Empty;
		}

		public static string ToLineList(this IEnumerable<string> entries, string prefix = null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (string entry in entries)
			{
				if (!flag)
				{
					stringBuilder.Append("\n");
				}
				if (prefix != null)
				{
					stringBuilder.Append(prefix);
				}
				stringBuilder.Append(entry);
				flag = false;
			}
			return stringBuilder.ToString();
		}

		public static string ToSpaceList(IEnumerable<string> entries)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (string entry in entries)
			{
				if (!flag)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(entry);
				flag = false;
			}
			return stringBuilder.ToString();
		}

		public static string ToCamelCase(string str)
		{
			str = ToTitleCaseSmart(str);
			return str.Replace(" ", null);
		}

		public static string Truncate(this string str, float width, Dictionary<string, string> cache = null)
		{
			if (cache != null && cache.TryGetValue(str, out string value))
			{
				return value;
			}
			Vector2 vector = Text.CalcSize(str);
			if (vector.x <= width)
			{
				cache?.Add(str, str);
				return str;
			}
			value = str;
			Vector2 vector2;
			do
			{
				value = value.Substring(0, value.Length - 1);
				if (value.Length <= 0)
				{
					break;
				}
				vector2 = Text.CalcSize(value + "...");
			}
			while (vector2.x > width);
			value += "...";
			cache?.Add(str, value);
			return value;
		}

		public static string Flatten(this string str)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			if (str.Contains("\n"))
			{
				str = str.Replace("\n", " ");
			}
			if (str.Contains("\r"))
			{
				str = str.Replace("\r", string.Empty);
			}
			str = str.MergeMultipleSpaces(leaveMultipleSpacesAtLineBeginning: false);
			return str.Trim(' ', '\n', '\r', '\t');
		}

		public static string MergeMultipleSpaces(this string str, bool leaveMultipleSpacesAtLineBeginning = true)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			if (!str.Contains("  "))
			{
				return str;
			}
			bool flag = true;
			tmpStringBuilder.Length = 0;
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '\r' || str[i] == '\n')
				{
					flag = true;
				}
				if ((leaveMultipleSpacesAtLineBeginning && flag) || str[i] != ' ' || i == 0 || str[i - 1] != ' ')
				{
					tmpStringBuilder.Append(str[i]);
				}
				if (!char.IsWhiteSpace(str[i]))
				{
					flag = false;
				}
			}
			return tmpStringBuilder.ToString();
		}

		public static string TrimmedToLength(this string str, int length)
		{
			if (str == null || str.Length <= length)
			{
				return str;
			}
			return str.Substring(0, length);
		}

		public static bool ContainsEmptyLines(string str)
		{
			if (str.NullOrEmpty())
			{
				return true;
			}
			if (str[0] == '\n' || str[0] == '\r')
			{
				return true;
			}
			if (str[str.Length - 1] == '\n' || str[str.Length - 1] == '\r')
			{
				return true;
			}
			if (str.Contains("\n\n") || str.Contains("\r\n\r\n") || str.Contains("\r\r"))
			{
				return true;
			}
			return false;
		}

		public static string ToStringByStyle(this float f, ToStringStyle style, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
		{
			if (style == ToStringStyle.Temperature && numberSense == ToStringNumberSense.Offset)
			{
				style = ToStringStyle.TemperatureOffset;
			}
			if (numberSense == ToStringNumberSense.Factor)
			{
				style = ((!(f >= 10f)) ? ToStringStyle.PercentZero : ToStringStyle.FloatMaxTwo);
			}
			string text;
			switch (style)
			{
			case ToStringStyle.Integer:
				text = Mathf.RoundToInt(f).ToString();
				break;
			case ToStringStyle.FloatOne:
				text = f.ToString("F1");
				break;
			case ToStringStyle.FloatTwo:
				text = f.ToString("F2");
				break;
			case ToStringStyle.FloatThree:
				text = f.ToString("F3");
				break;
			case ToStringStyle.FloatMaxOne:
				text = f.ToString("0.#");
				break;
			case ToStringStyle.FloatMaxTwo:
				text = f.ToString("0.##");
				break;
			case ToStringStyle.FloatMaxThree:
				text = f.ToString("0.###");
				break;
			case ToStringStyle.FloatTwoOrThree:
				text = f.ToString((f != 0f && !(Mathf.Abs(f) >= 0.01f)) ? "F3" : "F2");
				break;
			case ToStringStyle.PercentZero:
				text = f.ToStringPercent();
				break;
			case ToStringStyle.PercentOne:
				text = f.ToStringPercent("F1");
				break;
			case ToStringStyle.PercentTwo:
				text = f.ToStringPercent("F2");
				break;
			case ToStringStyle.Temperature:
				text = f.ToStringTemperature();
				break;
			case ToStringStyle.TemperatureOffset:
				text = f.ToStringTemperatureOffset();
				break;
			case ToStringStyle.WorkAmount:
				text = f.ToStringWorkAmount();
				break;
			default:
				Log.Error("Unknown ToStringStyle " + style);
				text = f.ToString();
				break;
			}
			switch (numberSense)
			{
			case ToStringNumberSense.Offset:
				if (f >= 0f)
				{
					text = "+" + text;
				}
				break;
			case ToStringNumberSense.Factor:
				text = "x" + text;
				break;
			}
			return text;
		}

		public static string ToStringDecimalIfSmall(this float f)
		{
			if (Mathf.Abs(f) < 1f)
			{
				return Math.Round((double)f, 2).ToString("0.##");
			}
			if (Mathf.Abs(f) < 10f)
			{
				return Math.Round((double)f, 1).ToString("0.#");
			}
			return Mathf.RoundToInt(f).ToStringCached();
		}

		public static string ToStringPercent(this float f)
		{
			return (f * 100f).ToStringDecimalIfSmall() + "%";
		}

		public static string ToStringPercent(this float f, string format)
		{
			return ((f + 1E-05f) * 100f).ToString(format) + "%";
		}

		public static string ToStringMoney(this float f, string format = null)
		{
			if (format == null)
			{
				format = ((!(f > 100f)) ? "F2" : "F0");
			}
			return "MoneyFormat".Translate(f.ToString(format));
		}

		public static string ToStringMoneyOffset(this float f, string format = null)
		{
			string text = f.ToStringMoney(format);
			if (f > 0f && text != "$0")
			{
				return "+" + text;
			}
			return text;
		}

		public static string ToStringWithSign(this int i)
		{
			return i.ToString("+#;-#;0");
		}

		public static string ToStringWithSign(this float f, string format = "0.##")
		{
			if (f > 0f)
			{
				return "+" + f.ToString(format);
			}
			return f.ToString(format);
		}

		public static string ToStringKilobytes(this int bytes, string format = "F2")
		{
			return ((float)bytes / 1024f).ToString(format) + "Kb";
		}

		public static string ToStringYesNo(this bool b)
		{
			return (!b) ? "No".Translate() : "Yes".Translate();
		}

		public static string ToStringLongitude(this float longitude)
		{
			bool flag = longitude < 0f;
			if (flag)
			{
				longitude = 0f - longitude;
			}
			return longitude.ToString("F2") + '°' + ((!flag) ? "E" : "W");
		}

		public static string ToStringLatitude(this float latitude)
		{
			bool flag = latitude < 0f;
			if (flag)
			{
				latitude = 0f - latitude;
			}
			return latitude.ToString("F2") + '°' + ((!flag) ? "N" : "S");
		}

		public static string ToStringMass(this float mass)
		{
			if (mass == 0f)
			{
				return "0 g";
			}
			float num = Mathf.Abs(mass);
			if (num >= 100f)
			{
				return mass.ToString("F0") + " kg";
			}
			if (num >= 10f)
			{
				return mass.ToString("0.#") + " kg";
			}
			if (num >= 0.1f)
			{
				return mass.ToString("0.##") + " kg";
			}
			float num2 = mass * 1000f;
			if (num >= 0.01f)
			{
				return num2.ToString("F0") + " g";
			}
			if (num >= 0.001f)
			{
				return num2.ToString("0.#") + " g";
			}
			return num2.ToString("0.##") + " g";
		}

		public static string ToStringMassOffset(this float mass)
		{
			string text = mass.ToStringMass();
			if (mass > 0f)
			{
				return "+" + text;
			}
			return text;
		}

		public static string ToStringSign(this float val)
		{
			if (val >= 0f)
			{
				return "+";
			}
			return string.Empty;
		}

		public static string ToStringEnsureThreshold(this float value, float threshold, int decimalPlaces)
		{
			if (value > threshold && Math.Round((double)value, decimalPlaces) <= Math.Round((double)threshold, decimalPlaces))
			{
				return (value + 1f / Mathf.Pow(10f, (float)decimalPlaces)).ToString("F" + decimalPlaces);
			}
			return value.ToString("F" + decimalPlaces);
		}

		public static string ToStringTemperature(this float celsiusTemp, string format = "F1")
		{
			celsiusTemp = GenTemperature.CelsiusTo(celsiusTemp, Prefs.TemperatureMode);
			return celsiusTemp.ToStringTemperatureRaw(format);
		}

		public static string ToStringTemperatureOffset(this float celsiusTemp, string format = "F1")
		{
			celsiusTemp = GenTemperature.CelsiusToOffset(celsiusTemp, Prefs.TemperatureMode);
			return celsiusTemp.ToStringTemperatureRaw(format);
		}

		public static string ToStringTemperatureRaw(this float temp, string format = "F1")
		{
			switch (Prefs.TemperatureMode)
			{
			case TemperatureDisplayMode.Celsius:
				return temp.ToString(format) + "C";
			case TemperatureDisplayMode.Fahrenheit:
				return temp.ToString(format) + "F";
			case TemperatureDisplayMode.Kelvin:
				return temp.ToString(format) + "K";
			default:
				throw new InvalidOperationException();
			}
		}

		public static string ToStringTwoDigits(this Vector2 v)
		{
			return "(" + v.x.ToString("F2") + ", " + v.y.ToString("F2") + ")";
		}

		public static string ToStringWorkAmount(this float workAmount)
		{
			return Mathf.CeilToInt(workAmount / 60f).ToString();
		}

		public static string ToStringBytes(this int b, string format = "F2")
		{
			return ((float)b / 8f / 1024f).ToString(format) + "kb";
		}

		public static string ToStringBytes(this uint b, string format = "F2")
		{
			return ((float)(double)b / 8f / 1024f).ToString(format) + "kb";
		}

		public static string ToStringBytes(this long b, string format = "F2")
		{
			return ((float)b / 8f / 1024f).ToString(format) + "kb";
		}

		public static string ToStringBytes(this ulong b, string format = "F2")
		{
			return ((float)(double)b / 8f / 1024f).ToString(format) + "kb";
		}

		public static string ToStringReadable(this KeyCode k)
		{
			switch (k)
			{
			case KeyCode.Keypad0:
				return "Kp0";
			case KeyCode.Keypad1:
				return "Kp1";
			case KeyCode.Keypad2:
				return "Kp2";
			case KeyCode.Keypad3:
				return "Kp3";
			case KeyCode.Keypad4:
				return "Kp4";
			case KeyCode.Keypad5:
				return "Kp5";
			case KeyCode.Keypad6:
				return "Kp6";
			case KeyCode.Keypad7:
				return "Kp7";
			case KeyCode.Keypad8:
				return "Kp8";
			case KeyCode.Keypad9:
				return "Kp9";
			case KeyCode.KeypadDivide:
				return "Kp/";
			case KeyCode.KeypadEnter:
				return "KpEnt";
			case KeyCode.KeypadEquals:
				return "Kp=";
			case KeyCode.KeypadMinus:
				return "Kp-";
			case KeyCode.KeypadMultiply:
				return "Kp*";
			case KeyCode.KeypadPeriod:
				return "Kp.";
			case KeyCode.KeypadPlus:
				return "Kp+";
			case KeyCode.Alpha0:
				return "0";
			case KeyCode.Alpha1:
				return "1";
			case KeyCode.Alpha2:
				return "2";
			case KeyCode.Alpha3:
				return "3";
			case KeyCode.Alpha4:
				return "4";
			case KeyCode.Alpha5:
				return "5";
			case KeyCode.Alpha6:
				return "6";
			case KeyCode.Alpha7:
				return "7";
			case KeyCode.Alpha8:
				return "8";
			case KeyCode.Alpha9:
				return "9";
			case KeyCode.Clear:
				return "Clr";
			case KeyCode.Backspace:
				return "Bksp";
			case KeyCode.Return:
				return "Ent";
			case KeyCode.Escape:
				return "Esc";
			case KeyCode.DoubleQuote:
				return "\"";
			case KeyCode.Exclaim:
				return "!";
			case KeyCode.Hash:
				return "#";
			case KeyCode.Dollar:
				return "$";
			case KeyCode.Ampersand:
				return "&";
			case KeyCode.Quote:
				return "'";
			case KeyCode.LeftParen:
				return "(";
			case KeyCode.RightParen:
				return ")";
			case KeyCode.Asterisk:
				return "*";
			case KeyCode.Plus:
				return "+";
			case KeyCode.Minus:
				return "-";
			case KeyCode.Comma:
				return ",";
			case KeyCode.Period:
				return ".";
			case KeyCode.Slash:
				return "/";
			case KeyCode.Colon:
				return ":";
			case KeyCode.Semicolon:
				return ";";
			case KeyCode.Less:
				return "<";
			case KeyCode.Greater:
				return ">";
			case KeyCode.Question:
				return "?";
			case KeyCode.At:
				return "@";
			case KeyCode.LeftBracket:
				return "[";
			case KeyCode.RightBracket:
				return "]";
			case KeyCode.Backslash:
				return "\\";
			case KeyCode.Caret:
				return "^";
			case KeyCode.Underscore:
				return "_";
			case KeyCode.BackQuote:
				return "`";
			case KeyCode.Delete:
				return "Del";
			case KeyCode.UpArrow:
				return "Up";
			case KeyCode.DownArrow:
				return "Down";
			case KeyCode.LeftArrow:
				return "Left";
			case KeyCode.RightArrow:
				return "Right";
			case KeyCode.Insert:
				return "Ins";
			case KeyCode.Home:
				return "Home";
			case KeyCode.End:
				return "End";
			case KeyCode.PageDown:
				return "PgDn";
			case KeyCode.PageUp:
				return "PgUp";
			case KeyCode.Numlock:
				return "NumL";
			case KeyCode.CapsLock:
				return "CapL";
			case KeyCode.ScrollLock:
				return "ScrL";
			case KeyCode.RightShift:
				return "RShf";
			case KeyCode.LeftShift:
				return "LShf";
			case KeyCode.RightControl:
				return "RCtrl";
			case KeyCode.LeftControl:
				return "LCtrl";
			case KeyCode.RightAlt:
				return "RAlt";
			case KeyCode.LeftAlt:
				return "LAlt";
			case KeyCode.RightCommand:
				return "Appl";
			case KeyCode.LeftCommand:
				return "Cmd";
			case KeyCode.LeftWindows:
				return "Win";
			case KeyCode.RightWindows:
				return "Win";
			case KeyCode.AltGr:
				return "AltGr";
			case KeyCode.Help:
				return "Help";
			case KeyCode.Print:
				return "Prnt";
			case KeyCode.SysReq:
				return "SysReq";
			case KeyCode.Break:
				return "Brk";
			case KeyCode.Menu:
				return "Menu";
			default:
				return k.ToString();
			}
		}

		public static void AppendWithComma(this StringBuilder sb, string text)
		{
			sb.AppendWithSeparator(text, ", ");
		}

		public static void AppendWithSeparator(this StringBuilder sb, string text, string separator)
		{
			if (!text.NullOrEmpty())
			{
				if (sb.Length > 0)
				{
					sb.Append(separator);
				}
				sb.Append(text);
			}
		}

		public static string WordWrapAt(this string text, float length)
		{
			Text.Font = GameFont.Medium;
			if (text.GetWidthCached() < length)
			{
				return text;
			}
			IEnumerable<Pair<char, int>> source = from p in text.Select((char c, int idx) => new Pair<char, int>(c, idx))
			where p.First == ' '
			select p;
			if (!source.Any())
			{
				return text;
			}
			Pair<char, int> pair = source.MinBy((Pair<char, int> p) => Mathf.Abs(text.Substring(0, p.Second).GetWidthCached() - text.Substring(p.Second + 1).GetWidthCached()));
			return text.Substring(0, pair.Second) + "\n" + text.Substring(pair.Second + 1);
		}
	}
}
