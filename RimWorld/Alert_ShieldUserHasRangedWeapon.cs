using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_ShieldUserHasRangedWeapon : Alert
	{
		private IEnumerable<Pawn> ShieldUsersWithRangedWeapon
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.equipment.Primary != null && item.equipment.Primary.def.IsRangedWeapon)
					{
						List<Apparel> ap = item.apparel.WornApparel;
						for (int i = 0; i < ap.Count; i++)
						{
							if (ap[i] is ShieldBelt)
							{
								yield return item;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				yield break;
				IL_014d:
				/*Error near IL_014e: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_ShieldUserHasRangedWeapon()
		{
			defaultLabel = "ShieldUserHasRangedWeapon".Translate();
			defaultExplanation = "ShieldUserHasRangedWeaponDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(ShieldUsersWithRangedWeapon);
		}
	}
}
