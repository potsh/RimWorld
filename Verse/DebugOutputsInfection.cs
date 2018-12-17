using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsInfection
	{
		private enum InfectionLuck
		{
			Bad,
			Normal,
			Good
		}

		private struct InfectionSimRow
		{
			public HediffDef illness;

			public int skill;

			public ThingDef medicine;

			public float deathChance;

			public float recoveryTimeDays;

			public float medicineUsed;
		}

		private static List<Pawn> GenerateDoctorArray()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, (Pawn p) => !p.story.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) && p.health.hediffSet.hediffs.Count == 0);
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i <= 20; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				pawn.skills.GetSkill(SkillDefOf.Medicine).Level = i;
				list.Add(pawn);
			}
			return list;
		}

		private static IEnumerable<HediffDef> InfectionList()
		{
			return from hediff in DefDatabase<HediffDef>.AllDefs
			where hediff.tendable && hediff.HasComp(typeof(HediffComp_TendDuration)) && hediff.HasComp(typeof(HediffComp_Immunizable)) && hediff.lethalSeverity > 0f
			select hediff;
		}

		[DebugOutput]
		public static void Infections()
		{
			Func<InfectionLuck, float> ilc = delegate(InfectionLuck il)
			{
				float result = 1f;
				if (il == InfectionLuck.Bad)
				{
					result = 0.8f;
				}
				if (il == InfectionLuck.Good)
				{
					result = 1.2f;
				}
				return result;
			};
			Func<Func<InfectionLuck, float>, string> stringizeWithLuck = (Func<InfectionLuck, float> func) => $"{func(InfectionLuck.Bad):F2} / {func(InfectionLuck.Good):F2}";
			Func<HediffDef, bool> isAnimal = (HediffDef d) => d.defName.Contains("Animal");
			Func<HediffDef, float> baseSeverityIncrease = (HediffDef d) => d.CompProps<HediffCompProperties_Immunizable>().severityPerDayNotImmune;
			Func<HediffDef, InfectionLuck, float> baseImmunityIncrease = (HediffDef d, InfectionLuck il) => d.CompProps<HediffCompProperties_Immunizable>().immunityPerDaySick * ilc(il);
			Func<HediffDef, float, float> tendedSeverityIncrease = (HediffDef d, float tend) => baseSeverityIncrease(d) + d.CompProps<HediffCompProperties_TendDuration>().severityPerDayTended * tend;
			Func<HediffDef, InfectionLuck, bool, float> immunityIncrease = delegate(HediffDef d, InfectionLuck il, bool bedridden)
			{
				float b = (!isAnimal(d)) ? ThingDefOf.Bed.GetStatValueAbstract(StatDefOf.ImmunityGainSpeedFactor) : 1f;
				float num3 = Mathf.Lerp(1f, b, (!bedridden) ? 0.3f : 1f);
				float num4 = num3 * StatDefOf.ImmunityGainSpeed.GetStatPart<StatPart_Resting>().factor;
				return baseImmunityIncrease(d, il) * num4;
			};
			Func<HediffDef, InfectionLuck, float, float> immunityOnLethality = delegate(HediffDef d, InfectionLuck il, float tend)
			{
				if (tendedSeverityIncrease(d, tend) <= 0f)
				{
					return float.PositiveInfinity;
				}
				return d.lethalSeverity / tendedSeverityIncrease(d, tend) * immunityIncrease(d, il, arg3: true);
			};
			List<TableDataGetter<HediffDef>> list = new List<TableDataGetter<HediffDef>>();
			list.Add(new TableDataGetter<HediffDef>("defName", (HediffDef d) => d.defName + ((!d.stages.Any((HediffStage stage) => stage.capMods.Any((PawnCapacityModifier cap) => cap.capacity == PawnCapacityDefOf.BloodFiltration))) ? string.Empty : " (inaccurate)")));
			list.Add(new TableDataGetter<HediffDef>("lethal\nseverity", (HediffDef d) => d.lethalSeverity.ToString("F2")));
			list.Add(new TableDataGetter<HediffDef>("base\nseverity\nincrease", (HediffDef d) => baseSeverityIncrease(d).ToString("F2")));
			list.Add(new TableDataGetter<HediffDef>("base\nimmunity\nincrease", (HediffDef d) => stringizeWithLuck((InfectionLuck il) => baseImmunityIncrease(d, il))));
			List<Pawn> source = GenerateDoctorArray();
			float tendquality;
			for (tendquality = 0f; tendquality <= 1.01f; tendquality += 0.1f)
			{
				tendquality = Mathf.Clamp01(tendquality);
				Pawn arg = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, null) >= Mathf.Clamp01(tendquality - 0.25f));
				Pawn arg2 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineHerbal) >= Mathf.Clamp01(tendquality - 0.25f));
				Pawn arg3 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineIndustrial) >= Mathf.Clamp01(tendquality - 0.25f));
				Pawn arg4 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineUltratech) >= Mathf.Clamp01(tendquality - 0.25f));
				Pawn arg5 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, null) >= tendquality);
				Pawn arg6 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineHerbal) >= tendquality);
				Pawn arg7 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineIndustrial) >= tendquality);
				Pawn arg8 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineUltratech) >= tendquality);
				Pawn arg9 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, null) >= Mathf.Clamp01(tendquality + 0.25f));
				Pawn arg10 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineHerbal) >= Mathf.Clamp01(tendquality + 0.25f));
				Pawn arg11 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineIndustrial) >= Mathf.Clamp01(tendquality + 0.25f));
				Pawn arg12 = source.FirstOrFallback((Pawn doc) => TendUtility.CalculateBaseTendQuality(doc, null, ThingDefOf.MedicineUltratech) >= Mathf.Clamp01(tendquality + 0.25f));
				Func<Pawn, Pawn, Pawn, string> func2 = delegate(Pawn low, Pawn exp, Pawn high)
				{
					string arg13 = (low == null) ? "X" : low.skills.GetSkill(SkillDefOf.Medicine).Level.ToString();
					string arg14 = (exp == null) ? "X" : exp.skills.GetSkill(SkillDefOf.Medicine).Level.ToString();
					string arg15 = (high == null) ? "X" : high.skills.GetSkill(SkillDefOf.Medicine).Level.ToString();
					return $"{arg13}-{arg14}-{arg15}";
				};
				string text = func2(arg, arg5, arg9);
				string text2 = func2(arg2, arg6, arg10);
				string text3 = func2(arg3, arg7, arg11);
				string text4 = func2(arg4, arg8, arg12);
				float tq = tendquality;
				list.Add(new TableDataGetter<HediffDef>($"survival chance at\ntend quality {tq.ToStringPercent()}\n\ndoc skill needed:\nno meds:  {text}\nherbal:  {text2}\nnormal:  {text3}\nglitter:  {text4}", delegate(HediffDef d)
				{
					float num = immunityOnLethality(d, InfectionLuck.Bad, tq);
					float num2 = immunityOnLethality(d, InfectionLuck.Good, tq);
					if (num == float.PositiveInfinity)
					{
						return float.PositiveInfinity.ToString();
					}
					return Mathf.Clamp01((num2 - 1f) / (num2 - num)).ToStringPercent();
				}));
			}
			DebugTables.MakeTablesDialog(InfectionList(), list.ToArray());
		}

		[DebugOutput]
		public static void InfectionSimulator()
		{
			LongEventHandler.QueueLongEvent(InfectionSimulatorWorker(), "Simulating . . .");
		}

		private static IEnumerable InfectionSimulatorWorker()
		{
			int trials = 2;
			List<Pawn> doctors = GenerateDoctorArray();
			List<int> testSkill = new List<int>
			{
				4,
				10,
				16
			};
			List<ThingDef> testMedicine = new List<ThingDef>
			{
				null,
				ThingDefOf.MedicineHerbal,
				ThingDefOf.MedicineIndustrial,
				ThingDefOf.MedicineUltratech
			};
			PawnGenerationRequest pawngen = new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer);
			int originalTicks = Find.TickManager.TicksGame;
			List<InfectionSimRow> results = new List<InfectionSimRow>();
			int totalTests = InfectionList().Count() * testMedicine.Count() * testSkill.Count() * trials;
			int currentTest = 0;
			foreach (HediffDef item in InfectionList())
			{
				foreach (ThingDef item2 in testMedicine)
				{
					foreach (int item3 in testSkill)
					{
						InfectionSimRow result = new InfectionSimRow
						{
							illness = item,
							skill = item3,
							medicine = item2
						};
						Pawn doctor = doctors[item3];
						int i = 0;
						if (i < trials)
						{
							Pawn patient = PawnGenerator.GeneratePawn(pawngen);
							int startTicks = Find.TickManager.TicksGame;
							patient.health.AddHediff(result.illness);
							Hediff activeHediff = patient.health.hediffSet.GetFirstHediffOfDef(result.illness);
							while (!patient.Dead && patient.health.hediffSet.HasHediff(result.illness))
							{
								if (activeHediff.TendableNow())
								{
									activeHediff.Tended(TendUtility.CalculateBaseTendQuality(doctor, patient, item2));
									result.medicineUsed += 1f;
								}
								foreach (Hediff item4 in patient.health.hediffSet.GetHediffsTendable())
								{
									item4.Tended(TendUtility.CalculateBaseTendQuality(doctor, patient, item2));
								}
								Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1);
								patient.health.HealthTick();
								if (Find.TickManager.TicksGame % 900 == 0)
								{
									yield return (object)null;
									/*Error: Unable to find new state assignment for yield return*/;
								}
							}
							if (patient.Dead)
							{
								result.deathChance += 1f;
							}
							else
							{
								result.recoveryTimeDays += (Find.TickManager.TicksGame - startTicks).TicksToDays();
							}
							currentTest++;
							LongEventHandler.SetCurrentEventText($"Simulating ({currentTest}/{totalTests})");
							yield return (object)null;
							/*Error: Unable to find new state assignment for yield return*/;
						}
						result.recoveryTimeDays /= (float)trials - result.deathChance;
						result.deathChance /= (float)trials;
						result.medicineUsed /= (float)trials;
						results.Add(result);
					}
				}
			}
			DebugTables.MakeTablesDialog(results, new TableDataGetter<InfectionSimRow>("defName", (InfectionSimRow isr) => isr.illness.defName), new TableDataGetter<InfectionSimRow>("meds", (InfectionSimRow isr) => (isr.medicine == null) ? "(none)" : isr.medicine.defName), new TableDataGetter<InfectionSimRow>("skill", (InfectionSimRow isr) => isr.skill.ToString()), new TableDataGetter<InfectionSimRow>("death chance", (InfectionSimRow isr) => isr.deathChance.ToStringPercent()), new TableDataGetter<InfectionSimRow>("recovery time (days)", (InfectionSimRow isr) => isr.recoveryTimeDays.ToString("F1")), new TableDataGetter<InfectionSimRow>("medicine used", (InfectionSimRow isr) => isr.medicineUsed.ToString()));
			Find.TickManager.DebugSetTicksGame(originalTicks);
			yield break;
			IL_06f6:
			/*Error near IL_06f7: Unexpected return in MoveNext()*/;
		}
	}
}
