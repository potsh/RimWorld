using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_BuildSandbags : Lesson_Instruction
	{
		private List<IntVec3> sandbagCells;

		protected override float ProgressPercent
		{
			get
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 sandbagCell in sandbagCells)
				{
					if (TutorUtility.BuildingOrBlueprintOrFrameCenterExists(sandbagCell, base.Map, ThingDefOf.Sandbags))
					{
						num2++;
					}
					num++;
				}
				return (float)num2 / (float)num;
			}
		}

		public override void OnActivated()
		{
			base.OnActivated();
			Find.TutorialState.sandbagsRect = TutorUtility.FindUsableRect(7, 7, base.Map);
			sandbagCells = new List<IntVec3>();
			foreach (IntVec3 edgeCell in Find.TutorialState.sandbagsRect.EdgeCells)
			{
				IntVec3 current = edgeCell;
				int x = current.x;
				IntVec3 centerCell = Find.TutorialState.sandbagsRect.CenterCell;
				if (x != centerCell.x)
				{
					int z = current.z;
					IntVec3 centerCell2 = Find.TutorialState.sandbagsRect.CenterCell;
					if (z != centerCell2.z)
					{
						sandbagCells.Add(current);
					}
				}
			}
			foreach (IntVec3 item in Find.TutorialState.sandbagsRect.ContractedBy(1))
			{
				if (!Find.TutorialState.sandbagsRect.ContractedBy(2).Contains(item))
				{
					List<Thing> thingList = item.GetThingList(base.Map);
					for (int num = thingList.Count - 1; num >= 0; num--)
					{
						Thing thing = thingList[num];
						if (thing.def.passability != 0 && (thing.def.category == ThingCategory.Plant || thing.def.category == ThingCategory.Item))
						{
							thing.Destroy();
						}
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref sandbagCells, "sandbagCells", LookMode.Undefined);
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawLabelOnGUI(Gen.AveragePosition(sandbagCells), def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			List<IntVec3> cells = (from c in sandbagCells
			where !TutorUtility.BuildingOrBlueprintOrFrameCenterExists(c, base.Map, ThingDefOf.Sandbags)
			select c).ToList();
			GenDraw.DrawFieldEdges(cells);
			GenDraw.DrawArrowPointingAt(Gen.AveragePosition(sandbagCells));
			if (ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-Sandbags")
			{
				return TutorUtility.EventCellsAreWithin(ep, sandbagCells);
			}
			return base.AllowAction(ep);
		}
	}
}
