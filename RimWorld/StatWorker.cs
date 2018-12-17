using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatWorker
	{
		protected StatDef stat;

		public void InitSetStat(StatDef newStat)
		{
			stat = newStat;
		}

		public float GetValue(Thing thing, bool applyPostProcess = true)
		{
			return GetValue(StatRequest.For(thing));
		}

		public float GetValue(StatRequest req, bool applyPostProcess = true)
		{
			if (stat.minifiedThingInherits)
			{
				MinifiedThing minifiedThing = req.Thing as MinifiedThing;
				if (minifiedThing != null)
				{
					if (minifiedThing.InnerThing == null)
					{
						Log.Error("MinifiedThing's inner thing is null.");
					}
					return minifiedThing.InnerThing.GetStatValue(stat, applyPostProcess);
				}
			}
			float val = GetValueUnfinalized(req, applyPostProcess);
			FinalizeValue(req, ref val, applyPostProcess);
			return val;
		}

		public float GetValueAbstract(BuildableDef def, ThingDef stuffDef = null)
		{
			return GetValue(StatRequest.For(def, stuffDef));
		}

		public virtual float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			if (Prefs.DevMode && IsDisabledFor(req.Thing))
			{
				Log.ErrorOnce($"Attempted to calculate value for disabled stat {stat}; this is meant as a consistency check, either set the stat to neverDisabled or ensure this pawn cannot accidentally use this stat (thing={req.Thing.ToStringSafe()})", 75193282 + stat.index);
			}
			float num = GetBaseValueFor(req.Def);
			Pawn pawn = req.Thing as Pawn;
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedOffsets != null)
					{
						for (int i = 0; i < stat.skillNeedOffsets.Count; i++)
						{
							num += stat.skillNeedOffsets[i].ValueFor(pawn);
						}
					}
				}
				else
				{
					num += stat.noSkillOffset;
				}
				if (stat.capacityOffsets != null)
				{
					for (int j = 0; j < stat.capacityOffsets.Count; j++)
					{
						PawnCapacityOffset pawnCapacityOffset = stat.capacityOffsets[j];
						num += pawnCapacityOffset.GetOffset(pawn.health.capacities.GetLevel(pawnCapacityOffset.capacity));
					}
				}
				if (pawn.story != null)
				{
					for (int k = 0; k < pawn.story.traits.allTraits.Count; k++)
					{
						num += pawn.story.traits.allTraits[k].OffsetOfStat(stat);
					}
				}
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int l = 0; l < hediffs.Count; l++)
				{
					HediffStage curStage = hediffs[l].CurStage;
					if (curStage != null)
					{
						num += curStage.statOffsets.GetStatOffsetFromList(stat);
					}
				}
				if (pawn.apparel != null)
				{
					for (int m = 0; m < pawn.apparel.WornApparel.Count; m++)
					{
						num += StatOffsetFromGear(pawn.apparel.WornApparel[m], stat);
					}
				}
				if (pawn.equipment != null && pawn.equipment.Primary != null)
				{
					num += StatOffsetFromGear(pawn.equipment.Primary, stat);
				}
				if (pawn.story != null)
				{
					for (int n = 0; n < pawn.story.traits.allTraits.Count; n++)
					{
						num *= pawn.story.traits.allTraits[n].MultiplierOfStat(stat);
					}
				}
				num *= pawn.ageTracker.CurLifeStage.statFactors.GetStatFactorFromList(stat);
			}
			if (req.StuffDef != null)
			{
				if (num > 0f || stat.applyFactorsIfNegative)
				{
					num *= req.StuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
				}
				num += req.StuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
			}
			if (req.HasThing)
			{
				CompAffectedByFacilities compAffectedByFacilities = req.Thing.TryGetComp<CompAffectedByFacilities>();
				if (compAffectedByFacilities != null)
				{
					num += compAffectedByFacilities.GetStatOffset(stat);
				}
				if (stat.statFactors != null)
				{
					for (int num2 = 0; num2 < stat.statFactors.Count; num2++)
					{
						num *= req.Thing.GetStatValue(stat.statFactors[num2]);
					}
				}
				if (pawn != null)
				{
					if (pawn.skills != null)
					{
						if (stat.skillNeedFactors != null)
						{
							for (int num3 = 0; num3 < stat.skillNeedFactors.Count; num3++)
							{
								num *= stat.skillNeedFactors[num3].ValueFor(pawn);
							}
						}
					}
					else
					{
						num *= stat.noSkillFactor;
					}
					if (stat.capacityFactors != null)
					{
						for (int num4 = 0; num4 < stat.capacityFactors.Count; num4++)
						{
							PawnCapacityFactor pawnCapacityFactor = stat.capacityFactors[num4];
							float factor = pawnCapacityFactor.GetFactor(pawn.health.capacities.GetLevel(pawnCapacityFactor.capacity));
							num = Mathf.Lerp(num, num * factor, pawnCapacityFactor.weight);
						}
					}
					if (pawn.Inspired)
					{
						num += pawn.InspirationDef.statOffsets.GetStatOffsetFromList(stat);
						num *= pawn.InspirationDef.statFactors.GetStatFactorFromList(stat);
					}
				}
			}
			return num;
		}

		public virtual string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			float baseValueFor = GetBaseValueFor(req.Def);
			if (baseValueFor != 0f)
			{
				stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(baseValueFor, numberSense));
				stringBuilder.AppendLine();
			}
			Pawn pawn = req.Thing as Pawn;
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedOffsets != null)
					{
						stringBuilder.AppendLine("StatsReport_Skills".Translate());
						for (int i = 0; i < stat.skillNeedOffsets.Count; i++)
						{
							SkillNeed skillNeed = stat.skillNeedOffsets[i];
							int level = pawn.skills.GetSkill(skillNeed.skill).Level;
							float val = skillNeed.ValueFor(pawn);
							stringBuilder.AppendLine("    " + skillNeed.skill.LabelCap + " (" + level + "): " + val.ToStringSign() + ValueToString(val, finalized: false));
						}
						stringBuilder.AppendLine();
					}
				}
				else if (stat.noSkillOffset != 0f)
				{
					stringBuilder.AppendLine("StatsReport_Skills".Translate());
					stringBuilder.AppendLine("    " + "default".Translate().CapitalizeFirst() + " : " + stat.noSkillOffset.ToStringSign() + ValueToString(stat.noSkillOffset, finalized: false));
					stringBuilder.AppendLine();
				}
				if (stat.capacityOffsets != null)
				{
					stringBuilder.AppendLine((!"StatsReport_Health".CanTranslate()) ? "StatsReport_HealthFactors".Translate() : "StatsReport_Health".Translate());
					foreach (PawnCapacityOffset item in from hfa in stat.capacityOffsets
					orderby hfa.capacity.listOrder
					select hfa)
					{
						string text = item.capacity.GetLabelFor(pawn).CapitalizeFirst();
						float level2 = pawn.health.capacities.GetLevel(item.capacity);
						float offset = item.GetOffset(pawn.health.capacities.GetLevel(item.capacity));
						string text2 = ValueToString(offset, finalized: false);
						string text3 = Mathf.Min(level2, item.max).ToStringPercent() + ", " + "HealthOffsetScale".Translate(item.scale.ToString() + "x");
						if (item.max < 999f)
						{
							text3 = text3 + ", " + "HealthFactorMaxImpact".Translate(item.max.ToStringPercent());
						}
						stringBuilder.AppendLine("    " + text + ": " + offset.ToStringSign() + text2 + " (" + text3 + ")");
					}
					stringBuilder.AppendLine();
				}
				if ((int)pawn.RaceProps.intelligence >= 1)
				{
					if (pawn.story != null && pawn.story.traits != null)
					{
						List<Trait> list = (from tr in pawn.story.traits.allTraits
						where tr.CurrentData.statOffsets != null && tr.CurrentData.statOffsets.Any((StatModifier se) => se.stat == stat)
						select tr).ToList();
						List<Trait> list2 = (from tr in pawn.story.traits.allTraits
						where tr.CurrentData.statFactors != null && tr.CurrentData.statFactors.Any((StatModifier se) => se.stat == stat)
						select tr).ToList();
						if (list.Count > 0 || list2.Count > 0)
						{
							stringBuilder.AppendLine("StatsReport_RelevantTraits".Translate());
							for (int j = 0; j < list.Count; j++)
							{
								Trait trait = list[j];
								string valueToStringAsOffset = trait.CurrentData.statOffsets.First((StatModifier se) => se.stat == stat).ValueToStringAsOffset;
								stringBuilder.AppendLine("    " + trait.LabelCap + ": " + valueToStringAsOffset);
							}
							for (int k = 0; k < list2.Count; k++)
							{
								Trait trait2 = list2[k];
								string toStringAsFactor = trait2.CurrentData.statFactors.First((StatModifier se) => se.stat == stat).ToStringAsFactor;
								stringBuilder.AppendLine("    " + trait2.LabelCap + ": " + toStringAsFactor);
							}
							stringBuilder.AppendLine();
						}
					}
					if (RelevantGear(pawn, stat).Any())
					{
						stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
						if (pawn.apparel != null)
						{
							for (int l = 0; l < pawn.apparel.WornApparel.Count; l++)
							{
								Apparel gear = pawn.apparel.WornApparel[l];
								stringBuilder.AppendLine(InfoTextLineFromGear(gear, stat));
							}
						}
						if (pawn.equipment != null && pawn.equipment.Primary != null)
						{
							stringBuilder.AppendLine(InfoTextLineFromGear(pawn.equipment.Primary, stat));
						}
						stringBuilder.AppendLine();
					}
				}
				bool flag = false;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int m = 0; m < hediffs.Count; m++)
				{
					HediffStage curStage = hediffs[m].CurStage;
					if (curStage != null)
					{
						float statOffsetFromList = curStage.statOffsets.GetStatOffsetFromList(stat);
						if (statOffsetFromList != 0f)
						{
							if (!flag)
							{
								stringBuilder.AppendLine("StatsReport_RelevantHediffs".Translate());
								flag = true;
							}
							stringBuilder.AppendLine("    " + hediffs[m].LabelBase.CapitalizeFirst() + ": " + ValueToString(statOffsetFromList, finalized: false, ToStringNumberSense.Offset));
							stringBuilder.AppendLine();
						}
					}
				}
				float statFactorFromList = pawn.ageTracker.CurLifeStage.statFactors.GetStatFactorFromList(stat);
				if (statFactorFromList != 1f)
				{
					stringBuilder.AppendLine("StatsReport_LifeStage".Translate() + " (" + pawn.ageTracker.CurLifeStage.label + "): " + statFactorFromList.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
					stringBuilder.AppendLine();
				}
			}
			if (req.StuffDef != null)
			{
				if (baseValueFor > 0f || stat.applyFactorsIfNegative)
				{
					float statFactorFromList2 = req.StuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
					if (statFactorFromList2 != 1f)
					{
						stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + req.StuffDef.LabelCap + "): " + statFactorFromList2.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
						stringBuilder.AppendLine();
					}
				}
				float statOffsetFromList2 = req.StuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
				if (statOffsetFromList2 != 0f)
				{
					stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + req.StuffDef.LabelCap + "): " + statOffsetFromList2.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
					stringBuilder.AppendLine();
				}
			}
			req.Thing.TryGetComp<CompAffectedByFacilities>()?.GetStatsExplanation(stat, stringBuilder);
			if (stat.statFactors != null)
			{
				stringBuilder.AppendLine("StatsReport_OtherStats".Translate());
				for (int n = 0; n < stat.statFactors.Count; n++)
				{
					StatDef statDef = stat.statFactors[n];
					stringBuilder.AppendLine("    " + statDef.LabelCap + ": x" + statDef.Worker.GetValue(req).ToStringPercent());
				}
				stringBuilder.AppendLine();
			}
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedFactors != null)
					{
						stringBuilder.AppendLine("StatsReport_Skills".Translate());
						for (int num = 0; num < stat.skillNeedFactors.Count; num++)
						{
							SkillNeed skillNeed2 = stat.skillNeedFactors[num];
							int level3 = pawn.skills.GetSkill(skillNeed2.skill).Level;
							stringBuilder.AppendLine("    " + skillNeed2.skill.LabelCap + " (" + level3 + "): x" + skillNeed2.ValueFor(pawn).ToStringPercent());
						}
						stringBuilder.AppendLine();
					}
				}
				else if (stat.noSkillFactor != 1f)
				{
					stringBuilder.AppendLine("StatsReport_Skills".Translate());
					stringBuilder.AppendLine("    " + "default".Translate().CapitalizeFirst() + " : x" + stat.noSkillFactor.ToStringPercent());
					stringBuilder.AppendLine();
				}
				if (stat.capacityFactors != null)
				{
					stringBuilder.AppendLine((!"StatsReport_Health".CanTranslate()) ? "StatsReport_HealthFactors".Translate() : "StatsReport_Health".Translate());
					if (stat.capacityFactors != null)
					{
						foreach (PawnCapacityFactor item2 in from hfa in stat.capacityFactors
						orderby hfa.capacity.listOrder
						select hfa)
						{
							string text4 = item2.capacity.GetLabelFor(pawn).CapitalizeFirst();
							float factor = item2.GetFactor(pawn.health.capacities.GetLevel(item2.capacity));
							string text5 = factor.ToStringPercent();
							string text6 = "HealthFactorPercentImpact".Translate(item2.weight.ToStringPercent());
							if (item2.max < 999f)
							{
								text6 = text6 + ", " + "HealthFactorMaxImpact".Translate(item2.max.ToStringPercent());
							}
							if (item2.allowedDefect != 0f)
							{
								text6 = text6 + ", " + "HealthFactorAllowedDefect".Translate((1f - item2.allowedDefect).ToStringPercent());
							}
							stringBuilder.AppendLine("    " + text4 + ": x" + text5 + " (" + text6 + ")");
						}
					}
					stringBuilder.AppendLine();
				}
				if (pawn.Inspired)
				{
					float statOffsetFromList3 = pawn.InspirationDef.statOffsets.GetStatOffsetFromList(stat);
					if (statOffsetFromList3 != 0f)
					{
						stringBuilder.AppendLine("StatsReport_Inspiration".Translate(pawn.Inspiration.def.LabelCap) + ": " + ValueToString(statOffsetFromList3, finalized: false, ToStringNumberSense.Offset));
						stringBuilder.AppendLine();
					}
					float statFactorFromList3 = pawn.InspirationDef.statFactors.GetStatFactorFromList(stat);
					if (statFactorFromList3 != 1f)
					{
						stringBuilder.AppendLine("StatsReport_Inspiration".Translate(pawn.Inspiration.def.LabelCap) + ": " + statFactorFromList3.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
						stringBuilder.AppendLine();
					}
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public virtual void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
		{
			if (stat.parts != null)
			{
				for (int i = 0; i < stat.parts.Count; i++)
				{
					stat.parts[i].TransformValue(req, ref val);
				}
			}
			if (applyPostProcess && stat.postProcessCurve != null)
			{
				val = stat.postProcessCurve.Evaluate(val);
			}
			if (Find.Scenario != null)
			{
				val *= Find.Scenario.GetStatFactor(stat);
			}
			if (Mathf.Abs(val) > stat.roundToFiveOver)
			{
				val = Mathf.Round(val / 5f) * 5f;
			}
			if (stat.roundValue)
			{
				val = (float)Mathf.RoundToInt(val);
			}
			val = Mathf.Clamp(val, stat.minValue, stat.maxValue);
		}

		public virtual string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (stat.parts != null)
			{
				for (int i = 0; i < stat.parts.Count; i++)
				{
					string text = stat.parts[i].ExplanationPart(req);
					if (!text.NullOrEmpty())
					{
						stringBuilder.AppendLine(text);
						stringBuilder.AppendLine();
					}
				}
			}
			if (stat.postProcessCurve != null)
			{
				float value = GetValue(req, applyPostProcess: false);
				float value2 = GetValue(req);
				if (!Mathf.Approximately(value, value2))
				{
					string text2 = ValueToString(value, finalized: false);
					string text3 = stat.ValueToString(value2, numberSense);
					stringBuilder.AppendLine("StatsReport_PostProcessed".Translate() + ": " + text2 + " => " + text3);
					stringBuilder.AppendLine();
				}
			}
			float statFactor = Find.Scenario.GetStatFactor(stat);
			if (statFactor != 1f)
			{
				stringBuilder.AppendLine("StatsReport_ScenarioFactor".Translate() + ": " + statFactor.ToStringPercent());
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(finalVal, stat.toStringNumberSense));
			return stringBuilder.ToString();
		}

		public string GetExplanationFull(StatRequest req, ToStringNumberSense numberSense, float value)
		{
			if (IsDisabledFor(req.Thing))
			{
				return "StatsReport_PermanentlyDisabled".Translate();
			}
			string text = stat.Worker.GetExplanationUnfinalized(req, numberSense).TrimEndNewlines();
			if (!text.NullOrEmpty())
			{
				text += "\n\n";
			}
			return text + stat.Worker.GetExplanationFinalizePart(req, numberSense, value);
		}

		public virtual bool ShouldShowFor(StatRequest req)
		{
			if (stat.alwaysHide)
			{
				return false;
			}
			BuildableDef def = req.Def;
			if (!stat.showIfUndefined && !def.statBases.StatListContains(stat))
			{
				return false;
			}
			ThingDef thingDef = def as ThingDef;
			if (thingDef != null && thingDef.category == ThingCategory.Pawn)
			{
				if (!stat.showOnPawns)
				{
					return false;
				}
				if (!stat.showOnHumanlikes && thingDef.race.Humanlike)
				{
					return false;
				}
				if (!stat.showOnNonWildManHumanlikes && thingDef.race.Humanlike && !((req.Thing as Pawn)?.IsWildMan() ?? false))
				{
					return false;
				}
				if (!stat.showOnAnimals && thingDef.race.Animal)
				{
					return false;
				}
				if (!stat.showOnMechanoids && thingDef.race.IsMechanoid)
				{
					return false;
				}
			}
			if (stat.category == StatCategoryDefOf.BasicsPawn || stat.category == StatCategoryDefOf.PawnCombat)
			{
				return thingDef != null && thingDef.category == ThingCategory.Pawn;
			}
			if (stat.category == StatCategoryDefOf.PawnMisc || stat.category == StatCategoryDefOf.PawnSocial || stat.category == StatCategoryDefOf.PawnWork)
			{
				return thingDef != null && thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike;
			}
			if (stat.category == StatCategoryDefOf.Building)
			{
				if (thingDef == null)
				{
					return false;
				}
				if (stat == StatDefOf.DoorOpenSpeed)
				{
					return thingDef.IsDoor;
				}
				if (!stat.showOnNonWorkTables && !thingDef.IsWorkTable)
				{
					return false;
				}
				return thingDef.category == ThingCategory.Building;
			}
			if (stat.category == StatCategoryDefOf.Apparel)
			{
				return thingDef != null && (thingDef.IsApparel || thingDef.category == ThingCategory.Pawn);
			}
			if (stat.category == StatCategoryDefOf.Weapon)
			{
				return thingDef != null && (thingDef.IsMeleeWeapon || thingDef.IsRangedWeapon);
			}
			if (stat.category == StatCategoryDefOf.BasicsNonPawn)
			{
				return thingDef == null || thingDef.category != ThingCategory.Pawn;
			}
			if (stat.category.displayAllByDefault)
			{
				return true;
			}
			Log.Error("Unhandled case: " + stat + ", " + def);
			return false;
		}

		public virtual bool IsDisabledFor(Thing thing)
		{
			if (stat.neverDisabled || (stat.skillNeedFactors.NullOrEmpty() && stat.skillNeedOffsets.NullOrEmpty()))
			{
				return false;
			}
			Pawn pawn = thing as Pawn;
			if (pawn != null && pawn.story != null)
			{
				if (stat.skillNeedFactors != null)
				{
					for (int i = 0; i < stat.skillNeedFactors.Count; i++)
					{
						if (pawn.skills.GetSkill(stat.skillNeedFactors[i].skill).TotallyDisabled)
						{
							return true;
						}
					}
				}
				if (stat.skillNeedOffsets != null)
				{
					for (int j = 0; j < stat.skillNeedOffsets.Count; j++)
					{
						if (pawn.skills.GetSkill(stat.skillNeedOffsets[j].skill).TotallyDisabled)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public virtual string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
		{
			return stat.ValueToString(value, numberSense);
		}

		private static string InfoTextLineFromGear(Thing gear, StatDef stat)
		{
			float f = StatOffsetFromGear(gear, stat);
			return "    " + gear.LabelCap + ": " + f.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset);
		}

		private static float StatOffsetFromGear(Thing gear, StatDef stat)
		{
			return gear.def.equippedStatOffsets.GetStatOffsetFromList(stat);
		}

		private static IEnumerable<Thing> RelevantGear(Pawn pawn, StatDef stat)
		{
			if (pawn.apparel != null)
			{
				foreach (Apparel item in pawn.apparel.WornApparel)
				{
					if (GearAffectsStat(item.def, stat))
					{
						yield return (Thing)item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (pawn.equipment != null)
			{
				foreach (ThingWithComps item2 in pawn.equipment.AllEquipmentListForReading)
				{
					if (GearAffectsStat(item2.def, stat))
					{
						yield return (Thing)item2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01a0:
			/*Error near IL_01a1: Unexpected return in MoveNext()*/;
		}

		private static bool GearAffectsStat(ThingDef gearDef, StatDef stat)
		{
			if (gearDef.equippedStatOffsets != null)
			{
				for (int i = 0; i < gearDef.equippedStatOffsets.Count; i++)
				{
					if (gearDef.equippedStatOffsets[i].stat == stat && gearDef.equippedStatOffsets[i].value != 0f)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected float GetBaseValueFor(BuildableDef def)
		{
			float result = stat.defaultBaseValue;
			if (def.statBases != null)
			{
				for (int i = 0; i < def.statBases.Count; i++)
				{
					if (def.statBases[i].stat == stat)
					{
						result = def.statBases[i].value;
						break;
					}
				}
			}
			return result;
		}

		public string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
		{
			if (!finalized)
			{
				return val.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense);
			}
			string text = val.ToStringByStyle(stat.toStringStyle, numberSense);
			if (numberSense != ToStringNumberSense.Factor && !stat.formatString.NullOrEmpty())
			{
				text = string.Format(stat.formatString, text);
			}
			return text;
		}
	}
}
