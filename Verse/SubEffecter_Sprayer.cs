using UnityEngine;

namespace Verse
{
	public abstract class SubEffecter_Sprayer : SubEffecter
	{
		public SubEffecter_Sprayer(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		protected void MakeMote(TargetInfo A, TargetInfo B)
		{
			Vector3 vector = Vector3.zero;
			switch (def.spawnLocType)
			{
			case MoteSpawnLocType.OnSource:
				vector = A.CenterVector3;
				break;
			case MoteSpawnLocType.BetweenPositions:
			{
				Vector3 vector2 = (!A.HasThing) ? A.Cell.ToVector3Shifted() : A.Thing.DrawPos;
				Vector3 vector3 = (!B.HasThing) ? B.Cell.ToVector3Shifted() : B.Thing.DrawPos;
				vector = ((A.HasThing && !A.Thing.Spawned) ? vector3 : ((!B.HasThing || B.Thing.Spawned) ? (vector2 * def.positionLerpFactor + vector3 * (1f - def.positionLerpFactor)) : vector2));
				break;
			}
			case MoteSpawnLocType.RandomCellOnTarget:
				vector = ((!B.HasThing) ? CellRect.CenteredOn(B.Cell, 0) : B.Thing.OccupiedRect()).RandomCell.ToVector3Shifted();
				break;
			case MoteSpawnLocType.BetweenTouchingCells:
				vector = A.Cell.ToVector3Shifted() + (B.Cell - A.Cell).ToVector3().normalized * 0.5f;
				break;
			}
			if (parent != null)
			{
				Rand.PushState(parent.GetHashCode());
				if (A.CenterVector3 != B.CenterVector3)
				{
					vector += (B.CenterVector3 - A.CenterVector3).normalized * parent.def.offsetTowardsTarget.RandomInRange;
				}
				vector += Gen.RandomHorizontalVector(parent.def.positionRadius);
				Rand.PopState();
			}
			Map map = A.Map ?? B.Map;
			float num = (!def.absoluteAngle) ? (B.Cell - A.Cell).AngleFlat : 0f;
			if (map != null && vector.ShouldSpawnMotesAt(map))
			{
				int randomInRange = def.burstCount.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					Mote mote = (Mote)ThingMaker.MakeThing(def.moteDef);
					GenSpawn.Spawn(mote, vector.ToIntVec3(), map);
					mote.Scale = def.scale.RandomInRange;
					mote.exactPosition = vector + Gen.RandomHorizontalVector(def.positionRadius);
					mote.rotationRate = def.rotationRate.RandomInRange;
					mote.exactRotation = def.rotation.RandomInRange + num;
					MoteThrown moteThrown = mote as MoteThrown;
					if (moteThrown != null)
					{
						moteThrown.airTimeLeft = def.airTime.RandomInRange;
						moteThrown.SetVelocity(def.angle.RandomInRange + num, def.speed.RandomInRange);
					}
				}
			}
		}
	}
}
