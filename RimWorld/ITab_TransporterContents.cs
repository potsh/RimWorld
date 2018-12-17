using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ITab_TransporterContents : ITab
	{
		private Vector2 scrollPosition;

		private float lastDrawnHeight;

		private List<Thing> thingsToSelect = new List<Thing>();

		private static List<Thing> tmpSingleThing = new List<Thing>();

		private const float TopPadding = 20f;

		private const float SpaceBetweenItemsLists = 10f;

		private const float ThingRowHeight = 28f;

		private const float ThingIconSize = 28f;

		private const float ThingLeftX = 36f;

		private static readonly Color ThingLabelColor = ITab_Pawn_Gear.ThingLabelColor;

		private static readonly Color ThingHighlightColor = ITab_Pawn_Gear.HighlightColor;

		public CompTransporter Transporter => base.SelThing.TryGetComp<CompTransporter>();

		public override bool IsVisible => Transporter != null && (Transporter.LoadingInProgressOrReadyToLaunch || Transporter.innerContainer.Any);

		public ITab_TransporterContents()
		{
			size = new Vector2(520f, 450f);
			labelKey = "TabTransporterContents";
		}

		protected override void FillTab()
		{
			thingsToSelect.Clear();
			Rect outRect = new Rect(default(Vector2), size).ContractedBy(10f);
			outRect.yMin += 20f;
			Rect rect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(lastDrawnHeight, outRect.height));
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
			float curY = 0f;
			DoItemsLists(rect, ref curY);
			lastDrawnHeight = curY;
			Widgets.EndScrollView();
			if (thingsToSelect.Any())
			{
				ITab_Pawn_FormingCaravan.SelectNow(thingsToSelect);
				thingsToSelect.Clear();
			}
		}

		private void DoItemsLists(Rect inRect, ref float curY)
		{
			CompTransporter transporter = Transporter;
			Rect position = new Rect(0f, curY, (inRect.width - 10f) / 2f, inRect.height);
			float curY2 = 0f;
			GUI.BeginGroup(position);
			Widgets.ListSeparator(ref curY2, position.width, "ItemsToLoad".Translate());
			bool flag = false;
			if (transporter.leftToLoad != null)
			{
				for (int i = 0; i < transporter.leftToLoad.Count; i++)
				{
					TransferableOneWay t = transporter.leftToLoad[i];
					if (t.CountToTransfer > 0 && t.HasAnyThing)
					{
						flag = true;
						DoThingRow(t.ThingDef, t.CountToTransfer, t.things, position.width, ref curY2, delegate(int x)
						{
							t.ForceTo(t.CountToTransfer - x);
							EndJobForEveryoneHauling(t);
						});
					}
				}
			}
			if (!flag)
			{
				Widgets.NoneLabel(ref curY2, position.width);
			}
			GUI.EndGroup();
			Rect position2 = new Rect((inRect.width + 10f) / 2f, curY, (inRect.width - 10f) / 2f, inRect.height);
			float curY3 = 0f;
			GUI.BeginGroup(position2);
			Widgets.ListSeparator(ref curY3, position2.width, "LoadedItems".Translate());
			bool flag2 = false;
			for (int j = 0; j < transporter.innerContainer.Count; j++)
			{
				Thing t2 = transporter.innerContainer[j];
				flag2 = true;
				tmpSingleThing.Clear();
				tmpSingleThing.Add(t2);
				DoThingRow(t2.def, t2.stackCount, tmpSingleThing, position2.width, ref curY3, delegate(int x)
				{
					GenDrop.TryDropSpawn(t2.SplitOff(x), base.SelThing.Position, base.SelThing.Map, ThingPlaceMode.Near, out Thing _);
				});
				tmpSingleThing.Clear();
			}
			if (!flag2)
			{
				Widgets.NoneLabel(ref curY3, position.width);
			}
			GUI.EndGroup();
			curY += Mathf.Max(curY2, curY3);
		}

		private void SelectLater(List<Thing> things)
		{
			thingsToSelect.Clear();
			thingsToSelect.AddRange(things);
		}

		private void DoThingRow(ThingDef thingDef, int count, List<Thing> things, float width, ref float curY, Action<int> discardAction)
		{
			Rect rect = new Rect(0f, curY, width, 28f);
			if (count != 1)
			{
				Rect butRect = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
				if (Widgets.ButtonImage(butRect, CaravanThingsTabUtility.AbandonSpecificCountButtonTex))
				{
					Find.WindowStack.Add(new Dialog_Slider("RemoveSliderText".Translate(thingDef.label), 1, count, discardAction));
				}
			}
			rect.width -= 24f;
			Rect butRect2 = new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f);
			if (Widgets.ButtonImage(butRect2, CaravanThingsTabUtility.AbandonButtonTex))
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRemoveItemDialog".Translate(thingDef.label), delegate
				{
					discardAction(count);
				}));
			}
			rect.width -= 24f;
			if (things.Count == 1)
			{
				Widgets.InfoCardButton(rect.width - 24f, curY, things[0]);
			}
			else
			{
				Widgets.InfoCardButton(rect.width - 24f, curY, thingDef);
			}
			rect.width -= 24f;
			if (Mouse.IsOver(rect))
			{
				GUI.color = ThingHighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (thingDef.DrawMatSingle != null && thingDef.DrawMatSingle.mainTexture != null)
			{
				Rect rect2 = new Rect(4f, curY, 28f, 28f);
				if (things.Count == 1)
				{
					Widgets.ThingIcon(rect2, things[0]);
				}
				else
				{
					Widgets.ThingIcon(rect2, thingDef);
				}
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = ThingLabelColor;
			Rect rect3 = new Rect(36f, curY, rect.width - 36f, rect.height);
			string str = (things.Count != 1) ? GenLabel.ThingLabel(thingDef, null, count).CapitalizeFirst() : things[0].LabelCap;
			Text.WordWrap = false;
			Widgets.Label(rect3, str.Truncate(rect3.width));
			Text.WordWrap = true;
			Text.Anchor = TextAnchor.UpperLeft;
			TooltipHandler.TipRegion(rect, str);
			if (Widgets.ButtonInvisible(rect))
			{
				SelectLater(things);
			}
			if (Mouse.IsOver(rect))
			{
				for (int i = 0; i < things.Count; i++)
				{
					TargetHighlighter.Highlight(things[i]);
				}
			}
			curY += 28f;
		}

		private void EndJobForEveryoneHauling(TransferableOneWay t)
		{
			List<Pawn> allPawnsSpawned = base.SelThing.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
				{
					JobDriver_HaulToTransporter jobDriver_HaulToTransporter = (JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver;
					if (jobDriver_HaulToTransporter.Transporter == Transporter && jobDriver_HaulToTransporter.ThingToCarry != null && jobDriver_HaulToTransporter.ThingToCarry.def == t.ThingDef)
					{
						allPawnsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
			}
		}
	}
}
