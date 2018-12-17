using Verse;

namespace RimWorld
{
	internal class IncidentWorker_PoisonShipPartCrash : IncidentWorker_ShipPartCrash
	{
		protected override int CountToSpawn => Rand.RangeInclusive(1, 1);
	}
}
