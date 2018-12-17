using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI.Group;

namespace Verse.AI
{
	public class Pawn_JobTracker : IExposable
	{
		protected Pawn pawn;

		public Job curJob;

		public JobDriver curDriver;

		public JobQueue jobQueue = new JobQueue();

		public PawnPosture posture;

		public bool startingNewJob;

		private int jobsGivenThisTick;

		private string jobsGivenThisTickTextual = string.Empty;

		private int lastJobGivenAtFrame = -1;

		private List<int> jobsGivenRecentTicks = new List<int>(10);

		private List<string> jobsGivenRecentTicksTextual = new List<string>(10);

		public bool debugLog;

		private const int RecentJobQueueMaxLength = 10;

		private const int MaxRecentJobs = 10;

		private int lastDamageCheckTick = -99999;

		private const int DamageCheckMinInterval = 180;

		public bool HandlingFacing => curDriver != null && curDriver.HandlingFacing;

		public Pawn_JobTracker(Pawn newPawn)
		{
			pawn = newPawn;
		}

		public virtual void ExposeData()
		{
			Scribe_Deep.Look(ref curJob, "curJob");
			Scribe_Deep.Look(ref curDriver, "curDriver");
			Scribe_Deep.Look(ref jobQueue, "jobQueue");
			Scribe_Values.Look(ref posture, "posture", PawnPosture.Standing);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (curDriver != null)
				{
					curDriver.pawn = pawn;
					curDriver.job = curJob;
				}
			}
			else if (Scribe.mode == LoadSaveMode.PostLoadInit && curDriver == null && curJob != null)
			{
				Log.Warning($"Cleaning up invalid job state on {pawn}");
				EndCurrentJob(JobCondition.Errored);
			}
		}

		public virtual void JobTrackerTick()
		{
			jobsGivenThisTick = 0;
			jobsGivenThisTickTextual = string.Empty;
			if (pawn.IsHashIntervalTick(30))
			{
				ThinkResult thinkResult = DetermineNextConstantThinkTreeJob();
				if (thinkResult.IsValid && ShouldStartJobFromThinkTree(thinkResult))
				{
					CheckLeaveJoinableLordBecauseJobIssued(thinkResult);
					StartJob(thinkResult.Job, JobCondition.InterruptForced, thinkResult.SourceNode, resumeCurJobAfterwards: false, cancelBusyStances: false, pawn.thinker.ConstantThinkTree, thinkResult.Tag);
				}
			}
			if (curDriver != null)
			{
				if (curJob.expiryInterval > 0 && (Find.TickManager.TicksGame - curJob.startTick) % curJob.expiryInterval == 0 && Find.TickManager.TicksGame != curJob.startTick)
				{
					if (!curJob.expireRequiresEnemiesNearby || PawnUtility.EnemiesAreNearby(pawn, 25))
					{
						if (debugLog)
						{
							DebugLogEvent("Job expire");
						}
						if (!curJob.checkOverrideOnExpire)
						{
							EndCurrentJob(JobCondition.Succeeded);
						}
						else
						{
							CheckForJobOverride();
						}
						FinalizeTick();
						return;
					}
					if (debugLog)
					{
						DebugLogEvent("Job expire skipped because there are no enemies nearby");
					}
				}
				curDriver.DriverTick();
			}
			if (curJob == null && !pawn.Dead && pawn.mindState.Active && CanDoAnyJob())
			{
				if (debugLog)
				{
					DebugLogEvent("Starting job from Tick because curJob == null.");
				}
				TryFindAndStartJob();
			}
			FinalizeTick();
		}

		private void FinalizeTick()
		{
			jobsGivenRecentTicks.Add(jobsGivenThisTick);
			jobsGivenRecentTicksTextual.Add(jobsGivenThisTickTextual);
			while (jobsGivenRecentTicks.Count > 10)
			{
				jobsGivenRecentTicks.RemoveAt(0);
				jobsGivenRecentTicksTextual.RemoveAt(0);
			}
			if (jobsGivenThisTick != 0)
			{
				int num = 0;
				for (int i = 0; i < jobsGivenRecentTicks.Count; i++)
				{
					num += jobsGivenRecentTicks[i];
				}
				if (num >= 10)
				{
					string text = jobsGivenRecentTicksTextual.ToCommaList();
					jobsGivenRecentTicks.Clear();
					jobsGivenRecentTicksTextual.Clear();
					JobUtility.TryStartErrorRecoverJob(pawn, pawn.ToStringSafe() + " started " + 10 + " jobs in " + 10 + " ticks. List: " + text);
				}
			}
		}

