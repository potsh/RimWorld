using Verse;

namespace RimWorld
{
	public class CompDeepScanner : ThingComp
	{
		private CompPowerTrader powerComp;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			powerComp = parent.GetComp<CompPowerTrader>();
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			if (powerComp.PowerOn)
			{
				parent.Map.deepResourceGrid.MarkForDraw();
			}
		}
	}
}
