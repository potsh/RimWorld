using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ScenPart_StartingThing_Defined : ScenPart_ThingCount
	{
		public const string PlayerStartWithTag = "PlayerStartsWith";

		public static string PlayerStartWithIntro => "ScenPart_StartWith".Translate();

		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", PlayerStartWithIntro);
		}

		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == "PlayerStartsWith")
			{
				yield return GenLabel.ThingLabel(thingDef, stuff, count).CapitalizeFirst();
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override IEnumerable<Thing> PlayerStartingThings()
		{
			Thing t = ThingMaker.MakeThing(thingDef, stuff);
			if (thingDef.Minifiable)
			{
				t = t.MakeMinified();
			}
			t.stackCount = count;
			yield return t;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