		public void StartJob(Job newJob, JobCondition lastJobEndCondition = JobCondition.None, ThinkNode jobGiver = null, bool resumeCurJobAfterwards = false, bool cancelBusyStances = true, ThinkTreeDef thinkTree = null, JobTag? tag = default(JobTag?), bool fromQueue = false)
		{
			startingNewJob = true;
			try
			{
				if (!fromQueue && (!Find.TickManager.Paused || lastJobGivenAtFrame == RealTime.frameCount))
				{
					jobsGivenThisTick++;
					jobsGivenThisTickTextual = jobsGivenThisTickTextual + "(" + newJob.ToString() + ") ";
				}
				lastJobGivenAtFrame = RealTime.frameCount;
				if (jobsGivenThisTick > 10)
				{
					string text = jobsGivenThisTickTextual;
					jobsGivenThisTick = 0;
					jobsGivenThisTickTextual = string.Empty;
					startingNewJob = false;
					pawn.ClearReservationsForJob(newJob);
					JobUtility.TryStartErrorRecoverJob(pawn, pawn.ToStringSafe() + " started 10 jobs in one tick. newJob=" + newJob.ToStringSafe() + " jobGiver=" + jobGiver.ToStringSafe() + " jobList=" + text);
				}
				else
				{
					if (debugLog)
					{
						DebugLogEvent("StartJob [" + newJob + "] lastJobEndCondition=" + lastJobEndCondition + ", jobGiver=" + jobGiver + ", cancelBusyStances=" + cancelBusyStances);
					}
					if (cancelBusyStances && pawn.stances.FullBodyBusy)
					{
						pawn.stances.CancelBusyStanceHard();
					}
					if (curJob != null)
					{
						if (lastJobEndCondition == JobCondition.None)
						{
							Log.Warning(pawn + " starting job " + newJob + " from JobGiver " + pawn.mindState.lastJobGiver + " while already having job " + curJob + " without a specific job end condition.");
							lastJobEndCondition = JobCondition.InterruptForced;
						}
						if (resumeCurJobAfterwards && curJob.def.suspendable)
						{
							jobQueue.EnqueueFirst(curJob);
							if (debugLog)
							{
								DebugLogEvent("   JobQueue EnqueueFirst curJob: " + curJob);
							}
							CleanupCurrentJob(lastJobEndCondition, releaseReservations: false, cancelBusyStances);
						}
						else
						{
							CleanupCurrentJob(lastJobEndCondition, releaseReservations: true, cancelBusyStances);
						}
					}
					if (newJob == null)
					{
						Log.Warning(pawn + " tried to start doing a null job.");
					}
					else
					{
						newJob.startTick = Find.TickManager.TicksGame;
						if (pawn.Drafted || newJob.playerForced)
						{
							newJob.ignoreForbidden = true;
							newJob.ignoreDesignations = true;
						}
						curJob = newJob;
						pawn.mindState.lastJobGiver = jobGiver;
						pawn.mindState.lastJobGiverThinkTree = thinkTree;
						curDriver = curJob.MakeDriver(pawn);
						bool flag = fromQueue;
						if (curDriver.TryMakePreToilReservations(!flag))
						{
							Job job = TryOpportunisticJob(newJob);
							if (job != null)
							{
								jobQueue.EnqueueFirst(newJob);
								curJob = null;
								curDriver = null;
								StartJob(job);
							}
							else
							{
								if (tag.HasValue)
								{
									pawn.mindState.lastJobTag = tag.Value;
								}
								curDriver.SetInitialPosture();
								curDriver.Notify_Starting();
								curDriver.SetupToils();
								curDriver.ReadyForNextToil();
							}
						}
						else if (flag)
						{
							EndCurrentJob(JobCondition.QueuedNoLongerValid);
						}
						else
						{
							Log.Warning("TryMakePreToilReservations() returned false for a non-queued job right after StartJob(). This should have been checked before. curJob=" + curJob.ToStringSafe());
							EndCurrentJob(JobCondition.Errored);
						}
					}
				}
			}
			finally
			{
				startingNewJob = false;
			}
		}

