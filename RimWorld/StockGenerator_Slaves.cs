using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Slaves : StockGenerator
	{
		private bool respectPopulationIntent;

		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			if (!respectPopulationIntent || !(Rand.Value > StorytellerUtilityPopulation.PopulationIntent))
			{
				int count = countRange.RandomInRange;
				int i = 0;
				if (i < count && (from fac in Find.FactionManager.AllFactionsVisible
				where fac != Faction.OfPlayer && fac.def.humanlikeFaction
				select fac).TryRandomElement(out Faction slaveFaction))
				{
					PawnKindDef slave = PawnKindDefOf.Slave;
					PawnGenerationRequest request = new PawnGenerationRequest(slave, slaveFaction, PawnGenerationContext.NonPlayer, forTile, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, !trader.orbital);
					yield return (Thing)PawnGenerator.GeneratePawn(request);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability != Tradeability.None;
		}
	}
}
