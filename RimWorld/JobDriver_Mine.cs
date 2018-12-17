using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Mine : JobDriver
	{
		private int ticksToPickHit = -1000;

		private Effecter effecter;

		public const int BaseTicksBetweenPickHits = 100;

		private const int BaseDamagePerPickHit_NaturalRock = 80;

		private const int BaseDamagePerPickHit_NotNaturalRock = 40;

		private const float MinMiningSpeedFactorForNPCs = 0.6f;

		private Thing MineTarget => job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = MineTarget;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0036: stateMachine*/;
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnCellMissingDesignation(TargetIndex.A, DesignationDefOf.Mine);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private void ResetTicksToPickHit()
		{
			float num = pawn.GetStatValue(StatDefOf.MiningSpeed);
			if (num < 0.6f && pawn.Faction != Faction.OfPlayer)
			{
				num = 0.6f;
			}
			ticksToPickHit = (int)Math.Round((double)(100f / num));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksToPickHit, "ticksToPickHit", 0);
		}
	}
}
