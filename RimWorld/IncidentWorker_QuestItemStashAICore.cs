using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestItemStashAICore : IncidentWorker_QuestItemStash
	{
		protected override List<Thing> GenerateItems(Faction siteFaction, float siteThreatPoints)
		{
			List<Thing> list = new List<Thing>();
			list.Add(ThingMaker.MakeThing(ThingDefOf.AIPersonaCore));
			return list;
		}
	}
}
