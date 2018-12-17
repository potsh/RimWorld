using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class StockGenerator_MiscItems : StockGenerator
	{
		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			int count = countRange.RandomInRange;
			int i = 0;
			if (i < count && (from t in DefDatabase<ThingDef>.AllDefs
			where ((_003CGenerateThings_003Ec__Iterator0)/*Error near IL_0048: stateMachine*/)._0024this.HandlesThingDef(t) && t.tradeability.TraderCanSell() && (int)t.techLevel <= (int)((_003CGenerateThings_003Ec__Iterator0)/*Error near IL_0048: stateMachine*/)._0024this.maxTechLevelGenerate
			select t).TryRandomElementByWeight(SelectionWeight, out ThingDef finalDef))
			{
				yield return MakeThing(finalDef);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		protected virtual Thing MakeThing(ThingDef def)
		{
			return StockGeneratorUtility.TryMakeForStockSingle(def, 1);
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.tradeability != 0 && (int)thingDef.techLevel <= (int)maxTechLevelBuy;
		}

		protected virtual float SelectionWeight(ThingDef thingDef)
		{
			return 1f;
		}
	}
}
