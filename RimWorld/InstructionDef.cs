using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InstructionDef : Def
	{
		public Type instructionClass = typeof(Instruction_Basic);

		[MustTranslate]
		public string text;

		public bool startCentered;

		public bool tutorialModeOnly = true;

		[NoTranslate]
		public string eventTagInitiate;

		public InstructionDef eventTagInitiateSource;

		[NoTranslate]
		public List<string> eventTagsEnd;

		[NoTranslate]
		public List<string> actionTagsAllowed;

		[MustTranslate]
		public string rejectInputMessage;

		public ConceptDef concept;

		[NoTranslate]
		public List<string> highlightTags;

		[MustTranslate]
		public string onMapInstruction;

		public int targetCount;

		public ThingDef thingDef;

		public RecipeDef recipeDef;

		public int recipeTargetCount = 1;

		public ThingDef giveOnActivateDef;

		public int giveOnActivateCount;

		public bool endTutorial;

		public bool resetBuildDesignatorStuffs;

		private static List<string> tmpParseErrors = new List<string>();

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (instructionClass == null)
			{
				yield return "no instruction class";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (text.NullOrEmpty())
			{
				yield return "no text";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (eventTagInitiate.NullOrEmpty())
			{
				yield return "no eventTagInitiate";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			tmpParseErrors.Clear();
			text.AdjustedForKeys(tmpParseErrors, resolveKeys: false);
			int i = 0;
			if (i < tmpParseErrors.Count)
			{
				yield return "text error: " + tmpParseErrors[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_01e4:
			/*Error near IL_01e5: Unexpected return in MoveNext()*/;
		}
	}
}
