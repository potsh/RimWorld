using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class WorldObjectCompProperties
	{
		[TranslationHandle]
		public Type compClass = typeof(WorldObjectComp);

		public virtual IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
		{
			if (compClass == null)
			{
				yield return parentDef.defName + " has WorldObjectCompProperties with null compClass.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public virtual void ResolveReferences(WorldObjectDef parentDef)
		{
		}
	}
}
