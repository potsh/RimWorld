using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiverDef : Def
	{
		public Type giverClass;

		public WorkTypeDef workType;

		public WorkTags workTags;

		public int priorityInType;

		[MustTranslate]
		public string verb;

		[MustTranslate]
		public string gerund;

		public bool scanThings = true;

		public bool scanCells;

		public bool emergency;

		public List<PawnCapacityDef> requiredCapacities = new List<PawnCapacityDef>();

		public bool directOrderable = true;

		public bool prioritizeSustains;

		public bool nonColonistsCanDo;

		public JobTag tagToGive = JobTag.MiscWork;

		public WorkGiverEquivalenceGroupDef equivalenceGroup;

		public bool canBeDoneWhileDrafted;

		public int autoTakeablePriorityDrafted = -1;

		public ThingDef forceMote;

		public List<ThingDef> fixedBillGiverDefs;

		public bool billGiversAllHumanlikes;

		public bool billGiversAllHumanlikesCorpses;

		public bool billGiversAllMechanoids;

		public bool billGiversAllMechanoidsCorpses;

		public bool billGiversAllAnimals;

		public bool billGiversAllAnimalsCorpses;

		public bool tendToHumanlikesOnly;

		public bool tendToAnimalsOnly;

		public bool feedHumanlikesOnly;

		public bool feedAnimalsOnly;

		[Unsaved]
		private WorkGiver workerInt;

		public WorkGiver Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (WorkGiver)Activator.CreateInstance(giverClass);
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
					string error = enumerator.Current;
					yield return error;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (verb.NullOrEmpty())
			{
				yield return defName + " lacks a verb.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (gerund.NullOrEmpty())
			{
				yield return defName + " lacks a gerund.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0149:
			/*Error near IL_014a: Unexpected return in MoveNext()*/;
		}
	}
}
