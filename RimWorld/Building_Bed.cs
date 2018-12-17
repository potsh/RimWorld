using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Building_Bed : Building, IAssignableBuilding
	{
		private bool forPrisonersInt;

		private bool medicalInt;

		private bool alreadySetDefaultMed;

		public List<Pawn> owners = new List<Pawn>();

		private static int lastPrisonerSetChangeFrame = -1;

		private static readonly Color SheetColorNormal = new Color(0.6313726f, 0.8352941f, 0.7058824f);

		private static readonly Color SheetColorRoyal = new Color(0.670588255f, 0.9137255f, 0.745098054f);

		public static readonly Color SheetColorForPrisoner = new Color(1f, 0.7176471f, 0.129411772f);

		private static readonly Color SheetColorMedical = new Color(0.3882353f, 0.623529434f, 0.8862745f);

		private static readonly Color SheetColorMedicalForPrisoner = new Color(0.654902f, 0.3764706f, 0.152941182f);

		public bool ForPrisoners
		{
			get
			{
				return forPrisonersInt;
			}
			set
			{
				if (value != forPrisonersInt && def.building.bed_humanlike)
				{
					if (Current.ProgramState != ProgramState.Playing && Scribe.mode != 0)
					{
						Log.Error("Tried to set ForPrisoners while game mode was " + Current.ProgramState);
					}
					else
					{
						RemoveAllOwners();
						forPrisonersInt = value;
						Notify_ColorChanged();
						NotifyRoomBedTypeChanged();
					}
				}
			}
		}

		public bool Medical
		{
			get
			{
				return medicalInt;
			}
			set
			{
				if (value != medicalInt && def.building.bed_humanlike)
				{
					RemoveAllOwners();
					medicalInt = value;
					Notify_ColorChanged();
					if (base.Spawned)
					{
						base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
						NotifyRoomBedTypeChanged();
					}
					FacilityChanged();
				}
			}
		}

		public bool AnyUnownedSleepingSlot
		{
			get
			{
				if (Medical)
				{
					Log.Warning("Tried to check for unowned sleeping slot on medical bed " + this);
					return false;
				}
				return owners.Count < SleepingSlotsCount;
			}
		}

		public bool AnyUnoccupiedSleepingSlot
		{
			get
			{
				for (int i = 0; i < SleepingSlotsCount; i++)
				{
					if (GetCurOccupant(i) == null)
					{
						return true;
					}
				}
				return false;
			}
		}

		public IEnumerable<Pawn> CurOccupants
		{
			get
			{
				int i = 0;
				Pawn occupant;
				while (true)
				{
					if (i >= SleepingSlotsCount)
					{
						yield break;
					}
					occupant = GetCurOccupant(i);
					if (occupant != null)
					{
						break;
					}
					i++;
				}
				yield return occupant;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override Color DrawColor
		{
			get
			{
				if (def.MadeFromStuff)
				{
					return base.DrawColor;
				}
				return DrawColorTwo;
			}
		}

		public override Color DrawColorTwo
		{
			get
			{
				if (!def.building.bed_humanlike)
				{
					return base.DrawColorTwo;
				}
				bool forPrisoners = ForPrisoners;
				bool medical = Medical;
				if (forPrisoners && medical)
				{
					return SheetColorMedicalForPrisoner;
				}
				if (forPrisoners)
				{
					return SheetColorForPrisoner;
				}
				if (medical)
				{
					return SheetColorMedical;
				}
				if (def == ThingDefOf.RoyalBed)
				{
					return SheetColorRoyal;
				}
				return SheetColorNormal;
			}
		}

		public int SleepingSlotsCount => BedUtility.GetSleepingSlotsCount(def.size);

		public IEnumerable<Pawn> AssigningCandidates
		{
			get
			{
				if (!base.Spawned)
				{
					return Enumerable.Empty<Pawn>();
				}
				return base.Map.mapPawns.FreeColonists;
			}
		}

		public IEnumerable<Pawn> AssignedPawns => owners;

		public int MaxAssignedPawnsCount => SleepingSlotsCount;

		private bool PlayerCanSeeOwners
		{
			get
			{
				if (base.Faction == Faction.OfPlayer)
				{
					return true;
				}
				for (int i = 0; i < owners.Count; i++)
				{
					if (owners[i].Faction == Faction.OfPlayer || owners[i].HostFaction == Faction.OfPlayer)
					{
						return true;
					}
				}
				return false;
			}
		}

		public void TryAssignPawn(Pawn owner)
		{
			owner.ownership.ClaimBedIfNonMedical(this);
		}

		public void TryUnassignPawn(Pawn pawn)
		{
			if (owners.Contains(pawn))
			{
				pawn.ownership.UnclaimBed();
			}
		}

		public bool AssignedAnything(Pawn pawn)
		{
			return pawn.ownership.OwnedBed != null;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			Region validRegionAt_NoRebuild = map.regionGrid.GetValidRegionAt_NoRebuild(base.Position);
			if (validRegionAt_NoRebuild != null && validRegionAt_NoRebuild.Room.isPrisonCell)
			{
				ForPrisoners = true;
			}
			if (!alreadySetDefaultMed)
			{
				alreadySetDefaultMed = true;
				if (def.building.bed_defaultMedical)
				{
					Medical = true;
				}
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			RemoveAllOwners();
			ForPrisoners = false;
			Medical = false;
			alreadySetDefaultMed = false;
			Room room = this.GetRoom();
			base.DeSpawn(mode);
			room?.Notify_RoomShapeOrContainedBedsChanged();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref forPrisonersInt, "forPrisoners", defaultValue: false);
			Scribe_Values.Look(ref medicalInt, "medical", defaultValue: false);
			Scribe_Values.Look(ref alreadySetDefaultMed, "alreadySetDefaultMed", defaultValue: false);
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			Room room = this.GetRoom();
			if (room != null && RoomCanBePrisonCell(room))
			{
				room.DrawFieldEdges();
			}
		}

		public static bool RoomCanBePrisonCell(Room r)
		{
			return !r.TouchesMapEdge && !r.IsHuge && r.RegionType == RegionType.Normal;
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
			if (def.building.bed_humanlike && base.Faction == Faction.OfPlayer)
			{
				Command_Toggle pris = new Command_Toggle
				{
					defaultLabel = "CommandBedSetForPrisonersLabel".Translate(),
					defaultDesc = "CommandBedSetForPrisonersDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners"),
					isActive = this.get_ForPrisoners,
					toggleAction = delegate
					{
						((_003CGetGizmos_003Ec__Iterator1)/*Error near IL_0158: stateMachine*/)._0024this.ToggleForPrisonersByInterface();
					}
				};
				if (!RoomCanBePrisonCell(this.GetRoom()) && !ForPrisoners)
				{
					pris.Disable("CommandBedSetForPrisonersFailOutdoors".Translate());
				}
				pris.hotKey = KeyBindingDefOf.Misc3;
				pris.turnOffSound = null;
				pris.turnOnSound = null;
				yield return (Gizmo)pris;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0355:
			/*Error near IL_0356: Unexpected return in MoveNext()*/;
		}

		private void ToggleForPrisonersByInterface()
		{
			if (lastPrisonerSetChangeFrame != Time.frameCount)
			{
				lastPrisonerSetChangeFrame = Time.frameCount;
				bool newForPrisoners = !ForPrisoners;
				SoundDef soundDef = (!newForPrisoners) ? SoundDefOf.Checkbox_TurnedOff : SoundDefOf.Checkbox_TurnedOn;
				soundDef.PlayOneShotOnCamera();
				List<Building_Bed> bedsToAffect = new List<Building_Bed>();
				foreach (Building_Bed item in Find.Selector.SelectedObjects.OfType<Building_Bed>())
				{
					if (item.ForPrisoners != newForPrisoners)
					{
						Room room = item.GetRoom();
						if (room == null || !RoomCanBePrisonCell(room))
						{
							if (!bedsToAffect.Contains(item))
							{
								bedsToAffect.Add(item);
							}
						}
						else
						{
							foreach (Building_Bed containedBed in room.ContainedBeds)
							{
								if (!bedsToAffect.Contains(containedBed))
								{
									bedsToAffect.Add(containedBed);
								}
							}
						}
					}
				}
				Action action = delegate
				{
					List<Room> list = new List<Room>();
					foreach (Building_Bed item2 in bedsToAffect)
					{
						Room room2 = item2.GetRoom();
						item2.ForPrisoners = (newForPrisoners && !room2.TouchesMapEdge);
						for (int j = 0; j < SleepingSlotsCount; j++)
						{
							GetCurOccupant(j)?.jobs.EndCurrentJob(JobCondition.InterruptForced);
						}
						if (!list.Contains(room2) && !room2.TouchesMapEdge)
						{
							list.Add(room2);
						}
					}
					foreach (Room item3 in list)
					{
						item3.Notify_RoomShapeOrContainedBedsChanged();
					}
				};
				if ((from b in bedsToAffect
				where b.owners.Any() && b != this
				select b).Count() == 0)
				{
					action();
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (newForPrisoners)
					{
						stringBuilder.Append("TurningOnPrisonerBedWarning".Translate());
					}
					else
					{
						stringBuilder.Append("TurningOffPrisonerBedWarning".Translate());
					}
					stringBuilder.AppendLine();
					foreach (Building_Bed item4 in bedsToAffect)
					{
						if ((newForPrisoners && !item4.ForPrisoners) || (!newForPrisoners && item4.ForPrisoners))
						{
							for (int i = 0; i < item4.owners.Count; i++)
							{
								stringBuilder.AppendLine();
								stringBuilder.Append(item4.owners[i].LabelShort);
							}
						}
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.Append("AreYouSure".Translate());
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(stringBuilder.ToString(), action));
				}
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (def.building.bed_humanlike)
			{
				stringBuilder.AppendLine();
				if (ForPrisoners)
				{
					stringBuilder.AppendLine("ForPrisonerUse".Translate());
				}
				else if (PlayerCanSeeOwners)
				{
					stringBuilder.AppendLine("ForColonistUse".Translate());
				}
				if (Medical)
				{
					stringBuilder.AppendLine("MedicalBed".Translate());
					if (base.Spawned)
					{
						stringBuilder.AppendLine("RoomInfectionChanceFactor".Translate() + ": " + this.GetRoom().GetStat(RoomStatDefOf.InfectionChanceFactor).ToStringPercent());
					}
				}
				else if (PlayerCanSeeOwners)
				{
					if (owners.Count == 0)
					{
						stringBuilder.AppendLine("Owner".Translate() + ": " + "Nobody".Translate());
					}
					else if (owners.Count == 1)
					{
						stringBuilder.AppendLine("Owner".Translate() + ": " + owners[0].Label);
					}
					else
					{
						stringBuilder.Append("Owners".Translate() + ": ");
						bool flag = false;
						for (int i = 0; i < owners.Count; i++)
						{
							if (flag)
							{
								stringBuilder.Append(", ");
							}
							flag = true;
							stringBuilder.Append(owners[i].LabelShort);
						}
						stringBuilder.AppendLine();
					}
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			_003CGetFloatMenuOptions_003Ec__Iterator2 _003CGetFloatMenuOptions_003Ec__Iterator = (_003CGetFloatMenuOptions_003Ec__Iterator2)/*Error near IL_0036: stateMachine*/;
			if (myPawn.RaceProps.Humanlike && !ForPrisoners && Medical && !myPawn.Drafted && base.Faction == Faction.OfPlayer && RestUtility.CanUseBedEver(myPawn, def))
			{
				if (!HealthAIUtility.ShouldSeekMedicalRest(myPawn) && !HealthAIUtility.ShouldSeekMedicalRestUrgent(myPawn))
				{
					yield return new FloatMenuOption("UseMedicalBed".Translate() + " (" + "NotInjured".Translate() + ")", null);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(action: delegate
				{
					if (!_003CGetFloatMenuOptions_003Ec__Iterator._0024this.ForPrisoners && _003CGetFloatMenuOptions_003Ec__Iterator._0024this.Medical && myPawn.CanReserveAndReach(_003CGetFloatMenuOptions_003Ec__Iterator._0024this, PathEndMode.ClosestTouch, Danger.Deadly, _003CGetFloatMenuOptions_003Ec__Iterator._0024this.SleepingSlotsCount, -1, null, ignoreOtherReservations: true))
					{
						if (myPawn.CurJobDef == JobDefOf.LayDown && myPawn.CurJob.GetTarget(TargetIndex.A).Thing == _003CGetFloatMenuOptions_003Ec__Iterator._0024this)
						{
							myPawn.CurJob.restUntilHealed = true;
						}
						else
						{
							Job job = new Job(JobDefOf.LayDown, _003CGetFloatMenuOptions_003Ec__Iterator._0024this)
							{
								restUntilHealed = true
							};
							myPawn.jobs.TryTakeOrderedJob(job);
						}
						myPawn.mindState.ResetLastDisturbanceTick();
					}
				}, label: "UseMedicalBed".Translate()), myPawn, this, (!AnyUnoccupiedSleepingSlot) ? "SomeoneElseSleeping" : "ReservedBy");
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override void DrawGUIOverlay()
		{
			if (!Medical && Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && PlayerCanSeeOwners)
			{
				Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
				if (!owners.Any())
				{
					GenMapUI.DrawThingLabel(this, "Unowned".Translate(), defaultThingLabelColor);
				}
				else if (owners.Count == 1)
				{
					if (!owners[0].InBed() || owners[0].CurrentBed() != this)
					{
						GenMapUI.DrawThingLabel(this, owners[0].LabelShort, defaultThingLabelColor);
					}
				}
				else
				{
					for (int i = 0; i < owners.Count; i++)
					{
						if (!owners[i].InBed() || owners[i].CurrentBed() != this || !(owners[i].Position == GetSleepingSlotPos(i)))
						{
							Vector3 multiOwnersLabelScreenPosFor = GetMultiOwnersLabelScreenPosFor(i);
							GenMapUI.DrawThingLabel(multiOwnersLabelScreenPosFor, owners[i].LabelShort, defaultThingLabelColor);
						}
					}
				}
			}
		}

		public Pawn GetCurOccupant(int slotIndex)
		{
			if (!base.Spawned)
			{
				return null;
			}
			IntVec3 sleepingSlotPos = GetSleepingSlotPos(slotIndex);
			List<Thing> list = base.Map.thingGrid.ThingsListAt(sleepingSlotPos);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i] as Pawn;
				if (pawn != null && pawn.CurJob != null && pawn.GetPosture() == PawnPosture.LayingInBed)
				{
					return pawn;
				}
			}
			return null;
		}

		public int GetCurOccupantSlotIndex(Pawn curOccupant)
		{
			for (int i = 0; i < SleepingSlotsCount; i++)
			{
				if (GetCurOccupant(i) == curOccupant)
				{
					return i;
				}
			}
			Log.Error("Could not find pawn " + curOccupant + " on any of sleeping slots.");
			return 0;
		}

		public Pawn GetCurOccupantAt(IntVec3 pos)
		{
			for (int i = 0; i < SleepingSlotsCount; i++)
			{
				if (GetSleepingSlotPos(i) == pos)
				{
					return GetCurOccupant(i);
				}
			}
			return null;
		}

		public IntVec3 GetSleepingSlotPos(int index)
		{
			return BedUtility.GetSleepingSlotPos(index, base.Position, base.Rotation, def.size);
		}

		public void SortOwners()
		{
			owners.SortBy((Pawn x) => x.thingIDNumber);
		}

		private void RemoveAllOwners()
		{
			for (int num = owners.Count - 1; num >= 0; num--)
			{
				owners[num].ownership.UnclaimBed();
			}
		}

		private void NotifyRoomBedTypeChanged()
		{
			this.GetRoom()?.Notify_BedTypeChanged();
		}

		private void FacilityChanged()
		{
			CompFacility compFacility = this.TryGetComp<CompFacility>();
			CompAffectedByFacilities compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
			compFacility?.Notify_ThingChanged();
			compAffectedByFacilities?.Notify_ThingChanged();
		}

		private Vector3 GetMultiOwnersLabelScreenPosFor(int slotIndex)
		{
			IntVec3 sleepingSlotPos = GetSleepingSlotPos(slotIndex);
			Vector3 drawPos = DrawPos;
			if (base.Rotation.IsHorizontal)
			{
				drawPos.z = (float)sleepingSlotPos.z + 0.6f;
			}
			else
			{
				drawPos.x = (float)sleepingSlotPos.x + 0.5f;
				drawPos.z += -0.4f;
			}
			Vector2 v = drawPos.MapToUIPosition();
			if (!base.Rotation.IsHorizontal && SleepingSlotsCount == 2)
			{
				v = AdjustOwnerLabelPosToAvoidOverlapping(v, slotIndex);
			}
			return v;
		}

		private Vector3 AdjustOwnerLabelPosToAvoidOverlapping(Vector3 screenPos, int slotIndex)
		{
			Text.Font = GameFont.Tiny;
			Vector2 vector = Text.CalcSize(owners[slotIndex].LabelShort);
			float num = vector.x + 1f;
			Vector2 vector2 = DrawPos.MapToUIPosition();
			float num2 = Mathf.Abs(screenPos.x - vector2.x);
			IntVec3 sleepingSlotPos = GetSleepingSlotPos(slotIndex);
			if (num > num2 * 2f)
			{
				float num3 = 0f;
				if (slotIndex == 0)
				{
					IntVec3 sleepingSlotPos2 = GetSleepingSlotPos(1);
					num3 = (float)sleepingSlotPos2.x;
				}
				else
				{
					IntVec3 sleepingSlotPos3 = GetSleepingSlotPos(0);
					num3 = (float)sleepingSlotPos3.x;
				}
				if ((float)sleepingSlotPos.x < num3)
				{
					screenPos.x -= (num - num2 * 2f) / 2f;
				}
				else
				{
					screenPos.x += (num - num2 * 2f) / 2f;
				}
			}
			return screenPos;
		}
	}
}
