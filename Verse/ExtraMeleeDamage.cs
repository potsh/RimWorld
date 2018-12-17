namespace Verse
{
	public class ExtraMeleeDamage
	{
		public DamageDef def;

		public int amount;

		public float armorPenetration = -1f;

		public float AdjustedDamageAmount(Verb verb, Pawn caster)
		{
			return (float)amount * verb.verbProps.GetDamageFactorFor(verb, caster);
		}

		public float AdjustedArmorPenetration(Verb verb, Pawn caster)
		{
			if (armorPenetration < 0f)
			{
				return AdjustedDamageAmount(verb, caster) * 0.015f;
			}
			return armorPenetration;
		}
	}
}
