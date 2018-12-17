using RimWorld;
using RimWorld.Planet;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse.AI;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;

namespace Verse
{
	public abstract class Root : MonoBehaviour
	{
		private static bool globalInitDone;

		private static bool prefsApplied;

		protected static bool checkedAutostartSaveFile;

		protected bool destroyed;

		public SoundRoot soundRoot;

		public UIRoot uiRoot;

		[CompilerGenerated]
		private static Action _003C_003Ef__mg_0024cache0;

		public virtual void Start()
		{
			try
			{
				CultureInfoUtility.EnsureEnglish();
				Current.Notify_LoadedSceneChanged();
				CheckGlobalInit();
				Action action = delegate
				{
					soundRoot = new SoundRoot();
					if (GenScene.InPlayScene)
					{
						uiRoot = new UIRoot_Play();
					}
					else if (GenScene.InEntryScene)
					{
						uiRoot = new UIRoot_Entry();
					}
					uiRoot.Init();
					Messages.Notify_LoadedLevelChanged();
					if (Current.SubcameraDriver != null)
					{
						Current.SubcameraDriver.Init();
					}
				};
				if (!PlayDataLoader.Loaded)
				{
					LongEventHandler.QueueLongEvent(delegate
					{
						PlayDataLoader.LoadAllPlayData();
					}, null, doAsynchronously: true, null);
					LongEventHandler.QueueLongEvent(action, "InitializingInterface", doAsynchronously: false, null);
				}
				else
				{
					action();
				}
			}
			catch (Exception arg)
			{
				Log.Error("Critical error in root Start(): " + arg);
			}
		}

		private static void CheckGlobalInit()
		{
			if (!globalInitDone)
			{
				UnityDataInitializer.CopyUnityData();
				SteamManager.InitIfNeeded();
				string[] commandLineArgs = Environment.GetCommandLineArgs();
				if (commandLineArgs != null && commandLineArgs.Length > 1)
				{
					Log.Message("Command line arguments: " + GenText.ToSpaceList(commandLineArgs.Skip(1)));
				}
				VersionControl.LogVersionNumber();
				Application.targetFrameRate = 60;
				Prefs.Init();
				if (Prefs.DevMode)
				{
					StaticConstructorOnStartupUtility.ReportProbablyMissingAttributes();
				}
				LongEventHandler.QueueLongEvent(StaticConstructorOnStartupUtility.CallAll, null, doAsynchronously: false, null);
				globalInitDone = true;
			}
		}

		public virtual void Update()
		{
			try
			{
				ResolutionUtility.Update();
				RealTime.Update();
				LongEventHandler.LongEventsUpdate(out bool sceneChanged);
				if (sceneChanged)
				{
					destroyed = true;
				}
				else if (!LongEventHandler.ShouldWaitForEvent)
				{
					Rand.EnsureStateStackEmpty();
					Widgets.EnsureMousePositionStackEmpty();
					SteamManager.Update();
					PortraitsCache.PortraitsCacheUpdate();
					AttackTargetsCache.AttackTargetsCacheStaticUpdate();
					Pawn_MeleeVerbs.PawnMeleeVerbsStaticUpdate();
					Storyteller.StorytellerStaticUpdate();
					CaravanInventoryUtility.CaravanInventoryUtilityStaticUpdate();
					uiRoot.UIRootUpdate();
					if (Time.frameCount > 3 && !prefsApplied)
					{
						prefsApplied = true;
						Prefs.Apply();
					}
					soundRoot.Update();
					try
					{
						MemoryTracker.Update();
					}
					catch (Exception arg)
					{
						Log.Error("Error in MemoryTracker: " + arg);
					}
					try
					{
						MapLeakTracker.Update();
					}
					catch (Exception arg2)
					{
						Log.Error("Error in MapLeakTracker: " + arg2);
					}
				}
			}
			catch (Exception arg3)
			{
				Log.Error("Root level exception in Update(): " + arg3);
			}
		}

		public void OnGUI()
		{
			try
			{
				if (!destroyed)
				{
					GUI.depth = 50;
					UI.ApplyUIScale();
					LongEventHandler.LongEventsOnGUI();
					if (LongEventHandler.ShouldWaitForEvent)
					{
						ScreenFader.OverlayOnGUI(new Vector2((float)UI.screenWidth, (float)UI.screenHeight));
					}
					else
					{
						uiRoot.UIRootOnGUI();
						ScreenFader.OverlayOnGUI(new Vector2((float)UI.screenWidth, (float)UI.screenHeight));
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error("Root level exception in OnGUI(): " + arg);
			}
		}

		public static void Shutdown()
		{
			SteamManager.ShutdownSteam();
			DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.TempFolderPath);
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				fileInfo.Delete();
			}
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				directoryInfo2.Delete(recursive: true);
			}
			Application.Quit();
		}
	}
}
