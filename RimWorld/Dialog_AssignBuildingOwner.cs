using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_AssignBuildingOwner : Window
	{
		private IAssignableBuilding assignable;

		private Vector2 scrollPosition;

		private const float EntryHeight = 35f;

		public override Vector2 InitialSize => new Vector2(620f, 500f);

		public Dialog_AssignBuildingOwner(IAssignableBuilding assignable)
		{
			this.assignable = assignable;
			doCloseButton = true;
			doCloseX = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(inRect);
			outRect.yMin += 20f;
			outRect.yMax -= 40f;
			outRect.width -= 16f;
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)assignable.AssigningCandidates.Count() * 35f + 100f);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			try
			{
				float num = 0f;
				bool flag = false;
				foreach (Pawn assignedPawn in assignable.AssignedPawns)
				{
					flag = true;
					Rect rect = new Rect(0f, num, viewRect.width * 0.6f, 32f);
					Widgets.Label(rect, assignedPawn.LabelCap);
					rect.x = rect.xMax;
					rect.width = viewRect.width * 0.4f;
					if (Widgets.ButtonText(rect, "BuildingUnassign".Translate()))
					{
						assignable.TryUnassignPawn(assignedPawn);
						SoundDefOf.Click.PlayOneShotOnCamera();
						return;
					}
					num += 35f;
				}
				if (flag)
				{
					num += 15f;
				}
				foreach (Pawn assigningCandidate in assignable.AssigningCandidates)
				{
					if (!assignable.AssignedPawns.Contains(assigningCandidate))
					{
						Rect rect2 = new Rect(0f, num, viewRect.width * 0.6f, 32f);
						Widgets.Label(rect2, assigningCandidate.LabelCap);
						rect2.x = rect2.xMax;
						rect2.width = viewRect.width * 0.4f;
						string label = (!assignable.AssignedAnything(assigningCandidate)) ? "BuildingAssign".Translate() : "BuildingReassign".Translate();
						if (Widgets.ButtonText(rect2, label))
						{
							assignable.TryAssignPawn(assigningCandidate);
							if (assignable.MaxAssignedPawnsCount == 1)
							{
								Close();
							}
							else
							{
								SoundDefOf.Click.PlayOneShotOnCamera();
							}
							break;
						}
						num += 35f;
					}
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}
	}
}
