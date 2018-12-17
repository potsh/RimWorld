using Verse;

namespace RimWorld
{
	public abstract class GenStep_Ambush : GenStep
	{
		public FloatRange defaultPointsRange = new FloatRange(180f, 340f);

		public override void Generate(Map map, GenStepParams parms)
		{
			if (SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out CellRect rectToDefend, out IntVec3 singleCellToSpawnNear, map))
			{
				SpawnTrigger(rectToDefend, singleCellToSpawnNear, map, parms);
			}
		}

		private void SpawnTrigger(CellRect rectToDefend, IntVec3 root, Map map, GenStepParams parms)
		{
			int nextSignalTagID = Find.UniqueIDsManager.GetNextSignalTagID();
			string signalTag = "ambushActivated-" + nextSignalTagID;
			CellRect rect = (!root.IsValid) ? rectToDefend.ExpandedBy(12) : CellRect.CenteredOn(root, 17);
			SignalAction_Ambush signalAction_Ambush = MakeAmbushSignalAction(rectToDefend, root, parms);
			signalAction_Ambush.signalTag = signalTag;
			GenSpawn.Spawn(signalAction_Ambush, rect.CenterCell, map);
			RectTrigger rectTrigger = MakeRectTrigger();
			rectTrigger.signalTag = signalTag;
			rectTrigger.Rect = rect;
			GenSpawn.Spawn(rectTrigger, rect.CenterCell, map);
			TriggerUnfogged triggerUnfogged = (TriggerUnfogged)ThingMaker.MakeThing(ThingDefOf.TriggerUnfogged);
			triggerUnfogged.signalTag = signalTag;
			GenSpawn.Spawn(triggerUnfogged, rect.CenterCell, map);
		}

		protected virtual RectTrigger MakeRectTrigger()
		{
			return (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
		}

		protected virtual SignalAction_Ambush MakeAmbushSignalAction(CellRect rectToDefend, IntVec3 root, GenStepParams parms)
		{
			SignalAction_Ambush signalAction_Ambush = (SignalAction_Ambush)ThingMaker.MakeThing(ThingDefOf.SignalAction_Ambush);
			if (parms.siteCoreOrPart != null)
			{
				signalAction_Ambush.points = parms.siteCoreOrPart.parms.threatPoints;
			}
			else
			{
				signalAction_Ambush.points = defaultPointsRange.RandomInRange;
			}
			switch (Rand.RangeInclusive(0, 2))
			{
			case 0:
				signalAction_Ambush.ambushType = SignalActionAmbushType.Manhunters;
				break;
			case 1:
				if (PawnGroupMakerUtility.CanGenerateAnyNormalGroup(Faction.OfMechanoids, signalAction_Ambush.points))
				{
					signalAction_Ambush.ambushType = SignalActionAmbushType.Mechanoids;
					break;
				}
				goto default;
			default:
				signalAction_Ambush.ambushType = SignalActionAmbushType.Normal;
				break;
			}
			return signalAction_Ambush;
		}
	}
}
