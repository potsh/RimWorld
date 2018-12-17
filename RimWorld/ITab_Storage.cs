using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Storage : ITab
	{
		private Vector2 scrollPosition;

		private static readonly Vector2 WinSize = new Vector2(300f, 480f);

		protected virtual IStoreSettingsParent SelStoreSettingsParent
		{
			get
			{
				Thing thing = base.SelObject as Thing;
				if (thing != null)
				{
					IStoreSettingsParent thingOrThingCompStoreSettingsParent = GetThingOrThingCompStoreSettingsParent(thing);
					if (thingOrThingCompStoreSettingsParent != null)
					{
						return thingOrThingCompStoreSettingsParent;
					}
					return null;
				}
				return base.SelObject as IStoreSettingsParent;
			}
		}

		public override bool IsVisible
		{
			get
			{
				Thing thing = base.SelObject as Thing;
				if (thing != null && thing.Faction != null && thing.Faction != Faction.OfPlayer)
				{
					return false;
				}
				return SelStoreSettingsParent.StorageTabVisible;
			}
		}

		protected virtual bool IsPrioritySettingVisible => true;

		private float TopAreaHeight => (float)((!IsPrioritySettingVisible) ? 20 : 35);

		public ITab_Storage()
		{
			size = WinSize;
			labelKey = "TabStorage";
			tutorTag = "Storage";
		}

		protected override void FillTab()
		{
			IStoreSettingsParent storeSettingsParent = SelStoreSettingsParent;
			StorageSettings settings = storeSettingsParent.GetStoreSettings();
			Vector2 winSize = WinSize;
			float x = winSize.x;
			Vector2 winSize2 = WinSize;
			Rect position = new Rect(0f, 0f, x, winSize2.y).ContractedBy(10f);
			GUI.BeginGroup(position);
			if (IsPrioritySettingVisible)
			{
				Text.Font = GameFont.Small;
				Rect rect = new Rect(0f, 0f, 160f, TopAreaHeight - 6f);
				if (Widgets.ButtonText(rect, "Priority".Translate() + ": " + settings.Priority.Label().CapitalizeFirst()))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					IEnumerator enumerator = Enum.GetValues(typeof(StoragePriority)).GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							StoragePriority storagePriority = (StoragePriority)enumerator.Current;
							if (storagePriority != 0)
							{
								StoragePriority localPr = storagePriority;
								list.Add(new FloatMenuOption(localPr.Label().CapitalizeFirst(), delegate
								{
									settings.Priority = localPr;
								}));
							}
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
				UIHighlighter.HighlightOpportunity(rect, "StoragePriority");
			}
			ThingFilter parentFilter = null;
			if (storeSettingsParent.GetParentStoreSettings() != null)
			{
				parentFilter = storeSettingsParent.GetParentStoreSettings().filter;
			}
			Rect rect2 = new Rect(0f, TopAreaHeight, position.width, position.height - TopAreaHeight);
			Bill[] first = (from b in BillUtility.GlobalBills()
			where b is Bill_Production && b.GetStoreZone() == storeSettingsParent && b.recipe.WorkerCounter.CanPossiblyStoreInStockpile((Bill_Production)b, b.GetStoreZone())
			select b).ToArray();
			ThingFilterUI.DoThingFilterConfigWindow(rect2, ref scrollPosition, settings.filter, parentFilter, 8);
			Bill[] second = (from b in BillUtility.GlobalBills()
			where b is Bill_Production && b.GetStoreZone() == storeSettingsParent && b.recipe.WorkerCounter.CanPossiblyStoreInStockpile((Bill_Production)b, b.GetStoreZone())
			select b).ToArray();
			IEnumerable<Bill> enumerable = first.Except(second);
			foreach (Bill item in enumerable)
			{
				Messages.Message("MessageBillValidationStoreZoneInsufficient".Translate(item.LabelCap, item.billStack.billGiver.LabelShort.CapitalizeFirst(), item.GetStoreZone().label), item.billStack.billGiver as Thing, MessageTypeDefOf.RejectInput, historical: false);
			}
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StorageTab, KnowledgeAmount.FrameDisplayed);
			GUI.EndGroup();
		}

		protected IStoreSettingsParent GetThingOrThingCompStoreSettingsParent(Thing t)
		{
			IStoreSettingsParent storeSettingsParent = t as IStoreSettingsParent;
			if (storeSettingsParent != null)
			{
				return storeSettingsParent;
			}
			ThingWithComps thingWithComps = t as ThingWithComps;
			if (thingWithComps != null)
			{
				List<ThingComp> allComps = thingWithComps.AllComps;
				for (int i = 0; i < allComps.Count; i++)
				{
					storeSettingsParent = (allComps[i] as IStoreSettingsParent);
					if (storeSettingsParent != null)
					{
						return storeSettingsParent;
					}
				}
			}
			return null;
		}
	}
}
