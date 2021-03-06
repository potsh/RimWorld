using RimWorld;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Verse
{
	public class HediffComp_HealPermanentWounds : HediffComp
	{
		private int ticksToHeal;

		[CompilerGenerated]
		private static Func<Hediff, bool> _003C_003Ef__mg_0024cache0;

		public HediffCompProperties_HealPermanentWounds Props => (HediffCompProperties_HealPermanentWounds)props;

		public override void CompPostMake()
		{
			base.CompPostMake();
			ResetTicksToHeal();
		}

		private void ResetTicksToHeal()
		{
			ticksToHeal = Rand.Range(15, 30) * 60000;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksToHeal--;
			if (ticksToHeal <= 0)
			{
				TryHealRandomPermanentWound();
				ResetTicksToHeal();
			}
		}

		private void TryHealRandomPermanentWound()
		{
			if (base.Pawn.health.hediffSet.hediffs.Where(HediffUtility.IsPermanent).TryRandomElement(out Hediff result))
			{
				result.Severity = 0f;
				if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
				{
					Messages.Message("MessagePermanentWoundHealed".Translate(parent.LabelCap, base.Pawn.LabelShort, result.Label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent);
				}
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
		}

		public override string CompDebugString()
		{
			return "ticksToHeal: " + ticksToHeal;
		}
	}
}
