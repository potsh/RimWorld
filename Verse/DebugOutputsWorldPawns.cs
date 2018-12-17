using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	[HasDebugOutput]
	public class DebugOutputsWorldPawns
	{
		[DebugOutput]
		[Category("World pawns")]
		[ModeRestrictionPlay]
		public static void ColonistRelativeChance()
		{
			HashSet<Pawn> hashSet = new HashSet<Pawn>(Find.WorldPawns.AllPawnsAliveOrDead);
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < 500; i++)
			{
				PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true);
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				list.Add(pawn);
				if (!pawn.IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
				}
			}
			int num = list.Count((Pawn x) => PawnRelationUtility.GetMostImportantColonyRelative(x) != null);
			Log.Message("Colony relatives: " + ((float)num / 500f).ToStringPercent() + " (" + num + " of " + 500 + ")");
			foreach (Pawn item in Find.WorldPawns.AllPawnsAliveOrDead.ToList())
			{
				if (!hashSet.Contains(item))
				{
					Find.WorldPawns.RemovePawn(item);
					Find.WorldPawns.PassToWorld(item, PawnDiscardDecideMode.Discard);
				}
			}
		}

		[DebugOutput]
		[Category("World pawns")]
		[ModeRestrictionPlay]
		public static void KidnappedPawns()
		{
			Find.FactionManager.LogKidnappedPawns();
		}

		[DebugOutput]
		[Category("World pawns")]
		[ModeRestrictionPlay]
		public static void WorldPawnList()
		{
			Find.WorldPawns.LogWorldPawns();
		}

		[DebugOutput]
		[Category("World pawns")]
		[ModeRestrictionPlay]
		public static void WorldPawnMothballInfo()
		{
			Find.WorldPawns.LogWorldPawnMothballPrevention();
		}

		[DebugOutput]
		[Category("World pawns")]
		[ModeRestrictionPlay]
		public static void WorldPawnGcBreakdown()
		{
			Find.WorldPawns.gc.LogGC();
		}

		[DebugOutput]
		[Category("World pawns")]
		[ModeRestrictionPlay]
		public static void WorldPawnDotgraph()
		{
			Find.WorldPawns.gc.LogDotgraph();
		}

		[DebugOutput]
		[Category("World pawns")]
		[ModeRestrictionPlay]
		public static void RunWorldPawnGc()
		{
			Find.WorldPawns.gc.RunGC();
		}

		[DebugOutput]
		[Category("World pawns")]
		[ModeRestrictionPlay]
		public static void RunWorldPawnMothball()
		{
			Find.WorldPawns.DebugRunMothballProcessing();
		}
	}
}
