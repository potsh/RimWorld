using RimWorld;
using System;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public class MentalBreakDef : Def
	{
		public Type workerClass = typeof(MentalBreakWorker);

		public MentalStateDef mentalState;

		public float baseCommonality;

		public SimpleCurve commonalityFactorPerPopulationCurve;

		public MentalBreakIntensity intensity;

		public TraitDef requiredTrait;

		private MentalBreakWorker workerInt;

		public MentalBreakWorker Worker
		{
			get
			{
				if (workerInt == null && workerClass != null)
				{
					workerInt = (MentalBreakWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (intensity == MentalBreakIntensity.None)
			{
				yield return "intensity not set";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_00ec:
			/*Error near IL_00ed: Unexpected return in MoveNext()*/;
		}
	}
}
