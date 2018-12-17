using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class SocialCardUtility
	{
		private class CachedSocialTabEntry
		{
			public Pawn otherPawn;

			public int opinionOfOtherPawn;

			public int opinionOfMe;

			public List<PawnRelationDef> relations = new List<PawnRelationDef>();
		}

		private class CachedSocialTabEntryComparer : IComparer<CachedSocialTabEntry>
		{
			public int Compare(CachedSocialTabEntry a, CachedSocialTabEntry b)
			{
				bool flag = a.relations.Any();
				bool flag2 = b.relations.Any();
				if (flag != flag2)
				{
					return flag2.CompareTo(flag);
				}
				if (flag && flag2)
				{
					float num = -3.40282347E+38f;
					for (int i = 0; i < a.relations.Count; i++)
					{
						if (a.relations[i].importance > num)
						{
							num = a.relations[i].importance;
						}
					}
					float num2 = -3.40282347E+38f;
					for (int j = 0; j < b.relations.Count; j++)
					{
						if (b.relations[j].importance > num2)
						{
							num2 = b.relations[j].importance;
						}
					}
					if (num != num2)
					{
						return num2.CompareTo(num);
					}
				}
				if (a.opinionOfOtherPawn != b.opinionOfOtherPawn)
				{
					return b.opinionOfOtherPawn.CompareTo(a.opinionOfOtherPawn);
				}
				return a.otherPawn.thingIDNumber.CompareTo(b.otherPawn.thingIDNumber);
			}
		}

		private static Vector2 listScrollPosition = Vector2.zero;

		private static float listScrollViewHeight = 0f;

		private static bool showAllRelations;

		private static List<CachedSocialTabEntry> cachedEntries = new List<CachedSocialTabEntry>();

		private static Pawn cachedForPawn;

		private const float TopPadding = 20f;

		private static readonly Color RelationLabelColor = new Color(0.6f, 0.6f, 0.6f);

		private static readonly Color PawnLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

		private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		private const float RowTopPadding = 3f;

		private const float RowLeftRightPadding = 5f;

		private static CachedSocialTabEntryComparer CachedEntriesComparer = new CachedSocialTabEntryComparer();

		private static HashSet<Pawn> tmpCached = new HashSet<Pawn>();

		private static HashSet<Pawn> tmpToCache = new HashSet<Pawn>();

		public static void DrawSocialCard(Rect rect, Pawn pawn)
		{
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			Rect rect2 = new Rect(0f, 20f, rect.width, rect.height - 20f);
			Rect rect3 = rect2.ContractedBy(10f);
			Rect rect4 = rect3;
			Rect rect5 = rect3;
			rect4.height *= 0.63f;
			rect5.y = rect4.yMax + 17f;
			rect5.yMax = rect3.yMax;
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(0f, (rect4.yMax + rect5.y) / 2f, rect.width);
			GUI.color = Color.white;
			if (Prefs.DevMode)
			{
				Rect rect6 = new Rect(5f, 5f, rect.width, 22f);
				DrawDebugOptions(rect6, pawn);
			}
			DrawRelationsAndOpinions(rect4, pawn);
			InteractionCardUtility.DrawInteractionsLog(rect5, pawn, Find.PlayLog.AllEntries, 12);
			GUI.EndGroup();
		}

		private static void CheckRecache(Pawn selPawnForSocialInfo)
		{
			if (cachedForPawn != selPawnForSocialInfo || Time.frameCount % 20 == 0)
			{
				Recache(selPawnForSocialInfo);
			}
		}

		private static void Recache(Pawn selPawnForSocialInfo)
		{
			cachedForPawn = selPawnForSocialInfo;
			tmpToCache.Clear();
			foreach (Pawn relatedPawn in selPawnForSocialInfo.relations.RelatedPawns)
			{
				if (ShouldShowPawnRelations(relatedPawn, selPawnForSocialInfo))
				{
					tmpToCache.Add(relatedPawn);
				}
			}
			List<Pawn> list = null;
			if (selPawnForSocialInfo.MapHeld != null)
			{
				list = selPawnForSocialInfo.MapHeld.mapPawns.AllPawnsSpawned;
			}
			else if (selPawnForSocialInfo.IsCaravanMember())
			{
				list = selPawnForSocialInfo.GetCaravan().PawnsListForReading;
			}
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = list[i];
					if (pawn.RaceProps.Humanlike && pawn != selPawnForSocialInfo && ShouldShowPawnRelations(pawn, selPawnForSocialInfo) && !tmpToCache.Contains(pawn) && (selPawnForSocialInfo.relations.OpinionOf(pawn) != 0 || pawn.relations.OpinionOf(selPawnForSocialInfo) != 0))
					{
						tmpToCache.Add(pawn);
					}
				}
			}
			cachedEntries.RemoveAll((CachedSocialTabEntry x) => !tmpToCache.Contains(x.otherPawn));
			tmpCached.Clear();
			for (int j = 0; j < cachedEntries.Count; j++)
			{
				tmpCached.Add(cachedEntries[j].otherPawn);
			}
			foreach (Pawn item in tmpToCache)
			{
				if (!tmpCached.Contains(item))
				{
					CachedSocialTabEntry cachedSocialTabEntry = new CachedSocialTabEntry();
					cachedSocialTabEntry.otherPawn = item;
					cachedEntries.Add(cachedSocialTabEntry);
				}
			}
			tmpCached.Clear();
			tmpToCache.Clear();
			for (int k = 0; k < cachedEntries.Count; k++)
			{
				RecacheEntry(cachedEntries[k], selPawnForSocialInfo);
			}
			cachedEntries.Sort(CachedEntriesComparer);
		}

		private static bool ShouldShowPawnRelations(Pawn pawn, Pawn selPawnForSocialInfo)
		{
			if (showAllRelations)
			{
				return true;
			}
			if (pawn.relations.everSeenByPlayer)
			{
				return true;
			}
			return false;
		}

		private static void RecacheEntry(CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
		{
			entry.opinionOfMe = entry.otherPawn.relations.OpinionOf(selPawnForSocialInfo);
			entry.opinionOfOtherPawn = selPawnForSocialInfo.relations.OpinionOf(entry.otherPawn);
			entry.relations.Clear();
			foreach (PawnRelationDef relation in selPawnForSocialInfo.GetRelations(entry.otherPawn))
			{
				entry.relations.Add(relation);
			}
			entry.relations.Sort((PawnRelationDef a, PawnRelationDef b) => b.importance.CompareTo(a.importance));
		}

		public static void DrawRelationsAndOpinions(Rect rect, Pawn selPawnForSocialInfo)
		{
			CheckRecache(selPawnForSocialInfo);
			if (Current.ProgramState != ProgramState.Playing)
			{
				showAllRelations = false;
			}
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listScrollViewHeight);
			Rect rect2 = rect;
			if (viewRect.height > outRect.height)
			{
				rect2.width -= 16f;
			}
			Widgets.BeginScrollView(outRect, ref listScrollPosition, viewRect);
			float num = 0f;
			float y = listScrollPosition.y;
			float num2 = listScrollPosition.y + outRect.height;
			for (int i = 0; i < cachedEntries.Count; i++)
			{
				float rowHeight = GetRowHeight(cachedEntries[i], rect2.width, selPawnForSocialInfo);
				if (num > y - rowHeight && num < num2)
				{
					DrawPawnRow(num, rect2.width, cachedEntries[i], selPawnForSocialInfo);
				}
				num += rowHeight;
			}
			if (!cachedEntries.Any())
			{
				GUI.color = Color.gray;
				Text.Anchor = TextAnchor.UpperCenter;
				Rect rect3 = new Rect(0f, 0f, rect2.width, 30f);
				Widgets.Label(rect3, "NoRelationships".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
			}
			if (Event.current.type == EventType.Layout)
			{
				listScrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			GUI.color = Color.white;
		}

		private static void DrawPawnRow(float y, float width, CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
		{
			float rowHeight = GetRowHeight(entry, width, selPawnForSocialInfo);
			Rect rect = new Rect(0f, y, width, rowHeight);
			Pawn otherPawn = entry.otherPawn;
			if (Mouse.IsOver(rect))
			{
				GUI.color = HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			TooltipHandler.TipRegion(rect, () => GetPawnRowTooltip(entry, selPawnForSocialInfo), entry.otherPawn.thingIDNumber * 13 + selPawnForSocialInfo.thingIDNumber);
			if (Widgets.ButtonInvisible(rect))
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					if (otherPawn.Dead)
					{
						Messages.Message("MessageCantSelectDeadPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
					}
					else if (otherPawn.SpawnedOrAnyParentSpawned || otherPawn.IsCaravanMember())
					{
						CameraJumper.TryJumpAndSelect(otherPawn);
					}
					else
					{
						Messages.Message("MessageCantSelectOffMapPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
					}
				}
				else if (Find.GameInitData.startingAndOptionalPawns.Contains(otherPawn))
				{
					Page_ConfigureStartingPawns page_ConfigureStartingPawns = Find.WindowStack.WindowOfType<Page_ConfigureStartingPawns>();
					if (page_ConfigureStartingPawns != null)
					{
						page_ConfigureStartingPawns.SelectPawn(otherPawn);
						SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
					}
				}
			}
			CalculateColumnsWidths(width, out float relationsWidth, out float pawnLabelWidth, out float myOpinionWidth, out float hisOpinionWidth, out float pawnSituationLabelWidth);
			Rect rect2 = new Rect(5f, y + 3f, relationsWidth, rowHeight - 3f);
			DrawRelationLabel(entry, rect2, selPawnForSocialInfo);
			Rect rect3 = new Rect(rect2.xMax, y + 3f, pawnLabelWidth, rowHeight - 3f);
			DrawPawnLabel(otherPawn, rect3);
			Rect rect4 = new Rect(rect3.xMax, y + 3f, myOpinionWidth, rowHeight - 3f);
			DrawMyOpinion(entry, rect4, selPawnForSocialInfo);
			Rect rect5 = new Rect(rect4.xMax, y + 3f, hisOpinionWidth, rowHeight - 3f);
			DrawHisOpinion(entry, rect5, selPawnForSocialInfo);
			DrawPawnSituationLabel(rect: new Rect(rect5.xMax, y + 3f, pawnSituationLabelWidth, rowHeight - 3f), pawn: entry.otherPawn, selPawnForSocialInfo: selPawnForSocialInfo);
		}

		private static float GetRowHeight(CachedSocialTabEntry entry, float rowWidth, Pawn selPawnForSocialInfo)
		{
			CalculateColumnsWidths(rowWidth, out float relationsWidth, out float pawnLabelWidth, out float _, out float _, out float _);
			float a = 0f;
			a = Mathf.Max(a, Text.CalcHeight(GetRelationsString(entry, selPawnForSocialInfo), relationsWidth));
			a = Mathf.Max(a, Text.CalcHeight(GetPawnLabel(entry.otherPawn), pawnLabelWidth));
			return a + 3f;
		}

		private static void CalculateColumnsWidths(float rowWidth, out float relationsWidth, out float pawnLabelWidth, out float myOpinionWidth, out float hisOpinionWidth, out float pawnSituationLabelWidth)
		{
			float num = rowWidth - 10f;
			relationsWidth = num * 0.23f;
			pawnLabelWidth = num * 0.41f;
			myOpinionWidth = num * 0.075f;
			hisOpinionWidth = num * 0.085f;
			pawnSituationLabelWidth = num * 0.2f;
			if (myOpinionWidth < 25f)
			{
				pawnLabelWidth -= 25f - myOpinionWidth;
				myOpinionWidth = 25f;
			}
			if (hisOpinionWidth < 35f)
			{
				pawnLabelWidth -= 35f - hisOpinionWidth;
				hisOpinionWidth = 35f;
			}
		}

		private static void DrawRelationLabel(CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
		{
			string relationsString = GetRelationsString(entry, selPawnForSocialInfo);
			if (!relationsString.NullOrEmpty())
			{
				GUI.color = RelationLabelColor;
				Widgets.Label(rect, relationsString);
			}
		}

		private static void DrawPawnLabel(Pawn pawn, Rect rect)
		{
			GUI.color = PawnLabelColor;
			Widgets.Label(rect, GetPawnLabel(pawn));
		}

		private static void DrawMyOpinion(CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
		{
			if (entry.otherPawn.RaceProps.Humanlike && selPawnForSocialInfo.RaceProps.Humanlike)
			{
				int opinionOfOtherPawn = entry.opinionOfOtherPawn;
				GUI.color = OpinionLabelColor(opinionOfOtherPawn);
				Widgets.Label(rect, opinionOfOtherPawn.ToStringWithSign());
			}
		}

		private static void DrawHisOpinion(CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
		{
			if (entry.otherPawn.RaceProps.Humanlike && selPawnForSocialInfo.RaceProps.Humanlike)
			{
				int opinionOfMe = entry.opinionOfMe;
				Color color = OpinionLabelColor(opinionOfMe);
				GUI.color = new Color(color.r, color.g, color.b, 0.4f);
				Widgets.Label(rect, "(" + opinionOfMe.ToStringWithSign() + ")");
			}
		}

		private static void DrawPawnSituationLabel(Pawn pawn, Rect rect, Pawn selPawnForSocialInfo)
		{
			GUI.color = Color.gray;
			string label = GetPawnSituationLabel(pawn, selPawnForSocialInfo).Truncate(rect.width);
			Widgets.Label(rect, label);
		}

		private static Color OpinionLabelColor(int opinion)
		{
			if (Mathf.Abs(opinion) < 10)
			{
				return Color.gray;
			}
			if (opinion < 0)
			{
				return Color.red;
			}
			return Color.green;
		}

		private static string GetPawnLabel(Pawn pawn)
		{
			if (pawn.Name != null)
			{
				return pawn.Name.ToStringFull;
			}
			return pawn.LabelCapNoCount;
		}

		public static string GetPawnSituationLabel(Pawn pawn, Pawn fromPOV)
		{
			if (pawn.Dead)
			{
				return "Dead".Translate();
			}
			if (pawn.Destroyed)
			{
				return "Missing".Translate();
			}
			if (PawnUtility.IsKidnappedPawn(pawn))
			{
				return "Kidnapped".Translate();
			}
			if (pawn.kindDef == PawnKindDefOf.Slave)
			{
				return "Slave".Translate();
			}
			if (PawnUtility.IsFactionLeader(pawn))
			{
				return "FactionLeader".Translate();
			}
			Faction faction = pawn.Faction;
			if (faction != fromPOV.Faction)
			{
				if (faction != null && fromPOV.Faction != null)
				{
					switch (faction.RelationKindWith(fromPOV.Faction))
					{
					case FactionRelationKind.Hostile:
						return "Hostile".Translate() + ", " + faction.Name;
					case FactionRelationKind.Neutral:
						return "Neutral".Translate() + ", " + faction.Name;
					case FactionRelationKind.Ally:
						return "Ally".Translate() + ", " + faction.Name;
					default:
						return string.Empty;
					}
				}
				return "Neutral".Translate();
			}
			return string.Empty;
		}

		private static string GetRelationsString(CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
		{
			string text = string.Empty;
			if (entry.relations.Count == 0)
			{
				if (entry.opinionOfOtherPawn < -20)
				{
					return "Rival".Translate();
				}
				if (entry.opinionOfOtherPawn > 20)
				{
					return "Friend".Translate();
				}
				return "Acquaintance".Translate();
			}
			for (int i = 0; i < entry.relations.Count; i++)
			{
				PawnRelationDef pawnRelationDef = entry.relations[i];
				text = (text.NullOrEmpty() ? pawnRelationDef.GetGenderSpecificLabelCap(entry.otherPawn) : (text + ", " + pawnRelationDef.GetGenderSpecificLabel(entry.otherPawn)));
			}
			return text;
		}

		private static string GetPawnRowTooltip(CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (entry.otherPawn.RaceProps.Humanlike && selPawnForSocialInfo.RaceProps.Humanlike)
			{
				stringBuilder.AppendLine(selPawnForSocialInfo.relations.OpinionExplanation(entry.otherPawn));
				stringBuilder.AppendLine();
				stringBuilder.Append("SomeonesOpinionOfMe".Translate(entry.otherPawn.LabelShort, entry.otherPawn));
				stringBuilder.Append(": ");
				stringBuilder.Append(entry.opinionOfMe.ToStringWithSign());
			}
			else
			{
				stringBuilder.AppendLine(entry.otherPawn.LabelCapNoCount);
				string pawnSituationLabel = GetPawnSituationLabel(entry.otherPawn, selPawnForSocialInfo);
				if (!pawnSituationLabel.NullOrEmpty())
				{
					stringBuilder.AppendLine(pawnSituationLabel);
				}
				stringBuilder.AppendLine("--------------");
				stringBuilder.Append(GetRelationsString(entry, selPawnForSocialInfo));
			}
			if (Prefs.DevMode)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("(debug) Compatibility: " + selPawnForSocialInfo.relations.CompatibilityWith(entry.otherPawn).ToString("F2"));
				stringBuilder.Append("(debug) RomanceChanceFactor: " + selPawnForSocialInfo.relations.SecondaryRomanceChanceFactor(entry.otherPawn).ToString("F2"));
			}
			return stringBuilder.ToString();
		}

		private static void DrawDebugOptions(Rect rect, Pawn pawn)
		{
			GUI.BeginGroup(rect);
			Widgets.CheckboxLabeled(new Rect(0f, 0f, 145f, 22f), "Dev: AllRelations", ref showAllRelations);
			if (Widgets.ButtonText(new Rect(150f, 0f, 115f, 22f), "Debug info"))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				list.Add(new FloatMenuOption("AttractionTo", delegate
				{
					StringBuilder stringBuilder5 = new StringBuilder();
					stringBuilder5.AppendLine("My gender: " + pawn.gender);
					stringBuilder5.AppendLine("My age: " + pawn.ageTracker.AgeBiologicalYears);
					stringBuilder5.AppendLine();
					IOrderedEnumerable<Pawn> orderedEnumerable4 = from x in pawn.Map.mapPawns.AllPawnsSpawned
					where x.def == pawn.def
					orderby pawn.relations.SecondaryRomanceChanceFactor(x) descending
					select x;
					foreach (Pawn item in orderedEnumerable4)
					{
						if (item != pawn)
						{
							stringBuilder5.AppendLine(item.LabelShort + " (" + item.gender + ", age: " + item.ageTracker.AgeBiologicalYears + ", compat: " + pawn.relations.CompatibilityWith(item).ToString("F2") + "): " + pawn.relations.SecondaryRomanceChanceFactor(item).ToStringPercent("F0") + "        [vs " + item.relations.SecondaryRomanceChanceFactor(pawn).ToStringPercent("F0") + "]");
						}
					}
					Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder5.ToString()));
				}));
				list.Add(new FloatMenuOption("CompatibilityTo", delegate
				{
					StringBuilder stringBuilder4 = new StringBuilder();
					stringBuilder4.AppendLine("My age: " + pawn.ageTracker.AgeBiologicalYears);
					stringBuilder4.AppendLine();
					IOrderedEnumerable<Pawn> orderedEnumerable3 = from x in pawn.Map.mapPawns.AllPawnsSpawned
					where x.def == pawn.def
					orderby pawn.relations.CompatibilityWith(x) descending
					select x;
					foreach (Pawn item2 in orderedEnumerable3)
					{
						if (item2 != pawn)
						{
							stringBuilder4.AppendLine(item2.LabelShort + " (" + item2.KindLabel + ", age: " + item2.ageTracker.AgeBiologicalYears + "): " + pawn.relations.CompatibilityWith(item2).ToString("0.##"));
						}
					}
					Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder4.ToString()));
				}));
				if (pawn.RaceProps.Humanlike)
				{
					list.Add(new FloatMenuOption("Interaction chance", delegate
					{
						StringBuilder stringBuilder3 = new StringBuilder();
						stringBuilder3.AppendLine("(selected pawn is the initiator)");
						stringBuilder3.AppendLine("(\"fight chance\" is the chance that the receiver will start social fight)");
						stringBuilder3.AppendLine("Interaction chance (real chance, not just weights):");
						IOrderedEnumerable<Pawn> orderedEnumerable2 = from x in pawn.Map.mapPawns.AllPawnsSpawned
						where x.RaceProps.Humanlike
						orderby (x.Faction != null) ? x.Faction.loadID : (-1)
						select x;
						foreach (Pawn item3 in orderedEnumerable2)
						{
							if (item3 != pawn)
							{
								stringBuilder3.AppendLine();
								stringBuilder3.AppendLine(item3.LabelShort + " (" + item3.KindLabel + ", " + item3.gender + ", age: " + item3.ageTracker.AgeBiologicalYears + ", compat: " + pawn.relations.CompatibilityWith(item3).ToString("F2") + ", attr: " + pawn.relations.SecondaryRomanceChanceFactor(item3).ToStringPercent("F0") + "):");
								List<InteractionDef> list2 = (from x in DefDatabase<InteractionDef>.AllDefs
								where x.Worker.RandomSelectionWeight(pawn, item3) > 0f
								orderby x.Worker.RandomSelectionWeight(pawn, item3) descending
								select x).ToList();
								float num12 = list2.Sum((InteractionDef x) => x.Worker.RandomSelectionWeight(pawn, item3));
								foreach (InteractionDef item4 in list2)
								{
									float f = item3.interactions.SocialFightChance(item4, pawn);
									float f2 = item4.Worker.RandomSelectionWeight(pawn, item3) / num12;
									stringBuilder3.AppendLine("  " + item4.defName + ": " + f2.ToStringPercent() + " (fight chance: " + f.ToStringPercent("F2") + ")");
									if (item4 == InteractionDefOf.RomanceAttempt)
									{
										stringBuilder3.AppendLine("    success chance: " + ((InteractionWorker_RomanceAttempt)item4.Worker).SuccessChance(pawn, item3).ToStringPercent());
									}
									else if (item4 == InteractionDefOf.MarriageProposal)
									{
										stringBuilder3.AppendLine("    acceptance chance: " + ((InteractionWorker_MarriageProposal)item4.Worker).AcceptanceChance(pawn, item3).ToStringPercent());
									}
								}
							}
						}
						Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder3.ToString()));
					}));
					list.Add(new FloatMenuOption("Lovin' MTB", delegate
					{
						StringBuilder stringBuilder2 = new StringBuilder();
						stringBuilder2.AppendLine("Lovin' MTB hours with pawn X.");
						stringBuilder2.AppendLine("Assuming both pawns are in bed and are partners.");
						stringBuilder2.AppendLine();
						IOrderedEnumerable<Pawn> orderedEnumerable = from x in pawn.Map.mapPawns.AllPawnsSpawned
						where x.def == pawn.def
						orderby pawn.relations.SecondaryRomanceChanceFactor(x) descending
						select x;
						foreach (Pawn item5 in orderedEnumerable)
						{
							if (item5 != pawn)
							{
								stringBuilder2.AppendLine(item5.LabelShort + " (" + item5.KindLabel + ", age: " + item5.ageTracker.AgeBiologicalYears + "): " + LovePartnerRelationUtility.GetLovinMtbHours(pawn, item5).ToString("F1") + " h");
							}
						}
						Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder2.ToString()));
					}));
				}
				list.Add(new FloatMenuOption("Test per pawns pair compatibility factor probability", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					int num5 = 0;
					int num6 = 0;
					int num7 = 0;
					int num8 = 0;
					float num9 = -999999f;
					float num10 = 999999f;
					for (int i = 0; i < 10000; i++)
					{
						int otherPawnID = Rand.RangeInclusive(0, 30000);
						float num11 = pawn.relations.ConstantPerPawnsPairCompatibilityOffset(otherPawnID);
						if (num11 < -3f)
						{
							num++;
						}
						else if (num11 < -2f)
						{
							num2++;
						}
						else if (num11 < -1f)
						{
							num3++;
						}
						else if (num11 < 0f)
						{
							num4++;
						}
						else if (num11 < 1f)
						{
							num5++;
						}
						else if (num11 < 2f)
						{
							num6++;
						}
						else if (num11 < 3f)
						{
							num7++;
						}
						else
						{
							num8++;
						}
						if (num11 > num9)
						{
							num9 = num11;
						}
						else if (num11 < num10)
						{
							num10 = num11;
						}
					}
					stringBuilder.AppendLine("< -3: " + ((float)num / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< -2: " + ((float)num2 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< -1: " + ((float)num3 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< 0: " + ((float)num4 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< 1: " + ((float)num5 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< 2: " + ((float)num6 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< 3: " + ((float)num7 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("> 3: " + ((float)num8 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("trials: " + 10000);
					stringBuilder.AppendLine("min: " + num10);
					stringBuilder.AppendLine("max: " + num9);
					Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder.ToString()));
				}));
				Find.WindowStack.Add(new FloatMenu(list));
			}
			GUI.EndGroup();
		}
	}
}
