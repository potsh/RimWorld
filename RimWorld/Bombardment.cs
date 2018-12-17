using System.Linq;
using Verse;

namespace RimWorld
{
	public class Bombardment : OrbitalStrike
	{
		private const int ImpactAreaRadius = 15;

		private const int ExplosionRadiusMin = 6;

		private const int ExplosionRadiusMax = 8;

		public const int EffectiveRadius = 23;

		public const int RandomFireRadius = 25;

		private const int BombIntervalTicks = 18;

		private const int StartRandomFireEveryTicks = 20;

		private static readonly SimpleCurve DistanceChanceFactor = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(15f, 0.1f)
		};

		public override void StartStrike()
		{
			base.StartStrike();
			MoteMaker.MakeBombardmentMote(base.Position, base.Map);
		}

		public override void Tick()
		{
			base.Tick();
			if (!base.Destroyed)
			{
				if (Find.TickManager.TicksGame % 18 == 0)
				{
					CreateRandomExplosion();
				}
				if (Find.TickManager.TicksGame % 20 == 0)
				{
					StartRandomFire();
				}
			}
		}

		private void CreateRandomExplosion()
		{
			IntVec3 intVec = (from x in GenRadial.RadialCellsAround(base.Position, 15f, useCenter: true)
			where x.InBounds(base.Map)
			select x).RandomElementByWeight((IntVec3 x) => DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position)));
			float num = (float)Rand.Range(6, 8);
			IntVec3 center = intVec;
			Map map = base.Map;
			float radius = num;
			DamageDef bomb = DamageDefOf.Bomb;
			Thing instigator = base.instigator;
			ThingDef def = base.def;
			ThingDef weaponDef = base.weaponDef;
			GenExplosion.DoExplosion(center, map, radius, bomb, instigator, -1, -1f, null, weaponDef, def);
		}

		private void StartRandomFire()
		{
			IntVec3 c = (from x in GenRadial.RadialCellsAround(base.Position, 25f, useCenter: true)
			where x.InBounds(base.Map)
			select x).RandomElementByWeight((IntVec3 x) => DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position)));
			FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
		}
	}
}
