using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class Dialog_NamePlayerFactionAndSettlement : Dialog_GiveName
	{
		private Settlement settlement;

		public Dialog_NamePlayerFactionAndSettlement(Settlement settlement)
		{
			this.settlement = settlement;
			if (settlement.HasMap && settlement.Map.mapPawns.FreeColonistsSpawnedCount != 0)
			{
				suggestingPawn = settlement.Map.mapPawns.FreeColonistsSpawned.RandomElement();
			}
			nameGenerator = (() => NameGenerator.GenerateName(Faction.OfPlayer.def.factionNameMaker, (Predicate<string>)((Dialog_GiveName)this).IsValidName, appendNumberIfNameUsed: false, (string)null, (string)null));
			curName = nameGenerator();
			nameMessageKey = "NamePlayerFactionMessage";
			invalidNameMessageKey = "PlayerFactionNameIsInvalid";
			useSecondName = true;
			secondNameGenerator = (() => NameGenerator.GenerateName(Faction.OfPlayer.def.settlementNameMaker, (Predicate<string>)((Dialog_GiveName)this).IsValidSecondName, appendNumberIfNameUsed: false, (string)null, (string)null));
			curSecondName = secondNameGenerator();
			secondNameMessageKey = "NamePlayerFactionBaseMessage_NameFactionContinuation";
			invalidSecondNameMessageKey = "PlayerFactionBaseNameIsInvalid";
			gainedNameMessageKey = "PlayerFactionAndBaseGainsName";
		}

		public override void PostOpen()
		{
			base.PostOpen();
			if (settlement.Map != null)
			{
				Current.Game.CurrentMap = settlement.Map;
			}
		}

		protected override bool IsValidName(string s)
		{
			return NamePlayerFactionDialogUtility.IsValidName(s);
		}

		protected override bool IsValidSecondName(string s)
		{
			return NamePlayerSettlementDialogUtility.IsValidName(s);
		}

		protected override void Named(string s)
		{
			NamePlayerFactionDialogUtility.Named(s);
		}

		protected override void NamedSecond(string s)
		{
			NamePlayerSettlementDialogUtility.Named(settlement, s);
		}
	}
}
