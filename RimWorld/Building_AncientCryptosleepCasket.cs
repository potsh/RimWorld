using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Building_AncientCryptosleepCasket : Building_CryptosleepCasket
	{
		public int groupID = -1;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref groupID, "groupID", 0);
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(ref dinfo, out absorbed);
			if (!absorbed)
			{
				if (!contentsKnown && innerContainer.Count > 0 && dinfo.Def.harmsHealth && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
				{
					bool flag = false;
					foreach (Thing item in (IEnumerable<Thing>)innerContainer)
					{
						Pawn pawn = item as Pawn;
						if (pawn != null)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						EjectContents();
					}
				}
				absorbed = false;
			}
		}

		public override void EjectContents()
		{
			List<Thing> list = new List<Thing>();
			if (!base.contentsKnown)
			{
				list.AddRange(innerContainer);
				list.AddRange(UnopenedCasketsInGroup().SelectMany((Building_AncientCryptosleepCasket c) => c.innerContainer));
			}
			bool contentsKnown = base.contentsKnown;
			base.EjectContents();
			if (!contentsKnown)
			{
				ThingDef filth_Slime = ThingDefOf.Filth_Slime;
				FilthMaker.MakeFilth(base.Position, base.Map, filth_Slime, Rand.Range(8, 12));
				SetFaction(null);
				foreach (Building_AncientCryptosleepCasket item in UnopenedCasketsInGroup())
				{
					item.EjectContents();
				}
				List<Pawn> source = list.OfType<Pawn>().ToList();
				IEnumerable<Pawn> enumerable = from p in source
				where p.RaceProps.Humanlike && p.GetLord() == null && p.Faction == Faction.OfAncientsHostile
				select p;
				if (enumerable.Any())
				{
					LordMaker.MakeNewLord(Faction.OfAncientsHostile, new LordJob_AssaultColony(Faction.OfAncientsHostile, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), base.Map, enumerable);
				}
			}
		}

		private IEnumerable<Building_AncientCryptosleepCasket> UnopenedCasketsInGroup()
		{
			yield return this;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
