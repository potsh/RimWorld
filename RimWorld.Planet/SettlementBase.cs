using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class SettlementBase : MapParent, ITrader
	{
		public SettlementBase_TraderTracker trader;

		public List<Pawn> previouslyGeneratedInhabitants = new List<Pawn>();

		public static readonly Texture2D ShowSellableItemsCommand = ContentFinder<Texture2D>.Get("UI/Commands/SellableItems");

		public static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan");

		public static readonly Texture2D AttackCommand = ContentFinder<Texture2D>.Get("UI/Commands/AttackSettlement");

		protected override bool UseGenericEnterMapFloatMenuOption => !Attackable;

		public virtual bool Visitable => base.Faction != Faction.OfPlayer && (base.Faction == null || !base.Faction.HostileTo(Faction.OfPlayer));

		public virtual bool Attackable => base.Faction != Faction.OfPlayer;

		public TraderKindDef TraderKind
		{
			get
			{
				if (trader == null)
				{
					return null;
				}
				return trader.TraderKind;
			}
		}

		public IEnumerable<Thing> Goods
		{
			get
			{
				if (trader == null)
				{
					return null;
				}
				return trader.StockListForReading;
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				if (trader == null)
				{
					return 0;
				}
				return trader.RandomPriceFactorSeed;
			}
		}

		public string TraderName
		{
			get
			{
				if (trader == null)
				{
					return null;
				}
				return trader.TraderName;
			}
		}

		public bool CanTradeNow
		{
			get
			{
				if (trader == null)
				{
					return false;
				}
				return trader.CanTradeNow;
			}
		}

		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				if (trader == null)
				{
					return 0f;
				}
				return trader.TradePriceImprovementOffsetForPlayer;
			}
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			if (trader == null)
			{
				return null;
			}
			return trader.ColonyThingsWillingToBuy(playerNegotiator);
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			trader.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			trader.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref previouslyGeneratedInhabitants, "previouslyGeneratedInhabitants", LookMode.Reference);
			Scribe_Deep.Look(ref trader, "trader", this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				previouslyGeneratedInhabitants.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (trader != null)
			{
				trader.TraderTrackerTick();
			}
		}

		public override void Notify_MyMapRemoved(Map map)
		{
			base.Notify_MyMapRemoved(map);
			for (int num = previouslyGeneratedInhabitants.Count - 1; num >= 0; num--)
			{
				Pawn pawn = previouslyGeneratedInhabitants[num];
				if (pawn.DestroyedOrNull() || !pawn.IsWorldPawn())
				{
					previouslyGeneratedInhabitants.RemoveAt(num);
				}
			}
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			return !base.Map.IsPlayerHome && !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			if (trader != null)
			{
				trader.TryDestroyStock();
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (base.Faction != Faction.OfPlayer)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += base.Faction.PlayerRelationKind.GetLabel();
				if (!base.Faction.def.hidden)
				{
					text = text + " (" + base.Faction.PlayerGoodwill.ToStringWithSign() + ")";
				}
			}
			return text;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g = enumerator.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (TraderKind != null)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "CommandShowSellableItems".Translate(),
					defaultDesc = "CommandShowSellableItemsDesc".Translate(),
					icon = ShowSellableItemsCommand,
					action = delegate
					{
						Find.WindowStack.Add(new Dialog_SellableItems(((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0113: stateMachine*/)._0024this.TraderKind));
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (base.Faction != Faction.OfPlayer && !PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.FormCaravan))
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "CommandFormCaravan".Translate(),
					defaultDesc = "CommandFormCaravanDesc".Translate(),
					icon = FormCaravanCommand,
					action = delegate
					{
						Find.Tutor.learningReadout.TryActivateConcept(ConceptDefOf.FormCaravan);
						Messages.Message("MessageSelectOwnBaseToFormCaravan".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_01fe:
			/*Error near IL_01ff: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
		{
			_003CGetCaravanGizmos_003Ec__Iterator1 _003CGetCaravanGizmos_003Ec__Iterator = (_003CGetCaravanGizmos_003Ec__Iterator1)/*Error near IL_0040: stateMachine*/;
			if (CanTradeNow && CaravanVisitUtility.SettlementVisitedNow(caravan) == this)
			{
				yield return (Gizmo)CaravanVisitUtility.TradeCommand(caravan);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if ((bool)CaravanArrivalAction_OfferGifts.CanOfferGiftsTo(caravan, this))
			{
				yield return (Gizmo)FactionGiftUtility.OfferGiftsCommand(caravan, this);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			using (IEnumerator<Gizmo> enumerator = base.GetCaravanGizmos(caravan).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g = enumerator.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (Attackable)
			{
				yield return (Gizmo)new Command_Action
				{
					icon = AttackCommand,
					defaultLabel = "CommandAttackSettlement".Translate(),
					defaultDesc = "CommandAttackSettlementDesc".Translate(),
					action = delegate
					{
						SettlementUtility.Attack(caravan, _003CGetCaravanGizmos_003Ec__Iterator._0024this);
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_022e:
			/*Error near IL_022f: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			using (IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(caravan).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption o = enumerator.Current;
					yield return o;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (CaravanVisitUtility.SettlementVisitedNow(caravan) != this)
			{
				using (IEnumerator<FloatMenuOption> enumerator2 = CaravanArrivalAction_VisitSettlement.GetFloatMenuOptions(caravan, this).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						FloatMenuOption f3 = enumerator2.Current;
						yield return f3;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator3 = CaravanArrivalAction_OfferGifts.GetFloatMenuOptions(caravan, this).GetEnumerator())
			{
				if (enumerator3.MoveNext())
				{
					FloatMenuOption f2 = enumerator3.Current;
					yield return f2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator4 = CaravanArrivalAction_AttackSettlement.GetFloatMenuOptions(caravan, this).GetEnumerator())
			{
				if (enumerator4.MoveNext())
				{
					FloatMenuOption f = enumerator4.Current;
					yield return f;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_029a:
			/*Error near IL_029b: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			using (IEnumerator<FloatMenuOption> enumerator = base.GetTransportPodsFloatMenuOptions(pods, representative).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption o = enumerator.Current;
					yield return o;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator2 = TransportPodsArrivalAction_VisitSettlement.GetFloatMenuOptions(representative, pods, this).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					FloatMenuOption f3 = enumerator2.Current;
					yield return f3;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator3 = TransportPodsArrivalAction_GiveGift.GetFloatMenuOptions(representative, pods, this).GetEnumerator())
			{
				if (enumerator3.MoveNext())
				{
					FloatMenuOption f2 = enumerator3.Current;
					yield return f2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator4 = TransportPodsArrivalAction_AttackSettlement.GetFloatMenuOptions(representative, pods, this).GetEnumerator())
			{
				if (enumerator4.MoveNext())
				{
					FloatMenuOption f = enumerator4.Current;
					yield return f;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_029c:
			/*Error near IL_029d: Unexpected return in MoveNext()*/;
		}

		public override void GetChildHolders(List<IThingHolder> outChildren)
		{
			base.GetChildHolders(outChildren);
			if (trader != null)
			{
				outChildren.Add(trader);
			}
		}
	}
}
