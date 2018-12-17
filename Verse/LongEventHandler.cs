using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Verse
{
	public static class LongEventHandler
	{
		private class QueuedLongEvent
		{
			public Action eventAction;

			public IEnumerator eventActionEnumerator;

			public string levelToLoad;

			public string eventTextKey = string.Empty;

			public string eventText = string.Empty;

			public bool doAsynchronously;

			public Action<Exception> exceptionHandler;

			public bool alreadyDisplayed;

			public bool canEverUseStandardWindow = true;

			public bool UseAnimatedDots => doAsynchronously || eventActionEnumerator != null;

			public bool ShouldWaitUntilDisplayed => !alreadyDisplayed && UseStandardWindow && !eventText.NullOrEmpty();

			public bool UseStandardWindow => canEverUseStandardWindow && !doAsynchronously && eventActionEnumerator == null;
		}

		private static Queue<QueuedLongEvent> eventQueue = new Queue<QueuedLongEvent>();

		private static QueuedLongEvent currentEvent = null;

		private static Thread eventThread = null;

		private static AsyncOperation levelLoadOp = null;

		private static List<Action> toExecuteWhenFinished = new List<Action>();

		private static bool executingToExecuteWhenFinished = false;

		private static readonly object CurrentEventTextLock = new object();

		private static readonly Vector2 GUIRectSize = new Vector2(240f, 75f);

		public static bool ShouldWaitForEvent
		{
			get
			{
				if (!AnyEventNowOrWaiting)
				{
					return false;
				}
				if (currentEvent != null && !currentEvent.UseStandardWindow)
				{
					return true;
				}
				if (Find.UIRoot == null || Find.WindowStack == null)
				{
					return true;
				}
				return false;
			}
		}

		public static bool CanApplyUIScaleNow => currentEvent?.levelToLoad.NullOrEmpty() ?? true;

		public static bool AnyEventNowOrWaiting => currentEvent != null || eventQueue.Count > 0;

		private static bool AnyEventWhichDoesntUseStandardWindowNowOrWaiting
		{
			get
			{
				QueuedLongEvent queuedLongEvent = currentEvent;
				if (queuedLongEvent != null && !queuedLongEvent.UseStandardWindow)
				{
					return true;
				}
				return eventQueue.Any((QueuedLongEvent x) => !x.UseStandardWindow);
			}
		}

		public static bool ForcePause => AnyEventNowOrWaiting;

		public static void QueueLongEvent(Action action, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventAction = action;
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = doAsynchronously;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			queuedLongEvent.canEverUseStandardWindow = !AnyEventWhichDoesntUseStandardWindowNowOrWaiting;
			eventQueue.Enqueue(queuedLongEvent);
		}

		public static void QueueLongEvent(IEnumerable action, string textKey, Action<Exception> exceptionHandler = null)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventActionEnumerator = action.GetEnumerator();
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = false;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			queuedLongEvent.canEverUseStandardWindow = !AnyEventWhichDoesntUseStandardWindowNowOrWaiting;
			eventQueue.Enqueue(queuedLongEvent);
		}

		public static void QueueLongEvent(Action preLoadLevelAction, string levelToLoad, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventAction = preLoadLevelAction;
			queuedLongEvent.levelToLoad = levelToLoad;
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = doAsynchronously;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			queuedLongEvent.canEverUseStandardWindow = !AnyEventWhichDoesntUseStandardWindowNowOrWaiting;
			eventQueue.Enqueue(queuedLongEvent);
		}

		public static void ClearQueuedEvents()
		{
			eventQueue.Clear();
		}

		public static void LongEventsOnGUI()
		{
			if (currentEvent != null)
			{
				Vector2 gUIRectSize = GUIRectSize;
				float num = gUIRectSize.x;
				lock (CurrentEventTextLock)
				{
					Text.Font = GameFont.Small;
					float a = num;
					Vector2 vector = Text.CalcSize(currentEvent.eventText + "...");
					num = Mathf.Max(a, vector.x + 40f);
				}
				float x = ((float)UI.screenWidth - num) / 2f;
				float num2 = (float)UI.screenHeight;
				Vector2 gUIRectSize2 = GUIRectSize;
				float y = (num2 - gUIRectSize2.y) / 2f;
				float width = num;
				Vector2 gUIRectSize3 = GUIRectSize;
				Rect rect = new Rect(x, y, width, gUIRectSize3.y);
				rect = rect.Rounded();
				if (!currentEvent.UseStandardWindow || Find.UIRoot == null || Find.WindowStack == null)
				{
					if (UIMenuBackgroundManager.background == null)
					{
						UIMenuBackgroundManager.background = new UI_BackgroundMain();
					}
					UIMenuBackgroundManager.background.BackgroundOnGUI();
					Widgets.DrawShadowAround(rect);
					Widgets.DrawWindowBackground(rect);
					DrawLongEventWindowContents(rect);
				}
				else
				{
					Find.WindowStack.ImmediateWindow(62893994, rect, WindowLayer.Super, delegate
					{
						DrawLongEventWindowContents(rect.AtZero());
					});
				}
			}
		}

		public static void LongEventsUpdate(out bool sceneChanged)
		{
			sceneChanged = false;
			if (currentEvent != null)
			{
				if (currentEvent.eventActionEnumerator != null)
				{
					UpdateCurrentEnumeratorEvent();
				}
				else if (currentEvent.doAsynchronously)
				{
					UpdateCurrentAsynchronousEvent();
				}
				else
				{
					UpdateCurrentSynchronousEvent(out sceneChanged);
				}
			}
			if (currentEvent == null && eventQueue.Count > 0)
			{
				currentEvent = eventQueue.Dequeue();
				if (currentEvent.eventTextKey == null)
				{
					currentEvent.eventText = string.Empty;
				}
				else
				{
					currentEvent.eventText = currentEvent.eventTextKey.Translate();
				}
			}
		}

		public static void ExecuteWhenFinished(Action action)
		{
			toExecuteWhenFinished.Add(action);
			if ((currentEvent == null || currentEvent.ShouldWaitUntilDisplayed) && !executingToExecuteWhenFinished)
			{
				ExecuteToExecuteWhenFinished();
			}
		}

		public static void SetCurrentEventText(string newText)
		{
			lock (CurrentEventTextLock)
			{
				if (currentEvent != null)
				{
					currentEvent.eventText = newText;
				}
			}
		}

		private static void UpdateCurrentEnumeratorEvent()
		{
			try
			{
				float num = Time.realtimeSinceStartup + 0.1f;
				while (currentEvent.eventActionEnumerator.MoveNext())
				{
					if (num <= Time.realtimeSinceStartup)
					{
						return;
					}
				}
				(currentEvent.eventActionEnumerator as IDisposable)?.Dispose();
				currentEvent = null;
				eventThread = null;
				levelLoadOp = null;
				ExecuteToExecuteWhenFinished();
			}
			catch (Exception ex)
			{
				Log.Error("Exception from long event: " + ex);
				if (currentEvent != null)
				{
					(currentEvent.eventActionEnumerator as IDisposable)?.Dispose();
					if (currentEvent.exceptionHandler != null)
					{
						currentEvent.exceptionHandler(ex);
					}
				}
				currentEvent = null;
				eventThread = null;
				levelLoadOp = null;
			}
		}

		private static void UpdateCurrentAsynchronousEvent()
		{
			if (eventThread == null)
			{
				eventThread = new Thread((ThreadStart)delegate
				{
					RunEventFromAnotherThread(currentEvent.eventAction);
				});
				eventThread.Start();
			}
			else if (!eventThread.IsAlive)
			{
				bool flag = false;
				if (!currentEvent.levelToLoad.NullOrEmpty())
				{
					if (levelLoadOp == null)
					{
						levelLoadOp = SceneManager.LoadSceneAsync(currentEvent.levelToLoad);
					}
					else if (levelLoadOp.isDone)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					currentEvent = null;
					eventThread = null;
					levelLoadOp = null;
					ExecuteToExecuteWhenFinished();
				}
			}
		}

		private static void UpdateCurrentSynchronousEvent(out bool sceneChanged)
		{
			sceneChanged = false;
			if (!currentEvent.ShouldWaitUntilDisplayed)
			{
				try
				{
					if (currentEvent.eventAction != null)
					{
						currentEvent.eventAction();
					}
					if (!currentEvent.levelToLoad.NullOrEmpty())
					{
						SceneManager.LoadScene(currentEvent.levelToLoad);
						sceneChanged = true;
					}
					currentEvent = null;
					eventThread = null;
					levelLoadOp = null;
					ExecuteToExecuteWhenFinished();
				}
				catch (Exception ex)
				{
					Log.Error("Exception from long event: " + ex);
					if (currentEvent != null && currentEvent.exceptionHandler != null)
					{
						currentEvent.exceptionHandler(ex);
					}
					currentEvent = null;
					eventThread = null;
					levelLoadOp = null;
				}
			}
		}

		private static void RunEventFromAnotherThread(Action action)
		{
			CultureInfoUtility.EnsureEnglish();
			try
			{
				action?.Invoke();
			}
			catch (Exception ex)
			{
				Log.Error("Exception from asynchronous event: " + ex);
				try
				{
					if (currentEvent != null && currentEvent.exceptionHandler != null)
					{
						currentEvent.exceptionHandler(ex);
					}
				}
				catch (Exception arg)
				{
					Log.Error("Exception was thrown while trying to handle exception. Exception: " + arg);
				}
			}
		}

		private static void ExecuteToExecuteWhenFinished()
		{
			if (executingToExecuteWhenFinished)
			{
				Log.Warning("Already executing.");
			}
			else
			{
				executingToExecuteWhenFinished = true;
				for (int i = 0; i < toExecuteWhenFinished.Count; i++)
				{
					try
					{
						toExecuteWhenFinished[i]();
					}
					catch (Exception arg)
					{
						Log.Error("Could not execute post-long-event action. Exception: " + arg);
					}
				}
				toExecuteWhenFinished.Clear();
				executingToExecuteWhenFinished = false;
			}
		}

		private static void DrawLongEventWindowContents(Rect rect)
		{
			if (currentEvent != null)
			{
				if (Event.current.type == EventType.Repaint)
				{
					currentEvent.alreadyDisplayed = true;
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				float num = 0f;
				if (levelLoadOp != null)
				{
					float f = 1f;
					if (!levelLoadOp.isDone)
					{
						f = levelLoadOp.progress;
					}
					string text = "LoadingAssets".Translate() + " " + f.ToStringPercent();
					Vector2 vector = Text.CalcSize(text);
					num = vector.x;
					Widgets.Label(rect, text);
				}
				else
				{
					lock (CurrentEventTextLock)
					{
						Vector2 vector2 = Text.CalcSize(currentEvent.eventText);
						num = vector2.x;
						Widgets.Label(rect, currentEvent.eventText);
					}
				}
				Text.Anchor = TextAnchor.MiddleLeft;
				Vector2 center = rect.center;
				rect.xMin = center.x + num / 2f;
				Widgets.Label(rect, currentEvent.UseAnimatedDots ? GenText.MarchingEllipsis() : "...");
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}
	}
}
