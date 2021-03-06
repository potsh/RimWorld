using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class CompProperties
	{
		[TranslationHandle]
		public Type compClass = typeof(ThingComp);

		public CompProperties()
		{
		}

		public CompProperties(Type compClass)
		{
			this.compClass = compClass;
		}

		public virtual void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude)
		{
		}

		public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (compClass == null)
			{
				yield return parentDef.defName + " has CompProperties with null compClass.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public virtual void ResolveReferences(ThingDef parentDef)
		{
		}

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			yield break;
		}
	}
}
