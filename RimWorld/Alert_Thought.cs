using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class Alert_Thought : Alert
	{
		protected string explanationKey;

		private static List<Thought> tmpThoughts = new List<Thought>();

		protected abstract ThoughtDef Thought
		{
			get;
		}

		private IEnumerable<Pawn> AffectedPawns()
		{
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
			{
				if (item.Dead)
				{
					Log.Error("Dead pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists:" + item);
				}
				else
				{
					item.needs.mood.thoughts.GetAllMoodThoughts(tmpThoughts);
					try
					{
						ThoughtDef requiredDef = Thought;
						for (int i = 0; i < tmpThoughts.Count; i++)
						{
							if (tmpThoughts[i].def == requiredDef)
							{
								yield return item;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
					finally
					{
						((_003CAffectedPawns_003Ec__Iterator0)/*Error near IL_0141: stateMachine*/)._003C_003E__Finally0();
					}
				}
			}
			yield break;
			IL_0181:
			/*Error near IL_0182: Unexpected return in MoveNext()*/;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(AffectedPawns());
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn item in AffectedPawns())
			{
				stringBuilder.AppendLine("    " + item.LabelShort);
			}
			return explanationKey.Translate(stringBuilder.ToString());
		}
	}
}
