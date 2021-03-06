using Verse;

namespace RimWorld
{
	public class Pawn_FoodRestrictionTracker : IExposable
	{
		public Pawn pawn;

		private FoodRestriction curRestriction;

		public FoodRestriction CurrentFoodRestriction
		{
			get
			{
				if (curRestriction == null)
				{
					curRestriction = Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
				}
				return curRestriction;
			}
			set
			{
				curRestriction = value;
			}
		}

		public bool Configurable => pawn.RaceProps.Humanlike && !pawn.Destroyed && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer);

		public Pawn_FoodRestrictionTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public Pawn_FoodRestrictionTracker()
		{
		}

		public FoodRestriction GetCurrentRespectedRestriction(Pawn getter = null)
		{
			if (!Configurable)
			{
				return null;
			}
			if (pawn.Faction != Faction.OfPlayer && (getter == null || getter.Faction != Faction.OfPlayer))
			{
				return null;
			}
			if (pawn.InMentalState)
			{
				return null;
			}
			return CurrentFoodRestriction;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref curRestriction, "curRestriction");
		}
	}
}
