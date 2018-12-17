using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldObject : IExposable, ILoadReferenceable, ISelectable
	{
		public WorldObjectDef def;

		public int ID = -1;

		private int tileInt = -1;

		private Faction factionInt;

		public int creationGameTicks = -1;

		private List<WorldObjectComp> comps = new List<WorldObjectComp>();

		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		private const float BaseDrawSize = 0.7f;

		public List<WorldObjectComp> AllComps => comps;

		public int Tile
		{
			get
			{
				return tileInt;
			}
			set
			{
				if (tileInt != value)
				{
					tileInt = value;
					if (Spawned && !def.useDynamicDrawer)
					{
						Find.World.renderer.Notify_StaticWorldObjectPosChanged();
					}
					PositionChanged();
				}
			}
		}

		public bool Spawned => Find.WorldObjects.Contains(this);

		public virtual Vector3 DrawPos => Find.WorldGrid.GetTileCenter(Tile);

		public Faction Faction => factionInt;

		public virtual string Label => def.label;

		public string LabelCap => Label.CapitalizeFirst();

		public virtual string LabelShort => Label;

		public virtual string LabelShortCap => LabelShort.CapitalizeFirst();

		public virtual bool HasName => false;

		public virtual Material Material => def.Material;

		public virtual bool SelectableNow => def.selectable;

		public virtual bool NeverMultiSelect => def.neverMultiSelect;

		public virtual Texture2D ExpandingIcon => def.ExpandingIconTexture ?? ((Texture2D)Material.mainTexture);

		public virtual Color ExpandingIconColor => Material.color;

		public virtual float ExpandingIconPriority => def.expandingIconPriority;

		public virtual bool ExpandMore => def.expandMore;

		public virtual bool AppendFactionToInspectString => true;

		public IThingHolder ParentHolder => (!Spawned) ? null : Find.World;

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats
		{
			get
			{
				yield break;
			}
		}

		public BiomeDef Biome => (!Spawned) ? null : Find.WorldGrid[Tile].biome;

		public virtual IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
		{
			if (def.IncidentTargetTags != null)
			{
				using (List<IncidentTargetTagDef>.Enumerator enumerator = def.IncidentTargetTags.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						IncidentTargetTagDef type2 = enumerator.Current;
						yield return type2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			for (int i = 0; i < comps.Count; i++)
			{
				using (IEnumerator<IncidentTargetTagDef> enumerator2 = comps[i].IncidentTargetTags().GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						IncidentTargetTagDef type = enumerator2.Current;
						yield return type;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01a4:
			/*Error near IL_01a5: Unexpected return in MoveNext()*/;
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				InitializeComps();
			}
			Scribe_Values.Look(ref ID, "ID", -1);
			Scribe_Values.Look(ref tileInt, "tile", -1);
			Scribe_References.Look(ref factionInt, "faction");
			Scribe_Values.Look(ref creationGameTicks, "creationGameTicks", 0);
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].PostExposeData();
			}
		}

		private void InitializeComps()
		{
			for (int i = 0; i < def.comps.Count; i++)
			{
				WorldObjectComp worldObjectComp = (WorldObjectComp)Activator.CreateInstance(def.comps[i].compClass);
				worldObjectComp.parent = this;
				comps.Add(worldObjectComp);
				worldObjectComp.Initialize(def.comps[i]);
			}
		}

		public virtual void SetFaction(Faction newFaction)
		{
			if (!def.canHaveFaction && newFaction != null)
			{
				Log.Warning("Tried to set faction to " + newFaction + " but this world object (" + this + ") cannot have faction.");
			}
			else
			{
				factionInt = newFaction;
			}
		}

		public virtual string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Faction != null && AppendFactionToInspectString)
			{
				stringBuilder.Append("Faction".Translate() + ": " + Faction.Name);
			}
			for (int i = 0; i < comps.Count; i++)
			{
				string text = comps[i].CompInspectStringExtra();
				if (!text.NullOrEmpty())
				{
					if (Prefs.DevMode && char.IsWhiteSpace(text[text.Length - 1]))
					{
						Log.ErrorOnce(comps[i].GetType() + " CompInspectStringExtra ended with whitespace: " + text, 25612);
						text = text.TrimEndNewlines();
					}
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(text);
				}
			}
			return stringBuilder.ToString();
		}

		public virtual void Tick()
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompTick();
			}
		}

		public virtual void ExtraSelectionOverlaysOnGUI()
		{
		}

		public virtual void DrawExtraSelectionOverlays()
		{
		}

		public virtual void PostMake()
		{
			InitializeComps();
		}

		public virtual void PostAdd()
		{
		}

		protected virtual void PositionChanged()
		{
		}

		public virtual void SpawnSetup()
		{
			if (!def.useDynamicDrawer)
			{
				Find.World.renderer.Notify_StaticWorldObjectPosChanged();
			}
			if (def.useDynamicDrawer)
			{
				Find.WorldDynamicDrawManager.RegisterDrawable(this);
			}
		}

		public virtual void PostRemove()
		{
			if (!def.useDynamicDrawer)
			{
				Find.World.renderer.Notify_StaticWorldObjectPosChanged();
			}
			if (def.useDynamicDrawer)
			{
				Find.WorldDynamicDrawManager.DeRegisterDrawable(this);
			}
			Find.WorldSelector.Deselect(this);
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].PostPostRemove();
			}
		}

		public virtual void Print(LayerSubMesh subMesh)
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			WorldRendererUtility.PrintQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.015f, subMesh, counterClockwise: false, randomizeRotation: true);
		}

		public virtual void Draw()
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			float transitionPct = ExpandableWorldObjectsUtility.TransitionPct;
			if (def.expandingIcon && transitionPct > 0f)
			{
				Color color = Material.color;
				float num = 1f - transitionPct;
				propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num));
				Vector3 drawPos = DrawPos;
				float size = 0.7f * averageTileSize;
				float altOffset = 0.015f;
				Material material = Material;
				MaterialPropertyBlock materialPropertyBlock = propertyBlock;
				WorldRendererUtility.DrawQuadTangentialToPlanet(drawPos, size, altOffset, material, counterClockwise: false, useSkyboxLayer: false, materialPropertyBlock);
			}
			else
			{
				WorldRendererUtility.DrawQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.015f, Material);
			}
		}

		public T GetComponent<T>() where T : WorldObjectComp
		{
			for (int i = 0; i < comps.Count; i++)
			{
				T val = comps[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return (T)null;
		}

		public WorldObjectComp GetComponent(Type type)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (type.IsAssignableFrom(comps[i].GetType()))
				{
					return comps[i];
				}
			}
			return null;
		}

		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			for (int i = 0; i < comps.Count; i++)
			{
				using (IEnumerator<Gizmo> enumerator = comps[i].GetGizmos().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Gizmo gizmo = enumerator.Current;
						yield return gizmo;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_00fe:
			/*Error near IL_00ff: Unexpected return in MoveNext()*/;
		}

		public virtual IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				using (IEnumerator<Gizmo> enumerator = comps[i].GetCaravanGizmos(caravan).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Gizmo g = enumerator.Current;
						yield return g;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_0104:
			/*Error near IL_0105: Unexpected return in MoveNext()*/;
		}

		public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				using (IEnumerator<FloatMenuOption> enumerator = comps[i].GetFloatMenuOptions(caravan).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						FloatMenuOption f = enumerator.Current;
						yield return f;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_0104:
			/*Error near IL_0105: Unexpected return in MoveNext()*/;
		}

		public virtual IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				using (IEnumerator<FloatMenuOption> enumerator = comps[i].GetTransportPodsFloatMenuOptions(pods, representative).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						FloatMenuOption f = enumerator.Current;
						yield return f;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_010a:
			/*Error near IL_010b: Unexpected return in MoveNext()*/;
		}

		public virtual IEnumerable<InspectTabBase> GetInspectTabs()
		{
			if (def.inspectorTabsResolved != null)
			{
				return def.inspectorTabsResolved;
			}
			return Enumerable.Empty<InspectTabBase>();
		}

		public virtual bool AllMatchingObjectsOnScreenMatchesWith(WorldObject other)
		{
			return Faction == other.Faction;
		}

		public override string ToString()
		{
			return GetType().Name + " " + LabelCap + " (tile=" + Tile + ")";
		}

		public override int GetHashCode()
		{
			return ID;
		}

		public string GetUniqueLoadID()
		{
			return "WorldObject_" + ID;
		}

		public virtual string GetDescription()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(def.description);
			for (int i = 0; i < comps.Count; i++)
			{
				string descriptionPart = comps[i].GetDescriptionPart();
				if (!descriptionPart.NullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(descriptionPart);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
