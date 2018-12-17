using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsTextGen
	{
		[DebugOutput]
		[Category("Text generation")]
		public static void FlavorfulCombatTest()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			IEnumerable<ManeuverDef> maneuvers = DefDatabase<ManeuverDef>.AllDefsListForReading;
			Func<ManeuverDef, RulePackDef>[] results = new Func<ManeuverDef, RulePackDef>[5]
			{
				(ManeuverDef m) => new RulePackDef[4]
				{
					m.combatLogRulesHit,
					m.combatLogRulesDeflect,
					m.combatLogRulesMiss,
					m.combatLogRulesDodge
				}.RandomElement(),
				(ManeuverDef m) => m.combatLogRulesHit,
				(ManeuverDef m) => m.combatLogRulesDeflect,
				(ManeuverDef m) => m.combatLogRulesMiss,
				(ManeuverDef m) => m.combatLogRulesDodge
			};
			string[] array = new string[5]
			{
				"(random)",
				"Hit",
				"Deflect",
				"Miss",
				"Dodge"
			};
			foreach (Pair<ManeuverDef, int> item2 in maneuvers.Concat(null).Cross(Enumerable.Range(0, array.Length)))
			{
				DebugMenuOption item = new DebugMenuOption(string.Format("{0}/{1}", (item2.First != null) ? item2.First.defName : "(random)", array[item2.Second]), DebugMenuOptionMode.Action, delegate
				{
					CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
					{
						StringBuilder stringBuilder7 = new StringBuilder();
						for (int num2 = 0; num2 < 100; num2++)
						{
							ManeuverDef maneuver = item2.First;
							if (maneuver == null)
							{
								maneuver = maneuvers.RandomElement();
							}
							RulePackDef rulePackDef = results[item2.Second](maneuver);
							List<BodyPartRecord> list8 = null;
							List<bool> list9 = null;
							if (rulePackDef == maneuver.combatLogRulesHit)
							{
								list8 = new List<BodyPartRecord>();
								list9 = new List<bool>();
								bodyPartCreator(list8, list9);
							}
							Pair<ThingDef, Tool> pair = (from ttp in (from td in DefDatabase<ThingDef>.AllDefsListForReading
							where td.IsMeleeWeapon && !td.tools.NullOrEmpty()
							select td).SelectMany((ThingDef td) => from tool in td.tools
							select new Pair<ThingDef, Tool>(td, tool))
							where ttp.Second.capacities.Contains(maneuver.requiredCapacity)
							select ttp).RandomElement();
							BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(rulePackDef, alwaysShowInCompact: false, CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), (pair.Second == null) ? ImplementOwnerTypeDefOf.Bodypart : ImplementOwnerTypeDefOf.Weapon, (pair.Second == null) ? "body part" : pair.Second.label, pair.First);
							battleLogEntry_MeleeCombat.FillTargets(list8, list9, battleLogEntry_MeleeCombat.RuleDef.defName.Contains("Deflect"));
							battleLogEntry_MeleeCombat.Debug_OverrideTicks(Rand.Int);
							stringBuilder7.AppendLine(battleLogEntry_MeleeCombat.ToGameStringFromPOV(null));
						}
						Log.Message(stringBuilder7.ToString());
					});
				});
				list.Add(item);
			}
			int rf;
			for (rf = 0; rf < 2; rf++)
			{
				list.Add(new DebugMenuOption((rf != 0) ? "Ranged fire burst" : "Ranged fire singleshot", DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder6 = new StringBuilder();
					for (int num = 0; num < 100; num++)
					{
						ThingDef thingDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement();
						bool flag = Rand.Value < 0.2f;
						bool flag2 = !flag && Rand.Value < 0.95f;
						BattleLogEntry_RangedFire battleLogEntry_RangedFire = new BattleLogEntry_RangedFire(CombatLogTester.GenerateRandom(), (!flag) ? CombatLogTester.GenerateRandom() : null, (!flag2) ? thingDef : null, null, rf != 0);
						battleLogEntry_RangedFire.Debug_OverrideTicks(Rand.Int);
						stringBuilder6.AppendLine(battleLogEntry_RangedFire.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder6.ToString());
				}));
			}
			list.Add(new DebugMenuOption("Ranged impact hit", DebugMenuOptionMode.Action, delegate
			{
				CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
				{
					StringBuilder stringBuilder5 = new StringBuilder();
					for (int n = 0; n < 100; n++)
					{
						ThingDef weaponDef3 = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement();
						List<BodyPartRecord> list6 = new List<BodyPartRecord>();
						List<bool> list7 = new List<bool>();
						bodyPartCreator(list6, list7);
						Pawn pawn2 = CombatLogTester.GenerateRandom();
						BattleLogEntry_RangedImpact battleLogEntry_RangedImpact3 = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), pawn2, pawn2, weaponDef3, null, ThingDefOf.Wall);
						battleLogEntry_RangedImpact3.FillTargets(list6, list7, Rand.Chance(0.5f));
						battleLogEntry_RangedImpact3.Debug_OverrideTicks(Rand.Int);
						stringBuilder5.AppendLine(battleLogEntry_RangedImpact3.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder5.ToString());
				});
			}));
			list.Add(new DebugMenuOption("Ranged impact miss", DebugMenuOptionMode.Action, delegate
			{
				StringBuilder stringBuilder4 = new StringBuilder();
				for (int l = 0; l < 100; l++)
				{
					ThingDef weaponDef2 = (from td in DefDatabase<ThingDef>.AllDefsListForReading
					where td.IsRangedWeapon
					select td).RandomElement();
					BattleLogEntry_RangedImpact battleLogEntry_RangedImpact2 = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), null, CombatLogTester.GenerateRandom(), weaponDef2, null, ThingDefOf.Wall);
					battleLogEntry_RangedImpact2.Debug_OverrideTicks(Rand.Int);
					stringBuilder4.AppendLine(battleLogEntry_RangedImpact2.ToGameStringFromPOV(null));
				}
				Log.Message(stringBuilder4.ToString());
			}));
			list.Add(new DebugMenuOption("Ranged impact hit incorrect", DebugMenuOptionMode.Action, delegate
			{
				CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
				{
					StringBuilder stringBuilder3 = new StringBuilder();
					for (int k = 0; k < 100; k++)
					{
						ThingDef weaponDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement();
						List<BodyPartRecord> list4 = new List<BodyPartRecord>();
						List<bool> list5 = new List<bool>();
						bodyPartCreator(list4, list5);
						BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), weaponDef, null, ThingDefOf.Wall);
						battleLogEntry_RangedImpact.FillTargets(list4, list5, Rand.Chance(0.5f));
						battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
						stringBuilder3.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder3.ToString());
				});
			}));
			foreach (RulePackDef item3 in from def in DefDatabase<RulePackDef>.AllDefsListForReading
			where def.defName.Contains("Transition") && !def.defName.Contains("Include")
			select def)
			{
				list.Add(new DebugMenuOption(item3.defName, DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					for (int j = 0; j < 100; j++)
					{
						Pawn pawn = CombatLogTester.GenerateRandom();
						Pawn initiator = CombatLogTester.GenerateRandom();
						BodyPartRecord partRecord = pawn.health.hediffSet.GetNotMissingParts().RandomElement();
						BattleLogEntry_StateTransition battleLogEntry_StateTransition = new BattleLogEntry_StateTransition(pawn, item3, initiator, HediffMaker.MakeHediff(DefDatabase<HediffDef>.AllDefsListForReading.RandomElement(), pawn, partRecord), pawn.RaceProps.body.AllParts.RandomElement());
						battleLogEntry_StateTransition.Debug_OverrideTicks(Rand.Int);
						stringBuilder2.AppendLine(battleLogEntry_StateTransition.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder2.ToString());
				}));
			}
			foreach (RulePackDef item4 in from def in DefDatabase<RulePackDef>.AllDefsListForReading
			where def.defName.Contains("DamageEvent") && !def.defName.Contains("Include")
			select def)
			{
				list.Add(new DebugMenuOption(item4.defName, DebugMenuOptionMode.Action, delegate
				{
					CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int i = 0; i < 100; i++)
						{
							List<BodyPartRecord> list2 = new List<BodyPartRecord>();
							List<bool> list3 = new List<bool>();
							bodyPartCreator(list2, list3);
							Pawn recipient = CombatLogTester.GenerateRandom();
							BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(recipient, item4);
							battleLogEntry_DamageTaken.FillTargets(list2, list3, deflected: false);
							battleLogEntry_DamageTaken.Debug_OverrideTicks(Rand.Int);
							stringBuilder.AppendLine(battleLogEntry_DamageTaken.ToGameStringFromPOV(null));
						}
						Log.Message(stringBuilder.ToString());
					});
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		private static void CreateDamagedDestroyedMenu(Action<Action<List<BodyPartRecord>, List<bool>>> callback)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			IEnumerable<int> damagedes = Enumerable.Range(0, 5);
			IEnumerable<int> destroyedes = Enumerable.Range(0, 5);
			foreach (Pair<int, int> item2 in damagedes.Concat(-1).Cross(destroyedes.Concat(-1)))
			{
				DebugMenuOption item = new DebugMenuOption(string.Format("{0} damaged/{1} destroyed", (item2.First != -1) ? item2.First.ToString() : "(random)", (item2.Second != -1) ? item2.Second.ToString() : "(random)"), DebugMenuOptionMode.Action, delegate
				{
					callback(delegate(List<BodyPartRecord> bodyparts, List<bool> flags)
					{
						int num = item2.First;
						int destroyed = item2.Second;
						if (num == -1)
						{
							num = damagedes.RandomElement();
						}
						if (destroyed == -1)
						{
							destroyed = destroyedes.RandomElement();
						}
						Pair<BodyPartRecord, bool>[] source = (from idx in Enumerable.Range(0, num + destroyed)
						select new Pair<BodyPartRecord, bool>(BodyDefOf.Human.AllParts.RandomElement(), idx < destroyed)).InRandomOrder().ToArray();
						bodyparts.Clear();
						flags.Clear();
						bodyparts.AddRange(from part in source
						select part.First);
						flags.AddRange(from part in source
						select part.Second);
					});
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		[Category("Text generation")]
		public static void ArtDescsSpecificTale()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (TaleDef item2 in from def in DefDatabase<TaleDef>.AllDefs
			orderby def.defName
			select def)
			{
				TaleDef localDef = item2;
				FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
				{
					LogSpecificTale(localDef, 40);
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		[Category("Text generation")]
		public static void NamesFromRulepack()
		{
			IEnumerable<RulePackDef> first = from f in DefDatabase<FactionDef>.AllDefsListForReading
			select f.factionNameMaker;
			IEnumerable<RulePackDef> second = from f in DefDatabase<FactionDef>.AllDefsListForReading
			select f.settlementNameMaker;
			IEnumerable<RulePackDef> second2 = from f in DefDatabase<FactionDef>.AllDefsListForReading
			select f.playerInitialSettlementNameMaker;
			IEnumerable<RulePackDef> second3 = from f in DefDatabase<FactionDef>.AllDefsListForReading
			select f.pawnNameMaker;
			IEnumerable<RulePackDef> second4 = from d in DefDatabase<RulePackDef>.AllDefsListForReading
			where d.defName.Contains("Namer")
			select d;
			IOrderedEnumerable<RulePackDef> orderedEnumerable = from d in (from d in first.Concat(second).Concat(second2).Concat(second3)
				.Concat(second4)
			where d != null
			select d).Distinct()
			orderby d.defName
			select d;
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (RulePackDef item2 in orderedEnumerable)
			{
				RulePackDef localNamer = item2;
				FloatMenuOption item = new FloatMenuOption(localNamer.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Testing RulePack " + localNamer.defName + " as  a name generator:");
					for (int i = 0; i < 200; i++)
					{
						string text = (i % 2 != 0) ? null : "Smithee";
						StringBuilder stringBuilder2 = stringBuilder;
						RulePackDef rootPack = localNamer;
						string testPawnNameSymbol = text;
						stringBuilder2.AppendLine(NameGenerator.GenerateName(rootPack, null, appendNumberIfNameUsed: false, null, testPawnNameSymbol));
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		[Category("Text generation")]
		public static void DatabaseTalesList()
		{
			Find.TaleManager.LogTales();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		[Category("Text generation")]
		public static void DatabaseTalesInterest()
		{
			Find.TaleManager.LogTaleInterestSummary();
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		[Category("Text generation")]
		public static void ArtDescsDatabaseTales()
		{
			LogTales(from t in Find.TaleManager.AllTalesListForReading
			where t.def.usableForArt
			select t);
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		[Category("Text generation")]
		public static void ArtDescsRandomTales()
		{
			int num = 40;
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < num; i++)
			{
				list.Add(TaleFactory.MakeRandomTestTale());
			}
			LogTales(list);
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		[Category("Text generation")]
		public static void ArtDescsTaleless()
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < 20; i++)
			{
				list.Add(null);
			}
			LogTales(list);
		}

		[DebugOutput]
		[Category("Text generation")]
		public static void InteractionLogs()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (InteractionDef item in DefDatabase<InteractionDef>.AllDefsListForReading)
			{
				list.Add(new DebugMenuOption(item.defName, DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
					Pawn recipient = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
					for (int i = 0; i < 100; i++)
					{
						PlayLogEntry_Interaction playLogEntry_Interaction = new PlayLogEntry_Interaction(item, pawn, recipient, null);
						stringBuilder.AppendLine(playLogEntry_Interaction.ToGameStringFromPOV(pawn));
					}
					Log.Message(stringBuilder.ToString());
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		private static void LogSpecificTale(TaleDef def, int count)
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < count; i++)
			{
				list.Add(TaleFactory.MakeRandomTestTale(def));
			}
			LogTales(list);
		}

		private static void LogTales(IEnumerable<Tale> tales)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (Tale tale in tales)
			{
				TaleReference tr = new TaleReference(tale);
				stringBuilder.AppendLine(RandomArtworkName(tr));
				stringBuilder.AppendLine(RandomArtworkDescription(tr));
				stringBuilder.AppendLine();
				num++;
				if (num % 20 == 0)
				{
					Log.Message(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
				}
			}
			if (!stringBuilder.ToString().NullOrEmpty())
			{
				Log.Message(stringBuilder.ToString());
			}
		}

		private static string RandomArtworkName(TaleReference tr)
		{
			RulePackDef extraInclude = null;
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				extraInclude = RulePackDefOf.NamerArtSculpture;
				break;
			case 1:
				extraInclude = RulePackDefOf.NamerArtWeaponMelee;
				break;
			case 2:
				extraInclude = RulePackDefOf.NamerArtWeaponGun;
				break;
			case 3:
				extraInclude = RulePackDefOf.NamerArtFurniture;
				break;
			case 4:
				extraInclude = RulePackDefOf.NamerArtSarcophagusPlate;
				break;
			}
			return tr.GenerateText(TextGenerationPurpose.ArtName, extraInclude);
		}

		private static string RandomArtworkDescription(TaleReference tr)
		{
			RulePackDef extraInclude = null;
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				extraInclude = RulePackDefOf.ArtDescription_Sculpture;
				break;
			case 1:
				extraInclude = RulePackDefOf.ArtDescription_WeaponMelee;
				break;
			case 2:
				extraInclude = RulePackDefOf.ArtDescription_WeaponGun;
				break;
			case 3:
				extraInclude = RulePackDefOf.ArtDescription_Furniture;
				break;
			case 4:
				extraInclude = RulePackDefOf.ArtDescription_SarcophagusPlate;
				break;
			}
			return tr.GenerateText(TextGenerationPurpose.ArtDescription, extraInclude);
		}
	}
}
