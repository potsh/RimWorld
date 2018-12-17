using System.Collections.Generic;

namespace Verse
{
	public class PawnInventoryOption
	{
		public ThingDef thingDef;

		public IntRange countRange = IntRange.one;

		public float choiceChance = 1f;

		public float skipChance;

		public List<PawnInventoryOption> subOptionsTakeAll;

		public List<PawnInventoryOption> subOptionsChooseOne;

		public IEnumerable<Thing> GenerateThings()
		{
			if (!(Rand.Value < skipChance))
			{
				if (thingDef != null && countRange.max > 0)
				{
					Thing thing = ThingMaker.MakeThing(thingDef);
					thing.stackCount = countRange.RandomInRange;
					yield return thing;
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (subOptionsTakeAll != null)
				{
					foreach (PawnInventoryOption item in subOptionsTakeAll)
					{
						using (IEnumerator<Thing> enumerator2 = item.GenerateThings().GetEnumerator())
						{
							if (enumerator2.MoveNext())
							{
								Thing subThing2 = enumerator2.Current;
								yield return subThing2;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				if (subOptionsChooseOne != null)
				{
					PawnInventoryOption chosen = subOptionsChooseOne.RandomElementByWeight((PawnInventoryOption o) => o.choiceChance);
					using (IEnumerator<Thing> enumerator3 = chosen.GenerateThings().GetEnumerator())
					{
						if (enumerator3.MoveNext())
						{
							Thing subThing = enumerator3.Current;
							yield return subThing;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_0299:
			/*Error near IL_029a: Unexpected return in MoveNext()*/;
		}
	}
}
