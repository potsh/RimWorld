using Verse;

namespace RimWorld
{
	public class StockGenerator_Armor : StockGenerator_MiscItems
	{
		public const float MinArmor = 0.15f;

		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(500f, 1f),
			new CurvePoint(1500f, 0.2f),
			new CurvePoint(5000f, 0.1f)
		};

		public override bool HandlesThingDef(ThingDef td)
		{
			if (td == ThingDefOf.Apparel_ShieldBelt)
			{
				return true;
			}
			if (td == ThingDefOf.Apparel_SmokepopBelt)
			{
				return true;
			}
			return base.HandlesThingDef(td) && td.IsApparel && (td.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt) > 0.15f || td.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp) > 0.15f);
		}

		protected override float SelectionWeight(ThingDef thingDef)
		{
			return SelectionWeightMarketValueCurve.Evaluate(thingDef.BaseMarketValue);
		}
	}
}
