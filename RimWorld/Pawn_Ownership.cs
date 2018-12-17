using System.Linq;
using Verse;

namespace RimWorld
{
	public class Pawn_Ownership : IExposable
	{
		private Pawn pawn;

		private Building_Bed intOwnedBed;

		public Building_Bed OwnedBed
		{
			get
			{
				return intOwnedBed;
			}
			private set
			{
				if (intOwnedBed != value)
				{
					intOwnedBed = value;
					ThoughtUtility.RemovePositiveBedroomThoughts(pawn);
				}
			}
		}

		public Building_Grave AssignedGrave
		{
			get;
			private set;
		}

		public Room OwnedRoom
		{
			get
			{
				if (OwnedBed == null)
				{
					return null;
				}
				Room room = OwnedBed.GetRoom();
				if (room == null)
				{
					return null;
				}
				if (room.Owners.Contains(pawn))
				{
					return room;
				}
				return null;
			}
		}

		public Pawn_Ownership(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Building_Grave refee = AssignedGrave;
			Scribe_References.Look(ref intOwnedBed, "ownedBed");
			Scribe_References.Look(ref refee, "assignedGrave");
			AssignedGrave = refee;
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (AssignedGrave != null)
				{
					AssignedGrave.assignedPawn = pawn;
				}
				if (OwnedBed != null)
				{
					OwnedBed.owners.Add(pawn);
					OwnedBed.SortOwners();
				}
			}
		}

		public void ClaimBedIfNonMedical(Building_Bed newBed)
		{
			if (!newBed.owners.Contains(pawn) && !newBed.Medical)
			{
				UnclaimBed();
				if (newBed.owners.Count == newBed.SleepingSlotsCount)
				{
					newBed.owners[newBed.owners.Count - 1].ownership.UnclaimBed();
				}
				newBed.owners.Add(pawn);
				newBed.SortOwners();
				OwnedBed = newBed;
				if (newBed.Medical)
				{
					Log.Warning(pawn.LabelCap + " claimed medical bed.");
					UnclaimBed();
				}
			}
		}

		public void UnclaimBed()
		{
			if (OwnedBed != null)
			{
				OwnedBed.owners.Remove(pawn);
				OwnedBed = null;
			}
		}

		public void ClaimGrave(Building_Grave newGrave)
		{
			if (newGrave.assignedPawn != pawn)
			{
				UnclaimGrave();
				if (newGrave.assignedPawn != null)
				{
					newGrave.assignedPawn.ownership.UnclaimBed();
				}
				newGrave.assignedPawn = pawn;
				newGrave.GetStoreSettings().Priority = StoragePriority.Critical;
				AssignedGrave = newGrave;
			}
		}

		public void UnclaimGrave()
		{
			if (AssignedGrave != null)
			{
				AssignedGrave.assignedPawn = null;
				AssignedGrave.GetStoreSettings().Priority = StoragePriority.Important;
				AssignedGrave = null;
			}
		}

		public void UnclaimAll()
		{
			UnclaimBed();
			UnclaimGrave();
		}

		public void Notify_ChangedGuestStatus()
		{
			if (OwnedBed != null && ((OwnedBed.ForPrisoners && !pawn.IsPrisoner) || (!OwnedBed.ForPrisoners && pawn.IsPrisoner)))
			{
				UnclaimBed();
			}
		}
	}
}
