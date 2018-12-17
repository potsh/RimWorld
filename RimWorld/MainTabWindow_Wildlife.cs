using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Wildlife : MainTabWindow_PawnTable
	{
		protected override PawnTableDef PawnTableDef => PawnTableDefOf.Wildlife;

		protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.AllPawns
		where p.Spawned && p.Faction == null && p.AnimalOrWildMan() && !p.Position.Fogged(p.Map)
		select p;

		public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}
}
