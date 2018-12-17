using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class PawnDiedOrDownedThoughtsUtility
	{
		private static List<IndividualThoughtToAdd> tmpIndividualThoughtsToAdd = new List<IndividualThoughtToAdd>();

		private static List<ThoughtDef> tmpAllColonistsThoughts = new List<ThoughtDef>();

		public static void TryGiveThoughts(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind)
		{
			try
			{
				if (!PawnGenerator.IsBeingGenerated(victim) && Current.ProgramState == ProgramState.Playing)
				{
					GetThoughts(victim, dinfo, thoughtsKind, tmpIndividualThoughtsToAdd, tmpAllColonistsThoughts);
					for (int i = 0; i < tmpIndividualThoughtsToAdd.Count; i++)
					{
						tmpIndividualThoughtsToAdd[i].Add();
					}
					if (tmpAllColonistsThoughts.Any())
					{
						foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
						{
							if (allMapsCaravansAndTravelingTransportPods_Alive_Colonist != victim)
							{
								for (int j = 0; j < tmpAllColonistsThoughts.Count; j++)
								{
									ThoughtDef def = tmpAllColonistsThoughts[j];
									allMapsCaravansAndTravelingTransportPods_Alive_Colonist.needs.mood.thoughts.memories.TryGainMemory(def);
								}
							}
						}
					}
					tmpIndividualThoughtsToAdd.Clear();
					tmpAllColonistsThoughts.Clear();
				}
			}
			catch (Exception arg)
			{
				Log.Error("Could not give thoughts: " + arg);
			}
		}

		public static void TryGiveThoughts(IEnumerable<Pawn> victims, PawnDiedOrDownedThoughtsKind thoughtsKind)
		{
			foreach (Pawn victim in victims)
			{
				TryGiveThoughts(victim, null, thoughtsKind);
			}
		}

		public static void GetThoughts(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			outIndividualThoughts.Clear();
			outAllColonistsThoughts.Clear();
			if (victim.RaceProps.Humanlike)
			{
				AppendThoughts_ForHumanlike(victim, dinfo, thoughtsKind, outIndividualThoughts, outAllColonistsThoughts);
			}
			if (victim.relations != null && victim.relations.everSeenByPlayer)
			{
				AppendThoughts_Relations(victim, dinfo, thoughtsKind, outIndividualThoughts, outAllColonistsThoughts);
			}
		}

		public static void BuildMoodThoughtsListString(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, StringBuilder sb, string individualThoughtsHeader, string allColonistsThoughtsHeader)
		{
			GetThoughts(victim, dinfo, thoughtsKind, tmpIndividualThoughtsToAdd, tmpAllColonistsThoughts);
			if (tmpAllColonistsThoughts.Any())
			{
				if (!allColonistsThoughtsHeader.NullOrEmpty())
				{
					sb.Append(allColonistsThoughtsHeader);
					sb.AppendLine();
				}
				for (int i = 0; i < tmpAllColonistsThoughts.Count; i++)
				{
					ThoughtDef thoughtDef = tmpAllColonistsThoughts[i];
					if (sb.Length > 0)
					{
						sb.AppendLine();
					}
					sb.Append("  - " + thoughtDef.stages[0].label.CapitalizeFirst() + " " + Mathf.RoundToInt(thoughtDef.stages[0].baseMoodEffect).ToStringWithSign());
				}
			}
			if (tmpIndividualThoughtsToAdd.Any((IndividualThoughtToAdd x) => x.thought.MoodOffset() != 0f))
			{
				if (!individualThoughtsHeader.NullOrEmpty())
				{
					sb.Append(individualThoughtsHeader);
				}
				foreach (IGrouping<Pawn, IndividualThoughtToAdd> item in from x in tmpIndividualThoughtsToAdd
				where x.thought.MoodOffset() != 0f
				group x by x.addTo)
				{
					if (sb.Length > 0)
					{
						sb.AppendLine();
						sb.AppendLine();
					}
					string value = item.Key.KindLabel.CapitalizeFirst() + " " + item.Key.LabelShort;
					sb.Append(value);
					sb.Append(":");
					foreach (IndividualThoughtToAdd item2 in item)
					{
						sb.AppendLine();
						sb.Append("    " + item2.LabelCap);
					}
				}
			}
		}

		public static void BuildMoodThoughtsListString(IEnumerable<Pawn> victims, PawnDiedOrDownedThoughtsKind thoughtsKind, StringBuilder sb, string individualThoughtsHeader, string allColonistsThoughtsHeader, string victimLabelKey)
		{
			foreach (Pawn victim in victims)
			{
				GetThoughts(victim, null, thoughtsKind, tmpIndividualThoughtsToAdd, tmpAllColonistsThoughts);
				if (tmpIndividualThoughtsToAdd.Any() || tmpAllColonistsThoughts.Any())
				{
					if (sb.Length > 0)
					{
						sb.AppendLine();
						sb.AppendLine();
					}
					string text = victim.KindLabel.CapitalizeFirst() + " " + victim.LabelShort;
					if (victimLabelKey.NullOrEmpty())
					{
						sb.Append(text + ":");
					}
					else
					{
						sb.Append(victimLabelKey.Translate(text));
					}
					BuildMoodThoughtsListString(victim, null, thoughtsKind, sb, individualThoughtsHeader, allColonistsThoughtsHeader);
				}
			}
		}

		private static void AppendThoughts_ForHumanlike(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			bool flag = dinfo.HasValue && dinfo.Value.Def.execution;
			bool flag2 = victim.IsPrisonerOfColony && !victim.guilt.IsGuilty && !victim.InAggroMentalState;
			if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(victim) && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn)
			{
				Pawn pawn = (Pawn)dinfo.Value.Instigator;
				if (!pawn.Dead && pawn.needs.mood != null && pawn.story != null && pawn != victim)
				{
					if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledHumanlikeBloodlust, pawn));
					}
					if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.HostileTo(pawn) && victim.Faction != null && PawnUtility.IsFactionLeader(victim) && victim.Faction.HostileTo(pawn.Faction))
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.DefeatedHostileFactionLeader, pawn, victim));
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && !flag)
			{
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (item != victim && item.needs != null && item.needs.mood != null && (item.MentalStateDef != MentalStateDefOf.SocialFighting || ((MentalState_SocialFighting)item.MentalState).otherPawn != victim))
					{
						if (Witnessed(item, victim))
						{
							if (item.Faction == victim.Faction)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathAlly, item));
							}
							else if (victim.Faction == null || !victim.Faction.HostileTo(item.Faction))
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathNonAlly, item));
							}
							if (item.relations.FamilyByBlood.Contains(victim))
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathFamily, item));
							}
							outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathBloodlust, item));
						}
						else if (victim.Faction == Faction.OfPlayer && victim.Faction == item.Faction && victim.HostFaction != item.Faction)
						{
							outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowColonistDied, item, victim));
						}
						if (flag2 && item.Faction == Faction.OfPlayer && !item.IsPrisoner)
						{
							outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowPrisonerDiedInnocent, item, victim));
						}
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Banished && victim.IsColonist)
			{
				outAllColonistsThoughts.Add(ThoughtDefOf.ColonistBanished);
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.BanishedToDie)
			{
				if (victim.IsColonist)
				{
					outAllColonistsThoughts.Add(ThoughtDefOf.ColonistBanishedToDie);
				}
				else if (victim.IsPrisonerOfColony)
				{
					outAllColonistsThoughts.Add(ThoughtDefOf.PrisonerBanishedToDie);
				}
			}
		}

		private static void AppendThoughts_Relations(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Banished && victim.RaceProps.Animal)
			{
				List<DirectPawnRelation> directRelations = victim.relations.DirectRelations;
				for (int i = 0; i < directRelations.Count; i++)
				{
					if (directRelations[i].otherPawn.needs != null && directRelations[i].otherPawn.needs.mood != null && PawnUtility.ShouldGetThoughtAbout(directRelations[i].otherPawn, victim) && directRelations[i].def == PawnRelationDefOf.Bond)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.BondedAnimalBanished, directRelations[i].otherPawn, victim));
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died || thoughtsKind == PawnDiedOrDownedThoughtsKind.BanishedToDie)
			{
				foreach (Pawn potentiallyRelatedPawn in victim.relations.PotentiallyRelatedPawns)
				{
					if (potentiallyRelatedPawn.needs != null && potentiallyRelatedPawn.needs.mood != null && PawnUtility.ShouldGetThoughtAbout(potentiallyRelatedPawn, victim))
					{
						PawnRelationDef mostImportantRelation = potentiallyRelatedPawn.GetMostImportantRelation(victim);
						if (mostImportantRelation != null)
						{
							ThoughtDef genderSpecificDiedThought = mostImportantRelation.GetGenderSpecificDiedThought(victim);
							if (genderSpecificDiedThought != null)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(genderSpecificDiedThought, potentiallyRelatedPawn, victim));
							}
						}
					}
				}
				if (dinfo.HasValue)
				{
					Pawn pawn = dinfo.Value.Instigator as Pawn;
					if (pawn != null && pawn != victim)
					{
						foreach (Pawn potentiallyRelatedPawn2 in victim.relations.PotentiallyRelatedPawns)
						{
							if (pawn != potentiallyRelatedPawn2 && potentiallyRelatedPawn2.needs != null && potentiallyRelatedPawn2.needs.mood != null)
							{
								PawnRelationDef mostImportantRelation2 = potentiallyRelatedPawn2.GetMostImportantRelation(victim);
								if (mostImportantRelation2 != null)
								{
									ThoughtDef genderSpecificKilledThought = mostImportantRelation2.GetGenderSpecificKilledThought(victim);
									if (genderSpecificKilledThought != null)
									{
										outIndividualThoughts.Add(new IndividualThoughtToAdd(genderSpecificKilledThought, potentiallyRelatedPawn2, pawn));
									}
								}
								if (potentiallyRelatedPawn2.RaceProps.IsFlesh)
								{
									int num = potentiallyRelatedPawn2.relations.OpinionOf(victim);
									if (num >= 20)
									{
										ThoughtDef killedMyFriend = ThoughtDefOf.KilledMyFriend;
										Pawn addTo = potentiallyRelatedPawn2;
										Pawn otherPawn = pawn;
										float friendDiedThoughtPowerFactor = victim.relations.GetFriendDiedThoughtPowerFactor(num);
										outIndividualThoughts.Add(new IndividualThoughtToAdd(killedMyFriend, addTo, otherPawn, 1f, friendDiedThoughtPowerFactor));
									}
									else if (num <= -20)
									{
										ThoughtDef killedMyFriend = ThoughtDefOf.KilledMyRival;
										Pawn otherPawn = potentiallyRelatedPawn2;
										Pawn addTo = pawn;
										float friendDiedThoughtPowerFactor = victim.relations.GetRivalDiedThoughtPowerFactor(num);
										outIndividualThoughts.Add(new IndividualThoughtToAdd(killedMyFriend, otherPawn, addTo, 1f, friendDiedThoughtPowerFactor));
									}
								}
							}
						}
					}
				}
				if (victim.RaceProps.Humanlike)
				{
					foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
					{
						if (item.needs != null && item.RaceProps.IsFlesh && item.needs.mood != null && PawnUtility.ShouldGetThoughtAbout(item, victim))
						{
							int num2 = item.relations.OpinionOf(victim);
							if (num2 >= 20)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.PawnWithGoodOpinionDied, item, victim, victim.relations.GetFriendDiedThoughtPowerFactor(num2)));
							}
							else if (num2 <= -20)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.PawnWithBadOpinionDied, item, victim, victim.relations.GetRivalDiedThoughtPowerFactor(num2)));
							}
						}
					}
				}
			}
		}

		private static bool Witnessed(Pawn p, Pawn victim)
		{
			if (!p.Awake() || !p.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				return false;
			}
			if (victim.IsCaravanMember())
			{
				return victim.GetCaravan() == p.GetCaravan();
			}
			if (!victim.Spawned || !p.Spawned)
			{
				return false;
			}
			if (!p.Position.InHorDistOf(victim.Position, 12f))
			{
				return false;
			}
			if (!GenSight.LineOfSight(victim.Position, p.Position, victim.Map))
			{
				return false;
			}
			return true;
		}

		public static void RemoveDiedThoughts(Pawn pawn)
		{
			foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive)
			{
				if (item.needs != null && item.needs.mood != null && item != pawn)
				{
					MemoryThoughtHandler memories = item.needs.mood.thoughts.memories;
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.KnowColonistDied, pawn);
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.KnowPrisonerDiedInnocent, pawn);
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.PawnWithGoodOpinionDied, pawn);
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.PawnWithBadOpinionDied, pawn);
					List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						ThoughtDef genderSpecificDiedThought = allDefsListForReading[i].GetGenderSpecificDiedThought(pawn);
						if (genderSpecificDiedThought != null)
						{
							memories.RemoveMemoriesOfDefWhereOtherPawnIs(genderSpecificDiedThought, pawn);
						}
					}
				}
			}
		}
	}
}
