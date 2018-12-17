using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GenExplosion
	{
		private static readonly int PawnNotifyCellCount = GenRadial.NumCellsInRadius(4.5f);

		public static void DoExplosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool damageFalloff = false)
		{
			if (map == null)
			{
				Log.Warning("Tried to do explosion in a null map.");
			}
			else
			{
				if (damAmount < 0)
				{
					damAmount = damType.defaultDamage;
					armorPenetration = damType.defaultArmorPenetration;
					if (damAmount < 0)
					{
						Log.ErrorOnce("Attempted to trigger an explosion without defined damage", 91094882);
						damAmount = 1;
					}
				}
				if (armorPenetration < 0f)
				{
					armorPenetration = (float)damAmount * 0.015f;
				}
				Explosion explosion = (Explosion)GenSpawn.Spawn(ThingDefOf.Explosion, center, map);
				explosion.radius = radius;
				explosion.damType = damType;
				explosion.instigator = instigator;
				explosion.damAmount = damAmount;
				explosion.armorPenetration = armorPenetration;
				explosion.weapon = weapon;
				explosion.projectile = projectile;
				explosion.intendedTarget = intendedTarget;
				explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
				explosion.preExplosionSpawnChance = preExplosionSpawnChance;
				explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
				explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
				explosion.postExplosionSpawnChance = postExplosionSpawnChance;
				explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
				explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
				explosion.chanceToStartFire = chanceToStartFire;
				explosion.damageFalloff = damageFalloff;
				explosion.StartExplosion(explosionSound);
			}
		}

		public static void RenderPredictedAreaOfEffect(IntVec3 loc, float radius)
		{
			GenDraw.DrawFieldEdges(DamageDefOf.Bomb.Worker.ExplosionCellsToHit(loc, Find.CurrentMap, radius).ToList());
		}

		public static void NotifyNearbyPawnsOfDangerousExplosive(Thing exploder, DamageDef damage, Faction onlyFaction = null)
		{
			Room room = exploder.GetRoom();
			for (int i = 0; i < PawnNotifyCellCount; i++)
			{
				IntVec3 c = exploder.Position + GenRadial.RadialPattern[i];
				if (c.InBounds(exploder.Map))
				{
					List<Thing> thingList = c.GetThingList(exploder.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						Pawn pawn = thingList[j] as Pawn;
						if (pawn != null && (int)pawn.RaceProps.intelligence >= 2 && (onlyFaction == null || pawn.Faction == onlyFaction) && damage.ExternalViolenceFor(pawn))
						{
							Room room2 = pawn.GetRoom();
							if (room2 == null || room2.CellCount == 1 || (room2 == room && GenSight.LineOfSight(exploder.Position, pawn.Position, exploder.Map, skipFirstCell: true)))
							{
								pawn.mindState.Notify_DangerousExploderAboutToExplode(exploder);
							}
						}
					}
				}
			}
		}
	}
}
