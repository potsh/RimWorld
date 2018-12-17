using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_CategoryIndividualMTBByBiome : StorytellerComp
	{
		protected StorytellerCompProperties_CategoryIndividualMTBByBiome Props => (StorytellerCompProperties_CategoryIndividualMTBByBiome)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (!(target is World))
			{
				List<IncidentDef> allIncidents = DefDatabase<IncidentDef>.AllDefsListForReading;
				int i = 0;
				IncidentDef inc;
				IncidentParms parms;
				while (true)
				{
					if (i >= allIncidents.Count)
					{
						yield break;
					}
					inc = allIncidents[i];
					if (inc.category == Props.category)
					{
						_003CMakeIntervalIncidents_003Ec__Iterator0 _003CMakeIntervalIncidents_003Ec__Iterator = (_003CMakeIntervalIncidents_003Ec__Iterator0)/*Error near IL_0095: stateMachine*/;
						BiomeDef biome = Find.WorldGrid[target.Tile].biome;
						if (inc.mtbDaysByBiome != null)
						{
							MTBByBiome entry = inc.mtbDaysByBiome.Find((MTBByBiome x) => x.biome == biome);
							if (entry != null)
							{
								float mtb = entry.mtbDays;
								if (Props.applyCaravanVisibility)
								{
									Caravan caravan = target as Caravan;
									if (caravan != null)
									{
										mtb /= caravan.Visibility;
									}
									else
									{
										Map map = target as Map;
										if (map != null && map.Parent.def.isTempIncidentMapOwner)
										{
											IEnumerable<Pawn> pawns = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Concat(map.mapPawns.PrisonersOfColonySpawned);
											mtb /= CaravanVisibilityCalculator.Visibility(pawns, caravanMovingNow: false);
										}
									}
								}
								if (Rand.MTBEventOccurs(mtb, 60000f, 1000f))
								{
									parms = GenerateParms(inc.category, target);
									if (inc.Worker.CanFireNow(parms))
									{
										break;
									}
								}
							}
						}
					}
					i++;
				}
				yield return new FiringIncident(inc, this, parms);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.category;
		}
	}
}
