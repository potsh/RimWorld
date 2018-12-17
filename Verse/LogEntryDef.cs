using UnityEngine;

namespace Verse
{
	public class LogEntryDef : Def
	{
		[NoTranslate]
		public string iconMiss;

		[NoTranslate]
		public string iconDamaged;

		[NoTranslate]
		public string iconDamagedFromInstigator;

		[Unsaved]
		public Texture2D iconMissTex;

		[Unsaved]
		public Texture2D iconDamagedTex;

		[Unsaved]
		public Texture2D iconDamagedFromInstigatorTex;

		public override void PostLoad()
		{
			base.PostLoad();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (!iconMiss.NullOrEmpty())
				{
					iconMissTex = ContentFinder<Texture2D>.Get(iconMiss);
				}
				if (!iconDamaged.NullOrEmpty())
				{
					iconDamagedTex = ContentFinder<Texture2D>.Get(iconDamaged);
				}
				if (!iconDamagedFromInstigator.NullOrEmpty())
				{
					iconDamagedFromInstigatorTex = ContentFinder<Texture2D>.Get(iconDamagedFromInstigator);
				}
			});
		}
	}
}
