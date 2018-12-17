using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Slaughter : Designator
	{
		private List<Pawn> justDesignated = new List<Pawn>();

		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => DesignationDefOf.Slaughter;

		public Designator_Slaughter()
		{
			defaultLabel = "DesignatorSlaughter".Translate();
			defaultDesc = "DesignatorSlaughterDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Slaughter");
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
			if (!SlaughterablesInCell(c).Any())
			{
				return "MessageMustDesignateSlaughterable".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			foreach (Pawn item in SlaughterablesInCell(loc))
			{
				DesignateThing(item);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null && pawn.def.race.Animal && pawn.Faction == Faction.OfPlayer && base.Map.designationManager.DesignationOn(pawn, Designation) == null && !pawn.InAggroMentalState)
			{
				return true;
			}
			return false;
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.AddDesignation(new Designation(t, Designation));
			justDesignated.Add((Pawn)t);
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			for (int i = 0; i < justDesignated.Count; i++)
			{
				SlaughterDesignatorUtility.CheckWarnAboutBondedAnimal(justDesignated[i]);
			}
			justDesignated.Clear();
		}

		private IEnumerable<Pawn> SlaughterablesInCell(IntVec3 c)
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
	}
}
