using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_NodeTreeWithFactionInfo : Dialog_NodeTree
	{
		private Faction faction;

		public Dialog_NodeTreeWithFactionInfo(DiaNode nodeRoot, Faction faction, bool delayInteractivity = false, bool radioMode = false, string title = null)
			: base(nodeRoot, delayInteractivity, radioMode, title)
		{
			this.faction = faction;
			if (faction != null)
			{
				minOptionsAreaHeight = 60f;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			if (faction != null)
			{
				DrawFactionInfo(new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 7f), faction);
			}
		}

		private void DrawFactionInfo(Rect rect, Faction faction)
		{
			Text.Anchor = TextAnchor.LowerRight;
			FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
			GUI.color = playerRelationKind.GetColor();
			Widgets.Label(rect, playerRelationKind.GetLabel());
			rect.height -= Text.CalcHeight(playerRelationKind.GetLabel(), rect.width) + Text.SpaceBetweenLines;
			GUI.color = Color.gray;
			Widgets.Label(rect, faction.Name + "\n" + "goodwill".Translate().CapitalizeFirst() + ": " + faction.PlayerGoodwill.ToStringWithSign());
			GenUI.ResetLabelAlign();
			GUI.color = Color.white;
		}
	}
}
