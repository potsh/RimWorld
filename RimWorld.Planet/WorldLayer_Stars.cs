using System;
using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Stars : WorldLayer
	{
		private bool calculatedForStaticRotation;

		private int calculatedForStartingTile = -1;

		public const float DistanceToStars = 10f;

		private static readonly FloatRange StarsDrawSize = new FloatRange(1f, 3.8f);

		private const int StarsCount = 1500;

		private const float DistToSunToReduceStarSize = 0.8f;

		protected override int Layer => WorldCameraManager.WorldSkyboxLayer;

		public override bool ShouldRegenerate => base.ShouldRegenerate || (Find.GameInitData != null && Find.GameInitData.startingTile != calculatedForStartingTile) || UseStaticRotation != calculatedForStaticRotation;

		private bool UseStaticRotation => Current.ProgramState == ProgramState.Entry;

		protected override Quaternion Rotation
		{
			get
			{
				if (UseStaticRotation)
				{
					return Quaternion.identity;
				}
				return Quaternion.LookRotation(GenCelestial.CurSunPositionInWorldSpace());
			}
		}

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
			for (int i = 0; i < 1500; i++)
			{
				Vector3 unitVector = Rand.UnitVector3;
				Vector3 pos = unitVector * 10f;
				LayerSubMesh subMesh = GetSubMesh(WorldMaterials.Stars);
				float num = StarsDrawSize.RandomInRange;
				Vector3 rhs = (!UseStaticRotation) ? Vector3.forward : GenCelestial.CurSunPositionInWorldSpace().normalized;
				float num2 = Vector3.Dot(unitVector, rhs);
				if (num2 > 0.8f)
				{
					num *= GenMath.LerpDouble(0.8f, 1f, 1f, 0.35f, num2);
				}
				WorldRendererUtility.PrintQuadTangentialToPlanet(pos, num, 0f, subMesh, counterClockwise: true, randomizeRotation: true);
			}
			calculatedForStartingTile = ((Find.GameInitData == null) ? (-1) : Find.GameInitData.startingTile);
			calculatedForStaticRotation = UseStaticRotation;
			Rand.PopState();
			FinalizeMesh(MeshParts.All);
			yield break;
			IL_01ee:
			/*Error near IL_01ef: Unexpected return in MoveNext()*/;
		}
	}
}
