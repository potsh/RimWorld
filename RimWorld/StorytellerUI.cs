using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class StorytellerUI
	{
		private static Vector2 scrollPosition = default(Vector2);

		private static readonly Texture2D StorytellerHighlightTex = ContentFinder<Texture2D>.Get("UI/HeroArt/Storytellers/Highlight");

		public static void DrawStorytellerSelectionInterface(Rect rect, ref StorytellerDef chosenStoryteller, ref DifficultyDef difficulty, Listing_Standard infoListing)
		{
			GUI.BeginGroup(rect);
			if (chosenStoryteller != null && chosenStoryteller.listVisible)
			{
				float height = rect.height;
				Vector2 portraitSizeLarge = Storyteller.PortraitSizeLarge;
				float y = height - portraitSizeLarge.y - 1f;
				Vector2 portraitSizeLarge2 = Storyteller.PortraitSizeLarge;
				float x = portraitSizeLarge2.x;
				Vector2 portraitSizeLarge3 = Storyteller.PortraitSizeLarge;
				Rect position = new Rect(390f, y, x, portraitSizeLarge3.y);
				GUI.DrawTexture(position, chosenStoryteller.portraitLargeTex);
				Widgets.DrawLineHorizontal(0f, rect.height, rect.width);
			}
			Vector2 portraitSizeTiny = Storyteller.PortraitSizeTiny;
			Rect outRect = new Rect(0f, 0f, portraitSizeTiny.x + 16f, rect.height);
			Vector2 portraitSizeTiny2 = Storyteller.PortraitSizeTiny;
			float x2 = portraitSizeTiny2.x;
			float num = (float)DefDatabase<StorytellerDef>.AllDefs.Count();
			Vector2 portraitSizeTiny3 = Storyteller.PortraitSizeTiny;
			Rect viewRect = new Rect(0f, 0f, x2, num * (portraitSizeTiny3.y + 10f));
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			Vector2 portraitSizeTiny4 = Storyteller.PortraitSizeTiny;
			float x3 = portraitSizeTiny4.x;
			Vector2 portraitSizeTiny5 = Storyteller.PortraitSizeTiny;
			Rect rect2 = new Rect(0f, 0f, x3, portraitSizeTiny5.y);
			foreach (StorytellerDef item in from tel in DefDatabase<StorytellerDef>.AllDefs
			orderby tel.listOrder
			select tel)
			{
				if (item.listVisible)
				{
					if (Widgets.ButtonImage(rect2, item.portraitTinyTex))
					{
						TutorSystem.Notify_Event("ChooseStoryteller");
						chosenStoryteller = item;
					}
					if (chosenStoryteller == item)
					{
						GUI.DrawTexture(rect2, StorytellerHighlightTex);
					}
					rect2.y += rect2.height + 8f;
				}
			}
			Widgets.EndScrollView();
			Text.Font = GameFont.Small;
			Rect rect3 = new Rect(outRect.xMax + 8f, 0f, 300f, 999f);
			Widgets.Label(rect3, "HowStorytellersWork".Translate());
			if (chosenStoryteller != null && chosenStoryteller.listVisible)
			{
				Rect rect4 = new Rect(outRect.xMax + 8f, outRect.yMin + 160f, 290f, 0f);
				rect4.height = rect.height - rect4.y;
				Text.Font = GameFont.Medium;
				Rect rect5 = new Rect(rect4.x + 15f, rect4.y - 40f, 9999f, 40f);
				Widgets.Label(rect5, chosenStoryteller.label);
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
				infoListing.Begin(rect4);
				infoListing.Label(chosenStoryteller.description, 160f);
				infoListing.Gap(6f);
				foreach (DifficultyDef allDef in DefDatabase<DifficultyDef>.AllDefs)
				{
					if (!allDef.isExtreme || Prefs.ExtremeDifficultyUnlocked)
					{
						GUI.color = allDef.drawColor;
						string labelCap = allDef.LabelCap;
						bool active = difficulty == allDef;
						string description = allDef.description;
						if (infoListing.RadioButton(labelCap, active, 0f, description))
						{
							difficulty = allDef;
						}
						infoListing.Gap(3f);
					}
				}
				GUI.color = Color.white;
				if (Current.ProgramState == ProgramState.Entry)
				{
					infoListing.Gap(25f);
					bool flag = Find.GameInitData.permadeathChosen && Find.GameInitData.permadeath;
					bool flag2 = Find.GameInitData.permadeathChosen && !Find.GameInitData.permadeath;
					string description = "ReloadAnytimeMode".Translate();
					bool active = flag2;
					string labelCap = "ReloadAnytimeModeInfo".Translate();
					if (infoListing.RadioButton(description, active, 0f, labelCap))
					{
						Find.GameInitData.permadeathChosen = true;
						Find.GameInitData.permadeath = false;
					}
					infoListing.Gap(3f);
					labelCap = "CommitmentMode".TranslateWithBackup("PermadeathMode");
					active = flag;
					description = "PermadeathModeInfo".Translate();
					if (infoListing.RadioButton(labelCap, active, 0f, description))
					{
						Find.GameInitData.permadeathChosen = true;
						Find.GameInitData.permadeath = true;
					}
				}
				infoListing.End();
			}
			GUI.EndGroup();
		}
	}
}
