using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class LearningReadout : IExposable
	{
		private List<ConceptDef> activeConcepts = new List<ConceptDef>();

		private ConceptDef selectedConcept;

		private bool showAllMode;

		private float contentHeight;

		private Vector2 scrollPosition = Vector2.zero;

		private string searchString = string.Empty;

		private float lastConceptActivateRealTime = -999f;

		private ConceptDef mouseoverConcept;

		private const float OuterMargin = 8f;

		private const float InnerMargin = 7f;

		private const float ReadoutWidth = 200f;

		private const float InfoPaneWidth = 310f;

		private const float OpenButtonSize = 24f;

		public static readonly Texture2D ProgressBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.745098054f, 0.6039216f, 0.2f));

		public static readonly Texture2D ProgressBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.509803951f, 0.407843143f, 0.13333334f));

		[CompilerGenerated]
		private static Predicate<ConceptDef> _003C_003Ef__mg_0024cache0;

		public int ActiveConceptsCount => activeConcepts.Count;

		public bool ShowAllMode => showAllMode;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref activeConcepts, "activeConcepts", LookMode.Undefined);
			Scribe_Defs.Look(ref selectedConcept, "selectedConcept");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				activeConcepts.RemoveAll(PlayerKnowledgeDatabase.IsComplete);
			}
		}

		public bool TryActivateConcept(ConceptDef conc)
		{
			if (activeConcepts.Contains(conc))
			{
				return false;
			}
			activeConcepts.Add(conc);
			SoundDefOf.Lesson_Activated.PlayOneShotOnCamera();
			lastConceptActivateRealTime = RealTime.LastRealTime;
			return true;
		}

		public bool IsActive(ConceptDef conc)
		{
			return activeConcepts.Contains(conc);
		}

		public void LearningReadoutUpdate()
		{
		}

		public void Notify_ConceptNewlyLearned(ConceptDef conc)
		{
			if (activeConcepts.Contains(conc) || selectedConcept == conc)
			{
				SoundDefOf.Lesson_Deactivated.PlayOneShotOnCamera();
				SoundDefOf.CommsWindow_Close.PlayOneShotOnCamera();
			}
			if (activeConcepts.Contains(conc))
			{
				activeConcepts.Remove(conc);
			}
			if (selectedConcept == conc)
			{
				selectedConcept = null;
			}
		}

		private string FilterSearchStringInput(string input)
		{
			if (input == searchString)
			{
				return input;
			}
			if (input.Length > 20)
			{
				input = input.Substring(0, 20);
			}
			return input;
		}

		public void LearningReadoutOnGUI()
		{
			if (!TutorSystem.TutorialMode && TutorSystem.AdaptiveTrainingEnabled && (Find.PlaySettings.showLearningHelper || activeConcepts.Count != 0) && !Find.WindowStack.IsOpen<Screen_Credits>())
			{
				float b = (float)UI.screenHeight / 2f;
				float a = contentHeight + 14f;
				Rect outRect = new Rect((float)UI.screenWidth - 8f - 200f, 8f, 200f, Mathf.Min(a, b));
				Rect rect = outRect;
				Find.WindowStack.ImmediateWindow(76136312, outRect, WindowLayer.Super, delegate
				{
					outRect = outRect.AtZero();
					Rect rect2 = outRect.ContractedBy(7f);
					Rect viewRect = rect2.AtZero();
					bool flag = contentHeight > rect2.height;
					Widgets.DrawWindowBackgroundTutor(outRect);
					if (flag)
					{
						viewRect.height = contentHeight + 40f;
						viewRect.width -= 20f;
						scrollPosition = GUI.BeginScrollView(rect2, scrollPosition, viewRect);
					}
					else
					{
						GUI.BeginGroup(rect2);
					}
					float num2 = 0f;
					Text.Font = GameFont.Small;
					Rect rect3 = new Rect(0f, 0f, viewRect.width - 24f, 24f);
					Widgets.Label(rect3, "LearningHelper".Translate());
					num2 = rect3.yMax;
					Rect butRect = new Rect(rect3.xMax, rect3.y, 24f, 24f);
					if (Widgets.ButtonImage(butRect, showAllMode ? TexButton.Minus : TexButton.Plus))
					{
						showAllMode = !showAllMode;
						if (showAllMode)
						{
							SoundDefOf.Tick_High.PlayOneShotOnCamera();
						}
						else
						{
							SoundDefOf.Tick_Low.PlayOneShotOnCamera();
						}
					}
					if (showAllMode)
					{
						Rect rect4 = new Rect(0f, num2, viewRect.width - 20f - 2f, 28f);
						searchString = FilterSearchStringInput(Widgets.TextField(rect4, searchString));
						if (searchString == string.Empty)
						{
							GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
							Text.Anchor = TextAnchor.MiddleLeft;
							Rect rect5 = rect4;
							rect5.xMin += 7f;
							Widgets.Label(rect5, "Filter".Translate() + "...");
							Text.Anchor = TextAnchor.UpperLeft;
							GUI.color = Color.white;
						}
						Rect butRect2 = new Rect(viewRect.width - 20f, num2 + 14f - 10f, 20f, 20f);
						if (Widgets.ButtonImage(butRect2, TexButton.CloseXSmall))
						{
							searchString = string.Empty;
							SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
						}
						num2 = rect4.yMax + 4f;
					}
					IEnumerable<ConceptDef> enumerable = showAllMode ? DefDatabase<ConceptDef>.AllDefs : activeConcepts;
					if (enumerable.Any())
					{
						GUI.color = new Color(1f, 1f, 1f, 0.5f);
						Widgets.DrawLineHorizontal(0f, num2, viewRect.width);
						GUI.color = Color.white;
						num2 += 4f;
					}
					if (showAllMode)
					{
						enumerable = from c in enumerable
						orderby DisplayPriority(c) descending, c.label
						select c;
					}
					foreach (ConceptDef item in enumerable)
					{
						if (!item.TriggeredDirect)
						{
							num2 = DrawConceptListRow(0f, num2, viewRect.width, item).yMax;
						}
					}
					contentHeight = num2;
					if (flag)
					{
						GUI.EndScrollView();
					}
					else
					{
						GUI.EndGroup();
					}
				}, doBackground: false);
				float num = Time.realtimeSinceStartup - lastConceptActivateRealTime;
				if (num < 1f && num > 0f)
				{
					float x = rect.x;
					Vector2 center = rect.center;
					GenUI.DrawFlash(x, center.y, (float)UI.screenWidth * 0.6f, Pulser.PulseBrightness(1f, 1f, num) * 0.85f, new Color(0.8f, 0.77f, 0.53f));
				}
				ConceptDef conceptDef = (selectedConcept == null) ? mouseoverConcept : selectedConcept;
				if (conceptDef != null)
				{
					DrawInfoPane(conceptDef);
					conceptDef.HighlightAllTags();
				}
				mouseoverConcept = null;
			}
		}

		private int DisplayPriority(ConceptDef conc)
		{
			int num = 1;
			if (MatchesSearchString(conc))
			{
				num += 10000;
			}
			return num;
		}

		private bool MatchesSearchString(ConceptDef conc)
		{
			return searchString != string.Empty && conc.label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private Rect DrawConceptListRow(float x, float y, float width, ConceptDef conc)
		{
			float knowledge = PlayerKnowledgeDatabase.GetKnowledge(conc);
			bool flag = PlayerKnowledgeDatabase.IsComplete(conc);
			bool flag2 = !flag && knowledge > 0f;
			float num = Text.CalcHeight(conc.LabelCap, width);
			if (flag2)
			{
				num = num;
			}
			Rect rect = new Rect(x, y, width, num);
			if (flag2)
			{
				Rect rect2 = new Rect(rect);
				rect2.yMin += 1f;
				rect2.yMax -= 1f;
				Widgets.FillableBar(rect2, PlayerKnowledgeDatabase.GetKnowledge(conc), ProgressBarFillTex, ProgressBarBGTex, doBorder: false);
			}
			if (flag)
			{
				GUI.DrawTexture(rect, BaseContent.GreyTex);
			}
			if (selectedConcept == conc)
			{
				GUI.DrawTexture(rect, TexUI.HighlightSelectedTex);
			}
			Widgets.DrawHighlightIfMouseover(rect);
			if (MatchesSearchString(conc))
			{
				Widgets.DrawHighlight(rect);
			}
			Widgets.Label(rect, conc.LabelCap);
			if (Mouse.IsOver(rect) && selectedConcept == null)
			{
				mouseoverConcept = conc;
			}
			if (Widgets.ButtonInvisible(rect, doMouseoverSound: true))
			{
				if (selectedConcept == conc)
				{
					selectedConcept = null;
				}
				else
				{
					selectedConcept = conc;
				}
				SoundDefOf.PageChange.PlayOneShotOnCamera();
			}
			return rect;
		}

		private Rect DrawInfoPane(ConceptDef conc)
		{
			float knowledge = PlayerKnowledgeDatabase.GetKnowledge(conc);
			bool complete = PlayerKnowledgeDatabase.IsComplete(conc);
			bool drawProgressBar = !complete && knowledge > 0f;
			Text.Font = GameFont.Medium;
			float titleHeight = Text.CalcHeight(conc.LabelCap, 276f);
			Text.Font = GameFont.Small;
			float textHeight = Text.CalcHeight(conc.HelpTextAdjusted, 296f);
			float num = titleHeight + textHeight + 14f + 5f;
			if (selectedConcept == conc)
			{
				num += 40f;
			}
			if (drawProgressBar)
			{
				num += 30f;
			}
			Rect outRect = new Rect((float)UI.screenWidth - 8f - 200f - 8f - 310f, 8f, 310f, num);
			Rect result = outRect;
			Find.WindowStack.ImmediateWindow(987612111, outRect, WindowLayer.Super, delegate
			{
				outRect = outRect.AtZero();
				Rect rect = outRect.ContractedBy(7f);
				Widgets.DrawShadowAround(outRect);
				Widgets.DrawWindowBackgroundTutor(outRect);
				Rect rect2 = rect;
				rect2.width -= 20f;
				rect2.height = titleHeight + 5f;
				Text.Font = GameFont.Medium;
				Widgets.Label(rect2, conc.LabelCap);
				Text.Font = GameFont.Small;
				Rect rect3 = rect;
				rect3.yMin = rect2.yMax;
				rect3.height = textHeight;
				Widgets.Label(rect3, conc.HelpTextAdjusted);
				if (drawProgressBar)
				{
					Rect rect4 = rect;
					rect4.yMin = rect3.yMax;
					rect4.height = 30f;
					Widgets.FillableBar(rect4, PlayerKnowledgeDatabase.GetKnowledge(conc), ProgressBarFillTex);
				}
				if (selectedConcept == conc)
				{
					if (Widgets.CloseButtonFor(outRect))
					{
						selectedConcept = null;
						SoundDefOf.PageChange.PlayOneShotOnCamera();
					}
					Vector2 center = rect.center;
					Rect rect5 = new Rect(center.x - 70f, rect.yMax - 30f, 140f, 30f);
					if (!complete)
					{
						if (Widgets.ButtonText(rect5, "MarkLearned".Translate()))
						{
							selectedConcept = null;
							SoundDefOf.PageChange.PlayOneShotOnCamera();
							PlayerKnowledgeDatabase.SetKnowledge(conc, 1f);
						}
					}
					else
					{
						GUI.color = new Color(1f, 1f, 1f, 0.5f);
						Text.Anchor = TextAnchor.MiddleCenter;
						Widgets.Label(rect5, "AlreadyLearned".Translate());
						Text.Anchor = TextAnchor.UpperLeft;
						GUI.color = Color.white;
					}
				}
			}, doBackground: false);
			return result;
		}
	}
}
