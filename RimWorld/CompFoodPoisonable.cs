using Verse;

namespace RimWorld
{
	public class CompFoodPoisonable : ThingComp
	{
		private float poisonPct;

		public FoodPoisonCause cause;

		public float PoisonPercent => poisonPct;

		public void SetPoisoned(FoodPoisonCause newCause)
		{
			poisonPct = 1f;
			cause = newCause;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref poisonPct, "poisonPct", 0f);
			Scribe_Values.Look(ref cause, "cause", FoodPoisonCause.Unknown);
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			CompFoodPoisonable compFoodPoisonable = piece.TryGetComp<CompFoodPoisonable>();
			compFoodPoisonable.poisonPct = poisonPct;
			compFoodPoisonable.cause = cause;
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			base.PreAbsorbStack(otherStack, count);
			CompFoodPoisonable compFoodPoisonable = otherStack.TryGetComp<CompFoodPoisonable>();
			if (cause == FoodPoisonCause.Unknown && compFoodPoisonable.cause != 0)
			{
				cause = compFoodPoisonable.cause;
			}
			else if (compFoodPoisonable.cause != 0 || cause != 0)
			{
				float num = poisonPct * (float)parent.stackCount;
				float num2 = compFoodPoisonable.poisonPct * (float)count;
				cause = ((!(num > num2)) ? compFoodPoisonable.cause : cause);
			}
			poisonPct = GenMath.WeightedAverage(poisonPct, (float)parent.stackCount, compFoodPoisonable.poisonPct, (float)count);
		}

		public override void PostIngested(Pawn ingester)
		{
			if (Rand.Chance(poisonPct * Find.Storyteller.difficulty.foodPoisonChanceFactor))
			{
				FoodUtility.AddFoodPoisoningHediff(ingester, parent, cause);
			}
		}
	}
}
