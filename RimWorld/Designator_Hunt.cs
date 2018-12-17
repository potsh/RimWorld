using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Hunt : Designator
	{
		private List<Pawn> justDesignated = new List<Pawn>();

		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => DesignationDefOf.Hunt;

		public Designator_Hunt()
		{
			defaultLabel = "DesignatorHunt".Translate();
			defaultDesc = "DesignatorHuntDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Hunt");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Hunt;
			hotKey = KeyBindingDefOf.Misc11;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!HuntablesInCell(c).Any())
			{
				return "MessageMustDesignateHuntable".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			foreach (Pawn item in HuntablesInCell(loc))
			{
				DesignateThing(item);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null && pawn.AnimalOrWildMan() && pawn.Faction == null && base.Map.designationManager.DesignationOn(pawn, Designation) == null)
			{
				return true;
			}
			return false;
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.RemoveAllDesignationsOn(t);
			base.Map.designationManager.AddDesignation(new Designation(t, Designation));
			justDesignated.Add((Pawn)t);
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			foreach (PawnKindDef item in (from p in justDesignated
			select p.kindDef).Distinct())
			{
				ShowDesignationWarnings(justDesignated.First((Pawn x) => x.kindDef == item));
			}
			justDesignated.Clear();
		}

		private IEnumerable<Pawn> HuntablesInCell(IntVec3 c)
		{
			if (!c.Fogged(base.Map))
			{
				List<Thing> thingList = c.GetThingList(base.Map);
				int i = 0;
				while (true)
				{
					if (i >= thingList.Count)
					{
						yield break;
					}
					if (CanDesignateThing(thingList[i]).Accepted)
					{
						break;
					}
					i++;
				}
				yield return (Pawn)thingList[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		private void ShowDesignationWarnings(Pawn pawn)
		{
			float manhunterOnDamageChance = pawn.RaceProps.manhunterOnDamageChance;
			float manhunterOnDamageChance2 = PawnUtility.GetManhunterOnDamageChance(pawn.kindDef);
			if (manhunterOnDamageChance >= 0.015f)
			{
				string text = "MessageAnimalsGoPsychoHunted".Translate(pawn.kindDef.GetLabelPlural().CapitalizeFirst(), manhunterOnDamageChance2.ToStringPercent(), pawn.Named("ANIMAL")).CapitalizeFirst();
				Messages.Message(text, pawn, MessageTypeDefOf.CautionInput, historical: false);
			}
		}
	}
}
