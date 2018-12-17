using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ShortCircuitUtility
	{
		private static Dictionary<PowerNet, bool> tmpPowerNetHasActivePowerSource = new Dictionary<PowerNet, bool>();

		private static List<IntVec3> tmpCells = new List<IntVec3>();

		public static IEnumerable<Building> GetShortCircuitablePowerConduits(Map map)
		{
			tmpPowerNetHasActivePowerSource.Clear();
			try
			{
				List<Thing> conduits = map.listerThings.ThingsOfDef(ThingDefOf.PowerConduit);
				int i = 0;
				Building b;
				while (true)
				{
					if (i >= conduits.Count)
					{
						yield break;
					}
					b = (Building)conduits[i];
					CompPower power = b.PowerComp;
					if (power != null)
					{
						if (!tmpPowerNetHasActivePowerSource.TryGetValue(power.PowerNet, out bool hasActivePowerSource))
						{
							hasActivePowerSource = power.PowerNet.HasActivePowerSource;
							tmpPowerNetHasActivePowerSource.Add(power.PowerNet, hasActivePowerSource);
						}
						if (hasActivePowerSource)
						{
							break;
						}
					}
					i++;
				}
				yield return b;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			finally
			{
				((_003CGetShortCircuitablePowerConduits_003Ec__Iterator0)/*Error near IL_0150: stateMachine*/)._003C_003E__Finally0();
			}
			IL_0160:
			/*Error near IL_0161: Unexpected return in MoveNext()*/;
		}

		public static void DoShortCircuit(Building culprit)
		{
			PowerNet powerNet = culprit.PowerComp.PowerNet;
			Map map = culprit.Map;
			float totalEnergy = 0f;
			float explosionRadius = 0f;
			bool flag = false;
			if (powerNet.batteryComps.Any((CompPowerBattery x) => x.StoredEnergy > 20f))
			{
				DrainBatteriesAndCauseExplosion(powerNet, culprit, out totalEnergy, out explosionRadius);
			}
			else
			{
				flag = TryStartFireNear(culprit);
			}
			string value = (culprit.def != ThingDefOf.PowerConduit) ? Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(culprit.Label) : "AnElectricalConduit".Translate();
			StringBuilder stringBuilder = new StringBuilder();
			if (flag)
			{
				stringBuilder.Append("ShortCircuitStartedFire".Translate(value));
			}
			else
			{
				stringBuilder.Append("ShortCircuit".Translate(value));
			}
			if (totalEnergy > 0f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("ShortCircuitDischargedEnergy".Translate(totalEnergy.ToString("F0")));
			}
			if (explosionRadius > 5f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("ShortCircuitWasLarge".Translate());
			}
			if (explosionRadius > 8f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("ShortCircuitWasHuge".Translate());
			}
			Find.LetterStack.ReceiveLetter("LetterLabelShortCircuit".Translate(), stringBuilder.ToString(), LetterDefOf.NegativeEvent, new TargetInfo(culprit.Position, map));
		}

		public static bool TryShortCircuitInRain(Thing thing)
		{
			CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
			if ((compPowerTrader != null && compPowerTrader.PowerOn && compPowerTrader.Props.shortCircuitInRain) || (thing.TryGetComp<CompPowerBattery>() != null && thing.TryGetComp<CompPowerBattery>().StoredEnergy > 100f))
			{
				string text = "ShortCircuitRain".Translate(thing.Label, thing);
				TargetInfo target = new TargetInfo(thing.Position, thing.Map);
				if (thing.Faction == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelShortCircuit".Translate(), text, LetterDefOf.NegativeEvent, target);
				}
				else
				{
					Messages.Message(text, target, MessageTypeDefOf.NeutralEvent);
				}
				GenExplosion.DoExplosion(thing.OccupiedRect().RandomCell, thing.Map, 1.9f, DamageDefOf.Flame, null);
				return true;
			}
			return false;
		}

		private static void DrainBatteriesAndCauseExplosion(PowerNet net, Building culprit, out float totalEnergy, out float explosionRadius)
		{
			totalEnergy = 0f;
			for (int i = 0; i < net.batteryComps.Count; i++)
			{
				CompPowerBattery compPowerBattery = net.batteryComps[i];
				totalEnergy += compPowerBattery.StoredEnergy;
				compPowerBattery.DrawPower(compPowerBattery.StoredEnergy);
			}
			explosionRadius = Mathf.Sqrt(totalEnergy) * 0.05f;
			explosionRadius = Mathf.Clamp(explosionRadius, 1.5f, 14.9f);
			GenExplosion.DoExplosion(culprit.Position, net.Map, explosionRadius, DamageDefOf.Flame, null);
			if (explosionRadius > 3.5f)
			{
				GenExplosion.DoExplosion(culprit.Position, net.Map, explosionRadius * 0.3f, DamageDefOf.Bomb, null);
			}
		}

		private static bool TryStartFireNear(Building b)
		{
			tmpCells.Clear();
			int num = GenRadial.NumCellsInRadius(3f);
			CellRect startRect = b.OccupiedRect();
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = b.Position + GenRadial.RadialPattern[i];
				if (GenSight.LineOfSight(b.Position, intVec, b.Map, startRect, CellRect.SingleCell(intVec)) && FireUtility.ChanceToStartFireIn(intVec, b.Map) > 0f)
				{
					tmpCells.Add(intVec);
				}
			}
			if (tmpCells.Any())
			{
				return FireUtility.TryStartFireIn(tmpCells.RandomElement(), b.Map, Rand.Range(0.1f, 1.75f));
			}
			return false;
		}
	}
}
