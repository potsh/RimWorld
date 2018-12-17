using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ITab_Pawn_Visitor : ITab
	{
		private const float CheckboxInterval = 30f;

		private const float CheckboxMargin = 50f;

		public ITab_Pawn_Visitor()
		{
			size = new Vector2(280f, 0f);
		}

		protected override void FillTab()
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.PrisonerTab, KnowledgeAmount.FrameDisplayed);
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, size.x, size.y);
			Rect rect2 = rect.ContractedBy(10f);
			rect2.yMin += 24f;
			bool isPrisonerOfColony = base.SelPawn.IsPrisonerOfColony;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.maxOneColumn = true;
			listing_Standard.Begin(rect2);
			Rect rect3 = listing_Standard.GetRect(28f);
			rect3.width = 140f;
			MedicalCareUtility.MedicalCareSetter(rect3, ref base.SelPawn.playerSettings.medCare);
			listing_Standard.Gap(4f);
			if (isPrisonerOfColony)
			{
				listing_Standard.Label("RecruitmentDifficulty".Translate() + ": " + base.SelPawn.RecruitDifficulty(Faction.OfPlayer).ToStringPercent());
				listing_Standard.Label("RecruitmentResistance".Translate() + ": " + base.SelPawn.guest.resistance.ToString("F1"));
				if (base.SelPawn.guilt.IsGuilty)
				{
					listing_Standard.Label("ConsideredGuilty".Translate(base.SelPawn.guilt.TicksUntilInnocent.ToStringTicksToPeriod()));
				}
				Rect rect4 = listing_Standard.GetRect(160f).Rounded();
				Widgets.DrawMenuSection(rect4);
				Rect position = rect4.ContractedBy(10f);
				GUI.BeginGroup(position);
				Rect rect5 = new Rect(0f, 0f, position.width, 30f);
				foreach (PrisonerInteractionModeDef item in from pim in DefDatabase<PrisonerInteractionModeDef>.AllDefs
				orderby pim.listOrder
				select pim)
				{
					if (Widgets.RadioButtonLabeled(rect5, item.LabelCap, base.SelPawn.guest.interactionMode == item))
					{
						base.SelPawn.guest.interactionMode = item;
						if (item == PrisonerInteractionModeDefOf.Execution && base.SelPawn.MapHeld != null && !ColonyHasAnyWardenCapableOfViolence(base.SelPawn.MapHeld))
						{
							Messages.Message("MessageCantDoExecutionBecauseNoWardenCapableOfViolence".Translate(), base.SelPawn, MessageTypeDefOf.CautionInput, historical: false);
						}
					}
					rect5.y += 28f;
				}
				GUI.EndGroup();
				if (Prefs.DevMode)
				{
					listing_Standard.Label("Dev: Prison break MTB days: " + (int)PrisonBreakUtility.InitiatePrisonBreakMtbDays(base.SelPawn));
				}
			}
			listing_Standard.End();
			size = new Vector2(280f, listing_Standard.CurHeight + 20f + 24f);
		}

		private bool ColonyHasAnyWardenCapableOfViolence(Map map)
		{
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.workSettings.WorkIsActive(WorkTypeDefOf.Warden) && (item.story == null || !item.story.WorkTagIsDisabled(WorkTags.Violent)))
				{
					return true;
				}
			}
			return false;
		}
	}
}
