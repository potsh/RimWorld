using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompForbiddable : ThingComp
	{
		private bool forbiddenInt;

		public bool Forbidden
		{
			get
			{
				return forbiddenInt;
			}
			set
			{
				if (value != forbiddenInt)
				{
					forbiddenInt = value;
					if (parent.Spawned)
					{
						if (forbiddenInt)
						{
							parent.Map.listerHaulables.Notify_Forbidden(parent);
							parent.Map.listerMergeables.Notify_Forbidden(parent);
						}
						else
						{
							parent.Map.listerHaulables.Notify_Unforbidden(parent);
							parent.Map.listerMergeables.Notify_Unforbidden(parent);
						}
						if (parent is Building_Door)
						{
							parent.Map.reachability.ClearCache();
						}
					}
				}
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref forbiddenInt, "forbidden", defaultValue: false);
		}

		public override void PostDraw()
		{
			if (forbiddenInt)
			{
				if (parent is Blueprint || parent is Frame)
				{
					if (parent.def.size.x > 1 || parent.def.size.z > 1)
					{
						parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.ForbiddenBig);
					}
					else
					{
						parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.Forbidden);
					}
				}
				else if (parent.def.category == ThingCategory.Building)
				{
					parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.ForbiddenBig);
				}
				else
				{
					parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.Forbidden);
				}
			}
		}

		public override void PostSplitOff(Thing piece)
		{
			piece.SetForbidden(forbiddenInt);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!(parent is Building) || parent.Faction == Faction.OfPlayer)
			{
				Command_Toggle com = new Command_Toggle
				{
					hotKey = KeyBindingDefOf.Command_ItemForbid,
					icon = TexCommand.ForbidOff,
					isActive = (() => !((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0086: stateMachine*/)._0024this.Forbidden),
					defaultLabel = "CommandAllow".TranslateWithBackup("DesignatorUnforbid")
				};
				if (forbiddenInt)
				{
					com.defaultDesc = "CommandForbiddenDesc".TranslateWithBackup("DesignatorUnforbidDesc");
				}
				else
				{
					com.defaultDesc = "CommandNotForbiddenDesc".TranslateWithBackup("DesignatorForbidDesc");
				}
				if (parent.def.IsDoor)
				{
					com.tutorTag = "ToggleForbidden-Door";
					com.toggleAction = delegate
					{
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_012a: stateMachine*/)._0024this.Forbidden = !((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_012a: stateMachine*/)._0024this.Forbidden;
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.ForbiddingDoors, KnowledgeAmount.SpecificInteraction);
					};
				}
				else
				{
					com.tutorTag = "ToggleForbidden";
					com.toggleAction = delegate
					{
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0156: stateMachine*/)._0024this.Forbidden = !((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0156: stateMachine*/)._0024this.Forbidden;
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Forbidding, KnowledgeAmount.SpecificInteraction);
					};
				}
				yield return (Gizmo)com;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