		public void EndCurrentOrQueuedJob(Job job, JobCondition condition)
		{
			if (debugLog)
			{
				DebugLogEvent("EndJob [" + job + "] condition=" + condition);
			}
			QueuedJob queuedJob = jobQueue.Extract(job);
			if (queuedJob != null)
			{
				pawn.ClearReservationsForJob(queuedJob.job);
			}
			if (curJob == job)
			{
				EndCurrentJob(condition);
			}
		}

		public void EndCurrentJob(JobCondition condition, bool startNewJob = true)
		{
			if (debugLog)
			{
				DebugLogEvent("EndCurrentJob " + ((curJob == null) ? "null" : curJob.ToString()) + " condition=" + condition + " curToil=" + ((curDriver == null) ? "null_driver" : curDriver.CurToilIndex.ToString()));
			}
			if (condition == JobCondition.Ongoing)
			{
				Log.Warning("Ending a job with Ongoing as the condition. This makes no sense.");
			}
			if (condition == JobCondition.Succeeded && curJob != null && curJob.def.taleOnCompletion != null)
			{
				TaleRecorder.RecordTale(curJob.def.taleOnCompletion, curDriver.TaleParameters());
			}
			Job job = curJob;
			CleanupCurrentJob(condition, releaseReservations: true);
			if (startNewJob)
			{
				switch (condition)
				{
				case JobCondition.Errored:
				case JobCondition.ErroredPather:
					StartJob(new Job(JobDefOf.Wait, 250));
					return;
				case JobCondition.Succeeded:
					if (job != null && job.def != JobDefOf.Wait_MaintainPosture && !pawn.pather.Moving)
					{
						StartJob(new Job(JobDefOf.Wait_MaintainPosture, 1), JobCondition.None, null, resumeCurJobAfterwards: false, cancelBusyStances: false);
						return;
					}
					break;
				}
				TryFindAndStartJob();
			}
		}

		private void CleanupCurrentJob(JobCondition condition, bool releaseReservations, bool cancelBusyStancesSoft = true)
		{
			if (debugLog)
			{
				DebugLogEvent("CleanupCurrentJob " + ((curJob == null) ? "null" : curJob.def.ToString()) + " condition " + condition);
			}
			if (curJob != null)
			{
				if (releaseReservations)
				{
					pawn.ClearReservationsForJob(curJob);
				}
				if (curDriver != null)
				{
					curDriver.ended = true;
					curDriver.Cleanup(condition);
				}
				curDriver = null;
				curJob = null;
				pawn.VerifyReservations();
				if (cancelBusyStancesSoft)
				{
					pawn.stances.CancelBusyStanceSoft();
				}
				if (!pawn.Destroyed && pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
				{
					pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing _);
				}
			}
		}

		public void ClearQueuedJobs()
		{
			if (debugLog)
			{
				DebugLogEvent("ClearQueuedJobs");
			}
			while (jobQueue.Count > 0)
			{
				pawn.ClearReservationsForJob(jobQueue.Dequeue().job);
			}
		}

		public void CheckForJobOverride()
		{
			if (debugLog)
			{
				DebugLogEvent("CheckForJobOverride");
			}
			ThinkTreeDef thinkTree;
			ThinkResult thinkResult = DetermineNextJob(out thinkTree);
			if (ShouldStartJobFromThinkTree(thinkResult))
			{
				CheckLeaveJoinableLordBecauseJobIssued(thinkResult);
				StartJob(thinkResult.Job, JobCondition.InterruptOptional, thinkResult.SourceNode, resumeCurJobAfterwards: false, cancelBusyStances: false, thinkTree, thinkResult.Tag, thinkResult.FromQueue);
			}
		}

		public void StopAll(bool ifLayingKeepLaying = false)
		{
			if ((!pawn.InBed() && (pawn.CurJob == null || pawn.CurJob.def != JobDefOf.LayDown)) || !ifLayingKeepLaying)
			{
				CleanupCurrentJob(JobCondition.InterruptForced, releaseReservations: true);
			}
			ClearQueuedJobs();
		}

