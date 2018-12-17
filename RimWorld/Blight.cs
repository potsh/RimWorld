using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Blight : Thing
	{
		private float severity = 0.2f;

		private int lastPlantHarmTick;

		private float lastMapMeshUpdateSeverity;

		private const float InitialSeverity = 0.2f;

		private const float SeverityPerDay = 1f;

		private const int DamagePerDay = 5;

		private const float MinSeverityToReproduce = 0.28f;

		private const float ReproduceMTBHoursAtMinSeverity = 16.8f;

		private const float ReproduceMTBHoursAtMaxSeverity = 2.1f;

		private const float ReproductionRadius = 4f;

		private static FloatRange SizeRange = new FloatRange(0.6f, 1f);

		private static Color32[] workingColors = new Color32[4];

		public float Severity
		{
			get
			{
				return severity;
			}
			set
			{
				severity = Mathf.Clamp01(value);
			}
		}

		public Plant Plant
		{
			get
			{
				if (!base.Spawned)
				{
					return null;
				}
				return BlightUtility.GetFirstBlightableEverPlant(base.Position, base.Map);
			}
		}

		protected float ReproduceMTBHours
		{
			get
			{
				if (severity < 0.28f)
				{
					return -1f;
				}
				return GenMath.LerpDouble(0.28f, 1f, 16.8f, 2.1f, severity);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref severity, "severity", 0f);
			Scribe_Values.Look(ref lastPlantHarmTick, "lastPlantHarmTick", 0);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				lastPlantHarmTick = Find.TickManager.TicksGame;
			}
			lastMapMeshUpdateSeverity = Severity;
		}

		public override void TickLong()
		{
			CheckHarmPlant();
			if (!DestroyIfNoPlantHere())
			{
				Severity += 0.0333333351f;
				float reproduceMTBHours = ReproduceMTBHours;
				if (reproduceMTBHours > 0f && Rand.MTBEventOccurs(reproduceMTBHours, 2500f, 2000f))
				{
					TryReproduceNow();
				}
				if (Mathf.Abs(Severity - lastMapMeshUpdateSeverity) >= 0.05f)
				{
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
					lastMapMeshUpdateSeverity = Severity;
				}
			}
		}

		public void Notify_PlantDeSpawned()
		{
			DestroyIfNoPlantHere();
		}

		private bool DestroyIfNoPlantHere()
		{
			if (base.Destroyed)
			{
				return true;
			}
			if (Plant == null)
			{
				Destroy();
				return true;
			}
			return false;
		}

		private void CheckHarmPlant()
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (ticksGame - lastPlantHarmTick >= 60000)
			{
				List<Thing> thingList = base.Position.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Plant plant = thingList[i] as Plant;
					if (plant != null)
					{
						HarmPlant(plant);
					}
				}
				lastPlantHarmTick = ticksGame;
			}
		}

		private void HarmPlant(Plant plant)
		{
			bool isCrop = plant.IsCrop;
			IntVec3 position = base.Position;
			Map map = base.Map;
			plant.TakeDamage(new DamageInfo(DamageDefOf.Rotting, 5f));
			if (plant.Destroyed && isCrop && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfBlight-" + plant.def.defName, 240f))
			{
				Messages.Message("MessagePlantDiedOfBlight".Translate(plant.Label, plant).CapitalizeFirst(), new TargetInfo(position, map), MessageTypeDefOf.NegativeEvent);
			}
		}

		public void TryReproduceNow()
		{
			GenRadial.ProcessEquidistantCells(base.Position, 4f, delegate(List<IntVec3> cells)
			{
				if ((from x in cells
				where BlightUtility.GetFirstBlightableNowPlant(x, base.Map) != null
				select x).TryRandomElement(out IntVec3 result))
				{
					BlightUtility.GetFirstBlightableNowPlant(result, base.Map).CropBlighted();
					return true;
				}
				return false;
			}, base.Map);
		}

		public override void Print(SectionLayer layer)
		{
			Plant plant = Plant;
			if (plant != null)
			{
				PlantUtility.SetWindExposureColors(workingColors, plant);
			}
			else
			{
				workingColors[0].a = (workingColors[1].a = (workingColors[2].a = (workingColors[3].a = 0)));
			}
			float num = SizeRange.LerpThroughRange(severity);
			if (plant != null)
			{
				float a = plant.Graphic.drawSize.x * plant.def.plant.visualSizeRange.LerpThroughRange(plant.Growth);
				num *= Mathf.Min(a, 1f);
			}
			num = Mathf.Clamp(num, 0.5f, 0.9f);
			Vector3 center = this.TrueCenter();
			Vector2 size = def.graphic.drawSize * num;
			Material mat = Graphic.MatAt(base.Rotation, this);
			Color32[] colors = workingColors;
			Printer_Plane.PrintPlane(layer, center, size, mat, 0f, flipUv: false, null, colors, 0.1f);
		}
	}
}
