using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class BuildableDef : Def
	{
		public List<StatModifier> statBases;

		public Traversability passability;

		public int pathCost;

		public bool pathCostIgnoreRepeat = true;

		public float fertility = -1f;

		public List<ThingDefCountClass> costList;

		public int costStuffCount;

		public List<StuffCategoryDef> stuffCategories;

		public int placingDraggableDimensions;

		public bool clearBuildingArea = true;

		public Rot4 defaultPlacingRot = Rot4.North;

		public float resourcesFractionWhenDeconstructed = 0.75f;

		public TerrainAffordanceDef terrainAffordanceNeeded;

		public List<ThingDef> buildingPrerequisites;

		public List<ResearchProjectDef> researchPrerequisites;

		public int constructionSkillPrerequisite;

		public TechLevel minTechLevelToBuild;

		public TechLevel maxTechLevelToBuild;

		public AltitudeLayer altitudeLayer = AltitudeLayer.Item;

		public EffecterDef repairEffect;

		public EffecterDef constructEffect;

		public bool menuHidden;

		public float specialDisplayRadius;

		public List<Type> placeWorkers;

		public DesignationCategoryDef designationCategory;

		public DesignatorDropdownGroupDef designatorDropdown;

		public KeyBindingDef designationHotKey;

		[NoTranslate]
		public string uiIconPath;

		public Vector2 uiIconOffset;

		public Color uiIconColor = Color.white;

		public int uiIconForStackCount = -1;

		[Unsaved]
		public ThingDef blueprintDef;

		[Unsaved]
		public ThingDef installBlueprintDef;

		[Unsaved]
		public ThingDef frameDef;

		[Unsaved]
		private List<PlaceWorker> placeWorkersInstantiatedInt;

		[Unsaved]
		public Graphic graphic = BaseContent.BadGraphic;

		[Unsaved]
		public Texture2D uiIcon = BaseContent.BadTex;

		[Unsaved]
		public float uiIconAngle;

		public virtual IntVec2 Size => new IntVec2(1, 1);

		public bool MadeFromStuff => !stuffCategories.NullOrEmpty();

		public bool BuildableByPlayer => designationCategory != null;

		public Material DrawMatSingle
		{
			get
			{
				if (graphic == null)
				{
					return null;
				}
				return graphic.MatSingle;
			}
		}

		public float Altitude => altitudeLayer.AltitudeFor();

		public List<PlaceWorker> PlaceWorkers
		{
			get
			{
				if (placeWorkers == null)
				{
					return null;
				}
				placeWorkersInstantiatedInt = new List<PlaceWorker>();
				foreach (Type placeWorker in placeWorkers)
				{
					placeWorkersInstantiatedInt.Add((PlaceWorker)Activator.CreateInstance(placeWorker));
				}
				return placeWorkersInstantiatedInt;
			}
		}

		public bool IsResearchFinished
		{
			get
			{
				if (researchPrerequisites != null)
				{
					for (int i = 0; i < researchPrerequisites.Count; i++)
					{
						if (!researchPrerequisites[i].IsFinished)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public bool ForceAllowPlaceOver(BuildableDef other)
		{
			if (PlaceWorkers == null)
			{
				return false;
			}
			for (int i = 0; i < PlaceWorkers.Count; i++)
			{
				if (PlaceWorkers[i].ForceAllowPlaceOver(other))
				{
					return true;
				}
			}
			return false;
		}

		public override void PostLoad()
		{
			base.PostLoad();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (!uiIconPath.NullOrEmpty())
				{
					uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
				}
				else
				{
					ResolveIcon();
				}
			});
		}

		protected virtual void ResolveIcon()
		{
			if (graphic != null && graphic != BaseContent.BadGraphic)
			{
				Graphic outerGraphic = graphic;
				if (uiIconForStackCount >= 1 && this is ThingDef)
				{
					Graphic_StackCount graphic_StackCount = graphic as Graphic_StackCount;
					if (graphic_StackCount != null)
					{
						outerGraphic = graphic_StackCount.SubGraphicForStackCount(uiIconForStackCount, (ThingDef)this);
					}
				}
				Material material = outerGraphic.ExtractInnerGraphicFor(null).MatAt(defaultPlacingRot);
				uiIcon = (Texture2D)material.mainTexture;
				uiIconColor = material.color;
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string error = enumerator.Current;
					yield return error;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00b9:
			/*Error near IL_00ba: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			using (IEnumerator<StatDrawEntry> enumerator = base.SpecialDisplayStats(req).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					StatDrawEntry stat = enumerator.Current;
					yield return stat;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			IEnumerable<TerrainAffordanceDef> affdefs = Enumerable.Empty<TerrainAffordanceDef>();
			if (PlaceWorkers != null)
			{
				affdefs = affdefs.Concat(PlaceWorkers.SelectMany((PlaceWorker pw) => pw.DisplayAffordances()));
			}
			if (terrainAffordanceNeeded != null)
			{
				affdefs = affdefs.Concat(terrainAffordanceNeeded);
			}
			string[] affordances = (from ta in affdefs.Distinct()
			orderby ta.order
			select ta.label).ToArray();
			if (affordances.Length > 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "TerrainRequirement".Translate(), affordances.ToCommaList().CapitalizeFirst(), 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_01f5:
			/*Error near IL_01f6: Unexpected return in MoveNext()*/;
		}

		public override string ToString()
		{
			return defName;
		}

		public override int GetHashCode()
		{
			return defName.GetHashCode();
		}
	}
}
