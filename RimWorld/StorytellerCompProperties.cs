using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerCompProperties
	{
		[TranslationHandle]
		public Type compClass;

		public float minDaysPassed;

		public List<IncidentTargetTagDef> allowedTargetTags;

		public List<IncidentTargetTagDef> disallowedTargetTags;

		public float minIncChancePopulationIntentFactor = 0.05f;

		public StorytellerCompProperties()
		{
		}

		public StorytellerCompProperties(Type compClass)
		{
			this.compClass = compClass;
		}

		public virtual IEnumerable<string> ConfigErrors(StorytellerDef parentDef)
		{
			if (compClass == null)
			{
				yield return "a StorytellerCompProperties has null compClass.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public virtual void ResolveReferences(StorytellerDef parentDef)
		{
		}
	}
}
