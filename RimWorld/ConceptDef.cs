using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ConceptDef : Def
	{
		public float priority = 3.40282347E+38f;

		public bool noteTeaches;

		public bool needsOpportunity;

		public bool opportunityDecays = true;

		public ProgramState gameMode = ProgramState.Playing;

		[MustTranslate]
		private string helpText;

		[NoTranslate]
		public List<string> highlightTags;

		private static List<string> tmpParseErrors = new List<string>();

		public bool TriggeredDirect => priority <= 0f;

		public string HelpTextAdjusted => helpText.AdjustedForKeys();

		public override void PostLoad()
		{
			base.PostLoad();
			if (defName == "UnnamedDef")
			{
				defName = defName.ToString();
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string str = enumerator.Current;
					yield return str;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (priority > 9999999f)
			{
				yield return "priority isn't set";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (helpText.NullOrEmpty())
			{
				yield return "no help text";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (TriggeredDirect && label.NullOrEmpty())
			{
				yield return "no label";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			tmpParseErrors.Clear();
			helpText.AdjustedForKeys(tmpParseErrors, resolveKeys: false);
			int i = 0;
			if (i < tmpParseErrors.Count)
			{
				yield return "helpText error: " + tmpParseErrors[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_01f9:
			/*Error near IL_01fa: Unexpected return in MoveNext()*/;
		}

		public static ConceptDef Named(string defName)
		{
			return DefDatabase<ConceptDef>.GetNamed(defName);
		}

		public void HighlightAllTags()
		{
			if (highlightTags != null)
			{
				for (int i = 0; i < highlightTags.Count; i++)
				{
					UIHighlighter.HighlightTag(highlightTags[i]);
				}
			}
		}
	}
}
