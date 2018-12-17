using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public sealed class TaleManager : IExposable
	{
		private List<Tale> tales = new List<Tale>();

		private const int MaxUnusedVolatileTales = 350;

		public List<Tale> AllTalesListForReading => tales;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref tales, "tales", LookMode.Deep);
		}

		public void TaleManagerTick()
		{
			RemoveExpiredTales();
		}

		public void Add(Tale tale)
		{
			tales.Add(tale);
			CheckCullTales(tale);
		}

		private void RemoveTale(Tale tale)
		{
			if (!tale.Unused)
			{
				Log.Warning("Tried to remove used tale " + tale);
			}
			else
			{
				tales.Remove(tale);
			}
		}

		private void CheckCullTales(Tale addedTale)
		{
			CheckCullUnusedVolatileTales();
			CheckCullUnusedTalesWithMaxPerPawnLimit(addedTale);
		}

		private void CheckCullUnusedVolatileTales()
		{
			int num = 0;
			for (int i = 0; i < tales.Count; i++)
			{
				if (tales[i].def.type == TaleType.Volatile && tales[i].Unused)
				{
					num++;
				}
			}
			while (num > 350)
			{
				Tale tale = null;
				float num2 = 3.40282347E+38f;
				for (int j = 0; j < tales.Count; j++)
				{
					if (tales[j].def.type == TaleType.Volatile && tales[j].Unused && tales[j].InterestLevel < num2)
					{
						tale = tales[j];
						num2 = tales[j].InterestLevel;
					}
				}
				RemoveTale(tale);
				num--;
			}
		}

		private void CheckCullUnusedTalesWithMaxPerPawnLimit(Tale addedTale)
		{
			if (addedTale.def.maxPerPawn >= 0 && addedTale.DominantPawn != null)
			{
				int num = 0;
				for (int i = 0; i < tales.Count; i++)
				{
					if (tales[i].Unused && tales[i].def == addedTale.def && tales[i].DominantPawn == addedTale.DominantPawn)
					{
						num++;
					}
				}
				while (num > addedTale.def.maxPerPawn)
				{
					Tale tale = null;
					int num2 = -1;
					for (int j = 0; j < tales.Count; j++)
					{
						if (tales[j].Unused && tales[j].def == addedTale.def && tales[j].DominantPawn == addedTale.DominantPawn && tales[j].AgeTicks > num2)
						{
							tale = tales[j];
							num2 = tales[j].AgeTicks;
						}
					}
					RemoveTale(tale);
					num--;
				}
			}
		}

		private void RemoveExpiredTales()
		{
			for (int num = tales.Count - 1; num >= 0; num--)
			{
				if (tales[num].Expired)
				{
					RemoveTale(tales[num]);
				}
			}
		}

		public TaleReference GetRandomTaleReferenceForArt(ArtGenerationContext source)
		{
			if (source == ArtGenerationContext.Outsider)
			{
				return TaleReference.Taleless;
			}
			if (tales.Count == 0)
			{
				return TaleReference.Taleless;
			}
			if (Rand.Value < 0.25f)
			{
				return TaleReference.Taleless;
			}
			if (!(from x in tales
			where x.def.usableForArt
			select x).TryRandomElementByWeight((Tale ta) => ta.InterestLevel, out Tale result))
			{
				return TaleReference.Taleless;
			}
			result.Notify_NewlyUsed();
			return new TaleReference(result);
		}

		public TaleReference GetRandomTaleReferenceForArtConcerning(Thing th)
		{
			if (tales.Count == 0)
			{
				return TaleReference.Taleless;
			}
			if (!(from x in tales
			where x.def.usableForArt && x.Concerns(th)
			select x).TryRandomElementByWeight((Tale x) => x.InterestLevel, out Tale result))
			{
				return TaleReference.Taleless;
			}
			result.Notify_NewlyUsed();
			return new TaleReference(result);
		}

		public Tale GetLatestTale(TaleDef def, Pawn pawn)
		{
			Tale tale = null;
			int num = 0;
			for (int i = 0; i < tales.Count; i++)
			{
				if (tales[i].def == def && tales[i].DominantPawn == pawn && (tale == null || tales[i].AgeTicks < num))
				{
					tale = tales[i];
					num = tales[i].AgeTicks;
				}
			}
			return tale;
		}

		public void Notify_PawnDestroyed(Pawn pawn)
		{
			for (int num = tales.Count - 1; num >= 0; num--)
			{
				if (tales[num].Unused && !tales[num].def.usableForArt && tales[num].def.type != TaleType.PermanentHistorical && tales[num].DominantPawn == pawn)
				{
					RemoveTale(tales[num]);
				}
			}
		}

		public void Notify_PawnDiscarded(Pawn p, bool silentlyRemoveReferences)
		{
			for (int num = tales.Count - 1; num >= 0; num--)
			{
				if (tales[num].Concerns(p))
				{
					if (!silentlyRemoveReferences)
					{
						Log.Warning("Discarding pawn " + p + ", but he is referenced by a tale " + tales[num] + ".");
					}
					else if (!tales[num].Unused)
					{
						Log.Warning("Discarding pawn " + p + ", but he is referenced by an active tale " + tales[num] + ".");
					}
					RemoveTale(tales[num]);
				}
			}
		}

		public bool AnyActiveTaleConcerns(Pawn p)
		{
			for (int i = 0; i < tales.Count; i++)
			{
				if (!tales[i].Unused && tales[i].Concerns(p))
				{
					return true;
				}
			}
			return false;
		}

		public bool AnyTaleConcerns(Pawn p)
		{
			for (int i = 0; i < tales.Count; i++)
			{
				if (tales[i].Concerns(p))
				{
					return true;
				}
			}
			return false;
		}

		public float GetMaxHistoricalTaleDay()
		{
			float num = 0f;
			for (int i = 0; i < tales.Count; i++)
			{
				Tale tale = tales[i];
				if (tale.def.type == TaleType.PermanentHistorical)
				{
					float num2 = (float)GenDate.TickAbsToGame(tale.date) / 60000f;
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
			return num;
		}

		public void LogTales()
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerable<Tale> enumerable = from x in tales
			where !x.Unused
			select x;
			IEnumerable<Tale> enumerable2 = from x in tales
			where x.def.type == TaleType.Volatile && x.Unused
			select x;
			IEnumerable<Tale> enumerable3 = from x in tales
			where x.def.type == TaleType.PermanentHistorical && x.Unused
			select x;
			IEnumerable<Tale> enumerable4 = from x in tales
			where x.def.type == TaleType.Expirable && x.Unused
			select x;
			stringBuilder.AppendLine("All tales count: " + tales.Count);
			stringBuilder.AppendLine("Used count: " + enumerable.Count());
			stringBuilder.AppendLine("Unused volatile count: " + enumerable2.Count() + " (max: " + 350 + ")");
			stringBuilder.AppendLine("Unused permanent count: " + enumerable3.Count());
			stringBuilder.AppendLine("Unused expirable count: " + enumerable4.Count());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("-------Used-------");
			foreach (Tale item in enumerable)
			{
				stringBuilder.AppendLine(item.ToString());
			}
			stringBuilder.AppendLine("-------Unused volatile-------");
			foreach (Tale item2 in enumerable2)
			{
				stringBuilder.AppendLine(item2.ToString());
			}
			stringBuilder.AppendLine("-------Unused permanent-------");
			foreach (Tale item3 in enumerable3)
			{
				stringBuilder.AppendLine(item3.ToString());
			}
			stringBuilder.AppendLine("-------Unused expirable-------");
			foreach (Tale item4 in enumerable4)
			{
				stringBuilder.AppendLine(item4.ToString());
			}
			Log.Message(stringBuilder.ToString());
		}

		public void LogTaleInterestSummary()
		{
			StringBuilder stringBuilder = new StringBuilder();
			float num = (from t in tales
			where t.def.usableForArt
			select t).Sum((Tale t) => t.InterestLevel);
			Func<TaleDef, float> defInterest = (TaleDef def) => (from t in tales
			where t.def == def
			select t).Sum((Tale t) => t.InterestLevel);
			foreach (TaleDef item in from def in DefDatabase<TaleDef>.AllDefs
			where def.usableForArt
			orderby defInterest(def) descending
			select def)
			{
				stringBuilder.AppendLine(item.defName + ":   [" + tales.Where((Tale t) => t.def == item).Count() + "]   " + (defInterest(item) / num).ToStringPercent("F2"));
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
