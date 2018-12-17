using Verse;

namespace RimWorld
{
	public class CompProperties_UseEffect : CompProperties
	{
		public bool doCameraShake;

		public CompProperties_UseEffect()
		{
			compClass = typeof(CompUseEffect);
		}
	}
}
