using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_TraderStock : ThingSetMaker
	{
		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			TraderKindDef traderKindDef = parms.traderDef ?? DefDatabase<TraderKindDef>.AllDefsListForReading.RandomElement();
			Faction traderFaction = parms.traderFaction;
			int? tile = parms.tile;
			int forTile = tile.HasValue ? parms.tile.Value : ((Find.AnyPlayerHomeMap != null) ? Find.AnyPlayerHomeMap.Tile : ((Find.CurrentMap == null) ? (-1) : Find.CurrentMap.Tile));
			for (int i = 0; i < traderKindDef.stockGenerators.Count; i++)
			{
				StockGenerator stockGenerator = traderKindDef.stockGenerators[i];
				foreach (Thing item in stockGenerator.GenerateThings(forTile))
				{
					if (!item.def.tradeability.TraderCanSell())
					{
						Log.Error(traderKindDef + " generated carrying " + item + " which can't be sold by traders. Ignoring...");
					}
					else
					{
						item.PostGeneratedForTrader(traderKindDef, forTile, traderFaction);
						outThings.Add(item);
					}
				}
			}
		}

		public float AverageTotalStockValue(TraderKindDef td)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = td;
			parms.tile = -1;
			float num = 0f;
			for (int i = 0; i < 50; i++)
			{
				foreach (Thing item in Generate(parms))
				{
					num += item.MarketValue * (float)item.stackCount;
				}
			}
			return num / 50f;
		}

		public string GenerationDataFor(TraderKindDef td)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(td.defName);
			stringBuilder.AppendLine("Average total market value:" + AverageTotalStockValue(td).ToString("F0"));
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = td;
			parms.tile = -1;
			stringBuilder.AppendLine("Example generated stock:\n\n");
			foreach (Thing item in Generate(parms))
			{
				MinifiedThing minifiedThing = item as MinifiedThing;
				Thing thing = (minifiedThing == null) ? item : minifiedThing.InnerThing;
				string labelCap = thing.LabelCap;
				labelCap = labelCap + " [" + (thing.MarketValue * (float)thing.stackCount).ToString("F0") + "]";
				stringBuilder.AppendLine(labelCap);
			}
			return stringBuilder.ToString();
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			if (parms.traderDef != null)
			{
				for (int i = 0; i < parms.traderDef.stockGenerators.Count; i++)
				{
					_003CAllGeneratableThingsDebugSub_003Ec__Iterator0 _003CAllGeneratableThingsDebugSub_003Ec__Iterator = (_003CAllGeneratableThingsDebugSub_003Ec__Iterator0)/*Error near IL_0055: stateMachine*/;
					StockGenerator stock = parms.traderDef.stockGenerators[i];
					using (IEnumerator<ThingDef> enumerator = (from x in DefDatabase<ThingDef>.AllDefs
					where x.tradeability.TraderCanSell() && stock.HandlesThingDef(x)
					select x).GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							ThingDef t = enumerator.Current;
							yield return t;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_0155:
			/*Error near IL_0156: Unexpected return in MoveNext()*/;
		}
	}
}
