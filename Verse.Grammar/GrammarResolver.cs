using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Verse.Grammar
{
	public static class GrammarResolver
	{
		private class RuleEntry
		{
			public Rule rule;

			public bool knownUnresolvable;

			public bool constantConstraintsChecked;

			public bool constantConstraintsValid;

			public int uses;

			public float SelectionWeight => rule.BaseSelectionWeight * 100000f / (float)((uses + 1) * 1000);

			public RuleEntry(Rule rule)
			{
				this.rule = rule;
				knownUnresolvable = false;
			}

			public void MarkKnownUnresolvable()
			{
				knownUnresolvable = true;
			}

			public bool ValidateConstantConstraints(Dictionary<string, string> constraints)
			{
				if (!constantConstraintsChecked)
				{
					constantConstraintsValid = true;
					if (rule.constantConstraints != null)
					{
						for (int i = 0; i < rule.constantConstraints.Count; i++)
						{
							Rule.ConstantConstraint constantConstraint = rule.constantConstraints[i];
							string a = (constraints == null) ? string.Empty : constraints.TryGetValue(constantConstraint.key, string.Empty);
							if (a == constantConstraint.value != constantConstraint.equality)
							{
								constantConstraintsValid = false;
								break;
							}
						}
					}
					constantConstraintsChecked = true;
				}
				return constantConstraintsValid;
			}

			public override string ToString()
			{
				return rule.ToString();
			}
		}

		private static SimpleLinearPool<List<RuleEntry>> rulePool = new SimpleLinearPool<List<RuleEntry>>();

		private static Dictionary<string, List<RuleEntry>> rules = new Dictionary<string, List<RuleEntry>>();

		private static int loopCount;

		private static StringBuilder logSb;

		private const int DepthLimit = 50;

		private const int LoopsLimit = 1000;

		private static Regex Spaces = new Regex(" +([,.])");

		private static readonly char[] SpecialChars = new char[4]
		{
			'[',
			']',
			'{',
			'}'
		};

		public static string Resolve(string rootKeyword, GrammarRequest request, string debugLabel = null, bool forceLog = false, string untranslatedRootKeyword = null)
		{
			if (LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage)
			{
				return ResolveUnsafe(rootKeyword, request, debugLabel, forceLog);
			}
			string text;
			bool success;
			Exception ex;
			try
			{
				text = ResolveUnsafe(rootKeyword, request, out success, debugLabel, forceLog);
				ex = null;
			}
			catch (Exception ex2)
			{
				success = false;
				text = string.Empty;
				ex = ex2;
			}
			if (success)
			{
				return text;
			}
			string text2 = "Failed to resolve text. Trying again with English.";
			if (ex != null)
			{
				text2 = text2 + " Exception: " + ex;
			}
			Log.ErrorOnce(text2, text.GetHashCode());
			string rootKeyword2 = untranslatedRootKeyword ?? rootKeyword;
			return ResolveUnsafe(rootKeyword2, request, out success, debugLabel, forceLog, useUntranslatedRules: true);
		}

		public static string ResolveUnsafe(string rootKeyword, GrammarRequest request, string debugLabel = null, bool forceLog = false, bool useUntranslatedRules = false)
		{
			bool success;
			return ResolveUnsafe(rootKeyword, request, out success, debugLabel, forceLog, useUntranslatedRules);
		}

		public static string ResolveUnsafe(string rootKeyword, GrammarRequest request, out bool success, string debugLabel = null, bool forceLog = false, bool useUntranslatedRules = false)
		{
			bool flag = forceLog || DebugViewSettings.logGrammarResolution;
			rules.Clear();
			rulePool.Clear();
			if (flag)
			{
				logSb = new StringBuilder();
			}
			List<Rule> list = request.GetRules();
			if (list != null)
			{
				if (flag)
				{
					logSb.AppendLine("Custom rules:");
				}
				for (int i = 0; i < list.Count; i++)
				{
					AddRule(list[i]);
					if (flag)
					{
						logSb.AppendLine("  " + list[i].ToString());
					}
				}
				if (flag)
				{
					logSb.AppendLine();
				}
			}
			List<RulePackDef> includes = request.GetIncludes();
			if (includes != null)
			{
				HashSet<RulePackDef> hashSet = new HashSet<RulePackDef>();
				List<RulePackDef> list2 = new List<RulePackDef>(includes);
				if (flag)
				{
					logSb.AppendLine("Includes:");
				}
				while (list2.Count > 0)
				{
					RulePackDef rulePackDef = list2[list2.Count - 1];
					list2.RemoveLast();
					if (!hashSet.Contains(rulePackDef))
					{
						if (flag)
						{
							logSb.AppendLine($"  {rulePackDef.defName}");
						}
						hashSet.Add(rulePackDef);
						List<Rule> list3 = (!useUntranslatedRules) ? rulePackDef.RulesImmediate : rulePackDef.UntranslatedRulesImmediate;
						if (list3 != null)
						{
							foreach (Rule item in list3)
							{
								AddRule(item);
							}
						}
						if (!rulePackDef.include.NullOrEmpty())
						{
							list2.AddRange(rulePackDef.include);
						}
					}
				}
				if (flag)
				{
					logSb.AppendLine();
				}
			}
			List<RulePack> includesBare = request.GetIncludesBare();
			if (includesBare != null)
			{
				if (flag)
				{
					logSb.AppendLine("Bare includes:");
				}
				for (int j = 0; j < includesBare.Count; j++)
				{
					List<Rule> list4 = (!useUntranslatedRules) ? includesBare[j].Rules : includesBare[j].UntranslatedRules;
					for (int k = 0; k < list4.Count; k++)
					{
						AddRule(list4[k]);
						if (flag)
						{
							logSb.AppendLine("  " + list4[k].ToString());
						}
					}
				}
				if (flag)
				{
					logSb.AppendLine();
				}
			}
			List<Rule> list5 = (!useUntranslatedRules) ? RulePackDefOf.GlobalUtility.RulesPlusIncludes : RulePackDefOf.GlobalUtility.UntranslatedRulesPlusIncludes;
			for (int l = 0; l < list5.Count; l++)
			{
				AddRule(list5[l]);
			}
			loopCount = 0;
			Dictionary<string, string> constants = request.GetConstants();
			if (flag && constants != null)
			{
				logSb.AppendLine("Constants:");
				foreach (KeyValuePair<string, string> item2 in constants)
				{
					logSb.AppendLine($"  {item2.Key}: {item2.Value}");
				}
			}
			string output = "err";
			bool flag2 = false;
			if (!TryResolveRecursive(new RuleEntry(new Rule_String(string.Empty, "[" + rootKeyword + "]")), 0, constants, out output, flag))
			{
				flag2 = true;
				output = "Could not resolve any root: " + rootKeyword;
				if (!debugLabel.NullOrEmpty())
				{
					output = output + " debugLabel: " + debugLabel;
				}
				else if (!request.Includes.NullOrEmpty())
				{
					output = output + " firstRulePack: " + request.Includes[0].defName;
				}
				if (flag)
				{
					logSb.Insert(0, "GrammarResolver failed to resolve a text (rootKeyword: " + rootKeyword + ")\n");
				}
				else
				{
					ResolveUnsafe(rootKeyword, request, debugLabel, forceLog: true);
				}
			}
			output = GenText.CapitalizeSentences(Find.ActiveLanguageWorker.PostProcessed(output));
			output = Spaces.Replace(output, (Match match) => match.Groups[1].Value);
			output = output.Trim();
			if (flag && flag2)
			{
				if (DebugViewSettings.logGrammarResolution)
				{
					Log.Error(logSb.ToString().Trim());
				}
				else
				{
					Log.ErrorOnce(logSb.ToString().Trim(), logSb.ToString().Trim().GetHashCode());
				}
			}
			else if (flag)
			{
				Log.Message(logSb.ToString().Trim());
			}
			success = !flag2;
			return output;
		}

		private static void AddRule(Rule rule)
		{
			List<RuleEntry> value = null;
			if (!rules.TryGetValue(rule.keyword, out value))
			{
				value = rulePool.Get();
				value.Clear();
				rules[rule.keyword] = value;
			}
			value.Add(new RuleEntry(rule));
		}

		private static bool TryResolveRecursive(RuleEntry entry, int depth, Dictionary<string, string> constants, out string output, bool log)
		{
			if (log)
			{
				logSb.AppendLine();
				logSb.Append(depth.ToStringCached() + " ");
				for (int i = 0; i < depth; i++)
				{
					logSb.Append("   ");
				}
				logSb.Append(entry + " ");
			}
			loopCount++;
			if (loopCount > 1000)
			{
				Log.Error("Hit loops limit resolving grammar.");
				output = "HIT_LOOPS_LIMIT";
				if (log)
				{
					logSb.Append("UNRESOLVABLE: Hit loops limit");
				}
				return false;
			}
			if (depth > 50)
			{
				Log.Error("Grammar recurred too deep while resolving keyword (>" + 50 + " deep)");
				output = "DEPTH_LIMIT_REACHED";
				if (log)
				{
					logSb.Append("UNRESOLVABLE: Depth limit reached");
				}
				return false;
			}
			string text = entry.rule.Generate();
			bool flag = false;
			int num = -1;
			for (int j = 0; j < text.Length; j++)
			{
				char c = text[j];
				if (c == '[')
				{
					num = j;
				}
				if (c == ']')
				{
					if (num == -1)
					{
						Log.Error("Could not resolve rule " + text + ": mismatched brackets.");
						output = "MISMATCHED_BRACKETS";
						if (log)
						{
							logSb.Append("UNRESOLVABLE: Mismatched brackets");
						}
						flag = true;
					}
					else
					{
						string text2 = text.Substring(num + 1, j - num - 1);
						while (true)
						{
							RuleEntry ruleEntry = RandomPossiblyResolvableEntry(text2, constants);
							if (ruleEntry == null)
							{
								entry.MarkKnownUnresolvable();
								output = "CANNOT_RESOLVE_SUBSYMBOL:" + text2;
								if (log)
								{
									logSb.Append("UNRESOLVABLE: Cannot resolve subsymbol '" + text2 + "'");
								}
								flag = true;
								break;
							}
							ruleEntry.uses++;
							if (TryResolveRecursive(ruleEntry, depth + 1, constants, out string output2, log))
							{
								text = text.Substring(0, num) + output2 + text.Substring(j + 1);
								j = num;
								break;
							}
							ruleEntry.MarkKnownUnresolvable();
						}
					}
				}
			}
			output = text;
			return !flag;
		}

		private static RuleEntry RandomPossiblyResolvableEntry(string keyword, Dictionary<string, string> constants)
		{
			return rules.TryGetValue(keyword)?.RandomElementByWeightWithFallback(delegate(RuleEntry rule)
			{
				if (rule.knownUnresolvable || !rule.ValidateConstantConstraints(constants))
				{
					return 0f;
				}
				return rule.SelectionWeight;
			});
		}

		public static bool ContainsSpecialChars(string str)
		{
			return str.IndexOfAny(SpecialChars) >= 0;
		}
	}
}
