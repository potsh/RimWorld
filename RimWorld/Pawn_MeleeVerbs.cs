using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_MeleeVerbs : IExposable
	{
		private Pawn pawn;

		private Verb curMeleeVerb;

		private Thing curMeleeVerbTarget;

		private int curMeleeVerbUpdateTick;

		private Pawn_MeleeVerbs_TerrainSource terrainVerbs;

		public int lastTerrainBasedVerbUseTick = -99999;

		private static List<VerbEntry> meleeVerbs = new List<VerbEntry>();

		private const int BestMeleeVerbUpdateInterval = 60;

		public const int TerrainBasedVerbUseDelay = 1200;

		private const float TerrainBasedVerbChooseChance = 0.04f;

		public Pawn Pawn => pawn;

		public Pawn_MeleeVerbs(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public static void PawnMeleeVerbsStaticUpdate()
		{
			meleeVerbs.Clear();
		}

		public Verb TryGetMeleeVerb(Thing target)
		{
			if (curMeleeVerb == null || curMeleeVerbTarget != target || Find.TickManager.TicksGame >= curMeleeVerbUpdateTick + 60 || !curMeleeVerb.IsStillUsableBy(pawn) || !curMeleeVerb.IsUsableOn(target))
			{
				ChooseMeleeVerb(target);
			}
			return curMeleeVerb;
		}

		private void ChooseMeleeVerb(Thing target)
		{
			bool flag = Rand.Chance(0.04f);
			List<VerbEntry> updatedAvailableVerbsList = GetUpdatedAvailableVerbsList(flag);
			bool flag2 = false;
			if (updatedAvailableVerbsList.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out VerbEntry result))
			{
				flag2 = true;
			}
			else if (flag)
			{
				updatedAvailableVerbsList = GetUpdatedAvailableVerbsList(terrainTools: false);
				flag2 = updatedAvailableVerbsList.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out result);
			}
			if (flag2)
			{
				SetCurMeleeVerb(result.verb, target);
			}
			else
			{
				Log.ErrorOnce(pawn.ToStringSafe() + " has no available melee attack, spawned=" + pawn.Spawned + " dead=" + pawn.Dead + " downed=" + pawn.Downed + " curJob=" + pawn.CurJob.ToStringSafe() + " verbList=" + updatedAvailableVerbsList.ToStringSafeEnumerable() + " bodyVerbs=" + pawn.verbTracker.AllVerbs.ToStringSafeEnumerable(), pawn.thingIDNumber ^ 0xBACB2DA);
				SetCurMeleeVerb(null, null);
			}
		}

		public bool TryMeleeAttack(Thing target, Verb verbToUse = null, bool surpriseAttack = false)
		{
			if (pawn.stances.FullBodyBusy)
			{
				return false;
			}
			if (verbToUse != null)
			{
				if (!verbToUse.IsStillUsableBy(pawn))
				{
					return false;
				}
				if (!verbToUse.IsMeleeAttack)
				{
					Log.Warning("Pawn " + pawn + " tried to melee attack " + target + " with non melee-attack verb " + verbToUse + ".");
					return false;
				}
			}
			Verb verb = (verbToUse == null) ? TryGetMeleeVerb(target) : verbToUse;
			if (verb == null)
			{
				return false;
			}
			verb.TryStartCastOn(target, surpriseAttack);
			return true;
		}

		public List<VerbEntry> GetUpdatedAvailableVerbsList(bool terrainTools)
		{
			meleeVerbs.Clear();
			if (!terrainTools)
			{
				List<Verb> allVerbs = pawn.verbTracker.AllVerbs;
				for (int i = 0; i < allVerbs.Count; i++)
				{
					if (allVerbs[i].IsStillUsableBy(pawn))
					{
						meleeVerbs.Add(new VerbEntry(allVerbs[i], pawn));
					}
				}
				if (pawn.equipment != null)
				{
					List<ThingWithComps> allEquipmentListForReading = pawn.equipment.AllEquipmentListForReading;
					for (int j = 0; j < allEquipmentListForReading.Count; j++)
					{
						ThingWithComps thingWithComps = allEquipmentListForReading[j];
						CompEquippable comp = thingWithComps.GetComp<CompEquippable>();
						if (comp != null)
						{
							List<Verb> allVerbs2 = comp.AllVerbs;
							if (allVerbs2 != null)
							{
								for (int k = 0; k < allVerbs2.Count; k++)
								{
									if (allVerbs2[k].IsStillUsableBy(pawn))
									{
										meleeVerbs.Add(new VerbEntry(allVerbs2[k], pawn));
									}
								}
							}
						}
					}
				}
				if (pawn.apparel != null)
				{
					List<Apparel> wornApparel = pawn.apparel.WornApparel;
					for (int l = 0; l < wornApparel.Count; l++)
					{
						Apparel apparel = wornApparel[l];
						CompEquippable comp2 = apparel.GetComp<CompEquippable>();
						if (comp2 != null)
						{
							List<Verb> allVerbs3 = comp2.AllVerbs;
							if (allVerbs3 != null)
							{
								for (int m = 0; m < allVerbs3.Count; m++)
								{
									if (allVerbs3[m].IsStillUsableBy(pawn))
									{
										meleeVerbs.Add(new VerbEntry(allVerbs3[m], pawn));
									}
								}
							}
						}
					}
				}
				foreach (Verb hediffsVerb in pawn.health.hediffSet.GetHediffsVerbs())
				{
					if (hediffsVerb.IsStillUsableBy(pawn))
					{
						meleeVerbs.Add(new VerbEntry(hediffsVerb, pawn));
					}
				}
			}
			else if (pawn.Spawned)
			{
				TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
				if (terrainVerbs == null || terrainVerbs.def != terrain)
				{
					terrainVerbs = Pawn_MeleeVerbs_TerrainSource.Create(this, terrain);
				}
				List<Verb> allVerbs4 = terrainVerbs.tracker.AllVerbs;
				for (int n = 0; n < allVerbs4.Count; n++)
				{
					Verb verb = allVerbs4[n];
					if (verb.IsStillUsableBy(pawn))
					{
						meleeVerbs.Add(new VerbEntry(verb, pawn));
					}
				}
			}
			return meleeVerbs;
		}

		public void Notify_PawnKilled()
		{
			SetCurMeleeVerb(null, null);
		}

		public void Notify_PawnDespawned()
		{
			SetCurMeleeVerb(null, null);
		}

		public void Notify_UsedTerrainBasedVerb()
		{
			lastTerrainBasedVerbUseTick = Find.TickManager.TicksGame;
		}

		private void SetCurMeleeVerb(Verb v, Thing target)
		{
			curMeleeVerb = v;
			curMeleeVerbTarget = target;
			if (Current.ProgramState != ProgramState.Playing)
			{
				curMeleeVerbUpdateTick = 0;
			}
			else
			{
				curMeleeVerbUpdateTick = Find.TickManager.TicksGame;
			}
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && curMeleeVerb != null && !curMeleeVerb.IsStillUsableBy(pawn))
			{
				curMeleeVerb = null;
			}
			Scribe_References.Look(ref curMeleeVerb, "curMeleeVerb");
			Scribe_Values.Look(ref curMeleeVerbUpdateTick, "curMeleeVerbUpdateTick", 0);
			Scribe_Deep.Look(ref terrainVerbs, "terrainVerbs");
			Scribe_Values.Look(ref lastTerrainBasedVerbUseTick, "lastTerrainBasedVerbUseTick", -99999);
			if (Scribe.mode == LoadSaveMode.LoadingVars && terrainVerbs != null)
			{
				terrainVerbs.parent = this;
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit && curMeleeVerb != null && curMeleeVerb.BuggedAfterLoading)
			{
				curMeleeVerb = null;
				Log.Warning(pawn.ToStringSafe() + " had a bugged melee verb after loading.");
			}
		}
	}
}
