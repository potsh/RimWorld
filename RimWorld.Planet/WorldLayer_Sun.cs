using System;
using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Sun : WorldLayer
	{
		private const float SunDrawSize = 15f;

		protected override int Layer => WorldCameraManager.WorldSkyboxLayer;

		protected override Quaternion Rotation => Quaternion.LookRotation(GenCelestial.CurSunPositionInWorldSpace());

		public override IEnumerable Regenerate()
		{
			IEnumerator enumerator = base.Regenerate().GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					object result = enumerator.Current;
					yield return result;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable2 = disposable = (enumerator as IDisposable);
				if (disposable != null)
				{
					disposable2.Dispose();
				}
			}
			Rand.PushState();
			Rand.Seed = Find.World.info.Seed;
			WorldRendererUtility.PrintQuadTangentialToPlanet(subMesh: GetSubMesh(WorldMaterials.Sun), pos: Vector3.forward * 10f, size: 15f, altOffset: 0f, counterClockwise: true);
			Rand.PopState();
			FinalizeMesh(MeshParts.All);
			yield break;
			IL_012f:
			/*Error near IL_0130: Unexpected return in MoveNext()*/;
		}
	}
}
