using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SlotGroup
	{
		public ISlotGroupParent parent;

		private Map Map => parent.Map;

		public StorageSettings Settings => parent.GetStoreSettings();

		public IEnumerable<Thing> HeldThings
		{
			get
			{
				List<IntVec3> cellsList = CellsList;
				Map map = Map;
				for (int j = 0; j < cellsList.Count; j++)
				{
					List<Thing> thingList = map.thingGrid.ThingsListAt(cellsList[j]);
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i].def.EverStorable(willMinifyIfPossible: false))
						{
							yield return thingList[i];
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
		}

		public List<IntVec3> CellsList => parent.AllSlotCellsList();

		public SlotGroup(ISlotGroupParent parent)
		{
			this.parent = parent;
		}

		public IEnumerator<IntVec3> GetEnumerator()
		{
			List<IntVec3> cellsList = CellsList;
			int i = 0;
			if (i < cellsList.Count)
			{
				yield return cellsList[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public void Notify_AddedCell(IntVec3 c)
		{
			Map.haulDestinationManager.SetCellFor(c, this);
			Map.listerHaulables.RecalcAllInCell(c);
			Map.listerMergeables.RecalcAllInCell(c);
		}

		public void Notify_LostCell(IntVec3 c)
		{
			Map.haulDestinationManager.ClearCellFor(c, this);
			Map.listerHaulables.RecalcAllInCell(c);
			Map.listerMergeables.RecalcAllInCell(c);
		}

		public override string ToString()
		{
			if (parent != null)
			{
				return parent.ToString();
			}
			return "NullParent";
		}
	}
}
