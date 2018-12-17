using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class VerbUtility
	{
		public struct VerbPropertiesWithSource
		{
			public VerbProperties verbProps;

			public Tool tool;

			public ManeuverDef maneuver;

			public ToolCapacityDef ToolCapacity => (maneuver == null) ? null : maneuver.requiredCapacity;

			public VerbPropertiesWithSource(VerbProperties verbProps)
			{
				this.verbProps = verbProps;
				tool = null;
				maneuver = null;
			}

			public VerbPropertiesWithSource(VerbProperties verbProps, Tool tool, ManeuverDef maneuver)
			{
				this.verbProps = verbProps;
				this.tool = tool;
				this.maneuver = maneuver;
			}
		}

		public static ThingDef GetProjectile(this Verb verb)
		{
			return (verb as Verb_LaunchProjectile)?.Projectile;
		}

		public static DamageDef GetDamageDef(this Verb verb)
		{
			if (verb.verbProps.LaunchesProjectile)
			{
				return verb.GetProjectile()?.projectile.damageDef;
			}
			return verb.verbProps.meleeDamageDef;
		}

		public static bool IsIncendiary(this Verb verb)
		{
			return verb.GetProjectile()?.projectile.ai_IsIncendiary ?? false;
		}

		public static bool ProjectileFliesOverhead(this Verb verb)
		{
			return verb.GetProjectile()?.projectile.flyOverhead ?? false;
		}

		public static bool HarmsHealth(this Verb verb)
		{
			return verb.GetDamageDef()?.harmsHealth ?? false;
		}

		public static bool IsEMP(this Verb verb)
		{
			return verb.GetDamageDef() == DamageDefOf.EMP;
		}

		public static bool UsesExplosiveProjectiles(this Verb verb)
		{
			ThingDef projectile = verb.GetProjectile();
			return projectile != null && projectile.projectile.explosionRadius > 0f;
		}

		public static List<Verb> GetConcreteExampleVerbs(Def def, ThingDef stuff = null)
		{
			List<Verb> result = null;
			ThingDef thingDef = def as ThingDef;
			if (thingDef != null)
			{
				Thing concreteExample = thingDef.GetConcreteExample(stuff);
				result = ((concreteExample is Pawn) ? ((Pawn)concreteExample).VerbTracker.AllVerbs : ((!(concreteExample is ThingWithComps)) ? null : ((ThingWithComps)concreteExample).GetComp<CompEquippable>().AllVerbs));
			}
			HediffDef hediffDef = def as HediffDef;
			if (hediffDef != null)
			{
				Hediff concreteExample2 = hediffDef.ConcreteExample;
				result = concreteExample2.TryGetComp<HediffComp_VerbGiver>().VerbTracker.AllVerbs;
			}
			return result;
		}

		public static float CalculateAdjustedForcedMiss(float forcedMiss, IntVec3 vector)
		{
			float num = (float)vector.LengthHorizontalSquared;
			if (num < 9f)
			{
				return 0f;
			}
			if (num < 25f)
			{
				return forcedMiss * 0.5f;
			}
			if (num < 49f)
			{
				return forcedMiss * 0.8f;
			}
			return forcedMiss;
		}

		public static float InterceptChanceFactorFromDistance(Vector3 origin, IntVec3 c)
		{
			float num = (c.ToVector3Shifted() - origin).MagnitudeHorizontalSquared();
			if (num <= 25f)
			{
				return 0f;
			}
			if (num >= 144f)
			{
				return 1f;
			}
			return Mathf.InverseLerp(25f, 144f, num);
		}

		public static IEnumerable<VerbPropertiesWithSource> GetAllVerbProperties(List<VerbProperties> verbProps, List<Tool> tools)
		{
			if (verbProps != null)
			{
				int k = 0;
				if (k < verbProps.Count)
				{
					yield return new VerbPropertiesWithSource(verbProps[k]);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (tools != null)
			{
				for (int j = 0; j < tools.Count; j++)
				{
					using (IEnumerator<ManeuverDef> enumerator = tools[j].Maneuvers.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							ManeuverDef i = enumerator.Current;
							yield return new VerbPropertiesWithSource(i.verb, tools[j], i);
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_018f:
			/*Error near IL_0190: Unexpected return in MoveNext()*/;
		}

		public static bool AllowAdjacentShot(LocalTargetInfo target, Thing caster)
		{
			if (!(caster is Pawn))
			{
				return true;
			}
			Pawn pawn = target.Thing as Pawn;
			return pawn == null || !pawn.HostileTo(caster) || pawn.Downed;
		}
	}
}
