using System;

namespace RimWorld
{
	public class SiteCoreDef : SiteCoreOrPartDefBase
	{
		public bool transportPodsCanLandAndGenerateMap = true;

		public float forceExitAndRemoveMapCountdownDurationDays = 3f;

		public new SiteCoreWorker Worker => (SiteCoreWorker)base.Worker;

		public SiteCoreDef()
		{
			workerClass = typeof(SiteCoreWorker);
		}

		public override bool FactionCanOwn(Faction faction)
		{
			return base.FactionCanOwn(faction) && Worker.FactionCanOwn(faction);
		}

		protected override SiteCoreOrPartWorkerBase CreateWorker()
		{
			return (SiteCoreWorker)Activator.CreateInstance(workerClass);
		}
	}
}
