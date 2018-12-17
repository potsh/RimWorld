using RimWorld;
using System;

namespace Verse.AI
{
	public static class JobUtility
	{
		private static bool startingErrorRecoverJob;

		public static void TryStartErrorRecoverJob(Pawn pawn, string message, Exception exception = null, JobDriver concreteDriver = null)
		{
			string msg = message;
			AppendVarsInfoToDebugMessage(pawn, ref msg, concreteDriver);
			if (exception != null)
			{
				msg = msg + "\n" + exception;
			}
			Log.Error(msg);
			if (pawn.jobs != null)
			{
				if (pawn.jobs.curJob != null)
				{
					pawn.jobs.EndCurrentJob(JobCondition.Errored, startNewJob: false);
				}
				if (startingErrorRecoverJob)
				{
					Log.Error("An error occurred while starting an error recover job. We have to stop now to avoid infinite loops. This means that the pawn is now jobless which can cause further bugs. pawn=" + pawn.ToStringSafe());
				}
				else
				{
					startingErrorRecoverJob = true;
					try
					{
						pawn.jobs.StartJob(new Job(JobDefOf.Wait, 150));
					}
					finally
					{
						startingErrorRecoverJob = false;
					}
				}
			}
		}

		private static void AppendVarsInfoToDebugMessage(Pawn pawn, ref string msg, JobDriver concreteDriver)
		{
			if (concreteDriver != null)
			{
				string text = msg;
				msg = text + " driver=" + concreteDriver.GetType().Name + " (toilIndex=" + concreteDriver.CurToilIndex + ")";
				if (concreteDriver.job != null)
				{
					msg = msg + " driver.job=(" + concreteDriver.job.ToStringSafe() + ")";
				}
			}
			else if (pawn.jobs != null)
			{
				if (pawn.jobs.curDriver != null)
				{
					string text = msg;
					msg = text + " curDriver=" + pawn.jobs.curDriver.GetType().Name + " (toilIndex=" + pawn.jobs.curDriver.CurToilIndex + ")";
				}
				if (pawn.jobs.curJob != null)
				{
					msg = msg + " curJob=(" + pawn.jobs.curJob.ToStringSafe() + ")";
				}
			}
			if (pawn.mindState != null)
			{
				msg = msg + " lastJobGiver=" + pawn.mindState.lastJobGiver.ToStringSafe();
			}
		}
	}
}
