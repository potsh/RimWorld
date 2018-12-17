using System;
using System.Collections;
using System.Collections.Generic;

namespace Verse
{
	public static class MapMeshFlagUtility
	{
		public static List<MapMeshFlag> allFlags;

		static MapMeshFlagUtility()
		{
			allFlags = new List<MapMeshFlag>();
			IEnumerator enumerator = Enum.GetValues(typeof(MapMeshFlag)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MapMeshFlag mapMeshFlag = (MapMeshFlag)enumerator.Current;
					if (mapMeshFlag != 0)
					{
						allFlags.Add(mapMeshFlag);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
