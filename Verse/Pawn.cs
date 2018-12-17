using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public class Pawn : ThingWithComps, IStrippable, IBillGiver, IVerbOwner, ITrader, IAttackTarget, IAttackTargetSearcher, IThingHolder, ILoadReferenceable
	{
		public PawnKindDef kindDef;

		private Name nameInt;

		public Gender gender;

		public Pawn_AgeTracker ageTracker;

		public Pawn_HealthTracker health;

		public Pawn_RecordsTracker records;

		public Pawn_InventoryTracker inventory;

		public Pawn_MeleeVerbs meleeVerbs;

		public VerbTracker verbTracker;

		public Pawn_CarryTracker carryTracker;

		public Pawn_NeedsTracker needs;

		public Pawn_MindState mindState;

		public Pawn_RotationTracker rotationTracker;

		public Pawn_PathFollower pather;

		public Pawn_Thinker thinker;

		public Pawn_JobTracker jobs;

		public Pawn_StanceTracker stances;

		public Pawn_NativeVerbs natives;

		public Pawn_FilthTracker filth;

		public Pawn_EquipmentTracker equipment;

		public Pawn_ApparelTracker apparel;

		public Pawn_Ownership ownership;

		public Pawn_SkillTracker skills;

		public Pawn_StoryTracker story;

		public Pawn_GuestTracker guest;

		public Pawn_GuiltTracker guilt;

		public Pawn_WorkSettings workSettings;

		public Pawn_TraderTracker trader;

		public Pawn_TrainingTracker training;

		public Pawn_CallTracker caller;

		public Pawn_RelationsTracker relations;

		public Pawn_InteractionsTracker interactions;

		public Pawn_PlayerSettings playerSettings;

		public Pawn_OutfitTracker outfits;

		public Pawn_DrugPolicyTracker drugs;

		public Pawn_FoodRestrictionTracker foodRestriction;

		public Pawn_TimetableTracker timetable;

		public Pawn_DraftController drafter;

		private Pawn_DrawTracker drawer;

		private const float HumanSizedHeatOutput = 0.3f;

		private const float AnimalHeatOutputFactor = 0.6f;

		private static string NotSurgeryReadyTrans;

		private static string CannotReachTrans;

		public const int MaxMoveTicks = 450;

		private static List<string> states = new List<string>();

		private int lastSleepDisturbedTick;

		private const int SleepDisturbanceMinInterval = 300;

		Thing IAttackTarget.Thing
		{
			get
			{
				return this;
			}
		}

		Thing IAttackTargetSearcher.Thing
		{
			get
			{
				return this;
			}
		}

		Thing IVerbOwner.ConstantCaster
		{
			get
			{
				return this;
			}
		}

		ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef
		{
			get
			{
				return ImplementOwnerTypeDefOf.Bodypart;
			}
		}

		public Name Name
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

		public RaceProperties RaceProps => def.race;

		public Job CurJob => (jobs == null) ? null : jobs.curJob;

		public JobDef CurJobDef => (CurJob == null) ? null : CurJob.def;

		public bool Downed => health.Downed;

		public bool Dead => health.Dead;

		public string KindLabel => GenLabel.BestKindLabel(this);

		public bool InMentalState
		{
			get
			{
				if (Dead)
				{
					return false;
				}
				return mindState.mentalStateHandler.InMentalState;
			}
		}

		public MentalState MentalState
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.mentalStateHandler.CurState;
			}
		}

		public MentalStateDef MentalStateDef
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.mentalStateHandler.CurStateDef;
			}
		}

		public bool InAggroMentalState
		{
			get
			{
				if (Dead)
				{
					return false;
				}
				return mindState.mentalStateHandler.InMentalState && mindState.mentalStateHandler.CurStateDef.IsAggro;
			}
		}

		public bool Inspired
		{
			get
			{
				if (Dead)
				{
					return false;
				}
				return mindState.inspirationHandler.Inspired;
			}
		}

		public Inspiration Inspiration
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.inspirationHandler.CurState;
			}
		}

		public InspirationDef InspirationDef
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.inspirationHandler.CurStateDef;
			}
		}

		public override Vector3 DrawPos => Drawer.DrawPos;

		public VerbTracker VerbTracker => verbTracker;

		public List<VerbProperties> VerbProperties => def.Verbs;

		public List<Tool> Tools => def.tools;

		public bool IsColonist => base.Faction != null && base.Faction.IsPlayer && RaceProps.Humanlike;

		public bool IsFreeColonist => IsColonist && HostFaction == null;

		public Faction HostFaction
		{
			get
			{
				if (guest == null)
				{
					return null;
				}
				return guest.HostFaction;
			}
		}

		public bool Drafted => drafter != null && drafter.Drafted;

		public bool IsPrisoner => guest != null && guest.IsPrisoner;

		public bool IsPrisonerOfColony => guest != null && guest.IsPrisoner && guest.HostFaction.IsPlayer;

		public bool IsColonistPlayerControlled => base.Spawned && IsColonist && MentalStateDef == null && HostFaction == null;

		public IEnumerable<IntVec3> IngredientStackCells
		{
			get
			{
				yield return InteractionCell;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public bool InContainerEnclosed => base.ParentHolder.IsEnclosingContainer();

		public Corpse Corpse => base.ParentHolder as Corpse;

		public Pawn CarriedBy
		{
			get
			{
				if (base.ParentHolder == null)
				{
					return null;
				}
				return (base.ParentHolder as Pawn_CarryTracker)?.pawn;
			}
		}

		public override string LabelNoCount
		{
			get
			{
				if (Name != null)
				{
					if (story == null || story.TitleShortCap.NullOrEmpty())
					{
						return Name.ToStringShort;
					}
					return Name.ToStringShort + ", " + story.TitleShortCap;
				}
				return KindLabel;
			}
		}

		public override string LabelShort
		{
			get
			{
				if (Name != null)
				{
					return Name.ToStringShort;
				}
				return LabelNoCount;
			}
		}

		public Pawn_DrawTracker Drawer
		{
			get
			{
				if (drawer == null)
				{
					drawer = new Pawn_DrawTracker(this);
				}
				return drawer;
			}
		}

		public BillStack BillStack => health.surgeryBills;

		public override IntVec3 InteractionCell
		{
			get
			{
				Building_Bed building_Bed = this.CurrentBed();
				if (building_Bed != null)
				{
					IntVec3 position = base.Position;
					IntVec3 position2 = base.Position;
					IntVec3 position3 = base.Position;
					IntVec3 position4 = base.Position;
					if (building_Bed.Rotation.IsHorizontal)
					{
						position.z++;
						position2.z--;
						position3.x--;
						position4.x++;
					}
					else
					{
						position.x--;
						position2.x++;
						position3.z++;
						position4.z--;
					}
					if (position.Standable(base.Map) && position.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null && position.GetDoor(base.Map) == null)
					{
						return position;
					}
					if (position2.Standable(base.Map) && position2.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null && position2.GetDoor(base.Map) == null)
					{
						return position2;
					}
					if (position3.Standable(base.Map) && position3.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null && position3.GetDoor(base.Map) == null)
					{
						return position3;
					}
					if (position4.Standable(base.Map) && position4.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null && position4.GetDoor(base.Map) == null)
					{
						return position4;
					}
					if (position.Standable(base.Map) && position.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null)
					{
						return position;
					}
					if (position2.Standable(base.Map) && position2.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null)
					{
						return position2;
					}
					if (position3.Standable(base.Map) && position3.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null)
					{
						return position3;
					}
					if (position4.Standable(base.Map) && position4.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null)
					{
						return position4;
					}
					if (position.Standable(base.Map))
					{
						return position;
					}
					if (position2.Standable(base.Map))
					{
						return position2;
					}
					if (position3.Standable(base.Map))
					{
						return position3;
					}
					if (position4.Standable(base.Map))
					{
						return position4;
					}
				}
				return base.InteractionCell;
			}
		}

		public TraderKindDef TraderKind => (trader == null) ? null : trader.traderKind;

		public IEnumerable<Thing> Goods => trader.Goods;

		public int RandomPriceFactorSeed => trader.RandomPriceFactorSeed;

		public string TraderName => trader.TraderName;

		public bool CanTradeNow => trader != null && trader.CanTradeNow;

		public float TradePriceImprovementOffsetForPlayer => 0f;

		public float BodySize => ageTracker.CurLifeStage.bodySizeFactor * RaceProps.baseBodySize;

		public float HealthScale => ageTracker.CurLifeStage.healthScaleFactor * RaceProps.baseHealthScale;

		public LocalTargetInfo TargetCurrentlyAimingAt
		{
			get
			{
				if (!base.Spawned)
				{
					return LocalTargetInfo.Invalid;
				}
				Stance curStance = stances.curStance;
				if (curStance is Stance_Warmup || curStance is Stance_Cooldown)
				{
					return ((Stance_Busy)curStance).focusTarg;
				}
				return LocalTargetInfo.Invalid;
			}
		}

		public LocalTargetInfo LastAttackedTarget => mindState.lastAttackedTarget;

		public int LastAttackTargetTick => mindState.lastAttackTargetTick;

		public Verb CurrentEffectiveVerb
		{
			get
			{
				Building_Turret building_Turret = this.MannedThing() as Building_Turret;
				if (building_Turret != null)
				{
					return building_Turret.AttackVerb;
				}
				return TryGetAttackVerb(null, !IsColonist);
			}
		}

		public int TicksPerMoveCardinal => TicksPerMove(diagonal: false);

		public int TicksPerMoveDiagonal => TicksPerMove(diagonal: true);

		string IVerbOwner.UniqueVerbOwnerID()
		{
			return GetUniqueLoadID();
		}

		bool IVerbOwner.VerbsStillUsableBy(Pawn p)
		{
			return p == this;
		}

		public int GetRootTile()
		{
			return base.Tile;
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
			if (inventory != null)
			{
				outChildren.Add(inventory);
			}
			if (carryTracker != null)
			{
				outChildren.Add(carryTracker);
			}
			if (equipment != null)
			{
				outChildren.Add(equipment);
			}
			if (apparel != null)
			{
				outChildren.Add(apparel);
			}
		}

		public string GetKindLabelPlural(int count = -1)
		{
			return GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: false, plural: true, count);
		}

		public static void ResetStaticData()
		{
			NotSurgeryReadyTrans = "NotSurgeryReady".Translate();
			CannotReachTrans = "CannotReach".Translate();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref kindDef, "kindDef");
			Scribe_Values.Look(ref gender, "gender", Gender.Male);
			Scribe_Deep.Look(ref nameInt, "name");
			Scribe_Deep.Look(ref mindState, "mindState", this);
			Scribe_Deep.Look(ref jobs, "jobs", this);
			Scribe_Deep.Look(ref stances, "stances", this);
			Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
			Scribe_Deep.Look(ref natives, "natives", this);
			Scribe_Deep.Look(ref meleeVerbs, "meleeVerbs", this);
			Scribe_Deep.Look(ref rotationTracker, "rotationTracker", this);
			Scribe_Deep.Look(ref pather, "pather", this);
			Scribe_Deep.Look(ref carryTracker, "carryTracker", this);
			Scribe_Deep.Look(ref apparel, "apparel", this);
			Scribe_Deep.Look(ref story, "story", this);
			Scribe_Deep.Look(ref equipment, "equipment", this);
			Scribe_Deep.Look(ref drafter, "drafter", this);
			Scribe_Deep.Look(ref ageTracker, "ageTracker", this);
			Scribe_Deep.Look(ref health, "healthTracker", this);
			Scribe_Deep.Look(ref records, "records", this);
			Scribe_Deep.Look(ref inventory, "inventory", this);
			Scribe_Deep.Look(ref filth, "filth", this);
			Scribe_Deep.Look(ref needs, "needs", this);
			Scribe_Deep.Look(ref guest, "guest", this);
			Scribe_Deep.Look(ref guilt, "guilt");
			Scribe_Deep.Look(ref relations, "social", this);
			Scribe_Deep.Look(ref ownership, "ownership", this);
			Scribe_Deep.Look(ref interactions, "interactions", this);
			Scribe_Deep.Look(ref skills, "skills", this);
			Scribe_Deep.Look(ref workSettings, "workSettings", this);
			Scribe_Deep.Look(ref trader, "trader", this);
			Scribe_Deep.Look(ref outfits, "outfits", this);
			Scribe_Deep.Look(ref drugs, "drugs", this);
			Scribe_Deep.Look(ref foodRestriction, "foodRestriction", this);
			Scribe_Deep.Look(ref timetable, "timetable", this);
			Scribe_Deep.Look(ref playerSettings, "playerSettings", this);
			Scribe_Deep.Look(ref training, "training", this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.PawnPostLoadInit(this);
			}
		}

		public override string ToString()
		{
			if (story != null)
			{
				return LabelShort;
			}
			if (thingIDNumber > 0)
			{
				return base.ThingID;
			}
			if (kindDef != null)
			{
				return KindLabel + "_" + base.ThingID;
			}
			if (def != null)
			{
				return base.ThingID;
			}
			return GetType().ToString();
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			if (Dead)
			{
				Log.Warning("Tried to spawn Dead Pawn " + this.ToStringSafe() + ". Replacing with corpse.");
				Corpse corpse = (Corpse)ThingMaker.MakeThing(RaceProps.corpseDef);
				corpse.InnerPawn = this;
				GenSpawn.Spawn(corpse, base.Position, map);
			}
			else if (def == null || kindDef == null)
			{
				Log.Warning("Tried to spawn pawn without def " + this.ToStringSafe() + ".");
			}
			else
			{
				base.SpawnSetup(map, respawningAfterLoad);
				if (Find.WorldPawns.Contains(this))
				{
					Find.WorldPawns.RemovePawn(this);
				}
				PawnComponentsUtility.AddComponentsForSpawn(this);
				if (!PawnUtility.InValidState(this))
				{
					Log.Error("Pawn " + this.ToStringSafe() + " spawned in invalid state. Destroying...");
					try
					{
						DeSpawn();
					}
					catch (Exception ex)
					{
						Log.Error("Tried to despawn " + this.ToStringSafe() + " because of the previous error but couldn't: " + ex);
					}
					Find.WorldPawns.PassToWorld(this, PawnDiscardDecideMode.Discard);
				}
				else
				{
					Drawer.Notify_Spawned();
					rotationTracker.Notify_Spawned();
					if (!respawningAfterLoad)
					{
						pather.ResetToCurrentPosition();
					}
					base.Map.mapPawns.RegisterPawn(this);
					if (RaceProps.IsFlesh)
					{
						relations.everSeenByPlayer = true;
					}
					AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
					if (needs != null && needs.mood != null && needs.mood.recentMemory != null)
					{
						needs.mood.recentMemory.Notify_Spawned(respawningAfterLoad);
					}
					if (!respawningAfterLoad)
					{
						records.AccumulateStoryEvent(StoryEventDefOf.Seen);
						Find.GameEnder.CheckOrUpdateGameOver();
						if (base.Faction == Faction.OfPlayer)
						{
							Find.StoryWatcher.statsRecord.UpdateGreatestPopulation();
						}
						PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(this);
					}
				}
			}
		}

		public override void PostMapInit()
		{
			base.PostMapInit();
			pather.TryResumePathingAfterLoading();
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			Drawer.DrawAt(drawLoc);
		}

		public override void DrawGUIOverlay()
		{
			Drawer.ui.DrawPawnGUIOverlay();
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (IsColonistPlayerControlled)
			{
				if (pather.curPath != null)
				{
					pather.curPath.DrawPath(this);
				}
				jobs.DrawLinesBetweenTargets();
			}
		}

		public override void TickRare()
		{
			base.TickRare();
			if (!base.Suspended)
			{
				if (apparel != null)
				{
					apparel.ApparelTrackerTickRare();
				}
				inventory.InventoryTrackerTickRare();
			}
			if (training != null)
			{
				training.TrainingTrackerTickRare();
			}
			if (base.Spawned && RaceProps.IsFlesh)
			{
				GenTemperature.PushHeat(this, 0.3f * BodySize * 4.16666651f * ((!def.race.Humanlike) ? 0.6f : 1f));
			}
		}

		public override void Tick()
		{
			if (DebugSettings.noAnimals && base.Spawned && RaceProps.Animal)
			{
				Destroy();
			}
			else
			{
				base.Tick();
				if (Find.TickManager.TicksGame % 250 == 0)
				{
					TickRare();
				}
				bool suspended = base.Suspended;
				if (!suspended)
				{
					if (base.Spawned)
					{
						pather.PatherTick();
					}
					if (base.Spawned)
					{
						stances.StanceTrackerTick();
						verbTracker.VerbsTick();
						natives.NativeVerbsTick();
					}
					if (base.Spawned)
					{
						jobs.JobTrackerTick();
					}
					if (base.Spawned)
					{
						Drawer.DrawTrackerTick();
						rotationTracker.RotationTrackerTick();
					}
					health.HealthTick();
					if (!Dead)
					{
						mindState.MindStateTick();
						carryTracker.CarryHandsTick();
					}
				}
				if (!Dead)
				{
					needs.NeedsTrackerTick();
				}
				if (!suspended)
				{
					if (equipment != null)
					{
						equipment.EquipmentTrackerTick();
					}
					if (apparel != null)
					{
						apparel.ApparelTrackerTick();
					}
					if (interactions != null && base.Spawned)
					{
						interactions.InteractionsTrackerTick();
					}
					if (caller != null)
					{
						caller.CallTrackerTick();
					}
					if (skills != null)
					{
						skills.SkillsTick();
					}
					if (inventory != null)
					{
						inventory.InventoryTrackerTick();
					}
					if (drafter != null)
					{
						drafter.DraftControllerTick();
					}
					if (relations != null)
					{
						relations.RelationsTrackerTick();
					}
					if (RaceProps.Humanlike)
					{
						guest.GuestTrackerTick();
					}
					ageTracker.AgeTick();
					records.RecordsTick();
				}
			}
		}

		public void TickMothballed(int interval)
		{
			if (!base.Suspended)
			{
				ageTracker.AgeTickMothballed(interval);
				records.RecordsTickMothballed(interval);
			}
		}

		public void Notify_Teleported(bool endCurrentJob = true, bool resetTweenedPos = true)
		{
			if (resetTweenedPos)
			{
				Drawer.tweener.ResetTweenedPosToRoot();
			}
			pather.Notify_Teleported_Int();
			if (endCurrentJob && jobs != null && jobs.curJob != null)
			{
				jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public void Notify_PassedToWorld()
		{
			if (((base.Faction == null && RaceProps.Humanlike) || (base.Faction != null && base.Faction.IsPlayer) || base.Faction == Faction.OfAncients || base.Faction == Faction.OfAncientsHostile) && !Dead && Find.WorldPawns.GetSituation(this) == WorldPawnSituation.Free)
			{
				bool tryMedievalOrBetter = base.Faction != null && (int)base.Faction.def.techLevel >= 3;
				if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out Faction faction, tryMedievalOrBetter))
				{
					if (base.Faction != faction)
					{
						SetFaction(faction);
					}
				}
				else if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter, allowDefeated: true))
				{
					if (base.Faction != faction)
					{
						SetFaction(faction);
					}
				}
				else if (base.Faction != null)
				{
					SetFaction(null);
				}
			}
			if (!this.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(this))
			{
				ClearMind();
			}
			if (relations != null)
			{
				relations.Notify_PassedToWorld();
			}
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(ref dinfo, out absorbed);
			if (!absorbed)
			{
				if (story != null && story.traits.HasTrait(TraitDefOf.Tough) && dinfo.Def.ExternalViolenceFor(this))
				{
					dinfo.SetAmount(dinfo.Amount * 0.5f);
				}
				health.PreApplyDamage(dinfo, out absorbed);
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostApplyDamage(dinfo, totalDamageDealt);
			if (dinfo.Def.ExternalViolenceFor(this))
			{
				records.AddTo(RecordDefOf.DamageTaken, totalDamageDealt);
			}
			if (dinfo.Def.makesBlood && !dinfo.InstantPermanentInjury && totalDamageDealt > 0f && Rand.Chance(0.5f))
			{
				health.DropBloodFilth();
			}
			records.AccumulateStoryEvent(StoryEventDefOf.DamageTaken);
			health.PostApplyDamage(dinfo, totalDamageDealt);
			if (!Dead)
			{
				mindState.Notify_DamageTaken(dinfo);
			}
		}

		public override Thing SplitOff(int count)
		{
			if (count <= 0 || count >= stackCount)
			{
				return base.SplitOff(count);
			}
			throw new NotImplementedException("Split off on Pawns is not supported (unless we're taking a full stack).");
		}

		private int TicksPerMove(bool diagonal)
		{
			float num = this.GetStatValue(StatDefOf.MoveSpeed);
			if (RestraintsUtility.InRestraints(this))
			{
				num *= 0.35f;
			}
			if (carryTracker != null && carryTracker.CarriedThing != null && carryTracker.CarriedThing.def.category == ThingCategory.Pawn)
			{
				num *= 0.6f;
			}
			float num2 = num / 60f;
			float num3;
			if (num2 == 0f)
			{
				num3 = 450f;
			}
			else
			{
				num3 = 1f / num2;
				if (base.Spawned && !base.Map.roofGrid.Roofed(base.Position))
				{
					num3 /= base.Map.weatherManager.CurMoveSpeedMultiplier;
				}
				if (diagonal)
				{
					num3 *= 1.41421f;
				}
			}
			int value = Mathf.RoundToInt(num3);
			return Mathf.Clamp(value, 1, 450);
		}

		public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
		{
			IntVec3 positionHeld = base.PositionHeld;
			Map map = base.Map;
			Map mapHeld = base.MapHeld;
			bool flag = base.Spawned;
			bool spawnedOrAnyParentSpawned = base.SpawnedOrAnyParentSpawned;
			bool wasWorldPawn = this.IsWorldPawn();
			Caravan caravan = this.GetCaravan();
			Building_Grave assignedGrave = null;
			if (ownership != null)
			{
				assignedGrave = ownership.AssignedGrave;
			}
			bool flag2 = this.InBed();
			float bedRotation = 0f;
			if (flag2)
			{
				bedRotation = this.CurrentBed().Rotation.AsAngle;
			}
			ThingOwner thingOwner = null;
			bool inContainerEnclosed = InContainerEnclosed;
			if (inContainerEnclosed)
			{
				thingOwner = holdingOwner;
				thingOwner.Remove(this);
			}
			bool flag3 = false;
			bool flag4 = false;
			if (Current.ProgramState == ProgramState.Playing && map != null)
			{
				flag3 = (map.designationManager.DesignationOn(this, DesignationDefOf.Hunt) != null);
				flag4 = (map.designationManager.DesignationOn(this, DesignationDefOf.Slaughter) != null);
			}
			bool flag5 = PawnUtility.ShouldSendNotificationAbout(this) && (!flag4 || !dinfo.HasValue || dinfo.Value.Def != DamageDefOf.ExecutionCut);
			float num = 0f;
			Thing attachment = this.GetAttachment(ThingDefOf.Fire);
			if (attachment != null)
			{
				num = ((Fire)attachment).CurrentSize();
			}
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(this, dinfo, PawnDiedOrDownedThoughtsKind.Died);
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.Storyteller.Notify_PawnEvent(this, AdaptationEvent.Died);
			}
			if (IsColonist)
			{
				Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
			}
			if (flag && dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(this))
			{
				LifeStageUtility.PlayNearestLifestageSound(this, (LifeStageAge ls) => ls.soundDeath);
			}
			if (dinfo.HasValue && dinfo.Value.Instigator != null)
			{
				Pawn pawn = dinfo.Value.Instigator as Pawn;
				if (pawn != null)
				{
					RecordsUtility.Notify_PawnKilled(this, pawn);
					if (IsColonist)
					{
						pawn.records.AccumulateStoryEvent(StoryEventDefOf.KilledPlayer);
					}
				}
			}
			TaleUtility.Notify_PawnDied(this, dinfo);
			if (flag)
			{
				Find.BattleLog.Add(new BattleLogEntry_StateTransition(this, RaceProps.DeathActionWorker.DeathRules, (!dinfo.HasValue) ? null : (dinfo.Value.Instigator as Pawn), exactCulprit, (!dinfo.HasValue) ? null : dinfo.Value.HitPart));
			}
			health.surgeryBills.Clear();
			if (apparel != null)
			{
				apparel.Notify_PawnKilled(dinfo);
			}
			if (RaceProps.IsFlesh)
			{
				relations.Notify_PawnKilled(dinfo, map);
			}
			meleeVerbs.Notify_PawnKilled();
			Pawn_CarryTracker pawn_CarryTracker = base.ParentHolder as Pawn_CarryTracker;
			if (pawn_CarryTracker != null && holdingOwner.TryDrop(this, pawn_CarryTracker.pawn.Position, pawn_CarryTracker.pawn.Map, ThingPlaceMode.Near, out Thing _))
			{
				map = pawn_CarryTracker.pawn.Map;
				flag = true;
			}
			health.SetDead();
			if (health.deflectionEffecter != null)
			{
				health.deflectionEffecter.Cleanup();
				health.deflectionEffecter = null;
			}
			caravan?.Notify_MemberDied(this);
			this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.IncappedOrKilled, dinfo);
			if (flag)
			{
				DropAndForbidEverything();
			}
			if (flag)
			{
				DeSpawn();
			}
			Corpse corpse = null;
			if (!PawnGenerator.IsBeingGenerated(this))
			{
				if (inContainerEnclosed)
				{
					corpse = MakeCorpse(assignedGrave, flag2, bedRotation);
					if (!thingOwner.TryAdd(corpse))
					{
						corpse.Destroy();
						corpse = null;
					}
				}
				else if (spawnedOrAnyParentSpawned)
				{
					if (holdingOwner != null)
					{
						holdingOwner.Remove(this);
					}
					corpse = MakeCorpse(assignedGrave, flag2, bedRotation);
					if (GenPlace.TryPlaceThing(corpse, positionHeld, mapHeld, ThingPlaceMode.Direct))
					{
						corpse.Rotation = base.Rotation;
						if (HuntJobUtility.WasKilledByHunter(this, dinfo))
						{
							((Pawn)dinfo.Value.Instigator).Reserve(corpse, ((Pawn)dinfo.Value.Instigator).CurJob);
						}
						else if (!flag3 && !flag4)
						{
							corpse.SetForbiddenIfOutsideHomeArea();
						}
						if (num > 0f)
						{
							FireUtility.TryStartFireIn(corpse.Position, corpse.Map, num);
						}
					}
					else
					{
						corpse.Destroy();
						corpse = null;
					}
				}
				else if (caravan != null && caravan.Spawned)
				{
					corpse = MakeCorpse(assignedGrave, flag2, bedRotation);
					caravan.AddPawnOrItem(corpse, addCarriedPawnToWorldPawnsIfAny: true);
				}
				else if (holdingOwner != null || this.IsWorldPawn())
				{
					Corpse.PostCorpseDestroy(this);
				}
				else
				{
					corpse = MakeCorpse(assignedGrave, flag2, bedRotation);
				}
			}
			if (corpse != null)
			{
				Hediff firstHediffOfDef = health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
				CompRottable comp = corpse.GetComp<CompRottable>();
				if (firstHediffOfDef != null && Rand.Value < firstHediffOfDef.Severity)
				{
					comp?.RotImmediately();
				}
			}
			if (!base.Destroyed)
			{
				Destroy(DestroyMode.KillFinalize);
			}
			PawnComponentsUtility.RemoveComponentsOnKilled(this);
			health.hediffSet.DirtyCache();
			PortraitsCache.SetDirty(this);
			for (int i = 0; i < health.hediffSet.hediffs.Count; i++)
			{
				health.hediffSet.hediffs[i].Notify_PawnDied();
			}
			if (base.Faction != null)
			{
				base.Faction.Notify_MemberDied(this, dinfo, wasWorldPawn, mapHeld);
			}
			if (corpse != null)
			{
				if (RaceProps.DeathActionWorker != null && flag)
				{
					RaceProps.DeathActionWorker.PawnDied(corpse);
				}
				if (Find.Scenario != null)
				{
					Find.Scenario.Notify_PawnDied(corpse);
				}
			}
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				BillUtility.Notify_ColonistUnavailable(this);
			}
			if (spawnedOrAnyParentSpawned)
			{
				GenHostility.Notify_PawnLostForTutor(this, mapHeld);
			}
			if (base.Faction != null && base.Faction.IsPlayer && Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
			if (flag5)
			{
				health.NotifyPlayerOfKilled(dinfo, exactCulprit, caravan);
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode != 0 && mode != DestroyMode.KillFinalize)
			{
				Log.Error("Destroyed pawn " + this + " with unsupported mode " + mode + ".");
			}
			base.Destroy(mode);
			Find.WorldPawns.Notify_PawnDestroyed(this);
			if (ownership != null)
			{
				ownership.UnclaimAll();
			}
			ClearMind(ifLayingKeepLaying: false, clearInspiration: true);
			Lord lord = this.GetLord();
			if (lord != null)
			{
				PawnLostCondition cond = (mode != DestroyMode.KillFinalize) ? PawnLostCondition.Vanished : PawnLostCondition.IncappedOrKilled;
				lord.Notify_PawnLost(this, cond);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.GameEnder.CheckOrUpdateGameOver();
				Find.TaleManager.Notify_PawnDestroyed(this);
			}
			foreach (Pawn item in from p in PawnsFinder.AllMapsWorldAndTemporary_Alive
			where p.playerSettings != null && p.playerSettings.Master == this
			select p)
			{
				item.playerSettings.Master = null;
			}
			if (mode != DestroyMode.KillFinalize)
			{
				if (equipment != null)
				{
					equipment.DestroyAllEquipment();
				}
				inventory.DestroyAll();
				if (apparel != null)
				{
					apparel.DestroyAll();
				}
			}
			WorldPawns worldPawns = Find.WorldPawns;
			if (!worldPawns.IsBeingDiscarded(this) && !worldPawns.Contains(this))
			{
				worldPawns.PassToWorld(this);
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			if (jobs != null && jobs.curJob != null)
			{
				jobs.StopAll();
			}
			base.DeSpawn(mode);
			if (pather != null)
			{
				pather.StopDead();
			}
			if (needs != null && needs.mood != null)
			{
				needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
			if (meleeVerbs != null)
			{
				meleeVerbs.Notify_PawnDespawned();
			}
			ClearAllReservations(releaseDestinationsOnlyIfObsolete: false);
			map?.mapPawns.DeRegisterPawn(this);
			PawnComponentsUtility.RemoveComponentsOnDespawned(this);
		}

		public override void Discard(bool silentlyRemoveReferences = false)
		{
			if (Find.WorldPawns.Contains(this))
			{
				Log.Warning("Tried to discard a world pawn " + this + ".");
			}
			else
			{
				base.Discard(silentlyRemoveReferences);
				if (relations != null)
				{
					relations.ClearAllRelations();
				}
				if (Current.ProgramState == ProgramState.Playing)
				{
					Find.PlayLog.Notify_PawnDiscarded(this, silentlyRemoveReferences);
					Find.BattleLog.Notify_PawnDiscarded(this, silentlyRemoveReferences);
					Find.TaleManager.Notify_PawnDiscarded(this, silentlyRemoveReferences);
				}
				foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive)
				{
					if (item.needs.mood != null)
					{
						item.needs.mood.thoughts.memories.Notify_PawnDiscarded(this);
					}
				}
				Corpse.PostCorpseDestroy(this);
			}
		}

		private Corpse MakeCorpse(Building_Grave assignedGrave, bool inBed, float bedRotation)
		{
			if (holdingOwner != null)
			{
				Log.Warning("We can't make corpse because the pawn is in a ThingOwner. Remove him from the container first. This should have been already handled before calling this method. holder=" + base.ParentHolder);
				return null;
			}
			Corpse corpse = (Corpse)ThingMaker.MakeThing(RaceProps.corpseDef);
			corpse.InnerPawn = this;
			if (assignedGrave != null)
			{
				corpse.InnerPawn.ownership.ClaimGrave(assignedGrave);
			}
			if (inBed)
			{
				corpse.InnerPawn.Drawer.renderer.wiggler.SetToCustomRotation(bedRotation + 180f);
			}
			return corpse;
		}

		public void ExitMap(bool allowedToJoinOrCreateCaravan, Rot4 exitDir)
		{
			if (this.IsWorldPawn())
			{
				Log.Warning("Called ExitMap() on world pawn " + this);
			}
			else if (allowedToJoinOrCreateCaravan && CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this))
			{
				CaravanExitMapUtility.ExitMapAndJoinOrCreateCaravan(this, exitDir);
			}
			else
			{
				this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ExitedMap);
				if (carryTracker != null && carryTracker.CarriedThing != null)
				{
					Pawn pawn = carryTracker.CarriedThing as Pawn;
					if (pawn != null)
					{
						if (base.Faction != null && base.Faction != pawn.Faction)
						{
							base.Faction.kidnapped.Kidnap(pawn, this);
						}
						else
						{
							carryTracker.innerContainer.Remove(pawn);
							pawn.ExitMap(allowedToJoinOrCreateCaravan: false, exitDir);
						}
					}
					else
					{
						carryTracker.CarriedThing.Destroy();
					}
					carryTracker.innerContainer.Clear();
				}
				bool flag = !this.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(this);
				if (base.Faction != null)
				{
					base.Faction.Notify_MemberExitedMap(this, flag);
				}
				if (ownership != null && flag)
				{
					ownership.UnclaimAll();
				}
				if (guest != null)
				{
					if (flag)
					{
						guest.SetGuestStatus(null);
					}
					guest.Released = false;
				}
				if (base.Spawned)
				{
					DeSpawn();
				}
				inventory.UnloadEverything = false;
				if (flag)
				{
					ClearMind();
				}
				if (relations != null)
				{
					relations.Notify_ExitedMap();
				}
				Find.WorldPawns.PassToWorld(this);
			}
		}

		public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PreTraded(action, playerNegotiator, trader);
			if (base.SpawnedOrAnyParentSpawned)
			{
				DropAndForbidEverything();
			}
			if (ownership != null)
			{
				ownership.UnclaimAll();
			}
			if (guest != null)
			{
				guest.SetGuestStatus(null);
			}
			switch (action)
			{
			case TradeAction.PlayerBuys:
				SetFaction(Faction.OfPlayer);
				break;
			case TradeAction.PlayerSells:
				if (RaceProps.Humanlike)
				{
					TaleRecorder.RecordTale(TaleDefOf.SoldPrisoner, playerNegotiator, this, trader);
				}
				if (base.Faction != null)
				{
					SetFaction(null);
				}
				if (RaceProps.IsFlesh)
				{
					relations.Notify_PawnSold(playerNegotiator);
				}
				if (RaceProps.Humanlike)
				{
					GenGuest.AddPrisonerSoldThoughts(this);
				}
				break;
			}
			ClearMind();
		}

		public void PreKidnapped(Pawn kidnapper)
		{
			Find.Storyteller.Notify_PawnEvent(this, AdaptationEvent.Kidnapped);
			if (IsColonist && kidnapper != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.KidnappedColonist, kidnapper, this);
			}
			if (ownership != null)
			{
				ownership.UnclaimAll();
			}
			if (guest != null)
			{
				guest.SetGuestStatus(null);
			}
			if (RaceProps.IsFlesh)
			{
				relations.Notify_PawnKidnapped();
			}
			ClearMind();
		}

		public override void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			if (newFaction == base.Faction)
			{
				Log.Warning("Used SetFaction to change " + this.ToStringSafe() + " to same faction " + newFaction.ToStringSafe());
			}
			else
			{
				Faction faction = base.Faction;
				if (guest != null)
				{
					guest.SetGuestStatus(null);
				}
				if (base.Spawned)
				{
					base.Map.mapPawns.DeRegisterPawn(this);
					base.Map.pawnDestinationReservationManager.ReleaseAllClaimedBy(this);
					base.Map.designationManager.RemoveAllDesignationsOn(this);
				}
				if ((newFaction == Faction.OfPlayer || base.Faction == Faction.OfPlayer) && Current.ProgramState == ProgramState.Playing)
				{
					Find.ColonistBar.MarkColonistsDirty();
				}
				this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ChangedFaction);
				if (base.Faction != null && base.Faction.leader == this)
				{
					base.Faction.Notify_LeaderLost();
				}
				if (newFaction == Faction.OfPlayer && RaceProps.Humanlike)
				{
					ChangeKind(newFaction.def.basicMemberKind);
				}
				base.SetFaction(newFaction);
				PawnComponentsUtility.AddAndRemoveDynamicComponents(this);
				if (base.Faction != null && base.Faction.IsPlayer)
				{
					if (workSettings != null)
					{
						workSettings.EnableAndInitialize();
					}
					Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(this, PopAdaptationEvent.GainedColonist);
				}
				if (Drafted)
				{
					drafter.Drafted = false;
				}
				ReachabilityUtility.ClearCacheFor(this);
				health.surgeryBills.Clear();
				if (base.Spawned)
				{
					base.Map.mapPawns.RegisterPawn(this);
				}
				GenerateNecessaryName();
				if (playerSettings != null)
				{
					playerSettings.ResetMedicalCare();
				}
				ClearMind(ifLayingKeepLaying: true);
				if (!Dead && needs.mood != null)
				{
					needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
				}
				if (base.Spawned)
				{
					base.Map.attackTargetsCache.UpdateTarget(this);
				}
				Find.GameEnder.CheckOrUpdateGameOver();
				AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
				if (needs != null)
				{
					needs.AddOrRemoveNeedsAsAppropriate();
				}
				if (playerSettings != null)
				{
					playerSettings.Notify_FactionChanged();
				}
				if (relations != null)
				{
					relations.Notify_ChangedFaction();
				}
				if (RaceProps.Animal && newFaction == Faction.OfPlayer)
				{
					training.SetWantedRecursive(TrainableDefOf.Tameness, checkOn: true);
					training.Train(TrainableDefOf.Tameness, recruiter, complete: true);
				}
				if (faction == Faction.OfPlayer)
				{
					BillUtility.Notify_ColonistUnavailable(this);
				}
				if (newFaction == Faction.OfPlayer)
				{
					Find.StoryWatcher.statsRecord.UpdateGreatestPopulation();
				}
			}
		}

		public void ClearMind(bool ifLayingKeepLaying = false, bool clearInspiration = false)
		{
			if (pather != null)
			{
				pather.StopDead();
			}
			if (mindState != null)
			{
				mindState.Reset(clearInspiration);
			}
			if (jobs != null)
			{
				jobs.StopAll(ifLayingKeepLaying);
			}
			VerifyReservations();
		}

		public void ClearAllReservations(bool releaseDestinationsOnlyIfObsolete = true)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (releaseDestinationsOnlyIfObsolete)
				{
					maps[i].pawnDestinationReservationManager.ReleaseAllObsoleteClaimedBy(this);
				}
				else
				{
					maps[i].pawnDestinationReservationManager.ReleaseAllClaimedBy(this);
				}
				maps[i].reservationManager.ReleaseAllClaimedBy(this);
				maps[i].physicalInteractionReservationManager.ReleaseAllClaimedBy(this);
				maps[i].attackTargetReservationManager.ReleaseAllClaimedBy(this);
			}
		}

		public void ClearReservationsForJob(Job job)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].pawnDestinationReservationManager.ReleaseClaimedBy(this, job);
				maps[i].reservationManager.ReleaseClaimedBy(this, job);
				maps[i].physicalInteractionReservationManager.ReleaseClaimedBy(this, job);
				maps[i].attackTargetReservationManager.ReleaseClaimedBy(this, job);
			}
		}

		public void VerifyReservations()
		{
			if (jobs != null && CurJob == null && jobs.jobQueue.Count <= 0 && !jobs.startingNewJob)
			{
				bool flag = false;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					LocalTargetInfo obj = maps[i].reservationManager.FirstReservationFor(this);
					if (obj.IsValid)
					{
						Log.ErrorOnce($"Reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {obj.ToStringSafe()}", 0x5D3DFA5 ^ thingIDNumber);
						flag = true;
					}
					LocalTargetInfo obj2 = maps[i].physicalInteractionReservationManager.FirstReservationFor(this);
					if (obj2.IsValid)
					{
						Log.ErrorOnce($"Physical interaction reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {obj2.ToStringSafe()}", 0x12ADECD ^ thingIDNumber);
						flag = true;
					}
					IAttackTarget attackTarget = maps[i].attackTargetReservationManager.FirstReservationFor(this);
					if (attackTarget != null)
					{
						Log.ErrorOnce($"Attack target reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {attackTarget.ToStringSafe()}", 0x5FD7206 ^ thingIDNumber);
						flag = true;
					}
					IntVec3 obj3 = maps[i].pawnDestinationReservationManager.FirstObsoleteReservationFor(this);
					if (obj3.IsValid)
					{
						Job job = maps[i].pawnDestinationReservationManager.FirstObsoleteReservationJobFor(this);
						Log.ErrorOnce($"Pawn destination reservation manager failed to clean up properly; {this.ToStringSafe()}/{job.ToStringSafe()}/{job.def.ToStringSafe()} still reserving {obj3.ToStringSafe()}", 0x1DE312 ^ thingIDNumber);
						flag = true;
					}
				}
				if (flag)
				{
					ClearAllReservations();
				}
			}
		}

		public void DropAndForbidEverything(bool keepInventoryAndEquipmentIfInBed = false)
		{
			if (kindDef.destroyGearOnDrop)
			{
				equipment.DestroyAllEquipment();
				apparel.DestroyAll();
			}
			if (InContainerEnclosed)
			{
				if (carryTracker != null && carryTracker.CarriedThing != null)
				{
					carryTracker.innerContainer.TryTransferToContainer(carryTracker.CarriedThing, holdingOwner);
				}
				if (equipment != null && equipment.Primary != null)
				{
					equipment.TryTransferEquipmentToContainer(equipment.Primary, holdingOwner);
				}
				if (inventory != null)
				{
					inventory.innerContainer.TryTransferAllToContainer(holdingOwner);
				}
			}
			else if (base.SpawnedOrAnyParentSpawned)
			{
				if (carryTracker != null && carryTracker.CarriedThing != null)
				{
					carryTracker.TryDropCarriedThing(base.PositionHeld, ThingPlaceMode.Near, out Thing _);
				}
				if (!keepInventoryAndEquipmentIfInBed || !this.InBed())
				{
					if (equipment != null)
					{
						equipment.DropAllEquipment(base.PositionHeld);
					}
					if (inventory != null && inventory.innerContainer.TotalStackCount > 0)
					{
						inventory.DropAllNearPawn(base.PositionHeld, forbid: true);
					}
				}
			}
		}

		public void GenerateNecessaryName()
		{
			if (base.Faction == Faction.OfPlayer && RaceProps.Animal && (Name == null || Name.Numerical))
			{
				if (Rand.Value < RaceProps.nameOnTameChance)
				{
					Name = PawnBioAndNameGenerator.GeneratePawnName(this);
				}
				else
				{
					Name = PawnBioAndNameGenerator.GeneratePawnName(this, NameStyle.Numeric);
				}
			}
		}

		public Verb TryGetAttackVerb(Thing target, bool allowManualCastWeapons = false)
		{
			if (equipment != null && equipment.Primary != null && equipment.PrimaryEq.PrimaryVerb.Available() && (!equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast || (CurJob != null && CurJob.def != JobDefOf.Wait_Combat) || allowManualCastWeapons))
			{
				return equipment.PrimaryEq.PrimaryVerb;
			}
			return meleeVerbs.TryGetMeleeVerb(target);
		}

		public bool TryStartAttack(LocalTargetInfo targ)
		{
			if (stances.FullBodyBusy)
			{
				return false;
			}
			if (story != null && story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return false;
			}
			bool allowManualCastWeapons = !IsColonist;
			return TryGetAttackVerb(targ.Thing, allowManualCastWeapons)?.TryStartCastOn(targ) ?? false;
		}

		public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
		{
			if (RaceProps.meatDef != null)
			{
				int meatCount = GenMath.RoundRandom(this.GetStatValue(StatDefOf.MeatAmount) * efficiency);
				if (meatCount > 0)
				{
					Thing meat = ThingMaker.MakeThing(RaceProps.meatDef);
					meat.stackCount = meatCount;
					yield return meat;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Thing> enumerator = base.ButcherProducts(butcher, efficiency).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Thing t = enumerator.Current;
					yield return t;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (RaceProps.leatherDef != null)
			{
				int leatherCount = GenMath.RoundRandom(this.GetStatValue(StatDefOf.LeatherAmount) * efficiency);
				if (leatherCount > 0)
				{
					Thing leather = ThingMaker.MakeThing(RaceProps.leatherDef);
					leather.stackCount = leatherCount;
					yield return leather;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (!RaceProps.Humanlike)
			{
				_003CButcherProducts_003Ec__Iterator1 _003CButcherProducts_003Ec__Iterator = (_003CButcherProducts_003Ec__Iterator1)/*Error near IL_0210: stateMachine*/;
				PawnKindLifeStage lifeStage = ageTracker.CurKindLifeStage;
				if (lifeStage.butcherBodyPart != null && (gender == Gender.None || (gender == Gender.Male && lifeStage.butcherBodyPart.allowMale) || (gender == Gender.Female && lifeStage.butcherBodyPart.allowFemale)))
				{
					BodyPartRecord record = (from x in health.hediffSet.GetNotMissingParts()
					where x.IsInGroup(lifeStage.butcherBodyPart.bodyPartGroup)
					select x).FirstOrDefault();
					if (record != null)
					{
						health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this, record));
						Thing thing = (lifeStage.butcherBodyPart.thing == null) ? ThingMaker.MakeThing(record.def.spawnThingOnRemoved) : ThingMaker.MakeThing(lifeStage.butcherBodyPart.thing);
						yield return thing;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_03b1:
			/*Error near IL_03b2: Unexpected return in MoveNext()*/;
		}

		public string MainDesc(bool writeAge)
		{
			string text = GenLabel.BestKindLabel(this, mustNoteGender: true, mustNoteLifeStage: true);
			if (base.Faction != null && !base.Faction.def.hidden)
			{
				text = "PawnMainDescFactionedWrap".Translate(text, base.Faction.Name);
			}
			if (writeAge && ageTracker != null)
			{
				text = text + ", " + "AgeIndicator".Translate(ageTracker.AgeNumberString);
			}
			return text.CapitalizeFirst();
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(MainDesc(writeAge: true));
			if (TraderKind != null)
			{
				stringBuilder.AppendLine(TraderKind.LabelCap);
			}
			if (InMentalState)
			{
				stringBuilder.AppendLine(MentalState.InspectLine);
			}
			states.Clear();
			if (stances != null && stances.stunner != null && stances.stunner.Stunned)
			{
				states.AddDistinct("StunLower".Translate());
			}
			if (health != null && health.hediffSet != null)
			{
				List<Hediff> hediffs = health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					Hediff hediff = hediffs[i];
					if (!hediff.def.battleStateLabel.NullOrEmpty())
					{
						states.AddDistinct(hediff.def.battleStateLabel);
					}
				}
			}
			if (states.Count > 0)
			{
				states.Sort();
				stringBuilder.AppendLine(string.Format("{0}: {1}", "State".Translate(), states.ToCommaList().CapitalizeFirst()));
				states.Clear();
			}
			if (Inspired)
			{
				stringBuilder.AppendLine(Inspiration.InspectLine);
			}
			if (equipment != null && equipment.Primary != null)
			{
				stringBuilder.AppendLine("Equipped".Translate() + ": " + ((equipment.Primary == null) ? "EquippedNothing".Translate() : equipment.Primary.Label).CapitalizeFirst());
			}
			if (carryTracker != null && carryTracker.CarriedThing != null)
			{
				stringBuilder.Append("Carrying".Translate() + ": ");
				stringBuilder.AppendLine(carryTracker.CarriedThing.LabelCap);
			}
			if ((base.Faction == Faction.OfPlayer || HostFaction == Faction.OfPlayer) && !InMentalState)
			{
				string text = null;
				Lord lord = this.GetLord();
				if (lord != null && lord.LordJob != null)
				{
					text = lord.LordJob.GetReport();
				}
				if (jobs.curJob != null)
				{
					try
					{
						string text2 = jobs.curDriver.GetReport().CapitalizeFirst();
						text = (text.NullOrEmpty() ? text2 : (text + ": " + text2));
					}
					catch (Exception arg)
					{
						Log.Error("JobDriver.GetReport() exception: " + arg);
					}
				}
				if (!text.NullOrEmpty())
				{
					stringBuilder.AppendLine(text);
				}
			}
			if (jobs.curJob != null && jobs.jobQueue.Count > 0)
			{
				try
				{
					string text3 = jobs.jobQueue[0].job.GetReport(this).CapitalizeFirst();
					if (jobs.jobQueue.Count > 1)
					{
						string text4 = text3;
						text3 = text4 + " (+" + (jobs.jobQueue.Count - 1) + ")";
					}
					stringBuilder.AppendLine("Queued".Translate() + ": " + text3);
				}
				catch (Exception arg2)
				{
					Log.Error("JobDriver.GetReport() exception: " + arg2);
				}
			}
			if (RestraintsUtility.ShouldShowRestraintsInfo(this))
			{
				stringBuilder.AppendLine("InRestraints".Translate());
			}
			stringBuilder.Append(InspectStringPartsFromComps());
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (IsColonistPlayerControlled)
			{
				using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Gizmo c2 = enumerator.Current;
						yield return c2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				if (drafter != null)
				{
					using (IEnumerator<Gizmo> enumerator2 = drafter.GetGizmos().GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							Gizmo c = enumerator2.Current;
							yield return c;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				using (IEnumerator<Gizmo> enumerator3 = PawnAttackGizmoUtility.GetAttackGizmos(this).GetEnumerator())
				{
					if (enumerator3.MoveNext())
					{
						Gizmo attack = enumerator3.Current;
						yield return attack;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (equipment != null)
			{
				using (IEnumerator<Gizmo> enumerator4 = equipment.GetGizmos().GetEnumerator())
				{
					if (enumerator4.MoveNext())
					{
						Gizmo g4 = enumerator4.Current;
						yield return g4;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (IsColonistPlayerControlled)
			{
				if (apparel != null)
				{
					using (IEnumerator<Gizmo> enumerator5 = apparel.GetGizmos().GetEnumerator())
					{
						if (enumerator5.MoveNext())
						{
							Gizmo g3 = enumerator5.Current;
							yield return g3;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				if (playerSettings != null)
				{
					using (IEnumerator<Gizmo> enumerator6 = playerSettings.GetGizmos().GetEnumerator())
					{
						if (enumerator6.MoveNext())
						{
							Gizmo g2 = enumerator6.Current;
							yield return g2;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			using (IEnumerator<Gizmo> enumerator7 = mindState.GetGizmos().GetEnumerator())
			{
				if (enumerator7.MoveNext())
				{
					Gizmo g = enumerator7.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0498:
			/*Error near IL_0499: Unexpected return in MoveNext()*/;
		}

		public virtual IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
		{
			yield break;
		}

		public override TipSignal GetTooltip()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(LabelCap);
			string text = string.Empty;
			if (gender != 0)
			{
				text = this.GetGenderLabel();
			}
			if (!LabelCap.EqualsIgnoreCase(KindLabel))
			{
				if (text != string.Empty)
				{
					text += " ";
				}
				text += KindLabel;
			}
			if (text != string.Empty)
			{
				stringBuilder.Append(" (" + text + ")");
			}
			stringBuilder.AppendLine();
			if (equipment != null && equipment.Primary != null)
			{
				stringBuilder.AppendLine(equipment.Primary.LabelCap);
			}
			stringBuilder.AppendLine(HealthUtility.GetGeneralConditionLabel(this));
			return new TipSignal(stringBuilder.ToString().TrimEndNewlines(), thingIDNumber * 152317, TooltipPriority.Pawn);
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			using (IEnumerator<StatDrawEntry> enumerator = base.SpecialDisplayStats().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					StatDrawEntry s = enumerator.Current;
					yield return s;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "BodySize".Translate(), BodySize.ToString("F2"), 0, string.Empty);
			/*Error: Unable to find new state assignment for yield return*/;
			IL_0109:
			/*Error near IL_010a: Unexpected return in MoveNext()*/;
		}

		public bool CurrentlyUsableForBills()
		{
			if (!this.InBed())
			{
				JobFailReason.Is(NotSurgeryReadyTrans);
				return false;
			}
			if (!InteractionCell.IsValid)
			{
				JobFailReason.Is(CannotReachTrans);
				return false;
			}
			return true;
		}

		public bool UsableForBillsAfterFueling()
		{
			return CurrentlyUsableForBills();
		}

		public bool AnythingToStrip()
		{
			return (equipment != null && equipment.HasAnything()) || (apparel != null && apparel.WornApparelCount > 0) || (inventory != null && inventory.innerContainer.Count > 0);
		}

		public void Strip()
		{
			Caravan caravan = this.GetCaravan();
			if (caravan != null)
			{
				CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(this, caravan.PawnsListForReading);
				if (apparel != null)
				{
					CaravanInventoryUtility.MoveAllApparelToSomeonesInventory(this, caravan.PawnsListForReading);
				}
				if (equipment != null)
				{
					CaravanInventoryUtility.MoveAllEquipmentToSomeonesInventory(this, caravan.PawnsListForReading);
				}
			}
			else
			{
				IntVec3 pos = (Corpse == null) ? base.PositionHeld : Corpse.PositionHeld;
				if (equipment != null)
				{
					equipment.DropAllEquipment(pos, forbid: false);
				}
				if (apparel != null)
				{
					apparel.DropAll(pos, forbid: false);
				}
				if (inventory != null)
				{
					inventory.DropAllNearPawn(pos);
				}
			}
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

		public void HearClamor(Thing source, ClamorDef type)
		{
			if (!Dead && !Downed)
			{
				if (type == ClamorDefOf.Movement)
				{
					Pawn pawn = source as Pawn;
					if (pawn != null)
					{
						CheckForDisturbedSleep(pawn);
					}
				}
				if (type == ClamorDefOf.Harm && base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction == source.Faction && HostFaction == null)
				{
					mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
					if (CurJob != null)
					{
						jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
				if (type == ClamorDefOf.Construction && base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction != source.Faction && HostFaction == null)
				{
					mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
					if (CurJob != null)
					{
						jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
				if (type == ClamorDefOf.Impact)
				{
					mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
					if (CurJob != null && !this.Awake())
					{
						jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
			}
		}

		private void CheckForDisturbedSleep(Pawn source)
		{
			if (needs.mood != null && !this.Awake() && base.Faction == Faction.OfPlayer && Find.TickManager.TicksGame >= lastSleepDisturbedTick + 300 && (source == null || (!LovePartnerRelationUtility.LovePartnerRelationExists(this, source) && !(source.RaceProps.petness > 0f) && (source.relations == null || !source.relations.DirectRelations.Any((DirectPawnRelation dr) => dr.def == PawnRelationDefOf.Bond)))))
			{
				lastSleepDisturbedTick = Find.TickManager.TicksGame;
				needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed);
			}
		}

		public bool CheckAcceptArrest(Pawn arrester)
		{
			if (health.Downed)
			{
				return true;
			}
			if (story != null && story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return true;
			}
			if (base.Faction != null && base.Faction != arrester.factionInt)
			{
				base.Faction.Notify_MemberCaptured(this, arrester.Faction);
			}
			float num = (!this.IsWildMan()) ? 0.6f : 0.3f;
			if (Rand.Value < num)
			{
				return true;
			}
			Messages.Message("MessageRefusedArrest".Translate(LabelShort, this), this, MessageTypeDefOf.ThreatSmall);
			if (base.Faction == null || !arrester.HostileTo(this))
			{
				mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
			}
			return false;
		}

		public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
		{
			if (!base.Spawned)
			{
				return true;
			}
			if (!InMentalState && this.GetTraderCaravanRole() == TraderCaravanRole.Carrier && !(jobs.curDriver is JobDriver_AttackMelee))
			{
				return true;
			}
			if (mindState.duty != null && mindState.duty.def.threatDisabled)
			{
				return true;
			}
			if (!mindState.Active)
			{
				return true;
			}
			if (Downed)
			{
				if (disabledFor == null)
				{
					return true;
				}
				Pawn pawn = disabledFor.Thing as Pawn;
				if (pawn == null || pawn.mindState == null || pawn.mindState.duty == null || !pawn.mindState.duty.attackDownedIfStarving || !pawn.Starving())
				{
					return true;
				}
			}
			return false;
		}

		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
			if (InAggroMentalState || (base.Faction.HostileTo(Faction.OfPlayer) && HostFaction == null && !Downed && !InMentalState))
			{
				reason = "Enemies".Translate();
				return true;
			}
			reason = null;
			return false;
		}

		public void ChangeKind(PawnKindDef newKindDef)
		{
			if (kindDef != newKindDef)
			{
				kindDef = newKindDef;
				if (kindDef == PawnKindDefOf.WildMan)
				{
					mindState.WildManEverReachedOutside = false;
					ReachabilityUtility.ClearCacheFor(this);
				}
			}
		}
	}
}
