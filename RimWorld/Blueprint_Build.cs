using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Blueprint_Build : Blueprint
	{
		public ThingDef stuffToUse;

		public override string Label
		{
			get
			{
				string label = base.Label;
				if (stuffToUse != null)
				{
					return "ThingMadeOfStuffLabel".Translate(stuffToUse.LabelAsStuff, label);
				}
				return label;
			}
		}

		protected override float WorkTotal => def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffToUse);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref stuffToUse, "stuffToUse");
		}

		public override ThingDef UIStuff()
		{
			return stuffToUse;
		}

		public override List<ThingDefCountClass> MaterialsNeeded()
		{
			return def.entityDefToBuild.CostListAdjusted(stuffToUse);
		}

		protected override Thing MakeSolidThing()
		{
			return ThingMaker.MakeThing(def.entityDefToBuild.frameDef, stuffToUse);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			Command buildCopy = BuildCopyCommandUtility.BuildCopyCommand(def.entityDefToBuild, stuffToUse);
			if (buildCopy != null)
			{
				yield return (Gizmo)buildCopy;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				using (IEnumerator<Command> enumerator2 = BuildFacilityCommandUtility.BuildFacilityCommands(def.entityDefToBuild).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Command facility = enumerator2.Current;
						yield return (Gizmo)facility;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01be:
			/*Error near IL_01bf: Unexpected return in MoveNext()*/;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("ContainedResources".Translate() + ":");
			bool flag = true;
			foreach (ThingDefCountClass item in MaterialsNeeded())
			{
				if (!flag)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(item.thingDef.LabelCap + ": 0 / " + item.count);
				flag = false;
			}
			return stringBuilder.ToString().Trim();
		}
	}
}
