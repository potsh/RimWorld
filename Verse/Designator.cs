using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Designator : Command
	{
		protected bool useMouseIcon;

		public SoundDef soundDragSustain;

		public SoundDef soundDragChanged;

		protected SoundDef soundSucceeded;

		protected SoundDef soundFailed = SoundDefOf.Designate_Failed;

		protected bool hasDesignateAllFloatMenuOption;

		protected string designateAllLabel;

		private string cachedTutorTagSelect;

		private string cachedTutorTagDesignate;

		protected string cachedHighlightTag;

		public Map Map => Find.CurrentMap;

		public virtual int DraggableDimensions => 0;

		public virtual bool DragDrawMeasurements => false;

		protected override bool DoTooltip => false;

		protected virtual DesignationDef Designation => null;

		public virtual float PanelReadoutTitleExtraRightMargin => 0f;

		public override string TutorTagSelect
		{
			get
			{
				if (tutorTag == null)
				{
					return null;
				}
				if (cachedTutorTagSelect == null)
				{
					cachedTutorTagSelect = "SelectDesignator-" + tutorTag;
				}
				return cachedTutorTagSelect;
			}
		}

		public string TutorTagDesignate
		{
			get
			{
				if (tutorTag == null)
				{
					return null;
				}
				if (cachedTutorTagDesignate == null)
				{
					cachedTutorTagDesignate = "Designate-" + tutorTag;
				}
				return cachedTutorTagDesignate;
			}
		}

		public override string HighlightTag
		{
			get
			{
				if (cachedHighlightTag == null && tutorTag != null)
				{
					cachedHighlightTag = "Designator-" + tutorTag;
				}
				return cachedHighlightTag;
			}
		}

		public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
		{
			get
			{
				_003C_003Ec__Iterator0 _003C_003Ec__Iterator = (_003C_003Ec__Iterator0)/*Error near IL_0044: stateMachine*/;
				using (IEnumerator<FloatMenuOption> enumerator = base.RightClickFloatMenuOptions.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						FloatMenuOption o = enumerator.Current;
						yield return o;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				if (hasDesignateAllFloatMenuOption)
				{
					_003C_003Ec__Iterator0 _003C_003Ec__Iterator2 = (_003C_003Ec__Iterator0)/*Error near IL_00f8: stateMachine*/;
					int count2 = 0;
					List<Thing> things = Map.listerThings.AllThings;
					for (int i = 0; i < things.Count; i++)
					{
						Thing t = things[i];
						if (!t.Fogged() && CanDesignateThing(t).Accepted)
						{
							count2++;
						}
					}
					if (count2 <= 0)
					{
						yield return new FloatMenuOption(designateAllLabel + " (" + "NoneLower".Translate() + ")", null);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					yield return new FloatMenuOption(designateAllLabel + " (" + "CountToDesignate".Translate(count2) + ")", delegate
					{
						for (int k = 0; k < things.Count; k++)
						{
							Thing t2 = things[k];
							if (!t2.Fogged() && _003C_003Ec__Iterator2._0024this.CanDesignateThing(t2).Accepted)
							{
								_003C_003Ec__Iterator2._0024this.DesignateThing(things[k]);
							}
						}
					});
					/*Error: Unable to find new state assignment for yield return*/;
				}
				DesignationDef designation = Designation;
				if (Designation != null)
				{
					_003C_003Ec__Iterator0 _003C_003Ec__Iterator3 = (_003C_003Ec__Iterator0)/*Error near IL_028a: stateMachine*/;
					int count = 0;
					List<Designation> designations = Map.designationManager.allDesignations;
					for (int j = 0; j < designations.Count; j++)
					{
						if (designations[j].def == designation && RemoveAllDesignationsAffects(designations[j].target))
						{
							count++;
						}
					}
					if (count <= 0)
					{
						yield return new FloatMenuOption("RemoveAllDesignations".Translate() + " (" + "NoneLower".Translate() + ")", null);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					yield return new FloatMenuOption("RemoveAllDesignations".Translate() + " (" + count + ")", delegate
					{
						for (int num = designations.Count - 1; num >= 0; num--)
						{
							if (designations[num].def == designation && _003C_003Ec__Iterator3._0024this.RemoveAllDesignationsAffects(designations[num].target))
							{
								_003C_003Ec__Iterator3._0024this.Map.designationManager.RemoveDesignation(designations[num]);
							}
						}
					});
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield break;
				IL_0420:
				/*Error near IL_0421: Unexpected return in MoveNext()*/;
			}
		}

		public Designator()
		{
			activateSound = SoundDefOf.SelectDesignator;
			designateAllLabel = "DesignateAll".Translate();
		}

		protected bool CheckCanInteract()
		{
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(TutorTagSelect))
			{
				return false;
			}
			return true;
		}

		public override void ProcessInput(Event ev)
		{
			if (CheckCanInteract())
			{
				base.ProcessInput(ev);
				Find.DesignatorManager.Select(this);
			}
		}

		public virtual AcceptanceReport CanDesignateThing(Thing t)
		{
			return AcceptanceReport.WasRejected;
		}

		public virtual void DesignateThing(Thing t)
		{
			throw new NotImplementedException();
		}

		public abstract AcceptanceReport CanDesignateCell(IntVec3 loc);

		public virtual void DesignateMultiCell(IEnumerable<IntVec3> cells)
		{
			if (!TutorSystem.TutorialMode || TutorSystem.AllowAction(new EventPack(TutorTagDesignate, cells)))
			{
				bool somethingSucceeded = false;
				bool flag = false;
				foreach (IntVec3 cell in cells)
				{
					if (CanDesignateCell(cell).Accepted)
					{
						DesignateSingleCell(cell);
						somethingSucceeded = true;
						if (!flag)
						{
							flag = ShowWarningForCell(cell);
						}
					}
				}
				Finalize(somethingSucceeded);
				if (TutorSystem.TutorialMode)
				{
					TutorSystem.Notify_Event(new EventPack(TutorTagDesignate, cells));
				}
			}
		}

		public virtual void DesignateSingleCell(IntVec3 c)
		{
			throw new NotImplementedException();
		}

		public virtual bool ShowWarningForCell(IntVec3 c)
		{
			return false;
		}

		public void Finalize(bool somethingSucceeded)
		{
			if (somethingSucceeded)
			{
				FinalizeDesignationSucceeded();
			}
			else
			{
				FinalizeDesignationFailed();
			}
		}

		protected virtual void FinalizeDesignationSucceeded()
		{
			if (soundSucceeded != null)
			{
				soundSucceeded.PlayOneShotOnCamera();
			}
		}

		protected virtual void FinalizeDesignationFailed()
		{
			if (soundFailed != null)
			{
				soundFailed.PlayOneShotOnCamera();
			}
			if (Find.DesignatorManager.Dragger.FailureReason != null)
			{
				Messages.Message(Find.DesignatorManager.Dragger.FailureReason, MessageTypeDefOf.RejectInput, historical: false);
			}
		}

		public virtual string LabelCapReverseDesignating(Thing t)
		{
			return LabelCap;
		}

		public virtual string DescReverseDesignating(Thing t)
		{
			return Desc;
		}

		public virtual Texture2D IconReverseDesignating(Thing t, out float angle, out Vector2 offset)
		{
			angle = iconAngle;
			offset = iconOffset;
			return icon;
		}

		protected virtual bool RemoveAllDesignationsAffects(LocalTargetInfo target)
		{
			return true;
		}

		public virtual void DrawMouseAttachments()
		{
			if (useMouseIcon)
			{
				GenUI.DrawMouseAttachment(icon, string.Empty, iconAngle, iconOffset);
			}
		}

		public virtual void DrawPanelReadout(ref float curY, float width)
		{
		}

		public virtual void DoExtraGuiControls(float leftX, float bottomY)
		{
		}

		public virtual void SelectedUpdate()
		{
		}

		public virtual void SelectedProcessInput(Event ev)
		{
		}

		public virtual void Rotate(RotationDirection rotDir)
		{
		}

		public virtual bool CanRemainSelected()
		{
			return true;
		}

		public virtual void Selected()
		{
		}

		public virtual void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableThings(this, dragCells);
		}
	}
}
