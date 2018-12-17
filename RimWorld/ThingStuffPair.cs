using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public struct ThingStuffPair : IEquatable<ThingStuffPair>
	{
		public ThingDef thing;

		public ThingDef stuff;

		public float commonalityMultiplier;

		private float cachedPrice;

		private float cachedInsulationCold;

		private float cachedInsulationHeat;

		public float Price => cachedPrice;

		public float InsulationCold => cachedInsulationCold;

		public float InsulationHeat => cachedInsulationHeat;

		public float Commonality
		{
			get
			{
				float num = commonalityMultiplier;
				num *= thing.generateCommonality;
				if (stuff != null)
				{
					num *= stuff.stuffProps.commonality;
				}
				if (PawnWeaponGenerator.IsDerpWeapon(thing, stuff))
				{
					num *= 0.01f;
				}
				return num;
			}
		}

		public ThingStuffPair(ThingDef thing, ThingDef stuff, float commonalityMultiplier = 1f)
		{
			this.thing = thing;
			this.stuff = stuff;
			this.commonalityMultiplier = commonalityMultiplier;
			if (stuff != null && !thing.MadeFromStuff)
			{
				Log.Warning("Created ThingStuffPairWithQuality with stuff " + stuff + " but " + thing + " is not made from stuff.");
				stuff = null;
			}
			cachedPrice = thing.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
			cachedInsulationCold = thing.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuff);
			cachedInsulationHeat = thing.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuff);
		}

		public static List<ThingStuffPair> AllWith(Predicate<ThingDef> thingValidator)
		{
			List<ThingStuffPair> list = new List<ThingStuffPair>();
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ThingDef thingDef = allDefsListForReading[i];
				if (thingValidator(thingDef))
				{
					if (!thingDef.MadeFromStuff)
					{
						list.Add(new ThingStuffPair(thingDef, null));
					}
					else
					{
						IEnumerable<ThingDef> enumerable = from st in DefDatabase<ThingDef>.AllDefs
						where st.IsStuff && st.stuffProps.CanMake(thingDef)
						select st;
						int num = enumerable.Count();
						float num2 = enumerable.Average((ThingDef st) => st.stuffProps.commonality);
						float num3 = 1f / (float)num / num2;
						foreach (ThingDef item in enumerable)
						{
							list.Add(new ThingStuffPair(thingDef, item, num3));
						}
					}
				}
			}
			return (from p in list
			orderby p.Price descending
			select p).ToList();
		}

		public override string ToString()
		{
			if (thing == null)
			{
				return "(null)";
			}
			string text = (stuff != null) ? (thing.label + " " + stuff.LabelAsStuff) : thing.label;
			return text + " $" + Price.ToString("F0") + " c=" + Commonality.ToString("F4");
		}

		public static bool operator ==(ThingStuffPair a, ThingStuffPair b)
		{
			return a.thing == b.thing && a.stuff == b.stuff && a.commonalityMultiplier == b.commonalityMultiplier;
		}

		public static bool operator !=(ThingStuffPair a, ThingStuffPair b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ThingStuffPair))
			{
				return false;
			}
			return Equals((ThingStuffPair)obj);
		}

		public bool Equals(ThingStuffPair other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine(seed, thing);
			seed = Gen.HashCombine(seed, stuff);
			return Gen.HashCombineStruct(seed, commonalityMultiplier);
		}

		public static explicit operator ThingStuffPair(ThingStuffPairWithQuality p)
		{
			return new ThingStuffPair(p.thing, p.stuff);
		}
	}
}
