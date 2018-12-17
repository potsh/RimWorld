using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[HasDebugOutput]
	public class PawnGroupMakerUtility
	{
		private static readonly SimpleCurve PawnWeightFactorByMostExpensivePawnCostFractionCurve = new SimpleCurve
		{
			new CurvePoint(0.2f, 0.01f),
			new CurvePoint(0.3f, 0.3f),
			new CurvePoint(0.5f, 1f)
		};

		public static IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, bool warnOnZeroResults = true)
		{
			PawnGroupMaker chosenGroupMaker;
			if (parms.groupKind == null)
			{
				Log.Error("Tried to generate pawns with null pawn group kind def. parms=" + parms);
			}
			else if (parms.faction == null)
			{
				Log.Error("Tried to generate pawn kinds with null faction. parms=" + parms);
			}
			else if (parms.faction.def.pawnGroupMakers.NullOrEmpty())
			{
				Log.Error("Faction " + parms.faction + " of def " + parms.faction.def + " has no any PawnGroupMakers.");
			}
			else if (!TryGetRandomPawnGroupMaker(parms, out chosenGroupMaker))
			{
				Log.Error("Faction " + parms.faction + " of def " + parms.faction.def + " has no usable PawnGroupMakers for parms " + parms);
			}
			else
			{
				using (IEnumerator<Pawn> enumerator = chosenGroupMaker.GeneratePawns(parms, warnOnZeroResults).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Pawn p = enumerator.Current;
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01f7:
			/*Error near IL_01f8: Unexpected return in MoveNext()*/;
		}

		public static IEnumerable<PawnKindDef> GeneratePawnKindsExample(PawnGroupMakerParms parms)
		{
			PawnGroupMaker chosenGroupMaker;
			if (parms.groupKind == null)
			{
				Log.Error("Tried to generate pawn kinds with null pawn group kind def. parms=" + parms);
			}
			else if (parms.faction == null)
			{
				Log.Error("Tried to generate pawn kinds with null faction. parms=" + parms);
			}
			else if (parms.faction.def.pawnGroupMakers.NullOrEmpty())
			{
				Log.Error("Faction " + parms.faction + " of def " + parms.faction.def + " has no any PawnGroupMakers.");
			}
			else if (!TryGetRandomPawnGroupMaker(parms, out chosenGroupMaker))
			{
				Log.Error("Faction " + parms.faction + " of def " + parms.faction.def + " has no usable PawnGroupMakers for parms " + parms);
			}
			else
			{
				using (IEnumerator<PawnKindDef> enumerator = chosenGroupMaker.GeneratePawnKindsExample(parms).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						PawnKindDef p = enumerator.Current;
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01f1:
			/*Error near IL_01f2: Unexpected return in MoveNext()*/;
		}

		private static bool TryGetRandomPawnGroupMaker(PawnGroupMakerParms parms, out PawnGroupMaker pawnGroupMaker)
		{
			if (parms.seed.HasValue)
			{
				Rand.PushState(parms.seed.Value);
			}
			IEnumerable<PawnGroupMaker> source = from gm in parms.faction.def.pawnGroupMakers
			where gm.kindDef == parms.groupKind && gm.CanGenerateFrom(parms)
			select gm;
			bool result = source.TryRandomElementByWeight((PawnGroupMaker gm) => gm.commonality, out pawnGroupMaker);
			if (parms.seed.HasValue)
			{
				Rand.PopState();
			}
			return result;
		}

		public static IEnumerable<PawnGenOption> ChoosePawnGenOptionsByPoints(float pointsTotal, List<PawnGenOption> options, PawnGroupMakerParms groupParms)
		{
			if (groupParms.seed.HasValue)
			{
				Rand.PushState(groupParms.seed.Value);
			}
			float num = MaxPawnCost(groupParms.faction, pointsTotal, groupParms.raidStrategy, groupParms.groupKind);
			List<PawnGenOption> list = new List<PawnGenOption>();
			List<PawnGenOption> list2 = new List<PawnGenOption>();
			float num2 = pointsTotal;
			bool flag = false;
			float highestCost = -1f;
			while (true)
			{
				list.Clear();
				for (int i = 0; i < options.Count; i++)
				{
					PawnGenOption pawnGenOption = options[i];
					if (!(pawnGenOption.Cost > num2) && !(pawnGenOption.Cost > num) && (!groupParms.generateFightersOnly || pawnGenOption.kind.isFighter) && (groupParms.raidStrategy == null || groupParms.raidStrategy.Worker.CanUsePawnGenOption(pawnGenOption, list2)) && (!groupParms.dontUseSingleUseRocketLaunchers || pawnGenOption.kind.weaponTags == null || !pawnGenOption.kind.weaponTags.Contains("GunHeavy")) && (!flag || !pawnGenOption.kind.factionLeader))
					{
						if (pawnGenOption.Cost > highestCost)
						{
							highestCost = pawnGenOption.Cost;
						}
						list.Add(pawnGenOption);
					}
				}
				if (list.Count == 0)
				{
					break;
				}
				Func<PawnGenOption, float> weightSelector = delegate(PawnGenOption gr)
				{
					float selectionWeight = gr.selectionWeight;
					return selectionWeight * PawnWeightFactorByMostExpensivePawnCostFractionCurve.Evaluate(gr.Cost / highestCost);
				};
				PawnGenOption pawnGenOption2 = list.RandomElementByWeight(weightSelector);
				list2.Add(pawnGenOption2);
				num2 -= pawnGenOption2.Cost;
				if (pawnGenOption2.kind.factionLeader)
				{
					flag = true;
				}
			}
			if (list2.Count == 1 && num2 > pointsTotal / 2f)
			{
				Log.Warning("Used only " + (pointsTotal - num2) + " / " + pointsTotal + " points generating for " + groupParms.faction);
			}
			if (groupParms.seed.HasValue)
			{
				Rand.PopState();
			}
			return list2;
		}

		public static float MaxPawnCost(Faction faction, float totalPoints, RaidStrategyDef raidStrategy, PawnGroupKindDef groupKind)
		{
			float a = faction.def.maxPawnCostPerTotalPointsCurve.Evaluate(totalPoints);
			if (raidStrategy != null)
			{
				a = Mathf.Min(a, totalPoints / raidStrategy.minPawns);
			}
			a = Mathf.Max(a, faction.def.MinPointsToGeneratePawnGroup(groupKind) * 1.2f);
			if (raidStrategy != null)
			{
				a = Mathf.Max(a, raidStrategy.Worker.MinMaxAllowedPawnGenOptionCost(faction, groupKind) * 1.2f);
			}
			return a;
		}

		public static bool CanGenerateAnyNormalGroup(Faction faction, float points)
		{
			if (faction.def.pawnGroupMakers == null)
			{
				return false;
			}
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.faction = faction;
			pawnGroupMakerParms.points = points;
			for (int i = 0; i < faction.def.pawnGroupMakers.Count; i++)
			{
				PawnGroupMaker pawnGroupMaker = faction.def.pawnGroupMakers[i];
				if (pawnGroupMaker.kindDef == PawnGroupKindDefOf.Combat && pawnGroupMaker.CanGenerateFrom(pawnGroupMakerParms))
				{
					return true;
				}
			}
			return false;
		}

		[DebugOutput]
		public static void PawnGroupsMade()
		{
			Dialog_DebugOptionListLister.ShowSimpleDebugMenu(from fac in Find.FactionManager.AllFactions
			where !fac.def.pawnGroupMakers.NullOrEmpty()
			select fac, (Faction fac) => fac.Name + " (" + fac.def.defName + ")", delegate(Faction fac)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("FACTION: " + fac.Name + " (" + fac.def.defName + ") min=" + fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
				Action<float> action = delegate(float points)
				{
					if (!(points < fac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)))
					{
						PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
						{
							groupKind = PawnGroupKindDefOf.Combat,
							tile = Find.CurrentMap.Tile,
							points = points,
							faction = fac
						};
						sb.AppendLine("Group with " + pawnGroupMakerParms.points + " points (max option cost: " + MaxPawnCost(fac, points, RaidStrategyDefOf.ImmediateAttack, PawnGroupKindDefOf.Combat) + ")");
						float num = 0f;
						foreach (Pawn item in GeneratePawns(pawnGroupMakerParms, warnOnZeroResults: false).OrderBy((Pawn pa) => pa.kindDef.combatPower))
						{
							string text = (item.equipment.Primary == null) ? "no-equipment" : item.equipment.Primary.Label;
							Apparel apparel = item.apparel.FirstApparelOnBodyPartGroup(BodyPartGroupDefOf.Torso);
							string text2 = (apparel == null) ? "shirtless" : apparel.LabelCap;
							sb.AppendLine("  " + item.kindDef.combatPower.ToString("F0").PadRight(6) + item.kindDef.defName + ", " + text + ", " + text2);
							num += item.kindDef.combatPower;
						}
						sb.AppendLine("         totalCost " + num);
						sb.AppendLine();
					}
				};
				foreach (float item2 in Dialog_DebugActionsMenu.PointsOptions(extended: false))
				{
					float obj = item2;
					action(obj);
				}
				Log.Message(sb.ToString());
			});
		}

		public static bool TryGetRandomFactionForCombatPawnGroup(float points, out Faction faction, Predicate<Faction> validator = null, bool allowNonHostileToPlayer = false, bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true)
		{
			List<Faction> source = (from f in Find.FactionManager.AllFactions
			where (allowHidden || !f.def.hidden) && (allowDefeated || !f.defeated) && (allowNonHumanlike || f.def.humanlikeFaction) && (allowNonHostileToPlayer || f.HostileTo(Faction.OfPlayer)) && f.def.pawnGroupMakers != null && f.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat) && (validator == null || validator(f)) && points >= f.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)
			select f).ToList();
			return source.TryRandomElementByWeight((Faction f) => f.def.RaidCommonalityFromPoints(points), out faction);
		}
	}
}