using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Page_SelectStoryteller : Page
	{
		private StorytellerDef storyteller;

		private DifficultyDef difficulty;

		private Listing_Standard selectedStorytellerInfoListing = new Listing_Standard();

		public override string PageTitle => "ChooseAIStoryteller".Translate();

		public override void PreOpen()
		{
			base.PreOpen();
			storyteller = (from d in DefDatabase<StorytellerDef>.AllDefs
			where d.listVisible
			orderby d.listOrder
			select d).First();
		}

		public override void DoWindowContents(Rect rect)
		{
			DrawPageTitle(rect);
			Rect mainRect = GetMainRect(rect);
			StorytellerUI.DrawStorytellerSelectionInterface(mainRect, ref storyteller, ref difficulty, selectedStorytellerInfoListing);
			string text = null;
			Action midAct = null;
			if (!Prefs.ExtremeDifficultyUnlocked)
			{
				text = "UnlockExtremeDifficulty".Translate();
				midAct = delegate
				{
					OpenDifficultyUnlockConfirmation();
				};
			}
			Rect rect2 = rect;
			string midLabel = text;
			DoBottomButtons(rect2, null, midLabel, midAct);
			float xMax = rect.xMax;
			Vector2 bottomButSize = Page.BottomButSize;
			float x = xMax - bottomButSize.x - 200f - 6f;
			float yMax = rect.yMax;
			Vector2 bottomButSize2 = Page.BottomButSize;
			float y = yMax - bottomButSize2.y;
			Vector2 bottomButSize3 = Page.BottomButSize;
			Rect rect3 = new Rect(x, y, 200f, bottomButSize3.y);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect3, "CanChangeStorytellerSettingsDuringPlay".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void OpenDifficultyUnlockConfirmation()
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnlockExtremeDifficulty".Translate(), delegate
			{
				Prefs.ExtremeDifficultyUnlocked = true;
				Prefs.Save();
			}, destructive: true));
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			if (difficulty == null)
			{
				if (!Prefs.DevMode)
				{
					Messages.Message("MustChooseDifficulty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				Messages.Message("Difficulty has been automatically selected (debug mode only)", MessageTypeDefOf.SilentInput, historical: false);
				difficulty = DifficultyDefOf.Rough;
			}
			if (!Find.GameInitData.permadeathChosen)
			{
				if (!Prefs.DevMode)
				{
					Messages.Message("MustChoosePermadeath".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				Messages.Message("Reload anytime mode has been automatically selected (debug mode only)", MessageTypeDefOf.SilentInput, historical: false);
				Find.GameInitData.permadeathChosen = true;
				Find.GameInitData.permadeath = false;
			}
			Current.Game.storyteller = new Storyteller(storyteller, difficulty);
			return true;
		}
	}
}
