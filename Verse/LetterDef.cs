using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class LetterDef : Def
	{
		public Type letterClass = typeof(StandardLetter);

		public Color color = Color.white;

		public Color flashColor = Color.white;

		public float flashInterval = 90f;

		public bool bounce;

		public SoundDef arriveSound;

		[NoTranslate]
		public string icon = "UI/Letters/LetterUnopened";

		public bool pauseIfPauseOnUrgentLetter;

		public bool forcedSlowdown;

		[Unsaved]
		private Texture2D iconTex;

		public Texture2D Icon
		{
			get
			{
				if (iconTex == null && !icon.NullOrEmpty())
				{
					iconTex = ContentFinder<Texture2D>.Get(icon);
				}
				return iconTex;
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (arriveSound == null)
			{
				arriveSound = SoundDefOf.LetterArrive;
			}
		}
	}
}
