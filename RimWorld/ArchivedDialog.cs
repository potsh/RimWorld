using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ArchivedDialog : IArchivable, IExposable, ILoadReferenceable
	{
		public int ID;

		public string text;

		public string title;

		public Faction relatedFaction;

		public int createdTick;

		Texture IArchivable.ArchivedIcon
		{
			get
			{
				return null;
			}
		}

		Color IArchivable.ArchivedIconColor
		{
			get
			{
				return Color.white;
			}
		}

		string IArchivable.ArchivedLabel
		{
			get
			{
				return text.Flatten();
			}
		}

		string IArchivable.ArchivedTooltip
		{
			get
			{
				return text;
			}
		}

		int IArchivable.CreatedTicksGame
		{
			get
			{
				return createdTick;
			}
		}

		bool IArchivable.CanCullArchivedNow
		{
			get
			{
				return true;
			}
		}

		LookTargets IArchivable.LookTargets
		{
			get
			{
				return null;
			}
		}

		public ArchivedDialog()
		{
		}

		public ArchivedDialog(string text, string title = null, Faction relatedFaction = null)
		{
			this.text = text;
			this.title = title;
			this.relatedFaction = relatedFaction;
			createdTick = GenTicks.TicksGame;
			if (Find.UniqueIDsManager != null)
			{
				ID = Find.UniqueIDsManager.GetNextArchivedDialogID();
			}
			else
			{
				ID = Rand.Int;
			}
		}

		void IArchivable.OpenArchived()
		{
			DiaNode diaNode = new DiaNode(this.text);
			DiaOption diaOption = new DiaOption("OK".Translate());
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			WindowStack windowStack = Find.WindowStack;
			DiaNode nodeRoot = diaNode;
			Faction faction = relatedFaction;
			string text = title;
			windowStack.Add(new Dialog_NodeTreeWithFactionInfo(nodeRoot, faction, delayInteractivity: false, radioMode: false, text));
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref ID, "ID", 0);
			Scribe_Values.Look(ref text, "text");
			Scribe_Values.Look(ref title, "title");
			Scribe_References.Look(ref relatedFaction, "relatedFaction");
			Scribe_Values.Look(ref createdTick, "createdTick", 0);
		}

		public string GetUniqueLoadID()
		{
			return "ArchivedDialog_" + ID;
		}
	}
}
