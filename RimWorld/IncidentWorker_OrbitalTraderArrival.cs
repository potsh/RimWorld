using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_OrbitalTraderArrival : IncidentWorker
	{
		private const int MaxShips = 5;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			if (map.passingShipManager.passingShips.Count >= 5)
			{
				return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (map.passingShipManager.passingShips.Count >= 5)
			{
				return false;
			}
			if (!(from x in DefDatabase<TraderKindDef>.AllDefs
			where x.orbital
			select x).TryRandomElementByWeight((TraderKindDef traderDef) => traderDef.CalculatedCommonality, out TraderKindDef result))
			{
				throw new InvalidOperationException();
			}
			TradeShip tradeShip = new TradeShip(result);
			if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
			{
				Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(tradeShip.name, tradeShip.def.label), LetterDefOf.PositiveEvent);
			}
			map.passingShipManager.AddShip(tradeShip);
			tradeShip.GenerateThings();
			return true;
		}
	}
}
