using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.Planet
{
	[HasDebugOutput]
	public class PeaceTalks : WorldObject
	{
		private Material cachedMat;

		private static readonly SimpleCurve BadOutcomeChanceFactorByNegotiationAbility = new SimpleCurve
		{
			new CurvePoint(0f, 4f),
			new CurvePoint(1f, 1f),
			new CurvePoint(1.5f, 0.4f)
		};

		private const float BaseWeight_Disaster = 0.05f;

		private const float BaseWeight_Backfire = 0.1f;

		private const float BaseWeight_TalksFlounder = 0.2f;

		private const float BaseWeight_Success = 0.55f;

		private const float BaseWeight_Triumph = 0.1f;

		private static List<Pair<Action, float>> tmpPossibleOutcomes = new List<Pair<Action, float>>();

		public override Material Material
		{
			get
			{
				if (cachedMat == null)
				{
					cachedMat = MaterialPool.MatFrom(color: (base.Faction == null) ? Color.white : base.Faction.Color, texPath: def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.WorldObjectRenderQueue);
				}
				return cachedMat;
			}
		}

		public void Notify_CaravanArrived(Caravan caravan)
		{
			Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
			if (pawn == null)
			{
				Messages.Message("MessagePeaceTalksNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
			}
			else
			{
				float badOutcomeWeightFactor = GetBadOutcomeWeightFactor(pawn);
				float num = 1f / badOutcomeWeightFactor;
				tmpPossibleOutcomes.Clear();
				tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
				{
					Outcome_Disaster(caravan);
				}, 0.05f * badOutcomeWeightFactor));
				tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
				{
					Outcome_Backfire(caravan);
				}, 0.1f * badOutcomeWeightFactor));
				tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
				{
					Outcome_TalksFlounder(caravan);
				}, 0.2f));
				tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
				{
					Outcome_Success(caravan);
				}, 0.55f * num));
				tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
				{
					Outcome_Triumph(caravan);
				}, 0.1f * num));
				Action first = tmpPossibleOutcomes.RandomElementByWeight((Pair<Action, float> x) => x.Second).First;
				first();
				pawn.skills.Learn(SkillDefOf.Social, 6000f, direct: true);
				Find.WorldObjects.Remove(this);
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			using (IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(caravan).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption o = enumerator.Current;
					yield return o;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator2 = CaravanArrivalAction_VisitPeaceTalks.GetFloatMenuOptions(caravan, this).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					FloatMenuOption f = enumerator2.Current;
					yield return f;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0156:
			/*Error near IL_0157: Unexpected return in MoveNext()*/;
		}

		private void Outcome_Disaster(Caravan caravan)
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				FactionRelationKind playerRelationKind = base.Faction.PlayerRelationKind;
				int randomInRange = DiplomacyTuning.Goodwill_PeaceTalksDisasterRange.RandomInRange;
				base.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange, canSendMessage: false, canSendHostilityLetter: false);
				base.Faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, canSendLetter: false);
				IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
				incidentParms.faction = base.Faction;
				PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, ensureCanGenerateAtLeastOnePawn: true);
				defaultPawnGroupMakerParms.generateFightersOnly = true;
				List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
				Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, list, sendLetterIfRelatedPawns: false);
				if (list.Any())
				{
					LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(base.Faction), map, list);
				}
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				GlobalTargetInfo target = (!list.Any()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(list[0].Position, map);
				string letterLabel = "LetterLabelPeaceTalks_Disaster".Translate();
				string letterText = GetLetterText("LetterPeaceTalks_Disaster".Translate(base.Faction.def.pawnsPlural.CapitalizeFirst(), base.Faction.Name, Mathf.RoundToInt((float)randomInRange)), caravan, playerRelationKind);
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
				Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.ThreatBig, target, base.Faction);
			}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
		}

		private void Outcome_Backfire(Caravan caravan)
		{
			FactionRelationKind playerRelationKind = base.Faction.PlayerRelationKind;
			int randomInRange = DiplomacyTuning.Goodwill_PeaceTalksBackfireRange.RandomInRange;
			base.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange, canSendMessage: false, canSendHostilityLetter: false);
			Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Backfire".Translate(), GetLetterText("LetterPeaceTalks_Backfire".Translate(base.Faction.Name, randomInRange), caravan, playerRelationKind), LetterDefOf.NegativeEvent, caravan, base.Faction);
		}

		private void Outcome_TalksFlounder(Caravan caravan)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_TalksFlounder".Translate(), GetLetterText("LetterPeaceTalks_TalksFlounder".Translate(base.Faction.Name), caravan, base.Faction.PlayerRelationKind), LetterDefOf.NeutralEvent, caravan, base.Faction);
		}

		private void Outcome_Success(Caravan caravan)
		{
			FactionRelationKind playerRelationKind = base.Faction.PlayerRelationKind;
			int randomInRange = DiplomacyTuning.Goodwill_PeaceTalksSuccessRange.RandomInRange;
			base.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange, canSendMessage: false, canSendHostilityLetter: false);
			Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Success".Translate(), GetLetterText("LetterPeaceTalks_Success".Translate(base.Faction.Name, randomInRange), caravan, playerRelationKind), LetterDefOf.PositiveEvent, caravan, base.Faction);
		}

		private void Outcome_Triumph(Caravan caravan)
		{
			FactionRelationKind playerRelationKind = base.Faction.PlayerRelationKind;
			int randomInRange = DiplomacyTuning.Goodwill_PeaceTalksTriumphRange.RandomInRange;
			base.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange, canSendMessage: false, canSendHostilityLetter: false);
			List<Thing> list = ThingSetMakerDefOf.Reward_PeaceTalksGift.root.Generate();
			for (int i = 0; i < list.Count; i++)
			{
				caravan.AddPawnOrItem(list[i], addCarriedPawnToWorldPawnsIfAny: true);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Triumph".Translate(), GetLetterText("LetterPeaceTalks_Triumph".Translate(base.Faction.Name, randomInRange, GenLabel.ThingsLabel(list)), caravan, playerRelationKind), LetterDefOf.PositiveEvent, caravan, base.Faction);
		}

		private string GetLetterText(string baseText, Caravan caravan, FactionRelationKind previousRelationKind)
		{
			string text = baseText;
			Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
			if (pawn != null)
			{
				text = text + "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, 6000f.ToString("F0"), pawn.Named("PAWN"));
			}
			base.Faction.TryAppendRelationKindChangedInfo(ref text, previousRelationKind, base.Faction.PlayerRelationKind);
			return text;
		}

		private static float GetBadOutcomeWeightFactor(Pawn diplomat)
		{
			float statValue = diplomat.GetStatValue(StatDefOf.NegotiationAbility);
			return GetBadOutcomeWeightFactor(statValue);
		}

		private static float GetBadOutcomeWeightFactor(float negotationAbility)
		{
			return BadOutcomeChanceFactorByNegotiationAbility.Evaluate(negotationAbility);
		}

		[DebugOutput]
		[Category("Incidents")]
		private static void PeaceTalksChances()
		{
			StringBuilder stringBuilder = new StringBuilder();
			AppendDebugChances(stringBuilder, 0f);
			AppendDebugChances(stringBuilder, 1f);
			AppendDebugChances(stringBuilder, 1.5f);
			Log.Message(stringBuilder.ToString());
		}

		private static void AppendDebugChances(StringBuilder sb, float negotiationAbility)
		{
			if (sb.Length > 0)
			{
				sb.AppendLine();
			}
			sb.AppendLine("--- NegotiationAbility = " + negotiationAbility.ToStringPercent() + " ---");
			float badOutcomeWeightFactor = GetBadOutcomeWeightFactor(negotiationAbility);
			float num = 1f / badOutcomeWeightFactor;
			sb.AppendLine("Bad outcome weight factor: " + badOutcomeWeightFactor.ToString("0.##"));
			float num2 = 0.05f * badOutcomeWeightFactor;
			float num3 = 0.1f * badOutcomeWeightFactor;
			float num4 = 0.2f;
			float num5 = 0.55f * num;
			float num6 = 0.1f * num;
			float num7 = num2 + num3 + num4 + num5 + num6;
			sb.AppendLine("Disaster: " + (num2 / num7).ToStringPercent());
			sb.AppendLine("Backfire: " + (num3 / num7).ToStringPercent());
			sb.AppendLine("Talks flounder: " + (num4 / num7).ToStringPercent());
			sb.AppendLine("Success: " + (num5 / num7).ToStringPercent());
			sb.AppendLine("Triumph: " + (num6 / num7).ToStringPercent());
		}
	}
}
