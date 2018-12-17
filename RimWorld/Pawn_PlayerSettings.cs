using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_PlayerSettings : IExposable
	{
		private Pawn pawn;

		private Area areaAllowedInt;

		public int joinTick = -1;

		private Pawn master;

		public bool followDrafted;

		public bool followFieldwork;

		public bool animalsReleased;

		public MedicalCareCategory medCare = MedicalCareCategory.NoMeds;

		public HostilityResponseMode hostilityResponse = HostilityResponseMode.Flee;

		public bool selfTend;

		public int displayOrder;

		public Pawn Master
		{
			get
			{
				return master;
			}
			set
			{
				if (master != value)
				{
					if (value != null && !pawn.training.HasLearned(TrainableDefOf.Obedience))
					{
						Log.ErrorOnce("Attempted to set master for non-obedient pawn", 73908573);
					}
					else
					{
						bool flag = ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn);
						master = value;
						if (pawn.Spawned && (flag || ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn)))
						{
							pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
						}
					}
				}
			}
		}

		public Area EffectiveAreaRestrictionInPawnCurrentMap
		{
			get
			{
				if (areaAllowedInt != null && areaAllowedInt.Map != pawn.MapHeld)
				{
					return null;
				}
				return EffectiveAreaRestriction;
			}
		}

		public Area EffectiveAreaRestriction
		{
			get
			{
				if (!RespectsAllowedArea)
				{
					return null;
				}
				return areaAllowedInt;
			}
		}

		public Area AreaRestriction
		{
			get
			{
				return areaAllowedInt;
			}
			set
			{
				if (areaAllowedInt != value)
				{
					areaAllowedInt = value;
					if (pawn.Spawned && value != null && value == EffectiveAreaRestrictionInPawnCurrentMap && value.TrueCount > 0 && !value[pawn.Position])
					{
						pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
			}
		}

		public bool RespectsAllowedArea
		{
			get
			{
				if (pawn.GetLord() != null)
				{
					return false;
				}
				return pawn.Faction == Faction.OfPlayer && pawn.HostFaction == null;
			}
		}

		public bool RespectsMaster
		{
			get
			{
				if (Master == null)
				{
					return false;
				}
				return pawn.Faction == Faction.OfPlayer && Master.Faction == pawn.Faction;
			}
		}

		public Pawn RespectedMaster => (!RespectsMaster) ? null : Master;

		public bool UsesConfigurableHostilityResponse => pawn.IsColonist && pawn.HostFaction == null;

		public Pawn_PlayerSettings(Pawn pawn)
		{
			this.pawn = pawn;
			if (Current.ProgramState == ProgramState.Playing)
			{
				joinTick = Find.TickManager.TicksGame;
			}
			else
			{
				joinTick = 0;
			}
			Notify_FactionChanged();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref joinTick, "joinTick", 0);
			Scribe_Values.Look(ref animalsReleased, "animalsReleased", defaultValue: false);
			Scribe_Values.Look(ref medCare, "medCare", MedicalCareCategory.NoCare);
			Scribe_References.Look(ref areaAllowedInt, "areaAllowed");
			Scribe_References.Look(ref master, "master");
			Scribe_Values.Look(ref followDrafted, "followDrafted", defaultValue: false);
			Scribe_Values.Look(ref followFieldwork, "followFieldwork", defaultValue: false);
			Scribe_Values.Look(ref hostilityResponse, "hostilityResponse", HostilityResponseMode.Flee);
			Scribe_Values.Look(ref selfTend, "selfTend", defaultValue: false);
			Scribe_Values.Look(ref displayOrder, "displayOrder", 0);
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			if (pawn.Drafted)
			{
				int count = 0;
				bool anyCanRelease = false;
				foreach (Pawn item in PawnUtility.SpawnedMasteredPawns(pawn))
				{
					if (item.training.HasLearned(TrainableDefOf.Release))
					{
						anyCanRelease = true;
						if (ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(item))
						{
							count++;
						}
					}
				}
				if (anyCanRelease)
				{
					Command_Toggle c = new Command_Toggle
					{
						defaultLabel = "CommandReleaseAnimalsLabel".Translate() + ((count == 0) ? string.Empty : (" (" + count + ")")),
						defaultDesc = "CommandReleaseAnimalsDesc".Translate(),
						icon = TexCommand.ReleaseAnimals,
						hotKey = KeyBindingDefOf.Misc7,
						isActive = (() => ((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_016b: stateMachine*/)._0024this.animalsReleased),
						toggleAction = delegate
						{
							((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0182: stateMachine*/)._0024this.animalsReleased = !((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0182: stateMachine*/)._0024this.animalsReleased;
							if (((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0182: stateMachine*/)._0024this.animalsReleased)
							{
								foreach (Pawn item2 in PawnUtility.SpawnedMasteredPawns(((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0182: stateMachine*/)._0024this.pawn))
								{
									if (item2.caller != null)
									{
										item2.caller.Notify_Released();
									}
									item2.jobs.EndCurrentJob(JobCondition.InterruptForced);
								}
							}
						}
					};
					if (count == 0)
					{
						c.Disable("CommandReleaseAnimalsFail_NoAnimals".Translate());
					}
					yield return (Gizmo)c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public void Notify_FactionChanged()
		{
			ResetMedicalCare();
			areaAllowedInt = null;
		}

		public void Notify_MadePrisoner()
		{
			ResetMedicalCare();
		}

		public void ResetMedicalCare()
		{
			if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				if (pawn.Faction == Faction.OfPlayer)
				{
					if (!pawn.RaceProps.Animal)
					{
						if (!pawn.IsPrisoner)
						{
							medCare = Find.PlaySettings.defaultCareForColonyHumanlike;
						}
						else
						{
							medCare = Find.PlaySettings.defaultCareForColonyPrisoner;
						}
					}
					else
					{
						medCare = Find.PlaySettings.defaultCareForColonyAnimal;
					}
				}
				else if (pawn.Faction == null && pawn.RaceProps.Animal)
				{
					medCare = Find.PlaySettings.defaultCareForNeutralAnimal;
				}
				else if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					medCare = Find.PlaySettings.defaultCareForNeutralFaction;
				}
				else
				{
					medCare = Find.PlaySettings.defaultCareForHostileFaction;
				}
			}
		}

		public void Notify_AreaRemoved(Area area)
		{
			if (areaAllowedInt == area)
			{
				areaAllowedInt = null;
			}
		}
	}
}
