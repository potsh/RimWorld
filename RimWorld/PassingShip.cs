using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PassingShip : IExposable, ICommunicable, ILoadReferenceable
	{
		public PassingShipManager passingShipManager;

		public string name = "Nameless";

		protected int loadID = -1;

		public int ticksUntilDeparture = 40000;

		public virtual string FullTitle => "ErrorFullTitle";

		public bool Departed => ticksUntilDeparture <= 0;

		public Map Map => (passingShipManager == null) ? null : passingShipManager.map;

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref name, "name");
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_Values.Look(ref ticksUntilDeparture, "ticksUntilDeparture", 0);
		}

		public virtual void PassingShipTick()
		{
			ticksUntilDeparture--;
			if (Departed)
			{
				Depart();
			}
		}

		public virtual void Depart()
		{
			if (Map.listerBuildings.ColonistsHaveBuilding((Thing b) => b.def.IsCommsConsole))
			{
				Messages.Message("MessageShipHasLeftCommsRange".Translate(FullTitle), MessageTypeDefOf.SituationResolved);
			}
			passingShipManager.RemoveShip(this);
		}

		public virtual void TryOpenComms(Pawn negotiator)
		{
			throw new NotImplementedException();
		}

		public virtual string GetCallLabel()
		{
			return name;
		}

		public string GetInfoText()
		{
			return FullTitle;
		}

		Faction ICommunicable.GetFaction()
		{
			return null;
		}

		public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
		{
			string label = "CallOnRadio".Translate(GetCallLabel());
			Action action = delegate
			{
				if (!Building_OrbitalTradeBeacon.AllPowered(Map).Any())
				{
					Messages.Message("MessageNeedBeaconToTradeWithShip".Translate(), console, MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					console.GiveUseCommsJob(negotiator, this);
				}
			};
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.InitiateSocial), negotiator, console);
		}

		public string GetUniqueLoadID()
		{
			return "PassingShip_" + loadID;
		}
	}
}
