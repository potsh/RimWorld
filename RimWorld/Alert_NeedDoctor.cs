using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_NeedDoctor : Alert
	{
		private IEnumerable<Pawn> Patients
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].IsPlayerHome)
					{
						bool healthyDoc = false;
						foreach (Pawn item in maps[i].mapPawns.FreeColonistsSpawned)
						{
							if (!item.Downed && item.workSettings != null && item.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
							{
								healthyDoc = true;
								break;
							}
						}
						if (!healthyDoc)
						{
							foreach (Pawn item2 in maps[i].mapPawns.FreeColonistsSpawned)
							{
								if ((item2.Downed && (int)item2.needs.food.CurCategory < 0 && item2.InBed()) || HealthAIUtility.ShouldBeTendedNowByPlayer(item2))
								{
									yield return item2;
									/*Error: Unable to find new state assignment for yield return*/;
								}
							}
						}
					}
				}
				yield break;
				IL_0220:
				/*Error near IL_0221: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_NeedDoctor()
		{
			defaultLabel = "NeedDoctor".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn patient in Patients)
			{
				stringBuilder.AppendLine("    " + patient.LabelShort);
			}
			return "NeedDoctorDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			if (Find.AnyPlayerHomeMap == null)
			{
				return false;
			}
			return AlertReport.CulpritsAre(Patients);
		}
	}
}
