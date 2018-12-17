using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Training : ITab
	{
		public override bool IsVisible => base.SelPawn.training != null && base.SelPawn.Faction == Faction.OfPlayer;

		public ITab_Pawn_Training()
		{
			size = new Vector2(300f, 150f + 28f * (float)DefDatabase<TrainableDef>.DefCount);
			labelKey = "TabTraining";
			tutorTag = "Training";
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(17f);
			rect.yMin += 10f;
			TrainingCardUtility.DrawTrainingCard(rect, base.SelPawn);
		}
	}
}
