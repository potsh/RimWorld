using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_RelationsTracker : IExposable
	{
		private Pawn pawn;

		private List<DirectPawnRelation> directRelations = new List<DirectPawnRelation>();

		public bool everSeenByPlayer;

		public bool canGetRescuedThought = true;

		public Pawn relativeInvolvedInRescueQuest;

		private HashSet<Pawn> pawnsWithDirectRelationsWithMe = new HashSet<Pawn>();

		private List<Pawn> cachedFamilyByBlood = new List<Pawn>();

		private bool familyByBloodIsCached;

		private bool canCacheFamilyByBlood;

		private const int CheckDevelopBondRelationIntervalTicks = 2500;

		private const float MaxBondRelationCheckDist = 12f;

		private const float BondRelationPerIntervalChance = 0.001f;

		public const int FriendOpinionThreshold = 20;

		public const int RivalOpinionThreshold = -20;

		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();

		public List<DirectPawnRelation> DirectRelations => directRelations;

		public IEnumerable<Pawn> Children
		{
			get
			{
				foreach (Pawn item in pawnsWithDirectRelationsWithMe)
				{
					List<DirectPawnRelation> hisDirectRels = item.relations.directRelations;
					for (int i = 0; i < hisDirectRels.Count; i++)
					{
						if (hisDirectRels[i].otherPawn == pawn && hisDirectRels[i].def == PawnRelationDefOf.Parent)
						{
							yield return item;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				yield break;
				IL_0145:
				/*Error near IL_0146: Unexpected return in MoveNext()*/;
			}
		}

		public int ChildrenCount => Children.Count();

		public bool RelatedToAnyoneOrAnyoneRelatedToMe => directRelations.Any() || pawnsWithDirectRelationsWithMe.Any();

		public IEnumerable<Pawn> FamilyByBlood
		{
			get
			{
				if (canCacheFamilyByBlood)
				{
					if (!familyByBloodIsCached)
					{
						cachedFamilyByBlood.Clear();
						cachedFamilyByBlood.AddRange(FamilyByBlood_Internal);
						familyByBloodIsCached = true;
					}
					return cachedFamilyByBlood;
				}
				return FamilyByBlood_Internal;
			}
		}

		private IEnumerable<Pawn> FamilyByBlood_Internal
		{
			get
			{
				if (RelatedToAnyoneOrAnyoneRelatedToMe)
				{
					try
					{
						List<Pawn> familyStack = SimplePool<List<Pawn>>.Get();
						List<Pawn> familyChildrenStack = SimplePool<List<Pawn>>.Get();
						HashSet<Pawn> familyVisited = SimplePool<HashSet<Pawn>>.Get();
						familyStack.Add(pawn);
						familyVisited.Add(pawn);
						Pawn p;
						while (true)
						{
							if (!familyStack.Any())
							{
								yield break;
							}
							p = familyStack[familyStack.Count - 1];
							familyStack.RemoveLast();
							if (p != pawn)
							{
								break;
							}
							Pawn father = p.GetFather();
							if (father != null && !familyVisited.Contains(father))
							{
								familyStack.Add(father);
								familyVisited.Add(father);
							}
							Pawn mother = p.GetMother();
							if (mother != null && !familyVisited.Contains(mother))
							{
								familyStack.Add(mother);
								familyVisited.Add(mother);
							}
							familyChildrenStack.Clear();
							familyChildrenStack.Add(p);
							while (familyChildrenStack.Any())
							{
								Pawn child = familyChildrenStack[familyChildrenStack.Count - 1];
								familyChildrenStack.RemoveLast();
								if (child != p && child != pawn)
								{
									yield return child;
									/*Error: Unable to find new state assignment for yield return*/;
								}
								IEnumerable<Pawn> children = child.relations.Children;
								foreach (Pawn item in children)
								{
									if (!familyVisited.Contains(item))
									{
										familyChildrenStack.Add(item);
										familyVisited.Add(item);
									}
								}
							}
						}
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
					finally
					{
						((_003C_003Ec__Iterator1)/*Error near IL_030c: stateMachine*/)._003C_003E__Finally0();
					}
				}
				yield break;
				IL_031c:
				/*Error near IL_031d: Unexpected return in MoveNext()*/;
			}
		}

		public IEnumerable<Pawn> PotentiallyRelatedPawns
		{
			get
			{
				if (RelatedToAnyoneOrAnyoneRelatedToMe)
				{
					try
					{
						List<Pawn> stack = SimplePool<List<Pawn>>.Get();
						HashSet<Pawn> visited = SimplePool<HashSet<Pawn>>.Get();
						stack.Add(pawn);
						visited.Add(pawn);
						Pawn p;
						while (true)
						{
							if (!stack.Any())
							{
								yield break;
							}
							p = stack[stack.Count - 1];
							stack.RemoveLast();
							if (p != pawn)
							{
								break;
							}
							for (int i = 0; i < p.relations.directRelations.Count; i++)
							{
								Pawn otherPawn = p.relations.directRelations[i].otherPawn;
								if (!visited.Contains(otherPawn))
								{
									stack.Add(otherPawn);
									visited.Add(otherPawn);
								}
							}
							foreach (Pawn item in p.relations.pawnsWithDirectRelationsWithMe)
							{
								if (!visited.Contains(item))
								{
									stack.Add(item);
									visited.Add(item);
								}
							}
						}
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
					finally
					{
						((_003C_003Ec__Iterator2)/*Error near IL_0204: stateMachine*/)._003C_003E__Finally0();
					}
				}
				yield break;
				IL_0214:
				/*Error near IL_0215: Unexpected return in MoveNext()*/;
			}
		}

		public IEnumerable<Pawn> RelatedPawns
		{
			get
			{
				canCacheFamilyByBlood = true;
				familyByBloodIsCached = false;
				cachedFamilyByBlood.Clear();
				try
				{
					foreach (Pawn potentiallyRelatedPawn in PotentiallyRelatedPawns)
					{
						if ((familyByBloodIsCached && cachedFamilyByBlood.Contains(potentiallyRelatedPawn)) || pawn.GetRelations(potentiallyRelatedPawn).Any())
						{
							yield return potentiallyRelatedPawn;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				finally
				{
					((_003C_003Ec__Iterator3)/*Error near IL_014a: stateMachine*/)._003C_003E__Finally0();
				}
				yield break;
				IL_015a:
				/*Error near IL_015b: Unexpected return in MoveNext()*/;
			}
		}

		public Pawn_RelationsTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref directRelations, "directRelations", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = 0; i < directRelations.Count; i++)
				{
					if (directRelations[i].otherPawn == null)
					{
						Log.Warning("Pawn " + pawn + " has relation \"" + directRelations[i].def.defName + "\" with null pawn after loading. This means that we forgot to serialize pawns somewhere (e.g. pawns from passing trade ships).");
					}
				}
				directRelations.RemoveAll((DirectPawnRelation x) => x.otherPawn == null);
				for (int j = 0; j < directRelations.Count; j++)
				{
					directRelations[j].otherPawn.relations.pawnsWithDirectRelationsWithMe.Add(pawn);
				}
			}
			Scribe_Values.Look(ref everSeenByPlayer, "everSeenByPlayer", defaultValue: true);
			Scribe_Values.Look(ref canGetRescuedThought, "canGetRescuedThought", defaultValue: true);
			Scribe_References.Look(ref relativeInvolvedInRescueQuest, "relativeInvolvedInRescueQuest");
		}

		public void RelationsTrackerTick()
		{
			if (!pawn.Dead)
			{
				Tick_CheckStartMarriageCeremony();
				Tick_CheckDevelopBondRelation();
			}
		}

		public DirectPawnRelation GetDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.");
				return null;
			}
			return directRelations.Find((DirectPawnRelation x) => x.def == def && x.otherPawn == otherPawn);
		}

		public Pawn GetFirstDirectRelationPawn(PawnRelationDef def, Predicate<Pawn> predicate = null)
		{
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.");
				return null;
			}
			for (int i = 0; i < directRelations.Count; i++)
			{
				DirectPawnRelation directPawnRelation = directRelations[i];
				if (directPawnRelation.def == def && (predicate == null || predicate(directPawnRelation.otherPawn)))
				{
					return directPawnRelation.otherPawn;
				}
			}
			return null;
		}

		public bool DirectRelationExists(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.");
				return false;
			}
			for (int i = 0; i < directRelations.Count; i++)
			{
				DirectPawnRelation directPawnRelation = directRelations[i];
				if (directPawnRelation.def == def && directPawnRelation.otherPawn == otherPawn)
				{
					return true;
				}
			}
			return false;
		}

		public void AddDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning("Tried to directly add implied pawn relation " + def + ", pawn=" + pawn + ", otherPawn=" + otherPawn);
			}
			else if (otherPawn == pawn)
			{
				Log.Warning("Tried to add pawn relation " + def + " with self, pawn=" + pawn);
			}
			else if (DirectRelationExists(def, otherPawn))
			{
				Log.Warning("Tried to add the same relation twice: " + def + ", pawn=" + pawn + ", otherPawn=" + otherPawn);
			}
			else
			{
				int startTicks = (Current.ProgramState == ProgramState.Playing) ? Find.TickManager.TicksGame : 0;
				directRelations.Add(new DirectPawnRelation(def, otherPawn, startTicks));
				otherPawn.relations.pawnsWithDirectRelationsWithMe.Add(pawn);
				if (def.reflexive)
				{
					otherPawn.relations.directRelations.Add(new DirectPawnRelation(def, pawn, startTicks));
					pawnsWithDirectRelationsWithMe.Add(otherPawn);
				}
				GainedOrLostDirectRelation();
				otherPawn.relations.GainedOrLostDirectRelation();
			}
		}

		public void RemoveDirectRelation(DirectPawnRelation relation)
		{
			RemoveDirectRelation(relation.def, relation.otherPawn);
		}

		public void RemoveDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (!TryRemoveDirectRelation(def, otherPawn))
			{
				Log.Warning("Could not remove relation " + def + " because it's not here. pawn=" + pawn + ", otherPawn=" + otherPawn);
			}
		}

		public bool TryRemoveDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning("Tried to remove implied pawn relation " + def + ", pawn=" + pawn + ", otherPawn=" + otherPawn);
				return false;
			}
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (directRelations[i].def == def && directRelations[i].otherPawn == otherPawn)
				{
					if (def.reflexive)
					{
						List<DirectPawnRelation> list = otherPawn.relations.directRelations;
						DirectPawnRelation item = list.Find((DirectPawnRelation x) => x.def == def && x.otherPawn == pawn);
						list.Remove(item);
						if (list.Find((DirectPawnRelation x) => x.otherPawn == pawn) == null)
						{
							pawnsWithDirectRelationsWithMe.Remove(otherPawn);
						}
					}
					directRelations.RemoveAt(i);
					if (directRelations.Find((DirectPawnRelation x) => x.otherPawn == otherPawn) == null)
					{
						otherPawn.relations.pawnsWithDirectRelationsWithMe.Remove(pawn);
					}
					GainedOrLostDirectRelation();
					otherPawn.relations.GainedOrLostDirectRelation();
					return true;
				}
			}
			return false;
		}

		public int OpinionOf(Pawn other)
		{
			if (!other.RaceProps.Humanlike || pawn == other)
			{
				return 0;
			}
			if (pawn.Dead)
			{
				return 0;
			}
			int num = 0;
			foreach (PawnRelationDef relation in pawn.GetRelations(other))
			{
				num += relation.opinionOffset;
			}
			if (pawn.RaceProps.Humanlike)
			{
				num += pawn.needs.mood.thoughts.TotalOpinionOffset(other);
			}
			if (num != 0)
			{
				float num2 = 1f;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					if (hediffs[i].CurStage != null)
					{
						num2 *= hediffs[i].CurStage.opinionOfOthersFactor;
					}
				}
				num = Mathf.RoundToInt((float)num * num2);
			}
			if (num > 0 && pawn.HostileTo(other))
			{
				num = 0;
			}
			return Mathf.Clamp(num, -100, 100);
		}

		public string OpinionExplanation(Pawn other)
		{
			if (!other.RaceProps.Humanlike || pawn == other)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("OpinionOf".Translate(other.LabelShort, other) + ": " + OpinionOf(other).ToStringWithSign());
			string pawnSituationLabel = SocialCardUtility.GetPawnSituationLabel(other, pawn);
			if (!pawnSituationLabel.NullOrEmpty())
			{
				stringBuilder.AppendLine(pawnSituationLabel);
			}
			stringBuilder.AppendLine("--------------");
			bool flag = false;
			if (pawn.Dead)
			{
				stringBuilder.AppendLine("IAmDead".Translate());
				flag = true;
			}
			else
			{
				IEnumerable<PawnRelationDef> relations = pawn.GetRelations(other);
				foreach (PawnRelationDef item in relations)
				{
					stringBuilder.AppendLine(item.GetGenderSpecificLabelCap(other) + ": " + item.opinionOffset.ToStringWithSign());
					flag = true;
				}
				if (pawn.RaceProps.Humanlike)
				{
					ThoughtHandler thoughts = pawn.needs.mood.thoughts;
					thoughts.GetDistinctSocialThoughtGroups(other, tmpSocialThoughts);
					for (int i = 0; i < tmpSocialThoughts.Count; i++)
					{
						ISocialThought socialThought = tmpSocialThoughts[i];
						int num = 1;
						Thought thought = (Thought)socialThought;
						if (thought.def.IsMemory)
						{
							num = thoughts.memories.NumMemoriesInGroup((Thought_MemorySocial)socialThought);
						}
						stringBuilder.Append(thought.LabelCapSocial);
						if (num != 1)
						{
							stringBuilder.Append(" x" + num);
						}
						stringBuilder.AppendLine(": " + thoughts.OpinionOffsetOfGroup(socialThought, other).ToStringWithSign());
						flag = true;
					}
				}
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int j = 0; j < hediffs.Count; j++)
				{
					HediffStage curStage = hediffs[j].CurStage;
					if (curStage != null && curStage.opinionOfOthersFactor != 1f)
					{
						stringBuilder.Append(hediffs[j].LabelBase.CapitalizeFirst());
						if (curStage.opinionOfOthersFactor != 0f)
						{
							stringBuilder.AppendLine(": x" + curStage.opinionOfOthersFactor.ToStringPercent());
						}
						else
						{
							stringBuilder.AppendLine();
						}
						flag = true;
					}
				}
				if (pawn.HostileTo(other))
				{
					stringBuilder.AppendLine("Hostile".Translate());
					flag = true;
				}
			}
			if (!flag)
			{
				stringBuilder.AppendLine("NoneBrackets".Translate());
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public float SecondaryLovinChanceFactor(Pawn otherPawn)
		{
			if (pawn.def != otherPawn.def || pawn == otherPawn)
			{
				return 0f;
			}
			if (!(Rand.ValueSeeded(pawn.thingIDNumber ^ 0x31F3EF) < 0.015f))
			{
				if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
				{
					if (otherPawn.gender != pawn.gender)
					{
						return 0f;
					}
				}
				else if (otherPawn.gender == pawn.gender)
				{
					return 0f;
				}
			}
			float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
			float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
			float num = 1f;
			if (pawn.gender == Gender.Male)
			{
				if (ageBiologicalYearsFloat2 < 16f)
				{
					return 0f;
				}
				float min = Mathf.Max(16f, ageBiologicalYearsFloat - 30f);
				float lower = Mathf.Max(20f, ageBiologicalYearsFloat - 10f);
				num = GenMath.FlatHill(0.15f, min, lower, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, 0.15f, ageBiologicalYearsFloat2);
			}
			else if (pawn.gender == Gender.Female)
			{
				if (ageBiologicalYearsFloat2 < 16f)
				{
					return 0f;
				}
				if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f)
				{
					return 0.15f;
				}
				num = ((!(ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)) ? GenMath.FlatHill(0.3f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 30f, 0.15f, ageBiologicalYearsFloat2) : (Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.3f));
			}
			float num2 = 1f;
			num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
			num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
			num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
			int num3 = 0;
			if (otherPawn.RaceProps.Humanlike)
			{
				num3 = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
			}
			float num4 = 1f;
			if (num3 < 0)
			{
				num4 = 0.3f;
			}
			else if (num3 > 0)
			{
				num4 = 2.3f;
			}
			float num5 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
			float num6 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
			return num * num2 * num5 * num6 * num4;
		}

		public float SecondaryRomanceChanceFactor(Pawn otherPawn)
		{
			float num = 1f;
			foreach (PawnRelationDef relation in pawn.GetRelations(otherPawn))
			{
				num *= relation.attractionFactor;
			}
			return SecondaryLovinChanceFactor(otherPawn) * num;
		}

		public float CompatibilityWith(Pawn otherPawn)
		{
			if (pawn.def != otherPawn.def || pawn == otherPawn)
			{
				return 0f;
			}
			float x = Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
			float value = GenMath.LerpDouble(0f, 20f, 0.45f, -0.45f, x);
			value = Mathf.Clamp(value, -0.45f, 0.45f);
			float num = ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);
			return value + num;
		}

		public float ConstantPerPawnsPairCompatibilityOffset(int otherPawnID)
		{
			Rand.PushState();
			Rand.Seed = (pawn.thingIDNumber ^ otherPawnID) * 37;
			float result = Rand.GaussianAsymmetric(0.3f, 1f, 1.4f);
			Rand.PopState();
			return result;
		}

		public void ClearAllRelations()
		{
			List<DirectPawnRelation> list = directRelations.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				RemoveDirectRelation(list[i]);
			}
			List<Pawn> list2 = pawnsWithDirectRelationsWithMe.ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				List<DirectPawnRelation> list3 = list2[j].relations.directRelations.ToList();
				for (int k = 0; k < list3.Count; k++)
				{
					if (list3[k].otherPawn == pawn)
					{
						list2[j].relations.RemoveDirectRelation(list3[k]);
					}
				}
			}
		}

		internal void Notify_PawnKilled(DamageInfo? dinfo, Map mapBeforeDeath)
		{
			foreach (Pawn potentiallyRelatedPawn in PotentiallyRelatedPawns)
			{
				if (!potentiallyRelatedPawn.Dead && potentiallyRelatedPawn.needs.mood != null)
				{
					potentiallyRelatedPawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
				}
			}
			RemoveMySpouseMarriageRelatedThoughts();
			if (everSeenByPlayer && !PawnGenerator.IsBeingGenerated(pawn) && !pawn.RaceProps.Animal)
			{
				AffectBondedAnimalsOnMyDeath();
			}
			Notify_FailedRescueQuest();
		}

		public void Notify_PassedToWorld()
		{
			if (!pawn.Dead)
			{
				relativeInvolvedInRescueQuest = null;
			}
		}

		public void Notify_ExitedMap()
		{
			CheckRescued();
		}

		public void Notify_ChangedFaction()
		{
			if (pawn.Faction == Faction.OfPlayer)
			{
				CheckRescued();
			}
		}

		public void Notify_PawnSold(Pawn playerNegotiator)
		{
			foreach (Pawn potentiallyRelatedPawn in PotentiallyRelatedPawns)
			{
				if (!potentiallyRelatedPawn.Dead && potentiallyRelatedPawn.needs.mood != null)
				{
					PawnRelationDef mostImportantRelation = potentiallyRelatedPawn.GetMostImportantRelation(pawn);
					if (mostImportantRelation != null && mostImportantRelation.soldThought != null)
					{
						potentiallyRelatedPawn.needs.mood.thoughts.memories.TryGainMemory(mostImportantRelation.soldThought, playerNegotiator);
					}
				}
			}
			RemoveMySpouseMarriageRelatedThoughts();
		}

		public void Notify_PawnKidnapped()
		{
			RemoveMySpouseMarriageRelatedThoughts();
		}

		public void Notify_RescuedBy(Pawn rescuer)
		{
			if (rescuer.RaceProps.Humanlike && canGetRescuedThought)
			{
				pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RescuedMe, rescuer);
				canGetRescuedThought = false;
			}
		}

		public void Notify_FailedRescueQuest()
		{
			if (relativeInvolvedInRescueQuest != null && !relativeInvolvedInRescueQuest.Dead && relativeInvolvedInRescueQuest.needs.mood != null)
			{
				Messages.Message("MessageFailedToRescueRelative".Translate(pawn.LabelShort, relativeInvolvedInRescueQuest.LabelShort, pawn.Named("PAWN"), relativeInvolvedInRescueQuest.Named("RELATIVE")), relativeInvolvedInRescueQuest, MessageTypeDefOf.PawnDeath);
				relativeInvolvedInRescueQuest.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedToRescueRelative, pawn);
			}
			relativeInvolvedInRescueQuest = null;
		}

		private void CheckRescued()
		{
			if (relativeInvolvedInRescueQuest != null && !relativeInvolvedInRescueQuest.Dead && relativeInvolvedInRescueQuest.needs.mood != null)
			{
				Messages.Message("MessageRescuedRelative".Translate(pawn.LabelShort, relativeInvolvedInRescueQuest.LabelShort, pawn.Named("PAWN"), relativeInvolvedInRescueQuest.Named("RELATIVE")), relativeInvolvedInRescueQuest, MessageTypeDefOf.PositiveEvent);
				relativeInvolvedInRescueQuest.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RescuedRelative, pawn);
			}
			relativeInvolvedInRescueQuest = null;
		}

		public float GetFriendDiedThoughtPowerFactor(int opinion)
		{
			return Mathf.Lerp(0.15f, 1f, Mathf.InverseLerp(20f, 100f, (float)opinion));
		}

		public float GetRivalDiedThoughtPowerFactor(int opinion)
		{
			return Mathf.Lerp(0.15f, 1f, Mathf.InverseLerp(-20f, -100f, (float)opinion));
		}

		private void RemoveMySpouseMarriageRelatedThoughts()
		{
			Pawn spouse = pawn.GetSpouse();
			if (spouse != null && !spouse.Dead && spouse.needs.mood != null)
			{
				MemoryThoughtHandler memories = spouse.needs.mood.thoughts.memories;
				memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
				memories.RemoveMemoriesOfDef(ThoughtDefOf.HoneymoonPhase);
			}
		}

		public void CheckAppendBondedAnimalDiedInfo(ref string letter, ref string label)
		{
			if (pawn.RaceProps.Animal && everSeenByPlayer && !PawnGenerator.IsBeingGenerated(pawn))
			{
				Predicate<Pawn> isAffected = delegate(Pawn x)
				{
					if (x.Dead)
					{
						return false;
					}
					if (x.RaceProps.Humanlike && x.story.traits.HasTrait(TraitDefOf.Psychopath))
					{
						return false;
					}
					return true;
				};
				int num = 0;
				for (int i = 0; i < directRelations.Count; i++)
				{
					if (directRelations[i].def == PawnRelationDefOf.Bond && isAffected(directRelations[i].otherPawn))
					{
						num++;
					}
				}
				string text;
				switch (num)
				{
				case 0:
					return;
				case 1:
				{
					Pawn firstDirectRelationPawn = GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => isAffected(x));
					text = "LetterPartBondedAnimalDied".Translate(pawn.LabelDefinite(), firstDirectRelationPawn.LabelShort, pawn.Named("ANIMAL"), firstDirectRelationPawn.Named("HUMAN")).CapitalizeFirst();
					break;
				}
				default:
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int j = 0; j < directRelations.Count; j++)
					{
						if (directRelations[j].def == PawnRelationDefOf.Bond && isAffected(directRelations[j].otherPawn))
						{
							stringBuilder.AppendLine("  - " + directRelations[j].otherPawn.LabelShort);
						}
					}
					text = "LetterPartBondedAnimalDiedMulti".Translate(stringBuilder.ToString().TrimEndNewlines());
					break;
				}
				}
				label = label + " (" + "LetterLabelSuffixBondedAnimalDied".Translate() + ")";
				if (!letter.NullOrEmpty())
				{
					letter += "\n\n";
				}
				letter += text;
			}
		}

		private void AffectBondedAnimalsOnMyDeath()
		{
			int num = 0;
			Pawn pawn = null;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (directRelations[i].def == PawnRelationDefOf.Bond && directRelations[i].otherPawn.Spawned)
				{
					pawn = directRelations[i].otherPawn;
					num++;
					float value = Rand.Value;
					MentalStateDef wander_Sad;
					if (value < 0.25f)
					{
						wander_Sad = MentalStateDefOf.Wander_Sad;
					}
					wander_Sad = ((value < 0.5f) ? MentalStateDefOf.Wander_Psychotic : ((!(value < 0.75f)) ? MentalStateDefOf.Manhunter : MentalStateDefOf.Berserk));
					directRelations[i].otherPawn.mindState.mentalStateHandler.TryStartMentalState(wander_Sad, "MentalStateReason_BondedHumanDeath".Translate(this.pawn), forceWake: true);
				}
			}
			if (num == 1)
			{
				string str = (pawn.Name == null || pawn.Name.Numerical) ? "MessageBondedAnimalMentalBreak".Translate(pawn.LabelIndefinite(), this.pawn.LabelShort, pawn.Named("ANIMAL"), this.pawn.Named("HUMAN")) : "MessageNamedBondedAnimalMentalBreak".Translate(pawn.KindLabelIndefinite(), pawn.Name.ToStringShort, this.pawn.LabelShort, pawn.Named("ANIMAL"), this.pawn.Named("HUMAN"));
				Messages.Message(str.CapitalizeFirst(), pawn, MessageTypeDefOf.ThreatSmall);
			}
			else if (num > 1)
			{
				Messages.Message("MessageBondedAnimalsMentalBreak".Translate(num, this.pawn.LabelShort, this.pawn.Named("HUMAN")).CapitalizeFirst(), pawn, MessageTypeDefOf.ThreatSmall);
			}
		}

		private void Tick_CheckStartMarriageCeremony()
		{
			if (pawn.Spawned && !pawn.RaceProps.Animal && pawn.IsHashIntervalTick(1017))
			{
				int ticksGame = Find.TickManager.TicksGame;
				for (int i = 0; i < directRelations.Count; i++)
				{
					float num = (float)(ticksGame - directRelations[i].startTicks) / 60000f;
					if (directRelations[i].def == PawnRelationDefOf.Fiance && pawn.thingIDNumber < directRelations[i].otherPawn.thingIDNumber && num > 10f && Rand.MTBEventOccurs(2f, 60000f, 1017f) && pawn.Map == directRelations[i].otherPawn.Map && pawn.Map.IsPlayerHome && MarriageCeremonyUtility.AcceptableGameConditionsToStartCeremony(pawn.Map) && MarriageCeremonyUtility.FianceReadyToStartCeremony(pawn, directRelations[i].otherPawn) && MarriageCeremonyUtility.FianceReadyToStartCeremony(directRelations[i].otherPawn, pawn))
					{
						pawn.Map.lordsStarter.TryStartMarriageCeremony(pawn, directRelations[i].otherPawn);
					}
				}
			}
		}

		private void Tick_CheckDevelopBondRelation()
		{
			if (pawn.Spawned && pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && pawn.playerSettings.RespectedMaster != null)
			{
				Pawn respectedMaster = pawn.playerSettings.RespectedMaster;
				if (pawn.IsHashIntervalTick(2500) && pawn.Position.InHorDistOf(respectedMaster.Position, 12f) && GenSight.LineOfSight(pawn.Position, respectedMaster.Position, pawn.Map))
				{
					RelationsUtility.TryDevelopBondRelation(respectedMaster, pawn, 0.001f);
				}
			}
		}

		private void GainedOrLostDirectRelation()
		{
			if (Current.ProgramState == ProgramState.Playing && !pawn.Dead && pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
		}
	}
}
