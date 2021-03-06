using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_DeadmansApparel : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			return (t as Apparel)?.WornByCorpse ?? false;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return def.IsApparel && def.apparel.careIfWornByCorpse;
		}
	}
}
