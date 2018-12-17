using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Conditional_MinMaxTotalMarketValue : ThingSetMaker_Conditional
	{
		public float minMaxTotalMarketValue;

		protected override bool Condition(ThingSetMakerParams parms)
		{
			FloatRange? totalMarketValueRange = parms.totalMarketValueRange;
			int result;
			if (totalMarketValueRange.HasValue)
			{
				FloatRange value = parms.totalMarketValueRange.Value;
				result = ((value.max >= minMaxTotalMarketValue) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}
	}
}
