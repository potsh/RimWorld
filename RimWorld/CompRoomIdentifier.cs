using Verse;

namespace RimWorld
{
	public class CompRoomIdentifier : ThingComp
	{
		private CompProperties_RoomIdentifier Props => (CompProperties_RoomIdentifier)props;

		public override string CompInspectStringExtra()
		{
			Room room = parent.GetRoom(RegionType.Set_All);
			return string.Concat(str2: (room != null && room.Role != RoomRoleDefOf.None) ? (room.Role.LabelCap + " (" + room.GetStatScoreStage(Props.roomStat).label + ", " + Props.roomStat.ScoreToString(room.GetStat(Props.roomStat)) + ")") : "Outdoors".Translate(), str0: "Room".Translate(), str1: ": ");
		}
	}
}
