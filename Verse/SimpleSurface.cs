using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class SimpleSurface : IEnumerable<SurfaceColumn>, IEnumerable
	{
		private List<SurfaceColumn> columns = new List<SurfaceColumn>();

		public float Evaluate(float x, float y)
		{
			if (columns.Count == 0)
			{
				Log.Error("Evaluating a SimpleCurve2D with no columns.");
				return 0f;
			}
			SurfaceColumn surfaceColumn = columns[0];
			if (x <= surfaceColumn.x)
			{
				SurfaceColumn surfaceColumn2 = columns[0];
				return surfaceColumn2.y.Evaluate(y);
			}
			SurfaceColumn surfaceColumn3 = columns[columns.Count - 1];
			if (x >= surfaceColumn3.x)
			{
				SurfaceColumn surfaceColumn4 = columns[columns.Count - 1];
				return surfaceColumn4.y.Evaluate(y);
			}
			SurfaceColumn surfaceColumn5 = columns[0];
			SurfaceColumn surfaceColumn6 = columns[columns.Count - 1];
			for (int i = 0; i < columns.Count; i++)
			{
				SurfaceColumn surfaceColumn7 = columns[i];
				if (x <= surfaceColumn7.x)
				{
					surfaceColumn6 = columns[i];
					if (i > 0)
					{
						surfaceColumn5 = columns[i - 1];
					}
					break;
				}
			}
			float t = (x - surfaceColumn5.x) / (surfaceColumn6.x - surfaceColumn5.x);
			return Mathf.Lerp(surfaceColumn5.y.Evaluate(y), surfaceColumn6.y.Evaluate(y), t);
		}

		public void Add(SurfaceColumn newColumn)
		{
			columns.Add(newColumn);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<SurfaceColumn> GetEnumerator()
		{
			using (List<SurfaceColumn>.Enumerator enumerator = columns.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					SurfaceColumn column = enumerator.Current;
					yield return column;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00b4:
			/*Error near IL_00b5: Unexpected return in MoveNext()*/;
		}

		public IEnumerable<string> ConfigErrors(string prefix)
		{
			int i = 0;
			while (true)
			{
				if (i >= columns.Count - 1)
				{
					yield break;
				}
				SurfaceColumn surfaceColumn = columns[i + 1];
				float x = surfaceColumn.x;
				SurfaceColumn surfaceColumn2 = columns[i];
				if (x < surfaceColumn2.x)
				{
					break;
				}
				i++;
			}
			yield return prefix + ": columns are out of order";
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
