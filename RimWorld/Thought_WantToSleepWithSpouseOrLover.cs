using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_WantToSleepWithSpouseOrLover : Thought_Situational
	{
		public override string LabelCap
		{
			get
			{
				DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, allowDead: false);
				return string.Format(base.CurStage.label, directPawnRelation.otherPawn.LabelShort).CapitalizeFirst();
			}
		}

		protected override float BaseMoodOffset
		{
			get
			{
				float a = -0.05f * (float)pawn.relations.OpinionOf(LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, allowDead: false).otherPawn);
				return Mathf.Min(a, -1f);
			}
		}
	}
}
