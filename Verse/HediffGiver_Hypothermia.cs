using RimWorld;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class HediffGiver_Hypothermia : HediffGiver
	{
		public HediffDef hediffInsectoid;

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			float ambientTemperature = pawn.AmbientTemperature;
			FloatRange floatRange = pawn.ComfortableTemperatureRange();
			FloatRange floatRange2 = pawn.SafeTemperatureRange();
			HediffSet hediffSet = pawn.health.hediffSet;
			HediffDef hediffDef = (pawn.RaceProps.FleshType != FleshTypeDefOf.Insectoid) ? hediff : hediffInsectoid;
			Hediff firstHediffOfDef = hediffSet.GetFirstHediffOfDef(hediffDef);
			if (ambientTemperature < floatRange2.min)
			{
				float num = Mathf.Abs(ambientTemperature - floatRange2.min);
				float a = num * 6.45E-05f;
				a = Mathf.Max(a, 0.00075f);
				HealthUtility.AdjustSeverity(pawn, hediffDef, a);
				if (pawn.Dead)
				{
					return;
				}
			}
			if (firstHediffOfDef != null)
			{
				if (ambientTemperature > floatRange.min)
				{
					float value = firstHediffOfDef.Severity * 0.027f;
					value = Mathf.Clamp(value, 0.0015f, 0.015f);
					firstHediffOfDef.Severity -= value;
				}
				else if (pawn.RaceProps.FleshType != FleshTypeDefOf.Insectoid && ambientTemperature < 0f && firstHediffOfDef.Severity > 0.37f)
				{
					float num2 = 0.025f * firstHediffOfDef.Severity;
					if (Rand.Value < num2 && (from x in pawn.RaceProps.body.AllPartsVulnerableToFrostbite
					where !hediffSet.PartIsMissing(x)
					select x).TryRandomElementByWeight((BodyPartRecord x) => x.def.frostbiteVulnerability, out BodyPartRecord result))
					{
						int num3 = Mathf.CeilToInt((float)result.def.hitPoints * 0.5f);
						DamageDef frostbite = DamageDefOf.Frostbite;
						float amount = (float)num3;
						BodyPartRecord hitPart = result;
						DamageInfo dinfo = new DamageInfo(frostbite, amount, 0f, -1f, null, hitPart);
						pawn.TakeDamage(dinfo);
					}
				}
			}
		}
	}
}
