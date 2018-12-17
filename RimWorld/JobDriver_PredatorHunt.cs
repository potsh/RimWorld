using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PredatorHunt : JobDriver
	{
		private bool notifiedPlayerAttacked;

		private bool notifiedPlayerAttacking;

		private bool firstHit = true;

		public const TargetIndex PreyInd = TargetIndex.A;

		private const TargetIndex CorpseInd = TargetIndex.A;

		private const int MaxHuntTicks = 5000;

		public Pawn Prey
		{
			get
			{
				Corpse corpse = Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				return (Pawn)job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Corpse Corpse => job.GetTarget(TargetIndex.A).Thing as Corpse;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref firstHit, "firstHit", defaultValue: false);
			Scribe_Values.Look(ref notifiedPlayerAttacking, "notifiedPlayerAttacking", defaultValue: false);
		}

		public override string GetReport()
		{
			if (Corpse != null)
			{
				return ReportStringProcessed(JobDefOf.Ingest.reportString);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_004a: stateMachine*/;
			AddFinishAction(delegate
			{
				_003CMakeNewToils_003Ec__Iterator._0024this.Map.attackTargetsCache.UpdateTarget(_003CMakeNewToils_003Ec__Iterator._0024this.pawn);
			});
			Toil prepareToEatCorpse = new Toil();
			prepareToEatCorpse.initAction = delegate
			{
				Pawn actor = prepareToEatCorpse.actor;
				Corpse corpse = _003CMakeNewToils_003Ec__Iterator._0024this.Corpse;
				if (corpse == null)
				{
					Pawn prey = _003CMakeNewToils_003Ec__Iterator._0024this.Prey;
					if (prey == null)
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable);
						return;
					}
					corpse = prey.Corpse;
					if (corpse == null || !corpse.Spawned)
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable);
						return;
					}
				}
				if (actor.Faction == Faction.OfPlayer)
				{
					corpse.SetForbidden(value: false, warnOnFail: false);
				}
				else
				{
					corpse.SetForbidden(value: true, warnOnFail: false);
				}
				actor.CurJob.SetTarget(TargetIndex.A, corpse);
			};
			yield return Toils_General.DoAtomic(delegate
			{
				_003CMakeNewToils_003Ec__Iterator._0024this.Map.attackTargetsCache.UpdateTarget(_003CMakeNewToils_003Ec__Iterator._0024this.pawn);
			});
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override void Notify_DamageTaken(DamageInfo dinfo)
		{
			base.Notify_DamageTaken(dinfo);
			if (dinfo.Def.ExternalViolenceFor(pawn) && dinfo.Def.isRanged && dinfo.Instigator != null && dinfo.Instigator != Prey && !pawn.InMentalState && !pawn.Downed)
			{
				pawn.mindState.StartFleeingBecauseOfPawnAction(dinfo.Instigator);
			}
		}

		private void CheckWarnPlayer()
		{
			if (!notifiedPlayerAttacking)
			{
				Pawn prey = Prey;
				if (prey.Spawned && prey.Faction == Faction.OfPlayer && Find.TickManager.TicksGame > pawn.mindState.lastPredatorHuntingPlayerNotificationTick + 2500 && prey.Position.InHorDistOf(pawn.Position, 60f))
				{
					if (prey.RaceProps.Humanlike)
					{
						Find.LetterStack.ReceiveLetter("LetterLabelPredatorHuntingColonist".Translate(pawn.LabelShort, prey.LabelDefinite(), pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), "LetterPredatorHuntingColonist".Translate(pawn.LabelIndefinite(), prey.LabelDefinite(), pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), LetterDefOf.ThreatBig, pawn);
					}
					else
					{
						Messages.Message("MessagePredatorHuntingPlayerAnimal".Translate(pawn.LabelIndefinite(), prey.LabelDefinite(), pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), pawn, MessageTypeDefOf.ThreatBig);
					}
					pawn.mindState.Notify_PredatorHuntingPlayerNotification();
					notifiedPlayerAttacking = true;
				}
			}
		}
	}
}
