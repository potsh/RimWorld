using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StunHandler : IExposable
	{
		public Thing parent;

		private int stunTicksLeft;

		private Mote moteStun;

		private int EMPAdaptedTicksLeft;

		public const float StunDurationTicksPerDamage = 30f;

		public bool Stunned => stunTicksLeft > 0;

		private int EMPAdaptationTicksDuration
		{
			get
			{
				Pawn pawn = parent as Pawn;
				if (pawn != null && pawn.RaceProps.IsMechanoid)
				{
					return 2200;
				}
				return 0;
			}
		}

		public StunHandler(Thing parent)
		{
			this.parent = parent;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref stunTicksLeft, "stunTicksLeft", 0);
			Scribe_Values.Look(ref EMPAdaptedTicksLeft, "EMPAdaptedTicksLeft", 0);
		}

		public void StunHandlerTick()
		{
			if (EMPAdaptedTicksLeft > 0)
			{
				EMPAdaptedTicksLeft--;
			}
			if (stunTicksLeft > 0)
			{
				stunTicksLeft--;
				if (moteStun == null || moteStun.Destroyed)
				{
					moteStun = MoteMaker.MakeStunOverlay(parent);
				}
				Pawn pawn = parent as Pawn;
				if (pawn != null && pawn.Downed)
				{
					stunTicksLeft = 0;
				}
				if (moteStun != null)
				{
					moteStun.Maintain();
				}
			}
		}

		public void Notify_DamageApplied(DamageInfo dinfo, bool affectedByEMP)
		{
			Pawn pawn = parent as Pawn;
			if (pawn == null || (!pawn.Downed && !pawn.Dead))
			{
				if (dinfo.Def == DamageDefOf.Stun)
				{
					StunFor(Mathf.RoundToInt(dinfo.Amount * 30f), dinfo.Instigator);
				}
				else if (dinfo.Def == DamageDefOf.EMP && affectedByEMP)
				{
					if (EMPAdaptedTicksLeft <= 0)
					{
						StunFor(Mathf.RoundToInt(dinfo.Amount * 30f), dinfo.Instigator);
						EMPAdaptedTicksLeft = EMPAdaptationTicksDuration;
					}
					else
					{
						IntVec3 position = parent.Position;
						float x = (float)position.x + 1f;
						IntVec3 position2 = parent.Position;
						float y = (float)position2.y;
						IntVec3 position3 = parent.Position;
						Vector3 loc = new Vector3(x, y, (float)position3.z + 1f);
						MoteMaker.ThrowText(loc, parent.Map, "Adapted".Translate(), Color.white);
					}
				}
			}
		}

		public void StunFor(int ticks, Thing instigator)
		{
			stunTicksLeft = Mathf.Max(stunTicksLeft, ticks);
			Find.BattleLog.Add(new BattleLogEntry_Event(parent, RulePackDefOf.Event_Stun, instigator));
		}
	}
}
