using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ITab_Pawn_Log_Utility
	{
		public class LogDrawData
		{
			public bool alternatingBackground;

			public LogEntry highlightEntry;

			public float highlightIntensity;

			public void StartNewDraw()
			{
				alternatingBackground = false;
			}
		}

		public abstract class LogLineDisplayable
		{
			private float cachedHeight = -1f;

			public float GetHeight(float width)
			{
				if (cachedHeight == -1f)
				{
					cachedHeight = GetHeight_Worker(width);
				}
				return cachedHeight;
			}

			public abstract float GetHeight_Worker(float width);

			public abstract void Draw(float position, float width, LogDrawData data);

			public abstract void AppendTo(StringBuilder sb);

			public virtual bool Matches(LogEntry log)
			{
				return false;
			}
		}

		public class LogLineDisplayableHeader : LogLineDisplayable
		{
			private string text;

			public LogLineDisplayableHeader(string text)
			{
				this.text = text;
			}

			public override float GetHeight_Worker(float width)
			{
				GameFont font = Text.Font;
				Text.Font = GameFont.Medium;
				float result = Text.CalcHeight(text, width);
				Text.Font = font;
				return result;
			}

			public override void Draw(float position, float width, LogDrawData data)
			{
				Text.Font = GameFont.Medium;
				Widgets.Label(new Rect(0f, position, width, GetHeight(width)), text);
				Text.Font = GameFont.Small;
			}

			public override void AppendTo(StringBuilder sb)
			{
				sb.AppendLine("--    " + text);
			}
		}

		public class LogLineDisplayableLog : LogLineDisplayable
		{
			private LogEntry log;

			private Pawn pawn;

			public LogLineDisplayableLog(LogEntry log, Pawn pawn)
			{
				this.log = log;
				this.pawn = pawn;
			}

			public override float GetHeight_Worker(float width)
			{
				float width2 = width - 29f;
				return Mathf.Max(26f, log.GetTextHeight(pawn, width2));
			}

			public override void Draw(float position, float width, LogDrawData data)
			{
				float height = GetHeight(width);
				float width2 = width - 29f;
				Rect rect = new Rect(0f, position, width, height);
				if (log == data.highlightEntry)
				{
					Widgets.DrawRectFast(rect, new Color(1f, 1f, 1f, HighlightAlpha * data.highlightIntensity));
					data.highlightIntensity = Mathf.Max(0f, data.highlightIntensity - Time.deltaTime / HighlightDuration);
				}
				else if (data.alternatingBackground)
				{
					Widgets.DrawRectFast(rect, new Color(1f, 1f, 1f, AlternateAlpha));
				}
				data.alternatingBackground = !data.alternatingBackground;
				Widgets.Label(new Rect(29f, position, width2, height), log.ToGameStringFromPOV(pawn));
				Texture2D texture2D = log.IconFromPOV(pawn);
				if (texture2D != null)
				{
					Rect position2 = new Rect(0f, position + (height - 26f) / 2f, 26f, 26f);
					GUI.DrawTexture(position2, texture2D);
				}
				Widgets.DrawHighlightIfMouseover(rect);
				TooltipHandler.TipRegion(rect, () => log.GetTipString(), 613261 + log.LogID * 2063);
				if (Widgets.ButtonInvisible(rect))
				{
					log.ClickedFromPOV(pawn);
				}
				if (DebugViewSettings.logCombatLogMouseover && Mouse.IsOver(rect))
				{
					log.ToGameStringFromPOV(pawn, forceLog: true);
				}
			}

			public override void AppendTo(StringBuilder sb)
			{
				sb.AppendLine(log.ToGameStringFromPOV(pawn));
			}

			public override bool Matches(LogEntry log)
			{
				return log == this.log;
			}
		}

		public class LogLineDisplayableGap : LogLineDisplayable
		{
			private float height;

			public LogLineDisplayableGap(float height)
			{
				this.height = height;
			}

			public override float GetHeight_Worker(float width)
			{
				return height;
			}

			public override void Draw(float position, float width, LogDrawData data)
			{
			}

			public override void AppendTo(StringBuilder sb)
			{
				sb.AppendLine();
			}
		}

		[TweakValue("Interface", 0f, 1f)]
		private static float AlternateAlpha = 0.03f;

		[TweakValue("Interface", 0f, 1f)]
		private static float HighlightAlpha = 0.2f;

		[TweakValue("Interface", 0f, 10f)]
		private static float HighlightDuration = 4f;

		[TweakValue("Interface", 0f, 30f)]
		private static float BattleBottomPadding = 20f;

		public static IEnumerable<LogLineDisplayable> GenerateLogLinesFor(Pawn pawn, bool showAll, bool showCombat, bool showSocial)
		{
			_003CGenerateLogLinesFor_003Ec__Iterator0 _003CGenerateLogLinesFor_003Ec__Iterator = (_003CGenerateLogLinesFor_003Ec__Iterator0)/*Error near IL_004c: stateMachine*/;
			LogEntry[] nonCombatLines = (!showSocial) ? new LogEntry[0] : (from e in Find.PlayLog.AllEntries
			where e.Concerns(pawn)
			select e).ToArray();
			int nonCombatIndex = 0;
			Battle currentBattle = null;
			if (showCombat)
			{
				bool atTop = true;
				foreach (Battle battle in Find.BattleLog.Battles)
				{
					if (battle.Concerns(pawn))
					{
						foreach (LogEntry entry in battle.Entries)
						{
							if (entry.Concerns(pawn) && (showAll || entry.ShowInCompactView()))
							{
								if (nonCombatIndex < nonCombatLines.Length && nonCombatLines[nonCombatIndex].Age < entry.Age)
								{
									if (currentBattle != null && currentBattle != battle)
									{
										yield return (LogLineDisplayable)new LogLineDisplayableGap(BattleBottomPadding);
										/*Error: Unable to find new state assignment for yield return*/;
									}
									LogEntry[] array = nonCombatLines;
									int num;
									nonCombatIndex = (num = nonCombatIndex) + 1;
									yield return (LogLineDisplayable)new LogLineDisplayableLog(array[num], pawn);
									/*Error: Unable to find new state assignment for yield return*/;
								}
								if (currentBattle == battle)
								{
									yield return (LogLineDisplayable)new LogLineDisplayableLog(entry, pawn);
									/*Error: Unable to find new state assignment for yield return*/;
								}
								if (atTop)
								{
									yield return (LogLineDisplayable)new LogLineDisplayableHeader(battle.GetName());
									/*Error: Unable to find new state assignment for yield return*/;
								}
								yield return (LogLineDisplayable)new LogLineDisplayableGap(BattleBottomPadding);
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
			}
			if (nonCombatIndex < nonCombatLines.Length)
			{
				if (currentBattle == null)
				{
					yield return (LogLineDisplayable)new LogLineDisplayableLog(nonCombatLines[nonCombatIndex], pawn);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield return (LogLineDisplayable)new LogLineDisplayableGap(BattleBottomPadding);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0424:
			/*Error near IL_0425: Unexpected return in MoveNext()*/;
		}
	}
}
