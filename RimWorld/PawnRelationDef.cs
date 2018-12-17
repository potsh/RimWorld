using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnRelationDef : Def
	{
		public Type workerClass = typeof(PawnRelationWorker);

		[MustTranslate]
		public string labelFemale;

		public float importance;

		public bool implied;

		public bool reflexive;

		public int opinionOffset;

		public float generationChanceFactor;

		public float attractionFactor = 1f;

		public float incestOpinionOffset;

		public bool familyByBloodRelation;

		public ThoughtDef diedThought;

		public ThoughtDef diedThoughtFemale;

		public ThoughtDef soldThought;

		public ThoughtDef killedThought;

		public ThoughtDef killedThoughtFemale;

		[Unsaved]
		private PawnRelationWorker workerInt;

		public PawnRelationWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (PawnRelationWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public string GetGenderSpecificLabel(Pawn pawn)
		{
			if (pawn.gender == Gender.Female && !labelFemale.NullOrEmpty())
			{
				return labelFemale;
			}
			return label;
		}

		public string GetGenderSpecificLabelCap(Pawn pawn)
		{
			return GetGenderSpecificLabel(pawn).CapitalizeFirst();
		}

		public ThoughtDef GetGenderSpecificDiedThought(Pawn killed)
		{
			if (killed.gender == Gender.Female && diedThoughtFemale != null)
			{
				return diedThoughtFemale;
			}
			return diedThought;
		}

		public ThoughtDef GetGenderSpecificKilledThought(Pawn killed)
		{
			if (killed.gender == Gender.Female && killedThoughtFemale != null)
			{
				return killedThoughtFemale;
			}
			return killedThought;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (implied && reflexive)
			{
				yield return defName + ": implied relations can't use the \"reflexive\" option.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0118:
			/*Error near IL_0119: Unexpected return in MoveNext()*/;
		}
	}
}
