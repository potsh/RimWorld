using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	[HasDebugOutput]
	public class StockGenerator_Animals : StockGenerator
	{
		[NoTranslate]
		private List<string> tradeTagsSell = new List<string>();

		[NoTranslate]
		private List<string> tradeTagsBuy = new List<string>();

		private IntRange kindCountRange = new IntRange(1, 1);

		private float minWildness;

		private float maxWildness = 1f;

		private bool checkTemperature = true;

		private static readonly SimpleCurve SelectionChanceFromWildnessCurve = new SimpleCurve
		{
			new CurvePoint(0f, 100f),
			new CurvePoint(0.25f, 60f),
			new CurvePoint(0.5f, 30f),
			new CurvePoint(0.75f, 12f),
			new CurvePoint(1f, 2f)
		};

		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			_003CGenerateThings_003Ec__Iterator0 _003CGenerateThings_003Ec__Iterator = (_003CGenerateThings_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/;
			int numKinds = kindCountRange.RandomInRange;
			int count = countRange.RandomInRange;
			List<PawnKindDef> kinds = new List<PawnKindDef>();
			for (int j = 0; j < numKinds; j++)
			{
				if (!(from k in DefDatabase<PawnKindDef>.AllDefs
				where !kinds.Contains(k) && _003CGenerateThings_003Ec__Iterator._0024this.PawnKindAllowed(k, forTile)
				select k).TryRandomElementByWeight((PawnKindDef k) => _003CGenerateThings_003Ec__Iterator._0024this.SelectionChance(k), out PawnKindDef result))
				{
					break;
				}
				kinds.Add(result);
			}
			int i = 0;
			if (i < count && kinds.TryRandomElement(out PawnKindDef kind))
			{
				PawnKindDef kind2 = kind;
				int tile = forTile;
				PawnGenerationRequest request = new PawnGenerationRequest(kind2, null, PawnGenerationContext.NonPlayer, tile);
				yield return (Thing)PawnGenerator.GeneratePawn(request);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		private float SelectionChance(PawnKindDef k)
		{
			return SelectionChanceFromWildnessCurve.Evaluate(k.RaceProps.wildness);
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Animal && thingDef.tradeability != 0 && (tradeTagsSell.Any((string tag) => thingDef.tradeTags != null && thingDef.tradeTags.Contains(tag)) || tradeTagsBuy.Any((string tag) => thingDef.tradeTags != null && thingDef.tradeTags.Contains(tag)));
		}

		private bool PawnKindAllowed(PawnKindDef kind, int forTile)
		{
			if (!kind.RaceProps.Animal || kind.RaceProps.wildness < minWildness || kind.RaceProps.wildness > maxWildness || kind.RaceProps.wildness > 1f)
			{
				return false;
			}
			if (checkTemperature)
			{
				int num = forTile;
				if (num == -1 && Find.AnyPlayerHomeMap != null)
				{
					num = Find.AnyPlayerHomeMap.Tile;
				}
				if (num != -1 && !Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(num, kind.race))
				{
					return false;
				}
			}
			if (kind.race.tradeTags == null)
			{
				return false;
			}
			if (!tradeTagsSell.Any((string x) => kind.race.tradeTags.Contains(x)))
			{
				return false;
			}
			if (!kind.race.tradeability.TraderCanSell())
			{
				return false;
			}
			return true;
		}

		public void LogAnimalChances()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PawnKindDef allDef in DefDatabase<PawnKindDef>.AllDefs)
			{
				stringBuilder.AppendLine(allDef.defName + ": " + SelectionChance(allDef).ToString("F2"));
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		private static void StockGenerationAnimals()
		{
			StockGenerator_Animals stockGenerator_Animals = new StockGenerator_Animals();
			stockGenerator_Animals.tradeTagsSell = new List<string>();
			stockGenerator_Animals.tradeTagsSell.Add("AnimalCommon");
			stockGenerator_Animals.tradeTagsSell.Add("AnimalUncommon");
			stockGenerator_Animals.LogAnimalChances();
		}
	}
}
