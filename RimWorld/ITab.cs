using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class ITab : InspectTabBase
	{
		protected object SelObject => Find.Selector.SingleSelectedObject;

		protected Thing SelThing => Find.Selector.SingleSelectedThing;

		protected Pawn SelPawn => SelThing as Pawn;

		private MainTabWindow_Inspect InspectPane => (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;

		protected override bool StillValid => Find.MainTabsRoot.OpenTab == MainButtonDefOf.Inspect && ((MainTabWindow_Inspect)Find.MainTabsRoot.OpenTab.TabWindow).CurTabs.Contains(this);

		protected override float PaneTopY => InspectPane.PaneTopY;

		protected override void CloseTab()
		{
			InspectPane.CloseOpenTab();
		}
	}
}
