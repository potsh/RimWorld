using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_TraderTracker : IExposable
	{
		private Pawn pawn;

		public TraderKindDef traderKind;

		private List<Pawn> soldPrisoners = new List<Pawn>();

		public IEnumerable<Thing> Goods
		{
			get
			{
				Lord lord = pawn.GetLord();
				if (lord == null || !(lord.LordJob is LordJob_TradeWithColony))
				{
					for (int k = 0; k < pawn.inventory.innerContainer.Count; k++)
					{
						Thing t = pawn.inventory.innerContainer[k];
						if (!pawn.inventory.NotForSale(t))
						{
							yield return t;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				if (lord != null)
				{
					for (int j = 0; j < lord.ownedPawns.Count; j++)
					{
						Pawn p = lord.ownedPawns[j];
						switch (p.GetTraderCaravanRole())
						{
						case TraderCaravanRole.Carrier:
						{
							int i = 0;
							if (i < p.inventory.innerContainer.Count)
							{
								yield return p.inventory.innerContainer[i];
								/*Error: Unable to find new state assignment for yield return*/;
							}
							break;
						}
						case TraderCaravanRole.Chattel:
							if (!soldPrisoners.Contains(p))
							{
								yield return (Thing)p;
								/*Error: Unable to find new state assignment for yield return*/;
							}
							break;
						}
					}
				}
			}
		}

		public int RandomPriceFactorSeed => Gen.HashCombineInt(pawn.thingIDNumber, 1149275593);

		public string TraderName => pawn.LabelShort;

		public bool CanTradeNow => !pawn.Dead && pawn.Spawned && pawn.mindState.wantsToTradeWithColony && pawn.CanCasuallyInteractNow() && !pawn.Downed && !pawn.IsPrisoner && pawn.Faction != Faction.OfPlayer && (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayer)) && Goods.Any((Thing x) => traderKind.WillTrade(x.def));

		public Pawn_TraderTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref traderKind, "traderKind");
			Scribe_Collections.Look(ref soldPrisoners, "soldPrisoners", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				soldPrisoners.RemoveAll((Pawn x) => x == null);
			}
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			IEnumerable<Thing> items = from x in pawn.Map.listerThings.AllThings
			where x.def.category == ThingCategory.Item && TradeUtility.PlayerSellableNow(x) && !x.Position.Fogged(x.Map) && (((_003CColonyThingsWillingToBuy_003Ec__Iterator1)/*Error near IL_0042: stateMachine*/)._0024this.pawn.Map.areaManager.Home[x.Position] || x.IsInAnyStorage()) && ((_003CColonyThingsWillingToBuy_003Ec__Iterator1)/*Error near IL_0042: stateMachine*/)._0024this.ReachableForTrade(x)
			select x;
			using (IEnumerator<Thing> enumerator = items.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Thing t = enumerator.Current;
					yield return t;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (pawn.GetLord() != null)
			{
				using (IEnumerator<Pawn> enumerator2 = (from x in TradeUtility.AllSellableColonyPawns(pawn.Map)
				where !x.Downed && ((_003CColonyThingsWillingToBuy_003Ec__Iterator1)/*Error near IL_011d: stateMachine*/)._0024this.ReachableForTrade(x)
				select x).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Pawn p = enumerator2.Current;
						yield return (Thing)p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01b8:
			/*Error near IL_01b9: Unexpected return in MoveNext()*/;
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			if (Goods.Contains(toGive))
			{
				Log.Error("Tried to add " + toGive + " to stock (pawn's trader tracker), but it's already here.");
			}
			else
			{
				Pawn pawn = toGive as Pawn;
				if (pawn != null)
				{
					pawn.PreTraded(TradeAction.PlayerSells, playerNegotiator, this.pawn);
					AddPawnToStock(pawn);
				}
				else
				{
					Thing thing = toGive.SplitOff(countToGive);
					thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this.pawn);
					Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this.pawn, thing);
					if (thing2 != null)
					{
						if (!thing2.TryAbsorbStack(thing, respectStackLimit: false))
						{
							thing.Destroy();
						}
					}
					else
					{
						AddThingToRandomInventory(thing);
					}
				}
			}
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Pawn pawn = toGive as Pawn;
			if (pawn != null)
			{
				pawn.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this.pawn);
				pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.Undefined);
				if (soldPrisoners.Contains(pawn))
				{
					soldPrisoners.Remove(pawn);
				}
			}
			else
			{
				IntVec3 positionHeld = toGive.PositionHeld;
				Map mapHeld = toGive.MapHeld;
				Thing thing = toGive.SplitOff(countToGive);
				thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this.pawn);
				if (GenPlace.TryPlaceThing(thing, positionHeld, mapHeld, ThingPlaceMode.Near))
				{
					this.pawn.GetLord()?.extraForbiddenThings.Add(thing);
				}
				else
				{
					Log.Error("Could not place bought thing " + thing + " at " + positionHeld);
					thing.Destroy();
				}
			}
		}

		private void AddPawnToStock(Pawn newPawn)
		{
			if (!newPawn.Spawned)
			{
				GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map);
			}
			if (newPawn.Faction != pawn.Faction)
			{
				newPawn.SetFaction(pawn.Faction);
			}
			if (newPawn.RaceProps.Humanlike)
			{
				newPawn.kindDef = PawnKindDefOf.Slave;
			}
			Lord lord = pawn.GetLord();
			if (lord == null)
			{
				newPawn.Destroy();
				Log.Error("Tried to sell pawn " + newPawn + " to " + pawn + ", but " + pawn + " has no lord. Traders without lord can't buy pawns.");
			}
			else
			{
				if (newPawn.RaceProps.Humanlike)
				{
					soldPrisoners.Add(newPawn);
				}
				lord.AddPawn(newPawn);
			}
		}

		private void AddThingToRandomInventory(Thing thing)
		{
			Lord lord = pawn.GetLord();
			IEnumerable<Pawn> source = Enumerable.Empty<Pawn>();
			if (lord != null)
			{
				source = from x in lord.ownedPawns
				where x.GetTraderCaravanRole() == TraderCaravanRole.Carrier
				select x;
			}
			if (source.Any())
			{
				if (!source.RandomElement().inventory.innerContainer.TryAdd(thing))
				{
					thing.Destroy();
				}
			}
			else if (!pawn.inventory.innerContainer.TryAdd(thing))
			{
				thing.Destroy();
			}
		}

		private bool ReachableForTrade(Thing thing)
		{
			if (pawn.Map != thing.Map)
			{
				return false;
			}
			return pawn.Map.reachability.CanReach(pawn.Position, thing, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Some);
		}
	}
}
