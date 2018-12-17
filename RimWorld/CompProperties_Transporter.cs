using Verse;

namespace RimWorld
{
	public class CompProperties_Transporter : CompProperties
	{
		public float massCapacity = 150f;

		public float restEffectiveness;

		public CompProperties_Transporter()
		{
			compClass = typeof(CompTransporter);
		}
	}
}
