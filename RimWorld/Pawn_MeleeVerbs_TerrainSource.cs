using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_MeleeVerbs_TerrainSource : IExposable, IVerbOwner
	{
		public Pawn_MeleeVerbs parent;

		public TerrainDef def;

		public VerbTracker tracker;

		Thing IVerbOwner.ConstantCaster
		{
			get
			{
				return parent.Pawn;
			}
		}

		ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef
		{
			get
			{
				return ImplementOwnerTypeDefOf.Terrain;
			}
		}

		public VerbTracker VerbTracker => tracker;

		public List<VerbProperties> VerbProperties => null;

		public List<Tool> Tools => def.tools;

		public static Pawn_MeleeVerbs_TerrainSource Create(Pawn_MeleeVerbs parent, TerrainDef terrainDef)
		{
			Pawn_MeleeVerbs_TerrainSource pawn_MeleeVerbs_TerrainSource = new Pawn_MeleeVerbs_TerrainSource();
			pawn_MeleeVerbs_TerrainSource.parent = parent;
			pawn_MeleeVerbs_TerrainSource.def = terrainDef;
			pawn_MeleeVerbs_TerrainSource.tracker = new VerbTracker(pawn_MeleeVerbs_TerrainSource);
			return pawn_MeleeVerbs_TerrainSource;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Deep.Look(ref tracker, "tracker", this);
		}

		string IVerbOwner.UniqueVerbOwnerID()
		{
			return "TerrainVerbs_" + parent.Pawn.ThingID;
		}

		bool IVerbOwner.VerbsStillUsableBy(Pawn p)
		{
			if (p != parent.Pawn)
			{
				return false;
			}
			if (!p.Spawned || def != p.Position.GetTerrain(p.Map))
			{
				return false;
			}
			if (Find.TickManager.TicksGame < parent.lastTerrainBasedVerbUseTick + 1200)
			{
				return false;
			}
			return true;
		}
	}
}
