using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class ResearchProjectDef : Def
	{
		public float baseCost = 100f;

		public List<ResearchProjectDef> prerequisites;

		public TechLevel techLevel;

		public List<ResearchProjectDef> requiredByThis;

		private List<ResearchMod> researchMods;

		public ThingDef requiredResearchBuilding;

		public List<ThingDef> requiredResearchFacilities;

		public List<ResearchProjectTagDef> tags;

		public ResearchTabDef tab;

		public float researchViewX = 1f;

		public float researchViewY = 1f;

		[MustTranslate]
		public string discoveredLetterTitle;

		[MustTranslate]
		public string discoveredLetterText;

		public int discoveredLetterMinDifficulty;

		public bool unlockExtremeDifficulty;

		[Unsaved]
		private float x = 1f;

		[Unsaved]
		private float y = 1f;

		[Unsaved]
		private bool positionModified;

		public const TechLevel MaxEffectiveTechLevel = TechLevel.Industrial;

		private const float ResearchCostFactorPerTechLevelDiff = 0.5f;

		public float ResearchViewX => x;

		public float ResearchViewY => y;

		public float CostApparent => baseCost * CostFactor(Faction.OfPlayer.def.techLevel);

		public float ProgressReal => Find.ResearchManager.GetProgress(this);

		public float ProgressApparent => ProgressReal * CostFactor(Faction.OfPlayer.def.techLevel);

		public float ProgressPercent => Find.ResearchManager.GetProgress(this) / baseCost;

		public bool IsFinished => ProgressReal >= baseCost;

		public bool CanStartNow => !IsFinished && PrerequisitesCompleted && (requiredResearchBuilding == null || PlayerHasAnyAppropriateResearchBench);

		public bool PrerequisitesCompleted
		{
			get
			{
				if (prerequisites != null)
				{
					for (int i = 0; i < prerequisites.Count; i++)
					{
						if (!prerequisites[i].IsFinished)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		private bool PlayerHasAnyAppropriateResearchBench
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Building> allBuildingsColonist = maps[i].listerBuildings.allBuildingsColonist;
					for (int j = 0; j < allBuildingsColonist.Count; j++)
					{
						Building_ResearchBench building_ResearchBench = allBuildingsColonist[j] as Building_ResearchBench;
						if (building_ResearchBench != null && CanBeResearchedAt(building_ResearchBench, ignoreResearchBenchPowerStatus: true))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public override void ResolveReferences()
		{
			if (tab == null)
			{
				tab = ResearchTabDefOf.Main;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (techLevel != 0)
			{
				if (!(ResearchViewX < 0f) && !(ResearchViewY < 0f))
				{
					List<ResearchProjectDef> rpDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
					int i = 0;
					while (true)
					{
						if (i >= rpDefs.Count)
						{
							yield break;
						}
						if (rpDefs[i] != this && rpDefs[i].tab == tab && rpDefs[i].ResearchViewX == ResearchViewX && rpDefs[i].ResearchViewY == ResearchViewY)
						{
							break;
						}
						i++;
					}
					yield return "same research view coords and tab as " + rpDefs[i] + ": " + ResearchViewX + ", " + ResearchViewY + "(" + tab + ")";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield return "researchViewX and/or researchViewY not set";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return "techLevel is Undefined";
			/*Error: Unable to find new state assignment for yield return*/;
			IL_029c:
			/*Error near IL_029d: Unexpected return in MoveNext()*/;
		}

		public float CostFactor(TechLevel researcherTechLevel)
		{
			TechLevel techLevel = (TechLevel)Mathf.Min((int)this.techLevel, 4);
			if ((int)researcherTechLevel >= (int)techLevel)
			{
				return 1f;
			}
			int num = techLevel - researcherTechLevel;
			return 1f + (float)num * 0.5f;
		}

		public bool HasTag(ResearchProjectTagDef tag)
		{
			if (tags == null)
			{
				return false;
			}
			return tags.Contains(tag);
		}

		public bool CanBeResearchedAt(Building_ResearchBench bench, bool ignoreResearchBenchPowerStatus)
		{
			if (requiredResearchBuilding != null && bench.def != requiredResearchBuilding)
			{
				return false;
			}
			if (!ignoreResearchBenchPowerStatus)
			{
				CompPowerTrader comp = bench.GetComp<CompPowerTrader>();
				if (comp != null && !comp.PowerOn)
				{
					return false;
				}
			}
			if (!requiredResearchFacilities.NullOrEmpty())
			{
				CompAffectedByFacilities affectedByFacilities = bench.TryGetComp<CompAffectedByFacilities>();
				if (affectedByFacilities == null)
				{
					return false;
				}
				List<Thing> linkedFacilitiesListForReading = affectedByFacilities.LinkedFacilitiesListForReading;
				int i;
				for (i = 0; i < requiredResearchFacilities.Count; i++)
				{
					if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredResearchFacilities[i] && affectedByFacilities.IsFacilityActive(x)) == null)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void ReapplyAllMods()
		{
			if (researchMods != null)
			{
				for (int i = 0; i < researchMods.Count; i++)
				{
					try
					{
						researchMods[i].Apply();
					}
					catch (Exception ex)
					{
						Log.Error("Exception applying research mod for project " + this + ": " + ex.ToString());
					}
				}
			}
		}

		public static ResearchProjectDef Named(string defName)
		{
			return DefDatabase<ResearchProjectDef>.GetNamed(defName);
		}

		public static void GenerateNonOverlappingCoordinates()
		{
			foreach (ResearchProjectDef item in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
			{
				item.x = item.researchViewX;
				item.y = item.researchViewY;
			}
			int num = 0;
			do
			{
				bool flag = false;
				foreach (ResearchProjectDef item2 in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
				{
					foreach (ResearchProjectDef item3 in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
					{
						if (item2 != item3 && item2.tab == item3.tab)
						{
							bool flag2 = Mathf.Abs(item2.x - item3.x) < 0.5f;
							bool flag3 = Mathf.Abs(item2.y - item3.y) < 0.25f;
							if (flag2 && flag3)
							{
								flag = true;
								if (item2.x <= item3.x)
								{
									item2.x -= 0.1f;
									item3.x += 0.1f;
								}
								else
								{
									item2.x += 0.1f;
									item3.x -= 0.1f;
								}
								if (item2.y <= item3.y)
								{
									item2.y -= 0.1f;
									item3.y += 0.1f;
								}
								else
								{
									item2.y += 0.1f;
									item3.y -= 0.1f;
								}
								item2.x += 0.001f;
								item2.y += 0.001f;
								item3.x -= 0.001f;
								item3.y -= 0.001f;
								ClampInCoordinateLimits(item2);
								ClampInCoordinateLimits(item3);
							}
						}
					}
				}
				if (!flag)
				{
					return;
				}
				num++;
			}
			while (num <= 200);
			Log.Error("Couldn't relax research project coordinates apart after " + 200 + " passes.");
		}

		private static void ClampInCoordinateLimits(ResearchProjectDef rp)
		{
			if (rp.x < 0f)
			{
				rp.x = 0f;
			}
			if (rp.y < 0f)
			{
				rp.y = 0f;
			}
			if (rp.y > 6.5f)
			{
				rp.y = 6.5f;
			}
		}

		public void Debug_ApplyPositionDelta(Vector2 delta)
		{
			x += delta.x;
			y += delta.y;
			positionModified = true;
		}

		public bool Debug_IsPositionModified()
		{
			return positionModified;
		}
	}
}
