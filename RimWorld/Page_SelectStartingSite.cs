using RimWorld.Planet;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Page_SelectStartingSite : Page
	{
		private const float GapBetweenBottomButtons = 10f;

		private const float UseTwoRowsIfScreenWidthBelow = 1340f;

		public override string PageTitle => "SelectStartingSite".TranslateWithBackup("SelectLandingSite");

		public override Vector2 InitialSize => Vector2.zero;

		protected override float Margin => 0f;

		public Page_SelectStartingSite()
		{
			absorbInputAroundWindow = false;
			shadowAlpha = 0f;
			preventCameraMotion = false;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.Planet;
			Find.WorldInterface.Reset();
			((MainButtonWorker_ToggleWorld)MainButtonDefOf.World.Worker).resetViewNextTime = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			Find.GameInitData.ChooseRandomStartingTile();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.WorldCameraMovement, OpportunityType.Important);
			TutorSystem.Notify_Event("PageStart-SelectStartingSite");
		}

		public override void PostClose()
		{
			base.PostClose();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}

		public override void DoWindowContents(Rect rect)
		{
			if (Find.WorldInterface.SelectedTile >= 0)
			{
				Find.GameInitData.startingTile = Find.WorldInterface.SelectedTile;
			}
			else if (Find.WorldSelector.FirstSelectedObject != null)
			{
				Find.GameInitData.startingTile = Find.WorldSelector.FirstSelectedObject.Tile;
			}
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			Text.Anchor = TextAnchor.UpperCenter;
			DrawPageTitle(new Rect(0f, 5f, (float)UI.screenWidth, 300f));
			Text.Anchor = TextAnchor.UpperLeft;
			DoCustomBottomButtons();
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			int selectedTile = Find.WorldInterface.SelectedTile;
			if (selectedTile < 0)
			{
				Messages.Message("MustSelectStartingSite".TranslateWithBackup("MustSelectLandingSite"), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (!TileFinder.IsValidTileForNewSettlement(selectedTile, stringBuilder))
			{
				Messages.Message(stringBuilder.ToString(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			Tile tile = Find.WorldGrid[selectedTile];
			if (!TutorSystem.AllowAction("ChooseBiome-" + tile.biome.defName + "-" + tile.hilliness.ToString()))
			{
				return false;
			}
			return true;
		}

		protected override void DoNext()
		{
			int selTile = Find.WorldInterface.SelectedTile;
			SettlementProximityGoodwillUtility.CheckConfirmSettle(selTile, delegate
			{
				Find.GameInitData.startingTile = selTile;
				base.DoNext();
			});
		}

		private void DoCustomBottomButtons()
		{
			int num = (!TutorSystem.TutorialMode) ? 5 : 4;
			int num2 = (num < 4 || !((float)UI.screenWidth < 1340f)) ? 1 : 2;
			int num3 = Mathf.CeilToInt((float)num / (float)num2);
			Vector2 bottomButSize = Page.BottomButSize;
			float num4 = bottomButSize.x * (float)num3 + 10f * (float)(num3 + 1);
			float num5 = (float)num2;
			Vector2 bottomButSize2 = Page.BottomButSize;
			float num6 = num5 * bottomButSize2.y + 10f * (float)(num2 + 1);
			Rect rect = new Rect(((float)UI.screenWidth - num4) / 2f, (float)UI.screenHeight - num6 - 4f, num4, num6);
			WorldInspectPane worldInspectPane = Find.WindowStack.WindowOfType<WorldInspectPane>();
			if (worldInspectPane != null && rect.x < InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f)
			{
				rect.x = InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f;
			}
			Widgets.DrawWindowBackground(rect);
			float num7 = rect.xMin + 10f;
			float num8 = rect.yMin + 10f;
			Text.Font = GameFont.Small;
			float x = num7;
			float y = num8;
			Vector2 bottomButSize3 = Page.BottomButSize;
			float x2 = bottomButSize3.x;
			Vector2 bottomButSize4 = Page.BottomButSize;
			if (Widgets.ButtonText(new Rect(x, y, x2, bottomButSize4.y), "Back".Translate()) && CanDoBack())
			{
				DoBack();
			}
			float num9 = num7;
			Vector2 bottomButSize5 = Page.BottomButSize;
			num7 = num9 + (bottomButSize5.x + 10f);
			if (!TutorSystem.TutorialMode)
			{
				float x3 = num7;
				float y2 = num8;
				Vector2 bottomButSize6 = Page.BottomButSize;
				float x4 = bottomButSize6.x;
				Vector2 bottomButSize7 = Page.BottomButSize;
				if (Widgets.ButtonText(new Rect(x3, y2, x4, bottomButSize7.y), "Advanced".Translate()))
				{
					Find.WindowStack.Add(new Dialog_AdvancedGameConfig(Find.WorldInterface.SelectedTile));
				}
				float num10 = num7;
				Vector2 bottomButSize8 = Page.BottomButSize;
				num7 = num10 + (bottomButSize8.x + 10f);
			}
			float x5 = num7;
			float y3 = num8;
			Vector2 bottomButSize9 = Page.BottomButSize;
			float x6 = bottomButSize9.x;
			Vector2 bottomButSize10 = Page.BottomButSize;
			if (Widgets.ButtonText(new Rect(x5, y3, x6, bottomButSize10.y), "SelectRandomSite".Translate()))
			{
				SoundDefOf.Click.PlayOneShotOnCamera();
				Find.WorldInterface.SelectedTile = TileFinder.RandomStartingTile();
				Find.WorldCameraDriver.JumpTo(Find.WorldGrid.GetTileCenter(Find.WorldInterface.SelectedTile));
			}
			float num11 = num7;
			Vector2 bottomButSize11 = Page.BottomButSize;
			num7 = num11 + (bottomButSize11.x + 10f);
			if (num2 == 2)
			{
				num7 = rect.xMin + 10f;
				float num12 = num8;
				Vector2 bottomButSize12 = Page.BottomButSize;
				num8 = num12 + (bottomButSize12.y + 10f);
			}
			float x7 = num7;
			float y4 = num8;
			Vector2 bottomButSize13 = Page.BottomButSize;
			float x8 = bottomButSize13.x;
			Vector2 bottomButSize14 = Page.BottomButSize;
			if (Widgets.ButtonText(new Rect(x7, y4, x8, bottomButSize14.y), "WorldFactionsTab".Translate()))
			{
				Find.WindowStack.Add(new Dialog_FactionDuringLanding());
			}
			float num13 = num7;
			Vector2 bottomButSize15 = Page.BottomButSize;
			num7 = num13 + (bottomButSize15.x + 10f);
			float x9 = num7;
			float y5 = num8;
			Vector2 bottomButSize16 = Page.BottomButSize;
			float x10 = bottomButSize16.x;
			Vector2 bottomButSize17 = Page.BottomButSize;
			if (Widgets.ButtonText(new Rect(x9, y5, x10, bottomButSize17.y), "Next".Translate()) && CanDoNext())
			{
				DoNext();
			}
			float num14 = num7;
			Vector2 bottomButSize18 = Page.BottomButSize;
			num7 = num14 + (bottomButSize18.x + 10f);
			GenUI.AbsorbClicksInRect(rect);
		}
	}
}
