using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompPowerPlantWater : CompPowerPlant
	{
		private float spinPosition;

		private bool cacheDirty = true;

		private bool waterUsable;

		private bool waterDoubleUsed;

		private float spinRate = 1f;

		private const float PowerFactorIfWaterDoubleUsed = 0.3f;

		private const float SpinRateFactor = 0.006666667f;

		private const float BladeOffset = 2.36f;

		private const int BladeCount = 9;

		public static readonly Material BladesMat = MaterialPool.MatFrom("Things/Building/Power/WatermillGenerator/WatermillGeneratorBlades");

		protected override float DesiredPowerOutput
		{
			get
			{
				if (cacheDirty)
				{
					RebuildCache();
				}
				if (!waterUsable)
				{
					return 0f;
				}
				if (waterDoubleUsed)
				{
					return base.DesiredPowerOutput * 0.3f;
				}
				return base.DesiredPowerOutput;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			spinPosition = Rand.Range(0f, 15f);
			RebuildCache();
			ForceOthersToRebuildCache(parent.Map);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			ForceOthersToRebuildCache(map);
		}

		private void ClearCache()
		{
			cacheDirty = true;
		}

		private void RebuildCache()
		{
			waterUsable = true;
			foreach (IntVec3 item in WaterCells())
			{
				if (item.InBounds(parent.Map) && !parent.Map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.MovingFluid))
				{
					waterUsable = false;
					break;
				}
			}
			waterDoubleUsed = false;
			IEnumerable<Building> enumerable = parent.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator);
			foreach (IntVec3 item2 in WaterUseCells())
			{
				if (item2.InBounds(parent.Map))
				{
					foreach (Building item3 in enumerable)
					{
						if (item3 != parent && item3.GetComp<CompPowerPlantWater>().WaterUseRect().Contains(item2))
						{
							waterDoubleUsed = true;
							break;
						}
					}
				}
			}
			if (!waterUsable)
			{
				spinRate = 0f;
			}
			else
			{
				Vector3 vector = Vector3.zero;
				foreach (IntVec3 item4 in WaterCells())
				{
					vector += parent.Map.waterInfo.GetWaterMovement(item4.ToVector3Shifted());
				}
				spinRate = Mathf.Sign(Vector3.Dot(vector, parent.Rotation.Rotated(RotationDirection.Clockwise).FacingCell.ToVector3()));
				spinRate *= Rand.RangeSeeded(0.9f, 1.1f, parent.thingIDNumber * 60509 + 33151);
				if (waterDoubleUsed)
				{
					spinRate *= 0.5f;
				}
				cacheDirty = false;
			}
		}

		private void ForceOthersToRebuildCache(Map map)
		{
			foreach (Building item in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator))
			{
				item.GetComp<CompPowerPlantWater>().ClearCache();
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (base.PowerOutput > 0.01f)
			{
				spinPosition = (spinPosition + 0.006666667f * spinRate + 6.28318548f) % 6.28318548f;
			}
		}

		public IEnumerable<IntVec3> WaterCells()
		{
			return WaterCells(parent.Position, parent.Rotation);
		}

		public static IEnumerable<IntVec3> WaterCells(IntVec3 loc, Rot4 rot)
		{
			IntVec3 facingCell = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
			yield return loc + rot.FacingCell * 3;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public CellRect WaterUseRect()
		{
			return WaterUseRect(parent.Position, parent.Rotation);
		}

		public static CellRect WaterUseRect(IntVec3 loc, Rot4 rot)
		{
			int width = (!rot.IsHorizontal) ? 13 : 7;
			int height = (!rot.IsHorizontal) ? 7 : 13;
			return CellRect.CenteredOn(loc + rot.FacingCell * 4, width, height);
		}

		public IEnumerable<IntVec3> WaterUseCells()
		{
			return WaterUseCells(parent.Position, parent.Rotation);
		}

		public static IEnumerable<IntVec3> WaterUseCells(IntVec3 loc, Rot4 rot)
		{
			CellRect.CellRectIterator ci = WaterUseRect(loc, rot).GetIterator();
			if (!ci.Done())
			{
				yield return ci.Current;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public IEnumerable<IntVec3> GroundCells()
		{
			return GroundCells(parent.Position, parent.Rotation);
		}

		public static IEnumerable<IntVec3> GroundCells(IntVec3 loc, Rot4 rot)
		{
			IntVec3 facingCell = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
			yield return loc - rot.FacingCell;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 a = parent.TrueCenter();
			a += parent.Rotation.FacingCell.ToVector3() * 2.36f;
			for (int i = 0; i < 9; i++)
			{
				float num = spinPosition + 6.28318548f * (float)i / 9f;
				float x = Mathf.Abs(4f * Mathf.Sin(num));
				bool flag = num % 6.28318548f < 3.14159274f;
				Vector2 vector = new Vector2(x, 1f);
				Vector3 s = new Vector3(vector.x, 1f, vector.y);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(a + Vector3.up * 0.046875f * Mathf.Cos(num), parent.Rotation.AsQuat, s);
				Graphics.DrawMesh((!flag) ? MeshPool.plane10Flip : MeshPool.plane10, matrix, BladesMat, 0);
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (waterUsable && waterDoubleUsed)
			{
				text = text + "\n" + "Watermill_WaterUsedTwice".Translate();
			}
			return text;
		}
	}
}
