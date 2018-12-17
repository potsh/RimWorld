using System.Collections.Generic;
using Verse.Grammar;

namespace Verse
{
	public class RulePackDef : Def
	{
		public List<RulePackDef> include;

		private RulePack rulePack;

		[Unsaved]
		private List<Rule> cachedRules;

		[Unsaved]
		private List<Rule> cachedUntranslatedRules;

		public List<Rule> RulesPlusIncludes
		{
			get
			{
				if (cachedRules == null)
				{
					cachedRules = new List<Rule>();
					if (rulePack != null)
					{
						cachedRules.AddRange(rulePack.Rules);
					}
					if (include != null)
					{
						for (int i = 0; i < include.Count; i++)
						{
							cachedRules.AddRange(include[i].RulesPlusIncludes);
						}
					}
				}
				return cachedRules;
			}
		}

		public List<Rule> UntranslatedRulesPlusIncludes
		{
			get
			{
				if (cachedUntranslatedRules == null)
				{
					cachedUntranslatedRules = new List<Rule>();
					if (rulePack != null)
					{
						cachedUntranslatedRules.AddRange(rulePack.UntranslatedRules);
					}
					if (include != null)
					{
						for (int i = 0; i < include.Count; i++)
						{
							cachedUntranslatedRules.AddRange(include[i].UntranslatedRulesPlusIncludes);
						}
					}
				}
				return cachedUntranslatedRules;
			}
		}

		public List<Rule> RulesImmediate => (rulePack == null) ? null : rulePack.Rules;

		public List<Rule> UntranslatedRulesImmediate => (rulePack == null) ? null : rulePack.UntranslatedRules;

		public string FirstRuleKeyword
		{
			get
			{
				List<Rule> rulesPlusIncludes = RulesPlusIncludes;
				return (!rulesPlusIncludes.Any()) ? "none" : rulesPlusIncludes[0].keyword;
			}
		}

		public string FirstUntranslatedRuleKeyword
		{
			get
			{
				List<Rule> untranslatedRulesPlusIncludes = UntranslatedRulesPlusIncludes;
				return (!untranslatedRulesPlusIncludes.Any()) ? "none" : untranslatedRulesPlusIncludes[0].keyword;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (include != null)
			{
				int i = 0;
				while (true)
				{
					if (i >= include.Count)
					{
						yield break;
					}
					if (include[i].include != null && include[i].include.Contains(this))
					{
						break;
					}
					i++;
				}
				yield return "includes other RulePackDef which includes it: " + include[i].defName;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_018c:
			/*Error near IL_018d: Unexpected return in MoveNext()*/;
		}

		public static RulePackDef Named(string defName)
		{
			return DefDatabase<RulePackDef>.GetNamed(defName);
		}
	}
}
