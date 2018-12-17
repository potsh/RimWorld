using System;

namespace RimWorld
{
	public class SitePartDef : SiteCoreOrPartDefBase
	{
		public bool alwaysHidden;

		public new SitePartWorker Worker => (SitePartWorker)base.Worker;

		public SitePartDef()
		{
			workerClass = typeof(SitePartWorker);
		}

		public override bool FactionCanOwn(Faction faction)
		{
			return base.FactionCanOwn(faction) && Worker.FactionCanOwn(faction);
		}

		protected override SiteCoreOrPartWorkerBase CreateWorker()
		{
			return (SitePartWorker)Activator.CreateInstance(workerClass);
		}
	}
}
