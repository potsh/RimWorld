using RimWorld;
using Steamworks;

namespace Verse
{
	public static class ScreenshotTaker
	{
		private static bool takeScreenshot;

		public static void Update()
		{
			if (!LongEventHandler.ShouldWaitForEvent && (KeyBindingDefOf.TakeScreenshot.JustPressed || takeScreenshot))
			{
				TakeShot();
				takeScreenshot = false;
			}
		}

		public static void QueueSilentScreenshot()
		{
			takeScreenshot = true;
		}

		private static void TakeShot()
		{
			SteamScreenshots.TriggerScreenshot();
		}
	}
}
