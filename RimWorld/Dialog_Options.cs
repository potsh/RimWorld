using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_Options : Window
	{
		private const float SubOptionTabWidth = 40f;

		private static readonly float[] UIScales = new float[9]
		{
			1f,
			1.25f,
			1.5f,
			1.75f,
			2f,
			2.5f,
			3f,
			3.5f,
			4f
		};

		[CompilerGenerated]
		private static Predicate<string> _003C_003Ef__mg_0024cache0;

		public override Vector2 InitialSize => new Vector2(900f, 700f);

		public Dialog_Options()
		{
			doCloseButton = true;
			doCloseX = true;
			forcePause = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = inRect.AtZero();
			rect.yMax -= 35f;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = (rect.width - 34f) / 3f;
			listing_Standard.Begin(rect);
			Text.Font = GameFont.Medium;
			listing_Standard.Label("Audiovisuals".Translate());
			Text.Font = GameFont.Small;
			listing_Standard.Gap();
			listing_Standard.Gap();
			listing_Standard.Label("GameVolume".Translate());
			Prefs.VolumeGame = listing_Standard.Slider(Prefs.VolumeGame, 0f, 1f);
			listing_Standard.Label("MusicVolume".Translate());
			Prefs.VolumeMusic = listing_Standard.Slider(Prefs.VolumeMusic, 0f, 1f);
			listing_Standard.Label("AmbientVolume".Translate());
			Prefs.VolumeAmbient = listing_Standard.Slider(Prefs.VolumeAmbient, 0f, 1f);
			if (listing_Standard.ButtonTextLabeled("Resolution".Translate(), ResToString(Screen.width, Screen.height)))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (Resolution item in from x in UnityGUIBugsFixer.ScreenResolutionsWithoutDuplicates
				where x.width >= 1024 && x.height >= 768
				orderby x.width, x.height
				select x)
				{
					list.Add(new FloatMenuOption(ResToString(item.width, item.height), delegate
					{
						if (!ResolutionUtility.UIScaleSafeWithResolution(Prefs.UIScale, item.width, item.height))
						{
							Messages.Message("MessageScreenResTooSmallForUIScale".Translate(), MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							ResolutionUtility.SafeSetResolution(item);
						}
					}));
				}
				if (!list.Any())
				{
					list.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			if (listing_Standard.ButtonTextLabeled("UIScale".Translate(), Prefs.UIScale.ToString() + "x"))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				for (int i = 0; i < UIScales.Length; i++)
				{
					float scale = UIScales[i];
					list2.Add(new FloatMenuOption(UIScales[i].ToString() + "x", delegate
					{
						if (scale != 1f && !ResolutionUtility.UIScaleSafeWithResolution(scale, Screen.width, Screen.height))
						{
							Messages.Message("MessageScreenResTooSmallForUIScale".Translate(), MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							ResolutionUtility.SafeSetUIScale(scale);
						}
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			bool checkOn = Prefs.CustomCursorEnabled;
			listing_Standard.CheckboxLabeled("CustomCursor".Translate(), ref checkOn);
			Prefs.CustomCursorEnabled = checkOn;
			bool checkOn2 = Screen.fullScreen;
			bool flag = checkOn2;
			listing_Standard.CheckboxLabeled("Fullscreen".Translate(), ref checkOn2);
			if (checkOn2 != flag)
			{
				ResolutionUtility.SafeSetFullscreen(checkOn2);
			}
			listing_Standard.Gap();
			bool checkOn3 = Prefs.HatsOnlyOnMap;
			listing_Standard.CheckboxLabeled("HatsShownOnlyOnMap".Translate(), ref checkOn3);
			if (checkOn3 != Prefs.HatsOnlyOnMap)
			{
				PortraitsCache.Clear();
			}
			Prefs.HatsOnlyOnMap = checkOn3;
			bool checkOn4 = Prefs.PlantWindSway;
			listing_Standard.CheckboxLabeled("PlantWindSway".Translate(), ref checkOn4);
			Prefs.PlantWindSway = checkOn4;
			bool checkOn5 = Prefs.ShowRealtimeClock;
			listing_Standard.CheckboxLabeled("ShowRealtimeClock".Translate(), ref checkOn5);
			Prefs.ShowRealtimeClock = checkOn5;
			if (listing_Standard.ButtonTextLabeled("ShowAnimalNames".Translate(), Prefs.AnimalNameMode.ToStringHuman()))
			{
				List<FloatMenuOption> list3 = new List<FloatMenuOption>();
				IEnumerator enumerator2 = Enum.GetValues(typeof(AnimalNameDisplayMode)).GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						AnimalNameDisplayMode animalNameDisplayMode = (AnimalNameDisplayMode)enumerator2.Current;
						AnimalNameDisplayMode localMode = animalNameDisplayMode;
						list3.Add(new FloatMenuOption(localMode.ToStringHuman(), delegate
						{
							Prefs.AnimalNameMode = localMode;
						}));
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator2 as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				Find.WindowStack.Add(new FloatMenu(list3));
			}
			listing_Standard.NewColumn();
			Text.Font = GameFont.Medium;
			listing_Standard.Label("Gameplay".Translate());
			Text.Font = GameFont.Small;
			listing_Standard.Gap();
			listing_Standard.Gap();
			if (listing_Standard.ButtonText("KeyboardConfig".Translate()))
			{
				Find.WindowStack.Add(new Dialog_KeyBindings());
			}
			if (listing_Standard.ButtonText("ChooseLanguage".Translate()))
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					Messages.Message("ChangeLanguageFromMainMenu".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					List<FloatMenuOption> list4 = new List<FloatMenuOption>();
					foreach (LoadedLanguage allLoadedLanguage in LanguageDatabase.AllLoadedLanguages)
					{
						LoadedLanguage localLang = allLoadedLanguage;
						list4.Add(new FloatMenuOption(localLang.FriendlyNameNative, delegate
						{
							LanguageDatabase.SelectLanguage(localLang);
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list4));
				}
			}
			if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
			{
				if (listing_Standard.ButtonText("OpenSaveGameDataFolder".Translate()))
				{
					Application.OpenURL(GenFilePaths.SaveDataFolderPath);
				}
			}
			else if (listing_Standard.ButtonText("ShowSaveGameDataLocation".Translate()))
			{
				Find.WindowStack.Add(new Dialog_MessageBox(Path.GetFullPath(GenFilePaths.SaveDataFolderPath)));
			}
			if (listing_Standard.ButtonText("ResetAdaptiveTutor".Translate()))
			{
				Messages.Message("AdaptiveTutorIsReset".Translate(), MessageTypeDefOf.TaskCompletion, historical: false);
				PlayerKnowledgeDatabase.ResetPersistent();
			}
			bool checkOn6 = Prefs.AdaptiveTrainingEnabled;
			listing_Standard.CheckboxLabeled("LearningHelper".Translate(), ref checkOn6);
			Prefs.AdaptiveTrainingEnabled = checkOn6;
			bool checkOn7 = Prefs.RunInBackground;
			listing_Standard.CheckboxLabeled("RunInBackground".Translate(), ref checkOn7);
			Prefs.RunInBackground = checkOn7;
			bool checkOn8 = Prefs.EdgeScreenScroll;
			listing_Standard.CheckboxLabeled("EdgeScreenScroll".Translate(), ref checkOn8);
			Prefs.EdgeScreenScroll = checkOn8;
			bool checkOn9 = Prefs.PauseOnLoad;
			listing_Standard.CheckboxLabeled("PauseOnLoad".Translate(), ref checkOn9);
			Prefs.PauseOnLoad = checkOn9;
			bool checkOn10 = Prefs.PauseOnUrgentLetter;
			listing_Standard.CheckboxLabeled("PauseOnUrgentLetter".Translate(), ref checkOn10);
			Prefs.PauseOnUrgentLetter = checkOn10;
			int maxNumberOfPlayerSettlements = Prefs.MaxNumberOfPlayerSettlements;
			listing_Standard.Label("MaxNumberOfPlayerSettlements".Translate(maxNumberOfPlayerSettlements));
			int num2 = Prefs.MaxNumberOfPlayerSettlements = Mathf.RoundToInt(listing_Standard.Slider((float)maxNumberOfPlayerSettlements, 1f, 5f));
			if (maxNumberOfPlayerSettlements != num2 && num2 > 1)
			{
				TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.MaxNumberOfPlayerSettlements);
			}
			if (listing_Standard.ButtonTextLabeled("TemperatureMode".Translate(), Prefs.TemperatureMode.ToStringHuman()))
			{
				List<FloatMenuOption> list5 = new List<FloatMenuOption>();
				IEnumerator enumerator4 = Enum.GetValues(typeof(TemperatureDisplayMode)).GetEnumerator();
				try
				{
					while (enumerator4.MoveNext())
					{
						TemperatureDisplayMode temperatureDisplayMode = (TemperatureDisplayMode)enumerator4.Current;
						TemperatureDisplayMode localTmode = temperatureDisplayMode;
						list5.Add(new FloatMenuOption(localTmode.ToString(), delegate
						{
							Prefs.TemperatureMode = localTmode;
						}));
					}
				}
				finally
				{
					IDisposable disposable2;
					if ((disposable2 = (enumerator4 as IDisposable)) != null)
					{
						disposable2.Dispose();
					}
				}
				Find.WindowStack.Add(new FloatMenu(list5));
			}
			float autosaveIntervalDays = Prefs.AutosaveIntervalDays;
			string text = "Days".Translate();
			string text2 = "Day".Translate().ToLower();
			if (listing_Standard.ButtonTextLabeled("AutosaveInterval".Translate(), autosaveIntervalDays + " " + ((autosaveIntervalDays != 1f) ? text : text2)))
			{
				List<FloatMenuOption> list6 = new List<FloatMenuOption>();
				if (Prefs.DevMode)
				{
					list6.Add(new FloatMenuOption("0.125 " + text + "(debug)", delegate
					{
						Prefs.AutosaveIntervalDays = 0.125f;
					}));
					list6.Add(new FloatMenuOption("0.25 " + text + "(debug)", delegate
					{
						Prefs.AutosaveIntervalDays = 0.25f;
					}));
				}
				list6.Add(new FloatMenuOption("0.5 " + text + string.Empty, delegate
				{
					Prefs.AutosaveIntervalDays = 0.5f;
				}));
				list6.Add(new FloatMenuOption(1.ToString() + " " + text2, delegate
				{
					Prefs.AutosaveIntervalDays = 1f;
				}));
				list6.Add(new FloatMenuOption(3.ToString() + " " + text, delegate
				{
					Prefs.AutosaveIntervalDays = 3f;
				}));
				list6.Add(new FloatMenuOption(7.ToString() + " " + text, delegate
				{
					Prefs.AutosaveIntervalDays = 7f;
				}));
				list6.Add(new FloatMenuOption(14.ToString() + " " + text, delegate
				{
					Prefs.AutosaveIntervalDays = 14f;
				}));
				Find.WindowStack.Add(new FloatMenu(list6));
			}
			if (Current.ProgramState == ProgramState.Playing && Current.Game.Info.permadeathMode && Prefs.AutosaveIntervalDays > 1f)
			{
				GUI.color = Color.red;
				listing_Standard.Label("MaxPermadeathAutosaveIntervalInfo".Translate(1f));
				GUI.color = Color.white;
			}
			if (Current.ProgramState == ProgramState.Playing && listing_Standard.ButtonText("ChangeStoryteller".Translate(), "OptionsButton-ChooseStoryteller") && TutorSystem.AllowAction("ChooseStoryteller"))
			{
				Find.WindowStack.Add(new Page_SelectStorytellerInGame());
			}
			if (!DevModePermanentlyDisabledUtility.Disabled && listing_Standard.ButtonText("PermanentlyDisableDevMode".Translate()))
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmPermanentlyDisableDevMode".Translate(), delegate
				{
					DevModePermanentlyDisabledUtility.Disable();
				}, destructive: true));
			}
			if (!DevModePermanentlyDisabledUtility.Disabled || Prefs.DevMode)
			{
				bool checkOn11 = Prefs.DevMode;
				listing_Standard.CheckboxLabeled("DevelopmentMode".Translate(), ref checkOn11);
				Prefs.DevMode = checkOn11;
			}
			bool checkOn12 = Prefs.TestMapSizes;
			listing_Standard.CheckboxLabeled("EnableTestMapSizes".Translate(), ref checkOn12);
			Prefs.TestMapSizes = checkOn12;
			if (Prefs.DevMode)
			{
				bool checkOn13 = Prefs.ResetModsConfigOnCrash;
				listing_Standard.CheckboxLabeled("ResetModsConfigOnCrash".Translate(), ref checkOn13);
				Prefs.ResetModsConfigOnCrash = checkOn13;
				bool checkOn14 = Prefs.LogVerbose;
				listing_Standard.CheckboxLabeled("LogVerbose".Translate(), ref checkOn14);
				Prefs.LogVerbose = checkOn14;
			}
			listing_Standard.NewColumn();
			Text.Font = GameFont.Medium;
			listing_Standard.Label(string.Empty);
			Text.Font = GameFont.Small;
			listing_Standard.Gap();
			listing_Standard.Gap();
			if (listing_Standard.ButtonText("ModSettings".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ModSettings());
			}
			listing_Standard.Label(string.Empty);
			listing_Standard.Label("NamesYouWantToSee".Translate());
			Prefs.PreferredNames.RemoveAll(GenText.NullOrEmpty);
			for (int j = 0; j < Prefs.PreferredNames.Count; j++)
			{
				string name = Prefs.PreferredNames[j];
				PawnBio pawnBio = (from b in SolidBioDatabase.allBios
				where b.name.ToString() == name
				select b).FirstOrDefault();
				if (pawnBio != null)
				{
					switch (pawnBio.BioType)
					{
					case PawnBioType.BackstoryInGame:
						name += " [B]";
						break;
					case PawnBioType.PirateKing:
						name += " [PK]";
						break;
					}
				}
				else
				{
					name += " [N]";
				}
				Rect rect2 = listing_Standard.GetRect(24f);
				Widgets.Label(rect2, name);
				Rect butRect = new Rect(rect2.xMax - 24f, rect2.y, 24f, 24f);
				if (Widgets.ButtonImage(butRect, TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor))
				{
					Prefs.PreferredNames.RemoveAt(j);
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
			}
			if (Prefs.PreferredNames.Count < 6 && listing_Standard.ButtonText("AddName".Translate() + "..."))
			{
				Find.WindowStack.Add(new Dialog_AddPreferredName());
			}
			listing_Standard.Label(string.Empty);
			if (listing_Standard.ButtonText("RestoreToDefaultSettings".Translate()))
			{
				RestoreToDefaultSettings();
			}
			listing_Standard.End();
		}

		public override void PreClose()
		{
			base.PreClose();
			Prefs.Save();
		}

		public static string ResToString(int width, int height)
		{
			string text = width + "x" + height;
			if (width == 1280 && height == 720)
			{
				text += " (720p)";
			}
			if (width == 1920 && height == 1080)
			{
				text += " (1080p)";
			}
			return text;
		}

		public void RestoreToDefaultSettings()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.ConfigFolderPath);
			FileInfo[] files = directoryInfo.GetFiles("*.xml");
			foreach (FileInfo fileInfo in files)
			{
				try
				{
					fileInfo.Delete();
				}
				catch (SystemException)
				{
				}
			}
			Find.WindowStack.Add(new Dialog_MessageBox("ResetAndRestart".Translate(), null, delegate
			{
				GenCommandLine.Restart();
			}));
		}
	}
}
