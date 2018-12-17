using RimWorld;

namespace Verse
{
	public class GenStepDef : Def
	{
		public SiteCoreOrPartDefBase linkWithSite;

		public float order;

		public GenStep genStep;

		public override void PostLoad()
		{
			base.PostLoad();
			genStep.def = this;
		}
	}
}
