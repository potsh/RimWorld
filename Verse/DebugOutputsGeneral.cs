using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsGeneral
	{
		[DebugOutput]
		public static void WeaponsRanged()
		{
			Func<ThingDef, int> damage = (ThingDef d) => (d.Verbs[0].defaultProjectile != null) ? d.Verbs[0].defaultProjectile.projectile.GetDamageAmount(null) : 0;
			Func<ThingDef, float> armorPenetration = (ThingDef d) => (d.Verbs[0].defaultProjectile == null) ? 0f : d.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(null);
			Func<ThingDef, float> stoppingPower = (ThingDef d) => (d.Verbs[0].defaultProjectile == null) ? 0f : d.Verbs[0].defaultProjectile.projectile.stoppingPower;
			Func<ThingDef, float> warmup = (ThingDef d) => d.Verbs[0].warmupTime;
			Func<ThingDef, float> cooldown = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown);
			Func<ThingDef, int> burstShots = (ThingDef d) => d.Verbs[0].burstShotCount;
			Func<ThingDef, float> fullcycle = (ThingDef d) => warmup(d) + cooldown(d) + ((d.Verbs[0].burstShotCount - 1) * d.Verbs[0].ticksBetweenBurstShots).TicksToSeconds();
			Func<ThingDef, float> dpsMissless = delegate(ThingDef d)
			{
				int num = burstShots(d);
				float num2 = warmup(d) + cooldown(d);
				num2 += (float)(num - 1) * ((float)d.Verbs[0].ticksBetweenBurstShots / 60f);
				return (float)(damage(d) * num) / num2;
			};
			Func<ThingDef, float> accTouch = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyTouch);
			Func<ThingDef, float> accShort = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyShort);
			Func<ThingDef, float> accMed = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyMedium);
			Func<ThingDef, float> accLong = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyLong);
			Func<ThingDef, float> accAvg = (ThingDef d) => (accTouch(d) + accShort(d) + accMed(d) + accLong(d)) / 4f;
			Func<ThingDef, float> dpsAvg = (ThingDef d) => dpsMissless(d) * accAvg(d);
			DebugTables.MakeTablesDialog((from d in DefDatabase<ThingDef>.AllDefs
			where d.IsRangedWeapon
			select d).OrderByDescending(dpsAvg), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("damage", (ThingDef d) => damage(d).ToString()), new TableDataGetter<ThingDef>("AP", (ThingDef d) => armorPenetration(d).ToStringPercent()), new TableDataGetter<ThingDef>("stop\npower", (ThingDef d) => (!(stoppingPower(d) > 0f)) ? string.Empty : stoppingPower(d).ToString("F1")), new TableDataGetter<ThingDef>("warmup", (ThingDef d) => warmup(d).ToString("F2")), new TableDataGetter<ThingDef>("burst\nshots", (ThingDef d) => burstShots(d).ToString()), new TableDataGetter<ThingDef>("cooldown", (ThingDef d) => cooldown(d).ToString("F2")), new TableDataGetter<ThingDef>("full\ncycle", (ThingDef d) => fullcycle(d).ToString("F2")), new TableDataGetter<ThingDef>("range", (ThingDef d) => d.Verbs[0].range.ToString("F1")), new TableDataGetter<ThingDef>("projectile\nspeed", (ThingDef d) => (d.projectile == null) ? string.Empty : d.projectile.speed.ToString("F0")), new TableDataGetter<ThingDef>("dps\nmissless", (ThingDef d) => dpsMissless(d).ToString("F2")), new TableDataGetter<ThingDef>("accuracy\ntouch (" + 3f + ")", (ThingDef d) => accTouch(d).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nshort (" + 12f + ")", (ThingDef d) => accShort(d).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nmed (" + 25f + ")", (ThingDef d) => accMed(d).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nlong (" + 40f + ")", (ThingDef d) => accLong(d).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\navg", (ThingDef d) => accAvg(d).ToString("F2")), new TableDataGetter<ThingDef>("forced\nmiss\nradius", (ThingDef d) => (!(d.Verbs[0].forcedMissRadius > 0f)) ? string.Empty : d.Verbs[0].forcedMissRadius.ToString()), new TableDataGetter<ThingDef>("dps\ntouch", (ThingDef d) => (dpsMissless(d) * accTouch(d)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nshort", (ThingDef d) => (dpsMissless(d) * accShort(d)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nmed", (ThingDef d) => (dpsMissless(d) * accMed(d)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nlong", (ThingDef d) => (dpsMissless(d) * accLong(d)).ToString("F2")), new TableDataGetter<ThingDef>("dps\navg", (ThingDef d) => dpsAvg(d).ToString("F2")), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F0")), new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake).ToString("F0")), new TableDataGetter<ThingDef>("dpsAvg*100 / market value", (ThingDef d) => (dpsAvg(d) * 100f / d.GetStatValueAbstract(StatDefOf.MarketValue)).ToString("F3")));
		}

		[DebugOutput]
		public static void WeaponsMelee()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("Stuffless", delegate
			{
				DoTablesInternalMelee(null);
			}));
			IEnumerable<ThingDef> source = from st in DefDatabase<ThingDef>.AllDefs
			where st.IsStuff
			where DefDatabase<ThingDef>.AllDefs.Any((ThingDef wd) => wd.IsMeleeWeapon && st.stuffProps.CanMake(wd))
			select st;
			foreach (ThingDef item in from td in source
			orderby td.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier) descending
			select td)
			{
				ThingDef localStuff = item;
				float statValueAbstract = localStuff.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier);
				float statValueAbstract2 = localStuff.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier);
				float statFactorFromList = localStuff.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier);
				list.Add(new FloatMenuOption(localStuff.defName + " (sharp " + statValueAbstract + ", blunt " + statValueAbstract2 + ", cooldown " + statFactorFromList + ")", delegate
				{
					DoTablesInternalMelee(localStuff);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		private static void DoTablesInternalMelee(ThingDef stuff, bool doRaces = false)
		{
			Func<Def, float> meleeDamageGetter = delegate(Def d)
			{
				List<Verb> concreteExampleVerbs5 = VerbUtility.GetConcreteExampleVerbs(d, stuff);
				if (concreteExampleVerbs5.OfType<Verb_MeleeAttack>().Any())
				{
					return concreteExampleVerbs5.OfType<Verb_MeleeAttack>().AverageWeighted((Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeSelectionWeight(v, null), (Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeDamageAmount(v, null));
				}
				return -1f;
			};
			Func<Def, float> rangedDamageGetter = delegate(Def d)
			{
				List<Verb> concreteExampleVerbs4 = VerbUtility.GetConcreteExampleVerbs(d, stuff);
				Verb_LaunchProjectile verb_LaunchProjectile = concreteExampleVerbs4.OfType<Verb_LaunchProjectile>().FirstOrDefault();
				if (verb_LaunchProjectile != null && verb_LaunchProjectile.GetProjectile() != null)
				{
					return (float)verb_LaunchProjectile.GetProjectile().projectile.GetDamageAmount(null);
				}
				return -1f;
			};
			Func<Def, float> rangedWarmupGetter = delegate(Def d)
			{
				List<Verb> concreteExampleVerbs3 = VerbUtility.GetConcreteExampleVerbs(d, stuff);
				return concreteExampleVerbs3.OfType<Verb_LaunchProjectile>().FirstOrDefault()?.verbProps.warmupTime ?? (-1f);
			};
			Func<Def, float> meleeCooldownGetter = delegate(Def d)
			{
				List<Verb> concreteExampleVerbs2 = VerbUtility.GetConcreteExampleVerbs(d, stuff);
				if (concreteExampleVerbs2.OfType<Verb_MeleeAttack>().Any())
				{
					return concreteExampleVerbs2.OfType<Verb_MeleeAttack>().AverageWeighted((Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeSelectionWeight(v, null), (Verb_MeleeAttack v) => v.verbProps.AdjustedCooldown(v, null));
				}
				return -1f;
			};
			Func<Def, float> rangedCooldownGetter = delegate(Def d)
			{
				List<Verb> concreteExampleVerbs = VerbUtility.GetConcreteExampleVerbs(d, stuff);
				return concreteExampleVerbs.OfType<Verb_LaunchProjectile>().FirstOrDefault()?.verbProps.defaultCooldownTime ?? (-1f);
			};
			Func<Def, float> meleeDpsGetter = (Def d) => meleeDamageGetter(d) * 0.82f / meleeCooldownGetter(d);
			Func<Def, float> marketValueGetter = delegate(Def d)
			{
				ThingDef thingDef3 = d as ThingDef;
				if (thingDef3 != null)
				{
					return thingDef3.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
				}
				HediffDef hediffDef = d as HediffDef;
				if (hediffDef != null)
				{
					if (hediffDef.spawnThingOnRemoved == null)
					{
						return 0f;
					}
					return hediffDef.spawnThingOnRemoved.GetStatValueAbstract(StatDefOf.MarketValue);
				}
				return -1f;
			};
			IEnumerable<Def> enumerable = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsWeapon
			select d).Cast<Def>().Concat((from h in DefDatabase<HediffDef>.AllDefs
			where h.CompProps<HediffCompProperties_VerbGiver>() != null
			select h).Cast<Def>());
			if (doRaces)
			{
				enumerable = enumerable.Concat((from d in DefDatabase<ThingDef>.AllDefs
				where d.race != null
				select d).Cast<Def>());
			}
			enumerable = from h in enumerable
			orderby meleeDpsGetter(h) descending
			select h;
			DebugTables.MakeTablesDialog(enumerable, new TableDataGetter<Def>("defName", (Def d) => d.defName), new TableDataGetter<Def>("melee\ndamage\naverage", (Def d) => meleeDamageGetter(d).ToString("F2")), new TableDataGetter<Def>("melee\ncooldown\naverage", (Def d) => meleeCooldownGetter(d).ToString("F2")), new TableDataGetter<Def>("melee\nDPS", (Def d) => meleeDpsGetter(d).ToString("F2")), new TableDataGetter<Def>("ranged\ndamage", (Def d) => rangedDamageGetter(d).ToString()), new TableDataGetter<Def>("ranged\nwarmup", (Def d) => rangedWarmupGetter(d).ToString("F2")), new TableDataGetter<Def>("ranged\ncooldown", (Def d) => rangedCooldownGetter(d).ToString("F2")), new TableDataGetter<Def>("market value", (Def d) => marketValueGetter(d).ToStringMoney()), new TableDataGetter<Def>("work to make", delegate(Def d)
			{
				ThingDef thingDef2 = d as ThingDef;
				if (thingDef2 == null)
				{
					return "-";
				}
				return thingDef2.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0");
			}), new TableDataGetter<Def>((stuff == null) ? "CanMake" : (stuff.defName + " CanMake"), delegate(Def d)
			{
				if (stuff == null)
				{
					return "n/a";
				}
				ThingDef thingDef = d as ThingDef;
				if (thingDef == null)
				{
					return "-";
				}
				return stuff.stuffProps.CanMake(thingDef).ToStringCheckBlank();
			}), new TableDataGetter<Def>("assumed\nmelee\nhit chance", (Def d) => 0.82f.ToStringPercent()));
		}

		[DebugOutput]
		public static void Tools()
		{
			var tools = (from x in (from x in DefDatabase<ThingDef>.AllDefs
			where !x.tools.NullOrEmpty()
			select x).SelectMany((ThingDef x) => from y in x.tools
			select new
			{
				Parent = (Def)x,
				Tool = y
			}).Concat((from x in DefDatabase<TerrainDef>.AllDefs
			where !x.tools.NullOrEmpty()
			select x).SelectMany((TerrainDef x) => from y in x.tools
			select new
			{
				Parent = (Def)x,
				Tool = y
			})).Concat((from x in DefDatabase<HediffDef>.AllDefs
			where x.HasComp(typeof(HediffComp_VerbGiver)) && !x.CompProps<HediffCompProperties_VerbGiver>().tools.NullOrEmpty()
			select x).SelectMany((HediffDef x) => from y in x.CompProps<HediffCompProperties_VerbGiver>().tools
			select new
			{
				Parent = (Def)x,
				Tool = y
			}))
			orderby x.Parent.defName, x.Tool.power descending
			select x).ToList();
			Dictionary<Tool, float> selWeight = tools.ToDictionary(x => x.Tool, x => x.Tool.VerbsProperties.Average((VerbProperties y) => y.AdjustedMeleeSelectionWeight(x.Tool, null, null, null, x.Parent is ThingDef && ((ThingDef)x.Parent).category == ThingCategory.Pawn)));
			Dictionary<Def, float> selWeightSumInGroup = (from x in tools
			select x.Parent).Distinct().ToDictionary((Def x) => x, (Def x) => (from y in tools
			where y.Parent == x
			select y).Sum(y => selWeight[y.Tool]));
			DebugTables.MakeTablesDialog(tools.Select((x, int index) => index), new TableDataGetter<int>("label", (int x) => tools[x].Tool.label), new TableDataGetter<int>("source", (int x) => tools[x].Parent.defName), new TableDataGetter<int>("power", (int x) => tools[x].Tool.power.ToString("0.##")), new TableDataGetter<int>("AP", delegate(int x)
			{
				float num = tools[x].Tool.armorPenetration;
				if (num < 0f)
				{
					num = tools[x].Tool.power * 0.015f;
				}
				return num.ToStringPercent();
			}), new TableDataGetter<int>("cooldown", (int x) => tools[x].Tool.cooldownTime.ToString("0.##")), new TableDataGetter<int>("selection weight", (int x) => selWeight[tools[x].Tool].ToString("0.##")), new TableDataGetter<int>("selection weight\nwithin def", (int x) => (selWeight[tools[x].Tool] / selWeightSumInGroup[tools[x].Parent]).ToStringPercent()), new TableDataGetter<int>("chance\nfactor", (int x) => (tools[x].Tool.chanceFactor != 1f) ? tools[x].Tool.chanceFactor.ToString("0.##") : string.Empty), new TableDataGetter<int>("adds hediff", (int x) => (tools[x].Tool.hediff == null) ? string.Empty : tools[x].Tool.hediff.defName), new TableDataGetter<int>("linked body parts", (int x) => (tools[x].Tool.linkedBodyPartsGroup == null) ? string.Empty : tools[x].Tool.linkedBodyPartsGroup.defName), new TableDataGetter<int>("surprise attack", (int x) => (tools[x].Tool.surpriseAttack == null || tools[x].Tool.surpriseAttack.extraMeleeDamages.NullOrEmpty()) ? string.Empty : (tools[x].Tool.surpriseAttack.extraMeleeDamages[0].amount.ToString("0.##") + " (" + tools[x].Tool.surpriseAttack.extraMeleeDamages[0].def.defName + ")")), new TableDataGetter<int>("capacities", (int x) => tools[x].Tool.capacities.ToStringSafeEnumerable()), new TableDataGetter<int>("maneuvers", (int x) => tools[x].Tool.Maneuvers.ToStringSafeEnumerable()), new TableDataGetter<int>("always weapon", (int x) => (!tools[x].Tool.alwaysTreatAsWeapon) ? string.Empty : "always wep"), new TableDataGetter<int>("id", (int x) => tools[x].Tool.id));
		}

		[DebugOutput]
		public static void ApparelByStuff()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("Stuffless", delegate
			{
				DoTableInternalApparel(null);
			}));
			foreach (ThingDef item in from td in DefDatabase<ThingDef>.AllDefs
			where td.IsStuff
			select td)
			{
				ThingDef localStuff = item;
				list.Add(new FloatMenuOption(localStuff.defName, delegate
				{
					DoTableInternalApparel(localStuff);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void ApparelArmor()
		{
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("label", (ThingDef x) => x.LabelCap));
			list.Add(new TableDataGetter<ThingDef>("none", delegate(ThingDef x)
			{
				if (x.MadeFromStuff)
				{
					return string.Empty;
				}
				return x.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Heat).ToStringPercent();
			}));
			foreach (ThingDef item in from x in DefDatabase<ThingDef>.AllDefs
			where x.IsStuff
			orderby x.BaseMarketValue
			select x)
			{
				ThingDef stuffLocal = item;
				if (DefDatabase<ThingDef>.AllDefs.Any((ThingDef x) => x.IsApparel && stuffLocal.stuffProps.CanMake(x)))
				{
					list.Add(new TableDataGetter<ThingDef>(stuffLocal.label.Shorten(), delegate(ThingDef x)
					{
						if (!stuffLocal.stuffProps.CanMake(x))
						{
							return string.Empty;
						}
						return x.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, stuffLocal).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, stuffLocal).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Heat, stuffLocal).ToStringPercent();
					}));
				}
			}
			DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
			where x.IsApparel
			orderby x.BaseMarketValue
			select x, list.ToArray());
		}

		[DebugOutput]
		public static void ApparelInsulation()
		{
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("label", (ThingDef x) => x.LabelCap));
			list.Add(new TableDataGetter<ThingDef>("none", delegate(ThingDef x)
			{
				if (x.MadeFromStuff)
				{
					return string.Empty;
				}
				return x.GetStatValueAbstract(StatDefOf.Insulation_Heat).ToStringTemperature() + " / " + x.GetStatValueAbstract(StatDefOf.Insulation_Cold).ToStringTemperature();
			}));
			foreach (ThingDef item in from x in DefDatabase<ThingDef>.AllDefs
			where x.IsStuff
			orderby x.BaseMarketValue
			select x)
			{
				ThingDef stuffLocal = item;
				if (DefDatabase<ThingDef>.AllDefs.Any((ThingDef x) => x.IsApparel && stuffLocal.stuffProps.CanMake(x)))
				{
					list.Add(new TableDataGetter<ThingDef>(stuffLocal.label.Shorten(), delegate(ThingDef x)
					{
						if (!stuffLocal.stuffProps.CanMake(x))
						{
							return string.Empty;
						}
						return x.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuffLocal).ToString("F1") + ", " + x.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuffLocal).ToString("F1");
					}));
				}
			}
			DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
			where x.IsApparel
			orderby x.BaseMarketValue
			select x, list.ToArray());
		}

		private static void DoTableInternalApparel(ThingDef stuff)
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.IsApparel && (stuff == null || (d.MadeFromStuff && stuff.stuffProps.CanMake(d)))
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("bodyParts", (ThingDef d) => GenText.ToSpaceList(d.apparel.bodyPartGroups.Select((BodyPartGroupDef bp) => bp.defName))), new TableDataGetter<ThingDef>("layers", (ThingDef d) => GenText.ToSpaceList(d.apparel.layers.Select((ApparelLayerDef l) => l.ToString()))), new TableDataGetter<ThingDef>("tags", (ThingDef d) => GenText.ToSpaceList(d.apparel.tags.Select((string t) => t.ToString()))), new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0")), new TableDataGetter<ThingDef>("mktval", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue, stuff).ToString("F0")), new TableDataGetter<ThingDef>("insCold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuff).ToString("F1")), new TableDataGetter<ThingDef>("insHeat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuff).ToString("F1")), new TableDataGetter<ThingDef>("blunt", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, stuff).ToString("F2")), new TableDataGetter<ThingDef>("sharp", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, stuff).ToString("F2")), new TableDataGetter<ThingDef>("SEMultArmor", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierArmor, stuff).ToString("F2")), new TableDataGetter<ThingDef>("SEMultInsuCold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierInsulation_Cold, stuff).ToString("F2")), new TableDataGetter<ThingDef>("SEMultInsuHeat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierInsulation_Heat, stuff).ToString("F2")), new TableDataGetter<ThingDef>("equipTime", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.EquipDelay, stuff).ToString("F1")));
		}

		[DebugOutput]
		public static void ThingsExistingList()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			IEnumerator enumerator = Enum.GetValues(typeof(ThingRequestGroup)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ThingRequestGroup thingRequestGroup = (ThingRequestGroup)enumerator.Current;
					ThingRequestGroup localRg = thingRequestGroup;
					FloatMenuOption item = new FloatMenuOption(localRg.ToString(), delegate
					{
						StringBuilder stringBuilder = new StringBuilder();
						List<Thing> list2 = Find.CurrentMap.listerThings.ThingsInGroup(localRg);
						stringBuilder.AppendLine("Global things in group " + localRg + " (count " + list2.Count + ")");
						Log.Message(DebugLogsUtility.ThingListToUniqueCountString(list2));
					});
					list.Add(item);
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
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void ThingFillageAndPassability()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.passability != 0 || allDef.fillPercent > 0f)
				{
					stringBuilder.Append(allDef.defName + " - pass=" + allDef.passability.ToString() + ", fill=" + allDef.fillPercent.ToStringPercent());
					if (allDef.passability == Traversability.Impassable && allDef.fillPercent < 0.1f)
					{
						stringBuilder.Append("   ALERT, impassable with low fill");
					}
					if (allDef.passability != Traversability.Impassable && allDef.fillPercent > 0.8f)
					{
						stringBuilder.Append("    ALERT, passabile with very high fill");
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void ThingDamageData()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.useHitPoints
			orderby d.category, d.defName
			select d, new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("hp", (ThingDef d) => d.BaseMaxHitPoints.ToString()), new TableDataGetter<ThingDef>("flammability", (ThingDef d) => (!(d.BaseFlammability > 0f)) ? string.Empty : d.BaseFlammability.ToString()), new TableDataGetter<ThingDef>("uses stuff", (ThingDef d) => d.MadeFromStuff.ToStringCheckBlank()), new TableDataGetter<ThingDef>("deterioration rate", (ThingDef d) => (!(d.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 0f)) ? string.Empty : d.GetStatValueAbstract(StatDefOf.DeteriorationRate).ToString()), new TableDataGetter<ThingDef>("days to deterioriate", (ThingDef d) => (!(d.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 0f)) ? string.Empty : ((float)d.BaseMaxHitPoints / d.GetStatValueAbstract(StatDefOf.DeteriorationRate)).ToString()));
		}

		[DebugOutput]
		public static void ThingMasses()
		{
			IOrderedEnumerable<ThingDef> dataSources = from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.category == ThingCategory.Item || x.Minifiable
			where x.thingClass != typeof(MinifiedThing) && x.thingClass != typeof(UnfinishedThing)
			orderby x.GetStatValueAbstract(StatDefOf.Mass), x.GetStatValueAbstract(StatDefOf.MarketValue)
			select x;
			Func<ThingDef, float, string> perPawn = (ThingDef d, float bodySize) => (bodySize * 35f / d.GetStatValueAbstract(StatDefOf.Mass)).ToString("F0");
			Func<ThingDef, string> perNutrition = delegate(ThingDef d)
			{
				if (d.ingestible == null || d.GetStatValueAbstract(StatDefOf.Nutrition) == 0f)
				{
					return string.Empty;
				}
				return (d.GetStatValueAbstract(StatDefOf.Mass) / d.GetStatValueAbstract(StatDefOf.Nutrition)).ToString("F2");
			};
			DebugTables.MakeTablesDialog(dataSources, new TableDataGetter<ThingDef>("defName", delegate(ThingDef d)
			{
				if (d.Minifiable)
				{
					return d.defName + " (minified)";
				}
				string text = d.defName;
				if (!d.EverHaulable)
				{
					text += " (not haulable)";
				}
				return text;
			}), new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Mass).ToString()), new TableDataGetter<ThingDef>("per human", (ThingDef d) => perPawn(d, ThingDefOf.Human.race.baseBodySize)), new TableDataGetter<ThingDef>("per muffalo", (ThingDef d) => perPawn(d, ThingDefOf.Muffalo.race.baseBodySize)), new TableDataGetter<ThingDef>("per dromedary", (ThingDef d) => perPawn(d, ThingDefOf.Dromedary.race.baseBodySize)), new TableDataGetter<ThingDef>("per nutrition", (ThingDef d) => perNutrition(d)), new TableDataGetter<ThingDef>("small volume", (ThingDef d) => (!d.smallVolume) ? string.Empty : "small"));
		}

		[DebugOutput]
		public static void ThingFillPercents()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.fillPercent > 0f
			orderby d.fillPercent descending
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("fillPercent", (ThingDef d) => d.fillPercent.ToStringPercent()), new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()));
		}

		[DebugOutput]
		public static void ThingNutritions()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.ingestible != null
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("market value", (ThingDef d) => d.BaseMarketValue.ToString("F1")), new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Nutrition).ToString("F2")), new TableDataGetter<ThingDef>("nutrition per value", (ThingDef d) => (d.GetStatValueAbstract(StatDefOf.Nutrition) / d.BaseMarketValue).ToString("F3")));
		}

		public static void MakeTablePairsByThing(List<ThingStuffPair> pairList)
		{
			DefMap<ThingDef, float> totalCommMult = new DefMap<ThingDef, float>();
			DefMap<ThingDef, float> totalComm = new DefMap<ThingDef, float>();
			DefMap<ThingDef, int> pairCount = new DefMap<ThingDef, int>();
			foreach (ThingStuffPair pair in pairList)
			{
				ThingStuffPair current = pair;
				ThingDef thing;
				DefMap<ThingDef, float> defMap;
				(defMap = totalCommMult)[thing = current.thing] = defMap[thing] + current.commonalityMultiplier;
				ThingDef thing2;
				(defMap = totalComm)[thing2 = current.thing] = defMap[thing2] + current.Commonality;
				DefMap<ThingDef, int> defMap2;
				ThingDef thing3;
				(defMap2 = pairCount)[thing3 = current.thing] = defMap2[thing3] + 1;
			}
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where pairList.Any((ThingStuffPair pa) => pa.thing == d)
			select d, new TableDataGetter<ThingDef>("thing", (ThingDef t) => t.defName), new TableDataGetter<ThingDef>("pair count", (ThingDef t) => pairCount[t].ToString()), new TableDataGetter<ThingDef>("total commonality multiplier ", (ThingDef t) => totalCommMult[t].ToString("F4")), new TableDataGetter<ThingDef>("total commonality", (ThingDef t) => totalComm[t].ToString("F4")), new TableDataGetter<ThingDef>("generateCommonality", (ThingDef t) => t.generateCommonality.ToString("F4")));
		}

		public static string ToStringEmptyZero(this float f, string format)
		{
			if (f <= 0f)
			{
				return string.Empty;
			}
			return f.ToString(format);
		}

		public static string ToStringPercentEmptyZero(this float f, string format = "F0")
		{
			if (f <= 0f)
			{
				return string.Empty;
			}
			return f.ToStringPercent(format);
		}

		public static string ToStringCheckBlank(this bool b)
		{
			return (!b) ? string.Empty : "✓";
		}
	}
}
