using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class Pawn_EquipmentTracker : IThingHolder, IExposable
	{
		public Pawn pawn;

		private ThingOwner<ThingWithComps> equipment;

		public ThingWithComps Primary
		{
			get
			{
				for (int i = 0; i < equipment.Count; i++)
				{
					if (equipment[i].def.equipmentType == EquipmentType.Primary)
					{
						return equipment[i];
					}
				}
				return null;
			}
			private set
			{
				if (Primary != value)
				{
					if (value != null && value.def.equipmentType != EquipmentType.Primary)
					{
						Log.Error("Tried to set non-primary equipment as primary.");
					}
					else
					{
						if (Primary != null)
						{
							equipment.Remove(Primary);
						}
						if (value != null)
						{
							equipment.TryAdd(value);
						}
						if (pawn.drafter != null)
						{
							pawn.drafter.Notify_PrimaryWeaponChanged();
						}
					}
				}
			}
		}

		public CompEquippable PrimaryEq => (Primary == null) ? null : Primary.GetComp<CompEquippable>();

		public List<ThingWithComps> AllEquipmentListForReading => equipment.InnerListForReading;

		public IEnumerable<Verb> AllEquipmentVerbs
		{
			get
			{
				List<ThingWithComps> list = AllEquipmentListForReading;
				int j = 0;
				List<Verb> verbs;
				int i;
				while (true)
				{
					if (j >= list.Count)
					{
						yield break;
					}
					ThingWithComps eq = list[j];
					verbs = eq.GetComp<CompEquippable>().AllVerbs;
					i = 0;
					if (i < verbs.Count)
					{
						break;
					}
					j++;
				}
				yield return verbs[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public IThingHolder ParentHolder => pawn;

		public Pawn_EquipmentTracker(Pawn newPawn)
		{
			pawn = newPawn;
			equipment = new ThingOwner<ThingWithComps>(this);
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref equipment, "equipment", this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				List<ThingWithComps> allEquipmentListForReading = AllEquipmentListForReading;
				for (int i = 0; i < allEquipmentListForReading.Count; i++)
				{
					ThingWithComps thingWithComps = allEquipmentListForReading[i];
					foreach (Verb allVerb in thingWithComps.GetComp<CompEquippable>().AllVerbs)
					{
						allVerb.caster = pawn;
					}
				}
			}
		}

		public void EquipmentTrackerTick()
		{
			List<ThingWithComps> allEquipmentListForReading = AllEquipmentListForReading;
			for (int i = 0; i < allEquipmentListForReading.Count; i++)
			{
				ThingWithComps thingWithComps = allEquipmentListForReading[i];
				thingWithComps.GetComp<CompEquippable>().verbTracker.VerbsTick();
			}
		}

		public bool HasAnything()
		{
			return equipment.Any;
		}

		public void MakeRoomFor(ThingWithComps eq)
		{
			if (eq.def.equipmentType == EquipmentType.Primary && Primary != null)
			{
				if (TryDropEquipment(Primary, out ThingWithComps resultingEq, pawn.Position))
				{
					resultingEq?.SetForbidden(value: false);
				}
				else
				{
					Log.Error(pawn + " couldn't make room for equipment " + eq);
				}
			}
		}

		public void Remove(ThingWithComps eq)
		{
			equipment.Remove(eq);
		}

		public bool TryDropEquipment(ThingWithComps eq, out ThingWithComps resultingEq, IntVec3 pos, bool forbid = true)
		{
			if (!pos.IsValid)
			{
				Log.Error(pawn + " tried to drop " + eq + " at invalid cell.");
				resultingEq = null;
				return false;
			}
			if (equipment.TryDrop(eq, pos, pawn.MapHeld, ThingPlaceMode.Near, out resultingEq))
			{
				if (resultingEq != null)
				{
					resultingEq.SetForbidden(forbid, warnOnFail: false);
				}
				return true;
			}
			return false;
		}

		public void DropAllEquipment(IntVec3 pos, bool forbid = true)
		{
			for (int num = equipment.Count - 1; num >= 0; num--)
			{
				TryDropEquipment(equipment[num], out ThingWithComps _, pos, forbid);
			}
		}

		public bool TryTransferEquipmentToContainer(ThingWithComps eq, ThingOwner container)
		{
			return equipment.TryTransferToContainer(eq, container);
		}

		public void DestroyEquipment(ThingWithComps eq)
		{
			if (!equipment.Contains(eq))
			{
				Log.Warning("Tried to destroy equipment " + eq + " but it's not here.");
			}
			else
			{
				Remove(eq);
				eq.Destroy();
			}
		}

		public void DestroyAllEquipment(DestroyMode mode = DestroyMode.Vanish)
		{
			equipment.ClearAndDestroyContents(mode);
		}

		public bool Contains(Thing eq)
		{
			return equipment.Contains(eq);
		}

		internal void Notify_PrimaryDestroyed()
		{
			if (Primary != null)
			{
				Remove(Primary);
			}
			if (pawn.Spawned)
			{
				pawn.stances.CancelBusyStanceSoft();
			}
		}

		public void AddEquipment(ThingWithComps newEq)
		{
			if (newEq.def.equipmentType == EquipmentType.Primary && Primary != null)
			{
				Log.Error("Pawn " + pawn.LabelCap + " got primaryInt equipment " + newEq + " while already having primaryInt equipment " + Primary);
			}
			else
			{
				equipment.TryAdd(newEq);
			}
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			if (PawnAttackGizmoUtility.CanShowEquipmentGizmos())
			{
				List<ThingWithComps> list = AllEquipmentListForReading;
				for (int i = 0; i < list.Count; i++)
				{
					ThingWithComps eq = list[i];
					using (IEnumerator<Command> enumerator = eq.GetComp<CompEquippable>().GetVerbsCommands().GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							Command command = enumerator.Current;
							switch (i)
							{
							case 0:
								command.hotKey = KeyBindingDefOf.Misc1;
								break;
							case 1:
								command.hotKey = KeyBindingDefOf.Misc2;
								break;
							case 2:
								command.hotKey = KeyBindingDefOf.Misc3;
								break;
							}
							yield return (Gizmo)command;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_0187:
			/*Error near IL_0188: Unexpected return in MoveNext()*/;
		}

		public void Notify_EquipmentAdded(ThingWithComps eq)
		{
			foreach (Verb allVerb in eq.GetComp<CompEquippable>().AllVerbs)
			{
				allVerb.caster = pawn;
				allVerb.Notify_PickedUp();
			}
		}

		public void Notify_EquipmentRemoved(ThingWithComps eq)
		{
			eq.GetComp<CompEquippable>().Notify_EquipmentLost();
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return equipment;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}
	}
}
