using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Tame : Designator
	{
		private List<Pawn> justDesignated = new List<Pawn>();

		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => DesignationDefOf.Tame;

		public Designator_Tame()
		{
			defaultLabel = "DesignatorTame".Translate();
			defaultDesc = "DesignatorTameDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Tame");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Claim;
			hotKey = KeyBindingDefOf.Misc4;
			tutorTag = "Tame";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!TameablesInCell(c).Any())
			{
				return "MessageMustDesignateTameable".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			foreach (Pawn item in TameablesInCell(loc))
			{
				DesignateThing(item);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			Pawn pawn = t as Pawn;
			return pawn != null && TameUtility.CanTame(pawn) && base.Map.designationManager.DesignationOn(pawn, Designation) == null;
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			foreach (PawnKindDef item in (from p in justDesignated
			select p.kindDef).Distinct())
			{
				TameUtility.ShowDesignationWarnings(justDesignated.First((Pawn x) => x.kindDef == item));
			}
			justDesignated.Clear();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnimalTaming, KnowledgeAmount.Total);
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.RemoveAllDesignationsOn(t);
			base.Map.designationManager.AddDesignation(new Designation(t, Designation));
			justDesignated.Add((Pawn)t);
		}

		private IEnumerable<Pawn> TameablesInCell(IntVec3 c)
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
