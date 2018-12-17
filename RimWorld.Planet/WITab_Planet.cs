using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WITab_Planet : WITab
	{
		private static readonly Vector2 WinSize = new Vector2(400f, 150f);

		public override bool IsVisible => base.SelTileID >= 0;

		private string Desc
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("PlanetSeed".Translate());
				stringBuilder.Append(": ");
				stringBuilder.AppendLine(Find.World.info.seedString);
				stringBuilder.Append("PlanetCoverageShort".Translate());
				stringBuilder.Append(": ");
				stringBuilder.AppendLine(Find.World.info.planetCoverage.ToStringPercent());
				return stringBuilder.ToString();
			}
		}

		public WITab_Planet()
		{
			size = WinSize;
			labelKey = "TabPlanet";
		}

		protected override void FillTab()
		{
			Vector2 winSize = WinSize;
			float x = winSize.x;
			Vector2 winSize2 = WinSize;
			Rect rect = new Rect(0f, 0f, x, winSize2.y).ContractedBy(10f);
			Rect rect2 = rect;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect2, Find.World.info.name);
			Rect rect3 = rect;
			rect3.yMin += 35f;
			Text.Font = GameFont.Small;
			Widgets.Label(rect3, Desc);
		}
	}
}
