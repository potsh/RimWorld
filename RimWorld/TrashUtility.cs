using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class TrashUtility
	{
		private const float ChanceHateInertBuilding = 0.008f;

		private static readonly IntRange TrashJobCheckOverrideInterval = new IntRange(450, 500);

		public static bool ShouldTrashPlant(Pawn pawn, Plant p)
		{
			if (!p.sown || p.def.plant.IsTree || !p.FlammableNow || !CanTrash(pawn, p))
			{
				return false;
			}
			CellRect.CellRectIterator iterator = CellRect.CenteredOn(p.Position, 2).ClipInsideMap(p.Map).GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (current.InBounds(p.Map) && current.ContainsStaticFire(p.Map))
				{
					return false;
				}
				iterator.MoveNext();
			}
			if (!p.Position.Roofed(p.Map) && p.Map.weatherManager.RainRate > 0.25f)
			{
				return false;
			}
			return true;
		}

		public static bool ShouldTrashBuilding(Pawn pawn, Building b, bool attackAllInert = false)
		{
			if (!b.def.useHitPoints)
			{
				return false;
			}
			if (pawn.mindState.spawnedByInfestationThingComp && b.GetComp<CompCreatesInfestations>() != null)
			{
				return false;
			}
			if (((b.def.building.isInert || b.def.IsFrame) && !attackAllInert) || b.def.building.isTrap)
			{
				int num = GenLocalDate.HourOfDay(pawn) / 3;
				int specialSeed = (b.GetHashCode() * 612361) ^ (pawn.GetHashCode() * 391) ^ (num * 73427324);
				if (!Rand.ChanceSeeded(0.008f, specialSeed))
				{
					return false;
				}
			}
			if (b.def.building.isTrap)
			{
				return false;
			}
			if (!CanTrash(pawn, b) || !pawn.HostileTo(b))
			{
				return false;
			}
			return true;
		}

		private static bool CanTrash(Pawn pawn, Thing t)
		{
			if (!pawn.CanReach(t, PathEndMode.Touch, Danger.Some) || t.IsBurning())
			{
				return false;
			}
			return true;
		}

		public static Job TrashJob(Pawn pawn, Thing t, bool allowPunchingInert = false)
		{
			Plant plant = t as Plant;
			if (plant != null)
			{
				Job job = new Job(JobDefOf.Ignite, t);
				FinalizeTrashJob(job);
				return job;
			}
			if (pawn.equipment != null && Rand.Value < 0.7f)
			{
				foreach (Verb allEquipmentVerb in pawn.equipment.AllEquipmentVerbs)
				{
					if (allEquipmentVerb.verbProps.ai_IsBuildingDestroyer)
					{
						Job job2 = new Job(JobDefOf.UseVerbOnThing, t);
						job2.verbToUse = allEquipmentVerb;
						FinalizeTrashJob(job2);
						return job2;
					}
				}
			}
			Job job3 = null;
			float value = Rand.Value;
			if (value < 0.35f && pawn.natives.IgniteVerb != null && pawn.natives.IgniteVerb.IsStillUsableBy(pawn) && t.FlammableNow && !t.IsBurning() && !(t is Building_Door))
			{
				job3 = new Job(JobDefOf.Ignite, t);
			}
			else
			{
				if (((t as Building)?.def.building.isInert ?? false) && !allowPunchingInert)
				{
					return null;
				}
				job3 = new Job(JobDefOf.AttackMelee, t);
			}
			FinalizeTrashJob(job3);
			return job3;
		}

		private static void FinalizeTrashJob(Job job)
		{
			job.expiryInterval = TrashJobCheckOverrideInterval.RandomInRange;
			job.checkOverrideOnExpire = true;
			job.expireRequiresEnemiesNearby = true;
		}
	}
}
