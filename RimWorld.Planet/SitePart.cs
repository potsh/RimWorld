using Verse;

namespace RimWorld.Planet
{
	public class SitePart : SiteCoreOrPartBase
	{
		public SitePartDef def;

		public override SiteCoreOrPartDefBase Def => def;

		public SitePart()
		{
		}

		public SitePart(SitePartDef def, SiteCoreOrPartParams parms)
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
