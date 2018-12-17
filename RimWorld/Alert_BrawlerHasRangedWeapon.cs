using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_BrawlerHasRangedWeapon : Alert
	{
		private IEnumerable<Pawn> BrawlersWithRangedWeapon
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.story.traits.HasTrait(TraitDefOf.Brawler) && item.equipment.Primary != null && item.equipment.Primary.def.IsRangedWeapon)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_0106:
				/*Error near IL_0107: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_BrawlerHasRangedWeapon()
		{
			defaultLabel = "BrawlerHasRangedWeapon".Translate();
			defaultExplanation = "BrawlerHasRangedWeaponDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BrawlersWithRangedWeapon);
		}
	}
}
