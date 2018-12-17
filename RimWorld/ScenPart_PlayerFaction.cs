using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_PlayerFaction : ScenPart
	{
		internal FactionDef factionDef;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref factionDef, "factionDef");
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
			if (Widgets.ButtonText(scenPartRect, factionDef.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (FactionDef item in from d in DefDatabase<FactionDef>.AllDefs
				where d.isPlayer
				select d)
				{
					FactionDef localFd = item;
					list.Add(new FloatMenuOption(localFd.LabelCap, delegate
					{
						factionDef = localFd;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_PlayerFaction".Translate(factionDef.label);
		}

		public override void Randomize()
		{
			factionDef = (from fd in DefDatabase<FactionDef>.AllDefs
			where fd.isPlayer
			select fd).RandomElement();
		}

		public override void PostWorldGenerate()
		{
			Find.GameInitData.playerFaction = FactionGenerator.NewGeneratedFaction(factionDef);
			Find.FactionManager.Add(Find.GameInitData.playerFaction);
			FactionGenerator.EnsureRequiredEnemies(Find.GameInitData.playerFaction);
		}

		public override void PreMapGenerate()
		{
			Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
			settlement.SetFaction(Find.GameInitData.playerFaction);
			settlement.Tile = Find.GameInitData.startingTile;
			settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, Find.GameInitData.playerFaction.def.playerInitialSettlementNameMaker);
			Find.WorldObjects.Add(settlement);
		}

		public override void PostGameStart()
		{
			Find.GameInitData.playerFaction = null;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (factionDef == null)
			{
				yield return "factionDef is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
