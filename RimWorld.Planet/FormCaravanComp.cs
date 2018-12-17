using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class FormCaravanComp : WorldObjectComp
	{
		public static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan");

		public WorldObjectCompProperties_FormCaravan Props => (WorldObjectCompProperties_FormCaravan)props;

		private MapParent MapParent => (MapParent)parent;

		public bool Reform => !MapParent.HasMap || !MapParent.Map.IsPlayerHome;

		public bool CanFormOrReformCaravanNow
		{
			get
			{
				MapParent mapParent = MapParent;
				if (!mapParent.HasMap)
				{
					return false;
				}
				if (Reform && (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map) || mapParent.Map.mapPawns.FreeColonistsSpawnedCount == 0))
				{
					return false;
				}
				return true;
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			_003CGetGizmos_003Ec__Iterator0 _003CGetGizmos_003Ec__Iterator = (_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0036: stateMachine*/;
			MapParent mapParent = (MapParent)parent;
			if (mapParent.HasMap)
			{
				if (!Reform)
				{
					yield return (Gizmo)new Command_Action
					{
						defaultLabel = "CommandFormCaravan".Translate(),
						defaultDesc = "CommandFormCaravanDesc".Translate(),
						icon = FormCaravanCommand,
						hotKey = KeyBindingDefOf.Misc2,
						tutorTag = "FormCaravan",
						action = delegate
						{
							Find.WindowStack.Add(new Dialog_FormCaravan(mapParent.Map));
						}
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (mapParent.Map.mapPawns.FreeColonistsSpawnedCount != 0)
				{
					Command_Action reformCaravan = new Command_Action
					{
						defaultLabel = "CommandReformCaravan".Translate(),
						defaultDesc = "CommandReformCaravanDesc".Translate(),
						icon = FormCaravanCommand,
						hotKey = KeyBindingDefOf.Misc2,
						tutorTag = "ReformCaravan",
						action = delegate
						{
							Find.WindowStack.Add(new Dialog_FormCaravan(mapParent.Map, reform: true));
						}
					};
					if (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map))
					{
						reformCaravan.Disable("CommandReformCaravanFailHostilePawns".Translate());
					}
					yield return (Gizmo)reformCaravan;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}
	}
}
