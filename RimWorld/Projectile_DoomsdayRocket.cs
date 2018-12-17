using Verse;

namespace RimWorld
{
	public class Projectile_DoomsdayRocket : Projectile
	{
		private const int ExtraExplosionCount = 3;

		private const int ExtraExplosionRadius = 5;

		protected override void Impact(Thing hitThing)
		{
			Map map = base.Map;
			base.Impact(hitThing);
			IntVec3 position = base.Position;
			Map map2 = map;
			float explosionRadius = def.projectile.explosionRadius;
			DamageDef bomb = DamageDefOf.Bomb;
			Thing launcher = base.launcher;
			int damageAmount = base.DamageAmount;
			float armorPenetration = base.ArmorPenetration;
			ThingDef equipmentDef = base.equipmentDef;
			GenExplosion.DoExplosion(position, map2, explosionRadius, bomb, launcher, damageAmount, armorPenetration, null, equipmentDef, def, intendedTarget.Thing);
			CellRect cellRect = CellRect.CenteredOn(base.Position, 5);
			cellRect.ClipInsideMap(map);
			for (int i = 0; i < 3; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				DoFireExplosion(randomCell, map, 3.9f);
			}
		}

		protected void DoFireExplosion(IntVec3 pos, Map map, float radius)
		{
			DamageDef flame = DamageDefOf.Flame;
			Thing launcher = base.launcher;
			int damageAmount = base.DamageAmount;
			float armorPenetration = base.ArmorPenetration;
			ThingDef filth_Fuel = ThingDefOf.Filth_Fuel;
			GenExplosion.DoExplosion(pos, map, radius, flame, launcher, damageAmount, armorPenetration, null, equipmentDef, def, intendedTarget.Thing, filth_Fuel, 0.2f);
		}
	}
}
