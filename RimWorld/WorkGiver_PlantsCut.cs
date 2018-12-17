using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_PlantsCut : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<Designation> desList = pawn.Map.designationManager.allDesignations;
			int i = 0;
			Designation des;
			while (true)
			{
				if (i >= desList.Count)
				{
					yield break;
				}
				des = desList[i];
				if (des.def == DesignationDefOf.CutPlant || des.def == DesignationDefOf.HarvestPlant)
				{
					break;
				}
				i++;
			}
			yield return des.target.Thing;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.def.category != ThingCategory.Plant)
			{
				return null;
			}
			LocalTargetInfo target = t;
			bool ignoreOtherReservations = forced;
			if (!pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations))
			{
				return null;
			}
			if (t.IsForbidden(pawn))
			{
				return null;
			}
			if (t.IsBurning())
			{
				return null;
			}
			foreach (Designation item in pawn.Map.designationManager.AllDesignationsOn(t))
			{
				if (item.def == DesignationDefOf.HarvestPlant)
				{
					if (!((Plant)t).HarvestableNow)
					{
						return null;
					}
					return new Job(JobDefOf.HarvestDesignated, t);
				}
				if (item.def == DesignationDefOf.CutPlant)
				{
					return new Job(JobDefOf.CutPlantDesignated, t);
				}
			}
			return null;
		}
	}
}
