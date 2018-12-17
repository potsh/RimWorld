namespace RimWorld
{
	public class PawnGroupMakerParms
	{
		public PawnGroupKindDef groupKind;

		public int tile = -1;

		public bool inhabitants;

		public float points;

		public Faction faction;

		public TraderKindDef traderKind;

		public bool generateFightersOnly;

		public bool dontUseSingleUseRocketLaunchers;

		public RaidStrategyDef raidStrategy;

		public bool forceOneIncap;

		public int? seed;

		public override string ToString()
		{
			return "groupKind=" + groupKind + ", tile=" + tile + ", inhabitants=" + inhabitants + ", points=" + points + ", faction=" + faction + ", traderKind=" + traderKind + ", generateFightersOnly=" + generateFightersOnly + ", dontUseSingleUseRocketLaunchers=" + dontUseSingleUseRocketLaunchers + ", raidStrategy=" + raidStrategy + ", forceOneIncap=" + forceOneIncap + ", seed=" + seed;
		}
	}
}
