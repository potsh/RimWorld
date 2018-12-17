using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class CompTargetEffect_GoodwillImpact : CompTargetEffect
	{
		protected CompProperties_TargetEffect_GoodwillImpact PropsGoodwillImpact => (CompProperties_TargetEffect_GoodwillImpact)props;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (user.Faction != null && target.Faction != null && !target.Faction.HostileTo(user.Faction))
			{
				Faction faction = target.Faction;
				Faction faction2 = user.Faction;
				int goodwillImpact = PropsGoodwillImpact.goodwillImpact;
				string reason = "GoodwillChangedReason_UsedItem".Translate(parent.LabelShort, target.LabelShort, parent.Named("ITEM"), target.Named("TARGET"));
				GlobalTargetInfo? lookTarget = target;
				faction.TryAffectGoodwillWith(faction2, goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, reason, lookTarget);
			}
		}
	}
}
