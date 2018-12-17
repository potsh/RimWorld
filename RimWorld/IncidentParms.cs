using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentParms : IExposable
	{
		public IIncidentTarget target;

		public float points = -1f;

		public Faction faction;

		public bool forced;

		public IntVec3 spawnCenter = IntVec3.Invalid;

		public Rot4 spawnRotation = Rot4.South;

		public bool generateFightersOnly;

		public bool dontUseSingleUseRocketLaunchers;

		public RaidStrategyDef raidStrategy;

		public PawnsArrivalModeDef raidArrivalMode;

		public bool raidForceOneIncap;

		public bool raidNeverFleeIndividual;

		public bool raidArrivalModeForQuickMilitaryAid;

		public Dictionary<Pawn, int> pawnGroups;

		public int? pawnGroupMakerSeed;

		public TraderKindDef traderKind;

		public int podOpenDelay = 140;

		private List<Pawn> tmpPawns;

		private List<int> tmpGroups;

		public void ExposeData()
		{
			Scribe_References.Look(ref target, "target");
			Scribe_Values.Look(ref points, "threatPoints", 0f);
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref forced, "forced", defaultValue: false);
			Scribe_Values.Look(ref spawnCenter, "spawnCenter");
			Scribe_Values.Look(ref spawnRotation, "spawnRotation");
			Scribe_Values.Look(ref generateFightersOnly, "generateFightersOnly", defaultValue: false);
			Scribe_Values.Look(ref dontUseSingleUseRocketLaunchers, "dontUseSingleUseRocketLaunchers", defaultValue: false);
			Scribe_Defs.Look(ref raidStrategy, "raidStrategy");
			Scribe_Defs.Look(ref raidArrivalMode, "raidArrivalMode");
			Scribe_Values.Look(ref raidForceOneIncap, "raidForceIncap", defaultValue: false);
			Scribe_Values.Look(ref raidNeverFleeIndividual, "raidNeverFleeIndividual", defaultValue: false);
			Scribe_Values.Look(ref raidArrivalModeForQuickMilitaryAid, "raidArrivalModeForQuickMilitaryAid", defaultValue: false);
			Scribe_Collections.Look(ref pawnGroups, "pawnGroups", LookMode.Reference, LookMode.Value, ref tmpPawns, ref tmpGroups);
			Scribe_Values.Look(ref pawnGroupMakerSeed, "pawnGroupMakerSeed");
			Scribe_Defs.Look(ref traderKind, "traderKind");
			Scribe_Values.Look(ref podOpenDelay, "podOpenDelay", 140);
		}

		public override string ToString()
		{
			string text = "(";
			if (target != null)
			{
				string text2 = text;
				text = text2 + "target=" + target + " ";
			}
			if (points >= 0f)
			{
				string text2 = text;
				text = text2 + "points=" + points + " ";
			}
			if (generateFightersOnly)
			{
				string text2 = text;
				text = text2 + "generateFightersOnly=" + generateFightersOnly + " ";
			}
			if (raidStrategy != null)
			{
				text = text + "raidStrategy=" + raidStrategy.defName + " ";
			}
			return text + ")";
		}
	}
}
