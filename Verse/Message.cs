using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Message : IArchivable, IExposable, ILoadReferenceable
	{
		public MessageTypeDef def;

		private int ID;

		public string text;

		private float startingTime;

		public int startingFrame;

		public int startingTick;

		public LookTargets lookTargets;

		private Vector2 cachedSize = new Vector2(-1f, -1f);

		public Rect lastDrawRect;

		private const float DefaultMessageLifespan = 13f;

		private const float FadeoutDuration = 0.6f;

		Texture IArchivable.ArchivedIcon
		{
			get
			{
				return null;
			}
		}

		Color IArchivable.ArchivedIconColor
		{
			get
			{
				return Color.white;
			}
		}

		string IArchivable.ArchivedLabel
		{
			get
			{
				return text.Flatten();
			}
		}

		string IArchivable.ArchivedTooltip
		{
			get
			{
				return text;
			}
		}

		int IArchivable.CreatedTicksGame
		{
			get
			{
				return startingTick;
			}
		}

		bool IArchivable.CanCullArchivedNow
		{
			get
			{
				return !Messages.IsLive(this);
			}
		}

		LookTargets IArchivable.LookTargets
		{
			get
			{
				return lookTargets;
			}
		}

		protected float Age => RealTime.LastRealTime - startingTime;

		protected float TimeLeft => 13f - Age;

		public bool Expired => TimeLeft <= 0f;

		public float Alpha
		{
			get
			{
				if (TimeLeft < 0.6f)
				{
					return TimeLeft / 0.6f;
				}
				return 1f;
			}
		}

		private static bool ShouldDrawBackground
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					return true;
				}
				WindowStack windowStack = Find.WindowStack;
				for (int i = 0; i < windowStack.Count; i++)
				{
					if (windowStack[i].CausesMessageBackground())
					{
						return true;
					}
				}
				return false;
			}
		}

		public Message()
		{
		}

		public Message(string text, MessageTypeDef def)
		{
			this.text = text;
			this.def = def;
			startingFrame = RealTime.frameCount;
			startingTime = RealTime.LastRealTime;
			startingTick = GenTicks.TicksGame;
			if (Find.UniqueIDsManager != null)
			{
				ID = Find.UniqueIDsManager.GetNextMessageID();
			}
			else
			{
				ID = Rand.Int;
			}
		}

		public Message(string text, MessageTypeDef def, LookTargets lookTargets)
			: this(text, def)
		{
			this.lookTargets = lookTargets;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref ID, "ID", 0);
			Scribe_Values.Look(ref text, "text");
			Scribe_Values.Look(ref startingTime, "startingTime", 0f);
			Scribe_Values.Look(ref startingFrame, "startingFrame", 0);
			Scribe_Values.Look(ref startingTick, "startingTick", 0);
			Scribe_Deep.Look(ref lookTargets, "lookTargets");
		}

		public Rect CalculateRect(float x, float y)
		{
			Text.Font = GameFont.Small;
			if (cachedSize.x < 0f)
			{
				cachedSize = Text.CalcSize(text);
			}
			lastDrawRect = new Rect(x, y, cachedSize.x, cachedSize.y);
			lastDrawRect = lastDrawRect.ContractedBy(-2f);
			return lastDrawRect;
		}

		public void Draw(int xOffset, int yOffset)
		{
			Rect rect = CalculateRect((float)xOffset, (float)yOffset);
			Find.WindowStack.ImmediateWindow(Gen.HashCombineInt(ID, 45574281), rect, WindowLayer.Super, delegate
			{
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect2 = rect.AtZero();
				float alpha = Alpha;
				GUI.color = new Color(1f, 1f, 1f, alpha);
				if (ShouldDrawBackground)
				{
					GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.8f * alpha);
					GUI.DrawTexture(rect2, BaseContent.WhiteTex);
					GUI.color = new Color(1f, 1f, 1f, alpha);
				}
				if (CameraJumper.CanJump(lookTargets.TryGetPrimaryTarget()))
				{
					UIHighlighter.HighlightOpportunity(rect2, "Messages");
					Widgets.DrawHighlightIfMouseover(rect2);
				}
				Rect rect3 = new Rect(2f, 0f, rect2.width - 2f, rect2.height);
				Widgets.Label(rect3, text);
				if (Current.ProgramState == ProgramState.Playing && CameraJumper.CanJump(lookTargets.TryGetPrimaryTarget()) && Widgets.ButtonInvisible(rect2))
				{
					CameraJumper.TryJumpAndSelect(lookTargets.TryGetPrimaryTarget());
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.ClickingMessages, KnowledgeAmount.Total);
				}
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				if (Mouse.IsOver(rect2))
				{
					Messages.Notify_Mouseover(this);
				}
			}, doBackground: false, absorbInputAroundWindow: false, 0f);
		}

		void IArchivable.OpenArchived()
		{
			Find.WindowStack.Add(new Dialog_MessageBox(text));
		}

		public string GetUniqueLoadID()
		{
			return "Message_" + ID;
		}
	}
}
