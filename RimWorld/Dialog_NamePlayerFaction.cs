using System;

namespace RimWorld
{
	public class Dialog_NamePlayerFaction : Dialog_GiveName
	{
		public Dialog_NamePlayerFaction()
		{
			nameGenerator = (() => NameGenerator.GenerateName(Faction.OfPlayer.def.factionNameMaker, (Predicate<string>)((Dialog_GiveName)this).IsValidName, appendNumberIfNameUsed: false, (string)null, (string)null));
			curName = nameGenerator();
			nameMessageKey = "NamePlayerFactionMessage";
			gainedNameMessageKey = "PlayerFactionGainsName";
			invalidNameMessageKey = "PlayerFactionNameIsInvalid";
		}

		protected override bool IsValidName(string s)
		{
			return NamePlayerFactionDialogUtility.IsValidName(s);
		}

		protected override void Named(string s)
		{
			NamePlayerFactionDialogUtility.Named(s);
		}
	}
}
