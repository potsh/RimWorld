using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Pawn : TaleData
	{
		public Pawn pawn;

		public PawnKindDef kind;

		public Faction faction;

		public Gender gender;

		public Name name;

		public string title;

		public ThingDef primaryEquipment;

		public ThingDef notableApparel;

		public override void ExposeData()
		{
			Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
			Scribe_Defs.Look(ref kind, "kind");
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref gender, "gender", Gender.None);
			Scribe_Deep.Look(ref name, "name");
			Scribe_Values.Look(ref gender, "title", Gender.None);
			Scribe_Defs.Look(ref primaryEquipment, "peq");
			Scribe_Defs.Look(ref notableApparel, "app");
		}

		public override IEnumerable<Rule> GetRules(string prefix)
		{
			return GrammarUtility.RulesForPawn(prefix, name, title, kind, gender, faction);
		}

		public static TaleData_Pawn GenerateFrom(Pawn pawn)
		{
			TaleData_Pawn taleData_Pawn = new TaleData_Pawn();
			taleData_Pawn.pawn = pawn;
			taleData_Pawn.kind = pawn.kindDef;
			taleData_Pawn.faction = pawn.Faction;
			taleData_Pawn.gender = (pawn.RaceProps.hasGenders ? pawn.gender : Gender.None);
			if (pawn.story != null)
			{
				taleData_Pawn.title = pawn.story.title;
			}
			if (pawn.RaceProps.Humanlike)
			{
				taleData_Pawn.name = pawn.Name;
				if (pawn.equipment.Primary != null)
				{
					taleData_Pawn.primaryEquipment = pawn.equipment.Primary.def;
				}
				if (pawn.apparel.WornApparel.TryRandomElement(out Apparel result))
				{
					taleData_Pawn.notableApparel = result.def;
				}
			}
			return taleData_Pawn;
		}

		public static TaleData_Pawn GenerateRandom()
		{
			PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
			Faction faction = FactionUtility.DefaultFactionFrom(random.defaultFactionType);
			Pawn pawn = PawnGenerator.GeneratePawn(random, faction);
			return GenerateFrom(pawn);
		}
	}
}
