using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeDPS : StatWorker
	{
		public override bool IsDisabledFor(Thing thing)
		{
			return base.IsDisabledFor(thing) || StatDefOf.MeleeHitChance.Worker.IsDisabledFor(thing);
		}

		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			if (req.Thing == null)
			{
				Log.Error("Getting MeleeDPS stat for " + req.Def + " without concrete pawn. This always returns 0.");
			}
			return GetMeleeDamage(req, applyPostProcess) * GetMeleeHitChance(req, applyPostProcess) / GetMeleeCooldown(req, applyPostProcess);
		}

		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("StatsReport_MeleeDPSExplanation".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_MeleeDamage".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
			stringBuilder.AppendLine("  " + GetMeleeDamage(req).ToString("0.##"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_Cooldown".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
			stringBuilder.AppendLine("  " + "StatsReport_CooldownFormat".Translate(GetMeleeCooldown(req).ToString("0.##")));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_MeleeHitChance".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(StatDefOf.MeleeHitChance.Worker.GetExplanationUnfinalized(req, StatDefOf.MeleeHitChance.toStringNumberSense).TrimEndNewlines().Indented());
			stringBuilder.AppendLine();
			stringBuilder.Append(StatDefOf.MeleeHitChance.Worker.GetExplanationFinalizePart(req, StatDefOf.MeleeHitChance.toStringNumberSense, GetMeleeHitChance(req)).Indented());
			return stringBuilder.ToString();
		}

		public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
		{
			return string.Format("{0} ( {1} x {2} / {3} )", value.ToStringByStyle(stat.toStringStyle, numberSense), GetMeleeDamage(optionalReq).ToString("0.##"), StatDefOf.MeleeHitChance.ValueToString(GetMeleeHitChance(optionalReq)), GetMeleeCooldown(optionalReq).ToString("0.##"));
		}

		private float GetMeleeDamage(StatRequest req, bool applyPostProcess = true)
		{
			Pawn pawn = req.Thing as Pawn;
			if (pawn == null)
			{
				return 0f;
			}
			List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList(terrainTools: false);
			if (updatedAvailableVerbsList.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
			{
				if (updatedAvailableVerbsList[i].IsMeleeAttack)
				{
					num += updatedAvailableVerbsList[i].GetSelectionWeight(null);
				}
			}
			if (num == 0f)
			{
				return 0f;
			}
			float num2 = 0f;
			for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
			{
				if (updatedAvailableVerbsList[j].IsMeleeAttack)
				{
					float num3 = num2;
					float num4 = updatedAvailableVerbsList[j].GetSelectionWeight(null) / num;
					VerbEntry verbEntry = updatedAvailableVerbsList[j];
					VerbProperties verbProps = verbEntry.verb.verbProps;
					VerbEntry verbEntry2 = updatedAvailableVerbsList[j];
					num2 = num3 + num4 * verbProps.AdjustedMeleeDamageAmount(verbEntry2.verb, pawn);
				}
			}
			return num2;
		}

		private float GetMeleeHitChance(StatRequest req, bool applyPostProcess = true)
		{
			if (req.HasThing)
			{
				return req.Thing.GetStatValue(StatDefOf.MeleeHitChance, applyPostProcess);
			}
			return req.Def.GetStatValueAbstract(StatDefOf.MeleeHitChance);
		}

		private float GetMeleeCooldown(StatRequest req, bool applyPostProcess = true)
		{
			Pawn pawn = req.Thing as Pawn;
			if (pawn == null)
			{
				return 1f;
			}
			List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList(terrainTools: false);
			if (updatedAvailableVerbsList.Count == 0)
			{
				return 1f;
			}
			float num = 0f;
			for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
			{
				if (updatedAvailableVerbsList[i].IsMeleeAttack)
				{
					num += updatedAvailableVerbsList[i].GetSelectionWeight(null);
				}
			}
			if (num == 0f)
			{
				return 1f;
			}
			float num2 = 0f;
			for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
			{
				if (updatedAvailableVerbsList[j].IsMeleeAttack)
				{
					float num3 = num2;
					float num4 = updatedAvailableVerbsList[j].GetSelectionWeight(null) / num;
					VerbEntry verbEntry = updatedAvailableVerbsList[j];
					VerbProperties verbProps = verbEntry.verb.verbProps;
					VerbEntry verbEntry2 = updatedAvailableVerbsList[j];
					num2 = num3 + num4 * (float)verbProps.AdjustedCooldownTicks(verbEntry2.verb, pawn);
				}
			}
			return num2 / 60f;
		}
	}
}
