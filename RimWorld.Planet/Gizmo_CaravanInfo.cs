using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Gizmo_CaravanInfo : Gizmo
	{
		private Caravan caravan;

		public Gizmo_CaravanInfo(Caravan caravan)
		{
			this.caravan = caravan;
			order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return Mathf.Min(520f, maxWidth);
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			if (!caravan.Spawned)
			{
				return new GizmoResult(GizmoState.Clear);
			}
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Widgets.DrawWindowBackground(rect);
			GUI.BeginGroup(rect);
			Rect rect2 = rect.AtZero();
			int? ticksToArrive = (!caravan.pather.Moving) ? null : new int?(CaravanArrivalTimeEstimator.EstimatedTicksToArrive(caravan, allowCaching: true));
			StringBuilder stringBuilder = new StringBuilder();
			float tilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(caravan, stringBuilder);
			CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(caravan.MassUsage, caravan.MassCapacity, caravan.MassCapacityExplanation, tilesPerDay, stringBuilder.ToString(), caravan.DaysWorthOfFood, caravan.forage.ForagedFoodPerDay, caravan.forage.ForagedFoodPerDayExplanation, caravan.Visibility, caravan.VisibilityExplanation), null, caravan.Tile, ticksToArrive, -9999f, rect2, lerpMassColor: true, null, multiline: true);
			GUI.EndGroup();
			GenUI.AbsorbClicksInRect(rect);
			return new GizmoResult(GizmoState.Clear);
		}
	}
}
