using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Screen_Credits : Window
	{
		private List<CreditsEntry> creds;

		public bool wonGame;

		private float timeUntilAutoScroll;

		private float scrollPosition;

		private bool playedMusic;

		public float creationRealtime = -1f;

		private const int ColumnWidth = 800;

		private const float InitialAutoScrollDelay = 1f;

		private const float InitialAutoScrollDelayWonGame = 6f;

		private const float AutoScrollDelayAfterManualScroll = 3f;

		private const float SongStartDelay = 5f;

		private const GameFont Font = GameFont.Medium;

		public override Vector2 InitialSize => new Vector2((float)UI.screenWidth, (float)UI.screenHeight);

		protected override float Margin => 0f;

		private float ViewWidth => 800f;

		private float ViewHeight
		{
			get
			{
				GameFont font = Text.Font;
				Text.Font = GameFont.Medium;
				float result = creds.Sum((CreditsEntry c) => c.DrawHeight(ViewWidth)) + 20f;
				Text.Font = font;
				return result;
			}
		}

		private float MaxScrollPosition => Mathf.Max(ViewHeight - (float)UI.screenHeight / 2f, 0f);

		private float AutoScrollRate
		{
			get
			{
				if (wonGame)
				{
					float num = SongDefOf.EndCreditsSong.clip.length + 5f - 6f;
					return MaxScrollPosition / num;
				}
				return 30f;
			}
		}

		public Screen_Credits()
			: this(string.Empty)
		{
		}

		public Screen_Credits(string preCreditsMessage)
		{
			doWindowBackground = false;
			doCloseButton = false;
			doCloseX = false;
			forcePause = true;
			creds = CreditsAssembler.AllCredits().ToList();
			creds.Insert(0, new CreditRecord_Space(100f));
			if (!preCreditsMessage.NullOrEmpty())
			{
				creds.Insert(1, new CreditRecord_Space(200f));
				creds.Insert(2, new CreditRecord_Text(preCreditsMessage));
				creds.Insert(3, new CreditRecord_Space(50f));
			}
			creds.Add(new CreditRecord_Space(300f));
			creds.Add(new CreditRecord_Text("ThanksForPlaying".Translate(), TextAnchor.UpperCenter));
		}

		public override void PreOpen()
		{
			base.PreOpen();
			creationRealtime = Time.realtimeSinceStartup;
			if (wonGame)
			{
				timeUntilAutoScroll = 6f;
			}
			else
			{
				timeUntilAutoScroll = 1f;
			}
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			if (timeUntilAutoScroll > 0f)
			{
				timeUntilAutoScroll -= Time.deltaTime;
			}
			else
			{
				scrollPosition += AutoScrollRate * Time.deltaTime;
			}
			if (wonGame && !playedMusic && Time.realtimeSinceStartup > creationRealtime + 5f)
			{
				Find.MusicManagerPlay.ForceStartSong(SongDefOf.EndCreditsSong, ignorePrefsVolume: true);
				playedMusic = true;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight);
			GUI.DrawTexture(rect, BaseContent.BlackTex);
			Rect position = new Rect(rect);
			position.yMin += 30f;
			position.yMax -= 30f;
			Vector2 center = rect.center;
			position.xMin = center.x - 400f;
			position.width = 800f;
			float viewWidth = ViewWidth;
			float viewHeight = ViewHeight;
			scrollPosition = Mathf.Clamp(scrollPosition, 0f, MaxScrollPosition);
			GUI.BeginGroup(position);
			Rect position2 = new Rect(0f, 0f, viewWidth, viewHeight);
			position2.y -= scrollPosition;
			GUI.BeginGroup(position2);
			Text.Font = GameFont.Medium;
			float num = 0f;
			foreach (CreditsEntry cred in creds)
			{
				float num2 = cred.DrawHeight(position2.width);
				Rect rect2 = new Rect(0f, num, position2.width, num2);
				cred.Draw(rect2);
				num += num2;
			}
			GUI.EndGroup();
			GUI.EndGroup();
			if (Event.current.type == EventType.ScrollWheel)
			{
				Vector2 delta = Event.current.delta;
				Scroll(delta.y * 25f);
				Event.current.Use();
			}
			if (Event.current.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == KeyCode.DownArrow)
				{
					Scroll(250f);
					Event.current.Use();
				}
				if (Event.current.keyCode == KeyCode.UpArrow)
				{
					Scroll(-250f);
					Event.current.Use();
				}
			}
		}

		private void Scroll(float offset)
		{
			scrollPosition += offset;
			timeUntilAutoScroll = 3f;
		}
	}
}
