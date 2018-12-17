using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_DraftController : IExposable
	{
		public Pawn pawn;

		private bool draftedInt;

		private bool fireAtWillInt = true;

		private AutoUndrafter autoUndrafter;

		public bool Drafted
		{
			get
			{
				return draftedInt;
			}
			set
			{
				if (value != draftedInt)
				{
					pawn.mindState.priorityWork.ClearPrioritizedWorkAndJobQueue();
					fireAtWillInt = true;
					draftedInt = value;
					if (!value && pawn.Spawned)
					{
						pawn.Map.pawnDestinationReservationManager.ReleaseAllClaimedBy(pawn);
					}
					pawn.jobs.ClearQueuedJobs();
					if (pawn.jobs.curJob != null && pawn.jobs.IsCurrentJobPlayerInterruptible())
					{
						pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					if (draftedInt)
					{
						Lord lord = pawn.GetLord();
						if (lord != null && lord.LordJob is LordJob_VoluntarilyJoinable)
						{
							lord.Notify_PawnLost(pawn, PawnLostCondition.Drafted);
						}
						autoUndrafter.Notify_Drafted();
					}
					else if (pawn.playerSettings != null)
					{
						pawn.playerSettings.animalsReleased = false;
					}
					foreach (Pawn item in PawnUtility.SpawnedMasteredPawns(pawn))
					{
						item.jobs.Notify_MasterDraftedOrUndrafted();
					}
				}
			}
		}

		public bool FireAtWill
		{
			get
			{
				return fireAtWillInt;
			}
			set
			{
				fireAtWillInt = value;
				if (!fireAtWillInt && pawn.stances.curStance is Stance_Warmup)
				{
					pawn.stances.CancelBusyStanceSoft();
				}
			}
		}

		public Pawn_DraftController(Pawn pawn)
		{
			this.pawn = pawn;
			autoUndrafter = new AutoUndrafter(pawn);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref draftedInt, "drafted", defaultValue: false);
			Scribe_Values.Look(ref fireAtWillInt, "fireAtWill", defaultValue: true);
			Scribe_Deep.Look(ref autoUndrafter, "autoUndrafter", pawn);
		}

		public void DraftControllerTick()
		{
			autoUndrafter.AutoUndraftTick();
		}

		internal IEnumerable<Gizmo> GetGizmos()
		{
			Command_Toggle draft = new Command_Toggle
			{
				hotKey = KeyBindingDefOf.Command_ColonistDraft,
				isActive = this.get_Drafted,
				toggleAction = delegate
				{
					((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0062: stateMachine*/)._0024this.Drafted = !((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0062: stateMachine*/)._0024this.Drafted;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
					if (((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0062: stateMachine*/)._0024this.Drafted)
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
					}
				},
				defaultDesc = "CommandToggleDraftDesc".Translate(),
				icon = TexCommand.Draft,
				turnOnSound = SoundDefOf.DraftOn,
				turnOffSound = SoundDefOf.DraftOff
			};
			if (!Drafted)
			{
				draft.defaultLabel = "CommandDraftLabel".Translate();
			}
			if (pawn.Downed)
			{
				draft.Disable("IsIncapped".Translate(pawn.LabelShort, pawn));
			}
			if (!Drafted)
			{
				draft.tutorTag = "Draft";
			}
			else
			{
				draft.tutorTag = "Undraft";
			}
			yield return (Gizmo)draft;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		internal void Notify_PrimaryWeaponChanged()
		{
			fireAtWillInt = true;
		}
	}
}
