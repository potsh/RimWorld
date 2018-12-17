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
	public static class DebugOutputsMisc
	{
		[CompilerGenerated]
		private static Func<float, string> _003C_003Ef__mg_0024cache0;

		[DebugOutput]
		public static void MiningResourceGeneration()
		{
			Func<ThingDef, ThingDef> mineable = delegate(ThingDef d)
			{
				List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (allDefsListForReading[i].building != null && allDefsListForReading[i].building.mineableThing == d)
					{
						return allDefsListForReading[i];
					}
				}
				return null;
			};
			Func<ThingDef, float> mineableCommonality = delegate(ThingDef d)
			{
				if (mineable(d) != null)
				{
					return mineable(d).building.mineableScatterCommonality;
				}
				return 0f;
			};
			Func<ThingDef, IntRange> mineableLumpSizeRange = delegate(ThingDef d)
			{
				if (mineable(d) != null)
				{
					return mineable(d).building.mineableScatterLumpSizeRange;
				}
				return IntRange.zero;
			};
			Func<ThingDef, float> mineableYield = delegate(ThingDef d)
			{
				if (mineable(d) != null)
				{
					return (float)mineable(d).building.mineableYield;
				}
				return 0f;
			};
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.deepCommonality > 0f || mineableCommonality(d) > 0f
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("market value", (ThingDef d) => d.BaseMarketValue.ToString("F2")), new TableDataGetter<ThingDef>("stackLimit", (ThingDef d) => d.stackLimit), new TableDataGetter<ThingDef>("deep\ncommonality", (ThingDef d) => d.deepCommonality.ToString("F2")), new TableDataGetter<ThingDef>("deep\nlump size", (ThingDef d) => d.deepLumpSizeRange), new TableDataGetter<ThingDef>("deep count\nper cell", (ThingDef d) => d.deepCountPerCell), new TableDataGetter<ThingDef>("deep count\nper portion", (ThingDef d) => d.deepCountPerPortion), new TableDataGetter<ThingDef>("deep portion value", (ThingDef d) => ((float)d.deepCountPerPortion * d.BaseMarketValue).ToStringMoney()), new TableDataGetter<ThingDef>("mineable\ncommonality", (ThingDef d) => mineableCommonality(d).ToString("F2")), new TableDataGetter<ThingDef>("mineable\nlump size", (ThingDef d) => mineableLumpSizeRange(d)), new TableDataGetter<ThingDef>("mineable yield\nper cell", (ThingDef d) => mineableYield(d)));
		}

		[DebugOutput]
		public static void DefaultStuffs()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.MadeFromStuff && !d.IsFrame
			select d, new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("default stuff", (ThingDef d) => GenStuff.DefaultStuffFor(d).defName), new TableDataGetter<ThingDef>("stuff categories", (ThingDef d) => d.stuffCategories.Select((StuffCategoryDef c) => c.defName).ToCommaList()));
		}

		[DebugOutput]
		public static void Beauties()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>()).Where(delegate(BuildableDef d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef != null)
				{
					return BeautyUtility.BeautyRelevant(thingDef.category);
				}
				return d is TerrainDef;
			})
			orderby (int)d.GetStatValueAbstract(StatDefOf.Beauty) descending
			select d, new TableDataGetter<BuildableDef>("category", (BuildableDef d) => (!(d is ThingDef)) ? "Terrain" : ((ThingDef)d).category.ToString()), new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName), new TableDataGetter<BuildableDef>("beauty", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.Beauty).ToString()), new TableDataGetter<BuildableDef>("market value", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F1")), new TableDataGetter<BuildableDef>("work to produce", (BuildableDef d) => DebugOutputsEconomy.WorkToProduceBest(d).ToString()), new TableDataGetter<BuildableDef>("beauty per market value", (BuildableDef d) => (!(d.GetStatValueAbstract(StatDefOf.Beauty) > 0f)) ? string.Empty : (d.GetStatValueAbstract(StatDefOf.Beauty) / d.GetStatValueAbstract(StatDefOf.MarketValue)).ToString("F5")));
		}

		[DebugOutput]
		public static void ThingsPowerAndHeat()
		{
			Func<ThingDef, CompProperties_HeatPusher> heatPusher = delegate(ThingDef d)
			{
				if (d.comps == null)
				{
					return null;
				}
				for (int i = 0; i < d.comps.Count; i++)
				{
					CompProperties_HeatPusher compProperties_HeatPusher = d.comps[i] as CompProperties_HeatPusher;
					if (compProperties_HeatPusher != null)
					{
						return compProperties_HeatPusher;
					}
				}
				return null;
			};
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Building || d.GetCompProperties<CompProperties_Power>() != null || heatPusher(d) != null) && !d.IsFrame
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("base\npower consumption", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() != null) ? d.GetCompProperties<CompProperties_Power>().basePowerConsumption.ToString() : string.Empty), new TableDataGetter<ThingDef>("short circuit\nin rain", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() == null) ? string.Empty : ((!d.GetCompProperties<CompProperties_Power>().shortCircuitInRain) ? string.Empty : "rainfire")), new TableDataGetter<ThingDef>("transmits\npower", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() == null) ? string.Empty : ((!d.GetCompProperties<CompProperties_Power>().transmitsPower) ? string.Empty : "transmit")), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue), new TableDataGetter<ThingDef>("cost list", (ThingDef d) => DebugOutputsEconomy.CostListString(d, divideByVolume: true, starIfOnlyBuyable: false)), new TableDataGetter<ThingDef>("heat pusher\ncompClass", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).compClass.ToString() : string.Empty), new TableDataGetter<ThingDef>("heat pusher\nheat per sec", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPerSecond.ToString() : string.Empty), new TableDataGetter<ThingDef>("heat pusher\nmin temp", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPushMinTemperature.ToStringTemperature() : string.Empty), new TableDataGetter<ThingDef>("heat pusher\nmax temp", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPushMaxTemperature.ToStringTemperature() : string.Empty));
		}

		[DebugOutput]
		public static void FoodPoisonChances()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.IsIngestible
			select d, new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("food poison chance", delegate(ThingDef d)
			{
				CompProperties_FoodPoisonable compProperties = d.GetCompProperties<CompProperties_FoodPoisonable>();
				if (compProperties != null)
				{
					return "poisonable by cook";
				}
				float statValueAbstract = d.GetStatValueAbstract(StatDefOf.FoodPoisonChanceFixedHuman);
				if (statValueAbstract != 0f)
				{
					return statValueAbstract.ToStringPercent();
				}
				return string.Empty;
			}));
		}

		[DebugOutput]
		public static void TechLevels()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Building || d.category == ThingCategory.Item
			where !d.IsFrame && (d.building == null || !d.building.isNaturalRock)
			orderby (int)d.techLevel descending
			select d, new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("tech level", (ThingDef d) => d.techLevel.ToString()));
		}

		[DebugOutput]
		public static void Stuffs()
		{
			Func<ThingDef, StatDef, string> getStatFactorString = delegate(ThingDef d, StatDef stat)
			{
				if (d.stuffProps.statFactors == null)
				{
					return string.Empty;
				}
				StatModifier statModifier = d.stuffProps.statFactors.FirstOrDefault((StatModifier fa) => fa.stat == stat);
				if (statModifier == null)
				{
					return string.Empty;
				}
				return stat.ValueToString(statModifier.value);
			};
			Func<ThingDef, float> meleeDpsSharpFactorOverall = delegate(ThingDef d)
			{
				float statValueAbstract2 = d.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier);
				float statFactorFromList2 = d.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier);
				return statValueAbstract2 / statFactorFromList2;
			};
			Func<ThingDef, float> meleeDpsBluntFactorOverall = delegate(ThingDef d)
			{
				float statValueAbstract = d.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier);
				float statFactorFromList = d.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier);
				return statValueAbstract / statFactorFromList;
			};
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.IsStuff
			orderby d.BaseMarketValue
			select d, new TableDataGetter<ThingDef>("fabric", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Fabric).ToStringCheckBlank()), new TableDataGetter<ThingDef>("leather", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Leathery).ToStringCheckBlank()), new TableDataGetter<ThingDef>("metal", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Metallic).ToStringCheckBlank()), new TableDataGetter<ThingDef>("stony", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Stony).ToStringCheckBlank()), new TableDataGetter<ThingDef>("woody", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Woody).ToStringCheckBlank()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("base\nmarket\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney()), new TableDataGetter<ThingDef>("melee\ncooldown\nmultiplier", (ThingDef d) => getStatFactorString(d, StatDefOf.MeleeWeapon_CooldownMultiplier)), new TableDataGetter<ThingDef>("melee\nsharp\ndamage\nmultiplier", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier).ToString("F2")), new TableDataGetter<ThingDef>("melee\nsharp\ndps factor\noverall", (ThingDef d) => meleeDpsSharpFactorOverall(d).ToString("F2")), new TableDataGetter<ThingDef>("melee\nblunt\ndamage\nmultiplier", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier).ToString("F2")), new TableDataGetter<ThingDef>("melee\nblunt\ndps factor\noverall", (ThingDef d) => meleeDpsBluntFactorOverall(d).ToString("F2")), new TableDataGetter<ThingDef>("armor power\nsharp", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Sharp).ToString("F2")), new TableDataGetter<ThingDef>("armor power\nblunt", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Blunt).ToString("F2")), new TableDataGetter<ThingDef>("armor power\nheat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Heat).ToString("F2")), new TableDataGetter<ThingDef>("insulation\ncold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Cold).ToString("F2")), new TableDataGetter<ThingDef>("insulation\nheat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Heat).ToString("F2")), new TableDataGetter<ThingDef>("flammability", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Flammability).ToString("F2")), new TableDataGetter<ThingDef>("factor\nFlammability", (ThingDef d) => getStatFactorString(d, StatDefOf.Flammability)), new TableDataGetter<ThingDef>("factor\nWorkToMake", (ThingDef d) => getStatFactorString(d, StatDefOf.WorkToMake)), new TableDataGetter<ThingDef>("factor\nWorkToBuild", (ThingDef d) => getStatFactorString(d, StatDefOf.WorkToBuild)), new TableDataGetter<ThingDef>("factor\nMaxHp", (ThingDef d) => getStatFactorString(d, StatDefOf.MaxHitPoints)), new TableDataGetter<ThingDef>("factor\nBeauty", (ThingDef d) => getStatFactorString(d, StatDefOf.Beauty)), new TableDataGetter<ThingDef>("factor\nDoorspeed", (ThingDef d) => getStatFactorString(d, StatDefOf.DoorOpenSpeed)));
		}

		[DebugOutput]
		public static void Drugs()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.IsDrug
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("pleasure", (ThingDef d) => (!d.IsPleasureDrug) ? string.Empty : "pleasure"), new TableDataGetter<ThingDef>("non-medical", (ThingDef d) => (!d.IsNonMedicalDrug) ? string.Empty : "non-medical"));
		}

		[DebugOutput]
		public static void Medicines()
		{
			List<float> list = new List<float>();
			list.Add(0.3f);
			list.AddRange(from d in DefDatabase<ThingDef>.AllDefs
			where typeof(Medicine).IsAssignableFrom(d.thingClass)
			select d.GetStatValueAbstract(StatDefOf.MedicalPotency));
			SkillNeed_Direct skillNeed_Direct = (SkillNeed_Direct)StatDefOf.MedicalTendQuality.skillNeedFactors[0];
			TableDataGetter<float>[] array = new TableDataGetter<float>[21]
			{
				new TableDataGetter<float>("potency", (Func<float, string>)GenText.ToStringPercent),
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			for (int i = 0; i < 20; i++)
			{
				float factor = skillNeed_Direct.valuesPerLevel[i];
				array[i + 1] = new TableDataGetter<float>((i + 1).ToString(), (float p) => (p * factor).ToStringPercent());
			}
			DebugTables.MakeTablesDialog(list, array);
		}

		[DebugOutput]
		public static void ShootingAccuracy()
		{
			StatDef stat = StatDefOf.ShootingAccuracyPawn;
			Func<int, float, int, float> accAtDistance = delegate(int level, float dist, int traitDegree)
			{
				float num = 1f;
				if (traitDegree != 0)
				{
					float value = TraitDef.Named("ShootingAccuracy").DataAtDegree(traitDegree).statOffsets.First((StatModifier so) => so.stat == stat).value;
					num += value;
				}
				foreach (SkillNeed skillNeedFactor in stat.skillNeedFactors)
				{
					SkillNeed_Direct skillNeed_Direct = skillNeedFactor as SkillNeed_Direct;
					num *= skillNeed_Direct.valuesPerLevel[level];
				}
				num = stat.postProcessCurve.Evaluate(num);
				return Mathf.Pow(num, dist);
			};
			List<int> list = new List<int>();
			for (int i = 0; i <= 20; i++)
			{
				list.Add(i);
			}
			DebugTables.MakeTablesDialog(list, new TableDataGetter<int>("No trait skill", (int lev) => lev.ToString()), new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, 0).ToStringPercent("F2")), new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, 0).ToStringPercent("F2")), new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, 0).ToStringPercent("F2")), new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, 0).ToStringPercent("F2")), new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, 0).ToStringPercent("F2")), new TableDataGetter<int>("Careful shooter skill", (int lev) => lev.ToString()), new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, 1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, 1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, 1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, 1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, 1).ToStringPercent("F2")), new TableDataGetter<int>("Trigger-happy skill", (int lev) => lev.ToString()), new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, -1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, -1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, -1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, -1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, -1).ToStringPercent("F2")));
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		public static void TemperatureData()
		{
			Find.CurrentMap.mapTemperature.DebugLogTemps();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		public static void WeatherChances()
		{
			Find.CurrentMap.weatherDecider.LogWeatherChances();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		public static void CelestialGlow()
		{
			GenCelestial.LogSunGlowForYear();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		public static void SunAngle()
		{
			GenCelestial.LogSunAngleForYear();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		public static void FallColor()
		{
			PlantUtility.LogFallColorForYear();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		public static void PawnsListAllOnMap()
		{
			Find.CurrentMap.mapPawns.LogListedPawns();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		public static void WindSpeeds()
		{
			Find.CurrentMap.windManager.LogWindSpeeds();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		public static void MapPawnsList()
		{
			Find.CurrentMap.mapPawns.LogListedPawns();
		}

		[DebugOutput]
		public static void Lords()
		{
			Find.CurrentMap.lordManager.LogLords();
		}

		[DebugOutput]
		public static void DamageTest()
		{
			ThingDef thingDef = ThingDef.Named("Bullet_BoltActionRifle");
			PawnKindDef slave = PawnKindDefOf.Slave;
			Faction faction = FactionUtility.DefaultFactionFrom(slave.defaultFactionType);
			DamageInfo dinfo = new DamageInfo(thingDef.projectile.damageDef, (float)thingDef.projectile.GetDamageAmount(null), thingDef.projectile.GetArmorPenetration(null));
			int num = 0;
			int num2 = 0;
			DefMap<BodyPartDef, int> defMap = new DefMap<BodyPartDef, int>();
			for (int i = 0; i < 500; i++)
			{
				PawnGenerationRequest request = new PawnGenerationRequest(slave, faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true);
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				List<BodyPartDef> list = (from hd in pawn.health.hediffSet.GetMissingPartsCommonAncestors()
				select hd.Part.def).ToList();
				for (int j = 0; j < 2; j++)
				{
					pawn.TakeDamage(dinfo);
					if (pawn.Dead)
					{
						num++;
						break;
					}
				}
				List<BodyPartDef> list2 = (from hd in pawn.health.hediffSet.GetMissingPartsCommonAncestors()
				select hd.Part.def).ToList();
				if (list2.Count > list.Count)
				{
					num2++;
					foreach (BodyPartDef item in list2)
					{
						DefMap<BodyPartDef, int> defMap2;
						BodyPartDef def;
						(defMap2 = defMap)[def = item] = defMap2[def] + 1;
					}
					foreach (BodyPartDef item2 in list)
					{
						DefMap<BodyPartDef, int> defMap2;
						BodyPartDef def2;
						(defMap2 = defMap)[def2 = item2] = defMap2[def2] - 1;
					}
				}
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Damage test");
			stringBuilder.AppendLine("Hit " + 500 + " " + slave.label + "s with " + 2 + "x " + thingDef.label + " (" + thingDef.projectile.GetDamageAmount(null) + " damage) each. Results:");
			stringBuilder.AppendLine("Killed: " + num + " / " + 500 + " (" + ((float)num / 500f).ToStringPercent() + ")");
			stringBuilder.AppendLine("Part losers: " + num2 + " / " + 500 + " (" + ((float)num2 / 500f).ToStringPercent() + ")");
			stringBuilder.AppendLine("Parts lost:");
			foreach (BodyPartDef allDef in DefDatabase<BodyPartDef>.AllDefs)
			{
				if (defMap[allDef] > 0)
				{
					stringBuilder.AppendLine("   " + allDef.label + ": " + defMap[allDef]);
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void BodyPartTagGroups()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (BodyDef allDef in DefDatabase<BodyDef>.AllDefs)
			{
				BodyDef localBd = allDef;
				FloatMenuOption item = new FloatMenuOption(localBd.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(localBd.defName + "\n----------------");
					foreach (BodyPartTagDef item2 in (from elem in localBd.AllParts.SelectMany((BodyPartRecord part) => part.def.tags)
					orderby elem
					select elem).Distinct())
					{
						stringBuilder.AppendLine(item2.defName);
						foreach (BodyPartRecord item3 in from part in localBd.AllParts
						where part.def.tags.Contains(item2)
						orderby part.def.defName
						select part)
						{
							stringBuilder.AppendLine("  " + item3.def.defName);
						}
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void MinifiableTags()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.Minifiable)
				{
					stringBuilder.Append(allDef.defName);
					if (!allDef.tradeTags.NullOrEmpty())
					{
						stringBuilder.Append(" - ");
						stringBuilder.Append(allDef.tradeTags.ToCommaList());
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void ListSolidBackstories()
		{
			IEnumerable<string> enumerable = SolidBioDatabase.allBios.SelectMany((PawnBio bio) => bio.adulthood.spawnCategories).Distinct();
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (string item2 in enumerable)
			{
				string catInner = item2;
				FloatMenuOption item = new FloatMenuOption(catInner, delegate
				{
					IEnumerable<PawnBio> enumerable2 = from b in SolidBioDatabase.allBios
					where b.adulthood.spawnCategories.Contains(catInner)
					select b;
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Backstories with category: " + catInner + " (" + enumerable2.Count() + ")");
					foreach (PawnBio item3 in enumerable2)
					{
						stringBuilder.AppendLine(item3.ToString());
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void ThingSetMakerTest()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingSetMakerDef allDef in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = allDef;
				Faction localF;
				TraderKindDef localKind;
				DebugMenuOption item = new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Action<ThingSetMakerParams> generate = delegate(ThingSetMakerParams parms)
					{
						StringBuilder stringBuilder = new StringBuilder();
						float num = 0f;
						float num2 = 0f;
						for (int i = 0; i < 50; i++)
						{
							List<Thing> list4 = localDef.root.Generate(parms);
							if (stringBuilder.Length > 0)
							{
								stringBuilder.AppendLine();
							}
							float num3 = 0f;
							float num4 = 0f;
							for (int j = 0; j < list4.Count; j++)
							{
								stringBuilder.AppendLine("-" + list4[j].LabelCap + " - $" + (list4[j].MarketValue * (float)list4[j].stackCount).ToString("F0"));
								num3 += list4[j].MarketValue * (float)list4[j].stackCount;
								if (!(list4[j] is Pawn))
								{
									num4 += list4[j].GetStatValue(StatDefOf.Mass) * (float)list4[j].stackCount;
								}
								list4[j].Destroy();
							}
							num += num3;
							num2 += num4;
							stringBuilder.AppendLine("   Total market value: $" + num3.ToString("F0"));
							stringBuilder.AppendLine("   Total mass: " + num4.ToStringMass());
						}
						StringBuilder stringBuilder2 = new StringBuilder();
						stringBuilder2.AppendLine("Default thing sets generated by: " + localDef.defName);
						string nonNullFieldsDebugInfo = Gen.GetNonNullFieldsDebugInfo(localDef.root.fixedParams);
						stringBuilder2.AppendLine("root fixedParams: " + ((!nonNullFieldsDebugInfo.NullOrEmpty()) ? nonNullFieldsDebugInfo : "none"));
						string nonNullFieldsDebugInfo2 = Gen.GetNonNullFieldsDebugInfo(parms);
						if (!nonNullFieldsDebugInfo2.NullOrEmpty())
						{
							stringBuilder2.AppendLine("(used custom debug params: " + nonNullFieldsDebugInfo2 + ")");
						}
						stringBuilder2.AppendLine("Average market value: $" + (num / 50f).ToString("F1"));
						stringBuilder2.AppendLine("Average mass: " + (num2 / 50f).ToStringMass());
						stringBuilder2.AppendLine();
						stringBuilder2.Append(stringBuilder.ToString());
						Log.Message(stringBuilder2.ToString());
					};
					if (localDef == ThingSetMakerDefOf.TraderStock)
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (Faction allFaction in Find.FactionManager.AllFactions)
						{
							if (allFaction != Faction.OfPlayer)
							{
								localF = allFaction;
								list2.Add(new DebugMenuOption(localF.Name + " (" + localF.def.defName + ")", DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list3 = new List<DebugMenuOption>();
									foreach (TraderKindDef item2 in (from x in DefDatabase<TraderKindDef>.AllDefs
									where x.orbital
									select x).Concat(localF.def.caravanTraderKinds).Concat(localF.def.visitorTraderKinds).Concat(localF.def.baseTraderKinds))
									{
										localKind = item2;
										list3.Add(new DebugMenuOption(localKind.defName, DebugMenuOptionMode.Action, delegate
										{
											ThingSetMakerParams obj = default(ThingSetMakerParams);
											obj.traderFaction = localF;
											obj.traderDef = localKind;
											generate(obj);
										}));
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
								}));
							}
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}
					else
					{
						generate(localDef.debugParams);
					}
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void ThingSetMakerPossibleDefs()
		{
			Dictionary<ThingSetMakerDef, List<ThingDef>> generatableThings = new Dictionary<ThingSetMakerDef, List<ThingDef>>();
			foreach (ThingSetMakerDef allDef in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef thingSetMakerDef = allDef;
				generatableThings[allDef] = thingSetMakerDef.root.AllGeneratableThingsDebug(thingSetMakerDef.debugParams).ToList();
			}
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
			list.Add(new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney()));
			list.Add(new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.BaseMass.ToStringMass()));
			foreach (ThingSetMakerDef allDef2 in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = allDef2;
				list.Add(new TableDataGetter<ThingDef>(localDef.defName.Shorten(), (ThingDef d) => generatableThings[localDef].Contains(d).ToStringCheckBlank()));
			}
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Item && !d.IsCorpse && !d.isUnfinishedThing) || (d.category == ThingCategory.Building && d.Minifiable) || d.category == ThingCategory.Pawn
			orderby d.BaseMarketValue descending
			select d, list.ToArray());
		}

		[DebugOutput]
		public static void ThingSetMakerSampled()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingSetMakerDef allDef in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = allDef;
				Faction localF;
				TraderKindDef localKind;
				DebugMenuOption item = new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Action<ThingSetMakerParams> generate = delegate(ThingSetMakerParams parms)
					{
						Dictionary<ThingDef, int> counts = new Dictionary<ThingDef, int>();
						for (int i = 0; i < 500; i++)
						{
							List<Thing> list4 = localDef.root.Generate(parms);
							foreach (ThingDef item2 in (from th in list4
							select th.GetInnerIfMinified().def).Distinct())
							{
								if (!counts.ContainsKey(item2))
								{
									counts.Add(item2, 0);
								}
								Dictionary<ThingDef, int> dictionary;
								ThingDef key;
								(dictionary = counts)[key = item2] = dictionary[key] + 1;
							}
							for (int j = 0; j < list4.Count; j++)
							{
								list4[j].Destroy();
							}
						}
						DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
						where counts.ContainsKey(d)
						orderby counts[d] descending
						select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney()), new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.BaseMass.ToStringMass()), new TableDataGetter<ThingDef>("appearance rate in " + localDef.defName, (ThingDef d) => ((float)counts[d] / 500f).ToStringPercent()));
					};
					if (localDef == ThingSetMakerDefOf.TraderStock)
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (Faction allFaction in Find.FactionManager.AllFactions)
						{
							if (allFaction != Faction.OfPlayer)
							{
								localF = allFaction;
								list2.Add(new DebugMenuOption(localF.Name + " (" + localF.def.defName + ")", DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list3 = new List<DebugMenuOption>();
									foreach (TraderKindDef item3 in (from x in DefDatabase<TraderKindDef>.AllDefs
									where x.orbital
									select x).Concat(localF.def.caravanTraderKinds).Concat(localF.def.visitorTraderKinds).Concat(localF.def.baseTraderKinds))
									{
										localKind = item3;
										list3.Add(new DebugMenuOption(localKind.defName, DebugMenuOptionMode.Action, delegate
										{
											ThingSetMakerParams obj = default(ThingSetMakerParams);
											obj.traderFaction = localF;
											obj.traderDef = localKind;
											generate(obj);
										}));
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
								}));
							}
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}
					else
					{
						generate(localDef.debugParams);
					}
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void WorkDisables()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PawnKindDef item2 in from ki in DefDatabase<PawnKindDef>.AllDefs
			where ki.RaceProps.Humanlike
			select ki)
			{
				PawnKindDef pkInner = item2;
				Faction faction = FactionUtility.DefaultFactionFrom(pkInner.defaultFactionType);
				FloatMenuOption item = new FloatMenuOption(pkInner.defName, delegate
				{
					int num = 500;
					DefMap<WorkTypeDef, int> defMap = new DefMap<WorkTypeDef, int>();
					for (int i = 0; i < num; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(pkInner, faction);
						foreach (WorkTypeDef disabledWorkType in pawn.story.DisabledWorkTypes)
						{
							DefMap<WorkTypeDef, int> defMap2;
							WorkTypeDef def;
							(defMap2 = defMap)[def = disabledWorkType] = defMap2[def] + 1;
						}
					}
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Generated " + num + " pawns of kind " + pkInner.defName + " on faction " + faction.ToStringSafe());
					stringBuilder.AppendLine("Work types disabled:");
					foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
					{
						if (allDef.workTags != 0)
						{
							stringBuilder.AppendLine("   " + allDef.defName + ": " + defMap[allDef] + "        " + ((float)defMap[allDef] / (float)num).ToStringPercent());
						}
					}
					IEnumerable<Backstory> enumerable = BackstoryDatabase.allBackstories.Select((KeyValuePair<string, Backstory> kvp) => kvp.Value);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Backstories WorkTypeDef disable rates (there are " + enumerable.Count() + " backstories):");
					foreach (WorkTypeDef allDef2 in DefDatabase<WorkTypeDef>.AllDefs)
					{
						int num2 = 0;
						foreach (Backstory item3 in enumerable)
						{
							if (item3.DisabledWorkTypes.Any((WorkTypeDef wd) => allDef2 == wd))
							{
								num2++;
							}
						}
						stringBuilder.AppendLine("   " + allDef2.defName + ": " + num2 + "     " + ((float)num2 / (float)BackstoryDatabase.allBackstories.Count).ToStringPercent());
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Backstories WorkTag disable rates (there are " + enumerable.Count() + " backstories):");
					IEnumerator enumerator6 = Enum.GetValues(typeof(WorkTags)).GetEnumerator();
					try
					{
						while (enumerator6.MoveNext())
						{
							WorkTags workTags = (WorkTags)enumerator6.Current;
							int num3 = 0;
							foreach (Backstory item4 in enumerable)
							{
								if ((workTags & item4.workDisables) != 0)
								{
									num3++;
								}
							}
							stringBuilder.AppendLine("   " + workTags + ": " + num3 + "     " + ((float)num3 / (float)BackstoryDatabase.allBackstories.Count).ToStringPercent());
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator6 as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void KeyStrings()
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerator enumerator = Enum.GetValues(typeof(KeyCode)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyCode k = (KeyCode)enumerator.Current;
					stringBuilder.AppendLine(k.ToString() + " - " + k.ToStringReadable());
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void SocialPropernessMatters()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Social-properness-matters things:");
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.socialPropernessMatters)
				{
					stringBuilder.AppendLine($"  {allDef.defName}");
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void FoodPreferability()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Food, ordered by preferability:");
			foreach (ThingDef item in from td in DefDatabase<ThingDef>.AllDefs
			where td.ingestible != null
			orderby td.ingestible.preferability
			select td)
			{
				stringBuilder.AppendLine($"  {item.ingestible.preferability}: {item.defName}");
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void MapDanger()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Map danger status:");
			foreach (Map map in Find.Maps)
			{
				stringBuilder.AppendLine($"{map}: {map.dangerWatcher.DangerRating}");
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void DefNames()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type item2 in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
			{
				DebugMenuOption item = new DebugMenuOption(item2.Name, DebugMenuOptionMode.Action, delegate
				{
					IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), item2, "AllDefs");
					int num = 0;
					StringBuilder stringBuilder = new StringBuilder();
					foreach (Def item3 in source.Cast<Def>())
					{
						stringBuilder.AppendLine(item3.defName);
						num++;
						if (num >= 500)
						{
							Log.Message(stringBuilder.ToString());
							stringBuilder = new StringBuilder();
							num = 0;
						}
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void DefNamesAll()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (Type item in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
			{
				IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), item, "AllDefs");
				stringBuilder.AppendLine("--    " + item.ToString());
				foreach (Def item2 in source.Cast<Def>().OrderBy((Def def) => def.defName))
				{
					stringBuilder.AppendLine(item2.defName);
					num++;
					if (num >= 500)
					{
						Log.Message(stringBuilder.ToString());
						stringBuilder = new StringBuilder();
						num = 0;
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void DefLabels()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type item2 in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
			{
				DebugMenuOption item = new DebugMenuOption(item2.Name, DebugMenuOptionMode.Action, delegate
				{
					IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), item2, "AllDefs");
					int num = 0;
					StringBuilder stringBuilder = new StringBuilder();
					foreach (Def item3 in source.Cast<Def>())
					{
						stringBuilder.AppendLine(item3.label);
						num++;
						if (num >= 500)
						{
							Log.Message(stringBuilder.ToString());
							stringBuilder = new StringBuilder();
							num = 0;
						}
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void Bodies()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (BodyDef allDef in DefDatabase<BodyDef>.AllDefs)
			{
				BodyDef localBd = allDef;
				list.Add(new FloatMenuOption(localBd.defName, delegate
				{
					DebugTables.MakeTablesDialog(from d in localBd.AllParts
					orderby d.height descending
					select d, new TableDataGetter<BodyPartRecord>("defName", (BodyPartRecord d) => d.def.defName), new TableDataGetter<BodyPartRecord>("hitPoints\n(non-adjusted)", (BodyPartRecord d) => d.def.hitPoints), new TableDataGetter<BodyPartRecord>("coverage", (BodyPartRecord d) => d.coverage.ToStringPercent()), new TableDataGetter<BodyPartRecord>("coverageAbsWithChildren", (BodyPartRecord d) => d.coverageAbsWithChildren.ToStringPercent()), new TableDataGetter<BodyPartRecord>("coverageAbs", (BodyPartRecord d) => d.coverageAbs.ToStringPercent()), new TableDataGetter<BodyPartRecord>("depth", (BodyPartRecord d) => d.depth.ToString()), new TableDataGetter<BodyPartRecord>("height", (BodyPartRecord d) => d.height.ToString()));
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void BodyParts()
		{
			DebugTables.MakeTablesDialog(DefDatabase<BodyPartDef>.AllDefs, new TableDataGetter<BodyPartDef>("defName", (BodyPartDef d) => d.defName), new TableDataGetter<BodyPartDef>("hit\npoints", (BodyPartDef d) => d.hitPoints), new TableDataGetter<BodyPartDef>("bleeding\nate\nmultiplier", (BodyPartDef d) => d.bleedRate.ToStringPercent()), new TableDataGetter<BodyPartDef>("perm injury\nchance factor", (BodyPartDef d) => d.permanentInjuryChanceFactor.ToStringPercent()), new TableDataGetter<BodyPartDef>("frostbite\nvulnerability", (BodyPartDef d) => d.frostbiteVulnerability), new TableDataGetter<BodyPartDef>("solid", (BodyPartDef d) => (!d.IsSolidInDefinition_Debug) ? string.Empty : "S"), new TableDataGetter<BodyPartDef>("beauty\nrelated", (BodyPartDef d) => (!d.beautyRelated) ? string.Empty : "B"), new TableDataGetter<BodyPartDef>("alive", (BodyPartDef d) => (!d.alive) ? string.Empty : "A"), new TableDataGetter<BodyPartDef>("conceptual", (BodyPartDef d) => (!d.conceptual) ? string.Empty : "C"), new TableDataGetter<BodyPartDef>("can\nsuggest\namputation", (BodyPartDef d) => (!d.canSuggestAmputation) ? "no A" : string.Empty), new TableDataGetter<BodyPartDef>("socketed", (BodyPartDef d) => (!d.socketed) ? string.Empty : "DoL"), new TableDataGetter<BodyPartDef>("skin covered", (BodyPartDef d) => (!d.IsSkinCoveredInDefinition_Debug) ? string.Empty : "skin"), new TableDataGetter<BodyPartDef>("pawn generator\ncan amputate", (BodyPartDef d) => (!d.pawnGeneratorCanAmputate) ? string.Empty : "amp"), new TableDataGetter<BodyPartDef>("spawn thing\non removed", (BodyPartDef d) => d.spawnThingOnRemoved), new TableDataGetter<BodyPartDef>("hitChanceFactors", (BodyPartDef d) => (d.hitChanceFactors != null) ? (from kvp in d.hitChanceFactors
			select kvp.ToString()).ToCommaList() : string.Empty), new TableDataGetter<BodyPartDef>("tags", (BodyPartDef d) => (d.tags != null) ? (from t in d.tags
			select t.defName).ToCommaList() : string.Empty));
		}

		[DebugOutput]
		public static void TraderKinds()
		{
			DebugTables.MakeTablesDialog(DefDatabase<TraderKindDef>.AllDefs, new TableDataGetter<TraderKindDef>("defName", (TraderKindDef d) => d.defName), new TableDataGetter<TraderKindDef>("commonality", (TraderKindDef d) => d.CalculatedCommonality.ToString("F2")));
		}

		[DebugOutput]
		public static void TraderKindThings()
		{
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
			foreach (TraderKindDef allDef in DefDatabase<TraderKindDef>.AllDefs)
			{
				TraderKindDef localTk = allDef;
				string defName = localTk.defName;
				defName = defName.Replace("Caravan", "Car");
				defName = defName.Replace("Visitor", "Vis");
				defName = defName.Replace("Orbital", "Orb");
				defName = defName.Replace("Neolithic", "Ne");
				defName = defName.Replace("Outlander", "Out");
				defName = defName.Replace("_", " ");
				defName = defName.Shorten();
				list.Add(new TableDataGetter<ThingDef>(defName, (ThingDef td) => localTk.WillTrade(td).ToStringCheckBlank()));
			}
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Item && d.BaseMarketValue > 0.001f && !d.isUnfinishedThing && !d.IsCorpse && !d.destroyOnDrop && d != ThingDefOf.Silver && !d.thingCategories.NullOrEmpty()) || (d.category == ThingCategory.Building && d.Minifiable) || d.category == ThingCategory.Pawn
			orderby d.thingCategories.NullOrEmpty() ? "zzzzzzz" : d.thingCategories[0].defName, d.BaseMarketValue
			select d, list.ToArray());
		}

		[DebugOutput]
		public static void Surgeries()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<RecipeDef>.AllDefs
			where d.IsSurgery
			orderby d.WorkAmountTotal(null) descending
			select d, new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName), new TableDataGetter<RecipeDef>("work", (RecipeDef d) => d.WorkAmountTotal(null).ToString("F0")), new TableDataGetter<RecipeDef>("ingredients", (RecipeDef d) => d.ingredients.Select((IngredientCount ing) => ing.ToString()).ToCommaList()), new TableDataGetter<RecipeDef>("skillRequirements", (RecipeDef d) => (d.skillRequirements != null) ? d.skillRequirements.Select((SkillRequirement ing) => ing.ToString()).ToCommaList() : "-"), new TableDataGetter<RecipeDef>("surgerySuccessChanceFactor", (RecipeDef d) => d.surgerySuccessChanceFactor.ToStringPercent()), new TableDataGetter<RecipeDef>("deathOnFailChance", (RecipeDef d) => d.deathOnFailedSurgeryChance.ToStringPercent()));
		}

		[DebugOutput]
		public static void HitsToKill()
		{
			var data = (from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null
			select d).Select(delegate(ThingDef x)
			{
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < 15; i++)
				{
					PawnGenerationRequest request = new PawnGenerationRequest(x.race.AnyPawnKind, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true);
					Pawn pawn = PawnGenerator.GeneratePawn(request);
					for (int j = 0; j < 1000; j++)
					{
						pawn.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10f));
						if (pawn.Destroyed)
						{
							num += j + 1;
							break;
						}
					}
					if (!pawn.Destroyed)
					{
						Log.Error("Could not kill pawn " + pawn.ToStringSafe());
					}
					if (pawn.health.ShouldBeDeadFromLethalDamageThreshold())
					{
						num2++;
					}
					if (Find.WorldPawns.Contains(pawn))
					{
						Find.WorldPawns.RemovePawn(pawn);
					}
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
				float hits = (float)num / 15f;
				return new
				{
					Race = x,
					Hits = hits,
					DiedDueToDamageThreshold = num2
				};
			}).ToDictionary(x => x.Race);
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null
			orderby d.race.baseHealthScale descending
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("10 damage hits", (ThingDef d) => data[d].Hits.ToString("F0")), new TableDataGetter<ThingDef>("died due to\ndam. thresh.", (ThingDef d) => data[d].DiedDueToDamageThreshold + "/" + 15), new TableDataGetter<ThingDef>("mech", (ThingDef d) => (!d.race.IsMechanoid) ? string.Empty : "mech"));
		}

		[DebugOutput]
		public static void Terrains()
		{
			DebugTables.MakeTablesDialog(DefDatabase<TerrainDef>.AllDefs, new TableDataGetter<TerrainDef>("defName", (TerrainDef d) => d.defName), new TableDataGetter<TerrainDef>("work", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.WorkToBuild).ToString()), new TableDataGetter<TerrainDef>("beauty", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Beauty).ToString()), new TableDataGetter<TerrainDef>("cleanliness", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Cleanliness).ToString()), new TableDataGetter<TerrainDef>("path\ncost", (TerrainDef d) => d.pathCost.ToString()), new TableDataGetter<TerrainDef>("fertility", (TerrainDef d) => d.fertility.ToStringPercentEmptyZero()), new TableDataGetter<TerrainDef>("accept\nfilth", (TerrainDef d) => d.acceptFilth.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("accept terrain\nsource filth", (TerrainDef d) => d.acceptTerrainSourceFilth.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("generated\nfilth", (TerrainDef d) => (d.generatedFilth == null) ? string.Empty : d.generatedFilth.defName), new TableDataGetter<TerrainDef>("hold\nsnow", (TerrainDef d) => d.holdSnow.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("take\nfootprints", (TerrainDef d) => d.takeFootprints.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("avoid\nwander", (TerrainDef d) => d.avoidWander.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("buildable", (TerrainDef d) => d.BuildableByPlayer.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("cost\nlist", (TerrainDef d) => DebugOutputsEconomy.CostListString(d, divideByVolume: false, starIfOnlyBuyable: false)), new TableDataGetter<TerrainDef>("research", (TerrainDef d) => (d.researchPrerequisites == null) ? string.Empty : (from pr in d.researchPrerequisites
			select pr.defName).ToCommaList()), new TableDataGetter<TerrainDef>("affordances", (TerrainDef d) => (from af in d.affordances
			select af.defName).ToCommaList()));
		}

		[DebugOutput]
		public static void TerrainAffordances()
		{
			DebugTables.MakeTablesDialog((from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Building && !d.IsFrame
			select d).Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>()), new TableDataGetter<BuildableDef>("type", (BuildableDef d) => (!(d is TerrainDef)) ? "building" : "terrain"), new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName), new TableDataGetter<BuildableDef>("terrain\naffordance\nneeded", (BuildableDef d) => (d.terrainAffordanceNeeded == null) ? string.Empty : d.terrainAffordanceNeeded.defName));
		}

		[DebugOutput]
		public static void MentalBreaks()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<MentalBreakDef>.AllDefs
			orderby d.intensity, d.defName
			select d, new TableDataGetter<MentalBreakDef>("defName", (MentalBreakDef d) => d.defName), new TableDataGetter<MentalBreakDef>("intensity", (MentalBreakDef d) => d.intensity.ToString()), new TableDataGetter<MentalBreakDef>("chance in intensity", (MentalBreakDef d) => (d.baseCommonality / DefDatabase<MentalBreakDef>.AllDefs.Where((MentalBreakDef x) => x.intensity == d.intensity).Sum((MentalBreakDef x) => x.baseCommonality)).ToStringPercent()), new TableDataGetter<MentalBreakDef>("min duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? ((float)d.mentalState.minTicksBeforeRecovery / 60000f).ToString("0.##") : string.Empty), new TableDataGetter<MentalBreakDef>("avg duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? (Mathf.Min((float)d.mentalState.minTicksBeforeRecovery + d.mentalState.recoveryMtbDays * 60000f, (float)d.mentalState.maxTicksBeforeRecovery) / 60000f).ToString("0.##") : string.Empty), new TableDataGetter<MentalBreakDef>("max duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? ((float)d.mentalState.maxTicksBeforeRecovery / 60000f).ToString("0.##") : string.Empty), new TableDataGetter<MentalBreakDef>("recoverFromSleep", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.recoverFromSleep) ? string.Empty : "recoverFromSleep"), new TableDataGetter<MentalBreakDef>("recoveryThought", (MentalBreakDef d) => (d.mentalState != null) ? d.mentalState.moodRecoveryThought.ToStringSafe() : string.Empty), new TableDataGetter<MentalBreakDef>("category", (MentalBreakDef d) => (d.mentalState == null) ? string.Empty : d.mentalState.category.ToString()), new TableDataGetter<MentalBreakDef>("blockNormalThoughts", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.blockNormalThoughts) ? string.Empty : "blockNormalThoughts"), new TableDataGetter<MentalBreakDef>("allowBeatfire", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.allowBeatfire) ? string.Empty : "allowBeatfire"));
		}

		[DebugOutput]
		public static void TraitsSampled()
		{
			List<Pawn> testColonists = new List<Pawn>();
			for (int i = 0; i < 4000; i++)
			{
				testColonists.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer));
			}
			Func<TraitDegreeData, TraitDef> getTrait = (TraitDegreeData d) => DefDatabase<TraitDef>.AllDefs.First((TraitDef td) => td.degreeDatas.Contains(d));
			Func<TraitDegreeData, float> getPrevalence = delegate(TraitDegreeData d)
			{
				float num = 0f;
				foreach (Pawn item in testColonists)
				{
					Trait trait = item.story.traits.GetTrait(getTrait(d));
					if (trait != null && trait.Degree == d.degree)
					{
						num += 1f;
					}
				}
				return num / 4000f;
			};
			DebugTables.MakeTablesDialog(DefDatabase<TraitDef>.AllDefs.SelectMany((TraitDef tr) => tr.degreeDatas), new TableDataGetter<TraitDegreeData>("trait", (TraitDegreeData d) => getTrait(d).defName), new TableDataGetter<TraitDegreeData>("trait commonality", (TraitDegreeData d) => getTrait(d).GetGenderSpecificCommonality(Gender.None).ToString("F2")), new TableDataGetter<TraitDegreeData>("trait commonalityFemale", (TraitDegreeData d) => getTrait(d).GetGenderSpecificCommonality(Gender.Female).ToString("F2")), new TableDataGetter<TraitDegreeData>("degree", (TraitDegreeData d) => d.label), new TableDataGetter<TraitDegreeData>("degree num", (TraitDegreeData d) => (getTrait(d).degreeDatas.Count <= 0) ? string.Empty : d.degree.ToString()), new TableDataGetter<TraitDegreeData>("degree commonality", (TraitDegreeData d) => (getTrait(d).degreeDatas.Count <= 0) ? string.Empty : d.commonality.ToString("F2")), new TableDataGetter<TraitDegreeData>("marketValueFactorOffset", (TraitDegreeData d) => d.marketValueFactorOffset.ToString("F0")), new TableDataGetter<TraitDegreeData>("prevalence among " + 4000 + "\ngenerated Colonists", (TraitDegreeData d) => getPrevalence(d).ToStringPercent()));
		}

		[DebugOutput]
		public static void BestThingRequestGroup()
		{
			DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
			where ListerThings.EverListable(x, ListerThingsUse.Global) || ListerThings.EverListable(x, ListerThingsUse.Region)
			orderby x.label
			select x, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("best local", delegate(ThingDef d)
			{
				IEnumerable<ThingRequestGroup> source2 = ListerThings.EverListable(d, ListerThingsUse.Region) ? ((ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup))).Where((ThingRequestGroup x) => x.StoreInRegion() && x.Includes(d)) : Enumerable.Empty<ThingRequestGroup>();
				if (!source2.Any())
				{
					return "-";
				}
				ThingRequestGroup best2 = source2.MinBy((ThingRequestGroup x) => DefDatabase<ThingDef>.AllDefs.Count((ThingDef y) => ListerThings.EverListable(y, ListerThingsUse.Region) && x.Includes(y)));
				return best2 + " (defs: " + DefDatabase<ThingDef>.AllDefs.Count((ThingDef x) => ListerThings.EverListable(x, ListerThingsUse.Region) && best2.Includes(x)) + ")";
			}), new TableDataGetter<ThingDef>("best global", delegate(ThingDef d)
			{
				IEnumerable<ThingRequestGroup> source = ListerThings.EverListable(d, ListerThingsUse.Global) ? ((ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup))).Where((ThingRequestGroup x) => x.Includes(d)) : Enumerable.Empty<ThingRequestGroup>();
				if (!source.Any())
				{
					return "-";
				}
				ThingRequestGroup best = source.MinBy((ThingRequestGroup x) => DefDatabase<ThingDef>.AllDefs.Count((ThingDef y) => ListerThings.EverListable(y, ListerThingsUse.Global) && x.Includes(y)));
				return best + " (defs: " + DefDatabase<ThingDef>.AllDefs.Count((ThingDef x) => ListerThings.EverListable(x, ListerThingsUse.Global) && best.Includes(x)) + ")";
			}));
		}

		[DebugOutput]
		public static void Prosthetics()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, (Pawn p) => p.health.hediffSet.hediffs.Count == 0);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			Action refreshPawn = delegate
			{
				while (pawn.health.hediffSet.hediffs.Count > 0)
				{
					pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs[0]);
				}
			};
			Func<RecipeDef, BodyPartRecord> getApplicationPoint = (RecipeDef recipe) => recipe.appliedOnFixedBodyParts.SelectMany((BodyPartDef bpd) => pawn.def.race.body.GetPartsWithDef(bpd)).FirstOrDefault();
			Func<RecipeDef, ThingDef> getProstheticItem = (RecipeDef recipe) => (from ic in recipe.ingredients
			select ic.filter.AnyAllowedDef).FirstOrDefault((ThingDef td) => !td.IsMedicine);
			List<TableDataGetter<RecipeDef>> list = new List<TableDataGetter<RecipeDef>>();
			list.Add(new TableDataGetter<RecipeDef>("defName", (RecipeDef r) => r.defName));
			list.Add(new TableDataGetter<RecipeDef>("price", (RecipeDef r) => getProstheticItem(r)?.BaseMarketValue ?? 0f));
			list.Add(new TableDataGetter<RecipeDef>("install time", (RecipeDef r) => r.workAmount));
			list.Add(new TableDataGetter<RecipeDef>("install total cost", delegate(RecipeDef r)
			{
				float num2 = r.ingredients.Sum((IngredientCount ic) => ic.filter.AnyAllowedDef.BaseMarketValue * ic.GetBaseCount());
				float num3 = r.workAmount * 0.0036f;
				return num2 + num3;
			}));
			list.Add(new TableDataGetter<RecipeDef>("install skill", (RecipeDef r) => (from sr in r.skillRequirements
			select sr.minLevel).Max()));
			foreach (PawnCapacityDef item in from pc in DefDatabase<PawnCapacityDef>.AllDefs
			orderby pc.listOrder
			select pc)
			{
				list.Add(new TableDataGetter<RecipeDef>(item.defName, delegate(RecipeDef r)
				{
					refreshPawn();
					r.Worker.ApplyOnPawn(pawn, getApplicationPoint(r), null, null, null);
					float num = pawn.health.capacities.GetLevel(item) - 1f;
					if ((double)Math.Abs(num) > 0.001)
					{
						return num.ToStringPercent();
					}
					refreshPawn();
					BodyPartRecord bodyPartRecord = getApplicationPoint(r);
					Pawn pawn2 = pawn;
					DamageDef executionCut = DamageDefOf.ExecutionCut;
					float amount = pawn.health.hediffSet.GetPartHealth(bodyPartRecord) / 2f;
					float armorPenetration = 999f;
					BodyPartRecord hitPart = bodyPartRecord;
					pawn2.TakeDamage(new DamageInfo(executionCut, amount, armorPenetration, -1f, null, hitPart));
					List<PawnCapacityUtility.CapacityImpactor> list2 = new List<PawnCapacityUtility.CapacityImpactor>();
					PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, item, list2);
					if (list2.Any((PawnCapacityUtility.CapacityImpactor imp) => imp.IsDirect))
					{
						return 0f.ToStringPercent();
					}
					return string.Empty;
				}));
			}
			list.Add(new TableDataGetter<RecipeDef>("tech level", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).techLevel.ToStringHuman() : string.Empty));
			list.Add(new TableDataGetter<RecipeDef>("thingSetMakerTags", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).thingSetMakerTags.ToCommaList() : string.Empty));
			list.Add(new TableDataGetter<RecipeDef>("techHediffsTags", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).techHediffsTags.ToCommaList() : string.Empty));
			DebugTables.MakeTablesDialog(from r in ThingDefOf.Human.AllRecipes
			where r.workerClass == typeof(Recipe_InstallArtificialBodyPart) || r.workerClass == typeof(Recipe_InstallNaturalBodyPart)
			select r, list.ToArray());
			Messages.Clear();
		}

		[DebugOutput]
		public static void JoyGivers()
		{
			DebugTables.MakeTablesDialog(DefDatabase<JoyGiverDef>.AllDefs, new TableDataGetter<JoyGiverDef>("defName", (JoyGiverDef d) => d.defName), new TableDataGetter<JoyGiverDef>("joyKind", (JoyGiverDef d) => (d.joyKind != null) ? d.joyKind.defName : "null"), new TableDataGetter<JoyGiverDef>("baseChance", (JoyGiverDef d) => d.baseChance.ToString()), new TableDataGetter<JoyGiverDef>("canDoWhileInBed", (JoyGiverDef d) => d.canDoWhileInBed.ToStringCheckBlank()), new TableDataGetter<JoyGiverDef>("desireSit", (JoyGiverDef d) => d.desireSit.ToStringCheckBlank()), new TableDataGetter<JoyGiverDef>("unroofedOnly", (JoyGiverDef d) => d.unroofedOnly.ToStringCheckBlank()), new TableDataGetter<JoyGiverDef>("jobDef", (JoyGiverDef d) => (d.jobDef != null) ? d.jobDef.defName : "null"), new TableDataGetter<JoyGiverDef>("pctPawnsEverDo", (JoyGiverDef d) => d.pctPawnsEverDo.ToStringPercent()), new TableDataGetter<JoyGiverDef>("requiredCapacities", (JoyGiverDef d) => (d.requiredCapacities != null) ? (from c in d.requiredCapacities
			select c.defName).ToCommaList() : string.Empty), new TableDataGetter<JoyGiverDef>("thingDefs", (JoyGiverDef d) => (d.thingDefs != null) ? (from c in d.thingDefs
			select c.defName).ToCommaList() : string.Empty), new TableDataGetter<JoyGiverDef>("JoyGainFactors", (JoyGiverDef d) => (d.thingDefs != null) ? (from c in d.thingDefs
			select c.GetStatValueAbstract(StatDefOf.JoyGainFactor).ToString("F2")).ToCommaList() : string.Empty));
		}

		[DebugOutput]
		public static void JoyJobs()
		{
			DebugTables.MakeTablesDialog(from j in DefDatabase<JobDef>.AllDefs
			where j.joyKind != null
			select j, new TableDataGetter<JobDef>("defName", (JobDef d) => d.defName), new TableDataGetter<JobDef>("joyKind", (JobDef d) => d.joyKind.defName), new TableDataGetter<JobDef>("joyDuration", (JobDef d) => d.joyDuration.ToString()), new TableDataGetter<JobDef>("joyGainRate", (JobDef d) => d.joyGainRate.ToString()), new TableDataGetter<JobDef>("joyMaxParticipants", (JobDef d) => d.joyMaxParticipants.ToString()), new TableDataGetter<JobDef>("joySkill", (JobDef d) => (d.joySkill == null) ? string.Empty : d.joySkill.defName), new TableDataGetter<JobDef>("joyXpPerTick", (JobDef d) => d.joyXpPerTick.ToString()));
		}

		[DebugOutput]
		public static void Thoughts()
		{
			Func<ThoughtDef, string> stagesText = delegate(ThoughtDef t)
			{
				string text = string.Empty;
				if (t.stages == null)
				{
					return null;
				}
				for (int i = 0; i < t.stages.Count; i++)
				{
					ThoughtStage thoughtStage = t.stages[i];
					string text2 = text;
					text = text2 + "[" + i + "] ";
					if (thoughtStage == null)
					{
						text += "null";
					}
					else
					{
						if (thoughtStage.label != null)
						{
							text += thoughtStage.label;
						}
						if (thoughtStage.labelSocial != null)
						{
							if (thoughtStage.label != null)
							{
								text += "/";
							}
							text += thoughtStage.labelSocial;
						}
						text += " ";
						if (thoughtStage.baseMoodEffect != 0f)
						{
							text = text + "[" + thoughtStage.baseMoodEffect.ToStringWithSign() + " Mo]";
						}
						if (thoughtStage.baseOpinionOffset != 0f)
						{
							text = text + "(" + thoughtStage.baseOpinionOffset.ToStringWithSign() + " Op)";
						}
					}
					if (i < t.stages.Count - 1)
					{
						text += "\n";
					}
				}
				return text;
			};
			DebugTables.MakeTablesDialog(DefDatabase<ThoughtDef>.AllDefs, new TableDataGetter<ThoughtDef>("defName", (ThoughtDef d) => d.defName), new TableDataGetter<ThoughtDef>("type", (ThoughtDef d) => (!d.IsMemory) ? "situ" : "mem"), new TableDataGetter<ThoughtDef>("social", (ThoughtDef d) => (!d.IsSocial) ? "mood" : "soc"), new TableDataGetter<ThoughtDef>("stages", (ThoughtDef d) => stagesText(d)), new TableDataGetter<ThoughtDef>("best\nmood", (ThoughtDef d) => (from st in d.stages
			where st != null
			select st).Max((ThoughtStage st) => st.baseMoodEffect)), new TableDataGetter<ThoughtDef>("worst\nmood", (ThoughtDef d) => (from st in d.stages
			where st != null
			select st).Min((ThoughtStage st) => st.baseMoodEffect)), new TableDataGetter<ThoughtDef>("stack\nlimit", (ThoughtDef d) => d.stackLimit.ToString()), new TableDataGetter<ThoughtDef>("stack\nlimit\nper o. pawn", (ThoughtDef d) => (d.stackLimitForSameOtherPawn >= 0) ? d.stackLimitForSameOtherPawn.ToString() : string.Empty), new TableDataGetter<ThoughtDef>("stacked\neffect\nmultiplier", (ThoughtDef d) => (d.stackLimit != 1) ? d.stackedEffectMultiplier.ToStringPercent() : string.Empty), new TableDataGetter<ThoughtDef>("duration\n(days)", (ThoughtDef d) => d.durationDays.ToString()), new TableDataGetter<ThoughtDef>("effect\nmultiplying\nstat", (ThoughtDef d) => (d.effectMultiplyingStat != null) ? d.effectMultiplyingStat.defName : string.Empty), new TableDataGetter<ThoughtDef>("game\ncondition", (ThoughtDef d) => (d.gameCondition != null) ? d.gameCondition.defName : string.Empty), new TableDataGetter<ThoughtDef>("hediff", (ThoughtDef d) => (d.hediff != null) ? d.hediff.defName : string.Empty), new TableDataGetter<ThoughtDef>("lerp opinion\nto zero\nafter duration pct", (ThoughtDef d) => d.lerpOpinionToZeroAfterDurationPct.ToStringPercent()), new TableDataGetter<ThoughtDef>("max cumulated\nopinion\noffset", (ThoughtDef d) => (!(d.maxCumulatedOpinionOffset > 99999f)) ? d.maxCumulatedOpinionOffset.ToString() : string.Empty), new TableDataGetter<ThoughtDef>("next\nthought", (ThoughtDef d) => (d.nextThought != null) ? d.nextThought.defName : string.Empty), new TableDataGetter<ThoughtDef>("nullified\nif not colonist", (ThoughtDef d) => d.nullifiedIfNotColonist.ToStringCheckBlank()), new TableDataGetter<ThoughtDef>("show\nbubble", (ThoughtDef d) => d.showBubble.ToStringCheckBlank()));
		}

		[DebugOutput]
		public static void GenSteps()
		{
			DebugTables.MakeTablesDialog(from x in DefDatabase<GenStepDef>.AllDefsListForReading
			orderby x.order, x.index
			select x, new TableDataGetter<GenStepDef>("defName", (GenStepDef x) => x.defName), new TableDataGetter<GenStepDef>("order", (GenStepDef x) => x.order.ToString("0.##")), new TableDataGetter<GenStepDef>("class", (GenStepDef x) => x.genStep.GetType().Name), new TableDataGetter<GenStepDef>("site", (GenStepDef x) => (x.linkWithSite == null) ? string.Empty : x.linkWithSite.defName));
		}

		[DebugOutput]
		public static void WorldGenSteps()
		{
			DebugTables.MakeTablesDialog(from x in DefDatabase<WorldGenStepDef>.AllDefsListForReading
			orderby x.order, x.index
			select x, new TableDataGetter<WorldGenStepDef>("defName", (WorldGenStepDef x) => x.defName), new TableDataGetter<WorldGenStepDef>("order", (WorldGenStepDef x) => x.order.ToString("0.##")), new TableDataGetter<WorldGenStepDef>("class", (WorldGenStepDef x) => x.worldGenStep.GetType().Name));
		}
	}
}
