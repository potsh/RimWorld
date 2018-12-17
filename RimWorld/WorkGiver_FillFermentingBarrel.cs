using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_FillFermentingBarrel : WorkGiver_Scanner
	{
		private static string TemperatureTrans;

		private static string NoWortTrans;

		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.FermentingBarrel);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public static void ResetStaticData()
		{
			TemperatureTrans = "BadTemperature".Translate().ToLower();
			NoWortTrans = "NoWort".Translate();
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_FermentingBarrel building_FermentingBarrel = t as Building_FermentingBarrel;
			if (building_FermentingBarrel == null || building_FermentingBarrel.Fermented || building_FermentingBarrel.SpaceLeftForWort <= 0)
			{
				return false;
			}
			float ambientTemperature = building_FermentingBarrel.AmbientTemperature;
			CompProperties_TemperatureRuinable compProperties = building_FermentingBarrel.def.GetCompProperties<CompProperties_TemperatureRuinable>();
			if (ambientTemperature < compProperties.minSafeTemperature + 2f || ambientTemperature > compProperties.maxSafeTemperature - 2f)
			{
				JobFailReason.Is(TemperatureTrans);
				return false;
			}
			if (!t.IsForbidden(pawn))
			{
				LocalTargetInfo target = t;
				bool ignoreOtherReservations = forced;
				if (pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations))
				{
					if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
					{
						return false;
					}
					Thing thing = FindWort(pawn, building_FermentingBarrel);
					if (thing == null)
					{
						JobFailReason.Is(NoWortTrans);
						return false;
					}
					if (t.IsBurning())
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_FermentingBarrel barrel = (Building_FermentingBarrel)t;
			Thing t2 = FindWort(pawn, barrel);
			return new Job(JobDefOf.FillFermentingBarrel, t, t2);
		}

		private Thing FindWort(Pawn pawn, Building_FermentingBarrel barrel)
		{
			Predicate<Thing> predicate = delegate(Thing x)
			{
				if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
				{
					return false;
				}
				return true;
			};
			IntVec3 position = pawn.Position;
			Map map = pawn.Map;
			ThingRequest thingReq = ThingRequest.ForDef(ThingDefOf.Wort);
			PathEndMode peMode = PathEndMode.ClosestTouch;
			TraverseParms traverseParams = TraverseParms.For(pawn);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator);
		}
	}
}
