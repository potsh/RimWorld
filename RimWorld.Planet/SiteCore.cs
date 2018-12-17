using Verse;

namespace RimWorld.Planet
{
	public class SiteCore : SiteCoreOrPartBase
	{
		public SiteCoreDef def;

		public override SiteCoreOrPartDefBase Def => def;

		public SiteCore()
		{
		}

		public SiteCore(SiteCoreDef def, SiteCoreOrPartParams parms)
		{
			this.def = def;
			base.parms = parms;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
		}
	}
}
