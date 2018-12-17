using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JoyGiver
	{
		public JoyGiverDef def;

		public virtual float GetChance(Pawn pawn)
		{
			return def.baseChance;
		}

		protected virtual void GetSearchSet(Pawn pawn, List<Thing> outCandidates)
		{
			outCandidates.Clear();
			if (def.thingDefs != null)
			{
				if (def.thingDefs.Count == 1)
				{
					outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(def.thingDefs[0]));
				}
				else
				{
					for (int i = 0; i < def.thingDefs.Count; i++)
					{
						outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(def.thingDefs[i]));
					}
				}
			}
		}

		public abstract Job TryGiveJob(Pawn pawn);

		public virtual Job TryGiveJobWhileInBed(Pawn pawn)
		{
			return null;
		}

		public virtual Job TryGiveJobInPartyArea(Pawn pawn, IntVec3 partySpot)
		{
			return null;
		}

		public PawnCapacityDef MissingRequiredCapacity(Pawn pawn)
		{
			for (int i = 0; i < def.requiredCapacities.Count; i++)
			{
				if (!pawn.health.capacities.CapableOf(def.requiredCapacities[i]))
				{
					return def.requiredCapacities[i];
				}
			}
			return null;
		}
	}
}
