using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtStage
	{
		[MustTranslate]
		public string label;

		[MustTranslate]
		public string labelSocial;

		[MustTranslate]
		public string description;

		public float baseMoodEffect;

		public float baseOpinionOffset;

		public bool visible = true;

		[Unsaved]
		[TranslationHandle(Priority = 100)]
		public string untranslatedLabel;

		[Unsaved]
		[TranslationHandle]
		public string untranslatedLabelSocial;

		public void PostLoad()
		{
			untranslatedLabel = label;
			untranslatedLabelSocial = labelSocial;
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (!labelSocial.NullOrEmpty() && labelSocial == label)
			{
				yield return "labelSocial is the same as label. labelSocial is unnecessary in this case";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (baseMoodEffect != 0f && description.NullOrEmpty())
			{
				yield return "affects mood but doesn't have a description";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
