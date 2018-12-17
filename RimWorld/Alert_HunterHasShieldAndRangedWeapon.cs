using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_HunterHasShieldAndRangedWeapon : Alert
	{
		private IEnumerable<Pawn> BadHunters
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && WorkGiver_HunterHunt.HasShieldAndRangedWeapon(item))
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00dd:
				/*Error near IL_00de: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_HunterHasShieldAndRangedWeapon()
		{
			defaultLabel = "HunterHasShieldAndRangedWeapon".Translate();
			defaultExplanation = "HunterHasShieldAndRangedWeaponDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BadHunters);
		}
	}
}
