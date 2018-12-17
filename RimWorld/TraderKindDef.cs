using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TraderKindDef : Def
	{
		public List<StockGenerator> stockGenerators = new List<StockGenerator>();

		public float commonality = 1f;

		public bool orbital;

		public bool requestable = true;

		public SimpleCurve commonalityMultFromPopulationIntent;

		public float CalculatedCommonality
		{
			get
			{
				float num = commonality;
				if (commonalityMultFromPopulationIntent != null)
				{
					num *= commonalityMultFromPopulationIntent.Evaluate(StorytellerUtilityPopulation.PopulationIntent);
				}
				return num;
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			foreach (StockGenerator stockGenerator in stockGenerators)
			{
				stockGenerator.ResolveReferences(this);
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err2 = enumerator.Current;
					yield return err2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			foreach (StockGenerator stockGenerator in stockGenerators)
			{
				using (IEnumerator<string> enumerator3 = stockGenerator.ConfigErrors(this).GetEnumerator())
				{
					if (enumerator3.MoveNext())
					{
						string err = enumerator3.Current;
						yield return err;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01b6:
			/*Error near IL_01b7: Unexpected return in MoveNext()*/;
		}

		public bool WillTrade(ThingDef td)
		{
			for (int i = 0; i < stockGenerators.Count; i++)
			{
				if (stockGenerators[i].HandlesThingDef(td))
				{
					return true;
				}
			}
			return false;
		}

		public PriceType PriceTypeFor(ThingDef thingDef, TradeAction action)
		{
			if (thingDef == ThingDefOf.Silver)
			{
				return PriceType.Undefined;
			}
			if (action == TradeAction.PlayerBuys)
			{
				for (int i = 0; i < stockGenerators.Count; i++)
				{
					if (stockGenerators[i].TryGetPriceType(thingDef, action, out PriceType priceType))
					{
						return priceType;
					}
				}
			}
			return PriceType.Normal;
		}
	}
}
