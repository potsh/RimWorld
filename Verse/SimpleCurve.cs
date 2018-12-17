using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class SimpleCurve : IEnumerable<CurvePoint>, IEnumerable
	{
		private List<CurvePoint> points = new List<CurvePoint>();

		[Unsaved]
		private SimpleCurveView view;

		private static Comparison<CurvePoint> CurvePointsComparer = delegate(CurvePoint a, CurvePoint b)
		{
			if (a.x < b.x)
			{
				return -1;
			}
			if (b.x < a.x)
			{
				return 1;
			}
			return 0;
		};

		public int PointsCount => points.Count;

		public List<CurvePoint> Points => points;

		public bool HasView => view != null;

		public SimpleCurveView View
		{
			get
			{
				if (view == null)
				{
					view = new SimpleCurveView();
					view.SetViewRectAround(this);
				}
				return view;
			}
		}

		public CurvePoint this[int i]
		{
			get
			{
				return points[i];
			}
			set
			{
				points[i] = value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<CurvePoint> GetEnumerator()
		{
			using (List<CurvePoint>.Enumerator enumerator = points.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					CurvePoint point = enumerator.Current;
					yield return point;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00b4:
			/*Error near IL_00b5: Unexpected return in MoveNext()*/;
		}

		public void SetPoints(IEnumerable<CurvePoint> newPoints)
		{
			points.Clear();
			foreach (CurvePoint newPoint in newPoints)
			{
				points.Add(newPoint);
			}
			SortPoints();
		}

		public void Add(float x, float y, bool sort = true)
		{
			CurvePoint newPoint = new CurvePoint(x, y);
			Add(newPoint, sort);
		}

		public void Add(CurvePoint newPoint, bool sort = true)
		{
			points.Add(newPoint);
			if (sort)
			{
				SortPoints();
			}
		}

		public void SortPoints()
		{
			points.Sort(CurvePointsComparer);
		}

		public void RemovePointNear(CurvePoint point)
		{
			int num = 0;
			while (true)
			{
				if (num >= points.Count)
				{
					return;
				}
				if ((points[num].Loc - point.Loc).sqrMagnitude < 0.001f)
				{
					break;
				}
				num++;
			}
			points.RemoveAt(num);
		}

		public float Evaluate(float x)
		{
			if (points.Count == 0)
			{
				Log.Error("Evaluating a SimpleCurve with no points.");
				return 0f;
			}
			if (x <= points[0].x)
			{
				return points[0].y;
			}
			if (x >= points[points.Count - 1].x)
			{
				return points[points.Count - 1].y;
			}
			CurvePoint curvePoint = points[0];
			CurvePoint curvePoint2 = points[points.Count - 1];
			for (int i = 0; i < points.Count; i++)
			{
				if (x <= points[i].x)
				{
					curvePoint2 = points[i];
					if (i > 0)
					{
						curvePoint = points[i - 1];
					}
					break;
				}
			}
			float t = (x - curvePoint.x) / (curvePoint2.x - curvePoint.x);
			return Mathf.Lerp(curvePoint.y, curvePoint2.y, t);
		}

		public float PeriodProbabilityFromCumulative(float startX, float span)
		{
			if (points.Count < 2)
			{
				return 0f;
			}
			if (points[0].y != 0f)
			{
				Log.Warning("PeriodProbabilityFromCumulative should only run on curves whose first point is 0.");
			}
			float num = Evaluate(startX + span) - Evaluate(startX);
			if (num < 0f)
			{
				Log.Error("PeriodicProbability got negative probability from " + this + ": slope should never be negative.");
				num = 0f;
			}
			if (num > 1f)
			{
				num = 1f;
			}
			return num;
		}

		public IEnumerable<string> ConfigErrors(string prefix)
		{
			int i = 0;
			while (true)
			{
				if (i >= points.Count - 1)
				{
					yield break;
				}
				if (points[i + 1].x < points[i].x)
				{
					break;
				}
				i++;
			}
			yield return prefix + ": points are out of order";
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
