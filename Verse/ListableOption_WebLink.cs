using System;
using UnityEngine;

namespace Verse
{
	public class ListableOption_WebLink : ListableOption
	{
		public Texture2D image;

		public string url;

		private static readonly Vector2 Imagesize = new Vector2(24f, 18f);

		public ListableOption_WebLink(string label, Texture2D image)
			: base(label, null)
		{
			minHeight = 24f;
			this.image = image;
		}

		public ListableOption_WebLink(string label, string url, Texture2D image)
			: this(label, image)
		{
			this.url = url;
		}

		public ListableOption_WebLink(string label, Action action, Texture2D image)
			: this(label, image)
		{
			base.action = action;
		}

		public override float DrawOption(Vector2 pos, float width)
		{
			Vector2 imagesize = Imagesize;
			float num = width - imagesize.x - 3f;
			float num2 = Text.CalcHeight(label, num);
			float num3 = Mathf.Max(minHeight, num2);
			Rect rect = new Rect(pos.x, pos.y, width, num3);
			GUI.color = Color.white;
			if (image != null)
			{
				float x = pos.x;
				float num4 = pos.y + num3 / 2f;
				Vector2 imagesize2 = Imagesize;
				float y = num4 - imagesize2.y / 2f;
				Vector2 imagesize3 = Imagesize;
				float x2 = imagesize3.x;
				Vector2 imagesize4 = Imagesize;
				Rect position = new Rect(x, y, x2, imagesize4.y);
				if (Mouse.IsOver(rect))
				{
					GUI.color = Widgets.MouseoverOptionColor;
				}
				GUI.DrawTexture(position, image);
			}
			Rect rect2 = new Rect(rect.xMax - num, pos.y, num, num2);
			Widgets.Label(rect2, label);
			GUI.color = Color.white;
			if (Widgets.ButtonInvisible(rect, doMouseoverSound: true))
			{
				if (action != null)
				{
					action();
				}
				else
				{
					Application.OpenURL(url);
				}
			}
			return num3;
		}
	}
}
