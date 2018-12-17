using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public class JobDriver_FollowClose : JobDriver
	{
		private const TargetIndex FolloweeInd = TargetIndex.A;

		private const int CheckPathIntervalTicks = 30;

		private Pawn Followee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		private bool CurrentlyWalkingToFollowee => pawn.pather.Moving && pawn.pather.Destination == Followee;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			if (job.followRadius <= 0f)
			{
				Log.Error("Follow radius is <= 0. pawn=" + pawn.ToStringSafe());
				job.followRadius = 10f;
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return new Toil
			{
				tickAction = delegate
				{
					Pawn followee = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.Followee;
					float followRadius = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.job.followRadius;
					if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.Moving || ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.IsHashIntervalTick(30))
					{
						bool flag = false;
						if (((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.CurrentlyWalkingToFollowee)
						{
							if (NearFollowee(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn, followee, followRadius))
							{
								flag = true;
							}
						}
						else
						{
							float radius = followRadius * 1.2f;
							if (NearFollowee(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn, followee, radius))
							{
								flag = true;
							}
							else
							{
								if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.CanReach(followee, PathEndMode.Touch, Danger.Deadly))
								{
									((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.EndJobWith(JobCondition.Incompletable);
									return;
								}
								((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.StartPath(followee, PathEndMode.Touch);
								((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.locomotionUrgencySameAs = null;
							}
						}
						if (flag)
						{
							if (NearDestinationOrNotMoving(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn, followee, followRadius))
							{
								((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.EndJobWith(JobCondition.Succeeded);
							}
							else
							{
								IntVec3 lastPassableCellInPath = followee.pather.LastPassableCellInPath;
								if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.Moving || ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.Destination.HasThing || !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.Destination.Cell.InHorDistOf(lastPassableCellInPath, followRadius))
								{
									IntVec3 intVec = CellFinder.RandomClosewalkCellNear(lastPassableCellInPath, ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.Map, Mathf.FloorToInt(followRadius));
									if (intVec == ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.Position)
									{
										((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.EndJobWith(JobCondition.Succeeded);
									}
									else if (intVec.IsValid && ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly))
									{
										((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.StartPath(intVec, PathEndMode.OnCell);
										((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.locomotionUrgencySameAs = followee;
									}
									else
									{
										((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.EndJobWith(JobCondition.Incompletable);
									}
								}
							}
						}
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override bool IsContinuation(Job j)
		{
			return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}

		public static bool FarEnoughAndPossibleToStartJob(Pawn follower, Pawn followee, float radius)
		{
			if (radius <= 0f)
			{
				string text = "Checking follow job with radius <= 0. pawn=" + follower.ToStringSafe();
				if (follower.mindState != null && follower.mindState.duty != null)
				{
					text = text + " duty=" + follower.mindState.duty.def;
				}
				Log.ErrorOnce(text, follower.thingIDNumber ^ 0x324308F9);
				return false;
			}
			if (!follower.CanReach(followee, PathEndMode.OnCell, Danger.Deadly))
			{
				return false;
			}
			float radius2 = radius * 1.2f;
			return !NearFollowee(follower, followee, radius2) || (!NearDestinationOrNotMoving(follower, followee, radius2) && follower.CanReach(followee.pather.LastPassableCellInPath, PathEndMode.OnCell, Danger.Deadly));
		}

		private static bool NearFollowee(Pawn follower, Pawn followee, float radius)
		{
			if (follower.Position.AdjacentTo8WayOrInside(followee.Position))
			{
				return true;
			}
			return follower.Position.InHorDistOf(followee.Position, radius) && GenSight.LineOfSight(follower.Position, followee.Position, follower.Map);
		}

		private static bool NearDestinationOrNotMoving(Pawn follower, Pawn followee, float radius)
		{
			if (!followee.pather.Moving)
			{
				return true;
			}
			IntVec3 lastPassableCellInPath = followee.pather.LastPassableCellInPath;
			if (!lastPassableCellInPath.IsValid)
			{
				return true;
			}
			if (follower.Position.AdjacentTo8WayOrInside(lastPassableCellInPath))
			{
				return true;
			}
			return follower.Position.InHorDistOf(lastPassableCellInPath, radius);
		}
	}
}
