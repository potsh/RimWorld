using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WITab_Terrain : WITab
	{
		private Vector2 scrollPosition;

		private float lastDrawnHeight;

		private static readonly Vector2 WinSize = new Vector2(440f, 540f);

		public override bool IsVisible => base.SelTileID >= 0;

		public WITab_Terrain()
		{
			size = WinSize;
			labelKey = "TabTerrain";
			tutorTag = "Terrain";
		}

		protected override void FillTab()
		{
			Vector2 winSize = WinSize;
			float x = winSize.x;
			Vector2 winSize2 = WinSize;
			Rect outRect = new Rect(0f, 0f, x, winSize2.y).ContractedBy(10f);
			Rect rect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(lastDrawnHeight, outRect.height));
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
			Rect rect2 = rect;
			Rect rect3 = rect2;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect3, base.SelTile.biome.LabelCap);
			Rect rect4 = rect2;
			rect4.yMin += 35f;
			rect4.height = 99999f;
			Text.Font = GameFont.Small;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.verticalSpacing = 0f;
			listing_Standard.Begin(rect4);
			Tile selTile = base.SelTile;
			int selTileID = base.SelTileID;
			listing_Standard.Label(selTile.biome.description);
			listing_Standard.Gap(8f);
			listing_Standard.GapLine();
			if (!selTile.biome.implemented)
			{
				listing_Standard.Label(selTile.biome.LabelCap + " " + "BiomeNotImplemented".Translate());
			}
			listing_Standard.LabelDouble("Terrain".Translate(), selTile.hilliness.GetLabelCap());
			if (selTile.Roads != null)
			{
				listing_Standard.LabelDouble("Road".Translate(), (from roadlink in selTile.Roads
				select roadlink.road.label).Distinct().ToCommaList(useAnd: true).CapitalizeFirst());
			}
			if (selTile.Rivers != null)
			{
				Listing_Standard listing_Standard2 = listing_Standard;
				string leftLabel = "River".Translate();
				Tile.RiverLink riverLink = selTile.Rivers.MaxBy((Tile.RiverLink riverlink) => riverlink.river.degradeThreshold);
				listing_Standard2.LabelDouble(leftLabel, riverLink.river.LabelCap);
			}
			if (!Find.World.Impassable(selTileID))
			{
				StringBuilder stringBuilder = new StringBuilder();
				int tile = selTileID;
				bool perceivedStatic = false;
				StringBuilder explanation = stringBuilder;
				string rightLabel = (WorldPathGrid.CalculatedMovementDifficultyAt(tile, perceivedStatic, null, explanation) * Find.WorldGrid.GetRoadMovementDifficultyMultiplier(selTileID, -1, stringBuilder)).ToString("0.#");
				if (WorldPathGrid.WillWinterEverAffectMovementDifficulty(selTileID) && WorldPathGrid.GetCurrentWinterMovementDifficultyOffset(selTileID) < 2f)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.Append(" (");
					stringBuilder.Append("MovementDifficultyOffsetInWinter".Translate("+" + 2f.ToString("0.#")));
					stringBuilder.Append(")");
				}
				listing_Standard.LabelDouble("MovementDifficulty".Translate(), rightLabel, stringBuilder.ToString());
			}
			if (selTile.biome.canBuildBase)
			{
				listing_Standard.LabelDouble("StoneTypesHere".Translate(), (from rt in Find.World.NaturalRockTypesIn(selTileID)
				select rt.label).ToCommaList(useAnd: true).CapitalizeFirst());
			}
			listing_Standard.LabelDouble("Elevation".Translate(), selTile.elevation.ToString("F0") + "m");
			listing_Standard.GapLine();
			listing_Standard.LabelDouble("AvgTemp".Translate(), GenTemperature.GetAverageTemperatureLabel(selTileID));
			listing_Standard.LabelDouble("OutdoorGrowingPeriod".Translate(), Zone_Growing.GrowingQuadrumsDescription(selTileID));
			listing_Standard.LabelDouble("Rainfall".Translate(), selTile.rainfall.ToString("F0") + "mm");
			if (selTile.biome.foragedFood != null && selTile.biome.forageability > 0f)
			{
				listing_Standard.LabelDouble("Forageability".Translate(), selTile.biome.forageability.ToStringPercent() + " (" + selTile.biome.foragedFood.label + ")");
			}
			else
			{
				listing_Standard.LabelDouble("Forageability".Translate(), "0%");
			}
			listing_Standard.LabelDouble("AnimalsCanGrazeNow".Translate(), (!VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsNowAt(selTileID)) ? "No".Translate() : "Yes".Translate());
			listing_Standard.GapLine();
			listing_Standard.LabelDouble("AverageDiseaseFrequency".Translate(), string.Format("{0} {1}", (60f / selTile.biome.diseaseMtbDays).ToString("F1"), "PerYear".Translate()));
			Listing_Standard listing_Standard3 = listing_Standard;
			string leftLabel2 = "TimeZone".Translate();
			Vector2 vector = Find.WorldGrid.LongLatOf(selTileID);
			listing_Standard3.LabelDouble(leftLabel2, GenDate.TimeZoneAt(vector.x).ToStringWithSign());
			StringBuilder stringBuilder2 = new StringBuilder();
			Rot4 rot = Find.World.CoastDirectionAt(selTileID);
			if (rot.IsValid)
			{
				stringBuilder2.AppendWithComma(("HasCoast" + rot.ToString()).Translate());
			}
			if (Find.World.HasCaves(selTileID))
			{
				stringBuilder2.AppendWithComma("HasCaves".Translate());
			}
			if (stringBuilder2.Length > 0)
			{
				listing_Standard.LabelDouble("SpecialFeatures".Translate(), stringBuilder2.ToString().CapitalizeFirst());
			}
			if (Prefs.DevMode)
			{
				listing_Standard.LabelDouble("Debug world tile ID", selTileID.ToString());
			}
			lastDrawnHeight = rect4.y + listing_Standard.CurHeight;
			listing_Standard.End();
			Widgets.EndScrollView();
		}
	}
}
