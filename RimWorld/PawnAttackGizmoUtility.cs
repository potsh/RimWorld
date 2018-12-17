using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnAttackGizmoUtility
	{
		public static IEnumerable<Gizmo> GetAttackGizmos(Pawn pawn)
		{
			if (ShouldUseMeleeAttackGizmo(pawn))
			{
				yield return GetMeleeAttackGizmo(pawn);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (ShouldUseSquadAttackGizmo())
			{
				yield return GetSquadAttackGizmo(pawn);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public static bool CanShowEquipmentGizmos()
		{
			return !AtLeastTwoSelectedColonistsHaveDifferentWeapons();
		}

		private static bool ShouldUseSquadAttackGizmo()
		{
			return AtLeastOneSelectedColonistHasRangedWeapon() && AtLeastTwoSelectedColonistsHaveDifferentWeapons();
		}

		private static Gizmo GetSquadAttackGizmo(Pawn pawn)
		{
			Command_Target command_Target = new Command_Target();
			command_Target.defaultLabel = "CommandSquadAttack".Translate();
			command_Target.defaultDesc = "CommandSquadAttackDesc".Translate();
			command_Target.targetingParams = TargetingParameters.ForAttackAny();
			command_Target.hotKey = KeyBindingDefOf.Misc1;
			command_Target.icon = TexCommand.SquadAttack;
			if (FloatMenuUtility.GetAttackAction(pawn, LocalTargetInfo.Invalid, out string failStr) == null)
			{
				command_Target.Disable(failStr.CapitalizeFirst() + ".");
			}
			command_Target.action = delegate(Thing target)
			{
				IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate(object x)
				{
					Pawn pawn2 = x as Pawn;
					return pawn2 != null && pawn2.IsColonistPlayerControlled && pawn2.Drafted;
				}).Cast<Pawn>();
				foreach (Pawn item in enumerable)
				{
					FloatMenuUtility.GetAttackAction(item, target, out string _)?.Invoke();
				}
			};
			return command_Target;
		}

		private static bool ShouldUseMeleeAttackGizmo(Pawn pawn)
		{
			if (!pawn.Drafted)
			{
				return false;
			}
			return AtLeastOneSelectedColonistHasRangedWeapon() || AtLeastOneSelectedColonistHasNoWeapon() || AtLeastTwoSelectedColonistsHaveDifferentWeapons();
		}

		private static Gizmo GetMeleeAttackGizmo(Pawn pawn)
		{
			Command_Target command_Target = new Command_Target();
			command_Target.defaultLabel = "CommandMeleeAttack".Translate();
			command_Target.defaultDesc = "CommandMeleeAttackDesc".Translate();
			command_Target.targetingParams = TargetingParameters.ForAttackAny();
			command_Target.hotKey = KeyBindingDefOf.Misc2;
			command_Target.icon = TexCommand.AttackMelee;
			if (FloatMenuUtility.GetMeleeAttackAction(pawn, LocalTargetInfo.Invalid, out string failStr) == null)
			{
				command_Target.Disable(failStr.CapitalizeFirst() + ".");
			}
			command_Target.action = delegate(Thing target)
			{
				IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate(object x)
				{
					Pawn pawn2 = x as Pawn;
					return pawn2 != null && pawn2.IsColonistPlayerControlled && pawn2.Drafted;
				}).Cast<Pawn>();
				foreach (Pawn item in enumerable)
				{
					FloatMenuUtility.GetMeleeAttackAction(item, target, out string _)?.Invoke();
				}
			};
			return command_Target;
		}

		private static bool AtLeastOneSelectedColonistHasRangedWeapon()
		{
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && pawn.IsColonistPlayerControlled && pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
				{
					return true;
				}
			}
			return false;
		}

		private static bool AtLeastOneSelectedColonistHasNoWeapon()
		{
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && pawn.IsColonistPlayerControlled && (pawn.equipment == null || pawn.equipment.Primary == null))
				{
					return true;
				}
			}
			return false;
		}

		private static bool AtLeastTwoSelectedColonistsHaveDifferentWeapons()
		{
			if (Find.Selector.NumSelected <= 1)
			{
				return false;
			}
			ThingDef thingDef = null;
			bool flag = false;
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && pawn.IsColonistPlayerControlled)
				{
					ThingDef thingDef2 = (pawn.equipment != null && pawn.equipment.Primary != null) ? pawn.equipment.Primary.def : null;
					if (!flag)
					{
						thingDef = thingDef2;
						flag = true;
					}
					else if (thingDef2 != thingDef)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
