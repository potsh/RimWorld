using System;
using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties
	{
		[TranslationHandle]
		public Type compClass;

		public virtual void PostLoad()
		{
		}

		public virtual IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			if (compClass != null)
			{
				int i = 0;
				while (true)
				{
					if (i >= parentDef.comps.Count)
					{
						yield break;
					}
					if (parentDef.comps[i] != this && parentDef.comps[i].compClass == compClass)
					{
						break;
					}
					i++;
				}
				yield return "two comps with same compClass: " + compClass;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return "compClass is null";
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
