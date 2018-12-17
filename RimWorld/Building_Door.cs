using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class Building_Door : Building
	{
		public CompPowerTrader powerComp;

		private bool openInt;

		private bool holdOpenInt;

		private int lastFriendlyTouchTick = -9999;

		protected int ticksUntilClose;

		protected int visualTicksOpen;

		private bool freePassageWhenClearedReachabilityCache;

		private const float OpenTicks = 45f;

		private const int CloseDelayTicks = 110;

		private const int WillCloseSoonThreshold = 111;

		private const int ApproachCloseDelayTicks = 300;

		private const int MaxTicksSinceFriendlyTouchToAutoClose = 120;

		private const float PowerOffDoorOpenSpeedFactor = 0.25f;

		private const float VisualDoorOffsetStart = 0f;

		private const float VisualDoorOffsetEnd = 0.45f;

		public bool Open => openInt;

		public bool HoldOpen => holdOpenInt;

		public bool FreePassage
		{
			get
			{
				if (!openInt)
				{
					return false;
				}
				return holdOpenInt || !WillCloseSoon;
			}
		}

		public bool WillCloseSoon
		{
			get
			{
				if (!base.Spawned)
				{
					return true;
				}
				if (!openInt)
				{
					return true;
				}
				if (holdOpenInt)
				{
					return false;
				}
				if (ticksUntilClose > 0 && ticksUntilClose <= 111 && !BlockedOpenMomentary)
				{
					return true;
				}
				if (CanTryCloseAutomatically && !BlockedOpenMomentary)
				{
					return true;
				}
				for (int i = 0; i < 5; i++)
				{
					IntVec3 c = base.Position + GenAdj.CardinalDirectionsAndInside[i];
					if (c.InBounds(base.Map))
					{
						List<Thing> thingList = c.GetThingList(base.Map);
						for (int j = 0; j < thingList.Count; j++)
						{
							Pawn pawn = thingList[j] as Pawn;
							if (pawn != null && !pawn.HostileTo(this) && !pawn.Downed && (pawn.Position == base.Position || (pawn.pather.Moving && pawn.pather.nextCell == base.Position)))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public bool BlockedOpenMomentary
		{
			get
			{
				List<Thing> thingList = base.Position.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Pawn)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool DoorPowerOn => powerComp != null && powerComp.PowerOn;

		public bool SlowsPawns => !DoorPowerOn || TicksToOpenNow > 20;

		public int TicksToOpenNow
		{
			get
			{
				float num = 45f / this.GetStatValue(StatDefOf.DoorOpenSpeed);
				if (DoorPowerOn)
				{
					num *= 0.25f;
				}
				return Mathf.RoundToInt(num);
			}
		}

		private bool CanTryCloseAutomatically => FriendlyTouchedRecently && !HoldOpen;

		private bool FriendlyTouchedRecently => Find.TickManager.TicksGame < lastFriendlyTouchTick + 120;

		private int VisualTicksToOpen => TicksToOpenNow;

		public override bool FireBulwark => !Open && base.FireBulwark;

		public override void PostMake()
		{
			base.PostMake();
			powerComp = GetComp<CompPowerTrader>();
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			ClearReachabilityCache(map);
			if (BlockedOpenMomentary)
			{
				DoorOpen();
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.DeSpawn(mode);
			ClearReachabilityCache(map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref openInt, "open", defaultValue: false);
			Scribe_Values.Look(ref holdOpenInt, "holdOpen", defaultValue: false);
			Scribe_Values.Look(ref lastFriendlyTouchTick, "lastFriendlyTouchTick", 0);
			if (Scribe.mode == LoadSaveMode.LoadingVars && openInt)
			{
				visualTicksOpen = VisualTicksToOpen;
			}
		}

		public override void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			base.SetFaction(newFaction, recruiter);
			if (base.Spawned)
			{
				ClearReachabilityCache(base.Map);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (FreePassage != freePassageWhenClearedReachabilityCache)
			{
				ClearReachabilityCache(base.Map);
			}
			if (!openInt)
			{
				if (visualTicksOpen > 0)
				{
					visualTicksOpen--;
				}
				if ((Find.TickManager.TicksGame + thingIDNumber.HashOffset()) % 375 == 0)
				{
					GenTemperature.EqualizeTemperaturesThroughBuilding(this, 1f, twoWay: false);
				}
			}
			else if (openInt)
			{
				if (visualTicksOpen < VisualTicksToOpen)
				{
					visualTicksOpen++;
				}
				List<Thing> thingList = base.Position.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null)
					{
						CheckFriendlyTouched(pawn);
					}
				}
				if (ticksUntilClose > 0)
				{
					if (base.Map.thingGrid.CellContains(base.Position, ThingCategory.Pawn))
					{
						ticksUntilClose = 110;
					}
					ticksUntilClose--;
					if (ticksUntilClose <= 0 && !holdOpenInt && !DoorTryClose())
					{
						ticksUntilClose = 1;
					}
				}
				else if (CanTryCloseAutomatically)
				{
					ticksUntilClose = 110;
				}
				if ((Find.TickManager.TicksGame + thingIDNumber.HashOffset()) % 34 == 0)
				{
					GenTemperature.EqualizeTemperaturesThroughBuilding(this, 1f, twoWay: false);
				}
			}
		}

		public void CheckFriendlyTouched(Pawn p)
		{
			if (!p.HostileTo(this) && PawnCanOpen(p))
			{
				lastFriendlyTouchTick = Find.TickManager.TicksGame;
			}
		}

		public void Notify_PawnApproaching(Pawn p, int moveCost)
		{
			CheckFriendlyTouched(p);
			if (PawnCanOpen(p))
			{
				base.Map.fogGrid.Notify_PawnEnteringDoor(this, p);
				if (!SlowsPawns)
				{
					int ticksToClose = Mathf.Max(300, moveCost + 1);
					DoorOpen(ticksToClose);
				}
			}
		}

		public bool CanPhysicallyPass(Pawn p)
		{
			return FreePassage || PawnCanOpen(p) || (Open && p.HostileTo(this));
		}

		public virtual bool PawnCanOpen(Pawn p)
		{
			Lord lord = p.GetLord();
			if (lord != null && lord.LordJob != null && lord.LordJob.CanOpenAnyDoor(p))
			{
				return true;
			}
			if (WildManUtility.WildManShouldReachOutsideNow(p))
			{
				return true;
			}
			if (base.Faction == null)
			{
				return true;
			}
			if (p.guest != null && p.guest.Released)
			{
				return true;
			}
			return GenAI.MachinesLike(base.Faction, p);
		}

		public override bool BlocksPawn(Pawn p)
		{
			if (openInt)
			{
				return false;
			}
			return !PawnCanOpen(p);
		}

		protected void DoorOpen(int ticksToClose = 110)
		{
			if (openInt)
			{
				ticksUntilClose = ticksToClose;
			}
			else
			{
				ticksUntilClose = TicksToOpenNow + ticksToClose;
			}
			if (!openInt)
			{
				openInt = true;
				CheckClearReachabilityCacheBecauseOpenedOrClosed();
				if (DoorPowerOn)
				{
					def.building.soundDoorOpenPowered.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
				else
				{
					def.building.soundDoorOpenManual.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
			}
		}

		protected bool DoorTryClose()
		{
			if (holdOpenInt || BlockedOpenMomentary)
			{
				return false;
			}
			openInt = false;
			CheckClearReachabilityCacheBecauseOpenedOrClosed();
			if (DoorPowerOn)
			{
				def.building.soundDoorClosePowered.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
			else
			{
				def.building.soundDoorCloseManual.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
			return true;
		}

		public void StartManualOpenBy(Pawn opener)
		{
			DoorOpen();
		}

		public void StartManualCloseBy(Pawn closer)
		{
			ticksUntilClose = 110;
		}

		public override void Draw()
		{
			base.Rotation = DoorRotationAt(base.Position, base.Map);
			float num = Mathf.Clamp01((float)visualTicksOpen / (float)VisualTicksToOpen);
			float d = 0.45f * num;
			for (int i = 0; i < 2; i++)
			{
				Vector3 vector = default(Vector3);
				Mesh mesh;
				if (i == 0)
				{
					vector = new Vector3(0f, 0f, -1f);
					mesh = MeshPool.plane10;
				}
				else
				{
					vector = new Vector3(0f, 0f, 1f);
					mesh = MeshPool.plane10Flip;
				}
				Rot4 rotation = base.Rotation;
				rotation.Rotate(RotationDirection.Clockwise);
				vector = rotation.AsQuat * vector;
				Vector3 vector2 = DrawPos;
				vector2.y = AltitudeLayer.DoorMoveable.AltitudeFor();
				vector2 += vector * d;
				Graphics.DrawMesh(mesh, vector2, base.Rotation.AsQuat, Graphic.MatAt(base.Rotation), 0);
			}
			Comps_PostDraw();
		}

		private static int AlignQualityAgainst(IntVec3 c, Map map)
		{
			if (!c.InBounds(map))
			{
				return 0;
			}
			if (!c.Walkable(map))
			{
				return 9;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (typeof(Building_Door).IsAssignableFrom(thing.def.thingClass))
				{
					return 1;
				}
				Thing thing2 = thing as Blueprint;
				if (thing2 != null)
				{
					if (thing2.def.entityDefToBuild.passability == Traversability.Impassable)
					{
						return 9;
					}
					if (typeof(Building_Door).IsAssignableFrom(thing.def.thingClass))
					{
						return 1;
					}
				}
			}
			return 0;
		}

		public static Rot4 DoorRotationAt(IntVec3 loc, Map map)
		{
			int num = 0;
			int num2 = 0;
			num += AlignQualityAgainst(loc + IntVec3.East, map);
			num += AlignQualityAgainst(loc + IntVec3.West, map);
			num2 += AlignQualityAgainst(loc + IntVec3.North, map);
			num2 += AlignQualityAgainst(loc + IntVec3.South, map);
			if (num >= num2)
			{
				return Rot4.North;
			}
			return Rot4.East;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g = enumerator.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (base.Faction == Faction.OfPlayer)
			{
				yield return (Gizmo)new Command_Toggle
				{
					defaultLabel = "CommandToggleDoorHoldOpen".Translate(),
					defaultDesc = "CommandToggleDoorHoldOpenDesc".Translate(),
					hotKey = KeyBindingDefOf.Misc3,
					icon = TexCommand.HoldOpen,
					isActive = (() => ((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0124: stateMachine*/)._0024this.holdOpenInt),
					toggleAction = delegate
					{
						((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_013b: stateMachine*/)._0024this.holdOpenInt = !((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_013b: stateMachine*/)._0024this.holdOpenInt;
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0175:
			/*Error near IL_0176: Unexpected return in MoveNext()*/;
		}

		private void ClearReachabilityCache(Map map)
		{
			map.reachability.ClearCache();
			freePassageWhenClearedReachabilityCache = FreePassage;
		}

		private void CheckClearReachabilityCacheBecauseOpenedOrClosed()
		{
			if (base.Spawned)
			{
				base.Map.reachability.ClearCacheForHostile(this);
			}
		}
	}
}
