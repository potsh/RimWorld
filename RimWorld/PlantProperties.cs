using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlantProperties
	{
		public List<PlantBiomeRecord> wildBiomes;

		public int wildClusterRadius = -1;

		public float wildClusterWeight = 15f;

		public float wildOrder = 2f;

		public bool wildEqualLocalDistribution = true;

		public bool cavePlant;

		public float cavePlantWeight = 1f;

		[NoTranslate]
		public List<string> sowTags = new List<string>();

		public float sowWork = 10f;

		public int sowMinSkill;

		public bool blockAdjacentSow;

		public List<ResearchProjectDef> sowResearchPrerequisites;

		public bool mustBeWildToSow;

		public float harvestWork = 10f;

		public float harvestYield;

		public ThingDef harvestedThingDef;

		[NoTranslate]
		public string harvestTag;

		public float harvestMinGrowth = 0.65f;

		public float harvestAfterGrowth;

		public bool harvestFailable = true;

		public SoundDef soundHarvesting;

		public SoundDef soundHarvestFinish;

		public float growDays = 2f;

		public float lifespanDaysPerGrowDays = 8f;

		public float growMinGlow = 0.51f;

		public float growOptimalGlow = 1f;

		public float fertilityMin = 0.9f;

		public float fertilitySensitivity = 0.5f;

		public bool dieIfLeafless;

		public bool neverBlightable;

		public bool interferesWithRoof;

		public PlantPurpose purpose = PlantPurpose.Misc;

		public float topWindExposure = 0.25f;

		public int maxMeshCount = 1;

		public FloatRange visualSizeRange = new FloatRange(0.9f, 1.1f);

		[NoTranslate]
		private string leaflessGraphicPath;

		[Unsaved]
		public Graphic leaflessGraphic;

		[NoTranslate]
		private string immatureGraphicPath;

		[Unsaved]
		public Graphic immatureGraphic;

		public bool dropLeaves;

		public const int MaxMaxMeshCount = 25;

		public bool Sowable => !sowTags.NullOrEmpty();

		public bool Harvestable => harvestYield > 0.001f;

		public bool HarvestDestroys => harvestAfterGrowth <= 0f;

		public bool IsTree => harvestTag == "Wood";

		public float LifespanDays => growDays * lifespanDaysPerGrowDays;

		public int LifespanTicks => (int)(LifespanDays * 60000f);

		public bool LimitedLifespan => lifespanDaysPerGrowDays > 0f;

		public bool Blightable => Sowable && Harvestable && !neverBlightable;

		public bool GrowsInClusters => wildClusterRadius > 0;

		public void PostLoadSpecial(ThingDef parentDef)
		{
			if (!leaflessGraphicPath.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					leaflessGraphic = GraphicDatabase.Get(parentDef.graphicData.graphicClass, leaflessGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo);
				});
			}
			if (!immatureGraphicPath.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					immatureGraphic = GraphicDatabase.Get(parentDef.graphicData.graphicClass, immatureGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo);
				});
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (maxMeshCount > 25)
			{
				yield return "maxMeshCount > MaxMaxMeshCount";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		internal IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (sowMinSkill <= 0)
			{
				string attributes = string.Empty;
				if (Harvestable)
				{
					if (!attributes.NullOrEmpty())
					{
						attributes += ", ";
					}
					attributes += "Harvestable".Translate();
				}
				if (LimitedLifespan)
				{
					if (!attributes.NullOrEmpty())
					{
						attributes += ", ";
					}
					string text = attributes + "LimitedLifespan".Translate();
				}
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "GrowingTime".Translate(), growDays.ToString("0.##") + " " + "Days".Translate(), 0, string.Empty)
				{
					overrideReportText = "GrowingTimeDesc".Translate()
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MinGrowingSkillToSow".Translate(), sowMinSkill.ToString(), 0, string.Empty);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
