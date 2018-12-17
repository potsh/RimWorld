using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainButtonDef : Def
	{
		public Type workerClass = typeof(MainButtonWorker_ToggleTab);

		public Type tabWindowClass;

		public bool buttonVisible = true;

		public int order;

		public KeyCode defaultHotKey;

		public bool canBeTutorDenied = true;

		public bool validWithoutMap;

		[Unsaved]
		public KeyBindingDef hotKey;

		[Unsaved]
		public string cachedTutorTag;

		[Unsaved]
		public string cachedHighlightTagClosed;

		[Unsaved]
		private MainButtonWorker workerInt;

		[Unsaved]
		private MainTabWindow tabWindowInt;

		[Unsaved]
		private string cachedShortenedLabelCap;

		[Unsaved]
		private float cachedLabelCapWidth = -1f;

		[Unsaved]
		private float cachedShortenedLabelCapWidth = -1f;

		public const int ButtonHeight = 35;

		public MainButtonWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (MainButtonWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public MainTabWindow TabWindow
		{
			get
			{
				if (tabWindowInt == null && tabWindowClass != null)
				{
					tabWindowInt = (MainTabWindow)Activator.CreateInstance(tabWindowClass);
					tabWindowInt.def = this;
				}
				return tabWindowInt;
			}
		}

		public string ShortenedLabelCap
		{
			get
			{
				if (cachedShortenedLabelCap == null)
				{
					cachedShortenedLabelCap = base.LabelCap.Shorten();
				}
				return cachedShortenedLabelCap;
			}
		}

		public float LabelCapWidth
		{
			get
			{
				if (cachedLabelCapWidth < 0f)
				{
					GameFont font = Text.Font;
					Text.Font = GameFont.Small;
					Vector2 vector = Text.CalcSize(base.LabelCap);
					cachedLabelCapWidth = vector.x;
					Text.Font = font;
				}
				return cachedLabelCapWidth;
			}
		}

		public float ShortenedLabelCapWidth
		{
			get
			{
				if (cachedShortenedLabelCapWidth < 0f)
				{
					GameFont font = Text.Font;
					Text.Font = GameFont.Small;
					Vector2 vector = Text.CalcSize(ShortenedLabelCap);
					cachedShortenedLabelCapWidth = vector.x;
					Text.Font = font;
				}
				return cachedShortenedLabelCapWidth;
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			cachedHighlightTagClosed = "MainTab-" + defName + "-Closed";
		}

		public void Notify_SwitchedMap()
		{
			if (tabWindowInt != null)
			{
				Find.WindowStack.TryRemove(tabWindowInt);
				tabWindowInt = null;
			}
		}

		public void Notify_ClearingAllMapsMemory()
		{
			tabWindowInt = null;
		}
	}
}
