using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WITab : InspectTabBase
	{
		protected WorldObject SelObject => Find.WorldSelector.SingleSelectedObject;

		protected int SelTileID => Find.WorldSelector.selectedTile;

		protected Tile SelTile => Find.WorldGrid[SelTileID];

		protected Caravan SelCaravan => SelObject as Caravan;

		private WorldInspectPane InspectPane => Find.World.UI.inspectPane;

		protected override bool StillValid
		{
			get
			{
				if (!WorldRendererUtility.WorldRenderedNow)
				{
					return false;
				}
				if (!Find.WindowStack.IsOpen<WorldInspectPane>())
				{
					return false;
				}
				return InspectPane.CurTabs.Contains(this);
			}
		}

		protected override float PaneTopY => InspectPane.PaneTopY;

		protected override void CloseTab()
		{
			InspectPane.CloseOpenTab();
		}
	}
}
