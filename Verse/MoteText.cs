using UnityEngine;

namespace Verse
{
	public class MoteText : MoteThrown
	{
		public string text;

		public Color textColor = Color.white;

		public float overrideTimeBeforeStartFadeout = -1f;

		protected float TimeBeforeStartFadeout => (!(overrideTimeBeforeStartFadeout >= 0f)) ? def.mote.solidTime : overrideTimeBeforeStartFadeout;

		protected override bool EndOfLife => base.AgeSecs >= TimeBeforeStartFadeout + def.mote.fadeOutTime;

		public override void Draw()
		{
		}

		public override void DrawGUIOverlay()
		{
			float a = 1f - (base.AgeSecs - TimeBeforeStartFadeout) / def.mote.fadeOutTime;
			GenMapUI.DrawText(textColor: new Color(textColor.r, textColor.g, textColor.b, a), worldPos: new Vector2(exactPosition.x, exactPosition.z), text: text);
		}
	}
}
