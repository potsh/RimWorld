using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse.AI.Group
{
	public sealed class LordManager : IExposable
	{
		public Map map;

		public List<Lord> lords = new List<Lord>();

		public LordManager(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref lords, "lords", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < lords.Count; i++)
				{
					lords[i].lordManager = this;
				}
			}
		}

		public void LordManagerTick()
		{
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].LordTick();
			}
			for (int num = lords.Count - 1; num >= 0; num--)
			{
				LordToil curLordToil = lords[num].CurLordToil;
				if (curLordToil.ShouldFail)
				{
					RemoveLord(lords[num]);
				}
			}
		}

		public void LordManagerUpdate()
		{
			if (DebugViewSettings.drawLords)
			{
				for (int i = 0; i < lords.Count; i++)
				{
					lords[i].DebugDraw();
				}
			}
		}

		public void LordManagerOnGUI()
		{
			if (DebugViewSettings.drawLords)
			{
				for (int i = 0; i < lords.Count; i++)
				{
					lords[i].DebugOnGUI();
				}
			}
			if (DebugViewSettings.drawDuties)
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.Font = GameFont.Tiny;
				foreach (Pawn allPawn in map.mapPawns.AllPawns)
				{
					if (allPawn.Spawned)
					{
						string text = string.Empty;
						if (!allPawn.Dead && allPawn.mindState.duty != null)
						{
							text = allPawn.mindState.duty.ToString();
						}
						if (allPawn.InMentalState)
						{
							text = text + "\nMentalState=" + allPawn.MentalState.ToString();
						}
						Vector2 vector = allPawn.DrawPos.MapToUIPosition();
						Widgets.Label(new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f), text);
					}
				}
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}

		public void AddLord(Lord newLord)
		{
			lords.Add(newLord);
			newLord.lordManager = this;
		}

		public void RemoveLord(Lord oldLord)
		{
			lords.Remove(oldLord);
			oldLord.Cleanup();
		}

		public Lord LordOf(Pawn p)
		{
			for (int i = 0; i < lords.Count; i++)
			{
				Lord lord = lords[i];
				for (int j = 0; j < lord.ownedPawns.Count; j++)
				{
					if (lord.ownedPawns[j] == p)
					{
						return lord;
					}
				}
			}
			return null;
		}

		public void LogLords()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= Lords =======");
			stringBuilder.AppendLine("Count: " + lords.Count);
			for (int i = 0; i < lords.Count; i++)
			{
				Lord lord = lords[i];
				stringBuilder.AppendLine();
				stringBuilder.Append("#" + (i + 1) + ": ");
				if (lord.LordJob == null)
				{
					stringBuilder.AppendLine("no-job");
				}
				else
				{
					stringBuilder.AppendLine(lord.LordJob.GetType().Name);
				}
				stringBuilder.Append("Current toil: ");
				if (lord.CurLordToil == null)
				{
					stringBuilder.AppendLine("null");
				}
				else
				{
					stringBuilder.AppendLine(lord.CurLordToil.GetType().Name);
				}
				stringBuilder.AppendLine("Members (count: " + lord.ownedPawns.Count + "):");
				for (int j = 0; j < lord.ownedPawns.Count; j++)
				{
					stringBuilder.AppendLine("  " + lord.ownedPawns[j].LabelShort + " (" + lord.ownedPawns[j].Faction + ")");
				}
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
