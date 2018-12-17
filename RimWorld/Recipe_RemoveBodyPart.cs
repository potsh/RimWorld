using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	internal class Recipe_RemoveBodyPart : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts();
			using (IEnumerator<BodyPartRecord> enumerator = parts.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CGetPartsToApplyOn_003Ec__Iterator0 _003CGetPartsToApplyOn_003Ec__Iterator = (_003CGetPartsToApplyOn_003Ec__Iterator0)/*Error near IL_0088: stateMachine*/;
					BodyPartRecord part = enumerator.Current;
					if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
					{
						yield return part;
						/*Error: Unable to find new state assignment for yield return*/;
					}
					if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
					{
						yield return part;
						/*Error: Unable to find new state assignment for yield return*/;
					}
					if (part != pawn.RaceProps.body.corePart && part.def.canSuggestAmputation && pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part))
					{
						yield return part;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_020b:
			/*Error near IL_020c: Unexpected return in MoveNext()*/;
		}

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			if (pawn.Faction == billDoerFaction || pawn.Faction == null)
			{
				return false;
			}
			if (HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest)
			{
				return true;
			}
			return false;
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			bool flag = MedicalRecipesUtility.IsClean(pawn, part);
			bool flag2 = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
				if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
				MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position, billDoer.Map);
				MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
			}
			DamageDef surgicalCut = DamageDefOf.SurgicalCut;
			float amount = 99999f;
			float armorPenetration = 999f;
			pawn.TakeDamage(new DamageInfo(surgicalCut, amount, armorPenetration, -1f, null, part));
			if (flag)
			{
				if (pawn.Dead)
				{
					ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, PawnExecutionKind.OrganHarvesting);
				}
				ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn);
			}
			if (flag2 && pawn.Faction != null && billDoer != null && billDoer.Faction != null)
			{
				Faction faction = pawn.Faction;
				Faction faction2 = billDoer.Faction;
				int goodwillChange = -15;
				string reason = "GoodwillChangedReason_RemovedBodyPart".Translate(part.LabelShort);
				GlobalTargetInfo? lookTarget = pawn;
				faction.TryAffectGoodwillWith(faction2, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, reason, lookTarget);
			}
		}

		public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
		{
			if (!pawn.RaceProps.IsMechanoid && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
			{
				switch (HealthUtility.PartRemovalIntent(pawn, part))
				{
				case BodyPartRemovalIntent.Amputate:
					if (part.depth == BodyPartDepth.Inside || part.def.socketed)
					{
						return "RemoveOrgan".Translate();
					}
					return "Amputate".Translate();
				case BodyPartRemovalIntent.Harvest:
					return "HarvestOrgan".Translate();
				default:
					throw new InvalidOperationException();
				}
			}
			return RecipeDefOf.RemoveBodyPart.label;
		}
	}
}
