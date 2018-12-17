using UnityEngine;

namespace Verse
{
	internal class MoteThrownAttached : MoteThrown
	{
		private Vector3 attacheeLastPosition = new Vector3(-1000f, -1000f, -1000f);

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (link1.Linked)
			{
				attacheeLastPosition = link1.LastDrawPos;
			}
			exactPosition += def.mote.attachedDrawOffset;
		}

		protected override Vector3 NextExactPosition(float deltaTime)
		{
			Vector3 vector = base.NextExactPosition(deltaTime);
			if (link1.Linked)
			{
				if (!link1.Target.ThingDestroyed)
				{
					link1.UpdateDrawPos();
				}
				Vector3 b = link1.LastDrawPos - attacheeLastPosition;
				vector += b;
				vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				attacheeLastPosition = link1.LastDrawPos;
			}
			return vector;
		}
	}
}
