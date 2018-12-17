using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class Hediff : IExposable
	{
		public HediffDef def;

		public int ageTicks;

		private BodyPartRecord part;

		public ThingDef source;

		public BodyPartGroupDef sourceBodyPartGroup;

		public HediffDef sourceHediffDef;

		public int loadID = -1;

		protected float severityInt;

		private bool recordedTale;

		protected bool causesNoPain;

		private bool visible;

		public WeakReference<LogEntry> combatLogEntry;

		public string combatLogText;

		public int temp_partIndexToSetLater = -1;

		[Unsaved]
		public Pawn pawn;

		public virtual string LabelBase => def.label;

		public string LabelBaseCap => LabelBase.CapitalizeFirst();

		public string Label
		{
			get
			{
				string labelInBrackets = LabelInBrackets;
				return LabelBase + ((!labelInBrackets.NullOrEmpty()) ? (" (" + labelInBrackets + ")") : string.Empty);
			}
		}

		public string LabelCap => Label.CapitalizeFirst();

		public virtual Color LabelColor => def.defaultLabelColor;

		public virtual string LabelInBrackets => (CurStage != null && !CurStage.label.NullOrEmpty()) ? CurStage.label : null;

		public virtual string SeverityLabel => (!(def.lethalSeverity <= 0f)) ? (Severity / def.lethalSeverity).ToStringPercent() : null;

		public virtual int UIGroupKey => Label.GetHashCode();

		public virtual string TipStringExtra
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (StatDrawEntry item in HediffStatsUtility.SpecialDisplayStats(CurStage, this))
				{
					if (item.ShouldDisplay)
					{
						stringBuilder.AppendLine(item.LabelCap + ": " + item.ValueString);
					}
				}
				return stringBuilder.ToString();
			}
		}

		public virtual HediffStage CurStage => (!def.stages.NullOrEmpty()) ? def.stages[CurStageIndex] : null;

		public virtual bool ShouldRemove => Severity <= 0f;

		public virtual bool Visible => visible || CurStage == null || CurStage.becomeVisible;

		public virtual float BleedRate => 0f;

		public bool Bleeding => BleedRate > 1E-05f;

		public virtual float PainOffset => (CurStage != null && !causesNoPain) ? CurStage.painOffset : 0f;

		public virtual float PainFactor => (CurStage != null) ? CurStage.painFactor : 1f;

		public List<PawnCapacityModifier> CapMods => (CurStage != null) ? CurStage.capMods : null;

		public virtual float SummaryHealthPercentImpact => 0f;

		public virtual float TendPriority
		{
			get
			{
				float a = 0f;
				HediffStage curStage = CurStage;
				if (curStage != null && curStage.lifeThreatening)
				{
					a = Mathf.Max(a, 1f);
				}
				a = Mathf.Max(a, BleedRate * 1.5f);
				HediffComp_TendDuration hediffComp_TendDuration = this.TryGetComp<HediffComp_TendDuration>();
				if (hediffComp_TendDuration != null && hediffComp_TendDuration.TProps.severityPerDayTended < 0f)
				{
					a = Mathf.Max(a, 0.025f);
				}
				return a;
			}
		}

		public virtual TextureAndColor StateIcon => TextureAndColor.None;

		public virtual int CurStageIndex
		{
			get
			{
				if (def.stages == null)
				{
					return 0;
				}
				List<HediffStage> stages = def.stages;
				float severity = Severity;
				for (int num = stages.Count - 1; num >= 0; num--)
				{
					if (severity >= stages[num].minSeverity)
					{
						return num;
					}
				}
				return 0;
			}
		}

		public virtual float Severity
		{
			get
			{
				return severityInt;
			}
			set
			{
				bool flag = false;
				if (def.lethalSeverity > 0f && value >= def.lethalSeverity)
				{
					value = def.lethalSeverity;
					flag = true;
				}
				bool flag2 = this is Hediff_Injury && value > severityInt && Mathf.RoundToInt(value) != Mathf.RoundToInt(severityInt);
				int curStageIndex = CurStageIndex;
				severityInt = Mathf.Clamp(value, def.minSeverity, def.maxSeverity);
				if ((CurStageIndex != curStageIndex || flag || flag2) && pawn.health.hediffSet.hediffs.Contains(this))
				{
					pawn.health.Notify_HediffChanged(this);
					if (!pawn.Dead && pawn.needs.mood != null)
					{
						pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
					}
				}
			}
		}

		public BodyPartRecord Part
		{
			get
			{
				return part;
			}
			set
			{
				if (pawn == null && part != null)
				{
					Log.Error("Hediff: Cannot set Part without setting pawn first.");
				}
				else if (UnityData.isDebugBuild && part != null && !pawn.RaceProps.body.AllParts.Contains(part))
				{
					Log.Error("Hediff: Cannot set BodyPartRecord which doesn't belong to the pawn " + pawn.ToStringSafe());
				}
				else
				{
					part = value;
				}
			}
		}

		public virtual bool TendableNow(bool ignoreTimer = false)
		{
			if (!def.tendable || Severity <= 0f || this.FullyImmune() || !Visible || this.IsPermanent())
			{
				return false;
			}
			if (!ignoreTimer)
			{
				HediffComp_TendDuration hediffComp_TendDuration = this.TryGetComp<HediffComp_TendDuration>();
				if (hediffComp_TendDuration != null && !hediffComp_TendDuration.AllowTend)
				{
					return false;
				}
			}
			return true;
		}

		public virtual void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && combatLogEntry != null)
			{
				LogEntry target = combatLogEntry.Target;
				if (!Current.Game.battleLog.IsEntryActive(target))
				{
					combatLogEntry = null;
				}
			}
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
			Scribe_Defs.Look(ref source, "source");
			Scribe_Defs.Look(ref sourceBodyPartGroup, "sourceBodyPartGroup");
			Scribe_Defs.Look(ref sourceHediffDef, "sourceHediffDef");
			Scribe_BodyParts.Look(ref part, "part");
			Scribe_Values.Look(ref severityInt, "severity", 0f);
			Scribe_Values.Look(ref recordedTale, "recordedTale", defaultValue: false);
			Scribe_Values.Look(ref causesNoPain, "causesNoPain", defaultValue: false);
			Scribe_Values.Look(ref visible, "visible", defaultValue: false);
			Scribe_References.Look(ref combatLogEntry, "combatLogEntry");
			Scribe_Values.Look(ref combatLogText, "combatLogText");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				BackCompatibility.HediffLoadingVars(this);
			}
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				BackCompatibility.HediffResolvingCrossRefs(this);
			}
		}

		public virtual void Tick()
		{
			ageTicks++;
			if (def.hediffGivers != null && pawn.IsHashIntervalTick(60))
			{
				for (int i = 0; i < def.hediffGivers.Count; i++)
				{
					def.hediffGivers[i].OnIntervalPassed(pawn, this);
				}
			}
			if (Visible && !visible)
			{
				visible = true;
				if (def.taleOnVisible != null)
				{
					TaleRecorder.RecordTale(def.taleOnVisible, pawn, def);
				}
			}
			HediffStage curStage = CurStage;
			if (curStage != null)
			{
				if (curStage.hediffGivers != null && pawn.IsHashIntervalTick(60))
				{
					for (int j = 0; j < curStage.hediffGivers.Count; j++)
					{
						curStage.hediffGivers[j].OnIntervalPassed(pawn, this);
					}
				}
				if (curStage.mentalStateGivers != null && pawn.IsHashIntervalTick(60) && !pawn.InMentalState)
				{
					for (int k = 0; k < curStage.mentalStateGivers.Count; k++)
					{
						MentalStateGiver mentalStateGiver = curStage.mentalStateGivers[k];
						if (Rand.MTBEventOccurs(mentalStateGiver.mtbDays, 60000f, 60f))
						{
							pawn.mindState.mentalStateHandler.TryStartMentalState(mentalStateGiver.mentalState, "MentalStateReason_Hediff".Translate(Label));
						}
					}
				}
				if (curStage.mentalBreakMtbDays > 0f && pawn.IsHashIntervalTick(60) && !pawn.InMentalState && Rand.MTBEventOccurs(curStage.mentalBreakMtbDays, 60000f, 60f) && (from x in DefDatabase<MentalBreakDef>.AllDefsListForReading
				where x.Worker.BreakCanOccur(pawn)
				select x).TryRandomElementByWeight((MentalBreakDef x) => x.Worker.CommonalityFor(pawn), out MentalBreakDef result))
				{
					result.Worker.TryStart(pawn, "MentalStateReason_Hediff".Translate(Label), causedByMood: false);
				}
				if (curStage.vomitMtbDays > 0f && pawn.IsHashIntervalTick(600) && Rand.MTBEventOccurs(curStage.vomitMtbDays, 60000f, 600f) && pawn.Spawned && pawn.Awake())
				{
					pawn.jobs.StartJob(new Job(JobDefOf.Vomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
				}
				if (curStage.forgetMemoryThoughtMtbDays > 0f && pawn.needs.mood != null && pawn.IsHashIntervalTick(400) && Rand.MTBEventOccurs(curStage.forgetMemoryThoughtMtbDays, 60000f, 400f) && pawn.needs.mood.thoughts.memories.Memories.TryRandomElement(out Thought_Memory result2))
				{
					pawn.needs.mood.thoughts.memories.RemoveMemory(result2);
				}
				if (!recordedTale && curStage.tale != null)
				{
					TaleRecorder.RecordTale(curStage.tale, pawn);
					recordedTale = true;
				}
				if (curStage.destroyPart && Part != null && Part != pawn.RaceProps.body.corePart)
				{
					pawn.health.AddHediff(HediffDefOf.MissingBodyPart, Part);
				}
				if (curStage.deathMtbDays > 0f && pawn.IsHashIntervalTick(200) && Rand.MTBEventOccurs(curStage.deathMtbDays, 60000f, 200f))
				{
					bool flag = PawnUtility.ShouldSendNotificationAbout(pawn);
					Caravan caravan = pawn.GetCaravan();
					pawn.Kill(null, null);
					if (flag)
					{
						pawn.health.NotifyPlayerOfKilled(null, this, caravan);
					}
				}
			}
		}

		public virtual void PostMake()
		{
			Severity = Mathf.Max(Severity, def.initialSeverity);
			causesNoPain = (Rand.Value < def.chanceToCauseNoPain);
		}

		public virtual void PostAdd(DamageInfo? dinfo)
		{
		}

		public virtual void PostRemoved()
		{
			if (def.causesNeed != null && !pawn.Dead)
			{
				pawn.needs.AddOrRemoveNeedsAsAppropriate();
			}
		}

		public virtual void PostTick()
		{
		}

		public virtual void Tended(float quality, int batchPosition = 0)
		{
		}

		public virtual void Heal(float amount)
		{
			if (!(amount <= 0f))
			{
				Severity -= amount;
				pawn.health.Notify_HediffChanged(this);
			}
		}

		public virtual void ModifyChemicalEffect(ChemicalDef chem, ref float effect)
		{
		}

		public virtual bool TryMergeWith(Hediff other)
		{
			if (other == null || other.def != def || other.Part != Part)
			{
				return false;
			}
			Severity += other.Severity;
			ageTicks = 0;
			return true;
		}

		public virtual bool CauseDeathNow()
		{
			if (def.lethalSeverity >= 0f)
			{
				return Severity >= def.lethalSeverity;
			}
			return false;
		}

		public virtual void Notify_PawnDied()
		{
		}

		public virtual string DebugString()
		{
			string str = string.Empty;
			if (!Visible)
			{
				str += "hidden\n";
			}
			str = str + "severity: " + Severity.ToString("F3") + ((!(Severity >= def.maxSeverity)) ? string.Empty : " (reached max)");
			if (TendableNow())
			{
				str = str + "\ntend priority: " + TendPriority;
			}
			return str.Indented();
		}

		public override string ToString()
		{
			return "(" + def.defName + ((part == null) ? string.Empty : (" " + part.Label)) + " ticksSinceCreation=" + ageTicks + ")";
		}

		public string GetUniqueLoadID()
		{
			return "Hediff_" + loadID;
		}
	}
}
