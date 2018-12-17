using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitSite : CaravanArrivalAction
	{
		private Site site;

		public override string Label => site.ApproachOrderString;

		public override string ReportString => site.ApproachingReportString;

		public CaravanArrivalAction_VisitSite()
		{
		}

		public CaravanArrivalAction_VisitSite(Site site)
		{
			this.site = site;
		}

		public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
			if (!(bool)floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (site != null && site.Tile != destinationTile)
			{
				return false;
			}
			return CanVisit(caravan, site);
		}

		public override void Arrived(Caravan caravan)
		{
			site.core.def.Worker.VisitAction(caravan, site);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref site, "site");
		}

		public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, Site site)
		{
			if (site == null || !site.Spawned)
			{
				return false;
			}
			if (site.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(site.EnterCooldownDaysLeft().ToString("0.#")));
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Site site)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, site), () => new CaravanArrivalAction_VisitSite(site), site.ApproachOrderString, caravan, site.Tile, site);
		}
	}
}
