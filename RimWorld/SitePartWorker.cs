using RimWorld.Planet;

namespace RimWorld
{
	public class SitePartWorker : SiteCoreOrPartWorkerBase
	{
		public SitePartDef Def => (SitePartDef)def;

		public virtual void SitePartWorkerTick(Site site)
		{
		}
	}
}
