using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class Dialog_InfoCard : Window
	{
		private enum InfoCardTab : byte
		{
			Stats,
			Character,
			Health,
			Records
		}

		private Thing thing;

		private ThingDef stuff;

		private Def def;

		private WorldObject worldObject;

		private InfoCardTab tab;

		private Def Def
		{
			get
			{
				if (thing != null)
				{
					return thing.def;
				}
				if (worldObject != null)
				{
					return worldObject.def;
				}
				return def;
			}
		}

		private Pawn ThingPawn => thing as Pawn;

		public override Vector2 InitialSize => new Vector2(950f, 760f);

		protected override float Margin => 0f;

		public Dialog_InfoCard(Thing thing)
		{
			this.thing = thing;
			tab = InfoCardTab.Stats;
			Setup();
		}

		public Dialog_InfoCard(Def onlyDef)
		{
			def = onlyDef;
			Setup();
		}

		public Dialog_InfoCard(ThingDef thingDef, ThingDef stuff)
		{
			def = thingDef;
			this.stuff = stuff;
			Setup();
		}

		public Dialog_InfoCard(WorldObject worldObject)
		{
			this.worldObject = worldObject;
			Setup();
		}

		private void Setup()
		{
			forcePause = true;
			doCloseButton = true;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			soundAppear = SoundDefOf.InfoCard_Open;
			soundClose = SoundDefOf.InfoCard_Close;
			StatsReportUtility.Reset();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InfoCard, KnowledgeAmount.Total);
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(inRect);
			rect = rect.ContractedBy(18f);
			rect.height = 34f;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, GetTitle());
			Rect rect2 = new Rect(inRect);
			rect2.yMin = rect.yMax;
			rect2.yMax -= 38f;
			Rect rect3 = rect2;
			rect3.yMin += 45f;
			List<TabRecord> list = new List<TabRecord>();
			TabRecord item = new TabRecord("TabStats".Translate(), delegate
			{
				tab = InfoCardTab.Stats;
			}, tab == InfoCardTab.Stats);
			list.Add(item);
			if (ThingPawn != null)
			{
				if (ThingPawn.RaceProps.Humanlike)
				{
					TabRecord item2 = new TabRecord("TabCharacter".Translate(), delegate
					{
						tab = InfoCardTab.Character;
					}, tab == InfoCardTab.Character);
					list.Add(item2);
				}
				TabRecord item3 = new TabRecord("TabHealth".Translate(), delegate
				{
					tab = InfoCardTab.Health;
				}, tab == InfoCardTab.Health);
				list.Add(item3);
				TabRecord item4 = new TabRecord("TabRecords".Translate(), delegate
				{
					tab = InfoCardTab.Records;
				}, tab == InfoCardTab.Records);
				list.Add(item4);
			}
			TabDrawer.DrawTabs(rect3, list);
			FillCard(rect3.ContractedBy(18f));
		}

		protected void FillCard(Rect cardRect)
		{
			if (tab == InfoCardTab.Stats)
			{
				if (thing != null)
				{
					Thing innerThing = thing;
					MinifiedThing minifiedThing = thing as MinifiedThing;
					if (minifiedThing != null)
					{
						innerThing = minifiedThing.InnerThing;
					}
					StatsReportUtility.DrawStatsReport(cardRect, innerThing);
				}
				else if (worldObject != null)
				{
					StatsReportUtility.DrawStatsReport(cardRect, worldObject);
				}
				else
				{
					StatsReportUtility.DrawStatsReport(cardRect, def, stuff);
				}
			}
			else if (tab == InfoCardTab.Character)
			{
				CharacterCardUtility.DrawCharacterCard(cardRect, (Pawn)thing);
			}
			else if (tab == InfoCardTab.Health)
			{
				cardRect.yMin += 8f;
				HealthCardUtility.DrawPawnHealthCard(cardRect, (Pawn)thing, allowOperations: false, showBloodLoss: false, null);
			}
			else if (tab == InfoCardTab.Records)
			{
				RecordsCardUtility.DrawRecordsCard(cardRect, (Pawn)thing);
			}
		}

		private string GetTitle()
		{
			if (thing != null)
			{
				return thing.LabelCapNoCount;
			}
			if (worldObject != null)
			{
				return worldObject.LabelCap;
			}
			ThingDef thingDef = Def as ThingDef;
			if (thingDef != null)
			{
				return GenLabel.ThingLabel(thingDef, stuff).CapitalizeFirst();
			}
			return Def.LabelCap;
		}
	}
}
