using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Instruction_DownRaider : Lesson_Instruction
	{
		private List<IntVec3> coverCells;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref coverCells, "coverCells", LookMode.Undefined);
		}

		public override void OnActivated()
		{
			base.OnActivated();
			CellRect cellRect = Find.TutorialState.sandbagsRect.ContractedBy(1);
			coverCells = new List<IntVec3>();
			foreach (IntVec3 edgeCell in cellRect.EdgeCells)
			{
				IntVec3 current = edgeCell;
				int x = current.x;
				IntVec3 centerCell = cellRect.CenterCell;
				if (x != centerCell.x)
				{
					int z = current.z;
					IntVec3 centerCell2 = cellRect.CenterCell;
					if (z != centerCell2.z)
					{
						coverCells.Add(current);
					}
				}
			}
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = base.Map;
			incidentParms.points = PawnKindDefOf.Drifter.combatPower;
			incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
			incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			incidentParms.raidForceOneIncap = true;
			incidentParms.raidNeverFleeIndividual = true;
			IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
		}

		private bool AllColonistsInCover()
		{
			foreach (Pawn item in base.Map.mapPawns.FreeColonistsSpawned)
			{
				if (!coverCells.Contains(item.Position))
				{
					return false;
				}
			}
			return true;
		}

		public override void LessonOnGUI()
		{
			if (!AllColonistsInCover())
			{
				TutorUtility.DrawCellRectOnGUI(Find.TutorialState.sandbagsRect, def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			if (!AllColonistsInCover())
			{
				for (int i = 0; i < coverCells.Count; i++)
				{
					Vector3 position = coverCells[i].ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
					Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, GenDraw.InteractionCellMaterial, 0);
				}
			}
			IEnumerable<Pawn> source = base.Map.mapPawns.PawnsInFaction(Faction.OfPlayer);
			if (source.Any((Pawn p) => p.Downed))
			{
				foreach (Pawn allPawn in base.Map.mapPawns.AllPawns)
				{
					if (allPawn.HostileTo(Faction.OfPlayer))
					{
						HealthUtility.DamageUntilDowned(allPawn);
					}
				}
			}
			if ((from p in base.Map.mapPawns.AllPawnsSpawned
			where p.HostileTo(Faction.OfPlayer) && !p.Downed
			select p).Count() == 0)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
