using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_RandomOption : ThingSetMaker
	{
		public class Option
		{
			public ThingSetMaker thingSetMaker;

			public float weight;

			public float? weightIfPlayerHasNoItem;

			public ThingDef weightIfPlayerHasNoItemItem;
		}

		public List<Option> options;

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i].thingSetMaker.CanGenerate(parms) && GetSelectionWeight(options[i], parms) > 0f)
				{
					return true;
				}
			}
			return false;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			if ((from x in options
			where x.thingSetMaker.CanGenerate(parms)
			select x).TryRandomElementByWeight((Option x) => GetSelectionWeight(x, parms), out Option result))
			{
				outThings.AddRange(result.thingSetMaker.Generate(parms));
			}
		}

		private float GetSelectionWeight(Option option, ThingSetMakerParams parms)
		{
			float? weightIfPlayerHasNoItem = option.weightIfPlayerHasNoItem;
			if (weightIfPlayerHasNoItem.HasValue && !PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas(option.weightIfPlayerHasNoItemItem))
			{
				return option.weightIfPlayerHasNoItem.Value;
			}
			return option.weight;
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
				float weight = options[i].weight;
				float? weightIfPlayerHasNoItem = options[i].weightIfPlayerHasNoItem;
				if (weightIfPlayerHasNoItem.HasValue)
				{
					weight = Mathf.Max(weight, options[i].weightIfPlayerHasNoItem.Value);
				}
				if (!(weight <= 0f))
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
			IL_0198:
			/*Error near IL_0199: Unexpected return in MoveNext()*/;
		}
	}
}
