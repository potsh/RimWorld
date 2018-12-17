using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGeneratorUtility
	{
		public static IEnumerable<Thing> TryMakeForStock(ThingDef thingDef, int count)
		{
			if (thingDef.MadeFromStuff)
			{
				int i = 0;
				Thing th2;
				while (true)
				{
					if (i >= count)
					{
						yield break;
					}
					th2 = TryMakeForStockSingle(thingDef, 1);
					if (th2 != null)
					{
						break;
					}
					i++;
				}
				yield return th2;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			Thing th = TryMakeForStockSingle(thingDef, count);
			if (th != null)
			{
				yield return th;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public static Thing TryMakeForStockSingle(ThingDef thingDef, int stackCount)
		{
			if (stackCount <= 0)
			{
				return null;
			}
			if (!thingDef.tradeability.TraderCanSell())
			{
				Log.Error("Tried to make non-trader-sellable thing for trader stock: " + thingDef);
				return null;
			}
			ThingDef result = null;
			if (thingDef.MadeFromStuff && !(from x in GenStuff.AllowedStuffsFor(thingDef)
			where !PawnWeaponGenerator.IsDerpWeapon(thingDef, x)
			select x).TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out result))
			{
				result = GenStuff.RandomStuffByCommonalityFor(thingDef);
			}
			Thing thing = ThingMaker.MakeThing(thingDef, result);
			thing.stackCount = stackCount;
			return thing;
		}
	}
}
