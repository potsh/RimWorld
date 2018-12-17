using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Tradeable_Pawn : Tradeable
	{
		public override Window NewInfoDialog => new Dialog_InfoCard(AnyPawn);

		public override string Label
		{
			get
			{
				string text = base.Label;
				if (AnyPawn.Name != null && !AnyPawn.Name.Numerical)
				{
					text = text + ", " + AnyPawn.def.label;
				}
				string text2 = text;
				return text2 + " (" + AnyPawn.GetGenderLabel() + ", " + AnyPawn.ageTracker.AgeBiologicalYearsFloat.ToString("F0") + ")";
			}
		}

		public override string TipDescription
		{
			get
			{
				if (!HasAnyThing)
				{
					return string.Empty;
				}
				string str = AnyPawn.MainDesc(writeAge: true);
				return str + "\n\n" + AnyPawn.def.description;
			}
		}

		private Pawn AnyPawn => (Pawn)AnyThing;

		public override void ResolveTrade()
		{
			if (base.ActionToDo == TradeAction.PlayerSells)
			{
				List<Pawn> list = thingsColony.Take(base.CountToTransferToDestination).Cast<Pawn>().ToList();
				for (int i = 0; i < list.Count; i++)
				{
					TradeSession.trader.GiveSoldThingToTrader(list[i], 1, TradeSession.playerNegotiator);
				}
			}
			else if (base.ActionToDo == TradeAction.PlayerBuys)
			{
				List<Pawn> list2 = thingsTrader.Take(base.CountToTransferToSource).Cast<Pawn>().ToList();
				for (int j = 0; j < list2.Count; j++)
				{
					TradeSession.trader.GiveSoldThingToPlayer(list2[j], 1, TradeSession.playerNegotiator);
				}
			}
		}
	}
}
