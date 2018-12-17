using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Building_Storage : Building, ISlotGroupParent, IStoreSettingsParent, IHaulDestination
	{
		public StorageSettings settings;

		public SlotGroup slotGroup;

		private List<IntVec3> cachedOccupiedCells;

		public bool StorageTabVisible => true;

		public bool IgnoreStoredThingsBeauty => def.building.ignoreStoredThingsBeauty;

		public Building_Storage()
		{
			slotGroup = new SlotGroup(this);
		}

		public SlotGroup GetSlotGroup()
		{
			return slotGroup;
		}

		public virtual void Notify_ReceivedThing(Thing newItem)
		{
			if (base.Faction == Faction.OfPlayer && newItem.def.storedConceptLearnOpportunity != null)
			{
				LessonAutoActivator.TeachOpportunity(newItem.def.storedConceptLearnOpportunity, OpportunityType.GoodToKnow);
			}
		}

		public virtual void Notify_LostThing(Thing newItem)
		{
		}

		public virtual IEnumerable<IntVec3> AllSlotCells()
		{
			using (IEnumerator<IntVec3> enumerator = GenAdj.CellsOccupiedBy(this).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					IntVec3 c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00b9:
			/*Error near IL_00ba: Unexpected return in MoveNext()*/;
		}

		public List<IntVec3> AllSlotCellsList()
		{
			if (cachedOccupiedCells == null)
			{
				cachedOccupiedCells = AllSlotCells().ToList();
			}
			return cachedOccupiedCells;
		}

		public StorageSettings GetStoreSettings()
		{
			return settings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return def.building.fixedStorageSettings;
		}

		public string SlotYielderLabel()
		{
			return LabelCap;
		}

		public bool Accepts(Thing t)
		{
			return settings.AllowedToAccept(t);
		}

		public override void PostMake()
		{
			base.PostMake();
			settings = new StorageSettings(this);
			if (def.building.defaultStorageSettings != null)
			{
				settings.CopyFrom(def.building.defaultStorageSettings);
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			cachedOccupiedCells = null;
			base.SpawnSetup(map, respawningAfterLoad);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref settings, "settings", this);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g2 = enumerator.Current;
					yield return g2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Gizmo> enumerator2 = StorageSettingsClipboard.CopyPasteGizmosFor(settings).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Gizmo g = enumerator2.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_014f:
			/*Error near IL_0150: Unexpected return in MoveNext()*/;
		}
	}
}
