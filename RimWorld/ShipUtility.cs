using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ShipUtility
	{
		private static Dictionary<ThingDef, int> requiredParts;

		private static List<Building> closedSet = new List<Building>();

		private static List<Building> openSet = new List<Building>();

		public static Dictionary<ThingDef, int> RequiredParts()
		{
			if (requiredParts == null)
			{
				requiredParts = new Dictionary<ThingDef, int>();
				requiredParts[ThingDefOf.Ship_CryptosleepCasket] = 1;
				requiredParts[ThingDefOf.Ship_ComputerCore] = 1;
				requiredParts[ThingDefOf.Ship_Reactor] = 1;
				requiredParts[ThingDefOf.Ship_Engine] = 3;
				requiredParts[ThingDefOf.Ship_Beam] = 1;
				requiredParts[ThingDefOf.Ship_SensorCluster] = 1;
			}
			return requiredParts;
		}

		public static IEnumerable<string> LaunchFailReasons(Building rootBuilding)
		{
			List<Building> shipParts = ShipBuildingsAttachedTo(rootBuilding).ToList();
			foreach (KeyValuePair<ThingDef, int> item in RequiredParts())
			{
				int shipPartCount = shipParts.Count((Building pa) => pa.def == item.Key);
				if (shipPartCount < item.Value)
				{
					yield return string.Format("{0}: {1}x {2} ({3} {4})", "ShipReportMissingPart".Translate(), item.Value - shipPartCount, item.Key.label, "ShipReportMissingPartRequires".Translate(), item.Value);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			bool fullPodFound = false;
			foreach (Building item2 in shipParts)
			{
				if (item2.def == ThingDefOf.Ship_CryptosleepCasket)
				{
					Building_CryptosleepCasket building_CryptosleepCasket = item2 as Building_CryptosleepCasket;
					if (building_CryptosleepCasket != null && building_CryptosleepCasket.HasAnyContents)
					{
						fullPodFound = true;
						break;
					}
				}
			}
			foreach (Building item3 in shipParts)
			{
				CompHibernatable hibernatable = item3.TryGetComp<CompHibernatable>();
				if (hibernatable != null && hibernatable.State == HibernatableStateDefOf.Hibernating)
				{
					yield return string.Format("{0}: {1}", "ShipReportHibernating".Translate(), item3.LabelCap);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (hibernatable != null && !hibernatable.Running)
				{
					yield return string.Format("{0}: {1}", "ShipReportNotReady".Translate(), item3.LabelCap);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (!fullPodFound)
			{
				yield return "ShipReportNoFullPods".Translate();
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_036f:
			/*Error near IL_0370: Unexpected return in MoveNext()*/;
		}

		public static bool HasHibernatingParts(Building rootBuilding)
		{
			List<Building> list = ShipBuildingsAttachedTo(rootBuilding).ToList();
			foreach (Building item in list)
			{
				CompHibernatable compHibernatable = item.TryGetComp<CompHibernatable>();
				if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
				{
					return true;
				}
			}
			return false;
		}

		public static void StartupHibernatingParts(Building rootBuilding)
		{
			List<Building> list = ShipBuildingsAttachedTo(rootBuilding).ToList();
			foreach (Building item in list)
			{
				CompHibernatable compHibernatable = item.TryGetComp<CompHibernatable>();
				if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
				{
					compHibernatable.Startup();
				}
			}
		}

		public static List<Building> ShipBuildingsAttachedTo(Building root)
		{
			closedSet.Clear();
			if (root == null || root.Destroyed)
			{
				return closedSet;
			}
			openSet.Clear();
			openSet.Add(root);
			while (openSet.Count > 0)
			{
				Building building = openSet[openSet.Count - 1];
				openSet.Remove(building);
				closedSet.Add(building);
				foreach (IntVec3 item in GenAdj.CellsAdjacentCardinal(building))
				{
					Building edifice = item.GetEdifice(building.Map);
					if (edifice != null && edifice.def.building.shipPart && !closedSet.Contains(edifice) && !openSet.Contains(edifice))
					{
						openSet.Add(edifice);
					}
				}
			}
			return closedSet;
		}

		public static IEnumerable<Gizmo> ShipStartupGizmos(Building building)
		{
			if (HasHibernatingParts(building))
			{
				yield return (Gizmo)new Command_Action
				{
					action = delegate
					{
						string text = "HibernateWarning";
						if (building.Map.info.parent.GetComponent<EscapeShipComp>() == null)
						{
							text += "Standalone";
						}
						if (!Find.Storyteller.difficulty.allowBigThreats)
						{
							text += "Pacifist";
						}
						DiaNode diaNode = new DiaNode(text.Translate());
						DiaOption item = new DiaOption("Confirm".Translate())
						{
							action = delegate
							{
								StartupHibernatingParts(building);
							},
							resolveTree = true
						};
						diaNode.options.Add(item);
						DiaOption item2 = new DiaOption("GoBack".Translate())
						{
							resolveTree = true
						};
						diaNode.options.Add(item2);
						Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
					},
					defaultLabel = "CommandShipStartup".Translate(),
					defaultDesc = "CommandShipStartupDesc".Translate(),
					hotKey = KeyBindingDefOf.Misc1,
					icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower")
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
