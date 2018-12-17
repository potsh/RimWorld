using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class GenStep_DownedRefugee : GenStep_Scatterer
	{
		public override int SeedPart => 931842770;

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			return base.CanScatterAt(c, map) && c.Standable(map);
		}

		protected override void ScatterAt(IntVec3 loc, Map map, int count = 1)
		{
			DownedRefugeeComp component = map.Parent.GetComponent<DownedRefugeeComp>();
			Pawn pawn = (component == null || !component.pawn.Any) ? DownedRefugeeQuestUtility.GenerateRefugee(map.Tile) : component.pawn.Take(component.pawn[0]);
			HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
			HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, allowBleedingWounds: false);
			GenSpawn.Spawn(pawn, loc, map);
			pawn.mindState.WillJoinColonyIfRescued = true;
			MapGenerator.rootsToUnfog.Add(loc);
		}
	}
}
