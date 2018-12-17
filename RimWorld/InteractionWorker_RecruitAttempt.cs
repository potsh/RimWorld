using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_RecruitAttempt : InteractionWorker
	{
		private const float BaseResistanceReductionPerInteraction = 1f;

		private static readonly SimpleCurve ResistanceImpactFactorCurve_Mood = new SimpleCurve
		{
			new CurvePoint(0f, 0.2f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(1f, 1.5f)
		};

		private static readonly SimpleCurve ResistanceImpactFactorCurve_Opinion = new SimpleCurve
		{
			new CurvePoint(-100f, 0.5f),
			new CurvePoint(0f, 1f),
			new CurvePoint(100f, 1.5f)
		};

		private const float RecruitChancePerNegotiatingAbility = 0.5f;

		private const float MaxMoodForWarning = 0.4f;

		private static readonly SimpleCurve RecruitChanceFactorCurve_Mood = new SimpleCurve
		{
			new CurvePoint(0f, 0.2f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(1f, 2f)
		};

		private const float MaxOpinionForWarning = -0.01f;

		private static readonly SimpleCurve RecruitChanceFactorCurve_Opinion = new SimpleCurve
		{
			new CurvePoint(-100f, 0.5f),
			new CurvePoint(0f, 1f),
			new CurvePoint(100f, 2f)
		};

		private static readonly SimpleCurve RecruitChanceFactorCurve_RecruitDifficulty = new SimpleCurve
		{
			new CurvePoint(0f, 2f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(1f, 0.02f)
		};

		private const float WildmanWildness = 0.2f;

		private static readonly SimpleCurve TameChanceFactorCurve_Wildness = new SimpleCurve
		{
			new CurvePoint(1f, 0f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(0f, 2f)
		};

		private const float TameChanceFactor_Bonded = 4f;

		private const float ChanceToDevelopBondRelationOnTamed = 0.01f;

		private const int MenagerieTaleThreshold = 5;

		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef)
		{
			letterText = null;
			letterLabel = null;
			letterDef = null;
			if (!recipient.mindState.CheckStartMentalStateBecauseRecruitAttempted(initiator))
			{
				bool flag = recipient.AnimalOrWildMan() && !recipient.IsPrisoner;
				float x = (float)((recipient.relations != null) ? recipient.relations.OpinionOf(initiator) : 0);
				bool flag2 = initiator.InspirationDef == InspirationDefOf.Inspired_Recruitment && !flag && recipient.guest.interactionMode != PrisonerInteractionModeDefOf.ReduceResistance;
				if (DebugSettings.instantRecruit)
				{
					recipient.guest.resistance = 0f;
				}
				if (!flag && recipient.guest.resistance > 0f && !flag2)
				{
					float num = 1f;
					num *= initiator.GetStatValue(StatDefOf.NegotiationAbility);
					num *= ResistanceImpactFactorCurve_Mood.Evaluate(recipient.needs.mood.CurLevelPercentage);
					num *= ResistanceImpactFactorCurve_Opinion.Evaluate(x);
					num = Mathf.Min(num, recipient.guest.resistance);
					float resistance = recipient.guest.resistance;
					recipient.guest.resistance = Mathf.Max(0f, recipient.guest.resistance - num);
					string text = "TextMote_ResistanceReduced".Translate(resistance.ToString("F1"), recipient.guest.resistance.ToString("F1"));
					if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
					{
						text = text + "\n(" + "lowMood".Translate() + ")";
					}
					if (recipient.relations != null && (float)recipient.relations.OpinionOf(initiator) < -0.01f)
					{
						text = text + "\n(" + "lowOpinion".Translate() + ")";
					}
					MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);
					if (recipient.guest.resistance == 0f)
					{
						string text2 = "MessagePrisonerResistanceBroken".Translate(recipient.LabelShort, initiator.LabelShort, initiator.Named("WARDEN"), recipient.Named("PRISONER"));
						if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
						{
							text2 = text2 + " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
						}
						Messages.Message(text2, recipient, MessageTypeDefOf.PositiveEvent);
					}
				}
				else
				{
					float statValue;
					if (flag)
					{
						statValue = initiator.GetStatValue(StatDefOf.TameAnimalChance);
						float x2 = (!recipient.IsWildMan()) ? recipient.RaceProps.wildness : 0.2f;
						statValue *= TameChanceFactorCurve_Wildness.Evaluate(x2);
						if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Bond, recipient))
						{
							statValue *= 4f;
						}
					}
					else if (flag2 || DebugSettings.instantRecruit)
					{
						statValue = 1f;
					}
					else
					{
						statValue = initiator.GetStatValue(StatDefOf.NegotiationAbility) * 0.5f;
						float x3 = recipient.RecruitDifficulty(initiator.Faction);
						statValue *= RecruitChanceFactorCurve_RecruitDifficulty.Evaluate(x3);
						statValue *= RecruitChanceFactorCurve_Opinion.Evaluate(x);
						if (recipient.needs.mood != null)
						{
							float curLevel = recipient.needs.mood.CurLevel;
							statValue *= RecruitChanceFactorCurve_Mood.Evaluate(curLevel);
						}
					}
					if (Rand.Chance(statValue))
					{
						DoRecruit(initiator, recipient, statValue, out letterLabel, out letterText, useAudiovisualEffects: true, sendLetter: false);
						if (!letterLabel.NullOrEmpty())
						{
							letterDef = LetterDefOf.PositiveEvent;
						}
						if (flag2)
						{
							initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Recruitment);
						}
						extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
					}
					else
					{
						string text3 = (!flag) ? "TextMote_RecruitFail".Translate(statValue.ToStringPercent()) : "TextMote_TameFail".Translate(statValue.ToStringPercent());
						if (!flag)
						{
							if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
							{
								text3 = text3 + "\n(" + "lowMood".Translate() + ")";
							}
							if (recipient.relations != null && (float)recipient.relations.OpinionOf(initiator) < -0.01f)
							{
								text3 = text3 + "\n(" + "lowOpinion".Translate() + ")";
							}
						}
						MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text3, 8f);
						extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
					}
				}
			}
		}

		public static void DoRecruit(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
		{
			DoRecruit(recruiter, recruitee, recruitChance, out string _, out string _, useAudiovisualEffects);
		}

		public static void DoRecruit(Pawn recruiter, Pawn recruitee, float recruitChance, out string letterLabel, out string letter, bool useAudiovisualEffects = true, bool sendLetter = true)
		{
			letterLabel = null;
			letter = null;
			recruitChance = Mathf.Clamp01(recruitChance);
			string value = recruitee.LabelIndefinite();
			if (recruitee.guest != null)
			{
				recruitee.guest.SetGuestStatus(null);
			}
			bool flag = recruitee.Name != null;
			if (recruitee.Faction != recruiter.Faction)
			{
				recruitee.SetFaction(recruiter.Faction, recruiter);
			}
			if (recruitee.RaceProps.Humanlike)
			{
				if (useAudiovisualEffects)
				{
					letterLabel = "LetterLabelMessageRecruitSuccess".Translate();
					if (sendLetter)
					{
						Find.LetterStack.ReceiveLetter(letterLabel, "MessageRecruitSuccess".Translate(recruiter, recruitee, recruitChance.ToStringPercent(), recruiter.Named("RECRUITER"), recruitee.Named("RECRUITEE")), LetterDefOf.PositiveEvent, recruitee);
					}
				}
				TaleRecorder.RecordTale(TaleDefOf.Recruited, recruiter, recruitee);
				recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
				recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RecruitedMe, recruiter);
			}
			else
			{
				if (useAudiovisualEffects)
				{
					if (!flag)
					{
						Messages.Message("MessageTameAndNameSuccess".Translate(recruiter.LabelShort, value, recruitChance.ToStringPercent(), recruitee.Name.ToStringFull, recruiter.Named("RECRUITER"), recruitee.Named("RECRUITEE")).AdjustedFor(recruitee), recruitee, MessageTypeDefOf.PositiveEvent);
					}
					else
					{
						Messages.Message("MessageTameSuccess".Translate(recruiter.LabelShort, value, recruitChance.ToStringPercent(), recruiter.Named("RECRUITER")), recruitee, MessageTypeDefOf.PositiveEvent);
					}
					if (recruiter.Spawned && recruitee.Spawned)
					{
						MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruiter.Map, "TextMote_TameSuccess".Translate(recruitChance.ToStringPercent()), 8f);
					}
				}
				recruiter.records.Increment(RecordDefOf.AnimalsTamed);
				RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.01f);
				float chance = Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness);
				if (Rand.Chance(chance) || recruitee.IsWildMan())
				{
					TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, recruiter, recruitee);
				}
				if (PawnsFinder.AllMapsWorldAndTemporary_Alive.Count((Pawn p) => p.playerSettings != null && p.playerSettings.Master == recruiter) >= 5)
				{
					TaleRecorder.RecordTale(TaleDefOf.IncreasedMenagerie, recruiter, recruitee);
				}
			}
			if (recruitee.caller != null)
			{
				recruitee.caller.DoCall();
			}
		}
	}
}
