using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Faction : IExposable, ILoadReferenceable, ICommunicable
	{
		public FactionDef def;

		private string name;

		public int loadID = -1;

		public int randomKey;

		public float colorFromSpectrum = -999f;

		public float centralMelanin = 0.5f;

		private List<FactionRelation> relations = new List<FactionRelation>();

		public Pawn leader;

		public KidnappedPawnsTracker kidnapped;

		private List<PredatorThreat> predatorThreats = new List<PredatorThreat>();

		public bool defeated;

		public int lastTraderRequestTick = -9999999;

		public int lastMilitaryAidRequestTick = -9999999;

		private int naturalGoodwillTimer;

		private List<Map> avoidGridsBasicKeysWorkingList;

		private List<ByteGrid> avoidGridsBasicValuesWorkingList;

		private List<Map> avoidGridsSmartKeysWorkingList;

		private List<ByteGrid> avoidGridsSmartValuesWorkingList;

		private static List<PawnKindDef> allPawnKinds = new List<PawnKindDef>();

		[CompilerGenerated]
		private static Predicate<Settlement> _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Predicate<Settlement> _003C_003Ef__mg_0024cache1;

		public string Name
		{
			get
			{
				if (HasName)
				{
					return name;
				}
				return def.LabelCap;
			}
			set
			{
				name = value;
			}
		}

		public bool HasName => name != null;

		public bool IsPlayer => def.isPlayer;

		public int PlayerGoodwill => GoodwillWith(OfPlayer);

		public FactionRelationKind PlayerRelationKind => RelationKindWith(OfPlayer);

		public Color Color
		{
			get
			{
				if (def.colorSpectrum.NullOrEmpty())
				{
					return Color.white;
				}
				return ColorsFromSpectrum.Get(def.colorSpectrum, colorFromSpectrum);
			}
		}

		public static Faction OfPlayer
		{
			get
			{
				Faction ofPlayerSilentFail = OfPlayerSilentFail;
				if (ofPlayerSilentFail == null)
				{
					Log.Error("Could not find player faction.");
				}
				return ofPlayerSilentFail;
			}
		}

		public static Faction OfMechanoids => Find.FactionManager.OfMechanoids;

		public static Faction OfInsects => Find.FactionManager.OfInsects;

		public static Faction OfAncients => Find.FactionManager.OfAncients;

		public static Faction OfAncientsHostile => Find.FactionManager.OfAncientsHostile;

		public static Faction OfPlayerSilentFail
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					GameInitData gameInitData = Find.GameInitData;
					if (gameInitData != null && gameInitData.playerFaction != null)
					{
						return gameInitData.playerFaction;
					}
				}
				return Find.FactionManager.OfPlayer;
			}
		}

		public Faction()
		{
			randomKey = Rand.Range(0, 2147483647);
			kidnapped = new KidnappedPawnsTracker(this);
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref leader, "leader");
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref name, "name");
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_Values.Look(ref randomKey, "randomKey", 0);
			Scribe_Values.Look(ref colorFromSpectrum, "colorFromSpectrum", 0f);
			Scribe_Values.Look(ref centralMelanin, "centralMelanin", 0f);
			Scribe_Collections.Look(ref relations, "relations", LookMode.Deep);
			Scribe_Deep.Look(ref kidnapped, "kidnapped", this);
			Scribe_Collections.Look(ref predatorThreats, "predatorThreats", LookMode.Deep);
			Scribe_Values.Look(ref defeated, "defeated", defaultValue: false);
			Scribe_Values.Look(ref lastTraderRequestTick, "lastTraderRequestTick", -9999999);
			Scribe_Values.Look(ref lastMilitaryAidRequestTick, "lastMilitaryAidRequestTick", -9999999);
			Scribe_Values.Look(ref naturalGoodwillTimer, "naturalGoodwillTimer", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				predatorThreats.RemoveAll((PredatorThreat x) => x.predator == null);
			}
		}

		public void FactionTick()
		{
			CheckNaturalTendencyToReachGoodwillThreshold();
			kidnapped.KidnappedPawnsTrackerTick();
			for (int num = predatorThreats.Count - 1; num >= 0; num--)
			{
				PredatorThreat predatorThreat = predatorThreats[num];
				if (predatorThreat.Expired)
				{
					predatorThreats.RemoveAt(num);
					if (predatorThreat.predator.Spawned)
					{
						predatorThreat.predator.Map.attackTargetsCache.UpdateTarget(predatorThreat.predator);
					}
				}
			}
			if (Find.TickManager.TicksGame % 1000 == 200 && IsPlayer)
			{
				if (NamePlayerFactionAndSettlementUtility.CanNameFactionNow())
				{
					Settlement settlement = Find.WorldObjects.Settlements.Find(NamePlayerFactionAndSettlementUtility.CanNameSettlementSoon);
					if (settlement != null)
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(settlement));
					}
					else
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFaction());
					}
				}
				else
				{
					Settlement settlement2 = Find.WorldObjects.Settlements.Find(NamePlayerFactionAndSettlementUtility.CanNameSettlementNow);
					if (settlement2 != null)
					{
						if (NamePlayerFactionAndSettlementUtility.CanNameFactionSoon())
						{
							Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(settlement2));
						}
						else
						{
							Find.WindowStack.Add(new Dialog_NamePlayerSettlement(settlement2));
						}
					}
				}
			}
		}

		private void CheckNaturalTendencyToReachGoodwillThreshold()
		{
			if (!IsPlayer)
			{
				int playerGoodwill = PlayerGoodwill;
				if (def.naturalColonyGoodwill.Includes(playerGoodwill))
				{
					naturalGoodwillTimer = 0;
				}
				else
				{
					naturalGoodwillTimer++;
					if (playerGoodwill < def.naturalColonyGoodwill.min)
					{
						if (def.goodwillDailyGain != 0f)
						{
							int num = (int)(10f / def.goodwillDailyGain * 60000f);
							if (naturalGoodwillTimer >= num)
							{
								Faction ofPlayer = OfPlayer;
								int goodwillChange = Mathf.Min(10, def.naturalColonyGoodwill.min - playerGoodwill);
								string reason = "GoodwillChangedReason_NaturallyOverTime".Translate(def.naturalColonyGoodwill.min.ToString());
								TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, reason);
								naturalGoodwillTimer = 0;
							}
						}
					}
					else if (playerGoodwill > def.naturalColonyGoodwill.max && def.goodwillDailyFall != 0f)
					{
						int num2 = (int)(10f / def.goodwillDailyFall * 60000f);
						if (naturalGoodwillTimer >= num2)
						{
							Faction ofPlayer = OfPlayer;
							int goodwillChange = -Mathf.Min(10, playerGoodwill - def.naturalColonyGoodwill.max);
							string reason = "GoodwillChangedReason_NaturallyOverTime".Translate(def.naturalColonyGoodwill.max.ToString());
							TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, reason);
							naturalGoodwillTimer = 0;
						}
					}
				}
			}
		}

		public void TryMakeInitialRelationsWith(Faction other)
		{
			if (RelationWith(other, allowNull: true) == null)
			{
				int a = (!def.permanentEnemy) ? def.startingGoodwill.RandomInRange : (-100);
				if (IsPlayer)
				{
					a = 100;
				}
				int b = (!other.def.permanentEnemy) ? other.def.startingGoodwill.RandomInRange : (-100);
				if (other.IsPlayer)
				{
					b = 100;
				}
				int num = Mathf.Min(a, b);
				FactionRelationKind kind = (num > -10) ? ((num < 75) ? FactionRelationKind.Neutral : FactionRelationKind.Ally) : FactionRelationKind.Hostile;
				FactionRelation factionRelation = new FactionRelation();
				factionRelation.other = other;
				factionRelation.goodwill = num;
				factionRelation.kind = kind;
				relations.Add(factionRelation);
				FactionRelation factionRelation2 = new FactionRelation();
				factionRelation2.other = this;
				factionRelation2.goodwill = num;
				factionRelation2.kind = kind;
				other.relations.Add(factionRelation2);
			}
		}

		public PawnKindDef RandomPawnKind()
		{
			allPawnKinds.Clear();
			if (def.pawnGroupMakers != null)
			{
				for (int i = 0; i < def.pawnGroupMakers.Count; i++)
				{
					List<PawnGenOption> options = def.pawnGroupMakers[i].options;
					for (int j = 0; j < options.Count; j++)
					{
						allPawnKinds.Add(options[j].kind);
					}
				}
			}
			if (!allPawnKinds.Any())
			{
				return def.basicMemberKind;
			}
			PawnKindDef result = allPawnKinds.RandomElement();
			allPawnKinds.Clear();
			return result;
		}

		public FactionRelation RelationWith(Faction other, bool allowNull = false)
		{
			if (other == this)
			{
				Log.Error("Tried to get relation between faction " + this + " and itself.");
				return new FactionRelation();
			}
			for (int i = 0; i < relations.Count; i++)
			{
				if (relations[i].other == other)
				{
					return relations[i];
				}
			}
			if (!allowNull)
			{
				Log.Error("Faction " + name + " has null relation with " + other + ". Returning dummy relation.");
				return new FactionRelation();
			}
			return null;
		}

		public int GoodwillWith(Faction other)
		{
			return RelationWith(other).goodwill;
		}

		public FactionRelationKind RelationKindWith(Faction other)
		{
			return RelationWith(other).kind;
		}

		public bool TryAffectGoodwillWith(Faction other, int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = default(GlobalTargetInfo?))
		{
			if (def.hidden || other.def.hidden || def.permanentEnemy || other.def.permanentEnemy || defeated || other.defeated || other == this)
			{
				return false;
			}
			if (goodwillChange > 0 && ((IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(other)) || (other.IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(this))))
			{
				return false;
			}
			if (goodwillChange == 0)
			{
				return true;
			}
			int num = GoodwillWith(other);
			int num2 = Mathf.Clamp(num + goodwillChange, -100, 100);
			if (num == num2)
			{
				return true;
			}
			FactionRelation factionRelation = RelationWith(other);
			factionRelation.goodwill = num2;
			factionRelation.CheckKindThresholds(this, canSendHostilityLetter, reason, (!lookTarget.HasValue) ? GlobalTargetInfo.Invalid : lookTarget.Value, out bool sentLetter);
			FactionRelation factionRelation2 = other.RelationWith(this);
			FactionRelationKind kind = factionRelation2.kind;
			factionRelation2.goodwill = factionRelation.goodwill;
			factionRelation2.kind = factionRelation.kind;
			bool sentLetter2;
			if (kind != factionRelation2.kind)
			{
				other.Notify_RelationKindChanged(this, kind, canSendHostilityLetter, reason, (!lookTarget.HasValue) ? GlobalTargetInfo.Invalid : lookTarget.Value, out sentLetter2);
			}
			else
			{
				sentLetter2 = false;
			}
			if (canSendMessage && !sentLetter && !sentLetter2 && Current.ProgramState == ProgramState.Playing && (IsPlayer || other.IsPlayer))
			{
				Faction faction = (!IsPlayer) ? this : other;
				string text = reason.NullOrEmpty() ? "MessageGoodwillChanged".Translate(faction.name, num.ToString("F0"), factionRelation.goodwill.ToString("F0")) : "MessageGoodwillChangedWithReason".Translate(faction.name, num.ToString("F0"), factionRelation.goodwill.ToString("F0"), reason);
				Messages.Message(text, (!lookTarget.HasValue) ? GlobalTargetInfo.Invalid : lookTarget.Value, (!((float)goodwillChange > 0f)) ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.PositiveEvent);
			}
			return true;
		}

		public bool TrySetNotHostileTo(Faction other, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = default(GlobalTargetInfo?))
		{
			if (RelationKindWith(other) == FactionRelationKind.Hostile)
			{
				TrySetRelationKind(other, FactionRelationKind.Neutral, canSendLetter, reason, lookTarget);
			}
			return RelationKindWith(other) != FactionRelationKind.Hostile;
		}

		public bool TrySetNotAlly(Faction other, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = default(GlobalTargetInfo?))
		{
			if (RelationKindWith(other) == FactionRelationKind.Ally)
			{
				TrySetRelationKind(other, FactionRelationKind.Neutral, canSendLetter, reason, lookTarget);
			}
			return RelationKindWith(other) != FactionRelationKind.Ally;
		}

		public bool TrySetRelationKind(Faction other, FactionRelationKind kind, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = default(GlobalTargetInfo?))
		{
			FactionRelation factionRelation = RelationWith(other);
			if (factionRelation.kind != kind)
			{
				switch (kind)
				{
				case FactionRelationKind.Hostile:
					TryAffectGoodwillWith(other, -75 - factionRelation.goodwill, canSendMessage: false, canSendLetter, reason, lookTarget);
					return factionRelation.kind == FactionRelationKind.Hostile;
				case FactionRelationKind.Neutral:
					TryAffectGoodwillWith(other, -factionRelation.goodwill, canSendMessage: false, canSendLetter, reason, lookTarget);
					return factionRelation.kind == FactionRelationKind.Neutral;
				case FactionRelationKind.Ally:
					TryAffectGoodwillWith(other, 75 - factionRelation.goodwill, canSendMessage: false, canSendLetter, reason, lookTarget);
					return factionRelation.kind == FactionRelationKind.Ally;
				default:
					throw new NotSupportedException(kind.ToString());
				}
			}
			return true;
		}

		public void RemoveAllRelations()
		{
			foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
			{
				if (item != this)
				{
					item.relations.RemoveAll((FactionRelation x) => x.other == this);
				}
			}
			relations.Clear();
		}

		public void TryAppendRelationKindChangedInfo(StringBuilder text, FactionRelationKind previousKind, FactionRelationKind newKind, string reason = null)
		{
			string text2 = null;
			TryAppendRelationKindChangedInfo(ref text2, previousKind, newKind, reason);
			if (!text2.NullOrEmpty())
			{
				text.AppendLine();
				text.AppendLine();
				text.Append(text2);
			}
		}

		public void TryAppendRelationKindChangedInfo(ref string text, FactionRelationKind previousKind, FactionRelationKind newKind, string reason = null)
		{
			if (previousKind != newKind)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				switch (newKind)
				{
				case FactionRelationKind.Hostile:
					text += "LetterRelationsChange_Hostile".Translate(name, PlayerGoodwill.ToStringWithSign(), (-75).ToStringWithSign(), 0.ToStringWithSign());
					if (!reason.NullOrEmpty())
					{
						text = text + "\n\n" + "FinalStraw".Translate(reason.CapitalizeFirst());
					}
					break;
				case FactionRelationKind.Ally:
					text += "LetterRelationsChange_Ally".Translate(name, PlayerGoodwill.ToStringWithSign(), 75.ToStringWithSign(), 0.ToStringWithSign());
					if (!reason.NullOrEmpty())
					{
						string text2 = text;
						text = text2 + "\n\n" + "LastFactionRelationsEvent".Translate() + ": " + reason.CapitalizeFirst();
					}
					break;
				case FactionRelationKind.Neutral:
					if (previousKind == FactionRelationKind.Hostile)
					{
						text += "LetterRelationsChange_NeutralFromHostile".Translate(name, PlayerGoodwill.ToStringWithSign(), 0.ToStringWithSign(), (-75).ToStringWithSign(), 75.ToStringWithSign());
						if (!reason.NullOrEmpty())
						{
							string text2 = text;
							text = text2 + "\n\n" + "LastFactionRelationsEvent".Translate() + ": " + reason.CapitalizeFirst();
						}
					}
					else
					{
						text += "LetterRelationsChange_NeutralFromAlly".Translate(name, PlayerGoodwill.ToStringWithSign(), 0.ToStringWithSign(), (-75).ToStringWithSign(), 75.ToStringWithSign());
						if (!reason.NullOrEmpty())
						{
							string text2 = text;
							text = text2 + "\n\n" + "Reason".Translate() + ": " + reason.CapitalizeFirst();
						}
					}
					break;
				}
			}
		}

		public void Notify_MemberTookDamage(Pawn member, DamageInfo dinfo)
		{
			if (dinfo.Instigator != null && !IsPlayer)
			{
				Pawn pawn = dinfo.Instigator as Pawn;
				if (pawn != null && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.PredatorHunt)
				{
					TookDamageFromPredator(pawn);
				}
				if (dinfo.Instigator.Faction != null && dinfo.Def.ExternalViolenceFor(member) && !this.HostileTo(dinfo.Instigator.Faction) && !member.InAggroMentalState && (pawn == null || !pawn.InMentalState || pawn.MentalStateDef != MentalStateDefOf.Berserk) && (!member.InMentalState || !member.MentalStateDef.IsExtreme || member.MentalStateDef.category != MentalStateCategory.Malicious || PlayerRelationKind != FactionRelationKind.Ally) && (dinfo.Instigator.Faction != OfPlayer || !PrisonBreakUtility.IsPrisonBreaking(member)) && dinfo.Instigator.Faction == OfPlayer && !IsMutuallyHostileCrossfire(dinfo))
				{
					float num = Mathf.Min(100f, dinfo.Amount);
					int num2 = (int)(-1.3f * num);
					Faction faction = dinfo.Instigator.Faction;
					int goodwillChange = num2;
					string reason = "GoodwillChangedReason_AttackedPawn".Translate(member.LabelShort, member);
					GlobalTargetInfo? lookTarget = member;
					TryAffectGoodwillWith(faction, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, reason, lookTarget);
				}
			}
		}

		public void Notify_MemberCaptured(Pawn member, Faction violator)
		{
			if (violator != this && RelationKindWith(violator) != 0)
			{
				FactionRelationKind kind = FactionRelationKind.Hostile;
				string reason = "GoodwillChangedReason_CapturedPawn".Translate(member.LabelShort, member);
				TrySetRelationKind(violator, kind, canSendLetter: true, reason, member);
			}
		}

		public void Notify_MemberDied(Pawn member, DamageInfo? dinfo, bool wasWorldPawn, Map map)
		{
			if (!IsPlayer)
			{
				if (!wasWorldPawn && !PawnGenerator.IsBeingGenerated(member) && Current.ProgramState == ProgramState.Playing && map != null && map.IsPlayerHome && !this.HostileTo(OfPlayer))
				{
					if (dinfo.HasValue && dinfo.Value.Category == DamageInfo.SourceCategory.Collapse)
					{
						bool flag = MessagesRepeatAvoider.MessageShowAllowed("FactionRelationAdjustmentCrushed-" + Name, 5f);
						Faction ofPlayer = OfPlayer;
						int goodwillChange = (!member.RaceProps.Humanlike) ? (-15) : (-25);
						bool canSendMessage = flag;
						string reason = "GoodwillChangedReason_PawnCrushed".Translate(member.LabelShort, member);
						TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage, canSendHostilityLetter: true, reason, new TargetInfo(member.Position, map));
					}
					else if (dinfo.HasValue && (dinfo.Value.Instigator == null || dinfo.Value.Instigator.Faction == null))
					{
						Faction ofPlayer = OfPlayer;
						int goodwillChange = (!member.RaceProps.Humanlike) ? (-3) : (-5);
						string reason = "GoodwillChangedReason_PawnDied".Translate(member.LabelShort, member);
						GlobalTargetInfo? lookTarget = member;
						TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, reason, lookTarget);
					}
				}
				if (member == leader)
				{
					Notify_LeaderDied();
				}
			}
		}

		public void Notify_LeaderDied()
		{
			Pawn pawn = leader;
			GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeadersDeathLabel".Translate(name, def.leaderTitle).CapitalizeFirst(), "LetterLeadersDeath".Translate(pawn.Name.ToStringFull, name, leader.Name.ToStringFull, def.leaderTitle, pawn.Named("OLDLEADER"), leader.Named("NEWLEADER")).CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this);
		}

		public void Notify_LeaderLost()
		{
			Pawn pawn = leader;
			GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeaderChangedLabel".Translate(name, def.leaderTitle).CapitalizeFirst(), "LetterLeaderChanged".Translate(pawn.Name.ToStringFull, name, leader.Name.ToStringFull, def.leaderTitle, pawn.Named("OLDLEADER"), leader.Named("NEWLEADER")).CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this);
		}

		public void Notify_RelationKindChanged(Faction other, FactionRelationKind previousKind, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter)
		{
			if (Current.ProgramState != ProgramState.Playing || other != OfPlayer)
			{
				canSendLetter = false;
			}
			sentLetter = false;
			FactionRelationKind factionRelationKind = RelationKindWith(other);
			if (factionRelationKind == FactionRelationKind.Hostile && Current.ProgramState == ProgramState.Playing)
			{
				foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive.ToList())
				{
					if ((item.Faction == this && item.HostFaction == other) || (item.Faction == other && item.HostFaction == this))
					{
						item.guest.SetGuestStatus(item.HostFaction, prisoner: true);
					}
				}
			}
			if (other == OfPlayer && !this.HostileTo(OfPlayer))
			{
				List<Site> list = new List<Site>();
				List<Site> sites = Find.WorldObjects.Sites;
				for (int i = 0; i < sites.Count; i++)
				{
					if (sites[i].factionMustRemainHostile && sites[i].Faction == this && !sites[i].HasMap)
					{
						list.Add(sites[i]);
					}
				}
				if (list.Any())
				{
					string label;
					string text;
					if (list.Count == 1)
					{
						label = "LetterLabelSiteNoLongerHostile".Translate();
						text = "LetterSiteNoLongerHostile".Translate(Name, list[0].Label);
					}
					else
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int j = 0; j < list.Count; j++)
						{
							if (stringBuilder.Length != 0)
							{
								stringBuilder.AppendLine();
							}
							stringBuilder.Append("  - " + list[j].LabelCap);
							ImportantPawnComp component = list[j].GetComponent<ImportantPawnComp>();
							if (component != null && component.pawn.Any)
							{
								stringBuilder.Append(" (" + component.pawn[0].LabelCap + ")");
							}
						}
						label = "LetterLabelSiteNoLongerHostileMulti".Translate();
						text = "LetterSiteNoLongerHostileMulti".Translate(Name) + ":\n\n" + stringBuilder;
					}
					Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new LookTargets(from x in list
					select new GlobalTargetInfo(x.Tile)));
					for (int k = 0; k < list.Count; k++)
					{
						Find.WorldObjects.Remove(list[k]);
					}
				}
			}
			if (other == OfPlayer && this.HostileTo(OfPlayer))
			{
				List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
				for (int l = 0; l < allWorldObjects.Count; l++)
				{
					if (allWorldObjects[l].Faction == this)
					{
						TradeRequestComp component2 = allWorldObjects[l].GetComponent<TradeRequestComp>();
						if (component2 != null && component2.ActiveRequest)
						{
							component2.Disable();
						}
					}
				}
			}
			if (canSendLetter)
			{
				string text2 = string.Empty;
				TryAppendRelationKindChangedInfo(ref text2, previousKind, factionRelationKind, reason);
				switch (factionRelationKind)
				{
				case FactionRelationKind.Hostile:
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Hostile".Translate(name), text2, LetterDefOf.NegativeEvent, lookTarget, this);
					sentLetter = true;
					break;
				case FactionRelationKind.Ally:
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Ally".Translate(name), text2, LetterDefOf.PositiveEvent, lookTarget, this);
					sentLetter = true;
					break;
				case FactionRelationKind.Neutral:
					if (previousKind == FactionRelationKind.Hostile)
					{
						Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromHostile".Translate(name), text2, LetterDefOf.PositiveEvent, lookTarget, this);
						sentLetter = true;
					}
					else
					{
						Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromAlly".Translate(name), text2, LetterDefOf.NeutralEvent, lookTarget, this);
						sentLetter = true;
					}
					break;
				}
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				List<Map> maps = Find.Maps;
				for (int m = 0; m < maps.Count; m++)
				{
					maps[m].attackTargetsCache.Notify_FactionHostilityChanged(this, other);
					LordManager lordManager = maps[m].lordManager;
					for (int n = 0; n < lordManager.lords.Count; n++)
					{
						Lord lord = lordManager.lords[n];
						if (lord.faction == other)
						{
							lord.Notify_FactionRelationsChanged(this, previousKind);
						}
						else if (lord.faction == this)
						{
							lord.Notify_FactionRelationsChanged(other, previousKind);
						}
					}
				}
			}
		}

		public void Notify_PlayerTraded(float marketValueSentByPlayer, Pawn playerNegotiator)
		{
			Faction ofPlayer = OfPlayer;
			int goodwillChange = (int)(marketValueSentByPlayer / 600f);
			string reason = "GoodwillChangedReason_Traded".Translate();
			GlobalTargetInfo? lookTarget = playerNegotiator;
			TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, reason, lookTarget);
		}

		public void Notify_MemberExitedMap(Pawn member, bool free)
		{
			if (free && member.HostFaction != null && member.guest != null && (member.guest.Released || !member.IsPrisoner))
			{
				bool flag = false;
				float num = 0f;
				if (!member.InMentalState && member.health.hediffSet.BleedRateTotal < 0.001f)
				{
					flag = true;
					num += 12f;
					if (PawnUtility.IsFactionLeader(member))
					{
						num += 40f;
					}
				}
				num += (float)Mathf.Min(member.mindState.timesGuestTendedToByPlayer, 10) * 1f;
				Faction hostFaction = member.HostFaction;
				int goodwillChange = (int)num;
				string reason = (!flag) ? "GoodwillChangedReason_Tended".Translate(member.LabelShort, member) : "GoodwillChangedReason_ExitedMapHealthy".Translate(member.LabelShort, member);
				TryAffectGoodwillWith(hostFaction, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, reason);
			}
			member.mindState.timesGuestTendedToByPlayer = 0;
		}

		public void GenerateNewLeader()
		{
			leader = null;
			if (def.pawnGroupMakers != null)
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (PawnGroupMaker item in from x in def.pawnGroupMakers
				where x.kindDef == PawnGroupKindDefOf.Combat
				select x)
				{
					foreach (PawnGenOption option in item.options)
					{
						if (option.kind.factionLeader)
						{
							list.Add(option.kind);
						}
					}
				}
				if (list.TryRandomElement(out PawnKindDef result))
				{
					leader = PawnGenerator.GeneratePawn(result, this);
					if (leader.RaceProps.IsFlesh)
					{
						leader.relations.everSeenByPlayer = true;
					}
					if (!Find.WorldPawns.Contains(leader))
					{
						Find.WorldPawns.PassToWorld(leader, PawnDiscardDecideMode.KeepForever);
					}
				}
			}
		}

		public string GetCallLabel()
		{
			return name;
		}

		public string GetInfoText()
		{
			string labelCap = def.LabelCap;
			string text = labelCap;
			return text + "\n" + "goodwill".Translate().CapitalizeFirst() + ": " + PlayerGoodwill.ToStringWithSign();
		}

		Faction ICommunicable.GetFaction()
		{
			return this;
		}

		public void TryOpenComms(Pawn negotiator)
		{
			Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, FactionDialogMaker.FactionDialogFor(negotiator, this), radioMode: true);
			dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
			Find.WindowStack.Add(dialog_Negotiation);
		}

		private bool LeaderIsAvailableToTalk()
		{
			if (leader == null)
			{
				return false;
			}
			if (leader.Spawned && (leader.Downed || leader.IsPrisoner || !leader.Awake() || leader.InMentalState))
			{
				return false;
			}
			return true;
		}

		public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
		{
			if (IsPlayer)
			{
				return null;
			}
			string text = "CallOnRadio".Translate(GetCallLabel());
			string text2 = text;
			text = text2 + " (" + PlayerRelationKind.GetLabel() + ", " + PlayerGoodwill.ToStringWithSign() + ")";
			if (!LeaderIsAvailableToTalk())
			{
				string str = (leader == null) ? "LeaderUnavailableNoLeader".Translate() : "LeaderUnavailable".Translate(leader.LabelShort, leader);
				return new FloatMenuOption(text + " (" + str + ")", null);
			}
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
			{
				console.GiveUseCommsJob(negotiator, this);
			}, MenuOptionPriority.InitiateSocial), negotiator, console);
		}

		private void TookDamageFromPredator(Pawn predator)
		{
			for (int i = 0; i < predatorThreats.Count; i++)
			{
				if (predatorThreats[i].predator == predator)
				{
					predatorThreats[i].lastAttackTicks = Find.TickManager.TicksGame;
					return;
				}
			}
			PredatorThreat predatorThreat = new PredatorThreat();
			predatorThreat.predator = predator;
			predatorThreat.lastAttackTicks = Find.TickManager.TicksGame;
			predatorThreats.Add(predatorThreat);
		}

		public bool HasPredatorRecentlyAttackedAnyone(Pawn predator)
		{
			for (int i = 0; i < predatorThreats.Count; i++)
			{
				if (predatorThreats[i].predator == predator)
				{
					return true;
				}
			}
			return false;
		}

		private bool IsMutuallyHostileCrossfire(DamageInfo dinfo)
		{
			return dinfo.Instigator != null && dinfo.IntendedTarget != null && dinfo.IntendedTarget.HostileTo(dinfo.Instigator) && dinfo.IntendedTarget.HostileTo(this);
		}

		public string GetUniqueLoadID()
		{
			return "Faction_" + loadID;
		}

		public override string ToString()
		{
			if (name != null)
			{
				return name;
			}
			if (def != null)
			{
				return def.defName;
			}
			return "[faction of no def]";
		}
	}
}
