using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	[StaticConstructorOnStartup]
	public sealed class PawnDestinationReservationManager : IExposable
	{
		public class PawnDestinationReservation : IExposable
		{
			public IntVec3 target;

			public Pawn claimant;

			public Job job;

			public bool obsolete;

			public void ExposeData()
			{
				Scribe_Values.Look(ref target, "target");
				Scribe_References.Look(ref claimant, "claimant");
				Scribe_References.Look(ref job, "job");
				Scribe_Values.Look(ref obsolete, "obsolete", defaultValue: false);
			}
		}

		public class PawnDestinationSet : IExposable
		{
			public List<PawnDestinationReservation> list = new List<PawnDestinationReservation>();

			public void ExposeData()
			{
				Scribe_Collections.Look(ref list, "list", LookMode.Deep);
				if (Scribe.mode == LoadSaveMode.PostLoadInit && list.RemoveAll((PawnDestinationReservation x) => x.claimant.DestroyedOrNull()) != 0)
				{
					Log.Warning("Some destination reservations had null or destroyed claimant.");
				}
			}
		}

		private Dictionary<Faction, PawnDestinationSet> reservedDestinations = new Dictionary<Faction, PawnDestinationSet>();

		private static readonly Material DestinationMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestination");

		private static readonly Material DestinationSelectionMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestinationSelection");

		private List<Faction> reservedDestinationsKeysWorkingList;

		private List<PawnDestinationSet> reservedDestinationsValuesWorkingList;

		public PawnDestinationReservationManager()
		{
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				RegisterFaction(allFaction);
			}
		}

		public void RegisterFaction(Faction faction)
		{
			reservedDestinations.Add(faction, new PawnDestinationSet());
		}

		public void Reserve(Pawn p, Job job, IntVec3 loc)
		{
			if (p.Faction != null)
			{
				ObsoleteAllClaimedBy(p);
				reservedDestinations[p.Faction].list.Add(new PawnDestinationReservation
				{
					target = loc,
					claimant = p,
					job = job
				});
			}
		}

		public PawnDestinationReservation MostRecentReservationFor(Pawn p)
		{
			if (p.Faction == null)
			{
				return null;
			}
			List<PawnDestinationReservation> list = reservedDestinations[p.Faction].list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].claimant == p && !list[i].obsolete)
				{
					return list[i];
				}
			}
			return null;
		}

		public IntVec3 FirstObsoleteReservationFor(Pawn p)
		{
			if (p.Faction == null)
			{
				return IntVec3.Invalid;
			}
			List<PawnDestinationReservation> list = reservedDestinations[p.Faction].list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].claimant == p && list[i].obsolete)
				{
					return list[i].target;
				}
			}
			return IntVec3.Invalid;
		}

		public Job FirstObsoleteReservationJobFor(Pawn p)
		{
			if (p.Faction == null)
			{
				return null;
			}
			List<PawnDestinationReservation> list = reservedDestinations[p.Faction].list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].claimant == p && list[i].obsolete)
				{
					return list[i].job;
				}
			}
			return null;
		}

		public bool IsReserved(IntVec3 loc)
		{
			foreach (KeyValuePair<Faction, PawnDestinationSet> reservedDestination in reservedDestinations)
			{
				List<PawnDestinationReservation> list = reservedDestination.Value.list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].target == loc)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool CanReserve(IntVec3 c, Pawn searcher, bool draftedOnly = false)
		{
			if (searcher.Faction == null)
			{
				return true;
			}
			List<PawnDestinationReservation> list = reservedDestinations[searcher.Faction].list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].target == c && list[i].claimant != searcher && (!draftedOnly || list[i].claimant.Drafted))
				{
					return false;
				}
			}
			return true;
		}

		public Pawn FirstReserverOf(IntVec3 c, Faction faction)
		{
			if (faction == null)
			{
				return null;
			}
			List<PawnDestinationReservation> list = reservedDestinations[faction].list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].target == c)
				{
					return list[i].claimant;
				}
			}
			return null;
		}

		public void ReleaseAllObsoleteClaimedBy(Pawn p)
		{
			if (p.Faction != null)
			{
				List<PawnDestinationReservation> list = reservedDestinations[p.Faction].list;
				int num = 0;
				while (num < list.Count)
				{
					if (list[num].claimant == p && list[num].obsolete)
					{
						list[num] = list[list.Count - 1];
						list.RemoveLast();
					}
					else
					{
						num++;
					}
				}
			}
		}

		public void ReleaseAllClaimedBy(Pawn p)
		{
			if (p.Faction != null)
			{
				List<PawnDestinationReservation> list = reservedDestinations[p.Faction].list;
				int num = 0;
				while (num < list.Count)
				{
					if (list[num].claimant == p)
					{
						list[num] = list[list.Count - 1];
						list.RemoveLast();
					}
					else
					{
						num++;
					}
				}
			}
		}

		public void ReleaseClaimedBy(Pawn p, Job job)
		{
			if (p.Faction != null)
			{
				List<PawnDestinationReservation> list = reservedDestinations[p.Faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].claimant == p && list[i].job == job)
					{
						list[i].job = null;
						if (list[i].obsolete)
						{
							list[i] = list[list.Count - 1];
							list.RemoveLast();
							i--;
						}
					}
				}
			}
		}

		public void ObsoleteAllClaimedBy(Pawn p)
		{
			if (p.Faction != null)
			{
				List<PawnDestinationReservation> list = reservedDestinations[p.Faction].list;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].claimant == p)
					{
						list[i].obsolete = true;
						if (list[i].job == null)
						{
							list[i] = list[list.Count - 1];
							list.RemoveLast();
							i--;
						}
					}
				}
			}
		}

		public void DebugDrawDestinations()
		{
			foreach (PawnDestinationReservation item in reservedDestinations[Faction.OfPlayer].list)
			{
				if (!(item.claimant.Position == item.target))
				{
					IntVec3 target = item.target;
					Vector3 s = new Vector3(1f, 1f, 1f);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(target.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), Quaternion.identity, s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, DestinationMat, 0);
					if (Find.Selector.IsSelected(item.claimant))
					{
						Graphics.DrawMesh(MeshPool.plane10, matrix, DestinationSelectionMat, 0);
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref reservedDestinations, "reservedDestinations", LookMode.Reference, LookMode.Deep, ref reservedDestinationsKeysWorkingList, ref reservedDestinationsValuesWorkingList);
		}
	}
}
