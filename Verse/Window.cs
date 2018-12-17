using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Window
	{
		public WindowLayer layer = WindowLayer.Dialog;

		public string optionalTitle;

		public bool doCloseX;

		public bool doCloseButton;

		public bool closeOnAccept = true;

		public bool closeOnCancel = true;

		public bool forceCatchAcceptAndCancelEventEvenIfUnfocused;

		public bool closeOnClickedOutside;

		public bool forcePause;

		public bool preventCameraMotion = true;

		public bool preventDrawTutor;

		public bool doWindowBackground = true;

		public bool onlyOneOfTypeAllowed = true;

		public bool absorbInputAroundWindow;

		public bool resizeable;

		public bool draggable;

		public bool drawShadow = true;

		public bool focusWhenOpened = true;

		public float shadowAlpha = 1f;

		public SoundDef soundAppear;

		public SoundDef soundClose;

		public SoundDef soundAmbient;

		public bool silenceAmbientSound;

		protected const float StandardMargin = 18f;

		protected readonly Vector2 CloseButSize = new Vector2(120f, 40f);

		public int ID;

		public Rect windowRect;

		private Sustainer sustainerAmbient;

		private WindowResizer resizer;

		private bool resizeLater;

		private Rect resizeLaterRect;

		public virtual Vector2 InitialSize => new Vector2(500f, 500f);

		protected virtual float Margin => 18f;

		public virtual bool IsDebug => false;

		public bool IsOpen => Find.WindowStack.IsOpen(this);

		public Window()
		{
			soundAppear = SoundDefOf.DialogBoxAppear;
			soundClose = SoundDefOf.Click;
		}

		public virtual void WindowUpdate()
		{
			if (sustainerAmbient != null)
			{
				sustainerAmbient.Maintain();
			}
		}

		public abstract void DoWindowContents(Rect inRect);

		public virtual void ExtraOnGUI()
		{
		}

		public virtual void PreOpen()
		{
			SetInitialSizeAndPosition();
			if (layer == WindowLayer.Dialog)
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					Find.DesignatorManager.Dragger.EndDrag();
					Find.DesignatorManager.Deselect();
					Find.Selector.Notify_DialogOpened();
				}
				if (Find.World != null)
				{
					Find.WorldSelector.Notify_DialogOpened();
				}
			}
		}

		public virtual void PostOpen()
		{
			if (soundAppear != null)
			{
				soundAppear.PlayOneShotOnCamera();
			}
			if (soundAmbient != null)
			{
				sustainerAmbient = soundAmbient.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerFrame));
			}
		}

		public virtual void PreClose()
		{
		}

		public virtual void PostClose()
		{
		}

		public virtual void WindowOnGUI()
		{
			if (resizeable)
			{
				if (resizer == null)
				{
					resizer = new WindowResizer();
				}
				if (resizeLater)
				{
					resizeLater = false;
					windowRect = resizeLaterRect;
				}
			}
			windowRect = windowRect.Rounded();
			Rect winRect = windowRect.AtZero();
			windowRect = GUI.Window(ID, windowRect, delegate
			{
				UnityGUIBugsFixer.OnGUI();
				Find.WindowStack.currentlyDrawnWindow = this;
				if (doWindowBackground)
				{
					Widgets.DrawWindowBackground(winRect);
				}
				if (KeyBindingDefOf.Cancel.KeyDownEvent)
				{
					Find.WindowStack.Notify_PressedCancel();
				}
				if (KeyBindingDefOf.Accept.KeyDownEvent)
				{
					Find.WindowStack.Notify_PressedAccept();
				}
				if (Event.current.type == EventType.MouseDown)
				{
					Find.WindowStack.Notify_ClickedInsideWindow(this);
				}
				if (Event.current.type == EventType.KeyDown && !Find.WindowStack.GetsInput(this))
				{
					Event.current.Use();
				}
				if (!optionalTitle.NullOrEmpty())
				{
					GUI.Label(new Rect(Margin, Margin, windowRect.width, 25f), optionalTitle);
				}
				if (doCloseX && Widgets.CloseButtonFor(winRect))
				{
					Close();
				}
				if (resizeable && Event.current.type != EventType.Repaint)
				{
					Rect lhs = resizer.DoResizeControl(windowRect);
					if (lhs != windowRect)
					{
						resizeLater = true;
						resizeLaterRect = lhs;
					}
				}
				Rect rect = winRect.ContractedBy(Margin);
				if (!optionalTitle.NullOrEmpty())
				{
					rect.yMin += Margin + 25f;
				}
				GUI.BeginGroup(rect);
				try
				{
					DoWindowContents(rect.AtZero());
				}
				catch (Exception ex)
				{
					Log.Error("Exception filling window for " + GetType() + ": " + ex);
				}
				GUI.EndGroup();
				if (resizeable && Event.current.type == EventType.Repaint)
				{
					resizer.DoResizeControl(windowRect);
				}
				if (doCloseButton)
				{
					Text.Font = GameFont.Small;
					float num = winRect.width / 2f;
					Vector2 closeButSize = CloseButSize;
					float x2 = num - closeButSize.x / 2f;
					float y = winRect.height - 55f;
					Vector2 closeButSize2 = CloseButSize;
					float x3 = closeButSize2.x;
					Vector2 closeButSize3 = CloseButSize;
					Rect rect2 = new Rect(x2, y, x3, closeButSize3.y);
					if (Widgets.ButtonText(rect2, "CloseButton".Translate()))
					{
						Close();
					}
				}
				if (KeyBindingDefOf.Cancel.KeyDownEvent && IsOpen)
				{
					OnCancelKeyPressed();
				}
				if (draggable)
				{
					GUI.DragWindow();
				}
				else if (Event.current.type == EventType.MouseDown)
				{
					Event.current.Use();
				}
				ScreenFader.OverlayOnGUI(winRect.size);
				Find.WindowStack.currentlyDrawnWindow = null;
			}, string.Empty, Widgets.EmptyStyle);
		}

		public virtual void Close(bool doCloseSound = true)
		{
			Find.WindowStack.TryRemove(this, doCloseSound);
		}

		public virtual bool CausesMessageBackground()
		{
			return false;
		}

		protected virtual void SetInitialSizeAndPosition()
		{
			float num = (float)UI.screenWidth;
			Vector2 initialSize = InitialSize;
			float x = (num - initialSize.x) / 2f;
			float num2 = (float)UI.screenHeight;
			Vector2 initialSize2 = InitialSize;
			float y = (num2 - initialSize2.y) / 2f;
			Vector2 initialSize3 = InitialSize;
			float x2 = initialSize3.x;
			Vector2 initialSize4 = InitialSize;
			windowRect = new Rect(x, y, x2, initialSize4.y);
			windowRect = windowRect.Rounded();
		}

		public virtual void OnCancelKeyPressed()
		{
			if (closeOnCancel)
			{
				Close();
				Event.current.Use();
			}
		}

		public virtual void OnAcceptKeyPressed()
		{
			if (closeOnAccept)
			{
				Close();
				Event.current.Use();
			}
		}

		public virtual void Notify_ResolutionChanged()
		{
			SetInitialSizeAndPosition();
		}
	}
}
