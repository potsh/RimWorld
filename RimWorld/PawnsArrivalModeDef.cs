using System;
using Verse;

namespace RimWorld
{
	public class PawnsArrivalModeDef : Def
	{
		public Type workerClass = typeof(PawnsArrivalModeWorker);

		public SimpleCurve selectionWeightCurve;

		public SimpleCurve pointsFactorCurve;

		public TechLevel minTechLevel;

		public bool forQuickMilitaryAid;

		[MustTranslate]
		public string textEnemy;

		[MustTranslate]
		public string textFriendly;

		[Unsaved]
		private PawnsArrivalModeWorker workerInt;

		public PawnsArrivalModeWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (PawnsArrivalModeWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
