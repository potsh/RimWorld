using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_HunterLacksRangedWeapon : Alert
	{
		private IEnumerable<Pawn> HuntersWithoutRangedWeapon
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && !WorkGiver_HunterHunt.HasHuntingWeapon(item) && !item.Downed)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00ed:
				/*Error near IL_00ee: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_HunterLacksRangedWeapon()
		{
			defaultLabel = "HunterLacksWeapon".Translate();
			defaultExplanation = "HunterLacksWeaponDesc".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(HuntersWithoutRangedWeapon);
		}
	}
}
