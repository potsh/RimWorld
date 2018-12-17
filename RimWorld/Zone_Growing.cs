using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Zone_Growing : Zone, IPlantToGrowSettable
	{
		private ThingDef plantDefToGrow = ThingDefOf.Plant_Potato;

		public bool allowSow = true;

		IEnumerable<IntVec3> IPlantToGrowSettable.Cells
		{
			get
			{
				return base.Cells;
			}
		}

		public override bool IsMultiselectable => true;

		protected override Color NextZoneColor => ZoneColorUtility.NextGrowingZoneColor();

		public Zone_Growing()
		{
		}

		public Zone_Growing(ZoneManager zoneManager)
			: base("GrowingZone".Translate(), zoneManager)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref plantDefToGrow, "plantDefToGrow");
			Scribe_Values.Look(ref allowSow, "allowSow", defaultValue: true);
		}

		public override string GetInspectString()
		{
			string text = string.Empty;
			if (!base.Cells.NullOrEmpty())
			{
				IntVec3 c = base.Cells.First();
				if (c.UsesOutdoorTemperature(base.Map))
				{
					string text2 = text;
					text = text2 + "OutdoorGrowingPeriod".Translate() + ": " + GrowingQuadrumsDescription(base.Map.Tile) + "\n";
				}
				text = ((!PlantUtility.GrowthSeasonNow(c, base.Map, forSowing: true)) ? (text + "CannotGrowBadSeasonTemperature".Translate()) : (text + "GrowSeasonHereNow".Translate()));
			}
			return text;
		}

		public static string GrowingQuadrumsDescription(int tile)
		{
			List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(tile, 10f, 42f);
			if (list.NullOrEmpty())
			{
				return "NoGrowingPeriod".Translate();
			}
			if (list.Count == 12)
			{
				return "GrowYearRound".Translate();
			}
			return "PeriodDays".Translate(list.Count * 5 + "/" + 60) + " (" + QuadrumUtility.QuadrumsRangeLabel(list) + ")";
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g = enumerator.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield return (Gizmo)PlantToGrowSettableUtility.SetPlantToGrowCommand(this);
			/*Error: Unable to find new state assignment for yield return*/;
			IL_0189:
			/*Error near IL_018a: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<Gizmo> GetZoneAddGizmos()
		{
			yield return (Gizmo)DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Growing_Expand>();
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public ThingDef GetPlantDefToGrow()
		{
			return plantDefToGrow;
		}

		public void SetPlantDefToGrow(ThingDef plantDef)
		{
			plantDefToGrow = plantDef;
		}

		public bool CanAcceptSowNow()
		{
			return true;
		}
	}
}
