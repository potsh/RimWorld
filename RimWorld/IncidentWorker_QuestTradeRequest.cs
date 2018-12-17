using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestTradeRequest : IncidentWorker
	{
		private const float MinNonTravelTimeFractionOfTravelTime = 0.35f;

		private const float MinNonTravelTimeDays = 6f;

		private const int MaxTileDistance = 36;

		private static readonly IntRange BaseValueWantedRange = new IntRange(500, 2500);

		private static readonly SimpleCurve ValueWantedFactorFromWealthCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0.3f),
			new CurvePoint(50000f, 1f),
			new CurvePoint(300000f, 2f)
		};

		private static readonly FloatRange RewardValueFactorRange = new FloatRange(1.5f, 2.1f);

		private static readonly SimpleCurve RewardValueFactorFromWealthCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1.15f),
			new CurvePoint(50000f, 1f),
			new CurvePoint(300000f, 0.85f)
		};

		private static Dictionary<ThingDef, int> requestCountDict = new Dictionary<ThingDef, int>();

		private static List<Map> tmpAvailableMaps = new List<Map>();

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			if (!TryGetRandomAvailableTargetMap(out Map map))
			{
				return false;
			}
			SettlementBase settlementBase = RandomNearbyTradeableSettlement(map.Tile);
			if (settlementBase == null)
			{
				return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!TryGetRandomAvailableTargetMap(out Map map))
			{
				return false;
			}
			SettlementBase settlementBase = RandomNearbyTradeableSettlement(map.Tile);
			if (settlementBase == null)
			{
				return false;
			}
			TradeRequestComp component = settlementBase.GetComponent<TradeRequestComp>();
			if (!TryGenerateTradeRequest(component, map))
			{
				return false;
			}
			string text = "LetterCaravanRequest".Translate(settlementBase.Label, TradeRequestUtility.RequestedThingLabel(component.requestThingDef, component.requestCount).CapitalizeFirst(), (component.requestThingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)component.requestCount).ToStringMoney("F0"), GenThing.ThingsToCommaList(component.rewards, useAnd: true).CapitalizeFirst(), GenThing.GetMarketValue(component.rewards).ToStringMoney("F0"), (component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F0"), CaravanArrivalTimeEstimator.EstimatedTicksToArrive(map.Tile, settlementBase.Tile, null).ToStringTicksToDays("0.#"));
			GenThing.TryAppendSingleRewardInfo(ref text, component.rewards);
			Find.LetterStack.ReceiveLetter("LetterLabelCaravanRequest".Translate(), text, LetterDefOf.PositiveEvent, settlementBase, settlementBase.Faction);
			return true;
		}

		public bool TryGenerateTradeRequest(TradeRequestComp target, Map map)
		{
			int num = RandomOfferDurationTicks(map.Tile, target.parent.Tile);
			if (num < 1)
			{
				return false;
			}
			if (!TryFindRandomRequestedThingDef(map, out target.requestThingDef, out target.requestCount))
			{
				return false;
			}
			target.rewards.ClearAndDestroyContents();
			target.rewards.TryAddRangeOrTransfer(GenerateRewardsFor(target.requestThingDef, target.requestCount, target.parent.Faction, map), canMergeWithExistingStacks: true, destroyLeftover: true);
			target.expiration = Find.TickManager.TicksGame + num;
			return true;
		}

		public static SettlementBase RandomNearbyTradeableSettlement(int originTile)
		{
			return Find.WorldObjects.SettlementBases.Where(delegate(SettlementBase settlement)
			{
				if (!settlement.Visitable || settlement.GetComponent<TradeRequestComp>() == null || settlement.GetComponent<TradeRequestComp>().ActiveRequest)
				{
					return false;
				}
				return Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f && Find.WorldReachability.CanReach(originTile, settlement.Tile);
			}).RandomElementWithFallback();
		}

		private static bool TryFindRandomRequestedThingDef(Map map, out ThingDef thingDef, out int count)
		{
			requestCountDict.Clear();
			Func<ThingDef, bool> globalValidator = delegate(ThingDef td)
			{
				if (td.BaseMarketValue / td.BaseMass < 5f)
				{
					return false;
				}
				if (!td.alwaysHaulable)
				{
					return false;
				}
				CompProperties_Rottable compProperties = td.GetCompProperties<CompProperties_Rottable>();
				if (compProperties != null && compProperties.daysToRotStart < 10f)
				{
					return false;
				}
				if (td.ingestible != null && td.ingestible.HumanEdible)
				{
					return false;
				}
				if (td == ThingDefOf.Silver)
				{
					return false;
				}
				if (!td.PlayerAcquirable)
				{
					return false;
				}
				int num = RandomRequestCount(td, map);
				requestCountDict.Add(td, num);
				if (!PlayerItemAccessibilityUtility.PossiblyAccessible(td, num, map))
				{
					return false;
				}
				if (!PlayerItemAccessibilityUtility.PlayerCanMake(td, map))
				{
					return false;
				}
				if (td.thingSetMakerTags != null && td.thingSetMakerTags.Contains("RewardSpecial"))
				{
					return false;
				}
				return true;
			};
			if ((from td in ThingSetMakerUtility.allGeneratableItems
			where globalValidator(td)
			select td).TryRandomElement(out thingDef))
			{
				count = requestCountDict[thingDef];
				return true;
			}
			count = 0;
			return false;
		}

		private bool TryGetRandomAvailableTargetMap(out Map map)
		{
			tmpAvailableMaps.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome && AtLeast2HealthyColonists(maps[i]) && RandomNearbyTradeableSettlement(maps[i].Tile) != null)
				{
					tmpAvailableMaps.Add(maps[i]);
				}
			}
			bool result = tmpAvailableMaps.TryRandomElement(out map);
			tmpAvailableMaps.Clear();
			return result;
		}

		private static int RandomRequestCount(ThingDef thingDef, Map map)
		{
			float num = (float)BaseValueWantedRange.RandomInRange;
			num *= ValueWantedFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal);
			return ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(num / thingDef.BaseMarketValue)));
		}

		private static List<Thing> GenerateRewardsFor(ThingDef thingDef, int quantity, Faction faction, Map map)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.totalMarketValueRange = RewardValueFactorRange * RewardValueFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal) * thingDef.BaseMarketValue * (float)quantity;
			parms.validator = ((ThingDef td) => td != thingDef);
			List<Thing> list = null;
			for (int i = 0; i < 10; i++)
			{
				if (list != null)
				{
					for (int j = 0; j < list.Count; j++)
					{
						list[j].Destroy();
					}
					list = null;
				}
				list = ThingSetMakerDefOf.Reward_TradeRequest.root.Generate(parms);
				float num = 0f;
				for (int k = 0; k < list.Count; k++)
				{
					num += list[k].MarketValue * (float)list[k].stackCount;
				}
				if (num > thingDef.BaseMarketValue * (float)quantity)
				{
					break;
				}
			}
			return list;
		}

		private int RandomOfferDurationTicks(int tileIdFrom, int tileIdTo)
		{
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			int num = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(tileIdFrom, tileIdTo, null);
			float num2 = (float)num / 60000f;
			int num3 = Mathf.CeilToInt(Mathf.Max(num2 + 6f, num2 * 1.35f));
			int num4 = num3;
			IntRange questSiteTimeoutDaysRange = SiteTuning.QuestSiteTimeoutDaysRange;
			if (num4 > questSiteTimeoutDaysRange.max)
			{
				return -1;
			}
			int num5 = Mathf.Max(randomInRange, num3);
			return 60000 * num5;
		}

		private bool AtLeast2HealthyColonists(Map map)
		{
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsFreeColonist && !HealthAIUtility.ShouldSeekMedicalRest(list[i]))
				{
					num++;
					if (num >= 2)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
