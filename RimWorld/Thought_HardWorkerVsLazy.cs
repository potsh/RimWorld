namespace RimWorld
{
	public class Thought_HardWorkerVsLazy : Thought_SituationalSocial
	{
		public override float OpinionOffset()
		{
			int num = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Industriousness);
			if (num <= 0)
			{
				switch (num)
				{
				case 0:
					return -5f;
				case -1:
					return -20f;
				default:
					return -30f;
				}
			}
			return 0f;
		}
	}
}
