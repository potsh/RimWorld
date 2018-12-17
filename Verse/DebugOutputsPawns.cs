using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsPawns
	{
		[CompilerGenerated]
		private static Func<ThingDef, bool> _003C_003Ef__mg_0024cache0;

		[DebugOutput]
		public static void PawnKindsBasics()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Humanlike
			select d into k
			orderby (k.defaultFactionType == null) ? string.Empty : k.defaultFactionType.label, k.combatPower
			select k, new TableDataGetter<PawnKindDef>("defName", (PawnKindDef d) => d.defName), new TableDataGetter<PawnKindDef>("faction", (PawnKindDef d) => (d.defaultFactionType == null) ? string.Empty : d.defaultFactionType.defName), new TableDataGetter<PawnKindDef>("points", (PawnKindDef d) => d.combatPower.ToString("F0")), new TableDataGetter<PawnKindDef>("minAge", (PawnKindDef d) => d.minGenerationAge.ToString("F0")), new TableDataGetter<PawnKindDef>("maxAge", (PawnKindDef d) => (d.maxGenerationAge >= 10000) ? string.Empty : d.maxGenerationAge.ToString("F0")), new TableDataGetter<PawnKindDef>("recruitDiff", (PawnKindDef d) => d.baseRecruitDifficulty.ToStringPercent()), new TableDataGetter<PawnKindDef>("itemQuality", (PawnKindDef d) => d.itemQuality.ToString()), new TableDataGetter<PawnKindDef>("forceNormGearQual", (PawnKindDef d) => d.forceNormalGearQuality.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("weapon$", (PawnKindDef d) => d.weaponMoney.ToString()), new TableDataGetter<PawnKindDef>("apparel$", (PawnKindDef d) => d.apparelMoney.ToString()), new TableDataGetter<PawnKindDef>("techHediffsCh", (PawnKindDef d) => d.techHediffsChance.ToStringPercentEmptyZero()), new TableDataGetter<PawnKindDef>("techHediffs$", (PawnKindDef d) => d.techHediffsMoney.ToString()), new TableDataGetter<PawnKindDef>("gearHealth", (PawnKindDef d) => d.gearHealthRange.ToString()), new TableDataGetter<PawnKindDef>("invNutrition", (PawnKindDef d) => d.invNutrition.ToString()), new TableDataGetter<PawnKindDef>("addictionChance", (PawnKindDef d) => d.chemicalAddictionChance.ToStringPercent()), new TableDataGetter<PawnKindDef>("combatDrugChance", (PawnKindDef d) => (!(d.combatEnhancingDrugsChance > 0f)) ? string.Empty : d.combatEnhancingDrugsChance.ToStringPercent()), new TableDataGetter<PawnKindDef>("combatDrugCount", (PawnKindDef d) => (d.combatEnhancingDrugsCount.max <= 0) ? string.Empty : d.combatEnhancingDrugsCount.ToString()), new TableDataGetter<PawnKindDef>("bsCryptosleepComm", (PawnKindDef d) => d.backstoryCryptosleepCommonality.ToStringPercentEmptyZero()));
		}

		[DebugOutput]
		public static void PawnKindsWeaponUsage()
		{
			List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
			list.Add(new TableDataGetter<PawnKindDef>("defName", (PawnKindDef x) => x.defName));
			list.Add(new TableDataGetter<PawnKindDef>("avg $", (PawnKindDef x) => x.weaponMoney.Average.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("min $", (PawnKindDef x) => x.weaponMoney.min.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("max $", (PawnKindDef x) => x.weaponMoney.max.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("points", (PawnKindDef x) => x.combatPower.ToString()));
			list.AddRange(from w in DefDatabase<ThingDef>.AllDefs
			where w.IsWeapon && !w.weaponTags.NullOrEmpty()
			orderby w.IsMeleeWeapon descending, w.techLevel, w.BaseMarketValue
			select new TableDataGetter<PawnKindDef>(w.label.Shorten() + "\n$" + w.BaseMarketValue.ToString("F0"), delegate(PawnKindDef k)
			{
				if (k.weaponTags != null && w.weaponTags.Any((string z) => k.weaponTags.Contains(z)))
				{
					float num = PawnWeaponGenerator.CheapestNonDerpPriceFor(w);
					if (k.weaponMoney.max < num)
					{
						return "-";
					}
					if (k.weaponMoney.min > num)
					{
						return "✓";
					}
					return (1f - (num - k.weaponMoney.min) / (k.weaponMoney.max - k.weaponMoney.min)).ToStringPercent("F0");
				}
				return string.Empty;
			}));
			DebugTables.MakeTablesDialog(from x in DefDatabase<PawnKindDef>.AllDefs
			where (int)x.RaceProps.intelligence >= 1
			orderby (x.defaultFactionType == null) ? 2147483647 : ((int)x.defaultFactionType.techLevel), x.combatPower
			select x, list.ToArray());
		}

		[DebugOutput]
		public static void PawnKindsApparelUsage()
		{
			List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
			list.Add(new TableDataGetter<PawnKindDef>("defName", (PawnKindDef x) => x.defName));
			list.Add(new TableDataGetter<PawnKindDef>("avg $", (PawnKindDef x) => x.apparelMoney.Average.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("min $", (PawnKindDef x) => x.apparelMoney.min.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("max $", (PawnKindDef x) => x.apparelMoney.max.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("points", (PawnKindDef x) => x.combatPower.ToString()));
			list.AddRange(from a in (from a in DefDatabase<ThingDef>.AllDefs
			where a.IsApparel
			select a).OrderBy(PawnApparelGenerator.IsHeadgear).ThenBy((ThingDef a) => a.BaseMarketValue)
			select new TableDataGetter<PawnKindDef>(a.label.Shorten() + "\n$" + a.BaseMarketValue.ToString("F0"), delegate(PawnKindDef k)
			{
				if (k.apparelRequired != null && k.apparelRequired.Contains(a))
				{
					return "Rq";
				}
				if (k.apparelAllowHeadgearChance <= 0f && PawnApparelGenerator.IsHeadgear(a))
				{
					return "nohat";
				}
				if (k.apparelTags != null && a.apparel.tags.Any((string z) => k.apparelTags.Contains(z)))
				{
					float baseMarketValue = a.BaseMarketValue;
					if (k.apparelMoney.max < baseMarketValue)
					{
						return "-";
					}
					if (k.apparelMoney.min > baseMarketValue)
					{
						return "✓";
					}
					return (1f - (baseMarketValue - k.apparelMoney.min) / (k.apparelMoney.max - k.apparelMoney.min)).ToStringPercent("F0");
				}
				return string.Empty;
			}));
			DebugTables.MakeTablesDialog(from x in DefDatabase<PawnKindDef>.AllDefs
			where x.RaceProps.Humanlike
			orderby (x.defaultFactionType == null) ? 2147483647 : ((int)x.defaultFactionType.techLevel), x.combatPower
			select x, list.ToArray());
		}

		[DebugOutput]
		public static void PawnKindsTechHediffUsage()
		{
			List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
			list.Add(new TableDataGetter<PawnKindDef>("defName", (PawnKindDef x) => x.defName));
			list.Add(new TableDataGetter<PawnKindDef>("chance", (PawnKindDef x) => x.techHediffsChance.ToStringPercent()));
			list.Add(new TableDataGetter<PawnKindDef>("avg $", (PawnKindDef x) => x.techHediffsMoney.Average.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("min $", (PawnKindDef x) => x.techHediffsMoney.min.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("max $", (PawnKindDef x) => x.techHediffsMoney.max.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("points", (PawnKindDef x) => x.combatPower.ToString()));
			list.AddRange(from t in DefDatabase<ThingDef>.AllDefs
			where t.isTechHediff && t.techHediffsTags != null
			orderby t.techLevel descending, t.BaseMarketValue
			select new TableDataGetter<PawnKindDef>(t.label.Shorten() + "\n$" + t.BaseMarketValue.ToString("F0"), delegate(PawnKindDef k)
			{
				if (k.techHediffsTags != null && t.techHediffsTags.Any((string tag) => k.techHediffsTags.Contains(tag)))
				{
					if (k.techHediffsMoney.max < t.BaseMarketValue)
					{
						return "-";
					}
					if (k.techHediffsMoney.min >= t.BaseMarketValue)
					{
						return "✓";
					}
					return (1f - (t.BaseMarketValue - k.techHediffsMoney.min) / (k.techHediffsMoney.max - k.techHediffsMoney.min)).ToStringPercent("F0");
				}
				return string.Empty;
			}));
			DebugTables.MakeTablesDialog(from x in DefDatabase<PawnKindDef>.AllDefs
			where x.RaceProps.Humanlike
			orderby (x.defaultFactionType == null) ? 2147483647 : ((int)x.defaultFactionType.techLevel), x.combatPower
			select x, list.ToArray());
		}

		[DebugOutput]
		public static void PawnKindGearSampled()
		{
			IOrderedEnumerable<PawnKindDef> orderedEnumerable = from k in DefDatabase<PawnKindDef>.AllDefs
			where k.RaceProps.ToolUser
			orderby k.combatPower
			select k;
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PawnKindDef item2 in orderedEnumerable)
			{
				Faction fac = FactionUtility.DefaultFactionFrom(item2.defaultFactionType);
				PawnKindDef kind = item2;
				FloatMenuOption item = new FloatMenuOption(kind.defName + " (" + kind.combatPower + ")", delegate
				{
					DefMap<ThingDef, int> weapons = new DefMap<ThingDef, int>();
					DefMap<ThingDef, int> apparel = new DefMap<ThingDef, int>();
					DefMap<HediffDef, int> hediffs = new DefMap<HediffDef, int>();
					for (int i = 0; i < 400; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(kind, fac);
						if (pawn.equipment.Primary != null)
						{
							DefMap<ThingDef, int> defMap;
							ThingDef def;
							(defMap = weapons)[def = pawn.equipment.Primary.def] = defMap[def] + 1;
						}
						foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
						{
							DefMap<HediffDef, int> defMap2;
							HediffDef def2;
							(defMap2 = hediffs)[def2 = hediff.def] = defMap2[def2] + 1;
						}
						foreach (Apparel item3 in pawn.apparel.WornApparel)
						{
							DefMap<ThingDef, int> defMap;
							ThingDef def3;
							(defMap = apparel)[def3 = item3.def] = defMap[def3] + 1;
						}
						pawn.Destroy();
					}
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Sampled " + 400 + "x " + kind.defName + ":");
					stringBuilder.AppendLine("Weapons");
					foreach (ThingDef item4 in from t in DefDatabase<ThingDef>.AllDefs
					orderby weapons[t] descending
					select t)
					{
						int num = weapons[item4];
						if (num > 0)
						{
							stringBuilder.AppendLine("  " + item4.defName + "    " + ((float)num / 400f).ToStringPercent());
						}
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Apparel");
					foreach (ThingDef item5 in from t in DefDatabase<ThingDef>.AllDefs
					orderby apparel[t] descending
					select t)
					{
						int num2 = apparel[item5];
						if (num2 > 0)
						{
							stringBuilder.AppendLine("  " + item5.defName + "    " + ((float)num2 / 400f).ToStringPercent());
						}
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Tech hediffs");
					foreach (HediffDef item6 in from h in DefDatabase<HediffDef>.AllDefs
					where h.spawnThingOnRemoved != null
					orderby hediffs[h] descending
					select h)
					{
						int num3 = hediffs[item6];
						if (num3 > 0)
						{
							stringBuilder.AppendLine("  " + item6.defName + "    " + ((float)num3 / 400f).ToStringPercent());
						}
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Addiction hediffs");
					foreach (HediffDef item7 in from h in DefDatabase<HediffDef>.AllDefs
					where h.IsAddiction
					orderby hediffs[h] descending
					select h)
					{
						int num4 = hediffs[item7];
						if (num4 > 0)
						{
							stringBuilder.AppendLine("  " + item7.defName + "    " + ((float)num4 / 400f).ToStringPercent());
						}
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Other hediffs");
					foreach (HediffDef item8 in from h in DefDatabase<HediffDef>.AllDefs
					where h.spawnThingOnRemoved == null && !h.IsAddiction
					orderby hediffs[h] descending
					select h)
					{
						int num5 = hediffs[item8];
						if (num5 > 0)
						{
							stringBuilder.AppendLine("  " + item8.defName + "    " + ((float)num5 / 400f).ToStringPercent());
						}
					}
					Log.Message(stringBuilder.ToString().TrimEndNewlines());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void PawnWorkDisablesSampled()
		{
			IOrderedEnumerable<PawnKindDef> orderedEnumerable = from k in DefDatabase<PawnKindDef>.AllDefs
			where k.RaceProps.Humanlike
			orderby k.combatPower
			select k;
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PawnKindDef item2 in orderedEnumerable)
			{
				PawnKindDef kind = item2;
				Faction fac = FactionUtility.DefaultFactionFrom(kind.defaultFactionType);
				FloatMenuOption item = new FloatMenuOption(kind.defName + " (" + kind.combatPower + ")", delegate
				{
					Dictionary<WorkTags, int> dictionary = new Dictionary<WorkTags, int>();
					for (int i = 0; i < 1000; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(kind, fac);
						WorkTags combinedDisabledWorkTags = pawn.story.CombinedDisabledWorkTags;
						IEnumerator enumerator2 = Enum.GetValues(typeof(WorkTags)).GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								WorkTags workTags = (WorkTags)enumerator2.Current;
								if (!dictionary.ContainsKey(workTags))
								{
									dictionary.Add(workTags, 0);
								}
								if ((combinedDisabledWorkTags & workTags) != 0)
								{
									Dictionary<WorkTags, int> dictionary2;
									WorkTags key;
									(dictionary2 = dictionary)[key = workTags] = dictionary2[key] + 1;
								}
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator2 as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
						pawn.Destroy();
					}
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Sampled " + 1000 + "x " + kind.defName + ":");
					stringBuilder.AppendLine("Worktags disabled");
					IEnumerator enumerator3 = Enum.GetValues(typeof(WorkTags)).GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							WorkTags key2 = (WorkTags)enumerator3.Current;
							int num = dictionary[key2];
							stringBuilder.AppendLine("  " + key2.ToString() + "    " + num + " (" + ((float)num / 1000f).ToStringPercent() + ")");
						}
					}
					finally
					{
						IDisposable disposable2;
						if ((disposable2 = (enumerator3 as IDisposable)) != null)
						{
							disposable2.Dispose();
						}
					}
					Log.Message(stringBuilder.ToString().TrimEndNewlines());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void RecruitDifficultiesSampled()
		{
			IOrderedEnumerable<PawnKindDef> orderedEnumerable = from k in DefDatabase<PawnKindDef>.AllDefs
			where k.RaceProps.Humanlike
			orderby k.combatPower
			select k;
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PawnKindDef item2 in orderedEnumerable)
			{
				PawnKindDef kind = item2;
				Faction fac = FactionUtility.DefaultFactionFrom(kind.defaultFactionType);
				if (kind == PawnKindDefOf.WildMan)
				{
					fac = null;
				}
				FloatMenuOption item = new FloatMenuOption(kind.defName + " (" + kind.baseRecruitDifficulty.ToStringPercent() + ")", delegate
				{
					Dictionary<int, int> dictionary = new Dictionary<int, int>();
					for (int i = 0; i < 21; i++)
					{
						dictionary.Add(i, 0);
					}
					for (int j = 0; j < 300; j++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(kind, fac);
						float num = pawn.RecruitDifficulty(Faction.OfPlayer);
						int num2 = Mathf.RoundToInt(num * 20f);
						Dictionary<int, int> dictionary2;
						int key;
						(dictionary2 = dictionary)[key = num2] = dictionary2[key] + 1;
						pawn.Destroy();
					}
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Sampled " + 300 + "x " + kind.defName + ":");
					for (int l = 0; l < 21; l++)
					{
						int num3 = dictionary[l];
						stringBuilder.AppendLine("  " + (l * 5).ToString() + "    " + num3 + " (" + ((float)num3 / 300f).ToStringPercent() + ")");
					}
					Log.Message(stringBuilder.ToString().TrimEndNewlines());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void LivePawnsInspirationChances()
		{
			List<TableDataGetter<Pawn>> list = new List<TableDataGetter<Pawn>>();
			list.Add(new TableDataGetter<Pawn>("name", (Pawn p) => p.Label));
			foreach (InspirationDef allDef in DefDatabase<InspirationDef>.AllDefs)
			{
				list.Add(new TableDataGetter<Pawn>(allDef.defName, (Pawn p) => allDef.Worker.InspirationCanOccur(p) ? allDef.Worker.CommonalityFor(p).ToString() : "-no-"));
			}
			DebugTables.MakeTablesDialog(Find.CurrentMap.mapPawns.FreeColonistsSpawned, list.ToArray());
		}

		[DebugOutput]
		public static void RacesFoodConsumption()
		{
			Func<ThingDef, int, string> lsName = delegate(ThingDef d, int lsIndex)
			{
				if (d.race.lifeStageAges.Count <= lsIndex)
				{
					return string.Empty;
				}
				LifeStageDef def3 = d.race.lifeStageAges[lsIndex].def;
				return def3.defName;
			};
			Func<ThingDef, int, string> maxFood = delegate(ThingDef d, int lsIndex)
			{
				if (d.race.lifeStageAges.Count <= lsIndex)
				{
					return string.Empty;
				}
				LifeStageDef def2 = d.race.lifeStageAges[lsIndex].def;
				return (d.race.baseBodySize * def2.bodySizeFactor * def2.foodMaxFactor).ToString("F2");
			};
			Func<ThingDef, int, string> hungerRate = delegate(ThingDef d, int lsIndex)
			{
				if (d.race.lifeStageAges.Count <= lsIndex)
				{
					return string.Empty;
				}
				LifeStageDef def = d.race.lifeStageAges[lsIndex].def;
				return (d.race.baseHungerRate * def.hungerRateFactor).ToString("F2");
			};
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null && d.race.EatsFood
			orderby d.race.baseHungerRate descending
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("Lifestage 0", (ThingDef d) => lsName(d, 0)), new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 0)), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 0)), new TableDataGetter<ThingDef>("Lifestage 1", (ThingDef d) => lsName(d, 1)), new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 1)), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 1)), new TableDataGetter<ThingDef>("Lifestage 2", (ThingDef d) => lsName(d, 2)), new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 2)), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 2)), new TableDataGetter<ThingDef>("Lifestage 3", (ThingDef d) => lsName(d, 3)), new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 3)), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 3)));
		}

		[DebugOutput]
		public static void RacesButchery()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null
			orderby d.race.baseBodySize
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("mktval", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F0")), new TableDataGetter<ThingDef>("healthScale", (ThingDef d) => d.race.baseHealthScale.ToString("F2")), new TableDataGetter<ThingDef>("hunger rate", (ThingDef d) => d.race.baseHungerRate.ToString("F2")), new TableDataGetter<ThingDef>("wildness", (ThingDef d) => d.race.wildness.ToStringPercent()), new TableDataGetter<ThingDef>("bodySize", (ThingDef d) => d.race.baseBodySize.ToString("F2")), new TableDataGetter<ThingDef>("meatAmount", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MeatAmount).ToString("F0")), new TableDataGetter<ThingDef>("leatherAmount", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.LeatherAmount).ToString("F0")));
		}

		[DebugOutput]
		public static void AnimalsBasics()
		{
			Func<PawnKindDef, float> dps = (PawnKindDef d) => RaceMeleeDpsEstimate(d.race);
			Func<PawnKindDef, float> pointsGuess = delegate(PawnKindDef d)
			{
				float num2 = 15f;
				num2 += dps(d) * 10f;
				num2 *= Mathf.Lerp(1f, d.race.GetStatValueAbstract(StatDefOf.MoveSpeed) / 3f, 0.25f);
				num2 *= d.RaceProps.baseHealthScale;
				num2 *= GenMath.LerpDouble(0.25f, 1f, 1.65f, 1f, Mathf.Clamp(d.RaceProps.baseBodySize, 0.25f, 1f));
				return num2 * 0.76f;
			};
			Func<PawnKindDef, float> mktValGuess = delegate(PawnKindDef d)
			{
				float num = 18f;
				num += pointsGuess(d) * 2.7f;
				if (d.RaceProps.trainability == TrainabilityDefOf.None)
				{
					num *= 0.5f;
				}
				else if (d.RaceProps.trainability == TrainabilityDefOf.Simple)
				{
					num *= 0.8f;
				}
				else if (d.RaceProps.trainability == TrainabilityDefOf.Intermediate)
				{
					num = num;
				}
				else
				{
					if (d.RaceProps.trainability != TrainabilityDefOf.Advanced)
					{
						throw new InvalidOperationException();
					}
					num += 250f;
				}
				num += d.RaceProps.baseBodySize * 80f;
				if (d.race.HasComp(typeof(CompMilkable)))
				{
					num += 125f;
				}
				if (d.race.HasComp(typeof(CompShearable)))
				{
					num += 90f;
				}
				if (d.race.HasComp(typeof(CompEggLayer)))
				{
					num += 90f;
				}
				num *= Mathf.Lerp(0.8f, 1.2f, d.RaceProps.wildness);
				return num * 0.75f;
			};
			DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			select d, new TableDataGetter<PawnKindDef>("defName", (PawnKindDef d) => d.defName), new TableDataGetter<PawnKindDef>("dps", (PawnKindDef d) => dps(d).ToString("F2")), new TableDataGetter<PawnKindDef>("healthScale", (PawnKindDef d) => d.RaceProps.baseHealthScale.ToString("F2")), new TableDataGetter<PawnKindDef>("points", (PawnKindDef d) => d.combatPower.ToString("F0")), new TableDataGetter<PawnKindDef>("points guess", (PawnKindDef d) => pointsGuess(d).ToString("F0")), new TableDataGetter<PawnKindDef>("speed", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.MoveSpeed).ToString("F2")), new TableDataGetter<PawnKindDef>("mktval", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F0")), new TableDataGetter<PawnKindDef>("mktval guess", (PawnKindDef d) => mktValGuess(d).ToString("F0")), new TableDataGetter<PawnKindDef>("bodySize", (PawnKindDef d) => d.RaceProps.baseBodySize.ToString("F2")), new TableDataGetter<PawnKindDef>("hunger", (PawnKindDef d) => d.RaceProps.baseHungerRate.ToString("F2")), new TableDataGetter<PawnKindDef>("wildness", (PawnKindDef d) => d.RaceProps.wildness.ToStringPercent()), new TableDataGetter<PawnKindDef>("lifespan", (PawnKindDef d) => d.RaceProps.lifeExpectancy.ToString("F1")), new TableDataGetter<PawnKindDef>("trainability", (PawnKindDef d) => (d.RaceProps.trainability == null) ? "null" : d.RaceProps.trainability.label), new TableDataGetter<PawnKindDef>("tempMin", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin).ToString("F0")), new TableDataGetter<PawnKindDef>("tempMax", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax).ToString("F0")));
		}

		private static float RaceMeleeDpsEstimate(ThingDef race)
		{
			return race.GetStatValueAbstract(StatDefOf.MeleeDPS);
		}

		[DebugOutput]
		public static void AnimalCombatBalance()
		{
			Func<PawnKindDef, float> meleeDps = delegate(PawnKindDef k)
			{
				Pawn pawn2 = PawnGenerator.GeneratePawn(new PawnGenerationRequest(k, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true));
				while (pawn2.health.hediffSet.hediffs.Count > 0)
				{
					pawn2.health.RemoveHediff(pawn2.health.hediffSet.hediffs[0]);
				}
				float statValue = pawn2.GetStatValue(StatDefOf.MeleeDPS);
				Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
				return statValue;
			};
			Func<PawnKindDef, float> averageArmor = delegate(PawnKindDef k)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(k);
				while (pawn.health.hediffSet.hediffs.Count > 0)
				{
					pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs[0]);
				}
				float result = (pawn.GetStatValue(StatDefOf.ArmorRating_Blunt) + pawn.GetStatValue(StatDefOf.ArmorRating_Sharp)) / 2f;
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return result;
			};
			Func<PawnKindDef, float> combatPowerCalculated = delegate(PawnKindDef k)
			{
				float num = 1f + meleeDps(k) * 2f;
				float num2 = 1f + (k.RaceProps.baseHealthScale + averageArmor(k) * 1.8f) * 2f;
				float num3 = num * num2 * 2.5f + 10f;
				return num3 + k.race.GetStatValueAbstract(StatDefOf.MoveSpeed) * 2f;
			};
			DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			orderby d.combatPower
			select d, new TableDataGetter<PawnKindDef>("animal", (PawnKindDef k) => k.defName), new TableDataGetter<PawnKindDef>("meleeDps", (PawnKindDef k) => meleeDps(k).ToString("F1")), new TableDataGetter<PawnKindDef>("baseHealthScale", (PawnKindDef k) => k.RaceProps.baseHealthScale.ToString()), new TableDataGetter<PawnKindDef>("moveSpeed", (PawnKindDef k) => k.race.GetStatValueAbstract(StatDefOf.MoveSpeed).ToString()), new TableDataGetter<PawnKindDef>("averageArmor", (PawnKindDef k) => averageArmor(k).ToStringPercent()), new TableDataGetter<PawnKindDef>("combatPowerCalculated", (PawnKindDef k) => combatPowerCalculated(k).ToString("F0")), new TableDataGetter<PawnKindDef>("combatPower", (PawnKindDef k) => k.combatPower.ToString()));
		}

		[DebugOutput]
		public static void AnimalTradeTags()
		{
			List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
			list.Add(new TableDataGetter<PawnKindDef>("animal", (PawnKindDef k) => k.defName));
			foreach (string item in (from k in DefDatabase<PawnKindDef>.AllDefs
			where k.race.tradeTags != null
			select k).SelectMany((PawnKindDef k) => k.race.tradeTags).Distinct())
			{
				list.Add(new TableDataGetter<PawnKindDef>(item, (PawnKindDef k) => (k.race.tradeTags != null && k.race.tradeTags.Contains(item)).ToStringCheckBlank()));
			}
			DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			select d, list.ToArray());
		}

		[DebugOutput]
		public static void AnimalBehavior()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			select d, new TableDataGetter<PawnKindDef>(string.Empty, (PawnKindDef k) => k.defName), new TableDataGetter<PawnKindDef>("wildness", (PawnKindDef k) => k.RaceProps.wildness.ToStringPercent()), new TableDataGetter<PawnKindDef>("manhunterOnDamageChance", (PawnKindDef k) => k.RaceProps.manhunterOnDamageChance.ToStringPercentEmptyZero("F1")), new TableDataGetter<PawnKindDef>("manhunterOnTameFailChance", (PawnKindDef k) => k.RaceProps.manhunterOnTameFailChance.ToStringPercentEmptyZero("F1")), new TableDataGetter<PawnKindDef>("predator", (PawnKindDef k) => k.RaceProps.predator.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("bodySize", (PawnKindDef k) => k.RaceProps.baseBodySize.ToString("F2")), new TableDataGetter<PawnKindDef>("maxPreyBodySize", (PawnKindDef k) => (!k.RaceProps.predator) ? string.Empty : k.RaceProps.maxPreyBodySize.ToString("F2")), new TableDataGetter<PawnKindDef>("canBePredatorPrey", (PawnKindDef k) => k.RaceProps.canBePredatorPrey.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("petness", (PawnKindDef k) => k.RaceProps.petness.ToStringPercent()), new TableDataGetter<PawnKindDef>("nuzzleMtbHours", (PawnKindDef k) => (!(k.RaceProps.nuzzleMtbHours > 0f)) ? string.Empty : k.RaceProps.nuzzleMtbHours.ToString()), new TableDataGetter<PawnKindDef>("packAnimal", (PawnKindDef k) => k.RaceProps.packAnimal.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("herdAnimal", (PawnKindDef k) => k.RaceProps.herdAnimal.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("wildGroupSizeMin", (PawnKindDef k) => (k.wildGroupSize.min == 1) ? string.Empty : k.wildGroupSize.min.ToString()), new TableDataGetter<PawnKindDef>("wildGroupSizeMax", (PawnKindDef k) => (k.wildGroupSize.max == 1) ? string.Empty : k.wildGroupSize.max.ToString()), new TableDataGetter<PawnKindDef>("CanDoHerdMigration", (PawnKindDef k) => k.RaceProps.CanDoHerdMigration.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("herdMigrationAllowed", (PawnKindDef k) => k.RaceProps.herdMigrationAllowed.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("mateMtb", (PawnKindDef k) => k.RaceProps.mateMtbHours.ToStringEmptyZero("F0")));
		}

		[DebugOutput]
		public static void AnimalsEcosystem()
		{
			Func<PawnKindDef, float> ecosystemWeightGuess = (PawnKindDef k) => k.RaceProps.baseBodySize * 0.2f + k.RaceProps.baseHungerRate * 0.8f;
			DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			orderby d.ecoSystemWeight descending
			select d, new TableDataGetter<PawnKindDef>("defName", (PawnKindDef d) => d.defName), new TableDataGetter<PawnKindDef>("bodySize", (PawnKindDef d) => d.RaceProps.baseBodySize.ToString("F2")), new TableDataGetter<PawnKindDef>("hunger rate", (PawnKindDef d) => d.RaceProps.baseHungerRate.ToString("F2")), new TableDataGetter<PawnKindDef>("ecosystem weight", (PawnKindDef d) => d.ecoSystemWeight.ToString("F2")), new TableDataGetter<PawnKindDef>("ecosystem weight guess", (PawnKindDef d) => ecosystemWeightGuess(d).ToString("F2")), new TableDataGetter<PawnKindDef>("predator", (PawnKindDef d) => d.RaceProps.predator.ToStringCheckBlank()));
		}
	}
}
