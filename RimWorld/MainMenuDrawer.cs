using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class MainMenuDrawer
	{
		private static bool anyMapFiles;

		private static Vector2 translationInfoScrollbarPos;

		private const float PlayRectWidth = 170f;

		private const float WebRectWidth = 145f;

		private const float RightEdgeMargin = 50f;

		private static readonly Vector2 PaneSize = new Vector2(450f, 450f);

		private static readonly Vector2 TitleSize = new Vector2(1032f, 146f);

		private static readonly Texture2D TexTitle = ContentFinder<Texture2D>.Get("UI/HeroArt/GameTitle");

		private const float TitleShift = 50f;

		private static readonly Vector2 LudeonLogoSize = new Vector2(200f, 58f);

		private static readonly Texture2D TexLudeonLogo = ContentFinder<Texture2D>.Get("UI/HeroArt/LudeonLogoSmall");

		private static readonly string TranslationsContributeURL = "https://rimworldgame.com/helptranslate";

		public static void Init()
		{
			PlayerKnowledgeDatabase.Save();
			ShipCountdown.CancelCountdown();
			anyMapFiles = GenFilePaths.AllSavedGameFiles.Any();
		}

		public static void MainMenuOnGUI()
		{
			VersionControl.DrawInfoInCorner();
			float num = (float)(UI.screenWidth / 2);
			Vector2 paneSize = PaneSize;
			float x = num - paneSize.x / 2f;
			float num2 = (float)(UI.screenHeight / 2);
			Vector2 paneSize2 = PaneSize;
			float y = num2 - paneSize2.y / 2f + 50f;
			Vector2 paneSize3 = PaneSize;
			float x2 = paneSize3.x;
			Vector2 paneSize4 = PaneSize;
			Rect rect = new Rect(x, y, x2, paneSize4.y);
			rect.x = (float)UI.screenWidth - rect.width - 30f;
			Rect rect2 = new Rect(0f, rect.y - 30f, (float)UI.screenWidth - 85f, 30f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.UpperRight;
			string text = "MainPageCredit".Translate();
			if (UI.screenWidth < 990)
			{
				Rect position = rect2;
				float xMax = position.xMax;
				Vector2 vector = Text.CalcSize(text);
				position.xMin = xMax - vector.x;
				position.xMin -= 4f;
				position.xMax += 4f;
				GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
				GUI.DrawTexture(position, BaseContent.WhiteTex);
				GUI.color = Color.white;
			}
			Widgets.Label(rect2, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			Vector2 a = TitleSize;
			if (a.x > (float)UI.screenWidth)
			{
				a *= (float)UI.screenWidth / a.x;
			}
			a *= 0.7f;
			Rect position2 = new Rect((float)UI.screenWidth - a.x - 50f, rect2.y - a.y, a.x, a.y);
			GUI.DrawTexture(position2, TexTitle, ScaleMode.StretchToFill, alphaBlend: true);
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			float num3 = (float)(UI.screenWidth - 8);
			Vector2 ludeonLogoSize = LudeonLogoSize;
			float x3 = num3 - ludeonLogoSize.x;
			Vector2 ludeonLogoSize2 = LudeonLogoSize;
			float x4 = ludeonLogoSize2.x;
			Vector2 ludeonLogoSize3 = LudeonLogoSize;
			Rect position3 = new Rect(x3, 8f, x4, ludeonLogoSize3.y);
			GUI.DrawTexture(position3, TexLudeonLogo, ScaleMode.StretchToFill, alphaBlend: true);
			GUI.color = Color.white;
			rect.yMin += 17f;
			DoMainMenuControls(rect, anyMapFiles);
			if (Debug.isDebugBuild)
			{
				Rect outRect = new Rect(rect.x - 310f, rect.y, 295f, 400f);
				DoDevBuildWarningRect(outRect);
			}
			Rect outRect2 = new Rect(8f, (float)(UI.screenHeight - 8 - 400), 240f, 400f);
			DoTranslationInfoRect(outRect2);
		}

		public static void DoMainMenuControls(Rect rect, bool anyMapFiles)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, 170f, rect.height);
			Rect rect3 = new Rect(rect2.xMax + 17f, 0f, 145f, rect.height);
			Text.Font = GameFont.Small;
			List<ListableOption> list = new List<ListableOption>();
			if (Current.ProgramState == ProgramState.Entry)
			{
				string label = "Tutorial".CanTranslate() ? "Tutorial".Translate() : "LearnToPlay".Translate();
				list.Add(new ListableOption(label, delegate
				{
					InitLearnToPlay();
				}));
				list.Add(new ListableOption("NewColony".Translate(), delegate
				{
					Find.WindowStack.Add(new Page_SelectScenario());
				}));
			}
			if (Current.ProgramState == ProgramState.Playing && !Current.Game.Info.permadeathMode)
			{
				list.Add(new ListableOption("Save".Translate(), delegate
				{
					CloseMainTab();
					Find.WindowStack.Add(new Dialog_SaveFileList_Save());
				}));
			}
			ListableOption item;
			if (anyMapFiles && (Current.ProgramState != ProgramState.Playing || !Current.Game.Info.permadeathMode))
			{
				item = new ListableOption("LoadGame".Translate(), delegate
				{
					CloseMainTab();
					Find.WindowStack.Add(new Dialog_SaveFileList_Load());
				});
				list.Add(item);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				list.Add(new ListableOption("ReviewScenario".Translate(), delegate
				{
					WindowStack windowStack = Find.WindowStack;
					string fullInformationText = Find.Scenario.GetFullInformationText();
					string name = Find.Scenario.name;
					windowStack.Add(new Dialog_MessageBox(fullInformationText, null, null, null, null, name));
				}));
			}
			item = new ListableOption("Options".Translate(), delegate
			{
				CloseMainTab();
				Find.WindowStack.Add(new Dialog_Options());
			}, "MenuButton-Options");
			list.Add(item);
			if (Current.ProgramState == ProgramState.Entry)
			{
				item = new ListableOption("Mods".Translate(), delegate
				{
					Find.WindowStack.Add(new Page_ModsConfig());
				});
				list.Add(item);
				if (Prefs.DevMode && LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage && LanguageDatabase.activeLanguage.anyError)
				{
					item = new ListableOption("SaveTranslationReport".Translate(), delegate
					{
						LanguageReportGenerator.SaveTranslationReport();
					});
					list.Add(item);
				}
				item = new ListableOption("Credits".Translate(), delegate
				{
					Find.WindowStack.Add(new Screen_Credits());
				});
				list.Add(item);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (Current.Game.Info.permadeathMode)
				{
					item = new ListableOption("SaveAndQuitToMainMenu".Translate(), delegate
					{
						LongEventHandler.QueueLongEvent(delegate
						{
							GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
							MemoryUtility.ClearAllMapsAndWorld();
						}, "Entry", "SavingLongEvent", doAsynchronously: false, null);
					});
					list.Add(item);
					item = new ListableOption("SaveAndQuitToOS".Translate(), delegate
					{
						LongEventHandler.QueueLongEvent(delegate
						{
							GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
							LongEventHandler.ExecuteWhenFinished(delegate
							{
								Root.Shutdown();
							});
						}, "SavingLongEvent", doAsynchronously: false, null);
					});
					list.Add(item);
				}
				else
				{
					Action action = delegate
					{
						if (GameDataSaveLoader.CurrentGameStateIsValuable)
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), delegate
							{
								GenScene.GoToMainMenu();
							}, destructive: true));
						}
						else
						{
							GenScene.GoToMainMenu();
						}
					};
					item = new ListableOption("QuitToMainMenu".Translate(), action);
					list.Add(item);
					Action action2 = delegate
					{
						if (GameDataSaveLoader.CurrentGameStateIsValuable)
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmQuit".Translate(), delegate
							{
								Root.Shutdown();
							}, destructive: true));
						}
						else
						{
							Root.Shutdown();
						}
					};
					item = new ListableOption("QuitToOS".Translate(), action2);
					list.Add(item);
				}
			}
			else
			{
				item = new ListableOption("QuitToOS".Translate(), delegate
				{
					Root.Shutdown();
				});
				list.Add(item);
			}
			OptionListingUtility.DrawOptionListing(rect2, list);
			Text.Font = GameFont.Small;
			List<ListableOption> list2 = new List<ListableOption>();
			ListableOption item2 = new ListableOption_WebLink("FictionPrimer".Translate(), "https://rimworldgame.com/backstory", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("LudeonBlog".Translate(), "https://ludeon.com/blog", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("Forums".Translate(), "https://ludeon.com/forums", TexButton.IconForums);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("OfficialWiki".Translate(), "https://rimworldwiki.com", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("TynansTwitter".Translate(), "https://twitter.com/TynanSylvester", TexButton.IconTwitter);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("TynansDesignBook".Translate(), "https://tynansylvester.com/book", TexButton.IconBook);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("HelpTranslate".Translate(), TranslationsContributeURL, TexButton.IconForums);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("BuySoundtrack".Translate(), "http://www.lasgameaudio.co.uk/#!store/t04fw", TexButton.IconSoundtrack);
			list2.Add(item2);
			float num = OptionListingUtility.DrawOptionListing(rect3, list2);
			GUI.BeginGroup(rect3);
			if (Current.ProgramState == ProgramState.Entry && Widgets.ButtonImage(new Rect(0f, num + 10f, 64f, 32f), LanguageDatabase.activeLanguage.icon))
			{
				List<FloatMenuOption> list3 = new List<FloatMenuOption>();
				foreach (LoadedLanguage allLoadedLanguage in LanguageDatabase.AllLoadedLanguages)
				{
					LoadedLanguage localLang = allLoadedLanguage;
					list3.Add(new FloatMenuOption(localLang.FriendlyNameNative, delegate
					{
						LanguageDatabase.SelectLanguage(localLang);
						Prefs.Save();
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list3));
			}
			GUI.EndGroup();
			GUI.EndGroup();
		}

		public static void DoTranslationInfoRect(Rect outRect)
		{
			if (LanguageDatabase.activeLanguage != LanguageDatabase.defaultLanguage)
			{
				Widgets.DrawWindowBackground(outRect);
				Rect rect = outRect.ContractedBy(8f);
				GUI.BeginGroup(rect);
				rect = rect.AtZero();
				Rect rect2 = new Rect(5f, rect.height - 25f, rect.width - 10f, 25f);
				rect.height -= 29f;
				Rect rect3 = new Rect(5f, rect.height - 25f, rect.width - 10f, 25f);
				rect.height -= 29f;
				Rect rect4 = new Rect(5f, rect.height - 25f, rect.width - 10f, 25f);
				rect.height -= 29f;
				string text = string.Empty;
				foreach (CreditsEntry credit in LanguageDatabase.activeLanguage.info.credits)
				{
					CreditRecord_Role creditRecord_Role = credit as CreditRecord_Role;
					if (creditRecord_Role != null)
					{
						text = text + creditRecord_Role.creditee + "\n";
					}
				}
				text = text.TrimEndNewlines();
				string label = "TranslationThanks".Translate(text) + "\n\n" + "TranslationHowToContribute".Translate();
				Widgets.LabelScrollable(rect, label, ref translationInfoScrollbarPos, dontConsumeScrollEventsIfNoScrollbar: false, takeScrollbarSpaceEvenIfNoScrollbar: false);
				if (Widgets.ButtonText(rect4, "LearnMore".Translate()))
				{
					Application.OpenURL(TranslationsContributeURL);
				}
				if (Widgets.ButtonText(rect3, "SaveTranslationReport".Translate()))
				{
					LanguageReportGenerator.SaveTranslationReport();
				}
				if (Widgets.ButtonText(rect2, "CleanupTranslationFiles".Translate()))
				{
					TranslationFilesCleaner.CleanupTranslationFiles();
				}
				GUI.EndGroup();
			}
		}

		private static void DoDevBuildWarningRect(Rect outRect)
		{
			Widgets.DrawWindowBackground(outRect);
			Rect rect = outRect.ContractedBy(17f);
			Widgets.Label(rect, "DevBuildWarning".Translate());
		}

		private static void InitLearnToPlay()
		{
			Current.Game = new Game();
			Current.Game.InitData = new GameInitData();
			Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
			Find.Scenario.PreConfigure();
			Current.Game.storyteller = new Storyteller(StorytellerDefOf.Tutor, DifficultyDefOf.Easy);
			Page firstConfigPage = Current.Game.Scenario.GetFirstConfigPage();
			Page next = firstConfigPage.next;
			next.prev = null;
			Find.WindowStack.Add(next);
		}

		private static void CloseMainTab()
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
			}
		}
	}
}
