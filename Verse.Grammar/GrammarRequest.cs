using System.Collections.Generic;

namespace Verse.Grammar
{
	public struct GrammarRequest
	{
		private List<Rule> rules;

		private List<RulePack> includesBare;

		private List<RulePackDef> includes;

		private Dictionary<string, string> constants;

		public List<Rule> Rules
		{
			get
			{
				if (rules == null)
				{
					rules = new List<Rule>();
				}
				return rules;
			}
		}

		public List<RulePack> IncludesBare
		{
			get
			{
				if (includesBare == null)
				{
					includesBare = new List<RulePack>();
				}
				return includesBare;
			}
		}

		public List<RulePackDef> Includes
		{
			get
			{
				if (includes == null)
				{
					includes = new List<RulePackDef>();
				}
				return includes;
			}
		}

		public Dictionary<string, string> Constants
		{
			get
			{
				if (constants == null)
				{
					constants = new Dictionary<string, string>();
				}
				return constants;
			}
		}

		public void Clear()
		{
			if (rules != null)
			{
				rules.Clear();
			}
			if (includesBare != null)
			{
				includesBare.Clear();
			}
			if (includes != null)
			{
				includes.Clear();
			}
			if (constants != null)
			{
				constants.Clear();
			}
		}

		public List<Rule> GetRules()
		{
			return rules;
		}

		public List<RulePack> GetIncludesBare()
		{
			return includesBare;
		}

		public List<RulePackDef> GetIncludes()
		{
			return includes;
		}

		public Dictionary<string, string> GetConstants()
		{
			return constants;
		}
	}
}
