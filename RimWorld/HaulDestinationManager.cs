using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld
{
	public sealed class HaulDestinationManager
	{
		private Map map;

		private List<IHaulDestination> allHaulDestinationsInOrder = new List<IHaulDestination>();

		private List<SlotGroup> allGroupsInOrder = new List<SlotGroup>();

		private SlotGroup[,,] groupGrid;

		[CompilerGenerated]
		private static Comparison<IHaulDestination> _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Comparison<SlotGroup> _003C_003Ef__mg_0024cache1;

		[CompilerGenerated]
		private static Comparison<IHaulDestination> _003C_003Ef__mg_0024cache2;

		[CompilerGenerated]
		private static Comparison<SlotGroup> _003C_003Ef__mg_0024cache3;

		public IEnumerable<IHaulDestination> AllHaulDestinations => allHaulDestinationsInOrder;

		public List<IHaulDestination> AllHaulDestinationsListForReading => allHaulDestinationsInOrder;

		public List<IHaulDestination> AllHaulDestinationsListInPriorityOrder => allHaulDestinationsInOrder;

		public IEnumerable<SlotGroup> AllGroups => allGroupsInOrder;

		public List<SlotGroup> AllGroupsListForReading => allGroupsInOrder;

		public List<SlotGroup> AllGroupsListInPriorityOrder => allGroupsInOrder;

		public IEnumerable<IntVec3> AllSlots
		{
			get
			{
				int j = 0;
				List<IntVec3> cellsList;
				int i;
				while (true)
				{
					if (j >= allGroupsInOrder.Count)
					{
						yield break;
					}
					cellsList = allGroupsInOrder[j].CellsList;
					i = 0;
					if (i < allGroupsInOrder.Count)
					{
						break;
					}
					j++;
				}
				yield return cellsList[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public HaulDestinationManager(Map map)
		{
			this.map = map;
			IntVec3 size = map.Size;
			int x = size.x;
			IntVec3 size2 = map.Size;
			int y = size2.y;
			IntVec3 size3 = map.Size;
			groupGrid = new SlotGroup[x, y, size3.z];
		}

		public void AddHaulDestination(IHaulDestination haulDestination)
		{
			if (allHaulDestinationsInOrder.Contains(haulDestination))
			{
				Log.Error("Double-added haul destination " + haulDestination.ToStringSafe());
			}
			else
			{
				allHaulDestinationsInOrder.Add(haulDestination);
				allHaulDestinationsInOrder.InsertionSort(CompareHaulDestinationPrioritiesDescending);
				ISlotGroupParent slotGroupParent = haulDestination as ISlotGroupParent;
				if (slotGroupParent != null)
				{
					SlotGroup slotGroup = slotGroupParent.GetSlotGroup();
					if (slotGroup == null)
					{
						Log.Error("ISlotGroupParent gave null slot group: " + slotGroupParent.ToStringSafe());
					}
					else
					{
						allGroupsInOrder.Add(slotGroup);
						allGroupsInOrder.InsertionSort(CompareSlotGroupPrioritiesDescending);
						List<IntVec3> cellsList = slotGroup.CellsList;
						for (int i = 0; i < cellsList.Count; i++)
						{
							SetCellFor(cellsList[i], slotGroup);
						}
						map.listerHaulables.Notify_SlotGroupChanged(slotGroup);
						map.listerMergeables.Notify_SlotGroupChanged(slotGroup);
					}
				}
			}
		}

		public void RemoveHaulDestination(IHaulDestination haulDestination)
		{
			if (!allHaulDestinationsInOrder.Contains(haulDestination))
			{
				Log.Error("Removing haul destination that isn't registered " + haulDestination.ToStringSafe());
			}
			else
			{
				allHaulDestinationsInOrder.Remove(haulDestination);
				ISlotGroupParent slotGroupParent = haulDestination as ISlotGroupParent;
				if (slotGroupParent != null)
				{
					SlotGroup slotGroup = slotGroupParent.GetSlotGroup();
					if (slotGroup == null)
					{
						Log.Error("ISlotGroupParent gave null slot group: " + slotGroupParent.ToStringSafe());
					}
					else
					{
						allGroupsInOrder.Remove(slotGroup);
						List<IntVec3> cellsList = slotGroup.CellsList;
						for (int i = 0; i < cellsList.Count; i++)
						{
							IntVec3 intVec = cellsList[i];
							groupGrid[intVec.x, intVec.y, intVec.z] = null;
						}
						map.listerHaulables.Notify_SlotGroupChanged(slotGroup);
						map.listerMergeables.Notify_SlotGroupChanged(slotGroup);
					}
				}
			}
		}

		public void Notify_HaulDestinationChangedPriority()
		{
			allHaulDestinationsInOrder.InsertionSort(CompareHaulDestinationPrioritiesDescending);
			allGroupsInOrder.InsertionSort(CompareSlotGroupPrioritiesDescending);
		}

		private static int CompareHaulDestinationPrioritiesDescending(IHaulDestination a, IHaulDestination b)
		{
			return ((int)b.GetStoreSettings().Priority).CompareTo((int)a.GetStoreSettings().Priority);
		}

		private static int CompareSlotGroupPrioritiesDescending(SlotGroup a, SlotGroup b)
		{
			return ((int)b.Settings.Priority).CompareTo((int)a.Settings.Priority);
		}

		public SlotGroup SlotGroupAt(IntVec3 loc)
		{
			return groupGrid[loc.x, loc.y, loc.z];
		}

		public ISlotGroupParent SlotGroupParentAt(IntVec3 loc)
		{
			return SlotGroupAt(loc)?.parent;
		}

		public void SetCellFor(IntVec3 c, SlotGroup group)
		{
			if (SlotGroupAt(c) != null)
			{
				Log.Error(group + " overwriting slot group square " + c + " of " + SlotGroupAt(c));
			}
			groupGrid[c.x, c.y, c.z] = group;
		}

		public void ClearCellFor(IntVec3 c, SlotGroup group)
		{
			if (SlotGroupAt(c) != group)
			{
				Log.Error(group + " clearing group grid square " + c + " containing " + SlotGroupAt(c));
			}
			groupGrid[c.x, c.y, c.z] = null;
		}
	}
}
