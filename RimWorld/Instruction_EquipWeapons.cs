using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_EquipWeapons : Lesson_Instruction
	{
		protected override float ProgressPercent => (float)(from c in base.Map.mapPawns.FreeColonists
		where c.equipment.Primary != null
		select c).Count() / (float)base.Map.mapPawns.FreeColonistsCount;

		private IEnumerable<Thing> Weapons => from it in Find.TutorialState.startingItems
		where IsWeapon(it) && it.Spawned
		select it;

		public static bool IsWeapon(Thing t)
		{
			return t.def.IsWeapon && t.def.BaseMarketValue > 30f;
		}

		public override void LessonOnGUI()
		{
			foreach (Thing weapon in Weapons)
			{
				TutorUtility.DrawLabelOnThingOnGUI(weapon, def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			foreach (Thing weapon in Weapons)
			{
				GenDraw.DrawArrowPointingAt(weapon.DrawPos, offscreenOnly: true);
			}
			if (ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
