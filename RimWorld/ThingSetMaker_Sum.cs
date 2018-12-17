using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Sum : ThingSetMaker
	{
		public class Option
		{
			public ThingSetMaker thingSetMaker;

			public float chance = 1f;
		}

		public List<Option> options;

		private List<Option> optionsInRandomOrder = new List<Option>();

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i].chance > 0f && options[i].thingSetMaker.CanGenerate(parms))
				{
					return true;
				}
			}
			return false;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			int num = 0;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			optionsInRandomOrder.Clear();
			optionsInRandomOrder.AddRange(options);
			optionsInRandomOrder.Shuffle();
			for (int i = 0; i < optionsInRandomOrder.Count; i++)
			{
				ThingSetMakerParams parms2 = parms;
				IntRange? countRange = parms2.countRange;
				if (countRange.HasValue)
				{
					IntRange value = parms2.countRange.Value;
					int min = value.min;
					IntRange value2 = parms2.countRange.Value;
					parms2.countRange = new IntRange(min, value2.max - num);
				}
				FloatRange? totalMarketValueRange = parms2.totalMarketValueRange;
				if (totalMarketValueRange.HasValue)
				{
					FloatRange value3 = parms2.totalMarketValueRange.Value;
					float min2 = value3.min;
					FloatRange value4 = parms2.totalMarketValueRange.Value;
					parms2.totalMarketValueRange = new FloatRange(min2, value4.max - num2);
				}
				FloatRange? totalNutritionRange = parms2.totalNutritionRange;
				if (totalNutritionRange.HasValue)
				{
					FloatRange value5 = parms2.totalNutritionRange.Value;
					float min3 = value5.min;
					FloatRange value6 = parms2.totalNutritionRange.Value;
					parms2.totalNutritionRange = new FloatRange(min3, value6.max - num3);
				}
				float? maxTotalMass = parms2.maxTotalMass;
				if (maxTotalMass.HasValue)
				{
					parms2.maxTotalMass -= num4;
				}
				if (Rand.Chance(optionsInRandomOrder[i].chance) && optionsInRandomOrder[i].thingSetMaker.CanGenerate(parms2))
				{
					List<Thing> list = optionsInRandomOrder[i].thingSetMaker.Generate(parms2);
					num += list.Count;
					for (int j = 0; j < list.Count; j++)
					{
						num2 += list[j].MarketValue * (float)list[j].stackCount;
						if (list[j].def.IsIngestible)
						{
							num3 += list[j].GetStatValue(StatDefOf.Nutrition) * (float)list[j].stackCount;
						}
						if (!(list[j] is Pawn))
						{
							num4 += list[j].GetStatValue(StatDefOf.Mass) * (float)list[j].stackCount;
						}
					}
					outThings.AddRange(list);
				}
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			for (int i = 0; i < options.Count; i++)
			{
				options[i].thingSetMaker.ResolveReferences();
			}
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (!(options[i].chance <= 0f))
				{
					using (IEnumerator<ThingDef> enumerator = options[i].thingSetMaker.AllGeneratableThingsDebug(parms).GetEnumerator())
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
			IL_0133:
			/*Error near IL_0134: Unexpected return in MoveNext()*/;
		}
	}
}
