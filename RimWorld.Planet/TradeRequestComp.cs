using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class TradeRequestComp : WorldObjectComp, IThingHolder
	{
		public ThingDef requestThingDef;

		public int requestCount;

		public ThingOwner rewards;

		public int expiration = -1;

		private static readonly Texture2D TradeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/FulfillTradeRequest");

		public bool ActiveRequest => expiration > Find.TickManager.TicksGame;

		public TradeRequestComp()
		{
			rewards = new ThingOwner<Thing>(this);
		}

		public override string CompInspectStringExtra()
		{
			if (ActiveRequest)
			{
				return "CaravanRequestInfo".Translate(TradeRequestUtility.RequestedThingLabel(requestThingDef, requestCount).CapitalizeFirst(), GenThing.ThingsToCommaList(rewards, useAnd: true).CapitalizeFirst(), (expiration - Find.TickManager.TicksGame).ToStringTicksToDays(), (requestThingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)requestCount).ToStringMoney(), GenThing.GetMarketValue(rewards).ToStringMoney());
			}
			return null;
		}

		public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
		{
			if (ActiveRequest && CaravanVisitUtility.SettlementVisitedNow(caravan) == parent)
			{
				yield return (Gizmo)FulfillRequestCommand(caravan);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return rewards;
		}

		public void Disable()
		{
			expiration = -1;
			rewards.ClearAndDestroyContents();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look(ref requestThingDef, "requestThingDef");
			Scribe_Values.Look(ref requestCount, "requestCount", 0);
			Scribe_Deep.Look(ref rewards, "rewards", this);
			Scribe_Values.Look(ref expiration, "expiration", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.TradeRequestCompPostLoadInit(this);
			}
		}

		public override void PostPostRemove()
		{
			base.PostPostRemove();
			rewards.ClearAndDestroyContents();
		}

		private Command FulfillRequestCommand(Caravan caravan)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandFulfillTradeOffer".Translate();
			command_Action.defaultDesc = "CommandFulfillTradeOfferDesc".Translate();
			command_Action.icon = TradeCommandTex;
			command_Action.action = delegate
			{
				if (!ActiveRequest)
				{
					Log.Error("Attempted to fulfill an unavailable request");
				}
				else if (!CaravanInventoryUtility.HasThings(caravan, requestThingDef, requestCount, PlayerCanGive))
				{
					Messages.Message("CommandFulfillTradeOfferFailInsufficient".Translate(TradeRequestUtility.RequestedThingLabel(requestThingDef, requestCount)), MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CommandFulfillTradeOfferConfirm".Translate(GenLabel.ThingLabel(requestThingDef, null, requestCount), GenThing.ThingsToCommaList(rewards, useAnd: true)), delegate
					{
						Fulfill(caravan);
					}));
				}
			};
			if (!CaravanInventoryUtility.HasThings(caravan, requestThingDef, requestCount, PlayerCanGive))
			{
				command_Action.Disable("CommandFulfillTradeOfferFailInsufficient".Translate(TradeRequestUtility.RequestedThingLabel(requestThingDef, requestCount)));
			}
			return command_Action;
		}

		private void Fulfill(Caravan caravan)
		{
			int remaining = requestCount;
			List<Thing> list = CaravanInventoryUtility.TakeThings(caravan, delegate(Thing thing)
			{
				if (requestThingDef != thing.def)
				{
					return 0;
				}
				if (!PlayerCanGive(thing))
				{
					return 0;
				}
				int num = Mathf.Min(remaining, thing.stackCount);
				remaining -= num;
				return num;
			});
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Destroy();
			}
			while (rewards.Count > 0)
			{
				Thing thing2 = rewards.Last();
				rewards.Remove(thing2);
				CaravanInventoryUtility.GiveThing(caravan, thing2);
			}
			if (parent.Faction != null)
			{
				Faction faction = parent.Faction;
				Faction ofPlayer = Faction.OfPlayer;
				int goodwillChange = 12;
				string reason = "GoodwillChangedReason_FulfilledTradeRequest".Translate();
				GlobalTargetInfo? lookTarget = parent;
				faction.TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, reason, lookTarget);
			}
			Disable();
		}

		private bool PlayerCanGive(Thing thing)
		{
			if (thing.GetRotStage() != 0)
			{
				return false;
			}
			Apparel apparel = thing as Apparel;
			if (apparel != null && apparel.WornByCorpse)
			{
				return false;
			}
			CompQuality compQuality = thing.TryGetComp<CompQuality>();
			if (compQuality != null && (int)compQuality.Quality < 2)
			{
				return false;
			}
			return true;
		}
	}
}
