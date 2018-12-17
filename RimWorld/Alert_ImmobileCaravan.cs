using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_ImmobileCaravan : Alert_Critical
	{
		private IEnumerable<Caravan> ImmobileCaravans
		{
			get
			{
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				int i = 0;
				while (true)
				{
					if (i >= caravans.Count)
					{
						yield break;
					}
					if (caravans[i].IsPlayerControlled && caravans[i].ImmobilizedByMass)
					{
						break;
					}
					i++;
				}
				yield return caravans[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public Alert_ImmobileCaravan()
		{
			defaultLabel = "ImmobileCaravan".Translate();
			defaultExplanation = "ImmobileCaravanDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(ImmobileCaravans);
		}
	}
}
