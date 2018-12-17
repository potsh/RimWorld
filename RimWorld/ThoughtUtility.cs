using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ThoughtUtility
	{
		public static List<ThoughtDef> situationalSocialThoughtDefs;

		public static List<ThoughtDef> situationalNonSocialThoughtDefs;

		public static void Reset()
		{
			situationalSocialThoughtDefs = (from x in DefDatabase<ThoughtDef>.AllDefs
			where x.IsSituational && x.IsSocial
			select x).ToList();
			situationalNonSocialThoughtDefs = (from x in DefDatabase<ThoughtDef>.AllDefs
			where x.IsSituational && !x.IsSocial
			select x).ToList();
		}

		public static void GiveThoughtsForPawnExecuted(Pawn victim, PawnExecutionKind kind)
		{
			if (victim.RaceProps.Humanlike)
			{
				int forcedStage = 1;
				if (!victim.guilt.IsGuilty)
				{
					switch (kind)
					{
					case PawnExecutionKind.GenericHumane:
						forcedStage = 1;
						break;
					case PawnExecutionKind.GenericBrutal:
						forcedStage = 2;
						break;
					case PawnExecutionKind.OrganHarvesting:
						forcedStage = 3;
						break;
					}
				}
				else
				{
					forcedStage = 0;
				}
				ThoughtDef def = (!victim.IsColonist) ? ThoughtDefOf.KnowGuestExecuted : ThoughtDefOf.KnowColonistExecuted;
				foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
				{
					allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, forcedStage));
				}
			}
		}

		public static void GiveThoughtsForPawnOrganHarvested(Pawn victim)
		{
			if (victim.RaceProps.Humanlike)
			{
				ThoughtDef thoughtDef = null;
				if (victim.IsColonist)
				{
					thoughtDef = ThoughtDefOf.KnowColonistOrganHarvested;
				}
				else if (victim.HostFaction == Faction.OfPlayer)
				{
					thoughtDef = ThoughtDefOf.KnowGuestOrganHarvested;
				}
				foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner == victim)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyOrganHarvested);
					}
					else if (thoughtDef != null)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
					}
				}
			}
		}

		public static bool IsSituationalThoughtNullifiedByHediffs(ThoughtDef def, Pawn pawn)
		{
			if (def.IsMemory)
			{
				return false;
			}
			float num = 0f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null && curStage.pctConditionalThoughtsNullified > num)
				{
					num = curStage.pctConditionalThoughtsNullified;
				}
			}
			if (num == 0f)
			{
				return false;
			}
			Rand.PushState();
			Rand.Seed = pawn.thingIDNumber * 31 + def.index * 139;
			bool result = Rand.Value < num;
			Rand.PopState();
			return result;
		}

		public static bool IsThoughtNullifiedByOwnTales(ThoughtDef def, Pawn pawn)
		{
			if (def.nullifyingOwnTales != null)
			{
				for (int i = 0; i < def.nullifyingOwnTales.Count; i++)
				{
					if (Find.TaleManager.GetLatestTale(def.nullifyingOwnTales[i], pawn) != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void RemovePositiveBedroomThoughts(Pawn pawn)
		{
			if (pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefIf(ThoughtDefOf.SleptInBedroom, (Thought_Memory thought) => thought.MoodOffset() > 0f);
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefIf(ThoughtDefOf.SleptInBarracks, (Thought_Memory thought) => thought.MoodOffset() > 0f);
			}
		}

		public static bool CanGetThought(Pawn pawn, ThoughtDef def)
		{
			try
			{
				if (!def.validWhileDespawned && !pawn.Spawned && !def.IsMemory)
				{
					return false;
				}
				if (def.nullifyingTraits != null)
				{
					for (int i = 0; i < def.nullifyingTraits.Count; i++)
					{
						if (pawn.story.traits.HasTrait(def.nullifyingTraits[i]))
						{
							return false;
						}
					}
				}
				if (!def.requiredTraits.NullOrEmpty())
				{
					bool flag = false;
					for (int j = 0; j < def.requiredTraits.Count; j++)
					{
						if (pawn.story.traits.HasTrait(def.requiredTraits[j]) && (!def.RequiresSpecificTraitsDegree || def.requiredTraitsDegree == pawn.story.traits.DegreeOfTrait(def.requiredTraits[j])))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				if (def.nullifiedIfNotColonist && !pawn.IsColonist)
				{
					return false;
				}
				if (IsSituationalThoughtNullifiedByHediffs(def, pawn))
				{
					return false;
				}
				if (IsThoughtNullifiedByOwnTales(def, pawn))
				{
					return false;
				}
			}
			finally
			{
			}
			return true;
		}
	}
}
