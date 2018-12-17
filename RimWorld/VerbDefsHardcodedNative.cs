using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class VerbDefsHardcodedNative
	{
		public static IEnumerable<VerbProperties> AllVerbDefs()
		{
			yield return new VerbProperties
			{
				verbClass = typeof(Verb_BeatFire),
				category = VerbCategory.BeatFire,
				range = 1.42f,
				noiseRadius = 3f,
				targetParams = 
				{
					canTargetFires = true,
					canTargetPawns = false,
					canTargetBuildings = false,
					mapObjectTargetsMustBeAutoAttackable = false
				},
				warmupTime = 0f,
				defaultCooldownTime = 1.1f,
				soundCast = SoundDefOf.Interact_BeatFire
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
