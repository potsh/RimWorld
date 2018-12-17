using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_FactionDuringLanding : Window
	{
		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;

		public override Vector2 InitialSize => new Vector2(1010f, 684f);

		public Dialog_FactionDuringLanding()
		{
			doCloseButton = true;
			forcePause = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			float x = inRect.x;
			float y = inRect.y;
			float width = inRect.width;
			float height = inRect.height;
			Vector2 closeButSize = CloseButSize;
			FactionUIUtility.DoWindowContents(new Rect(x, y, width, height - closeButSize.y), ref scrollPosition, ref scrollViewHeight);
		}
	}
}
