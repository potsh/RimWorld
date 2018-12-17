using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Lesson_Note : Lesson
	{
		public ConceptDef def;

		public bool doFadeIn = true;

		private float expiryTime = 3.40282347E+38f;

		private const float RectWidth = 500f;

		private const float TextWidth = 432f;

		private const float FadeInDuration = 0.4f;

		private const float DoneButPad = 8f;

		private const float DoneButSize = 32f;

		private const float ExpiryDuration = 2.1f;

		private const float ExpiryFadeTime = 1.1f;

		public bool Expiring => expiryTime < 3.40282347E+38f;

		public Rect MainRect
		{
			get
			{
				float num = Text.CalcHeight(def.HelpTextAdjusted, 432f);
				float height = num + 20f;
				Vector2 messagesTopLeftStandard = Messages.MessagesTopLeftStandard;
				return new Rect(messagesTopLeftStandard.x, 0f, 500f, height);
			}
		}

		public override float MessagesYOffset => MainRect.height;

		public Lesson_Note()
		{
		}

		public Lesson_Note(ConceptDef concept)
		{
			def = concept;
		}

		public override void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
		}

		public override void OnActivated()
		{
			base.OnActivated();
			SoundDefOf.TutorMessageAppear.PlayOneShotOnCamera();
		}

		public override void LessonOnGUI()
		{
			Rect mainRect = MainRect;
			float alpha = 1f;
			if (doFadeIn)
			{
				alpha = Mathf.Clamp01(base.AgeSeconds / 0.4f);
			}
			if (Expiring)
			{
				float num = expiryTime - Time.timeSinceLevelLoad;
				if (num < 1.1f)
				{
					alpha = num / 1.1f;
				}
			}
			WindowStack windowStack = Find.WindowStack;
			int iD = 134706;
			Rect rect = mainRect;
			WindowLayer layer = WindowLayer.Super;
			Action doWindowFunc = delegate
			{
				Rect rect2 = mainRect.AtZero();
				Text.Font = GameFont.Small;
				if (!Expiring)
				{
					def.HighlightAllTags();
				}
				if (doFadeIn || Expiring)
				{
					GUI.color = new Color(1f, 1f, 1f, alpha);
				}
				Widgets.DrawWindowBackgroundTutor(rect2);
				Rect rect3 = rect2.ContractedBy(10f);
				rect3.width = 432f;
				Widgets.Label(rect3, def.HelpTextAdjusted);
				Rect butRect = new Rect(rect2.xMax - 32f - 8f, rect2.y + 8f, 32f, 32f);
				Texture2D tex = (!Expiring) ? TexButton.CloseXBig : Widgets.CheckboxOnTex;
				if (Widgets.ButtonImage(butRect, tex, new Color(0.95f, 0.95f, 0.95f), new Color(0.8352941f, 0.6666667f, 0.274509817f)))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					CloseButtonClicked();
				}
				if (Time.timeSinceLevelLoad > expiryTime)
				{
					CloseButtonClicked();
				}
				GUI.color = Color.white;
			};
			bool doBackground = false;
			float shadowAlpha = alpha;
			windowStack.ImmediateWindow(iD, rect, layer, doWindowFunc, doBackground, absorbInputAroundWindow: false, shadowAlpha);
		}

		private void CloseButtonClicked()
		{
			KnowledgeAmount know = (!def.noteTeaches) ? KnowledgeAmount.NoteClosed : KnowledgeAmount.NoteTaught;
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(def, know);
			Find.ActiveLesson.Deactivate();
		}

		public override void Notify_KnowledgeDemonstrated(ConceptDef conc)
		{
			if (def == conc && PlayerKnowledgeDatabase.GetKnowledge(conc) > 0.2f && !Expiring)
			{
				expiryTime = Time.timeSinceLevelLoad + 2.1f;
			}
		}
	}
}
