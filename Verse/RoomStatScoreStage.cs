namespace Verse
{
	public class RoomStatScoreStage
	{
		public float minScore = -3.40282347E+38f;

		public string label;

		[Unsaved]
		[TranslationHandle]
		public string untranslatedLabel;

		public void PostLoad()
		{
			untranslatedLabel = label;
		}
	}
}
