using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_ZoneAddStockpile_Dumping : Designator_ZoneAddStockpile
	{
		public Designator_ZoneAddStockpile_Dumping()
		{
			preset = StorageSettingsPreset.DumpingStockpile;
			defaultLabel = preset.PresetName();
			defaultDesc = "DesignatorZoneCreateStorageDumpingDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile");
			hotKey = KeyBindingDefOf.Misc3;
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.StorageTab, OpportunityType.GoodToKnow);
		}
	}
}
