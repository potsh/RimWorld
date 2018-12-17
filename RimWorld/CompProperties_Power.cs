using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Power : CompProperties
	{
		public bool transmitsPower;

		public float basePowerConsumption;

		public bool shortCircuitInRain;

		public SoundDef soundPowerOn;

		public SoundDef soundPowerOff;

		public SoundDef soundAmbientPowered;

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			using (IEnumerator<StatDrawEntry> enumerator = base.SpecialDisplayStats(req).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					StatDrawEntry s = enumerator.Current;
					yield return s;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (basePowerConsumption > 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Building, "PowerConsumption".Translate(), basePowerConsumption.ToString("F0") + " W", 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_012b:
			/*Error near IL_012c: Unexpected return in MoveNext()*/;
		}
	}
}
