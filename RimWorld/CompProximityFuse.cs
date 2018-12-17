using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompProximityFuse : ThingComp
	{
		public CompProperties_ProximityFuse Props => (CompProperties_ProximityFuse)props;

		public override void CompTick()
		{
			if (Find.TickManager.TicksGame % 250 == 0)
			{
				CompTickRare();
			}
		}

		public override void CompTickRare()
		{
			Thing thing = GenClosest.ClosestThingReachable(parent.Position, parent.Map, ThingRequest.ForDef(Props.target), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors), Props.radius);
			if (thing != null)
			{
				parent.GetComp<CompExplosive>().StartWick();
			}
		}
	}
}
