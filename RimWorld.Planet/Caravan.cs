using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class Caravan : WorldObject, IThingHolder, IIncidentTarget, ITrader, ILoadReferenceable
	{
		private int uniqueId = -1;

		private string nameInt;

		public ThingOwner<Pawn> pawns;

		public bool autoJoinable;

		public Caravan_PathFollower pather;

		public Caravan_GotoMoteRenderer gotoMote;

		public Caravan_Tweener tweener;

		public Caravan_TraderTracker trader;

		public Caravan_ForageTracker forage;

		public Caravan_NeedsTracker needs;

		public Caravan_CarryTracker carryTracker;

		public Caravan_BedsTracker beds;

		public StoryState storyState;

		private Material cachedMat;

		private bool cachedImmobilized;

		private int cachedImmobilizedForTicks = -99999;

		private Pair<float, float> cachedDaysWorthOfFood;

		private int cachedDaysWorthOfFoodForTicks = -99999;

		public bool notifiedOutOfFood;

		private const int ImmobilizedCacheDuration = 60;

		private const int DaysWorthOfFoodCacheDuration = 3000;

		private static readonly Texture2D SplitCommand = ContentFinder<Texture2D>.Get("UI/Commands/SplitCaravan");

		private static readonly Color PlayerCaravanColor = new Color(1f, 0.863f, 0.33f);

		public List<Pawn> PawnsListForReading => pawns.InnerListForReading;

		public override Material Material
		{
			get
			{
				if (cachedMat == null)
				{
					cachedMat = MaterialPool.MatFrom(color: (base.Faction == null) ? Color.white : ((!base.Faction.IsPlayer) ? base.Faction.Color : PlayerCaravanColor), texPath: def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.DynamicObjectRenderQueue);
				}
				return cachedMat;
			}
		}

		public string Name
		{
			get
			{
				return nameInt;
			}
			set
			{
				nameInt = value;
			}
		}

		public override Vector3 DrawPos => tweener.TweenedPos;

		public bool IsPlayerControlled => base.Faction == Faction.OfPlayer;

		public bool ImmobilizedByMass
		{
			get
			{
				if (Find.TickManager.TicksGame - cachedImmobilizedForTicks < 60)
				{
					return cachedImmobilized;
				}
				cachedImmobilized = (MassUsage > MassCapacity);
				cachedImmobilizedForTicks = Find.TickManager.TicksGame;
				return cachedImmobilized;
			}
		}

		public Pair<float, float> DaysWorthOfFood
		{
			get
			{
				if (Find.TickManager.TicksGame - cachedDaysWorthOfFoodForTicks < 3000)
				{
					return cachedDaysWorthOfFood;
				}
				cachedDaysWorthOfFood = new Pair<float, float>(DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this), DaysUntilRotCalculator.ApproxDaysUntilRot(this));
				cachedDaysWorthOfFoodForTicks = Find.TickManager.TicksGame;
				return cachedDaysWorthOfFood;
			}
		}

		public bool CantMove => NightResting || AllOwnersHaveMentalBreak || AllOwnersDowned || ImmobilizedByMass;

		public float MassCapacity => CollectionsMassCalculator.Capacity(PawnsListForReading);

		public string MassCapacityExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				CollectionsMassCalculator.Capacity(PawnsListForReading, stringBuilder);
				return stringBuilder.ToString();
			}
		}

		public float MassUsage => CollectionsMassCalculator.MassUsage(PawnsListForReading, IgnorePawnsInventoryMode.DontIgnore);

		public bool AllOwnersDowned
		{
			get
			{
				for (int i = 0; i < pawns.Count; i++)
				{
					if (IsOwner(pawns[i]) && !pawns[i].Downed)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool AllOwnersHaveMentalBreak
		{
			get
			{
				for (int i = 0; i < pawns.Count; i++)
				{
					if (IsOwner(pawns[i]) && !pawns[i].InMentalState)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool NightResting
		{
			get
			{
				if (!base.Spawned)
				{
					return false;
				}
				if (pather.Moving && pather.nextTile == pather.Destination && Caravan_PathFollower.IsValidFinalPushDestination(pather.Destination) && Mathf.CeilToInt(pather.nextTileCostLeft / 1f) <= 10000)
				{
					return false;
				}
				return CaravanNightRestUtility.RestingNowAt(base.Tile);
			}
		}

		public int LeftRestTicks
		{
			get
			{
				if (!NightResting)
				{
					return 0;
				}
				return CaravanNightRestUtility.LeftRestTicksAt(base.Tile);
			}
		}

		public int LeftNonRestTicks
		{
			get
			{
				if (NightResting)
				{
					return 0;
				}
				return CaravanNightRestUtility.LeftNonRestTicksAt(base.Tile);
			}
		}

		public override string Label
		{
			get
			{
				if (nameInt != null)
				{
					return nameInt;
				}
				return base.Label;
			}
		}

		public override bool HasName => !nameInt.NullOrEmpty();

		public int TicksPerMove => CaravanTicksPerMoveUtility.GetTicksPerMove(this);

		public override bool AppendFactionToInspectString => false;

		public float Visibility => CaravanVisibilityCalculator.Visibility(this);

		public string VisibilityExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				CaravanVisibilityCalculator.Visibility(this, stringBuilder);
				return stringBuilder.ToString();
			}
		}

		public string TicksPerMoveExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				CaravanTicksPerMoveUtility.GetTicksPerMove(this, stringBuilder);
				return stringBuilder.ToString();
			}
		}

		public IEnumerable<Thing> AllThings => CaravanInventoryUtility.AllInventoryItems(this).Concat(pawns);

		public int ConstantRandSeed => uniqueId ^ 0x2B6813E1;

		public StoryState StoryState => storyState;

		public GameConditionManager GameConditionManager
		{
			get
			{
				Log.ErrorOnce("Attempted to retrieve condition manager directly from caravan", 13291050);
				return null;
			}
		}

		public float PlayerWealthForStoryteller
		{
			get
			{
				if (!IsPlayerControlled)
				{
					return 0f;
				}
				float num = 0f;
				for (int i = 0; i < pawns.Count; i++)
				{
					num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(pawns[i]);
					if (pawns[i].Faction == Faction.OfPlayer)
					{
						num += pawns[i].MarketValue;
					}
				}
				return num * 0.7f;
			}
		}

		public IEnumerable<Pawn> PlayerPawnsForStoryteller
		{
			get
			{
				if (!IsPlayerControlled)
				{
					return Enumerable.Empty<Pawn>();
				}
				return from x in PawnsListForReading
				where x.Faction == Faction.OfPlayer
				select x;
			}
		}

		public FloatRange IncidentPointsRandomFactorRange => StorytellerUtility.CaravanPointsRandomFactorRange;

		public TraderKindDef TraderKind => trader.TraderKind;

		public IEnumerable<Thing> Goods => trader.Goods;

		public int RandomPriceFactorSeed => trader.RandomPriceFactorSeed;

		public string TraderName => trader.TraderName;

		public bool CanTradeNow => trader.CanTradeNow;

		public float TradePriceImprovementOffsetForPlayer => 0f;

		public Caravan()
		{
			pawns = new ThingOwner<Pawn>(this, oneStackOnly: false, LookMode.Reference);
			pather = new Caravan_PathFollower(this);
			gotoMote = new Caravan_GotoMoteRenderer();
			tweener = new Caravan_Tweener(this);
			trader = new Caravan_TraderTracker(this);
			forage = new Caravan_ForageTracker(this);
			needs = new Caravan_NeedsTracker(this);
			carryTracker = new Caravan_CarryTracker(this);
			beds = new Caravan_BedsTracker(this);
			storyState = new StoryState(this);
		}

		public void SetUniqueId(int newId)
		{
			if (uniqueId != -1 || newId < 0)
			{
				Log.Error("Tried to set caravan with uniqueId " + uniqueId + " to have uniqueId " + newId);
			}
			uniqueId = newId;
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			return trader.ColonyThingsWillingToBuy(playerNegotiator);
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			trader.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			trader.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				pawns.RemoveAll((Pawn x) => x.Destroyed);
			}
			Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
			Scribe_Values.Look(ref nameInt, "name");
			Scribe_Deep.Look(ref pawns, "pawns", this);
			Scribe_Values.Look(ref autoJoinable, "autoJoinable", defaultValue: false);
			Scribe_Deep.Look(ref pather, "pather", this);
			Scribe_Deep.Look(ref trader, "trader", this);
			Scribe_Deep.Look(ref forage, "forage", this);
			Scribe_Deep.Look(ref needs, "needs", this);
			Scribe_Deep.Look(ref carryTracker, "carryTracker", this);
			Scribe_Deep.Look(ref beds, "beds", this);
			Scribe_Deep.Look(ref storyState, "storyState", this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.CaravanPostLoadInit(this);
			}
		}

		public override void PostAdd()
		{
			base.PostAdd();
			carryTracker.Notify_CaravanSpawned();
			beds.Notify_CaravanSpawned();
			Find.ColonistBar.MarkColonistsDirty();
		}

		public override void PostRemove()
		{
			base.PostRemove();
			pather.StopDead();
			Find.ColonistBar.MarkColonistsDirty();
		}

		public override void Tick()
		{
			base.Tick();
			CheckAnyNonWorldPawns();
			pather.PatherTick();
			tweener.TweenerTick();
			forage.ForageTrackerTick();
			carryTracker.CarryTrackerTick();
			beds.BedsTrackerTick();
			needs.NeedsTrackerTick();
			CaravanDrugPolicyUtility.CheckTakeScheduledDrugs(this);
			CaravanTendUtility.CheckTend(this);
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			tweener.ResetTweenedPosToRoot();
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (IsPlayerControlled && pather.curPath != null)
			{
				pather.curPath.DrawPath(this);
			}
			gotoMote.RenderMote();
		}

		public void AddPawn(Pawn p, bool addCarriedPawnToWorldPawnsIfAny)
		{
			if (p == null)
			{
				Log.Warning("Tried to add a null pawn to " + this);
			}
			else if (p.Dead)
			{
				Log.Warning("Tried to add " + p + " to " + this + ", but this pawn is dead.");
			}
			else
			{
				Pawn pawn = p.carryTracker.CarriedThing as Pawn;
				if (pawn != null)
				{
					p.carryTracker.innerContainer.Remove(pawn);
				}
				if (p.Spawned)
				{
					p.DeSpawn();
				}
				if (pawns.TryAdd(p))
				{
					if (ShouldAutoCapture(p))
					{
						p.guest.CapturedBy(base.Faction);
					}
					if (pawn != null)
					{
						if (ShouldAutoCapture(pawn))
						{
							pawn.guest.CapturedBy(base.Faction, p);
						}
						AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny);
						if (addCarriedPawnToWorldPawnsIfAny)
						{
							Find.WorldPawns.PassToWorld(pawn);
						}
					}
				}
				else
				{
					Log.Error("Couldn't add pawn " + p + " to caravan.");
				}
			}
		}

		public void AddPawnOrItem(Thing thing, bool addCarriedPawnToWorldPawnsIfAny)
		{
			if (thing == null)
			{
				Log.Warning("Tried to add a null thing to " + this);
			}
			else
			{
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny);
				}
				else
				{
					CaravanInventoryUtility.GiveThing(this, thing);
				}
			}
		}

		public bool ContainsPawn(Pawn p)
		{
			return pawns.Contains(p);
		}

		public void RemovePawn(Pawn p)
		{
			pawns.Remove(p);
		}

		public void RemoveAllPawns()
		{
			pawns.Clear();
		}

		public bool IsOwner(Pawn p)
		{
			return pawns.Contains(p) && CaravanUtility.IsOwner(p, base.Faction);
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].IsColonist)
				{
					num++;
				}
				else if (pawns[i].RaceProps.Animal)
				{
					num2++;
				}
				else if (pawns[i].IsPrisoner)
				{
					num3++;
				}
				if (pawns[i].Downed)
				{
					num4++;
				}
				if (pawns[i].InMentalState)
				{
					num5++;
				}
			}
			stringBuilder.Append("CaravanColonistsCount".Translate(num, (num != 1) ? Faction.OfPlayer.def.pawnsPlural : Faction.OfPlayer.def.pawnSingular));
			if (num2 == 1)
			{
				stringBuilder.Append(", " + "CaravanAnimal".Translate());
			}
			else if (num2 > 1)
			{
				stringBuilder.Append(", " + "CaravanAnimalsCount".Translate(num2));
			}
			if (num3 == 1)
			{
				stringBuilder.Append(", " + "CaravanPrisoner".Translate());
			}
			else if (num3 > 1)
			{
				stringBuilder.Append(", " + "CaravanPrisonersCount".Translate(num3));
			}
			stringBuilder.AppendLine();
			if (num5 > 0)
			{
				stringBuilder.Append("CaravanPawnsInMentalState".Translate(num5));
			}
			if (num4 > 0)
			{
				if (num5 > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append("CaravanPawnsDowned".Translate(num4));
			}
			if (num5 > 0 || num4 > 0)
			{
				stringBuilder.AppendLine();
			}
			if (pather.Moving)
			{
				if (pather.ArrivalAction != null)
				{
					stringBuilder.Append(pather.ArrivalAction.ReportString);
				}
				else
				{
					stringBuilder.Append("CaravanTraveling".Translate());
				}
			}
			else
			{
				SettlementBase settlementBase = CaravanVisitUtility.SettlementVisitedNow(this);
				if (settlementBase != null)
				{
					stringBuilder.Append("CaravanVisiting".Translate(settlementBase.Label));
				}
				else
				{
					stringBuilder.Append("CaravanWaiting".Translate());
				}
			}
			if (pather.Moving)
			{
				float num6 = (float)CaravanArrivalTimeEstimator.EstimatedTicksToArrive(this, allowCaching: true) / 60000f;
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanEstimatedTimeToDestination".Translate(num6.ToString("0.#")));
			}
			if (AllOwnersDowned)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("AllCaravanMembersDowned".Translate());
			}
			else if (AllOwnersHaveMentalBreak)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("AllCaravanMembersMentalBreak".Translate());
			}
			else if (ImmobilizedByMass)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanImmobilizedByMass".Translate());
			}
			if (needs.AnyPawnOutOfFood(out string malnutritionHediff))
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanOutOfFood".Translate());
				if (!malnutritionHediff.NullOrEmpty())
				{
					stringBuilder.Append(" ");
					stringBuilder.Append(malnutritionHediff);
					stringBuilder.Append(".");
				}
			}
			if (!pather.MovingNow)
			{
				int usedBedCount = beds.GetUsedBedCount();
				stringBuilder.AppendLine();
				stringBuilder.Append(CaravanBedUtility.AppendUsingBedsLabel("CaravanResting".Translate(), usedBedCount));
			}
			else
			{
				string inspectStringLine = carryTracker.GetInspectStringLine();
				if (!inspectStringLine.NullOrEmpty())
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(inspectStringLine);
				}
				string inBedForMedicalReasonsInspectStringLine = beds.GetInBedForMedicalReasonsInspectStringLine();
				if (!inBedForMedicalReasonsInspectStringLine.NullOrEmpty())
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(inBedForMedicalReasonsInspectStringLine);
				}
			}
			return stringBuilder.ToString();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (Find.WorldSelector.SingleSelectedObject == this)
			{
				yield return (Gizmo)new Gizmo_CaravanInfo(this);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g2 = enumerator.Current;
					yield return g2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (IsPlayerControlled)
			{
				if (Find.WorldSelector.SingleSelectedObject == this)
				{
					yield return (Gizmo)SettleInEmptyTileUtility.SettleCommand(this);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (Find.WorldSelector.SingleSelectedObject == this && PawnsListForReading.Count((Pawn x) => x.IsColonist) >= 2)
				{
					yield return (Gizmo)new Command_Action
					{
						defaultLabel = "CommandSplitCaravan".Translate(),
						defaultDesc = "CommandSplitCaravanDesc".Translate(),
						icon = SplitCommand,
						action = delegate
						{
							Find.WindowStack.Add(new Dialog_SplitCaravan(((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_01ff: stateMachine*/)._0024this));
						}
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (pather.Moving)
				{
					yield return (Gizmo)new Command_Toggle
					{
						hotKey = KeyBindingDefOf.Misc1,
						isActive = (() => ((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0266: stateMachine*/)._0024this.pather.Paused),
						toggleAction = delegate
						{
							if (((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_027d: stateMachine*/)._0024this.pather.Moving)
							{
								((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_027d: stateMachine*/)._0024this.pather.Paused = !((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_027d: stateMachine*/)._0024this.pather.Paused;
							}
						},
						defaultDesc = "CommandToggleCaravanPauseDesc".Translate(2f.ToString("0.#"), 0.3f.ToStringPercent()),
						icon = TexCommand.PauseCaravan,
						defaultLabel = "CommandPauseCaravan".Translate()
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (CaravanMergeUtility.ShouldShowMergeCommand)
				{
					yield return (Gizmo)CaravanMergeUtility.MergeCommand(this);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				using (IEnumerator<Gizmo> enumerator2 = forage.GetGizmos().GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Gizmo g = enumerator2.Current;
						yield return g;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				foreach (WorldObject item in Find.WorldObjects.ObjectsAt(base.Tile))
				{
					using (IEnumerator<Gizmo> enumerator4 = item.GetCaravanGizmos(this).GetEnumerator())
					{
						if (enumerator4.MoveNext())
						{
							Gizmo gizmo = enumerator4.Current;
							yield return gizmo;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			if (Prefs.DevMode)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "Dev: Mental break",
					action = delegate
					{
						if ((from x in ((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0502: stateMachine*/)._0024this.PawnsListForReading
						where x.RaceProps.Humanlike && !x.InMentalState
						select x).TryRandomElement(out Pawn result))
						{
							result.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Sad);
						}
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_072f:
			/*Error near IL_0730: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			using (IEnumerator<FloatMenuOption> enumerator = base.GetTransportPodsFloatMenuOptions(pods, representative).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption o = enumerator.Current;
					yield return o;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator2 = TransportPodsArrivalAction_GiveToCaravan.GetFloatMenuOptions(representative, pods, this).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					FloatMenuOption f = enumerator2.Current;
					yield return f;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0162:
			/*Error near IL_0163: Unexpected return in MoveNext()*/;
		}

		public void RecacheImmobilizedNow()
		{
			cachedImmobilizedForTicks = -99999;
		}

		public void RecacheDaysWorthOfFood()
		{
			cachedDaysWorthOfFoodForTicks = -99999;
		}

		public virtual void Notify_MemberDied(Pawn member)
		{
			if (!base.Spawned)
			{
				Log.Error("Caravan member died in an unspawned caravan. Unspawned caravans shouldn't be kept for more than a single frame.");
			}
			if (!PawnsListForReading.Any((Pawn x) => x != member && IsOwner(x)))
			{
				RemovePawn(member);
				if (base.Faction == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAllCaravanColonistsDied".Translate(), "LetterAllCaravanColonistsDied".Translate(Name).CapitalizeFirst(), LetterDefOf.NegativeEvent, new GlobalTargetInfo(base.Tile));
				}
				pawns.Clear();
				Find.WorldObjects.Remove(this);
			}
			else
			{
				member.Strip();
				RemovePawn(member);
			}
		}

		public virtual void Notify_Merged(List<Caravan> group)
		{
			notifiedOutOfFood = false;
		}

		public virtual void Notify_StartedTrading()
		{
			notifiedOutOfFood = false;
		}

		private void CheckAnyNonWorldPawns()
		{
			for (int num = pawns.Count - 1; num >= 0; num--)
			{
				if (!pawns[num].IsWorldPawn())
				{
					Log.Error("Caravan member " + pawns[num] + " is not a world pawn. Removing...");
					pawns.Remove(pawns[num]);
				}
			}
		}

		private bool ShouldAutoCapture(Pawn p)
		{
			return CaravanUtility.ShouldAutoCapture(p, base.Faction);
		}

		public void Notify_PawnRemoved(Pawn p)
		{
			Find.ColonistBar.MarkColonistsDirty();
			RecacheImmobilizedNow();
			RecacheDaysWorthOfFood();
			carryTracker.Notify_PawnRemoved();
			beds.Notify_PawnRemoved();
		}

		public void Notify_PawnAdded(Pawn p)
		{
			Find.ColonistBar.MarkColonistsDirty();
			RecacheImmobilizedNow();
			RecacheDaysWorthOfFood();
		}

		public void Notify_DestinationOrPauseStatusChanged()
		{
			RecacheDaysWorthOfFood();
		}

		public void Notify_Teleported()
		{
			tweener.ResetTweenedPosToRoot();
			pather.Notify_Teleported_Int();
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return pawns;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}
	}
}
