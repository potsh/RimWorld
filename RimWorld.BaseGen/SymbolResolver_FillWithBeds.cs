using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_FillWithBeds : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef thingDef = (rp.singleThingDef != null) ? rp.singleThingDef : ((rp.faction == null || (int)rp.faction.def.techLevel < 3) ? Rand.Element(ThingDefOf.Bed, ThingDefOf.Bedroll, ThingDefOf.SleepingSpot) : ThingDefOf.Bed);
			ThingDef singleThingStuff = (rp.singleThingStuff == null || !rp.singleThingStuff.stuffProps.CanMake(thingDef)) ? GenStuff.RandomStuffInexpensiveFor(thingDef, rp.faction) : rp.singleThingStuff;
			bool @bool = Rand.Bool;
			foreach (IntVec3 item in rp.rect)
			{
				IntVec3 current = item;
				if (@bool)
				{
					if (current.x % 3 != 0 || current.z % 2 != 0)
					{
						continue;
					}
				}
				else if (current.x % 2 != 0 || current.z % 3 != 0)
				{
					continue;
				}
				Rot4 rot = (!@bool) ? Rot4.North : Rot4.West;
				if (!GenSpawn.WouldWipeAnythingWith(current, rot, thingDef, map, (Thing x) => x.def.category == ThingCategory.Building) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(GenAdj.OccupiedRect(current, rot, thingDef.Size), map))
				{
					ResolveParams resolveParams = rp;
					resolveParams.rect = GenAdj.OccupiedRect(current, rot, thingDef.size);
					resolveParams.singleThingDef = thingDef;
					resolveParams.singleThingStuff = singleThingStuff;
					resolveParams.thingRot = rot;
					BaseGen.symbolStack.Push("bed", resolveParams);
				}
			}
		}
	}
}
