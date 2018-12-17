using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlantsCut : Designator_Plants
	{
		public Designator_PlantsCut()
		{
			defaultLabel = "DesignatorCutPlants".Translate();
			defaultDesc = "DesignatorCutPlantsDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/CutPlants");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_CutPlants;
			hotKey = KeyBindingDefOf.Misc3;
			designationDef = DesignationDefOf.CutPlant;
		}

		public override Texture2D IconReverseDesignating(Thing t, out float angle, out Vector2 offset)
		{
			if (!t.def.plant.IsTree)
			{
				return base.IconReverseDesignating(t, out angle, out offset);
			}
			angle = 0f;
			offset = default(Vector2);
			return TexCommand.TreeChop;
		}

		public override string LabelCapReverseDesignating(Thing t)
		{
			if (!t.def.plant.IsTree)
			{
				return base.LabelCapReverseDesignating(t);
			}
			return "DesignatorHarvestWood".Translate();
		}

		public override string DescReverseDesignating(Thing t)
		{
			if (!t.def.plant.IsTree)
			{
				return base.DescReverseDesignating(t);
			}
			return "DesignatorHarvestWoodDesc".Translate();
		}
	}
}
