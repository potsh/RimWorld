using System;
using Verse;

namespace RimWorld
{
	public struct ThingStuffPairWithQuality : IEquatable<ThingStuffPairWithQuality>
	{
		public ThingDef thing;

		public ThingDef stuff;

		public QualityCategory? quality;

		public QualityCategory Quality
		{
			get
			{
				QualityCategory? qualityCategory = quality;
				return (!qualityCategory.HasValue) ? QualityCategory.Normal : qualityCategory.Value;
			}
		}

		public ThingStuffPairWithQuality(ThingDef thing, ThingDef stuff, QualityCategory quality)
		{
			this.thing = thing;
			this.stuff = stuff;
			this.quality = quality;
			if (quality != QualityCategory.Normal && !thing.HasComp(typeof(CompQuality)))
			{
				Log.Warning("Created ThingStuffPairWithQuality with quality" + quality + " but " + thing + " doesn't have CompQuality.");
				quality = QualityCategory.Normal;
			}
			if (stuff != null && !thing.MadeFromStuff)
			{
				Log.Warning("Created ThingStuffPairWithQuality with stuff " + stuff + " but " + thing + " is not made from stuff.");
				stuff = null;
			}
		}

		public float GetStatValue(StatDef stat)
		{
			return stat.Worker.GetValue(StatRequest.For(thing, stuff, Quality));
		}

		public static bool operator ==(ThingStuffPairWithQuality a, ThingStuffPairWithQuality b)
		{
			int result;
			if (a.thing == b.thing && a.stuff == b.stuff)
			{
				QualityCategory? qualityCategory = a.quality;
				QualityCategory valueOrDefault = qualityCategory.GetValueOrDefault();
				QualityCategory? qualityCategory2 = b.quality;
				result = ((valueOrDefault == qualityCategory2.GetValueOrDefault() && qualityCategory.HasValue == qualityCategory2.HasValue) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		public static bool operator !=(ThingStuffPairWithQuality a, ThingStuffPairWithQuality b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ThingStuffPairWithQuality))
			{
				return false;
			}
			return Equals((ThingStuffPairWithQuality)obj);
		}

		public bool Equals(ThingStuffPairWithQuality other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine(seed, thing);
			seed = Gen.HashCombine(seed, stuff);
			return Gen.HashCombine(seed, quality);
		}

		public static explicit operator ThingStuffPairWithQuality(ThingStuffPair p)
		{
			return new ThingStuffPairWithQuality(p.thing, p.stuff, QualityCategory.Normal);
		}

		public Thing MakeThing()
		{
			Thing result = ThingMaker.MakeThing(thing, stuff);
			result.TryGetComp<CompQuality>()?.SetQuality(Quality, ArtGenerationContext.Outsider);
			return result;
		}
	}
}
