using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public sealed class OutfitDatabase : IExposable
	{
		private List<Outfit> outfits = new List<Outfit>();

		public List<Outfit> AllOutfits => outfits;

		public OutfitDatabase()
		{
			GenerateStartingOutfits();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref outfits, "outfits", LookMode.Deep);
		}

		public Outfit DefaultOutfit()
		{
			if (outfits.Count == 0)
			{
				MakeNewOutfit();
			}
			return outfits[0];
		}

		public AcceptanceReport TryDelete(Outfit outfit)
		{
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			{
				if (item.outfits != null && item.outfits.CurrentOutfit == outfit)
				{
					return new AcceptanceReport("OutfitInUse".Translate(item));
				}
			}
			foreach (Pawn item2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				if (item2.outfits != null && item2.outfits.CurrentOutfit == outfit)
				{
					item2.outfits.CurrentOutfit = null;
				}
			}
			outfits.Remove(outfit);
			return AcceptanceReport.WasAccepted;
		}

		public Outfit MakeNewOutfit()
		{
			int uniqueId = (!outfits.Any()) ? 1 : (outfits.Max((Outfit o) => o.uniqueId) + 1);
			Outfit outfit = new Outfit(uniqueId, "Outfit".Translate() + " " + uniqueId.ToString());
			outfit.filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
			outfits.Add(outfit);
			return outfit;
		}

		private void GenerateStartingOutfits()
		{
			Outfit outfit = MakeNewOutfit();
			outfit.label = "OutfitAnything".Translate();
			Outfit outfit2 = MakeNewOutfit();
			outfit2.label = "OutfitWorker".Translate();
			outfit2.filter.SetDisallowAll();
			outfit2.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, allow: false);
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.apparel != null && allDef.apparel.defaultOutfitTags != null && allDef.apparel.defaultOutfitTags.Contains("Worker"))
				{
					outfit2.filter.SetAllow(allDef, allow: true);
				}
			}
			Outfit outfit3 = MakeNewOutfit();
			outfit3.label = "OutfitSoldier".Translate();
			outfit3.filter.SetDisallowAll();
			outfit3.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, allow: false);
			foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef2.apparel != null && allDef2.apparel.defaultOutfitTags != null && allDef2.apparel.defaultOutfitTags.Contains("Soldier"))
				{
					outfit3.filter.SetAllow(allDef2, allow: true);
				}
			}
			Outfit outfit4 = MakeNewOutfit();
			outfit4.label = "OutfitNudist".Translate();
			outfit4.filter.SetDisallowAll();
			outfit4.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, allow: false);
			foreach (ThingDef allDef3 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef3.apparel != null && !allDef3.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !allDef3.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
				{
					outfit4.filter.SetAllow(allDef3, allow: true);
				}
			}
		}
	}
}
