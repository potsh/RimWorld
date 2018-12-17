using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class HediffCompProperties_VerbGiver : HediffCompProperties
	{
		public List<VerbProperties> verbs;

		public List<Tool> tools;

		public HediffCompProperties_VerbGiver()
		{
			compClass = typeof(HediffComp_VerbGiver);
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (tools != null)
			{
				for (int i = 0; i < tools.Count; i++)
				{
					tools[i].id = i.ToString();
				}
			}
		}

		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors(parentDef).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (tools != null)
			{
				Tool dupeTool = tools.SelectMany(delegate(Tool lhs)
				{
					HediffCompProperties_VerbGiver _0024this = ((_003CConfigErrors_003Ec__Iterator0)/*Error near IL_00da: stateMachine*/)._0024this;
					return from rhs in ((_003CConfigErrors_003Ec__Iterator0)/*Error near IL_00da: stateMachine*/)._0024this.tools
					where lhs != rhs && lhs.id == rhs.id
					select rhs;
				}).FirstOrDefault();
				if (dupeTool != null)
				{
					yield return $"duplicate hediff tool id {dupeTool.id}";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				foreach (Tool tool in tools)
				{
					using (IEnumerator<string> enumerator3 = tool.ConfigErrors().GetEnumerator())
					{
						if (enumerator3.MoveNext())
						{
							string e = enumerator3.Current;
							yield return e;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_022b:
			/*Error near IL_022c: Unexpected return in MoveNext()*/;
		}
	}
}
