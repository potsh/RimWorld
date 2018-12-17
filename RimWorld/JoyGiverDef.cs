using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class JoyGiverDef : Def
	{
		public Type giverClass;

		public float baseChance;

		public bool requireChair = true;

		public List<ThingDef> thingDefs;

		public JobDef jobDef;

		public bool desireSit = true;

		public float pctPawnsEverDo = 1f;

		public bool unroofedOnly;

		public JoyKindDef joyKind;

		public List<PawnCapacityDef> requiredCapacities = new List<PawnCapacityDef>();

		public bool canDoWhileInBed;

		private JoyGiver workerInt;

		public JoyGiver Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (JoyGiver)Activator.CreateInstance(giverClass);
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
			if (jobDef != null && jobDef.joyKind != joyKind)
			{
				yield return "jobDef " + jobDef + " has joyKind " + jobDef.joyKind + " which does not match our joyKind " + joyKind;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0159:
			/*Error near IL_015a: Unexpected return in MoveNext()*/;
		}
	}
}
