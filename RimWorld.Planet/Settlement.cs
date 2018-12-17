using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Settlement : SettlementBase
	{
		private string nameInt;

		public bool namedByPlayer;

		private Material cachedMat;

		public string Name
		{
			get
			{
				return nameInt;
			}
			set
			{
				nameInt = value;
			}
		}

		public override Texture2D ExpandingIcon => base.Faction.def.ExpandingIconTexture;

		public override string Label => (nameInt == null) ? base.Label : nameInt;

		public override bool HasName => !nameInt.NullOrEmpty();

		public override Material Material
		{
			get
			{
				if (cachedMat == null)
				{
					cachedMat = MaterialPool.MatFrom(base.Faction.def.homeIconPath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
				}
				return cachedMat;
			}
		}

		public override MapGeneratorDef MapGeneratorDef
		{
			get
			{
				if (base.Faction == Faction.OfPlayer)
				{
					return MapGeneratorDefOf.Base_Player;
				}
				return MapGeneratorDefOf.Base_Faction;
			}
		}

		public Settlement()
		{
			trader = new Settlement_TraderTracker(this);
		}

		public override IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
		{
			using (IEnumerator<IncidentTargetTagDef> enumerator = base.IncidentTargetTags().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					IncidentTargetTagDef type = enumerator.Current;
					yield return type;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (base.Faction != Faction.OfPlayer)
			{
				yield return IncidentTargetTagDefOf.Map_Misc;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return IncidentTargetTagDefOf.Map_PlayerHome;
			/*Error: Unable to find new state assignment for yield return*/;
			IL_0119:
			/*Error near IL_011a: Unexpected return in MoveNext()*/;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref nameInt, "nameInt");
			Scribe_Values.Look(ref namedByPlayer, "namedByPlayer", defaultValue: false);
		}

		public override void Tick()
		{
			base.Tick();
			SettlementDefeatUtility.CheckDefeated(this);
		}
	}
}