		private void TryFindAndStartJob()
		{
			if (pawn.thinker == null)
			{
				Log.ErrorOnce(pawn + " did TryFindAndStartJob but had no thinker.", 8573261);
			}
			else
			{
				if (curJob != null)
				{
					Log.Warning(pawn + " doing TryFindAndStartJob while still having job " + curJob);
				}
				if (debugLog)
				{
					DebugLogEvent("TryFindAndStartJob");
				}
				if (!CanDoAnyJob())
				{
					if (debugLog)
					{
						DebugLogEvent("   CanDoAnyJob is false. Clearing queue and returning");
					}
					ClearQueuedJobs();
				}
				else
				{
					ThinkTreeDef thinkTree;
					ThinkResult result = DetermineNextJob(out thinkTree);
					if (result.IsValid)
					{
						CheckLeaveJoinableLordBecauseJobIssued(result);
						Job job = result.Job;
						ThinkNode sourceNode = result.SourceNode;
						ThinkTreeDef thinkTree2 = thinkTree;
						StartJob(job, JobCondition.None, sourceNode, resumeCurJobAfterwards: false, cancelBusyStances: false, thinkTree2, result.Tag, result.FromQueue);
					}
				}
			}
		}

		public Job TryOpportunisticJob(Job job)
		{
			if ((int)pawn.def.race.intelligence < 2)
			{
				return null;
			}
			if (pawn.Faction != Faction.OfPlayer)
			{
				return null;
			}
			if (pawn.Drafted)
			{
				return null;
			}
			if (job.playerForced)
			{
				return null;
			}
			if ((int)pawn.RaceProps.intelligence < 2)
			{
				return null;
			}
			if (!job.def.allowOpportunisticPrefix)
			{
				return null;
			}
			if (pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb | WorkTags.Hauling))
			{
				return null;
			}
			if (pawn.InMentalState || pawn.IsBurning())
			{
				return null;
			}
			IntVec3 cell = job.targetA.Cell;
			if (!cell.IsValid || cell.IsForbidden(pawn))
			{
				return null;
			}
			float num = pawn.Position.DistanceTo(cell);
			if (num < 3f)
			{
				return null;
			}
			List<Thing> list = pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				float num2 = pawn.Position.DistanceTo(thing.Position);
				if (!(num2 > 30f) && !(num2 > num * 0.5f) && !(num2 + thing.Position.DistanceTo(cell) > num * 1.7f) && pawn.Map.reservationManager.FirstRespectedReserver(thing, pawn) == null && !thing.IsForbidden(pawn) && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, thing, forced: false))
				{
					StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
					IntVec3 foundCell = IntVec3.Invalid;
					if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, pawn.Map, currentPriority, pawn.Faction, out foundCell))
					{
						float num3 = foundCell.DistanceTo(cell);
						if (!(num3 > 50f) && !(num3 > num * 0.6f) && !(num2 + thing.Position.DistanceTo(foundCell) + num3 > num * 1.7f) && !(num2 + num3 > num) && pawn.Position.WithinRegions(thing.Position, pawn.Map, 25, TraverseParms.For(pawn)) && foundCell.WithinRegions(cell, pawn.Map, 25, TraverseParms.For(pawn)))
						{
							if (DebugViewSettings.drawOpportunisticJobs)
							{
								Log.Message("Opportunistic job spawned");
								pawn.Map.debugDrawer.FlashLine(pawn.Position, thing.Position, 600, SimpleColor.Red);
								pawn.Map.debugDrawer.FlashLine(thing.Position, foundCell, 600, SimpleColor.Green);
								pawn.Map.debugDrawer.FlashLine(foundCell, cell, 600, SimpleColor.Blue);
							}
							return HaulAIUtility.HaulToCellStorageJob(pawn, thing, foundCell, fitInStoreCell: false);
						}
					}
				}
			}
			return null;
		}

		private ThinkResult DetermineNextJob(out ThinkTreeDef thinkTree)
		{
			ThinkResult result = DetermineNextConstantThinkTreeJob();
			if (result.Job != null)
			{
				thinkTree = pawn.thinker.ConstantThinkTree;
				return result;
			}
			ThinkResult result2 = ThinkResult.NoJob;
			try
			{
				result2 = pawn.thinker.MainThinkNodeRoot.TryIssueJobPackage(pawn, default(JobIssueParams));
			}
			catch (Exception exception)
			{
				JobUtility.TryStartErrorRecoverJob(pawn, pawn.ToStringSafe() + " threw exception while determining job (main)", exception);
				thinkTree = null;
				return ThinkResult.NoJob;
			}
			finally
			{
			}
			thinkTree = pawn.thinker.MainThinkTree;
			return result2;
		}

		private ThinkResult DetermineNextConstantThinkTreeJob()
		{
			if (pawn.thinker.ConstantThinkTree == null)
			{
				return ThinkResult.NoJob;
			}
			try
			{
				return pawn.thinker.ConstantThinkNodeRoot.TryIssueJobPackage(pawn, default(JobIssueParams));
			}
			catch (Exception exception)
			{
				JobUtility.TryStartErrorRecoverJob(pawn, pawn.ToStringSafe() + " threw exception while determining job (constant)", exception);
			}
			finally
			{
			}
			return ThinkResult.NoJob;
		}

		private void CheckLeaveJoinableLordBecauseJobIssued(ThinkResult result)
		{
			if (result.IsValid && result.SourceNode != null)
			{
				Lord lord = pawn.GetLord();
				if (lord != null && lord.LordJob is LordJob_VoluntarilyJoinable)
				{
					bool flag = false;
					ThinkNode thinkNode = result.SourceNode;
					do
					{
						if (thinkNode.leaveJoinableLordIfIssuesJob)
						{
							flag = true;
							break;
						}
						thinkNode = thinkNode.parent;
					}
					while (thinkNode != null);
					if (flag)
					{
						lord.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
					}
				}
			}
		}

		private bool CanDoAnyJob()
		{
			return pawn.Spawned;
		}

		private bool ShouldStartJobFromThinkTree(ThinkResult thinkResult)
		{
			if (curJob == null)
			{
				return true;
			}
			if (curJob == thinkResult.Job)
			{
				return false;
			}
			if (thinkResult.FromQueue)
			{
				return true;
			}
			if (thinkResult.Job.def != curJob.def || thinkResult.SourceNode != pawn.mindState.lastJobGiver || !curDriver.IsContinuation(thinkResult.Job))
			{
				return true;
			}
			return false;
		}

		public bool IsCurrentJobPlayerInterruptible()
		{
			if (curJob != null && !curJob.def.playerInterruptible)
			{
				return false;
			}
			if (pawn.HasAttachment(ThingDefOf.Fire))
			{
				return false;
			}
			return true;
		}

		public bool TryTakeOrderedJobPrioritizedWork(Job job, WorkGiver giver, IntVec3 cell)
		{
			if (TryTakeOrderedJob(job, giver.def.tagToGive))
			{
				pawn.mindState.lastGivenWorkType = giver.def.workType;
				if (giver.def.prioritizeSustains)
				{
					pawn.mindState.priorityWork.Set(cell, giver.def.workType);
				}
				return true;
			}
			return false;
		}

		public bool TryTakeOrderedJob(Job job, JobTag tag = JobTag.Misc)
		{
			if (debugLog)
			{
				DebugLogEvent("TryTakeOrderedJob " + job);
			}
			job.playerForced = true;
			if (curJob != null && curJob.JobIsSameAs(job))
			{
				return true;
			}
			bool flag = pawn.jobs.IsCurrentJobPlayerInterruptible();
			bool flag2 = pawn.mindState.IsIdle || pawn.CurJob == null || pawn.CurJob.def.isIdle;
			bool isDownEvent = KeyBindingDefOf.QueueOrder.IsDownEvent;
			if (isDownEvent)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.QueueOrders, KnowledgeAmount.NoteTaught);
			}
			if (flag && (!isDownEvent || flag2))
			{
				pawn.stances.CancelBusyStanceSoft();
				if (debugLog)
				{
					DebugLogEvent("    Queueing job");
				}
				ClearQueuedJobs();
				if (job.TryMakePreToilReservations(pawn, errorOnFailed: true))
				{
					jobQueue.EnqueueFirst(job, tag);
					if (curJob != null)
					{
						curDriver.EndJobWith(JobCondition.InterruptForced);
					}
					else
					{
						CheckForJobOverride();
					}
					return true;
				}
				Log.Warning("TryMakePreToilReservations() returned false right after TryTakeOrderedJob(). This should have been checked before. job=" + job.ToStringSafe());
				pawn.ClearReservationsForJob(job);
				return false;
			}
			if (isDownEvent)
			{
				if (job.TryMakePreToilReservations(pawn, errorOnFailed: true))
				{
					jobQueue.EnqueueLast(job, tag);
					return true;
				}
				Log.Warning("TryMakePreToilReservations() returned false right after TryTakeOrderedJob(). This should have been checked before. job=" + job.ToStringSafe());
				pawn.ClearReservationsForJob(job);
				return false;
			}
			ClearQueuedJobs();
			if (job.TryMakePreToilReservations(pawn, errorOnFailed: true))
			{
				jobQueue.EnqueueLast(job, tag);
				return true;
			}
			Log.Warning("TryMakePreToilReservations() returned false right after TryTakeOrderedJob(). This should have been checked before. job=" + job.ToStringSafe());
			pawn.ClearReservationsForJob(job);
			return false;
		}

		public void Notify_TuckedIntoBed(Building_Bed bed)
		{
			pawn.Position = RestUtility.GetBedSleepingSlotPosFor(pawn, bed);
			pawn.Notify_Teleported(endCurrentJob: false);
			pawn.stances.CancelBusyStanceHard();
			StartJob(new Job(JobDefOf.LayDown, bed), JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, JobTag.TuckedIntoBed);
		}

		public void Notify_DamageTaken(DamageInfo dinfo)
		{
			if (curJob != null)
			{
				Job job = curJob;
				curDriver.Notify_DamageTaken(dinfo);
				if (curJob == job && dinfo.Def.ExternalViolenceFor(pawn) && dinfo.Def.canInterruptJobs && !curJob.playerForced && Find.TickManager.TicksGame >= lastDamageCheckTick + 180)
				{
					Thing instigator = dinfo.Instigator;
					if (curJob.def.checkOverrideOnDamage == CheckJobOverrideOnDamageMode.Always || (curJob.def.checkOverrideOnDamage == CheckJobOverrideOnDamageMode.OnlyIfInstigatorNotJobTarget && !curJob.AnyTargetIs(instigator)))
					{
						lastDamageCheckTick = Find.TickManager.TicksGame;
						CheckForJobOverride();
					}
				}
			}
		}

		internal void Notify_MasterDraftedOrUndrafted()
		{
			Pawn master = pawn.playerSettings.Master;
			if (master.Spawned && master.Map == pawn.Map && pawn.playerSettings.followDrafted)
			{
				EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public void DrawLinesBetweenTargets()
		{
			Vector3 a = pawn.Position.ToVector3Shifted();
			if (pawn.pather.curPath != null)
			{
				a = pawn.pather.Destination.CenterVector3;
			}
			else if (curJob != null && curJob.targetA.IsValid && (!curJob.targetA.HasThing || (curJob.targetA.Thing.Spawned && curJob.targetA.Thing.Map == pawn.Map)))
			{
				GenDraw.DrawLineBetween(a, curJob.targetA.CenterVector3, AltitudeLayer.Item.AltitudeFor());
				a = curJob.targetA.CenterVector3;
			}
			for (int i = 0; i < jobQueue.Count; i++)
			{
				if (jobQueue[i].job.targetA.IsValid)
				{
					if (!jobQueue[i].job.targetA.HasThing || (jobQueue[i].job.targetA.Thing.Spawned && jobQueue[i].job.targetA.Thing.Map == pawn.Map))
					{
						Vector3 centerVector = jobQueue[i].job.targetA.CenterVector3;
						GenDraw.DrawLineBetween(a, centerVector, AltitudeLayer.Item.AltitudeFor());
						a = centerVector;
					}
				}
				else
				{
					List<LocalTargetInfo> targetQueueA = jobQueue[i].job.targetQueueA;
					if (targetQueueA != null)
					{
						for (int j = 0; j < targetQueueA.Count; j++)
						{
							if (!targetQueueA[j].HasThing || (targetQueueA[j].Thing.Spawned && targetQueueA[j].Thing.Map == pawn.Map))
							{
								Vector3 centerVector2 = targetQueueA[j].CenterVector3;
								GenDraw.DrawLineBetween(a, centerVector2, AltitudeLayer.Item.AltitudeFor());
								a = centerVector2;
							}
						}
					}
				}
			}
		}

		public void DebugLogEvent(string s)
		{
			if (debugLog)
			{
				Log.Message(Find.TickManager.TicksGame + " " + pawn + ": " + s);
			}
		}
	}
}
