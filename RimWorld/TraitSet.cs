using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TraitSet : IExposable
	{
		protected Pawn pawn;

		public List<Trait> allTraits = new List<Trait>();

		public IEnumerable<MentalBreakDef> TheOnlyAllowedMentalBreaks
		{
			get
			{
				int j = 0;
				Trait trait;
				int i;
				while (true)
				{
					if (j >= allTraits.Count)
					{
						yield break;
					}
					trait = allTraits[j];
					if (trait.CurrentData.theOnlyAllowedMentalBreaks != null)
					{
						i = 0;
						if (i < trait.CurrentData.theOnlyAllowedMentalBreaks.Count)
						{
							break;
						}
					}
					j++;
				}
				yield return trait.CurrentData.theOnlyAllowedMentalBreaks[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public TraitSet(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref allTraits, "allTraits", LookMode.Deep);
		}

		public void GainTrait(Trait trait)
		{
			if (HasTrait(trait.def))
			{
				Log.Warning(pawn + " already has trait " + trait.def);
			}
			else
			{
				allTraits.Add(trait);
				if (pawn.workSettings != null)
				{
					pawn.workSettings.Notify_GainedTrait();
				}
				pawn.story.Notify_TraitChanged();
				if (pawn.skills != null)
				{
					pawn.skills.Notify_SkillDisablesChanged();
				}
				if (!pawn.Dead && pawn.RaceProps.Humanlike)
				{
					pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
				}
			}
		}

		public bool HasTrait(TraitDef tDef)
		{
			for (int i = 0; i < allTraits.Count; i++)
			{
				if (allTraits[i].def == tDef)
				{
					return true;
				}
			}
			return false;
		}

		public Trait GetTrait(TraitDef tDef)
		{
			for (int i = 0; i < allTraits.Count; i++)
			{
				if (allTraits[i].def == tDef)
				{
					return allTraits[i];
				}
			}
			return null;
		}

		public int DegreeOfTrait(TraitDef tDef)
		{
			for (int i = 0; i < allTraits.Count; i++)
			{
				if (allTraits[i].def == tDef)
				{
					return allTraits[i].Degree;
				}
			}
			return 0;
		}
	}
}
